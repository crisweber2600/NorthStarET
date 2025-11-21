using NorthStarET.Foundation.Domain.Events;

namespace NorthStarET.Foundation.Identity.Domain.Events;

/// <summary>
/// Event raised when a user logs out
/// </summary>
public record UserLoggedOutEvent(
    Guid UserId,
    Guid TenantId,
    Guid SessionId,
    DateTime LoggedOutAt) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
