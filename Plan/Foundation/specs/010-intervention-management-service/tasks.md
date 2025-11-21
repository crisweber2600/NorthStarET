---
description: "Task list for Intervention Management Service Migration (MTSS/RTI tracking)"
---

# Tasks: Intervention Management Service Migration

**Specification Branch**: `Foundation/010-intervention-management-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/010-intervention-management-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/010-intervention-management-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Intervention/`  
**Specification Path**: `Plan/Foundation/specs/010-intervention-management-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/services/Intervention/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/010-intervention-management-service/`)
- [ ] Shared dependencies (Assessment/Configuration/Section APIs, MassTransit, multi-tenancy) align between plan and spec
- [ ] Event publishers/subscribers documented consistently

---

## Layer Compliance Validation

- [ ] T001 Verify service references only Foundation shared libraries (`Src/Foundation/services/Intervention/Intervention.csproj`)
- [ ] T002 Verify no higher-layer coupling; downstream data via HTTP clients/events only
- [ ] T003 Ensure AppHost registers Intervention service with tenant middleware and RLS (`Src/Foundation/AppHost/Program.cs`)
- [ ] T004 Update README with layer placement, endpoints, and event catalog (`Src/Foundation/services/Intervention/README.md`)

---

## Identity & Authentication Compliance

