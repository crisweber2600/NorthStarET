# Foundation Layer: Shared Infrastructure

**Purpose**: Shared infrastructure utilities (database patterns, caching, messaging, Azure services) used across all Foundation services.

**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Overview

The `Infrastructure` project provides common infrastructure patterns and utilities that Foundation services use to interact with external systems:

- **Database Patterns**: Multi-tenancy interceptors, audit interceptors, soft-delete filters
- **Caching**: Redis-based distributed caching abstraction
- **Messaging**: MassTransit/Azure Service Bus abstractions
- **Azure Services**: Key Vault, Blob Storage, Application Insights clients
- **Current User Implementation**: JWT claims-based user context
- **Tenant Context Implementation**: HTTP/claims-based tenant resolution

**Consumed By**: All Foundation service infrastructure projects (Identity.Infrastructure, Student.Infrastructure, etc.)

---

## Key Components

### 1. Multi-Tenancy Support

**Tenant Interceptor** (EF Core SaveChangesInterceptor):

Automatically filters queries by `tenant_id` and sets `TenantId` on insert:

```csharp
public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        foreach (var entry in context.ChangeTracker.Entries<EntityBase<Guid>>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == default)
            {
                entry.Entity.TenantId = new TenantId(_tenantContext.TenantId);
            }
        }
        return base.SavingChanges(eventData, result);
    }
}
```

**Query Filter** (applied in `OnModelCreating`):

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply tenant filter to all entities inheriting EntityBase<T>
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(EntityBase<Guid>).IsAssignableFrom(entityType.ClrType))
        {
            var method = SetGlobalQueryMethod.MakeGenericMethod(entityType.ClrType);
            method.Invoke(this, new object[] { modelBuilder });
        }
    }
}

private static void SetGlobalQuery<T>(ModelBuilder builder) where T : EntityBase<Guid>
{
    builder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
}
```

### 2. Audit Interceptor

Automatically sets `CreatedAt`/`CreatedBy` and `UpdatedAt`/`UpdatedBy`:

```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var userId = _currentUser.UserId;
        var now = DateTime.UtcNow;
        
        foreach (var entry in eventData.Context.ChangeTracker.Entries<EntityBase<Guid>>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = new UserId(userId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = new UserId(userId);
            }
        }
        return base.SavingChanges(eventData, result);
    }
}
```

### 3. Soft Delete Support

Query filter to exclude soft-deleted records:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    builder.Entity<T>().HasQueryFilter(e => e.DeletedAt == null);
}
```

### 4. Cache Service (Redis)

Distributed caching abstraction:

```csharp
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    
    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(key, cancellationToken);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync(key, json, options, cancellationToken);
    }
    
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
```

### 5. Event Publisher (MassTransit)

Domain event publishing abstraction:

```csharp
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
    {
        await _publishEndpoint.Publish(@event, cancellationToken);
    }
}
```

### 6. Current User Service (JWT Claims)

Implementation that reads from HTTP context claims:

```csharp
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public Guid UserId => Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());
    public string Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    public IReadOnlyCollection<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? Array.Empty<string>();
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public bool IsInRole(string role) => Roles.Contains(role);
    public bool HasPermission(string permission) { /* Check claims or call authorization service */ }
}
```

### 7. Tenant Context (HTTP Headers + Claims)

Implementation that resolves tenant from multiple sources:

```csharp
public class TenantContextService : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid _tenantId;
    private string _tenantName;
    
    public Guid TenantId => _tenantId != Guid.Empty ? _tenantId : ResolveTenantId();
    public string TenantName => _tenantName ?? ResolveTenantName();
    public bool IsTenantSet => _tenantId != Guid.Empty;
    
    private Guid ResolveTenantId()
    {
        // Check HTTP header
        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue) == true
            && Guid.TryParse(headerValue, out var tenantId))
        {
            return tenantId;
        }
        
        // Check JWT claims
        var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
        return Guid.TryParse(claimValue, out var claimTenantId) ? claimTenantId : Guid.Empty;
    }
    
    public void SetTenant(Guid tenantId, string tenantName)
    {
        _tenantId = tenantId;
        _tenantName = tenantName;
    }
    
    public void ClearTenant()
    {
        _tenantId = Guid.Empty;
        _tenantName = null;
    }
}
```

### 8. Azure Key Vault Client

Wrapper for Azure Key Vault secrets:

