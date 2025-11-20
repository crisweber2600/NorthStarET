# Caching & Performance Pattern

**Constitution Principle**: Principle 4 - Event-Driven Data Discipline (Performance & Resilience)  
**Priority**: 🟡 Medium  
**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Table of Contents

- [Overview](#overview)
- [Redis Stack Configuration](#redis-stack-configuration)
- [IDistributedCache Patterns](#idistributedcache-patterns)
- [Cache-Aside (Lazy Loading)](#cache-aside-lazy-loading)
- [Write-Through Cache](#write-through-cache)
- [Event-Driven Cache Invalidation](#event-driven-cache-invalidation)
- [Session Caching (Identity Service)](#session-caching-identity-service)
- [Idempotency Windows (10 Minutes)](#idempotency-windows-10-minutes)
- [Performance SLOs](#performance-slos)
- [Testing with Redis Test Containers](#testing-with-redis-test-containers)
- [Anti-Patterns](#anti-patterns)
- [Performance Tuning](#performance-tuning)
- [References](#references)

---

## Overview

NorthStar LMS uses **Redis Stack** for distributed caching to achieve sub-200ms API response times (p95) and ensure resilience. Caching strategies follow Constitution Principle 4: event-driven patterns with idempotency guarantees.

**Caching Architecture**:
```
┌─────────────────────────────────────────────────────────────┐
│                        API Request                           │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                  Application Layer (MediatR)                 │
│  ✓ Check Redis Cache First   ✓ Query Handler                │
└───────────┬────────────────────────────────┬────────────────┘
            │ Cache Hit (<5ms)                │ Cache Miss
            │                                 │
            ▼                                 ▼
    ┌───────────────┐               ┌──────────────────┐
    │  Redis Stack  │               │  PostgreSQL DB   │
    │  (In-Memory)  │               │  (Disk-Based)    │
    └───────────────┘               └─────────┬────────┘
                                              │
                                    Warm Cache │
                                              ▼
                                    ┌───────────────┐
                                    │  Redis Stack  │
                                    │  (Write-Back) │
                                    └───────────────┘
```

**Use Cases**:
- Session caching (Identity Service: 8-hour sessions, 30-min sliding window)
- Idempotency windows (10-minute deduplication for events/commands)
- Frequently accessed data (student rosters, assessment results)
- Permission checks (user roles per tenant)
- Configuration data (feature flags, settings)

---

## Redis Stack Configuration

### Aspire AppHost Configuration

```csharp
// Location: Src/Foundation/AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Add Redis Stack with persistent data volume
var redis = builder.AddRedis("cache")
    .WithDataVolume()
    .WithRedisCommander(); // Optional: Web UI at http://localhost:8081

// Add services with Redis reference
builder.AddProject<Projects.IdentityApi>("identity-api")
    .WithReference(redis)
    .WaitFor(redis);

builder.Build().Run();
```

### Service Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Redis": {
    "InstanceName": "northstar:",
    "DefaultSlidingExpiration": "00:30:00",
    "DefaultAbsoluteExpiration": "02:00:00"
  }
}
```

### DependencyInjection Registration

```csharp
// Location: Src/Foundation/shared/Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace NorthStarET.Foundation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // IDistributedCache implementation with Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = configuration["Redis:InstanceName"] ?? "northstar:";
            
            // Connection pool configuration
            options.ConfigurationOptions = new ConfigurationOptions
            {
                EndPoints = { configuration.GetConnectionString("Redis")! },
                ConnectRetry = 3,
                ConnectTimeout = 5000,
                SyncTimeout = 5000,
                AbortOnConnectFail = false, // Continue if Redis unavailable
                KeepAlive = 60
            };
        });
        
        // Register IConnectionMultiplexer for advanced Redis operations
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = configuration.GetConnectionString("Redis")!;
            return ConnectionMultiplexer.Connect(connectionString);
        });
        
        return services;
    }
}
```

### Production Configuration (Azure Redis Cache)

```json
{
  "ConnectionStrings": {
    "Redis": "{redis-name}.redis.cache.windows.net:6380,password={access-key},ssl=True,abortConnect=False"
  },
  "Redis": {
    "InstanceName": "northstar-prod:",
    "DefaultSlidingExpiration": "00:30:00"
  }
}
```

---

## IDistributedCache Patterns

### Basic Cache Operations

```csharp
// Location: Application layer service
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

public sealed class StudentQueryService
{
    private readonly IDistributedCache _cache;
    private readonly IStudentRepository _repository;
    
    public async Task<Student?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var cacheKey = $"student:{studentId}";
        
        // 1. Try cache first
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        
        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<Student>(cachedData);
        }
        
        // 2. Cache miss: query database
        var student = await _repository.GetByIdAsync(studentId, cancellationToken);
        
        if (student == null)
        {
            return null;
        }
        
        // 3. Warm cache for next request
        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
        };
        
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(student),
            cacheOptions,
            cancellationToken);
        
        return student;
    }
}
```

### Cache Key Naming Convention

```csharp
// Standard naming: {entity-type}:{entity-id}[:{sub-key}]
var studentKey = $"student:{studentId}";
var sessionKey = $"session:{sessionId}";
var permissionKey = $"permissions:{userId}:{tenantId}";
var idempotencyKey = $"idempotency:student-created:{eventId}";

// Tenant-scoped keys
var tenantStudentListKey = $"tenant:{tenantId}:students:active";

// Time-based keys (for analytics)
var dailyStatsKey = $"stats:daily:{DateTime.UtcNow:yyyy-MM-dd}";
```

---

## Cache-Aside (Lazy Loading)

**Pattern**: Check cache first, query database on miss, warm cache.

### Implementation

```csharp
// Location: Src/Foundation/services/Student/Application/Queries/GetStudentByIdQueryHandler.cs
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace NorthStarET.Foundation.Student.Application.Queries;

public sealed class GetStudentByIdQueryHandler 
    : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
{
    private readonly IStudentRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetStudentByIdQueryHandler> _logger;
    
    public async Task<Result<StudentDto>> Handle(
        GetStudentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"student:{query.StudentId}";
        
        // 1. Check cache
        var cachedStudent = await GetFromCacheAsync(cacheKey, cancellationToken);
        if (cachedStudent != null)
        {
            _logger.LogDebug("Cache hit for student {StudentId}", query.StudentId);
            return Result.Success(cachedStudent);
        }
        
        _logger.LogDebug("Cache miss for student {StudentId}", query.StudentId);
        
        // 2. Query database
        var student = await _repository.GetByIdAsync(query.StudentId, cancellationToken);
        
        if (student == null)
        {
            return Result.Failure<StudentDto>(
                StudentErrors.NotFound(query.StudentId));
        }
        
        var studentDto = StudentDto.FromEntity(student);
        
        // 3. Warm cache
        await SetCacheAsync(cacheKey, studentDto, cancellationToken);
        
        return Result.Success(studentDto);
    }
    
    private async Task<StudentDto?> GetFromCacheAsync(
        string key,
        CancellationToken cancellationToken)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key, cancellationToken);
            
            return cachedData != null
                ? JsonSerializer.Deserialize<StudentDto>(cachedData)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read from cache, key: {CacheKey}", key);
            return null; // Degrade gracefully
        }
    }
    
    private async Task SetCacheAsync(
        string key,
        StudentDto value,
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            };
            
            await _cache.SetStringAsync(
                key,
                JsonSerializer.Serialize(value),
                cacheOptions,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write to cache, key: {CacheKey}", key);
            // Don't throw - cache failure shouldn't break query
        }
    }
}
```

---

## Write-Through Cache

**Pattern**: Update cache immediately when entity is modified.

### Implementation

```csharp
// Location: Src/Foundation/services/Student/Application/Commands/UpdateStudentCommandHandler.cs
public sealed class UpdateStudentCommandHandler 
    : IRequestHandler<UpdateStudentCommand, Result>
{
    private readonly IStudentRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UpdateStudentCommandHandler> _logger;
    
    public async Task<Result> Handle(
        UpdateStudentCommand command,
        CancellationToken cancellationToken)
    {
        var student = await _repository.GetByIdAsync(command.StudentId, cancellationToken);
        
        if (student == null)
        {
            return Result.Failure(StudentErrors.NotFound(command.StudentId));
        }
        
        // Update entity
        student.UpdateName(command.FirstName, command.LastName);
        student.UpdateDateOfBirth(command.DateOfBirth);
        
        // Save to database
        await _repository.UpdateAsync(student, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        
        // Write-through: Update cache immediately
        var cacheKey = $"student:{student.Id}";
        var studentDto = StudentDto.FromEntity(student);
        
        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
        };
        
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(studentDto),
            cacheOptions,
            cancellationToken);
        
        _logger.LogInformation(
            "Updated student {StudentId} in database and cache",
            student.Id);
        
        return Result.Success();
    }
}
```

---

## Event-Driven Cache Invalidation

**Pattern**: Invalidate cache entries when domain events are published.

### Cache Invalidation Consumer

```csharp
// Location: Src/Foundation/services/Assessment/Application/Consumers/StudentUpdatedEventConsumer.cs
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NorthStarET.Foundation.Student.Domain.Events;

