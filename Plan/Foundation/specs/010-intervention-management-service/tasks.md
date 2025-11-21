---
description: "Task list for Intervention Management Service Migration (MTSS/RTI workflows, tenant-isolated)"
---

# Tasks: Intervention Management Service Migration

**Specification Branch**: `Foundation/010-intervention-management-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/010-intervention-management-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/010-intervention-management-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

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
- [ ] Shared dependencies (ServiceDefaults, Domain, Application, Infrastructure) align between plan and spec
- [ ] Cross-layer dependencies (Student/Staff services, Configuration) documented

---

## Layer Compliance Validation

- [ ] T001 Verify service references only shared Foundation infrastructure (`Src/Foundation/services/Intervention/Intervention.Api/Intervention.Api.csproj`)
- [ ] T002 Verify NO direct references to higher-layer services; interactions via events or HTTP contracts only
- [ ] T003 Ensure AppHost registers Intervention service in Foundation layer isolation (`Src/Foundation/AppHost/Program.cs`)
- [ ] T004 Update README with layer placement, dependencies, and contracts (`Src/Foundation/services/Intervention/README.md`)

---

## Identity & Authentication Compliance

- [ ] T005 Verify Microsoft.Identity.Web used for JWT validation; no custom token issuance (`Src/Foundation/services/Intervention/Intervention.Api/Program.cs`)
- [ ] T006 Ensure SessionAuthenticationHandler + Redis session caching configured (`Src/Foundation/services/Intervention/Intervention.Api/Program.cs`)
- [ ] T007 Confirm tenant_id enforced in DbContext via multi-tenant infrastructure (`Src/Foundation/services/Intervention/Infrastructure/InterventionDbContext.cs`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold service projects, pipelines, and baseline configuration

- [ ] T008 Scaffold solution projects (Api, Domain, Application, Infrastructure, Scheduling, Messaging, Communications, Tests) under `Src/Foundation/services/Intervention/`
- [ ] T009 Configure EF Core migrations + DbContext for intervention schema (`Src/Foundation/services/Intervention/Infrastructure/InterventionDbContext.cs`)
- [ ] T010 [P] Add MassTransit + Azure Service Bus configuration for event publishing (`Src/Foundation/services/Intervention/Intervention.Api/Program.cs`)
- [ ] T011 [P] Add OpenTelemetry tracing/metrics with tenant attributes (`Src/Foundation/services/Intervention/Infrastructure/Telemetry/TelemetryConfig.cs`)
- [ ] T012 [P] Setup test projects (unit, integration, BDD/contract, performance) with fixtures (`tests/Foundation/Intervention/`)
- [ ] T013 [P] Configure Redis optional caching for dashboard metrics (`Src/Foundation/services/Intervention/Infrastructure/Caching/RedisCacheConfig.cs`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain, persistence, scheduling engine, and authorization scaffolding

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T014 Implement domain aggregates (Intervention, InterventionSession, Enrollment, AttendanceRecord, ProgressNote) with events in `Src/Foundation/services/Intervention/Domain/Aggregates/`
- [ ] T015 Create InterventionTemplate entity with versioning support in `Src/Foundation/services/Intervention/Domain/Entities/InterventionTemplate.cs`
- [ ] T016 Create ExitCriteria entity with threshold evaluation logic in `Src/Foundation/services/Intervention/Domain/Entities/ExitCriteria.cs`
- [ ] T017 Create CommunicationLog entity for caregiver messaging audit in `Src/Foundation/services/Intervention/Domain/Entities/CommunicationLog.cs`
- [ ] T018 Create AuditRecord entity for immutable change tracking in `Src/Foundation/services/Intervention/Domain/Entities/AuditRecord.cs`
- [ ] T019 Create repositories + unit of work with tenant-aware query filters (`Src/Foundation/services/Intervention/Infrastructure/Repositories/`)
- [ ] T020 Add idempotency hash store for creates/enrollments (`Src/Foundation/services/Intervention/Infrastructure/Idempotency/IdempotencyStore.cs`)
- [ ] T021 Implement authorization service checking role-based permissions for interventions in `Src/Foundation/services/Intervention/Application/Authorization/InterventionAuthorizationService.cs`
- [ ] T022 Add audit interceptor logging tenant/user/action/entity to `audit.AuditRecords` (`Src/Foundation/services/Intervention/Infrastructure/Audit/AuditInterceptor.cs`)
- [ ] T023 Implement scheduling conflict detection engine for facilitator/room/student overlaps in `Src/Foundation/services/Intervention/Scheduling/ConflictDetectionService.cs`
- [ ] T024 Implement session generation engine from templates in `Src/Foundation/services/Intervention/Scheduling/SessionGeneratorService.cs`
- [ ] T025 Add database indexes per data-model: (tenant_id, facilitator_id, start_time), (tenant_id, student_id, status) via migration (`Src/Foundation/services/Intervention/Infrastructure/Migrations/`)
- [ ] T026 Create materialized view for attendance rollups per intervention and student (`Src/Foundation/services/Intervention/Infrastructure/Migrations/CreateAttendanceRollupsView.sql`)

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Create interventions and enroll students (Priority: P1) 🎯 MVP

**Goal**: Educators create intervention groups or individual plans, schedule sessions, and enroll students manually or via risk lists

**Independent Test**: Create an intervention with schedule, enroll students (manual and auto), and verify events and visibility for participants

**Acceptance Scenarios**:
1. Given a new intervention definition with schedule, when saved, then sessions are generated, the intervention is persisted, and an InterventionCreated notification is emitted
2. Given a risk list feed, when auto-enrollment criteria are met, then eligible students are enrolled with audit history and an enrollment event is published
3. Given missing required fields (objective, owner, schedule), when creating an intervention, then the request is rejected with validation errors and no sessions are produced

### Tests for User Story 1 (BDD/Contract)

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T027 [P] [US1] Create Reqnroll feature file for intervention creation scenarios in `specs/010-intervention-management-service/features/intervention-creation.feature`
- [ ] T028 [P] [US1] Create Reqnroll feature file for enrollment scenarios in `specs/010-intervention-management-service/features/student-enrollment.feature`
- [ ] T029 [P] [US1] Implement step definitions for intervention creation in `tests/Foundation/Intervention/BDD/Steps/InterventionCreationSteps.cs`
- [ ] T030 [P] [US1] Implement step definitions for enrollment in `tests/Foundation/Intervention/BDD/Steps/EnrollmentSteps.cs`
- [ ] T031 [P] [US1] Create consumer tests for InterventionCreated event in `tests/Foundation/Intervention/Contract/InterventionCreatedEventTests.cs`
- [ ] T032 [P] [US1] Create consumer tests for InterventionEnrollmentChanged event in `tests/Foundation/Intervention/Contract/EnrollmentChangedEventTests.cs`

### Implementation for User Story 1

- [ ] T033 [P] [US1] Implement POST /api/interventions/templates with validation in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/InterventionTemplatesController.cs`
- [ ] T034 [P] [US1] Implement GET /api/interventions/templates with filtering in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/InterventionTemplatesController.cs`
- [ ] T035 [US1] Implement CreateInterventionCommand with validation in `Src/Foundation/services/Intervention/Application/Interventions/Commands/CreateInterventionCommand.cs`
- [ ] T036 [US1] Implement CreateInterventionCommandHandler with session generation and event publishing in `Src/Foundation/services/Intervention/Application/Interventions/Commands/CreateInterventionCommandHandler.cs`
- [ ] T037 [US1] Implement POST /api/interventions endpoint calling CreateInterventionCommand in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/InterventionsController.cs`
- [ ] T038 [US1] Implement GET /api/interventions with filtering by status, owner, school in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/InterventionsController.cs`
- [ ] T039 [US1] Implement GET /api/interventions/{id} ensuring tenant filter in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/InterventionsController.cs`
- [ ] T040 [P] [US1] Implement EnrollStudentsCommand for manual enrollment in `Src/Foundation/services/Intervention/Application/Enrollments/Commands/EnrollStudentsCommand.cs`
- [ ] T041 [US1] Implement EnrollStudentsCommandHandler with idempotency and event publishing in `Src/Foundation/services/Intervention/Application/Enrollments/Commands/EnrollStudentsCommandHandler.cs`
- [ ] T042 [US1] Implement POST /api/interventions/{id}/enrollments for manual enrollment in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/EnrollmentsController.cs`
- [ ] T043 [P] [US1] Implement auto-enrollment consumer listening to risk list events in `Src/Foundation/services/Intervention/Messaging/Consumers/RiskListUpdatedConsumer.cs`
- [ ] T044 [US1] Implement InterventionCreated event publishing in `Src/Foundation/services/Intervention/Messaging/Events/InterventionCreatedEvent.cs`
- [ ] T045 [US1] Implement InterventionEnrollmentChanged event publishing in `Src/Foundation/services/Intervention/Messaging/Events/InterventionEnrollmentChangedEvent.cs`
- [ ] T046 [US1] Implement InterventionSessionScheduled event publishing in `Src/Foundation/services/Intervention/Messaging/Events/InterventionSessionScheduledEvent.cs`
- [ ] T047 [US1] Add validation rules for intervention creation using FluentValidation in `Src/Foundation/services/Intervention/Application/Interventions/Validators/CreateInterventionValidator.cs`
- [ ] T048 [US1] Add integration tests for intervention CRUD operations (`tests/Foundation/Intervention/Integration/InterventionCrudTests.cs`)

**Checkpoint**: Intervention creation and enrollment validated with events

---

## Phase 4: User Story 2 - Record attendance, notes, and progress (Priority: P2)

**Goal**: Facilitators capture session attendance, notes, and progress ratings with conflict detection and audit

**Independent Test**: Record attendance for a session, add progress notes, detect scheduling conflicts, and confirm audit entries and downstream notifications

**Acceptance Scenarios**:
1. Given an active session, when attendance is recorded for all participants, then present/absent statuses and notes are stored and an attendance event is sent
2. Given overlapping sessions for a facilitator or room, when saving attendance, then the system flags the conflict and prevents double booking
3. Given a progress note update, when saved, then the note persists with rating, actor, and timestamp and is included in the audit trail

### Tests for User Story 2 (BDD/Contract)

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T049 [P] [US2] Create Reqnroll feature file for attendance recording in `specs/010-intervention-management-service/features/attendance-recording.feature`
- [ ] T050 [P] [US2] Create Reqnroll feature file for progress notes in `specs/010-intervention-management-service/features/progress-notes.feature`
- [ ] T051 [P] [US2] Create Reqnroll feature file for conflict detection in `specs/010-intervention-management-service/features/scheduling-conflicts.feature`
- [ ] T052 [P] [US2] Implement step definitions for attendance in `tests/Foundation/Intervention/BDD/Steps/AttendanceSteps.cs`
- [ ] T053 [P] [US2] Implement step definitions for progress notes in `tests/Foundation/Intervention/BDD/Steps/ProgressNoteSteps.cs`
- [ ] T054 [P] [US2] Implement step definitions for conflict detection in `tests/Foundation/Intervention/BDD/Steps/ConflictDetectionSteps.cs`
- [ ] T055 [P] [US2] Create consumer tests for AttendanceRecorded event in `tests/Foundation/Intervention/Contract/AttendanceRecordedEventTests.cs`
- [ ] T056 [P] [US2] Create consumer tests for ProgressUpdated event in `tests/Foundation/Intervention/Contract/ProgressUpdatedEventTests.cs`

### Implementation for User Story 2

- [ ] T057 [P] [US2] Implement RecordAttendanceCommand with status and notes in `Src/Foundation/services/Intervention/Application/Attendance/Commands/RecordAttendanceCommand.cs`
- [ ] T058 [US2] Implement RecordAttendanceCommandHandler with conflict detection and event publishing in `Src/Foundation/services/Intervention/Application/Attendance/Commands/RecordAttendanceCommandHandler.cs`
- [ ] T059 [US2] Implement POST /api/interventions/sessions/{sessionId}/attendance endpoint in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/AttendanceController.cs`
- [ ] T060 [US2] Enhance conflict detection to validate facilitator availability in `Src/Foundation/services/Intervention/Scheduling/ConflictDetectionService.cs`
- [ ] T061 [US2] Enhance conflict detection to validate room availability in `Src/Foundation/services/Intervention/Scheduling/ConflictDetectionService.cs`
- [ ] T062 [US2] Enhance conflict detection to validate student not double-booked in `Src/Foundation/services/Intervention/Scheduling/ConflictDetectionService.cs`
- [ ] T063 [P] [US2] Implement AddProgressNoteCommand with rating and notes in `Src/Foundation/services/Intervention/Application/Progress/Commands/AddProgressNoteCommand.cs`
- [ ] T064 [US2] Implement AddProgressNoteCommandHandler with audit tracking in `Src/Foundation/services/Intervention/Application/Progress/Commands/AddProgressNoteCommandHandler.cs`
- [ ] T065 [US2] Implement POST /api/interventions/{id}/progress endpoint in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/ProgressController.cs`
- [ ] T066 [US2] Implement GET /api/interventions/{id}/progress for chronological history in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/ProgressController.cs`
- [ ] T067 [US2] Implement AttendanceRecorded event publishing in `Src/Foundation/services/Intervention/Messaging/Events/AttendanceRecordedEvent.cs`
- [ ] T068 [US2] Implement ProgressUpdated event publishing in `Src/Foundation/services/Intervention/Messaging/Events/ProgressUpdatedEvent.cs`
- [ ] T069 [US2] Add validation rules for attendance recording using FluentValidation in `Src/Foundation/services/Intervention/Application/Attendance/Validators/RecordAttendanceValidator.cs`
- [ ] T070 [US2] Add integration tests for attendance recording with conflict detection (`tests/Foundation/Intervention/Integration/AttendanceTests.cs`)
- [ ] T071 [US2] Add integration tests for progress notes (`tests/Foundation/Intervention/Integration/ProgressNoteTests.cs`)
- [ ] T072 [US2] Add performance tests for conflict detection accuracy >99% (`tests/Foundation/Intervention/Performance/ConflictDetectionPerfTests.cs`)

