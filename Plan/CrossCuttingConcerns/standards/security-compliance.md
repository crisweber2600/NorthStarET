# Security & Compliance Pattern

**Constitution Principle**: Principle 5 - Security & Compliance Safeguards  
**Priority**: 🔴 Critical  
**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Table of Contents

- [Overview](#overview)
- [Microsoft Entra ID Authentication](#microsoft-entra-id-authentication)
- [Custom SessionAuthenticationHandler](#custom-sessionauthenticationhandler)
- [Token Exchange Service (BFF Pattern)](#token-exchange-service-bff-pattern)
- [Role-Based Authorization (RBAC)](#role-based-authorization-rbac)
- [Multi-Tenant Data Isolation Security](#multi-tenant-data-isolation-security)
- [FERPA Compliance Patterns](#ferpa-compliance-patterns)
- [Secret Management](#secret-management)
- [Audit Logging](#audit-logging)
- [Security Testing Patterns](#security-testing-patterns)
- [Anti-Patterns](#anti-patterns)
- [Performance Considerations](#performance-considerations)
- [References](#references)

---

## Overview

Security and compliance are non-negotiable requirements for NorthStar LMS. Constitution Principle 5 mandates layered defense mechanisms, least privilege access, and complete audit trails for FERPA compliance.

**Core Security Principles**:
1. **Authentication**: Microsoft Entra ID (OIDC) - no other identity providers
2. **Authorization**: Role-based (RBAC) with tenant-scoped permissions
3. **Data Isolation**: Multi-tenant boundaries enforced at application and database layers
4. **Compliance**: FERPA audit trails, data encryption, secure secret management
5. **Defense in Depth**: Multiple security layers (JWT validation, session validation, RLS policies)

**Security Layers**:
```
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway (YARP)                      │
│  ✓ JWT Validation   ✓ Rate Limiting   ✓ TLS Termination    │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                 SessionAuthenticationHandler                 │
│  ✓ Session Validation   ✓ Tenant Claims   ✓ Token Refresh  │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                   Application Authorization                  │
│  ✓ RBAC Policies   ✓ Permission Checks   ✓ Tenant Scope    │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│               Database (PostgreSQL RLS + Audit)              │
│  ✓ Row-Level Security   ✓ Audit Triggers   ✓ Encryption    │
└─────────────────────────────────────────────────────────────┘
```

---

## Microsoft Entra ID Authentication

### OIDC Configuration

NorthStar uses **Microsoft Entra ID exclusively** for authentication via OpenID Connect (OIDC).

**Web Application Configuration** (`Program.cs`):

```csharp
// Location: Src/Foundation/services/Identity/Api/Program.cs
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Microsoft Entra ID authentication (OIDC)
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        
        // Required scopes
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("offline_access");
        
        // Token validation
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 min clock drift
        };
        
        // Event handlers for logging/telemetry
        options.Events = new OpenIdConnectEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                // Extract claims and create session
                var tokenExchangeService = context.HttpContext
                    .RequestServices.GetRequiredService<ITokenExchangeService>();
                
                var sessionId = await tokenExchangeService
                    .ExchangeEntraTokenForSessionAsync(
                        context.SecurityToken.RawData,
                        context.HttpContext.RequestAborted);
                
                // Add session ID to claims
                var identity = (ClaimsIdentity)context.Principal!.Identity!;
                identity.AddClaim(new Claim("session_id", sessionId.ToString()));
            }
        };
    });

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
```

**Configuration** (`appsettings.json`):

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "northstaret.onmicrosoft.com",
    "TenantId": "common", // Multi-tenant: supports any org tenant
    "ClientId": "your-client-id-from-entra",
    "ClientSecret": "stored-in-key-vault",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "ResponseType": "code", // Authorization code flow
    "SaveTokens": true // Cache tokens in session
  }
}
```

### API JWT Validation

**API Services** use Microsoft.Identity.Web to validate JWTs from Entra ID:

```csharp
// Location: Src/Foundation/services/Student/Api/Program.cs
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// JWT Bearer authentication for APIs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudiences = new[] 
            { 
                "api://northstar-lms",
                "api://northstar-student-api" 
            },
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/v2.0"
            }
        };
    }, options =>
    {
        builder.Configuration.Bind("AzureAd", options);
    });
```

### User Claims Mapping

**Claims extracted from Entra ID tokens**:

```csharp
// Location: Src/Foundation/shared/Infrastructure/Identity/ClaimsPrincipalExtensions.cs
using System.Security.Claims;

namespace NorthStarET.Foundation.Infrastructure.Identity;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var oid = principal.FindFirstValue("oid") // Entra ID object ID
               ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new InvalidOperationException("User ID claim not found");
        
        return Guid.Parse(oid);
    }
    
    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("preferred_username")
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? throw new InvalidOperationException("Email claim not found");
    }
    
    public static Guid GetTenantId(this ClaimsPrincipal principal)
    {
        var tenantClaim = principal.FindFirstValue("tenant_id")
            ?? throw new InvalidOperationException("Tenant ID claim not found");
        
        return Guid.Parse(tenantClaim);
    }
    
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll("roles")
            .Select(c => c.Value)
            .Concat(principal.FindAll(ClaimTypes.Role).Select(c => c.Value))
            .Distinct();
    }
}
```

---

## Custom SessionAuthenticationHandler

NorthStar uses a custom authentication handler for **Backend-for-Frontend (BFF)** pattern, exchanging Entra tokens for internal session IDs cached in Redis.

### SessionAuthenticationHandler Implementation

```csharp
// Location: Src/Foundation/services/Identity/Infrastructure/Authentication/SessionAuthenticationHandler.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NorthStarET.Foundation.Identity.Infrastructure.Authentication;

/// <summary>
/// Custom authentication handler that validates session IDs and loads claims from Redis/DB
/// </summary>
public sealed class SessionAuthenticationHandler : AuthenticationHandler<SessionAuthenticationOptions>
{
    private readonly IDistributedCache _cache;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<SessionAuthenticationHandler> _logger;
    
    public SessionAuthenticationHandler(
        IOptionsMonitor<SessionAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IDistributedCache cache,
        ISessionRepository sessionRepository)
        : base(options, logger, encoder)
    {
        _cache = cache;
        _sessionRepository = sessionRepository;
        _logger = logger.CreateLogger<SessionAuthenticationHandler>();
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract session ID from Authorization header or cookie
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return AuthenticateResult.NoResult();
        }
        
        var sessionIdString = authHeader.ToString().Replace("Bearer ", "");
        if (!Guid.TryParse(sessionIdString, out var sessionId))
        {
            _logger.LogWarning("Invalid session ID format: {SessionId}", sessionIdString);
            return AuthenticateResult.Fail("Invalid session ID format");
        }
        
        // Check cache first (Redis)
        var cacheKey = $"session:{sessionId}";
        var cachedSessionJson = await _cache.GetStringAsync(cacheKey, Context.RequestAborted);
        
        UserSession? session;
        
        if (cachedSessionJson != null)
        {
            // Cache hit
            session = JsonSerializer.Deserialize<UserSession>(cachedSessionJson);
            _logger.LogDebug("Session {SessionId} loaded from cache", sessionId);
        }
        else
        {
            // Cache miss - load from database
            session = await _sessionRepository.GetByIdAsync(sessionId, Context.RequestAborted);
            
            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found in cache or database", sessionId);
                return AuthenticateResult.Fail("Session not found");
            }
            
            // Warm cache with sliding expiration
            var cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };
            
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(session),
                cacheOptions,
                Context.RequestAborted);
            
            _logger.LogInformation("Session {SessionId} loaded from database and cached", sessionId);
        }
        
        // Validate session expiry
        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Session {SessionId} expired at {ExpiresAt}", sessionId, session.ExpiresAt);
            await InvalidateSessionAsync(sessionId);
            return AuthenticateResult.Fail("Session expired");
        }
        
        // Build claims principal
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new Claim("oid", session.UserId.ToString()), // Entra ID object ID
            new Claim(ClaimTypes.Email, session.Email),
            new Claim("tenant_id", session.TenantId.ToString()),
            new Claim("session_id", sessionId.ToString())
        };
        
        // Add roles
        foreach (var role in session.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("roles", role)); // Entra ID role claim format
        }
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }
    
    private async Task InvalidateSessionAsync(Guid sessionId)
    {
        // Remove from cache
        await _cache.RemoveAsync($"session:{sessionId}");
        
        // Mark as expired in database
        await _sessionRepository.ExpireSessionAsync(sessionId, Context.RequestAborted);
    }
}

