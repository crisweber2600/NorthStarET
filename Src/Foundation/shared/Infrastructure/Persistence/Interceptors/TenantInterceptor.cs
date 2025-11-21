using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NorthStarET.Foundation.Domain.Entities;
using NorthStarET.Foundation.Infrastructure.Persistence.Entities;

namespace NorthStarET.Foundation.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor to enforce tenant isolation.
/// Automatically filters queries by TenantId and validates TenantId on save.
/// Use ITenantContext.BypassFilter() to temporarily bypass filtering with audit logging.
/// </summary>
public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;

    public TenantInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
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
        var tenantId = _tenantContext.TenantId;
        var bypassFilter = _tenantContext.BypassTenantFilter;
        
        // Create audit log if bypass is active
        if (bypassFilter)
        {
            var tenantContextInstance = _tenantContext as TenantContext;
            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                Action = "BypassTenantFilter",
                EntityType = context.GetType().Name,
                Context = $"Reason: {tenantContextInstance?.BypassReason ?? "Unknown"}, User: {Environment.UserName}",
                Timestamp = DateTime.UtcNow
            };
            
            context.Set<AuditLog>().Add(auditLog);
        }
        else if (tenantId is null)
        {
            throw new InvalidOperationException(
                "TenantId is required for this operation. Ensure the tenant context is set.");
        }

        // Set TenantId on entities being added or modified
        var entries = context.ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added && !bypassFilter)
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
            else if (entry.State == EntityState.Modified && !bypassFilter)
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