- [ ] T005 Configure Microsoft.Identity.Web JWT validation + tenant claim enforcement (`Src/Foundation/services/Intervention/Program.cs`)
- [ ] T006 Ensure SessionAuthenticationHandler + Redis session caching registered (`Src/Foundation/services/Intervention/Program.cs`)
- [ ] T007 Enforce RLS via DbContext interceptor for `app.current_tenant` (`Src/Foundation/services/Intervention/Infrastructure/InterventionDbContext.cs`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold projects, migrations, bus, telemetry

- [ ] T008 Scaffold solution projects (API, Application, Domain, Infrastructure, Tests) under `Src/Foundation/services/Intervention/`
- [ ] T009 Create migrations for interventions, enrollments, sessions, attendance, progress notes, resources (`Src/Foundation/services/Intervention/Infrastructure/Migrations/`)
- [ ] T010 [P] Configure MassTransit + Azure Service Bus for intervention events (`Src/Foundation/services/Intervention/Program.cs`)
- [ ] T011 [P] Add OpenTelemetry tracing/metrics for attendance/effectiveness (`Src/Foundation/services/Intervention/Telemetry/TelemetryConfig.cs`)
- [ ] T012 [P] Setup tests (unit/integration) with seeded tenants (`tests/Foundation/Intervention/`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain model, repositories, audit, authorization

- [ ] T013 Implement domain entities + events (Intervention, Enrollment, Session, Attendance, ProgressNote, Resource) in `Src/Foundation/services/Intervention/Domain/`
- [ ] T014 Implement repositories with tenant filters and indexes (`Src/Foundation/services/Intervention/Infrastructure/Repositories/`)
- [ ] T015 Add RBAC policies (intervention.read/write) and role mapping (specialist, teacher, admin) (`Src/Foundation/services/Intervention/Infrastructure/Auth/Policies.cs`)
- [ ] T016 Add audit interceptor capturing before/after changes and communications (`Src/Foundation/services/Intervention/Infrastructure/Audit/AuditInterceptor.cs`)
- [ ] T017 Add idempotency cache for creation/attendance windows (`Src/Foundation/services/Intervention/Infrastructure/Idempotency/IdempotencyStore.cs`)

**Checkpoint**: Core model + auth/audit ready

---

## Phase 3: User Story 1 - Intervention Creation & Templates (Priority: P1) **MVP**

**Goal**: Create interventions (Tier 2/3), auto-generate sessions, support templates (scenarios 1, 10)

**Independent Test**: POST /interventions creates sessions per schedule, emits InterventionCreatedEvent, templates can be instantiated

### Implementation for User Story 1

- [ ] T018 [P] [US1] Implement POST /api/v1/interventions with schedule and auto session generation (`Src/Foundation/services/Intervention/API/InterventionsController.cs`)
- [ ] T019 [US1] Implement template CRUD + instantiation (`Src/Foundation/services/Intervention/API/TemplatesController.cs`)
- [ ] T020 [P] [US1] Publish InterventionCreatedEvent with minimal payload (`Src/Foundation/services/Intervention/Application/Events/InterventionEventPublisher.cs`)
- [ ] T021 [US1] Add integration tests for creation, session generation, template usage (`tests/Foundation/Intervention/Integration/InterventionCreationTests.cs`)

**Checkpoint**: Intervention creation + templates complete

---

## Phase 4: User Story 2 - Enrollment (Manual & Auto) (Priority: P1)

**Goal**: Enroll students manually and via assessment risk list (scenario 2)

**Independent Test**: Enrollment enforces uniqueness/active status; auto-enrollment consumes Assessment risk list; StudentEnrolledInInterventionEvent emitted

### Implementation for User Story 2

- [ ] T022 [P] [US2] Implement POST /api/v1/interventions/{id}/enroll endpoint with validation (`Src/Foundation/services/Intervention/API/EnrollmentController.cs`)
- [ ] T023 [US2] Implement auto-enrollment job consuming Assessment risk list API/event (`Src/Foundation/services/Intervention/Jobs/AutoEnrollmentJob.cs`)
- [ ] T024 [P] [US2] Publish StudentEnrolledInInterventionEvent with tenant metadata (`Src/Foundation/services/Intervention/Application/Events/EnrollmentEventPublisher.cs`)
- [ ] T025 [US2] Add integration tests for manual + auto enrollment (`tests/Foundation/Intervention/Integration/EnrollmentTests.cs`)

**Checkpoint**: Enrollment flows operational

---

## Phase 5: User Story 3 - Attendance & Progress Notes (Priority: P1)

**Goal**: Record session attendance and progress notes with eventing (scenarios 3, 4)

**Independent Test**: Attendance recorded with absence flagging, ProgressNote validates rating, events emitted; idempotency within 5m

### Implementation for User Story 3

- [ ] T026 [P] [US3] Implement POST /api/v1/sessions/{id}/attendance with absence streak detection (`Src/Foundation/services/Intervention/API/AttendanceController.cs`)
- [ ] T027 [US3] Implement progress note endpoint with rating validation (`Src/Foundation/services/Intervention/API/ProgressNotesController.cs`)
- [ ] T028 [P] [US3] Publish InterventionAttendanceRecordedEvent and ProgressNoteAddedEvent (`Src/Foundation/services/Intervention/Application/Events/AttendanceEventPublisher.cs`)
- [ ] T029 [US3] Add integration tests for attendance + notes + idempotency (`tests/Foundation/Intervention/Integration/AttendanceNoteTests.cs`)

**Checkpoint**: Attendance + notes complete

---

## Phase 6: User Story 4 - Scheduling Conflicts & Resources (Priority: P2)

**Goal**: Detect scheduling conflicts and manage resources (scenarios 5, 9)

**Independent Test**: Conflicts detected using Section/Configuration data; resource CRUD prevents double-booking; conflict returns 409

### Implementation for User Story 4

- [ ] T030 [P] [US4] Implement scheduling conflict checker using Section/Configuration service APIs (`Src/Foundation/services/Intervention/Application/Scheduling/ConflictChecker.cs`)
- [ ] T031 [US4] Implement resource CRUD with lock/status to avoid conflicts (`Src/Foundation/services/Intervention/API/ResourcesController.cs`)
- [ ] T032 [US4] Add integration tests for conflict detection and resource usage (`tests/Foundation/Intervention/Integration/ConflictResourceTests.cs`)

**Checkpoint**: Conflict/resource controls in place

---

## Phase 7: User Story 5 - Effectiveness & Dashboard (Priority: P2)

**Goal**: Compute effectiveness metrics and dashboard (scenario 6)

**Independent Test**: Effectiveness endpoint returns attendance %, score delta from Assessment Service, projection; p95 <200ms

### Implementation for User Story 5

- [ ] T033 [P] [US5] Implement effectiveness query service aggregating attendance + assessment deltas (`Src/Foundation/services/Intervention/Application/Effectiveness/EffectivenessService.cs`)
- [ ] T034 [US5] Implement dashboard endpoint exposing metrics (`Src/Foundation/services/Intervention/API/EffectivenessController.cs`)
- [ ] T035 [US5] Add performance tests for effectiveness calculation latency (`tests/Foundation/Intervention/Perf/EffectivenessPerfTests.cs`)

**Checkpoint**: Effectiveness dashboard live

---

## Phase 8: User Story 6 - Tier Transitions, Communications, Exit (Priority: P3)

**Goal**: Handle tier history, parent communications, and exit criteria evaluation (scenarios 7, 8, 11)

**Independent Test**: Tier changes recorded with events; communications logged; exit evaluator runs weekly and emits StudentExitedInterventionEvent

### Implementation for User Story 6

- [ ] T036 [P] [US6] Implement tier history tracking + future TierEscalatedEvent placeholder (`Src/Foundation/services/Intervention/Application/Tier/TierHistoryService.cs`)
- [ ] T037 [US6] Implement communication logging + email hooks for enrollment/progress summaries (`Src/Foundation/services/Intervention/Application/Communications/CommunicationService.cs`)
- [ ] T038 [US6] Implement exit criteria evaluator hosted job + StudentExitedInterventionEvent (`Src/Foundation/services/Intervention/Jobs/ExitCriteriaJob.cs`)
- [ ] T039 [US6] Tests for tier history, communications logging, and exit evaluation (`tests/Foundation/Intervention/Integration/TierExitTests.cs`)

**Checkpoint**: Tier transitions + communications handled

---

## Phase 9: User Story 7 - Audit Trail (Priority: P3)

**Goal**: Immutable audit for modifications and communications (scenario 12)

**Independent Test**: Audit endpoint returns before/after with user/timestamp; communications logged; export available

### Implementation for User Story 7

- [ ] T040 [P] [US7] Persist audit records via interceptor for all mutation endpoints (`Src/Foundation/services/Intervention/Infrastructure/Audit/AuditInterceptor.cs`)
- [ ] T041 [US7] Implement audit query/export endpoint (`Src/Foundation/services/Intervention/API/AuditController.cs`)
- [ ] T042 [US7] Add tests verifying audit coverage across interventions/enrollments/notes/communications (`tests/Foundation/Intervention/Integration/AuditTests.cs`)

**Checkpoint**: Audit trail complete

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T043 [P] Add dashboards/metrics for attendance rate, effectiveness calc duration, reminder job health (`Src/Foundation/services/Intervention/Telemetry/InterventionMetrics.cs`)
- [ ] T044 Harden error handling (problem+json) + resilience on downstream Assessment/Configuration calls (`Src/Foundation/services/Intervention/API/Middleware/ErrorHandlingMiddleware.cs`)
- [ ] T045 [P] Documentation: runbook, event catalog, conflict rules (`Src/Foundation/services/Intervention/docs/runbook.md`)
- [ ] T046 Final audit against spec scenarios + acceptance criteria (`Plan/Foundation/specs/010-intervention-management-service/tasks.md`)

---

## Dependencies & Execution Order

- Setup (Phase 1)  Foundational (Phase 2)  US1/US2/US3 (P1)  US4/US5 (P2)  US6/US7 (P3)  Polish
- US1 depends on domain + idempotency
- US2 depends on assessments/auto-enroll client
- US3 depends on sessions + idempotency
- US4 depends on section/configuration APIs + resource entities
- US5 depends on attendance/results data + assessment client
- US6 depends on events + communications logging
- US7 depends on audit interceptor

## Parallel Execution Examples

- T008-T012 parallel (projects, migrations, bus, telemetry, tests)
- US1 tasks T018-T020 parallelizable; T021 follows
- US2 tasks T022-T024 parallel; T025 follows
- US4 tasks T030-T031 parallel; T032 follows
- US6 tasks T036-T038 parallel; T039 follows

## Implementation Strategy

- MVP = Phases 1-3 delivering creation/templates/enrollment/attendance/notes with events
- Next deliver conflict/resource controls and effectiveness dashboard, then tier/exit/communications and audit hardening
- Keep performance budgets monitored (attendance/effectiveness); rely on cached external data where possible
