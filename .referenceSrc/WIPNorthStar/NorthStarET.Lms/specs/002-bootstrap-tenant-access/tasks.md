# Tasks: Tenant-Isolated District Access

**Input**: Design documents from `/specs/002-bootstrap-tenant-access/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: TDD is mandatory. Include tasks for unit tests, Reqnroll Features + step definitions (committed before implementation), Aspire integration tests, and Playwright journeys. Execute every suite Red → Green before writing production code, capturing failing and passing transcripts for `dotnet test` (unit + Reqnroll + Aspire) and `pwsh tests/ui/playwright.ps1` (Playwright).

**Tool-Assisted Implementation** (for AI agents):

- Begin each session with structured thinking: `#think` or `#mcp_sequentialthi_sequentialthinking` to plan approach
- Query official documentation for .NET/Azure decisions: `#microsoft.docs.mcp` for API contracts and best practices
- UI tasks follow tool-assisted pipeline:
  1. Extract design: `#figma/dev-mode-mcp-server` to gather component specs from Figma frames
  2. Implement UI: Build Razor Pages/components per specifications
  3. Test automation: `#mcp_playwright_browser_navigate`, `#mcp_playwright_browser_click`, `#mcp_playwright_browser_fill_form` for journeys
  4. Debug live: `#mcp_chromedevtool_take_snapshot`, `#mcp_chromedevtool_list_console_messages`, `#mcp_chromedevtool_list_network_requests`

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions
- UI tasks MUST include the exact Figma frame/flow link; if unavailable, label the work "Skipped — No Figma", create the supporting `figma-prompts/` collateral using `#figma/dev-mode-mcp-server`, and omit implementation tasks until the design artifact arrives.
- Reference owning Reqnroll Feature, test assets (unit, BDD, Playwright), and Aspire touchpoints so traceability back to the constitution is obvious.

## Path Conventions

Clean Architecture solution structure:

- **Domain**: `src/NorthStarET.NextGen.Lms.Domain/` (pure business entities, zero dependencies)
- **Application**: `src/NorthStarET.NextGen.Lms.Application/` (use cases, MediatR, FluentValidation)
- **Infrastructure**: `src/NorthStarET.NextGen.Lms.Infrastructure/` (EF Core, Redis, integrations)
- **Contracts**: `src/NorthStarET.NextGen.Lms.Contracts/` (DTOs, requests/responses)
- **API**: `src/NorthStarET.NextGen.Lms.Api/` (REST endpoints, controllers)
- **Web**: `src/NorthStarET.NextGen.Lms.Web/` (Razor Pages frontend)
- **AppHost**: `src/NorthStarET.NextGen.Lms.AppHost/` (Aspire orchestration)
- **Tests**: `tests/unit/`, `tests/bdd/`, `tests/ui/`, `tests/aspire/`, `tests/integration/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and database schema foundation

- [x] T001 Add EF Core migrations for District, DistrictAdmin, AuditRecord, DomainEventEnvelope tables in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/Migrations/` (Migration `InitialDistricts` created: 20251025215148)
- [x] T002 [P] Configure Redis Stack idempotency service in `src/NorthStarET.NextGen.Lms.Infrastructure/Idempotency/IdempotencyService.cs`
- [x] T003 [P] Register Aspire Redis resource in `src/NorthStarET.NextGen.Lms.AppHost/Program.cs` and wire to API/Web projects
- [x] T004 [P] Add Azure Event Grid emulator configuration to AppHost for domain event publishing

**Checkpoint**: Foundation database schema and infrastructure services ready

---

## Phase 2: Foundational (Blocking Prerequisites) ✅ COMPLETE

**Purpose**: Core domain aggregates, repositories, and authorization infrastructure that ALL user stories depend on

**Status**: ✅ All 28 tasks complete (T005-T032) - Foundation ready for user story implementation

### Domain Layer (Pure Business Logic)

- [x] T005 [P] Create `District` aggregate root in `src/NorthStarET.NextGen.Lms.Domain/Districts/District.cs` with Name, Suffix, DeletedAt properties
- [x] T006 [P] Create `DistrictAdmin` aggregate in `src/NorthStarET.NextGen.Lms.Domain/DistrictAdmins/DistrictAdmin.cs` with Email, Status, InvitationSentAtUtc, InvitationExpiresAtUtc properties
- [x] T007 [P] Create `AuditRecord` entity in `src/NorthStarET.NextGen.Lms.Domain/Auditing/AuditRecord.cs` with ActorId, ActorRole, Action, EntityType, BeforePayload, AfterPayload
- [x] T008 [P] Define domain events: `DistrictCreated`, `DistrictUpdated`, `DistrictDeleted` (embedded in District aggregate)
- [x] T009 [P] Define domain events: `DistrictAdminInvited`, `DistrictAdminVerified`, `DistrictAdminRevoked` (embedded in DistrictAdmin aggregate)
- [x] T010 [P] Create `IDistrictRepository` interface in `src/NorthStarET.NextGen.Lms.Domain/Districts/IDistrictRepository.cs`
- [x] T011 [P] Create `IDistrictAdminRepository` interface in `src/NorthStarET.NextGen.Lms.Domain/DistrictAdmins/IDistrictAdminRepository.cs`
- [x] T012 [P] Create `IAuditRepository` interface in `src/NorthStarET.NextGen.Lms.Domain/Auditing/IAuditRepository.cs`

