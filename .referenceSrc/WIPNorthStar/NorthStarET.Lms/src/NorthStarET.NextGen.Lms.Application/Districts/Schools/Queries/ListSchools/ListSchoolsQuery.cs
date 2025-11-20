using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.Schools;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Queries.ListSchools;

/// <summary>
/// Query to retrieve schools scoped to a district with optional search and sort.
/// </summary>
public sealed record ListSchoolsQuery(
    Guid DistrictId,
    string? Search = null,
    string? Sort = "name-asc"
) : IRequest<Result<ListSchoolsResponse>>, ITenantScoped
{
    Guid ITenantScoped.DistrictId => DistrictId;
}