public sealed class SessionAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "SessionAuthentication";
}

/// <summary>
/// Session model loaded from Redis/DB
/// </summary>
public sealed record UserSession
{
    public required Guid SessionId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid TenantId { get; init; }
    public required string Email { get; init; }
    public required List<string> Roles { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required DateTime LastAccessedAt { get; init; }
}
```

### Registering SessionAuthenticationHandler

```csharp
// Location: Src/Foundation/services/Identity/Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register session authentication
builder.Services.AddAuthentication(SessionAuthenticationOptions.DefaultScheme)
    .AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>(
        SessionAuthenticationOptions.DefaultScheme,
        options => { });

// Register dependencies
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "northstar:";
});
```

---

## Token Exchange Service (BFF Pattern)

The **Token Exchange Service** converts Entra ID tokens into internal session IDs for security and decoupling.

### ITokenExchangeService Interface

```csharp
// Location: Src/Foundation/services/Identity/Application/Services/ITokenExchangeService.cs
namespace NorthStarET.Foundation.Identity.Application.Services;

public interface ITokenExchangeService
{
    /// <summary>
    /// Exchanges an Entra ID JWT token for an internal session ID
    /// </summary>
    /// <param name="entraToken">JWT token from Microsoft Entra ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session ID for use with SessionAuthenticationHandler</returns>
    Task<Guid> ExchangeEntraTokenForSessionAsync(
        string entraToken,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Refreshes an existing session (extends expiry)
    /// </summary>
    Task<bool> RefreshSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Invalidates a session (logout)
    /// </summary>
    Task InvalidateSessionAsync(Guid sessionId, CancellationToken cancellationToken);
}
```

### TokenExchangeService Implementation

```csharp
// Location: Src/Foundation/services/Identity/Infrastructure/Services/TokenExchangeService.cs
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace NorthStarET.Foundation.Identity.Infrastructure.Services;

public sealed class TokenExchangeService : ITokenExchangeService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TokenExchangeService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    
    public TokenExchangeService(
        ISessionRepository sessionRepository,
        IUserRepository userRepository,
        IDistributedCache cache,
        ILogger<TokenExchangeService> logger)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _cache = cache;
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();
    }
    
    public async Task<Guid> ExchangeEntraTokenForSessionAsync(
        string entraToken,
        CancellationToken cancellationToken)
    {
        // Parse JWT without validation (already validated by Microsoft.Identity.Web)
        var jwt = _tokenHandler.ReadJwtToken(entraToken);
        
        // Extract claims
        var oidClaim = jwt.Claims.FirstOrDefault(c => c.Type == "oid")
            ?? throw new InvalidOperationException("oid claim not found in token");
        var userId = Guid.Parse(oidClaim.Value);
        
        var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")
            ?? throw new InvalidOperationException("preferred_username claim not found");
        var email = emailClaim.Value;
        
        // Get or create user record
        var user = await _userRepository.GetByEntraIdAsync(userId, cancellationToken)
            ?? await _userRepository.CreateFromEntraIdAsync(userId, email, cancellationToken);
        
        // Get tenant context (assume from existing claim or user profile)
        var tenantId = user.PrimaryTenantId;
        
        // Load roles for this tenant
        var roles = await _userRepository.GetRolesForTenantAsync(
            userId,
            tenantId,
            cancellationToken);
        
        // Create session
        var sessionId = Guid.NewGuid();
        var session = new UserSession
        {
            SessionId = sessionId,
            UserId = userId,
            TenantId = tenantId,
            Email = email,
            Roles = roles.ToList(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(8), // 8-hour session
            LastAccessedAt = DateTime.UtcNow
        };
        
        // Persist session to database
        await _sessionRepository.CreateAsync(session, cancellationToken);
        
        // Cache session in Redis with sliding expiration
        var cacheKey = $"session:{sessionId}";
        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
        
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(session),
            cacheOptions,
            cancellationToken);
        
        _logger.LogInformation(
            "Created session {SessionId} for user {UserId} in tenant {TenantId}",
            sessionId, userId, tenantId);
        
        return sessionId;
    }
    
    public async Task<bool> RefreshSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null || session.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }
        
        // Extend expiration
        session = session with 
        { 
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            LastAccessedAt = DateTime.UtcNow
        };
        
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        
        // Update cache
        var cacheKey = $"session:{sessionId}";
        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
        
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(session),
            cacheOptions,
            cancellationToken);
        
        return true;
    }
    
    public async Task InvalidateSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync($"session:{sessionId}", cancellationToken);
        await _sessionRepository.DeleteAsync(sessionId, cancellationToken);
        
        _logger.LogInformation("Invalidated session {SessionId}", sessionId);
    }
}
```

---

## Role-Based Authorization (RBAC)

### Authorization Policies

```csharp
// Location: Src/Foundation/services/Student/Api/Program.cs
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization(options =>
{
    // Require authenticated user for all endpoints by default
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    
    // Role-based policies
    options.AddPolicy("TeacherOnly", policy => 
        policy.RequireRole("Teacher", "Administrator"));
    
    options.AddPolicy("AdministratorOnly", policy => 
        policy.RequireRole("Administrator"));
    
    options.AddPolicy("ViewStudents", policy =>
        policy.RequireClaim("permissions", "students:read"));
    
    options.AddPolicy("ManageStudents", policy =>
        policy.RequireClaim("permissions", "students:write", "students:delete"));
});
```

### Custom Authorization Handler (Tenant-Scoped)

```csharp
// Location: Src/Foundation/shared/Infrastructure/Authorization/TenantAuthorizationHandler.cs
using Microsoft.AspNetCore.Authorization;

