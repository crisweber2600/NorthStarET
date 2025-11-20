# Spec: Data Migration from Legacy to Multi-Tenant Architecture

Short Name: data-migration-etl
Layer: Foundation
Status: Draft (Specification)
Version: 0.1.0
Created: 2025-11-20

## Feature
Migrate 383 legacy per-district entities into consolidated multi-tenant service databases with tenant tagging, referential integrity transformation, historical preservation, and resumable, performant ETL.

## Business Value
Reduces operational overhead (hundreds of DBs → 11), enables modern multi-tenant services, improves performance, centralizes audit/compliance, and unlocks future analytics.

## Target Layer
Foundation

## Actors
- Migration ETL Operator
- District Staff (during dual-write window)
- Validation / Audit Systems

## Assumptions
- Legacy SQL Server + new PostgreSQL databases accessible.
- Service schemas & RLS policies already in place (prereq from multi-tenant architecture feature).
- UUID primary keys established for new entities.

## Constraints
- Zero data loss (record count parity per entity per district).
- Referential integrity maintained logically (mapping tables).
- Migration resumable after interruption.
- Performance targets per scenario.

## Scenarios (Condensed)

### Scenario 1: Student Records Migration with Tenant Tagging
Given District A (5000 students) and District B (3000)
When student migration runs
Then all students inserted with correct tenant_id and legacy_id retention
And related demographics, contacts, enrollments migrated with same tenant_id
And no data loss

### Scenario 2: Assessment Results Migration (Large Volume)
Given 5M assessment results
When migrated in 10k batches
Then tenant_id applied
And throughput ≥5000 records/sec
And progress logged, resumable
And all results migrated

### Scenario 3: Foreign Key Relationship Transformation
Given legacy FK constraints
When migrated
Then FK replaced by UUID references
And application enforces integrity
And cross-tenant references prevented

### Scenario 4: Dual-Write Pattern During Transition
Given District A still active in legacy
When new student created in legacy
Then record written legacy + new DB atomically
And nightly reconciliation validates consistency

### Scenario 5: Data Validation and Reconciliation
Given migration complete
When validation script runs
Then counts match per entity per tenant
And sample field comparison passes
And orphan check returns zero
And reconciliation report generated

### Scenario 6: Historical Data Preservation
Given 15 years of history
When migrated
Then timestamps preserved
And deleted/archived status retained
And audit trail reproduced

### Scenario 7: Identity and GUID Mapping
Given legacy integer identities
When migrated
Then UUIDs generated
And mapping table stores legacy_id ↔ new_uuid per tenant
And supports bi-directional lookup

### Scenario 8: Handling Data Type Conversions
Given datetime to timestamptz
When converted
Then values normalized to UTC with correct offset
And date-only preserved
And precision retained

### Scenario 9: Incremental Migration by Service
Given phased approach
When Phase 1 ends
Then only identity/config migrated
When Phase 2 starts
Then student/staff/assessment migrate
And unmigrated data remains legacy safely

### Scenario 10: Rollback Scenario for Failed Migration
Given partial failure (60%)
When rollback triggered
Then partial new records removed
And legacy unchanged
And logs captured
And safe retry possible

### Scenario 11: Reference Data Migration (Lookup Tables)
Given shared + tenant-specific lookups
When migrated
Then shared lookups created once (no tenant_id)
And tenant-specific include tenant_id
And no duplicates

### Scenario 12: Handling Denormalized Data
Given denormalized legacy student table
When migrated
Then normalization splits into distinct tables
And tenant_id consistent across related rows
And no data lost

## Non-Functional Requirements
- Throughput: Students ≥10k/sec; Assessments ≥5k/sec.
- Resumability via checkpoint state.
- Logging: structured logs per batch.
- Monitoring: progress metrics exported.

## Acceptance Criteria Summary
All scenarios validated via automated migration dry-run on sampled subset + full rehearsal for pilot districts. Reconciliation report must indicate 100% parity and referential integrity.

## Out of Scope
- Real-time CDC beyond dual-write window.
- Analytics data transformations (future reporting service).

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Partial batch failure | Idempotent batch + checkpoint resume |
| Performance shortfall | COPY optimization + parallelization |
| Data type surprises | Pre-migration profiling scripts |
| Legacy dirty data | Validation + cleansing rules |

## Initial Roadmap
1. Profiling of legacy schema & volume metrics.
2. Implement mapping + batch framework.
3. Build student migration job (pilot).
4. Add reconciliation + reporting modules.
5. Implement dual-write for active entities.
6. Scale to remaining high-volume entities.
7. Final full-district rehearsal & sign-off.

## Audit & Compliance
All migration operations log: batch_id, entity_type, tenant_id, count, duration, checksum.

---
Generated manually (no agent invocation).