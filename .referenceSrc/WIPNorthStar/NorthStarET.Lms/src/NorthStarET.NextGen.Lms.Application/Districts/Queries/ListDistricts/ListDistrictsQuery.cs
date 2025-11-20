using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.Districts;

namespace NorthStarET.NextGen.Lms.Application.Districts.Queries.ListDistricts;

/// <summary>
/// Query to retrieve a paginated list of active districts ordered by name.
/// Excludes soft-deleted districts. Implements tenant scoping.
/// </summary>
public sealed record ListDistrictsQuery(int PageNumber, int PageSize)
    : IRequest<Result<PagedResult<DistrictSummaryResponse>>>;
