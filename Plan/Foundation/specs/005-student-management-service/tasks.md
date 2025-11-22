---
description: "Task list for Student Management Service Migration (event-driven, tenant-isolated)"
---

# Tasks: Student Management Service Migration

**Specification Branch**: `Foundation/005-student-management-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/005-student-management-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/005-student-management-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/StudentManagement/`  
**Specification Path**: `Plan/Foundation/specs/005-student-management-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/services/StudentManagement/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/005-student-management-service/`)
- [ ] Shared dependencies (Identity, Configuration, MassTransit, Blob storage) align between plan and spec
- [ ] Cross-layer dependencies (Assessment/Section/Reporting consumers) documented

---

## Layer Compliance Validation

- [ ] T001 Verify service references only shared Foundation infrastructure and bus abstractions (`Src/Foundation/services/StudentManagement/StudentManagement.csproj`)
- [ ] T002 Verify NO direct references to higher-layer services; outbound interactions via events or HTTP contracts only
- [ ] T003 Ensure AppHost registers Student service in Foundation layer isolation (`Src/Foundation/AppHost/Program.cs`)
- [ ] T004 Update README with layer placement, dependencies, and contracts (`Src/Foundation/services/StudentManagement/README.md`)

---

## Identity & Authentication Compliance

- [ ] T005 Verify Microsoft.Identity.Web used for JWT validation; no custom token issuance (`Src/Foundation/services/StudentManagement/Program.cs`)
- [ ] T006 Ensure SessionAuthenticationHandler + Redis session caching configured (`Src/Foundation/services/StudentManagement/Program.cs`)
- [ ] T007 Confirm tenant_id enforced in DbContext via multi-tenant infrastructure (`Src/Foundation/services/StudentManagement/Infrastructure/StudentDbContext.cs`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold service projects, pipelines, and baseline configuration

- [ ] T008 Scaffold solution projects (API, Domain, Application, Infrastructure, Tests) under `Src/Foundation/services/StudentManagement/`
- [ ] T009 Configure EF Core migrations + DbContext for student/enrollment schema (`Src/Foundation/services/StudentManagement/Infrastructure/Migrations/`)
- [ ] T010 [P] Add MassTransit + Azure Service Bus configuration for event publishing (`Src/Foundation/services/StudentManagement/Program.cs`)
- [ ] T011 [P] Add OpenTelemetry tracing/metrics with tenant attributes (`Src/Foundation/services/StudentManagement/Telemetry/TelemetryConfig.cs`)
- [ ] T012 [P] Setup test projects (unit, integration, contract/BDD) with fixtures (`tests/Foundation/StudentManagement/`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain, persistence, and authorization scaffolding

- [ ] T013 Implement domain aggregates (Student, Enrollment) with events in `Src/Foundation/services/StudentManagement/Domain/`
- [ ] T014 Create repositories + unit of work with tenant-aware query filters (`Src/Foundation/services/StudentManagement/Infrastructure/Repositories/`)
- [ ] T015 Add idempotency hash store for creates/imports (`Src/Foundation/services/StudentManagement/Infrastructure/Idempotency/IdempotencyStore.cs`)
- [ ] T016 Implement authorization service checking educational interest (Enrollment + Section membership) in `Src/Foundation/services/StudentManagement/Application/Authorization/EducationalInterestValidator.cs`
- [ ] T017 Add audit interceptor logging tenant/user/action/entity to `audit.AuditRecords` (`Src/Foundation/services/StudentManagement/Infrastructure/Audit/AuditInterceptor.cs`)

**Checkpoint**: Domain model + infra ready for user stories

---

## Phase 3: User Story 1 - Student CRUD + Demographics Events (Priority: P1) **MVP**

**Goal**: Create/read/update students with events and audit (scenarios 1-3)

**Independent Test**: Create/update returns 201/200 with tenant isolation; StudentCreated/StudentDemographicsChanged events emitted; audit entries recorded

### Implementation for User Story 1

- [ ] T018 [P] [US1] Implement POST /api/v1/students with validation + idempotency hash in `Src/Foundation/services/StudentManagement/API/StudentsController.cs`
- [ ] T019 [US1] Implement GET /api/v1/students/{id} ensuring tenant filter and masked fields (`Src/Foundation/services/StudentManagement/API/StudentsController.cs`)
- [ ] T020 [US1] Implement PUT/PATCH for demographics updates and publish StudentDemographicsChangedEvent (`Src/Foundation/services/StudentManagement/Application/Students/UpdateDemographicsHandler.cs`)
- [ ] T021 [P] [US1] Add StudentCreatedEvent/StudentDemographicsChangedEvent publishing with MassTransit (`Src/Foundation/services/StudentManagement/Application/Students/StudentEventPublisher.cs`)
- [ ] T022 [US1] Add integration + contract tests for create/get/update endpoints (`tests/Foundation/StudentManagement/Integration/StudentCrudTests.cs`)

**Checkpoint**: Student CRUD + events validated

---

## Phase 4: User Story 2 - Search & Query Performance (Priority: P1)

**Goal**: Tenant-filtered search with p95 <100ms and wildcard support (scenario 4)

**Independent Test**: Search endpoint returns expected results, honors filters, and meets latency SLO under load

### Implementation for User Story 2

- [ ] T023 [P] [US2] Implement search endpoint with pagination/filtering in `Src/Foundation/services/StudentManagement/API/StudentsSearchController.cs`
- [ ] T024 [US2] Add indexes per data-model (TenantId, LastName, CreatedAt, Status) via migration (`Src/Foundation/services/StudentManagement/Infrastructure/Migrations/AddStudentIndexes.cs`)
- [ ] T025 [P] [US2] Add caching for photo metadata to avoid blob retrieval in search (`Src/Foundation/services/StudentManagement/Infrastructure/Photos/PhotoMetadataCache.cs`)
- [ ] T026 [US2] Add performance/load tests for search latency (`tests/Foundation/StudentManagement/Perf/SearchLatencyTests.cs`)

**Checkpoint**: Search meets SLO with tenant isolation

---

## Phase 5: User Story 3 - Enrollment & Withdrawal (Priority: P1)

**Goal**: Manage enrollments and withdrawals with events (scenarios 5, 8)

**Independent Test**: Enrollment creates record, publishes StudentEnrolledEvent; withdrawal updates status, hides from active lists

### Implementation for User Story 3

- [ ] T027 [P] [US3] Implement POST /api/v1/students/{id}/enrollments endpoint (`Src/Foundation/services/StudentManagement/API/EnrollmentsController.cs`)
- [ ] T028 [US3] Implement POST /api/v1/students/{id}/withdraw with status update + event (`Src/Foundation/services/StudentManagement/API/WithdrawStudentController.cs`)
- [ ] T029 [P] [US3] Publish StudentEnrolledEvent/StudentWithdrawnEvent with tenant metadata (`Src/Foundation/services/StudentManagement/Application/Enrollments/EnrollmentEventPublisher.cs`)
- [ ] T030 [US3] Integration tests for enrollment/withdraw flow including status updates (`tests/Foundation/StudentManagement/Integration/EnrollmentTests.cs`)

**Checkpoint**: Enrollment + withdrawal functional with events

---

## Phase 6: User Story 4 - Bulk Import & Export (Priority: P2)

**Goal**: Bulk CSV import and state export with idempotency and audit (scenarios 6, 12)

**Independent Test**: Import 500 students <2 minutes with summary report; export generates filtered CSV excluding confidential fields

### Implementation for User Story 4

- [ ] T031 [P] [US4] Implement CSV import endpoint with validation + batch idempotency token (`Src/Foundation/services/StudentManagement/API/ImportsController.cs`)
- [ ] T032 [US4] Add import parser + validator with row-level error collection (`Src/Foundation/services/StudentManagement/Application/Imports/ImportService.cs`)
- [ ] T033 [US4] Publish StudentCreatedEvent per imported student with throttling (`Src/Foundation/services/StudentManagement/Application/Imports/ImportEventPublisher.cs`)
- [ ] T034 [US4] Implement export endpoint streaming CSV with allowed fields + SAS token (`Src/Foundation/services/StudentManagement/API/ExportsController.cs`)
- [ ] T035 [US4] Add integration tests for import/export throughput + correctness (`tests/Foundation/StudentManagement/Integration/ImportExportTests.cs`)

**Checkpoint**: Import/export ready with audit + SLO validation

---

## Phase 7: User Story 5 - Dashboard Aggregation (Priority: P2)

**Goal**: Aggregate student dashboard data via orchestrated downstream calls (scenario 7)

**Independent Test**: Dashboard returns within <200ms, handles downstream timeouts with partial fallback and correlation IDs

### Implementation for User Story 5

- [ ] T036 [P] [US5] Implement dashboard orchestrator calling Assessment/Intervention/Section services in parallel (`Src/Foundation/services/StudentManagement/Application/Dashboard/DashboardOrchestrator.cs`)
- [ ] T037 [US5] Add circuit breaker + timeout policies with graceful partial responses (`Src/Foundation/services/StudentManagement/Infrastructure/Resilience/DashboardResiliencePolicy.cs`)
- [ ] T038 [US5] Add telemetry spans + metrics for dashboard fan-out latency (`Src/Foundation/services/StudentManagement/Telemetry/DashboardInstrumentation.cs`)
- [ ] T039 [US5] Integration tests for dashboard aggregator with mocked downstream services (`tests/Foundation/StudentManagement/Integration/DashboardAggregatorTests.cs`)

**Checkpoint**: Dashboard aggregation meets SLO with resilience

---

## Phase 8: User Story 6 - Privacy, Authorization, and Audit (Priority: P2)

**Goal**: Enforce educational interest, FERPA compliance, and audit (scenario 9)

**Independent Test**: Unauthorized access denied; audit log records all successful reads/mutations with tenant/user/action/entity

### Implementation for User Story 6

- [ ] T040 [US6] Implement authorization decorator using Enrollment + Section membership for reads (`Src/Foundation/services/StudentManagement/Application/Authorization/EducationalInterestDecorator.cs`)
- [ ] T041 [P] [US6] Add audit logging for successful reads (query handlers) to audit schema (`Src/Foundation/services/StudentManagement/Application/Audit/ReadAuditBehavior.cs`)
- [ ] T042 [US6] Add RBAC policy definitions + integration tests for access control (`tests/Foundation/StudentManagement/Integration/AuthorizationTests.cs`)

**Checkpoint**: Privacy and audit enforced

---

## Phase 9: User Story 7 - Merge & Photo Upload (Priority: P3)

**Goal**: Merge duplicates safely and handle photo upload workflow (scenarios 10, 11)

**Independent Test**: Merge reassigns relationships, soft-deletes secondary; photo upload stores blob per tenant with resized image

### Implementation for User Story 7

- [ ] T043 [P] [US7] Implement merge workflow (command + domain service) with MergeReferenceId handling (`Src/Foundation/services/StudentManagement/Application/Merge/StudentMergeHandler.cs`)
- [ ] T044 [US7] Add transactionally safe reassignment of enrollments during merge (`Src/Foundation/services/StudentManagement/Infrastructure/Repositories/EnrollmentRepository.cs`)
- [ ] T045 [P] [US7] Implement photo upload endpoint with virus scan placeholder + resize + blob storage path (`Src/Foundation/services/StudentManagement/API/PhotosController.cs`)
- [ ] T046 [US7] Add integration tests for merge + photo upload path (`tests/Foundation/StudentManagement/Integration/MergeAndPhotoTests.cs`)

**Checkpoint**: Merge + media workflows validated

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T047 [P] Add bundle/performance monitoring for create/search/dashboard endpoints (`Src/Foundation/services/StudentManagement/Telemetry/ServiceMetrics.cs`)
- [ ] T048 Harden error handling + retries on event publish failures (`Src/Foundation/services/StudentManagement/Application/Common/EventPublishBehavior.cs`)
- [ ] T049 [P] Documentation updates (runbook, event catalog, SLA) (`Src/Foundation/services/StudentManagement/docs/runbook.md`)
- [ ] T050 Final audit: ensure all spec scenarios + acceptance criteria covered (`Plan/Foundation/specs/005-student-management-service/tasks.md`)

---

## Dependencies & Execution Order

- Setup (Phase 1)  Foundational (Phase 2)  US1/US2/US3 (P1)  US4/US5/US6 (P2)  US7 (P3)  Polish
- US1 depends on domain + infra; US2/US3 rely on same foundation and can proceed after US1 scaffolding
- US4 import/export depends on CRUD + idempotency
- US5 dashboard depends on downstream contracts + events from US1/US3
- US6 privacy applies across all endpoints; complete before production rollout
- US7 depends on CRUD + enrollment data integrity

## Parallel Execution Examples

- T008-T012 can run in parallel (project setup + telemetry + tests)
- US1 tasks T018-T021 can parallelize implementation and event publishing; tests follow
- US4 tasks T031-T034 can run in parallel once import infrastructure ready
- US5 resilience/telemetry tasks T036-T038 can run alongside US4 once base endpoints exist

## Implementation Strategy

- MVP = Phases 1-3 to deliver core CRUD + events with tenant isolation
- Next, deliver search/enrollment + import/export and dashboard to meet functional scenarios
- Harden privacy/audit then merge/photo workflows; keep performance budgets with dedicated tests
