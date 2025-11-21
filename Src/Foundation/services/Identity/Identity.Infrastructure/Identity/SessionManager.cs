using Microsoft.Extensions.Configuration;
using NorthStarET.Foundation.Identity.Application.Interfaces;
using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;
using NorthStarET.Foundation.Identity.Infrastructure.Caching;
using NorthStarET.Foundation.Identity.Infrastructure.Repositories;

namespace NorthStarET.Foundation.Identity.Infrastructure.Identity;

public class SessionManager : ISessionManager
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionCacheService _cacheService;
    private readonly IConfiguration _configuration;
    
    public SessionManager(
        ISessionRepository sessionRepository,
        ISessionCacheService cacheService,
        IConfiguration configuration)
    {
        _sessionRepository = sessionRepository;
        _cacheService = cacheService;
        _configuration = configuration;
    }
    
    public async Task<Session> CreateSessionAsync(
        Guid tenantId,
        Guid userId,
        string userPrincipalName,
        bool isAdmin,
        string? ipAddress = null,
        string? userAgent = null,
        string? claimsJson = null,
        CancellationToken cancellationToken = default)
    {
        var sessionHours = isAdmin
            ? _configuration.GetValue("SessionSettings:AdminSessionDurationHours", 1)
            : _configuration.GetValue("SessionSettings:StaffSessionDurationHours", 8);
        
        var duration = TimeSpan.FromHours(sessionHours);
        
        var session = new Session(
            tenantId: tenantId,
            userId: userId,
            userPrincipalName: userPrincipalName,
            duration: duration,
            ipAddress: ipAddress,
            userAgent: userAgent,
            claimsJson: claimsJson
        );
        
        await _sessionRepository.AddAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.SetSessionAsync(session, cancellationToken);
        
        return session;
    }
    
    public async Task<Session?> GetSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default)
    {
        var cachedSession = await _cacheService.GetSessionAsync(sessionId, cancellationToken);
        if (cachedSession != null && cachedSession.IsValid())
        {
            return cachedSession;
        }
        
        var session = await _sessionRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (session != null && session.IsValid())
        {
            await _cacheService.SetSessionAsync(session, cancellationToken);
            return session;
        }
        
        return null;
    }
    
    public async Task<Session> RefreshSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }
        
        var sessionHours = 8; // Default, should check user role
        var duration = TimeSpan.FromHours(sessionHours);
        session.Refresh(duration);
        
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.SetSessionAsync(session, cancellationToken);
        
        return session;
    }
    
    public async Task RevokeSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default)
    {
        await _sessionRepository.RevokeAsync(sessionId, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveSessionAsync(sessionId, cancellationToken);
    }
}
