namespace NorthStarET.NextGen.Lms.Application.Common.Behaviors;

/// <summary>
/// Marker interface for commands that require idempotency protection.
/// Provides metadata consumed by the IdempotencyBehavior to build Redis keys.
/// </summary>
public interface IIdempotentCommand
{
    /// <summary>
    /// Descriptive operation name (e.g., "Districts.Create").
    /// </summary>
    string Operation { get; }

    /// <summary>
    /// Entity identifier that scopes the idempotency window.
    /// Should remain stable across retries for the same logical entity.
    /// </summary>
    Guid EntityId { get; }
}