### Infrastructure Layer (Persistence & Integration)

- [x] T013 Create `DistrictEntityConfiguration` EF Core entity config in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/DistrictEntityConfiguration.cs` with unique suffix index
- [x] T014 [P] Create `DistrictAdminEntityConfiguration` EF Core entity config in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/DistrictAdminEntityConfiguration.cs`
- [x] T015 [P] Create `AuditRecordEntityConfiguration` EF Core entity config in `src/NorthStarET.NextGen.Lms.Infrastructure/Auditing/Persistence/AuditRecordEntityConfiguration.cs`
- [x] T016 Implement `DistrictRepository` in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/DistrictRepository.cs` with tenant-scoped queries
- [x] T017 [P] Implement `DistrictAdminRepository` in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/DistrictAdminRepository.cs`
- [x] T018 [P] Implement `AuditRepository` in `src/NorthStarET.NextGen.Lms.Infrastructure/Auditing/Persistence/AuditRepository.cs`
- [x] T019 Configure DbContext with global query filters for soft-delete in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/DistrictsDbContext.cs`
- [x] T020 [P] Implement Redis-backed idempotency token storage in `src/NorthStarET.NextGen.Lms.Infrastructure/Idempotency/IdempotencyService.cs` with 10-minute window (completed in Phase 1)
- [x] T021 [P] Implement domain event outbox publisher to Azure Event Grid in `src/NorthStarET.NextGen.Lms.Infrastructure/Common/MediatRDomainEventPublisher.cs`
- [x] T022 Register all repositories and services in `src/NorthStarET.NextGen.Lms.Infrastructure/DependencyInjection.cs`

### Application Layer (Use Case Orchestration)

- [x] T023 [P] Create `TenantIsolationBehavior` MediatR pipeline in `src/NorthStarET.NextGen.Lms.Infrastructure/Common/Behaviors/TenantIsolationBehavior.cs` to enforce DistrictId scoping
- [x] T024 [P] Create `IdempotencyBehavior` MediatR pipeline in `src/NorthStarET.NextGen.Lms.Infrastructure/Common/Behaviors/IdempotencyBehavior.cs` for create/update commands
- [x] T025 Register MediatR, FluentValidation, and pipeline behaviors in `src/NorthStarET.NextGen.Lms.Application/DependencyInjection.cs` and `src/NorthStarET.NextGen.Lms.Infrastructure/DependencyInjection.cs`

### Contracts Layer (DTOs)

- [x] T026 [P] Create `CreateDistrictRequest` in `src/NorthStarET.NextGen.Lms.Contracts/Districts/CreateDistrictRequest.cs`
- [x] T027 [P] Create `UpdateDistrictRequest` in `src/NorthStarET.NextGen.Lms.Contracts/Districts/UpdateDistrictRequest.cs`
- [x] T028 [P] Create `DistrictResponse` in `src/NorthStarET.NextGen.Lms.Contracts/Districts/DistrictResponse.cs`
- [x] T029 [P] Create `DistrictDetailResponse` in `src/NorthStarET.NextGen.Lms.Contracts/Districts/DistrictDetailResponse.cs`
- [x] T030 [P] Create `InviteDistrictAdminRequest` in `src/NorthStarET.NextGen.Lms.Contracts/DistrictAdmins/InviteDistrictAdminRequest.cs`
- [x] T031 [P] Create `DistrictAdminResponse` in `src/NorthStarET.NextGen.Lms.Contracts/DistrictAdmins/DistrictAdminResponse.cs`
- [x] T032 [P] Create `AuditRecordResponse` in `src/NorthStarET.NextGen.Lms.Contracts/Audit/AuditRecordResponse.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - System Admin Manages Districts (Priority: P1) 🎯 MVP

**Goal**: System Admin can view, create, edit, and delete districts with unique suffixes and tenant isolation

**Independent Test**: Navigate to District Management page, create district with unique suffix, verify in list, edit district, delete district, confirm audit trail

