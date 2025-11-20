using System;
using System.Collections.Generic;
using System.Linq;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Entities;

public sealed class User
{
    private readonly List<Membership> memberships = new();
    private readonly List<Session> sessions = new();

    public User(
        Guid id,
        EntraSubjectId entraSubjectId,
        string email,
        string firstName,
        string lastName,
        DateTimeOffset createdAt,
        bool isActive = true)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
        }

        if (email.Length > 256)
        {
            throw new ArgumentOutOfRangeException(nameof(email), email.Length, "Email cannot exceed 256 characters.");
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be null or whitespace.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be null or whitespace.", nameof(lastName));
        }

        Id = id;
        EntraSubjectId = entraSubjectId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = createdAt;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public EntraSubjectId EntraSubjectId { get; }

    public string Email { get; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string DisplayName => string.IsNullOrWhiteSpace(displayNameOverride)
        ? $"{FirstName} {LastName}".Trim()
        : displayNameOverride!;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public IReadOnlyCollection<Membership> Memberships => memberships;

    public IReadOnlyCollection<Session> Sessions => sessions;

    private string? displayNameOverride;

    public void UpdateName(string firstName, string lastName, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be null or whitespace.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be null or whitespace.", nameof(lastName));
        }

        FirstName = firstName;
        LastName = lastName;
        displayNameOverride = string.IsNullOrWhiteSpace(displayName) ? null : displayName;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void RegisterLogin(DateTimeOffset loggedInAt)
    {
        LastLoginAt = loggedInAt;
    }

    public void AddMembership(Membership membership)
    {
        if (membership is null)
        {
            throw new ArgumentNullException(nameof(membership));
        }

        if (membership.UserId != Id)
        {
            throw new InvalidOperationException("Cannot add membership that belongs to a different user.");
        }

        if (memberships.Any(m => m.TenantId == membership.TenantId))
        {
            throw new InvalidOperationException("User already has a membership for the specified tenant.");
        }

        memberships.Add(membership);
    }

    public void RemoveMembership(Guid membershipId)
    {
        var membership = memberships.SingleOrDefault(m => m.Id == membershipId);
        if (membership is null)
        {
            return;
        }

        memberships.Remove(membership);
    }

    public void AttachSession(Session session)
    {
        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (session.UserId != Id)
        {
            throw new InvalidOperationException("Cannot attach session that belongs to another user.");
        }

        sessions.Add(session);
    }

    public void InvalidateSession(Guid sessionId)
    {
        var session = sessions.FirstOrDefault(s => s.Id == sessionId);
        session?.Revoke();
    }
}
