# Implementation Plan: Phase 1 Foundation Services

**Branch**: `001-phase1-foundation-services` | **Date**: 2025-11-19 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-phase1-foundation-services/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Establish the foundational infrastructure for NorthStar's microservices architecture by implementing three critical services: **Identity & Authentication** (Microsoft Entra ID SSO + local fallback), **API Gateway** (YARP with routing, rate limiting, circuit breakers), and **Configuration Service** (district/school/calendar management). These services provide the security, routing, and organizational context required for all downstream domain services, following Clean Architecture, .NET Aspire orchestration, and TDD principles with ≥80% test coverage.

## Technical Context

**Language/Version**: C# 12 / .NET 10.0  
**Primary Dependencies**: 
  - ASP.NET Core 10.0 (Web API)
  - .NET Aspire 13.0.0 (Orchestration)
  - Entity Framework Core 10.0 (Data Access)
  - MediatR 12.4.0 (CQRS pattern)
  - Microsoft.Identity.Web 3.5.0 (Entra ID integration)
  - Yarp.ReverseProxy 2.2.0 (API Gateway)
  - MassTransit 8.x (Event-driven messaging)
  
**Storage**: 
  - PostgreSQL 16+ (primary data store with multi-tenant Row-Level Security)
  - Redis 7+ (distributed caching)
  - Azure Service Bus (event messaging)
  - Azure Key Vault (secrets management)
  
**Testing**: 
  - xUnit 2.9.0 (unit testing)
  - Reqnroll 2.0.0 (BDD acceptance tests)
  - Aspire.Hosting.Testing 13.0.0 (integration tests)
  - TestContainers 3.10.0 (database testing)
  
**Target Platform**: Linux containers (Docker) / Azure Kubernetes Service (AKS)  
**Project Type**: Microservices architecture with 3 services in Phase 1  
**Performance Goals**: 
  - Authentication token validation: <50ms (P95)
  - API Gateway routing: <10ms overhead (P95)
  - Configuration data retrieval: <100ms cached (P95)
  - SSO authentication flow: <3 seconds end-to-end
  
**Constraints**: 
  - All services must follow Clean Architecture (Domain → Application → Infrastructure → API)
  - Zero hardcoded credentials (Azure Key Vault only)
  - ≥80% test coverage (constitutional requirement)
  - Multi-tenant data isolation enforced at database and application layers
  - Event-driven integration (prefer async over sync calls)
  
**Scale/Scope**: 
  - Support 1,000+ concurrent authenticated users per service
  - 100+ school districts (tenants) in multi-tenant databases
  - 10,000+ API requests per minute through gateway
  - 3 foundational services in Phase 1

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Based on **NorthStarET NextGen LMS Constitution v1.6.0**, Phase 1 Foundation Services compliance:

✅ **Clean Architecture & Aspire Orchestration**
- All three services (Identity, Gateway, Configuration) follow Clean Architecture layers
- All services orchestrated through .NET Aspire 13.0.0 AppHost
- Aspire integration test projects required for each service

✅ **Test-Driven Quality Gates**
- TDD workflow: Red → Green → Refactor with evidence capture
- Reqnroll BDD features written before implementation for each user story
- xUnit unit tests with ≥80% coverage requirement
- Integration tests using Aspire.Hosting.Testing

✅ **UX Traceability**
- Phase 1 is API-focused (no UI components)
- Future UI work will preserve existing OldNorthStar layouts (no Figma required)
- Authentication flows integrate with existing AngularJS frontend during migration

✅ **Event-Driven Data Discipline**
- Configuration changes publish events via Azure Service Bus (ConfigurationChangedEvent)
- Identity events for authentication activities (UserLoggedInEvent, PasswordChangedEvent)
- Async integration preferred; sync calls documented with latency budgets
- Idempotent event handlers with versioned schemas

✅ **Security & Compliance Safeguards**
- Microsoft Entra ID integration follows WIPNorthStar Feature 001 proven patterns
- JWT token validation in API Gateway before routing
- Role-based access control (RBAC) enforced in Application layer
- All secrets exclusively in Azure Key Vault
- Multi-tenant isolation via PostgreSQL Row-Level Security + application filtering

