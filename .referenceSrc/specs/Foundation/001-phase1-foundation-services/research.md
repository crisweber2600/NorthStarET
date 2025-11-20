# Phase 0: Research - Phase 1 Foundation Services

**Date**: 2025-11-19  
**Purpose**: Resolve technical unknowns and establish architectural patterns for Identity, Gateway, and Configuration services

---

## Research Areas

### 1. .NET Aspire 13.0.0 Orchestration Patterns

**Question**: How do we configure and orchestrate multiple microservices with PostgreSQL, Redis, and Azure Service Bus using .NET Aspire 13.0.0?

**Decision**: Use Aspire AppHost with service defaults pattern

**Rationale**:
- Aspire 13.0.0 provides built-in service discovery, configuration, and observability
- `ServiceDefaults` project provides shared health checks, OpenTelemetry, and logging configuration
- AppHost `Program.cs` declares all services and dependencies in a single orchestration point
- Built-in support for PostgreSQL, Redis, and Azure Service Bus via resource APIs
- Development dashboard at `http://localhost:15888` provides real-time monitoring

**Implementation Pattern**:
```csharp
// NorthStar.AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure resources
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("identitydb")
    .AddDatabase("configdb");

var redis = builder.AddRedis("redis");

var serviceBus = builder.AddAzureServiceBus("servicebus");

var keyVault = builder.AddAzureKeyVault("keyvault");

// Services
var identityApi = builder.AddProject<Projects.NorthStar_Identity_Api>("identity-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(serviceBus)
    .WithReference(keyVault);

var configApi = builder.AddProject<Projects.NorthStar_Configuration_Api>("configuration-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(serviceBus);

var gateway = builder.AddProject<Projects.NorthStar_Gateway>("gateway")
    .WithReference(identityApi)
    .WithReference(configApi);

await builder.Build().RunAsync();
```

**Alternatives Considered**:
- Docker Compose: Rejected - less .NET integration, no built-in observability dashboard
- Manual service discovery: Rejected - reinventing Aspire capabilities
- Kubernetes directly: Rejected for development - Aspire simplifies local development

