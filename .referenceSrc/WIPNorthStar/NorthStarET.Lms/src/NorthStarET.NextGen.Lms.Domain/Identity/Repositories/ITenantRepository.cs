using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Tenant>> GetByIdsAsync(IEnumerable<TenantId> tenantIds, CancellationToken cancellationToken = default);
}
