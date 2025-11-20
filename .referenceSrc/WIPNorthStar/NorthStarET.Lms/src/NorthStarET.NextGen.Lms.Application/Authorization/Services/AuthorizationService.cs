using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;
using NorthStarET.NextGen.Lms.Application.Common.Caching;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Services;

public sealed class AuthorizationService : IAuthorizationService
{
    private static readonly Meter Meter = new("NorthStarET.NextGen.Lms.Authorization", "1.0.0");
    private static readonly Histogram<double> AuthorizationLatency = Meter.CreateHistogram<double>(
        "authorization.decision.latency",
        unit: "ms",
        description: "Authorization decision latency in milliseconds");
    private static readonly Counter<long> AuthorizationCacheHits = Meter.CreateCounter<long>(
        "authorization.cache.hits",
        unit: "{hit}",
        description: "Number of authorization cache hits");
    private static readonly Counter<long> AuthorizationCacheMisses = Meter.CreateCounter<long>(
        "authorization.cache.misses",
        unit: "{miss}",
        description: "Number of authorization cache misses");
    private static readonly Counter<long> AuthorizationDecisions = Meter.CreateCounter<long>(
        "authorization.decisions",
        unit: "{decision}",
        description: "Total number of authorization decisions");

    private readonly ILogger<AuthorizationService> logger;
    private readonly IIdentityAuthorizationDataService dataService;
    private readonly IAuthorizationCache cache;
    private readonly IAuthorizationAuditRepository auditRepository;
    private readonly IdentityModuleSettings settings;

    public AuthorizationService(
        ILogger<AuthorizationService> logger,
        IIdentityAuthorizationDataService dataService,
        IAuthorizationCache cache,
        IAuthorizationAuditRepository auditRepository,
        IOptions<IdentityModuleSettings> options)
    {
        this.logger = logger;
        this.dataService = dataService;
        this.cache = cache;
        this.auditRepository = auditRepository;
        settings = options.Value;
    }

    public async Task<AuthorizationDecision> CheckPermissionAsync(
        Guid userId,
        Guid tenantId,
        string resource,
        string action,
        IReadOnlyDictionary<string, string>? context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startTime = Stopwatch.GetTimestamp();
        AuthorizationDecision decision;
        bool cacheHit = false;

        try
        {
            var cachedDecision = await cache
                .GetAsync(userId, tenantId, resource, action, cancellationToken)
                .ConfigureAwait(false);

            if (cachedDecision is not null)
            {
                logger.LogDebug("Authorization decision cache hit for user {UserId} tenant {TenantId} resource {Resource} action {Action}.", userId, tenantId, resource, action);
                cacheHit = true;
                AuthorizationCacheHits.Add(1, new KeyValuePair<string, object?>("resource", resource), new KeyValuePair<string, object?>("action", action));
                decision = cachedDecision;
            }
            else
            {
                logger.LogDebug("Authorization decision cache miss for user {UserId} tenant {TenantId} resource {Resource} action {Action}; fetching from identity service.", userId, tenantId, resource, action);
                AuthorizationCacheMisses.Add(1, new KeyValuePair<string, object?>("resource", resource), new KeyValuePair<string, object?>("action", action));

                decision = await dataService
                    .FetchDecisionAsync(userId, tenantId, resource, action, context, cancellationToken)
                    .ConfigureAwait(false);

                var ttlMinutes = Math.Max(settings.AuthorizationCacheTtlMinutes, 1);
                var ttl = TimeSpan.FromMinutes(ttlMinutes);

                try
                {
                    await cache.SetAsync(decision, ttl, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to cache authorization decision for user {UserId} tenant {TenantId} resource {Resource} action {Action}.", userId, tenantId, resource, action);
                }

                await PersistAuditRecordAsync(decision, cancellationToken).ConfigureAwait(false);
            }

            // Record decision metrics
            AuthorizationDecisions.Add(1,
                new KeyValuePair<string, object?>("resource", resource),
                new KeyValuePair<string, object?>("action", action),
                new KeyValuePair<string, object?>("allowed", decision.Allowed),
                new KeyValuePair<string, object?>("cache_hit", cacheHit));

            return decision;
        }
        finally
        {
            // Record latency
            var elapsed = Stopwatch.GetElapsedTime(startTime);
            AuthorizationLatency.Record(elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("resource", resource),
                new KeyValuePair<string, object?>("action", action),
                new KeyValuePair<string, object?>("cache_hit", cacheHit));
        }
    }

    private async Task PersistAuditRecordAsync(AuthorizationDecision decision, CancellationToken cancellationToken)
    {
        try
        {
            var entry = new AuthorizationAuditLog(
                Guid.NewGuid(),
                decision.UserId,
                new TenantId(decision.TenantId),
                decision.Resource,
                decision.Action,
                decision.Allowed,
                decision.CheckedAt,
                decision.RoleId,
                decision.RoleName,
                decision.Reason,
                Activity.Current?.Id);

            await auditRepository.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist authorization audit log for user {UserId} tenant {TenantId} resource {Resource} action {Action}.",
                decision.UserId,
                decision.TenantId,
                decision.Resource,
                decision.Action);
        }
    }
}