**Figma Links**:

- District Management page: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-2&m=dev
- Create District button: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-43&m=dev
- Create New District modal: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-67&m=dev
- Edit District modal: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-278&m=dev

### Tests for User Story 1 (MANDATORY — execute before implementation) ⚠️

**NOTE: Commit the Reqnroll Feature and step definitions before production code; run `dotnet test` Red → Green and capture both outputs.**

- [x] T033 [US1] Add `specs/002-bootstrap-tenant-access/features/DistrictManagement.feature` with Given/When/Then scenarios for create/edit/delete flows per Figma (18 scenarios created covering full CRUD lifecycle)
- [x] T034 [US1] Implement step definitions in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/DistrictSteps.cs` (40+ step definitions with PendingStepException - Red state achieved)
- [x] T035 [P] [US1] Create Playwright journey test in `tests/ui/tests/district-management.spec.ts` covering Figma frames 22-2, 22-43, 22-67, 22-278
- [x] T036 [P] [US1] Build Aspire integration test project in `tests/aspire/NorthStarET.NextGen.Lms.AspireTests/DistrictManagementTests.cs` to validate API/Web hosting

### Unit Tests for User Story 1 (TDD Red → Green)

- [x] T037 [P] [US1] Create `DistrictTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Domain.Tests/Districts/` for aggregate behavior (suffix uniqueness, soft delete) (12 tests created)
- [x] T038 [P] [US1] Create `CreateDistrictCommandHandlerTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/` for creation logic (4 tests created)
- [x] T039 [P] [US1] Create `UpdateDistrictCommandHandlerTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/` for update logic (5 tests created)
- [x] T040 [P] [US1] Create `DeleteDistrictCommandHandlerTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/` for soft delete logic (4 tests created)
- [x] T041 [P] [US1] Create `DistrictsControllerTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Api.Tests/Districts/` for endpoint validation (8 tests created)

### Application Layer Implementation (Commands, Queries, Validators)

- [x] T042 [P] [US1] Create `CreateDistrictCommand` in `src/NorthStarET.NextGen.Lms.Application/Districts/Commands/CreateDistrict/CreateDistrictCommand.cs` implementing IRequest<Result<CreateDistrictResponse>> with IIdempotentCommand, IAuditableCommand, ITenantScoped
- [x] T043 [P] [US1] Create `UpdateDistrictCommand` in `src/NorthStarET.NextGen.Lms.Application/Districts/Commands/UpdateDistrict/UpdateDistrictCommand.cs` with IAuditableCommand, ITenantScoped
- [x] T044 [P] [US1] Create `DeleteDistrictCommand` in `src/NorthStarET.NextGen.Lms.Application/Districts/Commands/DeleteDistrict/DeleteDistrictCommand.cs` with IAuditableCommand, ITenantScoped
- [x] T045 [P] [US1] Create `GetDistrictQuery` in `src/NorthStarET.NextGen.Lms.Application/Districts/Queries/GetDistrict/GetDistrictQuery.cs` with ITenantScoped
- [x] T046 [P] [US1] Create `ListDistrictsQuery` in `src/NorthStarET.NextGen.Lms.Application/Districts/Queries/ListDistricts/ListDistrictsQuery.cs` with pagination and ITenantScoped
- [x] T047 [US1] Implement `CreateDistrictCommandHandler` in `src/NorthStarET.NextGen.Lms.Application/Districts/Commands/CreateDistrictCommandHandler.cs` with idempotency check, suffix uniqueness validation, audit record creation, and DistrictCreated event emission
- [x] T048 [US1] Implement `UpdateDistrictCommandHandler` in `src/NorthStarET.NextGen.Lms.Application/Districts/Commands/UpdateDistrictCommandHandler.cs` with idempotency, audit, and DistrictUpdated event
- [x] T049 [US1] Implement `DeleteDistrictCommandHandler` in `src/NorthStarET.NextGen.Lms.Application/Districts/Commands/DeleteDistrictCommandHandler.cs` with soft delete, admin cascade archive, audit, and DistrictDeleted event
- [x] T050 [US1] Implement `GetDistrictQueryHandler` in `src/NorthStarET.NextGen.Lms.Application/Districts/Queries/GetDistrictQueryHandler.cs`
- [x] T051 [US1] Implement `ListDistrictsQueryHandler` in `src/NorthStarET.NextGen.Lms.Application/Districts/Queries/ListDistrictsQueryHandler.cs` with tenant filtering
- [x] T052 [P] [US1] Create `CreateDistrictValidator` in `src/NorthStarET.NextGen.Lms.Application/Districts/Validators/CreateDistrictValidator.cs` for Name (3-100 chars) and Suffix (regex ^[a-z0-9.-]+$)
- [x] T053 [P] [US1] Create `UpdateDistrictValidator` in `src/NorthStarET.NextGen.Lms.Application/Districts/Validators/UpdateDistrictValidator.cs`

### API Layer Implementation (REST Endpoints)

- [x] T054 [US1] Implement `DistrictsController` in `src/NorthStarET.NextGen.Lms.Api/Districts/DistrictsController.cs` with endpoints: POST /api/districts, GET /api/districts, GET /api/districts/{id}, PUT /api/districts/{id}, DELETE /api/districts/{id}
- [x] T055 [US1] Add rate limiting middleware (10 req/min) for district create/update endpoints in `src/NorthStarET.NextGen.Lms.Api/Program.cs`
- [x] T056 [P] [US1] Configure anti-CSRF tokens for district mutation endpoints in API Program.cs

### UI Layer Implementation (Razor Pages)

**Use `#figma/dev-mode-mcp-server` to extract design tokens before implementation**