namespace NorthStarET.Foundation.Infrastructure.Authorization;

/// <summary>
/// Ensures user has permission within their current tenant context
/// </summary>
public sealed class TenantScopedPermissionRequirement : IAuthorizationRequirement
{
    public required string Permission { get; init; }
}

public sealed class TenantScopedPermissionHandler 
    : AuthorizationHandler<TenantScopedPermissionRequirement>
{
    private readonly ITenantContext _tenantContext;
    private readonly IPermissionRepository _permissionRepository;
    
    public TenantScopedPermissionHandler(
        ITenantContext tenantContext,
        IPermissionRepository permissionRepository)
    {
        _tenantContext = tenantContext;
        _permissionRepository = permissionRepository;
    }
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantScopedPermissionRequirement requirement)
    {
        var userId = context.User.GetUserId();
        var tenantId = _tenantContext.TenantId;
        
        // Check if user has permission in current tenant
        var hasPermission = await _permissionRepository.UserHasPermissionAsync(
            userId,
            tenantId,
            requirement.Permission,
            CancellationToken.None);
        
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                $"User {userId} lacks permission '{requirement.Permission}' in tenant {tenantId}"));
        }
    }
}
```

### Controller Authorization

```csharp
// Location: Src/Foundation/services/Student/Api/Controllers/StudentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NorthStarET.Foundation.Student.Api.Controllers;

