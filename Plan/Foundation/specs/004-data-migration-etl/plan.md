# Plan: Data Migration ETL
Version: 0.1.0
Status: Draft (Planning)
Layer: Foundation
Spec Ref: 004-data-migration-etl/spec.md

## Objectives
- Consolidate per-district SQL Server databases into multi-tenant PostgreSQL service databases.
- Preserve historical integrity and support rollback/resume.
- Achieve performance targets using batching + COPY bulk ingest.

## Architecture Components
1. Orchestrator (Console / Worker Service)
   - Reads migration manifest (entities, order, batch sizes).
   - Coordinates parallel jobs (respect dependencies).
2. Legacy Data Access
   - EF6 or Dapper (performance) against SQL Server.
   - Pagination queries with stable ordering (PK ascending).
3. Target Data Access
   - Npgsql + COPY for bulk writes; EF Core for small reference entities.
4. Mapping Store
   - `migration.LegacyIdMapping` per entity + tenant.
   - Upsert on duplicate for idempotency.
5. Checkpointing
   - `migration.BatchState(batch_id, entity_type, tenant_id, last_legacy_id, status, started_at, completed_at)`.
6. Validation Module
   - Count parity checks; random sampling; orphan detection.
7. Dual-Write Module
   - Transaction abstraction ensuring legacy + new commit/rollback.

## Batch Flow
1. Load next batch boundary (last_legacy_id).
2. Query legacy subset.
3. Map & transform (types, normalization, UUID generation).
4. COPY ingest into staging temp table.
5. Merge from staging to target (optional) or direct ingest.
6. Update checkpoint; log metrics.

## Performance Strategy
- Parallelization: distinct tenants processed concurrently for large entities.
- Network optimization: compress COPY streams if needed.
- Pre-create indexes (evaluate dropping & re-adding for extreme volume).

## Rollback Procedure
- Identify failed batch range.
- Delete inserted rows using batch_id or legacy_id range & tenant_id.
- Restore checkpoint state to prior boundary.

## Pseudocode
```csharp
while (!AllTenantsComplete(entity)) {
  var batch = await legacyRepo.FetchStudents(afterId: state.LastLegacyId, size: settings.BatchSize);
  if (batch.Count == 0) break;
  var transformed = mapper.Map(batch, tenantId);
  await bulkWriter.CopyAsync(transformed);
  state.LastLegacyId = batch.Max(x => x.Id);
  await checkpointRepo.Save(state);
  metrics.Log(entity, batch.Count, elapsed);
}
```

## Validation Queries
- Count parity per entity per tenant.
- Sample join validations (students ↔ enrollments).
- Orphan detection using LEFT JOIN where child missing parent.

## Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| COPY ingest row error | Partial failure | Row-level validation pre-COPY |
| Legacy schema drift mid-migration | Incorrect transforms | Freeze schema + diff monitor |
| Resource contention (I/O) | Slower progress | Throttle concurrency + monitor throughput |

## Test Strategy
- Unit: mappers, type converters, UUID mapping.
- Integration: batch processing end-to-end on small fixture sets.
- Load: synthetic large volume simulation.
- Reconciliation: automated script produce report artifact.

## Completion Criteria
- All spec scenarios green (dry-run + full run).
- Reconciliation report indicates 100% counts & referential integrity.
- Dual-write stabilized for active entities until cutover.

---
Draft plan.