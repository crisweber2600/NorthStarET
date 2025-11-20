using System;
using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

public readonly record struct Permission
{
    private static readonly HashSet<string> AllowedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Read",
        "Write",
        "Delete",
        "Manage"
    };

    private const int MaxNameLength = 100;

    public Permission(string resource, string action)
    {
        if (string.IsNullOrWhiteSpace(resource))
        {
            throw new ArgumentException("Permission resource cannot be null or whitespace.", nameof(resource));
        }

        if (resource.Length > MaxNameLength)
        {
            throw new ArgumentOutOfRangeException(nameof(resource), resource.Length, $"Resource name cannot exceed {MaxNameLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Permission action cannot be null or whitespace.", nameof(action));
        }

        if (!AllowedActions.Contains(action))
        {
            throw new ArgumentOutOfRangeException(nameof(action), action, $"Action must be one of: {string.Join(", ", AllowedActions)}");
        }

        Resource = resource;
        Action = action;
    }

    public string Resource { get; }

    public string Action { get; }

    public override string ToString() => $"{Resource}:{Action}";
}
