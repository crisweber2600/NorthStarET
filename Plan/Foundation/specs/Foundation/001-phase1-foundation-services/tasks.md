---
description: "Dependency-ordered tasks for Phase 1 Foundation Services implementation"
feature: "001-phase1-foundation-services"
timeline: "8 weeks"
---

# Tasks: Phase 1 Foundation Services

**Feature**: Identity & Authentication, API Gateway (YARP), Configuration Service  
**Timeline**: 8 weeks (Weeks 1-8)  
**Constitutional Requirements**: Clean Architecture, TDD Red→Green workflow, ≥80% test coverage, .NET Aspire orchestration

**Input Documents**:
- Plans/MASTER_MIGRATION_PLAN.md (Phase 1 requirements)
- Plans/microservices/services/identity-service.md
- Plans/microservices/services/configuration-service.md
- Plans/microservices/docs/api-gateway-config.md
- Src/WIPNorthStar/NorthStarET.Lms/ (implementation patterns)

**Organization**: Tasks follow TDD workflow (Red→Green→Refactor) and are ordered by dependencies to enable incremental delivery.

## Format: `- [ ] [ID] [P?] Description with file path`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- All tasks include exact file paths
- Tasks are 2-4 hours each for focused implementation

---

## Phase 1: Aspire Infrastructure Setup (Week 1)

**Purpose**: Establish .NET Aspire orchestration foundation for all microservices

**Acceptance Criteria**: Aspire AppHost successfully orchestrates service discovery, health checks, and observability

### Week 1, Days 1-2: Aspire AppHost Project

- [ ] T001 Create NewDesign/NorthStarET.Lms solution structure matching UpgradedNorthStar target
- [ ] T002 Create Aspire AppHost project at NewDesign/NorthStarET.Lms/src/NorthStar.AppHost/
- [ ] T003 Configure Aspire.Hosting NuGet packages (Microsoft.NET.Sdk.Aspire) in AppHost
- [ ] T004 Create ServiceDefaults project at NewDesign/NorthStarET.Lms/src/NorthStar.ServiceDefaults/
- [ ] T005 [P] Implement health check middleware in NorthStar.ServiceDefaults/Extensions/HealthCheckExtensions.cs
- [ ] T006 [P] Implement OpenTelemetry configuration in NorthStar.ServiceDefaults/Extensions/ObservabilityExtensions.cs
- [ ] T007 [P] Implement structured logging in NorthStar.ServiceDefaults/Extensions/LoggingExtensions.cs

### Week 1, Days 3-5: Shared Infrastructure Services

- [ ] T008 Configure PostgreSQL resource in AppHost Program.cs for multi-service database
- [ ] T009 Configure Redis resource in AppHost Program.cs for distributed caching
- [ ] T010 Configure Azure Service Bus resource in AppHost Program.cs for event messaging
- [ ] T011 Create Docker Compose override for local development at NewDesign/NorthStarET.Lms/docker-compose.override.yml
- [ ] T012 Configure Azure Key Vault integration in ServiceDefaults for secrets management
- [ ] T013 [P] Create Aspire integration test project at tests/NorthStar.Aspire.Tests/
- [ ] T014 Write Aspire resource discovery test in tests/NorthStar.Aspire.Tests/ResourceDiscoveryTests.cs (RED)
- [ ] T015 Implement Aspire resource registration to make test GREEN
- [ ] T016 Document Aspire setup in NewDesign/NorthStarET.Lms/docs/aspire-setup.md

**Checkpoint**: Aspire AppHost starts successfully with infrastructure services (PostgreSQL, Redis, Service Bus)

---

## Phase 2: Identity Service - Domain Layer (Week 2)

**Purpose**: Implement Identity Service domain entities and business logic following Clean Architecture

**Acceptance Criteria**: Domain layer compiles with ≥80% unit test coverage for business rules

### Week 2, Days 1-2: Identity Domain Project Setup

- [ ] T017 Create Identity.Domain project at NewDesign/NorthStarET.Lms/src/services/Identity/Identity.Domain/
- [ ] T018 Create Identity.UnitTests project at NewDesign/NorthStarET.Lms/tests/Identity.UnitTests/
- [ ] T019 Configure project references and dependencies (no Infrastructure dependencies)
- [ ] T020 Create domain folder structure: Entities/, Events/, ValueObjects/, Aggregates/

### Week 2, Days 2-3: Core Domain Entities (TDD)

- [ ] T021 Write failing unit test for User entity creation in tests/Identity.UnitTests/Domain/UserTests.cs (RED)
- [ ] T022 Implement User entity in Identity.Domain/Entities/User.cs to make test GREEN
- [ ] T023 Write failing unit test for password validation business rules (RED)
- [ ] T024 Implement password validation in User entity to make test GREEN
- [ ] T025 [P] Write failing unit test for Role entity in tests/Identity.UnitTests/Domain/RoleTests.cs (RED)
- [ ] T026 [P] Implement Role entity in Identity.Domain/Entities/Role.cs to make test GREEN
- [ ] T027 [P] Write failing unit test for RefreshToken lifecycle (RED)
- [ ] T028 [P] Implement RefreshToken entity in Identity.Domain/Entities/RefreshToken.cs to make test GREEN
- [ ] T029 [P] Implement ExternalProvider entity in Identity.Domain/Entities/ExternalProvider.cs for Entra ID B2B/B2C linkage

### Week 2, Days 3-4: Session, Membership, Invitation Entities (TDD - NEW for B2B/B2C)

- [ ] T029a Write failing unit test for Session entity in tests/Identity.UnitTests/Domain/SessionTests.cs (RED)
- [ ] T029b Implement Session entity in Identity.Domain/Entities/Session.cs with ActiveTenantId, EntraTokenHash, LMSTokenMetadata (GREEN)
- [ ] T029c Write test for Session.SwitchTenant(targetTenantId) method (RED)
- [ ] T029d Implement SwitchTenant method with validation (GREEN)
- [ ] T029e Write failing unit test for Membership entity in tests/Identity.UnitTests/Domain/MembershipTests.cs (RED)
- [ ] T029f Implement Membership entity in Identity.Domain/Entities/Membership.cs with user-tenant-role association (GREEN)
- [ ] T029g Write test for Membership.Verify() and Membership.Revoke() methods (RED)
- [ ] T029h Implement Verify/Revoke methods with status transitions (GREEN)
- [ ] T029i Write failing unit test for Invitation entity in tests/Identity.UnitTests/Domain/InvitationTests.cs (RED)
- [ ] T029j Implement Invitation entity in Identity.Domain/Entities/Invitation.cs with secure token, 7-day expiration, 10-min idempotency (GREEN)
- [ ] T029k Write test for Invitation.Accept() method and expiration validation (RED)
- [ ] T029l Implement Accept method with expiration checks and single-use token pattern (GREEN)

### Week 2, Days 4-5: Domain Events and Value Objects (TDD)

