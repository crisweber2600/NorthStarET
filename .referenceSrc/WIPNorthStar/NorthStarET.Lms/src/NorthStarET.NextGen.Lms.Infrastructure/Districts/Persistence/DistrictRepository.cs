using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Districts;

namespace NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

/// <summary>
/// Entity Framework Core implementation of IDistrictRepository.
/// Applies global query filter to exclude soft-deleted districts.
/// </summary>
internal sealed class DistrictRepository : IDistrictRepository
{
    private readonly DistrictsDbContext _context;

    public DistrictRepository(DistrictsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<District?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Districts
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<District?> GetBySuffixAsync(string suffix, CancellationToken cancellationToken = default)
    {
        var normalizedSuffix = suffix.ToLowerInvariant();
        return await _context.Districts
            .FirstOrDefaultAsync(d => d.Suffix == normalizedSuffix, cancellationToken);
    }

    public async Task<List<District>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        // Global query filter already excludes soft-deleted districts
        return await _context.Districts
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SuffixExistsAsync(string suffix, CancellationToken cancellationToken = default)
    {
        var normalizedSuffix = suffix.ToLowerInvariant();
        return await _context.Districts
            .AnyAsync(d => d.Suffix == normalizedSuffix, cancellationToken);
    }

    public async Task<bool> IsSuffixUniqueAsync(string suffix, CancellationToken cancellationToken = default)
    {
        var normalizedSuffix = suffix.ToLowerInvariant();
        return !await _context.Districts
            .AnyAsync(d => d.Suffix == normalizedSuffix, cancellationToken);
    }

    public async Task<bool> IsSuffixUniqueAsync(string suffix, Guid excludeDistrictId, CancellationToken cancellationToken = default)
    {
        var normalizedSuffix = suffix.ToLowerInvariant();
        return !await _context.Districts
            .AnyAsync(d => d.Suffix == normalizedSuffix && d.Id != excludeDistrictId, cancellationToken);
    }

    public async Task<List<District>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Districts
            .OrderBy(d => d.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Districts.CountAsync(cancellationToken);
    }

    public async Task<int> GetActiveAdminCountAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        // Placeholder - will be implemented in Phase 4 (User Story 2)
        return await Task.FromResult(0);
    }

    public async Task<int> GetPendingAdminCountAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        // Placeholder - will be implemented in Phase 4 (User Story 2)
        return await Task.FromResult(0);
    }

    public async Task<int> GetRevokedAdminCountAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        // Placeholder - will be implemented in Phase 4 (User Story 2)
        return await Task.FromResult(0);
    }

    public async Task AddAsync(District district, CancellationToken cancellationToken = default)
    {
        await _context.Districts.AddAsync(district, cancellationToken);
    }

    public Task UpdateAsync(District district, CancellationToken cancellationToken = default)
    {
        _context.Districts.Update(district);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(District district, CancellationToken cancellationToken = default)
    {
        _context.Districts.Remove(district);
        return Task.CompletedTask;
    }
}
