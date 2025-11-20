# Dependency Injection Pattern

**Constitution Principle**: Principle 1 - Clean Architecture & Aspire Orchestration  
**Priority**: 🟠 High  
**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Service Lifetimes](#service-lifetimes)
- [Layer-Specific Registration](#layer-specific-registration)
- [Shared Infrastructure DI](#shared-infrastructure-di)
- [Configuration Patterns](#configuration-patterns)
- [Testing with DI](#testing-with-di)
- [Anti-Patterns](#anti-patterns)
- [Performance Considerations](#performance-considerations)
- [References](#references)

---

## Overview

Dependency Injection (DI) is the mechanism for managing service dependencies in the NorthStar LMS platform. It enforces loose coupling, testability, and adheres to the Dependency Inversion Principle of Clean Architecture.

**Key Benefits**:
- **Testability**: Easy to swap implementations with mocks
- **Maintainability**: Change implementations without modifying consumers
- **Lifetime Management**: Framework handles object creation/disposal
- **Configuration**: Services configured in one place

---

## Core Concepts

### Dependency Inversion Principle

```
High-level modules should not depend on low-level modules.
Both should depend on abstractions.
```

```csharp
// ❌ BAD: Direct dependency on implementation
public class StudentCommandHandler
{
    private readonly StudentRepository _repository; // Tight coupling

    public StudentCommandHandler()
    {
        _repository = new StudentRepository(); // Hard to test!
    }
}

// ✅ GOOD: Dependency on abstraction
public class StudentCommandHandler
{
    private readonly IStudentRepository _repository; // Loose coupling

    public StudentCommandHandler(IStudentRepository repository)
    {
        _repository = repository; // Injected by DI container
    }
}
```

### Service Registration

Services are registered in `DependencyInjection.cs` files per layer:

```csharp
// Student.Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IStudentApplicationService, StudentApplicationService>();
        return services;
    }
}
```

---

## Service Lifetimes

### Singleton

**Lifetime**: Created once for application lifetime  
**Use For**: Stateless services, configuration, caching

```csharp
services.AddSingleton<IDistributedCache, RedisCache>();
services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
services.AddSingleton<IConfiguration>(Configuration);
```

**Example**:
```csharp
// Caching service (stateless, thread-safe)
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis; // Singleton Redis connection
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }
}
```

### Scoped

**Lifetime**: Created once per HTTP request (or DI scope)  
**Use For**: DbContext, repositories, unit of work, per-request state

```csharp
services.AddScoped<StudentDbContext>();
services.AddScoped<IStudentRepository, StudentRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ITenantContext, TenantContext>();
```

**Example**:
```csharp
// Repository (scoped to HTTP request)
public class StudentRepository : IStudentRepository
{
    private readonly StudentDbContext _context; // Scoped DbContext

    public StudentRepository(StudentDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _context.Students.FindAsync(id);
    }
}
```

### Transient

**Lifetime**: Created every time requested  
**Use For**: Lightweight, stateless services

```csharp
services.AddTransient<IEmailService, EmailService>();
services.AddTransient<IPasswordHasher, PasswordHasher>();
```

**Example**:
```csharp
// Email service (lightweight, no shared state)
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Sending email to {To}", to);
        // Send email logic
    }
}
```

---

## Layer-Specific Registration

### Domain Layer

Domain layer has **NO** dependencies (pure business logic).

```csharp
// No DependencyInjection.cs needed - domain entities are instantiated directly
var student = Student.Create(firstName, lastName, dateOfBirth, grade, districtId);
```

### Application Layer

Registers MediatR, FluentValidation, application services.

```csharp
// Student.Application/DependencyInjection.cs
using MediatR;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace NorthStar.Student.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR for CQRS
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Application services
        services.AddScoped<IStudentApplicationService, StudentApplicationService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();

        return services;
    }
}
```

### Infrastructure Layer

Registers DbContext, repositories, messaging, caching.

```csharp
// Student.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorthStar.Student.Application.Interfaces;
using NorthStar.Student.Infrastructure.Data;
using NorthStar.Student.Infrastructure.Data.Repositories;
using NorthStar.Student.Infrastructure.Caching;
using NorthStar.Student.Infrastructure.Messaging;
using MassTransit;
using StackExchange.Redis;

namespace NorthStar.Student.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database (Aspire-managed connection string)
        services.AddDbContext<StudentDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("student-db");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(StudentDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });
        });

        // Repositories
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Redis caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("redis");
            options.InstanceName = "NorthStar:Student:";
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        // MassTransit messaging
        services.AddMassTransit(x =>
        {
            // Register consumers
            x.AddConsumers(Assembly.GetExecutingAssembly());

            // Configure RabbitMQ (local) or Azure Service Bus (production)
            if (configuration.GetValue<bool>("UseAzureServiceBus"))
            {
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString("servicebus"));
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration.GetConnectionString("rabbitmq"));
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        // Domain event publisher
        services.AddScoped<IDomainEventPublisher, MassTransitDomainEventPublisher>();

        return services;
    }
}
```

### API Layer

Wires all layers together in `Program.cs`.

```csharp
// Student.API/Program.cs
using NorthStar.Student.Application;
using NorthStar.Student.Infrastructure;
using NorthStar.SharedKernel.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (logging, tracing, health checks)
builder.AddServiceDefaults();

// Add layer dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply EF Core migrations on startup (Aspire pattern)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints(); // Aspire health checks

app.Run();
```

---

## Shared Infrastructure DI

### ServiceDefaults (Shared Aspire Configuration)

Every service applies shared defaults.

```csharp
// Src/Foundation/shared/ServiceDefaults/Extensions.cs
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        // OpenTelemetry tracing
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource("MassTransit")
                .AddAzureMonitorTraceExporter());

        // Health checks
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        // Service discovery
        builder.Services.AddServiceDiscovery();

        // Resilience (Polly)
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }
}
```

**Usage in Service**:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults(); // One line applies all defaults
```

### Shared Domain Registration

```csharp
// Src/Foundation/shared/Domain/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
}
```

### Shared Infrastructure Registration

```csharp
// Src/Foundation/shared/Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddSingleton<IIdempotencyService, RedisIdempotencyService>();
        return services;
    }
}
```

---

## Configuration Patterns

### Options Pattern

```csharp
// appsettings.json
{
  "FeatureFlags": {
    "EnableNewDashboard": true,
    "EnableDigitalInk": false
  },
  "RateLimiting": {
    "MaxRequestsPerMinute": 100
  }
}
```

```csharp
// Configuration class
public class FeatureFlagOptions
{
    public bool EnableNewDashboard { get; set; }
    public bool EnableDigitalInk { get; set; }
}

// Registration
services.Configure<FeatureFlagOptions>(configuration.GetSection("FeatureFlags"));

// Usage via IOptions<T>
public class StudentController
{
    private readonly IOptions<FeatureFlagOptions> _featureFlags;

    public StudentController(IOptions<FeatureFlagOptions> featureFlags)
    {
        _featureFlags = featureFlags;
    }

    [HttpGet("dashboard")]
    public IActionResult GetDashboard()
    {
        if (_featureFlags.Value.EnableNewDashboard)
        {
            return Ok(new NewDashboard());
        }
        return Ok(new LegacyDashboard());
    }
}
```

### Named HttpClients

```csharp
// Registration
services.AddHttpClient("student-api", client =>
{
    client.BaseAddress = new Uri("https+http://student-api"); // Aspire service discovery
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler();

// Usage
public class AssessmentCommandHandler
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AssessmentCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("student-api");
        var response = await httpClient.GetAsync($"/api/students/{request.StudentId}", cancellationToken);
        // ...
    }
}
```

---

## Testing with DI

### Unit Tests (Mocking Dependencies)

```csharp
// tests/Unit/Student.Application.Tests/CreateStudentCommandHandlerTests.cs
using Moq;
using Xunit;
using FluentAssertions;

public class CreateStudentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesStudent()
    {
        // Arrange
        var repositoryMock = new Mock<IStudentRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        
        unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateStudentCommandHandler(repositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateStudentCommand("John", "Doe", DateTime.UtcNow.AddYears(-10), 5, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Integration Tests (Real DI Container)

```csharp
// tests/Integration/Student.API.Tests/StudentControllerTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class StudentControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StudentControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateStudent_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateStudentRequest { FirstName = "John", LastName = "Doe", Grade = 5 };

        // Act
        var response = await client.PostAsJsonAsync("/api/students", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### Test-Specific Service Overrides

```csharp
var factory = new WebApplicationFactory<Program>()
    .WithWebHostBuilder(builder =>
    {
        builder.ConfigureServices(services =>
        {
            // Remove real database
            services.RemoveAll<StudentDbContext>();
            
            // Add in-memory database
            services.AddDbContext<StudentDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // Mock external services
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, FakeEmailService>();
        });
    });
```

---

## Anti-Patterns

### ❌ Anti-Pattern 1: Service Locator

```csharp
// BAD: Service locator (anti-pattern)
public class StudentCommandHandler
{
    public async Task Handle(CreateStudentCommand request)
    {
        var repository = ServiceLocator.GetService<IStudentRepository>(); // Don't do this!
        await repository.AddAsync(student);
    }
}
```

**Why Bad**: Hides dependencies, makes testing hard, violates DI principle.

**✅ Correct**: Constructor injection
```csharp
public class StudentCommandHandler
{
    private readonly IStudentRepository _repository;

    public StudentCommandHandler(IStudentRepository repository)
    {
        _repository = repository; // Dependencies explicit and testable
    }
}
```

### ❌ Anti-Pattern 2: Captive Dependencies

```csharp
// BAD: Singleton service depends on Scoped service
services.AddSingleton<MyService>(); // Singleton
services.AddScoped<StudentDbContext>(); // Scoped

public class MyService
{
    private readonly StudentDbContext _context; // SCOPED INJECTED INTO SINGLETON!

    public MyService(StudentDbContext context)
    {
        _context = context; // DbContext will be held for entire app lifetime (memory leak!)
    }
}
```

**Why Bad**: Scoped service held by singleton causes memory leaks and stale data.

**✅ Correct**: Use IServiceScopeFactory
```csharp
public class MyService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MyService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task DoWork()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        // Use context within scope
    }
}
```

### ❌ Anti-Pattern 3: Constructor Over-Injection

```csharp
// BAD: Too many dependencies (code smell - violates SRP)
public class StudentCommandHandler
{
    private readonly IStudentRepository _studentRepo;
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly IDemographicsRepository _demographicsRepo;
    private readonly IContactRepository _contactRepo;
    private readonly ILogger _logger;
    private readonly IEmailService _emailService;
    private readonly IValidator _validator;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    // 9+ dependencies = too many!

    public StudentCommandHandler(/* 9 parameters */) { }
}
```

**Why Bad**: Class doing too much (violates Single Responsibility Principle).

**✅ Correct**: Split into smaller handlers or use Facade pattern
```csharp
public class CreateStudentCommandHandler
{
    private readonly IStudentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    // Only 2 dependencies!

    public CreateStudentCommandHandler(IStudentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
}
```

---

## Performance Considerations

### Minimize Scoped Services in Singletons

```csharp
// Expensive: Creates scope on every call
public class BackgroundService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public async Task ProcessAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
        // Process...
    }
}
```

**Optimization**: Batch processing, reuse scopes
```csharp
public async Task ProcessBatchAsync()
{
    using var scope = _scopeFactory.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
    
    var students = await repository.GetAllAsync();
    foreach (var student in students)
    {
        // Process all students in single scope
    }
}
```

### Avoid Resolving Services in Hot Paths

```csharp
// BAD: Resolve service on every request
[HttpGet]
public async Task<IActionResult> GetStudent(Guid id)
{
    var repository = HttpContext.RequestServices.GetRequiredService<IStudentRepository>(); // Don't do this!
    var student = await repository.GetByIdAsync(id);
    return Ok(student);
}
```

**✅ Correct**: Inject in constructor
```csharp
private readonly IStudentRepository _repository;

public StudentsController(IStudentRepository repository)
{
    _repository = repository; // Resolved once per controller instance
}

[HttpGet]
public async Task<IActionResult> GetStudent(Guid id)
{
    var student = await _repository.GetByIdAsync(id);
    return Ok(student);
}
```

---

## References

### Constitution & Architecture

- [NorthStarET Constitution v2.0.0](../../../.specify/memory/constitution.md) - Principle 1: Clean Architecture
- [LAYERS.md](../../LAYERS.md) - Layer dependency rules
- [ServiceDefaults README](../../../Src/Foundation/shared/ServiceDefaults/README.md)

### Service Architectures

- [Identity Service](../architecture/services/identity-service.md)
- [Student Management Service](../architecture/services/student-management-service.md)

### Related Patterns

- [Clean Architecture](./clean-architecture.md)
- [Aspire Orchestration](./aspire-orchestration.md)
- [Messaging & Integration](./messaging-integration.md)

### Microsoft Documentation

- [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Service Lifetimes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Options Pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)

---

**Last Updated**: 2025-11-20  
**Pattern Owner**: Platform Team  
**Constitution Version**: 2.2.0
