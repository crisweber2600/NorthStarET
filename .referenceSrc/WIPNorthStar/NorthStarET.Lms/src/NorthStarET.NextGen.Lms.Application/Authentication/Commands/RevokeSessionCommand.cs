using System;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Commands;

public sealed record RevokeSessionCommand(Guid SessionId) : IRequest, ICommand;
