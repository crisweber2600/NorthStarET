using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

/// <summary>
/// EF Core implementation of ISchoolRepository.
/// Provides tenant-scoped CRUD operations for School aggregates.
/// </summary>
public sealed class SchoolRepository : ISchoolRepository
{
    private readonly DistrictsDbContext _context;

    public SchoolRepository(DistrictsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<School?> GetByIdAsync(Guid schoolId, Guid districtId, CancellationToken cancellationToken = default)
    {
        return await _context.Schools
            .Where(s => s.Id == schoolId && s.DistrictId == districtId && s.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<School?> GetByIdWithGradesAsync(Guid schoolId, Guid districtId, CancellationToken cancellationToken = default)
    {
        return await _context.Schools
            .Include(s => s.GradeOfferings)
            .Where(s => s.Id == schoolId && s.DistrictId == districtId && s.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<School>> ListByDistrictAsync(
        Guid districtId,
        string? searchTerm = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Schools
            .AsNoTracking()
            .Include(s => s.GradeOfferings)
            .Where(s => s.DistrictId == districtId);

        if (!includeDeleted)
        {
            query = query.Where(s => s.DeletedAt == null);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim();
            var pattern = $"%{search}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, pattern) ||
                (s.Code != null && EF.Functions.ILike(s.Code, pattern)));
        }

        var schools = await query
            .ToListAsync(cancellationToken);

        return schools.AsReadOnly();
    }

    public async Task<bool> ExistsWithNameAsync(
        Guid districtId,
        string name,
        Guid? excludeSchoolId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Schools
            .Where(s => s.DistrictId == districtId &&
                       s.DeletedAt == null &&
                       s.Name.ToLower() == name.Trim().ToLower());

        if (excludeSchoolId.HasValue)
        {
            query = query.Where(s => s.Id != excludeSchoolId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithCodeAsync(
        Guid districtId,
        string code,
        Guid? excludeSchoolId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var query = _context.Schools
            .Where(s => s.DistrictId == districtId &&
                       s.DeletedAt == null &&
                       s.Code != null &&
                       s.Code.ToLower() == code.Trim().ToLower());

        if (excludeSchoolId.HasValue)
        {
            query = query.Where(s => s.Id != excludeSchoolId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(School school, CancellationToken cancellationToken = default)
    {
        await _context.Schools.AddAsync(school, cancellationToken);
    }

    public Task UpdateAsync(School school, CancellationToken cancellationToken = default)
    {
        _context.Schools.Update(school);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(School school, CancellationToken cancellationToken = default)
    {
        // Soft delete is handled by calling school.Delete() before this method
        _context.Schools.Update(school);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