- [x] T057 [US1] Create District Management page at `src/NorthStarET.NextGen.Lms.Web/Pages/Admin/Districts/Index.cshtml` and `Index.cshtml.cs` per Figma 22-2 showing paginated district list
- [x] T058 [US1] Create Create District modal logic in `src/NorthStarET.NextGen.Lms.Web/Pages/Admin/Districts/Create.cshtml` and `Create.cshtml.cs` per Figma 22-67 with Name and Suffix fields
- [x] T059 [US1] Create Edit District modal logic in `src/NorthStarET.NextGen.Lms.Web/Pages/Admin/Districts/Edit.cshtml` and `Edit.cshtml.cs` per Figma 22-278
- [x] T060 [US1] Add district deletion confirmation modal with cascade warning in `src/NorthStarET.NextGen.Lms.Web/Pages/Admin/Districts/Delete.cshtml` and `Delete.cshtml.cs`
- [x] T061 [P] [US1] Create CSS styling for district management components in `src/NorthStarET.NextGen.Lms.Web/wwwroot/css/districts.css`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. Run full test suite (unit, BDD, Aspire, Playwright) and capture Red→Green transcripts.

---

## Phase 4: User Story 2 - System Admin Delegates District Admins (Priority: P2)

**Goal**: System Admin can invite, resend, and revoke district admin assignments with verification status tracking

**Independent Test**: Open Manage Admins page, invite new admin with email matching district suffix, verify Unverified status, resend invite, revoke admin, confirm audit trail

**Figma Links**:

- Manage Admins page: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-181&m=dev
- First Name field: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-285&m=dev
- Last Name field: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-282&m=dev
- Email field: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-288&m=dev
- Send Invitation button: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-314&m=dev
- Unverified status: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-410&m=dev
- Verified status: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-403&m=dev
- Resend button: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=340-96&m=dev
- Remove button: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=340-99&m=dev

### Tests for User Story 2 (MANDATORY — execute before implementation) ⚠️

