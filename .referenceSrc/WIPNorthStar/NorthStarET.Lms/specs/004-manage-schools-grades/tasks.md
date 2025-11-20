---
description: "Task list for Manage Schools & Grades feature"
---

# Tasks: Manage Schools & Grades

**Input**: Design documents from `specs/004-manage-schools-grades/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: TDD remains mandatory. Execute `dotnet test --configuration Debug --verbosity normal`, `dotnet test tests/bdd/NorthStarET.NextGen.Lms.Bdd/NorthStarET.NextGen.Lms.Bdd.csproj`, `dotnet test tests/aspire/NorthStarET.NextGen.Lms.AspireTests/NorthStarET.NextGen.Lms.AspireTests.csproj`, and `pwsh tests/ui/playwright.ps1` in Red -> Green cycles, capturing transcripts under `specs/004-manage-schools-grades/checklists/`.

**Tool-Assisted Implementation**: Begin every session with structured thinking (`#mcp_sequentialthi_sequentialthinking`). Ground .NET and Azure decisions via `#microsoft.docs.mcp`. UI remains blocked until Figma frames arrive - maintain prompts in `specs/004-manage-schools-grades/figma-prompts/` and mark tasks "Skipped - No Figma" where applicable. When designs land, extract specs with `#figma/dev-mode-mcp-server`, validate journeys with Playwright MCP tools, and debug via Chrome DevTools MCP.

**Organization**: Tasks are grouped by user story to preserve Clean Architecture boundaries and keep each story independently implementable and testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Task can proceed in parallel with unrelated work
- **[Story]**: User story label (US1-US4). Setup, Foundational, and Polish tasks omit story labels
- Include exact file paths in every description
- UI delivery is blocked - mark tasks as "Skipped - No Figma" and update the associated prompt file until the design is available

## Path Conventions

- Production code lives under `src/`
- Tests live under `tests/`
- Feature collateral stays in `specs/004-manage-schools-grades/`
- OpenAPI contract lives at `specs/004-manage-schools-grades/contracts/schools.openapi.yaml`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish evidence and BDD scaffolding before implementation.

- [X] T001 Create Red-Green evidence folder `specs/004-manage-schools-grades/checklists/phase-1-evidence/` with `.gitkeep` placeholder for transcript uploads.
- [X] T002 Create `specs/004-manage-schools-grades/features/` directory and add `README.md` describing required Reqnroll assets for the Schools & Grades feature slice.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain and infrastructure scaffolding required by every story.

