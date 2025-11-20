# Identity & Authentication Service

**Phase**: 1 (Weeks 1-8)  
**Priority**: Critical (Blocking - Required by all other services)  
**Status**: To Be Implemented

---

## Overview

The Identity & Authentication Service provides centralized authentication and authorization for all Foundation services using:

- **Microsoft Entra ID** (OAuth 2.0 / OpenID Connect provider)
- **Microsoft.Identity.Web** (JWT token validation)
- **Custom session authentication** (SessionAuthenticationHandler with Redis caching)
- **Token exchange** (Backend-for-Frontend pattern - Entra tokens → LMS sessions)
- **User management** (profile updates, role assignment)
- **Role-Based Access Control (RBAC)**

**Bounded Context**: Identity & Authentication  
**Database**: `Identity_DB` (PostgreSQL with multi-tenancy)

---

## Responsibilities

1. **User Authentication**
   - Email/password login
   - Microsoft Entra ID SSO
   - Password reset flows
   - Two-factor authentication (future)

2. **Token Management**
   - JWT token validation (tokens issued by Entra ID)
   - Session creation and management (Redis-cached)
   - Token exchange (Entra ID tokens → LMS session IDs)
   - Session expiration and sliding window refresh
   - Token introspection via Microsoft.Identity.Web

3. **User Management**
   - User registration
   - Profile updates
   - Password management
   - Account activation/deactivation

4. **Authorization**
   - Role assignment (SuperAdmin, DistrictAdmin, SchoolAdmin, Teacher, Parent, Student)
   - Permission management
   - Claims-based authorization

---

## API Endpoints

### Authentication

- `POST /api/identity/auth/login` - Email/password login
- `POST /api/identity/auth/logout` - Logout (revoke tokens)
- `POST /api/identity/auth/refresh` - Refresh access token
- `GET /api/identity/auth/entra-login` - Initiate Entra ID SSO
- `GET /api/identity/auth/entra-callback` - Entra ID callback

### User Management

- `POST /api/identity/users/register` - User registration
- `GET /api/identity/users/{id}` - Get user by ID
- `PUT /api/identity/users/{id}` - Update user profile
- `DELETE /api/identity/users/{id}` - Deactivate user (soft delete)
- `POST /api/identity/users/{id}/reset-password` - Initiate password reset

### Roles & Permissions

- `GET /api/identity/roles` - List all roles
- `POST /api/identity/users/{id}/roles` - Assign role to user
- `DELETE /api/identity/users/{id}/roles/{roleId}` - Remove role from user
- `GET /api/identity/permissions` - List all permissions

---

## Domain Model

### Entities

- **User**: Core user entity (Id, Email, PasswordHash, TenantId, Roles)
- **Role**: User role (SuperAdmin, DistrictAdmin, etc.)
- **RefreshToken**: Refresh token tracking (UserId, Token, Expiration, Revoked)
- **PasswordResetToken**: Password reset tokens (UserId, Token, Expiration, Used)

### Value Objects

- `EmailAddress` - Email validation
- `PasswordHash` - BCrypt hashed password
- `TenantId` - District/tenant identifier

### Domain Events

- `UserRegisteredEvent` - New user registered
- `UserLoggedInEvent` - User authenticated
- `UserLoggedOutEvent` - User logged out
- `PasswordChangedEvent` - User changed password
- `RoleAssignedEvent` - Role assigned to user

---

## Technology Stack

- **ASP.NET Core Web API** (.NET 10)
- **Microsoft Entra ID** (OAuth 2.0/OIDC identity provider)
- **Microsoft.Identity.Web** (JWT token validation)
- **Custom SessionAuthenticationHandler** (Session-based API authorization)
- **Entity Framework Core 10** (PostgreSQL)
- **Redis Stack** (Session caching, idempotency windows)
- **MediatR** (CQRS)
- **FluentValidation** (Input validation)

---

## Legacy Components to Migrate

From `OldNorthStar`:

- `IdentityServer/` - Existing IdentityServer 3 setup
- `NS4.WebAPI/Controllers/AuthController.cs` - Authentication endpoints
- `NS4.WebAPI/Controllers/PasswordResetController.cs` - Password management
- `NorthStar.Core/Identity/` - Identity domain logic
- `EntityDto/Entity/User.cs`, `UserRole.cs` - User entities

---

## Dependencies