✅ **Tool-Assisted Development Workflow**
- Leverage existing WIPNorthStar Feature 001/002/004 implementations as templates
- Query Microsoft docs for Aspire, YARP, Entra ID best practices
- Structured thinking for architectural decisions

**Delivery Workflow Compliance**:
- Feature branch: `001-phase1-foundation-services`
- Phase review branches: `git push origin HEAD:001review-Phase1`
- Red/Green evidence: `dotnet test > test-evidence-{red|green}.txt`
- No direct pushes to main/develop
- Architecture review required (foundational infrastructure changes)

**RESULT**: ✅ **PASSES ALL GATES** - Proceed to Phase 0 Research

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Src/UpgradedNorthStar/
├── src/
│   ├── Common/
│   │   ├── NorthStar.Common/                    # Shared kernel (value objects, base entities)
│   │   └── NorthStar.ServiceDefaults/           # Aspire service defaults (health, observability)
│   │
│   ├── Identity/
│   │   ├── NorthStar.Identity.Domain/           # Domain entities (User, Role, RefreshToken)
│   │   ├── NorthStar.Identity.Application/      # CQRS handlers (LoginCommand, RegisterCommand)
│   │   ├── NorthStar.Identity.Infrastructure/   # EF Core, Entra ID, Azure Service Bus
│   │   └── NorthStar.Identity.Api/              # REST API endpoints
│   │
│   ├── Gateway/
│   │   └── NorthStar.Gateway/                   # YARP reverse proxy configuration
│   │
│   ├── Configuration/
│   │   ├── NorthStar.Configuration.Domain/      # Domain entities (District, School, Calendar)
│   │   ├── NorthStar.Configuration.Application/ # CQRS handlers (CreateDistrictCommand, etc.)
│   │   ├── NorthStar.Configuration.Infrastructure/ # EF Core, caching, events
│   │   └── NorthStar.Configuration.Api/         # REST API endpoints
│   │
│   └── AppHost/
│       └── NorthStar.AppHost/                   # Aspire orchestration (Program.cs)
│
├── tests/
│   ├── Identity/
│   │   ├── NorthStar.Identity.UnitTests/        # xUnit unit tests
│   │   ├── NorthStar.Identity.IntegrationTests/ # Aspire integration tests
│   │   └── NorthStar.Identity.BddTests/         # Reqnroll BDD scenarios
│   │
│   ├── Configuration/
│   │   ├── NorthStar.Configuration.UnitTests/
│   │   ├── NorthStar.Configuration.IntegrationTests/
│   │   └── NorthStar.Configuration.BddTests/
│   │
│   └── Gateway/
│       └── NorthStar.Gateway.IntegrationTests/  # Gateway routing tests
│
├── Directory.Packages.props                      # Centralized package versions
├── Directory.Build.props                         # Shared build configuration
└── NorthStar.sln                                # Solution file
```

**Structure Decision**: **Microservices architecture** with Clean Architecture per service

- **Per-Service Structure**: Each microservice (Identity, Configuration) follows Clean Architecture with separate projects for Domain, Application, Infrastructure, and API layers
- **Shared Infrastructure**: `NorthStar.Common` provides shared kernel; `NorthStar.ServiceDefaults` provides Aspire service defaults
- **Gateway**: Standalone YARP reverse proxy (single project - no domain logic)
- **Aspire Orchestration**: `NorthStar.AppHost` orchestrates all services with service discovery and configuration
- **Test Organization**: Test projects mirror source structure with unit, integration (Aspire), and BDD (Reqnroll) test suites per service
- **Centralized Management**: `Directory.Packages.props` manages NuGet package versions across all projects

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**No constitutional violations** - Phase 1 Foundation Services fully complies with all constitutional principles.

All architectural decisions align with:
- Clean Architecture boundaries
- Aspire orchestration patterns
- Event-driven integration
- Security safeguards
- TDD workflow

No complexity justification required.

---

## Implementation Guidance

### Phase 1 Breakdown: Implementation Tasks

The implementation should follow Clean Architecture layer-by-layer with TDD workflow. Tasks are organized by service and architectural layer.

#### Task Organization Principles

1. **Domain First**: Start with pure business logic (no dependencies)
2. **Application Layer**: CQRS commands/queries with validation
3. **Infrastructure**: Database, external services, event publishing
4. **API Layer**: Thin controllers orchestrating application layer
5. **Tests Throughout**: RED → GREEN → Refactor at every step

#### Estimated Timeline: 8 Weeks

- **Week 1-2**: Foundation & Infrastructure Setup
- **Week 3-4**: Identity Service Implementation
- **Week 4-5**: Configuration Service Implementation  
- **Week 5-6**: API Gateway Implementation
- **Week 7**: Integration & Event-Driven Flows
- **Week 8**: Performance Testing & Documentation

---

### Service 1: Identity & Authentication Service

**Priority**: Critical Path (Weeks 3-4)

#### Domain Layer Tasks

**Task 1.1: User Entity & Value Objects**
```
Duration: 4 hours
TDD: UserTests.cs → User.cs