**Checkpoint**: Attendance and progress recording functional with conflict prevention

---

## Phase 5: User Story 3 - Evaluate effectiveness and communicate outcomes (Priority: P3)

**Goal**: Educators monitor dashboards (attendance %, progress trajectory, exit criteria) and send communications to caregivers with auditability

**Independent Test**: View effectiveness metrics for a group, trigger exit upon meeting criteria, and send a caregiver summary while confirming delivery and audit logging

**Acceptance Scenarios**:
1. Given configured exit criteria, when a student meets the threshold, then a recommended exit status is generated and an exit event is recorded upon confirmation
2. Given a caregiver communication, when sent, then the message uses the selected template, records delivery status, and attaches to the student's communication log
3. Given a dashboard request, when data is aggregated, then metrics (attendance rate, progress trend) return within target latency and reflect latest attendance/results

### Tests for User Story 3 (BDD/Contract)

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T073 [P] [US3] Create Reqnroll feature file for exit criteria evaluation in `specs/010-intervention-management-service/features/exit-criteria.feature`
- [ ] T074 [P] [US3] Create Reqnroll feature file for caregiver communications in `specs/010-intervention-management-service/features/caregiver-communications.feature`
- [ ] T075 [P] [US3] Create Reqnroll feature file for effectiveness dashboards in `specs/010-intervention-management-service/features/effectiveness-dashboards.feature`
- [ ] T076 [P] [US3] Implement step definitions for exit criteria in `tests/Foundation/Intervention/BDD/Steps/ExitCriteriaSteps.cs`
- [ ] T077 [P] [US3] Implement step definitions for communications in `tests/Foundation/Intervention/BDD/Steps/CommunicationSteps.cs`
- [ ] T078 [P] [US3] Implement step definitions for dashboards in `tests/Foundation/Intervention/BDD/Steps/DashboardSteps.cs`
- [ ] T079 [P] [US3] Create consumer tests for InterventionExited event in `tests/Foundation/Intervention/Contract/InterventionExitedEventTests.cs`
- [ ] T080 [P] [US3] Create consumer tests for CommunicationSent event in `tests/Foundation/Intervention/Contract/CommunicationSentEventTests.cs`

