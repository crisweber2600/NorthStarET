using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Queries;

public sealed record GetCurrentUserContextQuery(Guid SessionId) : IRequest<GetCurrentUserContextResult>;

public sealed record GetCurrentUserContextResult(
    Guid SessionId,
    DateTimeOffset ExpiresAt,
    UserContextModel User,
    IReadOnlyCollection<TenantSummaryModel> AvailableTenants);

internal sealed class GetCurrentUserContextQueryHandler : IRequestHandler<GetCurrentUserContextQuery, GetCurrentUserContextResult>
{
    private readonly ILogger<GetCurrentUserContextQueryHandler> logger;
    private readonly ISessionStore sessionStore;
    private readonly ISessionRepository sessionRepository;
    private readonly IUserRepository userRepository;
    private readonly IMembershipRepository membershipRepository;
    private readonly ITenantRepository tenantRepository;
    private readonly IRoleRepository roleRepository;

    public GetCurrentUserContextQueryHandler(
        ILogger<GetCurrentUserContextQueryHandler> logger,
        ISessionStore sessionStore,
        ISessionRepository sessionRepository,
        IUserRepository userRepository,
        IMembershipRepository membershipRepository,
        ITenantRepository tenantRepository,
        IRoleRepository roleRepository)
    {
        this.logger = logger;
        this.sessionStore = sessionStore;
        this.sessionRepository = sessionRepository;
        this.userRepository = userRepository;
        this.membershipRepository = membershipRepository;
        this.tenantRepository = tenantRepository;
        this.roleRepository = roleRepository;
    }

    public async Task<GetCurrentUserContextResult> Handle(GetCurrentUserContextQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await ResolveActiveSessionAsync(request.SessionId, cancellationToken);

        var user = await userRepository.GetAsync(snapshot.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User {snapshot.UserId} not found for session {snapshot.SessionId}.");

        var memberships = await membershipRepository.GetActiveMembershipsForUserAsync(user.Id, cancellationToken);

        if (memberships.Count == 0)
        {
            logger.LogWarning("User {UserId} does not have active memberships when resolving session {SessionId}.", user.Id, snapshot.SessionId);
            throw new InvalidOperationException("Active membership required to resolve user context.");
        }

        var activeMembership = memberships.SingleOrDefault(membership => membership.TenantId.Value == snapshot.ActiveTenantId)
            ?? throw new InvalidOperationException("Active membership for session tenant not found.");

        var tenants = await tenantRepository.GetByIdsAsync(memberships.Select(membership => membership.TenantId).ToArray(), cancellationToken);

        var activeTenant = tenants.SingleOrDefault(tenant => tenant.Id == new TenantId(snapshot.ActiveTenantId))
            ?? throw new InvalidOperationException("Active tenant not found for session.");

        var role = await roleRepository.GetAsync(activeMembership.RoleId, cancellationToken)
            ?? throw new InvalidOperationException($"Role {activeMembership.RoleId} referenced by membership is missing.");

        var tenantSummaries = BuildTenantSummaries(memberships, tenants);

        var userContext = new UserContextModel(
            user.Id,
            user.Email,
            user.DisplayName,
            activeTenant.Id.Value,
            activeTenant.Name,
            activeTenant.Type.ToString(),
            role.DisplayName);

        return new GetCurrentUserContextResult(
            snapshot.SessionId,
            snapshot.ExpiresAt,
            userContext,
            tenantSummaries);
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
                cachedSession.ExpiresAt);
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
            session.ExpiresAt);
    }

    private static IReadOnlyCollection<TenantSummaryModel> BuildTenantSummaries(
        IReadOnlyCollection<Membership> memberships,
        IReadOnlyCollection<Tenant> tenants)
    {
        var tenantDictionary = tenants.ToDictionary(tenant => tenant.Id, tenant => tenant);
        var summaries = new List<TenantSummaryModel>(memberships.Count);

        foreach (var membership in memberships)
        {
            if (!tenantDictionary.TryGetValue(membership.TenantId, out var tenant))
            {
                continue;
            }

            summaries.Add(new TenantSummaryModel(tenant.Id.Value, tenant.Name, tenant.Type.ToString()));
        }

        return summaries;
    }

    private sealed record SessionSnapshot(
        Guid SessionId,
        Guid UserId,
        Guid ActiveTenantId,
        DateTimeOffset ExpiresAt);
}
