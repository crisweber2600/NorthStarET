namespace NorthStarET.NextGen.Lms.Contracts.Schools;

/// <summary>
/// Grade selection for school assignment.
/// </summary>
public sealed record GradeSelectionDto
{
    /// <summary>
    /// Grade identifier (e.g., "PreK", "Kindergarten", "Grade1").
    /// </summary>
    public string GradeId { get; init; } = string.Empty;

    /// <summary>
    /// School type category for the grade.
    /// </summary>
    public string SchoolType { get; init; } = string.Empty;

    /// <summary>
    /// Whether this grade is selected/offered by the school.
    /// </summary>
    public bool Selected { get; init; }
}

/// <summary>
/// Detailed school response including grade assignments and audit trail.
/// </summary>
public sealed record SchoolDetailResponse
{
    /// <summary>
    /// School unique identifier.
    /// </summary>
    public Guid SchoolId { get; init; }

    /// <summary>
    /// District unique identifier.
    /// </summary>
    public Guid DistrictId { get; init; }

    /// <summary>
    /// School name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional school code.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Optional notes about the school.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// School status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Grade offerings/selections for the school.
    /// </summary>
    public IReadOnlyList<GradeSelectionDto> GradeSelections { get; init; } = Array.Empty<GradeSelectionDto>();

    /// <summary>
    /// Concurrency token for optimistic concurrency control.
    /// </summary>
    public string ConcurrencyStamp { get; init; } = string.Empty;

    /// <summary>
    /// Audit information.
    /// </summary>
    public SchoolAuditDto Audit { get; init; } = new();
}

/// <summary>
/// Audit metadata for schools.
/// </summary>
public sealed record SchoolAuditDto
{
    /// <summary>
    /// User ID who created the school.
    /// </summary>
    public Guid CreatedBy { get; init; }

    /// <summary>
    /// UTC timestamp when the school was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// User ID who last updated the school.
    /// </summary>
    public Guid? UpdatedBy { get; init; }

    /// <summary>
    /// UTC timestamp when the school was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
