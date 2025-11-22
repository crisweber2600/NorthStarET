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

- [x] T001 Create NewDesign/NorthStarET.Lms solution structure matching UpgradedNorthStar target
- [x] T002 Create Aspire AppHost project at NewDesign/NorthStarET.Lms/src/NorthStar.AppHost/
- [x] T003 Configure Aspire.Hosting NuGet packages (Microsoft.NET.Sdk.Aspire) in AppHost
- [x] T004 Create ServiceDefaults project at NewDesign/NorthStarET.Lms/src/NorthStar.ServiceDefaults/
- [x] T005 [P] Implement health check middleware in NorthStar.ServiceDefaults/Extensions/HealthCheckExtensions.cs
- [x] T006 [P] Implement OpenTelemetry configuration in NorthStar.ServiceDefaults/Extensions/ObservabilityExtensions.cs
- [x] T007 [P] Implement structured logging in NorthStar.ServiceDefaults/Extensions/LoggingExtensions.cs

### Week 1, Days 3-5: Shared Infrastructure Services

- [x] T008 Configure PostgreSQL resource in AppHost Program.cs for multi-service database
- [x] T009 Configure Redis resource in AppHost Program.cs for distributed caching
- [x] T010 Configure Azure Service Bus resource in AppHost Program.cs for event messaging
- [x] T011 Create Docker Compose override for local development at NewDesign/NorthStarET.Lms/docker-compose.override.yml
- [x] T012 Configure Azure Key Vault integration in ServiceDefaults for secrets management
- [x] T013 [P] Create Aspire integration test project at tests/NorthStar.Aspire.Tests/
- [x] T014 Write Aspire resource discovery test in tests/NorthStar.Aspire.Tests/ResourceDiscoveryTests.cs (RED)
- [x] T015 Implement Aspire resource registration to make test GREEN
- [x] T016 Document Aspire setup in NewDesign/NorthStarET.Lms/docs/aspire-setup.md

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
- [ ] T029 [P] Implement ExternalProvider entity in Identity.Domain/Entities/ExternalProvider.cs for Entra ID linkage

### Week 2, Days 4-5: Domain Events and Value Objects (TDD)

- [ ] T030 Write test for UserRegisteredEvent in tests/Identity.UnitTests/Events/UserRegisteredEventTests.cs (RED)
- [ ] T031 Implement UserRegisteredEvent in Identity.Domain/Events/UserRegisteredEvent.cs (GREEN)
- [ ] T032 [P] Implement UserLoggedInEvent in Identity.Domain/Events/UserLoggedInEvent.cs
- [ ] T033 [P] Implement PasswordChangedEvent in Identity.Domain/Events/PasswordChangedEvent.cs
- [ ] T034 [P] Implement UserRoleChangedEvent in Identity.Domain/Events/UserRoleChangedEvent.cs
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

**Checkpoint**: Identity Application layer complete with CQRS handlers tested

---

## Phase 4: Identity Service - Infrastructure Layer (Week 4)

**Purpose**: Implement data access, Entra ID integration, and event publishing

**Acceptance Criteria**: EF Core migrations created, Entra ID SSO working, events published to Service Bus

### Week 4, Days 1-2: Infrastructure Project and DbContext (TDD)

- [ ] T064 Create Identity.Infrastructure project at NewDesign/NorthStarET.Lms/src/services/Identity/Identity.Infrastructure/
- [ ] T065 Add EF Core, Npgsql, and MassTransit NuGet packages
- [ ] T066 Create IdentityDbContext in Identity.Infrastructure/Data/IdentityDbContext.cs
- [ ] T067 Configure entity mappings in Identity.Infrastructure/Data/Configurations/
- [ ] T068 Write integration test for User repository operations (RED)
- [ ] T069 Implement UserRepository in Identity.Infrastructure/Data/Repositories/UserRepository.cs (GREEN)
- [ ] T070 Create initial EF Core migration: dotnet ef migrations add InitialIdentitySchema
- [ ] T071 Apply migration to local PostgreSQL: dotnet ef database update
- [ ] T072 Write test for repository idempotency (duplicate user prevention)

### Week 4, Days 2-3: Microsoft Entra ID Integration (TDD)

