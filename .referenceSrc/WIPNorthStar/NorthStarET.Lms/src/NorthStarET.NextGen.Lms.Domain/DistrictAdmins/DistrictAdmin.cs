using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;

namespace NorthStarET.NextGen.Lms.Domain.DistrictAdmins;

/// <summary>
/// District admin aggregate representing authorized personnel for a district tenant.
/// Implements invitation workflow with 7-day expiry and revocation capabilities.
/// </summary>
public sealed class DistrictAdmin : AggregateRoot
{
    /// <summary>
    /// Number of days before an invitation expires.
    /// </summary>
    public const int InvitationExpiryDays = 7;

    private DistrictAdmin(
        Guid id,
        Guid districtId,
        string email,
        DistrictAdminStatus status,
        DateTime invitedAtUtc)
        : base(id)
    {
        DistrictId = districtId;
        Email = email;
        Status = status;
        InvitedAtUtc = invitedAtUtc;
    }

    /// <summary>
    /// Foreign key to the owning District (tenant isolation).
    /// </summary>
    public Guid DistrictId { get; private set; }

    /// <summary>
    /// Admin email address (case-insensitive, unique per district).
    /// Must match district's email suffix pattern.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Current status: Unverified (pending), Verified (active), or Revoked (inactive).
    /// </summary>
    public DistrictAdminStatus Status { get; private set; }

    /// <summary>
    /// UTC timestamp when the invitation was sent.
    /// </summary>
    public DateTime InvitedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the admin verified their email (nullable).
    /// </summary>
    public DateTime? VerifiedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the admin was revoked (nullable).
    /// </summary>
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Expiry timestamp for invitation (InvitationExpiryDays from InvitedAtUtc).
    /// </summary>
    public DateTime InvitationExpiresAtUtc => InvitedAtUtc.AddDays(InvitationExpiryDays);

    /// <summary>
    /// Indicates whether the invitation has expired.
    /// </summary>
    /// <param name="dateTimeProvider">Date time provider for current time</param>
    /// <returns>True if invitation has expired, false otherwise</returns>
    public bool IsInvitationExpired(IDateTimeProvider dateTimeProvider)
        => dateTimeProvider.UtcNow > InvitationExpiresAtUtc;

    /// <summary>
    /// Indicates whether the admin is currently active (verified and not revoked).
    /// </summary>
    public bool IsActive => Status == DistrictAdminStatus.Verified;

    /// <summary>
    /// Factory method to create a new DistrictAdmin with Unverified status.
    /// Emits DistrictAdminInvited domain event.
    /// </summary>
    /// <param name="id">Unique identifier for the district admin</param>
    /// <param name="districtId">ID of the owning district</param>
    /// <param name="email">Admin email address (will be normalized to lowercase)</param>
    /// <param name="dateTimeProvider">Date time provider for current time</param>
    /// <returns>New DistrictAdmin instance</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static DistrictAdmin Create(Guid id, Guid districtId, string email, IDateTimeProvider dateTimeProvider)
    {
        ValidateEmail(email);

        var admin = new DistrictAdmin(
            id,
            districtId,
            email.ToLowerInvariant(),
            DistrictAdminStatus.Unverified,
            dateTimeProvider.UtcNow
        );

        admin.RaiseDomainEvent(new DistrictAdminInvitedEvent(
            admin.Id,
            admin.DistrictId,
            admin.Email,
            admin.InvitedAtUtc,
            admin.InvitationExpiresAtUtc
        ));

        return admin;
    }