namespace NorthStarET.Foundation.Assessment.Application.Consumers;

/// <summary>
/// Invalidates cached assessment data when student is updated
/// </summary>
public sealed class StudentUpdatedEventConsumer : IConsumer<StudentUpdatedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<StudentUpdatedEventConsumer> _logger;
    
    public async Task Consume(ConsumeContext<StudentUpdatedEvent> context)
    {
        var studentEvent = context.Message;
        
        // Invalidate student-related caches in Assessment Service
        var cacheKeys = new[]
        {
            $"assessment-profile:{studentEvent.StudentId}",
            $"student-assessments:{studentEvent.StudentId}",
            $"tenant:{studentEvent.TenantId}:assessment-summary"
        };
        
        foreach (var key in cacheKeys)
        {
            try
            {
                await _cache.RemoveAsync(key, context.CancellationToken);
                
                _logger.LogDebug(
                    "Invalidated cache key {CacheKey} due to StudentUpdatedEvent {EventId}",
                    key,
                    studentEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to invalidate cache key {CacheKey}",
                    key);
            }
        }
    }
}
```

### Delete with Write-Through Invalidation

```csharp
// Location: Application command handler
public async Task<Result> Handle(
    DeleteStudentCommand command,
    CancellationToken cancellationToken)
{
    var student = await _repository.GetByIdAsync(command.StudentId, cancellationToken);
    
    if (student == null)
    {
        return Result.Failure(StudentErrors.NotFound(command.StudentId));
    }
    
    // Soft delete
    student.MarkAsDeleted();
    await _repository.UpdateAsync(student, cancellationToken);
    await _repository.SaveChangesAsync(cancellationToken);
    
    // Invalidate all related caches
    var cacheKey = $"student:{student.Id}";
    await _cache.RemoveAsync(cacheKey, cancellationToken);
    
    // Invalidate list caches (may contain this student)
    var listCacheKey = $"tenant:{student.TenantId}:students:active";
    await _cache.RemoveAsync(listCacheKey, cancellationToken);
    
    _logger.LogInformation(
        "Deleted student {StudentId} and invalidated caches",
        student.Id);
    
    return Result.Success();
}
```

---

## Session Caching (Identity Service)

**Pattern**: Cache user sessions with 8-hour absolute + 30-minute sliding expiration.

### Session Creation with Cache

```csharp
// Location: Src/Foundation/services/Identity/Infrastructure/Services/TokenExchangeService.cs
public async Task<Guid> ExchangeEntraTokenForSessionAsync(
    string entraToken,
    CancellationToken cancellationToken)
{
    // ... extract claims from JWT ...
    
    var sessionId = Guid.NewGuid();
    var session = new UserSession
    {
        SessionId = sessionId,
        UserId = userId,
        TenantId = tenantId,
        Email = email,
        Roles = roles.ToList(),
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddHours(8), // 8-hour absolute
        LastAccessedAt = DateTime.UtcNow
    };
    
    // 1. Persist to database (source of truth)
    await _sessionRepository.CreateAsync(session, cancellationToken);
    
    // 2. Cache with sliding expiration
    var cacheKey = $"session:{sessionId}";
    var cacheOptions = new DistributedCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(30), // Extend on access
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8) // Hard limit
    };
    
    await _cache.SetStringAsync(
        cacheKey,
        JsonSerializer.Serialize(session),
        cacheOptions,
        cancellationToken);
    
    _logger.LogInformation(
        "Created session {SessionId} for user {UserId}, cached with 30min sliding / 8hr absolute expiration",
        sessionId,
        userId);
    
    return sessionId;
}
```

### Session Retrieval (Cache-First)

```csharp
// Location: Src/Foundation/services/Identity/Infrastructure/Authentication/SessionAuthenticationHandler.cs
protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
{
    var sessionId = ExtractSessionIdFromHeader();
    
    var cacheKey = $"session:{sessionId}";
    
    // 1. Check cache first
    var cachedSessionJson = await _cache.GetStringAsync(cacheKey, Context.RequestAborted);
    
    UserSession? session;
    
    if (cachedSessionJson != null)
    {
        // Cache hit: <5ms response
        session = JsonSerializer.Deserialize<UserSession>(cachedSessionJson);
        _logger.LogDebug("Session {SessionId} loaded from cache", sessionId);
    }
    else
    {
        // Cache miss: query database
        session = await _sessionRepository.GetByIdAsync(sessionId, Context.RequestAborted);
        
        if (session == null)
        {
            return AuthenticateResult.Fail("Session not found");
        }
        
        // Warm cache with sliding expiration
        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
        
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(session),
            cacheOptions,
            Context.RequestAborted);
        
        _logger.LogInformation("Session {SessionId} loaded from database and cached", sessionId);
    }
    
    // Validate expiry
    if (session.ExpiresAt < DateTime.UtcNow)
    {
        await InvalidateSessionAsync(sessionId);
        return AuthenticateResult.Fail("Session expired");
    }
    
    // Build claims principal
    var principal = BuildClaimsPrincipal(session);
    var ticket = new AuthenticationTicket(principal, Scheme.Name);
    
    return AuthenticateResult.Success(ticket);
}
```

---

## Idempotency Windows (10 Minutes)

**Pattern**: Deduplicate events/commands using Redis with 10-minute TTL.

### IIdempotencyService Implementation

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
        var value = Encoding.UTF8.GetBytes($"processed-{DateTime.UtcNow:O}");
        
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

### Usage in Event Consumer

```csharp
// Standard 10-minute window across all consumers
public async Task Consume(ConsumeContext<StudentCreatedEvent> context)
{
    var studentEvent = context.Message;
    
    var idempotencyKey = $"student-created:{studentEvent.EventId}";
    
    // Check if already processed
    if (await _idempotencyService.IsProcessedAsync(idempotencyKey, context.CancellationToken))
    {
        _logger.LogInformation("Event {EventId} already processed, skipping", studentEvent.EventId);
        return; // Safe to skip
    }
    
    // Process event
    var assessmentProfile = AssessmentProfile.Create(
        studentId: studentEvent.StudentId,
        tenantId: studentEvent.TenantId);
    
    await _repository.AddAsync(assessmentProfile, context.CancellationToken);
    await _repository.SaveChangesAsync(context.CancellationToken);
    
    // Mark as processed with 10-minute window
    await _idempotencyService.MarkAsProcessedAsync(
        idempotencyKey,
        TimeSpan.FromMinutes(10), // Standard window
        context.CancellationToken);
}
```

---

## Performance SLOs

### Target Latencies

| Operation | Target (p95) | Cache Strategy |
|-----------|--------------|----------------|
| Session validation | <5ms | Cache-first (sliding 30min) |
| Student query by ID | <10ms | Cache-aside (30min) |
| Permission check | <10ms | Cache-aside (15min) |
| List query (paginated) | <50ms | Cache list results (5min) |
| Aggregate query | <100ms | Cache aggregates (10min) |
| Idempotency check | <5ms | Absolute expiry (10min) |

### Cache Hit Ratio Targets

- Session cache: **>95%** hit ratio
- Entity cache (by ID): **>80%** hit ratio
- Permission cache: **>90%** hit ratio
- Idempotency cache: **>99%** hit ratio (within window)

### Monitoring Cache Performance

```csharp
// Location: Src/Foundation/shared/Infrastructure/Monitoring/CacheMetrics.cs
using System.Diagnostics.Metrics;

