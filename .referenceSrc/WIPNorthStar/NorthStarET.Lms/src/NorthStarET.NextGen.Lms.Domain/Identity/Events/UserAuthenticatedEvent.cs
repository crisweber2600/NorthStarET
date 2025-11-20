using System;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Events;

public sealed record UserAuthenticatedEvent(
    Guid UserId,
    Guid SessionId,
    TenantId TenantId,
    DateTimeOffset AuthenticatedAt);
