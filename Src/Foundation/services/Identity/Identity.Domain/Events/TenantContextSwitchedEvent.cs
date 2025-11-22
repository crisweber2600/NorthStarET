using NorthStarET.Foundation.Domain.Events;

namespace NorthStarET.Foundation.Identity.Domain.Events;

/// <summary>
/// Event raised when a user switches tenant context
/// </summary>
public record TenantContextSwitchedEvent(
    Guid UserId,
    Guid SessionId,
    Guid FromTenantId,
    Guid ToTenantId,
    DateTime SwitchedAt) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
