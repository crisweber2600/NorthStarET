using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Repositories;

internal sealed class MembershipRepository : RepositoryBase<Membership>, IMembershipRepository
{
    public MembershipRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Membership?> GetByUserAndTenantAsync(Guid userId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(membership => membership.UserId == userId && membership.TenantId == tenantId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Membership>> GetActiveMembershipsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await DbSet
            .Where(membership => membership.UserId == userId && membership.IsActive && (!membership.ExpiresAt.HasValue || membership.ExpiresAt > now))
            .ToListAsync(cancellationToken);
    }
}