[ApiController]
[Route("api/students")]
[Authorize] // Require authentication for all endpoints
public sealed class StudentsController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ViewStudents")] // Require specific permission
    public async Task<IActionResult> GetStudentsAsync(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentsQuery();
        var result = await mediator.Send(query, cancellationToken);
        
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);
    }
    
    [HttpPost]
    [Authorize(Roles = "Teacher,Administrator")] // Role-based
    public async Task<IActionResult> CreateStudentAsync(
        [FromBody] CreateStudentRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateStudentCommand(request.FirstName, request.LastName);
        var result = await mediator.Send(command, cancellationToken);
        
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetStudentByIdAsync), new { id = result.Value }, result.Value)
            : Problem(result.Error);
    }
}
```

---

## Multi-Tenant Data Isolation Security

### Tenant Context Middleware

```csharp
// Location: Src/Foundation/shared/Infrastructure/MultiTenancy/TenantMiddleware.cs
using Microsoft.AspNetCore.Http;

namespace NorthStarET.Foundation.Infrastructure.MultiTenancy;

/// <summary>
/// Extracts tenant ID from JWT claims and sets ITenantContext
/// </summary>
public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;
    
    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value
                    ?? throw new InvalidOperationException("tenant_id claim not found");
                
                var tenantId = Guid.Parse(tenantIdClaim);
                tenantContext.SetTenant(tenantId);
                
                _logger.LogDebug("Tenant context set to {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract tenant ID from claims");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid tenant context");
                return;
            }
        }
        
        await _next(context);
    }
}

public interface ITenantContext
{
    Guid TenantId { get; }
    void SetTenant(Guid tenantId);
}

public sealed class TenantContext : ITenantContext
{
    private Guid? _tenantId;
    
    public Guid TenantId => _tenantId 
        ?? throw new InvalidOperationException("Tenant context not set");
    
    public void SetTenant(Guid tenantId)
    {
        if (_tenantId.HasValue)
        {
            throw new InvalidOperationException("Tenant context already set");
        }
        
        _tenantId = tenantId;
    }
}
```

### Database Row-Level Security (RLS)

```sql
-- Location: Migrations SQL scripts
-- Enable RLS on all tenant-scoped tables

-- Example: Students table
CREATE TABLE student.students (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    -- ... other columns
    CONSTRAINT fk_students_tenant FOREIGN KEY (tenant_id) 
        REFERENCES configuration.tenants(id)
);

-- Create index for tenant filtering
CREATE INDEX idx_students_tenant_id ON student.students(tenant_id);

-- Enable RLS
ALTER TABLE student.students ENABLE ROW LEVEL SECURITY;

-- Create policy: users can only see their own tenant's data
CREATE POLICY tenant_isolation_policy ON student.students
    USING (tenant_id = current_setting('app.current_tenant')::uuid);

-- Prevent tenant_id modification
CREATE POLICY prevent_tenant_change ON student.students
    FOR UPDATE
    USING (tenant_id = current_setting('app.current_tenant')::uuid)
    WITH CHECK (tenant_id = current_setting('app.current_tenant')::uuid);
```

### EF Core Tenant Interceptor

```csharp
// Location: Src/Foundation/shared/Infrastructure/Persistence/Interceptors/TenantInterceptor.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NorthStarET.Foundation.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Automatically sets tenant_id on entities and enforces session variable
/// </summary>
public sealed class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;
    
    public TenantInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }
    
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return result;
        }
        
        // Set session variable for RLS
        var tenantId = _tenantContext.TenantId;
        await eventData.Context.Database
            .ExecuteSqlRawAsync(
                $"SET LOCAL app.current_tenant = '{tenantId}'",
                cancellationToken);
        
        // Auto-set tenant_id on new entities
        foreach (var entry in eventData.Context.ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = tenantId;
            }
            else if (entry.State == EntityState.Modified)
            {
                // Prevent tenant_id changes
                entry.Property(e => e.TenantId).IsModified = false;
            }
        }
        
        return result;
    }
}

/// <summary>
/// Marker interface for tenant-scoped entities
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
```

---

## FERPA Compliance Patterns

### Audit Logging Entity

```csharp
// Location: Src/Foundation/shared/Domain/Audit/AuditRecord.cs
namespace NorthStarET.Foundation.Domain.Audit;

