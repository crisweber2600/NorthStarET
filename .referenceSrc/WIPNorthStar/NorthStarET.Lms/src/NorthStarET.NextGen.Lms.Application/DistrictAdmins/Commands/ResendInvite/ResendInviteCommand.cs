using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;

namespace NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.ResendInvite;

public sealed record ResendInviteCommand(
    Guid DistrictId,
    Guid AdminId) : IRequest<Result>, ICommand;
