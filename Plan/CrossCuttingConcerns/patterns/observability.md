# Observability Pattern

**Constitution Principle**: Principle 1 - Clean Architecture & Aspire Orchestration (Observability)  
**Priority**: 🟡 Medium  
**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Table of Contents

- [Overview](#overview)
- [Serilog Structured Logging](#serilog-structured-logging)
- [OpenTelemetry with Aspire Service Defaults](#opentelemetry-with-aspire-service-defaults)
- [Custom Activities and Correlation IDs](#custom-activities-and-correlation-ids)
- [Custom Metrics (IMeterFactory)](#custom-metrics-imeterfactory)
- [Health Checks (IHealthCheck)](#health-checks-ihealthcheck)
- [Application Insights Integration](#application-insights-integration)
- [Aspire Dashboard Usage](#aspire-dashboard-usage)
- [Distributed Tracing Patterns](#distributed-tracing-patterns)
- [Log Levels and Message Templates](#log-levels-and-message-templates)
- [Anti-Patterns](#anti-patterns)
- [Performance Considerations](#performance-considerations)
- [References](#references)

---

## Overview

NorthStar LMS follows Constitution Principle 1: all services use **.NET Aspire Service Defaults** for consistent observability (logging, metrics, tracing, health checks). Observability is critical for monitoring distributed microservices and diagnosing issues in production.

**Observability Pillars**:
```
┌─────────────────────────────────────────────────────────────┐
│                   Observability Stack                        │
├─────────────────────────────────────────────────────────────┤
│  📝 Structured Logging (Serilog)                            │
│     └─ Console, File, Application Insights                  │
│                                                              │
│  📊 Metrics (OpenTelemetry)                                 │
│     └─ Request rates, latencies, cache hits, queue depth    │
│                                                              │
│  🔍 Distributed Tracing (OpenTelemetry)                     │
│     └─ End-to-end request flows across services             │
│                                                              │
│  ❤️ Health Checks                                           │
│     └─ Database, Redis, RabbitMQ, custom checks             │
└─────────────────────────────────────────────────────────────┘
```

**Key Benefits**:
- **Correlation IDs**: Track requests across service boundaries
- **Structured logs**: Query-friendly JSON with consistent fields
- **Real-time metrics**: Dashboards for request rates, latencies, errors
- **Distributed traces**: Visualize call chains (API → Service A → Service B)
- **Health endpoints**: Kubernetes readiness/liveness probes

---

## Serilog Structured Logging

### Configuration in Program.cs

```csharp
// Location: Src/Foundation/services/Student/Api/Program.cs
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "NorthStarET.Student")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithMachineName()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
                           "{Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            new CompactJsonFormatter(),
            path: "logs/student-service-.json",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            restrictedToMinimumLevel: LogEventLevel.Information);
    
    // Add Application Insights sink in production
    if (context.HostingEnvironment.IsProduction())
    {
        var appInsightsConnectionString = context.Configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            configuration.WriteTo.ApplicationInsights(
                appInsightsConnectionString,
                TelemetryConverter.Traces);
        }
    }
});

var app = builder.Build();

// Use Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

app.Run();
```

### appsettings.json Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.AspNetCore.Authentication": "Information",
        "Microsoft.AspNetCore.Authorization": "Warning",
        "System": "Warning",
        "MassTransit": "Information"
      }
    },
    "Properties": {
      "Application": "NorthStarET.Student"
    }
  }
}
```

### Structured Logging Examples

```csharp
// Location: Application layer
public sealed class CreateStudentCommandHandler 
    : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly ILogger<CreateStudentCommandHandler> _logger;
    
    public async Task<Result<Guid>> Handle(
        CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        // ✅ Structured log with properties
        _logger.LogInformation(
            "Creating student {FirstName} {LastName} in tenant {TenantId}",
            command.FirstName,
            command.LastName,
            command.TenantId);
        
        try
        {
            var student = Student.Create(
                firstName: command.FirstName,
                lastName: command.LastName,
                dateOfBirth: command.DateOfBirth,
                tenantId: command.TenantId);
            
            await _repository.AddAsync(student, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Successfully created student {StudentId} in tenant {TenantId}",
                student.Id,
                student.TenantId);
            
            return Result.Success(student.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create student {FirstName} {LastName} in tenant {TenantId}",
                command.FirstName,
                command.LastName,
                command.TenantId);
            
            return Result.Failure<Guid>(StudentErrors.CreateFailed());
        }
    }
}
```

**Generated JSON Log**:
```json
{
  "@t": "2025-11-20T10:30:45.1234567Z",
  "@l": "Information",
  "@mt": "Successfully created student {StudentId} in tenant {TenantId}",
  "StudentId": "abc-123-student-id",
  "TenantId": "def-456-tenant-id",
  "Application": "NorthStarET.Student",
  "Environment": "Production",
  "MachineName": "student-api-pod-1"
}
```

---

## OpenTelemetry with Aspire Service Defaults

### Aspire Service Defaults Registration

```csharp
// Location: Src/Foundation/shared/ServiceDefaults/Extensions.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        
        return builder;
    }
    
    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter("MassTransit")
                    .AddMeter("Microsoft.EntityFrameworkCore")
                    .AddMeter("NorthStarET.*"); // Custom meters
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource("MassTransit")
                    .AddSource("NorthStarET.*"); // Custom activity sources
            });
        
        // Export to OTLP (Aspire Dashboard, Prometheus, Jaeger)
        builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
        builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        
        return builder;
    }
    
    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });
        
        return builder;
    }
    
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Health check endpoints
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
        
        return app;
    }
}
```

### Service Registration

```csharp
// Location: Src/Foundation/services/Student/Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Aspire Service Defaults (OpenTelemetry + Health Checks)
builder.AddServiceDefaults();

