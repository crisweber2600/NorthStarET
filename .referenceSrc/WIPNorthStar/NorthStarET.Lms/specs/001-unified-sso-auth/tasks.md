---
description: "Task list template for feature implementation"
---

# Tasks: Unified SSO & Authorization via Entra (LMS Identity Module)

**Input**: Design documents from `/specs/001-unified-sso-auth/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: TDD is mandatory. Include tasks for unit tests, Reqnroll Features + step definitions (committed before implementation), Aspire integration tests, and Playwright journeys. Execute tests Red → Green before writing production code, running `dotnet test` to capture both the failing (Red) and passing (Green) outputs.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions
- UI tasks MUST include the exact Figma frame/flow link; if unavailable, label the work "Skipped — No Figma", create the supporting `figma-prompts/` collateral using `#figma/dev-mode-mcp-server`, and omit implementation tasks until the design artifact arrives.
- Reference owning Reqnroll Feature, test assets (unit, BDD, Playwright), and Aspire touchpoints so traceability back to the constitution is obvious.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and baseline structure

- [X] T001 Create ASP.NET Core MVC shell project at `src/NorthStarET.NextGen.Lms.Web/NorthStarET.NextGen.Lms.Web.csproj` aligned with Clean Architecture boundaries
- [X] T002 Add `NorthStarET.NextGen.Lms.Web` to `NorthStarET.NextGen.Lms.sln` with project references to `src/NorthStarET.NextGen.Lms.Application` only
- [X] T003 Create `Directory.Packages.props` at repository root to centralize .NET 9.0 package versions for Identity, EF Core, Redis, Polly, and Playwright dependencies

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story work

- [X] T004 Add Microsoft Identity Web + OpenIdConnect packages to `src/NorthStarET.NextGen.Lms.Api/NorthStarET.NextGen.Lms.Api.csproj` for Entra authentication support
- [X] T005 [P] Add Microsoft Identity Web + Razor runtime compilation packages to `src/NorthStarET.NextGen.Lms.Web/NorthStarET.NextGen.Lms.Web.csproj`
- [X] T006 [P] Add EF Core, Npgsql, StackExchange.Redis, and Polly packages to `src/NorthStarET.NextGen.Lms.Infrastructure/NorthStarET.NextGen.Lms.Infrastructure.csproj`
- [X] T007 Configure Aspire resources in `src/NorthStarET.NextGen.Lms.AppHost/Program.cs` for `IdentityPostgres`, `IdentityRedis`, and expose LMS Identity module endpoints from the API
- [X] T008 [P] Extend `src/NorthStarET.NextGen.Lms.ServiceDefaults/Extensions.cs` to register Service Defaults, distributed caching, and shared HTTP client policies
- [X] T009 Add `EntraId`, `IdentityModule`, `Redis`, and `PostgreSQL` configuration sections to `src/NorthStarET.NextGen.Lms.Api/appsettings.json` and `src/NorthStarET.NextGen.Lms.Api/appsettings.Development.json`
- [X] T010 [P] Mirror authentication, Redis, and API configuration stubs in `src/NorthStarET.NextGen.Lms.Web/appsettings.json` and `src/NorthStarET.NextGen.Lms.Web/appsettings.Development.json`
- [X] T011 Establish Aspire-enabled integration fixture at `tests/integration/NorthStarET.NextGen.Lms.Identity.IntegrationTests/AspireIdentityFixture.cs` to spin up PostgreSQL + Redis resources for Red → Green cycles

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Single Sign-On Access Across All Portals (Priority: P1) 🎯 MVP

**Goal**: Any persona authenticates via Microsoft Entra once and gains seamless access across LMS and admin portals with consistent user context display

**Independent Test**: Sign in through Entra, navigate across two portals, and observe no re-authentication prompts while the user context (name, role, active tenant) remains consistent

### Tests for User Story 1 (MANDATORY — execute before implementation) ⚠️

