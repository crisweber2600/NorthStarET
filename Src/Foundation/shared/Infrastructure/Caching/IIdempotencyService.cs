namespace NorthStarET.Foundation.Infrastructure.Caching;

/// <summary>
/// Service for managing idempotency with Redis
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Checks if an idempotency key exists and returns the stored result
    /// </summary>
    Task<(bool Exists, string? Result)> CheckIdempotencyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a result with an idempotency key (10-minute TTL)
    /// </summary>
    Task StoreIdempotencyAsync(string key, string result, CancellationToken cancellationToken = default);
}