// Add application services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Map default endpoints (/health, /alive)
app.MapDefaultEndpoints();

app.Run();
```

---

## Custom Activities and Correlation IDs

### Creating Custom Activities

```csharp
// Location: Src/Foundation/shared/Infrastructure/Observability/ActivitySourceProvider.cs
using System.Diagnostics;

namespace NorthStarET.Foundation.Infrastructure.Observability;

public static class ActivitySourceProvider
{
    public static readonly ActivitySource StudentService = new("NorthStarET.Student", "1.0.0");
    public static readonly ActivitySource AssessmentService = new("NorthStarET.Assessment", "1.0.0");
}
```

### Using Activities in Command Handlers

```csharp
// Location: Application command handler
using System.Diagnostics;

public sealed class CreateStudentCommandHandler 
    : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySourceProvider.StudentService.StartActivity(
            "CreateStudent",
            ActivityKind.Internal);
        
        // Add tags for filtering/querying traces
        activity?.SetTag("student.firstName", command.FirstName);
        activity?.SetTag("student.lastName", command.LastName);
        activity?.SetTag("tenant.id", command.TenantId);
        
        try
        {
            var student = Student.Create(
                firstName: command.FirstName,
                lastName: command.LastName,
                dateOfBirth: command.DateOfBirth,
                tenantId: command.TenantId);
            
            await _repository.AddAsync(student, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            // Add result tag
            activity?.SetTag("student.id", student.Id);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return Result.Success(student.Id);
        }
        catch (Exception ex)
        {
            // Record exception in trace
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            
            throw;
        }
    }
}
```

### Correlation ID Propagation

```csharp
// Location: Middleware for correlation ID
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract or generate correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        
        // Add to response headers
        context.Response.Headers.Add(CorrelationIdHeader, correlationId);
        
        // Add to current activity
        Activity.Current?.SetTag("correlation.id", correlationId);
        
        // Add to log context
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

// Register in Program.cs
app.UseMiddleware<CorrelationIdMiddleware>();
```

---

## Custom Metrics (IMeterFactory)

### Custom Meter Creation

```csharp
// Location: Src/Foundation/shared/Infrastructure/Observability/StudentMetrics.cs
using System.Diagnostics.Metrics;

namespace NorthStarET.Foundation.Infrastructure.Observability;

public sealed class StudentMetrics
{
    private readonly Counter<long> _studentsCreated;
    private readonly Counter<long> _studentsDeleted;
    private readonly Histogram<double> _createStudentDuration;
    private readonly ObservableGauge<int> _activeStudents;
    
    public StudentMetrics(IMeterFactory meterFactory, IStudentRepository repository)
    {
        var meter = meterFactory.Create("NorthStarET.Student");
        
        _studentsCreated = meter.CreateCounter<long>(
            "students.created",
            description: "Number of students created");
        
        _studentsDeleted = meter.CreateCounter<long>(
            "students.deleted",
            description: "Number of students deleted");
        
        _createStudentDuration = meter.CreateHistogram<double>(
            "students.create.duration",
            unit: "ms",
            description: "Duration of student creation operations");
        
        _activeStudents = meter.CreateObservableGauge<int>(
            "students.active",
            observeValue: () => repository.GetActiveCountAsync(CancellationToken.None).Result,
            description: "Current number of active students");
    }
    
