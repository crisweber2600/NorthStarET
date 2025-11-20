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

internal sealed class TenantRepository : RepositoryBase<Tenant>, ITenantRepository
{
    public TenantRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Tenant?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External id cannot be null or whitespace.", nameof(externalId));
        }

        return await DbSet
            .Include(tenant => tenant.Memberships)
            .AsSplitQuery()
            .SingleOrDefaultAsync(tenant => tenant.ExternalId == externalId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Tenant>> GetByIdsAsync(IEnumerable<TenantId> tenantIds, CancellationToken cancellationToken = default)
    {
        // Convert TenantId value objects to their underlying Guid values
        var idList = tenantIds?.Select(id => id.Value).ToList() ?? new List<Guid>();

        if (idList.Count == 0)
        {
            return Array.Empty<Tenant>();
        }

        // Fetch all tenants first, then filter in memory
        // This works around EF Core's limitation with value object comparisons
        var allTenants = await DbSet
            .Include(tenant => tenant.Memberships)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        // Filter in memory after materialization
        return allTenants
            .Where(tenant => idList.Contains(tenant.Id.Value))
            .ToList();
    }
}