- [ ] T030 Write test for UserRegisteredEvent in tests/Identity.UnitTests/Events/UserRegisteredEventTests.cs (RED)
- [ ] T031 Implement UserRegisteredEvent in Identity.Domain/Events/UserRegisteredEvent.cs (GREEN)
- [ ] T032 [P] Implement UserLoggedInEvent in Identity.Domain/Events/UserLoggedInEvent.cs
- [ ] T033 [P] Implement PasswordChangedEvent in Identity.Domain/Events/PasswordChangedEvent.cs
- [ ] T034 [P] Implement UserRoleChangedEvent in Identity.Domain/Events/UserRoleChangedEvent.cs
- [ ] T034a [P] Implement TenantContextSwitchedEvent in Identity.Domain/Events/TenantContextSwitchedEvent.cs
- [ ] T034b [P] Implement MembershipInvitedEvent in Identity.Domain/Events/MembershipInvitedEvent.cs
- [ ] T034c [P] Implement MembershipVerifiedEvent in Identity.Domain/Events/MembershipVerifiedEvent.cs
- [ ] T034d [P] Implement MembershipRevokedEvent in Identity.Domain/Events/MembershipRevokedEvent.cs
- [ ] T034e [P] Implement InvitationSentEvent in Identity.Domain/Events/InvitationSentEvent.cs
- [ ] T034f [P] Implement InvitationAcceptedEvent in Identity.Domain/Events/InvitationAcceptedEvent.cs
- [ ] T035 Write test for Email value object validation (RED)
- [ ] T036 Implement Email value object in Identity.Domain/ValueObjects/Email.cs (GREEN)
- [ ] T037 [P] Implement PasswordHash value object in Identity.Domain/ValueObjects/PasswordHash.cs
- [ ] T038 Capture test evidence: dotnet test > evidence/identity-domain-red.txt and identity-domain-green.txt

**Checkpoint**: Identity Domain layer complete with ≥80% test coverage verified

---

## Phase 3: Identity Service - Application Layer (Week 3)

**Purpose**: Implement CQRS commands and queries for Identity Service

**Acceptance Criteria**: Application layer handlers tested with unit tests, MediatR integration validated

### Week 3, Days 1-2: Application Project and CQRS Setup

- [ ] T039 Create Identity.Application project at NewDesign/NorthStarET.Lms/src/services/Identity/Identity.Application/
- [ ] T040 Add MediatR NuGet package and configure DI in Application layer
- [ ] T041 Create folder structure: Commands/, Queries/, DTOs/, Interfaces/, Behaviors/
- [ ] T042 Create Identity.IntegrationTests project at tests/Identity.IntegrationTests/

### Week 3, Days 2-3: User Registration Command (TDD)

- [ ] T043 Write failing integration test for RegisterUserCommand in tests/Identity.IntegrationTests/Commands/RegisterUserCommandTests.cs (RED)
- [ ] T044 Create RegisterUserCommand in Identity.Application/Commands/RegisterUser/RegisterUserCommand.cs
- [ ] T045 Create RegisterUserCommandHandler in Identity.Application/Commands/RegisterUser/RegisterUserCommandHandler.cs
- [ ] T046 Implement command validation using FluentValidation in RegisterUserCommandValidator.cs
- [ ] T047 Run test to verify GREEN state, refactor handler
- [ ] T048 Write unit test for duplicate email validation (RED)
- [ ] T049 Implement idempotency logic (10-minute window) to make test GREEN

### Week 3, Day 3: Token Exchange Command (NEW for B2B/B2C Pattern)

- [ ] T049a Write failing test for ExchangeEntraTokenCommand in tests/Identity.IntegrationTests/Commands/ExchangeEntraTokenCommandTests.cs (RED)
- [ ] T049b Create ExchangeEntraTokenCommand in Identity.Application/Commands/ExchangeEntraToken/ExchangeEntraTokenCommand.cs
- [ ] T049c Create ExchangeEntraTokenCommandHandler in Identity.Application/Commands/ExchangeEntraToken/ExchangeEntraTokenCommandHandler.cs
- [ ] T049d Create ITokenExchangeService interface in Identity.Application/Authentication/Services/ITokenExchangeService.cs
- [ ] T049e Create TokenExchangeCommandContext DTO with EntraToken, ActiveTenantId, IpAddress, UserAgent
- [ ] T049f Create TokenExchangeResult DTO with LMS custom JWT and session info
- [ ] T049g Implement handler to delegate to ITokenExchangeService (GREEN)
- [ ] T049h Write test for token exchange with invalid Entra token (RED)
- [ ] T049i Implement validation and error handling (GREEN)

### Week 3, Days 3-4: Authentication Query (TDD)

- [ ] T050 Write failing test for AuthenticateUserQuery in tests/Identity.IntegrationTests/Queries/AuthenticateUserQueryTests.cs (RED)
- [ ] T051 Create AuthenticateUserQuery in Identity.Application/Queries/AuthenticateUser/AuthenticateUserQuery.cs
- [ ] T052 Create AuthenticateUserQueryHandler in Identity.Application/Queries/AuthenticateUser/AuthenticateUserQueryHandler.cs
- [ ] T053 Implement password verification logic in handler
- [ ] T054 Implement JWT token generation in handler
- [ ] T055 Run test to verify GREEN, refactor for token expiration logic
- [ ] T056 [P] Write unit test for failed authentication scenarios (invalid password, locked account)
- [ ] T057 [P] Implement account lockout logic after failed attempts

### Week 3, Day 5: Password Management Commands (TDD)

- [ ] T058 Write test for RequestPasswordResetCommand (RED)
- [ ] T059 Implement RequestPasswordResetCommand and handler in Identity.Application/Commands/PasswordReset/
- [ ] T060 [P] Write test for ResetPasswordCommand (RED)
- [ ] T061 [P] Implement ResetPasswordCommand and handler in Identity.Application/Commands/PasswordReset/
- [ ] T062 Create PasswordResetToken value object with expiration logic
- [ ] T063 Capture test evidence: dotnet test > evidence/identity-application-red.txt and identity-application-green.txt

### Week 3, Day 5: Invitation Workflow Commands (NEW for Delegation)

- [ ] T063a Write failing test for InviteUserCommand in tests/Identity.IntegrationTests/Commands/InviteUserCommandTests.cs (RED)
- [ ] T063b Create InviteUserCommand in Identity.Application/Commands/InviteUser/InviteUserCommand.cs (email, tenant, role)
- [ ] T063c Create InviteUserCommandHandler with idempotency check (10-minute window) (GREEN)
- [ ] T063d Write test for AcceptInvitationCommand (RED)
- [ ] T063e Create AcceptInvitationCommand and handler in Identity.Application/Commands/AcceptInvitation/ (GREEN)
- [ ] T063f Write test for ResendInvitationCommand (RED)
- [ ] T063g Create ResendInvitationCommand and handler (GREEN)
- [ ] T063h Write test for RevokeInvitationCommand (RED)
- [ ] T063i Create RevokeInvitationCommand and handler (GREEN)
- [ ] T063j Create IInvitationRepository and IEmailService interfaces
- [ ] T063k Write test for invitation expiration (7 days) validation (RED)
- [ ] T063l Implement expiration checks in AcceptInvitationCommandHandler (GREEN)