    public void RecordStudentCreated(Guid tenantId)
    {
        _studentsCreated.Add(1, new KeyValuePair<string, object?>("tenant.id", tenantId.ToString()));
    }
    
    public void RecordStudentDeleted(Guid tenantId)
    {
        _studentsDeleted.Add(1, new KeyValuePair<string, object?>("tenant.id", tenantId.ToString()));
    }
    
    public void RecordCreateDuration(double milliseconds, Guid tenantId)
    {
        _createStudentDuration.Record(milliseconds, new KeyValuePair<string, object?>("tenant.id", tenantId.ToString()));
    }
}
```

### Using Metrics in Handlers

```csharp
// Location: Application command handler
public sealed class CreateStudentCommandHandler 
    : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly StudentMetrics _metrics;
    
    public async Task<Result<Guid>> Handle(
        CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var student = Student.Create(
                firstName: command.FirstName,
                lastName: command.LastName,
                dateOfBirth: command.DateOfBirth,
                tenantId: command.TenantId);
            
            await _repository.AddAsync(student, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            
            stopwatch.Stop();
            
            // Record metrics
            _metrics.RecordStudentCreated(student.TenantId);
            _metrics.RecordCreateDuration(stopwatch.Elapsed.TotalMilliseconds, student.TenantId);
            
            return Result.Success(student.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create student");
            throw;
        }
    }
}
```

---

## Health Checks (IHealthCheck)

### Database Health Check

```csharp
// Location: Src/Foundation/shared/Infrastructure/HealthChecks/DatabaseHealthCheck.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NorthStarET.Foundation.Infrastructure.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly StudentDbContext _dbContext;
    
    public DatabaseHealthCheck(StudentDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple query to test connection
            await _dbContext.Database.CanConnectAsync(cancellationToken);
            
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection failed",
                exception: ex);
        }
    }
}
```

### Redis Health Check

```csharp
// Location: Src/Foundation/shared/Infrastructure/HealthChecks/RedisHealthCheck.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace NorthStarET.Foundation.Infrastructure.HealthChecks;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    
    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            await database.PingAsync();
            
            return HealthCheckResult.Healthy("Redis connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Redis connection failed",
                exception: ex);
        }
    }
}
```

### Registering Health Checks

```csharp
// Location: Infrastructure DependencyInjection.cs
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Add DbContext
    services.AddDbContext<StudentDbContext>(/* ... */);
    
    // Add Redis
    services.AddStackExchangeRedisCache(/* ... */);
    services.AddSingleton<IConnectionMultiplexer>(/* ... */);
    
    // Add health checks
    services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>(
            "database",
            tags: new[] { "ready", "db" })
        .AddCheck<RedisHealthCheck>(
            "redis",
            tags: new[] { "ready", "cache" });
    
    return services;
}
```

### Health Check Endpoints

```csharp
// Location: Program.cs
var app = builder.Build();

// Liveness probe (is service alive?)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});

// Readiness probe (is service ready to accept traffic?)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

// Detailed health check (all checks with results)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        };
        
        await context.Response.WriteAsJsonAsync(result);
    }
});
```

---

## Application Insights Integration

### Configuration

```csharp
// Location: Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});

// Serilog sink (already configured in Serilog section)
```

### Custom Events and Dependencies

```csharp
// Location: Application layer
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

public sealed class CreateStudentCommandHandler
{
    private readonly TelemetryClient _telemetryClient;
    
    public async Task<Result<Guid>> Handle(
        CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        // Track custom event
        var properties = new Dictionary<string, string>
        {
            { "TenantId", command.TenantId.ToString() },
            { "FirstName", command.FirstName },
            { "LastName", command.LastName }
        };
        
        _telemetryClient.TrackEvent("StudentCreated", properties);
        
        // Track external dependency (if calling external API)
        var dependency = new DependencyTelemetry
        {
            Name = "ExternalAPI",
            Type = "HTTP",
            Data = "POST /api/students",
            Timestamp = DateTimeOffset.UtcNow
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // ... create student ...
            
            dependency.Success = true;
            dependency.Duration = stopwatch.Elapsed;
            _telemetryClient.TrackDependency(dependency);
            
            return Result.Success(student.Id);
        }
        catch (Exception ex)
        {
            dependency.Success = false;
            dependency.Duration = stopwatch.Elapsed;
            _telemetryClient.TrackDependency(dependency);
            
            _telemetryClient.TrackException(ex);
            throw;
        }
    }
}
```

---

## Aspire Dashboard Usage

### Accessing the Dashboard

```bash
# Run AppHost to start Aspire Dashboard
dotnet run --project Src/Foundation/AppHost

