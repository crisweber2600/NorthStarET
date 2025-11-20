# Foundation Layer: AppHost (.NET Aspire Orchestration)

**Purpose**: .NET Aspire AppHost orchestrates all Foundation services, databases, caches, and messaging infrastructure for local development and testing.

**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Overview

The `AppHost` project is the .NET Aspire orchestration entry point that:

- Defines service topology (PostgreSQL, Redis, Service Bus, services)
- Manages service dependencies (wait-for, health checks)
- Provides local development environment via Aspire Dashboard
- Configures service discovery and inter-service communication
- Enables integration testing with full hosting stack

**Run Command**: `dotnet run --project Src/Foundation/AppHost`  
**Dashboard**: `http://localhost:15888` (or as shown in terminal)

---

## Service Topology

### Phase 1 (Weeks 1-8)

```
AppHost
├── PostgreSQL (11 databases, one per service)
│   ├── Identity_DB
│   ├── Configuration_DB
│   └── ... (9 more databases for Phase 2-4 services)
├── Redis (distributed caching)
├── RabbitMQ (local dev messaging)
│
├── Identity Service (API + Worker)
├── API Gateway (YARP)
└── Configuration Service (API)
```

### Phase 2-4 (Weeks 9-28)

Additional services added incrementally:
- Student Management Service
- Staff Management Service
- Assessment Service
- Intervention Management Service
- Section & Roster Service
- Data Import & Integration Service
- Reporting & Analytics Service
- Content & Media Service

---

## AppHost Configuration

`Program.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume();

var redis = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithDataVolume();

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .WithDataVolume();

// Databases (per-service pattern)
var identityDb = postgres.AddDatabase("Identity_DB");
var configDb = postgres.AddDatabase("Configuration_DB");
var studentDb = postgres.AddDatabase("Student_DB");
// ... (8 more databases)

// Phase 1 Services
var identityService = builder.AddProject<Projects.Identity_Api>("identity")
    .WithReference(identityDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(identityDb)
    .WaitFor(redis)
    .WaitFor(rabbitmq);

var configService = builder.AddProject<Projects.Configuration_Api>("configuration")
    .WithReference(configDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(configDb)
    .WaitFor(redis)
    .WaitFor(rabbitmq);

var apiGateway = builder.AddProject<Projects.ApiGateway>("apigateway")
    .WithReference(identityService)
    .WithReference(configService)
    .WaitFor(identityService)
    .WaitFor(configService);

await builder.Build().RunAsync();
```

---

## Service Dependencies

### Phase 1 Foundation Services

```
┌───────────────────────────────────────┐
│        API Gateway (YARP)             │
│   (Routes to Identity + Config)       │
└───────────────┬───────────────────────┘
                │
        ┌───────┴────────┐
        │                │
        ▼                ▼
┌──────────────┐  ┌──────────────┐
│   Identity   │  │Configuration │
│   Service    │  │   Service    │
└──────┬───────┘  └──────┬───────┘
       │                 │
       └────────┬────────┘
                ▼
      ┌─────────────────┐
      │   PostgreSQL    │
      │  (11 databases) │
      └─────────────────┘
                ▼
      ┌─────────────────┐
      │      Redis      │
      │   (Caching)     │
      └─────────────────┘
                ▼
      ┌─────────────────┐
      │    RabbitMQ     │
      │  (Messaging)    │
      └─────────────────┘
```

**Startup Order**:
1. PostgreSQL (all databases)
2. Redis
3. RabbitMQ
4. Identity Service (depends on Identity_DB, Redis, RabbitMQ)
5. Configuration Service (depends on Configuration_DB, Redis, RabbitMQ)
6. API Gateway (depends on Identity + Configuration services)

---

## Local Development Workflow

### 1. Start AppHost

```bash
cd Src/Foundation/AppHost
dotnet run
```

### 2. Access Aspire Dashboard

Open `http://localhost:15888` in browser to view:

- **Resources Tab**: PostgreSQL, Redis, RabbitMQ, services with health status
- **Console Logs**: Real-time logs from all services
- **Traces**: Distributed tracing across service calls
- **Metrics**: Request rates, error rates, latency percentiles

### 3. Access Services

- **API Gateway**: `https://localhost:7001` (proxies to all services)
- **Identity Service**: `https://localhost:7002/api/identity/*`
- **Configuration Service**: `https://localhost:7003/api/configuration/*`
- **PostgreSQL**: `localhost:5432` (via PgAdmin: `http://localhost:5050`)
- **Redis**: `localhost:6379` (via RedisCommander: `http://localhost:8081`)
- **RabbitMQ**: `localhost:5672` (Management UI: `http://localhost:15672`)

