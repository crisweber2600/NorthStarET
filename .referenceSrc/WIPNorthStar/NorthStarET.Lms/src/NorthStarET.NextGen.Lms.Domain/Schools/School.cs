using NorthStarET.NextGen.Lms.Domain.Common;

namespace NorthStarET.NextGen.Lms.Domain.Schools;

/// <summary>
/// School aggregate root representing an educational institution within a district tenant.
/// Implements soft-delete pattern with grade offering archival and emits domain events for lifecycle changes.
/// </summary>
public sealed class School : AggregateRoot
{
    private readonly List<GradeOffering> _gradeOfferings = new();

    private School(
        Guid id,
        Guid districtId,
        string name,
        string? code,
        string? notes,
        SchoolStatus status,
        Guid createdBy,
        DateTime createdAtUtc)
        : base(id)
    {
        DistrictId = districtId;
        Name = name;
        Code = code;
        Notes = notes;
        Status = status;
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Foreign key to the owning District (tenant boundary).
    /// </summary>
    public Guid DistrictId { get; private set; }

    /// <summary>
    /// School display name (required, unique per district, max 200 characters).
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Optional school code (unique per district when provided, max 50 characters).
    /// </summary>
    public string? Code { get; private set; }

    /// <summary>
    /// Optional administrative notes (max 1000 characters).
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Operational status of the school (Active or Inactive).
    /// Default is Active.
    /// </summary>
    public SchoolStatus Status { get; private set; }

    /// <summary>
    /// Concurrency stamp for optimistic concurrency control.
    /// </summary>
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the user who created this school.
    /// </summary>
    public Guid CreatedBy { get; private set; }

    /// <summary>
    /// Identifier of the user who last updated this school.
    /// </summary>
    public Guid? UpdatedBy { get; private set; }

    /// <summary>
    /// UTC timestamp when the school was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the school was last updated (nullable).
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the school was soft-deleted (nullable).
    /// When set, triggers archival of all grade offerings.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Indicates whether the school is in a soft-deleted state.
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;

    /// <summary>
    /// Factory method to create a new School.
    /// </summary>
    /// <param name="districtId">Owning district ID (tenant)</param>
    /// <param name="name">School name (required, max 200 chars)</param>
    /// <param name="code">Optional school code (max 50 chars)</param>
    /// <param name="notes">Optional notes (max 1000 chars)</param>
    /// <param name="createdBy">User creating the school</param>
    /// <returns>New School instance</returns>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static School Create(
        Guid districtId,
        string name,
        string? code,
        string? notes,
        Guid createdBy)
    {
        if (districtId == Guid.Empty)
            throw new ArgumentException("District ID cannot be empty", nameof(districtId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("School name is required", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("School name cannot exceed 200 characters", nameof(name));

        if (code?.Length > 50)
            throw new ArgumentException("School code cannot exceed 50 characters", nameof(code));

        if (notes?.Length > 1000)
            throw new ArgumentException("School notes cannot exceed 1000 characters", nameof(notes));

        if (createdBy == Guid.Empty)
            throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

        var school = new School(
            id: Guid.NewGuid(),
            districtId: districtId,
            name: name.Trim(),
            code: code?.Trim(),
            notes: notes?.Trim(),
            status: SchoolStatus.Active,
            createdBy: createdBy,
            createdAtUtc: DateTime.UtcNow);

        school.RaiseDomainEvent(new SchoolCreatedEvent(school.Id, school.DistrictId, school.Name, createdBy));

        return school;
    }

    /// <summary>
    /// Updates the school's core properties.
    /// </summary>
    /// <param name="name">Updated name</param>
    /// <param name="code">Updated code</param>
    /// <param name="notes">Updated notes</param>
    /// <param name="status">Updated status</param>
    /// <param name="updatedBy">User performing the update</param>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown if school is deleted</exception>
    public void Update(
        string name,
        string? code,
        string? notes,
        SchoolStatus status,
        Guid updatedBy)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update a deleted school");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("School name is required", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("School name cannot exceed 200 characters", nameof(name));

        if (code?.Length > 50)
            throw new ArgumentException("School code cannot exceed 50 characters", nameof(code));

        if (notes?.Length > 1000)
            throw new ArgumentException("School notes cannot exceed 1000 characters", nameof(notes));

        if (updatedBy == Guid.Empty)
            throw new ArgumentException("UpdatedBy cannot be empty", nameof(updatedBy));

        Name = name.Trim();
        Code = code?.Trim();
        Notes = notes?.Trim();
        Status = status;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();

        RaiseDomainEvent(new SchoolUpdatedEvent(Id, DistrictId, Name, updatedBy));
    }

    /// <summary>
    /// Soft-deletes the school and raises a domain event.
    /// Grade offerings will be archived by event handlers.
    /// </summary>
    /// <param name="deletedBy">User performing the deletion</param>
    /// <exception cref="InvalidOperationException">Thrown if school is already deleted</exception>
    public void Delete(Guid deletedBy)
    {
        if (IsDeleted)
            throw new InvalidOperationException("School is already deleted");

        if (deletedBy == Guid.Empty)
            throw new ArgumentException("DeletedBy cannot be empty", nameof(deletedBy));

        DeletedAt = DateTime.UtcNow;
        UpdatedBy = deletedBy;
        UpdatedAtUtc = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();

        RaiseDomainEvent(new SchoolDeletedEvent(Id, DistrictId, deletedBy));
    }

    /// <summary>
    /// Restores a soft-deleted school.
    /// </summary>
    /// <param name="restoredBy">User performing the restoration</param>
    /// <exception cref="InvalidOperationException">Thrown if school is not deleted</exception>
    public void Restore(Guid restoredBy)
    {
        if (!IsDeleted)
            throw new InvalidOperationException("School is not deleted");

        if (restoredBy == Guid.Empty)
            throw new ArgumentException("RestoredBy cannot be empty", nameof(restoredBy));

        DeletedAt = null;
        UpdatedBy = restoredBy;
        UpdatedAtUtc = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();

        RaiseDomainEvent(new SchoolRestoredEvent(Id, DistrictId, restoredBy));
    }

    /// <summary>
    /// Read-only collection of grade offerings for this school.
    /// </summary>
    public IReadOnlyCollection<GradeOffering> GradeOfferings => _gradeOfferings.AsReadOnly();

    /// <summary>
    /// Gets the minimum grade level currently offered (null if no grades).
    /// </summary>
    public GradeLevel? GradeRangeMin => _gradeOfferings
        .Where(g => g.IsActive)
        .OrderBy(g => g.GradeLevel)
        .FirstOrDefault()?.GradeLevel;

    /// <summary>
    /// Gets the maximum grade level currently offered (null if no grades).
    /// </summary>
    public GradeLevel? GradeRangeMax => _gradeOfferings
        .Where(g => g.IsActive)
        .OrderByDescending(g => g.GradeLevel)
        .FirstOrDefault()?.GradeLevel;

    /// <summary>
    /// Sets the complete list of grades this school will offer.
    /// Replaces all existing active offerings with the new set.
    /// </summary>
    /// <param name="gradeLevels">New set of grade levels (can be empty with confirmation)</param>
    /// <param name="updatedBy">User performing the update</param>
    /// <exception cref="ArgumentNullException">Thrown if gradeLevels is null</exception>
    /// <exception cref="InvalidOperationException">Thrown if school is deleted</exception>
    public void SetGradeOfferings(IEnumerable<GradeLevel> gradeLevels, Guid updatedBy)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update grades for a deleted school");

        if (gradeLevels == null)
            throw new ArgumentNullException(nameof(gradeLevels));

        if (updatedBy == Guid.Empty)
            throw new ArgumentException("UpdatedBy cannot be empty", nameof(updatedBy));

        var newGrades = gradeLevels.Distinct().ToList();

        // Validate all grades
        foreach (var grade in newGrades)
        {
            if (!GradeTaxonomy.IsValidGrade(grade))
                throw new ArgumentException($"Invalid grade level: {grade}", nameof(gradeLevels));
        }

        // Retire all current active offerings
        var currentOfferings = _gradeOfferings.Where(g => g.IsActive).ToList();
        foreach (var offering in currentOfferings)
        {
            offering.Retire(updatedBy);
        }

        // Create new offerings
        foreach (var grade in newGrades)
        {
            var offering = GradeOffering.Create(Id, grade, updatedBy);
            _gradeOfferings.Add(offering);
        }

        UpdatedBy = updatedBy;
        UpdatedAtUtc = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();

        RaiseDomainEvent(new SchoolGradesUpdatedEvent(Id, DistrictId, newGrades, updatedBy));
    }

    /// <summary>
    /// Sets grades using a contiguous range between min and max (inclusive).
    /// Helper method for UI range selection.
    /// </summary>
    /// <param name="minGrade">Minimum grade in range</param>
    /// <param name="maxGrade">Maximum grade in range</param>
    /// <param name="updatedBy">User performing the update</param>
    public void SetGradeRange(GradeLevel minGrade, GradeLevel maxGrade, Guid updatedBy)
    {
        var range = GradeTaxonomy.GetGradeRange(minGrade, maxGrade);
        SetGradeOfferings(range, updatedBy);
    }

    /// <summary>
    /// Sets all grades for a specific school type.
    /// Helper method for "Select All" functionality.
    /// </summary>
    /// <param name="schoolType">School type to select grades for</param>
    /// <param name="updatedBy">User performing the update</param>
    public void SetAllGradesForType(SchoolType schoolType, Guid updatedBy)
    {
        var grades = GradeTaxonomy.GetGradesForSchoolType(schoolType);
        SetGradeOfferings(grades, updatedBy);
    }

    /// <summary>
    /// Clears all grade offerings for this school.
    /// Used when admin explicitly confirms serving no grades.
    /// </summary>
    /// <param name="updatedBy">User performing the update</param>
    public void ClearAllGrades(Guid updatedBy)
    {
        SetGradeOfferings(Array.Empty<GradeLevel>(), updatedBy);
    }
}
