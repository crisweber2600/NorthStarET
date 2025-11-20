using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;

namespace NorthStarET.NextGen.Lms.Domain.Auditing;

/// <summary>
/// Immutable audit log record for tenant-scoped operations.
/// Captures before/after state changes with full actor context.
/// </summary>
public sealed class AuditRecord
{
    private AuditRecord(
        Guid id,
        Guid districtId,
        Guid actorId,
        ActorRole actorRole,
        string action,
        string entityType,
        Guid? entityId,
        string? beforePayload,
        string? afterPayload,
        DateTime timestampUtc,
        Guid correlationId)
    {
        Id = id;
        DistrictId = districtId;
        ActorId = actorId;
        ActorRole = actorRole;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        BeforePayload = beforePayload;
        AfterPayload = afterPayload;
        TimestampUtc = timestampUtc;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Unique identifier for this audit record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// District ID for tenant isolation (scopes audit to specific district).
    /// </summary>
    public Guid DistrictId { get; private set; }

    /// <summary>
    /// ID of the user/service who performed the action.
    /// </summary>
    public Guid ActorId { get; private set; }

    /// <summary>
    /// Role of the actor at the time of action (e.g., PlatformAdmin, DistrictAdmin).
    /// </summary>
    public ActorRole ActorRole { get; private set; }

    /// <summary>
    /// Action performed (e.g., "CreateDistrict", "UpdateAdmin", "DeleteDistrict").
    /// </summary>
    public string Action { get; private set; }

    /// <summary>
    /// Type of entity affected (e.g., "District", "DistrictAdmin").
    /// </summary>
    public string EntityType { get; private set; }

    /// <summary>
    /// ID of the affected entity (nullable for system-level actions).
    /// </summary>
    public Guid? EntityId { get; private set; }

    /// <summary>
    /// JSON snapshot of entity state before the action (null for create operations).
    /// </summary>
    public string? BeforePayload { get; private set; }

    /// <summary>
    /// JSON snapshot of entity state after the action (null for delete operations).
    /// </summary>
    public string? AfterPayload { get; private set; }

    /// <summary>
    /// UTC timestamp when the action occurred.
    /// </summary>
    public DateTime TimestampUtc { get; private set; }

    /// <summary>
    /// Correlation ID to group related audit records (e.g., all records from one HTTP request).
    /// </summary>
    public Guid CorrelationId { get; private set; }

    /// <summary>
    /// Factory method to create a new audit record.
    /// </summary>
    /// <param name="id">Unique audit record ID</param>
    /// <param name="districtId">District ID for tenant scoping</param>
    /// <param name="actorId">ID of the user/service performing the action</param>
    /// <param name="actorRole">Role of the actor</param>
    /// <param name="action">Action performed</param>
    /// <param name="entityType">Type of entity affected</param>
    /// <param name="entityId">ID of the affected entity (optional)</param>
    /// <param name="beforePayload">JSON before-state (optional)</param>
    /// <param name="afterPayload">JSON after-state (optional)</param>
    /// <param name="correlationId">Request correlation ID</param>
    /// <param name="dateTimeProvider">Date time provider for current time</param>
    /// <returns>New AuditRecord instance</returns>
    public static AuditRecord Create(
        Guid id,
        Guid districtId,
        Guid actorId,
        ActorRole actorRole,
        string action,
        string entityType,
        Guid? entityId,
        string? beforePayload,
        string? afterPayload,
        Guid correlationId,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action, nameof(action));
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType, nameof(entityType));

        return new AuditRecord(
            id,
            districtId,
            actorId,
            actorRole,
            action,
            entityType,
            entityId,
            beforePayload,
            afterPayload,
            dateTimeProvider.UtcNow,
            correlationId
        );
    }

    /// <summary>
    /// Creates an audit record for entity creation (no before-state).
    /// </summary>
    public static AuditRecord CreateForCreation(
        Guid id,
        Guid districtId,
        Guid actorId,
        ActorRole actorRole,
        string entityType,
        Guid entityId,
        string afterPayload,
        Guid correlationId,
        IDateTimeProvider dateTimeProvider)
    {
        return Create(
            id,
            districtId,
            actorId,
            actorRole,
            $"Create{entityType}",
            entityType,
            entityId,
            null,
            afterPayload,
            correlationId,
            dateTimeProvider
        );
    }

    /// <summary>
    /// Creates an audit record for entity update (before and after states).
    /// </summary>
    public static AuditRecord CreateForUpdate(
        Guid id,
        Guid districtId,
        Guid actorId,
        ActorRole actorRole,
        string entityType,
        Guid entityId,
        string beforePayload,
        string afterPayload,
        Guid correlationId,
        IDateTimeProvider dateTimeProvider)
    {
        return Create(
            id,
            districtId,
            actorId,
            actorRole,
            $"Update{entityType}",
            entityType,
            entityId,
            beforePayload,
            afterPayload,
            correlationId,
            dateTimeProvider
        );
    }

    /// <summary>
    /// Creates an audit record for entity deletion (no after-state).
    /// </summary>
    public static AuditRecord CreateForDeletion(
        Guid id,
        Guid districtId,
        Guid actorId,
        ActorRole actorRole,
        string entityType,
        Guid entityId,
        string beforePayload,
        Guid correlationId,
        IDateTimeProvider dateTimeProvider)
    {
        return Create(
            id,
            districtId,
            actorId,
            actorRole,
            $"Delete{entityType}",
            entityType,
            entityId,
            beforePayload,
            null,
            correlationId,
            dateTimeProvider
        );
    }
}

/// <summary>
/// Actor role enumeration for audit trail.
/// </summary>
public enum ActorRole
{
    /// <summary>
    /// System-level administrator with platform-wide access.
    /// </summary>
    PlatformAdmin = 0,

    /// <summary>
    /// District-level administrator with tenant-scoped access.
    /// </summary>
    DistrictAdmin = 1,

    /// <summary>
    /// System service performing background operations.
    /// </summary>
    SystemService = 2
}
