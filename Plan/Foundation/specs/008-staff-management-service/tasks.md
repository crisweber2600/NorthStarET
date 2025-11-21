---
description: "Task breakdown for Staff Management Service migration"
---

# Tasks: Staff Management Service Migration

**Specification Branch**: `Foundation/008-staff-management-service-spec` *(planning artifacts)*  
**Implementation Branch**: `Foundation/008-staff-management-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/008-staff-management-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Staff/`  
**Specification Path**: `Plan/Foundation/specs/008-staff-management-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification (Foundation)
- [ ] Target Layer matches plan.md Layer Identification (Foundation)
- [ ] Implementation path follows layer structure (`Src/Foundation/services/Staff/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/008-staff-management-service/`)
- [ ] Shared infrastructure dependencies match between spec and plan (ServiceDefaults, Domain, Application, Infrastructure)
- [ ] Cross-layer dependencies (if any) justified in both spec and plan (Identity for provisioning, Configuration for school validation)

---

## Layer Compliance Validation

*MANDATORY: Include these validation tasks to ensure mono-repo layer isolation (Constitution Principle 6)*

- [ ] T001 Verify project references ONLY shared infrastructure from approved layers (`Src/Foundation/shared/*`)
- [ ] T002 Verify NO direct service-to-service references across layers (must use events/contracts for cross-layer communication)
- [ ] T003 Verify AppHost orchestration includes this service with correct layer isolation
- [ ] T004 Verify README.md documents layer position and shared infrastructure dependencies
- [ ] T005 Verify no circular dependencies between layers (Foundation cannot depend on higher layers)

---

## Identity & Authentication Compliance

*MANDATORY: Include if this feature requires authentication/authorization*

- [ ] T006 Verify NO references to Duende IdentityServer or custom token issuance
- [ ] T007 Verify Microsoft.Identity.Web used for JWT token validation (NOT custom JWT generation)
- [ ] T008 Verify SessionAuthenticationHandler registered for session-based API authorization
- [ ] T009 Verify Redis configured for session caching (Aspire.Hosting.Redis)
- [ ] T010 Verify TokenExchangeService implements BFF pattern (Entra tokens → LMS sessions)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T011 Create Staff service project structure in `Src/Foundation/services/Staff/`
- [ ] T012 [P] Create Staff.Domain project with entities in `Src/Foundation/services/Staff/Staff.Domain/`
- [ ] T013 [P] Create Staff.Application project with CQRS patterns in `Src/Foundation/services/Staff/Staff.Application/`
- [ ] T014 [P] Create Staff.Infrastructure project with EF Core in `Src/Foundation/services/Staff/Staff.Infrastructure/`
- [ ] T015 [P] Create Staff.Api project with REST endpoints in `Src/Foundation/services/Staff/Staff.Api/`
- [ ] T016 [P] Create Staff.Messaging project for events/consumers in `Src/Foundation/services/Staff/Staff.Messaging/`
- [ ] T017 [P] Create Staff.Import project for bulk import pipeline in `Src/Foundation/services/Staff/Staff.Import/`
- [ ] T018 [P] Create test projects structure in `tests/staff-service/` (unit, integration, contract, performance)
- [ ] T019 Configure NuGet package dependencies in `Directory.Packages.props` for Staff projects
- [ ] T020 [P] Configure linting and code analysis rules in `.editorconfig`
- [ ] T021 [P] Add Staff service to Aspire AppHost in `Src/Foundation/AppHost/Program.cs`
- [ ] T022 Add ServiceDefaults reference to Staff.Api project for hosting patterns
- [ ] T023 [P] Create README.md in `Src/Foundation/services/Staff/` documenting layer position and dependencies

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T024 Create StaffDbContext with tenant isolation in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/StaffDbContext.cs`
- [ ] T025 Configure PostgreSQL with RLS policies for tenant_id in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/Migrations/`
- [ ] T026 [P] Implement TenantInterceptor for multi-tenancy in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/Interceptors/TenantInterceptor.cs`
- [ ] T027 [P] Implement AuditInterceptor for audit records in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/Interceptors/AuditInterceptor.cs`
- [ ] T028 Create AuditRecord entity and table in `Src/Foundation/services/Staff/Staff.Domain/Entities/AuditRecord.cs`
- [ ] T029 Configure MassTransit with Azure Service Bus in `Src/Foundation/services/Staff/Staff.Infrastructure/Messaging/MassTransitConfiguration.cs`
- [ ] T030 [P] Setup Redis for session caching and idempotency in `Src/Foundation/services/Staff/Staff.Infrastructure/Caching/RedisCacheConfiguration.cs`
- [ ] T031 [P] Implement ITenantContext service in `Src/Foundation/services/Staff/Staff.Application/Common/Interfaces/ITenantContext.cs`
- [ ] T032 Implement base Result<T> type for operation results in `Src/Foundation/services/Staff/Staff.Application/Common/Models/Result.cs`
- [ ] T033 [P] Setup FluentValidation pipeline with MediatR in `Src/Foundation/services/Staff/Staff.Application/Common/Behaviors/ValidationBehavior.cs`
- [ ] T034 [P] Implement authorization behavior for role-based access control in `Src/Foundation/services/Staff/Staff.Application/Common/Behaviors/AuthorizationBehavior.cs`
- [ ] T035 Configure error handling middleware in `Src/Foundation/services/Staff/Staff.Api/Middleware/ExceptionHandlerMiddleware.cs`
- [ ] T036 [P] Configure API versioning and OpenAPI documentation in `Src/Foundation/services/Staff/Staff.Api/Program.cs`
- [ ] T037 [P] Setup health checks for database, messaging, and cache in `Src/Foundation/services/Staff/Staff.Api/Program.cs`
- [ ] T038 Configure CORS and rate limiting in `Src/Foundation/services/Staff/Staff.Api/Program.cs`
- [ ] T039 Implement IdempotencyService for duplicate prevention in `Src/Foundation/services/Staff/Staff.Infrastructure/Services/IdempotencyService.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Create and maintain staff profiles with identity linkage (Priority: P1) 🎯 MVP

**Goal**: District/school admins can create staff profiles, link to identity accounts, and manage role/permission metadata with full audit trail

**Independent Test**: Create a staff member, verify identity provisioning event, update profile fields, and confirm audit history and search visibility

### Domain & Data Layer for User Story 1

- [ ] T040 [P] [US1] Create StaffProfile entity in `Src/Foundation/services/Staff/Staff.Domain/Entities/StaffProfile.cs`
- [ ] T041 [P] [US1] Create StaffStatus enum in `Src/Foundation/services/Staff/Staff.Domain/Enums/StaffStatus.cs`
- [ ] T042 [P] [US1] Create StaffRole enum in `Src/Foundation/services/Staff/Staff.Domain/Enums/StaffRole.cs`
- [ ] T043 [US1] Add StaffProfile DbSet and configuration to StaffDbContext in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/StaffDbContext.cs`
- [ ] T044 [US1] Create initial migration for StaffProfile table in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/Migrations/`
- [ ] T045 [P] [US1] Add search indexes on (tenant_id, last_name, first_name, email) in migration
- [ ] T046 [US1] Implement IStaffRepository interface in `Src/Foundation/services/Staff/Staff.Application/Common/Interfaces/IStaffRepository.cs`
- [ ] T047 [US1] Implement StaffRepository with EF Core in `Src/Foundation/services/Staff/Staff.Infrastructure/Repositories/StaffRepository.cs`

### Commands & Queries for User Story 1

- [ ] T048 [P] [US1] Create CreateStaffCommand in `Src/Foundation/services/Staff/Staff.Application/Staff/Commands/CreateStaff/CreateStaffCommand.cs`
- [ ] T049 [P] [US1] Create CreateStaffCommandValidator in `Src/Foundation/services/Staff/Staff.Application/Staff/Commands/CreateStaff/CreateStaffCommandValidator.cs`
- [ ] T050 [US1] Implement CreateStaffCommandHandler with identity provisioning event in `Src/Foundation/services/Staff/Staff.Application/Staff/Commands/CreateStaff/CreateStaffCommandHandler.cs`
- [ ] T051 [P] [US1] Create UpdateStaffCommand in `Src/Foundation/services/Staff/Staff.Application/Staff/Commands/UpdateStaff/UpdateStaffCommand.cs`
- [ ] T052 [P] [US1] Create UpdateStaffCommandValidator in `Src/Foundation/services/Staff/Staff.Application/Staff/Commands/UpdateStaff/UpdateStaffCommandValidator.cs`
- [ ] T053 [US1] Implement UpdateStaffCommandHandler with audit and update events in `Src/Foundation/services/Staff/Staff.Application/Staff/Commands/UpdateStaff/UpdateStaffCommandHandler.cs`
- [ ] T054 [P] [US1] Create ArchiveStaffCommand in `Src/Foundation/services/Staff/Staff.Application/Staff/Commands/ArchiveStaff/ArchiveStaffCommand.cs`
- [ ] T055 [US1] Implement ArchiveStaffCommandHandler with soft delete in `Src/Foundation/services/Staff/Staff.Application/Staff/Commands/ArchiveStaff/ArchiveStaffCommandHandler.cs`
- [ ] T056 [P] [US1] Create GetStaffByIdQuery in `Src/Foundation/services/Staff/Staff.Application/Staff/Queries/GetStaffById/GetStaffByIdQuery.cs`
- [ ] T057 [US1] Implement GetStaffByIdQueryHandler in `Src/Foundation/services/Staff/Staff.Application/Staff/Queries/GetStaffById/GetStaffByIdQueryHandler.cs`
- [ ] T058 [P] [US1] Create SearchStaffQuery with tenant and privacy filters in `Src/Foundation/services/Staff/Staff.Application/Staff/Queries/SearchStaff/SearchStaffQuery.cs`
- [ ] T059 [US1] Implement SearchStaffQueryHandler with performance optimization in `Src/Foundation/services/Staff/Staff.Application/Staff/Queries/SearchStaff/SearchStaffQueryHandler.cs`

### Events for User Story 1

- [ ] T060 [P] [US1] Create StaffCreated event in `Src/Foundation/services/Staff/Staff.Domain/Events/StaffCreated.cs`
- [ ] T061 [P] [US1] Create StaffUpdated event in `Src/Foundation/services/Staff/Staff.Domain/Events/StaffUpdated.cs`
- [ ] T062 [P] [US1] Create IdentityProvisioningRequested event in `Src/Foundation/services/Staff/Staff.Domain/Events/IdentityProvisioningRequested.cs`
- [ ] T063 [US1] Configure MassTransit publishers for staff events in `Src/Foundation/services/Staff/Staff.Messaging/Publishers/StaffEventPublisher.cs`

### API Endpoints for User Story 1

- [ ] T064 [P] [US1] Create StaffController in `Src/Foundation/services/Staff/Staff.Api/Controllers/StaffController.cs`
- [ ] T065 [US1] Implement POST /api/staff endpoint for staff creation in StaffController
- [ ] T066 [US1] Implement PUT /api/staff/{id} endpoint for profile updates in StaffController
- [ ] T067 [US1] Implement GET /api/staff/{id} endpoint for profile retrieval in StaffController
- [ ] T068 [US1] Implement GET /api/staff endpoint for search/directory in StaffController
- [ ] T069 [P] [US1] Add request/response DTOs in `Src/Foundation/services/Staff/Staff.Api/Models/` for staff endpoints
- [ ] T070 [US1] Add API authorization attributes requiring appropriate roles for staff endpoints

### Testing for User Story 1

- [ ] T071 [P] [US1] Create unit tests for CreateStaffCommandHandler in `tests/staff-service/unit/Commands/CreateStaffCommandHandlerTests.cs`
- [ ] T072 [P] [US1] Create unit tests for UpdateStaffCommandHandler in `tests/staff-service/unit/Commands/UpdateStaffCommandHandlerTests.cs`
- [ ] T073 [P] [US1] Create unit tests for SearchStaffQueryHandler in `tests/staff-service/unit/Queries/SearchStaffQueryHandlerTests.cs`
- [ ] T074 [P] [US1] Create integration tests for staff CRUD operations in `tests/staff-service/integration/StaffCrudIntegrationTests.cs`
- [ ] T075 [P] [US1] Create contract tests for StaffCreated event in `tests/staff-service/contract/StaffCreatedContractTests.cs`
- [ ] T076 [P] [US1] Create Reqnroll feature file for staff profile management in `Plan/Foundation/specs/008-staff-management-service/features/staff-profile-management.feature`
- [ ] T077 [US1] Implement step definitions for staff profile BDD tests in `tests/staff-service/bdd/StaffProfileSteps.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Manage multi-school assignments and schedules (Priority: P2)

**Goal**: Administrators assign staff to multiple schools with FTE percentages, schedules, and availability while preventing conflicts

**Independent Test**: Assign a staff member to two schools, set FTE totals, define availability blocks, and verify conflict detection and context switching

### Domain & Data Layer for User Story 2

- [ ] T078 [P] [US2] Create StaffAssignment entity in `Src/Foundation/services/Staff/Staff.Domain/Entities/StaffAssignment.cs`
- [ ] T079 [P] [US2] Create AvailabilityBlock entity in `Src/Foundation/services/Staff/Staff.Domain/Entities/AvailabilityBlock.cs`
- [ ] T080 [P] [US2] Create AssignmentStatus enum in `Src/Foundation/services/Staff/Staff.Domain/Enums/AssignmentStatus.cs`
- [ ] T081 [US2] Add StaffAssignment and AvailabilityBlock DbSets to StaffDbContext in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/StaffDbContext.cs`
- [ ] T082 [US2] Create migration for StaffAssignment and AvailabilityBlock tables in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/Migrations/`
- [ ] T083 [P] [US2] Add check constraint for FTE validation (>0, cumulative <=1.0) in migration
- [ ] T084 [P] [US2] Add unique constraint for availability overlap prevention in migration
- [ ] T085 [US2] Implement IAssignmentRepository interface in `Src/Foundation/services/Staff/Staff.Application/Common/Interfaces/IAssignmentRepository.cs`
- [ ] T086 [US2] Implement AssignmentRepository with EF Core in `Src/Foundation/services/Staff/Staff.Infrastructure/Repositories/AssignmentRepository.cs`

### Commands & Queries for User Story 2

- [ ] T087 [P] [US2] Create CreateAssignmentCommand in `Src/Foundation/services/Staff/Staff.Application/Assignments/Commands/CreateAssignment/CreateAssignmentCommand.cs`
- [ ] T088 [P] [US2] Create CreateAssignmentCommandValidator with FTE validation in `Src/Foundation/services/Staff/Staff.Application/Assignments/Commands/CreateAssignment/CreateAssignmentCommandValidator.cs`
- [ ] T089 [US2] Implement CreateAssignmentCommandHandler with cumulative FTE check in `Src/Foundation/services/Staff/Staff.Application/Assignments/Commands/CreateAssignment/CreateAssignmentCommandHandler.cs`
- [ ] T090 [P] [US2] Create UpdateAssignmentCommand in `Src/Foundation/services/Staff/Staff.Application/Assignments/Commands/UpdateAssignment/UpdateAssignmentCommand.cs`
- [ ] T091 [US2] Implement UpdateAssignmentCommandHandler in `Src/Foundation/services/Staff/Staff.Application/Assignments/Commands/UpdateAssignment/UpdateAssignmentCommandHandler.cs`
- [ ] T092 [P] [US2] Create ManageAvailabilityCommand in `Src/Foundation/services/Staff/Staff.Application/Availability/Commands/ManageAvailability/ManageAvailabilityCommand.cs`
- [ ] T093 [P] [US2] Create ManageAvailabilityCommandValidator with overlap detection in `Src/Foundation/services/Staff/Staff.Application/Availability/Commands/ManageAvailability/ManageAvailabilityCommandValidator.cs`
- [ ] T094 [US2] Implement ManageAvailabilityCommandHandler in `Src/Foundation/services/Staff/Staff.Application/Availability/Commands/ManageAvailability/ManageAvailabilityCommandHandler.cs`
- [ ] T095 [P] [US2] Create GetStaffAssignmentsQuery in `Src/Foundation/services/Staff/Staff.Application/Assignments/Queries/GetStaffAssignments/GetStaffAssignmentsQuery.cs`
- [ ] T096 [US2] Implement GetStaffAssignmentsQueryHandler in `Src/Foundation/services/Staff/Staff.Application/Assignments/Queries/GetStaffAssignments/GetStaffAssignmentsQueryHandler.cs`
- [ ] T097 [P] [US2] Create GetStaffAvailabilityQuery in `Src/Foundation/services/Staff/Staff.Application/Availability/Queries/GetStaffAvailability/GetStaffAvailabilityQuery.cs`
- [ ] T098 [US2] Implement GetStaffAvailabilityQueryHandler in `Src/Foundation/services/Staff/Staff.Application/Availability/Queries/GetStaffAvailability/GetStaffAvailabilityQueryHandler.cs`

### Events for User Story 2

- [ ] T099 [P] [US2] Create StaffAssignmentChanged event in `Src/Foundation/services/Staff/Staff.Domain/Events/StaffAssignmentChanged.cs`
- [ ] T100 [P] [US2] Create StaffAvailabilityChanged event in `Src/Foundation/services/Staff/Staff.Domain/Events/StaffAvailabilityChanged.cs`
- [ ] T101 [US2] Configure MassTransit publishers for assignment events in `Src/Foundation/services/Staff/Staff.Messaging/Publishers/AssignmentEventPublisher.cs`

### API Endpoints for User Story 2

- [ ] T102 [P] [US2] Create AssignmentsController in `Src/Foundation/services/Staff/Staff.Api/Controllers/AssignmentsController.cs`
- [ ] T103 [US2] Implement POST /api/staff/{id}/assignments endpoint for creating assignments in AssignmentsController
- [ ] T104 [US2] Implement PUT /api/staff/{id}/assignments/{assignmentId} endpoint for updating assignments in AssignmentsController
- [ ] T105 [US2] Implement GET /api/staff/{id}/assignments endpoint for retrieving assignments in AssignmentsController
- [ ] T106 [P] [US2] Create AvailabilityController in `Src/Foundation/services/Staff/Staff.Api/Controllers/AvailabilityController.cs`
- [ ] T107 [US2] Implement POST /api/staff/{id}/availability endpoint for managing availability in AvailabilityController
- [ ] T108 [US2] Implement GET /api/staff/{id}/availability endpoint for retrieving availability in AvailabilityController
- [ ] T109 [P] [US2] Add request/response DTOs for assignment and availability endpoints in `Src/Foundation/services/Staff/Staff.Api/Models/`
- [ ] T110 [US2] Add authorization for multi-school context switching

### Testing for User Story 2

- [ ] T111 [P] [US2] Create unit tests for FTE validation logic in `tests/staff-service/unit/Validators/FteValidationTests.cs`
- [ ] T112 [P] [US2] Create unit tests for availability overlap detection in `tests/staff-service/unit/Validators/AvailabilityOverlapTests.cs`
- [ ] T113 [P] [US2] Create integration tests for assignment CRUD with FTE validation in `tests/staff-service/integration/AssignmentIntegrationTests.cs`
- [ ] T114 [P] [US2] Create integration tests for availability conflict detection in `tests/staff-service/integration/AvailabilityIntegrationTests.cs`
- [ ] T115 [P] [US2] Create contract tests for StaffAssignmentChanged event in `tests/staff-service/contract/AssignmentChangedContractTests.cs`
- [ ] T116 [P] [US2] Create Reqnroll feature file for multi-school assignments in `Plan/Foundation/specs/008-staff-management-service/features/multi-school-assignments.feature`
- [ ] T117 [US2] Implement step definitions for assignments BDD tests in `tests/staff-service/bdd/AssignmentSteps.cs`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Track certifications, teams, and bulk import (Priority: P3)

**Goal**: Admins manage certifications with expiry alerts, organize teams/PLCs, and import staff data in bulk with validation and audit

**Independent Test**: Create a certification with expiry, enroll staff to a team, run a CSV import for multiple staff, and verify alerts, team membership, and audit coverage

### Domain & Data Layer for User Story 3

- [ ] T118 [P] [US3] Create Certification entity in `Src/Foundation/services/Staff/Staff.Domain/Entities/Certification.cs`
- [ ] T119 [P] [US3] Create Team entity in `Src/Foundation/services/Staff/Staff.Domain/Entities/Team.cs`
- [ ] T120 [P] [US3] Create TeamMembership entity in `Src/Foundation/services/Staff/Staff.Domain/Entities/TeamMembership.cs`
- [ ] T121 [P] [US3] Create ImportJob entity in `Src/Foundation/services/Staff/Staff.Domain/Entities/ImportJob.cs`
- [ ] T122 [P] [US3] Create CertificationStatus enum in `Src/Foundation/services/Staff/Staff.Domain/Enums/CertificationStatus.cs`
- [ ] T123 [P] [US3] Create TeamScope enum in `Src/Foundation/services/Staff/Staff.Domain/Enums/TeamScope.cs`
- [ ] T124 [US3] Add Certification, Team, TeamMembership, ImportJob DbSets to StaffDbContext in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/StaffDbContext.cs`
- [ ] T125 [US3] Create migration for certification and team tables in `Src/Foundation/services/Staff/Staff.Infrastructure/Data/Migrations/`
- [ ] T126 [US3] Implement ICertificationRepository interface in `Src/Foundation/services/Staff/Staff.Application/Common/Interfaces/ICertificationRepository.cs`
- [ ] T127 [US3] Implement CertificationRepository with EF Core in `Src/Foundation/services/Staff/Staff.Infrastructure/Repositories/CertificationRepository.cs`
- [ ] T128 [US3] Implement ITeamRepository interface in `Src/Foundation/services/Staff/Staff.Application/Common/Interfaces/ITeamRepository.cs`
- [ ] T129 [US3] Implement TeamRepository with EF Core in `Src/Foundation/services/Staff/Staff.Infrastructure/Repositories/TeamRepository.cs`

### Commands & Queries for Certifications (User Story 3)

- [ ] T130 [P] [US3] Create CreateCertificationCommand in `Src/Foundation/services/Staff/Staff.Application/Certifications/Commands/CreateCertification/CreateCertificationCommand.cs`
- [ ] T131 [P] [US3] Create CreateCertificationCommandValidator in `Src/Foundation/services/Staff/Staff.Application/Certifications/Commands/CreateCertification/CreateCertificationCommandValidator.cs`
- [ ] T132 [US3] Implement CreateCertificationCommandHandler in `Src/Foundation/services/Staff/Staff.Application/Certifications/Commands/CreateCertification/CreateCertificationCommandHandler.cs`
- [ ] T133 [P] [US3] Create RenewCertificationCommand in `Src/Foundation/services/Staff/Staff.Application/Certifications/Commands/RenewCertification/RenewCertificationCommand.cs`
- [ ] T134 [US3] Implement RenewCertificationCommandHandler in `Src/Foundation/services/Staff/Staff.Application/Certifications/Commands/RenewCertification/RenewCertificationCommandHandler.cs`
- [ ] T135 [P] [US3] Create GetExpiringCertificationsQuery in `Src/Foundation/services/Staff/Staff.Application/Certifications/Queries/GetExpiringCertifications/GetExpiringCertificationsQuery.cs`
- [ ] T136 [US3] Implement GetExpiringCertificationsQueryHandler in `Src/Foundation/services/Staff/Staff.Application/Certifications/Queries/GetExpiringCertifications/GetExpiringCertificationsQueryHandler.cs`

### Commands & Queries for Teams (User Story 3)

- [ ] T137 [P] [US3] Create CreateTeamCommand in `Src/Foundation/services/Staff/Staff.Application/Teams/Commands/CreateTeam/CreateTeamCommand.cs`
- [ ] T138 [US3] Implement CreateTeamCommandHandler in `Src/Foundation/services/Staff/Staff.Application/Teams/Commands/CreateTeam/CreateTeamCommandHandler.cs`
- [ ] T139 [P] [US3] Create ManageTeamMembershipCommand in `Src/Foundation/services/Staff/Staff.Application/Teams/Commands/ManageTeamMembership/ManageTeamMembershipCommand.cs`
- [ ] T140 [US3] Implement ManageTeamMembershipCommandHandler in `Src/Foundation/services/Staff/Staff.Application/Teams/Commands/ManageTeamMembership/ManageTeamMembershipCommandHandler.cs`
- [ ] T141 [P] [US3] Create GetTeamRosterQuery in `Src/Foundation/services/Staff/Staff.Application/Teams/Queries/GetTeamRoster/GetTeamRosterQuery.cs`
- [ ] T142 [US3] Implement GetTeamRosterQueryHandler in `Src/Foundation/services/Staff/Staff.Application/Teams/Queries/GetTeamRoster/GetTeamRosterQueryHandler.cs`

### Bulk Import Pipeline (User Story 3)

- [ ] T143 [US3] Create IStaffImportService interface in `Src/Foundation/services/Staff/Staff.Application/Common/Interfaces/IStaffImportService.cs`
- [ ] T144 [US3] Implement StaffImportService with CSV parsing in `Src/Foundation/services/Staff/Staff.Import/Services/StaffImportService.cs`
- [ ] T145 [US3] Create import validation rules in `Src/Foundation/services/Staff/Staff.Import/Validators/StaffImportValidator.cs`
- [ ] T146 [US3] Implement duplicate detection logic in import service
- [ ] T147 [US3] Implement error reporting with blob storage in `Src/Foundation/services/Staff/Staff.Import/Services/ImportErrorReporter.cs`
- [ ] T148 [P] [US3] Create ImportStaffCommand in `Src/Foundation/services/Staff/Staff.Application/Import/Commands/ImportStaff/ImportStaffCommand.cs`
- [ ] T149 [US3] Implement ImportStaffCommandHandler with batch processing in `Src/Foundation/services/Staff/Staff.Application/Import/Commands/ImportStaff/ImportStaffCommandHandler.cs`
- [ ] T150 [P] [US3] Create GetImportJobStatusQuery in `Src/Foundation/services/Staff/Staff.Application/Import/Queries/GetImportJobStatus/GetImportJobStatusQuery.cs`
- [ ] T151 [US3] Implement GetImportJobStatusQueryHandler in `Src/Foundation/services/Staff/Staff.Application/Import/Queries/GetImportJobStatus/GetImportJobStatusQueryHandler.cs`

### Background Workers (User Story 3)

- [ ] T152 [US3] Create CertificationReminderWorker background service in `Src/Foundation/services/Staff/Staff.Api/Workers/CertificationReminderWorker.cs`
- [ ] T153 [US3] Implement reminder scheduling logic with configurable window (60 days default)
- [ ] T154 [US3] Configure worker registration in Staff.Api Program.cs

### Events for User Story 3

- [ ] T155 [P] [US3] Create CertificationExpiring event in `Src/Foundation/services/Staff/Staff.Domain/Events/CertificationExpiring.cs`
- [ ] T156 [P] [US3] Create TeamMembershipChanged event in `Src/Foundation/services/Staff/Staff.Domain/Events/TeamMembershipChanged.cs`
- [ ] T157 [P] [US3] Create StaffImported event in `Src/Foundation/services/Staff/Staff.Domain/Events/StaffImported.cs`
- [ ] T158 [US3] Configure MassTransit publishers for certification and team events in `Src/Foundation/services/Staff/Staff.Messaging/Publishers/CertificationEventPublisher.cs`

### API Endpoints for User Story 3

- [ ] T159 [P] [US3] Create CertificationsController in `Src/Foundation/services/Staff/Staff.Api/Controllers/CertificationsController.cs`
- [ ] T160 [US3] Implement POST /api/staff/{id}/certifications endpoint in CertificationsController
- [ ] T161 [US3] Implement PUT /api/staff/{id}/certifications/{certId} endpoint for renewal in CertificationsController
- [ ] T162 [US3] Implement GET /api/certifications/expiring endpoint in CertificationsController
- [ ] T163 [P] [US3] Create TeamsController in `Src/Foundation/services/Staff/Staff.Api/Controllers/TeamsController.cs`
- [ ] T164 [US3] Implement POST /api/teams endpoint for team creation in TeamsController
- [ ] T165 [US3] Implement POST /api/staff/{id}/teams/{teamId} endpoint for membership in TeamsController
- [ ] T166 [US3] Implement GET /api/teams/{id}/roster endpoint in TeamsController
- [ ] T167 [P] [US3] Create ImportController in `Src/Foundation/services/Staff/Staff.Api/Controllers/ImportController.cs`
- [ ] T168 [US3] Implement POST /api/staff/import endpoint with file upload in ImportController
- [ ] T169 [US3] Implement GET /api/staff/import/{jobId} endpoint for status in ImportController
- [ ] T170 [P] [US3] Add request/response DTOs for certification, team, and import endpoints in `Src/Foundation/services/Staff/Staff.Api/Models/`

### Testing for User Story 3

- [ ] T171 [P] [US3] Create unit tests for certification expiry logic in `tests/staff-service/unit/Certifications/ExpiryLogicTests.cs`
- [ ] T172 [P] [US3] Create unit tests for import validation in `tests/staff-service/unit/Import/ImportValidationTests.cs`
- [ ] T173 [P] [US3] Create integration tests for certification workflow in `tests/staff-service/integration/CertificationIntegrationTests.cs`
- [ ] T174 [P] [US3] Create integration tests for team management in `tests/staff-service/integration/TeamIntegrationTests.cs`
- [ ] T175 [P] [US3] Create performance tests for bulk import (50 staff <60s target) in `tests/staff-service/performance/ImportPerformanceTests.cs`
- [ ] T176 [P] [US3] Create contract tests for CertificationExpiring event in `tests/staff-service/contract/CertificationExpiringContractTests.cs`
- [ ] T177 [P] [US3] Create Reqnroll feature file for certifications in `Plan/Foundation/specs/008-staff-management-service/features/certification-tracking.feature`
- [ ] T178 [P] [US3] Create Reqnroll feature file for teams in `Plan/Foundation/specs/008-staff-management-service/features/team-management.feature`
- [ ] T179 [P] [US3] Create Reqnroll feature file for bulk import in `Plan/Foundation/specs/008-staff-management-service/features/bulk-import.feature`
- [ ] T180 [US3] Implement step definitions for certification BDD tests in `tests/staff-service/bdd/CertificationSteps.cs`
- [ ] T181 [US3] Implement step definitions for team BDD tests in `tests/staff-service/bdd/TeamSteps.cs`
- [ ] T182 [US3] Implement step definitions for import BDD tests in `tests/staff-service/bdd/ImportSteps.cs`

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T183 [P] Optimize search query performance for <100ms p95 target in `Src/Foundation/services/Staff/Staff.Infrastructure/Repositories/StaffRepository.cs`
- [ ] T184 [P] Add Redis caching layer for directory search results in `Src/Foundation/services/Staff/Staff.Infrastructure/Caching/StaffDirectoryCacheService.cs`
- [ ] T185 [P] Implement privacy filter logic for directory visibility in `Src/Foundation/services/Staff/Staff.Application/Staff/Queries/SearchStaff/SearchStaffQueryHandler.cs`
- [ ] T186 [US3] Create quickstart validation script per quickstart.md in `Src/Foundation/services/Staff/scripts/validate-quickstart.sh`
- [ ] T187 [P] Add comprehensive API documentation with examples in `Src/Foundation/services/Staff/Staff.Api/Program.cs`
- [ ] T188 [P] Add monitoring dashboards for search performance and event publishing in `Src/Foundation/services/Staff/Staff.Api/`
- [ ] T189 [P] Add telemetry for certification reminder worker success/failure rates
- [ ] T190 [P] Security review: verify all endpoints require authorization
- [ ] T191 [P] Security review: verify audit records for all mutations
- [ ] T192 [P] Security review: verify tenant isolation in all queries
- [ ] T193 [P] Code cleanup: remove unused imports and refactor duplicated validation logic
- [ ] T194 [P] Performance optimization: add database indexes based on query patterns
- [ ] T195 [P] Add integration test for event publishing latency (<30s target)
- [ ] T196 [P] Add resilience tests for transient failures (circuit breaker, retry)
- [ ] T197 Update README.md with architecture diagrams and component interactions in `Src/Foundation/services/Staff/README.md`
- [ ] T198 [P] Create ADR (Architecture Decision Record) for import pipeline design in `Src/Foundation/services/Staff/docs/adr/`
- [ ] T199 Verify all success criteria are testable and implement missing metrics
- [ ] T200 Run full test suite (unit + integration + BDD + performance) and capture results

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational phase completion - Core MVP story
- **User Story 2 (Phase 4)**: Depends on Foundational phase completion - Builds on US1 staff profiles but independently testable
- **User Story 3 (Phase 5)**: Depends on Foundational phase completion - Builds on US1 profiles and US2 assignments but independently testable
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories ✓ Core MVP
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - References StaffProfile from US1 but independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - References StaffProfile from US1 but independently testable

### Within Each User Story

- Domain entities before repositories
- Repositories before command/query handlers
- Commands/queries before API endpoints
- Events defined before publishers
- Validators before handlers
- Tests can be written in parallel with implementation (or before for TDD)
- Story complete before moving to next priority

### Parallel Opportunities

**Phase 1 (Setup)**: Tasks T012-T018 (all project creation), T020-T021 (config files)

**Phase 2 (Foundational)**: Tasks T026-T027 (interceptors), T030-T031 (cache and tenant context), T033-T034 (behaviors), T036-T038 (API config)

**Phase 3 (User Story 1)**:
- Domain: T040-T042 (entities and enums)
- Commands: T048-T049 (create), T051-T052 (update), T054 (archive)
- Queries: T056, T058 (query objects)
- Events: T060-T062 (event definitions)
- API: T069 (DTOs)
- Tests: T071-T073, T074-T076 (all test files)

**Phase 4 (User Story 2)**:
- Domain: T078-T080 (entities and enums), T083-T084 (constraints)
- Commands: T087-T088 (create), T092-T093 (availability)
- Queries: T095, T097 (query objects)
- Events: T099-T100 (event definitions)
- Controllers: T102, T106 (separate controllers)
- Tests: T111-T116 (all test files)

**Phase 5 (User Story 3)**:
- Domain: T118-T123 (entities and enums)
- Commands: T130-T131 (create cert), T133, T135, T137, T139, T141 (various commands/queries)
- Queries: T150 (import status)
- Events: T155-T157 (event definitions)
- Controllers: T159, T163, T167 (separate controllers), T170 (DTOs)
- Tests: T171-T179 (all test files)

**Phase 6 (Polish)**: Most tasks (T183-T199) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all domain entities for User Story 1 together:
Task T040: "Create StaffProfile entity in Src/Foundation/services/Staff/Staff.Domain/Entities/StaffProfile.cs"
Task T041: "Create StaffStatus enum in Src/Foundation/services/Staff/Staff.Domain/Enums/StaffStatus.cs"
Task T042: "Create StaffRole enum in Src/Foundation/services/Staff/Staff.Domain/Enums/StaffRole.cs"

# Launch all command validators for User Story 1 together:
Task T049: "Create CreateStaffCommandValidator in Src/Foundation/services/Staff/Staff.Application/Staff/Commands/CreateStaff/CreateStaffCommandValidator.cs"
Task T052: "Create UpdateStaffCommandValidator in Src/Foundation/services/Staff/Staff.Application/Staff/Commands/UpdateStaff/UpdateStaffCommandValidator.cs"

# Launch all event definitions for User Story 1 together:
Task T060: "Create StaffCreated event in Src/Foundation/services/Staff/Staff.Domain/Events/StaffCreated.cs"
Task T061: "Create StaffUpdated event in Src/Foundation/services/Staff/Staff.Domain/Events/StaffUpdated.cs"
Task T062: "Create IdentityProvisioningRequested event in Src/Foundation/services/Staff/Staff.Domain/Events/IdentityProvisioningRequested.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T023)
2. Complete Phase 2: Foundational (T024-T039) ⚠️ CRITICAL - blocks all stories
3. Complete Phase 3: User Story 1 (T040-T077)
4. **STOP and VALIDATE**: Test User Story 1 independently
   - Run unit tests for staff CRUD
   - Run integration tests for event publishing
   - Run BDD tests for profile management
   - Verify search performance <100ms p95
   - Verify audit records for all mutations
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready (T001-T039)
2. Add User Story 1 → Test independently → Deploy/Demo (T040-T077) 🎯 MVP!
   - Delivers: Staff profile creation, identity linking, search, audit
3. Add User Story 2 → Test independently → Deploy/Demo (T078-T117)
   - Adds: Multi-school assignments, FTE validation, availability scheduling
4. Add User Story 3 → Test independently → Deploy/Demo (T118-T182)
   - Adds: Certifications with expiry alerts, team management, bulk import
5. Polish & optimize → Final deployment (T183-T200)

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together (T001-T039)
2. Once Foundational is done:
   - Developer A: User Story 1 - Staff Profiles (T040-T077)
   - Developer B: User Story 2 - Assignments & Schedules (T078-T117)
   - Developer C: User Story 3 - Certifications & Import (T118-T182)
3. Stories complete and integrate independently
4. Team collaborates on Polish phase (T183-T200)

---

## Success Criteria Validation

Match each success criterion to implementation:

- **SC-001** (Search <100ms p95): Tasks T059, T183-T184 (search optimization + caching)
- **SC-002** (100% audit): Tasks T027-T028 (AuditInterceptor), T191 (security review)
- **SC-003** (Event publishing <30s): Tasks T063, T050 (event publishers), T195 (integration test)
- **SC-004** (Certification reminders 60 days): Tasks T152-T153 (reminder worker), T189 (telemetry)
- **SC-005** (Import 50 staff <60s): Tasks T144-T149 (import service), T175 (performance test)

---

## Notes

- **[P]** tasks = different files, no dependencies - can run in parallel
- **[Story]** label maps task to specific user story (US1, US2, US3) for traceability
- Each user story is independently completable and testable
- All entities include `tenant_id` for multi-tenancy (enforced by TenantInterceptor)
- All mutations produce audit records (enforced by AuditInterceptor)
- Events published via MassTransit with Azure Service Bus
- Redis used for session caching and idempotency windows (10-minute TTL)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Run quickstart.md validation before final approval
- Reference architecture patterns from `Plan/CrossCuttingConcerns/`
