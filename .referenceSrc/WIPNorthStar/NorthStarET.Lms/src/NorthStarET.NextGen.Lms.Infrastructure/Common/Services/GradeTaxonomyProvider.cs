using NorthStarET.NextGen.Lms.Application.Common.Abstractions;
using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Infrastructure.Common.Services;

/// <summary>
/// Default implementation of IGradeTaxonomyProvider that uses the static GradeTaxonomy from the domain.
/// </summary>
public sealed class GradeTaxonomyProvider : IGradeTaxonomyProvider
{
    public IReadOnlyList<GradeLevel> GetAllGrades() => GradeTaxonomy.AllGrades;

    public IReadOnlyList<GradeLevel> GetGradesForSchoolType(SchoolType schoolType) =>
        GradeTaxonomy.GetGradesForSchoolType(schoolType);

    public IReadOnlyList<GradeLevel> GetGradeRange(GradeLevel minGrade, GradeLevel maxGrade) =>
        GradeTaxonomy.GetGradeRange(minGrade, maxGrade);

    public string GetDisplayName(GradeLevel grade) => GradeTaxonomy.GetDisplayName(grade);

    public SchoolType GetTypicalSchoolType(GradeLevel grade) => GradeTaxonomy.GetTypicalSchoolType(grade);

    public bool IsValidGrade(GradeLevel grade) => GradeTaxonomy.IsValidGrade(grade);
}
