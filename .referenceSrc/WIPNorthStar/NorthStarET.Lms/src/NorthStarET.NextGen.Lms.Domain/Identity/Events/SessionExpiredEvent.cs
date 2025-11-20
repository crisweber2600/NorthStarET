using System;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Events;

public sealed record SessionExpiredEvent(
    Guid SessionId,
    Guid UserId,
    DateTimeOffset ExpiredAt);
