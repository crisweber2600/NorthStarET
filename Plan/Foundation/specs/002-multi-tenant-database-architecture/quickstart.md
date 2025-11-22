# Quickstart: Multi-Tenant Database Architecture
Layer: Foundation

## Prerequisites
- .NET 8 SDK, PostgreSQL 16, Redis (optional) available locally.
- `psql` CLI for verifying RLS policies.
- Tenant-aware gateway/BFF running to supply authenticated requests with tenant claims.

## Bootstrap
1. Restore and build shared infrastructure packages:
   ```pwsh
   dotnet restore
   dotnet build Src/Foundation/shared/Infrastructure/Infrastructure.csproj -c Release
   ```
2. Apply sample RLS migration to a target service (example: Student service):
   ```pwsh
   dotnet ef database update `
     --project Src/Foundation/services/Student/Student.Infrastructure `
     --startup-project Src/Foundation/services/Student/Student.Api `
     --context StudentDbContext
   ```

## Verify Tenant Context
1. Set session variable in psql to mimic middleware behavior:
   ```sql
   SELECT set_config('app.current_tenant', '00000000-0000-0000-0000-000000000001', false);
   SELECT * FROM student.students LIMIT 5; -- should return rows for that tenant only
   ```
2. Run integration tests to validate RLS enforcement and tenant propagation:
   ```pwsh
   dotnet test tests/multi-tenancy/ -c Release
   ```

## Performance & Observability
- Use `psql EXPLAIN ANALYZE` to confirm indexes on `(tenant_id, created_at)` and other hot paths.
- Enable OpenTelemetry exporters via ServiceDefaults and verify spans contain `tenant.id`.

## Rollout Guidance
- Apply RLS migrations per service, then enable middleware that sets `app.current_tenant` from claims.
- Lock down exceptions (reference data) explicitly in policy definitions; document any exclusions.
