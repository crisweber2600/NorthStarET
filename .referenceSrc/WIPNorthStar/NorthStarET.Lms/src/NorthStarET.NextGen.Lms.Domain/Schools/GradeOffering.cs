namespace NorthStarET.NextGen.Lms.Domain.Schools;

/// <summary>
/// Represents a grade level offered by a specific school.
/// Tracks the association between schools and grades with audit metadata.
/// This is a value object owned by the School aggregate.
/// </summary>
public sealed class GradeOffering
{
    private GradeOffering(
        Guid id,
        Guid schoolId,
        GradeLevel gradeLevel,
        SchoolType schoolType,
        DateTime effectiveFrom,
        DateTime? effectiveTo,
        Guid createdBy,
        DateTime createdAtUtc)
    {
        Id = id;
        SchoolId = schoolId;
        GradeLevel = gradeLevel;
        SchoolType = schoolType;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Unique identifier for this grade offering.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Foreign key to the school offering this grade.
    /// </summary>
    public Guid SchoolId { get; private set; }

    /// <summary>
    /// The grade level being offered (e.g., PreK, K, Grade1, ..., Grade12).
    /// </summary>
    public GradeLevel GradeLevel { get; private set; }

    /// <summary>
    /// The school type classification for this offering.
    /// Typically derived from the grade level but can be overridden.
    /// </summary>
    public SchoolType SchoolType { get; private set; }

    /// <summary>
    /// UTC timestamp when this grade offering becomes effective.
    /// Defaults to the time of creation.
    /// </summary>
    public DateTime EffectiveFrom { get; private set; }

    /// <summary>
    /// UTC timestamp when this grade offering ends/expires (nullable).
    /// Used for historical tracking when a grade is no longer offered.
    /// </summary>
    public DateTime? EffectiveTo { get; private set; }

    /// <summary>
    /// Concurrency stamp for optimistic concurrency control.
    /// </summary>
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the user who created this grade offering.
    /// </summary>
    public Guid CreatedBy { get; private set; }

    /// <summary>
    /// Identifier of the user who last updated this grade offering.
    /// </summary>
    public Guid? UpdatedBy { get; private set; }

    /// <summary>
    /// UTC timestamp when the grade offering was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the grade offering was last updated (nullable).
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Indicates whether this grade offering is currently active.
    /// Active offerings have no EffectiveTo date or EffectiveTo is in the future.
    /// </summary>
    public bool IsActive => !EffectiveTo.HasValue || EffectiveTo.Value > DateTime.UtcNow;

    /// <summary>
    /// Factory method to create a new grade offering.
    /// </summary>
    /// <param name="schoolId">School offering the grade</param>
    /// <param name="gradeLevel">Grade level being offered</param>
    /// <param name="schoolType">Optional school type (defaults to typical type for grade)</param>
    /// <param name="createdBy">User creating the offering</param>
    /// <returns>New GradeOffering instance</returns>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static GradeOffering Create(
        Guid schoolId,
        GradeLevel gradeLevel,
        Guid createdBy,
        SchoolType? schoolType = null)
    {
        if (schoolId == Guid.Empty)
            throw new ArgumentException("School ID cannot be empty", nameof(schoolId));

        if (!GradeTaxonomy.IsValidGrade(gradeLevel))
            throw new ArgumentException($"Invalid grade level: {gradeLevel}", nameof(gradeLevel));

        if (createdBy == Guid.Empty)
            throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

        var effectiveSchoolType = schoolType ?? GradeTaxonomy.GetTypicalSchoolType(gradeLevel);

        return new GradeOffering(
            id: Guid.NewGuid(),
            schoolId: schoolId,
            gradeLevel: gradeLevel,
            schoolType: effectiveSchoolType,
            effectiveFrom: DateTime.UtcNow,
            effectiveTo: null,
            createdBy: createdBy,
            createdAtUtc: DateTime.UtcNow);
    }

    /// <summary>
    /// Retires this grade offering by setting the EffectiveTo timestamp.
    /// Used when a school stops offering a particular grade.
    /// </summary>
    /// <param name="retiredBy">User retiring the offering</param>
    /// <param name="effectiveTo">Optional end date (defaults to now)</param>
    /// <exception cref="InvalidOperationException">Thrown if already retired</exception>
    public void Retire(Guid retiredBy, DateTime? effectiveTo = null)
    {
        if (EffectiveTo.HasValue)
            throw new InvalidOperationException("Grade offering is already retired");

        if (retiredBy == Guid.Empty)
            throw new ArgumentException("RetiredBy cannot be empty", nameof(retiredBy));

        EffectiveTo = effectiveTo ?? DateTime.UtcNow;
        UpdatedBy = retiredBy;
        UpdatedAtUtc = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Reactivates a retired grade offering by clearing the EffectiveTo timestamp.
    /// </summary>
    /// <param name="reactivatedBy">User reactivating the offering</param>
    /// <exception cref="InvalidOperationException">Thrown if not retired</exception>
    public void Reactivate(Guid reactivatedBy)
    {
        if (!EffectiveTo.HasValue)
            throw new InvalidOperationException("Grade offering is not retired");

        if (reactivatedBy == Guid.Empty)
            throw new ArgumentException("ReactivatedBy cannot be empty", nameof(reactivatedBy));

        EffectiveTo = null;
        UpdatedBy = reactivatedBy;
        UpdatedAtUtc = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }
}
