# Tasks: Identity Service with Microsoft Entra ID

**Specification Branch**: `CrossCuttingConcerns/001-identity-service-entra-id-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `CrossCuttingConcerns/001-identity-service-entra-id` *(created by `/speckit.implement`)*  
**Feature**: 001-identity-service-entra-id  
**Input**: plan.md, spec.md, data-model.md, research.md, contracts/  

---

## Layer Context (MANDATORY)

**Target Layer**: Foundation service (CrossCuttingConcerns specification)  
**Implementation Path**: `Src/Foundation/services/Identity/`  
**Specification Path**: `Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/{TargetLayer}/...`)
- [ ] Specification path follows layer structure (`Plan/{TargetLayer}/specs/...`)
- [ ] Shared infrastructure dependencies match between spec and plan
- [ ] Cross-layer dependencies (if any) justified in both spec and plan

---

## Layer Compliance Validation

- [ ] T001 Validate Identity service project references only Foundation/shared infrastructure (no upstream layer leaks) in `Src/Foundation/services/Identity/*/*.csproj`
- [ ] T002 Confirm AppHost registration isolates Identity service with Postgres, Redis, Service Bus references in `Src/Foundation/AppHost/Program.cs`
- [ ] T003 Verify README/plan captures shared infrastructure dependencies and layer boundaries in `Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id/plan.md`
- [ ] T004 Check for circular dependencies across layers in solution graph (Identity should not depend on other services)

---

## Identity & Authentication Compliance

- [ ] T005 Ensure no Duende IdentityServer or custom token issuance packages in `Src/Foundation/services/Identity/*/*.csproj`
- [ ] T006 Configure Microsoft.Identity.Web for JWT validation (no custom JWT generation) in `Src/Foundation/services/Identity/Identity.API/Program.cs`
- [ ] T007 Register `SessionAuthenticationHandler` for session-based API authorization in `Identity.API/Middleware/SessionAuthenticationHandler.cs` and DI setup
- [ ] T008 Configure Redis session caching with Aspire.Hosting.Redis in `Identity.Infrastructure/Caching/RedisCacheService.cs`
- [ ] T009 Ensure `sessions` table includes `tenant_id` and TTL fields in `Identity.Infrastructure/Data/Migrations/*`
- [ ] T010 Implement TokenExchangeService (BFF pattern: Entra tokens -> LMS sessions) in `Identity.Application/Commands/ExchangeTokenCommandHandler.cs`
- [ ] T011 Validate auth flow aligns with `Plan/Foundation/docs/legacy-identityserver-migration.md` (no local password storage, Entra-driven MFA)

---

## Format

Tasks use `- [ ] T### [P?] [Story] Description with file path`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish project structure, app configuration, and hosting resources.

- [ ] T012 Run `.specify/scripts/bash/check-prerequisites.sh --json` and record FEATURE_DIR/LAYER in notes for this spec
- [ ] T013 Create solution projects `Identity.API`, `Identity.Application`, `Identity.Domain`, `Identity.Infrastructure` under `Src/Foundation/services/Identity/` with csproj references and add to solution
- [ ] T014 [P] Add baseline appsettings (Entra authorities, audiences, Redis, Postgres, Service Bus) in `Identity.API/appsettings.Development.json`
- [ ] T015 [P] Wire Identity service into Aspire AppHost with Postgres/Redis/Service Bus resources in `Src/Foundation/AppHost/Program.cs`
- [ ] T016 [P] Add CI-friendly launchSettings and developer certificates for Identity.API in `Identity.API/Properties/launchSettings.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that blocks all user stories.

- [ ] T017 [P] Define domain entities and value objects (User, Session, Role, ExternalProviderLink, AuditRecord, SessionId, EntraSubjectId, TenantId) in `Identity.Domain/Entities` and `Identity.Domain/ValueObjects`
- [ ] T018 Configure `IdentityDbContext` with EF Core mappings, global tenant filters, and migrations for all core tables in `Identity.Infrastructure/Data`
- [ ] T019 [P] Implement Redis caching helpers and cache key builders for sessions/claims in `Identity.Infrastructure/Caching`
- [ ] T020 Set up Azure Service Bus publisher and subscription wiring for domain events in `Identity.Infrastructure/Messaging`
- [ ] T021 [P] Establish cross-cutting middleware (exception handling, correlation IDs, request logging) in `Identity.API/Middleware` and DI registration
- [ ] T022 Configure Microsoft.Identity.Web authentication/authorization defaults and health checks in `Identity.API/Program.cs`
- [ ] T023 Create contract/event definitions (`UserAuthenticated`, `UserLoggedOut`, `SessionRefreshed`, `TenantContextSwitched`) in `Identity.Domain/Events`
- [ ] T024 Add baseline observability (structured logging, OpenTelemetry tracing, metrics) in `Identity.API/Program.cs`

---

## Phase 3: User Story 1 - Staff Member Logs In Using Microsoft Entra ID SSO (Priority: P1) ✔ MVP

**Goal**: Staff can complete Entra ID SSO and receive an LMS session cookie for dashboard access.  
**Independent Test**: Test user completes login, `Set-Cookie` issued for session, dashboard loads within 2s.

### Tests (write-first)
- [ ] T025 [P] [US1] Integration test for `/api/auth/exchange-token` returning session cookie and HTTP 200 in `Src/Foundation/services/Identity/tests/Identity.IntegrationTests/Auth/LoginFlowTests.cs`
- [ ] T026 [P] [US1] Contract test validating Entra access token verification (issuer/audience/signature) using Microsoft.Identity.Web test keys in `tests/Identity.ContractTests/Auth/ExchangeTokenContractTests.cs`

### Implementation
- [ ] T027 [P] [US1] Implement `AuthenticationController` `/api/auth/exchange-token` endpoint in `Identity.API/Controllers/AuthenticationController.cs`
- [ ] T028 [US1] Implement `ExchangeTokenCommandHandler` to validate Entra token and map to `Session` creation in `Identity.Application/Commands/ExchangeTokenCommandHandler.cs`
- [ ] T029 [US1] Persist session with sliding expiration to Postgres and Redis via `SessionManager` in `Identity.Infrastructure/Identity/SessionManager.cs`
- [ ] T030 [US1] Write authentication audit record on successful login in `Identity.Infrastructure/Repositories/AuditRepository.cs`

---

## Phase 4: User Story 2 - Administrator Logs In Using Entra ID with MFA (Priority: P1)

**Goal**: Admins authenticate with Entra-required MFA and receive 1-hour sessions.  
**Independent Test**: Admin login requires MFA claim, issues 1-hour session, returns dashboard within 2s.

### Tests (write-first)
- [ ] T031 [P] [US2] Integration test enforcing MFA claim (`amr`/`acr`) presence for admin login in `tests/Identity.IntegrationTests/Auth/AdminMfaTests.cs`
- [ ] T032 [P] [US2] Session duration test ensuring admin session expires at 1 hour in `tests/Identity.IntegrationTests/Sessions/AdminSessionDurationTests.cs`

### Implementation
- [ ] T033 [US2] Extend `ExchangeTokenValidator` to require MFA for admin roles in `Identity.Application/Validators/ExchangeTokenValidator.cs`
- [ ] T034 [US2] Apply admin-specific session duration and hardened cookie settings in `SessionManager` in `Identity.Infrastructure/Identity/SessionManager.cs`
- [ ] T035 [US2] Emit `UserAuthenticatedEvent` with role context for admin logins in `Identity.Domain/Events/UserAuthenticatedEvent.cs`

---

## Phase 5: User Story 4 - Token Refresh and Session Management (Priority: P1)

**Goal**: Refresh tokens transparently extend sessions without interrupting users.  
**Independent Test**: Refresh endpoint renews session before expiry; latency <50ms; no forced logout during active use.

### Tests (write-first)
- [ ] T036 [P] [US4] Integration test for `/api/auth/refresh` validating new session cookie and extended expiry in `tests/Identity.IntegrationTests/Auth/RefreshSessionTests.cs`
- [ ] T037 [P] [US4] Unit test for `TokenRefreshBackgroundService` sliding window behavior in `tests/Identity.UnitTests/Infrastructure/TokenRefreshBackgroundServiceTests.cs`

### Implementation
- [ ] T038 [US4] Implement `RefreshSessionCommandHandler` and API endpoint binding in `Identity.Application/Commands/RefreshSessionCommandHandler.cs` and `Identity.API/Controllers/AuthenticationController.cs`
- [ ] T039 [US4] Update Redis/Postgres session update pipeline for sliding expiration in `Identity.Infrastructure/Identity/SessionManager.cs`
- [ ] T040 [US4] Publish `SessionRefreshedEvent` and log audit trail in `Identity.Domain/Events/SessionRefreshedEvent.cs` and `AuditRepository`

---

## Phase 6: User Story 7 - Role-Based Authorization Check (Priority: P1)

**Goal**: Fast RBAC enforcement using cached roles/claims with <50ms P95 checks.  
**Independent Test**: Protected endpoint returns 403 when role missing; 200 when authorized; checks <50ms.

### Tests (write-first)
- [ ] T041 [P] [US7] Unit test for `AuthorizationService` role evaluation latency and correctness in `tests/Identity.UnitTests/Application/AuthorizationServiceTests.cs`
- [ ] T042 [P] [US7] Integration test for `/api/auth/claims` returning tenant-scoped roles/claims in `tests/Identity.IntegrationTests/Auth/ClaimsEndpointTests.cs`

### Implementation
- [ ] T043 [US7] Implement `AuthorizationService` with cached role/permission lookup in `Identity.Infrastructure/Identity/AuthorizationService.cs`
- [ ] T044 [US7] Cache user claims/roles per tenant with Redis key strategy in `Identity.Infrastructure/Caching/SessionCacheKeyBuilder.cs`
- [ ] T045 [US7] Attach claims principal via `SessionAuthenticationHandler` for downstream authorization in `Identity.API/Middleware/SessionAuthenticationHandler.cs`

---

## Phase 7: User Story 8 - Session Termination and Logout (Priority: P1)

**Goal**: Users can terminate sessions and ensure invalidation across cache and database.  
**Independent Test**: Logout endpoint removes session from Redis/Postgres and blocks reuse immediately.

### Tests (write-first)
- [ ] T046 [P] [US8] Integration test for `/api/auth/logout` invalidating session IDs and returning 204 in `tests/Identity.IntegrationTests/Auth/LogoutTests.cs`
- [ ] T047 [P] [US8] Contract test verifying `UserLoggedOutEvent` published on logout in `tests/Identity.ContractTests/Events/LogoutEventTests.cs`

### Implementation
- [ ] T048 [US8] Implement `LogoutCommandHandler` to revoke session and delete cache entry in `Identity.Application/Commands/LogoutCommandHandler.cs`
- [ ] T049 [US8] Add logout endpoint routing in `Identity.API/Controllers/AuthenticationController.cs`
- [ ] T050 [US8] Emit logout audit record and event in `Identity.Infrastructure/Repositories/AuditRepository.cs` and `Identity.Domain/Events/UserLoggedOutEvent.cs`

---

## Phase 8: User Story 10 - Failed Authentication Handling (Priority: P1)

**Goal**: Secure handling of failed auth with lockout, clear messaging, and auditing.  
**Independent Test**: Invalid token returns 401 with sanitized error; repeated failures trigger lockout and audit entry.

### Tests (write-first)
- [ ] T051 [P] [US10] Integration test for invalid token responses and error shape in `tests/Identity.IntegrationTests/Auth/FailedAuthTests.cs`
- [ ] T052 [P] [US10] Unit test for lockout thresholds and counters in `tests/Identity.UnitTests/Application/LockoutPolicyTests.cs`

### Implementation
- [ ] T053 [US10] Implement lockout/backoff policy in `SessionManager` for repeated failures in `Identity.Infrastructure/Identity/SessionManager.cs`
- [ ] T054 [US10] Add failure audit logging with reason codes in `Identity.Infrastructure/Repositories/AuditRepository.cs`
- [ ] T055 [US10] Expose standardized problem details for auth failures via `ExceptionHandlerMiddleware` in `Identity.API/Middleware/ExceptionHandlerMiddleware.cs`

---

## Phase 9: User Story 3 - Entra ID Configuration and User Provisioning (Priority: P2)

**Goal**: Configure Entra app registrations and provision initial admin users per tenant.  
**Independent Test**: Admin consent and tenant settings stored; initial admin created per tenant without manual password management.

### Tests (write-first)
- [ ] T056 [P] [US3] Integration test loading Entra settings (tenant ID, client ID, scopes) from configuration source in `tests/Identity.IntegrationTests/Configuration/EntraSettingsTests.cs`
- [ ] T057 [P] [US3] Provisioning test ensures seeding creates tenant admin user linked to Entra subject in `tests/Identity.IntegrationTests/Provisioning/AdminProvisioningTests.cs`

### Implementation
- [ ] T058 [US3] Document Entra app registration values and required permissions in `Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id/entra-id-config.md`
- [ ] T059 [US3] Implement configuration loading and validation for Entra settings in `Identity.API/Program.cs` and `Identity.Infrastructure/Identity/EntraIdTokenValidator.cs`
- [ ] T060 [US3] Add provisioning routine to create initial admin and external provider link per tenant in `Identity.Application/Commands/ProvisionAdminsCommandHandler.cs`

---

## Phase 10: User Story 5 - Cross-District Access with Tenant Switching (Priority: P2)

**Goal**: Authorized users switch tenant context quickly (<200ms) without re-authentication.  
**Independent Test**: Tenant switch updates claims and session tenant_id; subsequent requests reflect new tenant context.

### Tests (write-first)
- [ ] T061 [P] [US5] Integration test for `/api/auth/switch-tenant` updating session cache/DB and returning new claims in `tests/Identity.IntegrationTests/Auth/TenantSwitchTests.cs`
- [ ] T062 [P] [US5] Unit test validating tenant switch authorization rules (role/tenant membership) in `tests/Identity.UnitTests/Application/TenantSwitchRulesTests.cs`

### Implementation
- [ ] T063 [US5] Implement `SwitchTenantCommandHandler` to update session tenant and claims in `Identity.Application/Commands/SwitchTenantCommandHandler.cs`
- [ ] T064 [US5] Publish `TenantContextSwitchedEvent` and cache new context in `Identity.Domain/Events/TenantContextSwitchedEvent.cs` and `Identity.Infrastructure/Caching`
- [ ] T065 [US5] Add tenant switch endpoint in `Identity.API/Controllers/AuthorizationController.cs`

---

## Phase 11: User Story 9 - Service-to-Service Authentication (Priority: P2)

**Goal**: Microservices authenticate with client credentials and receive scoped claims.  
**Independent Test**: Service principal tokens validated; unauthorized app ID rejected; authorized calls succeed with service claims.

### Tests (write-first)
- [ ] T066 [P] [US9] Integration test validating client credential token handling for service principals in `tests/Identity.IntegrationTests/Auth/ServicePrincipalTests.cs`
- [ ] T067 [P] [US9] Contract test ensuring app role mapping resolves to service claims in `tests/Identity.ContractTests/Auth/ServiceRoleContractTests.cs`

### Implementation
- [ ] T068 [US9] Extend `EntraIdTokenValidator` to accept service principal audiences and app roles in `Identity.Infrastructure/Identity/EntraIdTokenValidator.cs`
- [ ] T069 [US9] Seed service principal role mappings and cache in `Identity.Infrastructure/Data/Migrations/*` and `Identity.Infrastructure/Caching`
- [ ] T070 [US9] Document service-to-service auth usage for downstream services in `Src/Foundation/services/Identity/README.md`

---

## Phase 12: User Story 6 - Password Reset Flow via Microsoft Entra ID (Priority: P3)

**Goal**: Provide self-service password reset via Entra without local password handling.  
**Independent Test**: Reset endpoint returns Entra reset URL/email trigger; no password data stored locally.

### Tests (write-first)
- [ ] T071 [P] [US6] Integration test for `/api/auth/password-reset` returning Entra SSPR link and not persisting passwords in `tests/Identity.IntegrationTests/Auth/PasswordResetTests.cs`
- [ ] T072 [P] [US6] Audit test confirming reset requests are logged without sensitive data in `tests/Identity.UnitTests/Infrastructure/AuditPasswordResetTests.cs`

### Implementation
- [ ] T073 [US6] Add password reset endpoint relaying Entra SSPR URL/email triggers in `Identity.API/Controllers/AuthenticationController.cs`
- [ ] T074 [US6] Ensure audit logging of reset requests without storing secrets in `Identity.Infrastructure/Repositories/AuditRepository.cs`
- [ ] T075 [US6] Update user-facing documentation for reset flow in `Src/Foundation/services/Identity/README.md`

---

## Phase 13: Polish & Cross-Cutting Concerns

- [ ] T076 [P] Add deployment/runbook updates and diagrams to `Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id/plan.md`
- [ ] T077 [P] Performance tune caching/database indices per P95 targets in `Identity.Infrastructure/Data/Configurations/*`
- [ ] T078 Security hardening review (headers, cookie flags, CORS) in `Identity.API/Program.cs`
- [ ] T079 [P] Final regression of independent tests across all user stories in `Src/Foundation/services/Identity/tests/`
- [ ] T080 Run quickstart/validation checklist and summarize in `tasks.md` notes

---

## Dependencies & Execution Order

- Setup (Phase 1) → Foundational (Phase 2) → User Stories (Phases 3-12) → Polish (Phase 13)
- User Stories priority order: US1 (P1) → US2 (P1) → US4 (P1) → US7 (P1) → US8 (P1) → US10 (P1) → US3 (P2) → US5 (P2) → US9 (P2) → US6 (P3)
- Within each story: tests → models/services → endpoints → events/audit

## Parallel Execution Examples

- After Phase 2, teams can parallelize US1/US2/US4 (distinct controllers/handlers) with coordinated changes in shared SessionManager.
- Tests marked [P] can run concurrently (separate files/namespaces).

## Implementation Strategy

- MVP: Complete Phases 1-5 to deliver staff/admin login and session refresh.
- Incremental: Add authorization, logout, failed auth, then P2 tenant switching/service principals, then P3 password reset.

---
