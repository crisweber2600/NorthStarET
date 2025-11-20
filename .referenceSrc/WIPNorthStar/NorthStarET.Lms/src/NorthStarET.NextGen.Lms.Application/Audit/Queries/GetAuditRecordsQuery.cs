using MediatR;
using NorthStarET.NextGen.Lms.Contracts.Audit;

namespace NorthStarET.NextGen.Lms.Application.Audit.Queries;

/// <summary>
/// Query to retrieve audit records with optional filtering and pagination
/// </summary>
public sealed record GetAuditRecordsQuery : IRequest<PagedAuditRecordsResponse>
{
    /// <summary>
    /// Optional district ID filter (enforces tenant isolation)
    /// </summary>
    public Guid? DistrictId { get; init; }

    /// <summary>
    /// Optional actor ID filter
    /// </summary>
    public Guid? ActorId { get; init; }

    /// <summary>
    /// Optional action filter (e.g., "Created", "Updated", "Deleted")
    /// </summary>
    public string? Action { get; init; }

    /// <summary>
    /// Optional entity type filter (e.g., "District", "DistrictAdmin")
    /// </summary>
    public string? EntityType { get; init; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Page size (default 20, max 100)
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Optional maximum count limit (overrides pagination)
    /// </summary>
    public int? Count { get; init; }
}
