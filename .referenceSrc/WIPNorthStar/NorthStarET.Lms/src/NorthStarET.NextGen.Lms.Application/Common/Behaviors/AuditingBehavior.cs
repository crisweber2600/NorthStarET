using MediatR;
using NorthStarET.NextGen.Lms.Domain.Auditing;

namespace NorthStarET.NextGen.Lms.Application.Common.Behaviors;

/// <summary>
/// Marker interface for commands that require automatic auditing.
/// </summary>
public interface IAuditableCommand
{
    Guid DistrictId { get; }
    string Action { get; }
    string EntityType { get; }
    Guid? EntityId { get; }
    string? BeforePayload { get; }
    string? AfterPayload { get; }
}

/// <summary>
/// Service interface for accessing current user context.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    ActorRole Role { get; }
    Guid? DistrictId { get; }
    Guid? CorrelationId { get; }
}

