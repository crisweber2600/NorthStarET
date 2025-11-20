# Plan: Multi-Tenant Database Architecture (Foundation Layer)
Version: 0.1.0
Status: Draft (Planning)
Layer: Foundation
Spec Ref: 002-multi-tenant-database-architecture/spec.md

## Technical Objectives
- Provide database-per-service pattern with strict tenant isolation via PostgreSQL RLS.
- Ensure Application layer enforces tenant filters and cannot bypass isolation.
- Provide migration scaffolding for legacy per-district consolidation.
- Implement tracing + auditing with tenant context propagation.

## Architectural Components
1. Tenant Context Middleware
   - Extract `tenant_id` from JWT claims.
   - Set ambient context (AsyncLocal + HttpContext Items).
   - Provide accessor `ITenantContext` for Application layer.
2. EF Core Infrastructure
   - Base `MultiTenantDbContext` abstract class adding `TenantId` shadow property or explicit property on entities.
   - Interceptor: inject `app.current_tenant` session variable via `DbConnection` before commands.
   - Global query filters per entity implementing `ITenantEntity`.
3. PostgreSQL RLS Policies
   - Migration to enable RLS on each tenant table.
   - Policies: SELECT/UPDATE/DELETE/INSERT with tenant match.
4. Audit Logging
   - Audit interceptor captures tenant_id, user_id, operation, entity type/id.
   - Writes to `audit.AuditRecords` table (also tenant-isolated).
5. Indexing Strategy
   - Composite indexes `(tenant_id, <frequent predicate>)` e.g. `(tenant_id, last_name)` for Students.
   - Analyze actual query patterns to finalize.
6. Backup/Restore Validation
   - Script to verify RLS remains enabled post-restore.
   - Automated test harness executing sample queries.

## Data Model Adjustments
- All domain aggregates include `TenantId` (GUID).
- Legacy integer IDs retained as `LegacyId` (nullable) for mapping during transition.
- Soft-delete columns (`DeletedAt`) maintain isolation (filters include `DeletedAt == null`).

## Key Interfaces
```csharp
public interface ITenantContext {
    Guid TenantId { get; }
}

public interface ITenantEntity {
    Guid TenantId { get; set; }
}
```

## Sample Middleware
```csharp
public sealed class TenantContextMiddleware {
    private readonly RequestDelegate _next;
    public TenantContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext http) {
        var tenantClaim = http.User.FindFirst("tenant_id")?.Value;
        if (tenantClaim == null) {
            http.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        TenantContext.Current = new TenantContext(Guid.Parse(tenantClaim));
        try { await _next(http); } finally { TenantContext.Reset(); }
    }
}
```

## EF Core Interceptor (Connection Level)
```csharp
public sealed class TenantConnectionInterceptor : DbConnectionInterceptor {
    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default) {
        if (TenantContext.Current is { } ctx) {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SET app.current_tenant = '{ctx.TenantId}'";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
```

## Global Query Filter Registration
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
        if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType)) {
            var method = typeof(ModelBuilderExtensions)
                .GetMethod(nameof(ModelBuilderExtensions.AddTenantFilter))!
                .MakeGenericMethod(entityType.ClrType);
            method.Invoke(null, new object[] { modelBuilder });
        }
    }
}
```

## Observability
- Enrich OpenTelemetry spans with `tenant.id` attribute.
- Metrics: request count per tenant (cardinality control via top-N + others bucket).

## Performance Validation
- Benchmark tenant-filtered queries against synthetic dataset (≥500k rows, 100 tenants).
- Tools: pgBadger for slow query analysis, EF Core logging for parameters.

## Security Controls
- Defense-in-depth: JWT tenant claim + RLS + application filters.
- Audit record immutability enforced (append-only table, periodic integrity checks).

## Migration Sequencing
1. Create base schemas & tables with `TenantId`.
2. Enable RLS and policies.
3. Migrate identity + configuration (foundation for tenant metadata).
4. Migrate student/staff/assessment in parallel streams.
5. Validate counts + mapping tables.

## Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| RLS misapplied | Data leak | Automated verification script after migration |
| Missing tenant claim | Request failure | Pre-flight validator + 401 early exit |
| Index bloat | Slower writes | Monitoring + periodic index review |
| High cardinality metrics | Cost / noise | Limit labels; aggregate low-volume tenants |

## Test Strategy
- Integration tests: verify RLS denies cross-tenant read/write.
- Unit tests: tenant context propagation + query filters.
- Load tests: simulate multi-tenant simultaneous queries.

## Completion Criteria
- All spec scenarios implemented + green tests.
- Audit trail shows tenant for all CRUD.
- Backup/restore validation script passes.
- Performance SLOs met under load.

---
Draft generated manually due to unavailable speckit.plan agent.