**References**:
- [.NET Aspire 13.0.0 Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [WIPNorthStar AppHost implementation](../../Src/WIPNorthStar/NorthStarET.Lms/src/AppHost/Program.cs)

---

### 2. PostgreSQL Multi-Tenancy with Row-Level Security

**Question**: How do we implement strict tenant isolation in PostgreSQL using Row-Level Security (RLS) policies while maintaining performance?

**Decision**: Use `tenant_id` column + PostgreSQL RLS policies + application-level session variables

**Rationale**:
- Database-per-service architecture (moving away from database-per-district)
- All tenant data shares same tables but isolated by `tenant_id` column
- PostgreSQL RLS enforces tenant boundaries at database level (defense-in-depth)
- Application sets tenant context via `SET LOCAL app.tenant_id = 'xxx'` in transactions
- EF Core interceptors automatically inject tenant filtering into all queries
- Performance impact minimal (<5ms overhead) with proper indexing

**Implementation Pattern**:

**Database Schema**:
```sql
-- Enable RLS on all tenant-scoped tables
CREATE TABLE districts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    name VARCHAR(200) NOT NULL,
    code VARCHAR(50) NOT NULL,
    CONSTRAINT uk_districts_code_tenant UNIQUE (code, tenant_id)
);

-- Create RLS policy
ALTER TABLE districts ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_policy ON districts
    USING (tenant_id = current_setting('app.tenant_id')::UUID);

-- Indexes for performance
CREATE INDEX idx_districts_tenant_id ON districts(tenant_id);
```

**EF Core Configuration**:
```csharp
// Infrastructure/Persistence/TenantDbInterceptor.cs
public class TenantDbInterceptor : DbCommandInterceptor
{
    private readonly ITenantContext _tenantContext;

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        if (_tenantContext.TenantId != null)
        {
            // Set tenant context for this transaction
            command.CommandText = $"SET LOCAL app.tenant_id = '{_tenantContext.TenantId}'; " 
                                + command.CommandText;
        }
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}

// Application layer sets tenant context from JWT claims
services.AddScoped<ITenantContext>(sp =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var tenantId = httpContext?.User?.FindFirst("tenant_id")?.Value;
    return new TenantContext(tenantId != null ? Guid.Parse(tenantId) : null);
});
```

**Alternatives Considered**:
- Database-per-tenant: Rejected - operational complexity (100s of databases)
- Schema-per-tenant: Rejected - PostgreSQL schema limitations, migration complexity
- Application-only filtering: Rejected - insufficient defense-in-depth
- Discriminator column only: Rejected - no database-level enforcement

**References**:
- [PostgreSQL Row-Level Security](https://www.postgresql.org/docs/16/ddl-rowsecurity.html)
- [Multi-Tenant Architecture Best Practices](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/overview)
- Master Migration Plan: Section on Multi-Tenancy Architecture

---

### 3. YARP (Yet Another Reverse Proxy) Configuration

**Question**: How do we configure YARP for API Gateway routing, authentication validation, rate limiting, and circuit breakers?

**Decision**: YARP 2.2.0 with JSON configuration + custom middleware for cross-cutting concerns

**Rationale**:
- YARP is Microsoft's official reverse proxy built on ASP.NET Core
- Supports dynamic configuration reloading without restart
- Built-in load balancing, health checks, and retry policies
- Extensible via ASP.NET Core middleware pipeline
- High performance (~10ms routing overhead)

**Implementation Pattern**:

**appsettings.json Configuration**:
```json
{
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity-cluster",
        "Match": {
          "Path": "/api/identity/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      },
      "configuration-route": {
        "ClusterId": "configuration-cluster",
        "Match": {
          "Path": "/api/configuration/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      },
      "legacy-route": {
        "ClusterId": "legacy-cluster",
        "Match": {
          "Path": "/api/legacy/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://identity-api:8080"
          }
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:10",
            "Timeout": "00:00:05",
            "Policy": "ConsecutiveFailures",
            "Path": "/health"
          }
        }
      },
      "configuration-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://configuration-api:8080"
          }
        }
      },
      "legacy-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://oldnorthstar:5000"
          }
        }
      }
    }
  }
}
```

**Custom Middleware**:
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); // Aspire service defaults

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<AuthenticationTransformProvider>();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

await app.RunAsync();
```

**Alternatives Considered**:
- Nginx: Rejected - less .NET integration, manual configuration
- Azure API Management: Rejected - too heavy for internal gateway, cost
- Ocelot: Rejected - YARP is newer, better maintained, Microsoft-backed
- Envoy: Rejected - operational complexity, C++ learning curve

**References**:
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [YARP Authentication & Authorization](https://microsoft.github.io/reverse-proxy/articles/authn-authz.html)

---

### 4. Microsoft Entra ID Integration (WIPNorthStar Feature 001 Patterns)

**Question**: How do we implement Microsoft Entra ID SSO with local authentication fallback following proven WIPNorthStar patterns?

**Decision**: Use Microsoft.Identity.Web 3.5.0 with OpenID Connect + local Identity fallback

**Rationale**:
- WIPNorthStar Feature 001 provides proven implementation (70% complete)
- Microsoft.Identity.Web abstracts Entra ID complexity
- Supports both web app and web API authentication flows
- Built-in token validation, claims transformation
- Local Identity as fallback maintains system availability during Entra ID outages

**Implementation Pattern**:

**Entra ID Configuration**:
```csharp
// Identity.Api/Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

// appsettings.json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<client-id>",
    "ClientSecret": "<from-keyvault>",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  }
}
```

**Hybrid Authentication Strategy**:
```csharp
// Application/Commands/LoginCommand.cs
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Try Entra ID first
        if (await _entraIdService.IsAvailableAsync())
        {
            return await AuthenticateWithEntraIdAsync(request);
        }
        
        // Fallback to local authentication
        _logger.LogWarning("Entra ID unavailable, using local authentication");
        return await AuthenticateLocallyAsync(request);
    }
    
    private async Task<LoginResult> AuthenticateLocallyAsync(LoginCommand request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return LoginResult.Failed("Invalid credentials");
        }
        
        var token = _tokenService.GenerateJwtToken(user);
        return LoginResult.Success(token);
    }
}
```

**Alternatives Considered**:
- Duende IdentityServer: Rejected - Entra ID provides enterprise SSO
- Auth0: Rejected - vendor lock-in, cost
- Custom OAuth implementation: Rejected - reinventing standards
- Entra ID only (no fallback): Rejected - single point of failure

**References**:
- [Microsoft.Identity.Web Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [WIPNorthStar Feature 001](../../Src/WIPNorthStar/NorthStarET.Lms/docs/features/001-unified-sso-authorization.md)
- Master Migration Plan: Phase 1 Foundation Services

---

### 5. Event-Driven Integration with Azure Service Bus

**Question**: How do we implement event publishing and subscription with idempotency and versioned schemas?

**Decision**: MassTransit 8.x with Azure Service Bus transport + versioned message contracts

**Rationale**:
- MassTransit abstracts Azure Service Bus complexity
- Built-in retry policies, error handling, dead letter queues
- Support for saga patterns (needed in Phase 3 for rollover)
- Message versioning support for backwards compatibility
- Observability integration with OpenTelemetry

**Implementation Pattern**:

**Event Contracts**:
```csharp
// Common/NorthStar.Common/Events/ConfigurationEvents.cs
namespace NorthStar.Common.Events.Configuration.V1;