Deliverables:
- User entity with validation rules
- Email value object with RFC 5322 validation
- PasswordHash value object with BCrypt
- UserStatus enumeration
- Domain events: UserRegisteredEvent, PasswordChangedEvent

Acceptance:
- User creation validates email format
- Password hashing uses BCrypt work factor 12
- Account lockout logic after 5 failed attempts
- Domain events raised on state changes
- 100% unit test coverage
```

**Task 1.2: Role Entity & Claim System**
```
Duration: 3 hours
TDD: RoleTests.cs → Role.cs, Claim.cs

Deliverables:
- Role entity with system role flag
- Claim entity for permissions
- RoleType enumeration (system roles)
- UserRole association logic
- Domain events: RoleAssignedEvent, RoleRevokedEvent

Acceptance:
- System roles cannot be deleted
- Claims properly associated with roles/users
- Role assignment validation
- 100% unit test coverage
```

**Task 1.3: RefreshToken & ExternalProviderLink Entities**
```
Duration: 2 hours
TDD: RefreshTokenTests.cs → RefreshToken.cs

Deliverables:
- RefreshToken entity with expiration logic
- ExternalProviderLink for Entra ID
- Token revocation logic
- Token rotation support

Acceptance:
- Tokens auto-expire based on ExpiresAt
- Revoked tokens cannot be reused
- Entra ID link one-per-user validation
- 100% unit test coverage
```

#### Application Layer Tasks

**Task 1.4: Login Command (Local Auth)**
```
Duration: 6 hours
TDD: LoginCommandHandlerTests.cs → LoginCommandHandler.cs

Deliverables:
- LoginCommand with email/password
- LoginCommandValidator (FluentValidation)
- LoginCommandHandler with password verification
- LoginResponse DTO with JWT token
- ITokenService interface
- IUserRepository interface

Acceptance:
- Valid credentials return JWT token + refresh token
- Invalid credentials return 401
- Locked accounts return 423
- Failed attempts increment counter
- Account locks after 5 failures
- ≥85% test coverage
```

**Task 1.5: Entra ID Authentication**
```
Duration: 8 hours
TDD: EntraIdAuthCommandHandlerTests.cs

Deliverables:
- EntraIdAuthCommand
- EntraIdAuthCommandHandler
- IEntraIdService interface
- External provider linkage logic
- Auto-user creation for Entra ID users

Acceptance:
- Entra ID callback creates/updates user
- Claims mapped from Entra ID token
- User linked to ExternalProviderLink
- Local fallback when Entra ID unavailable
- ≥80% test coverage
```

**Task 1.6: User Registration**
```
Duration: 4 hours
TDD: RegisterUserCommandHandlerTests.cs

Deliverables:
- RegisterUserCommand
- RegisterUserCommandValidator
- RegisterUserCommandHandler
- Email uniqueness check
- Password hashing

Acceptance:
- User created with hashed password
- Email confirmation required
- UserRegisteredEvent published
- Duplicate emails rejected (409)
- ≥85% test coverage
```

**Task 1.7: Token Refresh & Logout**
```
Duration: 3 hours
TDD: RefreshTokenCommandHandlerTests.cs

Deliverables:
- RefreshTokenCommand & Handler
- LogoutCommand & Handler
- Token rotation logic
- Token revocation

Acceptance:
- Valid refresh token returns new access token
- Expired/revoked tokens rejected
- Logout revokes all user tokens
- ≥85% test coverage
```

**Task 1.8: User Management Queries**
```
Duration: 4 hours
TDD: GetUserQueryHandlerTests.cs, ListUsersQueryHandlerTests.cs

