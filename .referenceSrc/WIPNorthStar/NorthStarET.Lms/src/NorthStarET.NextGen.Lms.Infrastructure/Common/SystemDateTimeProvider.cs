using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;

namespace NorthStarET.NextGen.Lms.Infrastructure.Common;

/// <summary>
/// System implementation of IDateTimeProvider that returns actual system time.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
