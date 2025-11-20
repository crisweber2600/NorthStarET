# Aspire Orchestration & Cross-Cutting Scaffolding

**Service Set**: AppHost + All Foundation Microservices  
**Patterns**: .NET Aspire Orchestration, Clean Architecture, Dependency Injection, Multi-Tenancy, Event-Driven Messaging, Caching, Observability, Idempotency, Strangler Fig (Migration)  
**Architecture References**:  
 - [Aspire Orchestration](../patterns/aspire-orchestration.md)  
 - [Clean Architecture](../patterns/clean-architecture.md)  
 - [Dependency Injection](../patterns/dependency-injection.md)  
 - [Multi-Tenancy](../patterns/multi-tenancy.md) / [Multi-Tenant Database](../patterns/multi-tenant-database.md)  
 - [Messaging Integration](../patterns/messaging-integration.md)  
 - [Caching & Performance](../patterns/caching-performance.md)  
 - [Observability](../patterns/observability.md)  
 - [API Contracts Specification](../standards/API_CONTRACTS_SPECIFICATION.md)  
 - Constitution v2.0.0 (copilot-instructions) – Quality Gates & Red→Green Evidence

**Business Value**: Establish a consistent, reproducible scaffolding baseline so every slice (Identity, Student, Assessment, etc.) inherits enforced tenant isolation, diagnostics, messaging, resiliency, and deployment orchestration from day one. Reduces integration friction, accelerates feature delivery, and ensures migration safety under the Strangler Fig strategy.

---

## Scenario 1: AppHost Boots Full Stack with Dependency Readiness
**Given** AppHost defines PostgreSQL, Redis, and RabbitMQ resources per constitution  
**And** each microservice declares `.WaitFor(postgres).WaitFor(redis)` where required  
**When** `dotnet run --project Src/Foundation/AppHost` is executed  
**Then** all containers start in dependency order  
**And** the Aspire dashboard lists healthy resources  
**And** service logs stream with unified correlation IDs.

---

## Scenario 2: New Microservice Scaffolding in Under 2 Minutes
**Given** a developer needs to add `Intervention Management Service`  
**When** they run an internal scaffold script (future automation)  
**Then** a project is created with Application/Domain/Infrastructure/API folders  
**And** `DependencyInjection.cs` stubs for Application & Infrastructure  
**And** Aspire AppHost references the new service  
**And** baseline tests (unit + BDD feature placeholder + health) are added.

---

## Scenario 3: Tenant Isolation Enforced Automatically
**Given** `TenantInterceptor` and global query filters are registered  
**And** every entity has `TenantId`  
**When** a repository queries `Students`  
**Then** only rows with current context `TenantId` are returned  
**And** attempts to override the filter require explicit opt-out with reviewed justification  
**And** audit logging captures any opt-out usage.

---

## Scenario 4: Event Publication on Domain Changes
**Given** a `StudentCreated` domain event is raised in Domain layer  
**When** the Application handler commits the transaction  
**Then** Infrastructure publishes an integration event via MassTransit (RabbitMQ locally)  
**And** subscribers receive the event within 500ms P95  
**And** idempotency ensures duplicate publishes inside a 10-minute window are ignored.

---

## Scenario 5: Redis Caching Accelerates Session & Idempotency Lookups
**Given** Redis resource is provisioned by Aspire  
**And** session validation logic is executed  
**When** the same session is validated multiple times in <5 minutes  
**Then** lookup cost stays <20ms P95 (cache hit)  
**And** fallback to PostgreSQL only occurs on cache miss  
**And** cache entries slide expiration per configured policy.

---

## Scenario 6: Unified Observability – Traces, Metrics, Logs
**Given** OpenTelemetry is configured in every service  
**And** correlation ID middleware sets `X-Correlation-ID`  
**When** a request flows API Gateway → Student Service → Message Bus → Assessment Service  
**Then** a single trace appears in dashboard with spans per hop  
**And** logs include correlation scope  
**And** metrics expose request duration, rate, error counts.

---

## Scenario 7: Strangler Fig Legacy Routing Toggle
**Given** certain endpoints still reside in legacy monolith  
**And** routing rules exist in API Gateway  
**When** configuration flips a feature flag `AssessmentService:Migrated=true`  
**Then** traffic shifts seamlessly to new microservice  
**And** rollback can occur by toggling flag off  
**And** no client URL changes are required.

---

