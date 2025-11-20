using System;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;

public sealed record UserContextModel(
    Guid Id,
    string Email,
    string DisplayName,
    Guid ActiveTenantId,
    string ActiveTenantName,
    string ActiveTenantType,
    string Role);
