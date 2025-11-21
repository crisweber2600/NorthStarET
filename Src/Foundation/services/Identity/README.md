# Identity Service

Foundation service for authentication and authorization using Microsoft Entra ID.

## Overview

The Identity Service is a foundational microservice that provides Microsoft Entra ID-based authentication, session management, and role-based authorization for all NorthStar LMS users. It acts as an **integration layer** between Microsoft Entra ID and NorthStar services.

**⚠️ CRITICAL**: This service does **NOT** store passwords or act as a local identity provider. All authentication is delegated to Microsoft Entra ID.

## Architecture

### Clean Architecture Layers

```
Identity.API (HTTP)
    ↓
Identity.Application (MediatR, Commands/Queries)
    ↓
Identity.Domain (Entities, Value Objects, Events)
    ↓
Identity.Infrastructure (EF Core, Redis, MassTransit)
```

### Key Components

- **Domain Layer**: Pure business logic
  - Entities: User, Session, Role, ExternalProviderLink, AuditRecord
  - Value Objects: SessionId, EntraSubjectId, TenantId
  - Events: UserAuthenticated, UserLoggedOut, SessionRefreshed, TenantContextSwitched

- **Application Layer**: Use cases via MediatR
  - Commands: ExchangeToken, RefreshSession, Logout, SwitchTenant
  - Queries: GetUser, GetSession, ValidateSession
  - Validators: FluentValidation for input validation

- **Infrastructure Layer**: External integrations
  - IdentityDbContext: PostgreSQL database with EF Core
  - Redis: Session caching for P95 <20ms validation
  - MassTransit: Domain event publishing to RabbitMQ

- **API Layer**: HTTP endpoints
  - AuthenticationController: Token exchange, login, logout, refresh
  - AuthorizationController: Claims, permissions, tenant switching
  - Middleware: Authentication, authorization, exception handling

## Database Schema

### Tables

- **users**: Core user profiles with tenant association
- **sessions**: Active user sessions with expiration tracking
- **roles**: System and Entra ID app roles with permissions
- **user_roles**: User-role assignments
- **external_provider_links**: Links users to Microsoft Entra ID
- **audit_records**: Audit trail for authentication/authorization events

### Key Indexes

- Tenant isolation: `(tenant_id)` on all tenant-scoped tables
- Session lookup: `(session_id)`, `(user_id)`, `(expires_at)`
- User lookup: `(tenant_id, email)` unique constraint
- Provider lookup: `(provider_name, subject_id)` unique constraint

## Configuration

### Required Settings (appsettings.json)

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Audience": "api://your-api-id"
  },
  "ConnectionStrings": {
    "IdentityDb": "Host=localhost;Database=NorthStarIdentity;..."
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "SessionSettings": {
    "StaffSessionDurationHours": 8,
    "AdminSessionDurationHours": 1,
    "SlidingExpirationEnabled": true
  }
}
```

### Aspire Configuration

The service is registered in `Src/Foundation/AppHost/AppHost.cs` with dependencies on PostgreSQL, Redis, and RabbitMQ.

## Development

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 16+
- Redis 7+
- RabbitMQ 3.13+

### Running Locally with Aspire

```bash
# From repository root
dotnet run --project Src/Foundation/AppHost

# Aspire dashboard: http://localhost:15000
# Identity API: https://localhost:7001 (auto-assigned)
```

### Running Standalone

```bash
cd Src/Foundation/services/Identity/Identity.API
dotnet run
```

### Database Migrations

```bash
# Add migration (from Identity.Infrastructure project)
dotnet ef migrations add MigrationName --project Identity.Infrastructure --startup-project Identity.API

# Apply migrations (Aspire auto-applies on startup in dev)
dotnet ef database update --project Identity.Infrastructure --startup-project Identity.API
```

## Authentication Flow

1. **User initiates login**: NorthStar Web redirects to Microsoft Entra ID
2. **Entra authentication**: User signs in with Microsoft credentials (+ MFA if required)
3. **Token exchange**: Web app sends Entra ID access token to Identity Service
4. **Session creation**: Identity Service validates token, creates session, returns session cookie
5. **Subsequent requests**: Session cookie validated against Redis cache (P95 <20ms)
6. **Token refresh**: Background service refreshes Entra tokens transparently
7. **Logout**: Session revoked from both database and cache

## Multi-Tenant Support

- Every session and user record includes `TenantId` (district ID)
- Tenant context switching updates session without re-authentication
- Global query filters enforce tenant isolation at database level
- Cross-tenant access requires explicit authorization and audit logging

## Security Features

- **No password storage**: All authentication via Microsoft Entra ID
- **MFA enforcement**: Admin roles require Entra ID MFA claim (`amr` or `acr`)
- **Session expiration**: Staff 8 hours, Admin 1 hour (configurable)
- **Audit logging**: All authentication/authorization events logged
- **Token validation**: Entra ID JWT signature and claims validation
- **Rate limiting**: Applied at API Gateway level

## Performance Targets

- Token exchange (Entra → Session): P95 <200ms
- Session validation (cached): P95 <20ms
- Session validation (DB fallback): P95 <100ms
- Session refresh: P95 <50ms

## Events Published

- `UserAuthenticatedEvent`: On successful login
- `UserLoggedOutEvent`: On logout
- `SessionRefreshedEvent`: On token refresh
- `TenantContextSwitchedEvent`: On tenant switch

## Monitoring

- Health endpoint: `/api/health`
- Aspire dashboard: Service logs, traces, metrics
- OpenTelemetry: Distributed tracing across services

## Testing

### Unit Tests

```bash
dotnet test Src/Foundation/services/Identity/tests/Identity.UnitTests
```

### Integration Tests

```bash
dotnet test Src/Foundation/services/Identity/tests/Identity.IntegrationTests
```

### Contract Tests

```bash
dotnet test Src/Foundation/services/Identity/tests/Identity.ContractTests
```

## Related Documentation

- Specification: `Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id/spec.md`
- Implementation Plan: `Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id/plan.md`
- Architecture: `Plan/CrossCuttingConcerns/architecture/services/identity-service.md`
- API Contracts: `Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id/contracts/`

## Support

For issues or questions, see:
- Project documentation: `docs/`
- Specification clarifications: `Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id/clarify.md`
