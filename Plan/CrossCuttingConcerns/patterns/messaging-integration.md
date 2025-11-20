# Messaging & Integration Pattern

**Constitution Principle**: Principle 4 - Event-Driven Data Discipline  
**Priority**: 🟠 High  
**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Table of Contents

- [Overview](#overview)
- [MassTransit Configuration](#masstransit-configuration)
- [Domain Event Publishing](#domain-event-publishing)
- [Event Consumers](#event-consumers)
- [Idempotency Patterns](#idempotency-patterns)
- [Transactional Outbox Pattern](#transactional-outbox-pattern)
- [Event Schema Versioning](#event-schema-versioning)
- [Retry Policies & Dead Letter Queues](#retry-policies--dead-letter-queues)
- [Testing with MassTransit Test Harness](#testing-with-masstransit-test-harness)
- [RabbitMQ (Local) vs Azure Service Bus (Production)](#rabbitmq-local-vs-azure-service-bus-production)
- [Anti-Patterns](#anti-patterns)
- [Performance Considerations](#performance-considerations)
- [References](#references)

---

## Overview

NorthStar LMS follows Constitution Principle 4: **prefer asynchronous event-driven integration**. MassTransit provides the messaging abstraction, supporting RabbitMQ for local development and Azure Service Bus for production.

**Core Integration Principles**:
1. **Events over calls**: Use domain events for cross-service communication
2. **Idempotency**: Every event consumer must be idempotent (safe to retry)
3. **Versioning**: All event schemas are versioned with deprecation windows
4. **Resilience**: Retry policies + dead letter queues for poison messages
5. **Observability**: OpenTelemetry traces span across message boundaries

**Messaging Architecture**:
```
┌─────────────────────────────────────────────────────────────┐
│                    Student Service (Producer)                │
│  ✓ Domain Event   ✓ Outbox Pattern   ✓ Publish to Bus      │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌───────────────────────────────────────────────────────────────┐
│          Message Bus (RabbitMQ / Azure Service Bus)           │
│  ✓ Durable Queues   ✓ Topic Exchange   ✓ Dead Letter        │
└───────────────────┬──────────────────────┬────────────────────┘
                    │                      │
        ┌───────────▼──────────┐  ┌────────▼──────────┐
        │  Assessment Service  │  │  Intervention Svc  │
        │  (Consumer)          │  │  (Consumer)        │
        │  ✓ Idempotent        │  │  ✓ Idempotent      │
        │  ✓ Retry Logic       │  │  ✓ Retry Logic     │
        └──────────────────────┘  └────────────────────┘
```

---

## MassTransit Configuration

### Infrastructure DependencyInjection

```csharp
// Location: Src/Foundation/shared/Infrastructure/DependencyInjection.cs
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthStarET.Foundation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Register consumers from calling assembly
            x.AddConsumers(Assembly.GetEntryAssembly());
            
            // Configure transport based on environment
            var environment = configuration["ASPNETCORE_ENVIRONMENT"];
            
            if (environment == "Production")
            {
                // Azure Service Bus for production
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString("ServiceBus"));
                    
                    // Message retry policy
                    cfg.UseMessageRetry(r => r.Incremental(
                        retryLimit: 5,
                        initialInterval: TimeSpan.FromSeconds(1),
                        intervalIncrement: TimeSpan.FromSeconds(2)));
                    
                    // Configure endpoints
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                // RabbitMQ for local/dev
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["MessageBus:Host"] ?? "localhost", h =>
                    {
                        h.Username(configuration["MessageBus:Username"] ?? "guest");
                        h.Password(configuration["MessageBus:Password"] ?? "guest");
                    });
                    
                    // Message retry policy
                    cfg.UseMessageRetry(r => r.Incremental(
                        retryLimit: 5,
                        initialInterval: TimeSpan.FromSeconds(1),
                        intervalIncrement: TimeSpan.FromSeconds(2)));
                    
                    // Configure endpoints
                    cfg.ConfigureEndpoints(context);
                });
            }
        });
        
        return services;
    }
}
```

### Aspire AppHost Configuration

```csharp
// Location: Src/Foundation/AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Add RabbitMQ for local development
var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .WithDataVolume();

// Add PostgreSQL (for outbox)
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var studentDb = postgres.AddDatabase("studentdb");

// Student Service (producer)
builder.AddProject<Projects.StudentApi>("student-api")
    .WithReference(rabbitmq)
    .WithReference(studentDb)
    .WaitFor(rabbitmq)
    .WaitFor(studentDb);

// Assessment Service (consumer)
builder.AddProject<Projects.AssessmentApi>("assessment-api")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.Build().Run();
```

### Configuration (appsettings.json)

```json
{
  "MessageBus": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "MassTransit": {
    "Endpoints": {
      "QueueName": "student-service-queue",
      "PrefetchCount": 10,
      "ConcurrentMessageLimit": 5
    }
  }
}
```

---

## Domain Event Publishing

### Domain Event Base Class

```csharp
// Location: Src/Foundation/shared/Domain/Common/DomainEvent.cs
namespace NorthStarET.Foundation.Domain.Common;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public required Guid EventId { get; init; }
    public required DateTime OccurredAt { get; init; }
}

/// <summary>
/// Tenant-aware domain event (most common)
/// </summary>
public abstract record TenantAwareDomainEvent : DomainEvent
{
    public required Guid TenantId { get; init; }
}
```

### Example Domain Event

```csharp
// Location: Src/Foundation/services/Student/Domain/Events/StudentCreatedEvent.cs
namespace NorthStarET.Foundation.Student.Domain.Events;

public sealed record StudentCreatedEvent : TenantAwareDomainEvent
{
    public required Guid StudentId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateTime DateOfBirth { get; init; }
    public string? StateStudentId { get; init; }
}
```

### IDomainEventPublisher Interface

```csharp
// Location: Src/Foundation/shared/Application/Messaging/IDomainEventPublisher.cs
namespace NorthStarET.Foundation.Application.Messaging;

/// <summary>
/// Publishes domain events to the message bus
/// </summary>
public interface IDomainEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : class, IDomainEvent;
    
    Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> domainEvents, CancellationToken cancellationToken)
        where TEvent : class, IDomainEvent;
}
```

### MassTransit Implementation

```csharp
// Location: Src/Foundation/shared/Infrastructure/Messaging/MassTransitDomainEventPublisher.cs
using MassTransit;
using Microsoft.Extensions.Logging;

namespace NorthStarET.Foundation.Infrastructure.Messaging;

public sealed class MassTransitDomainEventPublisher : IDomainEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitDomainEventPublisher> _logger;
    
    public MassTransitDomainEventPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<MassTransitDomainEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }
    
    public async Task PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken)
        where TEvent : class, IDomainEvent
    {
        try
        {
            await _publishEndpoint.Publish(domainEvent, cancellationToken);
            
            _logger.LogInformation(
                "Published event {EventType} with ID {EventId}",
                typeof(TEvent).Name,
                domainEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish event {EventType} with ID {EventId}",
                typeof(TEvent).Name,
                domainEvent.EventId);
            
            throw;
        }
    }
    
    public async Task PublishBatchAsync<TEvent>(
        IEnumerable<TEvent> domainEvents,
        CancellationToken cancellationToken)
        where TEvent : class, IDomainEvent
    {
        var events = domainEvents.ToList();
        
        try
        {
            await _publishEndpoint.PublishBatch(events, cancellationToken);
            
            _logger.LogInformation(
                "Published batch of {Count} events of type {EventType}",
                events.Count,
                typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish batch of {Count} events of type {EventType}",
                events.Count,
                typeof(TEvent).Name);
            
            throw;
        }
    }
}
```

### Publishing from Command Handler

```csharp
// Location: Src/Foundation/services/Student/Application/Commands/CreateStudentCommandHandler.cs
using MediatR;
using NorthStarET.Foundation.Student.Domain.Events;

namespace NorthStarET.Foundation.Student.Application.Commands;

public sealed class CreateStudentCommandHandler 
    : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _repository;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public async Task<Result<Guid>> Handle(
        CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        // Create student entity
        var student = Student.Create(
            firstName: command.FirstName,
            lastName: command.LastName,
            dateOfBirth: command.DateOfBirth,
            tenantId: _tenantContext.TenantId);
        
        // Persist to database
        await _repository.AddAsync(student, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        
        // Publish domain event AFTER successful persistence
        var studentCreatedEvent = new StudentCreatedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = _dateTimeProvider.UtcNow,
            TenantId = student.TenantId,
            StudentId = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            DateOfBirth = student.DateOfBirth,
            StateStudentId = student.StateStudentId
        };
        
        await _eventPublisher.PublishAsync(studentCreatedEvent, cancellationToken);
        
        return Result.Success(student.Id);
    }
}
```

---

## Event Consumers

### IConsumer Implementation

```csharp
// Location: Src/Foundation/services/Assessment/Application/Consumers/StudentCreatedEventConsumer.cs
using MassTransit;
using Microsoft.Extensions.Logging;
using NorthStarET.Foundation.Student.Domain.Events;

namespace NorthStarET.Foundation.Assessment.Application.Consumers;

/// <summary>
/// Consumes StudentCreatedEvent to create assessment profile
/// </summary>
public sealed class StudentCreatedEventConsumer : IConsumer<StudentCreatedEvent>
{
    private readonly IAssessmentProfileRepository _repository;
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<StudentCreatedEventConsumer> _logger;
    
    public StudentCreatedEventConsumer(
        IAssessmentProfileRepository repository,
        IIdempotencyService idempotencyService,
        ILogger<StudentCreatedEventConsumer> logger)
    {
        _repository = repository;
        _idempotencyService = idempotencyService;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<StudentCreatedEvent> context)
    {
        var studentEvent = context.Message;
        
        _logger.LogInformation(
            "Received StudentCreatedEvent for student {StudentId} in tenant {TenantId}",
            studentEvent.StudentId,
            studentEvent.TenantId);
        
        // Idempotency check: have we already processed this event?
        var idempotencyKey = $"student-created:{studentEvent.EventId}";
        var isProcessed = await _idempotencyService.IsProcessedAsync(
            idempotencyKey,
            context.CancellationToken);
        
        if (isProcessed)
        {
            _logger.LogInformation(
                "Event {EventId} already processed, skipping",
                studentEvent.EventId);
            return;
        }
        
        try
        {
            // Create assessment profile for new student
            var assessmentProfile = AssessmentProfile.Create(
                studentId: studentEvent.StudentId,
                tenantId: studentEvent.TenantId);
            
            await _repository.AddAsync(assessmentProfile, context.CancellationToken);
            await _repository.SaveChangesAsync(context.CancellationToken);
            
            // Mark event as processed (10-minute idempotency window)
            await _idempotencyService.MarkAsProcessedAsync(
                idempotencyKey,
                TimeSpan.FromMinutes(10),
                context.CancellationToken);
            
            _logger.LogInformation(
                "Created assessment profile for student {StudentId}",
                studentEvent.StudentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create assessment profile for student {StudentId}",
                studentEvent.StudentId);
            
            // Throw to trigger MassTransit retry
            throw;
        }
    }
}
```

### Consumer Registration

```csharp
// Location: Src/Foundation/services/Assessment/Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{
    // Register all consumers in this assembly
    x.AddConsumers(Assembly.GetExecutingAssembly());
    
    // Or register individually:
    x.AddConsumer<StudentCreatedEventConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        // Configure specific endpoint for consumer
        cfg.ReceiveEndpoint("assessment-student-created", e =>
        {
            e.PrefetchCount = 10;
            e.ConcurrentMessageLimit = 5;
            
            e.ConfigureConsumer<StudentCreatedEventConsumer>(context);
        });
    });
});
```

### Consumer Filters (Logging, Validation)

```csharp
// Location: Src/Foundation/shared/Infrastructure/Messaging/Filters/LoggingConsumeFilter.cs
using MassTransit;

namespace NorthStarET.Foundation.Infrastructure.Messaging.Filters;

/// <summary>
/// Logs all consumed messages for observability
/// </summary>
public sealed class LoggingConsumeFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<LoggingConsumeFilter<T>> _logger;
    
    public LoggingConsumeFilter(ILogger<LoggingConsumeFilter<T>> logger)
    {
        _logger = logger;
    }
    
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var messageType = typeof(T).Name;
        var messageId = context.MessageId ?? Guid.Empty;
        
        _logger.LogInformation(
            "Consuming message {MessageType} with ID {MessageId}",
            messageType,
            messageId);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await next.Send(context);
            
            _logger.LogInformation(
                "Successfully consumed message {MessageType} in {ElapsedMs}ms",
                messageType,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to consume message {MessageType} after {ElapsedMs}ms",
                messageType,
                stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
    
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("loggingConsume");
    }
}
```

---

## Idempotency Patterns

### IIdempotencyService Interface

```csharp
// Location: Src/Foundation/shared/Application/Services/IIdempotencyService.cs
namespace NorthStarET.Foundation.Application.Services;

/// <summary>
/// Tracks processed events/commands to prevent duplicate execution
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Check if operation has already been processed
    /// </summary>
    Task<bool> IsProcessedAsync(string key, CancellationToken cancellationToken);
    
    /// <summary>
    /// Mark operation as processed with TTL
    /// </summary>
    Task MarkAsProcessedAsync(string key, TimeSpan expiry, CancellationToken cancellationToken);
}
```

### Redis Implementation

```csharp
// Location: Src/Foundation/shared/Infrastructure/Services/RedisIdempotencyService.cs
using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace NorthStarET.Foundation.Infrastructure.Services;

public sealed class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisIdempotencyService> _logger;
    
    private const string KeyPrefix = "idempotency:";
    
    public RedisIdempotencyService(
        IDistributedCache cache,
        ILogger<RedisIdempotencyService> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<bool> IsProcessedAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{KeyPrefix}{key}";
        var value = await _cache.GetAsync(cacheKey, cancellationToken);
        
        var isProcessed = value != null;
        
        _logger.LogDebug(
            "Idempotency check for key {Key}: {IsProcessed}",
            key,
            isProcessed);
        
        return isProcessed;
    }
    
    public async Task MarkAsProcessedAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{KeyPrefix}{key}";
        var value = Encoding.UTF8.GetBytes("processed");
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };
        
        await _cache.SetAsync(cacheKey, value, options, cancellationToken);
        
        _logger.LogInformation(
            "Marked key {Key} as processed with {Expiry} expiry",
            key,
            expiry);
    }
}
```

### 10-Minute Idempotency Window

**Standard across NorthStar**: All event consumers use a **10-minute idempotency window** stored in Redis.

```csharp
// Standard pattern in all consumers
var idempotencyKey = $"{eventType}:{eventId}";
var isProcessed = await _idempotencyService.IsProcessedAsync(
    idempotencyKey,
    cancellationToken);

if (isProcessed)
{
    _logger.LogInformation("Event {EventId} already processed", eventId);
    return; // Safe to skip
}

// ... process event ...

await _idempotencyService.MarkAsProcessedAsync(
    idempotencyKey,
    TimeSpan.FromMinutes(10), // Standard window
    cancellationToken);
```

---

## Transactional Outbox Pattern

The **Outbox Pattern** ensures domain events are published reliably even if the message bus is unavailable.

### Outbox Table Schema

```sql
-- Location: Database migrations
CREATE TABLE messaging.outbox (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type VARCHAR(255) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE NOT NULL,
    processed_at TIMESTAMP WITH TIME ZONE NULL,
    failed_attempts INT NOT NULL DEFAULT 0,
    last_error TEXT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now()
);

CREATE INDEX idx_outbox_unprocessed 
    ON messaging.outbox(processed_at, occurred_at)
    WHERE processed_at IS NULL;
```

### Outbox Entity

```csharp
// Location: Src/Foundation/shared/Domain/Messaging/OutboxMessage.cs
namespace NorthStarET.Foundation.Domain.Messaging;

public sealed class OutboxMessage
{
    public required Guid Id { get; init; }
    public required string EventType { get; init; }
    public required string EventData { get; init; } // JSON serialized event
    public required DateTime OccurredAt { get; init; }
    public DateTime? ProcessedAt { get; set; }
    public int FailedAttempts { get; set; }
    public string? LastError { get; set; }
    public required DateTime CreatedAt { get; init; }
}
```

### Saving to Outbox on Event Publish

```csharp
// Location: Src/Foundation/shared/Infrastructure/Messaging/OutboxDomainEventPublisher.cs
using System.Text.Json;

namespace NorthStarET.Foundation.Infrastructure.Messaging;

/// <summary>
/// Publishes events to outbox table instead of directly to message bus
/// </summary>
public sealed class OutboxDomainEventPublisher : IDomainEventPublisher
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<OutboxDomainEventPublisher> _logger;
    
    public async Task PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken)
        where TEvent : class, IDomainEvent
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = typeof(TEvent).AssemblyQualifiedName!,
            EventData = JsonSerializer.Serialize(domainEvent),
            OccurredAt = domainEvent.OccurredAt,
            CreatedAt = DateTime.UtcNow
        };
        
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        
        _logger.LogInformation(
            "Saved event {EventType} to outbox with ID {OutboxId}",
            typeof(TEvent).Name,
            outboxMessage.Id);
    }
    
    public async Task PublishBatchAsync<TEvent>(
        IEnumerable<TEvent> domainEvents,
        CancellationToken cancellationToken)
        where TEvent : class, IDomainEvent
    {
        var outboxMessages = domainEvents.Select(e => new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = typeof(TEvent).AssemblyQualifiedName!,
            EventData = JsonSerializer.Serialize(e),
            OccurredAt = e.OccurredAt,
            CreatedAt = DateTime.UtcNow
        }).ToList();
        
        await _outboxRepository.AddRangeAsync(outboxMessages, cancellationToken);
        
        _logger.LogInformation(
            "Saved {Count} events to outbox",
            outboxMessages.Count);
    }
}
```

### Background Worker to Process Outbox

```csharp
// Location: Src/Foundation/shared/Infrastructure/Messaging/OutboxProcessorBackgroundService.cs
using MassTransit;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace NorthStarET.Foundation.Infrastructure.Messaging;

/// <summary>
/// Background service that processes outbox messages every 5 seconds
/// </summary>
public sealed class OutboxProcessorBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorBackgroundService> _logger;
    
    public OutboxProcessorBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessorBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        
        _logger.LogInformation("Outbox processor stopped");
    }
    
    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        // Fetch unprocessed messages (limit batch size)
        var messages = await outboxRepository.GetUnprocessedAsync(
            batchSize: 50,
            cancellationToken);
        
        if (!messages.Any())
        {
            return;
        }
        
        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);
        
        foreach (var message in messages)
        {
            try
            {
                // Deserialize event
                var eventType = Type.GetType(message.EventType)!;
                var domainEvent = JsonSerializer.Deserialize(message.EventData, eventType)!;
                
                // Publish to message bus
                await publishEndpoint.Publish(domainEvent, cancellationToken);
                
                // Mark as processed
                message.ProcessedAt = DateTime.UtcNow;
                await outboxRepository.UpdateAsync(message, cancellationToken);
                
                _logger.LogDebug(
                    "Published outbox message {OutboxId} to bus",
                    message.Id);
            }
            catch (Exception ex)
            {
                message.FailedAttempts++;
                message.LastError = ex.Message;
                await outboxRepository.UpdateAsync(message, cancellationToken);
                
                _logger.LogError(
                    ex,
                    "Failed to publish outbox message {OutboxId} (attempt {Attempts})",
                    message.Id,
                    message.FailedAttempts);
            }
        }
    }
}
```

---

## Event Schema Versioning

### Versioned Event Classes

```csharp
// Version 1 - Original schema
namespace NorthStarET.Foundation.Student.Domain.Events.V1;

public sealed record StudentCreatedEvent : TenantAwareDomainEvent
{
    public required Guid StudentId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

// Version 2 - Added DateOfBirth (breaking change)
namespace NorthStarET.Foundation.Student.Domain.Events.V2;

public sealed record StudentCreatedEvent : TenantAwareDomainEvent
{
    public required Guid StudentId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateTime DateOfBirth { get; init; } // NEW
}
```

### Consumer Supporting Multiple Versions

```csharp
// Location: Src/Foundation/services/Assessment/Application/Consumers/
using V1Event = NorthStarET.Foundation.Student.Domain.Events.V1.StudentCreatedEvent;
using V2Event = NorthStarET.Foundation.Student.Domain.Events.V2.StudentCreatedEvent;

namespace NorthStarET.Foundation.Assessment.Application.Consumers;

/// <summary>
/// Consumes both V1 and V2 StudentCreatedEvent
/// </summary>
public sealed class StudentCreatedEventConsumerV1 : IConsumer<V1Event>
{
    private readonly IAssessmentProfileRepository _repository;
    
    public async Task Consume(ConsumeContext<V1Event> context)
    {
        var studentEvent = context.Message;
        
        // Create profile without DateOfBirth (V1 doesn't have it)
        var profile = AssessmentProfile.Create(
            studentId: studentEvent.StudentId,
            tenantId: studentEvent.TenantId);
        
        await _repository.AddAsync(profile, context.CancellationToken);
        await _repository.SaveChangesAsync(context.CancellationToken);
    }
}

public sealed class StudentCreatedEventConsumerV2 : IConsumer<V2Event>
{
    private readonly IAssessmentProfileRepository _repository;
    
    public async Task Consume(ConsumeContext<V2Event> context)
    {
        var studentEvent = context.Message;
        
        // Create profile WITH DateOfBirth (V2)
        var profile = AssessmentProfile.CreateWithDateOfBirth(
            studentId: studentEvent.StudentId,
            tenantId: studentEvent.TenantId,
            dateOfBirth: studentEvent.DateOfBirth);
        
        await _repository.AddAsync(profile, context.CancellationToken);
        await _repository.SaveChangesAsync(context.CancellationToken);
    }
}
```

### Deprecation Window Policy

**Constitution Principle 4**: Provide deprecation windows for breaking changes.

```markdown
## Event Deprecation Process

1. **Announce deprecation** (release notes, internal docs)
   - "StudentCreatedEvent V1 deprecated, use V2"
   - Timeline: 3 months until removal

2. **Deploy V2 consumers** (all consuming services)
   - Ensure consumers support both V1 and V2

3. **Migrate producers** to V2
   - Student Service starts publishing V2 events

4. **Monitor V1 usage** (via metrics/logs)
   - Confirm no V1 events consumed for 1 month

5. **Remove V1 support**
   - Delete V1 consumer classes
   - Archive V1 event schema for reference
```

---

## Retry Policies & Dead Letter Queues

### MassTransit Retry Configuration

```csharp
// Location: Infrastructure/DependencyInjection.cs
x.UsingRabbitMq((context, cfg) =>
{
    cfg.Host("localhost");
    
    // Global retry policy
    cfg.UseMessageRetry(r => r.Incremental(
        retryLimit: 5,
        initialInterval: TimeSpan.FromSeconds(1),
        intervalIncrement: TimeSpan.FromSeconds(2)));
    // Retry schedule: 1s, 3s, 5s, 7s, 9s = total ~25s
    
    cfg.ConfigureEndpoints(context);
});
```

### Per-Consumer Retry Policy

```csharp
// Override global retry for specific consumer
cfg.ReceiveEndpoint("assessment-student-created", e =>
{
    e.UseMessageRetry(r => r.Exponential(
        retryLimit: 3,
        minInterval: TimeSpan.FromSeconds(1),
        maxInterval: TimeSpan.FromMinutes(1),
        intervalDelta: TimeSpan.FromSeconds(5)));
    // Retry schedule: 1s, 6s, 36s
    
    e.ConfigureConsumer<StudentCreatedEventConsumer>(context);
});
```

### Dead Letter Queue (RabbitMQ)

RabbitMQ automatically creates a dead letter exchange for failed messages:

```csharp
cfg.ReceiveEndpoint("assessment-student-created", e =>
{
    // After all retries exhausted, move to _error queue
    e.BindDeadLetterQueue("assessment-student-created_error");
    
    e.ConfigureConsumer<StudentCreatedEventConsumer>(context);
});
```

### Monitoring Dead Letter Queues

```csharp
// Location: Background service to alert on poison messages
public sealed class DeadLetterQueueMonitor : BackgroundService
{
    private readonly IManagementClient _rabbitMqClient; // RabbitMQ.Client
    private readonly ILogger<DeadLetterQueueMonitor> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var errorQueues = await _rabbitMqClient.GetQueuesAsync(".*_error");
            
            foreach (var queue in errorQueues)
            {
                if (queue.Messages > 10)
                {
                    _logger.LogError(
                        "Dead letter queue {QueueName} has {MessageCount} messages! " +
                        "Investigate poison messages.",
                        queue.Name,
                        queue.Messages);
                    
                    // TODO: Send alert (email, Slack, PagerDuty)
                }
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

---

## Testing with MassTransit Test Harness

### Unit Testing Event Publishing

```csharp
// Location: tests/unit/Student.Application.Tests/Commands/CreateStudentCommandHandlerTests.cs
using MassTransit.Testing;

namespace NorthStarET.Foundation.Student.Application.Tests.Commands;

public sealed class CreateStudentCommandHandlerTests
{
    [Fact]
    public async Task CreateStudent_Publishes_StudentCreatedEvent()
    {
        // Arrange
        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer<StudentCreatedEventConsumer>();
        
        await harness.Start();
        
        try
        {
            var handler = new CreateStudentCommandHandler(
                repository: CreateMockRepository(),
                eventPublisher: new MassTransitDomainEventPublisher(
                    harness.Bus,
                    NullLogger<MassTransitDomainEventPublisher>.Instance),
                tenantContext: CreateTenantContext(),
                dateTimeProvider: new FakeDateTimeProvider());
            
            var command = new CreateStudentCommand(
                FirstName: "John",
                LastName: "Doe",
                DateOfBirth: new DateTime(2010, 1, 1));
            
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            
            // Assert
            result.IsSuccess.Should().BeTrue();
            
            // Verify event was published
            (await harness.Published.Any<StudentCreatedEvent>()).Should().BeTrue();
            
            var publishedEvent = (await harness.Published
                .SelectAsync<StudentCreatedEvent>()
                .FirstOrDefault())!.Context.Message;
            
            publishedEvent.StudentId.Should().Be(result.Value);
            publishedEvent.FirstName.Should().Be("John");
            publishedEvent.LastName.Should().Be("Doe");
        }
        finally
        {
            await harness.Stop();
        }
    }
}
```

### Integration Testing Event Consumption

```csharp
// Location: tests/integration/Assessment.IntegrationTests/Consumers/StudentCreatedEventConsumerTests.cs
using MassTransit.Testing;

namespace NorthStarET.Foundation.Assessment.IntegrationTests.Consumers;

public sealed class StudentCreatedEventConsumerTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    [Fact]
    public async Task StudentCreatedEvent_Creates_AssessmentProfile()
    {
        // Arrange
        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => 
            new StudentCreatedEventConsumer(
                repository: _fixture.CreateAssessmentProfileRepository(),
                idempotencyService: CreateMockIdempotencyService(),
                logger: NullLogger<StudentCreatedEventConsumer>.Instance));
        
        await harness.Start();
        
        try
        {
            var studentId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            
            var studentCreatedEvent = new StudentCreatedEvent
            {
                EventId = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                TenantId = tenantId,
                StudentId = studentId,
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = new DateTime(2011, 5, 15)
            };
            
            // Act
            await harness.InputQueueSendEndpoint.Send(studentCreatedEvent);
            
            // Assert
            (await consumerHarness.Consumed.Any<StudentCreatedEvent>()).Should().BeTrue();
            
            // Verify assessment profile was created in database
            var dbContext = _fixture.CreateDbContext();
            var profile = await dbContext.AssessmentProfiles
                .FirstOrDefaultAsync(p => p.StudentId == studentId);
            
            profile.Should().NotBeNull();
            profile!.TenantId.Should().Be(tenantId);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
```

---

## RabbitMQ (Local) vs Azure Service Bus (Production)

### RabbitMQ for Local Development

**Aspire Configuration**:
```csharp
// AppHost/Program.cs
var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin() // Access http://localhost:15672
    .WithDataVolume();
```

**Management UI**: `http://localhost:15672` (guest/guest)

**Characteristics**:
- Fast local setup (Docker container)
- Management UI for debugging
- Topic exchanges for event fanout
- Persistent queues for durability

### Azure Service Bus for Production

**Configuration**:
```json
{
  "ConnectionStrings": {
    "ServiceBus": "Endpoint=sb://northstar.servicebus.windows.net/;..."
  }
}
```

**MassTransit Configuration**:
```csharp
x.UsingAzureServiceBus((context, cfg) =>
{
    cfg.Host(configuration.GetConnectionString("ServiceBus"));
    
    // Azure Service Bus supports topics and subscriptions natively
    cfg.ConfigureEndpoints(context);
});
```

**Characteristics**:
- Enterprise-grade reliability (99.9% SLA)
- Geo-replication for disaster recovery
- Native dead-lettering
- Session support for ordered message processing
- Built-in monitoring via Azure Monitor

### Environment-Based Selection

```csharp
// Automatically choose transport based on environment
var environment = configuration["ASPNETCORE_ENVIRONMENT"];

if (environment == "Production")
{
    x.UsingAzureServiceBus((context, cfg) => { /* ... */ });
}
else
{
    x.UsingRabbitMq((context, cfg) => { /* ... */ });
}
```

---

## Anti-Patterns

### ❌ Synchronous API Calls Disguised as Events

**Never**:
```csharp
// ❌ BAD: Event handler calls another service's API synchronously
public async Task Consume(ConsumeContext<StudentCreatedEvent> context)
{
    var httpClient = new HttpClient();
    var response = await httpClient.PostAsync(
        "https://intervention-service/api/students",
        JsonContent.Create(context.Message));
    
    // This defeats the purpose of event-driven architecture!
}
```

**Instead**:
```csharp
// ✅ GOOD: Each service consumes events independently
public async Task Consume(ConsumeContext<StudentCreatedEvent> context)
{
    // Intervention service has its own consumer
    // No synchronous coupling!
    var interventionProfile = InterventionProfile.Create(...);
    await _repository.AddAsync(interventionProfile, context.CancellationToken);
}
```

### ❌ Publishing Events Before Persisting Data

**Never**:
```csharp
// ❌ BAD: Publish before SaveChangesAsync
await _eventPublisher.PublishAsync(studentCreatedEvent, cancellationToken);
await _repository.SaveChangesAsync(cancellationToken);

// Problem: Consumers process event but data not yet committed!
```

**Instead**:
```csharp
// ✅ GOOD: Persist first, then publish
await _repository.SaveChangesAsync(cancellationToken);
await _eventPublisher.PublishAsync(studentCreatedEvent, cancellationToken);

// Or use Outbox Pattern for guaranteed delivery
```

### ❌ Non-Idempotent Consumers

**Never**:
```csharp
// ❌ BAD: No idempotency check
public async Task Consume(ConsumeContext<StudentCreatedEvent> context)
{
    var profile = AssessmentProfile.Create(...);
    await _repository.AddAsync(profile, context.CancellationToken);
    
    // Problem: Retry causes duplicate profiles!
}
```

**Instead**:
```csharp
// ✅ GOOD: Idempotency check first
public async Task Consume(ConsumeContext<StudentCreatedEvent> context)
{
    var idempotencyKey = $"student-created:{context.Message.EventId}";
    if (await _idempotencyService.IsProcessedAsync(idempotencyKey, cancellationToken))
    {
        return; // Already processed
    }
    
    var profile = AssessmentProfile.Create(...);
    await _repository.AddAsync(profile, context.CancellationToken);
    
    await _idempotencyService.MarkAsProcessedAsync(
        idempotencyKey, TimeSpan.FromMinutes(10), cancellationToken);
}
```

### ❌ Ignoring Message Ordering

**Never**:
```csharp
// ❌ BAD: Assuming events arrive in order
// StudentCreatedEvent and StudentDeletedEvent might arrive out of order!
public async Task Consume(ConsumeContext<StudentDeletedEvent> context)
{
    await _repository.DeleteAsync(context.Message.StudentId, cancellationToken);
    // What if StudentCreatedEvent arrives AFTER this?
}
```

**Instead**:
```csharp
// ✅ GOOD: Use version numbers or timestamps
public async Task Consume(ConsumeContext<StudentDeletedEvent> context)
{
    var student = await _repository.GetByIdAsync(context.Message.StudentId, cancellationToken);
    
    if (student == null)
    {
        _logger.LogWarning("Student {StudentId} not found, ignoring delete", ...);
        return; // Out-of-order arrival
    }
    
    if (student.CreatedAt > context.Message.OccurredAt)
    {
        _logger.LogWarning("Delete event older than student record, ignoring");
        return;
    }
    
    await _repository.DeleteAsync(student.Id, cancellationToken);
}
```

---

## Performance Considerations

### Message Throughput Tuning

```csharp
cfg.ReceiveEndpoint("assessment-student-created", e =>
{
    // PrefetchCount: messages fetched in advance
    e.PrefetchCount = 50; // Higher = better throughput, more memory
    
    // ConcurrentMessageLimit: parallel processing
    e.ConcurrentMessageLimit = 10; // Balance CPU vs. contention
    
    e.ConfigureConsumer<StudentCreatedEventConsumer>(context);
});
```

**Guidelines**:
- Low traffic (< 100 msg/min): `PrefetchCount=10`, `ConcurrentMessageLimit=2`
- Medium traffic (< 1000 msg/min): `PrefetchCount=50`, `ConcurrentMessageLimit=10`
- High traffic (> 1000 msg/min): `PrefetchCount=100`, `ConcurrentMessageLimit=20`

### Batch Publishing

```csharp
// Instead of publishing 100 events individually:
foreach (var student in students)
{
    await _eventPublisher.PublishAsync(
        new StudentCreatedEvent { ... },
        cancellationToken);
}
// Total time: 100 * 5ms = 500ms

// ✅ GOOD: Batch publish
var events = students.Select(s => new StudentCreatedEvent { ... }).ToList();
await _eventPublisher.PublishBatchAsync(events, cancellationToken);
// Total time: ~20ms (single network round-trip)
```

### Outbox Processing Interval

```csharp
// Trade-off: latency vs. database load
await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Standard

// High-traffic scenarios:
await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Lower latency

// Low-traffic scenarios:
await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Reduce DB queries
```

### Consumer Performance Targets

| Metric | Target |
|--------|--------|
| Message processing (simple) | <50ms (p95) |
| Message processing (complex) | <200ms (p95) |
| Queue depth (steady state) | <100 messages |
| Dead letter rate | <1% of total messages |
| Idempotency check latency | <5ms (Redis) |

---

## References

### Internal Documents
- [Constitution Principle 4: Event-Driven Data Discipline](../../.specify/memory/constitution.md#4-event-driven-data-discipline)
- [Domain Events Schema Standard](../architecture/domain-events-schema.md)
- [Clean Architecture Pattern](./clean-architecture.md)
- [Aspire Orchestration Pattern](./aspire-orchestration.md)
- [Idempotency & Caching Pattern](./caching-performance.md)

### External References
- [MassTransit Documentation](https://masstransit-project.com/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Azure Service Bus Documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [Transactional Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [Idempotent Consumer Pattern](https://www.enterpriseintegrationpatterns.com/patterns/messaging/IdempotentReceiver.html)

### Service Architectures
- [Student Management Service](../architecture/services/student-management-service-detailed.md)
- [Assessment Service](../architecture/services/assessment-service-detailed.md)
- [Intervention Management Service](../architecture/services/intervention-management-service-detailed.md)

---

**Version History**:
- 1.0.0 (2025-11-20): Initial messaging & integration pattern document
