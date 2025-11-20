using System;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Commands;

public sealed record RefreshSessionCommand(
    Guid SessionId,
    string NewToken) : IRequest, ICommand;