public sealed class CacheMetrics
{
    private readonly Counter<long> _cacheHits;
    private readonly Counter<long> _cacheMisses;
    private readonly Histogram<double> _cacheLatency;
    
    public CacheMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("NorthStarET.Cache");
        
        _cacheHits = meter.CreateCounter<long>(
            "cache.hits",
            description: "Number of cache hits");
        
        _cacheMisses = meter.CreateCounter<long>(
            "cache.misses",
            description: "Number of cache misses");
        
        _cacheLatency = meter.CreateHistogram<double>(
            "cache.latency",
            unit: "ms",
            description: "Cache operation latency");
    }
    
    public void RecordHit(string cacheType)
    {
        _cacheHits.Add(1, new KeyValuePair<string, object?>("cache.type", cacheType));
    }
    
    public void RecordMiss(string cacheType)
    {
        _cacheMisses.Add(1, new KeyValuePair<string, object?>("cache.type", cacheType));
    }
    
    public void RecordLatency(double milliseconds, string operation)
    {
        _cacheLatency.Record(milliseconds, new KeyValuePair<string, object?>("cache.operation", operation));
    }
}
```

---

## Testing with Redis Test Containers

### Integration Test Setup

```csharp
// Location: tests/integration/Shared/RedisFixture.cs
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace NorthStarET.Foundation.IntegrationTests.Shared;

