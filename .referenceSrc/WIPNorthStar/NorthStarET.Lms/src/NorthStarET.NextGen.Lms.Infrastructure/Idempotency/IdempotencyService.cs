using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace NorthStarET.NextGen.Lms.Infrastructure.Idempotency;

/// <summary>
/// Redis-backed idempotency service for preventing duplicate operations within a configurable time window.
/// Implements 10-minute idempotency windows for create/update commands as specified in feature 002 requirements.
/// </summary>
public sealed class IdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly TimeSpan _idempotencyWindow;

    public IdempotencyService(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _idempotencyWindow = TimeSpan.FromMinutes(10); // 10-minute window per spec
    }

    /// <summary>
    /// Attempts to execute an operation with idempotency protection.
    /// If the same operation (by key and payload) was executed within the window, returns the cached result.
    /// If a different payload is attempted with the same key within the window, throws IdempotencyConflictException.
    /// </summary>
    /// <typeparam name="TResult">The result type of the operation</typeparam>
    /// <param name="actorId">The user/actor performing the operation</param>
    /// <param name="action">The action being performed (e.g., "CreateDistrict", "UpdateDistrict")</param>
    /// <param name="resourceId">The resource identifier (e.g., district ID, admin ID)</param>
    /// <param name="payload">The operation payload to hash for comparison</param>
    /// <param name="operation">The actual operation to execute if not idempotent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation (cached or newly executed)</returns>
    public async Task<TResult> ExecuteWithIdempotencyAsync<TResult>(
        string actorId,
        string action,
        string resourceId,
        object payload,
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(operation);

        var db = _redis.GetDatabase();
        var idempotencyKey = BuildIdempotencyKey(actorId, action, resourceId);
        var payloadHash = ComputePayloadHash(payload);

        // Check if operation already exists in idempotency window
        var existingEntry = await db.HashGetAllAsync(idempotencyKey);

        if (existingEntry.Length > 0)
        {
            var storedHash = existingEntry.FirstOrDefault(h => h.Name == "PayloadHash").Value;
            var storedResult = existingEntry.FirstOrDefault(h => h.Name == "Result").Value;

            if (storedHash.HasValue && storedHash != payloadHash)
            {
                // Same idempotency key but different payload - conflict
                throw new IdempotencyConflictException(
                    $"Idempotency conflict: Operation '{action}' on resource '{resourceId}' by actor '{actorId}' " +
                    $"was already attempted within the {_idempotencyWindow.TotalMinutes}-minute window with a different payload.");
            }

            if (storedResult.HasValue)
            {
                // Same operation with same payload - return cached result
                return DeserializeResult<TResult>(storedResult!);
            }
        }

        // Execute the operation
        var result = await operation();

        // Store result with expiry
        var hashEntries = new[]
        {
            new HashEntry("PayloadHash", payloadHash),
            new HashEntry("Result", SerializeResult(result)),
            new HashEntry("Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await db.HashSetAsync(idempotencyKey, hashEntries);
        await db.KeyExpireAsync(idempotencyKey, _idempotencyWindow);

        return result;
    }

    /// <summary>
    /// Checks if an operation is within the idempotency window (without executing).
    /// </summary>
    public async Task<bool> IsWithinIdempotencyWindowAsync(
        string actorId,
        string action,
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);

        var db = _redis.GetDatabase();
        var idempotencyKey = BuildIdempotencyKey(actorId, action, resourceId);

        return await db.KeyExistsAsync(idempotencyKey);
    }

    /// <summary>
    /// Clears the idempotency window for a specific operation (useful for testing or manual intervention).
    /// </summary>
    public async Task ClearIdempotencyWindowAsync(
        string actorId,
        string action,
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);

        var db = _redis.GetDatabase();
        var idempotencyKey = BuildIdempotencyKey(actorId, action, resourceId);

        await db.KeyDeleteAsync(idempotencyKey);
    }

    private static string BuildIdempotencyKey(string actorId, string action, string resourceId)
    {
        // Format: idempotency:{action}:{actorId}:{resourceId}
        return $"idempotency:{action}:{actorId}:{resourceId}";
    }

    private static string ComputePayloadHash(object payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToBase64String(hashBytes);
    }

    private static string SerializeResult<TResult>(TResult result)
    {
        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static TResult DeserializeResult<TResult>(string json)
    {
        return JsonSerializer.Deserialize<TResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }) ?? throw new InvalidOperationException("Failed to deserialize idempotency result.");
    }
}

/// <summary>
/// Interface for idempotency service operations.
/// </summary>
public interface IIdempotencyService
{
    Task<TResult> ExecuteWithIdempotencyAsync<TResult>(
        string actorId,
        string action,
        string resourceId,
        object payload,
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    Task<bool> IsWithinIdempotencyWindowAsync(
        string actorId,
        string action,
        string resourceId,
        CancellationToken cancellationToken = default);

    Task ClearIdempotencyWindowAsync(
        string actorId,
        string action,
        string resourceId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Exception thrown when an idempotency conflict is detected (same key, different payload within window).
/// </summary>
public sealed class IdempotencyConflictException : Exception
{
    public IdempotencyConflictException(string message) : base(message)
    {
    }

    public IdempotencyConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
