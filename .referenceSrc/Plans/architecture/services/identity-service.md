# Identity & Authentication Service

> **⚠️ OBSOLETE REFERENCE DOCUMENT**: This document is from the WIPNorthStar research phase and contains outdated Duende IdentityServer references.  
> **Current Documentation**: See `docs/architecture/services/identity-service.md` and `Plan/Foundation/Plans/docs/legacy-identityserver-migration.md` for active architecture using Microsoft Entra ID with session authentication.

## Overview

The Identity & Authentication Service is the foundation security service for the NorthStar LMS platform, providing centralized authentication, authorization, and user identity management across all microservices. It serves as the **login management layer** for the LMS administrative platform, integrating with **Microsoft Entra ID** (formerly Azure AD) for enterprise single sign-on (SSO) while supporting local authentication for students and external users.

## Service Classification

- **Type**: Foundation Service
- **Phase**: Phase 1 (Weeks 5-8)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Identity/`
- **Priority**: Critical (blocks all other services)
- **LMS Role**: Login management for system administrators, district administrators, school administrators, and staff

## Current State (Legacy)

**Location**: `IdentityServer/` project  
**Framework**: .NET Framework 4.6 with IdentityServer4  
**Database**: `LoginContext` (cross-district user database)

**Key Components**:
- `IdentityServer/Startup.cs` - OAuth 2.0/OIDC configuration
- User authentication and token issuance
- Claims management for authorization
- Multi-district user resolution
- **Limited external identity integration** (legacy system uses local accounts only)

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Identity.API/                    # UI Layer (REST endpoints)
├── Controllers/
├── Middleware/
└── Program.cs

Identity.Application/            # Application Layer
├── Commands/                   # User registration, password reset
├── Queries/                    # User lookup, claims retrieval
├── DTOs/
├── Interfaces/
└── ExternalProviders/
    └── EntraIdIntegration/     # Microsoft Entra ID SSO

Identity.Domain/                # Domain Layer
├── Entities/
│   ├── User.cs
│   ├── Role.cs
│   ├── Claim.cs
│   ├── RefreshToken.cs
│   └── ExternalProvider.cs     # External identity linkage
├── Events/
│   ├── UserRegisteredEvent.cs
│   ├── UserLoggedInEvent.cs
│   └── PasswordChangedEvent.cs
└── ValueObjects/

Identity.Infrastructure/        # Infrastructure Layer
├── Data/
│   ├── IdentityDbContext.cs
│   └── Repositories/
├── Identity/
│   ├── DuendeIdentityServerConfig.cs
│   └── EntraIdProvider.cs      # Microsoft Entra ID integration
└── MessageBus/
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Identity Provider**: Duende IdentityServer (successor to IdentityServer4)
- **External Authentication**: Microsoft Entra ID (Azure AD) integration for staff/administrator SSO
- **Data Access**: EF Core with SQL Server
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Caching**: Redis for token validation
- **Orchestration**: .NET Aspire hosting

### Owned Data

**Database**: `NorthStar_Identity_DB`

**Tables**:
- Users (Id, Email, PasswordHash, EmailConfirmed, ExternalProviderId, ProviderType, etc.)
- Roles (Id, Name, Description)
- UserRoles (UserId, RoleId)
- UserClaims (UserId, ClaimType, ClaimValue)
- RefreshTokens (Token, UserId, Expires, Created)
- PersistedGrants (Key, Type, SubjectId, ClientId, Data, Expiration)
- ExternalProviderLinks (UserId, Provider, ExternalUserId, Email, LastSync)

### Service Boundaries

**Owned Responsibilities**:
- User authentication (login/logout) for local accounts
- **Microsoft Entra ID integration** for staff and administrator SSO
- OAuth 2.0 / OpenID Connect token issuance
- JWT token generation and validation
- Password management (reset, change) for local accounts
- User registration and profile updates
- Role and claims management (RBAC for LMS administrative hierarchy)
- External provider account linking and synchronization
- Multi-factor authentication (future)
- Session management

**Authentication Strategy**:
- **Staff & Administrators**: Microsoft Entra ID SSO (preferred), local fallback
- **Students**: Local accounts (username/password)
- **External Users**: Local accounts or future social providers

**Not Owned** (delegated to other services):
- Student-specific data (enrollment, demographics) → Student Management Service
- Staff-specific data (teaching assignments, teams) → Staff Management Service
- District/school configuration → Configuration Service
- Administrative hierarchy management → Distributed across services

### Domain Events Published

**Event Schema Version**: 1.0

- `UserRegisteredEvent` - When a new user account is created
  ```
  - UserId: Guid
  - Email: string
  - Roles: string[]
  - Timestamp: DateTime
  ```

- `UserLoggedInEvent` - When a user successfully authenticates
  ```
  - UserId: Guid
  - Email: string
  - LoginTimestamp: DateTime
  - IPAddress: string
  ```

- `UserLoggedOutEvent` - When a user logs out
  ```
  - UserId: Guid
  - LogoutTimestamp: DateTime
  ```

- `PasswordChangedEvent` - When a user changes password
  ```
  - UserId: Guid
  - ChangedAt: DateTime
  ```

- `UserRoleChangedEvent` - When user roles are modified
  ```
  - UserId: Guid
  - AddedRoles: string[]
  - RemovedRoles: string[]
  - ChangedBy: Guid
  - Timestamp: DateTime
  ```

### Domain Events Subscribed

- None (foundation service with no upstream dependencies)

### API Endpoints (Functional Intent)

**Authentication**:
- Authenticate user with credentials → returns access token
- Refresh expired token → returns new access token
- Logout user → invalidates refresh token
- Validate token → returns claims

**User Management**:
- Register new user → creates user account
- Request password reset → sends reset email
- Reset password with token → updates password
- Change password → updates password (authenticated)
- Update user profile → modifies user data

**Authorization**:
- Get user claims → returns user's claims
- Assign role to user → updates user roles
- Remove role from user → updates user roles
- Check user permissions → validates claim

### Service Level Objectives (SLOs)

- **Availability**: 99.95% uptime
- **Authentication Latency**: 
  - p95 < 200ms for login
  - p99 < 500ms for login
- **Token Validation**: p95 < 50ms (with Redis caching)
- **User Registration**: p95 < 1 second
- **Password Reset Email**: Async, confirmation within 800ms

### Idempotency & Consistency

**Idempotency Windows**:
- User registration: 10 minutes (duplicate email prevention)
- Password reset requests: 5 minutes (prevent spam)
- Token refresh: Handled by token expiration logic

**Consistency Model**:
- Strong consistency for authentication operations
- Eventual consistency for user profile updates propagated via events

### Security Considerations

**Constitutional Requirements**:
- Enforce least privilege principle
- Role-based access control (RBAC) in Application layer
- Secrets stored in Azure Key Vault only
- No direct UI → Infrastructure coupling

**Implementation**:
- Password hashing with PBKDF2/bcrypt (configurable rounds)
- JWT tokens with short expiration (15 minutes)
- Refresh tokens with rotation
- HTTPS only, no plain HTTP
- Rate limiting on authentication endpoints
- Account lockout after failed attempts
- Audit logging for all authentication events

### Testing Requirements

**Constitutional Compliance**:
- Reqnroll BDD features before implementation
- TDD Red → Green with test evidence
- ≥ 80% code coverage

**Test Categories**:

1. **Unit Tests** (Identity.UnitTests):
   - Password validation logic
   - Token generation and validation
   - User registration business rules
   - Password reset token generation

2. **Integration Tests** (Identity.IntegrationTests):
   - Database operations via EF Core
   - OAuth 2.0 token flows
   - Event publishing to message bus
   - Aspire orchestration validation

3. **BDD Tests** (Reqnroll features):
   - `UserRegistration.feature` - New user signup flow
   - `UserAuthentication.feature` - Login scenarios
   - `PasswordReset.feature` - Password recovery
   - `TokenRefresh.feature` - Token lifecycle

4. **UI Tests** (Playwright):
   - Login page user journey
   - Registration form submission
   - Password reset flow
   - Logout confirmation

### Dependencies

**External Services**:
- Azure Key Vault - Secret storage
- Azure Service Bus - Event publishing
- Redis - Token caching
- SMTP Server - Password reset emails

**Infrastructure Dependencies**:
- SQL Server - User database
- .NET Aspire AppHost - Service orchestration

### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 1a** (Week 5): Deploy new Duende IdentityServer alongside legacy
   - Configure dual issuer validation in API Gateway
   - Route new user registrations to new service
   - Legacy users continue using old IdentityServer

2. **Phase 1b** (Week 6): Gradual user migration
   - On next login, migrate user to new database
   - Update token issuer claim
   - Maintain both systems in parallel

3. **Phase 1c** (Week 7): Complete cutover
   - Route all authentication to new service
   - Keep legacy IdentityServer as read-only fallback
   - Monitor error rates and rollback if needed

4. **Phase 1d** (Week 8): Decommission legacy
   - Verify all users migrated
   - Archive legacy IdentityServer database
   - Remove legacy service from infrastructure

### Configuration

**Aspire Service Defaults**:
```csharp
builder.AddServiceDefaults(); // Logging, metrics, health checks
```

**Environment Variables**:
- `ConnectionStrings__IdentityDb` - Database connection
- `JwtSettings__Secret` - From Key Vault
- `JwtSettings__Issuer` - Token issuer URL
- `JwtSettings__Audience` - Token audience
- `JwtSettings__ExpirationMinutes` - Access token lifetime
- `MessageBus__ConnectionString` - Azure Service Bus
- `Redis__ConnectionString` - Redis cache
- `Email__SmtpServer` - Email server for password resets

### Monitoring & Observability

**Metrics to Track**:
- Authentication success/failure rates
- Token validation latency (p50, p95, p99)
- Active user sessions
- Failed login attempts per user
- Password reset request rate
- Token refresh rate

**Logging**:
- All authentication attempts (success/failure)
- Password changes
- Role modifications
- Token issuance and validation
- Security events (lockouts, suspicious activity)

**Distributed Tracing**:
- OpenTelemetry instrumentation
- Correlation IDs across service calls
- Integration with Application Insights

### Compliance & Audit

**Audit Trail**:
- All authentication events logged
- User profile changes tracked
- Role/permission modifications recorded
- Password resets documented
- Login history maintained (90 days minimum)

**GDPR Considerations**:
- User data export capability
- User data deletion (right to be forgotten)
- Consent tracking
- Data retention policies

## Implementation Checklist

- [ ] Set up `NewDesign/NorthStarET.Lms/src/services/Identity/` project structure
- [ ] Create Clean Architecture layer projects
- [ ] Configure Duende IdentityServer with .NET 8
- [ ] Implement EF Core DbContext and migrations
- [ ] Create domain entities and events
- [ ] Implement user registration command handler
- [ ] Implement authentication query handlers
- [ ] Configure MassTransit event publishing
- [ ] Add Redis caching for token validation
- [ ] Write Reqnroll BDD features
- [ ] Implement step definitions
- [ ] Create unit tests (TDD Red → Green)
- [ ] Create integration tests with Aspire
- [ ] Configure Aspire AppHost orchestration
- [ ] Set up health checks and metrics
- [ ] Configure CI/CD pipeline
- [ ] Deploy to staging environment
- [ ] Execute migration from legacy IdentityServer
- [ ] Monitor and validate production deployment

## Related Documentation

- [API Gateway Configuration](../docs/api-gateway-config.md) - Gateway integration
- [Bounded Contexts](../architecture/bounded-contexts.md) - Service boundaries
- [Development Guide](../docs/development-guide.md) - Setup instructions
- [Deployment Guide](../docs/deployment-guide.md) - Deployment procedures

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete, Ready for Implementation