/// <summary>
/// FERPA-compliant audit record for all data access and modifications
/// </summary>
public sealed class AuditRecord
{
    public required Guid Id { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid UserId { get; init; }
    public required string Action { get; init; } // Create, Read, Update, Delete
    public required string EntityType { get; init; } // Student, Assessment, etc.
    public required Guid EntityId { get; init; }
    public required DateTime OccurredAt { get; init; }
    public required string IpAddress { get; init; }
    public required string UserAgent { get; init; }
    public string? ChangeDetails { get; init; } // JSON diff for updates
    public string? Justification { get; init; } // Required for sensitive operations
}
```

### Audit Interceptor

```csharp
// Location: Src/Foundation/shared/Infrastructure/Persistence/Interceptors/AuditInterceptor.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace NorthStarET.Foundation.Infrastructure.Persistence.Interceptors;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public AuditInterceptor(
        ITenantContext tenantContext,
        IHttpContextAccessor httpContextAccessor,
        IDateTimeProvider dateTimeProvider)
    {
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
        _dateTimeProvider = dateTimeProvider;
    }
    
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return result;
        }
        
        var auditRecords = new List<AuditRecord>();
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.GetUserId() ?? Guid.Empty;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        
        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditRecord) continue; // Don't audit audit records
            
            var entityType = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry.Entity);
            
            if (entityId == Guid.Empty) continue;
            
            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => null
            };
            
            if (action == null) continue;
            
            var changeDetails = entry.State == EntityState.Modified
                ? JsonSerializer.Serialize(GetChangedProperties(entry))
                : null;
            
            var auditRecord = new AuditRecord
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantContext.TenantId,
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OccurredAt = _dateTimeProvider.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ChangeDetails = changeDetails
            };
            
            auditRecords.Add(auditRecord);
        }
        
        if (auditRecords.Any())
        {
            eventData.Context.Set<AuditRecord>().AddRange(auditRecords);
            await eventData.Context.SaveChangesAsync(cancellationToken);
        }
        
        return result;
    }
    
    private static Guid GetEntityId(object entity)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        return idProperty?.GetValue(entity) as Guid? ?? Guid.Empty;
    }
    
    private static Dictionary<string, object?> GetChangedProperties(EntityEntry entry)
    {
        return entry.Properties
            .Where(p => p.IsModified)
            .ToDictionary(
                p => p.Metadata.Name,
                p => new { Original = p.OriginalValue, Current = p.CurrentValue }
            );
    }
}
```

### FERPA Access Justification

For sensitive student data access (e.g., viewing grades, discipline records):

```csharp
// Location: Application layer command/query
public sealed record ViewStudentGradesQuery : IRequest<Result<StudentGradesDto>>
{
    public required Guid StudentId { get; init; }
    
    /// <summary>
    /// FERPA requires justification for accessing student records
    /// </summary>
    public required string AccessJustification { get; init; }
}

public sealed class ViewStudentGradesQueryHandler 
    : IRequestHandler<ViewStudentGradesQuery, Result<StudentGradesDto>>
{
    private readonly IAuditRepository _auditRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITenantContext _tenantContext;
    
    public async Task<Result<StudentGradesDto>> Handle(
        ViewStudentGradesQuery request,
        CancellationToken cancellationToken)
    {
        // Log FERPA access with justification
        await _auditRepository.LogAccessAsync(new AuditRecord
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            UserId = /* from claims */,
            Action = "Read",
            EntityType = "StudentGrades",
            EntityId = request.StudentId,
            OccurredAt = DateTime.UtcNow,
            Justification = request.AccessJustification,
            // ... other required fields
        }, cancellationToken);
        
        // Proceed with query
        var grades = await _studentRepository.GetGradesAsync(
            request.StudentId,
            cancellationToken);
        
        return Result.Success(grades);
    }
}
```

---

## Secret Management

### Azure Key Vault Integration

```csharp
// Location: Program.cs (all services)
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    // Use Azure Key Vault for production secrets
    var keyVaultUrl = new Uri(builder.Configuration["KeyVault:Url"]!);
    builder.Configuration.AddAzureKeyVault(
        keyVaultUrl,
        new DefaultAzureCredential());
}
else
{
    // Use User Secrets for local development
    builder.Configuration.AddUserSecrets<Program>();
}
```

**Key Vault Secret Naming Convention**:
- `AzureAd--ClientSecret` (double dash replaces colons)
- `ConnectionStrings--PostgreSQL`
- `ConnectionStrings--Redis`
- `MessageBus--ConnectionString`

### User Secrets (Local Development)

```bash
# Initialize user secrets
dotnet user-secrets init --project Src/Foundation/services/Identity/Api

# Set secrets
dotnet user-secrets set "AzureAd:ClientSecret" "your-client-secret" \
    --project Src/Foundation/services/Identity/Api

dotnet user-secrets set "ConnectionStrings:PostgreSQL" \
    "Host=localhost;Database=northstar_identity;Username=dev;Password=dev123" \
    --project Src/Foundation/services/Identity/Api
```

### Secret Validation at Startup

```csharp
// Location: Program.cs
var builder = WebApplication.CreateBuilder(args);