- [ ] T073 Write Reqnroll BDD feature for Entra ID SSO in tests/Identity.BddTests/Features/EntraIdAuthentication.feature
- [ ] T074 Create EntraIdProvider in Identity.Infrastructure/Identity/EntraIdProvider.cs
- [ ] T075 Configure Microsoft.Identity.Web NuGet package for Entra ID
- [ ] T076 Implement Entra ID token validation in EntraIdProvider
- [ ] T077 Implement external provider account linking logic
- [ ] T078 Write step definitions for Entra ID BDD feature in tests/Identity.BddTests/Steps/EntraIdSteps.cs
- [ ] T079 Run BDD tests to verify Entra ID login flow (RED → GREEN)
- [ ] T080 Implement user synchronization from Entra ID to local database

### Week 4, Days 4-5: Event Publishing and Caching (TDD)

- [ ] T081 Configure MassTransit with Azure Service Bus in Identity.Infrastructure/MessageBus/MessageBusConfiguration.cs
- [ ] T082 Write integration test for UserRegisteredEvent publishing (RED)
- [ ] T083 Implement event publishing in RegisterUserCommandHandler (GREEN)
- [ ] T084 [P] Implement event publishing for UserLoggedInEvent, PasswordChangedEvent
- [ ] T085 Configure Redis caching in Identity.Infrastructure/Caching/RedisCacheService.cs
- [ ] T086 Write test for token validation caching (p95 < 50ms target)
- [ ] T087 Implement Redis token caching with expiration
- [ ] T088 Configure Duende IdentityServer in Identity.Infrastructure/Identity/DuendeIdentityServerConfig.cs
- [ ] T089 Capture test evidence: dotnet test > evidence/identity-infrastructure-red.txt and identity-infrastructure-green.txt

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
- [ ] T100 Implement AuthController.Login() to return JWT access token (GREEN)
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

### Week 8, Days 2-3: End-to-End Integration Testing

- [ ] T203 Write end-to-end test for user registration → login via Gateway in tests/NorthStar.E2ETests/UserAuthenticationTests.cs (RED)
- [ ] T204 Ensure all services start successfully to make test GREEN
- [ ] T205 Write E2E test for district creation → school creation workflow (RED)
- [ ] T206 Implement cross-service event flow to make test GREEN
- [ ] T207 Write E2E test for Entra ID SSO via Gateway
- [ ] T208 Verify Entra ID integration works end-to-end
- [ ] T209 Write performance test for full authentication flow (p95 < 200ms)
- [ ] T210 Optimize services if performance targets not met
- [ ] T211 Write E2E test for multi-tenant isolation (verify no cross-district data leakage)
- [ ] T212 Verify multi-tenant data isolation at database level

### Week 8, Days 3-4: Playwright UI Tests (Optional if UI exists)

- [ ] T213 Create Playwright test project at tests/NorthStar.UITests/
- [ ] T214 Write Playwright test for login page journey in tests/NorthStar.UITests/LoginTests.cs
- [ ] T215 Write Playwright test for user registration form
- [ ] T216 Write Playwright test for password reset flow
- [ ] T217 Write Playwright test for district setup wizard
- [ ] T218 Run all Playwright tests against running services

### Week 8, Days 4-5: Deployment Preparation and Documentation

- [ ] T219 Create Dockerfile for Identity Service at NewDesign/NorthStarET.Lms/src/services/Identity/Identity.API/Dockerfile
- [ ] T220 Create Dockerfile for Configuration Service
- [ ] T221 Create Dockerfile for Gateway
- [ ] T222 Create Docker Compose file for all Phase 1 services at NewDesign/NorthStarET.Lms/docker-compose.yml
- [ ] T223 Test Docker Compose deployment locally
- [ ] T224 Create Kubernetes manifests in NewDesign/NorthStarET.Lms/k8s/ for Identity, Configuration, Gateway
- [ ] T225 Configure Azure Container Registry (ACR) integration
- [ ] T226 Create CI/CD pipeline configuration in .github/workflows/phase1-foundation.yml
- [ ] T227 Configure Azure Key Vault secrets for production
- [ ] T228 Write deployment guide in NewDesign/NorthStarET.Lms/docs/deployment-guide.md
- [ ] T229 Write operations runbook in NewDesign/NorthStarET.Lms/docs/operations-runbook.md
- [ ] T230 Create Phase 1 completion report documenting all deliverables
- [ ] T231 Verify ≥80% test coverage for all services: dotnet test --collect:"XPlat Code Coverage"
- [ ] T232 Archive all test evidence in evidence/ directory

