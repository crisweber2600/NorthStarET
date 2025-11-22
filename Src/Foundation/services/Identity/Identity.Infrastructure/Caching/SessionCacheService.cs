using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Infrastructure.Caching;

/// <summary>
/// Redis-based session caching service
/// </summary>
public class SessionCacheService : ISessionCacheService
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public SessionCacheService(IDistributedCache cache)
    {
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }
    
    public async Task<Session?> GetSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default)
    {
        var key = SessionCacheKeyBuilder.ForSession(sessionId);
        var cachedData = await _cache.GetStringAsync(key, cancellationToken);
        
        if (string.IsNullOrEmpty(cachedData))
            return null;
        
        return JsonSerializer.Deserialize<Session>(cachedData, _jsonOptions);
    }
    
    public async Task SetSessionAsync(Session session, CancellationToken cancellationToken = default)
    {
        var key = SessionCacheKeyBuilder.ForSession(session.SessionId);
        var serialized = JsonSerializer.Serialize(session, _jsonOptions);
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = session.ExpiresAt
        };
        
        await _cache.SetStringAsync(key, serialized, options, cancellationToken);
    }
    
    public async Task RemoveSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default)
    {
        var key = SessionCacheKeyBuilder.ForSession(sessionId);
        await _cache.RemoveAsync(key, cancellationToken);
    }
    
    public async Task<string?> GetUserClaimsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var key = SessionCacheKeyBuilder.ForUserClaims(userId, tenantId);
        return await _cache.GetStringAsync(key, cancellationToken);
    }
    
    public async Task SetUserClaimsAsync(Guid userId, Guid tenantId, string claimsJson, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var key = SessionCacheKeyBuilder.ForUserClaims(userId, tenantId);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        
        await _cache.SetStringAsync(key, claimsJson, options, cancellationToken);
    }
    
    public async Task RemoveUserClaimsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var key = SessionCacheKeyBuilder.ForUserClaims(userId, tenantId);
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
