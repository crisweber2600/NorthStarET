using NorthStarET.Foundation.Domain.Entities;

namespace NorthStarET.Foundation.Infrastructure.Persistence.Entities;

/// <summary>
/// Audit log entity for tracking cross-tenant access and other security events
/// </summary>
public class AuditLog : EntityBase
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public required string Action { get; set; }
    public required string EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Changes { get; set; } // JSON serialized changes
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Context { get; set; } // Additional context (e.g., IP address, user agent)
}
