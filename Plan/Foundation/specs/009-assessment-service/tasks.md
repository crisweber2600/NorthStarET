---
description: "Task list for Assessment Service Migration (definitions, assignments, scoring, analytics)"
---

# Tasks: Assessment Service Migration

**Specification Branch**: `Foundation/009-assessment-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/009-assessment-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/009-assessment-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Assessment/`  
**Specification Path**: `Plan/Foundation/specs/009-assessment-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/services/Assessment/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/009-assessment-service/`)
- [ ] Shared dependencies (MassTransit, PostgreSQL/JSONB, Redis cache, OpenTelemetry) align between plan and spec
- [ ] Cross-service events (StudentEnrolled, Config scale) documented

---

## Layer Compliance Validation

- [ ] T001 Verify service references only Foundation shared libraries and multi-tenant infrastructure (`Src/Foundation/services/Assessment/Assessment.csproj`)
- [ ] T002 Verify no higher-layer references; external interactions via contracts/events only
- [ ] T003 Ensure AppHost registers Assessment service with RLS tenant middleware (`Src/Foundation/AppHost/Program.cs`)
- [ ] T004 Update README with layer placement, endpoints, and event catalog (`Src/Foundation/services/Assessment/README.md`)

---

## Identity & Authentication Compliance

- [ ] T005 Configure Microsoft.Identity.Web JWT validation with tenant claim enforcement (`Src/Foundation/services/Assessment/Program.cs`)
- [ ] T006 Ensure SessionAuthenticationHandler + Redis session caching registered (`Src/Foundation/services/Assessment/Program.cs`)
- [ ] T007 Enforce RLS via DbContext interceptor setting `app.current_tenant` (`Src/Foundation/services/Assessment/Infrastructure/AssessmentDbContext.cs`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold projects, migrations, cache/telemetry/event plumbing

- [ ] T008 Scaffold solution projects (API, Application, Domain, Infrastructure, Tests) under `Src/Foundation/services/Assessment/`
- [ ] T009 Create initial migrations for assessments, fields, assignments, results, benchmarks, templates (`Src/Foundation/services/Assessment/Infrastructure/Migrations/`)
- [ ] T010 [P] Configure MassTransit + Azure Service Bus for assessment events (`Src/Foundation/services/Assessment/Program.cs`)
- [ ] T011 [P] Add Redis cache provider for templates/benchmarks (`Src/Foundation/services/Assessment/Infrastructure/Cache/RedisCacheProvider.cs`)
- [ ] T012 [P] Add OpenTelemetry tracing/metrics for scoring/trends (`Src/Foundation/services/Assessment/Telemetry/TelemetryConfig.cs`)
- [ ] T013 [P] Setup tests (unit, integration, perf) with seeded tenants (`tests/Foundation/Assessment/`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain model, repositories, scoring engine strategies, audit interceptor

- [ ] T014 Implement domain entities + events (Assessment, Field, Assignment, Result, Benchmark, Template) in `Src/Foundation/services/Assessment/Domain/`
- [ ] T015 Implement repositories with tenant filters and JSONB support (`Src/Foundation/services/Assessment/Infrastructure/Repositories/`)
- [ ] T016 Implement scoring strategy pattern (Points, Weighted, Rubric) in `Src/Foundation/services/Assessment/Application/Scoring/`
- [ ] T017 Add audit interceptor capturing before/after snapshots (`Src/Foundation/services/Assessment/Infrastructure/Audit/AuditInterceptor.cs`)
- [ ] T018 Add idempotency cache for assessment creation (10m) and result recording (5m) (`Src/Foundation/services/Assessment/Infrastructure/Idempotency/IdempotencyStore.cs`)

**Checkpoint**: Core model + scoring + audit ready

---

## Phase 3: User Story 1 - Assessment Definitions & Templates (Priority: P1) **MVP**

**Goal**: Create assessments with custom fields, templates, and publish creation events (scenarios 1, 10)

**Independent Test**: POST /assessments persists definition with fields, emits AssessmentCreatedEvent, templates can be materialized

### Implementation for User Story 1

- [ ] T019 [P] [US1] Implement POST /api/v1/assessments with custom fields (JSONB) validation (`Src/Foundation/services/Assessment/API/AssessmentsController.cs`)
- [ ] T020 [US1] Implement template library (create/list/apply) endpoints (`Src/Foundation/services/Assessment/API/TemplatesController.cs`)
- [ ] T021 [P] [US1] Publish AssessmentCreatedEvent with minimal payload (`Src/Foundation/services/Assessment/Application/Events/AssessmentEventPublisher.cs`)
- [ ] T022 [US1] Add integration tests for create/template flows + event emission (`tests/Foundation/Assessment/Integration/AssessmentCreationTests.cs`)

**Checkpoint**: Definitions/templates operational

---

## Phase 4: User Story 2 - Assignments (Priority: P1)

**Goal**: Assign assessments to rosters/students and publish events (scenario 2)

**Independent Test**: Assignments created for students with due dates; AssessmentAssignedEvent emitted per student; tenant isolation enforced

### Implementation for User Story 2

- [ ] T023 [P] [US2] Implement POST /api/v1/assessments/{id}/assign with batch assignment logic (`Src/Foundation/services/Assessment/API/AssignmentsController.cs`)
- [ ] T024 [US2] Publish AssessmentAssignedEvent per student with batching considerations (`Src/Foundation/services/Assessment/Application/Events/AssignmentEventPublisher.cs`)
- [ ] T025 [US2] Integration tests for assignment creation and events (`tests/Foundation/Assessment/Integration/AssignmentTests.cs`)

**Checkpoint**: Assignment flow complete

---

## Phase 5: User Story 3 - Record Results, Scoring, Benchmarks (Priority: P1)

**Goal**: Record results with scoring engine, benchmark classification, and events (scenarios 3, 4, 5)

**Independent Test**: Recording results computes score + benchmarkLevel, publishes AssessmentResultRecordedEvent; validation rejects out-of-range values

### Implementation for User Story 3

- [ ] T026 [P] [US3] Implement POST /api/v1/assessments/{id}/results with scoring strategy selection (`Src/Foundation/services/Assessment/API/ResultsController.cs`)
- [ ] T027 [US3] Implement benchmark CRUD + classification service (`Src/Foundation/services/Assessment/Application/Benchmarks/BenchmarkService.cs`)
- [ ] T028 [P] [US3] Publish AssessmentResultRecordedEvent including benchmarkLevel (`Src/Foundation/services/Assessment/Application/Events/ResultEventPublisher.cs`)
- [ ] T029 [US3] Add integration + unit tests for scoring strategies and benchmark classification (`tests/Foundation/Assessment/Integration/ScoringTests.cs`)

**Checkpoint**: Result recording + benchmarks validated

---

## Phase 6: User Story 4 - Search & Trends (Priority: P2)

**Goal**: Search assessments and compute student trends (scenarios 6, 7)

**Independent Test**: Search returns tenant-filtered results with p95 <100ms; trend endpoint returns slope/direction <200ms

### Implementation for User Story 4

- [ ] T030 [P] [US4] Implement search endpoint with filters + pagination + indexes (`Src/Foundation/services/Assessment/API/SearchController.cs`)
- [ ] T031 [US4] Add indexes (tenant_id, subject, assessment_type, created_at DESC) migration (`Src/Foundation/services/Assessment/Infrastructure/Migrations/AddSearchIndexes.cs`)
- [ ] T032 [P] [US4] Implement StudentTrendsQuery with regression + optional cache (`Src/Foundation/services/Assessment/Application/Trends/StudentTrendsService.cs`)
- [ ] T033 [US4] Performance tests for search and trends latency (`tests/Foundation/Assessment/Perf/SearchTrendPerfTests.cs`)

**Checkpoint**: Search + trends meet SLOs

---

## Phase 7: User Story 5 - Export & State Test Import (Priority: P2)

**Goal**: Export results securely and import state tests via strategy parsers (scenarios 8, 9)

**Independent Test**: Export returns authorized CSV with audit; state test import parses by format and publishes StateTestResultsImportedEvent

### Implementation for User Story 5

- [ ] T034 [P] [US5] Implement export endpoint with pagination and audit logging (`Src/Foundation/services/Assessment/API/ExportController.cs`)
- [ ] T035 [US5] Implement state test import endpoint with parser strategies (Calpads/Peims) (`Src/Foundation/services/Assessment/Application/Import/StateTestImportService.cs`)
- [ ] T036 [P] [US5] Publish StateTestResultsImportedEvent on successful import (`Src/Foundation/services/Assessment/Application/Events/StateTestEventPublisher.cs`)
- [ ] T037 [US5] Add integration tests for export + state test import flows (`tests/Foundation/Assessment/Integration/ExportImportTests.cs`)

**Checkpoint**: Export/import features validated

---

## Phase 8: User Story 6 - Scheduling & Reminders (Priority: P3)

**Goal**: Schedule reminders for upcoming assessments and completion dashboard (scenario 11)

**Independent Test**: Scheduled reminders fire at 7d/1d/day-of; dashboard shows completion counts; events emitted for reminders

### Implementation for User Story 6

- [ ] T038 [P] [US6] Implement scheduling configuration storage + cron parsing (`Src/Foundation/services/Assessment/Infrastructure/Scheduling/ScheduleStore.cs`)
- [ ] T039 [US6] Implement hosted service to send reminders + publish events (`Src/Foundation/services/Assessment/Jobs/ReminderJob.cs`)
- [ ] T040 [US6] Add dashboard endpoint summarizing completion status (`Src/Foundation/services/Assessment/API/DashboardController.cs`)
- [ ] T041 [US6] Add tests for reminder scheduling and dashboard counts (`tests/Foundation/Assessment/Integration/ReminderDashboardTests.cs`)

**Checkpoint**: Scheduling/reminders operational

---

## Phase 9: User Story 7 - Audit Trail (Priority: P3)

**Goal**: Immutable audit trail for all mutations (scenario 12)

**Independent Test**: Audit records capture user/timestamp/before/after/reason for create/update operations; exportable for compliance

### Implementation for User Story 7

- [ ] T042 [P] [US7] Persist audit records via interceptor across entities (`Src/Foundation/services/Assessment/Infrastructure/Audit/AuditInterceptor.cs`)
- [ ] T043 [US7] Implement audit query/export endpoint with filters (`Src/Foundation/services/Assessment/API/AuditController.cs`)
- [ ] T044 [US7] Tests ensuring audit captured for create/update/delete across key entities (`tests/Foundation/Assessment/Integration/AuditTests.cs`)

**Checkpoint**: Audit trail complete

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T045 [P] Add projection view + materialized view refresh for field_scores (`Src/Foundation/services/Assessment/Infrastructure/Migrations/AddProjectionViews.cs`)
- [ ] T046 Harden error handling for import/scoring (problem+json responses) (`Src/Foundation/services/Assessment/API/Middleware/ErrorHandlingMiddleware.cs`)
- [ ] T047 [P] Add metrics/dashboards for scoring/trend/assignment throughput (`Src/Foundation/services/Assessment/Telemetry/AssessmentMetrics.cs`)
- [ ] T048 Final audit vs plan/spec scenarios + acceptance criteria (`Plan/Foundation/specs/009-assessment-service/tasks.md`)

---

## Dependencies & Execution Order

- Setup (Phase 1)  Foundational (Phase 2)  US1/US2/US3 (P1)  US4/US5 (P2)  US6/US7 (P3)  Polish
- US1 depends on domain/scoring/audit/idempotency
- US2 depends on definitions existing
- US3 depends on scoring engine + benchmarks
- US4 depends on search indexes + result data
- US5 depends on assignments/results + export/audit
- US6 depends on assignments + scheduler infra
- US7 depends on audit interceptor baseline

## Parallel Execution Examples

- T008-T013 parallel (projects, migrations, bus, cache, telemetry, tests)
- US1 tasks T019-T021 parallelizable; T022 follows
- US3 tasks T026-T028 parallel; T029 follows
- US5 tasks T034-T036 parallel; tests follow

## Implementation Strategy

- MVP = Phases 1-3 (definitions/templates + assignments + scoring/benchmarks)
- Next deliver search/trends and export/import, then scheduling/reminders and audit hardening
- Monitor performance budgets early with perf tests; cache hot paths (templates/benchmarks/trends) as needed
