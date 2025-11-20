using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;
using NorthStarET.NextGen.Lms.Application.Common.Caching;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using StackExchange.Redis;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Caching;

internal sealed class AuthorizationCacheService : IAuthorizationCache
{
    private readonly IDatabase database;
    private readonly ILogger<AuthorizationCacheService> logger;
    private readonly IdentityModuleSettings settings;

    public AuthorizationCacheService(IConnectionMultiplexer connectionMultiplexer, IOptions<IdentityModuleSettings> options, ILogger<AuthorizationCacheService> logger)
    {
        database = connectionMultiplexer.GetDatabase();
        settings = options.Value;
        this.logger = logger;
    }

    public async Task<AuthorizationDecision?> GetAsync(Guid userId, Guid tenantId, string resource, string action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(userId, tenantId, resource, action);

        RedisValue payload;

        try
        {
            payload = await database.StringGetAsync(key, CommandFlags.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read authorization decision from cache for user {UserId} tenant {TenantId} resource {Resource} action {Action}.", userId, tenantId, resource, action);
            throw;
        }

        if (payload.IsNullOrEmpty)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AuthorizationDecision>(payload.ToString());
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Authorization cache contained invalid payload for key {AuthorizationCacheKey}; entry will be evicted.", key);
            await database.KeyDeleteAsync(key, CommandFlags.None).ConfigureAwait(false);
            return null;
        }
    }

    public async Task SetAsync(AuthorizationDecision decision, TimeSpan timeToLive, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = BuildKey(decision.UserId, decision.TenantId, decision.Resource, decision.Action);
        var indexKey = BuildIndexKey(decision.UserId, decision.TenantId);
        var payload = JsonSerializer.Serialize(decision);

        var effectiveTtl = timeToLive > TimeSpan.Zero
            ? timeToLive
            : TimeSpan.FromMinutes(settings.AuthorizationCacheTtlMinutes);

        try
        {
            var stored = await database.StringSetAsync(key, payload, effectiveTtl, When.Always, CommandFlags.None).ConfigureAwait(false);

            if (!stored)
            {
                logger.LogWarning("Failed to store authorization decision in cache for key {AuthorizationCacheKey}.", key);
                // Do not update the tracking set if the cache entry was not stored,
                // to avoid tracking keys that do not exist in Redis.
                return;
            }

            // Add the key to the tracking set for this user/tenant
            var addedToSet = await database.SetAddAsync(indexKey, key, CommandFlags.None).ConfigureAwait(false);
            if (!addedToSet)
            {
                logger.LogWarning("Failed to add cache key {AuthorizationCacheKey} to tracking set {AuthorizationCacheIndexKey}.", key, indexKey);
            }

            // Set expiry on the index key to be longer than individual cache entries to ensure cleanup
            // Use 2x the TTL to allow for clock skew and ensure the index outlives the cached entries
            await database.KeyExpireAsync(indexKey, effectiveTtl.Add(effectiveTtl), ExpireWhen.Always, CommandFlags.FireAndForget).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to store authorization decision in cache for key {AuthorizationCacheKey}.", key);
            throw;
        }
    }

    public async Task ClearForUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var indexKey = BuildIndexKey(userId, tenantId);

        try
        {
            // Retrieve all keys from the tracking set for this user/tenant
            var keys = await database.SetMembersAsync(indexKey, CommandFlags.None).ConfigureAwait(false);

            var deletedCount = 0;
            if (keys.Length > 0)
            {
                // Delete all cache entries
                var redisKeys = keys.Select(k => (RedisKey)k.ToString()).ToArray();
                deletedCount = (int)await database.KeyDeleteAsync(redisKeys, CommandFlags.None).ConfigureAwait(false);

                // Remove the deleted keys from the tracking set to prevent race conditions
                // with concurrent SetAsync calls that may add new entries while we're clearing
                await database.SetRemoveAsync(indexKey, keys, CommandFlags.None).ConfigureAwait(false);
            }

            logger.LogInformation(
                "Cleared {DeletedCount} authorization cache entries for user {UserId} tenant {TenantId}",
                deletedCount,
                userId,
                tenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clear authorization cache for user {UserId} tenant {TenantId}", userId, tenantId);
            throw;
        }
    }

    private static string BuildKey(Guid userId, Guid tenantId, string resource, string action) =>
        $"authz:{userId}:{tenantId}:{resource}:{action}";

    private static string BuildIndexKey(Guid userId, Guid tenantId) =>
        $"authz:index:{userId}:{tenantId}";
}
