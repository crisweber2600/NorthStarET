using System;

namespace NorthStarET.NextGen.Lms.Contracts.Authorization;

public sealed class SwitchTenantRequest
{
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public Guid TargetTenantId { get; set; }
}