### Phase 1 Dependencies

- **Configuration Service**: Tenant configuration, feature flags
- **PostgreSQL**: Identity_DB database
- **Redis**: Token caching, session storage
- **Azure Service Bus**: Domain event publishing

### Services Depending on Identity

- **ALL Services**: Every Foundation service requires Identity for authentication/authorization

---

## Clean Architecture Structure

```
Identity/
├── Identity.Domain/                  # Domain entities, events, interfaces
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   └── RefreshToken.cs
│   ├── ValueObjects/
│   │   ├── EmailAddress.cs
│   │   └── PasswordHash.cs
│   ├── Events/
│   │   ├── UserRegisteredEvent.cs
│   │   └── UserLoggedInEvent.cs
│   └── Interfaces/
│       ├── IUserRepository.cs
│       └── ITokenService.cs
│
├── Identity.Application/             # CQRS handlers, DTOs, validators
│   ├── Commands/
│   │   ├── RegisterUser/
│   │   │   ├── RegisterUserCommand.cs
│   │   │   ├── RegisterUserCommandHandler.cs
│   │   │   └── RegisterUserValidator.cs
│   │   └── LoginUser/
│   │       ├── LoginUserCommand.cs
│   │       └── LoginUserCommandHandler.cs
│   ├── Queries/
│   │   ├── GetUser/
│   │   │   ├── GetUserQuery.cs
│   │   │   └── GetUserQueryHandler.cs
│   │   └── ListUsers/
│   │       ├── ListUsersQuery.cs
│   │       └── ListUsersQueryHandler.cs
│   └── DTOs/
│       ├── UserDto.cs
│       ├── LoginResponseDto.cs
│       └── TokenResponseDto.cs
│
├── Identity.Infrastructure/          # EF Core, repositories, Entra ID integration
│   ├── Persistence/
│   │   ├── IdentityDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs
│   │   │   └── RoleConfiguration.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   └── RoleRepository.cs
│   ├── Identity/
│   │   ├── EntraIdConfiguration.cs           # Microsoft.Identity.Web setup
│   │   ├── SessionAuthenticationHandler.cs   # Custom session auth
│   │   └── TokenExchangeService.cs           # BFF token exchange
│   │   └── TokenService.cs
│   └── DependencyInjection.cs
│
├── Identity.Api/                     # REST API endpoints
│   ├── Program.cs
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   └── RolesController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── appsettings.json
│
└── Identity.Tests/                   # Tests
    ├── Domain.Tests/
    ├── Application.Tests/
    ├── Infrastructure.Tests/
    └── Integration.Tests/
```

---

## Configuration

```json
{
  "IdentityServer": {
    "IssuerUri": "https://identity.northstar.local",
    "Clients": [
      {
        "ClientId": "northstar-web",
        "ClientSecrets": ["secret-from-keyvault"],
        "AllowedScopes": ["openid", "profile", "email", "api"]
      }
    ]
  },
  "EntraId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "tenant-id-from-azure",
    "ClientId": "app-registration-client-id",
    "ClientSecret": "secret-from-keyvault"
  },
  "ConnectionStrings": {
    "IdentityDb": "Host=postgres;Database=Identity_DB;Username=postgres;Password=postgres"
  }
}
```

---

## Testing Requirements

### Unit Tests (≥80% coverage)

- Domain entities and value objects
- Command/query handlers
- Validators

### Integration Tests (Aspire)

- Authentication flows (login, logout, refresh)
- User registration and management
- Role assignment
- Entra ID integration (mocked)

### BDD Tests (Reqnroll)

Feature files in `specs/002-identity-authentication/features/`:
- `user-registration.feature`
- `user-login.feature`
- `password-reset.feature`
- `role-assignment.feature`

---

## References

- **Specification**: [002-identity-authentication](../../../Plan/Foundation/specs/Foundation/002-identity-authentication/)
- **Architecture**: [identity-service.md](../../../docs/architecture/services/identity-service.md)
- **Scenario**: [01-identity-migration-entra-id.md](../../../Plan/Foundation/Plans/scenarios/01-identity-migration-entra-id.md)
- **Constitution**: [Principle 5 - Security & Compliance Safeguards](../../../.specify/memory/constitution.md)

---

**Status**: Specification Complete, Implementation Pending  
**Start Date**: TBD (Phase 1, Week 1)  
**Completion Target**: Week 4