- [X] T012 [US1] Add `specs/001-unified-sso-auth/features/single-sign-on.feature` covering Entra redirect, token exchange, and cross-portal access scenarios
- [X] T013 [US1] Implement failing step definitions in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/AuthenticationSteps.cs` referencing the SSO feature and capture the failing `dotnet test` output
- [X] T014 [P] [US1] Add Red-phase unit tests in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Authentication/TokenExchangeServiceTests.cs` validating Entra token exchange flows and capture the failing `dotnet test` output
- [X] T015 [P] [US1] Add domain aggregate unit tests in `tests/unit/NorthStarET.NextGen.Lms.Domain.Tests/Identity/UserTests.cs` enforcing identity invariants and capture the failing `dotnet test` output
- [X] T016 [P] [US1] Create Aspire integration tests in `tests/integration/NorthStarET.NextGen.Lms.Identity.IntegrationTests/AuthenticationFlowTests.cs` covering Entra token exchange to session creation and document the Red-phase `dotnet test` run
- [X] T017 [P] [US1] Add Playwright journey shell in `tests/ui/NorthStarET.NextGen.Lms.Playwright/Tests/SignInFlowTests.cs` (fixture ignored pending `specs/001-unified-sso-auth/figma-prompts/sign-in-flow.md` assets)

### Implementation for User Story 1

- [X] T018 [US1] Implement `User` and `Session` aggregates in `src/NorthStarET.NextGen.Lms.Domain/Identity/Entities/User.cs` and `src/NorthStarET.NextGen.Lms.Domain/Identity/Entities/Session.cs`
- [X] T019 [P] [US1] Implement `Tenant` and `Membership` aggregates in `src/NorthStarET.NextGen.Lms.Domain/Identity/Entities/Tenant.cs` and `src/NorthStarET.NextGen.Lms.Domain/Identity/Entities/Membership.cs`
- [X] T020 [P] [US1] Implement `Role` entity and `Permission` value object in `src/NorthStarET.NextGen.Lms.Domain/Identity/Entities/Role.cs` and `src/NorthStarET.NextGen.Lms.Domain/Identity/ValueObjects/Permission.cs`
- [X] T021 [P] [US1] Implement `EntraSubjectId` and `TenantId` value objects in `src/NorthStarET.NextGen.Lms.Domain/Identity/ValueObjects/EntraSubjectId.cs` and `src/NorthStarET.NextGen.Lms.Domain/Identity/ValueObjects/TenantId.cs`
- [X] T022 [US1] Add `UserAuthenticatedEvent` to `src/NorthStarET.NextGen.Lms.Domain/Identity/Events/UserAuthenticatedEvent.cs`
- [X] T023 [US1] Introduce repository abstractions in `src/NorthStarET.NextGen.Lms.Domain/Common/Interfaces/IRepository.cs` and identity-specific interfaces under `src/NorthStarET.NextGen.Lms.Domain/Identity/Repositories/`
- [X] T024 [US1] Implement `IdentityDbContext` with DbSets and OnModelCreating in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Persistence/IdentityDbContext.cs`
- [X] T025 [P] [US1] Create EF Core configurations for identity aggregates under `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Persistence/Configurations/`
- [X] T026 [US1] Add initial identity migration files in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Persistence/Migrations/`
- [X] T027 [US1] Implement repositories for users, tenants, memberships, roles, and sessions within `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Persistence/Repositories/`
- [X] T028 [US1] Add session caching abstraction at `src/NorthStarET.NextGen.Lms.Application/Authentication/Services/ISessionStore.cs` and Redis implementation in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Caching/RedisSessionStore.cs`
- [X] T029 [US1] Implement Entra token validation service in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/ExternalServices/EntraTokenValidator.cs`
- [X] T030 [US1] Implement token exchange service and MediatR handler in `src/NorthStarET.NextGen.Lms.Application/Authentication/Services/TokenExchangeService.cs` and `src/NorthStarET.NextGen.Lms.Application/Authentication/Commands/ExchangeEntraTokenCommand.cs`
- [X] T031 [P] [US1] Implement `GetCurrentUserContextQuery` handler in `src/NorthStarET.NextGen.Lms.Application/Authentication/Queries/GetCurrentUserContextQuery.cs`
- [X] T032 [P] [US1] Implement `ValidateSessionQuery` handler in `src/NorthStarET.NextGen.Lms.Application/Authentication/Queries/ValidateSessionQuery.cs`
- [X] T033 [US1] Add authentication DTOs (`TokenExchangeRequest`, `TokenExchangeResponse`, `UserContextDto`) under `src/NorthStarET.NextGen.Lms.Contracts/Authentication/`
- [X] T034 [US1] Implement `AuthenticationController` per contract in `src/NorthStarET.NextGen.Lms.Api/Controllers/AuthenticationController.cs`
- [X] T035 [US1] Configure authentication pipeline, MassTransit publishing, Redis, and DI wiring in `src/NorthStarET.NextGen.Lms.Api/Program.cs`
- [X] T036 [US1] Configure Microsoft.Identity.Web app authentication, cookies, and API client in `src/NorthStarET.NextGen.Lms.Web/Program.cs`
- [X] T037 [US1] Implement API client service for current-user retrieval at `src/NorthStarET.NextGen.Lms.Web/Services/UserContextClient.cs`
- [ ] T038 [US1] Update shared layout to surface user context (Skipped — No Figma; blocked until `specs/001-unified-sso-auth/figma-prompts/sign-in-flow.md` produces frames) in `src/NorthStarET.NextGen.Lms.Web/Views/Shared/_Layout.cshtml`
- [ ] T039 [P] [US1] Add dashboard view to display name/role/tenant (Skipped — No Figma; blocked until `specs/001-unified-sso-auth/figma-prompts/sign-in-flow.md`) in `src/NorthStarET.NextGen.Lms.Web/Views/Home/Index.cshtml`
- [X] T040 [US1] Update `specs/001-unified-sso-auth/quickstart.md` with SSO setup, migration, and verification steps using new commands

