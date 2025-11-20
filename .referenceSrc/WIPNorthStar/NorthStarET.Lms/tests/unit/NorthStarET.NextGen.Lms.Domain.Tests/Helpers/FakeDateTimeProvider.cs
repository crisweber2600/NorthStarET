using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;

namespace NorthStarET.NextGen.Lms.Domain.Tests.Helpers;

/// <summary>
/// Fake implementation of IDateTimeProvider for testing purposes.
/// Allows setting a fixed time for deterministic tests.
/// </summary>
public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    private DateTime _utcNow;

    public FakeDateTimeProvider(DateTime utcNow)
    {
        _utcNow = utcNow;
    }

    public FakeDateTimeProvider()
        : this(DateTime.UtcNow)
    {
    }

    public DateTime UtcNow => _utcNow;

    /// <summary>
    /// Sets the current UTC time for testing.
    /// </summary>
    public void SetUtcNow(DateTime utcNow)
    {
        _utcNow = utcNow;
    }

    /// <summary>
    /// Advances the current time by the specified duration.
    /// </summary>
    public void Advance(TimeSpan duration)
    {
        _utcNow = _utcNow.Add(duration);
    }
}
