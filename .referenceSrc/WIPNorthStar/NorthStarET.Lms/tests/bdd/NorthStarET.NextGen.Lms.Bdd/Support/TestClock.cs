using System;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;

namespace NorthStarET.NextGen.Lms.Bdd.Support;

/// <summary>
/// Simple controllable clock used to simulate time-dependent behavior in BDD scenarios.
/// Implements IDateTimeProvider to integrate with domain entities.
/// </summary>
public sealed class TestClock : IDateTimeProvider
{
    private DateTimeOffset _utcNow;

    public TestClock()
    {
        _utcNow = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the current timestamp used by the scenario as DateTime.
    /// </summary>
    DateTime IDateTimeProvider.UtcNow => _utcNow.UtcDateTime;

    /// <summary>
    /// Gets the current timestamp used by the scenario as DateTimeOffset.
    /// </summary>
    public DateTimeOffset UtcNow => _utcNow;

    /// <summary>
    /// Advances the clock by the specified duration.
    /// </summary>
    public void Advance(TimeSpan duration)
    {
        _utcNow = _utcNow.Add(duration);
    }

    /// <summary>
    /// Sets the clock to a specific timestamp.
    /// </summary>
    public void Set(DateTimeOffset timestamp)
    {
        _utcNow = timestamp;
    }
}
