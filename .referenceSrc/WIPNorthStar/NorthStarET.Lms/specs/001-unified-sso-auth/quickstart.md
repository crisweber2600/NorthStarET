# Quickstart Guide: Unified SSO & Authorization

**Feature**: 001-unified-sso-auth  
**Last Updated**: 2025-10-20

This guide helps developers quickly set up and understand the authentication and authorization system for local development.

---

## Prerequisites

- .NET 9.0 SDK installed
- Docker Desktop running (for PostgreSQL and Redis via Aspire)
- Visual Studio 2022 or VS Code with C# extension
- Microsoft Entra ID test tenant configured (see Setup section)
- Git repository cloned

---

## Quick Setup (5 minutes)

### 1. Clone and Restore

```bash
cd /home/cweber/NorthStarET.Lms
dotnet restore NorthStarET.NextGen.Lms.sln
```

### 2. Configure Entra ID Settings

Create `src/NorthStarET.NextGen.Lms.AppHost/appsettings.Development.json`:

```json
{
  "EntraId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_ENTRA_TENANT_ID",
    "ClientId": "YOUR_APP_REGISTRATION_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "CallbackPath": "/signin-oidc",
    "B2C": {
      "Instance": "https://YOUR_TENANT.b2clogin.com/",
      "Domain": "YOUR_TENANT.onmicrosoft.com",
      "SignUpSignInPolicyId": "B2C_1_susi"
    }
  },
  "IdentityModule": {
    "Schema": "identity",
    "AuthorizationCacheTtlMinutes": 10
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "PostgreSQL": {
    "ConnectionString": "Host=localhost;Database=LmsIdentity;Username=postgres;Password=postgres"
  }
}
```

**Important**: Never commit this file. Actual secrets should be in user secrets or Azure Key Vault.

### 3. Set User Secrets (Recommended)

```bash
cd src/NorthStarET.NextGen.Lms.Api
dotnet user-secrets init
dotnet user-secrets set "EntraId:ClientSecret" "YOUR_CLIENT_SECRET"
# Identity module currently uses database + Redis configuration only; no additional secrets required
```

### 4. Run Database Migrations

```bash
cd src/NorthStarET.NextGen.Lms.Infrastructure
dotnet ef migrations add InitialIdentitySchema --context IdentityDbContext --output-dir Identity/Persistence/Migrations
dotnet ef database update --context IdentityDbContext
```

### 5. Start Aspire AppHost

```bash
cd src/NorthStarET.NextGen.Lms.AppHost
dotnet run
```

This starts:
- Aspire Dashboard: `http://localhost:15000`
- Web MVC app: `https://localhost:7001`
- Web API: `https://localhost:7002`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

### 6. Open Aspire Dashboard

Navigate to `http://localhost:15000` to see:
- Running services
- Logs and traces
- Resource health

---

## Project Structure Overview

```
src/
├── NorthStarET.NextGen.Lms.Web/              # ASP.NET Core MVC app (user-facing)
├── NorthStarET.NextGen.Lms.Api/              # Web API (authentication endpoints)
├── NorthStarET.NextGen.Lms.Application/      # Application services (CQRS)
├── NorthStarET.NextGen.Lms.Domain/           # Domain entities and events
├── NorthStarET.NextGen.Lms.Infrastructure/   # EF Core, Redis, HTTP clients
├── NorthStarET.NextGen.Lms.Contracts/        # Shared DTOs
├── NorthStarET.NextGen.Lms.AppHost/          # Aspire orchestration
└── NorthStarET.NextGen.LMS.ServiceDefaults/  # Aspire service defaults
```

---

## Key Concepts

### Authentication Flow

1. **User navigates to MVC app** → Redirected to Entra ID sign-in
2. **User authenticates with Entra** → Entra redirects back with JWT
3. **MVC app calls API `/auth/exchange-token`** → Validates Entra JWT, creates session
4. **API returns LMS access token** → Stored in HTTP-only cookie
5. **User navigates between portals** → LMS token grants access without re-auth

### Authorization Flow

1. **User attempts protected action** (e.g., "Edit Student Grades")
2. **Application layer calls `IAuthorizationService.CheckPermission`**
3. **Service checks Redis cache** → Cache hit? Return result
4. **On cache miss, query LMS Identity module projections** via `IIdentityAuthorizationDataService`
5. **Cache result for 10 minutes** → Return allow/deny to user

### Tenant Switching

1. **User selects different tenant** from dropdown
2. **Application layer calls `SwitchTenantContextCommand`**
3. **Validates user has membership** in target tenant
4. **Updates session in Redis and PostgreSQL**
5. **Clears authorization cache** for old tenant
6. **Raises `TenantSwitchedEvent`**

