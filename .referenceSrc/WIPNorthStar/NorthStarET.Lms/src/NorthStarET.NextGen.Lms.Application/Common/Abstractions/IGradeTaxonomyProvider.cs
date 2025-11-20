using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Application.Common.Abstractions;

/// <summary>
/// Abstraction for accessing the centralized grade taxonomy.
/// Provides read-only access to valid grade levels and school type classifications.
/// </summary>
public interface IGradeTaxonomyProvider
{
    /// <summary>
    /// Gets all available grade levels in sequential order.
    /// </summary>
    IReadOnlyList<GradeLevel> GetAllGrades();

    /// <summary>
    /// Gets grades typically associated with a specific school type.
    /// </summary>
    /// <param name="schoolType">The school type to filter by</param>
    IReadOnlyList<GradeLevel> GetGradesForSchoolType(SchoolType schoolType);

    /// <summary>
    /// Gets a contiguous range of grades between min and max (inclusive).
    /// </summary>
    /// <param name="minGrade">Minimum grade in range</param>
    /// <param name="maxGrade">Maximum grade in range</param>
    IReadOnlyList<GradeLevel> GetGradeRange(GradeLevel minGrade, GradeLevel maxGrade);

    /// <summary>
    /// Gets the display name for a grade level.
    /// </summary>
    /// <param name="grade">Grade level</param>
    string GetDisplayName(GradeLevel grade);

    /// <summary>
    /// Gets the typical school type for a given grade.
    /// </summary>
    /// <param name="grade">Grade level</param>
    SchoolType GetTypicalSchoolType(GradeLevel grade);

    /// <summary>
    /// Validates that a grade exists in the taxonomy.
    /// </summary>
    /// <param name="grade">Grade level to validate</param>
    bool IsValidGrade(GradeLevel grade);
}
