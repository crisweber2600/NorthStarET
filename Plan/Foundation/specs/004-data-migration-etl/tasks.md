---
description: "Task list for Data Migration ETL (legacy SQL Server to multi-tenant PostgreSQL)"
---

# Tasks: Data Migration ETL

**Specification Branch**: `Foundation/004-data-migration-etl-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/004-data-migration-etl` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/004-data-migration-etl/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/ (internal CLI args)

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/tools/data-migration/`  
**Specification Path**: `Plan/Foundation/specs/004-data-migration-etl/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/tools/data-migration/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/004-data-migration-etl/`)
- [ ] Dependencies (SQL Server source, PostgreSQL target, RLS, mapping tables) aligned between plan and spec
- [ ] Cross-layer dependencies (Identity/Configuration) documented if needed for validation

---

## Layer Compliance Validation

- [ ] T001 Verify migration tool references only Foundation shared libraries (`Src/Foundation/shared/*`)
- [ ] T002 Verify no cross-layer service coupling; data access isolated through infra projects
- [ ] T003 Ensure AppHost/dev containers expose required source/target DB connections for migration only
- [ ] T004 Update migration README with layer placement and dependencies (`Src/Foundation/tools/data-migration/README.md`)

---

## Identity & Authentication Compliance

*Internal tool; still enforce secure access to secrets*

- [ ] T005 Ensure connection strings/secrets loaded via managed identity/Key Vault (no hardcoded secrets) in `Src/Foundation/tools/data-migration/appsettings.json`
- [ ] T006 Verify migration runs under service principal with least privilege; document in `Src/Foundation/tools/data-migration/docs/security.md`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize migration tool workspace, configuration, and scaffolding

- [ ] T007 Scaffold migration console/worker project with dependency injection (`Src/Foundation/tools/data-migration/DataMigration.Worker.csproj`)
- [ ] T008 Add configuration model for source/target DBs, batch sizes, manifest path (`Src/Foundation/tools/data-migration/appsettings.schema.json`)
- [ ] T009 [P] Add health check CLI command to verify connectivity to SQL Server + PostgreSQL (`Src/Foundation/tools/data-migration/Commands/CheckConnectionsCommand.cs`)
- [ ] T010 [P] Add logging/telemetry setup with OpenTelemetry exporter (`Src/Foundation/tools/data-migration/Infrastructure/Telemetry/TelemetrySetup.cs`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core migration framework, manifest parsing, checkpointing (scenario 9)

- [ ] T011 Implement migration manifest parser (entities, order, batch sizes) in `Src/Foundation/tools/data-migration/Infrastructure/Manifest/ManifestLoader.cs`
- [ ] T012 Create EF/Dapper repositories for legacy SQL Server reads with stable ordering (`Src/Foundation/tools/data-migration/Infrastructure/Legacy/LegacyStudentRepository.cs`)
- [ ] T013 Create PostgreSQL bulk writer supporting COPY + staging tables (`Src/Foundation/tools/data-migration/Infrastructure/Target/BulkCopyWriter.cs`)
- [ ] T014 Implement checkpoint store using `migration.BatchState` table (`Src/Foundation/shared/Infrastructure/MultiTenancy/Migration/BatchStateRepository.cs`)
- [ ] T015 [P] Add mapping repository for `migration.LegacyIdMapping` with idempotent upsert (`Src/Foundation/shared/Infrastructure/MultiTenancy/Migration/LegacyIdMappingRepository.cs`)

**Checkpoint**: Framework ready for entity-specific jobs; resumable

---

## Phase 3: User Story 1 - Student & Enrollment Migration (Priority: P1) **MVP**

**Goal**: Migrate students and enrollments with tenant tagging, UUID mapping, and resumability (scenarios 1, 3, 12)

**Independent Test**: Dry-run migrates pilot tenants, preserves tenant_id + legacy_id, resumes after failure without duplicates

### Implementation for User Story 1

- [ ] T016 [P] [US1] Implement student mapper (type conversions, tenant assignment, legacy_id retention) in `Src/Foundation/tools/data-migration/Mappers/StudentMapper.cs`
- [ ] T017 [US1] Implement enrollment mapper ensuring FK references use mapped student UUIDs (`Src/Foundation/tools/data-migration/Mappers/EnrollmentMapper.cs`)
- [ ] T018 [US1] Build student migration job orchestrating batch fetch -> map -> COPY ingest -> checkpoint (`Src/Foundation/tools/data-migration/Jobs/StudentMigrationJob.cs`)
- [ ] T019 [US1] Add dry-run mode to student job with validation-only flow (`Src/Foundation/tools/data-migration/Commands/StudentDryRunCommand.cs`)
- [ ] T020 [US1] Integration test: resume student migration after simulated failure using BatchState (`tests/Foundation/DataMigration/Integration/StudentMigrationResumeTests.cs`)

**Checkpoint**: Student/enrollment migration solid with resume + validation

---

## Phase 4: User Story 2 - High-Volume Assessment Migration (Priority: P2)

**Goal**: Migrate large assessment result volumes meeting throughput targets (scenario 2, 8)

**Independent Test**: 5M assessment results migrate at >5k records/sec with correct tenant_id and timestamp normalization

### Implementation for User Story 2

- [ ] T021 [P] [US2] Implement assessment result mapper with UTC normalization (`Src/Foundation/tools/data-migration/Mappers/AssessmentResultMapper.cs`)
- [ ] T022 [US2] Build assessment migration job using COPY with configurable batch size and parallel per-tenant execution (`Src/Foundation/tools/data-migration/Jobs/AssessmentMigrationJob.cs`)
- [ ] T023 [US2] Add performance benchmark harness capturing throughput + latency metrics (`tests/Foundation/DataMigration/Perf/AssessmentThroughputBenchmarks.cs`)
- [ ] T024 [US2] Add monitoring dashboard for migration throughput/error rates (`observability/dashboards/migration.json`)

**Checkpoint**: Assessment migration meets performance goals with visibility

---

## Phase 5: User Story 3 - Dual-Write & Transition Safety (Priority: P2)

**Goal**: Support dual-write during transition and safe rollback/retry (scenarios 4, 10)

**Independent Test**: Dual-write path keeps legacy + new in sync; rollback removes partial inserts and restores checkpoint

### Implementation for User Story 3

- [ ] T025 [P] [US3] Implement dual-write abstraction (legacy + new transaction) for active entities (`Src/Foundation/tools/data-migration/DualWrite/DualWriteCoordinator.cs`)
- [ ] T026 [US3] Add nightly reconciliation job comparing legacy/new counts per entity per tenant (`Src/Foundation/tools/data-migration/Jobs/ReconciliationJob.cs`)
- [ ] T027 [US3] Implement rollback command removing batch range by batch_id/tenant (`Src/Foundation/tools/data-migration/Commands/RollbackCommand.cs`)
- [ ] T028 [US3] Add integration test covering dual-write failure + rollback recovery (`tests/Foundation/DataMigration/Integration/DualWriteRollbackTests.cs`)

**Checkpoint**: Dual-write/rollback safety verified

---

## Phase 6: User Story 4 - Validation, Audit, and Reporting (Priority: P3)

**Goal**: Validate counts, relationships, history preservation, and audit reporting (scenarios 5, 6, 7, 11)

**Independent Test**: Validation reports show 100% parity; audit log captures batch_id/tenant/entity counts; lookup migrations handled correctly

### Implementation for User Story 4

- [ ] T029 [P] [US4] Create validation SQL views (`validation.student_count_parity`, `validation.orphan_enrollments`) in `Src/Foundation/shared/Infrastructure/Data/Migrations/AddValidationViews.cs`
- [ ] T030 [US4] Generate reconciliation report writer (CSV/Markdown) including orphan checks and sampling results in `Src/Foundation/tools/data-migration/Reporting/ReconciliationReportGenerator.cs`
- [ ] T031 [P] [US4] Implement GUID mapping lookup API/library for cross-entity FK resolution (`Src/Foundation/tools/data-migration/Infrastructure/MappingLookup.cs`)
- [ ] T032 [US4] Add historical data preservation checks (timestamps, deleted flags) in `tests/Foundation/DataMigration/Integration/HistoricalPreservationTests.cs`
- [ ] T033 [US4] Document data masking strategy for non-prod rehearsals in `Src/Foundation/tools/data-migration/docs/data-masking.md`

**Checkpoint**: Validation + reporting ensures integrity and auditability

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T034 [P] Add CLI UX polish: help text, sample manifests, error codes (`Src/Foundation/tools/data-migration/Commands/README.md`)
- [ ] T035 Harden telemetry + alerting for failed batches and slow throughput (`Src/Foundation/tools/data-migration/Infrastructure/Telemetry/AlertsConfig.json`)
- [ ] T036 [P] Add container image + pipeline to run migration in controlled environment (`deploy/migration/Dockerfile`)
- [ ] T037 Final audit: ensure tasks cover all spec scenarios and acceptance criteria (`Plan/Foundation/specs/004-data-migration-etl/tasks.md`)

---

## Dependencies & Execution Order

- Setup (Phase 1)  Foundational (Phase 2)  US1 (P1)  US2/US3 (P2)  US4 (P3)  Polish
- US1 depends on manifest/bulk writer/checkout (Phase 2)
- US2 can start after bulk writer + mapping in place; relies on Phase 2
- US3 dual-write depends on US1 groundwork and mapping repository
- US4 validation depends on prior entity migrations to validate

## Parallel Execution Examples

- T007-T010 can run in parallel (project scaffolding + telemetry setup)
- T011-T015 parallelizable across manifest, repo, bulk writer components
- T021/T022 can run parallel with T023 performance harness once base mapping available
- T029-T031 can run parallel after migrations exist

## Implementation Strategy

- MVP = Phases 1-3 to migrate students/enrollments with resumable pipeline
- Incremental: add high-volume assessments (US2), dual-write/rollback safety (US3), then comprehensive validation/reporting (US4)
- Always run dry-run + validation scripts before full runs; keep reconciliation artifacts for audit
