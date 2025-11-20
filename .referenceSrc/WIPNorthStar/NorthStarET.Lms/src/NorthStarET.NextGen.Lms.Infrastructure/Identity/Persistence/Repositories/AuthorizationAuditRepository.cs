using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Repositories;

internal sealed class AuthorizationAuditRepository : RepositoryBase<AuthorizationAuditLog>, IAuthorizationAuditRepository
{
    public AuthorizationAuditRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public override Task AddAsync(AuthorizationAuditLog aggregate, CancellationToken cancellationToken = default) =>
        base.AddAsync(aggregate, cancellationToken);
}
