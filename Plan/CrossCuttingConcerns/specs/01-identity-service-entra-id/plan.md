# Implementation Plan: Identity Service with Microsoft Entra ID Authentication

**Feature ID**: `01-identity-service-entra-id`  
**Target Layer**: CrossCuttingConcerns (Foundation Service)  
**Service**: Identity & Authentication Service  
**Specification**: [spec.md](./spec.md)  
**Architecture Reference**: [Identity Service Architecture](../../architecture/services/identity-service.md)  
**Plan Version**: 1.0.0  
**Created**: 2025-11-20  
**Status**: Ready for Implementation

---

## Executive Summary

This plan implements the Identity Service as a foundational microservice providing Microsoft Entra ID-based authentication, session management, and role-based authorization for all NorthStar LMS users. The implementation follows Clean Architecture principles with .NET Aspire orchestration and delivers authentication flows, token management, multi-tenant context switching, and comprehensive security auditing.

**Success Criteria**:
- Staff and administrators authenticate via Microsoft Entra ID SSO
- Sessions managed with 8-hour sliding window (staff) / 1-hour (admin)
- Authorization decisions complete in <50ms (P95)
- ≥80% test coverage with Red→Green evidence for all phases
- Zero security incidents during initial deployment

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Technology Stack](#technology-stack)
- [Implementation Phases](#implementation-phases)
- [Service Structure](#service-structure)
- [Data Model](#data-model)
- [API Contracts](#api-contracts)
- [Testing Strategy](#testing-strategy)
- [Infrastructure Setup](#infrastructure-setup)
- [Security Considerations](#security-considerations)
- [Performance Optimization](#performance-optimization)
- [Deployment Strategy](#deployment-strategy)
- [Open Questions & Risks](#open-questions--risks)

---

## Architecture Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        User (Browser)                            │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                ┌───────────▼────────────┐
                │  Microsoft Entra ID    │
                │  (OAuth 2.0/OIDC)      │
                └───────────┬────────────┘
                            │ JWT Token
                ┌───────────▼────────────┐
                │  NorthStar Web App     │
                │  (Razor Pages)         │
                └───────────┬────────────┘
                            │ POST /api/auth/exchange-token
                ┌───────────▼────────────┐
                │  API Gateway (YARP)    │
                └───────────┬────────────┘
                            │
                ┌───────────▼────────────┐
                │  Identity.API          │
                │  - SessionAuth         │
                │  - Token Validation    │
                └─────┬──────────┬───────┘
                      │          │
         ┌────────────▼──┐   ┌──▼─────────────┐
         │  PostgreSQL   │   │  Redis Cache   │
         │  (Sessions,   │   │  (Session      │
         │   Users,      │   │   Cache)       │
         │   Roles)      │   │                │
         └───────────────┘   └────────────────┘
                      │
         ┌────────────▼─────────────┐
         │  Azure Service Bus       │
         │  (Domain Events)         │
         └──────────────────────────┘
```

### Authentication Flow (Backend-for-Frontend Pattern)

1. **User clicks "Sign in with Microsoft"** → Web redirects to Entra ID `/authorize`
2. **Entra ID authenticates user** → Returns authorization code
3. **Web exchanges code for JWT** → Receives `access_token`, `refresh_token`, `id_token`
4. **Web calls Identity.API** → `POST /api/auth/exchange-token` with Entra Bearer token
5. **Identity.API validates token** → Microsoft.Identity.Web verifies signature, issuer, audience
6. **API creates LMS session** → Insert into PostgreSQL + cache in Redis
7. **API returns session ID** → Web stores in HTTP-only, secure cookie
8. **Subsequent requests** → SessionAuthenticationHandler validates session from Redis/DB

### Clean Architecture Layers

```
Identity.API/                           # Presentation Layer
├── Controllers/
│   ├── AuthenticationController.cs    # /api/auth/* endpoints
│   └── AuthorizationController.cs     # /api/auth/claims, /switch-tenant
├── Middleware/
│   ├── SessionAuthenticationHandler.cs  # Custom auth handler
│   └── ExceptionHandlerMiddleware.cs
└── Program.cs                          # Startup, Aspire registration

Identity.Application/                   # Application Layer (Use Cases)
├── Commands/
│   ├── ExchangeTokenCommand.cs         # Entra JWT → LMS session
│   ├── RefreshSessionCommand.cs        # Token refresh
│   ├── LogoutCommand.cs                # Session termination
│   └── SwitchTenantCommand.cs          # Multi-tenant context switch
├── Queries/
│   ├── GetSessionQuery.cs              # Current session info
│   ├── GetUserClaimsQuery.cs           # Roles & permissions
│   └── ValidateSessionQuery.cs         # Session validation
├── Interfaces/
│   ├── ITokenValidator.cs              # Entra ID token validation
│   ├── ISessionManager.cs              # Session lifecycle
│   └── IAuthorizationService.cs        # Permission checks
├── DTOs/
│   ├── AuthenticationRequest.cs
│   ├── AuthenticationResponse.cs
│   └── UserClaimsResponse.cs
└── Validators/
    └── ExchangeTokenValidator.cs       # FluentValidation

Identity.Domain/                        # Domain Layer (Core Logic)
├── Entities/
│   ├── User.cs                         # Aggregate root
│   ├── Session.cs                      # Session entity
│   ├── Role.cs                         # RBAC role
│   ├── ExternalProviderLink.cs         # Entra ID linkage
│   └── AuditRecord.cs                  # Security audit
├── Events/
│   ├── UserAuthenticatedEvent.cs       # Published on login
│   ├── UserLoggedOutEvent.cs           # Published on logout
│   ├── SessionRefreshedEvent.cs        # Published on token refresh
│   └── TenantContextSwitchedEvent.cs   # Published on tenant switch
├── ValueObjects/
│   ├── SessionId.cs                    # "lms_session_{guid}"
│   ├── EntraSubjectId.cs               # Entra "sub" claim
│   └── TenantId.cs                     # District identifier
└── Exceptions/
    ├── InvalidTokenException.cs
    └── SessionExpiredException.cs

Identity.Infrastructure/                # Infrastructure Layer
├── Data/
│   ├── IdentityDbContext.cs            # EF Core context
│   ├── Configurations/
│   │   ├── UserConfiguration.cs        # Entity mapping
│   │   ├── SessionConfiguration.cs
│   │   └── RoleConfiguration.cs
│   ├── Migrations/                     # EF Core migrations
│   └── Repositories/
│       ├── UserRepository.cs
│       ├── SessionRepository.cs
│       └── RoleRepository.cs
├── Identity/
│   ├── EntraIdTokenValidator.cs        # Microsoft.Identity.Web wrapper
│   ├── SessionManager.cs               # Session CRUD + Redis cache
│   └── TokenRefreshBackgroundService.cs  # Hosted service
├── Caching/
│   ├── RedisCacheService.cs            # Redis abstraction
│   └── SessionCacheKeyBuilder.cs       # Cache key formatting
├── Messaging/
│   └── DomainEventPublisher.cs         # MassTransit integration
└── DependencyInjection.cs              # Infrastructure registration
```

---

## Technology Stack

### Core Framework
- **.NET 10**: Latest LTS, C# 12 features (record types, file-scoped namespaces)
- **ASP.NET Core**: Web API framework
- **.NET Aspire 10.0**: Orchestration, service discovery, health checks

### Authentication & Authorization
- **Microsoft.Identity.Web 3.3.0**: Official Entra ID integration library
  - JWT token validation (RS256 signature verification)
  - Claims transformation
  - Token refresh logic
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT middleware
- **Custom SessionAuthenticationHandler**: Hybrid session-based auth

### Data Persistence
- **Entity Framework Core 9.0**: ORM
- **Npgsql.EntityFrameworkCore.PostgreSQL 9.0**: PostgreSQL provider
- **PostgreSQL 17**: RDBMS (per-service database pattern)

### Caching
- **StackExchange.Redis 2.8.16**: Redis client
- **Redis Stack 7.4**: In-memory cache (session storage, idempotency)

### Messaging
- **MassTransit 8.3.3**: Message bus abstraction
- **Azure Service Bus**: Production messaging (local: RabbitMQ)

### CQRS & Validation
- **MediatR 12.4.1**: Command/query mediator pattern
- **FluentValidation 11.10.0**: Input validation

### Resilience
- **Polly 8.4.2**: Circuit breaker, retry policies

### Testing
- **xUnit 2.9.0**: Unit test framework
- **Moq 4.20.72**: Mocking library
- **FluentAssertions 6.12.1**: Assertion library
- **Reqnroll 2.2.0**: BDD testing (Gherkin syntax)
- **Testcontainers 3.10.0**: Docker-based integration tests (PostgreSQL, Redis)

### Observability
- **OpenTelemetry 1.9.0**: Distributed tracing, metrics
- **Serilog 4.0.2**: Structured logging
- **Seq**: Log aggregation (dev), ELK stack (prod)

---

## Implementation Phases

### Phase 1: Core Authentication Flow (P1 - MVP) [Weeks 1-2]

**Goal**: Enable staff members to log in with Microsoft Entra ID and access authenticated content.

**Deliverables**:
- Entra ID app registrations configured (Web + API)
- OAuth 2.0/OIDC integration with `Microsoft.Identity.Web`
- `POST /api/auth/exchange-token` endpoint (Entra JWT → LMS session)
- Session creation in PostgreSQL + Redis cache
- `SessionAuthenticationHandler` for session validation
- `GET /api/auth/session` endpoint (current session info)

**User Stories**: US-1 (Staff member logs in via Entra ID SSO)

**Tasks**:
1. **Setup Entra ID Applications**
   - Register "NorthStar LMS Web" app (OAuth client)
   - Register "NorthStar LMS API" app (resource server)
   - Configure redirect URIs, scopes, client secrets
   - Store secrets in Azure Key Vault

2. **Create Domain Entities**
   - `User` entity (aggregate root)
   - `Session` entity
   - `UserAuthenticatedEvent` domain event
   - Value objects: `SessionId`, `EntraSubjectId`, `TenantId`

3. **Implement Token Exchange**
   - `ExchangeTokenCommand` + handler
   - `EntraIdTokenValidator` (wraps Microsoft.Identity.Web)
   - Token validation: signature (RS256), issuer, audience, expiration
   - Extract claims: `sub`, `email`, `name`, `roles`

4. **Build Session Management**
   - `SessionManager` class (CRUD operations)
   - `IdentityDbContext` + EF Core migrations
   - `SessionRepository` implementation
   - Redis cache integration (write-through pattern)
   - Session ID generation: `lms_session_{guid}`

5. **Implement Session Authentication**
   - `SessionAuthenticationHandler` (custom authentication handler)
   - Cookie configuration: HTTP-only, secure, SameSite=Strict
   - Session validation flow: Redis → PostgreSQL fallback
   - Populate `ClaimsPrincipal` with user_id, tenant_id, roles

6. **Build API Endpoints**
   - `AuthenticationController` with token exchange endpoint
   - `GET /api/auth/session` endpoint
   - Exception handling middleware
   - OpenAPI documentation (Swagger)

7. **Testing**
   - Unit tests: `ExchangeTokenCommandHandlerTests`, `SessionManagerTests`
   - Integration tests: `TokenExchangeIntegrationTests` (Testcontainers)
   - BDD: `UserLogin.feature` (Reqnroll)
   - Aspire orchestration test: Verify service health

**Evidence Capture**:
```bash
# Red phase
dotnet test --project tests/Identity.Application.Tests > evidence/phase1-red-unit.txt
dotnet test --project tests/Identity.IntegrationTests > evidence/phase1-red-integration.txt

# Green phase (after implementation)
dotnet test --project tests/Identity.Application.Tests > evidence/phase1-green-unit.txt
dotnet test --project tests/Identity.IntegrationTests > evidence/phase1-green-integration.txt
```

**Phase Review Checkpoint**: Push to `CrossCuttingConcerns/01-identity-service-entra-id-spec-review-Phase1`

---

### Phase 2: Session Management & Logout (P1 - Security) [Week 3]

**Goal**: Implement session lifecycle with automatic token refresh and secure logout.

**Deliverables**:
- Token refresh background service
- Sliding session window (8h staff, 1h admin)
- `POST /api/auth/logout` endpoint
- Entra ID logout redirect
- Session termination (DB + Redis)

**User Stories**: US-4 (Token refresh), US-8 (Session termination)

**Tasks**:
1. **Token Refresh Background Service**
   - `TokenRefreshBackgroundService` (HostedService)
   - Query sessions expiring within 5 minutes (every 4 minutes)
   - Call Entra ID token refresh endpoint
   - Update session `expires_at` and `refreshed_at` columns
   - Publish `SessionRefreshedEvent`
   - Handle failures: retry 3 times, logout on exhaustion

2. **Sliding Session Window**
   - Session expiration calculation: 8h staff, 1h admin
   - Update `expires_at` on each request (activity-based sliding)
   - Redis TTL management (match DB expiration)

3. **Logout Flow**
   - `LogoutCommand` + handler
   - Set session `expires_at = NOW()`
   - Delete session from Redis
   - Clear session cookie
   - Return Entra ID logout URL
   - Publish `UserLoggedOutEvent`

4. **Testing**
   - Unit tests: `TokenRefreshBackgroundServiceTests`, `LogoutCommandHandlerTests`
   - Integration tests: `TokenRefreshIntegrationTests`
   - BDD: `TokenRefresh.feature`, `UserLogout.feature`

**Phase Review Checkpoint**: Push to branch with `-review-Phase2` suffix

---

### Phase 3: Role-Based Authorization (P1 - Authorization) [Week 4]

**Goal**: Enable fine-grained access control with role-based permissions and fast authorization checks.

**Deliverables**:
- Role and permission schema (Roles, UserRoles tables)
- `GET /api/auth/claims` endpoint
- Authorization handler with <50ms P95 latency
- Permission caching in Redis
- Cache invalidation on role changes

**User Stories**: US-7 (Role-based authorization check)

**Tasks**:
1. **Role Domain Model**
   - `Role` entity (RoleName, Permissions JSONB)
   - `UserRole` link table
   - `UserRoleAssignedEvent`, `UserRoleRevokedEvent`

2. **Authorization Service**
   - `AuthorizationService` implementation
   - Permission lookup: user_id → roles → permissions
   - Redis caching (key: `lms_permissions:{user_id}:{tenant_id}`)
   - Cache TTL: 1 hour or until role change event

3. **Claims Endpoint**
   - `GET /api/auth/claims` → returns `{ userId, roles, permissions, tenantIds }`
   - Used by client apps to show/hide UI elements

4. **Event Handlers**
   - `UserRoleAssignedEventHandler`: Invalidate permission cache
   - `UserRoleRevokedEventHandler`: Invalidate permission cache

5. **Testing**
   - Unit tests: `AuthorizationServiceTests`, `RoleTests`
   - Integration tests: `AuthorizationIntegrationTests`
   - BDD: `Authorization.feature`
   - Performance test: Verify <50ms P95 latency

**Phase Review Checkpoint**: Push to branch with `-review-Phase3` suffix

---

### Phase 4: Failed Auth & MFA (P1 - Security Edge Cases) [Week 5]

**Goal**: Comprehensive security audit logging and failed authentication handling.

**Deliverables**:
- Failed authentication logging to `AuditRecords`
- MFA validation (delegated to Entra ID)
- Administrator 1-hour session timeout enforcement
- Security event monitoring

**User Stories**: US-2 (Administrator MFA login), US-10 (Failed authentication handling)

**Tasks**:
1. **Audit Logging**
   - `AuditRecord` entity
   - `AuditRepository` implementation
   - Log all auth events: login (success/failure), logout, session refresh, role changes
   - Fields: userId, eventType, tenantId, ipAddress, timestamp, details (JSONB)

2. **Failed Authentication Handling**
   - Log failed attempts with IP address and timestamp
   - Rely on Entra ID lockout policies (5 attempts → 30-minute lock)
   - Display generic error messages (don't reveal email vs password)

3. **MFA Validation**
   - Entra ID conditional access policies handle MFA
   - Verify MFA claims in JWT token
   - No custom MFA logic in NorthStar

4. **Admin Session Timeout**
   - Differentiate session duration by role (1h admin, 8h staff)
   - Background service enforces timeout

5. **Testing**
   - Unit tests: `AuditRecordTests`, `FailedAuthenticationHandlerTests`
   - Integration tests: `SecurityAuditTests`
   - BDD: `FailedAuthentication.feature`, `AdminMfa.feature`

**Phase Review Checkpoint**: Push to branch with `-review-Phase4` suffix

---

### Phase 5: Multi-Tenant Context Switching (P2) [Week 6]

**Goal**: Enable users with access to multiple districts to switch tenant context seamlessly.

**Deliverables**:
- `POST /api/auth/switch-tenant` endpoint
- Tenant authorization validation
- Session context update (DB + Redis)
- `TenantContextSwitchedEvent` publishing
- P95 latency <200ms

**User Stories**: US-5 (Cross-district access with tenant switching)

**Tasks**:
1. **Tenant Switch Command**
   - `SwitchTenantCommand` + handler
   - Validate user has access to target tenant
   - Update `session.tenant_id`
   - Invalidate permission cache for old tenant
   - Publish `TenantContextSwitchedEvent`

2. **Tenant Authorization**
   - Query user's assigned tenants
   - Return 403 Forbidden if no access to target tenant
   - Log failed attempts

3. **Performance Optimization**
   - Use indexed queries on `user_roles.user_id` + `user_roles.tenant_id`
   - Cache tenant list per user

4. **Testing**
   - Unit tests: `SwitchTenantCommandHandlerTests`
   - Integration tests: `TenantSwitchIntegrationTests`
   - BDD: `TenantSwitch.feature`
   - Performance test: Verify <200ms P95

**Phase Review Checkpoint**: Push to branch with `-review-Phase5` suffix

---

### Phase 6: IdentityServer User Migration (P2) [Week 7]

**Goal**: Migrate existing users from legacy IdentityServer to Entra ID authentication.

**Deliverables**:
- User matching script (email-based)
- `ExternalProviderLink` creation
- Role/permission preservation
- Migration report with unmatched users
- Manual review workflow

**User Stories**: US-3 (Entra ID configuration and user provisioning)

**Tasks**:
1. **Migration Script**
   - Query legacy IdentityServer database
   - Match users by email to Entra ID accounts
   - Create `ExternalProviderLink` records
   - Preserve roles, permissions, tenant associations
   - Mark legacy passwords as deprecated

2. **Unmatched User Handling**
   - Generate CSV report of unmatched users
   - Admin UI for manual linking
   - Send migration notification emails

3. **Rollback Plan**
   - Database backup before migration
   - Rollback script to remove `ExternalProviderLinks`

4. **Testing**
   - Unit tests: `UserMigrationServiceTests`
   - Integration tests: `MigrationIntegrationTests` (test database)
   - Dry-run on production copy

**Phase Review Checkpoint**: Push to branch with `-review-Phase6` suffix

---

### Phase 7: Service-to-Service Auth (P2) [Week 8]

**Goal**: Enable secure communication between microservices using service principals.

**Deliverables**:
- Service principal registration in Entra ID
- Service-to-service token validation
- Tenant context propagation in service calls
- Audit logging for service calls

**User Stories**: US-9 (Service-to-service authentication)

**Tasks**:
1. **Service Principal Setup**
   - Register service principals for each microservice
   - Grant API permissions (access_as_user scope)
   - Store client credentials in Azure Key Vault

2. **Service Token Validation**
   - `ServicePrincipalAuthenticationHandler`
   - Validate service principal tokens
   - Extract service identity and tenant context

3. **Tenant Context Propagation**
   - Include `X-Tenant-Id` header in service calls
   - Validate tenant context matches token claims

4. **Testing**
   - Unit tests: `ServicePrincipalAuthenticationHandlerTests`
   - Integration tests: `ServiceToServiceTests`
   - BDD: `ServiceAuthentication.feature`

**Phase Review Checkpoint**: Push to branch with `-review-Phase7` suffix

---

### Phase 8: Password Reset & Advanced Features (P3) [Week 9]

**Goal**: Polish user experience with password reset delegation and advanced session security.

**Deliverables**:
- Password reset delegation to Entra ID
- Optional IP address binding (configurable)
- Optional device fingerprinting
- Concurrent session limits

**User Stories**: US-6 (Password reset flow via Entra ID)

**Tasks**:
1. **Password Reset Delegation**
   - "Forgot Password" link redirects to Entra ID password reset
   - Log password reset events in NorthStar
   - Invalidate existing sessions on password change

2. **Advanced Session Security**
   - IP address binding (optional, configurable per tenant)
   - Device fingerprinting using User-Agent + client hints
   - Concurrent session limits (configurable)
   - Anomaly detection logging (unusual login times/locations)

3. **Testing**
   - Unit tests: `PasswordResetTests`, `SessionSecurityTests`
   - Integration tests: `AdvancedSecurityTests`
   - BDD: `PasswordReset.feature`

**Phase Review Checkpoint**: Push to branch with `-review-Phase8` suffix

---

## Service Structure

### Project Organization

```
Src/Foundation/services/Identity/
├── Identity.API/
│   ├── Controllers/
│   │   ├── AuthenticationController.cs
│   │   └── AuthorizationController.cs
│   ├── Middleware/
│   │   ├── SessionAuthenticationHandler.cs
│   │   └── ExceptionHandlerMiddleware.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Identity.API.csproj
│
├── Identity.Application/
│   ├── Commands/
│   │   ├── ExchangeTokenCommand.cs
│   │   ├── ExchangeTokenCommandHandler.cs
│   │   ├── RefreshSessionCommand.cs
│   │   ├── RefreshSessionCommandHandler.cs
│   │   ├── LogoutCommand.cs
│   │   ├── LogoutCommandHandler.cs
│   │   ├── SwitchTenantCommand.cs
│   │   └── SwitchTenantCommandHandler.cs
│   ├── Queries/
│   │   ├── GetSessionQuery.cs
│   │   ├── GetSessionQueryHandler.cs
│   │   ├── GetUserClaimsQuery.cs
│   │   ├── GetUserClaimsQueryHandler.cs
│   │   ├── ValidateSessionQuery.cs
│   │   └── ValidateSessionQueryHandler.cs
│   ├── Interfaces/
│   │   ├── ITokenValidator.cs
│   │   ├── ISessionManager.cs
│   │   ├── IAuthorizationService.cs
│   │   └── IAuditService.cs
│   ├── DTOs/
│   │   ├── AuthenticationRequest.cs
│   │   ├── AuthenticationResponse.cs
│   │   ├── SessionInfo.cs
│   │   ├── UserClaimsResponse.cs
│   │   └── TenantSwitchRequest.cs
│   ├── Validators/
│   │   ├── ExchangeTokenValidator.cs
│   │   └── SwitchTenantValidator.cs
│   ├── DependencyInjection.cs
│   └── Identity.Application.csproj
│
├── Identity.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Session.cs
│   │   ├── Role.cs
│   │   ├── ExternalProviderLink.cs
│   │   └── AuditRecord.cs
│   ├── Events/
│   │   ├── UserAuthenticatedEvent.cs
│   │   ├── UserLoggedOutEvent.cs
│   │   ├── SessionRefreshedEvent.cs
│   │   ├── TenantContextSwitchedEvent.cs
│   │   ├── UserRoleAssignedEvent.cs
│   │   └── UserRoleRevokedEvent.cs
│   ├── ValueObjects/
│   │   ├── SessionId.cs
│   │   ├── EntraSubjectId.cs
│   │   └── TenantId.cs
│   ├── Exceptions/
│   │   ├── InvalidTokenException.cs
│   │   ├── SessionExpiredException.cs
│   │   └── UnauthorizedTenantAccessException.cs
│   └── Identity.Domain.csproj
│
└── Identity.Infrastructure/
    ├── Data/
    │   ├── IdentityDbContext.cs
    │   ├── Configurations/
    │   │   ├── UserConfiguration.cs
    │   │   ├── SessionConfiguration.cs
    │   │   ├── RoleConfiguration.cs
    │   │   ├── ExternalProviderLinkConfiguration.cs
    │   │   └── AuditRecordConfiguration.cs
    │   ├── Migrations/
    │   └── Repositories/
    │       ├── UserRepository.cs
    │       ├── SessionRepository.cs
    │       ├── RoleRepository.cs
    │       └── AuditRepository.cs
    ├── Identity/
    │   ├── EntraIdTokenValidator.cs
    │   ├── SessionManager.cs
    │   ├── AuthorizationService.cs
    │   └── TokenRefreshBackgroundService.cs
    ├── Caching/
    │   ├── RedisCacheService.cs
    │   └── SessionCacheKeyBuilder.cs
    ├── Messaging/
    │   ├── DomainEventPublisher.cs
    │   └── EventHandlers/
    │       ├── UserRoleAssignedEventHandler.cs
    │       └── UserRoleRevokedEventHandler.cs
    ├── DependencyInjection.cs
    └── Identity.Infrastructure.csproj
```

### Aspire AppHost Registration

```csharp
// Src/Foundation/AppHost/Program.cs

var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure resources
var postgres = builder.AddPostgres("NorthStarPostgres")
    .WithPgAdmin()
    .AddDatabase("IdentityDb");

var redis = builder.AddRedis("NorthStarRedis")
    .WithRedisCommander();

var serviceBus = builder.AddAzureServiceBus("NorthStarServiceBus");

// Identity Service (Foundation Service)
var identityApi = builder.AddProject<Projects.Identity_API>("identity-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(serviceBus)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WithHttpsEndpoint(port: 5001, targetPort: 5001, name: "https");

// Other services depend on Identity
builder.AddProject<Projects.Student_API>("student-api")
    .WithReference(identityApi)
    .WaitFor(identityApi);

builder.AddProject<Projects.Staff_API>("staff-api")
    .WithReference(identityApi)
    .WaitFor(identityApi);

builder.Build().Run();
```

---

## Data Model

See [data-model.md](./data-model.md) for complete entity definitions, relationships, and database schema.

**Quick Summary**:
- **Users**: Core user accounts with tenant isolation
- **Sessions**: Active sessions with Entra ID linkage
- **Roles**: RBAC role definitions with JSONB permissions
- **UserRoles**: Many-to-many link between users and roles (per tenant)
- **ExternalProviderLinks**: Entra ID subject mapping
- **AuditRecords**: Security event logging

**Database Naming**: `NorthStar_Identity_DB` (PostgreSQL)

**Multi-Tenancy**: All tables include `tenant_id` column with global query filter enforcement via `TenantInterceptor`.

---

## API Contracts

See [contracts/](./contracts/) directory for complete OpenAPI specifications.

**Quick Summary**:

### Authentication Endpoints
- `POST /api/auth/exchange-token` - Exchange Entra JWT for LMS session
- `POST /api/auth/refresh` - Manually trigger token refresh
- `POST /api/auth/logout` - Terminate session and get Entra logout URL
- `GET /api/auth/session` - Get current session info

### Authorization Endpoints
- `GET /api/auth/claims` - Get user's roles and permissions
- `POST /api/auth/switch-tenant` - Switch active tenant context

### Admin Endpoints (Future)
- `POST /api/admin/users/{userId}/roles` - Assign role to user
- `DELETE /api/admin/users/{userId}/roles/{roleId}` - Revoke role from user

**Error Format**: RFC 7807 Problem Details

**Authentication**: Bearer token (Entra ID JWT) for token exchange; Session cookie for all other endpoints

---

## Testing Strategy

### Test Coverage Target

≥80% across all layers per Constitution Principle 2.

### Unit Tests (70% of total)

**Domain Layer**:
- `UserTests`: Aggregate root behavior, soft delete
- `SessionTests`: Expiration calculation, refresh logic
- `RoleTests`: Permission JSONB serialization
- `SessionIdTests`: Format validation (`lms_session_{guid}`)

**Application Layer**:
- `ExchangeTokenCommandHandlerTests`: Token validation, session creation
- `RefreshSessionCommandHandlerTests`: Expiration extension logic
- `LogoutCommandHandlerTests`: Session termination
- `GetUserClaimsQueryHandlerTests`: Permission aggregation
- All handlers use mocked repositories (Moq)

**Infrastructure Layer**:
- `EntraIdTokenValidatorTests`: JWT signature validation (mocked OIDC endpoint)
- `SessionManagerTests`: Redis cache read-through logic
- `AuthorizationServiceTests`: Permission lookup with cache

### Integration Tests (15% of total)

**Database Tests**:
- `IdentityDbContextTests`: Migrations apply cleanly, tenant isolation enforced
- `UserRepositoryTests`: CRUD operations with Testcontainers PostgreSQL
- `SessionRepositoryTests`: Soft delete queries

**Redis Tests**:
- `SessionCacheIntegrationTests`: TTL management, eviction
- `PermissionCacheTests`: Cache invalidation on role changes

**Entra ID Integration Tests**:
- `TokenExchangeIntegrationTests`: Full OAuth flow with mock OIDC server

**Aspire Tests**:
- `IdentityServiceHealthTests`: Service starts, PostgreSQL/Redis healthy
- `ServiceDiscoveryTests`: Identity API registered with Aspire

### BDD Tests (10% of total - Reqnroll)

**Feature Files** (in `specs/01-identity-service-entra-id/features/`):

1. **UserLogin.feature**
   ```gherkin
   Feature: User Login with Microsoft Entra ID
     As a staff member
     I want to log in using my Microsoft account
     So that I can access the NorthStar LMS
   
     Scenario: Successful login with valid credentials
       Given the Identity Service is running
       And I have a valid Entra ID account
       When I navigate to the NorthStar login page
       And I click "Sign in with Microsoft"
       And I authenticate with Entra ID
       Then I should be redirected to my dashboard
       And my session should be created
       And I should see my name in the navigation
   ```

2. **TokenRefresh.feature**
3. **TenantSwitch.feature**
4. **Authorization.feature**
5. **UserLogout.feature**
6. **FailedAuthentication.feature**

### UI Tests (5% of total - Playwright)

**Critical User Journeys**:
- End-to-end login flow
- Session expiration handling
- Tenant switching UX
- Logout and re-authentication

### Red→Green Evidence Capture (Mandatory)

**Per Phase**:
```bash
# Red phase (before implementation)
dotnet test --project tests/Identity.Application.Tests --verbosity normal > evidence/phase1-red-unit.txt
dotnet test --project tests/Identity.IntegrationTests --verbosity normal > evidence/phase1-red-integration.txt
dotnet test --project tests/Identity.BddTests --verbosity normal > evidence/phase1-red-bdd.txt

# Green phase (after implementation)
dotnet test --project tests/Identity.Application.Tests --verbosity normal > evidence/phase1-green-unit.txt
dotnet test --project tests/Identity.IntegrationTests --verbosity normal > evidence/phase1-green-integration.txt
dotnet test --project tests/Identity.BddTests --verbosity normal > evidence/phase1-green-bdd.txt

# Coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report"
```

**Phase Review Requirement**: All 6 transcript files (3 red + 3 green) attached before push.

---

## Infrastructure Setup

### Prerequisites

- Azure subscription with Entra ID tenant
- .NET 10 SDK
- PostgreSQL 17 (via Aspire)
- Redis Stack 7.4 (via Aspire)
- Docker Desktop (for Testcontainers)
- Azure Key Vault (for secrets)

### Entra ID App Registrations

**1. NorthStar LMS Web Application**
```bash
# Manual steps (Azure Portal or Azure CLI)
az ad app create \
  --display-name "NorthStar LMS Web" \
  --sign-in-audience AzureADMyOrg \
  --web-redirect-uris "https://lms.northstaret.com/signin-oidc" "https://localhost:7002/signin-oidc" \
  --enable-id-token-issuance true

# Generate client secret
az ad app credential reset --id <app-id> --append
# Store secret in Azure Key Vault
az keyvault secret set --vault-name NorthStarKeyVault --name "EntraId-ClientSecret" --value "<secret>"
```

**2. NorthStar LMS API Application**
```bash
az ad app create \
  --display-name "NorthStar LMS API" \
  --sign-in-audience AzureADMyOrg \
  --identifier-uris "api://northstar-lms"

# Expose API scope
az ad app permission add \
  --id <api-app-id> \
  --api <api-app-id> \
  --api-permissions <scope-id>=Scope
```

**3. Service Principals (per microservice)**
```bash
# Example: Student Service principal
az ad sp create-for-rbac --name "northstar-student-service" --skip-assignment
# Store credentials in Key Vault
```

### Database Migrations

```bash
# Create initial migration
cd Src/Foundation/services/Identity/Identity.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Identity.API --context IdentityDbContext

# Apply migrations (Aspire auto-applies in dev)
dotnet ef database update --startup-project ../Identity.API --context IdentityDbContext
```

### Configuration Files

**appsettings.json** (Identity.API)
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "{tenant-id}",
    "ClientId": "{web-app-client-id}",
    "ClientSecret": "{from-key-vault}",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-oidc"
  },
  "AzureAdApi": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "{tenant-id}",
    "ClientId": "{api-client-id}",
    "Audience": "api://northstar-lms"
  },
  "SessionManagement": {
    "StaffSessionDurationHours": 8,
    "AdminSessionDurationHours": 1,
    "TokenRefreshThresholdMinutes": 5,
    "EnableIpAddressBinding": false
  },
  "ConnectionStrings": {
    "IdentityDb": "{aspire-provided}"
  },
  "Redis": {
    "ConnectionString": "{aspire-provided}",
    "SessionCachePrefix": "lms_session:",
    "PermissionCachePrefix": "lms_permissions:"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Identity": "Information"
    }
  }
}
```

### Running Locally

```bash
# Start Aspire AppHost (orchestrates all services + infra)
cd Src/Foundation/AppHost
dotnet run

# Access Aspire Dashboard: http://localhost:15000
# Identity API: https://localhost:5001
# View logs, traces, metrics in dashboard
```

---

## Security Considerations

### Threat Model

**Threats Mitigated**:
1. **Token Forgery**: RS256 signature verification prevents tampering
2. **Session Hijacking**: HTTP-only cookies, optional IP binding
3. **Brute Force**: Entra ID lockout policies (5 attempts → 30-minute lock)
4. **Replay Attacks**: Short token lifetime (15 minutes) + nonce validation
5. **Cross-Site Scripting (XSS)**: HTTP-only cookies not accessible to JavaScript
6. **Man-in-the-Middle (MITM)**: TLS 1.3 required for all communication

**Residual Risks**:
- Entra ID outage disables authentication (mitigation: cached session fallback for 1 hour)
- Key Vault compromise exposes client secrets (mitigation: secret rotation, audit logging)
- Redis cache poisoning (mitigation: TLS encryption, network isolation)

### Compliance Requirements

- **FERPA**: All student data access logged to AuditRecords
- **COPPA**: Student authentication via school-issued accounts only
- **SOC 2**: Entra ID provides SOC 2 Type II certification
- **GDPR** (if applicable): User data deletion via soft delete, audit log retention 90 days

### Secret Management

- **Client Secrets**: Stored in Azure Key Vault, rotated every 90 days
- **Connection Strings**: Managed by Aspire, injected at runtime
- **JWT Signing Keys**: Managed by Entra ID (RS256 asymmetric keys)

### Audit Logging

**Events Logged to AuditRecords**:
- `UserAuthenticated`: userId, tenantId, ipAddress, timestamp
- `UserLoggedOut`: userId, timestamp, reason
- `SessionRefreshed`: sessionId, userId, newExpiresAt
- `TenantContextSwitched`: userId, fromTenantId, toTenantId, timestamp
- `AuthenticationFailed`: email, ipAddress, timestamp, reason
- `UnauthorizedTenantAccess`: userId, targetTenantId, timestamp

**Retention**: 90 days hot storage (PostgreSQL), 7 years cold storage (Azure Blob)

---

## Performance Optimization

### SLO Targets (from spec.md)

- Token exchange: P95 < 200ms, P99 < 500ms
- Session validation (Redis): P95 < 20ms, P99 < 50ms
- Session validation (DB fallback): P95 < 100ms, P99 < 250ms
- Authorization decision: P95 < 50ms, P99 < 100ms
- Tenant context switch: P95 < 200ms, P99 < 400ms

### Optimization Strategies

**1. Redis Caching**
- Cache sessions with TTL matching DB expiration
- Cache user permissions for session duration
- Write-through pattern: Write to DB, then cache

**2. Database Indexing**
```sql
-- Session lookup by ID (primary use case)
CREATE INDEX idx_sessions_id ON identity.sessions(id) WHERE expires_at > NOW();

-- User lookup by email + tenant
CREATE INDEX idx_users_tenant_email ON identity.users(tenant_id, email) WHERE deleted_at IS NULL;

-- User roles lookup
CREATE INDEX idx_user_roles_user_tenant ON identity.user_roles(user_id, tenant_id);

-- Audit query by user
CREATE INDEX idx_audit_records_user_timestamp ON identity.audit_records(user_id, timestamp);
```

**3. Connection Pooling**
```csharp
// Program.cs
builder.Services.AddDbContextPool<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.EnableRetryOnFailure(maxRetryCount: 3))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking), // Perf: read-only queries
    poolSize: 128); // Max concurrent DB connections
```

**4. Async All the Way**
- All repository methods async
- Use `IAsyncEnumerable<T>` for large result sets
- Background token refresh runs async

**5. Query Optimization**
- Use `.AsNoTracking()` for read-only queries
- Avoid N+1 queries: `.Include()` related entities
- Paginate large result sets (roles, audit logs)

### Load Testing Plan

```bash
# Artillery load test script
artillery run tests/load/identity-authentication.yml

# Target: 1,000 concurrent logins, 10,000 session validations/sec
# Success criteria: P95 latency within SLOs, 0 errors
```

---

## Deployment Strategy

### Deployment Phases

**Phase 1: Development Environment**
- Deploy to Azure App Service (dev slot)
- PostgreSQL: Azure Database for PostgreSQL (Basic tier)
- Redis: Azure Cache for Redis (Basic tier)
- Service Bus: Development namespace

**Phase 2: Staging Environment**
- Blue-green deployment slots
- PostgreSQL: General Purpose tier (HA)
- Redis: Standard tier (replication)
- Service Bus: Standard namespace
- Smoke tests + integration tests

**Phase 3: Production Environment**
- Rolling deployment (zero downtime)
- PostgreSQL: Premium tier (read replicas)
- Redis: Premium tier (clustering)
- Service Bus: Premium namespace
- Canary deployment (5% traffic → 50% → 100%)

### Migration Strategy (Legacy IdentityServer)

**Pre-Migration**:
1. Database backup of legacy IdentityServer DB
2. Dry-run migration script on production copy
3. Generate unmatched users report
4. Communication plan: email all users 1 week before

**Migration Day** (Maintenance Window: 2am-4am):
1. Disable legacy IdentityServer login
2. Run migration script (match users by email)
3. Create `ExternalProviderLinks` for matched users
4. Enable Entra ID authentication
5. Monitor error logs for 1 hour

**Post-Migration**:
1. Send welcome email with "Sign in with Microsoft" instructions
2. Help desk prepared for password reset questions
3. Manually resolve unmatched users within 1 week

**Rollback Plan**:
- If >10% authentication failures within 1 hour: Re-enable legacy IdentityServer
- Delete `ExternalProviderLinks` table
- Restore database from backup

### Monitoring & Alerting

**Metrics to Monitor**:
- `identity_login_success_count` (rate)
- `identity_login_failure_count` (rate)
- `identity_session_validation_latency` (P95, P99)
- `identity_token_refresh_count` (rate)
- `identity_authorization_decision_latency` (P95)

**Alerts**:
- Auth failure rate >5% for 5 minutes → Page on-call
- Session validation P95 >50ms for 10 minutes → Warning
- Token refresh failure rate >1% for 5 minutes → Warning
- Entra ID API latency >500ms for 10 minutes → Warning

**Dashboards**:
- Grafana: Real-time authentication metrics
- Seq: Structured log search (audit trail)
- Aspire Dashboard: Service health, distributed traces

---

## Open Questions & Risks

### Open Questions for Stakeholders

1. **Entra ID Tenant Strategy**
   - **Question**: Should we use a single Entra ID tenant for all districts or per-district tenants?
   - **Recommendation**: Single tenant with custom `district_id` claim (simpler management)
   - **Decision**: [Pending]

2. **Role Synchronization**
   - **Question**: Should roles be managed entirely in Entra ID (via app roles) or in NorthStar database?
   - **Recommendation**: Hybrid - base roles from Entra ID, granular permissions in NorthStar
   - **Decision**: [Pending]

3. **Multi-Region Session Replication**
   - **Question**: How to replicate sessions across Azure regions for disaster recovery?
   - **Recommendation**: PostgreSQL streaming replication (deferred to future phase)
   - **Decision**: [Pending]

4. **SMTP Provider for Notifications**
   - **Question**: Which email service for password reset notifications?
   - **Options**: SendGrid, Azure Communication Services, Office 365 SMTP
   - **Recommendation**: Azure Communication Services (integrated with Azure)
   - **Decision**: [Pending]

5. **Audit Retention Policy**
   - **Question**: How long to retain audit records?
   - **Recommendation**: 90 days hot (PostgreSQL), 7 years cold (Azure Blob)
   - **Decision**: [Pending]

### Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **Entra ID Service Outage** | Low | Critical | Cached session fallback (1 hour grace period), status page monitoring |
| **User Migration Failures** | Medium | High | Phased migration with manual review, comprehensive unmatched user report |
| **Performance Under Peak Load** | Medium | High | Load testing before production, Redis clustering, connection pooling |
| **Session Hijacking** | Low | High | HTTP-only cookies, optional IP binding, audit logging |
| **Key Vault Secrets Leak** | Low | Critical | Secret rotation every 90 days, audit logging, least privilege access |
| **Multi-Tenant Data Leakage** | Low | Critical | Global query filters, row-level security, comprehensive testing |

### Dependencies

**External Services**:
- Microsoft Entra ID (99.99% SLA)
- Azure Key Vault (99.9% SLA)
- Azure Database for PostgreSQL (99.99% SLA with HA)
- Azure Cache for Redis (99.9% SLA)
- Azure Service Bus (99.9% SLA)

**Internal Services**:
- None (Identity is a foundation service with no upstream dependencies)

**Blocking Issues**:
- Entra ID tenant must be provisioned before development starts
- Azure Key Vault must be configured with secrets
- SMTP service must be selected for password reset notifications

---

## Next Steps

1. **Review this plan** with architecture team and stakeholders
2. **Resolve open questions** (Entra ID tenant strategy, role synchronization)
3. **Provision infrastructure** (Entra ID tenant, Key Vault, app registrations)
4. **Create feature branch**: `CrossCuttingConcerns/01-identity-service-entra-id`
5. **Begin Phase 1 implementation** with TDD workflow (Red → Green → Refactor)
6. **Capture evidence** for each phase (unit, integration, BDD tests)
7. **Phase review pushes** after each phase with evidence transcripts
8. **Final review** before merge to main

---

## Appendix

### References

- [Microsoft.Identity.Web Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [OAuth 2.0 Authorization Code Flow](https://tools.ietf.org/html/rfc6749#section-4.1)
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
- [NorthStar Constitution v2.0.0](.specify/memory/constitution.md)
- [Clean Architecture Pattern](../../patterns/clean-architecture.md)
- [Aspire Orchestration Pattern](../../patterns/aspire-orchestration.md)
- [Testing Strategy](../../standards/TESTING_STRATEGY.md)

### Glossary

- **Entra ID**: Microsoft's cloud identity service (formerly Azure Active Directory)
- **OIDC**: OpenID Connect, authentication layer on top of OAuth 2.0
- **JWT**: JSON Web Token, compact claims representation
- **BFF**: Backend-for-Frontend, pattern where backend manages tokens
- **SLO**: Service Level Objective, performance target
- **P95**: 95th percentile latency (95% of requests faster than this)
- **TDD**: Test-Driven Development (Red → Green → Refactor)
- **RBAC**: Role-Based Access Control

---

**Plan Prepared By**: GitHub Copilot  
**Plan Reviewed By**: [Pending]  
**Plan Approved By**: [Pending]  
**Implementation Start Date**: [TBD]
