namespace NorthStarET.Foundation.Infrastructure.Persistence;

/// <summary>
/// Scoped tenant context implementation
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly Func<Guid?> _tenantIdProvider;
    private bool _bypassFilter;
    private string? _bypassReason;

    public TenantContext(Func<Guid?> tenantIdProvider)
    {
        _tenantIdProvider = tenantIdProvider ?? throw new ArgumentNullException(nameof(tenantIdProvider));
    }

    public Guid? TenantId => _tenantIdProvider();

    public bool BypassTenantFilter => _bypassFilter;

    public string? BypassReason => _bypassReason;

    public IDisposable BypassFilter(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason for bypassing tenant filter must be provided", nameof(reason));
        }

        return new BypassScope(this, reason);
    }

    private class BypassScope : IDisposable
    {
        private readonly TenantContext _context;
        private readonly bool _previousValue;

        public BypassScope(TenantContext context, string reason)
        {
            _context = context;
            _previousValue = _context._bypassFilter;
            _context._bypassFilter = true;
            _context._bypassReason = reason;
        }

        public void Dispose()
        {
            _context._bypassFilter = _previousValue;
            _context._bypassReason = null;
        }
    }
}
