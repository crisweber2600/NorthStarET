using System;
using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Contracts.Authorization;

public sealed class CheckPermissionRequest
{
    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public string Resource { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public Dictionary<string, string>? Context { get; set; }
}
