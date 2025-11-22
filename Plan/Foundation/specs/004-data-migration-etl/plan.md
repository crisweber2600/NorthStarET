# Implementation Plan: Data Migration from Legacy to Multi-Tenant Architecture

**Specification Branch**: `Foundation/004-data-migration-etl-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/004-data-migration-etl` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/004-data-migration-etl/spec.md

**Note**: Migration depends on multi-tenant architecture readiness (feature 002) and Identity/Configuration availability.

## Summary

Migrate 383 legacy per-district entities from SQL Server into consolidated multi-tenant PostgreSQL service databases with UUID remapping, tenant tagging, referential integrity preservation, and resumable checkpoints. Execute dual-write during transition, validate parity with reconciliation reports, and support rollback of partial runs.

## Technical Context

**Language/Version**: C# / .NET 8 (worker services)  
**Primary Dependencies**: EF Core 9 + Npgsql + SqlClient, Dapper for high-volume batches, Polly for retries, CsvHelper for reports, MassTransit for progress events  
**Storage**: SQL Server (source), PostgreSQL per-service targets, checkpoint store in PostgreSQL/Redis, blob storage for logs/reports  
**Testing**: xUnit integration for batch idempotency, Reqnroll BDD for migration scenarios, reconciliation scripts with row counts/checksums, performance harness for throughput targets  
**Target Platform**: Linux containers/worker jobs orchestrated via Aspire  
**Project Type**: Backend ETL workers and scripts  
**Performance Goals**: Students >=10k/sec; assessments >=5k/sec; resumable checkpoints; overnight windows for pilot districts  
**Constraints**: Zero data loss; referential integrity maintained; tenant_id required on all migrated rows; rollback supported; branch naming with layer prefix  
**Scale/Scope**: 15 years of historical data across all districts; millions of rows per entity

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID for operator access to runbooks and dashboards  
- Authentication Pattern: Session/BFF for admin UI if exposed; service-to-service uses managed identity  
- Token Validation: Microsoft.Identity.Web where applicable  
- Session Storage: Not primary; operators authenticated via gateway/admin tools

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/tools/data-migration` (workers + scripts) and service-specific migration packages  
**Specification Path**: `Plan/Foundation/specs/004-data-migration-etl/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting, logging, health probes for workers  
- [x] Domain - Value objects for tenant IDs and legacy IDs  
- [x] Application - Command handlers for batch orchestration  
- [x] Infrastructure - PostgreSQL/Redis providers, messaging for status events

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: Tenant metadata from Configuration Service; Identity for operator auth; ServiceDefaults/Infrastructure for DB access and messaging  
**Justification**: Migration uses shared services but does not add cross-layer runtime coupling.  
**Constitutional Compliance**: Principle 6 upheld; dependencies limited to Foundation shared components and required services.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only; no implementation code OK  
- Multi-tenancy enforced via tenant_id tagging and RLS-ready schemas OK  
- Security: credentials via secret store; no inline secrets OK  
- Testing: dry-run + reconciliation + BDD planned OK  
- No UI; Figma requirement not applicable OK

## Project Structure

### Documentation (this feature)

```
specs/004-data-migration-etl/
- plan.md
- research.md          # Source profiling, mapping rules
- data-model.md        # Mapping tables, UUID strategy
- quickstart.md        # Running pilots, checkpoints, rollback
- contracts/           # Event/report schemas if emitted
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/tools/data-migration/
- Workers/             # ETL runners per entity group
- Pipelines/           # Batch + checkpoint orchestration
- Mappings/            # Legacy -> new schema maps
- Reports/             # Reconciliation and audit exports
- Tests/               # Unit + integration + BDD

tests/data-migration/
- reconciliation/
- performance/
- bdd/
```

**Structure Decision**: Worker-based ETL toolkit with shared mapping libraries; no frontend components.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