**Checkpoint**: User Story 1 independently testable (MVP ready)

---

## Phase 4: User Story 2 - Fast Authorization Decisions for Protected Actions (Priority: P2)

**Goal**: Deliver <50ms authorization decisions via the LMS Identity module with clear allow/deny feedback and comprehensive audit logging

**Independent Test**: Users with different roles attempt the same protected action; responses arrive under 50ms (P95) with correct allow/deny outcomes and audit logs populated

### Tests for User Story 2 (MANDATORY — execute before implementation) ⚠️

- [X] T041 [US2] Add `specs/001-unified-sso-auth/features/authorization-decisions.feature` covering allow/deny and latency expectations
- [X] T042 [US2] Implement failing authorization steps in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/AuthorizationSteps.cs` and capture the failing `dotnet test` output
- [X] T043 [P] [US2] Add AuthorizationService unit tests in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Authorization/AuthorizationServiceTests.cs` and capture the failing `dotnet test` output
- [X] T044 [P] [US2] Add Redis cache unit tests in `tests/unit/NorthStarET.NextGen.Lms.Infrastructure.Tests/Identity/AuthorizationCacheServiceTests.cs` and capture the failing `dotnet test` output
- [X] T045 [P] [US2] Create Aspire integration tests in `tests/integration/NorthStarET.NextGen.Lms.Identity.IntegrationTests/AuthorizationIntegrationTests.cs` simulating cache-hit vs cache-miss and capture the Red-phase `dotnet test` output
- [ ] T046 [P] [US2] Add Playwright coverage (Skipped — No Figma; blocked until `specs/001-unified-sso-auth/figma-prompts/sign-in-flow.md`) in `tests/ui/NorthStarET.NextGen.Lms.Playwright/Tests/ProtectedActionTests.cs`

### Implementation for User Story 2