Deliverables:
- GetUserQuery & Handler
- ListUsersQuery & Handler (paginated)
- GetUserRolesQuery & Handler
- UserDto mapping

Acceptance:
- Users filtered by tenant_id automatically
- Pagination works correctly
- Role information included
- ≥80% test coverage
```

**Task 1.9: Role Assignment Commands**
```
Duration: 3 hours
TDD: AssignRoleCommandHandlerTests.cs

Deliverables:
- AssignRoleCommand & Handler
- RevokeRoleCommand & Handler
- Authorization checks (admin only)

Acceptance:
- Roles assigned to users
- System roles cannot be modified
- RoleAssignedEvent published
- ≥85% test coverage
```

#### Infrastructure Layer Tasks

**Task 1.10: EF Core DbContext & Configurations**
```
Duration: 6 hours
TDD: IdentityDbContextTests.cs (integration tests)

Deliverables:
- IdentityDbContext with DbSets
- Entity configurations (Fluent API)
- Row-Level Security setup
- PostgreSQL conventions
- Initial migration

Acceptance:
- All entities mapped correctly
- Composite keys configured
- Indexes created
- RLS policies applied
- Migration generates correct SQL
```

**Task 1.11: User Repository**
```
Duration: 4 hours
TDD: UserRepositoryTests.cs (integration tests)

Deliverables:
- UserRepository implementing IUserRepository
- GetByIdAsync, GetByEmailAsync methods
- AddAsync, UpdateAsync, DeleteAsync (soft)
- Tenant filtering via TenantDbInterceptor

Acceptance:
- CRUD operations work correctly
- Tenant isolation enforced
- Soft delete sets IsActive=false
- ≥80% integration test coverage
```

**Task 1.12: Token Service (JWT Generation)**
```
Duration: 5 hours
TDD: JwtTokenServiceTests.cs

Deliverables:
- JwtTokenService implementing ITokenService
- JWT generation with claims
- Token validation
- Refresh token generation

Acceptance:
- Tokens include user_id, tenant_id, roles claims
- Tokens expire after configured duration
- Token signature validated
- ≥85% test coverage
```

**Task 1.13: Entra ID Integration**
```
Duration: 8 hours
TDD: EntraIdAuthenticationServiceTests.cs

Deliverables:
- EntraIdAuthenticationService implementing IEntraIdService
- Microsoft.Identity.Web configuration
- Token acquisition
- Claims transformation
- Fallback detection (IsAvailable check)

Acceptance:
- OAuth flow redirects to Entra ID
- Callback exchanges code for token
- Claims extracted from ID token
- Fallback activates on Entra ID errors
- ≥70% test coverage (external dependency)
```

**Task 1.14: Event Publisher (Azure Service Bus)**
```
Duration: 4 hours
TDD: DomainEventPublisherTests.cs

Deliverables:
- DomainEventPublisher implementing IDomainEventPublisher
- MassTransit configuration
- Event serialization
- Retry policies

Acceptance:
- Events published to Azure Service Bus
- Automatic retry on transient failures
- Dead letter queue for failed events
- ≥75% test coverage
```

**Task 1.15: TenantDbInterceptor**
```
Duration: 3 hours
TDD: TenantDbInterceptorTests.cs

Deliverables:
- TenantDbInterceptor for EF Core
- Tenant context from HTTP claims
- SET LOCAL app.tenant_id execution

Acceptance:
- Tenant context automatically injected
- All queries filtered by tenant_id
- Works with RLS policies
- ≥80% test coverage
```

#### API Layer Tasks

**Task 1.16: Authentication Controller**
```
Duration: 5 hours
TDD: BDD scenarios (Reqnroll)

Deliverables:
- AuthenticationController
- POST /api/auth/login
- GET /api/auth/login/entra
- GET /api/auth/callback/entra
- POST /api/auth/refresh
- POST /api/auth/logout
- Error handling middleware

Acceptance:
- All endpoints work end-to-end
- Swagger documentation complete
- Error responses RFC 7807 compliant
- BDD scenarios pass
```

**Task 1.17: Users Controller**
```
Duration: 4 hours
TDD: BDD scenarios (Reqnroll)