# Dashboard URL (shown in console output)
# http://localhost:15000
```

### Dashboard Features

1. **Resources Tab**
   - View all services (APIs, databases, message queues)
   - Check health status (green/yellow/red)
   - View connection strings and environment variables

2. **Console Logs Tab**
   - Real-time structured logs from all services
   - Filter by service, log level, or search text
   - View log details (timestamp, properties, exception)

3. **Traces Tab**
   - Distributed traces across service boundaries
   - Waterfall view of request flow (API → DB → Queue → Consumer)
   - Filter by trace ID, duration, or service

4. **Metrics Tab**
   - Real-time charts for request rates, latencies, errors
   - Custom metrics (students.created, cache.hits, etc.)
   - Percentile graphs (p50, p95, p99)

### Example Distributed Trace

```
Trace: Create Student and Send Assessment Event
Duration: 145ms

┌─ [Student API] POST /api/students (100ms)
│  ├─ [EF Core] INSERT INTO students (15ms)
│  ├─ [Redis] SET session:abc-123 (3ms)
│  └─ [MassTransit] PUBLISH StudentCreatedEvent (25ms)
│
└─ [Assessment API] CONSUME StudentCreatedEvent (45ms)
   ├─ [EF Core] INSERT INTO assessment_profiles (20ms)
   └─ [Redis] SET idempotency:student-created:xyz (2ms)
```

---

## Distributed Tracing Patterns

### Trace Context Propagation

OpenTelemetry automatically propagates trace context via W3C Trace Context headers:

```http
GET /api/students/123 HTTP/1.1
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
tracestate: congo=t61rcWkgMzE
```

### Cross-Service Trace Example

```csharp
// Service A: Student API
[HttpPost("students")]
public async Task<IActionResult> CreateStudentAsync(
    [FromBody] CreateStudentRequest request,
    [FromServices] IMediator mediator)
{
    // Activity automatically created by ASP.NET Core instrumentation
    // Trace ID: 4bf92f3577b34da6a3ce929d0e0e4736
    // Span ID: 00f067aa0ba902b7
    
    var command = new CreateStudentCommand(
        FirstName: request.FirstName,
        LastName: request.LastName,
        DateOfBirth: request.DateOfBirth);
    
    var result = await mediator.Send(command);
    
    // Publish event (trace context propagated automatically)
    await _eventPublisher.PublishAsync(new StudentCreatedEvent { ... });
    
    return CreatedAtAction(nameof(GetStudentByIdAsync), new { id = result.Value }, result.Value);
}

// Service B: Assessment API (Event Consumer)
public async Task Consume(ConsumeContext<StudentCreatedEvent> context)
{
    // MassTransit automatically extracts trace context from message headers
    // Parent Span ID: 00f067aa0ba902b7 (from Student API)
    // New Span ID: a1b2c3d4e5f6g7h8
    
    var studentEvent = context.Message;
    
    var profile = AssessmentProfile.Create(
        studentId: studentEvent.StudentId,
        tenantId: studentEvent.TenantId);
    
    await _repository.AddAsync(profile, context.CancellationToken);
    await _repository.SaveChangesAsync(context.CancellationToken);
    
    // Trace shows parent-child relationship between API request and event consumption
}
```

---

## Log Levels and Message Templates

### Standard Log Levels

| Level | Usage | Examples |
|-------|-------|----------|
| `Trace` | Very detailed diagnostic info | "Entering method Foo with params X, Y" |
| `Debug` | Internal system events | "Cache miss for key student:123" |
| `Information` | General application flow | "Student abc-123 created successfully" |
| `Warning` | Abnormal events, recoverable | "Cache write failed, continuing without cache" |
| `Error` | Error events, operation failed | "Failed to create student: database timeout" |
| `Critical` | Application/system crash | "Database connection pool exhausted" |

### Message Template Best Practices

```csharp
// ✅ GOOD: Structured logging with properties
_logger.LogInformation(
    "Student {StudentId} created by user {UserId} in tenant {TenantId}",
    studentId, userId, tenantId);

// ❌ BAD: String interpolation (not structured)
_logger.LogInformation(
    $"Student {studentId} created by user {userId} in tenant {tenantId}");

