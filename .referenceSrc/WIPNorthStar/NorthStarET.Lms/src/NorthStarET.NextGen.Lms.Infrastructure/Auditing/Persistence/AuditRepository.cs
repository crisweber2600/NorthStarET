using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Auditing;
using NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

namespace NorthStarET.NextGen.Lms.Infrastructure.Auditing.Persistence;

/// <summary>
/// Entity Framework Core implementation of IAuditRepository.
/// All queries are tenant-scoped by DistrictId.
/// </summary>
internal sealed class AuditRepository : IAuditRepository
{
    private readonly DistrictsDbContext _context;

    public AuditRepository(DistrictsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AuditRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditRecords
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);
    }

    public async Task<List<AuditRecord>> GetByDistrictIdAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditRecords
            .Where(ar => ar.DistrictId == districtId)
            .OrderByDescending(ar => ar.TimestampUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditRecord>> GetByEntityAsync(
        Guid districtId,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditRecords
            .Where(ar => ar.DistrictId == districtId
                      && ar.EntityType == entityType
                      && ar.EntityId == entityId)
            .OrderByDescending(ar => ar.TimestampUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditRecord>> GetByCorrelationIdAsync(Guid districtId, Guid correlationId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditRecords
            .Where(ar => ar.DistrictId == districtId && ar.CorrelationId == correlationId)
            .OrderBy(ar => ar.TimestampUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditRecord>> GetByActorAsync(Guid districtId, Guid actorId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditRecords
            .Where(ar => ar.DistrictId == districtId && ar.ActorId == actorId)
            .OrderByDescending(ar => ar.TimestampUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AuditRecord auditRecord, CancellationToken cancellationToken = default)
    {
        await _context.AuditRecords.AddAsync(auditRecord, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<AuditRecord> auditRecords, CancellationToken cancellationToken = default)
    {
        await _context.AuditRecords.AddRangeAsync(auditRecords, cancellationToken);
    }

    public IQueryable<AuditRecord> GetQueryable()
    {
        return _context.AuditRecords.AsQueryable();
    }
}