- [X] T003 Define `SchoolStatus`, `SchoolType`, and `GradeLevel` enums plus grade taxonomy seed in `src/NorthStarET.NextGen.Lms.Domain/Districts/Schools/SchoolEnumerations.cs` per `data-model.md`.
- [X] T004 Create `GradeOffering` entity with audit fields and validation in `src/NorthStarET.NextGen.Lms.Domain/Districts/Schools/GradeOffering.cs`.
- [X] T005 Implement tenant-scoped `School` aggregate with audit metadata, soft delete, and range helpers in `src/NorthStarET.NextGen.Lms.Domain/Districts/Schools/School.cs`.
- [X] T006 Add `SchoolChangeEvent` domain event (Created, Updated, Deleted, GradesUpdated) in `src/NorthStarET.NextGen.Lms.Domain/Districts/Schools/Events/SchoolChangeEvent.cs`.
- [X] T007 Introduce `ISchoolRepository` abstraction in `src/NorthStarET.NextGen.Lms.Domain/Districts/Schools/ISchoolRepository.cs` covering list, detail, create, update, delete, and grade assignment operations.
- [X] T008 Add EF Core entity configurations for `School` and `GradeOffering` and update `DistrictsDbContext` in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/` to include new `DbSet`s and model builder rules.
- [X] T009 Create migration `AddSchoolsAndGradeOfferings` in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/Migrations/` to provision tables, indexes, and uniqueness constraints.
- [X] T010 Implement `SchoolRepository` skeleton in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/SchoolRepository.cs` and register it in `src/NorthStarET.NextGen.Lms.Infrastructure/DependencyInjection.cs`.
- [X] T011 Add `IGradeTaxonomyProvider` abstraction with default adapter in `src/NorthStarET.NextGen.Lms.Application/Common/Abstractions/IGradeTaxonomyProvider.cs` sourcing the central grade list for all stories.

---

## Phase 3: User Story 1 - Maintain District Schools (Priority: P1) MVP

**Goal**: District admins manage their district's school catalog (list, search, create, update, delete) with tenant isolation and clear feedback.

**Independent Test**: Execute `SchoolCatalogServiceTests`, `district-admin-manages-schools.feature`, `SchoolCatalogAspireTests`, and `SchoolCatalogManagementTests` (Playwright) in Red -> Green cycles; confirm success messages and soft delete flows via API responses.

### Tests & Red Phase

- [X] T012 [US1] Add `specs/004-manage-schools-grades/features/district-admin-manages-schools.feature` covering list/search/create/update/delete scenarios (link acceptance criteria and note missing Figma frames).
- [X] T013 [US1] Create BDD step definitions stub `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/SchoolCatalogSteps.cs` exercising planned API endpoints.
- [X] T014 [US1] Add failing unit test skeleton `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/Schools/SchoolCatalogServiceTests.cs` capturing validation and event expectations.
- [X] T015 [P] [US1] Add Playwright stub `tests/ui/NorthStarET.NextGen.Lms.Playwright/Tests/SchoolCatalogManagementTests.cs` annotated with `Assert.Ignore("Skipped - No Figma")` until UI assets arrive.
- [X] T016 [P] [US1] Add Aspire integration stub `tests/aspire/NorthStarET.NextGen.Lms.AspireTests/SchoolCatalogAspireTests.cs` wiring catalog API to AppHost resources.
- [X] T017 [US1] Capture Red-state transcripts (`dotnet test ... > specs/004-manage-schools-grades/checklists/phase-1-evidence/us1-dotnet-test-red.txt` and `pwsh tests/ui/playwright.ps1 > .../us1-playwright-red.txt`).

### Implementation & Green Phase

### Implementation & Green Phase

- [X] T018 [P] [US1] Add school contract DTOs (`SchoolListItemResponse`, `SchoolDetailResponse`, `CreateSchoolRequest`, `UpdateSchoolRequest`) under `src/NorthStarET.NextGen.Lms.Contracts/Schools/` and matching application models in `src/NorthStarET.NextGen.Lms.Application/Districts/Schools/Models/`.
- [X] T019 [US1] Implement `ListSchoolsQuery` and handler in `src/NorthStarET.NextGen.Lms.Application/Districts/Queries/Schools/ListSchoolsQuery.cs` with search, sort, and tenant filters.
- [X] T020 [US1] Implement `CreateSchoolCommand`, `CreateSchoolCommandValidator`, and `CreateSchoolCommandHandler` in `src/NorthStarET.NextGen.Lms.Application/Districts/Commands/Schools/CreateSchool/` checking uniqueness constraints.
- [X] T021 [US1] Implement `UpdateSchoolCommand`, `UpdateSchoolCommandValidator`, and `UpdateSchoolCommandHandler` in `.../UpdateSchool/` with concurrency token validation.
- [X] T022 [US1] Implement `DeleteSchoolCommand`, `DeleteSchoolCommandValidator`, and `DeleteSchoolCommandHandler` in `.../DeleteSchool/` marking DeletedAt timestamp.
- [X] T023 [US1] Complete repository methods (`ISchoolRepository.AddAsync`, `UpdateAsync`) and integrate them into command handlers.
- [X] T024 [US1] Add `SchoolsController` in `src/NorthStarET.NextGen.Lms.Api/Controllers/` wiring `GET/POST/PUT/DELETE /api/districts/{districtId}/schools` to query/command handlers, returning appropriate status codes and tenant-scoped JSON.
- [X] T025 [US1] Flesh out `SchoolCatalogSteps.cs` to call API endpoints and assert outcomes using contract DTOs.
- [X] T026 [US1] Extend `SchoolCatalogServiceTests` to cover list/create/update/delete success, validation failures, and event publishing paths.
- [X] T027 [US1] Capture Green-state transcripts (`dotnet test ... > specs/004-manage-schools-grades/checklists/phase-1-evidence/us1-dotnet-test-green.txt` and `pwsh tests/ui/playwright.ps1 > .../us1-playwright-green.txt`).
- [X] T028 [US1] Skipped - No Figma: Update `specs/004-manage-schools-grades/figma-prompts/p1-maintain-district-schools-prompt.md` with implementation notes and block Razor delivery until frames arrive.

---

## Phase 4: User Story 2 - Configure School Grades (Priority: P1)

**Goal**: District admins manage grade offerings per school with range selection and select-all helpers.

**Independent Test**: Run grade assignment unit tests, `district-admin-configures-grades.feature`, Aspire wiring, and Playwright journey once UI lands; confirm grade selections persist idempotently.

### Tests & Red Phase

- [ ] T029 [US2] Add `specs/004-manage-schools-grades/features/district-admin-configures-grades.feature` describing checklist, range, and select-all scenarios.
- [ ] T030 [US2] Extend `SchoolCatalogSteps.cs` with grade management step bindings in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/`.
- [ ] T031 [US2] Add failing unit test skeleton `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/Schools/GradeAssignmentsServiceTests.cs` covering range selection logic.
- [ ] T032 [P] [US2] Add Playwright stub `tests/ui/NorthStarET.NextGen.Lms.Playwright/Tests/ConfigureSchoolGradesTests.cs` with `Assert.Ignore("Skipped - No Figma")`.
- [ ] T033 [P] [US2] Add Aspire integration stub `tests/aspire/NorthStarET.NextGen.Lms.AspireTests/SchoolGradesAspireTests.cs` for grade workflows.
- [ ] T034 [US2] Capture Red-state transcripts (`dotnet test ... > specs/004-manage-schools-grades/checklists/phase-1-evidence/us2-dotnet-test-red.txt` and `pwsh tests/ui/playwright.ps1 > .../us2-playwright-red.txt`).

