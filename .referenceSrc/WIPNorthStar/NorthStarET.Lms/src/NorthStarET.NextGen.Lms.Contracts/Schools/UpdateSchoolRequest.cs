using System.ComponentModel.DataAnnotations;

namespace NorthStarET.NextGen.Lms.Contracts.Schools;

/// <summary>
/// Request to update school metadata (excluding grade assignments).
/// </summary>
public sealed record UpdateSchoolRequest
{
    /// <summary>
    /// School name (3-100 characters). Must be unique within the district.
    /// </summary>
    [Required(ErrorMessage = "School name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "School name must be between 3 and 100 characters.")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional school code (2-20 characters). Must be unique within the district if provided.
    /// </summary>
    [StringLength(20, MinimumLength = 2, ErrorMessage = "School code must be between 2 and 20 characters.")]
    public string? Code { get; init; }

    /// <summary>
    /// Optional notes about the school (max 500 characters).
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string? Notes { get; init; }

    /// <summary>
    /// Concurrency token for optimistic concurrency control.
    /// </summary>
    [Required(ErrorMessage = "Concurrency stamp is required.")]
    public string ConcurrencyStamp { get; init; } = string.Empty;
}