## Scenario 8: Resilient Messaging with Retry & DLQ
**Given** MassTransit configured with retry and dead-letter queues  
**When** `AssessmentCalculated` consumer throws a transient exception  
**Then** message is retried with exponential backoff  
**And** after max attempts it lands in DLQ  
**And** DLQ metrics surface in observability dashboard.

---

## Scenario 9: Onboarding a New Entity with Idempotent Create
**Given** `CreateDistrictCommand` includes idempotency key  
**When** a client submits the same payload twice within 10 minutes  
**Then** Redis idempotency envelope deduplicates the second call  
**And** response returns original entity identifier  
**And** audit trail notes idempotent replay.

---

## Scenario 10: Performance Budget Verification at Build Time
**Given** performance SLO budgets defined (e.g., token exchange <200ms P95)  
**When** integration test suite runs in CI  
**Then** tests assert metrics thresholds via exported OpenTelemetry data  
**And** build fails if budgets are exceeded  
**And** failure artifacts attach Red evidence.

---

## Scenario 11: Multi-Tenant Migration Safety
**Given** legacy data migration executes into per-service PostgreSQL schemas  
**When** ETL jobs load student records  
**Then** each record is stamped with correct `TenantId`  
**And** cross-tenant contamination attempts are blocked by RLS  
**And** validation scripts confirm row counts per tenant.

---

## Scenario 12: Rapid Local Developer Feedback Loop
**Given** developer modifies a command handler  
**When** they run `dotnet test` followed by `dotnet run --project AppHost`  
**Then** Aspire restarts only impacted services  
**And** updated spans appear live  
**And** BDD scenario transitions from Red to Green within minutes.

---

## Related Architecture & Standards
**Aspire Orchestration**: See pattern reference for resource declaration & dependency graph.  
**Messaging Integration**: MassTransit conventions for exchange naming & consumers.  
**Caching & Performance**: Redis utilization guidelines, key naming strategies.  
**Observability**: Correlation, tracing, metrics export configuration.  
**Testing Strategy**: Red→Green workflow, coverage ≥80% gate.  
**Clean Architecture**: Enforcement of vertical slice isolation.  
**Dependency Injection**: Service registration boundaries (Application vs Infrastructure).  
**Multi-Tenant Database**: RLS + global filters.

---

## Technical Implementation Notes

### Aspire AppHost Resource Declaration (Excerpt)
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("Postgres");
var redis = builder.AddRedis("Redis");
var rabbit = builder.AddRabbitMQ("RabbitMq");

var identityDb = postgres.AddDatabase("IdentityDb");
var studentDb = postgres.AddDatabase("StudentDb");
var interventionDb = postgres.AddDatabase("InterventionDb");

builder.AddProject<Projects.IdentityService>("identity")
       .WithReference(identityDb).WithReference(redis).WithReference(rabbit)
       .WaitFor(identityDb).WaitFor(redis).WaitFor(rabbit);

builder.AddProject<Projects.StudentService>("students")
       .WithReference(studentDb).WithReference(redis).WithReference(rabbit)
       .WaitFor(studentDb).WaitFor(redis).WaitFor(rabbit);

builder.AddProject<Projects.InterventionService>("interventions")
       .WithReference(interventionDb).WithReference(redis).WithReference(rabbit)
       .WaitFor(interventionDb).WaitFor(redis).WaitFor(rabbit);

builder.Build().Run();
```

### Clean Architecture Service Registration
```csharp
// Application layer
public static class ApplicationRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(ApplicationRegistration).Assembly);
        services.AddValidatorsFromAssembly(typeof(ApplicationRegistration).Assembly);
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        return services;
    }
}

