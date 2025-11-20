using System;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;

public sealed record SessionCacheModel(
    Guid SessionId,
    Guid UserId,
    Guid ActiveTenantId,
    DateTimeOffset ExpiresAt,
    DateTimeOffset LastActivityAt,
    bool IsRevoked);