### Implementation & Green Phase

- [ ] T035 [US2] Implement grade range and select-all helpers inside `School` aggregate in `src/NorthStarET.NextGen.Lms.Domain/Districts/Schools/School.cs`.
- [ ] T036 [US2] Implement `SetSchoolGradesCommand` handler and validator in `src/NorthStarET.NextGen.Lms.Application/Districts/Commands/Schools/SetGrades/` enforcing at-least-one-grade confirmations.
- [ ] T037 [US2] Extend `SchoolRepository` to replace grade offerings transactionally in `src/NorthStarET.NextGen.Lms.Infrastructure/Districts/Persistence/SchoolRepository.cs`.
- [ ] T038 [US2] Emit `SchoolChangeEvent` with `GradesUpdated` change type from the infrastructure pipeline (update `SchoolRepository` and event dispatcher).
- [ ] T039 [US2] Add `PUT /districts/{districtId}/schools/{schoolId}/grades` endpoint to `SchoolsController` in `src/NorthStarET.NextGen.Lms.Api/Controllers/`.
- [ ] T040 [US2] Complete BDD grade steps in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/SchoolCatalogSteps.cs` asserting grade persistence and select-all behavior.
- [ ] T041 [US2] Implement `GradeAssignmentsServiceTests` verifying command handler success, empty-grade confirmation, and range auto-selection.
- [ ] T042 [US2] Capture Green-state transcripts (`dotnet test ... > specs/004-manage-schools-grades/checklists/phase-1-evidence/us2-dotnet-test-green.txt` and `pwsh tests/ui/playwright.ps1 > .../us2-playwright-green.txt`).
- [ ] T043 [US2] Skipped - No Figma: Update `specs/004-manage-schools-grades/figma-prompts/p1-configure-school-grades-prompt.md` with acceptance coverage notes and defer Razor checklist work.

---

## Phase 5: User Story 3 - System Admin District Oversight (Priority: P2)

**Goal**: System admins switch districts, audit school catalogs, and apply updates with tenant safeguards.

**Independent Test**: Execute system-admin scope unit tests, `system-admin-oversees-districts.feature`, Aspire orchestration, and Playwright once UI assets exist; verify audit logging and isolation.

### Tests & Red Phase

- [ ] T044 [US3] Add `specs/004-manage-schools-grades/features/system-admin-oversees-districts.feature` for district switching and edit flows.
- [ ] T045 [US3] Extend `SchoolCatalogSteps.cs` with system-admin context bindings in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/`.
- [ ] T046 [US3] Add failing unit test skeleton `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/Schools/SystemAdminScopeServiceTests.cs` covering tenant override rules.
- [ ] T047 [P] [US3] Add Playwright stub `tests/ui/NorthStarET.NextGen.Lms.Playwright/Tests/SystemAdminManageSchoolsTests.cs` with `Assert.Ignore("Skipped - No Figma")`.
- [ ] T048 [P] [US3] Add Aspire integration stub `tests/aspire/NorthStarET.NextGen.Lms.AspireTests/SystemAdminSchoolsAspireTests.cs`.
- [ ] T049 [US3] Capture Red-state transcripts (`dotnet test ... > specs/004-manage-schools-grades/checklists/phase-1-evidence/us3-dotnet-test-red.txt` and `pwsh tests/ui/playwright.ps1 > .../us3-playwright-red.txt`).

