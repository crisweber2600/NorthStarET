using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default);
}