### Implementation for User Story 3

- [ ] T081 [P] [US3] Implement EvaluateExitCriteriaCommand in `Src/Foundation/services/Intervention/Application/ExitCriteria/Commands/EvaluateExitCriteriaCommand.cs`
- [ ] T082 [US3] Implement EvaluateExitCriteriaCommandHandler with threshold evaluation logic in `Src/Foundation/services/Intervention/Application/ExitCriteria/Commands/EvaluateExitCriteriaCommandHandler.cs`
- [ ] T083 [P] [US3] Implement ConfirmExitCommand in `Src/Foundation/services/Intervention/Application/ExitCriteria/Commands/ConfirmExitCommand.cs`
- [ ] T084 [US3] Implement ConfirmExitCommandHandler with enrollment status update in `Src/Foundation/services/Intervention/Application/ExitCriteria/Commands/ConfirmExitCommandHandler.cs`
- [ ] T085 [US3] Implement POST /api/interventions/{id}/exit endpoint in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/ExitController.cs`
- [ ] T086 [US3] Implement GET /api/interventions/{id}/exit-recommendations in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/ExitController.cs`
- [ ] T087 [P] [US3] Implement SendCommunicationCommand with template reference in `Src/Foundation/services/Intervention/Application/Communications/Commands/SendCommunicationCommand.cs`
- [ ] T088 [US3] Implement SendCommunicationCommandHandler with template retrieval from Configuration service in `Src/Foundation/services/Intervention/Application/Communications/Commands/SendCommunicationCommandHandler.cs`
- [ ] T089 [US3] Implement POST /api/interventions/{id}/communications endpoint in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/CommunicationsController.cs`
- [ ] T090 [US3] Implement communication delivery tracking in `Src/Foundation/services/Intervention/Communications/DeliveryTrackingService.cs`
- [ ] T091 [US3] Implement communication template rendering in `Src/Foundation/services/Intervention/Communications/TemplateRenderingService.cs`
- [ ] T092 [P] [US3] Implement GetDashboardMetricsQuery in `Src/Foundation/services/Intervention/Application/Dashboard/Queries/GetDashboardMetricsQuery.cs`
- [ ] T093 [US3] Implement GetDashboardMetricsQueryHandler using materialized view for performance in `Src/Foundation/services/Intervention/Application/Dashboard/Queries/GetDashboardMetricsQueryHandler.cs`
- [ ] T094 [US3] Implement GET /api/interventions/{id}/dashboard endpoint with p95 <200ms in `Src/Foundation/services/Intervention/Intervention.Api/Controllers/DashboardController.cs`
- [ ] T095 [US3] Add Redis caching for dashboard metrics with appropriate TTL in `Src/Foundation/services/Intervention/Infrastructure/Caching/DashboardMetricsCacheService.cs`
- [ ] T096 [US3] Implement InterventionExited event publishing in `Src/Foundation/services/Intervention/Messaging/Events/InterventionExitedEvent.cs`
- [ ] T097 [US3] Implement CommunicationSent event publishing in `Src/Foundation/services/Intervention/Messaging/Events/CommunicationSentEvent.cs`
- [ ] T098 [US3] Add validation rules for communications using FluentValidation in `Src/Foundation/services/Intervention/Application/Communications/Validators/SendCommunicationValidator.cs`
- [ ] T099 [US3] Add integration tests for exit criteria evaluation (`tests/Foundation/Intervention/Integration/ExitCriteriaTests.cs`)
- [ ] T100 [US3] Add integration tests for caregiver communications with delivery tracking (`tests/Foundation/Intervention/Integration/CommunicationTests.cs`)
- [ ] T101 [US3] Add performance tests for dashboard latency p95 <200ms (`tests/Foundation/Intervention/Performance/DashboardLatencyTests.cs`)

**Checkpoint**: All user stories independently functional with dashboards and communications

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T102 [P] Add API documentation with OpenAPI/Swagger specs in `Src/Foundation/services/Intervention/Intervention.Api/OpenApi/`
- [ ] T103 [P] Update service README with architecture, endpoints, events, and quickstart in `Src/Foundation/services/Intervention/README.md`
- [ ] T104 Add comprehensive error handling and standardized error responses in `Src/Foundation/services/Intervention/Intervention.Api/Middleware/ErrorHandlingMiddleware.cs`
- [ ] T105 [P] Add rate limiting for create/enroll endpoints (10 req/min per spec) in `Src/Foundation/services/Intervention/Intervention.Api/Middleware/RateLimitingMiddleware.cs`
- [ ] T106 [P] Add request logging with correlation IDs for distributed tracing in `Src/Foundation/services/Intervention/Intervention.Api/Middleware/RequestLoggingMiddleware.cs`
- [ ] T107 [P] Add health checks for database, Redis, message bus in `Src/Foundation/services/Intervention/Intervention.Api/HealthChecks/`
- [ ] T108 Add background service for refreshing materialized views in `Src/Foundation/services/Intervention/Infrastructure/BackgroundServices/ViewRefreshService.cs`
- [ ] T109 [P] Add unit tests for domain aggregates and value objects (`tests/Foundation/Intervention/Unit/Domain/`)
- [ ] T110 [P] Add unit tests for application command/query handlers (`tests/Foundation/Intervention/Unit/Application/`)
- [ ] T111 Add code coverage report generation and validation ≥80% coverage
- [ ] T112 Validate quickstart.md steps with fresh environment (`Plan/Foundation/specs/010-intervention-management-service/quickstart.md`)
- [ ] T113 [P] Add deployment configuration for Aspire AppHost in `Src/Foundation/AppHost/InterventionServiceConfig.cs`
- [ ] T114 Security audit: verify tenant isolation, RLS policies, RBAC enforcement
- [ ] T115 Performance validation: verify all SLOs (create/enroll <100ms p95, attendance <2s, conflict >99%, dashboard <200ms p95)
- [ ] T116 [P] Create operational runbook documentation in `docs/operations/intervention-service-runbook.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories. **This is the MVP.**
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Depends on US1 for intervention and enrollment data, but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Depends on US1 (intervention/enrollment) and US2 (attendance/progress data) for meaningful dashboards and exit criteria

