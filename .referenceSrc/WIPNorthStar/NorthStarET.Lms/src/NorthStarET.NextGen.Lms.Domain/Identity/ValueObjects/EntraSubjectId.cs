using System;

namespace NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

public readonly record struct EntraSubjectId
{
    private const int MaxLength = 256;

    public EntraSubjectId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Entra subject id cannot be null or whitespace.", nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value.Length, $"Entra subject id cannot exceed {MaxLength} characters.");
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static implicit operator string(EntraSubjectId subjectId) => subjectId.Value;

    public static implicit operator EntraSubjectId(string value) => new(value);
}