    /// <summary>
    /// Verifies the admin's email, transitioning status to Verified.
    /// Emits DistrictAdminVerified domain event.
    /// </summary>
    /// <param name="dateTimeProvider">Date time provider for current time</param>
    /// <exception cref="InvalidOperationException">Thrown when verification is not allowed</exception>
    public void Verify(IDateTimeProvider dateTimeProvider)
    {
        if (Status == DistrictAdminStatus.Verified)
        {
            throw new InvalidOperationException("Admin is already verified.");
        }

        if (Status == DistrictAdminStatus.Revoked)
        {
            throw new InvalidOperationException("Cannot verify a revoked admin.");
        }

        if (IsInvitationExpired(dateTimeProvider))
        {
            throw new InvalidOperationException("Invitation has expired.");
        }

        Status = DistrictAdminStatus.Verified;
        VerifiedAtUtc = dateTimeProvider.UtcNow;

        RaiseDomainEvent(new DistrictAdminVerifiedEvent(
            Id,
            DistrictId,
            Email,
            VerifiedAtUtc.Value
        ));
    }

    /// <summary>
    /// Revokes admin access, transitioning status to Revoked.
    /// Emits DistrictAdminRevoked domain event.
    /// </summary>
    /// <param name="reason">Reason for revocation (e.g., "User request", "District deleted")</param>
    /// <param name="dateTimeProvider">Date time provider for current time</param>
    /// <exception cref="InvalidOperationException">Thrown when revocation is not allowed</exception>
    public void Revoke(string reason, IDateTimeProvider dateTimeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason, nameof(reason));

        if (Status == DistrictAdminStatus.Revoked)
        {
            throw new InvalidOperationException("Admin is already revoked.");
        }

        Status = DistrictAdminStatus.Revoked;
        RevokedAtUtc = dateTimeProvider.UtcNow;

        RaiseDomainEvent(new DistrictAdminRevokedEvent(
            Id,
            DistrictId,
            Email,
            RevokedAtUtc.Value,
            reason
        ));
    }

    /// <summary>
    /// Resends invitation by updating InvitedAtUtc timestamp (extends expiry by 7 days).
    /// Only allowed for Unverified status.
    /// Emits DistrictAdminInvited domain event.
    /// </summary>
    /// <param name="dateTimeProvider">Date time provider for current time</param>
    public void ResendInvitation(IDateTimeProvider dateTimeProvider)
    {
        if (Status != DistrictAdminStatus.Unverified)
        {
            throw new InvalidOperationException("Can only resend invitation for unverified admins.");
        }

        InvitedAtUtc = dateTimeProvider.UtcNow;

        RaiseDomainEvent(new DistrictAdminInvitedEvent(
            Id,
            DistrictId,
            Email,
            InvitedAtUtc,
            InvitationExpiresAtUtc
        ));
    }

    private static void ValidateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        // Basic email format validation
        if (!email.Contains('@') || email.Length < 5 || email.Length > 255)
        {
            throw new ArgumentException("Invalid email format.", nameof(email));
        }

        // Additional validation can be added for email regex pattern
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
        {
            throw new ArgumentException("Invalid email format.", nameof(email));
        }
    }
}

/// <summary>
/// Status enumeration for district admin lifecycle.
/// </summary>
public enum DistrictAdminStatus
{
    /// <summary>
    /// Invitation sent but not yet verified (pending email confirmation).
    /// </summary>
    Unverified = 0,

    /// <summary>
    /// Email verified, admin is active.
    /// </summary>
    Verified = 1,

    /// <summary>
    /// Admin access revoked (manually or via district deletion).
    /// </summary>
    Revoked = 2
}

/// <summary>
/// Domain event raised when a district admin is invited.
/// </summary>
public sealed record DistrictAdminInvitedEvent(
    Guid AdminId,
    Guid DistrictId,
    string Email,
    DateTime InvitedAtUtc,
    DateTime ExpiresAtUtc
) : IDomainEvent;

/// <summary>
/// Domain event raised when a district admin verifies their email.
/// </summary>
public sealed record DistrictAdminVerifiedEvent(
    Guid AdminId,
    Guid DistrictId,
    string Email,
    DateTime VerifiedAtUtc
) : IDomainEvent;

/// <summary>
/// Domain event raised when a district admin is revoked.
/// </summary>
public sealed record DistrictAdminRevokedEvent(
    Guid AdminId,
    Guid DistrictId,
    string Email,
    DateTime RevokedAtUtc,
    string Reason
) : IDomainEvent;
