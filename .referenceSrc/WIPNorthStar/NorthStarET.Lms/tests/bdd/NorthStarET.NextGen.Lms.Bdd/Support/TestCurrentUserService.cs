using System;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Domain.Auditing;

namespace NorthStarET.NextGen.Lms.Bdd.Support;

/// <summary>
/// Static current-user context used by BDD steps to emulate an authenticated system admin.
/// </summary>
public sealed class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; } = Guid.NewGuid();
    public ActorRole Role { get; set; } = ActorRole.PlatformAdmin;
    public Guid? DistrictId { get; set; }
    public Guid? CorrelationId { get; set; } = Guid.NewGuid();

    public void SetSystemAdmin()
    {
        UserId = Guid.NewGuid();
        Role = ActorRole.PlatformAdmin;
        DistrictId = null;
        CorrelationId = Guid.NewGuid();
    }

    public void SetDistrictAdmin(Guid districtId)
    {
        UserId = Guid.NewGuid();
        Role = ActorRole.DistrictAdmin;
        DistrictId = districtId;
        CorrelationId = Guid.NewGuid();
    }
}
