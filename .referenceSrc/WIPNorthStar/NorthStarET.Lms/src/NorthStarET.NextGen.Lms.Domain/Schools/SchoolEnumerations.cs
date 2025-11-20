namespace NorthStarET.NextGen.Lms.Domain.Schools;

/// <summary>
/// Type classification for schools based on the grades they primarily serve.
/// Used to organize grade offerings and provide filtering capabilities.
/// </summary>
public enum SchoolType
{
    /// <summary>
    /// Elementary school typically serving grades PK-5
    /// </summary>
    Elementary = 0,

    /// <summary>
    /// Middle school typically serving grades 6-8
    /// </summary>
    Middle = 1,

    /// <summary>
    /// High school typically serving grades 9-12
    /// </summary>
    High = 2,

    /// <summary>
    /// Other school type (e.g., K-12, alternative education, specialized programs)
    /// </summary>
    Other = 3
}

/// <summary>
/// Standard grade levels supported by the LMS, aligned with K-12 education system.
/// Ordered sequentially to support range selection and validation.
/// </summary>
public enum GradeLevel
{
    /// <summary>
    /// Pre-Kindergarten (youngest students, age 3-4)
    /// </summary>
    PreK = 0,

    /// <summary>
    /// Kindergarten (age 5)
    /// </summary>
    K = 1,

    /// <summary>
    /// First grade
    /// </summary>
    Grade1 = 2,

    /// <summary>
    /// Second grade
    /// </summary>
    Grade2 = 3,

    /// <summary>
    /// Third grade
    /// </summary>
    Grade3 = 4,

    /// <summary>
    /// Fourth grade
    /// </summary>
    Grade4 = 5,

    /// <summary>
    /// Fifth grade
    /// </summary>
    Grade5 = 6,

    /// <summary>
    /// Sixth grade
    /// </summary>
    Grade6 = 7,

    /// <summary>
    /// Seventh grade
    /// </summary>
    Grade7 = 8,

    /// <summary>
    /// Eighth grade
    /// </summary>
    Grade8 = 9,

    /// <summary>
    /// Ninth grade (Freshman)
    /// </summary>
    Grade9 = 10,

    /// <summary>
    /// Tenth grade (Sophomore)
    /// </summary>
    Grade10 = 11,

    /// <summary>
    /// Eleventh grade (Junior)
    /// </summary>
    Grade11 = 12,

    /// <summary>
    /// Twelfth grade (Senior)
    /// </summary>
    Grade12 = 13
}

/// <summary>
/// Provides centralized grade taxonomy and classification logic.
/// Supports range queries and school type associations.
/// </summary>
public static class GradeTaxonomy
{
    /// <summary>
    /// All available grade levels in sequential order.
    /// </summary>
    public static readonly IReadOnlyList<GradeLevel> AllGrades = Enum.GetValues<GradeLevel>()
        .OrderBy(g => g)
        .ToList();

    /// <summary>
    /// Elementary school grade levels (PK-5).
    /// </summary>
    public static readonly IReadOnlyList<GradeLevel> ElementaryGrades = new[]
    {
        GradeLevel.PreK,
        GradeLevel.K,
        GradeLevel.Grade1,
        GradeLevel.Grade2,
        GradeLevel.Grade3,
        GradeLevel.Grade4,
        GradeLevel.Grade5
    };

    /// <summary>
    /// Middle school grade levels (6-8).
    /// </summary>
    public static readonly IReadOnlyList<GradeLevel> MiddleGrades = new[]
    {
        GradeLevel.Grade6,
        GradeLevel.Grade7,
        GradeLevel.Grade8
    };

    /// <summary>
    /// High school grade levels (9-12).
    /// </summary>
    public static readonly IReadOnlyList<GradeLevel> HighGrades = new[]
    {
        GradeLevel.Grade9,
        GradeLevel.Grade10,
        GradeLevel.Grade11,
        GradeLevel.Grade12
    };

    /// <summary>
    /// Gets the human-readable display name for a grade level.
    /// </summary>
    public static string GetDisplayName(GradeLevel grade) => grade switch
    {
        GradeLevel.PreK => "Pre-K",
        GradeLevel.K => "Kindergarten",
        GradeLevel.Grade1 => "1st Grade",
        GradeLevel.Grade2 => "2nd Grade",
        GradeLevel.Grade3 => "3rd Grade",
        GradeLevel.Grade4 => "4th Grade",
        GradeLevel.Grade5 => "5th Grade",
        GradeLevel.Grade6 => "6th Grade",
        GradeLevel.Grade7 => "7th Grade",
        GradeLevel.Grade8 => "8th Grade",
        GradeLevel.Grade9 => "9th Grade",
        GradeLevel.Grade10 => "10th Grade",
        GradeLevel.Grade11 => "11th Grade",
        GradeLevel.Grade12 => "12th Grade",
        _ => grade.ToString()
    };

    /// <summary>
    /// Gets the typical school type for a given grade level.
    /// </summary>
    public static SchoolType GetTypicalSchoolType(GradeLevel grade) => grade switch
    {
        GradeLevel.PreK => SchoolType.Elementary,
        GradeLevel.K => SchoolType.Elementary,
        GradeLevel.Grade1 => SchoolType.Elementary,
        GradeLevel.Grade2 => SchoolType.Elementary,
        GradeLevel.Grade3 => SchoolType.Elementary,
        GradeLevel.Grade4 => SchoolType.Elementary,
        GradeLevel.Grade5 => SchoolType.Elementary,
        GradeLevel.Grade6 => SchoolType.Middle,
        GradeLevel.Grade7 => SchoolType.Middle,
        GradeLevel.Grade8 => SchoolType.Middle,
        GradeLevel.Grade9 => SchoolType.High,
        GradeLevel.Grade10 => SchoolType.High,
        GradeLevel.Grade11 => SchoolType.High,
        GradeLevel.Grade12 => SchoolType.High,
        _ => SchoolType.Other
    };

    /// <summary>
    /// Gets all grades for a specific school type.
    /// </summary>
    public static IReadOnlyList<GradeLevel> GetGradesForSchoolType(SchoolType type) => type switch
    {
        SchoolType.Elementary => ElementaryGrades,
        SchoolType.Middle => MiddleGrades,
        SchoolType.High => HighGrades,
        SchoolType.Other => AllGrades,
        _ => Array.Empty<GradeLevel>()
    };

    /// <summary>
    /// Gets a contiguous range of grades between min and max (inclusive).
    /// Used for range selection in UI.
    /// </summary>
    public static IReadOnlyList<GradeLevel> GetGradeRange(GradeLevel min, GradeLevel max)
    {
        if (min > max)
            throw new ArgumentException("Minimum grade cannot be greater than maximum grade");

        return AllGrades.Where(g => g >= min && g <= max).ToList();
    }

    /// <summary>
    /// Validates that a grade exists in the taxonomy.
    /// </summary>
    public static bool IsValidGrade(GradeLevel grade) => AllGrades.Contains(grade);
}
