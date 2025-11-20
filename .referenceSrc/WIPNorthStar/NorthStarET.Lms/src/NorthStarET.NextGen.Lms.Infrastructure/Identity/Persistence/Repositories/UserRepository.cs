using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Repositories;

internal sealed class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<User?> GetByEntraSubjectIdAsync(EntraSubjectId subjectId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(user => user.Memberships)
            .Include(user => user.Sessions)
            .AsSplitQuery()
            .SingleOrDefaultAsync(user => user.EntraSubjectId == subjectId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(user => user.Memberships)
            .Include(user => user.Sessions)
            .AsSplitQuery()
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public async Task<int> GetUserCountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }
}
