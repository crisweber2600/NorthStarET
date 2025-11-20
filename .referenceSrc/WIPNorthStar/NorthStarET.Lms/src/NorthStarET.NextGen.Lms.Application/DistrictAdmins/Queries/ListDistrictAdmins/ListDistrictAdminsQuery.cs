using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;

namespace NorthStarET.NextGen.Lms.Application.DistrictAdmins.Queries.ListDistrictAdmins;

public sealed record ListDistrictAdminsQuery(
    Guid DistrictId,
    string? StatusFilter = null) : IRequest<Result<IReadOnlyList<DistrictAdminResponse>>>, ITenantScoped;
