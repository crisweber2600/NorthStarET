using System;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Models;

public sealed record UserTenantMembership(
    Guid TenantId,
    string TenantName,
    TenantType TenantType,
    Guid? ParentTenantId,
    Guid RoleId,
    string RoleName,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt);
