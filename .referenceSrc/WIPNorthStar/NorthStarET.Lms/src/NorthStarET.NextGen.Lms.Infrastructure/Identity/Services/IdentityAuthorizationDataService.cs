using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;
using NorthStarET.NextGen.Lms.Application.Authorization.Services;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Services;

internal sealed class IdentityAuthorizationDataService : IIdentityAuthorizationDataService
{
    private readonly IdentityDbContext dbContext;
    private readonly ILogger<IdentityAuthorizationDataService> logger;

    public IdentityAuthorizationDataService(IdentityDbContext dbContext, ILogger<IdentityAuthorizationDataService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task<AuthorizationDecision> FetchDecisionAsync(
        Guid userId,
        Guid tenantId,
        string resource,
        string action,
        IReadOnlyDictionary<string, string>? context,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return BuildDeniedDecision(userId, tenantId, resource, action, "User is inactive or does not exist.", now);
        }

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == new TenantId(tenantId), cancellationToken);

        if (tenant is null || !tenant.IsActive)
        {
            return BuildDeniedDecision(userId, tenantId, resource, action, "Tenant is inactive or does not exist.", now);
        }

        var membership = await dbContext.Memberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId && m.TenantId == new TenantId(tenantId), cancellationToken);

        if (membership is null)
        {
            return BuildDeniedDecision(userId, tenantId, resource, action, "User does not have membership for the tenant.", now);
        }

        if (!membership.IsActive)
        {
            return BuildDeniedDecision(userId, tenantId, resource, action, "Membership is inactive.", now);
        }

        if (membership.ExpiresAt.HasValue && membership.ExpiresAt <= now)
        {
            return BuildDeniedDecision(userId, tenantId, resource, action, "Membership has expired.", now);
        }

        var role = await dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == membership.RoleId, cancellationToken);

        if (role is null)
        {
            return BuildDeniedDecision(userId, tenantId, resource, action, "Membership role could not be resolved.", now);
        }

        var allowed = role.HasPermission(resource, action);
        var reason = allowed ? null : $"Role '{role.DisplayName}' does not grant {action} on {resource}.";

        logger.LogDebug(
            "Authorization evaluation completed for user {UserId} tenant {TenantId} resource {Resource} action {Action}. Allowed={Allowed}.",
            userId,
            tenantId,
            resource,
            action,
            allowed);

        return new AuthorizationDecision(
            userId,
            tenantId,
            resource,
            action,
            allowed,
            allowed ? role.Id : (Guid?)null,
            allowed ? role.DisplayName : null,
            reason,
            now);
    }

    public async Task<GetUserTenantsResult> GetUserTenantsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var memberships = await dbContext.Memberships
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.IsActive && (!m.ExpiresAt.HasValue || m.ExpiresAt > now))
            .Join(
                dbContext.Tenants.AsNoTracking(),
                membership => membership.TenantId,
                tenant => tenant.Id,
                (membership, tenant) => new { membership, tenant })
            .Join(
                dbContext.Roles.AsNoTracking(),
                combined => combined.membership.RoleId,
                role => role.Id,
                (combined, role) => new { combined.membership, combined.tenant, role })
            .OrderBy(result => result.tenant.Name)
            .ToListAsync(cancellationToken);

        var tenantMemberships = memberships
            .Select(result => new UserTenantMembership(
                result.tenant.Id,
                result.tenant.Name,
                result.tenant.Type,
                result.tenant.ParentTenantId,
                result.membership.RoleId,
                result.role.DisplayName,
                result.membership.GrantedAt,
                result.membership.ExpiresAt))
            .ToArray();

        return new GetUserTenantsResult(userId, tenantMemberships);
    }

    private static AuthorizationDecision BuildDeniedDecision(
        Guid userId,
        Guid tenantId,
        string resource,
        string action,
        string reason,
        DateTimeOffset evaluatedAt)
    {
        return new AuthorizationDecision(
            userId,
            tenantId,
            resource,
            action,
            allowed: false,
            roleId: null,
            roleName: null,
            reason,
            evaluatedAt);
    }
}
