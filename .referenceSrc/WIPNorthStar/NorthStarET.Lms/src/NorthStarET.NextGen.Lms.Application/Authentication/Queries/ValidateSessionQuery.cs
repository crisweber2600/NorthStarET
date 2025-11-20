using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Queries;

public sealed record ValidateSessionQuery(Guid SessionId) : IRequest<ValidateSessionResult>;

public sealed record ValidateSessionResult(
    Guid SessionId,
    Guid UserId,
    Guid ActiveTenantId,
    DateTimeOffset ExpiresAt,
    DateTimeOffset LastActivityAt);

internal sealed class ValidateSessionQueryHandler : IRequestHandler<ValidateSessionQuery, ValidateSessionResult>
{
    private readonly ILogger<ValidateSessionQueryHandler> logger;
    private readonly ISessionStore sessionStore;
    private readonly ISessionRepository sessionRepository;

    public ValidateSessionQueryHandler(
        ILogger<ValidateSessionQueryHandler> logger,
        ISessionStore sessionStore,
        ISessionRepository sessionRepository)
    {
        this.logger = logger;
        this.sessionStore = sessionStore;
        this.sessionRepository = sessionRepository;
    }

    public async Task<ValidateSessionResult> Handle(ValidateSessionQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await ResolveActiveSessionAsync(request.SessionId, cancellationToken);

        logger.LogDebug(
            "Validated session {SessionId} for user {UserId} (tenant {TenantId}) expiring at {ExpiresAt}.",
            snapshot.SessionId,
            snapshot.UserId,
            snapshot.ActiveTenantId,
            snapshot.ExpiresAt);

        return new ValidateSessionResult(
            snapshot.SessionId,
            snapshot.UserId,
            snapshot.ActiveTenantId,
            snapshot.ExpiresAt,
            snapshot.LastActivityAt);
    }

    private async Task<SessionSnapshot> ResolveActiveSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var cachedSession = await sessionStore.GetSessionAsync(sessionId, cancellationToken);

        if (cachedSession is not null)
        {
            if (cachedSession.IsRevoked || cachedSession.ExpiresAt <= now)
            {
                throw new InvalidOperationException("Session has expired or has been revoked.");
            }

            return new SessionSnapshot(
                cachedSession.SessionId,
                cachedSession.UserId,
                cachedSession.ActiveTenantId,
                cachedSession.ExpiresAt,
                cachedSession.LastActivityAt);
        }

        var session = await sessionRepository.GetActiveSessionAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Active session not found.");

        if (session.IsRevoked || session.ExpiresAt <= now)
        {
            throw new InvalidOperationException("Session has expired or has been revoked.");
        }

        await sessionStore.CacheSessionAsync(session, session.ExpiresAt - now, cancellationToken);

        return new SessionSnapshot(
            session.Id,
            session.UserId,
            session.ActiveTenantId.Value,
            session.ExpiresAt,
            session.LastActivityAt);
    }

    private sealed record SessionSnapshot(
        Guid SessionId,
        Guid UserId,
        Guid ActiveTenantId,
        DateTimeOffset ExpiresAt,
        DateTimeOffset LastActivityAt);
}