### Implementation & Green Phase

- [ ] T050 [US3] Implement system admin tenant context service or command in `src/NorthStarET.NextGen.Lms.Application/Authorization/SystemAdminTenantContext/` to set active district safely.
- [ ] T051 [US3] Extend API (`src/NorthStarET.NextGen.Lms.Api/Controllers/TenantController.cs`) to expose district-switch endpoint guarded by system admin claims.
- [ ] T052 [US3] Update session cache and Redis projections in `src/NorthStarET.NextGen.Lms.Infrastructure/Authentication/SessionStore/` to persist system admin district selections with sliding expiration.
- [ ] T053 [US3] Finalize system-admin BDD steps in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/SchoolCatalogSteps.cs` ensuring cross-district operations honor isolation.
- [ ] T054 [US3] Implement `SystemAdminScopeServiceTests` asserting authorized and unauthorized transitions.
- [ ] T055 [US3] Capture Green-state transcripts (`dotnet test ... > specs/004-manage-schools-grades/checklists/phase-1-evidence/us3-dotnet-test-green.txt` and `pwsh tests/ui/playwright.ps1 > .../us3-playwright-green.txt`).
- [ ] T056 [US3] Skipped - No Figma: Update `specs/004-manage-schools-grades/figma-prompts/p2-system-admin-oversight-prompt.md` with required states and leave UI pending.

---

## Phase 6: User Story 4 - District Admin Operates Within Their District (Priority: P4)

**Goal**: Provide district-scoped home experience with KPIs and limited admin management while enforcing isolation.

**Independent Test**: Run district-admin home unit tests, `district-admin-home.feature`, Aspire orchestration, and Playwright journey once UI ships; validate all API calls remain tenant-scoped.

### Tests & Red Phase

- [ ] T057 [US4] Add `specs/004-manage-schools-grades/features/district-admin-home.feature` outlining KPIs, invite management, and access control scenarios.
- [ ] T058 [US4] Extend `SchoolCatalogSteps.cs` (or add a dedicated file) with district-admin home bindings in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/`.
- [ ] T059 [US4] Add failing unit test skeleton `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/Home/DistrictAdminHomeQueryTests.cs` covering dashboard aggregations.
- [ ] T060 [P] [US4] Add Playwright stub `tests/ui/NorthStarET.NextGen.Lms.Playwright/Tests/DistrictAdminHomeTests.cs` with `Assert.Ignore("Skipped - No Figma")`.
- [ ] T061 [P] [US4] Add Aspire integration stub `tests/aspire/NorthStarET.NextGen.Lms.AspireTests/DistrictAdminHomeAspireTests.cs`.
- [ ] T062 [US4] Capture Red-state transcripts (`dotnet test ... > specs/004-manage-schools-grades/checklists/phase-1-evidence/us4-dotnet-test-red.txt` and `pwsh tests/ui/playwright.ps1 > .../us4-playwright-red.txt`).

