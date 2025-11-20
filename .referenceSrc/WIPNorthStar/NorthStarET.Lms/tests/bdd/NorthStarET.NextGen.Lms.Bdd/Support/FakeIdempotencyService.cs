using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Infrastructure.Idempotency;

namespace NorthStarET.NextGen.Lms.Bdd.Support;

/// <summary>
/// In-memory implementation of <see cref="IIdempotencyService"/> tailored for BDD scenarios.
/// Mirrors the production behavior by enforcing a 10-minute window keyed by actor/action/resource.
/// </summary>
public sealed class FakeIdempotencyService : IIdempotencyService
{
    private readonly ConcurrentDictionary<string, IdempotencyRecord> _records = new();
    private readonly TestClock _clock;
    private readonly TimeSpan _window = TimeSpan.FromMinutes(10);

    public FakeIdempotencyService(TestClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public bool LastInvocationUsedCachedResult { get; private set; }

    public Task<TResult> ExecuteWithIdempotencyAsync<TResult>(
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

        var key = BuildKey(actorId, action, resourceId);
        var hash = ComputePayloadHash(payload);

        if (_records.TryGetValue(key, out var record))
        {
            var withinWindow = _clock.UtcNow - record.Timestamp <= _window;

            if (withinWindow && record.PayloadHash != hash)
            {
                throw new IdempotencyConflictException(
                    $"Idempotency conflict for actor '{actorId}', action '{action}', resource '{resourceId}'.");
            }

            if (withinWindow && record.Result is TResult cachedResult)
            {
                LastInvocationUsedCachedResult = true;
                return Task.FromResult(cachedResult);
            }

            if (!withinWindow)
            {
                _records.TryRemove(key, out _);
            }
        }

        return ExecuteAndStoreAsync();

        async Task<TResult> ExecuteAndStoreAsync()
        {
            var result = await operation().ConfigureAwait(false);
            LastInvocationUsedCachedResult = false;

            var newRecord = new IdempotencyRecord(hash, _clock.UtcNow, result!);
            _records[key] = newRecord;

            return result;
        }
    }

    public Task<bool> IsWithinIdempotencyWindowAsync(
        string actorId,
        string action,
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);

        var key = BuildKey(actorId, action, resourceId);
        return Task.FromResult(
            _records.TryGetValue(key, out var record) &&
            _clock.UtcNow - record.Timestamp <= _window);
    }

    public Task ClearIdempotencyWindowAsync(
        string actorId,
        string action,
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);

        var key = BuildKey(actorId, action, resourceId);
        _records.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _records.Clear();
        LastInvocationUsedCachedResult = false;
    }

    private static string BuildKey(string actorId, string action, string resourceId)
        => $"idempotency:{action}:{actorId}:{resourceId}";

    private static string ComputePayloadHash(object payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    private sealed record IdempotencyRecord(string PayloadHash, DateTimeOffset Timestamp, object Result);
}
