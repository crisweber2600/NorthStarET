namespace NorthStarET.Foundation.Infrastructure.Persistence;

/// <summary>
/// Context for tenant information in the current scope
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant ID
    /// </summary>
    Guid? TenantId { get; }
    
    /// <summary>
    /// Indicates if tenant filtering should be bypassed for the current operation
    /// </summary>
    bool BypassTenantFilter { get; }
    
    /// <summary>
    /// Temporarily bypasses tenant filtering for the duration of the returned disposable
    /// </summary>
    IDisposable BypassFilter(string reason);
}
