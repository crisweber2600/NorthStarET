using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Commands;

public sealed class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionStore _sessionStore;
    private readonly ILogger<RevokeSessionCommandHandler> _logger;

    public RevokeSessionCommandHandler(
        ISessionRepository sessionRepository,
        ISessionStore sessionStore,
        ILogger<RevokeSessionCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public async Task Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetActiveSessionAsync(request.SessionId, cancellationToken);

        if (session is null)
        {
            throw new InvalidOperationException($"Session {request.SessionId} not found or is already revoked.");
        }

        // Mark session as revoked in domain
        session.Revoke();

        // Update in repository
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        // Remove from Redis cache
        await _sessionStore.RemoveSessionAsync(session.Id, cancellationToken);

        _logger.LogInformation(
            "Session {SessionId} revoked successfully for user {UserId}",
            session.Id,
            session.UserId);
    }
}
