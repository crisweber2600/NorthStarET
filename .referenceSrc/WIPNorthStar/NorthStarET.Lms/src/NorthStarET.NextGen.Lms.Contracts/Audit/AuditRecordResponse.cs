namespace NorthStarET.NextGen.Lms.Contracts.Audit;

/// <summary>
/// Audit record response for tenant-scoped operations.
/// </summary>
public sealed record AuditRecordResponse
{
    /// <summary>
    /// Audit record unique identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// District ID for tenant scoping.
    /// </summary>
    public Guid DistrictId { get; init; }

    /// <summary>
    /// ID of the user/service who performed the action.
    /// </summary>
    public Guid ActorId { get; init; }

    /// <summary>
    /// Role of the actor at the time of action (e.g., PlatformAdmin, DistrictAdmin).
    /// </summary>
    public string ActorRole { get; init; } = string.Empty;

    /// <summary>
    /// Action performed (e.g., "CreateDistrict", "UpdateAdmin").
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// Type of entity affected (e.g., "District", "DistrictAdmin").
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// ID of the affected entity (nullable for system-level actions).
    /// </summary>
    public Guid? EntityId { get; init; }

    /// <summary>
    /// JSON snapshot of entity state before the action (null for create operations).
    /// </summary>
    public string? BeforePayload { get; init; }

    /// <summary>
    /// JSON snapshot of entity state after the action (null for delete operations).
    /// </summary>
    public string? AfterPayload { get; init; }

    /// <summary>
    /// UTC timestamp when the action occurred.
    /// </summary>
    public DateTime TimestampUtc { get; init; }

    /// <summary>
    /// Correlation ID to group related audit records.
    /// </summary>
    public Guid CorrelationId { get; init; }
}