- [x] T062 [US2] Add `specs/002-bootstrap-tenant-access/features/DistrictAdminDelegation.feature` with scenarios for invite/resend/revoke flows per Figma
- [x] T063 [US2] Implement step definitions in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/DistrictAdminSteps.cs`
- [x] T064 [P] [US2] Create Playwright journey test in `tests/ui/tests/district-admin-delegation.spec.ts` covering Figma frames 24-181, 24-285, 24-282, 24-288, 24-314, 24-410, 24-403
- [x] T065 [P] [US2] Extend Aspire integration tests in `tests/aspire/NorthStarET.NextGen.Lms.AspireTests/DistrictAdminTests.cs`

### Unit Tests for User Story 2 (TDD Red → Green)

- [x] T066 [P] [US2] Create `DistrictAdminTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Domain.Tests/DistrictAdmins/` for invite/verify/revoke state transitions (24 tests created)
- [x] T067 [P] [US2] Create `InviteDistrictAdminCommandHandlerTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/DistrictAdmins/` (5 tests created)
- [x] T068 [P] [US2] Create `ResendInviteCommandHandlerTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/DistrictAdmins/` (5 tests created)
- [x] T069 [P] [US2] Create `RevokeDistrictAdminCommandHandlerTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/DistrictAdmins/` (5 tests created)
- [x] T070 [P] [US2] Create `DistrictAdminsControllerTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Api.Tests/DistrictAdmins/` (8 tests created, skipped until T082 controller implementation)

### Application Layer Implementation (Commands, Queries, Validators)

- [x] T071 [P] [US2] Create `InviteDistrictAdminCommand` in `src/NorthStarET.NextGen.Lms.Application/DistrictAdmins/Commands/InviteDistrictAdminCommand.cs` with DistrictId, Email, FirstName, LastName
- [x] T072 [P] [US2] Create `ResendInviteCommand` in `src/NorthStarET.NextGen.Lms.Application/DistrictAdmins/Commands/ResendInviteCommand.cs`
- [x] T073 [P] [US2] Create `RevokeDistrictAdminCommand` in `src/NorthStarET.NextGen.Lms.Application/DistrictAdmins/Commands/RevokeDistrictAdminCommand.cs`
- [x] T074 [P] [US2] Create `ListDistrictAdminsQuery` in `src/NorthStarET.NextGen.Lms.Application/DistrictAdmins/Queries/ListDistrictAdminsQuery.cs`
- [x] T075 [US2] Implement `InviteDistrictAdminCommandHandler` in `src/NorthStarET.NextGen.Lms.Application/DistrictAdmins/Commands/InviteDistrictAdminCommandHandler.cs` with email suffix validation, idempotency, audit, DistrictAdminInvited event, and email dispatch
- [x] T076 [US2] Implement `ResendInviteCommandHandler` in `src/NorthStarET.NextGen.Lms.Application/DistrictAdmins/Commands/ResendInviteCommandHandler.cs` with idempotency, expiration refresh (7 days), audit
- [x] T077 [US2] Implement `RevokeDistrictAdminCommandHandler` in `src/NorthStarET.NextGen.Lms.Application/DistrictAdmins/Commands/RevokeDistrictAdminCommandHandler.cs` with immediate access revocation, audit, DistrictAdminRevoked event
- [x] T078 [US2] Implement `ListDistrictAdminsQueryHandler` in `src/NorthStarET.NextGen.Lms.Application/DistrictAdmins/Queries/ListDistrictAdminsQueryHandler.cs` with tenant filtering
- [x] T079 [P] [US2] Create `InviteDistrictAdminValidator` in `src/NorthStarET.NextGen.Lms.Application/DistrictAdmins/Validators/InviteDistrictAdminValidator.cs` for email format (RFC 5322) and suffix alignment

### Infrastructure Layer (Email Service with Retry)

- [x] T080 [US2] Implement email invitation service with exponential backoff retry (3 attempts) in `src/NorthStarET.NextGen.Lms.Infrastructure/Notifications/EmailInvitationService.cs`
- [x] T081 [US2] Implement dead-letter queue for failed email deliveries in `src/NorthStarET.NextGen.Lms.Infrastructure/Notifications/EmailFailureHandler.cs`

### API Layer Implementation (REST Endpoints)

- [x] T082 [US2] Implement `DistrictAdminsController` in `src/NorthStarET.NextGen.Lms.Api/Controllers/DistrictAdminsController.cs` with endpoints: POST /api/districts/{districtId}/admins, GET /api/districts/{districtId}/admins, POST /api/districts/{districtId}/admins/{adminId}/resend, DELETE /api/districts/{districtId}/admins/{adminId}
- [x] T083 [US2] Add rate limiting middleware (10 req/min) for invite/resend endpoints in API Program.cs

### UI Layer Implementation (Razor Pages)

**Use `#figma/dev-mode-mcp-server` to extract design tokens before implementation**

- [x] T084 [US2] Create Manage Admins page at `src/NorthStarET.NextGen.Lms.Web/Pages/Admin/DistrictAdmins/Manage.cshtml` and `Manage.cshtml.cs` per Figma 24-181 showing admin list with status indicators
- [x] T085 [US2] Create invite form with FirstName, LastName, Email fields per Figma 24-285, 24-282, 24-288 in Manage.cshtml
- [x] T086 [US2] Implement Unverified status badge per Figma 24-410 in Manage.cshtml
- [x] T087 [US2] Implement Verified status badge per Figma 24-403 in Manage.cshtml
- [x] T088 [US2] Add Resend Invite button per Figma 340-96 with confirmation modal
- [x] T089 [US2] Add Remove Admin button per Figma 340-99 with confirmation modal
- [x] T090 [P] [US2] Create CSS styling for admin management components in `src/NorthStarET.NextGen.Lms.Web/wwwroot/css/district-admins.css`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently. Run full test suite and capture Red→Green transcripts.

---

## Phase 5: User Story 3 - District Admin Operates Within Their District (Priority: P3) - DEFERRED

**Status**: DEFERRED to subsequent feature after System Admin workflows are validated

**Goal**: District Admin lands on district-scoped home experience with no access to platform-wide administration

**Independent Test**: Log in as District Admin, confirm landing on district home, attempt to access global routes, verify access denied