### Week 3, Day 5: Tenant Context Switching Command (NEW for Multi-Tenant)

- [ ] T063m Write failing test for SwitchTenantContextCommand in tests/Identity.IntegrationTests/Commands/SwitchTenantContextCommandTests.cs (RED)
- [ ] T063n Create SwitchTenantContextCommand in Identity.Application/Commands/SwitchTenantContext/SwitchTenantContextCommand.cs
- [ ] T063o Create SwitchTenantContextCommandHandler (GREEN)
- [ ] T063p Create ISessionRepository and IMembershipRepository interfaces
- [ ] T063q Implement handler logic: validate membership, update session, clear cache (GREEN)
- [ ] T063r Write test for switching to unauthorized tenant (RED)
- [ ] T063s Implement authorization validation (GREEN)
- [ ] T063t Write performance test ensuring <200ms completion (RED/GREEN)

### Week 3, Day 5: User Context Queries (Enhanced)

- [ ] T063u Write test for GetCurrentUserContextQuery (RED)
- [ ] T063v Create GetCurrentUserContextQuery and handler returning UserContextModel with session + active tenant (GREEN)
- [ ] T063w Write test for GetUserTenantsQuery (RED)
- [ ] T063x Create GetUserTenantsQuery and handler returning all user's tenant memberships (GREEN)
- [ ] T063y Create UserContextModel, TenantSummaryModel DTOs

**Checkpoint**: Identity Application layer complete with CQRS handlers tested

---

## Phase 4: Identity Service - Infrastructure Layer (Week 4)

**Purpose**: Implement data access, Entra ID integration, and event publishing

**Acceptance Criteria**: EF Core migrations created, Entra ID SSO working, events published to Service Bus

### Week 4, Days 1-2: Infrastructure Project and DbContext (TDD)

- [ ] T064 Create Identity.Infrastructure project at NewDesign/NorthStarET.Lms/src/services/Identity/Identity.Infrastructure/
- [ ] T065 Add EF Core, Npgsql, and MassTransit NuGet packages
- [ ] T066 Create IdentityDbContext in Identity.Infrastructure/Data/IdentityDbContext.cs
- [ ] T067 Configure entity mappings in Identity.Infrastructure/Data/Configurations/ (User, Role, RefreshToken, Session, Membership, Invitation)
- [ ] T067a Add SessionConfiguration with composite key (UserId, SessionId), indexes on ExpiresAt
- [ ] T067b Add MembershipConfiguration with composite key (UserId, TenantId, RoleId), indexes on AssignmentStatus
- [ ] T067c Add InvitationConfiguration with unique index on Token, indexes on ExpiresAt and Status
- [ ] T068 Write integration test for User repository operations (RED)
- [ ] T069 Implement UserRepository in Identity.Infrastructure/Data/Repositories/UserRepository.cs with GetByEntraSubjectIdAsync (GREEN)
- [ ] T069a Write integration test for Session repository operations (RED)
- [ ] T069b Implement SessionRepository in Identity.Infrastructure/Data/Repositories/SessionRepository.cs (GREEN)
- [ ] T069c Write integration test for Membership repository operations (RED)
- [ ] T069d Implement MembershipRepository in Identity.Infrastructure/Data/Repositories/MembershipRepository.cs (GREEN)
- [ ] T069e Write integration test for Invitation repository operations (RED)
- [ ] T069f Implement InvitationRepository in Identity.Infrastructure/Data/Repositories/InvitationRepository.cs (GREEN)
- [ ] T070 Create initial EF Core migration: dotnet ef migrations add InitialIdentitySchema
- [ ] T071 Apply migration to local PostgreSQL: dotnet ef database update
- [ ] T072 Write test for repository idempotency (duplicate user prevention)

### Week 4, Days 2-3: Microsoft Entra ID B2B/B2C Integration (TDD)

- [ ] T073 Write Reqnroll BDD feature for Entra ID B2B SSO in tests/Identity.BddTests/Features/EntraIdB2BAuthentication.feature
- [ ] T073a Write Reqnroll BDD feature for Entra ID B2C SSO in tests/Identity.BddTests/Features/EntraIdB2CAuthentication.feature
- [ ] T074 Create EntraIdProvider in Identity.Infrastructure/Identity/EntraIdProvider.cs supporting both B2B and B2C
- [ ] T075 Configure Microsoft.Identity.Web NuGet package for Entra ID with B2B tenant configuration
- [ ] T075a Configure Microsoft.Identity.Web for Entra ID B2C tenant (policy: B2C_1_susi or equivalent)
- [ ] T076 Implement Entra ID B2B token validation in EntraIdProvider using local JWT validation with cached public keys
- [ ] T076a Implement Entra ID B2C token validation with B2C-specific claims mapping
- [ ] T076b Implement IEntraTokenValidator interface for reusable token validation
- [ ] T077 Implement external provider account linking logic for both B2B and B2C
- [ ] T077a Implement TokenExchangeService (Entra JWT → LMS custom JWT with tenant context)
- [ ] T078 Write step definitions for Entra ID B2B/B2C BDD features in tests/Identity.BddTests/Steps/EntraIdSteps.cs
- [ ] T079 Run BDD tests to verify Entra ID B2B and B2C login flows (RED → GREEN)
- [ ] T080 Implement user synchronization from Entra ID (B2B/B2C) to local database with subject ID mapping

### Week 4, Days 4-5: Event Publishing, Authorization Caching, and Email Service (TDD)

- [ ] T081 Configure MassTransit with Azure Service Bus in Identity.Infrastructure/MessageBus/MessageBusConfiguration.cs
- [ ] T082 Write integration test for UserRegisteredEvent publishing (RED)
- [ ] T083 Implement event publishing in RegisterUserCommandHandler (GREEN)
- [ ] T084 [P] Implement event publishing for UserLoggedInEvent, PasswordChangedEvent, TenantContextSwitchedEvent
- [ ] T084a [P] Implement event publishing for MembershipInvitedEvent, MembershipVerifiedEvent, MembershipRevokedEvent
- [ ] T084b [P] Implement event publishing for InvitationSentEvent, InvitationAcceptedEvent
- [ ] T085 Configure Redis caching in Identity.Infrastructure/Caching/RedisCacheService.cs
- [ ] T085a Write test for AuthorizationCacheService with tenant-scoped cache keys (RED)
- [ ] T085b Implement AuthorizationCacheService in Identity.Infrastructure/Caching/AuthorizationCacheService.cs (GREEN)
- [ ] T085c Implement cache key pattern: authz:{userId}:{tenantId}:{resource}:{action}
- [ ] T085d Implement ClearForUserAndTenantAsync for cache invalidation
- [ ] T085e Write performance test ensuring <50ms authorization decision with cache hit (RED/GREEN)
- [ ] T086 Write test for token validation caching (p95 < 50ms target)
- [ ] T087 Implement Redis token caching with expiration
- [ ] T087a Write test for IEmailService interface for invitation emails (RED)
- [ ] T087b Implement EmailService in Identity.Infrastructure/Email/EmailService.cs with retry logic (exponential backoff, 3 attempts) (GREEN)
- [ ] T087c Implement invitation email template rendering
- [ ] T088 Capture test evidence: dotnet test > evidence/identity-infrastructure-red.txt and identity-infrastructure-green.txt