Deliverables:
- UsersController
- GET/POST/PUT/DELETE /api/users
- GET/POST/DELETE /api/users/{id}/roles
- Authorization policies

Acceptance:
- CRUD operations authenticated
- Role assignment admin-only
- Multi-tenant isolation verified
- BDD scenarios pass
```

**Task 1.18: Roles Controller**
```
Duration: 2 hours
TDD: BDD scenarios (Reqnroll)

Deliverables:
- RolesController
- GET/POST/DELETE /api/roles
- Admin-only authorization

Acceptance:
- Role CRUD operations work
- System roles protected
- BDD scenarios pass
```

**Task 1.19: Health Checks**
```
Duration: 2 hours
TDD: HealthCheckTests.cs

Deliverables:
- Database health check
- Entra ID health check
- Service Bus health check
- /health endpoint

Acceptance:
- Health check reports service status
- Dependency status included
- Response time < 1 second
```

**Task 1.20: Aspire Integration**
```
Duration: 3 hours
TDD: Aspire integration tests

Deliverables:
- ServiceDefaults registration
- Aspire AppHost configuration
- OpenTelemetry setup
- Distributed tracing

Acceptance:
- Service appears in Aspire Dashboard
- Traces captured correctly
- Logs aggregated
```

---

### Service 2: Configuration Service

**Priority**: High (Weeks 4-5, parallel with Identity completion)

#### Domain Layer Tasks

**Task 2.1: District Entity**
```
Duration: 3 hours
TDD: DistrictTests.cs → District.cs

Deliverables:
- District entity (serves as tenant)
- State value object with validation
- TimeZone value object (IANA zones)
- DistrictCreatedEvent, DistrictUpdatedEvent

Acceptance:
- State validates US state codes
- TimeZone validates IANA zones
- Code uniqueness enforced
- 100% unit test coverage
```

**Task 2.2: School Entity**
```
Duration: 4 hours
TDD: SchoolTests.cs → School.cs

Deliverables:
- School entity with multi-tenant support
- Address value object
- SchoolType enumeration
- SchoolCreatedEvent, SchoolUpdatedEvent

Acceptance:
- School belongs to district
- Address validation complete
- Soft delete supported
- 100% unit test coverage
```

**Task 2.3: Calendar & CalendarDay Entities**
```
Duration: 5 hours
TDD: CalendarTests.cs → Calendar.cs, CalendarDay.cs

Deliverables:
- Calendar entity
- CalendarDay entity
- DayType enumeration
- Calendar validation (start < end, instructional days)
- CalendarUpdatedEvent

Acceptance:
- Calendar date range validation
- Day type business rules enforced
- Instructional day count accurate
- 100% unit test coverage
```

**Task 2.4: GradeLevel Entity**
```
Duration: 2 hours
TDD: GradeLevelTests.cs → GradeLevel.cs

Deliverables:
- GradeLevel entity
- Sequence ordering
- Standard grades (PK, K, 1-12)

Acceptance:
- Sequence unique within district
- Standard grades pre-seeded
- 100% unit test coverage
```

#### Application Layer Tasks

**Task 2.5: District Commands & Queries**
```
Duration: 6 hours
TDD: Command/Query handler tests

Deliverables:
- CreateDistrictCommand & Handler
- UpdateDistrictCommand & Handler
- GetDistrictQuery & Handler
- ListDistrictsQuery & Handler
- Authorization (system admin only for create)

Acceptance:
- Districts created/updated correctly
- Events published
- Multi-tenant isolation
- ≥85% test coverage
```

**Task 2.6: School Commands & Queries**
```
Duration: 6 hours
TDD: Command/Query handler tests

Deliverables:
- CreateSchoolCommand & Handler
- UpdateSchoolCommand & Handler
- DeleteSchoolCommand & Handler (soft)
- GetSchoolQuery & Handler
- ListSchoolsQuery & Handler

Acceptance:
- Schools scoped to district
- Enrollment count cached
- Events published
- ≥85% test coverage
```

**Task 2.7: Calendar Commands & Queries**
```
Duration: 8 hours
TDD: Command/Query handler tests

