using System;
using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Models;

public sealed record GetUserTenantsResult(
    Guid UserId,
    IReadOnlyCollection<UserTenantMembership> Tenants);
