using StackExchange.Redis;

namespace NorthStarET.Foundation.Infrastructure.Caching;

/// <summary>
/// Redis-backed idempotency service with 10-minute TTL
/// </summary>
public class IdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;
    private static readonly TimeSpan IdempotencyWindow = TimeSpan.FromMinutes(10);

    public IdempotencyService(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    public async Task<(bool Exists, string? Result)> CheckIdempotencyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Idempotency key cannot be null or empty", nameof(key));
        }

        var db = _redis.GetDatabase();
        var redisKey = $"idempotency:{key}";
        
        var result = await db.StringGetAsync(redisKey);
        
        return (result.HasValue, result.HasValue ? result.ToString() : null);
    }

    public async Task StoreIdempotencyAsync(
        string key,
        string result,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Idempotency key cannot be null or empty", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new ArgumentException("Result cannot be null or empty", nameof(result));
        }

        var db = _redis.GetDatabase();
        var redisKey = $"idempotency:{key}";
        
        await db.StringSetAsync(redisKey, result, IdempotencyWindow);
    }
}
