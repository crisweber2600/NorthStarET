using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Commands;

public sealed class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionStore _sessionStore;
    private readonly ILogger<RefreshSessionCommandHandler> _logger;
    private readonly IdentityModuleSettings _settings;

    public RefreshSessionCommandHandler(
        ISessionRepository sessionRepository,
        ISessionStore sessionStore,
        ILogger<RefreshSessionCommandHandler> logger,
        IOptions<IdentityModuleSettings> settings)
    {
        _sessionRepository = sessionRepository;
        _sessionStore = sessionStore;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetActiveSessionAsync(request.SessionId, cancellationToken);

        if (session is null)
        {
            throw new InvalidOperationException($"Session {request.SessionId} not found or is no longer active.");
        }

        // Calculate new expiration time using configured session expiration
        var now = DateTimeOffset.UtcNow;
        var newExpiration = now.AddMinutes(_settings.SessionSlidingExpirationMinutes);

        // Refresh the session with new token and expiration
        session.Refresh(request.NewToken, newExpiration, now);

        // Update in repository
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        // Update cache with extended TTL
        var ttl = newExpiration - now;
        await _sessionStore.CacheSessionAsync(session, ttl, cancellationToken);

        _logger.LogInformation(
            "Session {SessionId} refreshed successfully. New expiration: {ExpiresAt}",
            session.Id,
            newExpiration);
    }
}