**Checkpoint**: Phase 1 Foundation Services complete and deployment-ready

---

## Phase 9: Data Migration from Legacy (Post-Week 8, ongoing)

**Purpose**: Migrate users and configuration data from OldNorthStar to new services

**Acceptance Criteria**: Historical data migrated, data integrity validated, dual-write operational

### Data Migration Tasks

- [ ] T233 Analyze legacy IdentityServer database schema at OldNorthStar/IdentityServer/LoginContext.cs
- [ ] T234 Create ETL scripts for user migration in NewDesign/NorthStarET.Lms/migration/scripts/migrate-users.sql
- [ ] T235 Create ETL scripts for district/school migration in migration/scripts/migrate-configuration.sql
- [ ] T236 Implement data validation scripts to compare legacy vs. new data
- [ ] T237 Write reconciliation report generator
- [ ] T238 Implement dual-write middleware for gradual migration
- [ ] T239 Configure feature flags for gradual service cutover
- [ ] T240 Execute user data migration in staging environment
- [ ] T241 Execute configuration data migration in staging environment
- [ ] T242 Validate data integrity with reconciliation scripts
- [ ] T243 Monitor dual-write performance and error rates
- [ ] T244 Execute production migration during maintenance window
- [ ] T245 Switch Gateway routing to new services
- [ ] T246 Monitor production performance for 48 hours
- [ ] T247 Decommission legacy IdentityServer after successful migration
- [ ] T248 Archive legacy databases

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
9. **Phase 9 (Ongoing)**: Data Migration - Can start after services are deployed

### Critical Path

```
Week 1: Aspire Setup (T001-T016)
   ↓
Week 2: Identity Domain (T017-T038)
   ↓
Week 3: Identity Application (T039-T063)
   ↓
Week 4: Identity Infrastructure (T064-T089)
   ↓
Week 5: Identity API (T090-T115)
   ↓
Week 6: Configuration Service (T116-T158) [could be parallel]
   ↓
Week 7: API Gateway (T159-T193)
   ↓
Week 8: Integration & Deployment (T194-T232)
```

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

✅ **Identity Service**
- [ ] User registration, login, logout working via API
- [ ] Microsoft Entra ID SSO functional for staff/admins
- [ ] JWT tokens issued and validated correctly
- [ ] Password reset flow working with email notifications
- [ ] Domain events published to Azure Service Bus
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

## Task Estimation Summary

- **Total Tasks**: 248 tasks
- **Average Task Duration**: 2-4 hours
- **Total Effort**: ~500-1000 hours (12-25 person-weeks)
- **Timeline**: 8 weeks with 1-2 developers
- **Phases**: 9 phases with clear checkpoints

### Tasks by Phase

| Phase | Tasks | Duration | Dependencies |
|-------|-------|----------|--------------|
| Phase 1: Aspire Setup | T001-T016 (16 tasks) | Week 1 | None |
| Phase 2: Identity Domain | T017-T038 (22 tasks) | Week 2 | Phase 1 |
| Phase 3: Identity Application | T039-T063 (25 tasks) | Week 3 | Phase 2 |
| Phase 4: Identity Infrastructure | T064-T089 (26 tasks) | Week 4 | Phase 3 |
| Phase 5: Identity API | T090-T115 (26 tasks) | Week 5 | Phase 4 |
| Phase 6: Configuration Service | T116-T158 (43 tasks) | Week 6 | Phase 1 |
| Phase 7: API Gateway | T159-T193 (35 tasks) | Week 7 | Phases 5, 6 |
| Phase 8: Integration & Deployment | T194-T232 (39 tasks) | Week 8 | Phase 7 |
| Phase 9: Data Migration | T233-T248 (16 tasks) | Ongoing | Phase 8 |

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
- **Duende IdentityServer**: Latest (successor to IdentityServer4)
- **Microsoft.Identity.Web**: Latest (Entra ID integration)
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