// Validate required secrets exist
var requiredSecrets = new[]
{
    "AzureAd:ClientSecret",
    "ConnectionStrings:PostgreSQL",
    "ConnectionStrings:Redis"
};

foreach (var secret in requiredSecrets)
{
    if (string.IsNullOrEmpty(builder.Configuration[secret]))
    {
        throw new InvalidOperationException(
            $"Required secret '{secret}' is missing. " +
            "Ensure it's configured in Key Vault (prod) or User Secrets (dev).");
    }
}
```

---

## Audit Logging

### Serilog Configuration for Security Events

```csharp
// Location: Program.cs
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore.Authorization", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "NorthStarET.Identity")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
                           "{Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/security-.txt",
            rollingInterval: RollingInterval.Day,
            restrictedToMinimumLevel: LogEventLevel.Warning,
            retainedFileCountLimit: 30)
        .WriteTo.ApplicationInsights(
            context.Configuration["ApplicationInsights:ConnectionString"]!,
            TelemetryConverter.Traces);
});
```

### Security Event Logging Helper

```csharp
// Location: Src/Foundation/shared/Infrastructure/Logging/SecurityLogger.cs
using Microsoft.Extensions.Logging;

namespace NorthStarET.Foundation.Infrastructure.Logging;

public static class SecurityLogger
{
    public static void LogAuthenticationSuccess(
        this ILogger logger,
        Guid userId,
        string email,
        Guid tenantId)
    {
        logger.LogInformation(
            "Authentication successful for user {UserId} ({Email}) in tenant {TenantId}",
            userId, email, tenantId);
    }
    
    public static void LogAuthenticationFailure(
        this ILogger logger,
        string reason,
        string? email = null)
    {
        logger.LogWarning(
            "Authentication failed: {Reason}. Email: {Email}",
            reason, email ?? "Unknown");
    }
    
    public static void LogAuthorizationFailure(
        this ILogger logger,
        Guid userId,
        string resource,
        string permission)
    {
        logger.LogWarning(
            "Authorization denied for user {UserId} accessing {Resource} " +
            "with permission {Permission}",
            userId, resource, permission);
    }
    
    public static void LogCrossTenantAccessAttempt(
        this ILogger logger,
        Guid userId,
        Guid requestedTenantId,
        Guid actualTenantId)
    {
        logger.LogError(
            "SECURITY VIOLATION: User {UserId} attempted to access tenant {RequestedTenantId} " +
            "but belongs to tenant {ActualTenantId}",
            userId, requestedTenantId, actualTenantId);
    }
    
    public static void LogSessionCreated(
        this ILogger logger,
        Guid sessionId,
        Guid userId,
        Guid tenantId,
        DateTime expiresAt)
    {
        logger.LogInformation(
            "Session {SessionId} created for user {UserId} in tenant {TenantId}, " +
            "expires at {ExpiresAt}",
            sessionId, userId, tenantId, expiresAt);
    }
    
    public static void LogSessionInvalidated(
        this ILogger logger,
        Guid sessionId,
        string reason)
    {
        logger.LogInformation(
            "Session {SessionId} invalidated: {Reason}",
            sessionId, reason);
    }
}
```

---

## Security Testing Patterns

### Authentication Testing

```csharp
// Location: tests/integration/Identity.IntegrationTests/AuthenticationTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace NorthStarET.Foundation.Identity.IntegrationTests;

public sealed class AuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public AuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Unauthenticated_Request_Returns_401()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/students");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Invalid_Session_Returns_401()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidSessionId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", invalidSessionId.ToString());
        
        // Act
        var response = await client.GetAsync("/api/students");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Valid_Session_Returns_200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var sessionId = await CreateTestSessionAsync();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", sessionId.ToString());
        
        // Act
        var response = await client.GetAsync("/api/students");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    private async Task<Guid> CreateTestSessionAsync()
    {
        // Create test session via TokenExchangeService
        // Implementation depends on test infrastructure
        throw new NotImplementedException();
    }
}
```

### Authorization Testing

```csharp
// Location: tests/unit/Student.Application.Tests/Authorization/StudentAuthorizationTests.cs
using Microsoft.AspNetCore.Authorization;
using NorthStarET.Foundation.Infrastructure.Authorization;

namespace NorthStarET.Foundation.Student.Application.Tests.Authorization;

public sealed class StudentAuthorizationTests
{
    [Fact]
    public async Task Teacher_Can_View_Students_In_Their_Tenant()
    {
        // Arrange
        var handler = new TenantScopedPermissionHandler(
            CreateTenantContext(tenantId: TenantA),
            CreatePermissionRepository(userId: TeacherId, tenantId: TenantA, permission: "students:read"));
        
        var requirement = new TenantScopedPermissionRequirement 
        { 
            Permission = "students:read" 
        };
        
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            CreateClaimsPrincipal(TeacherId, TenantA),
            null);
        
        // Act
        await handler.HandleAsync(context);
        
        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
    