public sealed class RedisFixture : IAsyncLifetime
{
    private readonly IContainer _redisContainer;
    
    public string ConnectionString { get; private set; } = null!;
    
    public RedisFixture()
    {
        _redisContainer = new ContainerBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
        
        var port = _redisContainer.GetMappedPublicPort(6379);
        ConnectionString = $"localhost:{port}";
    }
    
    public async Task DisposeAsync()
    {
        await _redisContainer.DisposeAsync();
    }
    
    public IDistributedCache CreateDistributedCache()
    {
        var services = new ServiceCollection();
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = ConnectionString;
            options.InstanceName = "test:";
        });
        
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IDistributedCache>();
    }
}
```

### Cache Integration Test

```csharp
// Location: tests/integration/Student.IntegrationTests/Caching/StudentCacheTests.cs
namespace NorthStarET.Foundation.Student.IntegrationTests.Caching;

public sealed class StudentCacheTests : IClassFixture<RedisFixture>
{
    private readonly RedisFixture _redisFixture;
    
    public StudentCacheTests(RedisFixture redisFixture)
    {
        _redisFixture = redisFixture;
    }
    
    [Fact]
    public async Task GetStudent_Returns_Cached_Value_On_Second_Call()
    {
        // Arrange
        var cache = _redisFixture.CreateDistributedCache();
        var studentId = Guid.NewGuid();
        var studentDto = new StudentDto
        {
            Id = studentId,
            FirstName = "John",
            LastName = "Doe"
        };
        
        var cacheKey = $"student:{studentId}";
        
        // Act: Set cache
        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(studentDto),
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
        
        // Act: Retrieve from cache
        var cachedData = await cache.GetStringAsync(cacheKey);
        var cachedStudent = JsonSerializer.Deserialize<StudentDto>(cachedData!);
        
        // Assert
        cachedStudent.Should().NotBeNull();
        cachedStudent!.Id.Should().Be(studentId);
        cachedStudent.FirstName.Should().Be("John");
    }
    