public record DistrictCreatedEvent
{
    public Guid DistrictId { get; init; }
    public string DistrictName { get; init; }
    public string Code { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid TenantId { get; init; }
    public string MessageVersion { get; init; } = "1.0";
}

public record SchoolCreatedEvent
{
    public Guid SchoolId { get; init; }
    public Guid DistrictId { get; init; }
    public string SchoolName { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid TenantId { get; init; }
    public string MessageVersion { get; init; } = "1.0";
}
```

**Publishing Events**:
```csharp
// Configuration.Infrastructure/EventPublisher.cs
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IBus _bus;

    public async Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : class
    {
        await _bus.Publish(domainEvent);
    }
}

// Configuration.Application/Commands/CreateDistrictCommand.cs
public class CreateDistrictCommandHandler : IRequestHandler<CreateDistrictCommand, District>
{
    public async Task<District> Handle(CreateDistrictCommand request, CancellationToken cancellationToken)
    {
        var district = new District(request.Name, request.Code);
        await _repository.AddAsync(district);
        await _unitOfWork.SaveChangesAsync();
        
        // Publish event
        await _eventPublisher.PublishAsync(new DistrictCreatedEvent
        {
            DistrictId = district.Id,
            DistrictName = district.Name,
            Code = district.Code,
            CreatedAt = DateTime.UtcNow,
            TenantId = district.TenantId
        });
        
        return district;
    }
}
```

**Consuming Events**:
```csharp
// Identity.Infrastructure/Consumers/DistrictCreatedConsumer.cs
public class DistrictCreatedConsumer : IConsumer<DistrictCreatedEvent>
{
    private readonly IMediator _mediator;
    
    public async Task Consume(ConsumeContext<DistrictCreatedEvent> context)
    {
        var message = context.Message;
        
        // Idempotency check
        var messageId = context.MessageId.ToString();
        if (await _processedMessageRepository.ExistsAsync(messageId))
        {
            _logger.LogInformation("Message {MessageId} already processed", messageId);
            return;
        }
        
        // Process event
        await _mediator.Send(new SyncDistrictCommand
        {
            DistrictId = message.DistrictId,
            DistrictName = message.DistrictName,
            TenantId = message.TenantId
        });
        
        // Mark as processed
        await _processedMessageRepository.AddAsync(messageId);
    }
}
```

**MassTransit Configuration**:
```csharp
// Program.cs
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<DistrictCreatedConsumer>();
    
    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServiceBus"));
        
        cfg.ConfigureEndpoints(context);
        
