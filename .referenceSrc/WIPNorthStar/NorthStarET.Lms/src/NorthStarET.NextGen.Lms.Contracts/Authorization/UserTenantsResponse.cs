using System;
using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Contracts.Authorization;

public sealed class UserTenantsResponse
{
    public Guid UserId { get; set; }

    public IReadOnlyCollection<TenantMembershipDto> Tenants { get; set; } = new List<TenantMembershipDto>();
}
