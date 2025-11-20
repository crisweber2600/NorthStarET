using System;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Commands;

public sealed record ExchangeEntraTokenCommand(
    string EntraToken,
    Guid ActiveTenantId,
    string? IpAddress,
    string? UserAgent) : IRequest<TokenExchangeResult>, ICommand;