// Infrastructure layer
public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddDbContext<LmsDbContext>(o =>
            o.UseNpgsql(cfg.GetConnectionString("StudentDb"))
             .AddInterceptors(new TenantInterceptor(), new AuditInterceptor()));

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumers(typeof(InfrastructureRegistration).Assembly);
            x.UsingRabbitMq((ctx, cfgBus) =>
            {
                cfgBus.Host(cfg["RabbitMq:Host"], h =>
                {
                    h.Username(cfg["RabbitMq:User"]);
                    h.Password(cfg["RabbitMq:Password"]);
                });
                cfgBus.ConfigureEndpoints(ctx);
            });
        });

        services.AddStackExchangeRedisCache(o => o.Configuration = cfg.GetConnectionString("Redis"));
        services.AddOpenTelemetry().WithMetrics(m => m.AddRuntimeInstrumentation())
                                     .WithTracing(t => t.AddAspNetCoreInstrumentation());
        return services;
    }
}
```

### Multi-Tenancy EF Core Interceptor (Conceptual)
```csharp
public sealed class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenant;
    public TenantInterceptor(ITenantContext tenant) => _tenant = tenant;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is ITenantEntity t && entry.State == EntityState.Added)
            {
                t.TenantId = _tenant.CurrentTenantId;
            }
        }
        return base.SavingChanges(eventData, result);
    }
}
```

### Idempotency Service (Redis Envelope)
```csharp
public sealed class IdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    public IdempotencyService(IDistributedCache cache) => _cache = cache;

    public async Task<bool> TryRegisterAsync(string operation, string hash, TimeSpan window)
    {
        var key = $"idempotency:{operation}:{hash}";
        var existing = await _cache.GetStringAsync(key);
        if (existing is not null) return false;
        await _cache.SetStringAsync(key, DateTime.UtcNow.ToString("O"), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = window
        });
        return true;
    }
}
```

### Correlation Middleware
```csharp
app.Use(async (ctx, next) =>
{
    var cid = ctx.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    ctx.Response.Headers["X-Correlation-ID"] = cid;
    using (logger.BeginScope(new Dictionary<string, object>{{"CorrelationId", cid}}))
    {
        await next();
    }
});
```

### Messaging Consumer with Retry
```csharp
public class AssessmentCalculatedConsumer : IConsumer<AssessmentCalculated>
{
    public async Task Consume(ConsumeContext<AssessmentCalculated> context)
    {
        // Domain reaction logic
    }
}

// MassTransit configuration excerpt (retry + DLQ assumed via topology conventions)
cfgBus.ReceiveEndpoint("assessment-calculated", e =>
{
    e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2)));
    e.ConfigureConsumer<AssessmentCalculatedConsumer>(ctx);
});
```

### BDD & Red→Green Enforcement (Workflow Snippets)
```bash
dotnet test --configuration Debug --verbosity normal > phase-red-dotnet-test.txt
# implement slice
dotnet test --configuration Debug --verbosity normal > phase-green-dotnet-test.txt
```

### Performance Budget Assertions (Conceptual Test)
```csharp
[Fact]
public async Task TokenExchange_P95_Under_200ms()
{
    var samples = await _metricsClient.GetLatencySamplesAsync("token_exchange");
    samples.P95.Should().BeLessThan(TimeSpan.FromMilliseconds(200));
}
```

---

## Performance & Resiliency Targets (Initial Baseline)
- AppHost startup (cold): < 15s total stack
- API request overhead (gateway + service): < 50ms P95 (excluding domain logic)
- Cache hit latency: < 20ms P95
- Event publication to consumer ack: < 500ms P95
- Circuit breaker recovery attempt: 30s window
- Message duplication tolerance: Zero (idempotency enforced)

## Security & Compliance Hooks
- Entra ID tokens validated at gateway (RS256) before propagation
- RLS enforced per service database (no shared cross-tenant queries)
- Audit trail records idempotent replays, tenant filter overrides, DLQ events
- Rate limits applied per tenant & sensitive endpoints (create/invite) 10 req/min

## Expansion Roadmap (Future Scaffolding Enhancements)
- Automated project template CLI (`northstar new service <Name>`) integrates all patterns
- Synthetic load test harness injecting distributed trace verification
- Policy-driven dynamic circuit breaker thresholds from configuration service
- Observability SLO automated rollback triggers (feature flag revert)

---

## Acceptance Summary
This scaffolding spec is accepted when:  
1. AppHost defines & boots baseline resources (Postgres, Redis, RabbitMQ) with health visible.  
2. A new service can be added following repeatable template with DI & tracing in place.  
3. Multi-tenancy filters & interceptors applied automatically without per-handler boilerplate.  
4. Messaging publishes domain events with retry/DLQ integration.  
5. Redis caching & idempotency services reachable and validated by tests.  
6. Unified traces (gateway → service → bus → service) visible in dashboard.  
7. Strangler Fig routing toggled via config/feature flag without client impact.  
8. Performance budget tests exist and fail builds when exceeded.  
9. Red→Green evidence captured for initial scaffolding test suites.  
10. Security hooks (token validation, RLS, audit logging) verified in integration tests.

---

## Next Actions
- Integrate this spec into feature planning (`specs/` folder for cross-cutting baseline).  
- Implement automated scaffold CLI for new services.  
- Add performance metric export & assertion library if absent.  
- Populate initial BDD feature placeholders per service using this foundation.
