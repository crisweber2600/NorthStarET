using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;

namespace NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

/// <summary>
/// Entity Framework Core implementation of IDistrictAdminRepository.
/// All queries are tenant-scoped by DistrictId.
/// </summary>
internal sealed class DistrictAdminRepository : IDistrictAdminRepository
{
    private readonly DistrictsDbContext _context;

    public DistrictAdminRepository(DistrictsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<DistrictAdmin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DistrictAdmins
            .FirstOrDefaultAsync(da => da.Id == id, cancellationToken);
    }

    public async Task<DistrictAdmin?> GetByEmailAsync(Guid districtId, string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _context.DistrictAdmins
            .FirstOrDefaultAsync(
                da => da.DistrictId == districtId && da.Email == normalizedEmail,
                cancellationToken);
    }

    public async Task<List<DistrictAdmin>> GetByDistrictIdAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        return await _context.DistrictAdmins
            .Where(da => da.DistrictId == districtId)
            .OrderBy(da => da.Email)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DistrictAdmin>> GetActiveByDistrictIdAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        return await _context.DistrictAdmins
            .Where(da => da.DistrictId == districtId && da.Status == DistrictAdminStatus.Verified)
            .OrderBy(da => da.Email)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DistrictAdmin>> GetPendingByDistrictIdAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.DistrictAdmins
            .Where(da => da.DistrictId == districtId
                      && da.Status == DistrictAdminStatus.Unverified
                      && da.InvitedAtUtc > now.AddDays(-DistrictAdmin.InvitationExpiryDays)) // Not expired (optimized for index usage)
            .OrderBy(da => da.InvitedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(Guid districtId, string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _context.DistrictAdmins
            .AnyAsync(
                da => da.DistrictId == districtId && da.Email == normalizedEmail,
                cancellationToken);
    }

    public async Task AddAsync(DistrictAdmin admin, CancellationToken cancellationToken = default)
    {
        await _context.DistrictAdmins.AddAsync(admin, cancellationToken);
    }

    public Task UpdateAsync(DistrictAdmin admin, CancellationToken cancellationToken = default)
    {
        _context.DistrictAdmins.Update(admin);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(DistrictAdmin admin, CancellationToken cancellationToken = default)
    {
        _context.DistrictAdmins.Remove(admin);
        return Task.CompletedTask;
    }
}
