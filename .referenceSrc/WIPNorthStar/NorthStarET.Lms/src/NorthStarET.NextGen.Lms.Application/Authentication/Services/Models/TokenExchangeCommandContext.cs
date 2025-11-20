using System;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;

public sealed record TokenExchangeCommandContext(
    string EntraToken,
    Guid ActiveTenantId,
    string? IpAddress,
    string? UserAgent);