**Checkpoint**: Identity Infrastructure layer complete with Entra ID and event publishing tested

---

## Phase 5: Identity Service - API Layer (Week 5)

**Purpose**: Implement REST API endpoints with authentication middleware

**Acceptance Criteria**: Identity API endpoints functional, Swagger documentation complete, API tests passing

### Week 5, Days 1-2: API Project Setup

- [ ] T090 Create Identity.API project at NewDesign/NorthStarET.Lms/src/services/Identity/Identity.API/
- [ ] T091 Configure ASP.NET Core Web API with minimal APIs or controllers
- [ ] T092 Add Swashbuckle for Swagger/OpenAPI documentation
- [ ] T093 Configure CORS policy for NorthStar LMS
- [ ] T094 Implement authentication middleware in Identity.API/Middleware/AuthenticationMiddleware.cs
- [ ] T095 [P] Implement rate limiting middleware in Identity.API/Middleware/RateLimitingMiddleware.cs
- [ ] T096 Configure health checks endpoint at /health

### Week 5, Days 2-4: Authentication Endpoints (TDD)

- [ ] T097 Write API test for POST /api/v1/auth/register in tests/Identity.IntegrationTests/Api/AuthControllerTests.cs (RED)
- [ ] T098 Implement RegisterController.Register() in Identity.API/Controllers/AuthController.cs (GREEN)
- [ ] T099 Write API test for POST /api/v1/auth/login (RED)
- [ ] T100 Implement AuthController.Login() to return LMS custom JWT token (GREEN)
- [ ] T100a Write API test for POST /api/v1/auth/exchange (token exchange endpoint) (RED)
- [ ] T100b Implement AuthController.ExchangeToken() accepting Entra JWT, returning LMS custom JWT with tenant context (GREEN)
- [ ] T100c Write API test for GET /api/v1/auth/login/entra (B2B redirect) (RED)
- [ ] T100d Implement AuthController.LoginEntra() redirecting to Entra ID B2B/B2C (GREEN)
- [ ] T100e Write API test for GET /api/v1/auth/callback/entra (OAuth callback) (RED)
- [ ] T100f Implement AuthController.EntraCallback() exchanging authorization code for tokens (GREEN)
- [ ] T101 [P] Write API test for POST /api/v1/auth/refresh (RED)
- [ ] T102 [P] Implement AuthController.RefreshToken() (GREEN)
- [ ] T103 [P] Write API test for POST /api/v1/auth/logout (RED)
- [ ] T104 [P] Implement AuthController.Logout() with token invalidation (GREEN)
- [ ] T105 Add request/response logging for all auth endpoints

### Week 5, Days 4-5: User Management Endpoints (TDD)

- [ ] T106 Write API test for POST /api/v1/users/{id}/password/reset-request (RED)
- [ ] T107 Implement UserController.RequestPasswordReset() (GREEN)
- [ ] T108 [P] Write API test for POST /api/v1/users/{id}/password/reset (RED)
- [ ] T109 [P] Implement UserController.ResetPassword() (GREEN)
- [ ] T110 [P] Write API test for PUT /api/v1/users/{id}/password (RED)
- [ ] T111 [P] Implement UserController.ChangePassword() (GREEN)
- [ ] T112 [P] Write API test for GET /api/v1/users/{id}/claims (RED)
- [ ] T113 [P] Implement UserController.GetUserClaims() (GREEN)
- [ ] T114 Add Swagger annotations for all endpoints
- [ ] T115 Capture test evidence: dotnet test > evidence/identity-api-red.txt and identity-api-green.txt

### Week 5, Day 5: Invitation and Context Endpoints (NEW for Delegation)