Deliverables:
- CreateCalendarCommand & Handler
- UpdateCalendarCommand & Handler
- DeleteCalendarCommand & Handler
- AddCalendarDayCommand & Handler
- GetCalendarQuery & Handler (with days)
- ListCalendarsQuery & Handler

Acceptance:
- Calendar validation enforced
- Days within calendar range
- Default calendar logic
- ≥85% test coverage
```

**Task 2.8: GradeLevel Commands & Queries**
```
Duration: 3 hours
TDD: Command/Query handler tests

Deliverables:
- CreateGradeLevelCommand & Handler
- ListGradeLevelsQuery & Handler
- Standard grade seeding

Acceptance:
- Custom grades supported
- Standard grades available
- ≥85% test coverage
```

#### Infrastructure Layer Tasks

**Task 2.9: EF Core DbContext & Configurations**
```
Duration: 5 hours
TDD: ConfigurationDbContextTests.cs (integration)

Deliverables:
- ConfigurationDbContext
- Entity configurations
- Row-Level Security (except districts table)
- Initial migration

Acceptance:
- All entities mapped
- RLS policies applied
- Composite keys with tenant_id
- Migration generates correct SQL
```

**Task 2.10: Repositories**
```
Duration: 6 hours
TDD: Repository integration tests

Deliverables:
- DistrictRepository
- SchoolRepository
- CalendarRepository
- GradeLevelRepository
- Tenant filtering

Acceptance:
- CRUD operations work
- Tenant isolation enforced
- ≥80% integration test coverage
```

**Task 2.11: Caching Layer (Redis)**
```
Duration: 4 hours
TDD: CacheServiceTests.cs

Deliverables:
- ICacheService interface
- RedisCacheService implementation
- District settings caching
- School list caching
- Cache invalidation on updates

Acceptance:
- Cached data returned <10ms
- Cache invalidated on events
- TTL expiration works
- ≥75% test coverage
```

**Task 2.12: Event Publisher & Consumers**
```
Duration: 4 hours
TDD: Event publisher/consumer tests

Deliverables:
- ConfigurationEventPublisher
- DistrictCreatedEvent consumer (Identity service)
- Cache invalidation consumers

Acceptance:
- Events published on changes
- Identity service syncs districts
- Cache invalidated correctly
- ≥75% test coverage
```

#### API Layer Tasks

**Task 2.13: Districts Controller**
```
Duration: 3 hours
TDD: BDD scenarios

Deliverables:
- DistrictsController
- GET/POST/PUT /api/districts
- System admin authorization

Acceptance:
- CRUD operations work
- BDD scenarios pass
```

**Task 2.14: Schools Controller**
```
Duration: 4 hours
TDD: BDD scenarios

Deliverables:
- SchoolsController
- GET/POST/PUT/DELETE /api/schools
- District admin authorization

Acceptance:
- CRUD operations work
- Tenant isolation enforced
- BDD scenarios pass
```

**Task 2.15: Calendars Controller**
```
Duration: 4 hours
TDD: BDD scenarios

Deliverables:
- CalendarsController
- GET/POST/PUT/DELETE /api/calendars
- POST /api/calendars/{id}/days

Acceptance:
- Calendar CRUD works
- Days added/updated
- BDD scenarios pass
```

**Task 2.16: Grades Controller**
```
Duration: 2 hours
TDD: BDD scenarios

Deliverables:
- GradesController
- GET/POST /api/grades

Acceptance:
- Grade levels retrieved
- Custom grades created
- BDD scenarios pass
```

**Task 2.17: Health Checks & Aspire Integration**
```
Duration: 3 hours
TDD: Health check + Aspire tests

Deliverables:
- Health checks (DB, cache, Service Bus)
- ServiceDefaults registration
- Aspire AppHost configuration

Acceptance:
- Service appears in Aspire Dashboard
- Health checks report correctly
```

---

### Service 3: API Gateway (YARP)

**Priority**: Medium (Weeks 5-6)

#### Gateway Tasks

**Task 3.1: YARP Configuration**
```
Duration: 4 hours
TDD: Gateway integration tests

Deliverables:
- appsettings.json route configuration
- Identity cluster configuration
- Configuration cluster configuration
- Legacy cluster configuration
- Dynamic reload support

