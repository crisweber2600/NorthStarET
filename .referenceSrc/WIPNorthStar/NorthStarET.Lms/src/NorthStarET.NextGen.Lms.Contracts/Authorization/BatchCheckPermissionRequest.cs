using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Contracts.Authorization;

public sealed class BatchCheckPermissionRequest
{
    public IReadOnlyCollection<CheckPermissionRequest> Checks { get; set; } = new List<CheckPermissionRequest>();
}
