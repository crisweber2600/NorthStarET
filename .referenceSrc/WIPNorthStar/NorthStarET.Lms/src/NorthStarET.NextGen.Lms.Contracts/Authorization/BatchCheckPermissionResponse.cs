using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Contracts.Authorization;

public sealed class BatchCheckPermissionResponse
{
    public IReadOnlyCollection<CheckPermissionResponse> Results { get; set; } = new List<CheckPermissionResponse>();
}
