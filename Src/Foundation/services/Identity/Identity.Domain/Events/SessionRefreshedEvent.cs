using NorthStarET.Foundation.Domain.Events;

namespace NorthStarET.Foundation.Identity.Domain.Events;

/// <summary>
/// Event raised when a session is refreshed
/// </summary>
public record SessionRefreshedEvent(
    Guid SessionId,
    Guid UserId,
    Guid TenantId,
    DateTime RefreshedAt,
    DateTime NewExpiresAt) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
