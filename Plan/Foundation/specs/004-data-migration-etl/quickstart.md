# Quickstart: Data Migration ETL
Layer: Foundation

## Prerequisites
- .NET 8 SDK; access to legacy SQL Server and target PostgreSQL instances.
- Redis (optional) for checkpoint caching; Azure Storage/Blob for logs and manifests.
- MassTransit + Azure Service Bus configured for progress events (optional).

## Prepare
1. Restore and build the migration toolkit:
   ```pwsh
   dotnet restore
   dotnet build Src/Foundation/tools/data-migration/DataMigration.Worker.csproj -c Release
   ```
2. Export or snapshot source data for pilot tenants; ensure target schemas (with tenant_id + RLS) are applied.

## Run Pilot
1. Execute a dry-run with manifest-driven entity order:
   ```pwsh
   dotnet run --project Src/Foundation/tools/data-migration/DataMigration.Worker.csproj -- `
     --mode DryRun `
     --manifest manifests/pilot.json `
     --source-connection "$env:LEGACY_SQL_CONN" `
     --target-connection "$env:TARGET_PG_CONN" `
     --checkpoint-store postgres `
     --report-path ".artifacts/data-migration/reports"
   ```
2. Review reconciliation CSVs (row counts, checksum deltas) and resolve mapping gaps.

## Execute Migration
1. Switch to execute mode once pilot is clean:
   ```pwsh
   dotnet run --project Src/Foundation/tools/data-migration/DataMigration.Worker.csproj -- `
     --mode Execute `
     --manifest manifests/pilot.json `
     --resume `
     --report-path ".artifacts/data-migration/reports"
   ```
2. Monitor progress via events or logs; checkpoints allow restart on failure without duplicates.

## Rollback & Evidence
- Rollback a specific run:
  ```pwsh
  dotnet run --project Src/Foundation/tools/data-migration/DataMigration.Worker.csproj -- --mode Rollback --run-id <runId>
  ```
- Capture `dotnet test tests/data-migration/` output (Red ➜ Green) and archive reports alongside manifests for audit.
