using System;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Commands;

public sealed record SwitchTenantContextCommand(
    Guid SessionId,
    Guid UserId,
    Guid TargetTenantId) : IRequest, ICommand;
