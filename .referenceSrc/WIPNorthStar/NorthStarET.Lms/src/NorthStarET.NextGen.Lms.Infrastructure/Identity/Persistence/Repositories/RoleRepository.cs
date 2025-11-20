using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Repositories;

internal sealed class RoleRepository : RepositoryBase<Role>, IRoleRepository
{
    public RoleRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await DbSet.SingleOrDefaultAsync(role => role.Name == roleName, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(role => role.IsSystemRole)
            .ToListAsync(cancellationToken);
    }
}
