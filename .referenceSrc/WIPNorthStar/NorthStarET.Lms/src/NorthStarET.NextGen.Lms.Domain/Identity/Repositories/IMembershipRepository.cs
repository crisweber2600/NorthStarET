using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

public interface IMembershipRepository : IRepository<Membership>
{
    Task<Membership?> GetByUserAndTenantAsync(Guid userId, TenantId tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Membership>> GetActiveMembershipsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