### Within Each User Story

- Tests (BDD/Contract) MUST be written and FAIL before implementation
- Domain events before event publishers
- Commands before command handlers
- Queries before query handlers
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- All BDD/contract tests for a user story marked [P] can run in parallel
- Domain entities marked [P] within Foundational can run in parallel
- Event implementations marked [P] within a user story can run in parallel
- Once Foundational phase completes, different team members can work on different user stories in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all BDD feature files for User Story 1 together:
Task: "Create Reqnroll feature file for intervention creation scenarios"
Task: "Create Reqnroll feature file for enrollment scenarios"

# Launch all event implementations for User Story 1 together:
Task: "Implement InterventionCreated event publishing"
Task: "Implement InterventionEnrollmentChanged event publishing"
Task: "Implement InterventionSessionScheduled event publishing"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

**MVP Deliverable**: Educators can create interventions with templates, generate sessions automatically, enroll students manually or via auto-enrollment from risk lists, and receive confirmation events. This establishes the core entity and workflow needed for any MTSS implementation.

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo (adds day-to-day operations)
4. Add User Story 3 → Test independently → Deploy/Demo (adds effectiveness monitoring and stakeholder engagement)
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (intervention creation and enrollment)
   - Developer B: User Story 2 (attendance and progress tracking) - can start parallel if US1 entities are stable
   - Developer C: User Story 3 (dashboards and communications) - starts after US1/US2 data available
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies, can run in parallel
- [US1], [US2], [US3] labels map task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify BDD/contract tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- Performance SLOs: Create/enroll p95 <100ms, Attendance events 99% <2s, Conflict detection >99% accuracy, Dashboard p95 <200ms
- All operations MUST enforce tenant isolation via `tenant_id` filtering and RLS policies
- All state changes MUST produce audit log entries
- Events MUST include retry logic for transient failures with exponential backoff
