---
description: "Task list for Staff Management Service Migration (profiles, assignments, teams, certifications)"
---

# Tasks: Staff Management Service Migration

**Specification Branch**: `Foundation/008-staff-management-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/008-staff-management-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/008-staff-management-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/StaffManagement/`  
**Specification Path**: `Plan/Foundation/specs/008-staff-management-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/services/StaffManagement/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/008-staff-management-service/`)
- [ ] Shared dependencies (Identity client, MassTransit, multi-tenancy, Redis) align between plan and spec
- [ ] Cross-layer consumers (Section/Reporting) documented for events

---

## Layer Compliance Validation

- [ ] T001 Verify service references only Foundation shared libraries and infra (`Src/Foundation/services/StaffManagement/StaffManagement.csproj`)
- [ ] T002 Verify no direct references to higher-layer services; interactions via HTTP clients or events only
- [ ] T003 Ensure AppHost registers Staff service with proper isolation + identity client configuration (`Src/Foundation/AppHost/Program.cs`)
- [ ] T004 Update README with layer placement, endpoints, and event contracts (`Src/Foundation/services/StaffManagement/README.md`)

---

## Identity & Authentication Compliance

- [ ] T005 Configure Microsoft.Identity.Web JWT validation and tenant enforcement (`Src/Foundation/services/StaffManagement/Program.cs`)
- [ ] T006 Ensure SessionAuthenticationHandler + Redis session caching registered (`Src/Foundation/services/StaffManagement/Program.cs`)
- [ ] T007 Enforce tenant filters in DbContext (`Src/Foundation/services/StaffManagement/Infrastructure/StaffDbContext.cs`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold projects, database migrations, identity client, telemetry

- [ ] T008 Scaffold solution projects (API, Application, Domain, Infrastructure, Tests) under `Src/Foundation/services/StaffManagement/`
- [ ] T009 Create initial migrations for staff, assignments, teams, certifications, schedules (`Src/Foundation/services/StaffManagement/Infrastructure/Migrations/`)
- [ ] T010 [P] Configure MassTransit + Azure Service Bus for staff events (`Src/Foundation/services/StaffManagement/Program.cs`)
- [ ] T011 [P] Add IdentityService client for provisioning + status polling (`Src/Foundation/services/StaffManagement/Infrastructure/Identity/IdentityServiceClient.cs`)
- [ ] T012 [P] Add OpenTelemetry tracing/metrics with tenant attributes (`Src/Foundation/services/StaffManagement/Telemetry/TelemetryConfig.cs`)
- [ ] T013 [P] Setup test projects with seeded tenants (`tests/Foundation/StaffManagement/`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain model, repositories, authorization, audit, idempotency

- [ ] T014 Implement domain entities + events (StaffMember, StaffAssignment, Team, Certification, Schedule) in `Src/Foundation/services/StaffManagement/Domain/`
- [ ] T015 Implement repositories with tenant-aware query filters (`Src/Foundation/services/StaffManagement/Infrastructure/Repositories/`)
- [ ] T016 Implement idempotency cache for staff creation/import (email + hash) (`Src/Foundation/services/StaffManagement/Infrastructure/Idempotency/IdempotencyStore.cs`)
- [ ] T017 Add RBAC policies (ViewStaff, ManageAssignments, ManageTeams, ManageCertifications) in `Src/Foundation/services/StaffManagement/Infrastructure/Auth/Policies.cs`
- [ ] T018 Add audit interceptor logging before/after diffs (`Src/Foundation/services/StaffManagement/Infrastructure/Audit/AuditInterceptor.cs`)

**Checkpoint**: Core model + infra ready

---

## Phase 3: User Story 1 - Staff Creation & Profile Updates (Priority: P1) **MVP**

**Goal**: Create staff profiles, update details, publish events, and trigger identity provisioning (scenarios 1, 2)

**Independent Test**: POST staff persists with tenant filter, emits StaffCreatedEvent, identity provisioning initiated; profile update emits StaffProfileUpdatedEvent with audit

### Implementation for User Story 1

- [ ] T019 [P] [US1] Implement POST /api/v1/staff with validation, idempotency, and event publish (`Src/Foundation/services/StaffManagement/API/StaffController.cs`)
- [ ] T020 [US1] Implement PUT/PATCH staff profile updates with audit + event (`Src/Foundation/services/StaffManagement/Application/Staff/UpdateStaffProfileHandler.cs`)
- [ ] T021 [P] [US1] Integrate identity provisioning call + status polling (`Src/Foundation/services/StaffManagement/Application/Staff/IdentityProvisioningService.cs`)
- [ ] T022 [US1] Add integration tests for create/update/events/identity calls (`tests/Foundation/StaffManagement/Integration/StaffCrudTests.cs`)

**Checkpoint**: Staff create/update ready with identity integration

---

## Phase 4: User Story 2 - Assignments & RBAC Context (Priority: P1)

**Goal**: Manage multi-school assignments with FTE validation and RBAC enforcement (scenarios 4, 5)

**Independent Test**: Assignment creation enforces total FTE <=1.0; context switching uses selected assignment; RBAC policies enforced

- [ ] T023 [P] [US2] Implement POST /api/v1/staff/{id}/assignments with FTE validation (`Src/Foundation/services/StaffManagement/API/AssignmentsController.cs`)
- [ ] T024 [US2] Implement GET assignments + active context selection (`Src/Foundation/services/StaffManagement/Application/Assignments/GetAssignmentsHandler.cs`)
- [ ] T025 [P] [US2] Publish StaffAssignedToSchoolEvent with tenant metadata (`Src/Foundation/services/StaffManagement/Application/Assignments/AssignmentEventPublisher.cs`)
- [ ] T026 [US2] Add RBAC checks for assignment endpoints (ManageAssignments) (`Src/Foundation/services/StaffManagement/Infrastructure/Auth/Policies.cs`)
- [ ] T027 [US2] Add integration tests for FTE validation and RBAC enforcement (`tests/Foundation/StaffManagement/Integration/AssignmentsTests.cs`)

**Checkpoint**: Assignments + RBAC validated

---

## Phase 5: User Story 3 - Teams & Collaboration (Priority: P2)

**Goal**: Create teams and manage membership with notifications (scenario 3)

**Independent Test**: Team creation publishes TeamCreated; adding members publishes TeamMemberAdded; notifications sent to members

- [ ] T028 [P] [US3] Implement POST /api/v1/teams and GET queries (`Src/Foundation/services/StaffManagement/API/TeamsController.cs`)
- [ ] T029 [US3] Implement team membership management (add/remove) with events (`Src/Foundation/services/StaffManagement/Application/Teams/TeamMembershipService.cs`)
- [ ] T030 [US3] Hook notifications (email placeholder) for member additions (`Src/Foundation/services/StaffManagement/Application/Teams/TeamNotificationService.cs`)
- [ ] T031 [US3] Add integration tests for teams + membership (`tests/Foundation/StaffManagement/Integration/TeamsTests.cs`)

**Checkpoint**: Team flows operational

---

## Phase 6: User Story 4 - Search & Directory Privacy (Priority: P1)

**Goal**: Provide search and directory listing with privacy controls (scenarios 6, 11)

**Independent Test**: Search returns tenant-filtered staff with p95 <100ms; directory hides private contact info unless admin

- [ ] T032 [P] [US4] Implement search endpoint with filters/sorting and indexes (`Src/Foundation/services/StaffManagement/API/SearchController.cs`)
- [ ] T033 [US4] Add directory endpoint applying privacy preferences (`Src/Foundation/services/StaffManagement/API/DirectoryController.cs`)
- [ ] T034 [P] [US4] Add indexes per data model (TenantId, LastName, Role) via migration (`Src/Foundation/services/StaffManagement/Infrastructure/Migrations/AddSearchIndexes.cs`)
- [ ] T035 [US4] Add performance tests for search latency (`tests/Foundation/StaffManagement/Perf/SearchLatencyTests.cs`)

**Checkpoint**: Search + directory fulfilled

---

## Phase 7: User Story 5 - Schedules & Availability (Priority: P2)

**Goal**: Manage schedules/availability and detect conflicts (scenario 7)

**Independent Test**: Schedules stored per staff; conflict detection prevents overlapping slots; availability returned via API

- [ ] T036 [P] [US5] Implement schedule endpoints (GET/PUT) for availability rows (`Src/Foundation/services/StaffManagement/API/ScheduleController.cs`)
- [ ] T037 [US5] Implement conflict detection logic for overlapping times (`Src/Foundation/services/StaffManagement/Application/Schedule/ScheduleConflictDetector.cs`)
- [ ] T038 [US5] Add tests for schedule conflicts and availability retrieval (`tests/Foundation/StaffManagement/Integration/ScheduleTests.cs`)

**Checkpoint**: Schedule management ready

---

## Phase 8: User Story 6 - Certification Tracking (Priority: P2)

**Goal**: Track certifications and send expiring notifications (scenario 8)

**Independent Test**: Certifications stored; expiring within 60 days emit CertificationExpiringEvent; renewal updates status

- [ ] T039 [P] [US6] Implement certification endpoints (POST/GET) with validation (`Src/Foundation/services/StaffManagement/API/CertificationsController.cs`)
- [ ] T040 [US6] Implement scheduled job to detect expiring certifications and publish events (`Src/Foundation/services/StaffManagement/Jobs/CertificationMonitorJob.cs`)
- [ ] T041 [US6] Add tests for expiring detection and event emission (`tests/Foundation/StaffManagement/Integration/CertificationTests.cs`)

**Checkpoint**: Certification monitoring operational

---

## Phase 9: User Story 7 - Bulk Import (Priority: P2)

**Goal**: Import staff via CSV with validation and identity provisioning (scenario 10)

**Independent Test**: Import 50 staff <60s with summary; events emitted; identity provisioning queued; idempotency prevents duplicates

- [ ] T042 [P] [US7] Implement import endpoint with CSV parser + validation (`Src/Foundation/services/StaffManagement/API/ImportController.cs`)
- [ ] T043 [US7] Batch insert/import pipeline with per-row errors + summary (`Src/Foundation/services/StaffManagement/Application/Import/StaffImportService.cs`)
- [ ] T044 [P] [US7] Trigger StaffCreatedEvent per imported staff + identity provisioning queue (`Src/Foundation/services/StaffManagement/Application/Import/ImportEventPublisher.cs`)
- [ ] T045 [US7] Add integration tests for import throughput + idempotency (`tests/Foundation/StaffManagement/Integration/ImportTests.cs`)

**Checkpoint**: Import path validated

---

## Phase 10: User Story 8 - Audit Trail & Future Reviews (Priority: P3)

**Goal**: Ensure immutable audit logs and placeholder for performance review workflow (scenarios 9, 12)

**Independent Test**: Audit log records all writes with before/after; optional performance review endpoints stubbed with audit

- [ ] T046 [P] [US8] Persist audit records for all mutations (`Src/Foundation/services/StaffManagement/Infrastructure/Audit/AuditInterceptor.cs`)
- [ ] T047 [US8] Expose audit query/export endpoint with filters (`Src/Foundation/services/StaffManagement/API/AuditController.cs`)
- [ ] T048 [US8] Stub performance review endpoints with persisted history + audit (`Src/Foundation/services/StaffManagement/API/PerformanceReviewsController.cs`)
- [ ] T049 [US8] Tests for audit coverage and performance review stubs (`tests/Foundation/StaffManagement/Integration/AuditTests.cs`)

**Checkpoint**: Audit + future review scaffolding ready

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T050 [P] Add metrics/dashboards for create/search/import/certification jobs (`Src/Foundation/services/StaffManagement/Telemetry/StaffMetrics.cs`)
- [ ] T051 Harden event publishing retries + dead-letter handling (`Src/Foundation/services/StaffManagement/Application/Common/EventPublishBehavior.cs`)
- [ ] T052 [P] Documentation: runbook, event catalog, FTE rules (`Src/Foundation/services/StaffManagement/docs/runbook.md`)
- [ ] T053 Final audit against spec scenarios and acceptance criteria (`Plan/Foundation/specs/008-staff-management-service/tasks.md`)

---

## Dependencies & Execution Order

- Setup (Phase 1)  Foundational (Phase 2)  US1/US2/US4 (P1)  US3/US5/US6/US7 (P2)  US8 (P3)  Polish
- US1 depends on domain/idempotency/identity client
- US2 depends on domain + RBAC policies
- US3 depends on base staff entities/events
- US4 depends on search indexes
- US5 depends on staff records existing
- US6 depends on certifications + scheduling infra
- US7 depends on events/idempotency
- US8 depends on audit infrastructure

## Parallel Execution Examples

- T008-T013 parallel (projects, migrations, bus, identity client, telemetry, tests)
- US1 tasks T019-T021 parallelizable; T022 follows
- US2 tasks T023-T025 parallel; T026-T027 follow
- US7 tasks T042-T044 parallel after import scaffolding

## Implementation Strategy

- MVP = Phases 1-4 delivering staff create/update + assignments + search/directory with events and RBAC
- Add teams, schedules, certifications, import, then audit/performance review stubs
- Keep FTE validation and security central; monitor create/search/import metrics continuously
