using MediatR;
using NorthStarET.NextGen.Lms.Contracts.Audit;
using NorthStarET.NextGen.Lms.Domain.Auditing;

namespace NorthStarET.NextGen.Lms.Application.Audit.Queries;

/// <summary>
/// Handler for retrieving audit records with filtering and pagination
/// </summary>
public sealed class GetAuditRecordsQueryHandler : IRequestHandler<GetAuditRecordsQuery, PagedAuditRecordsResponse>
{
    private readonly IAuditRepository _auditRepository;

    public GetAuditRecordsQueryHandler(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public Task<PagedAuditRecordsResponse> Handle(GetAuditRecordsQuery request, CancellationToken cancellationToken)
    {
        // Validate page size
        var pageSize = Math.Min(request.PageSize, 100);
        if (pageSize < 1) pageSize = 20;

        var pageNumber = Math.Max(request.PageNumber, 1);

        // Build query with filters
        var query = _auditRepository.GetQueryable();

        if (request.DistrictId.HasValue)
        {
            query = query.Where(a => a.DistrictId == request.DistrictId.Value);
        }

        if (request.ActorId.HasValue)
        {
            query = query.Where(a => a.ActorId == request.ActorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(a => a.Action == request.Action);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }

        // Order by timestamp descending (most recent first)
        query = query.OrderByDescending(a => a.TimestampUtc);

        // Get total count before pagination
        var totalCount = query.Count();

        // Apply count limit if specified (overrides pagination)
        if (request.Count.HasValue && request.Count.Value > 0)
        {
            query = query.Take(request.Count.Value);
            pageSize = request.Count.Value;
            pageNumber = 1;
        }
        else
        {
            // Apply pagination
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        var auditRecords = query.ToList();

        var auditResponses = auditRecords.Select(MapToResponse).ToList();

        return Task.FromResult(new PagedAuditRecordsResponse
        {
            Records = auditResponses,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    private static AuditRecordResponse MapToResponse(AuditRecord auditRecord)
    {
        return new AuditRecordResponse
        {
            Id = auditRecord.Id,
            DistrictId = auditRecord.DistrictId,
            ActorId = auditRecord.ActorId,
            ActorRole = auditRecord.ActorRole.ToString(),
            Action = auditRecord.Action,
            EntityType = auditRecord.EntityType,
            EntityId = auditRecord.EntityId,
            BeforePayload = auditRecord.BeforePayload,
            AfterPayload = auditRecord.AfterPayload,
            CorrelationId = auditRecord.CorrelationId,
            TimestampUtc = auditRecord.TimestampUtc
        };
    }
}
