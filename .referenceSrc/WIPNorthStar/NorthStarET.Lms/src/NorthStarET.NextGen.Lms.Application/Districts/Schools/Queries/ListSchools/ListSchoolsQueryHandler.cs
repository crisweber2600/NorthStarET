using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Contracts.Schools;
using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Queries.ListSchools;

/// <summary>
/// Handler for retrieving schools scoped to a district.
/// </summary>
public sealed class ListSchoolsQueryHandler : IRequestHandler<ListSchoolsQuery, Result<ListSchoolsResponse>>
{
    private readonly ISchoolRepository _repository;

    public ListSchoolsQueryHandler(ISchoolRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ListSchoolsResponse>> Handle(ListSchoolsQuery request, CancellationToken cancellationToken)
    {
        var searchTerm = string.IsNullOrWhiteSpace(request.Search)
            ? null
            : request.Search.Trim();

        var sortOrder = string.IsNullOrWhiteSpace(request.Sort)
            ? "name-asc"
            : request.Sort.Trim().ToLowerInvariant();

        var schools = await _repository.ListByDistrictAsync(
            request.DistrictId,
            searchTerm,
            includeDeleted: false,
            cancellationToken);

        var ordered = sortOrder switch
        {
            "name-desc" => schools.OrderByDescending(static s => s.Name),
            _ => schools.OrderBy(static s => s.Name)
        };

        var items = ordered.Select(static school => new SchoolListItemResponse
        {
            SchoolId = school.Id,
            Name = school.Name,
            Code = school.Code,
            Status = school.Status.ToString(),
            GradeRangeMin = school.GradeRangeMin?.ToString(),
            GradeRangeMax = school.GradeRangeMax?.ToString(),
            LastUpdated = school.UpdatedAtUtc ?? school.CreatedAtUtc
        }).ToList();

        return Result.Success(new ListSchoolsResponse
        {
            Items = items,
            TotalCount = items.Count
        });
    }
}
