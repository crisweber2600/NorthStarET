namespace NorthStarET.NextGen.Lms.Contracts.Schools;

/// <summary>
/// School summary for list views.
/// </summary>
public sealed record SchoolListItemResponse
{
    /// <summary>
    /// School unique identifier.
    /// </summary>
    public Guid SchoolId { get; init; }

    /// <summary>
    /// School name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional school code.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// School status (Active, Inactive).
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Minimum grade level offered (e.g., "PreK", "Kindergarten", "Grade1").
    /// </summary>
    public string? GradeRangeMin { get; init; }

    /// <summary>
    /// Maximum grade level offered (e.g., "Grade5", "Grade8", "Grade12").
    /// </summary>
    public string? GradeRangeMax { get; init; }

    /// <summary>
    /// UTC timestamp when the school was last updated.
    /// </summary>
    public DateTime LastUpdated { get; init; }
}
