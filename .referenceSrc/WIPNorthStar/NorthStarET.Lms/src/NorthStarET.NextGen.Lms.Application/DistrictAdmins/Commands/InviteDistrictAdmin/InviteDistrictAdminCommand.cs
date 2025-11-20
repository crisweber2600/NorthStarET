using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;

namespace NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.InviteDistrictAdmin;

public sealed record InviteDistrictAdminCommand(
    Guid DistrictId,
    string FirstName,
    string LastName,
    string Email) : IRequest<Result<InviteDistrictAdminResponse>>, ICommand;