Acceptance:
- Routes configured correctly
- Service discovery works
- Configuration reloads without restart
```

**Task 3.2: Authentication Middleware**
```
Duration: 4 hours
TDD: Authentication middleware tests

Deliverables:
- JWT validation middleware
- Bearer token extraction
- Claims transformation
- Authorization policies (authenticated, admin-only)

Acceptance:
- Valid tokens pass through
- Invalid tokens rejected (401)
- Claims propagated to backend services
- ≥85% test coverage
```

**Task 3.3: Rate Limiting**
```
Duration: 3 hours
TDD: Rate limiting tests

Deliverables:
- Rate limiter configuration
- Per-client rate limits (100 req/min)
- Rate limit exceeded responses (429)

Acceptance:
- Clients rate limited per identity
- Rate limit headers included
- ≥80% test coverage
```

**Task 3.4: Circuit Breaker**
```
Duration: 3 hours
TDD: Circuit breaker tests

Deliverables:
- Circuit breaker configuration per cluster
- Failure threshold (15 failures/minute)
- Open circuit fallback responses
- Reset interval (5 minutes)

Acceptance:
- Circuit opens on failures
- Requests blocked when open
- Circuit resets after interval
- ≥75% test coverage
```

**Task 3.5: Request Timeout Policies**
```
Duration: 2 hours
TDD: Timeout policy tests

Deliverables:
- Timeout configuration per route
- Default timeout (30 seconds)
- Timeout responses (504)

Acceptance:
- Long requests timeout
- Timeout responses correct
```

**Task 3.6: Health Checks & Routing**
```
Duration: 3 hours
TDD: Health check tests

Deliverables:
- Active health checks per cluster
- Passive health checks (failure detection)
- Unhealthy destination removal
- Health check intervals (10 seconds)

Acceptance:
- Unhealthy services detected
- Traffic routed to healthy destinations
- Reactivation after recovery
```

**Task 3.7: Distributed Tracing**
```
Duration: 2 hours
TDD: Tracing integration tests

Deliverables:
- OpenTelemetry integration
- Trace propagation to backend services
- Correlation ID generation

Acceptance:
- Traces span gateway + backend
- Correlation IDs in logs
- Traces visible in Aspire Dashboard
```

**Task 3.8: Logging & Observability**
```
Duration: 2 hours
TDD: Logging tests

Deliverables:
- Request/response logging
- Performance logging
- Error logging
- Structured logging (JSON)

Acceptance:
- All requests logged
- Response times captured
- Errors logged with context
```

**Task 3.9: Aspire Integration**
```
Duration: 2 hours
TDD: Aspire integration tests

Deliverables:
- ServiceDefaults registration
- Aspire AppHost configuration
- Service discovery integration

Acceptance:
- Gateway appears in Dashboard
- Discovers backend services
```

---

### Cross-Service Integration Tasks

**Priority**: High (Week 7)

**Task 4.1: End-to-End Authentication Flow**
```
Duration: 6 hours
TDD: E2E BDD scenarios

Deliverables:
- User login → JWT token → API call
- Entra ID SSO → User creation → API call
- Token refresh flow
- Multi-tenant isolation verification

Acceptance:
- Complete flow works end-to-end
- Tokens validated at gateway
- Services receive tenant context
- BDD scenarios pass
```

**Task 4.2: Configuration Event Flows**
```
Duration: 4 hours
TDD: Event integration tests

Deliverables:
- District created → Identity syncs
- School created → Cache invalidated
- Calendar updated → Events published

Acceptance:
- Events published correctly
- Consumers process events
- Idempotency verified
```

**Task 4.3: Multi-Tenant Data Isolation Testing**
```
Duration: 6 hours
TDD: Security integration tests

Deliverables:
- Cross-tenant access attempts
- RLS policy verification
- Application-level filtering verification
- Penetration testing scenarios

Acceptance:
- Cross-tenant access blocked
- Security violations logged
- Zero data leakage
```

**Task 4.4: Performance Testing**
```
Duration: 8 hours
TDD: Load tests (K6 or JMeter)

Deliverables:
- Authentication performance (<50ms P95)
- Gateway routing performance (<10ms)
- Configuration queries (<100ms cached P95)
- Concurrent user testing (1000+ users)
- Load test reports

