using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NorthStarET.Foundation.Domain.Entities;
using NorthStarET.Foundation.Infrastructure.Attributes;
using NorthStarET.Foundation.Infrastructure.Persistence.Entities;

namespace NorthStarET.Foundation.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor to enforce tenant isolation.
/// Automatically filters queries by TenantId and validates TenantId on save.
/// </summary>
public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly Func<Guid?> _tenantIdProvider;

    public TenantInterceptor(Func<Guid?> tenantIdProvider)
    {
        _tenantIdProvider = tenantIdProvider ?? throw new ArgumentNullException(nameof(tenantIdProvider));
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            EnforceTenantId(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            EnforceTenantId(eventData.Context);
        }
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EnforceTenantId(DbContext context)
    {
        var tenantId = _tenantIdProvider();
        
        // Check if the calling method has IgnoreTenantFilterAttribute
        var stackTrace = new StackTrace();
        var hasIgnoreAttribute = false;
        
        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            var method = stackTrace.GetFrame(i)?.GetMethod();
            if (method?.GetCustomAttribute<IgnoreTenantFilterAttribute>() is not null)
            {
                hasIgnoreAttribute = true;
                
                // Create audit log entry
                var auditLog = new AuditLog
                {
                    TenantId = tenantId,
                    Action = "BypassTenantFilter",
                    EntityType = method.DeclaringType?.Name ?? "Unknown",
                    Context = $"Method: {method.Name}, Caller: {Environment.UserName}",
                    Timestamp = DateTime.UtcNow
                };
                
                context.Set<AuditLog>().Add(auditLog);
                break;
            }
        }

        if (!hasIgnoreAttribute && tenantId is null)
        {
            throw new InvalidOperationException(
                "TenantId is required for this operation. Ensure the tenant context is set.");
        }

        // Set TenantId on entities being added or modified
        var entries = context.ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added && !hasIgnoreAttribute)
            {
                if (tenantId.HasValue)
                {
                    entry.Entity.TenantId = tenantId.Value;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot add entity {entry.Entity.GetType().Name} without a valid TenantId.");
                }
            }
            else if (entry.State == EntityState.Modified && !hasIgnoreAttribute)
            {
                // Prevent changing TenantId
                var originalTenantId = entry.OriginalValues.GetValue<Guid>(nameof(ITenantEntity.TenantId));
                if (entry.Entity.TenantId != originalTenantId)
                {
                    throw new InvalidOperationException(
                        $"Cannot change TenantId of entity {entry.Entity.GetType().Name}.");
                }
            }
        }
    }
}
