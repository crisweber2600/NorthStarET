using NorthStarET.Foundation.Domain.Events;

namespace NorthStarET.Foundation.Identity.Domain.Events;

/// <summary>
/// Event raised when a user successfully authenticates
/// </summary>
public record UserAuthenticatedEvent(
    Guid UserId,
    Guid TenantId,
    Guid SessionId,
    string UserPrincipalName,
    DateTime AuthenticatedAt,
    string[] Roles) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
