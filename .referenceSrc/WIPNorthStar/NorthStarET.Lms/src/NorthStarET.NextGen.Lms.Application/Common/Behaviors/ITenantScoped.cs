using MediatR;

namespace NorthStarET.NextGen.Lms.Application.Common.Behaviors;

/// <summary>
/// Marker interface for commands/queries that require tenant isolation enforcement.
/// Pipeline behavior will validate that user's DistrictId matches the requested tenant.
/// </summary>
public interface ITenantScoped
{
    /// <summary>
    /// District ID for tenant scoping (must match user's district).
    /// </summary>
    Guid DistrictId { get; }
}
