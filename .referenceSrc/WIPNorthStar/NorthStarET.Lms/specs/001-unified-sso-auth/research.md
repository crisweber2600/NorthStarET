# Research: Unified SSO & Authorization via Entra (LMS Identity Module)

**Date**: 2025-10-20  
**Feature**: 001-unified-sso-auth  
**Phase**: 0 - Research & Decision Documentation

## Research Tasks Overview

This document resolves all technical unknowns from the Technical Context and evaluates best practices for the chosen technology stack.

---

## 1. Microsoft Entra ID Integration with ASP.NET Core Identity

### Decision
Use **Microsoft.Identity.Web** library (v3.x) for Entra ID integration with ASP.NET Core, combined with ASP.NET Core Identity for local user management and session handling.

### Rationale
- **Microsoft.Identity.Web** is the official Microsoft library for Entra ID (Azure AD) integration with ASP.NET Core
- Provides out-of-box support for:
  - OpenID Connect authentication flow with Entra ID
  - JWT token validation (local, no round-trip to Entra per request)
  - Token acquisition and caching
  - B2B and B2C scenario support
  - Claims transformation
- ASP.NET Core Identity complements it by managing:
  - Local user records linked to Entra subject IDs
  - Session state and cookies
  - Role/claim augmentation beyond Entra tokens
  
### Alternatives Considered
1. **Pure ASP.NET Core Identity with external login providers** - Rejected because it requires more manual configuration for Entra-specific features like B2B/B2C and doesn't leverage Microsoft's optimized libraries
2. **Direct OpenIdConnect middleware without Microsoft.Identity.Web** - Rejected because Microsoft.Identity.Web provides critical abstractions (token acquisition, caching, downstream API auth) that we'd have to build manually
3. **IdentityServer/Duende** - Rejected as overkill; we're federating to Entra, not building our own identity provider

### Implementation Notes
- Use `AddMicrosoftIdentityWebAppAuthentication()` for the MVC app
- Use `AddMicrosoftIdentityWebApiAuthentication()` for the API project
- Configure both B2B (organizational accounts) and B2C (external identities) tenant settings
- Map Entra claims to ASP.NET Core Identity user claims
- Store user profile data locally in PostgreSQL via EF Core

