using System.ComponentModel.DataAnnotations;

namespace NorthStarET.NextGen.Lms.Contracts.Districts;

/// <summary>
/// Request to update an existing district's name and/or suffix.
/// </summary>
public sealed record UpdateDistrictRequest
{
    /// <summary>
    /// District display name (3-100 characters).
    /// </summary>
    [Required(ErrorMessage = "District name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "District name must be between 3 and 100 characters.")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Unique case-insensitive email domain suffix.
    /// Must match regex ^[a-z0-9.-]+$ and be unique platform-wide.
    /// </summary>
    [Required(ErrorMessage = "District suffix is required.")]
    [RegularExpression(@"^[a-z0-9.-]+$", ErrorMessage = "Suffix must contain only lowercase letters, numbers, dots, and hyphens.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Suffix must be between 2 and 50 characters.")]
    public string Suffix { get; init; } = string.Empty;
}
