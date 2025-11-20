using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Contracts.Schools;
using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Queries.GetSchool;

/// <summary>
/// Handler for retrieving detailed school information.
/// </summary>
public sealed class GetSchoolQueryHandler : IRequestHandler<GetSchoolQuery, Result<SchoolDetailResponse>>
{
    private readonly ISchoolRepository _repository;

    public GetSchoolQueryHandler(ISchoolRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SchoolDetailResponse>> Handle(GetSchoolQuery request, CancellationToken cancellationToken)
    {
        // Get school with grade offerings
        var school = await _repository.GetByIdWithGradesAsync(
            request.SchoolId,
            request.DistrictId,
            cancellationToken);

        if (school == null)
        {
            return Result.Failure<SchoolDetailResponse>(
                new Error("School.NotFound", "School not found in the specified district."));
        }

        if (school.DeletedAt.HasValue)
        {
            return Result.Failure<SchoolDetailResponse>(
                new Error("School.Deleted", "School has been deleted."));
        }

        // Map grade offerings to DTOs
        var gradeSelections = school.GradeOfferings
            .Where(g => !g.EffectiveTo.HasValue) // Only active grade offerings
            .Select(g => new GradeSelectionDto
            {
                GradeId = g.GradeLevel.ToString(),
                SchoolType = g.SchoolType.ToString(),
                Selected = true
            }).ToList();

        var response = new SchoolDetailResponse
        {
            SchoolId = school.Id,
            DistrictId = school.DistrictId,
            Name = school.Name,
            Code = school.Code,
            Notes = school.Notes,
            Status = school.DeletedAt.HasValue ? "Inactive" : "Active",
            GradeSelections = gradeSelections,
            ConcurrencyStamp = school.ConcurrencyStamp,
            Audit = new SchoolAuditDto
            {
                CreatedBy = school.CreatedBy,
                CreatedAt = school.CreatedAtUtc,
                UpdatedBy = school.UpdatedBy,
                UpdatedAt = school.UpdatedAtUtc
            }
        };

        return Result.Success(response);
    }
}
