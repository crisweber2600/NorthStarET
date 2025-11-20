namespace NorthStarET.NextGen.Lms.Contracts.Schools;

/// <summary>
/// Paginated list of schools response.
/// </summary>
public sealed record ListSchoolsResponse
{
    /// <summary>
    /// Schools scoped to the district.
    /// </summary>
    public IReadOnlyList<SchoolListItemResponse> Items { get; init; } = Array.Empty<SchoolListItemResponse>();

    /// <summary>
    /// Total count of schools in the district (before pagination).
    /// </summary>
    public int TotalCount { get; init; }
}