**Figma Links**: NOT AVAILABLE - This user story is deferred pending design completion

**Tasks**: SKIPPED — No Figma links available. Create `figma-prompts/district-admin-ux.md` using `#figma/dev-mode-mcp-server` for future implementation once designs arrive.

---

## Phase 6: User Story 4 - Platform Captures Governance Signals (Priority: P4)

**Goal**: Audit trail and domain events reliably capture all district/admin lifecycle actions

**Independent Test**: Trigger create/update/delete and invite lifecycle actions, query audit log API, verify Event Grid emulator received events

### Tests for User Story 4 (MANDATORY — execute before implementation) ⚠️

- [x] T091 [US4] Add `specs/002-bootstrap-tenant-access/features/AuditAndEvents.feature` with scenarios for audit capture and event emission (pre-existing)
- [x] T092 [US4] Implement step definitions in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/AuditSteps.cs` (pre-existing)
- [x] T093 [P] [US4] Create integration test in `tests/integration/NorthStarET.NextGen.Lms.IntegrationTests/Audit/AuditEndToEndTests.cs` validating audit records in PostgreSQL (pre-existing)
- [x] T094 [P] [US4] Create integration test in `tests/integration/NorthStarET.NextGen.Lms.IntegrationTests/Events/EventPublishingTests.cs` validating events in Event Grid emulator (pre-existing)

### Unit Tests for User Story 4 (TDD Red → Green)

- [x] T095 [P] [US4] Create `AuditRepositoryTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Infrastructure.Tests/Auditing/` (pre-existing)
- [x] T096 [P] [US4] Create `DomainEventPublisherTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Infrastructure.Tests/Eventing/` (pre-existing)
- [x] T097 [P] [US4] Create `GetAuditRecordsQueryHandlerTests.cs` in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Audit/` (pre-existing)

### Application Layer Implementation (Queries)

- [x] T098 [P] [US4] Create `GetAuditRecordsQuery` in `src/NorthStarET.NextGen.Lms.Application/Audit/Queries/GetAuditRecordsQuery.cs` with pagination and districtId filter
- [x] T099 [US4] Implement `GetAuditRecordsQueryHandler` in `src/NorthStarET.NextGen.Lms.Application/Audit/Queries/GetAuditRecordsQueryHandler.cs`

### Infrastructure Layer (Event Publishing)

- [ ] T100 [US4] **DEFERRED** - Implement outbox pattern processor background service in `src/NorthStarET.NextGen.Lms.Infrastructure/Eventing/OutboxProcessor.cs` to publish DomainEventEnvelope records to Event Grid (requires Event Grid Aspire integration story)
- [ ] T101 [US4] **DEFERRED** - Configure retry policy (max 5 attempts) for Event Grid publishing in OutboxProcessor (blocked by T100)

### API Layer Implementation (REST Endpoints)

- [x] T102 [US4] Implement `AuditController` in `src/NorthStarET.NextGen.Lms.Api/Audit/AuditController.cs` with endpoints: GET /api/audit, GET /api/audit?districtId={id}, GET /api/audit?count={n}

**Checkpoint**: All user stories should now be independently functional with complete audit and event traceability.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T103 [P] Add comprehensive API documentation using Swagger/OpenAPI annotations in all controllers
- [ ] T104 [P] Implement global exception handling middleware in `src/NorthStarET.NextGen.Lms.Api/Middleware/GlobalExceptionHandler.cs`
- [ ] T105 [P] Add structured logging with correlation IDs using Serilog throughout Application and API layers
- [ ] T106 [P] Implement SQL injection prevention via parameterized queries (verify EF Core configuration)
- [ ] T107 [P] Implement XSS prevention via Razor encoding (verify Web layer configuration)
- [ ] T108 [P] Add CSRF token validation middleware in `src/NorthStarET.NextGen.Lms.Api/Middleware/AntiForgeryMiddleware.cs`
- [ ] T109 [P] Implement optimistic concurrency control via EF Core row version in District and DistrictAdmin entities
- [ ] T110 [P] Add health checks for PostgreSQL, Redis, Event Grid in `src/NorthStarET.NextGen.Lms.ServiceDefaults/Extensions.cs`
- [ ] T111 [P] Configure telemetry and distributed tracing via OpenTelemetry in ServiceDefaults
- [ ] T112 Run `dotnet format` and verify code style compliance
- [ ] T113 Run full test suite with coverage: `dotnet test --collect:"XPlat Code Coverage"` and verify ≥80% line coverage
- [ ] T114 Execute quickstart.md validation end-to-end per documented steps
- [ ] T115 Capture final Red→Green transcript evidence for all test suites and commit to phase review branch
- [ ] T116 Update IMPLEMENTATION_STATUS.md with feature completion status

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (Phase 4)**: Depends on Foundational (Phase 2) - Can integrate with US1 but independently testable
- **User Story 3 (Phase 5)**: DEFERRED - Awaiting Figma designs
- **User Story 4 (Phase 6)**: Depends on Foundational (Phase 2) - Can run in parallel with US1/US2
- **Polish (Phase 7)**: Depends on completion of US1, US2, US4 (US3 deferred)

