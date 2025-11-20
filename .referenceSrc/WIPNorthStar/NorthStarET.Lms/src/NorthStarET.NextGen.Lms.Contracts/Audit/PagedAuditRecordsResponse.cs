namespace NorthStarET.NextGen.Lms.Contracts.Audit;

/// <summary>
/// Paginated response for audit records query
/// </summary>
public sealed record PagedAuditRecordsResponse
{
    /// <summary>
    /// Collection of audit records for the current page
    /// </summary>
    public IReadOnlyList<AuditRecordResponse> Records { get; init; } = Array.Empty<AuditRecordResponse>();

    /// <summary>
    /// Total number of audit records matching the query
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Number of records per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Indicates whether there are more pages available
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Indicates whether there are previous pages available
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}
