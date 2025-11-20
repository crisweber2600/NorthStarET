namespace NorthStarET.NextGen.Lms.Domain.Common.Interfaces;

/// <summary>
/// Abstraction for providing current date and time.
/// Enables deterministic time-dependent behavior in tests.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