### 4. Run Migrations

Aspire auto-applies EF Core migrations on startup (dev mode). Manual migration:

```bash
dotnet ef database update --project Src/Foundation/services/Identity/Identity.Infrastructure --startup-project Src/Foundation/AppHost
```

### 5. Run Tests

Aspire integration tests use the same AppHost configuration:

```bash
dotnet test Src/Foundation/services/Identity/Identity.Tests.Integration
```

---

## Environment Configuration

AppHost reads from `appsettings.json` and environment variables:

```json
{
  "Aspire": {
    "Dashboard": {
      "Port": 15888
    }
  },
  "Services": {
    "Identity": {
      "HttpPort": 7002,
      "HttpsPort": 7003
    },
    "Configuration": {
      "HttpPort": 7004,
      "HttpsPort": 7005
    },
    "ApiGateway": {
      "HttpPort": 7000,
      "HttpsPort": 7001
    }
  },
  "Databases": {
    "Postgres": {
      "Port": 5432,
      "Username": "postgres",
      "Password": "postgres"
    }
  }
}
```

**Production**: AppHost is NOT deployed. Azure resources (AKS, PostgreSQL, Service Bus) are provisioned separately. Services use Azure-managed infrastructure connection strings.

---

## Health Checks

Each service exposes health check endpoints:

- `/health` - Overall health (liveness probe)
- `/ready` - Readiness check (dependencies healthy)

AppHost dashboard aggregates health status:

```csharp
var identityService = builder.AddProject<Projects.Identity_Api>("identity")
    .WithHealthCheck("/health")
    .WithReadinessCheck("/ready");
```

---

## Service Discovery

Aspire provides built-in service discovery:

```csharp
// In Identity.Api, call Configuration.Api via service name
var httpClient = httpClientFactory.CreateClient();
var response = await httpClient.GetAsync("http://configuration/api/configuration/settings");
```

Aspire resolves `http://configuration` to actual URL automatically.

---

## Integration Testing

Aspire test projects use `DistributedApplicationTestingBuilder`:

```csharp
public class IdentityServiceTests : IAsyncLifetime
{
    private DistributedApplication _app;
    private HttpClient _httpClient;
    
    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();
        _app = await appHost.BuildAsync();
        await _app.StartAsync();
        
        _httpClient = _app.CreateHttpClient("identity");
    }
    
    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _httpClient.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        
        var health = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();
        Assert.Equal("Healthy", health.Status);
    }
    
    public async Task DisposeAsync()
    {
        await _app.DisposeAsync();
    }
}
```

---

## Troubleshooting

### PostgreSQL Connection Issues

Check Aspire dashboard → Resources → PostgreSQL → Details:

```bash
Host: localhost
Port: 5432
Database: Identity_DB
Username: postgres
Password: postgres
```

Test connection:

```bash
psql -h localhost -p 5432 -U postgres -d Identity_DB
```

### Redis Connection Issues

Check Aspire dashboard → Resources → Redis → Details. Test:

```bash
redis-cli -h localhost -p 6379 ping
```

### Service Not Starting

Check Aspire dashboard → Console Logs → Filter by service name. Common issues:

- **Port conflict**: Another process using ports 7000-7010
- **Database migration failure**: Check migrations exist, connection string correct
- **Missing dependency**: Service depends on another service not yet started (check `WaitFor`)

### Aspire Dashboard Not Accessible

Aspire dashboard URL shown in terminal output when running AppHost:

```
Now listening on: http://localhost:15888
```

If different port, update browser URL accordingly.

---

## Dependencies

- `Aspire.Hosting` - Aspire AppHost SDK
- `Aspire.Hosting.PostgreSQL` - PostgreSQL resource
- `Aspire.Hosting.Redis` - Redis resource
- `Aspire.Hosting.RabbitMQ` - RabbitMQ resource
- Service projects (Identity.Api, Configuration.Api, ApiGateway, etc.)

---

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Service Discovery](https://learn.microsoft.com/en-us/dotnet/aspire/service-discovery/overview)
- [Aspire Integration Testing](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing)
- [Constitution: Principle 1 - Clean Architecture & Aspire Orchestration](../../../.specify/memory/constitution.md)
- [LAYERS.md: Shared Infrastructure - ServiceDefaults](../../../Plan/Foundation/LAYERS.md#servicedefaults)

---

**Status**: To Be Implemented (Phase 1 - Week 1)  
**Related Spec**: [001-phase1-foundation-services](../../../Plan/Foundation/specs/Foundation/001-phase1-foundation-services/)