### User Story Dependencies

- **User Story 1 (P1)**: MVP - Must complete first for district CRUD foundation
- **User Story 2 (P2)**: Depends on US1 (requires districts to exist) but can run in parallel if districts seeded
- **User Story 3 (P3)**: DEFERRED - Awaiting design completion
- **User Story 4 (P4)**: No dependencies on US1/US2 - Can run in parallel (audit/events are cross-cutting)

### Within Each User Story

- Tests MUST be written and FAIL before implementation (unit/TDD, Reqnroll, Playwright, Aspire integration)
- Domain models before Application commands/queries
- Application layer before API layer
- API layer before UI layer
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

**Phase 1 (Setup)**: All tasks marked [P] can run in parallel (T002, T003, T004)

**Phase 2 (Foundational)**: All tasks within each layer can run in parallel:

- Domain layer: T005-T012 can run in parallel
- Infrastructure layer: T014, T015, T017, T018, T020, T021 can run in parallel (T013 blocks T016)
- Application layer: T023, T024 can run in parallel
- Contracts layer: T026-T032 can run in parallel

**Phase 3 (User Story 1)**:

- Tests: T033-T041 can run in parallel
- Commands/Queries: T042-T046, T052-T053 can run in parallel (T047-T051 depend on these)
- API layer: T056 can run in parallel with T054-T055 completion
- UI layer: T061 can run in parallel with T057-T060

**Phase 4 (User Story 2)**:

- Tests: T062-T070 can run in parallel
- Commands/Queries: T071-T074, T079 can run in parallel (T075-T078 depend on these)
- Infrastructure: T080-T081 can run in parallel with command handlers
- UI layer: T090 can run in parallel with T084-T089

**Phase 6 (User Story 4)**:

- Tests: T091-T097 can run in parallel
- Application: T098 can run in parallel with T095-T097
- Infrastructure: T101 can run in parallel with T100

**Phase 7 (Polish)**: All tasks marked [P] (T103-T111) can run in parallel

### Parallel Example: User Story 1

```bash
# Terminal 1: Run all tests in parallel (Red phase)
dotnet test tests/unit/NorthStarET.NextGen.Lms.Domain.Tests/Districts/DistrictTests.cs
dotnet test tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/CreateDistrictCommandHandlerTests.cs
dotnet test tests/bdd/NorthStarET.NextGen.Lms.Bdd/ --filter "Category=US1"
pwsh tests/ui/playwright.ps1 --filter "district-management"

# Terminal 2: Create all domain models in parallel
code src/NorthStarET.NextGen.Lms.Contracts/Districts/CreateDistrictRequest.cs
code src/NorthStarET.NextGen.Lms.Contracts/Districts/UpdateDistrictRequest.cs
code src/NorthStarET.NextGen.Lms.Contracts/Districts/DistrictResponse.cs

# Terminal 3: Create all validators in parallel (after commands exist)
code src/NorthStarET.NextGen.Lms.Application/Districts/Validators/CreateDistrictValidator.cs
code src/NorthStarET.NextGen.Lms.Application/Districts/Validators/UpdateDistrictValidator.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 + User Story 4 Only)

1. Complete Phase 1: Setup → Foundation database and infrastructure
2. Complete Phase 2: Foundational → Domain/Application/Infrastructure ready
3. Complete Phase 3: User Story 1 → District CRUD with UI
4. Complete Phase 6: User Story 4 → Audit trail and events
5. **STOP and VALIDATE**: Test US1 + US4 independently, capture Red→Green evidence
6. Deploy/demo MVP showing district management with full audit traceability

### Incremental Delivery

1. **Foundation**: Setup (Phase 1) + Foundational (Phase 2) → Foundation ready
2. **MVP Delivery**: User Story 1 + User Story 4 → Test independently → Deploy/Demo
3. **Delegation**: Add User Story 2 → Test independently → Deploy/Demo
4. **District Admin UX**: Wait for Figma designs → Implement User Story 3 → Deploy/Demo
5. **Polish**: Complete Phase 7 cross-cutting concerns → Final production release

### Parallel Team Strategy

With multiple developers after Foundational phase completes:

- **Developer A**: User Story 1 (district CRUD + UI)
- **Developer B**: User Story 4 (audit/events)
- **Developer C**: User Story 2 (admin delegation)

Stories complete and integrate independently via clean interfaces.

---

## Red→Green Evidence Requirements (MANDATORY)

### Before Implementation (Red Phase)

Capture failing test output to prove TDD discipline:

```bash
# Unit and BDD tests (before production code)
dotnet test tests/unit/NorthStarET.NextGen.Lms.Application.Tests --configuration Debug --verbosity normal > phase-red-unit-tests.txt
dotnet test tests/bdd/NorthStarET.NextGen.Lms.Bdd --configuration Debug --verbosity normal > phase-red-bdd-tests.txt