### Implementation & Green Phase

- [ ] T063 [US4] Implement district admin home query or service in `src/NorthStarET.NextGen.Lms.Application/Districts/Home/GetDistrictAdminDashboardQuery.cs` aggregating KPIs and invites.
- [ ] T064 [US4] Add API endpoint or Razor view model in `src/NorthStarET.NextGen.Lms.Api/Controllers/DistrictAdminHomeController.cs` (or equivalent) returning dashboard data.
- [ ] T065 [US4] Finalize BDD steps validating district-only navigation and audit logging in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/`.
- [ ] T066 [US4] Implement `DistrictAdminHomeQueryTests` ensuring isolation, audit, and invite metrics.
- [ ] T067 [US4] Capture Green-state transcripts (`dotnet test ... > specs/004-manage-schools-grades/checklists/phase-1-evidence/us4-dotnet-test-green.txt` and `pwsh tests/ui/playwright.ps1 > .../us4-playwright-green.txt`).
- [ ] T068 [US4] Skipped - No Figma: Document deferred UI delivery in `specs/004-manage-schools-grades/figma-prompts/` and leave Razor work pending.

---

## Final Phase: Polish and Cross-Cutting Concerns

**Purpose**: Harden implementation, align documentation, and prepare phase review artifacts.

- [ ] T069 Run `dotnet build --configuration Debug --verbosity normal /warnaserror` at solution root to ensure no compiler warnings regress.
- [ ] T070 Capture consolidated Green transcripts (`dotnet test --configuration Debug --verbosity normal > specs/004-manage-schools-grades/checklists/phase-1-evidence/dotnet-test-green.txt` and `pwsh tests/ui/playwright.ps1 > .../playwright-green.txt`).
- [ ] T071 Execute `dotnet test --collect:"XPlat Code Coverage"` and confirm Schools & Grades coverage stays at or above 80 percent.
- [ ] T072 Align `specs/004-manage-schools-grades/contracts/schools.openapi.yaml` with implemented endpoints and response schemas.
- [ ] T073 Update `docs/aspire-fixture-integration.md` with new AppHost resources and Redis/PostgreSQL usage for schools catalog.
- [ ] T074 Record completion status and next steps in `IMPLEMENTATION_STATUS_002.md` referencing phase review branch `004review-Phase[n]`.

---

## Dependencies and Execution Order

- Setup (Phase 1) precedes all work.
- Foundational (Phase 2) must complete before any user story begins; schema, repositories, and taxonomy are shared prerequisites.
- User Story 1 (US1) depends on Foundational work and delivers the MVP.
- User Story 2 (US2) depends on US1 because it reuses school catalog primitives.
- User Story 3 (US3) depends on US1 for catalog operations and on Foundational authentication pieces; it can start once US1 infrastructure stabilizes.
- User Story 4 (US4) depends on US1 and US2 data flows plus existing authentication context.
- Polish runs after the desired stories are complete and before phase review submission.

## Parallel Opportunities

- T015, T016, and T018 can run concurrently once US1 tests and Red state exist because they touch different files.
- T032 and T033 parallelize with US2 server work while grade handlers are implemented.
- T047 and T048 can proceed in parallel with US3 backend tasks.
- T060 and T061 can execute alongside US4 application logic.
- Documentation and OpenAPI alignment (T072 and T073) can run concurrently with final verification once APIs stabilize.

## Implementation Strategy

1. Complete Phases 1-2 (Setup plus Foundational) to establish domain schema.
2. Deliver the MVP via User Story 1 (school catalog CRUD) and verify independently.
3. Layer in User Story 2 (grade configuration) to complete P1 scope.
4. Add User Story 3 for system admin oversight once catalog flows are stable.
5. Optionally deliver User Story 4 (P4) after higher-priority stories, keeping UI blocked pending Figma.
6. Finish with Polish tasks, ensuring Red -> Green evidence, coverage, documentation, and phase review readiness.
