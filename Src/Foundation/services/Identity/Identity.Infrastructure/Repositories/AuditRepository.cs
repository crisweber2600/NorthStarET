using NorthStarET.Foundation.Identity.Application.Interfaces;
using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Infrastructure.Data;

namespace NorthStarET.Foundation.Identity.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly IdentityDbContext _context;
    
    public AuditRepository(IdentityDbContext context)
    {
        _context = context;
    }
    
    public async Task LogAuthenticationAsync(
        Guid tenantId,
        Guid? userId,
        Guid? sessionId,
        string eventType,
        bool isSuccess,
        string? eventData = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var auditRecord = new AuditRecord(
            tenantId: tenantId,
            eventType: eventType,
            isSuccess: isSuccess,
            userId: userId,
            sessionId: sessionId,
            eventData: eventData,
            ipAddress: ipAddress,
            userAgent: userAgent,
            errorMessage: errorMessage
        );
        
        await _context.AuditRecords.AddAsync(auditRecord, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