    [Fact]
    public async Task Teacher_Cannot_View_Students_In_Other_Tenant()
    {
        // Arrange
        var handler = new TenantScopedPermissionHandler(
            CreateTenantContext(tenantId: TenantB), // Teacher in TenantA
            CreatePermissionRepository(userId: TeacherId, tenantId: TenantA, permission: "students:read"));
        
        var requirement = new TenantScopedPermissionRequirement 
        { 
            Permission = "students:read" 
        };
        
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            CreateClaimsPrincipal(TeacherId, TenantA),
            null);
        
        // Act
        await handler.HandleAsync(context);
        
        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
```

### Tenant Isolation Testing

```csharp
// Location: tests/integration/Student.IntegrationTests/TenantIsolationTests.cs
using Microsoft.EntityFrameworkCore;

namespace NorthStarET.Foundation.Student.IntegrationTests;

public sealed class TenantIsolationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public TenantIsolationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task Query_Only_Returns_Students_For_Current_Tenant()
    {
        // Arrange
        var dbContext = _fixture.CreateDbContext();
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(TenantA);
        
        // Seed data: 5 students in TenantA, 5 in TenantB
        await SeedStudentsAsync(dbContext, TenantA, count: 5);
        await SeedStudentsAsync(dbContext, TenantB, count: 5);
        
        // Act
        await dbContext.Database.ExecuteSqlRawAsync(
            $"SET LOCAL app.current_tenant = '{TenantA}'");
        
        var students = await dbContext.Students.ToListAsync();
        
        // Assert
        students.Should().HaveCount(5);
        students.Should().OnlyContain(s => s.TenantId == TenantA);
    }
    
    [Fact]
    public async Task Cannot_Insert_Student_With_Different_TenantId()
    {
        // Arrange
        var dbContext = _fixture.CreateDbContext();
        await dbContext.Database.ExecuteSqlRawAsync(
            $"SET LOCAL app.current_tenant = '{TenantA}'");
        
        var student = new Student
        {
            Id = Guid.NewGuid(),
            TenantId = TenantB, // Wrong tenant!
            FirstName = "John",
            LastName = "Doe"
        };
        
        dbContext.Students.Add(student);
        
        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => 
            dbContext.SaveChangesAsync());
    }
}
```

### Penetration Testing Scenarios

Document for security team:

```markdown
## Penetration Testing Scenarios

### 1. JWT Token Manipulation
**Scenario**: Attacker modifies JWT claims to access other tenants
**Expected**: API rejects modified tokens (signature validation fails)
**Test**: Use jwt.io to modify tenant_id claim, replay to API

### 2. Session Hijacking
**Scenario**: Attacker steals session ID from another user
**Expected**: Session tied to IP/User-Agent, rejected if different
**Test**: Capture valid session ID, replay from different IP

### 3. SQL Injection via Tenant ID
**Scenario**: Attacker injects SQL in tenant_id parameter
**Expected**: Parameterized queries + RLS prevent injection
**Test**: Send tenant_id=' OR '1'='1 in request

### 4. Cross-Tenant Data Access
**Scenario**: User modifies API request to access other tenant's data
**Expected**: Application + database layers both reject
**Test**: Authenticated as TenantA, request TenantB student ID

### 5. Privilege Escalation
**Scenario**: Teacher attempts to access admin-only endpoints
**Expected**: Authorization policy rejects based on roles
**Test**: Teacher token sent to DELETE /api/districts/{id}

### 6. Session Replay After Logout
**Scenario**: Attacker replays session ID after user logs out
**Expected**: Session invalidated in Redis/DB, request rejected
**Test**: Capture session before logout, replay after logout
```

---

## Anti-Patterns

### ❌ Storing Secrets in Code

**Never**:
```csharp
// ❌ BAD: Hardcoded secret
var clientSecret = "abc123-secret-key";

// ❌ BAD: Secret in appsettings.json (committed to repo)
{
  "AzureAd": {
    "ClientSecret": "abc123-secret-key"
  }
}
```

**Instead**:
```csharp
// ✅ GOOD: Load from Key Vault/User Secrets
var clientSecret = builder.Configuration["AzureAd:ClientSecret"]
    ?? throw new InvalidOperationException("ClientSecret not configured");
```

### ❌ Bypassing Authentication

**Never**:
```csharp
// ❌ BAD: Anonymous endpoint without justification
[AllowAnonymous]
[HttpGet("students")]
public async Task<IActionResult> GetStudentsAsync() { }
```

**Instead**:
```csharp
// ✅ GOOD: Require authentication
[Authorize]
[HttpGet("students")]
public async Task<IActionResult> GetStudentsAsync() { }
```

### ❌ Trusting Client-Provided Tenant ID

**Never**:
```csharp
// ❌ BAD: Accepting tenant ID from request body
public async Task<IActionResult> GetStudentsAsync([FromBody] Guid tenantId)
{
    var students = await _repository.GetByTenantAsync(tenantId);
    return Ok(students);
}
```

**Instead**:
```csharp
// ✅ GOOD: Extract tenant from authenticated claims
public async Task<IActionResult> GetStudentsAsync(
    [FromServices] ITenantContext tenantContext)
{
    var tenantId = tenantContext.TenantId; // From JWT claims
    var students = await _repository.GetByTenantAsync(tenantId);
    return Ok(students);
}
```

### ❌ Logging Sensitive Data

**Never**:
```csharp
// ❌ BAD: Logging PII or credentials
_logger.LogInformation(
    "User {Email} logged in with password {Password}",
    email, password);