        // Retry policy
        cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000, 5000));
        
        // Circuit breaker
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });
    });
});
```

**Alternatives Considered**:
- Direct Azure Service Bus SDK: Rejected - too low-level, manual retry handling
- RabbitMQ: Rejected - Azure Service Bus preferred for production
- Apache Kafka: Rejected - overkill for event counts, operational complexity
- HTTP webhooks: Rejected - no built-in reliability, manual retry logic

**References**:
- [MassTransit Documentation](https://masstransit.io/)
- [MassTransit with Azure Service Bus](https://masstransit.io/documentation/configuration/transports/azure-service-bus)
- Master Migration Plan: Event-Driven Data Discipline

---

### 6. Clean Architecture Layer Dependencies

**Question**: How do we enforce Clean Architecture boundaries in our project structure?

**Decision**: Project references enforce dependency rules: API → Application → Domain ← Infrastructure

**Rationale**:
- Domain layer has ZERO external dependencies (pure business logic)
- Application layer depends only on Domain (CQRS handlers, interfaces)
- Infrastructure implements Application interfaces (EF Core, Azure Service Bus)
- API layer orchestrates (thin controllers, dependency injection)
- Compile-time enforcement prevents architectural violations

**Layer Responsibilities**:

**Domain Layer** (`NorthStar.Identity.Domain`):
- Entities: `User`, `Role`, `RefreshToken`, `ExternalProviderLink`
- Value Objects: `Email`, `PasswordHash`
- Domain Events: `UserRegisteredEvent`, `PasswordChangedEvent`
- Enumerations: `UserStatus`, `RoleType`
- No external dependencies (except common primitives)

**Application Layer** (`NorthStar.Identity.Application`):
- CQRS Commands: `LoginCommand`, `RegisterUserCommand`, `ResetPasswordCommand`
- CQRS Queries: `GetUserQuery`, `GetUserRolesQuery`
- Interfaces: `IUserRepository`, `ITokenService`, `IEmailService`
- DTOs: `LoginRequest`, `LoginResponse`, `UserDto`
- Validators: `LoginCommandValidator` (FluentValidation)
- Depends on: Domain layer only

**Infrastructure Layer** (`NorthStar.Identity.Infrastructure`):
- EF Core: `IdentityDbContext`, entity configurations
- Repositories: `UserRepository`, `RoleRepository`
- External Services: `EntraIdAuthenticationService`, `SmtpEmailService`
- Event Publishing: `AzureServiceBusEventPublisher`
- Depends on: Application layer (implements interfaces), Domain layer

**API Layer** (`NorthStar.Identity.Api`):
- Controllers: `AuthenticationController`, `UsersController`
- Middleware: Exception handling, logging
- Configuration: Dependency injection, Aspire integration
- Depends on: Application layer, Infrastructure layer (DI registration only)

**Enforcement**:
```xml
<!-- NorthStar.Identity.Domain.csproj -->
<!-- NO project references -->

<!-- NorthStar.Identity.Application.csproj -->
<ItemGroup>
  <ProjectReference Include="..\NorthStar.Identity.Domain\NorthStar.Identity.Domain.csproj" />
</ItemGroup>

<!-- NorthStar.Identity.Infrastructure.csproj -->
<ItemGroup>
  <ProjectReference Include="..\NorthStar.Identity.Application\NorthStar.Identity.Application.csproj" />
  <ProjectReference Include="..\NorthStar.Identity.Domain\NorthStar.Identity.Domain.csproj" />
</ItemGroup>

<!-- NorthStar.Identity.Api.csproj -->
<ItemGroup>
  <ProjectReference Include="..\NorthStar.Identity.Application\NorthStar.Identity.Application.csproj" />
  <ProjectReference Include="..\NorthStar.Identity.Infrastructure\NorthStar.Identity.Infrastructure.csproj" />
</ItemGroup>
```

**Alternatives Considered**:
- Monolithic project: Rejected - no boundary enforcement
- ArchUnit/NetArchTest validation: Considered - good addition but not replacement
- Vertical slice architecture: Rejected - Clean Architecture better for bounded contexts

**References**:
- [Clean Architecture (Robert C. Martin)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- Master Migration Plan: Constitutional Compliance

---

## Research Summary

All technical unknowns have been resolved with concrete implementation patterns:

1. ✅ **Aspire Orchestration**: AppHost pattern with service discovery and dashboard
2. ✅ **Multi-Tenancy**: PostgreSQL RLS + application-level tenant context
3. ✅ **API Gateway**: YARP 2.2.0 with custom middleware for cross-cutting concerns
4. ✅ **Entra ID**: Microsoft.Identity.Web with local fallback following WIPNorthStar patterns
5. ✅ **Event-Driven**: MassTransit 8.x with Azure Service Bus + idempotency
6. ✅ **Clean Architecture**: Project references enforce layer boundaries

**Ready for Phase 1: Design & Contracts**
