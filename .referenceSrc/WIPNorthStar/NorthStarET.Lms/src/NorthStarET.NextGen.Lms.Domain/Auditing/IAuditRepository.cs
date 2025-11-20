namespace NorthStarET.NextGen.Lms.Domain.Auditing;

/// <summary>
/// Repository interface for AuditRecord persistence.
/// All queries are tenant-scoped (filter by DistrictId).
/// </summary>
public interface IAuditRepository
{
    /// <summary>
    /// Retrieves an audit record by its unique identifier.
    /// </summary>
    /// <param name="id">Audit record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AuditRecord if found, null otherwise</returns>
    Task<AuditRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all audit records for a specific district, ordered by timestamp descending.
    /// </summary>
    /// <param name="districtId">District ID for tenant scoping</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit records</returns>
    Task<List<AuditRecord>> GetByDistrictIdAsync(Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit records for a specific entity within a district.
    /// </summary>
    /// <param name="districtId">District ID for tenant scoping</param>
    /// <param name="entityType">Type of entity (e.g., "District", "DistrictAdmin")</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit records ordered by timestamp descending</returns>
    Task<List<AuditRecord>> GetByEntityAsync(
        Guid districtId,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit records by correlation ID (all records from a single request) within a district.
    /// </summary>
    /// <param name="districtId">District ID for tenant scoping</param>
    /// <param name="correlationId">Request correlation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit records ordered by timestamp ascending</returns>
    Task<List<AuditRecord>> GetByCorrelationIdAsync(Guid districtId, Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit records performed by a specific actor within a district.
    /// </summary>
    /// <param name="districtId">District ID for tenant scoping</param>
    /// <param name="actorId">ID of the actor who performed the actions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit records ordered by timestamp descending</returns>
    Task<List<AuditRecord>> GetByActorAsync(Guid districtId, Guid actorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new audit record to the repository.
    /// </summary>
    /// <param name="auditRecord">Audit record to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(AuditRecord auditRecord, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple audit records to the repository in a single batch.
    /// </summary>
    /// <param name="auditRecords">Audit records to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddRangeAsync(IEnumerable<AuditRecord> auditRecords, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a queryable for audit records to enable flexible filtering and pagination.
    /// </summary>
    /// <returns>IQueryable of AuditRecord</returns>
    IQueryable<AuditRecord> GetQueryable();
}
