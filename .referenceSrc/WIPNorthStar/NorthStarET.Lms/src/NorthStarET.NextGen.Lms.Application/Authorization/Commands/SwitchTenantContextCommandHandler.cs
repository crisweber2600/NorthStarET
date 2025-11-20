using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Common.Caching;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Commands;

public sealed class SwitchTenantContextCommandHandler : IRequestHandler<SwitchTenantContextCommand>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IAuthorizationCache _authorizationCache;
    private readonly ILogger<SwitchTenantContextCommandHandler> _logger;

    public SwitchTenantContextCommandHandler(
        ISessionRepository sessionRepository,
        IMembershipRepository membershipRepository,
        IAuthorizationCache authorizationCache,
        ILogger<SwitchTenantContextCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _membershipRepository = membershipRepository;
        _authorizationCache = authorizationCache;
        _logger = logger;
    }

    public async Task Handle(SwitchTenantContextCommand request, CancellationToken cancellationToken)
    {
        // Get the active session
        var session = await _sessionRepository.GetActiveSessionAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException($"Session {request.SessionId} not found or is no longer active.");
        }

        // Verify the session belongs to the requesting user
        if (session.UserId != request.UserId)
        {
            throw new InvalidOperationException("Session does not belong to the requesting user.");
        }

        // Verify user has membership in the target tenant
        var targetTenantId = new TenantId(request.TargetTenantId);
        var membership = await _membershipRepository.GetByUserAndTenantAsync(request.UserId, targetTenantId, cancellationToken);
        if (membership is null)
        {
            throw new InvalidOperationException($"User {request.UserId} does not have membership in tenant {request.TargetTenantId}.");
        }

        // Store previous tenant for cache invalidation
        var previousTenantId = session.ActiveTenantId.Value;

        // Switch the tenant context
        session.SwitchTenant(targetTenantId);

        // Update session activity timestamp
        session.Touch(DateTimeOffset.UtcNow);

        // Persist the updated session
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        // Clear authorization cache for the previous tenant
        await _authorizationCache.ClearForUserAndTenantAsync(request.UserId, previousTenantId, cancellationToken);

        _logger.LogInformation(
            "User {UserId} switched tenant context from {PreviousTenantId} to {NewTenantId} in session {SessionId}",
            request.UserId,
            previousTenantId,
            request.TargetTenantId,
            request.SessionId);
    }
}