// ✅ GOOD: Consistent property names across services
_logger.LogWarning(
    "Operation {OperationName} took {Duration}ms (threshold: {Threshold}ms)",
    "CreateStudent", duration, threshold);

// ❌ BAD: Inconsistent naming
_logger.LogWarning(
    "Op {OpName} took {DurationMs}ms (max: {MaxDuration}ms)",
    "CreateStudent", duration, threshold);
```

### Semantic Logging Helper

```csharp
// Location: Src/Foundation/shared/Infrastructure/Logging/LoggerExtensions.cs
public static class LoggerExtensions
{
    public static void LogStudentCreated(
        this ILogger logger,
        Guid studentId,
        Guid tenantId,
        string firstName,
        string lastName)
    {
        logger.LogInformation(
            "Student {StudentId} ({FirstName} {LastName}) created in tenant {TenantId}",
            studentId, firstName, lastName, tenantId);
    }
    
    public static void LogStudentNotFound(
        this ILogger logger,
        Guid studentId)
    {
        logger.LogWarning(
            "Student {StudentId} not found",
            studentId);
    }
    
    public static void LogDatabaseQuerySlow(
        this ILogger logger,
        string queryName,
        double durationMs)
    {
        logger.LogWarning(
            "Query {QueryName} took {Duration}ms (above threshold)",
            queryName, durationMs);
    }
}
```

---

## Anti-Patterns

### ❌ Logging Sensitive Data

**Never**:
```csharp
// ❌ BAD: Logging PII, passwords, tokens
_logger.LogInformation("User {Email} logged in with password {Password}", email, password);
_logger.LogDebug("Auth token: {Token}", jwtToken);
```

**Instead**:
```csharp
// ✅ GOOD: Log identifiers only
_logger.LogInformation("User {UserId} authenticated successfully", userId);
```

### ❌ Over-Logging in Hot Paths

**Never**:
```csharp
// ❌ BAD: Debug logging in high-traffic endpoint
public async Task<Student?> GetByIdAsync(Guid studentId)
{
    _logger.LogDebug("Querying student {StudentId}", studentId); // 10,000 times/sec!
    return await _dbContext.Students.FindAsync(studentId);
}
```

**Instead**:
```csharp
// ✅ GOOD: Log only at Information+ for hot paths
public async Task<Student?> GetByIdAsync(Guid studentId)
{
    // No logging for successful reads
    var student = await _dbContext.Students.FindAsync(studentId);
    
    if (student == null)
    {
        _logger.LogWarning("Student {StudentId} not found", studentId); // Only log miss
    }
    
    return student;
}
```

### ❌ Ignoring Trace Context

**Never**:
```csharp
// ❌ BAD: Creating new HTTP client without trace propagation
var httpClient = new HttpClient();
await httpClient.GetAsync("https://external-api/students");
// Trace context lost!
```

**Instead**:
```csharp
// ✅ GOOD: Use IHttpClientFactory (auto trace propagation)
var httpClient = _httpClientFactory.CreateClient("ExternalAPI");
await httpClient.GetAsync("https://external-api/students");
// Trace context propagated automatically
```

---

## Performance Considerations

### Sampling for High Traffic

```csharp
// Configure sampling for production
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.EnableAdaptiveSampling = true;
    options.SamplingPercentage = 5; // Sample 5% of traces
});
```

### Async Logging

Serilog uses async sinks by default, but for very high throughput:

```csharp
configuration.WriteTo.Async(a => a.File(
    path: "logs/student-service-.json",
    rollingInterval: RollingInterval.Day,
    buffered: true));
```

### Metric Cardinality

```csharp
// ❌ BAD: High cardinality tag (unique per user)
_counter.Add(1, new KeyValuePair<string, object?>("user.id", userId.ToString()));
// Creates millions of time series!

// ✅ GOOD: Low cardinality tag
_counter.Add(1, new KeyValuePair<string, object?>("tenant.id", tenantId.ToString()));
// Creates ~100 time series (100 tenants)
```

---

## References

### Internal Documents
- [Constitution Principle 1: Clean Architecture & Aspire Orchestration](../../.specify/memory/constitution.md#1-clean-architecture--aspire-orchestration)
- [Aspire Orchestration Pattern](./aspire-orchestration.md)
- [Testing Strategy](../standards/TESTING_STRATEGY.md)

### External References
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [Serilog Documentation](https://serilog.net/)
- [Application Insights Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

---

**Version History**:
- 1.0.0 (2025-11-20): Initial observability pattern document
