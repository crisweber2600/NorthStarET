namespace NorthStarET.NextGen.Lms.Contracts.Districts;

/// <summary>
/// Basic district response for list views.
/// </summary>
public sealed record DistrictResponse
{
    /// <summary>

    /// <summary>
    /// UTC timestamp when the district was soft-deleted (nullable).
    /// </summary>
    public DateTime? DeletedAt { get; init; }

    /// <summary>
    /// Count of active (verified) admins assigned to the district.
    /// </summary>
    public int ActiveAdminCount { get; init; }

    /// <summary>
    /// Count of pending (unverified) admin invitations for the district.
    /// </summary>
    public int PendingAdminCount { get; init; }

    /// <summary>
    /// Count of revoked admins for the district.
    /// </summary>
    public int RevokedAdminCount { get; init; }
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
    /// Indicates whether the district is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; init; }
}