    [Fact]
    public async Task Cache_Expires_After_Sliding_Window()
    {
        // Arrange
        var cache = _redisFixture.CreateDistributedCache();
        var key = "test:sliding-expiry";
        
        await cache.SetStringAsync(
            key,
            "value",
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(2)
            });
        
        // Act: Wait 1 second, access to extend sliding window
        await Task.Delay(TimeSpan.FromSeconds(1));
        var value1 = await cache.GetStringAsync(key);
        
        // Wait another 1 second (within sliding window)
        await Task.Delay(TimeSpan.FromSeconds(1));
        var value2 = await cache.GetStringAsync(key);
        
        // Wait 3 seconds (beyond sliding window)
        await Task.Delay(TimeSpan.FromSeconds(3));
        var value3 = await cache.GetStringAsync(key);
        
        // Assert
        value1.Should().Be("value"); // Still valid
        value2.Should().Be("value"); // Extended by access
        value3.Should().BeNull(); // Expired
    }
}
```

---

## Anti-Patterns

### ❌ Caching Without Expiration

**Never**:
```csharp
// ❌ BAD: No expiration = stale data forever
await _cache.SetStringAsync(cacheKey, data);
```

**Instead**:
```csharp
// ✅ GOOD: Always set expiration
await _cache.SetStringAsync(
    cacheKey,
    data,
    new DistributedCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(30),
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
    });
