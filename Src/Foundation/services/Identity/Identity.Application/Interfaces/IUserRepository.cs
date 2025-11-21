using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Application.Interfaces;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task<User?> GetByExternalProviderAsync(string providerName, EntraSubjectId subjectId, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