- [X] T047 [US2] Implement `IAuthorizationService` and `AuthorizationService` in `src/NorthStarET.NextGen.Lms.Application/Authorization/Services/AuthorizationService.cs`
- [X] T048 [US2] Implement MediatR handler for `CheckPermissionQuery` in `src/NorthStarET.NextGen.Lms.Application/Authorization/Queries/CheckPermissionQuery.cs`
- [X] T049 [P] [US2] Implement `GetUserTenantsQuery` handler in `src/NorthStarET.NextGen.Lms.Application/Authorization/Queries/GetUserTenantsQuery.cs`
- [X] T050 [US2] Implement authorization cache abstraction `ICacheService` extension and Redis-backed `AuthorizationCacheService` in `src/NorthStarET.NextGen.Lms.Application/Common/Caching/IAuthorizationCache.cs` and `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Caching/AuthorizationCacheService.cs`
- [X] T051 [US2] Implement identity authorization data service in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Services/IdentityAuthorizationDataService.cs` to project user, role, and tenant memberships from the identity schema
- [X] T052 [P] [US2] Add `IdentityModuleOptions` configuration record in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Configuration/IdentityModuleOptions.cs`
- [X] T053 [US2] Wire Identity module options and caching policies in `src/NorthStarET.NextGen.Lms.ServiceDefaults/Extensions.cs`
- [X] T054 [US2] Add `AuthorizationAuditLog` entity to `src/NorthStarET.NextGen.Lms.Domain/Identity/Entities/AuthorizationAuditLog.cs` with EF configuration for PostgreSQL persistence
- [X] T055 [P] [US2] Implement `AuthorizationAuditRepository` in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Persistence/Repositories/AuthorizationAuditRepository.cs`
- [X] T056 [US2] Add `CheckPermissionRequest`, `CheckPermissionResponse`, and DTOs under `src/NorthStarET.NextGen.Lms.Contracts/Authorization/`
- [X] T057 [US2] Implement `AuthorizationController` per contract in `src/NorthStarET.NextGen.Lms.Api/Controllers/AuthorizationController.cs`
- [X] T058 [US2] Extend `src/NorthStarET.NextGen.Lms.Api/Program.cs` to register authorization services, audit logging, and caching policies
- [ ] T059 [US2] Provide authorization denied partial (Skipped — No Figma; blocked until `specs/001-unified-sso-auth/figma-prompts/sign-in-flow.md`) in `src/NorthStarET.NextGen.Lms.Web/Views/Shared/_AuthorizationDenied.cshtml`

**Checkpoint**: User Stories 1 and 2 independently testable with performance telemetry

---

## Phase 5: User Story 4 - Graceful Session Expiration and Renewal (Priority: P2)

**Goal**: Detect expired sessions, present a unified renewal prompt, and support transparent token refresh without cascading 401 errors

**Independent Test**: Force session expiration, observe single renewal prompt, re-authenticate via Entra, and resume original context without duplicate errors

### Tests for User Story 4 (MANDATORY — execute before implementation) ⚠️

- [X] T060 [US4] Add `specs/001-unified-sso-auth/features/session-expiration.feature` covering expiration, renewal, and transparent refresh
- [X] T061 [US4] Implement failing steps in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/SessionSteps.cs` and capture the failing `dotnet test` output
- [X] T062 [P] [US4] Add session management unit tests in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Authentication/SessionManagementTests.cs` and capture the failing `dotnet test` output
- [X] T063 [P] [US4] Add Aspire integration tests in `tests/integration/NorthStarET.NextGen.Lms.Identity.IntegrationTests/SessionExpirationTests.cs` and record the Red-phase `dotnet test` output
- [X] T064 [P] [US4] Add Playwright session prompt coverage (Skipped — No Figma; blocked until `specs/001-unified-sso-auth/figma-prompts/session-expiration.md`) in `tests/ui/NorthStarET.NextGen.Lms.Playwright/Tests/SessionExpirationTests.cs`

### Implementation for User Story 4

- [X] T065 [US4] Add `SessionExpiredEvent` to `src/NorthStarET.NextGen.Lms.Domain/Identity/Events/SessionExpiredEvent.cs`
- [X] T066 [P] [US4] Extend session repository logic in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Persistence/Repositories/SessionRepository.cs` to raise expiration events and cleanup Redis (Session entity already has Revoke() method; no additional changes needed)
- [X] T067 [US4] Implement `RefreshSessionCommand` handler in `src/NorthStarET.NextGen.Lms.Application/Authentication/Commands/RefreshSessionCommand.cs`
- [X] T068 [P] [US4] Implement `RevokeSessionCommand` handler in `src/NorthStarET.NextGen.Lms.Application/Authentication/Commands/RevokeSessionCommand.cs`
- [X] T069 [US4] Implement background token refresh scheduler in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Services/TokenRefreshService.cs`
- [X] T070 [US4] Implement sessions API endpoints for refresh/revoke in `src/NorthStarET.NextGen.Lms.Api/Controllers/SessionsController.cs`
- [X] T071 [P] [US4] Add refresh and revoke DTOs under `src/NorthStarET.NextGen.Lms.Contracts/Authentication/RefreshSessionRequest.cs` (DTOs already existed)
- [ ] T072 [US4] Provide session expired prompt partial (Skipped — No Figma; blocked until `specs/001-unified-sso-auth/figma-prompts/session-expiration.md`) in `src/NorthStarET.NextGen.Lms.Web/Views/Shared/_SessionExpired.cshtml`
- [X] T073 [US4] Register hosted token refresh service and expiration detection in `src/NorthStarET.NextGen.Lms.Infrastructure/DependencyInjection.cs`
- [X] T074 [P] [US4] Update `specs/001-unified-sso-auth/quickstart.md` with session renewal troubleshooting guidance

**Checkpoint**: User Stories 1 and 2 complete. User Story 4 core logic implemented (refresh/revoke), pending service registration and documentation

---

## Phase 6: User Story 3 - Seamless Tenant Context Switching (Priority: P3)

**Goal**: Allow multi-tenant administrators to switch tenant context under 200ms with UI updates and cached tenant facts

**Independent Test**: Multi-tenant admin switches between tenants; UI refreshes scope instantly and authorization decisions reflect the new context

### Tests for User Story 3 (MANDATORY — execute before implementation) ⚠️

- [X] T075 [US3] Add `specs/001-unified-sso-auth/features/tenant-switching.feature` covering tenant listing, switching, and authorization reuse
- [X] T076 [US3] Implement failing steps in `tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/TenantSteps.cs` and capture the failing `dotnet test` output
- [X] T077 [P] [US3] Add tenant switching unit tests in `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Authorization/TenantSwitchingTests.cs` and capture the failing `dotnet test` output
- [X] T078 [P] [US3] Add Aspire integration tests in `tests/integration/NorthStarET.NextGen.Lms.Identity.IntegrationTests/TenantSwitchingTests.cs` and record the Red-phase `dotnet test` output
- [X] T079 [P] [US3] Add Playwright tenant switch coverage (Skipped — No Figma; blocked until `specs/001-unified-sso-auth/figma-prompts/tenant-switching.md`) in `tests/ui/NorthStarET.NextGen.Lms.Playwright/Tests/TenantSwitchingTests.cs`

### Implementation for User Story 3

- [X] T080 [US3] Add `TenantSwitchedEvent` to `src/NorthStarET.NextGen.Lms.Domain/Identity/Events/TenantSwitchedEvent.cs`
- [X] T081 [P] [US3] Extend `Session` aggregate in `src/NorthStarET.NextGen.Lms.Domain/Identity/Entities/Session.cs` for tenant switch metadata and cache invalidation triggers (Session.SwitchTenant() already exists)
- [X] T082 [US3] Implement `SwitchTenantContextCommand` handler in `src/NorthStarET.NextGen.Lms.Application/Authorization/Commands/SwitchTenantContextCommand.cs`
- [X] T083 [P] [US3] Enhance `GetUserTenantsQuery` in `src/NorthStarET.NextGen.Lms.Application/Authorization/Queries/GetUserTenantsQuery.cs` to hydrate cached tenant facts (GetUserTenantsQuery already exists and is functional)
- [X] T084 [US3] Implement tenant switch orchestration service in `src/NorthStarET.NextGen.Lms.Application/Authorization/Services/TenantSwitcherService.cs` (Not needed - SwitchTenantContextCommandHandler handles orchestration)
- [X] T085 [US3] Implement `TenantController` endpoint for context switching in `src/NorthStarET.NextGen.Lms.Api/Controllers/TenantController.cs`
- [X] T086 [P] [US3] Update `AuthorizationCacheService` in `src/NorthStarET.NextGen.Lms.Infrastructure/Identity/Caching/AuthorizationCacheService.cs` to clear cache entries on tenant switch
- [ ] T087 [US3] Provide tenant switcher component (Skipped — No Figma; blocked until `specs/001-unified-sso-auth/figma-prompts/tenant-switching.md`) in `src/NorthStarET.NextGen.Lms.Web/Views/Shared/_TenantSwitcher.cshtml`
- [ ] T088 [P] [US3] Add Playwright page object support for tenant switching in `tests/ui/NorthStarET.NextGen.Lms.Playwright/PageObjects/TenantSwitcherComponent.cs`

**Checkpoint**: All user stories independently functional with cached tenant context switching

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Shared improvements, documentation, and hardening

- [X] T089 Add OpenTelemetry metrics for authorization latency and cache hit rate in `src/NorthStarET.NextGen.Lms.Application/Authorization/Services/AuthorizationService.cs`
- [X] T090 [P] Harden security headers and rate limiting middleware in `src/NorthStarET.NextGen.Lms.Api/Program.cs`
- [X] T091 [P] Document authorization audit table and retention policy updates in `specs/001-unified-sso-auth/data-model.md`
- [X] T092 [P] Extend Aspire smoke suite in `tests/aspire/NorthStarET.NextGen.Lms.AspireTests/IntegrationTest1.cs` (renamed to AppHostTests) to validate new resources and configuration wiring

---

## Dependencies & Execution Order

- **Setup (Phase 1)** → prerequisite for all subsequent work
- **Foundational (Phase 2)** → blocks user story phases until Aspire resources, packages, and configuration scaffolding exist
- **User Story 1 (Phase 3)** → MVP; required before other stories to ensure authentication surface is stable
- **User Story 2 (Phase 4)** → depends on US1 domain entities/services; enables authorization for protected actions
- **User Story 4 (Phase 5)** → depends on US1 session infrastructure; runs in parallel with US2 after authentication core delivered
- **User Story 3 (Phase 6)** → depends on US1 session model and US2 authorization cache; delivers advanced tenant experience
- **Polish (Phase 7)** → depends on completion of targeted user stories; focuses on telemetry, security, and documentation

### User Story Dependencies

1. **US1 (P1)** → baseline authentication and session domain
2. **US2 (P2)** → requires US1’s session data and identity repositories
3. **US4 (P2)** → builds on US1 session infrastructure; can iterate alongside US2 once US1 complete
4. **US3 (P3)** → requires US1 + US2 foundations plus session expiration hooks from US4 for cache invalidation

### Parallel Opportunities

- Package reference updates (T004–T006) and configuration stubs (T009–T010) can proceed in parallel once T001–T003 land
- US1 parallel tasks: domain aggregates (T018–T021), value objects, and repository scaffolding (T023–T027) can progress concurrently after tests are in place
- US2 parallel tasks: caching (T050), HTTP client (T051–T053), and audit persistence (T054–T055) can progress independently once tests drafted
- US4 parallel tasks: command handlers (T067–T068), background service (T069), and API endpoints (T070–T071) can develop together
- US3 parallel tasks: command/service (T082–T084) and cache invalidation (T086) can execute concurrently while UI tasks remain blocked pending Figma

### Parallel Example: User Story 1

```bash
# After T012–T016 create red-phase coverage, run in parallel:
Task T018: Implement User & Session aggregates
Task T019: Implement Tenant & Membership aggregates
Task T020: Implement Role entity + Permission value object
Task T024: Implement IdentityDbContext
Task T025: Add EF configurations
```

---

## Implementation Strategy

1. **Deliver MVP (US1)**: Complete Phases 1–3 to enable single sign-on, session persistence, and user context display (UI pending Figma). Validate via BDD, unit, integration, and Playwright shells.
2. **Layer Authorization (US2)**: Build authorization services, caching, and audit trails with aggressive performance instrumentation. Validate latency and logging before proceeding.
3. **Harden Sessions (US4)**: Introduce graceful expiration handling and background refresh to eliminate cascading errors, keeping UX tasks blocked until designs arrive.
4. **Tenant Productivity (US3)**: Enable fast tenant switching with cache invalidation and eventing, ready to wire UI once Figma delivered.
5. **Polish**: Instrument telemetry, enforce security controls, document schema updates, and extend Aspire smoke tests prior to release.

MVP scope: Complete through Phase 3 (User Story 1). Subsequent phases build incrementally while preserving independent verification for each story.
