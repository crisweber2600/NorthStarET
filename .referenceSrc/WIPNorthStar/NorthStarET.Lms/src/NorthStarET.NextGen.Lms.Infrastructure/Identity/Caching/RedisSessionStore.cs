using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using StackExchange.Redis;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Caching;

internal sealed class RedisSessionStore : ISessionStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDatabase database;
    private readonly ILogger<RedisSessionStore> logger;
    private readonly TimeSpan slidingExpiration;

    public RedisSessionStore(IConnectionMultiplexer connectionMultiplexer, IOptions<IdentityModuleSettings> options, ILogger<RedisSessionStore> logger)
    {
        database = connectionMultiplexer.GetDatabase();
        this.logger = logger;
        slidingExpiration = TimeSpan.FromMinutes(Math.Max(1, options.Value.SessionSlidingExpirationMinutes));
    }

    public async Task CacheSessionAsync(Session session, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var cacheEntry = new SessionCacheModel(
            session.Id,
            session.UserId,
            session.ActiveTenantId.Value,
            session.ExpiresAt,
            session.LastActivityAt,
            session.IsRevoked);

        var payload = JsonSerializer.Serialize(cacheEntry, SerializerOptions);
        var expiration = ttl > TimeSpan.Zero ? ttl : slidingExpiration;

        await database.StringSetAsync(GetKey(session.Id), payload, expiration);
        logger.LogDebug("Cached session {SessionId} for user {UserId} with expiration {Expiration} minutes", session.Id, session.UserId, expiration.TotalMinutes);
    }

    public async Task<SessionCacheModel?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var value = await database.StringGetAsync(GetKey(sessionId));
        if (!value.HasValue)
        {
            return null;
        }

        var cacheEntry = JsonSerializer.Deserialize<SessionCacheModel>(value!.ToString(), SerializerOptions);
        if (cacheEntry is null)
        {
            await database.KeyDeleteAsync(GetKey(sessionId));
            logger.LogWarning("Failed to deserialize cached session {SessionId}; entry removed", sessionId);
            return null;
        }

        await database.KeyExpireAsync(GetKey(sessionId), slidingExpiration);
        return cacheEntry;
    }

    public async Task RemoveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await database.KeyDeleteAsync(GetKey(sessionId));
        logger.LogDebug("Removed session {SessionId} from cache", sessionId);
    }

    private static string GetKey(Guid sessionId) => $"session:{sessionId}";
}