---

## Common Development Tasks

### Add a New Protected Action

1. **Define permission** in the LMS Identity module seed data or administration tooling
2. **In your controller/service**, call authorization:
   ```csharp
   var allowed = await _authorizationService.CheckPermissionAsync(
       userId,
       activeTenantId,
       resource: "Grades",
       action: "Write");
   
   if (!allowed)
   {
       return Forbid();
   }
   ```

### Test with Different Roles

Seed test users in `Infrastructure/Identity/Persistence/IdentityDbContextSeed.cs`:

```csharp
public static async Task SeedAsync(IdentityDbContext context)
{
    // System Admin
    var sysAdmin = new User(
        new EntraSubjectId("entra-subject-123"),
        "admin@example.com",
        "System",
        "Admin");
    
    // Grant membership
    var membership = new Membership(
        sysAdmin.Id,
        districtTenantId,
        systemAdminRoleId);
    
    await context.Users.AddAsync(sysAdmin);
    await context.Memberships.AddAsync(membership);
    await context.SaveChangesAsync();
}
```

### Debug Authorization Decisions

Check Aspire Dashboard logs for:
- `Authorization decision: User={userId}, Tenant={tenantId}, Resource={resource}, Action={action}, Outcome={outcome}`

Or query PostgreSQL audit table:
```sql
SELECT * FROM AuthorizationAuditLogs
WHERE UserId = 'user-guid'
ORDER BY Timestamp DESC
LIMIT 20;
```

### Invalidate Cache for Testing

```bash
# Connect to Redis
docker exec -it <redis-container-id> redis-cli

# Clear all authorization cache
KEYS authz:*
DEL <key1> <key2> ...

# Or flush all (development only!)
FLUSHALL
```

---

## Testing

### Unit Tests

```bash
cd tests/unit/NorthStarET.NextGen.Lms.Application.Tests
dotnet test
```

**Example test**:
```csharp
[Fact]
public async Task CheckPermission_WhenUserHasRole_ReturnsAllowed()
{
    // Arrange
  var service = new AuthorizationService(
    mockIdentityAuthorizationDataService.Object,
    mockCacheService.Object);
    
    // Act
    var result = await service.CheckPermissionAsync(
        userId, tenantId, "Grades", "Write");
    
    // Assert
    Assert.True(result);
}
```

### Integration Tests (Aspire)

```bash
cd tests/integration/NorthStarET.NextGen.Lms.Identity.IntegrationTests
dotnet test
```

**Example test**:
```csharp
public class AuthenticationFlowTests : IClassFixture<AspireFixture>
{
    [Fact]
    public async Task ExchangeToken_WithValidEntraToken_CreatesSession()
    {
        // Uses real PostgreSQL and Redis via Aspire test host
        var response = await client.PostAsync("/auth/exchange-token", ...);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = await redis.GetAsync<Session>(...);
        Assert.NotNull(session);
    }
}
```

### BDD Tests (Reqnroll)

```bash
cd tests/bdd/NorthStarET.NextGen.Lms.Bdd
dotnet test
```

**Example feature**:
```gherkin
Feature: Single Sign-On Access
  
  Scenario: User signs in and accesses multiple portals
    Given a user with email "teacher@lincoln.edu"
    When they navigate to the LMS portal
    Then they are redirected to Entra sign-in
    When they authenticate with valid credentials
    Then they are redirected back to the LMS portal
    And they see their name "Bob Teacher" displayed
    When they navigate to the Admin portal
    Then they gain immediate access without re-authentication
```

### UI Tests (Playwright)

```bash
cd tests/ui/NorthStarET.NextGen.Lms.Playwright
pwsh playwright.ps1
```

---

## Troubleshooting

### "Entra token validation failed"

**Symptom**: 401 Unauthorized when calling `/auth/exchange-token`

**Causes**:
- Entra JWT is expired (check `exp` claim)
- Client ID mismatch (check `aud` claim)
- Invalid signature (check Entra public keys cache)

**Fix**:
1. Decode JWT at https://jwt.ms
2. Verify `aud` matches your Client ID in appsettings
3. Check Aspire logs for detailed validation errors

### "Identity module persistence unavailable"

**Symptom**: Authorization checks fail with 503

**Causes**:
- PostgreSQL unavailable (identity schema offline)
- Network connectivity issue
- Database credentials invalid

**Fix**:
1. Verify PostgreSQL container/resource is healthy in Aspire Dashboard
2. Confirm connection string in appsettings matches running PostgreSQL instance
3. Check Polly circuit breaker state in Aspire Dashboard for identity data access