_logger.LogInformation("Student SSN: {SSN}", student.SSN);
```

**Instead**:
```csharp
// ✅ GOOD: Log identifiers only, no PII
_logger.LogInformation(
    "User {UserId} authenticated successfully",
    userId);

_logger.LogInformation(
    "Student record {StudentId} accessed",
    studentId);
```

### ❌ Disabling RLS in Production

**Never**:
```sql
-- ❌ BAD: Disabling security features
ALTER TABLE students DISABLE ROW LEVEL SECURITY;
```

**Instead**:
```sql
-- ✅ GOOD: Keep RLS enabled, test policies thoroughly
ALTER TABLE students ENABLE ROW LEVEL SECURITY;
```

---

## Performance Considerations

### Session Caching Strategy

**Problem**: Database query on every request kills performance.

**Solution**: Redis cache with sliding expiration.

```csharp
// Cache hierarchy:
// 1. Redis (fast, 30-min sliding window) ← Check first
// 2. PostgreSQL (fallback) ← Only if cache miss
// 3. Re-warm cache after DB read

// Performance targets:
// - Cache hit: <5ms
// - Cache miss: <50ms (DB read + cache write)
// - p95 latency for authenticated requests: <100ms
```

### Token Refresh Optimization

**Problem**: Refreshing tokens on every request causes Entra ID API throttling.

**Solution**: Refresh only when token expires within 5 minutes.

```csharp
// Check token expiry before refresh
var expiresAt = jwt.ValidTo;
var shouldRefresh = expiresAt - DateTime.UtcNow < TimeSpan.FromMinutes(5);

if (shouldRefresh)
{
    await tokenExchangeService.RefreshSessionAsync(sessionId, cancellationToken);
}
```

### Authorization Cache

**Problem**: Permission checks on every request slow down API.

**Solution**: Cache permissions per user/tenant in Redis.

```csharp
// Cache key: "permissions:{userId}:{tenantId}"
// Expiry: 15 minutes
// Invalidate on role assignment changes (via domain event)

var cacheKey = $"permissions:{userId}:{tenantId}";
var cachedPermissions = await _cache.GetStringAsync(cacheKey);

if (cachedPermissions != null)
{
    return JsonSerializer.Deserialize<List<string>>(cachedPermissions);
}

// Cache miss: query DB and cache result
var permissions = await _permissionRepository.GetUserPermissionsAsync(
    userId, tenantId, cancellationToken);

await _cache.SetStringAsync(
    cacheKey,
    JsonSerializer.Serialize(permissions),
    new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
    });
```

### RLS Performance Tuning

**Problem**: Tenant filtering on large tables causes slow queries.

**Solution**: Composite indexes with tenant_id as first column.

```sql
-- ✅ GOOD: Tenant-first composite index
CREATE INDEX idx_students_tenant_created 
    ON students(tenant_id, created_at DESC);

-- Query uses index efficiently:
SELECT * FROM students 
WHERE tenant_id = 'abc-123'  -- Uses index
ORDER BY created_at DESC     -- Index covers sort
LIMIT 10;

-- ❌ BAD: Index without tenant_id
CREATE INDEX idx_students_created ON students(created_at DESC);
-- Forces full table scan even with RLS policy!
```

**Performance Targets**:
- Session validation: <5ms (p95)
- Authorization check: <10ms (p95)
- Authenticated API request: <100ms (p95)
- Token exchange (cache miss): <50ms (p95)

---

## References

### Internal Documents
- [Constitution Principle 5: Security & Compliance](../../.specify/memory/constitution.md#5-security--compliance-safeguards)
- [Identity Migration Scenario](../../Foundation/scenarios/01-identity-migration-entra-id.md)
- [Multi-Tenant Database Architecture](../../Foundation/scenarios/02-multi-tenant-database-architecture.md)
- [Clean Architecture Pattern](../patterns/clean-architecture.md)
- [Multi-Tenancy Pattern](../patterns/multi-tenancy.md)

### External References
- [Microsoft Entra ID Documentation](https://learn.microsoft.com/en-us/entra/identity/)
- [Microsoft.Identity.Web Documentation](https://learn.microsoft.com/en-us/entra/msal/dotnet/microsoft-identity-web/)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [PostgreSQL Row-Level Security](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)
- [FERPA Compliance Guidelines](https://www2.ed.gov/policy/gen/guid/fpco/ferpa/index.html)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)

### Service Architecture
- [Identity Service Detailed Architecture](../architecture/services/identity-service-detailed.md)
- [API Gateway Configuration](./api-gateway-config.md)
- [Testing Strategy](./TESTING_STRATEGY.md)

---

**Version History**:
- 1.0.0 (2025-11-20): Initial security & compliance pattern document
