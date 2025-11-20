using System;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Events;

public sealed record TenantSwitchedEvent(
    Guid SessionId,
    Guid UserId,
    Guid PreviousTenantId,
    Guid NewTenantId,
    DateTimeOffset SwitchedAt);
