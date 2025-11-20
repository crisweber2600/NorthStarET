using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Contracts.Schools;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Queries.GetSchool;

/// <summary>
/// Query to retrieve detailed school information including grade offerings.
/// </summary>
public sealed record GetSchoolQuery(Guid DistrictId, Guid SchoolId) : IRequest<Result<SchoolDetailResponse>>;