```

### ❌ Caching Sensitive Data

**Never**:
```csharp
// ❌ BAD: Caching passwords, PII without encryption
var cacheKey = $"user:{userId}:password";
await _cache.SetStringAsync(cacheKey, passwordHash);
```

**Instead**:
```csharp
// ✅ GOOD: Only cache non-sensitive identifiers
var cacheKey = $"user:{userId}:profile";
var profile = new { UserId = userId, Email = email }; // No password
await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(profile));
```

### ❌ Ignoring Cache Failures

**Never**:
```csharp
// ❌ BAD: Let cache failure break query
var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
return JsonSerializer.Deserialize<StudentDto>(cachedData); // Throws if null!
```

**Instead**:
```csharp
// ✅ GOOD: Degrade gracefully on cache failure
try
{
    var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
    if (cachedData != null)
    {
        return JsonSerializer.Deserialize<StudentDto>(cachedData);
    }
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Cache read failed, falling back to database");
}

// Fallback to database
return await _repository.GetByIdAsync(studentId, cancellationToken);
```

---

## Performance Tuning

### Redis Connection Pool

```csharp
// Tune connection pool for high throughput
options.ConfigurationOptions = new ConfigurationOptions
{
    EndPoints = { connectionString },
    ConnectRetry = 3,
    ConnectTimeout = 5000,
    SyncTimeout = 5000,
    AsyncTimeout = 5000,
    AbortOnConnectFail = false,
    KeepAlive = 60,
    ConnectRetry = 3,
    ReconnectRetryPolicy = new ExponentialRetry(1000) // 1s, 2s, 4s...
};
```

### Batch Operations

```csharp
// Instead of N individual cache gets:
foreach (var studentId in studentIds)
{
    var key = $"student:{studentId}";
    var data = await _cache.GetStringAsync(key); // N round-trips
}

// ✅ GOOD: Batch with IConnectionMultiplexer
var database = _connectionMultiplexer.GetDatabase();
var keys = studentIds.Select(id => (RedisKey)$"student:{id}").ToArray();
var values = await database.StringGetAsync(keys); // 1 round-trip
```

### Cache Compression (Large Objects)

```csharp
// For large DTOs, compress before caching
using System.IO.Compression;

public static async Task SetCompressedAsync(
    this IDistributedCache cache,
    string key,
    string data,
    DistributedCacheEntryOptions options,
    CancellationToken cancellationToken)
{
    using var memoryStream = new MemoryStream();
    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        await gzipStream.WriteAsync(bytes, cancellationToken);
    }
    
    await cache.SetAsync(key, memoryStream.ToArray(), options, cancellationToken);
}
```

---

## References

### Internal Documents
- [Constitution Principle 4: Event-Driven Data Discipline](../../.specify/memory/constitution.md#4-event-driven-data-discipline)
- [Messaging Integration Pattern](./messaging-integration.md)
- [Multi-Tenancy Pattern](./multi-tenancy.md)
- [Security & Compliance](../standards/security-compliance.md)

### External References
- [Redis Documentation](https://redis.io/documentation)
- [IDistributedCache Documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [Azure Redis Cache Best Practices](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-best-practices)

---

**Version History**:
- 1.0.0 (2025-11-20): Initial caching & performance pattern document
