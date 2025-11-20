namespace NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;

using System.ComponentModel.DataAnnotations;
public sealed record InviteDistrictAdminRequest
{
    /// <summary>
    /// The first name of the district admin to invite. Required. Maximum length: 100 characters.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// The last name of the district admin to invite. Required. Maximum length: 100 characters.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// The email address of the district admin to invite. Required. Must be a valid email address. Maximum length: 100 characters.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; init; } = string.Empty;
}

public sealed record InviteDistrictAdminResponse
{
    /// <summary>
    /// The unique identifier of the invited district admin.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The unique identifier of the district.
    /// </summary>
    public Guid DistrictId { get; init; }

    /// <summary>
    /// The email address of the invited district admin. Maximum length: 100 characters.
    /// </summary>
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The status of the invitation (e.g., Pending, Accepted, Expired).
    /// </summary>
    [MaxLength(50)]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The UTC date and time when the invitation was sent.
    /// </summary>
    public DateTime InvitedAtUtc { get; init; }

    /// <summary>
    /// The UTC date and time when the invitation expires.
    /// </summary>
    public DateTime InvitationExpiresAtUtc { get; init; }
}

public sealed record DistrictAdminResponse
{
    /// <summary>
    /// The unique identifier of the district admin.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The unique identifier of the district.
    /// </summary>
    public Guid DistrictId { get; init; }

    /// <summary>
    /// The email address of the district admin. Maximum length: 100 characters.
    /// </summary>
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The status of the district admin (e.g., Pending, Verified, Revoked, Expired).
    /// </summary>
    [MaxLength(50)]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The UTC date and time when the invitation was sent.
    /// </summary>
    public DateTime InvitedAtUtc { get; init; }

    /// <summary>
    /// The UTC date and time when the district admin verified their account, if applicable.
    /// </summary>
    public DateTime? VerifiedAtUtc { get; init; }

    /// <summary>
    /// The UTC date and time when the district admin's access was revoked, if applicable.
    /// </summary>
    public DateTime? RevokedAtUtc { get; init; }

    /// <summary>
    /// The UTC date and time when the invitation expires.
    /// </summary>
    public DateTime InvitationExpiresAtUtc { get; init; }

    /// <summary>
    /// Indicates whether the invitation has expired.
    /// </summary>
    public bool IsInvitationExpired { get; init; }
}
