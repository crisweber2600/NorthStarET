using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;

namespace NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.RevokeDistrictAdmin;

public sealed record RevokeDistrictAdminCommand(
    Guid DistrictId,
    Guid AdminId,
    string Reason) : IRequest<Result>, ICommand;