Acceptance:
- All SLOs met under load
- No memory leaks
- Graceful degradation
```

**Task 4.5: Resilience Testing**
```
Duration: 4 hours
TDD: Chaos engineering tests

Deliverables:
- Database connection failures
- Service Bus outages
- Entra ID unavailability
- Circuit breaker activation

Acceptance:
- Services handle failures gracefully
- Fallbacks activate correctly
- No cascading failures
```

---

### Documentation & Finalization

**Priority**: Medium (Week 8)

**Task 5.1: API Documentation**
```
Duration: 4 hours

Deliverables:
- OpenAPI specs complete (Swagger UI)
- API usage examples
- Authentication guide
- Error code documentation

Acceptance:
- All endpoints documented
- Examples work correctly
```

**Task 5.2: Deployment Documentation**
```
Duration: 4 hours

Deliverables:
- Azure infrastructure guide
- Kubernetes deployment manifests
- CI/CD pipeline documentation
- Environment configuration guide

Acceptance:
- Deployment runbook complete
- CI/CD pipeline tested
```

**Task 5.3: Developer Onboarding Guide**
```
Duration: 3 hours

Deliverables:
- Quickstart guide updates
- Architecture decision records (ADRs)
- Troubleshooting guide
- FAQs

Acceptance:
- New developers can start quickly
- Common issues documented
```

**Task 5.4: Security Documentation**
```
Duration: 3 hours

Deliverables:
- Security architecture document
- Threat model
- Secrets management guide
- Compliance checklist

Acceptance:
- Security posture documented
- Audit ready
```

**Task 5.5: Operational Runbooks**
```
Duration: 4 hours

Deliverables:
- Incident response procedures
- Monitoring and alerting setup
- Backup and recovery procedures
- Scaling guidelines

Acceptance:
- Operations team trained
- Runbooks tested
```

---

## Implementation Checklist

### Pre-Implementation
- [ ] Repository cloned
- [ ] .NET 10 SDK installed
- [ ] Aspire workload installed
- [ ] Docker Desktop running
- [ ] Azure resources provisioned
- [ ] Secrets configured

### Phase 0: Research ✅
- [x] Research document complete
- [x] Technical patterns decided
- [x] Constitution check passed

### Phase 1: Design ✅
- [x] Data model document complete
- [x] API contracts (OpenAPI) generated
- [x] Quickstart guide created
- [x] Agent context updated

### Phase 2: Implementation (Week 3-8)
- [ ] Identity Service (19 tasks)
- [ ] Configuration Service (17 tasks)
- [ ] API Gateway (9 tasks)
- [ ] Cross-service integration (5 tasks)
- [ ] Documentation (5 tasks)

### Quality Gates
- [ ] ≥80% test coverage all services
- [ ] All BDD scenarios passing
- [ ] All Aspire integration tests passing
- [ ] Performance SLOs met
- [ ] Security audit passed
- [ ] Multi-tenant isolation verified

### Phase Review
- [ ] Code review completed
- [ ] Architecture review (if needed)
- [ ] Constitution compliance verified
- [ ] Evidence attached to PR
- [ ] Pushed to `001review-Phase1`

---

## Success Metrics

### Quantitative
- ✅ 3 services deployed and healthy
- ✅ ≥80% test coverage (unit + integration + BDD)
- ✅ <50ms authentication decision (P95)
- ✅ <10ms gateway routing overhead (P95)
- ✅ <100ms configuration query (cached, P95)
- ✅ 1000+ concurrent users supported
- ✅ Zero hardcoded credentials
- ✅ Zero cross-tenant data leakage

### Qualitative
- ✅ Clean Architecture boundaries respected
- ✅ TDD workflow followed (Red → Green evidence)
- ✅ Event-driven integration working
- ✅ Aspire orchestration functional
- ✅ Swagger documentation complete
- ✅ Operational runbooks ready

---

## Next Steps After Phase 1

1. **Phase 2 Planning** - Student, Staff, Assessment services
2. **UI Migration** - Angular/Blazor frontend (parallel with Phase 3-4)
3. **Data Migration** - Legacy database ETL (parallel with Phase 2)
4. **Advanced Features** - MFA, advanced reporting (Phase 4)

---

**Implementation plan complete. Ready for /speckit.tasks command to generate detailed task breakdown.**