```csharp
public interface ISecretService
{
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
}

public class KeyVaultSecretService : ISecretService
{
    private readonly SecretClient _secretClient;
    
    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        var secret = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
        return secret.Value.Value;
    }
}
```

---

## Project Structure

```
Infrastructure/
├── Infrastructure.csproj             # Project file
├── Persistence/
│   ├── Interceptors/
│   │   ├── TenantInterceptor.cs      # Multi-tenancy filtering
│   │   ├── AuditInterceptor.cs       # Audit fields
│   │   └── SoftDeleteInterceptor.cs  # Soft delete support
│   └── Extensions/
│       └── ModelBuilderExtensions.cs # Query filter extensions
├── Caching/
│   └── RedisCacheService.cs          # Redis caching implementation
├── Messaging/
│   └── MassTransitEventPublisher.cs  # Domain event publishing
├── Identity/
│   ├── CurrentUserService.cs         # JWT claims-based user context
│   └── TenantContextService.cs       # Tenant resolution
├── Azure/
│   ├── KeyVaultSecretService.cs      # Azure Key Vault
│   └── BlobStorageService.cs         # Azure Blob Storage
└── README.md                         # This file
```

---

## Usage in Services

Services register infrastructure implementations in `DependencyInjection.cs`:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext with interceptors
        services.AddDbContext<StudentDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("StudentDb"));
            options.AddInterceptors(
                sp.GetRequiredService<TenantInterceptor>(),
                sp.GetRequiredService<AuditInterceptor>());
        });
        
        // Register interceptors
        services.AddScoped<TenantInterceptor>();
        services.AddScoped<AuditInterceptor>();
        
        // Register shared infrastructure services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantContext, TenantContextService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        services.AddScoped<ISecretService, KeyVaultSecretService>();
        
        // Register repositories
        services.AddScoped<IStudentRepository, StudentRepository>();
        
        return services;
    }
}
```

---

## PostgreSQL Row-Level Security (RLS)

In addition to EF Core query filters, PostgreSQL RLS policies enforce tenant isolation at the database level:

```sql
-- Enable RLS on table
ALTER TABLE students ENABLE ROW LEVEL SECURITY;

-- Create policy to filter by tenant_id
CREATE POLICY tenant_isolation_policy ON students
    USING (tenant_id = current_setting('app.current_tenant')::uuid);

-- Set tenant context in connection (via EF Core interceptor)
SET app.current_tenant = 'tenant-guid-here';
```

EF Core connection interceptor sets `app.current_tenant` session variable from `ITenantContext`.

---

## Dependencies

- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL provider
- `Microsoft.Extensions.Caching.StackExchangeRedis` - Redis caching
- `MassTransit` - Messaging abstraction
- `MassTransit.Azure.ServiceBus.Core` - Azure Service Bus transport
- `Azure.Security.KeyVault.Secrets` - Key Vault SDK
- `Azure.Storage.Blobs` - Blob Storage SDK
- `Microsoft.AspNetCore.Http` - HTTP context access

---

## Security Considerations

### Multi-Tenancy Enforcement

- **Defense in Depth**: EF Core query filters + PostgreSQL RLS policies
- **Connection Pooling**: Safe with RLS (session variables per connection)
- **Testing**: Integration tests verify no cross-tenant data leakage

### Secrets Management

- All secrets in Azure Key Vault (connection strings, API keys, certificates)
- No secrets in code, configuration files, or logs
- Secrets cached in memory with short TTL (5 minutes)

### Audit Logging

- All entity changes tracked with `CreatedBy`/`UpdatedBy`
- Immutable audit trail (no updates to audit fields after creation)
- Soft deletes preserve historical data

---

## References

- [EF Core Interceptors](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [PostgreSQL Row-Level Security](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)
- [MassTransit](https://masstransit.io/)
- [Azure SDK for .NET](https://learn.microsoft.com/en-us/dotnet/azure/)
- [Constitution: Principle 4 - Event-Driven Data Discipline](../../../../.specify/memory/constitution.md)
- [Constitution: Principle 5 - Security & Compliance Safeguards](../../../../.specify/memory/constitution.md)
- [LAYERS.md: Shared Infrastructure](../../../../Plan/Foundation/LAYERS.md#infrastructure-utilities)

---

**Status**: To Be Implemented (Phase 1 - Weeks 1-2)  
**Related Spec**: [001-phase1-foundation-services](../../../../Plan/Foundation/specs/Foundation/001-phase1-foundation-services/)