### "Session not found"

**Symptom**: User sees "Session expired" even though recently logged in

**Causes**:
- Redis cache evicted session (memory pressure)
- Session expired (check TTL)
- Session revoked

**Fix**:
1. Check Redis: `docker exec -it <redis> redis-cli KEYS session:*`
2. Verify session exists in PostgreSQL: `SELECT * FROM Sessions WHERE Id = 'session-guid'`
3. Check `ExpiresAt` and `IsRevoked` columns

### Performance: Authorization checks >50ms

**Symptom**: Slow authorization decisions

**Causes**:
- Cache misses (falling back to database queries)
- Identity module queries slow (missing indexes)
- Network latency

**Fix**:
1. Check cache hit rate in Aspire metrics (target >90%)
2. Monitor database query performance for identity projections (should be <20ms)
3. Verify Redis is local (not remote)
4. Check circuit breaker isn't open (repeated failures)

### Session Expiration and Renewal

**Symptom**: Users are prompted to re-authenticate frequently or sessions expire unexpectedly

**Causes**:
- Session TTL too short (default is 30 minutes)
- TokenRefreshService not running or failing
- Entra refresh token expired
- Redis cache eviction due to memory pressure

**Fix**:
1. **Check active session expiration**:
   ```sql
   SELECT Id, UserId, CreatedAt, ExpiresAt, LastActivityAt, IsRevoked
   FROM identity.Sessions
   WHERE UserId = 'user-guid'
   AND IsRevoked = false
   ORDER BY CreatedAt DESC
   LIMIT 5;
   ```

2. **Verify TokenRefreshService is running**:
   - Check Aspire Dashboard logs for "TokenRefreshService starting..."
   - Look for background refresh activity every 5 minutes
   - Verify no exceptions in refresh logic

3. **Manually refresh a session via API**:
   ```bash
   curl -X POST https://localhost:7002/api/sessions/{sessionId}/refresh \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"sessionId": "session-guid"}'
   ```

4. **Check Redis cache for session**:
   ```bash
   docker exec -it <redis-container> redis-cli
   KEYS identity:session:*
   TTL identity:session:{session-guid}
   GET identity:session:{session-guid}
   ```

5. **Increase session TTL** (if needed):
   Update `appsettings.json`:
   ```json
   {
     "IdentityModule": {
       "SessionTtlMinutes": 60,
       "TokenRefreshThresholdMinutes": 15
     }
   }
   ```

6. **Monitor session lifecycle events**:
   - Check for `UserAuthenticatedEvent` - session created
   - Check for `SessionExpiredEvent` - session expired
   - Look for automatic refresh attempts in logs

**Common Issues**:
- **Session revoked unexpectedly**: Check if admin or system revoked the session via `/api/sessions/{id}/revoke`
- **Background refresh not working**: Verify `ISessionRepository` is registered and accessible
- **Cache invalidation**: Session may be valid in DB but missing from Redis - force cache refresh by calling validate endpoint

### Session Revocation

**Symptom**: Need to immediately terminate user sessions (e.g., for security incident)

**How to revoke a specific session**:
```bash
curl -X POST https://localhost:7002/api/sessions/{sessionId}/revoke \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"sessionId": "session-guid"}'
```

**How to revoke all sessions for a user**:
```sql
-- Mark all sessions as revoked
UPDATE identity.Sessions
SET IsRevoked = true
WHERE UserId = 'user-guid' AND IsRevoked = false;

-- Clear from Redis cache
-- Connect to Redis and run:
-- KEYS identity:session:*
-- DEL identity:session:{each-session-id}
```

**Verify revocation**:
```bash
# Attempt to use revoked session - should return 401
curl https://localhost:7002/api/authentication/validate \
  -H "X-Session-Id: revoked-session-guid"
```

---

## Next Steps

1. **Implement your first protected action**: Add authorization check to a controller
2. **Create custom role**: Update LMS Identity module role seed or admin tooling to define permissions
3. **Add tenant-scoped data**: Query data filtered by `activeTenantId`
4. **Write tests**: TDD for every new authorization rule

---

## Additional Resources

- [Feature Specification](./spec.md)
- [Implementation Plan](./plan.md)
- [Data Model](./data-model.md)
- [API Contracts](./contracts/)
- [Research Decisions](./research.md)

---

## Support

For questions or issues:
- **Team Chat**: #lms-auth-team
- **Wiki**: https://wiki.northstaret.com/lms/auth
- **Architecture Review**: Schedule via calendar