- [ ] T115a Write API test for POST /api/v1/invitations in tests/Identity.IntegrationTests/Api/InvitationsControllerTests.cs (RED)
- [ ] T115b Create InvitationsController in Identity.API/Controllers/InvitationsController.cs
- [ ] T115c Implement InvitationsController.InviteUser() (System Admin only) (GREEN)
- [ ] T115d Write API test for POST /api/v1/invitations/{id}/resend (RED)
- [ ] T115e Implement InvitationsController.ResendInvitation() (GREEN)
- [ ] T115f Write API test for DELETE /api/v1/invitations/{id} (RED)
- [ ] T115g Implement InvitationsController.RevokeInvitation() (GREEN)
- [ ] T115h Write API test for GET /api/v1/invitations/verify/{token} (public endpoint) (RED)
- [ ] T115i Implement InvitationsController.VerifyInvitation() with no authentication required (GREEN)
- [ ] T115j Write API test for GET /api/v1/context/tenants (get user's available tenants) (RED)
- [ ] T115k Create TenantContextController in Identity.API/Controllers/TenantContextController.cs
- [ ] T115l Implement TenantContextController.GetUserTenants() (GREEN)
- [ ] T115m Write API test for POST /api/v1/context/switch (switch active tenant) (RED)
- [ ] T115n Implement TenantContextController.SwitchTenant() with performance test (<200ms) (GREEN)
- [ ] T115o Write API test for GET /api/v1/context/current (get current context) (RED)
- [ ] T115p Implement TenantContextController.GetCurrentContext() (GREEN)
- [ ] T115q Configure System Admin authorization policy for invitation endpoints
- [ ] T115r Add Swagger annotations for all invitation and context endpoints

**Checkpoint**: Identity Service API complete with all endpoints tested and documented

---

## Phase 6: Configuration Service - Full Stack (Week 6)

**Purpose**: Implement Configuration Service following same pattern as Identity Service

**Acceptance Criteria**: Configuration Service operational with district, school, calendar, and settings management

### Week 6, Days 1-2: Configuration Domain and Application Layers (TDD)

- [ ] T116 Create Configuration.Domain project at NewDesign/NorthStarET.Lms/src/services/Configuration/Configuration.Domain/
- [ ] T117 Write test for District aggregate in tests/Configuration.UnitTests/Domain/DistrictAggregateTests.cs (RED)
- [ ] T118 Implement District aggregate in Configuration.Domain/Aggregates/DistrictAggregate.cs (GREEN)
- [ ] T119 [P] Write test for School entity (RED)
- [ ] T120 [P] Implement School entity in Configuration.Domain/Entities/School.cs (GREEN)
- [ ] T121 [P] Write test for Calendar entity with day calculations (RED)
- [ ] T122 [P] Implement Calendar entity in Configuration.Domain/Entities/Calendar.cs (GREEN)
- [ ] T123 [P] Implement GradeLevel entity in Configuration.Domain/Entities/GradeLevel.cs
- [ ] T124 [P] Implement SystemSetting entity in Configuration.Domain/Entities/SystemSetting.cs
- [ ] T125 Implement domain events: DistrictCreatedEvent, SchoolCreatedEvent, CalendarUpdatedEvent, SettingChangedEvent in Configuration.Domain/Events/
- [ ] T126 Create Configuration.Application project with CQRS structure
- [ ] T127 Write test for CreateDistrictCommand (RED)
- [ ] T128 Implement CreateDistrictCommandHandler in Configuration.Application/Commands/CreateDistrict/ (GREEN)
- [ ] T129 [P] Write test for CreateSchoolCommand (RED)
- [ ] T130 [P] Implement CreateSchoolCommandHandler (GREEN)
- [ ] T131 [P] Implement GetDistrictByIdQuery and handler
- [ ] T132 [P] Implement GetSchoolsByDistrictQuery and handler

### Week 6, Days 3-4: Configuration Infrastructure and API (TDD)

- [ ] T133 Create Configuration.Infrastructure project with ConfigurationDbContext
- [ ] T134 Configure EF Core entity mappings for District, School, Calendar, GradeLevel, SystemSetting
- [ ] T135 Create initial migration: dotnet ef migrations add InitialConfigurationSchema
- [ ] T136 Apply migration to PostgreSQL database
- [ ] T137 Write integration test for DistrictRepository operations (RED)
- [ ] T138 Implement DistrictRepository in Configuration.Infrastructure/Data/Repositories/ (GREEN)
- [ ] T139 [P] Implement SchoolRepository
- [ ] T140 [P] Implement CalendarRepository
- [ ] T141 Configure MassTransit for Configuration events
- [ ] T142 Implement Redis caching for frequently accessed settings (p95 < 100ms target)
- [ ] T143 Write test for cache invalidation on setting updates
- [ ] T144 Implement cache invalidation logic
- [ ] T145 Create Configuration.API project with REST endpoints
- [ ] T146 Write API test for POST /api/v1/configuration/districts (RED)
- [ ] T147 Implement DistrictsController.CreateDistrict() (GREEN)
- [ ] T148 [P] Write API test for POST /api/v1/configuration/schools (RED)
- [ ] T149 [P] Implement SchoolsController.CreateSchool() (GREEN)
- [ ] T150 [P] Write API test for GET /api/v1/configuration/settings/{key} (RED)
- [ ] T151 [P] Implement SettingsController.GetSetting() (GREEN)

### Week 6, Day 5: Configuration Service Testing and Documentation

- [ ] T152 Write Reqnroll BDD feature for district creation in tests/Configuration.BddTests/Features/DistrictManagement.feature
- [ ] T153 Write BDD feature for calendar setup in tests/Configuration.BddTests/Features/CalendarManagement.feature
- [ ] T154 Implement step definitions for Configuration BDD features
- [ ] T155 Run all Configuration Service tests to verify ≥80% coverage
- [ ] T156 Add Swagger documentation for all Configuration endpoints
- [ ] T157 Document Configuration Service setup in NewDesign/NorthStarET.Lms/docs/configuration-service.md
- [ ] T158 Capture test evidence: dotnet test > evidence/configuration-service-red.txt and configuration-service-green.txt

**Checkpoint**: Configuration Service operational with all features tested

---

## Phase 7: API Gateway with YARP (Week 7)

**Purpose**: Implement API Gateway as single entry point with routing, authentication, and rate limiting

**Acceptance Criteria**: YARP gateway routes requests to Identity and Configuration services, JWT validation working

### Week 7, Days 1-2: Gateway Project Setup (TDD)

- [ ] T159 Create Gateway project at NewDesign/NorthStarET.Lms/src/NorthStar.Gateway/
- [ ] T160 Add Yarp.ReverseProxy NuGet package
- [ ] T161 Configure YARP routes in appsettings.json for Identity and Configuration services
- [ ] T162 Configure YARP clusters with health checks and load balancing
- [ ] T163 Write integration test for gateway routing to Identity service (RED)
- [ ] T164 Implement YARP configuration in Program.cs (GREEN)
- [ ] T165 Write test for gateway routing to Configuration service (RED)
- [ ] T166 Configure Configuration service routes (GREEN)

### Week 7, Days 2-3: JWT Authentication and Authorization (TDD)

- [ ] T167 Configure JWT Bearer authentication in Gateway Program.cs
- [ ] T168 Write test for JWT token validation at gateway (RED)
- [ ] T169 Implement token validation middleware (GREEN)
- [ ] T170 Configure Redis token caching for p95 < 50ms validation
- [ ] T171 Write test for unauthorized request rejection (RED)
- [ ] T172 Implement authorization policies for endpoints (GREEN)
- [ ] T173 Configure role-based access control (RBAC) for admin endpoints
- [ ] T174 Write test for RBAC enforcement
- [ ] T175 Implement claims-based authorization

### Week 7, Days 3-4: Gateway Features (Rate Limiting, CORS, Logging)

- [ ] T176 Configure rate limiting using AspNetCoreRateLimit package
- [ ] T177 Write test for rate limit enforcement (RED)
- [ ] T178 Implement rate limiting policies (GREEN)
- [ ] T179 Configure CORS policy for frontend applications
- [ ] T180 Write test for CORS preflight requests
- [ ] T181 Implement request/response logging middleware in Gateway/Middleware/RequestLoggingMiddleware.cs
- [ ] T182 Configure OpenTelemetry distributed tracing for gateway
- [ ] T183 Implement custom YARP transforms for request/response modification
- [ ] T184 Add health check aggregation endpoint at /health

### Week 7, Days 4-5: Gateway Testing and Documentation

- [ ] T185 Write Reqnroll BDD feature for gateway authentication flow in tests/Gateway.BddTests/Features/GatewayAuth.feature
- [ ] T186 Write BDD feature for gateway routing scenarios
- [ ] T187 Implement step definitions for Gateway BDD features
- [ ] T188 Create Gateway.IntegrationTests for end-to-end routing tests
- [ ] T189 Write performance test for gateway latency (p95 < 100ms target)
- [ ] T190 Run all Gateway tests to verify functionality
- [ ] T191 Add Swagger aggregation for downstream service documentation
- [ ] T192 Document gateway configuration in NewDesign/NorthStarET.Lms/docs/api-gateway.md
- [ ] T193 Capture test evidence: dotnet test > evidence/gateway-red.txt and gateway-green.txt

**Checkpoint**: API Gateway operational, routing all requests with authentication

---

## Phase 8: Aspire Integration and Deployment (Week 8)

**Purpose**: Complete Aspire orchestration, integration testing, and deployment preparation

**Acceptance Criteria**: All services orchestrated via Aspire, integration tests passing, deployment-ready

### Week 8, Days 1-2: Aspire Service Orchestration

- [ ] T194 Register Identity Service in AppHost Program.cs with AddProject<IdentityApi>()
- [ ] T195 Register Configuration Service in AppHost Program.cs
- [ ] T196 Register Gateway in AppHost Program.cs
- [ ] T197 Configure service dependencies in Aspire (Gateway depends on Identity + Configuration)
- [ ] T198 Configure Aspire service discovery for inter-service communication
- [ ] T199 Write Aspire integration test for service startup order (RED)
- [ ] T200 Implement service dependency startup logic (GREEN)
- [ ] T201 Configure Aspire Dashboard for observability
- [ ] T202 Test Aspire Dashboard access at http://localhost:15888

### Week 8-9, Days 2-4: End-to-End Integration Testing (Enhanced for B2B/B2C)

- [ ] T203 Write end-to-end test for user registration → login via Gateway in tests/NorthStar.E2ETests/UserAuthenticationTests.cs (RED)
- [ ] T204 Ensure all services start successfully to make test GREEN
- [ ] T205 Write E2E test for district creation → school creation workflow (RED)
- [ ] T206 Implement cross-service event flow to make test GREEN
- [ ] T207 Write E2E test for Entra ID B2B SSO via Gateway (organizational account) (RED)
- [ ] T207a Write E2E test for Entra ID B2C SSO via Gateway (external identity) (RED)
- [ ] T208 Verify Entra ID B2B integration works end-to-end (GREEN)
- [ ] T208a Verify Entra ID B2C integration works end-to-end (GREEN)
- [ ] T208b Write E2E test for token exchange flow: Entra JWT → LMS custom token (RED)
- [ ] T208c Verify token exchange completes in <50ms (GREEN)
- [ ] T209 Write E2E test for System Admin inviting District Admin (RED)
- [ ] T209a Verify invitation email sent, token generated, 7-day expiration set (GREEN)
- [ ] T209b Write E2E test for invitation acceptance flow (email link → verification → membership active) (RED)
- [ ] T209c Verify invitation acceptance creates active membership (GREEN)
- [ ] T209d Write E2E test for expired invitation (>7 days) rejection (RED)
- [ ] T209e Verify expired invitations cannot be accepted (GREEN)
- [ ] T210 Write E2E test for tenant context switching (RED)
- [ ] T210a Verify District Admin can switch between district and school contexts (GREEN)
- [ ] T210b Verify tenant switch completes in <200ms (GREEN)
- [ ] T210c Verify authorization cache invalidated on tenant switch (GREEN)
- [ ] T211 Write performance test for full authentication flow with token exchange (p95 < 200ms) (RED)
- [ ] T211a Optimize services if performance targets not met (GREEN)
- [ ] T212 Write E2E test for multi-tenant isolation (verify no cross-district data leakage) (RED)
- [ ] T212a Verify District Admin cannot access other districts (GREEN)
- [ ] T212b Verify multi-tenant data isolation at database RLS level (GREEN)
- [ ] T213 Write E2E test for authorization caching performance (<50ms P95) (RED)
- [ ] T213a Verify authorization decisions cached and cache hit rate >90% (GREEN)

### Week 8, Days 3-4: Playwright UI Tests (Optional if UI exists)

- [ ] T214 Create Playwright test project at tests/NorthStar.UITests/
- [ ] T215 Write Playwright test for login page journey in tests/NorthStar.UITests/LoginTests.cs
- [ ] T216 Write Playwright test for user registration form
- [ ] T217 Write Playwright test for password reset flow
- [ ] T218 Write Playwright test for district setup wizard
- [ ] T219 Run all Playwright tests against running services

### Week 8, Days 4-5: Deployment Preparation and Documentation

- [ ] T220 Create Dockerfile for Identity Service at NewDesign/NorthStarET.Lms/src/services/Identity/Identity.API/Dockerfile
- [ ] T221 Create Dockerfile for Configuration Service
- [ ] T222 Create Dockerfile for Gateway
- [ ] T223 Create Docker Compose file for all Phase 1 services at NewDesign/NorthStarET.Lms/docker-compose.yml
- [ ] T224 Test Docker Compose deployment locally
- [ ] T225 Create Kubernetes manifests in NewDesign/NorthStarET.Lms/k8s/ for Identity, Configuration, Gateway
- [ ] T226 Configure Azure Container Registry (ACR) integration
- [ ] T227 Create CI/CD pipeline configuration in .github/workflows/phase1-foundation.yml
- [ ] T228 Configure Azure Key Vault secrets for production
- [ ] T229 Write deployment guide in NewDesign/NorthStarET.Lms/docs/deployment-guide.md
- [ ] T230 Write operations runbook in NewDesign/NorthStarET.Lms/docs/operations-runbook.md
- [ ] T231 Create Phase 1 completion report documenting all deliverables
- [ ] T232 Verify ≥80% test coverage for all services: dotnet test --collect:"XPlat Code Coverage"
- [ ] T233 Archive all test evidence in evidence/ directory

**Checkpoint**: Phase 1 Foundation Services functionally complete, pending security validation

---

## Phase 9: Security Testing and Documentation (Week 10 - NEW)

**Purpose**: Validate security posture for B2B/B2C integration, tenant isolation, and invitation workflow

**Acceptance Criteria**: Security vulnerabilities identified and resolved, penetration testing completed

### Week 10, Days 1-2: Security Testing

- [ ] T234 Conduct penetration testing for invitation token reuse attacks
- [ ] T235 Test SQL injection resistance in all user input fields (district suffix, email, etc.)
- [ ] T236 Test XSS payload injection in user-supplied content
- [ ] T237 Verify CSRF protection on all state-changing operations (invite, revoke, context switch)
- [ ] T238 Test rate limiting enforcement (max 10 requests/minute for invitation endpoints)
- [ ] T239 Verify tenant boundary enforcement (attempt cross-district access with manipulated JWT claims)
- [ ] T240 Test Entra token signature validation (attempt JWT forgery)
- [ ] T241 Verify expired invitation token rejection (>7 days)
- [ ] T242 Test session hijacking resistance (token hash validation)
- [ ] T243 Document all security findings and resolutions

### Week 10, Days 3-4: B2B/B2C Documentation

- [ ] T244 Document Entra B2B tenant configuration in docs/entra-b2b-setup.md
- [ ] T245 Document Entra B2C tenant configuration in docs/entra-b2c-setup.md
- [ ] T246 Document token exchange flow with sequence diagrams
- [ ] T247 Document invitation workflow with email templates
- [ ] T248 Document tenant context switching for multi-tenant users
- [ ] T249 Document authorization caching strategy and cache key patterns
- [ ] T250 Update API documentation with new invitation and context endpoints
- [ ] T251 Create troubleshooting guide for common B2B/B2C authentication issues

### Week 10, Day 5: Final Verification and Sign-off

- [ ] T252 Verify all acceptance criteria from spec.md are met
- [ ] T253 Verify all new success criteria (SC-016, SC-017, SC-018) are achieved
- [ ] T254 Run final code coverage report (target ≥80%)
- [ ] T255 Generate final performance test report (authorization <50ms, tenant switch <200ms)
- [ ] T256 Conduct architecture review with stakeholders
- [ ] T257 Obtain sign-off on Phase 1 completion

**Checkpoint**: Phase 1 Foundation Services security-validated and production-ready

---

## Phase 10: Data Migration from Legacy (Post-Week 10, ongoing)

**Purpose**: Migrate users and configuration data from OldNorthStar to new services

**Acceptance Criteria**: Historical data migrated, data integrity validated, dual-write operational

### Data Migration Tasks

- [ ] T258 Analyze legacy IdentityServer database schema at OldNorthStar/IdentityServer/LoginContext.cs
- [ ] T259 Create ETL scripts for user migration in NewDesign/NorthStarET.Lms/migration/scripts/migrate-users.sql
- [ ] T260 Create ETL scripts for district/school migration in migration/scripts/migrate-configuration.sql
- [ ] T261 Implement data validation scripts to compare legacy vs. new data
- [ ] T262 Write reconciliation report generator
- [ ] T263 Implement dual-write middleware for gradual migration
- [ ] T264 Configure feature flags for gradual service cutover
- [ ] T265 Execute user data migration in staging environment
- [ ] T266 Execute configuration data migration in staging environment
- [ ] T267 Validate data integrity with reconciliation scripts
- [ ] T268 Monitor dual-write performance and error rates
- [ ] T269 Execute production migration during maintenance window
- [ ] T270 Switch Gateway routing to new services
- [ ] T271 Monitor production performance for 48 hours
- [ ] T272 Decommission legacy IdentityServer after successful migration
- [ ] T273 Archive legacy databases

---

## Dependencies & Execution Order

### Phase Dependencies

1. **Phase 1 (Week 1)**: Aspire Infrastructure Setup - NO dependencies, start immediately
2. **Phase 2 (Week 2)**: Identity Domain - Depends on Phase 1 Aspire setup
3. **Phase 3 (Week 3)**: Identity Application - Depends on Phase 2 Domain layer
4. **Phase 4 (Week 4)**: Identity Infrastructure - Depends on Phase 3 Application layer
5. **Phase 5 (Week 5)**: Identity API - Depends on Phase 4 Infrastructure layer
6. **Phase 6 (Week 6)**: Configuration Service - Can start in parallel with Identity (after Phase 1), but shown sequentially here
7. **Phase 7 (Week 7)**: API Gateway - Depends on Identity API and Configuration API being ready
8. **Phase 8 (Week 8)**: Aspire Integration - Depends on all services being implemented
9. **Phase 9 (Week 10)**: Security Testing - Depends on Phase 8 completion
10. **Phase 10 (Ongoing)**: Data Migration - Can start after services are deployed

### Critical Path

```
Week 1: Aspire Setup (T001-T016)
   ↓
Week 2: Identity Domain (T017-T038 + NEW: T029a-T029l, T034a-T034f)
   ↓
Week 3: Identity Application (T039-T063 + NEW: T049a-T049i, T063a-T063y)
   ↓
Week 4: Identity Infrastructure (T064-T089 + NEW: T067a-T067c, T069a-T069f, T073a, T075a, T076a-T076b, T077a, T084a-T084b, T085a-T085e, T087a-T087c)
   ↓
Week 5-6: Identity API (T090-T115 + NEW: T100a-T100f, T115a-T115r)
   ↓
Week 6-7: Configuration Service (T116-T158) [can overlap with Identity API]
   ↓
Week 7-8: API Gateway (T159-T193)
   ↓
Week 9: Integration & Deployment (T194-T233 + NEW E2E tests)
   ↓
Week 10: Security Testing & Documentation (T234-T257)
```

**Updated Task Counts:**
- Phase 2 (Identity Domain): 22 original + 18 new = 40 tasks
- Phase 3 (Identity Application): 25 original + 34 new = 59 tasks
- Phase 4 (Identity Infrastructure): 26 original + 27 new = 53 tasks
- Phase 5 (Identity API): 26 original + 24 new = 50 tasks
- **Total NEW Tasks Added**: ~103 new granular tasks for B2B/B2C features

### Parallel Opportunities

- **Within Domain Layer**: Entity tests (T021-T029) can run in parallel
- **Within Application Layer**: Multiple command/query handlers can be developed in parallel
- **Infrastructure Layer**: Event publishing (T082-T084) can be implemented in parallel
- **API Layer**: Endpoint implementations (T097-T113) can be developed in parallel
- **Configuration Service (Week 6)**: Could be developed in parallel with Identity Service by a second team member, starting after Week 1
- **Testing**: Unit tests, integration tests, and BDD features can be written in parallel with implementation

### TDD Workflow for Each Task

For all implementation tasks, follow this workflow:

1. **RED**: Write failing test first (e.g., T021 - write test, T022 - implement)
2. **GREEN**: Implement minimal code to make test pass
3. **REFACTOR**: Clean up code while keeping tests green
4. **EVIDENCE**: Capture test output in evidence/ directory

---

## Test Coverage Requirements

**Constitutional Requirement**: ≥80% code coverage for Domain and Application layers

### Coverage Verification Commands

```bash
# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html

# View coverage report
open coverage-report/index.html
```

### Coverage Targets by Layer

- **Domain Layer**: ≥85% (business logic critical)
- **Application Layer**: ≥80% (CQRS handlers)
- **Infrastructure Layer**: ≥70% (external dependencies make 80% harder)
- **API Layer**: ≥75% (controller logic + integration tests)

---

## Acceptance Criteria Summary

### Phase 1 Foundation Services is COMPLETE when:

✅ **Aspire Orchestration**
- [ ] All services register with Aspire AppHost successfully
- [ ] Aspire Dashboard shows all services healthy at http://localhost:15888
- [ ] Service discovery working between services
- [ ] OpenTelemetry traces visible for all requests

✅ **Identity Service (Enhanced for B2B/B2C)**
- [ ] User registration, login, logout working via API
- [ ] Microsoft Entra ID B2B SSO functional for staff/admins (organizational accounts)
- [ ] Microsoft Entra ID B2C configured for future student/parent access (external identities)
- [ ] Token exchange pattern working (Entra JWT → LMS custom token with tenant context)
- [ ] JWT tokens issued and validated correctly with tenant-scoped claims
- [ ] Password reset flow working with email notifications
- [ ] **NEW: Invitation workflow functional** (System Admin → District Admin promotion)
- [ ] **NEW: Invitation acceptance** with 7-day expiration and single-use token
- [ ] **NEW: Tenant context switching** (<200ms) for multi-tenant users
- [ ] **NEW: Session tracking** with active tenant ID
- [ ] **NEW: Membership management** (user-tenant-role associations)
- [ ] **NEW: Authorization caching** in Redis (<50ms P95, >90% hit rate)
- [ ] Domain events published to Azure Service Bus (including new events: TenantContextSwitched, MembershipInvited, etc.)
- [ ] ≥80% test coverage verified

✅ **Configuration Service**
- [ ] Districts and schools can be created and managed
- [ ] Academic calendars configured correctly
- [ ] System settings cached in Redis with <100ms retrieval
- [ ] Cache invalidation working across services
- [ ] ≥80% test coverage verified

✅ **API Gateway**
- [ ] YARP routes all requests to correct services
- [ ] JWT authentication enforced at gateway
- [ ] Rate limiting prevents abuse
- [ ] CORS configured for frontend applications
- [ ] Health check aggregation working

✅ **Testing & Quality**
- [ ] All BDD features (Reqnroll) passing
- [ ] All integration tests passing
- [ ] All API tests passing
- [ ] Performance SLOs met (auth p95 < 200ms, config p95 < 100ms)
- [ ] Test evidence captured in evidence/ directory

✅ **Deployment Ready**
- [ ] Docker Compose starts all services locally
- [ ] Kubernetes manifests created and validated
- [ ] CI/CD pipeline configured
- [ ] Documentation complete (setup, deployment, operations)
- [ ] Azure Key Vault configured for secrets

✅ **Data Migration (if executed)**
- [ ] User data migrated from legacy IdentityServer
- [ ] Configuration data migrated from legacy database
- [ ] Data integrity validated
- [ ] Dual-write operational for gradual cutover

---

## Task Estimation Summary (Updated for B2B/B2C Features)

- **Total Tasks**: 248 original + ~103 new = **351 tasks**
- **Average Task Duration**: 2-4 hours
- **Total Effort**: ~700-1400 hours (17-35 person-weeks)
- **Timeline**: **9-10 weeks** with 1-2 developers (extended from 8 weeks)
- **Phases**: **10 phases** with clear checkpoints (added Phase 9: Security Testing, renumbered Data Migration to Phase 10)

### Tasks by Phase (Updated)

| Phase | Tasks | Duration | Dependencies |
|-------|-------|----------|--------------|
| Phase 1: Aspire Setup | T001-T016 (16 tasks) | Week 1 | None |
| Phase 2: Identity Domain | T017-T038 + NEW (40 tasks) | Week 2 | Phase 1 |
| Phase 3: Identity Application | T039-T063 + NEW (59 tasks) | Week 3 | Phase 2 |
| Phase 4: Identity Infrastructure | T064-T089 + NEW (53 tasks) | Week 4 | Phase 3 |
| Phase 5: Identity API | T090-T115 + NEW (50 tasks) | Weeks 5-6 | Phase 4 |
| Phase 6: Configuration Service | T116-T158 (43 tasks) | Weeks 6-7 | Phase 1 |
| Phase 7: API Gateway | T159-T193 (35 tasks) | Weeks 7-8 | Phases 5, 6 |
| Phase 8: Integration & Deployment | T194-T233 + NEW E2E (40 tasks) | Week 9 | Phase 7 |
| Phase 9: Security Testing | T234-T257 (24 tasks) | Week 10 | Phase 8 |
| Phase 10: Data Migration | T258-T273 (16 tasks) | Post-Week 10 | Phase 9 |

### New Tasks Added for B2B/B2C Features

**Domain Layer (Week 2)**:
- Session entity (12 tasks: T029a-T029l)
- Membership entity (included in Session tasks)
- Invitation entity (included in Session tasks)
- Additional domain events (6 tasks: T034a-T034f)

**Application Layer (Week 3)**:
- Token exchange command (9 tasks: T049a-T049i)
- Invitation workflow commands (12 tasks: T063a-T063l)
- Tenant context switching (8 tasks: T063m-T063t)
- Enhanced user context queries (5 tasks: T063u-T063y)

**Infrastructure Layer (Week 4)**:
- Enhanced DbContext configurations (3 tasks: T067a-T067c)
- Additional repositories (6 tasks: T069a-T069f)
- B2B/B2C Entra configuration (5 tasks: T073a, T075a, T076a-T076b, T077a)
- Enhanced event publishing (2 tasks: T084a-T084b)
- Authorization cache service (5 tasks: T085a-T085e)
- Email service (3 tasks: T087a-T087c)

**API Layer (Weeks 5-6)**:
- Token exchange endpoints (6 tasks: T100a-T100f)
- Invitation and context endpoints (18 tasks: T115a-T115r)

**Integration Testing (Week 9)**:
- Enhanced E2E tests (16 tasks: T207a-T213a)

**Security Testing (Week 10)**:
- New security testing phase (24 tasks: T232a-T232x)

**Total New Tasks**: ~103 tasks

### Parallel Work Opportunities

- **Configuration Service (Week 6)** can be developed in parallel with Identity Service by second developer
- **Within each phase**: Tasks marked [P] can be executed in parallel
- **Testing tasks**: BDD features, unit tests, integration tests can be written in parallel

---

## Implementation Notes

### TDD Red → Green Workflow

Every implementation task follows this pattern:

1. **Write failing test first** (RED)
   - Write test that exercises the feature
   - Run test to verify it fails
   - Capture failure output in evidence/[service]-red.txt

2. **Implement minimal code** (GREEN)
   - Write just enough code to make test pass
   - Run test to verify it succeeds
   - Capture success output in evidence/[service]-green.txt

3. **Refactor** (CLEAN)
   - Improve code quality
   - Ensure tests still pass
   - Commit changes

### Constitutional Compliance Checklist

For each service, verify:

- [ ] Clean Architecture layers enforced (Domain → Application → Infrastructure)
- [ ] No UI → Infrastructure coupling
- [ ] TDD Red → Green evidence captured
- [ ] Reqnroll BDD features written before implementation
- [ ] ≥80% code coverage achieved
- [ ] .NET Aspire orchestration configured
- [ ] Health checks implemented
- [ ] OpenTelemetry tracing configured
- [ ] Secrets in Azure Key Vault only
- [ ] RBAC implemented in Application layer

### Technology Stack

- **.NET**: 10.0
- **Aspire**: 13.0.0
- **EF Core**: 10.0
- **PostgreSQL**: Latest
- **Redis**: Latest
- **Azure Service Bus**: Latest
- **MassTransit**: Latest
- **YARP**: Latest
- **Microsoft.Identity.Web**: Latest (Entra ID B2B/B2C integration - all authentication flows)
- **MediatR**: Latest (CQRS)
- **FluentValidation**: Latest
- **Reqnroll**: Latest (BDD)
- **Playwright**: Latest (UI tests)
- **xUnit**: Latest (unit/integration tests)

---

## Related Documentation

- **Master Migration Plan**: Plans/MASTER_MIGRATION_PLAN.md
- **Identity Service Spec**: Plans/microservices/services/identity-service.md
- **Configuration Service Spec**: Plans/microservices/services/configuration-service.md
- **API Gateway Spec**: Plans/microservices/docs/api-gateway-config.md
- **Constitution**: .specify/memory/constitution.md
- **WIPNorthStar Implementation**: Src/WIPNorthStar/NorthStarET.Lms/ (reference patterns)

---

**Document Version**: 1.0  
**Created**: 2025-11-19  
**Status**: Ready for Implementation  
**Approval**: Pending architectural review