### References
- [Microsoft.Identity.Web documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [ASP.NET Core authentication with Entra ID](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/azure-active-directory/)

---

## 2. Token Exchange Pattern (BFF/Gateway)

### Decision
Implement a **Backend-for-Frontend (BFF) pattern** in the Web API project that:
1. Validates incoming Entra JWT tokens locally (no round-trip)
2. Exchanges them for short-lived LMS access tokens (custom JWT or opaque tokens)
3. Uses LMS access tokens for inter-service communication

### Rationale
- **Security isolation**: Frontend never sees backend service tokens
- **Token customization**: LMS tokens can include custom claims (tenant context, cached roles) without modifying Entra tokens
- **Flexibility**: Can change backend token format without affecting Entra integration
- **Performance**: Local JWT validation is fast (<5ms); token exchange happens once per session
- **Multi-tenant context**: LMS tokens can embed active tenant selection, avoiding repeated lookups

### Alternatives Considered
1. **Pass Entra tokens directly to backend services** - Rejected because:
   - Exposes Entra token details to all services
   - Harder to embed custom claims (tenant context, cached permissions)
   - Couples all services to Entra token format
2. **OAuth2 token exchange (RFC 8693)** - Rejected as over-engineered for our use case; we control both frontend and backend
3. **Session-based cookies only** - Rejected because APIs need bearer tokens for non-browser clients (future mobile apps, service-to-service calls)

### Implementation Notes
- Validate Entra JWT signature using Entra's public keys (cached locally)
- Issue LMS tokens as JWTs signed with a local signing key (stored in platform secret store)
- Set LMS token expiration to 1 hour (shorter than Entra token)
- Store token metadata in Redis for revocation checks
- Use `ITokenAcquisition` from Microsoft.Identity.Web for any downstream calls that need Entra tokens

### References
- [BFF pattern](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-browser-based-apps)
- [ASP.NET Core JWT bearer authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)

---

## 3. Authorization via LMS Identity Module

### Decision
Embed the identity and authorization model directly inside the LMS API as an **LMS Identity module** that:
- Exposes application services for permission checks
- Persists users, roles, memberships, and sessions in the `identity` schema of the LMS PostgreSQL database
- Uses cached projections (Redis) for sub-5ms authorization lookups
- Publishes and consumes domain events to keep cached data in sync

### Rationale
- Keeps authorization logic co-located with the LMS API, reducing cross-service latency and operational overhead
- Eliminates the need for a separate HTTP hop while still enforcing clear module boundaries
- Allows richer transactional queries (joins across users, roles, tenants) using EF Core without introducing eventual consistency delays
- Redis caching maintains <5ms access times while PostgreSQL remains the source of truth
- Module architecture still allows other services to consume identity data via published events or future API endpoints if needed

### Alternatives Considered
1. **Separate Identity API service** – Rejected due to additional deployment/runtime complexity and unnecessary network latency for every authorization decision
2. **Event-only projection with no synchronous reads** – Rejected because authorization decisions must reflect the latest permissions in real time (<50ms) and cannot tolerate stale data
3. **Direct Entra token claim evaluation** – Rejected because it would prevent tenant-context overrides, user deactivation, and granular LMS-specific permissions managed inside our domain

### Implementation Notes
- Define `IIdentityAuthorizationDataService` in the Application layer to abstract identity lookups
- Implement `IdentityAuthorizationDataService` in Infrastructure using EF Core against the `identity` schema, returning DTOs optimized for caching
- Continue to cache authorization responses and tenant facts in Redis with 10-minute TTL using a cache-aside pattern
- Publish domain events (`RoleMembershipChangedEvent`, `RoleUpdatedEvent`, `TenantCreatedEvent`) from the identity module; consume them to invalidate caches or notify other services
- Log all authorization checks (allow and deny) for audit trail compliance

### References
- [Cache-aside pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
- [CQRS and transactional read models](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [EF Core performance best practices](https://learn.microsoft.com/en-us/ef/core/performance/)

---

## 4. Distributed Caching with Redis

### Decision
Use **StackExchange.Redis** for distributed caching of:
- Authorization decisions (10-minute TTL)
- Tenant facts (10-minute TTL)
- Session state (sliding expiration)
- Invitation idempotency windows

### Rationale
- Redis provides sub-millisecond read latency, critical for <50ms authorization target
- Supports atomic operations (SETNX) for idempotency checks
- Scales horizontally if needed
- Aspire provides first-class Redis resource support
- StackExchange.Redis is the de facto .NET client

### Alternatives Considered
1. **In-memory caching (IMemoryCache)** - Rejected because it's not shared across Web and API instances; would cause inconsistencies in distributed deployments
2. **SQL Server caching** - Rejected because latency too high (~20-50ms) for authorization checks
3. **NCache or other commercial cache** - Rejected due to licensing costs and operational complexity

### Implementation Notes
- Declare `IdentityRedis` resource in Aspire AppHost
- Use sliding expiration for session state (refresh on each access)
- Use absolute expiration for authorization cache (10 minutes)
- Implement cache key naming convention: `authz:{userId}:{tenantId}:{resource}:{action}`
- Use Redis hash structures for tenant facts to support efficient partial updates
- Monitor cache hit rate (target >90% for authorization decisions)

### References
- [StackExchange.Redis documentation](https://stackexchange.github.io/StackExchange.Redis/)
- [.NET Aspire Redis integration](https://learn.microsoft.com/en-us/dotnet/aspire/caching/stackexchange-redis-component)

---

## 5. PostgreSQL for Identity Persistence

### Decision
Use **PostgreSQL** with **Entity Framework Core 9.0** for storing:
- User entities (linked to Entra subject IDs)
- Tenant entities (districts and schools)
- Membership entities (user-tenant-role associations)
- Session metadata (for revocation checks)

### Rationale
- PostgreSQL is robust, ACID-compliant, and well-supported by EF Core
- Aspire provides first-class PostgreSQL resource support
- Supports complex queries (multi-tenant lookups, role hierarchies) efficiently
- JSON column support for flexible session metadata storage
- Strong referential integrity for membership relationships

### Alternatives Considered
1. **SQL Server** - Rejected in favor of PostgreSQL due to licensing costs and organizational preference for open-source
2. **MongoDB** - Rejected because relational model (users, tenants, memberships) is well-suited to SQL; authorization queries benefit from joins
3. **Cosmos DB** - Rejected due to higher cost and overkill for this use case

### Implementation Notes
- Declare `IdentityPostgres` resource in Aspire AppHost
- Create `IdentityDbContext` inheriting from `DbContext`
- Use Fluent API for entity configurations (separate configuration classes)
- Index on:
  - `User.EntraSubjectId` (unique, for lookups during authentication)
  - `Membership.UserId` and `Membership.TenantId` (for authorization queries)
  - `Session.ExpiresAt` (for cleanup jobs)
- Store session tokens as hashed values (SHA256) to prevent token leakage if DB compromised
- Use EF Core migrations for schema versioning

### References
- [EF Core with PostgreSQL](https://learn.microsoft.com/en-us/ef/core/providers/npgsql/)
- [.NET Aspire PostgreSQL integration](https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-component)

---

## 6. .NET Aspire Orchestration

### Decision
Orchestrate all services, databases, and caches through **.NET Aspire 9.0** AppHost with Service Defaults applied to every project.

### Rationale
- Aspire provides unified local development experience (single F5 for entire system)
- Service discovery and configuration propagation handled automatically
- Built-in observability (OpenTelemetry, logs, metrics, traces)
- Aspire dashboard for monitoring during development
- First-class support for PostgreSQL, Redis, and ASP.NET Core apps

### Alternatives Considered
1. **Docker Compose** - Rejected because Aspire provides better .NET integration and built-in observability
2. **Manual configuration** - Rejected due to high friction for multi-service development

### Implementation Notes
- In `AppHost/Program.cs`:
  ```csharp
  var postgres = builder.AddPostgres("IdentityPostgres").AddDatabase("IdentityDb");
  var redis = builder.AddRedis("IdentityRedis");
  
  var api = builder.AddProject<Projects.NorthStarET_NextGen_Lms_Api>("api")
      .WithReference(postgres)
      .WithReference(redis);
  
  var web = builder.AddProject<Projects.NorthStarET_NextGen_Lms_Web>("web")
      .WithReference(api);
  ```
- Apply Service Defaults in each project's `Program.cs`:
  ```csharp
  builder.AddServiceDefaults();
  ```
- Use Aspire service discovery for Web → API communication
- Expose the LMS Identity module via the existing API project (no separate HTTP resource required)

### References
- [.NET Aspire documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire service defaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults)

---

## 8. Session Management and Token Refresh

### Decision
Implement a **hybrid session strategy**:
- Server-side session state stored in Redis (sliding expiration: 30 minutes)
- Client-side authentication cookie (HTTP-only, secure, SameSite=Lax)
- Background token refresh 5 minutes before expiration

### Rationale
- Server-side sessions allow instant revocation (critical for security)
- HTTP-only cookies prevent XSS token theft
- Sliding expiration keeps active users logged in
- Background refresh prevents "mid-action" session expiration

### Alternatives Considered
1. **Client-side JWT-only (no server session)** - Rejected because can't revoke tokens before expiration; required for compliance
2. **Server-side session-only (no tokens)** - Rejected because APIs need bearer tokens for non-browser clients
3. **Database-backed sessions** - Rejected due to higher latency than Redis

### Implementation Notes
- Use ASP.NET Core's `AddDistributedMemoryCache()` backed by Redis
- Configure cookie authentication:
  ```csharp
  services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
      .AddCookie(options => {
          options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
          options.SlidingExpiration = true;
          options.Cookie.HttpOnly = true;
          options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
          options.Cookie.SameSite = SameSiteMode.Lax;
      });
  ```
- Implement JavaScript timer that calls `/api/auth/refresh` 5 minutes before expiration
- On refresh, validate existing token, issue new LMS access token, extend session
- Store session metadata in Redis: `session:{sessionId}` with fields for userId, tenantId, expiresAt

### References
- [ASP.NET Core distributed caching](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [Cookie authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie)

---

## 8. Audit Logging for Authorization Decisions

### Decision
Use **structured logging** (Microsoft.Extensions.Logging with Serilog enrichment) to capture all authorization decisions, writing to:
- Console (development)
- Azure Application Insights (production, via Aspire OpenTelemetry integration)
- PostgreSQL audit table (long-term retention)

### Rationale
- Compliance requirements demand audit trail of who accessed what and when
- Structured logs enable querying by userId, tenantId, resource, action, outcome
- Application Insights provides real-time dashboards and alerting
- PostgreSQL audit table supports long-term retention and regulatory compliance

### Alternatives Considered
1. **File-based logging** - Rejected due to difficulty of querying and lack of real-time analysis
2. **Event sourcing** - Rejected as overkill; authorization decisions are point-in-time, not domain events
3. **Separate audit service** - Deferred; can evolve to this if volume warrants, but synchronous logging is simpler initially

### Implementation Notes
- Create `AuthorizationAuditLog` entity with fields:
  - Timestamp, UserId, TenantId, Resource, Action, Outcome (Allow/Deny), Reason
- Log every authorization check:
  ```csharp
  logger.LogInformation(
      "Authorization decision: User={UserId}, Tenant={TenantId}, Resource={Resource}, Action={Action}, Outcome={Outcome}",
      userId, tenantId, resource, action, outcome);
  ```
- Use Serilog enrichers to add request context (IP, user agent)
- Write to PostgreSQL asynchronously to avoid blocking authorization checks
- Set up Application Insights alert for denied authorization attempts (potential security issue)

### References
- [Serilog structured logging](https://serilog.net/)
- [ASP.NET Core logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [.NET Aspire OpenTelemetry integration](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/telemetry)

---

## 9. Testing Strategy

### Decision
Implement a **four-tier testing strategy**:
1. **Unit tests (xUnit)**: Domain logic, Application services (mocked dependencies)
2. **Integration tests (Aspire test projects)**: Database interactions, Redis caching, full authentication flows
3. **BDD tests (Reqnroll)**: User scenarios from spec (Given/When/Then)
4. **UI tests (Playwright)**: End-to-end browser automation

### Rationale
- Constitution mandates Red → Green TDD
- Unit tests provide fast feedback (<1s per test)
- Aspire integration tests validate real PostgreSQL/Redis interactions
- Reqnroll makes requirements executable and traceable
- Playwright validates actual browser behavior (critical for authentication redirects)

### Alternatives Considered
1. **Manual testing only** - Rejected due to constitution TDD requirement and risk of regressions
2. **Selenium instead of Playwright** - Rejected because Playwright has better developer experience, faster execution, and built-in waiting strategies
3. **SpecFlow instead of Reqnroll** - Reqnroll is the open-source continuation of SpecFlow and better maintained

### Implementation Notes
- **Unit tests**: Mock `IIdentityAuthorizationDataService`, `ICacheService`, `IRepository<T>`; test in isolation
- **Integration tests**: Use `WebApplicationFactory<TEntryPoint>` with Aspire test host; seed test data in PostgreSQL; validate cache writes in Redis
- **BDD tests**: Map spec user stories to `.feature` files; implement step definitions calling Application services; use test doubles for the LMS Identity module when integration is not required
- **Playwright tests**: Test actual Entra redirect flow using test tenant; validate session cookies; test tenant switcher interactions

### Target Coverage
- Unit tests: ≥80% code coverage (measured by line coverage)
- Integration tests: Cover all Application service methods and Repository implementations
- BDD tests: 100% coverage of user stories from spec
- Playwright tests: Cover all critical user journeys (sign-in, authorization deny, tenant switch, session expiration)

### References
- [xUnit documentation](https://xunit.net/)
- [Aspire testing](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing)
- [Reqnroll documentation](https://reqnroll.net/)
- [Playwright for .NET](https://playwright.dev/dotnet/)

---

## Summary of Key Decisions

| Area | Decision | Critical Dependencies |
|------|----------|----------------------|
| Authentication | Microsoft.Identity.Web + ASP.NET Core Identity | Microsoft.Identity.Web 3.x, ASP.NET Core 9.0 |
| Authorization | LMS Identity module with Redis caching | EF Core, StackExchange.Redis, Polly |
| Token Strategy | BFF pattern with Entra JWT → LMS JWT exchange | System.IdentityModel.Tokens.Jwt |
| Storage | PostgreSQL (EF Core) for persistence, Redis for caching | Npgsql.EntityFrameworkCore.PostgreSQL, StackExchange.Redis |
| Orchestration | .NET Aspire with Service Defaults | .NET Aspire 9.0 |
| Session Management | Redis-backed distributed sessions with sliding expiration | ASP.NET Core Data Protection, Redis |
| Testing | xUnit + Aspire integration + Reqnroll + Playwright | xUnit, Reqnroll, Playwright, Aspire.Hosting.Testing |

---

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|-----------|
| LMS Identity module persistence unavailable | Authorization checks fail | Redis caching (10-minute TTL), circuit breaker on data access, fail-closed strategy |
| Entra ID outage | Users can't sign in | Cached tokens continue to work until expiration; communicate outage to users |
| Redis cache miss storm | High latency to identity persistence | Implement cache warming on startup; use Polly bulkhead to limit concurrent data lookups |
| Token expiration mid-action | Poor UX (action fails) | Background token refresh 5 minutes before expiration; graceful session expiration flow |
| Latency budget miss (<50ms) | User perceives sluggishness | Monitor P95 latency; optimize cache hit rate; use Redis pipelining for batch checks |
| Figma assets delayed | UI implementation blocked | Label tasks "Skipped — No Figma"; create `figma-prompts/` for async collaboration; prioritize API/backend work |

---

## Open Questions for Phase 1

1. **Identity module service contracts**: What data shape do the Application services require for permission checks and tenant listings?
2. **Event schema for role changes**: What fields are included in `RoleMembershipChangedEvent` emitted by the LMS Identity module?
3. **Entra tenant configuration**: Are B2B and B2C in separate tenants or combined?
4. **Token signing key rotation**: How often should LMS token signing keys be rotated, and what's the process?
5. **Audit retention policy**: How long should authorization audit logs be retained in PostgreSQL?

These will be clarified during Phase 1 (Design & Contracts).
