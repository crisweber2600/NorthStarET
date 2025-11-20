using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.DeleteSchool;

/// <summary>
/// Command to soft delete a school.
/// </summary>
public sealed record DeleteSchoolCommand(
    Guid DistrictId,
    Guid SchoolId,
    Guid DeletedBy
) : IRequest<Result>;
