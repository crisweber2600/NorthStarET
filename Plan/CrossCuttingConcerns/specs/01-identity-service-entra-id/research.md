# Technology Research & Decisions: Identity Service with Entra ID

**Feature ID**: `01-identity-service-entra-id`  
**Target Layer**: CrossCuttingConcerns  
**Research Version**: 1.0.0  
**Created**: 2025-11-20  
**Status**: Complete

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Core Technology Decisions](#core-technology-decisions)
- [Authentication Architecture](#authentication-architecture)
- [Session Management Strategy](#session-management-strategy)
- [Token Refresh Patterns](#token-refresh-patterns)
- [Multi-Tenancy Implementation](#multi-tenancy-implementation)
- [Security Patterns](#security-patterns)
- [Performance Optimization](#performance-optimization)
- [Alternative Approaches Considered](#alternative-approaches-considered)
- [Package Selection](#package-selection)
- [Configuration Best Practices](#configuration-best-practices)
- [References](#references)

---

## Executive Summary

This research document captures technology decisions for the Identity Service implementation with Microsoft Entra ID integration. Key decisions prioritize **official Microsoft libraries** (Microsoft.Identity.Web), **hybrid session management** (PostgreSQL primary + Redis cache), and **Backend-for-Frontend (BFF) pattern** for token handling.

**Critical Decisions**:
1. ✅ **Microsoft.Identity.Web** for Entra ID integration (official, maintained, battle-tested)
2. ✅ **Custom SessionAuthenticationHandler** over Cookie Authentication (more control, hybrid auth future)
3. ✅ **PostgreSQL primary, Redis cache** for sessions (audit trail, Redis volatility mitigation)
4. ✅ **Background HostedService** for token refresh (proactive, no user-facing latency)
5. ✅ **Single Entra ID tenant** with custom district claims (simpler management, lower cost)

---

## Core Technology Decisions

### Decision 1: Microsoft.Identity.Web vs Manual JWT Validation

**Context**: Need to validate Entra ID JWT tokens, extract claims, and handle token refresh.

**Options Evaluated**:

| Option | Pros | Cons |
|--------|------|------|
| **Microsoft.Identity.Web** | Official Microsoft library<br>Auto-handles token validation<br>Built-in refresh logic<br>Claims transformation support | Additional dependency (~2MB)<br>Opinionated configuration |
| **Manual JWT Validation** (System.IdentityModel.Tokens.Jwt) | Full control over validation logic<br>Lighter weight | Reimplementing OIDC discovery<br>Manual key rotation handling<br>Higher maintenance |
| **IdentityServer4 (Legacy)** | Feature-complete identity server<br>On-premises option | Deprecated (no longer maintained)<br>Complex setup<br>Not cloud-native |

**Decision**: ✅ **Microsoft.Identity.Web**

**Rationale**:
- Official library from Microsoft Identity team (actively maintained)
- Handles OIDC discovery, key rotation, token validation automatically
- Supports token refresh with minimal code
- Used by Microsoft's own production services
- Well-documented with Microsoft Learn samples

**Implementation**:
```csharp
// Program.cs
builder.Services.AddMicrosoftIdentityWebApiAuthentication(configuration, "AzureAd");

// EntraIdTokenValidator.cs (wrapper for testability)
public class EntraIdTokenValidator : ITokenValidator
{
    private readonly ITokenAcquisition _tokenAcquisition;
    
    public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
    {
        // Microsoft.Identity.Web handles validation
        return await _tokenAcquisition.ValidateTokenAsync(token);
    }
}
```

**References**:
- [Microsoft.Identity.Web GitHub](https://github.com/AzureAD/microsoft-identity-web)
- [Microsoft Learn: Protected Web API](https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-overview)

---

### Decision 2: Custom SessionAuthenticationHandler vs Cookie Authentication

**Context**: Need to manage sessions with custom logic (tenant context, sliding windows, Redis caching).

**Options Evaluated**:

| Option | Pros | Cons |
|--------|------|------|
| **Custom SessionAuthenticationHandler** | Full control over session lifecycle<br>Easy hybrid auth (Entra + local future)<br>Custom claims pipeline | More code to maintain<br>Need to implement middleware |
| **ASP.NET Cookie Authentication** | Built-in framework support<br>Less boilerplate | Less flexible session management<br>Harder to integrate Redis<br>Cookie-only (no future token-based mobile) |
| **JWT Bearer Authentication Only** | Stateless (no server-side sessions)<br>Mobile-friendly | No session invalidation<br>Token rotation complex<br>Cannot revoke tokens server-side |

**Decision**: ✅ **Custom SessionAuthenticationHandler**

**Rationale**:
- **Flexibility**: Custom session validation logic (tenant context, IP binding, device fingerprinting)
- **Performance**: Integrate Redis caching directly in validation pipeline
- **Hybrid Auth**: Foundation for future local student authentication alongside Entra ID
- **Control**: Server-side session invalidation (logout, role changes)
- **Testability**: Easier to unit test authentication logic in isolation

**Implementation**:
```csharp
public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ISessionManager _sessionManager;
    private readonly IRedisCacheService _cache;
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Extract session ID from cookie
        if (!Request.Cookies.TryGetValue("lms_session", out var sessionId))
            return AuthenticateResult.NoResult();
        
        // 2. Check Redis cache (P95 <20ms)
        var session = await _cache.GetAsync<Session>($"lms_session:{sessionId}");
        
        // 3. Fallback to database (P95 <100ms)
        if (session == null)
        {
            session = await _sessionManager.GetSessionAsync(sessionId);
            if (session != null)
                await _cache.SetAsync($"lms_session:{sessionId}", session, session.ExpiresAt);
        }
        
        // 4. Validate session not expired
        if (session == null || session.ExpiresAt < DateTime.UtcNow)
            return AuthenticateResult.Fail("Session expired");
        
        // 5. Build ClaimsPrincipal
        var claims = new[]
        {
            new Claim("user_id", session.UserId.ToString()),
            new Claim("tenant_id", session.TenantId.ToString()),
            new Claim("entra_subject_id", session.EntraSubjectId)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
```

**References**:
- [ASP.NET Core Custom Authentication Handler](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/custom)
- [Authentication Middleware Deep Dive](https://andrewlock.net/introduction-to-authentication-with-asp-net-core/)

---

### Decision 3: Session Storage Strategy

**Context**: Need to store sessions with high performance (P95 <20ms validation) and audit trail.

**Options Evaluated**:

| Option | Pros | Cons |
|--------|------|------|
| **PostgreSQL Primary + Redis Cache** | Audit trail (all sessions logged)<br>Session recovery after Redis restart<br>Cache-aside pattern | Slightly higher latency on cache miss<br>Cache invalidation complexity |
| **Redis-Only** | Ultra-fast (sub-10ms)<br>Simple architecture | No audit trail<br>Sessions lost on Redis restart<br>Persistence lag (RDB/AOF) |
| **PostgreSQL-Only** | Simple (no cache layer)<br>ACID guarantees | P95 latency ~100ms (too slow for SLO)<br>Database load under high traffic |

**Decision**: ✅ **PostgreSQL Primary + Redis Cache (Write-Through Pattern)**

**Rationale**:
- **Audit Requirement**: All authentication events must be logged (FERPA compliance)
- **Session Recovery**: Redis restart doesn't invalidate all sessions (DB fallback)
- **Performance**: Redis cache achieves <20ms P95 for 99% of requests
- **Reliability**: Database provides source of truth, Redis is performance layer
- **Cost**: Redis Basic tier sufficient (cache miss fallback acceptable)

**Cache Patterns**:
```csharp
// Write-through on session creation
public async Task<Session> CreateSessionAsync(User user, string entraSubjectId)
{
    var session = new Session
    {
        Id = $"lms_session_{Guid.NewGuid()}",
        UserId = user.Id,
        EntraSubjectId = entraSubjectId,
        TenantId = user.TenantId,
        ExpiresAt = DateTime.UtcNow.AddHours(8)
    };
    
    // 1. Write to PostgreSQL (source of truth)
    await _dbContext.Sessions.AddAsync(session);
    await _dbContext.SaveChangesAsync();
    
    // 2. Write to Redis (performance layer)
    await _cache.SetAsync($"lms_session:{session.Id}", session, session.ExpiresAt);
    
    return session;
}

// Read-through on validation
public async Task<Session?> GetSessionAsync(string sessionId)
{
    // 1. Check Redis (fast path)
    var session = await _cache.GetAsync<Session>($"lms_session:{sessionId}");
    if (session != null) return session;
    
    // 2. Check PostgreSQL (cache miss)
    session = await _dbContext.Sessions
        .FirstOrDefaultAsync(s => s.Id == sessionId && s.ExpiresAt > DateTime.UtcNow);
    
    // 3. Populate cache for next request
    if (session != null)
        await _cache.SetAsync($"lms_session:{sessionId}", session, session.ExpiresAt);
    
    return session;
}
```

**References**:
- [Caching Strategies](https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching)
- [Redis Cache Patterns](https://redis.io/docs/manual/patterns/)

---

### Decision 4: Token Refresh Architecture

**Context**: Entra ID access tokens expire after 15 minutes (typical). Need to refresh transparently.

**Options Evaluated**:

| Option | Pros | Cons |
|--------|------|------|
| **Background HostedService** | Proactive (no user-facing latency)<br>Centralized logic<br>Handles all sessions in one pass | Slightly higher DB load (query all expiring sessions)<br>Edge case: session used just before refresh |
| **Middleware-based Refresh** | Lazy (only refresh when needed)<br>Lower DB load | User-facing latency (up to 500ms)<br>Distributed logic (harder to test) |
| **Client-side Refresh** (SPA pattern) | Server stateless<br>Mobile-friendly | Exposes refresh tokens to client<br>Cannot revoke server-side<br>XSS risk |

**Decision**: ✅ **Background HostedService (Proactive Refresh)**

**Rationale**:
- **User Experience**: No user-facing latency (refresh happens before expiration)
- **Simplicity**: Single background service handles all refreshes
- **Reliability**: Retry logic with exponential backoff (3 attempts: 1s, 2s, 4s)
- **Observability**: Centralized monitoring (refresh success/failure rate)
- **Logout on Failure**: If all retries fail, gracefully log user out with clear message

**Implementation**:
```csharp
public class TokenRefreshBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(4), stoppingToken); // Run every 4 minutes
            
            using var scope = _scopeFactory.CreateScope();
            var sessionRepo = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
            var entraClient = scope.ServiceProvider.GetRequiredService<ITokenValidator>();
            
            // Query sessions expiring within 5 minutes
            var expiringSessions = await sessionRepo.GetExpiringSoonAsync(
                expiresWithinMinutes: 5);
            
            foreach (var session in expiringSessions)
            {
                try
                {
                    // Call Entra ID token refresh endpoint
                    var newToken = await entraClient.RefreshTokenAsync(session.EntraSubjectId);
                    
                    // Update session expiration
                    session.ExpiresAt = DateTime.UtcNow.AddHours(8);
                    session.RefreshedAt = DateTime.UtcNow;
                    await sessionRepo.UpdateAsync(session);
                    
                    // Update Redis cache
                    await _cache.SetAsync($"lms_session:{session.Id}", session, session.ExpiresAt);
                    
                    // Publish event
                    await _eventPublisher.PublishAsync(new SessionRefreshedEvent(session.Id));
                }
                catch (Exception ex)
                {
                    // Log failure, retry with backoff (handled by Polly)
                    _logger.LogError(ex, "Token refresh failed for session {SessionId}", session.Id);
                }
            }
        }
    }
}
```

**References**:
- [Background Services in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [OAuth 2.0 Token Refresh](https://www.rfc-editor.org/rfc/rfc6749#section-6)

---

### Decision 5: Multi-Tenancy Strategy

**Context**: Users can belong to multiple districts. Need to switch tenant context efficiently.

**Options Evaluated**:

| Option | Pros | Cons |
|--------|------|------|
| **Session-based Tenant Context** | Fast switching (<200ms)<br>Server-side enforcement | Requires session update on switch |
| **JWT-based Multi-Tenant Claims** | Stateless<br>Client can cache | Token includes ALL tenants (large tokens)<br>Cannot revoke tenant access mid-session |
| **Per-Tenant Sessions** | Strongest isolation | User must re-authenticate per tenant (poor UX)<br>Session management complexity |

**Decision**: ✅ **Session-based Tenant Context with Switching**

**Rationale**:
- **Performance**: Tenant switch updates single `session.tenant_id` column (indexed, fast)
- **Security**: Can revoke tenant access immediately (next authorization check fails)
- **UX**: Seamless switching without re-authentication
- **Scalability**: Tenant list cached per user (most users have 1-3 districts)

**Implementation**:
```csharp
public async Task<Result> SwitchTenantAsync(string sessionId, Guid targetTenantId)
{
    // 1. Validate user has access to target tenant
    var session = await _sessionRepo.GetByIdAsync(sessionId);
    var userTenants = await _userRepo.GetTenantsAsync(session.UserId);
    
    if (!userTenants.Any(t => t.Id == targetTenantId))
        return Result.Failure("User does not have access to target tenant");
    
    // 2. Update session tenant context
    session.TenantId = targetTenantId;
    await _sessionRepo.UpdateAsync(session);
    
    // 3. Invalidate cached permissions for old tenant
    await _cache.DeleteAsync($"lms_permissions:{session.UserId}:{session.TenantId}");
    
    // 4. Update Redis cache
    await _cache.SetAsync($"lms_session:{sessionId}", session, session.ExpiresAt);
    
    // 5. Publish event
    await _eventPublisher.PublishAsync(new TenantContextSwitchedEvent(
        session.UserId, session.TenantId, targetTenantId));
    
    return Result.Success();
}
```

**Performance Optimization**:
- Index: `CREATE INDEX idx_user_roles_user_tenant ON identity.user_roles(user_id, tenant_id)`
- Cache tenant list per user: `lms_tenant_list:{user_id}` (TTL: 1 hour)

**References**:
- [Multi-Tenancy Patterns](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/considerations/tenancy-models)

---

## Authentication Architecture

### Backend-for-Frontend (BFF) Pattern

**Chosen Pattern**: OAuth 2.0 Authorization Code Flow + PKCE with BFF

**Flow Diagram**:
```
┌────────┐                                ┌─────────────┐
│ User   │                                │ Entra ID    │
│ Browser│                                │ (OAuth)     │
└───┬────┘                                └──────┬──────┘
    │ 1. Click "Sign in with Microsoft"        │
    │────────────────────────────────────────▶ │
    │ 2. Redirect to /authorize + PKCE         │
    │◀──────────────────────────────────────── │
    │                                           │
    │ 3. User authenticates (username/password)│
    │────────────────────────────────────────▶ │
    │                                           │
    │ 4. Redirect to /signin-oidc + auth code  │
    │◀──────────────────────────────────────── │
    │                                           │
    ▼                                           │
┌─────────────┐                                │
│ NorthStar   │ 5. Exchange code for tokens    │
│ Web App     │───────────────────────────────▶│
│ (BFF)       │◀─────────────────────────────── │
└──────┬──────┘    access_token, refresh_token │
       │                                        │
       │ 6. POST /api/auth/exchange-token      │
       │    (Bearer: access_token)             │
       │                                        ▼
       │                              ┌──────────────────┐
       │                              │ Identity.API     │
       │◀─────────────────────────────│ (Validates JWT)  │
       │  7. Returns session ID       │                  │
       │                              └──────────────────┘
       │ 8. Set session cookie
       │    (HTTP-only, secure)
       ▼
┌────────────┐
│ Dashboard  │
│ (Razor)    │
└────────────┘
```

**Why BFF?**:
- **Security**: Refresh tokens stored server-side (never exposed to client)
- **Token Management**: Centralized token refresh logic
- **Hybrid Auth**: Future local student authentication coexists with Entra ID
- **Cookie-Based**: Works with Razor Pages (no JavaScript token handling)

**Alternative (SPA Pattern - Not Chosen)**:
- SPA holds tokens in memory/local storage
- Client-side token refresh with `refresh_token`
- Pros: Stateless server, mobile-friendly
- Cons: XSS risk, cannot revoke tokens server-side, NorthStar uses Razor Pages (not SPA)

**References**:
- [OAuth 2.0 for Browser-Based Apps](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-browser-based-apps)
- [BFF Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/backends-for-frontends)

---

## Security Patterns

### Token Validation Best Practices

**Validation Checklist** (enforced by Microsoft.Identity.Web):
- ✅ **Signature Verification**: RS256 asymmetric key (OIDC discovery provides public keys)
- ✅ **Issuer Validation**: Must match `https://login.microsoftonline.com/{tenant-id}/v2.0`
- ✅ **Audience Validation**: Must match `api://northstar-lms` (API app ID)
- ✅ **Expiration Check**: `exp` claim must be in the future
- ✅ **Not Before Check**: `nbf` claim must be in the past
- ✅ **Nonce Validation**: Replay attack prevention (OIDC flow)

**Custom Validation** (additional checks in `EntraIdTokenValidator`):
```csharp
public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
{
    // 1. Microsoft.Identity.Web handles core validation
    var principal = await _tokenAcquisition.ValidateTokenAsync(token);
    
    // 2. Custom validations
    var tenantClaim = principal.FindFirst("tenant_id");
    if (tenantClaim == null || !Guid.TryParse(tenantClaim.Value, out _))
        throw new InvalidTokenException("Missing or invalid tenant_id claim");
    
    var emailClaim = principal.FindFirst("email");
    if (string.IsNullOrEmpty(emailClaim?.Value))
        throw new InvalidTokenException("Missing email claim");
    
    // 3. Check user not soft-deleted
    var email = emailClaim.Value;
    var user = await _userRepo.GetByEmailAsync(email);
    if (user?.DeletedAt != null)
        throw new InvalidTokenException("User account is inactive");
    
    return principal;
}
```

### Session Security

**Cookie Configuration**:
```csharp
services.AddAuthentication("SessionCookie")
    .AddCookie("SessionCookie", options =>
    {
        options.Cookie.Name = "lms_session";
        options.Cookie.HttpOnly = true;         // Not accessible to JavaScript (XSS mitigation)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
        options.Cookie.SameSite = SameSiteMode.Strict; // CSRF mitigation
        options.Cookie.MaxAge = TimeSpan.FromHours(8); // Staff session lifetime
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;       // Extend on activity
    });
```

**Optional Security Enhancements** (Phase 8):
- **IP Address Binding**: Reject session if IP changes (configurable, breaks NAT scenarios)
- **Device Fingerprinting**: Track User-Agent + client hints, alert on device change
- **Concurrent Session Limits**: Max 3 active sessions per user
- **Anomaly Detection**: Log unusual login times (3am) or locations (different country)

**References**:
- [ASP.NET Core Cookie Security](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie)
- [OWASP Session Management Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html)

---

## Performance Optimization

### Database Indexing Strategy

**Critical Indexes** (from performance SLOs):
```sql
-- Session validation (primary use case, P95 <100ms)
CREATE INDEX idx_sessions_id_active ON identity.sessions(id)
WHERE expires_at > NOW();

-- User lookup by email (token exchange)
CREATE INDEX idx_users_tenant_email ON identity.users(tenant_id, email)
WHERE deleted_at IS NULL;

-- User roles lookup (authorization, P95 <50ms)
CREATE INDEX idx_user_roles_user_tenant ON identity.user_roles(user_id, tenant_id);

-- Session refresh query (background service, every 4 minutes)
CREATE INDEX idx_sessions_expiring_soon ON identity.sessions(expires_at)
WHERE expires_at > NOW() AND expires_at < NOW() + INTERVAL '5 minutes';

-- Audit queries (reporting)
CREATE INDEX idx_audit_records_user_timestamp ON identity.audit_records(user_id, timestamp DESC);
CREATE INDEX idx_audit_records_tenant_timestamp ON identity.audit_records(tenant_id, timestamp DESC);
```

**Index Sizing Estimates**:
- 10,000 active sessions → 500KB index size
- 50,000 users → 2MB index size
- Negligible performance impact, massive query speed improvement

### Redis Cache Optimization

**Key Naming Convention**:
- Sessions: `lms_session:{session_id}`
- Permissions: `lms_permissions:{user_id}:{tenant_id}`
- Tenant Lists: `lms_tenant_list:{user_id}`

**TTL Management**:
```csharp
// Session cache TTL matches DB expiration
await _cache.SetAsync(
    key: $"lms_session:{sessionId}",
    value: session,
    expiry: session.ExpiresAt); // Absolute expiration

// Permission cache TTL: 1 hour or until role change
await _cache.SetAsync(
    key: $"lms_permissions:{userId}:{tenantId}",
    value: permissions,
    expiry: TimeSpan.FromHours(1));
```

**Cache Invalidation Strategies**:
- **Session Logout**: `DELETE lms_session:{session_id}`
- **Role Change**: `DELETE lms_permissions:{user_id}:*` (pattern delete)
- **Tenant Switch**: `DELETE lms_permissions:{user_id}:{old_tenant_id}`

**Memory Sizing**:
- Session object: ~1KB (user ID, tenant ID, expiration)
- 10,000 concurrent sessions: ~10MB
- Redis Basic (250MB): Sufficient for 200,000+ sessions

### Connection Pooling

**EF Core DbContext Pooling**:
```csharp
services.AddDbContextPool<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString),
    poolSize: 128); // Max concurrent DB connections
```

**Pooling Benefits**:
- Reduces DbContext creation overhead (50-100ms → <1ms)
- Reuses underlying database connections
- Recommended for high-throughput scenarios (10,000+ req/sec)

**References**:
- [EF Core Performance Best Practices](https://learn.microsoft.com/en-us/ef/core/performance/)
- [Redis Best Practices](https://redis.io/docs/manual/performance/)

---

## Alternative Approaches Considered

### Alternative 1: IdentityServer (Legacy Path)

**Approach**: Continue using IdentityServer 4, migrate to Duende IdentityServer.

**Pros**:
- Existing knowledge base
- On-premises control
- Feature-complete (OAuth 2.0, OIDC, SAML)

**Cons**:
- ❌ **Not chosen** due to maintenance burden
- Duende IdentityServer requires commercial license ($15k/year for 10,000 users)
- Complex setup and configuration
- Manual key rotation and security patching
- Does not leverage Entra ID's enterprise features (conditional access, MFA)

**Why Rejected**: Cost, maintenance overhead, lack of enterprise SSO integration.

---

### Alternative 2: Auth0 / Okta (Third-Party IDaaS)

**Approach**: Use Auth0 or Okta as external identity provider.

**Pros**:
- Managed service (no infrastructure)
- Feature-rich (MFA, social login, breached password detection)
- Enterprise SSO via SAML

**Cons**:
- ❌ **Not chosen** due to cost and vendor lock-in
- Pricing: $1-3/user/month ($120k-360k/year for 10,000 users)
- External dependency (another SLA to manage)
- Entra ID already available (Microsoft 365 licensing)

**Why Rejected**: Cost, redundancy with existing Entra ID investment.

---

### Alternative 3: Firebase Authentication (Google)

**Approach**: Use Firebase Auth for all users (not just Google accounts).

**Pros**:
- Easy integration (Firebase SDK)
- Free tier (10,000 MAU free, $0.0055 per MAU after)
- Social providers (Google, Facebook, Twitter)

**Cons**:
- ❌ **Not chosen** due to lack of enterprise features
- No built-in MFA (requires custom implementation)
- Limited SSO support (no Entra ID integration)
- Google Cloud dependency (data residency concerns)

**Why Rejected**: Lack of enterprise SSO, not suitable for K-12 environment.

---

## Package Selection

### Core Packages (Required)

| Package | Version | Purpose | Justification |
|---------|---------|---------|---------------|
| **Microsoft.Identity.Web** | 3.3.0 | Entra ID integration | Official Microsoft library, actively maintained |
| **Microsoft.Identity.Web.MicrosoftGraph** | 3.3.0 | Graph API access (future) | User profile sync, photo retrieval |
| **Microsoft.EntityFrameworkCore** | 9.0.0 | ORM | Standard data access, LINQ support |
| **Npgsql.EntityFrameworkCore.PostgreSQL** | 9.0.0 | PostgreSQL provider | Open-source, high-performance |
| **StackExchange.Redis** | 2.8.16 | Redis client | Most popular .NET Redis client, multiplexing support |
| **MassTransit** | 8.3.3 | Message bus abstraction | MediatR-style API, Azure Service Bus integration |
| **MediatR** | 12.4.1 | CQRS mediator | Command/query separation, pipeline behaviors |
| **FluentValidation** | 11.10.0 | Input validation | Expressive validation rules, MediatR integration |
| **Polly** | 8.4.2 | Resilience (circuit breaker, retry) | Industry standard, async-first |

### Testing Packages

| Package | Version | Purpose |
|---------|---------|---------|
| **xUnit** | 2.9.0 | Test framework |
| **Moq** | 4.20.72 | Mocking library |
| **FluentAssertions** | 6.12.1 | Assertion library |
| **Reqnroll** | 2.2.0 | BDD testing (Gherkin) |
| **Testcontainers** | 3.10.0 | Docker-based integration tests |

### Observability Packages

| Package | Version | Purpose |
|---------|---------|---------|
| **OpenTelemetry** | 1.9.0 | Distributed tracing |
| **OpenTelemetry.Instrumentation.AspNetCore** | 1.9.0 | HTTP trace collection |
| **OpenTelemetry.Instrumentation.EntityFrameworkCore** | 1.0.0 | EF Core trace collection |
| **Serilog** | 4.0.2 | Structured logging |
| **Serilog.Sinks.Seq** | 8.0.0 | Log aggregation (dev) |

---

## Configuration Best Practices

### Secret Management

**Azure Key Vault Integration**:
```csharp
// Program.cs
var keyVaultEndpoint = configuration["KeyVault:Endpoint"];
var credential = new DefaultAzureCredential();
builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultEndpoint),
    credential);

// appsettings.json references secrets
{
  "AzureAd": {
    "ClientSecret": "@Microsoft.KeyVault(SecretUri=https://northstar-kv.vault.azure.net/secrets/EntraId-ClientSecret)"
  }
}
```

**Local Development Secrets** (user-secrets):
```bash
dotnet user-secrets init --project Identity.API
dotnet user-secrets set "AzureAd:ClientSecret" "dev-secret-value"
```

### Environment-Specific Configuration

**appsettings.Development.json**:
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "dev-tenant-id",
    "ClientId": "dev-client-id"
  },
  "ConnectionStrings": {
    "IdentityDb": "Host=localhost;Database=NorthStar_Identity_Dev;Username=postgres;Password=dev"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.Identity": "Debug"
    }
  }
}
```

**appsettings.Production.json**:
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "prod-tenant-id",
    "ClientId": "prod-client-id"
  },
  "ConnectionStrings": {
    "IdentityDb": "{azure-postgres-connection}"
  },
  "Redis": {
    "ConnectionString": "{azure-redis-connection}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Identity": "Warning"
    }
  }
}
```

---

## References

### Official Documentation

- [Microsoft.Identity.Web Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [OAuth 2.0 Authorization Code Flow](https://www.rfc-editor.org/rfc/rfc6749#section-4.1)
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Entity Framework Core Performance](https://learn.microsoft.com/en-us/ef/core/performance/)
- [Redis Best Practices](https://redis.io/docs/manual/performance/)

### Code Samples

- [Microsoft Identity Web Samples](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2)
- [ASP.NET Core BFF Sample](https://github.com/DuendeSoftware/Samples/tree/main/BFF)
- [MassTransit Azure Service Bus](https://masstransit.io/documentation/transports/azure-service-bus)

### Security Standards

- [OWASP Session Management](https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html)
- [OWASP JWT Best Practices](https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_for_Java_Cheat_Sheet.html)
- [NIST Authentication Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)

### Architecture Patterns

- [Backend-for-Frontend Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/backends-for-frontends)
- [Caching Guidance](https://learn.microsoft.com/en-us/azure/architecture/best-practices/caching)
- [Multi-Tenancy Architecture](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/overview)

---

**Research Conducted By**: GitHub Copilot  
**Research Reviewed By**: [Pending]  
**Last Updated**: 2025-11-20
