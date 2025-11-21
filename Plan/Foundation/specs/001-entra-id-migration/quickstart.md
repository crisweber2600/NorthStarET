# Quickstart: Legacy IdentityServer to Microsoft Entra ID Migration
Layer: Foundation

## Prerequisites
- .NET 8 SDK installed; PowerShell 7+ for scripts.
- Access to legacy SQL Server (source) and PostgreSQL Identity DB (target).
- Entra ID tenant with app registration credentials (client id/secret or certificate).
- Redis and MassTransit/Azure Service Bus available if using event notifications.
- Secrets provided via the platform secret store; never commit connection strings.

## Bootstrap
1. From repo root, restore and build migration workers:
   ```pwsh
   dotnet restore
   dotnet build Src/Foundation/services/Identity/Migration/Migration.Worker.csproj -c Release
   ```
2. Apply schema updates (ExternalProviderLinks + auth_deprecated_at):
   ```pwsh
   dotnet ef database update `
     --project Src/Foundation/services/Identity/Identity.Infrastructure `
     --startup-project Src/Foundation/services/Identity/Identity.API `
     --context IdentityDbContext
   ```

## Run a Dry-Run Pilot
1. Export source snapshot for pilot tenants (SQL Server backup or filtered export).
2. Execute the migration worker in dry-run mode to validate mappings and report matches:
   ```pwsh
   dotnet run --project Src/Foundation/services/Identity/Migration/Migration.Worker.csproj -- `
     --mode DryRun `
     --source-connection "$env:LEGACY_SQL_CONN" `
     --target-connection "$env:IDENTITY_PG_CONN" `
     --tenant-filter "PilotDistrict01" `
     --report-path ".artifacts/identity-migration/reports"
   ```
3. Review generated reconciliation reports (match rate, missing emails, role deltas) and resolve blockers before live run.

## Execute Cutover
1. Switch to `--mode Execute` with the approved tenant list and enable audit logging:
   ```pwsh
   dotnet run --project Src/Foundation/services/Identity/Migration/Migration.Worker.csproj -- `
     --mode Execute `
     --source-connection "$env:LEGACY_SQL_CONN" `
     --target-connection "$env:IDENTITY_PG_CONN" `
     --tenant-filter "PilotDistrict01" `
     --enable-audit `
     --report-path ".artifacts/identity-migration/reports"
   ```
2. Monitor progress via MassTransit/Azure Service Bus events (`MigrationRunStarted`, `MigrationRunProgress`, `MigrationRunCompleted`).
3. If severe issues occur, trigger rollback:
   ```pwsh
   dotnet run --project Src/Foundation/services/Identity/Migration/Migration.Worker.csproj -- --mode Rollback --run-id <runId>
   ```

## Validation & Evidence
- Run `dotnet test tests/identity-migration/` to capture Red/Green evidence.
- Perform sample logins through gateway/BFF to verify Entra ID session issuance; ensure `auth_deprecated_at` blocks legacy password logins.
- Archive dry-run and cutover reports in `.artifacts/identity-migration/` for review.
