using Microsoft.EntityFrameworkCore;
using NorthStarET.Foundation.Identity.Application.Interfaces;
using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;
using NorthStarET.Foundation.Identity.Infrastructure.Data;

namespace NorthStarET.Foundation.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;
    
    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.ExternalProviders)
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);
    }
    
    public async Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.ExternalProviders)
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email && u.DeletedAt == null, cancellationToken);
    }
    
    public async Task<User?> GetByExternalProviderAsync(string providerName, EntraSubjectId subjectId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.ExternalProviders)
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.ExternalProviders.Any(p => p.ProviderName == providerName && p.SubjectId == subjectId) && u.DeletedAt == null, cancellationToken);
    }
    
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user;
    }
    
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
