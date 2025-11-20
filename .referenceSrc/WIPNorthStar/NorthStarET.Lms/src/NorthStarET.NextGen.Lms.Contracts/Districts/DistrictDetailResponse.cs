namespace NorthStarET.NextGen.Lms.Contracts.Districts;

/// <summary>
/// Detailed district response including admin count and audit trail.
/// </summary>
public sealed record DistrictDetailResponse
{
    /// <summary>
    /// District unique identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// District display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Unique email domain suffix.
    /// </summary>
    public string Suffix { get; init; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the district was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when the district was last updated (nullable).
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>
    /// UTC timestamp when the district was soft-deleted (nullable).
    /// </summary>
    public DateTime? DeletedAt { get; init; }

    /// <summary>
    /// Count of active (verified) district admins.
    /// </summary>
    public int ActiveAdminCount { get; init; }

    /// <summary>
    /// Count of pending (unverified) district admin invitations.
    /// </summary>
    public int PendingAdminCount { get; init; }

    /// <summary>
    /// Count of revoked district admins.
    /// </summary>
    public int RevokedAdminCount { get; init; }
}
