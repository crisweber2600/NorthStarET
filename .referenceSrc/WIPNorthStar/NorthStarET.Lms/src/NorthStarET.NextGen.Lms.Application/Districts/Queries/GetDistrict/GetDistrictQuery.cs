using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.Districts;

namespace NorthStarET.NextGen.Lms.Application.Districts.Queries.GetDistrict;

/// <summary>
/// Query to retrieve a single district by ID with admin counts.
/// Implements tenant scoping.
/// </summary>
public sealed record GetDistrictQuery(Guid DistrictId)
    : IRequest<Result<DistrictResponse>>, ITenantScoped;
