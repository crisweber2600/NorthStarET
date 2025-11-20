using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEntraSubjectIdAsync(EntraSubjectId subjectId, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<int> GetUserCountAsync(CancellationToken cancellationToken = default);
}