# Aspire integration tests (before wiring complete)
dotnet test tests/aspire/NorthStarET.NextGen.Lms.AspireTests --configuration Debug --verbosity normal > phase-red-aspire-tests.txt

# Playwright UI tests (before UI implementation)
pwsh tests/ui/playwright.ps1 > phase-red-playwright-tests.txt
```

### After Implementation (Green Phase)

Capture passing test output to prove completion:

```bash
# Unit and BDD tests (after production code)
dotnet test tests/unit/NorthStarET.NextGen.Lms.Application.Tests --configuration Debug --verbosity normal > phase-green-unit-tests.txt
dotnet test tests/bdd/NorthStarET.NextGen.Lms.Bdd --configuration Debug --verbosity normal > phase-green-bdd-tests.txt

# Aspire integration tests (after wiring complete)
dotnet test tests/aspire/NorthStarET.NextGen.Lms.AspireTests --configuration Debug --verbosity normal > phase-green-aspire-tests.txt

# Playwright UI tests (after UI implementation)
pwsh tests/ui/playwright.ps1 > phase-green-playwright-tests.txt
```

### Attachment to Phase Review

All 8 transcript files (4 Red + 4 Green) MUST be committed to phase review branch before PR creation:

```bash
git add phase-*-*.txt
git commit -m "docs: attach Red→Green evidence for Phase 3 (US1)"
git push origin HEAD:002review-Phase3
```

---

## Notes

- **[P]** tasks = different files, no dependencies - can run in parallel
- **[Story]** label maps task to specific user story for traceability (US1, US2, US3, US4)
- Each user story should be independently completable and testable
- **Red→Green Evidence (MANDATORY)**: Capture terminal output BEFORE and AFTER implementation for all test suites
- After each task completes: `git add . && git commit -m "feat: <description>" && git pull && git push origin HEAD:002review-Phase[N]`
- Maintain Clean Architecture boundaries (UI → Application → Domain; Infrastructure behind interfaces) in every task
- **AI agents**: Use tool-assisted patterns (structured thinking with `#think`, official docs queries with `#microsoft.docs.mcp`, Figma extraction with `#figma/dev-mode-mcp-server`, Playwright automation, DevTools debugging) as mandated by constitution v1.5.0
- Stop at any checkpoint to validate story independently before proceeding
- **UI tasks without Figma**: Label "Skipped — No Figma", create `figma-prompts/` collateral, HALT implementation until design arrives
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- **Code coverage**: Maintain ≥80% line coverage throughout; CI blocks merges below threshold
- **Performance**: All API operations MUST meet P95 < 500ms latency target
- **Security**: Apply rate limiting, CSRF protection, SQL injection prevention, XSS encoding per constitution requirements

---

## Task Summary

**Total Tasks**: 116

**Tasks by User Story**:

- Setup (Phase 1): 4 tasks
- Foundational (Phase 2): 28 tasks (BLOCKS all user stories)
- User Story 1 (P1 - MVP): 29 tasks (T033-T061)
- User Story 2 (P2): 29 tasks (T062-T090)
- User Story 3 (P3): DEFERRED (awaiting Figma)
- User Story 4 (P4): 12 tasks (T091-T102)
- Polish (Phase 7): 14 tasks (T103-T116)

**Parallel Opportunities**: 62 tasks marked [P] can run in parallel within their phase

**Independent Test Criteria**:

- **US1**: Create/edit/delete district via UI, verify in PostgreSQL, check audit trail
- **US2**: Invite/resend/revoke admin via UI, verify status transitions, check email delivery logs
- **US3**: DEFERRED
- **US4**: Trigger actions, query audit API, verify events in Event Grid emulator

**Suggested MVP Scope**: Phase 1 (Setup) + Phase 2 (Foundational) + Phase 3 (US1) + Phase 6 (US4)

**Constitution Compliance**: ✅ All tasks follow checklist format with Task ID, optional [P] marker, [Story] label for user story phases, and exact file paths
