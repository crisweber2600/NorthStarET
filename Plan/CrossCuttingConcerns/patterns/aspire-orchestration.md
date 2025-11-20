# Aspire Orchestration Pattern

**Constitution Principle**: Principle 1 - Clean Architecture & Aspire Orchestration  
**Priority**: 🔴 Critical  
**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [AppHost Implementation](#apphost-implementation)
- [Service Registration Patterns](#service-registration-patterns)
- [Resource Dependencies](#resource-dependencies)
- [Service Discovery](#service-discovery)
- [Configuration Management](#configuration-management)
- [Health Checks & Resilience](#health-checks--resilience)
- [Testing Patterns](#testing-patterns)
- [Anti-Patterns](#anti-patterns)
- [Performance Considerations](#performance-considerations)
- [References](#references)

---

## Overview

.NET Aspire is the orchestration framework mandated by Constitution Principle 1 for the NorthStar LMS modernization. It provides:

- **Service Orchestration**: Coordinated startup of microservices, databases, message brokers, and caches
- **Resource Management**: Lifecycle management for PostgreSQL, Redis, RabbitMQ, Azure Service Bus
- **Service Discovery**: Automatic endpoint resolution between services
- **Configuration Injection**: Environment-aware configuration distribution
- **Developer Dashboard**: Real-time observability into running services and resources

**Why Aspire?**
- Eliminates Docker Compose complexity for local development
- Provides production-parity configuration management
- Integrates deeply with OpenTelemetry and Application Insights
- Supports database-per-service pattern with minimal configuration
- Enables dependency-aware startup ordering

---

## Core Concepts

### AppHost Project

The AppHost is the orchestration entry point that defines the entire distributed application topology.

**Location**: `Src/Foundation/AppHost/`

**Responsibilities**:
- Define all services, databases, caches, and message brokers
- Establish resource dependencies (e.g., API depends on PostgreSQL)
- Configure service-to-service communication
- Inject connection strings and configuration
- Enable distributed tracing and metrics collection

**Constitution Compliance**:
- ✅ All services MUST be orchestrated via AppHost (no ad-hoc Docker Compose files)
- ✅ Warnings treated as errors (`/warnaserror`)
- ✅ Database-per-service pattern enforced
- ✅ Cross-layer isolation validated at startup

### Service Registration

Services register with AppHost using `AddProject<T>()` method:

```csharp
var identityService = builder.AddProject<Projects.Identity_Api>("identity-api")
    .WithReference(identityDb)
    .WithReference(redis)
    .WaitFor(identityDb)
    .WaitFor(redis);
```

**Key Methods**:
- `AddProject<T>()` - Registers a .NET project as a service
- `WithReference()` - Injects resource connection strings into service
- `WaitFor()` - Ensures dependency is healthy before starting service
- `WithReplicas()` - Scales service instances (load testing)
- `WithEnvironment()` - Injects environment variables

### Resource Registration

Resources are infrastructure components managed by Aspire:

```csharp
// PostgreSQL database-per-service
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("identity-db")
    .AddDatabase("student-db")
    .AddDatabase("assessment-db");

// Redis Stack for caching
var redis = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithDataVolume();

// RabbitMQ for local messaging (Azure Service Bus in prod)
var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();
```

---

## AppHost Implementation

### Complete Example: Foundation Layer AppHost

```csharp
// Src/Foundation/AppHost/Program.cs
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// ============================================================
// INFRASTRUCTURE RESOURCES
// ============================================================

// PostgreSQL cluster with database-per-service
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume("northstar-postgres-data") // Persistent storage
    .AddDatabase("identity-db", "NorthStar_Identity")
    .AddDatabase("configuration-db", "NorthStar_Configuration")
    .AddDatabase("student-db", "NorthStar_Student")
    .AddDatabase("staff-db", "NorthStar_Staff")
    .AddDatabase("assessment-db", "NorthStar_Assessment")
    .AddDatabase("intervention-db", "NorthStar_Intervention")
    .AddDatabase("section-db", "NorthStar_Section")
    .AddDatabase("dataimport-db", "NorthStar_DataImport")
    .AddDatabase("reporting-db", "NorthStar_Reporting")
    .AddDatabase("content-db", "NorthStar_Content")
    .AddDatabase("digitalink-db", "NorthStar_DigitalInk");

// Redis Stack for session caching, idempotency windows, feature flags
var redis = builder.AddRedis("redis")
    .WithRedisCommander() // Web UI for Redis debugging
    .WithDataVolume("northstar-redis-data");

// RabbitMQ for local development (replaced by Azure Service Bus in production)
var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin(); // RabbitMQ Management UI at http://localhost:15672

// Azure Service Bus (production only - requires connection string)
var serviceBus = builder.AddAzureServiceBus("servicebus");

// ============================================================
// FOUNDATION LAYER SERVICES
// ============================================================

// Identity & Authentication Service (no upstream dependencies)
var identityDb = postgres.GetDatabase("identity-db");
var identityApi = builder.AddProject<Projects.Identity_Api>("identity-api")
    .WithReference(identityDb)
    .WithReference(redis)
    .WaitFor(identityDb)
    .WaitFor(redis)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithHttpsEndpoint(port: 7001, name: "https"); // Fixed port for local dev

// Configuration Service
var configDb = postgres.GetDatabase("configuration-db");
var configApi = builder.AddProject<Projects.Configuration_Api>("configuration-api")
    .WithReference(configDb)
    .WithReference(redis)
    .WithReference(rabbitMq) // Publishes DistrictConfigChangedEvent
    .WaitFor(configDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WithHttpsEndpoint(port: 7003, name: "https");

// Student Management Service
var studentDb = postgres.GetDatabase("student-db");
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(studentDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(identityApi) // For JWT validation
    .WaitFor(studentDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi) // Must wait for Identity to be healthy
    .WithHttpsEndpoint(port: 7004, name: "https");

// Staff Management Service
var staffDb = postgres.GetDatabase("staff-db");
var staffApi = builder.AddProject<Projects.Staff_Api>("staff-api")
    .WithReference(staffDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(identityApi)
    .WaitFor(staffDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi)
    .WithHttpsEndpoint(port: 7005, name: "https");

// Assessment Service
var assessmentDb = postgres.GetDatabase("assessment-db");
var assessmentApi = builder.AddProject<Projects.Assessment_Api>("assessment-api")
    .WithReference(assessmentDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(identityApi)
    .WithReference(studentApi) // For student validation
    .WaitFor(assessmentDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi)
    .WaitFor(studentApi) // Must wait for Student service
    .WithHttpsEndpoint(port: 7006, name: "https");

// Intervention Management Service
var interventionDb = postgres.GetDatabase("intervention-db");
var interventionApi = builder.AddProject<Projects.Intervention_Api>("intervention-api")
    .WithReference(interventionDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(identityApi)
    .WithReference(studentApi)
    .WithReference(assessmentApi) // Subscribes to AssessmentCompletedEvent
    .WaitFor(interventionDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi)
    .WaitFor(studentApi)
    .WaitFor(assessmentApi)
    .WithHttpsEndpoint(port: 7007, name: "https");

// Section & Roster Service
var sectionDb = postgres.GetDatabase("section-db");
var sectionApi = builder.AddProject<Projects.Section_Api>("section-api")
    .WithReference(sectionDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(identityApi)
    .WithReference(studentApi)
    .WithReference(staffApi) // For teacher assignments
    .WaitFor(sectionDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi)
    .WaitFor(studentApi)
    .WaitFor(staffApi)
    .WithHttpsEndpoint(port: 7008, name: "https");

// Data Import Service
var dataImportDb = postgres.GetDatabase("dataimport-db");
var dataImportApi = builder.AddProject<Projects.DataImport_Api>("dataimport-api")
    .WithReference(dataImportDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(identityApi)
    .WithReference(studentApi) // Bulk imports students
    .WithReference(staffApi)   // Bulk imports staff
    .WaitFor(dataImportDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi)
    .WaitFor(studentApi)
    .WaitFor(staffApi)
    .WithHttpsEndpoint(port: 7009, name: "https");

// Reporting & Analytics Service
var reportingDb = postgres.GetDatabase("reporting-db");
var reportingApi = builder.AddProject<Projects.Reporting_Api>("reporting-api")
    .WithReference(reportingDb)
    .WithReference(redis)
    .WithReference(rabbitMq) // Subscribes to all domain events for read models
    .WithReference(identityApi)
    .WaitFor(reportingDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi)
    .WithHttpsEndpoint(port: 7010, name: "https");

// Content & Media Service
var contentDb = postgres.GetDatabase("content-db");
var contentApi = builder.AddProject<Projects.Content_Api>("content-api")
    .WithReference(contentDb)
    .WithReference(redis)
    .WithReference(identityApi)
    .WaitFor(contentDb)
    .WaitFor(redis)
    .WaitFor(identityApi)
    .WithHttpsEndpoint(port: 7011, name: "https");

// ============================================================
// DIGITAL INK LAYER (Phase 4)
// ============================================================

var digitalInkDb = postgres.GetDatabase("digitalink-db");
var digitalInkApi = builder.AddProject<Projects.DigitalInk_Api>("digitalink-api")
    .WithReference(digitalInkDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(identityApi)
    .WithReference(assessmentApi) // Subscribes to AssessmentAssignedEvent
    .WithReference(studentApi)    // Subscribes to StudentWithdrawnEvent
    .WaitFor(digitalInkDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi)
    .WaitFor(assessmentApi)
    .WaitFor(studentApi)
    .WithHttpsEndpoint(port: 7013, name: "https");

// ============================================================
// API GATEWAY (YARP - Strangler Fig Pattern)
// ============================================================

var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithReference(identityApi) // For JWT validation
    .WithReference(redis)       // For rate limiting
    .WithReference(studentApi)
    .WithReference(staffApi)
    .WithReference(assessmentApi)
    .WithReference(interventionApi)
    .WithReference(sectionApi)
    .WithReference(dataImportApi)
    .WithReference(reportingApi)
    .WithReference(contentApi)
    .WithReference(digitalInkApi)
    .WaitFor(identityApi) // Gateway waits for all services
    .WaitFor(studentApi)
    .WaitFor(staffApi)
    .WaitFor(assessmentApi)
    .WaitFor(interventionApi)
    .WaitFor(sectionApi)
    .WaitFor(dataImportApi)
    .WaitFor(reportingApi)
    .WaitFor(contentApi)
    .WithHttpsEndpoint(port: 7000, name: "https"); // Gateway is public entry point

// ============================================================
// WEB UI (Razor Pages - Migrated UI Preservation)
// ============================================================

var webApp = builder.AddProject<Projects.Web>("web-app")
    .WithReference(apiGateway) // All HTTP calls go through gateway
    .WithReference(redis)      // For session storage
    .WaitFor(apiGateway)
    .WaitFor(redis)
    .WithHttpsEndpoint(port: 7002, name: "https");

builder.Build().Run();
```

**Key Patterns Demonstrated**:
1. **Database-per-Service**: Each service has its own PostgreSQL database
2. **Dependency Injection**: Services reference resources via `WithReference()`
3. **Startup Ordering**: `WaitFor()` ensures dependencies are healthy first
4. **Fixed Ports**: `WithHttpsEndpoint()` for predictable local development
5. **Layer Isolation**: DigitalInk layer services isolated from Foundation internals
6. **Gateway Pattern**: API Gateway is single public entry point

---

## Service Registration Patterns

### Pattern 1: Foundation Service (No Dependencies)

```csharp
// Identity Service has no upstream service dependencies
var identityApi = builder.AddProject<Projects.Identity_Api>("identity-api")
    .WithReference(identityDb)
    .WithReference(redis)
    .WaitFor(identityDb)
    .WaitFor(redis)
    .WithHttpsEndpoint(port: 7001, name: "https");
```

**When to Use**: Foundation services that provide core capabilities (Identity, Configuration)

### Pattern 2: Domain Service (Depends on Foundation)

```csharp
// Student Service depends on Identity for authentication
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(studentDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(identityApi) // Service reference for JWT validation
    .WaitFor(studentDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi) // Must wait for Identity to be healthy
    .WithHttpsEndpoint(port: 7004, name: "https");
```

**When to Use**: Domain services that require authentication/authorization

### Pattern 3: Orchestration Service (Depends on Multiple Services)

```csharp
// Assessment Service depends on Identity and Student
var assessmentApi = builder.AddProject<Projects.Assessment_Api>("assessment-api")
    .WithReference(assessmentDb)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithReference(identityApi)
    .WithReference(studentApi) // For student validation
    .WaitFor(assessmentDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi)
    .WaitFor(studentApi) // Ensure Student is healthy before starting
    .WithHttpsEndpoint(port: 7006, name: "https");
```

**When to Use**: Services that orchestrate workflows across multiple services

### Pattern 4: Event-Driven Service (Message Bus Only)

```csharp
// Reporting Service consumes domain events, doesn't make synchronous calls
var reportingApi = builder.AddProject<Projects.Reporting_Api>("reporting-api")
    .WithReference(reportingDb)
    .WithReference(redis)
    .WithReference(rabbitMq) // Only depends on message bus
    .WithReference(identityApi) // For authentication
    .WaitFor(reportingDb)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(identityApi)
    .WithHttpsEndpoint(port: 7010, name: "https");
```

**When to Use**: Services that build read models from domain events (CQRS pattern)

### Pattern 5: Gateway Service (Routes to All Services)

```csharp
// API Gateway references all backend services for routing
var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithReference(identityApi)
    .WithReference(studentApi)
    .WithReference(assessmentApi)
    // ... all other services
    .WaitFor(identityApi)
    .WaitFor(studentApi)
    .WaitFor(assessmentApi)
    // ... all other services
    .WithHttpsEndpoint(port: 7000, name: "https");
```

**When to Use**: API Gateway or BFF (Backend for Frontend) pattern

---

## Resource Dependencies

### PostgreSQL Database-per-Service

```csharp
// Create PostgreSQL cluster
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin() // Web UI at http://localhost:5050
    .WithDataVolume("northstar-postgres-data"); // Persistent storage

// Add database for each service
var identityDb = postgres.AddDatabase("identity-db", "NorthStar_Identity");
var studentDb = postgres.AddDatabase("student-db", "NorthStar_Student");

// Service references specific database
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(studentDb) // Injects connection string as "ConnectionStrings__student-db"
    .WaitFor(studentDb);      // Waits for database to be ready
```

**Connection String Injection**:
```csharp
// In Student.Api/Program.cs
builder.Services.AddDbContext<StudentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("student-db");
    options.UseNpgsql(connectionString);
});
```

**Constitution Compliance**:
- ✅ Each service owns its own database schema (no shared databases)
- ✅ Cross-service queries prohibited (must use domain events)
- ✅ Migrations run independently per service

### Redis Stack Caching

```csharp
// Single Redis instance shared across services
var redis = builder.AddRedis("redis")
    .WithRedisCommander() // Web UI at http://localhost:8081
    .WithDataVolume("northstar-redis-data");

// Services reference Redis for different purposes
var identityApi = builder.AddProject<Projects.Identity_Api>("identity-api")
    .WithReference(redis); // Uses for session caching

var assessmentApi = builder.AddProject<Projects.Assessment_Api>("assessment-api")
    .WithReference(redis); // Uses for idempotency windows
```

**Redis Usage Patterns**:
- **Identity Service**: Session storage with sliding expiration
- **All Services**: Idempotency windows (10-minute deduplication)
- **Configuration Service**: Feature flag caching
- **API Gateway**: Rate limiting counters

**Connection String Injection**:
```csharp
// In service Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});
```

### RabbitMQ (Local) vs Azure Service Bus (Production)

```csharp
// Local development - RabbitMQ
var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin(); // Management UI at http://localhost:15672

// Production - Azure Service Bus
var serviceBus = builder.AddAzureServiceBus("servicebus");

// Service registers both, uses environment-based selection
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);
```

**MassTransit Configuration**:
```csharp
// Infrastructure/DependencyInjection.cs
services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());

    if (builder.Environment.IsDevelopment())
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(builder.Configuration.GetConnectionString("servicebus"));
            cfg.ConfigureEndpoints(context);
        });
    }
});
```

---

## Service Discovery

Aspire provides automatic service discovery without manual configuration.

### Service-to-Service HTTP Calls

```csharp
// Assessment Service calls Student Service to validate student exists
public class AssessmentCommandHandler : IRequestHandler<CreateAssessmentCommand, Result<Guid>>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AssessmentCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<Guid>> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
    {
        // Aspire resolves "student-api" service name to actual endpoint
        var httpClient = _httpClientFactory.CreateClient("student-api");
        var response = await httpClient.GetAsync($"/api/students/{request.StudentId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result<Guid>.Failure("Student not found");
        }

        // Continue with assessment creation...
    }
}
```

**HttpClient Registration** (in service `Program.cs`):
```csharp
builder.Services.AddHttpClient("student-api", client =>
{
    // Aspire service discovery resolves this automatically
    client.BaseAddress = new Uri("https+http://student-api");
})
.AddStandardResilienceHandler(); // Polly retry/circuit breaker policies
```

**How It Works**:
1. AppHost defines service name: `builder.AddProject<Projects.Student_Api>("student-api")`
2. Aspire injects service discovery configuration into all services
3. HttpClient with name "student-api" resolves to actual HTTPS endpoint
4. Works identically in local dev, Azure Container Apps, Kubernetes

---

## Configuration Management

### AppHost Configuration Sources

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Configuration is read from:
// 1. appsettings.json (AppHost project)
// 2. appsettings.{Environment}.json
// 3. User secrets (dotnet user-secrets)
// 4. Environment variables
// 5. Azure App Configuration (production)

var postgresPassword = builder.Configuration["PostgreSQL:Password"] 
    ?? throw new InvalidOperationException("PostgreSQL password not configured");

var postgres = builder.AddPostgres("postgres", password: postgresPassword);
```

### Service Configuration Injection

```csharp
// Inject configuration from AppHost to service
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithEnvironment("FeatureFlags__EnableNewDashboard", "true")
    .WithEnvironment("Logging__LogLevel__Microsoft", "Warning")
    .WithReference(studentDb)  // Injects connection string
    .WithReference(redis);     // Injects Redis connection string
```

**Service Reads Configuration**:
```csharp
// Student.Api/Program.cs
var enableNewDashboard = builder.Configuration.GetValue<bool>("FeatureFlags:EnableNewDashboard");
var logLevel = builder.Configuration.GetValue<string>("Logging:LogLevel:Microsoft");
```

### User Secrets for Local Development

```bash
# Set PostgreSQL password in user secrets (never commit)
cd Src/Foundation/AppHost
dotnet user-secrets init
dotnet user-secrets set "PostgreSQL:Password" "DevPassword123!"

# Set Azure Service Bus connection string for testing
dotnet user-secrets set "AzureServiceBus:ConnectionString" "Endpoint=sb://..."
```

---

## Health Checks & Resilience

### ServiceDefaults Health Check Registration

```csharp
// Src/Foundation/shared/ServiceDefaults/Extensions.cs
public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy())
        .AddDbContextCheck<TDbContext>()
        .AddRedis(builder.Configuration.GetConnectionString("redis"))
        .AddRabbitMQ(rabbitConnectionString: builder.Configuration.GetConnectionString("rabbitmq"));

    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("MassTransit"));

    builder.Services.ConfigureHttpClientDefaults(http =>
    {
        http.AddStandardResilienceHandler(); // Polly retry/circuit breaker
        http.AddServiceDiscovery();
    });

    return builder;
}
```

### Service Health Check Endpoint

```csharp
// Every service Program.cs
var app = builder.Build();

app.MapDefaultEndpoints(); // Exposes /health and /alive endpoints

app.Run();
```

**Health Check Response**:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "self": {
      "status": "Healthy"
    },
    "student-db": {
      "status": "Healthy",
      "duration": "00:00:00.0050000"
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0020000"
    },
    "rabbitmq": {
      "status": "Healthy",
      "duration": "00:00:00.0030000"
    }
  }
}
```

### Polly Resilience Policies

```csharp
// Automatic retry, circuit breaker, and timeout for all HttpClient calls
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler(options =>
    {
        // Retry policy
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;

        // Circuit breaker
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        options.CircuitBreaker.FailureRatio = 0.5; // Open circuit if 50% fail
        options.CircuitBreaker.MinimumThroughput = 10;

        // Timeout
        options.Timeout.Timeout = TimeSpan.FromSeconds(30);
    });
});
```

---

## Testing Patterns

### Pattern 1: Integration Tests with Aspire Test Host

```csharp
// tests/Integration/NorthStar.Foundation.IntegrationTests/StudentServiceTests.cs
using Aspire.Hosting;
using Aspire.Hosting.Testing;

public class StudentServiceTests : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _studentClient;

    public async Task InitializeAsync()
    {
        // Start the entire Aspire application for integration testing
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // Get HttpClient for Student API
        _studentClient = _app.CreateHttpClient("student-api");
    }

    [Fact]
    public async Task CreateStudent_ValidData_ReturnsCreated()
    {
        // Arrange
        var createRequest = new CreateStudentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Grade = 5,
            DistrictId = Guid.NewGuid()
        };

        // Act
        var response = await _studentClient.PostAsJsonAsync("/api/students", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var studentId = await response.Content.ReadFromJsonAsync<Guid>();
        studentId.Should().NotBeEmpty();
    }

    public async Task DisposeAsync()
    {
        await _app?.StopAsync()!;
        _app?.Dispose();
        _studentClient?.Dispose();
    }
}
```

### Pattern 2: Testing Service Health Checks

```csharp
[Fact]
public async Task StudentService_HealthCheck_ReturnsHealthy()
{
    // Arrange
    var healthClient = _app.CreateHttpClient("student-api");

    // Act
    var response = await healthClient.GetAsync("/health");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var healthReport = await response.Content.ReadFromJsonAsync<HealthReport>();
    healthReport.Status.Should().Be("Healthy");
    healthReport.Entries["student-db"].Status.Should().Be("Healthy");
    healthReport.Entries["redis"].Status.Should().Be("Healthy");
}
```

### Pattern 3: Testing Service Discovery

```csharp
[Fact]
public async Task AssessmentService_CanDiscoverStudentService()
{
    // Arrange
    var assessmentClient = _app.CreateHttpClient("assessment-api");
    var studentId = await CreateTestStudent(); // Helper method

    // Act - Assessment service internally calls Student service
    var createAssessmentRequest = new CreateAssessmentRequest
    {
        StudentId = studentId,
        AssessmentType = "Math Fluency",
        ScheduledDate = DateTime.UtcNow
    };
    var response = await assessmentClient.PostAsJsonAsync("/api/assessments", createAssessmentRequest);

    // Assert - If service discovery fails, this would return 503 or 500
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### Pattern 4: Testing Database Migrations

```csharp
[Fact]
public async Task StudentService_DatabaseMigrations_Applied()
{
    // Arrange
    var studentClient = _app.CreateHttpClient("student-api");

    // Act - Aspire automatically applies EF Core migrations on startup
    var response = await studentClient.GetAsync("/api/students?page=1&pageSize=10");

    // Assert - If migrations didn't apply, this would fail
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

---

## Anti-Patterns

### ❌ Anti-Pattern 1: Hardcoded Connection Strings

```csharp
// BAD: Hardcoded connection string bypasses Aspire orchestration
services.AddDbContext<StudentDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=StudentDb;Username=postgres;Password=admin"));
```

**Why Bad**: Breaks when running in Aspire (different ports), production, or containers.

**✅ Correct Pattern**:
```csharp
// GOOD: Use Aspire-injected connection string
services.AddDbContext<StudentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("student-db");
    options.UseNpgsql(connectionString);
});
```

### ❌ Anti-Pattern 2: Missing WaitFor() Dependencies

```csharp
// BAD: Service starts before dependencies are ready
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(studentDb)
    .WithReference(redis);
    // Missing .WaitFor(studentDb) and .WaitFor(redis)
```

**Why Bad**: Service crashes on startup when trying to connect to unhealthy dependencies.

**✅ Correct Pattern**:
```csharp
// GOOD: Wait for dependencies to be healthy
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(studentDb)
    .WithReference(redis)
    .WaitFor(studentDb)
    .WaitFor(redis);
```

### ❌ Anti-Pattern 3: Circular Service Dependencies

```csharp
// BAD: Circular dependency between Assessment and Intervention
var assessmentApi = builder.AddProject<Projects.Assessment_Api>("assessment-api")
    .WithReference(interventionApi); // Assessment depends on Intervention

var interventionApi = builder.AddProject<Projects.Intervention_Api>("intervention-api")
    .WithReference(assessmentApi); // Intervention depends on Assessment (CIRCULAR!)
```

**Why Bad**: Deadlock during startup, violates event-driven architecture.

**✅ Correct Pattern**:
```csharp
// GOOD: Use domain events instead of synchronous calls
var assessmentApi = builder.AddProject<Projects.Assessment_Api>("assessment-api")
    .WithReference(rabbitMq); // Publishes AssessmentCompletedEvent

var interventionApi = builder.AddProject<Projects.Intervention_Api>("intervention-api")
    .WithReference(rabbitMq); // Subscribes to AssessmentCompletedEvent (async)
```

### ❌ Anti-Pattern 4: Shared Database Across Services

```csharp
// BAD: Multiple services sharing the same database
var sharedDb = postgres.AddDatabase("shared-db");
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(sharedDb);
var assessmentApi = builder.AddProject<Projects.Assessment_Api>("assessment-api")
    .WithReference(sharedDb); // SHARING DATABASE - VIOLATES PRINCIPLE 6!
```

**Why Bad**: Violates database-per-service pattern, creates tight coupling, prevents independent deployments.

**✅ Correct Pattern**:
```csharp
// GOOD: Each service has its own database
var studentDb = postgres.AddDatabase("student-db");
var assessmentDb = postgres.AddDatabase("assessment-db");

var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(studentDb);

var assessmentApi = builder.AddProject<Projects.Assessment_Api>("assessment-api")
    .WithReference(assessmentDb);
```

### ❌ Anti-Pattern 5: Running Aspire with Docker Compose

```yaml
# BAD: Do NOT use Docker Compose when using Aspire
version: '3.8'
services:
  postgres:
    image: postgres:16
    ports:
      - "5432:5432"
  redis:
    image: redis:7
    ports:
      - "6379:6379"
```

**Why Bad**: Duplicates Aspire functionality, creates configuration drift, breaks service discovery.

**✅ Correct Pattern**:
```csharp
// GOOD: Use Aspire for ALL orchestration
var postgres = builder.AddPostgres("postgres");
var redis = builder.AddRedis("redis");
```

---

## Performance Considerations

### Service Launch Times (SLO)

| Service Type | Target Startup Time | Health Check Pass Time |
|-------------|---------------------|------------------------|
| Identity API | < 5 seconds | < 2 seconds |
| Domain APIs | < 8 seconds | < 3 seconds |
| API Gateway | < 10 seconds | < 5 seconds (waits for all services) |
| Web UI | < 12 seconds | < 5 seconds |

**Measurement**:
```csharp
// Aspire Dashboard shows startup times in real-time
// Logs include startup timestamps:
// [2025-11-20 10:00:00] Application started in 4.2 seconds
```

### Resource Limits

```csharp
// Configure resource limits for containerized deployment
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(studentDb)
    .WithResourceLimits(cpu: 1.0, memory: 512); // 1 CPU, 512MB RAM
```

**Recommended Limits**:
- Identity Service: 0.5 CPU, 256MB RAM
- Domain Services: 1.0 CPU, 512MB RAM
- API Gateway: 1.0 CPU, 512MB RAM
- PostgreSQL: 2.0 CPU, 1GB RAM
- Redis: 0.5 CPU, 256MB RAM

### Connection Pooling

```csharp
// PostgreSQL connection pooling (EF Core)
services.AddDbContext<StudentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("student-db");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MaxBatchSize(100);
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    });
});

// Redis connection pooling (StackExchange.Redis)
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
    options.InstanceName = "NorthStar:";
});
```

### Service Replicas for Load Testing

```csharp
// Scale service instances for load testing
var studentApi = builder.AddProject<Projects.Student_Api>("student-api")
    .WithReference(studentDb)
    .WithReplicas(3); // Run 3 instances for load testing
```

---

## References

### Constitution & Layer Architecture

- [NorthStarET Constitution v2.0.0](../../../.specify/memory/constitution.md) - Principle 1: Clean Architecture & Aspire Orchestration
- [LAYERS.md](../../LAYERS.md) - Mono-repo layer structure and shared infrastructure
- [ServiceDefaults README](../../../Src/Foundation/shared/ServiceDefaults/README.md) - Shared Aspire configuration

### Service Architectures

- [Identity Service Architecture](../architecture/services/identity-service.md)
- [Student Management Service Architecture](../architecture/services/student-management-service.md)
- [Assessment Service Architecture](../architecture/services/assessment-service.md)

### Implementation Scenarios

- [Scenario 01: Identity Migration to Entra ID](../../Foundation/scenarios/01-identity-migration-entra-id.md)
- [Scenario 02: Multi-Tenant Database Architecture](../../Foundation/scenarios/02-multi-tenant-database-architecture.md)
- [Scenario 06: API Gateway Orchestration](../../Foundation/scenarios/06-api-gateway-orchestration.md)

### Microsoft Documentation

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Service Discovery](https://learn.microsoft.com/en-us/dotnet/aspire/service-discovery/overview)
- [Aspire Testing](https://learn.microsoft.com/en-us/dotnet/aspire/testing/overview)
- [Aspire PostgreSQL Integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-component)
- [Aspire Redis Integration](https://learn.microsoft.com/en-us/dotnet/aspire/caching/stackexchange-redis-component)

### Related Patterns

- [Clean Architecture](./clean-architecture.md) - Layer separation enforced by Aspire
- [Dependency Injection](./dependency-injection.md) - Service registration patterns
- [Messaging & Integration](./messaging-integration.md) - Event-driven communication
- [Observability](./observability.md) - OpenTelemetry integration with Aspire

---

**Last Updated**: 2025-11-20  
**Pattern Owner**: Platform Team  
**Constitution Version**: 2.2.0
