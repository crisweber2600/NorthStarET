using System;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Entities;

public sealed class Session
{
    private Session(
        Guid id,
        Guid userId,
        string entraTokenHash,
        string lmsAccessToken,
        TenantId activeTenantId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset lastActivityAt,
        bool isRevoked,
        string? ipAddress,
        string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(entraTokenHash))
        {
            throw new ArgumentException("Entra token hash cannot be null or whitespace.", nameof(entraTokenHash));
        }

        if (string.IsNullOrWhiteSpace(lmsAccessToken))
        {
            throw new ArgumentException("LMS access token cannot be null or whitespace.", nameof(lmsAccessToken));
        }

        if (expiresAt <= createdAt)
        {
            throw new ArgumentException("Session expiration must be in the future relative to creation time.", nameof(expiresAt));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = userId;
        EntraTokenHash = entraTokenHash;
        LmsAccessToken = lmsAccessToken;
        ActiveTenantId = activeTenantId;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        LastActivityAt = lastActivityAt;
        IsRevoked = isRevoked;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public string EntraTokenHash { get; private set; }

    public string LmsAccessToken { get; private set; }

    public TenantId ActiveTenantId { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset LastActivityAt { get; private set; }

    public bool IsRevoked { get; private set; }

    public string? IpAddress { get; }

    public string? UserAgent { get; }

    public static Session Create(
        Guid userId,
        string entraTokenHash,
        string lmsAccessToken,
        TenantId activeTenantId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        string? ipAddress,
        string? userAgent)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        return new Session(
            Guid.NewGuid(),
            userId,
            entraTokenHash,
            lmsAccessToken,
            activeTenantId,
            createdAt,
            expiresAt,
            createdAt,
            false,
            ipAddress,
            userAgent);
    }

    /// <summary>
    /// Creates a new session with a specific session ID. Useful when the ID needs to be known before creation (e.g., for JWT token generation).
    /// </summary>
    public static Session CreateWithId(
        Guid sessionId,
        Guid userId,
        string entraTokenHash,
        string lmsAccessToken,
        TenantId activeTenantId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        string? ipAddress,
        string? userAgent)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        return new Session(
            sessionId,
            userId,
            entraTokenHash,
            lmsAccessToken,
            activeTenantId,
            createdAt,
            expiresAt,
            createdAt,
            false,
            ipAddress,
            userAgent);
    }

    public void Refresh(string lmsAccessToken, DateTimeOffset expiresAt, DateTimeOffset activityAt)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Cannot refresh a revoked session.");
        }

        if (string.IsNullOrWhiteSpace(lmsAccessToken))
        {
            throw new ArgumentException("LMS access token cannot be null or whitespace.", nameof(lmsAccessToken));
        }

        if (expiresAt <= activityAt)
        {
            throw new ArgumentException("Expiration must occur after the activity timestamp.", nameof(expiresAt));
        }

        LmsAccessToken = lmsAccessToken;
        ExpiresAt = expiresAt;
        LastActivityAt = activityAt;
    }

    public void Touch(DateTimeOffset activityAt)
    {
        if (activityAt < CreatedAt)
        {
            throw new ArgumentException("Activity timestamp cannot be earlier than session creation.", nameof(activityAt));
        }

        LastActivityAt = activityAt;
    }

    public void SwitchTenant(TenantId tenantId)
    {
        ActiveTenantId = tenantId;
    }

    public void UpdateEntraTokenHash(string entraTokenHash)
    {
        if (string.IsNullOrWhiteSpace(entraTokenHash))
        {
            throw new ArgumentException("Entra token hash cannot be null or whitespace.", nameof(entraTokenHash));
        }

        EntraTokenHash = entraTokenHash;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}
