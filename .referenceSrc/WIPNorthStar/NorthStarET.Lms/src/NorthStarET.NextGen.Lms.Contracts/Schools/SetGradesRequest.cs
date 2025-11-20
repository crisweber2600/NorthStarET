using System.ComponentModel.DataAnnotations;

namespace NorthStarET.NextGen.Lms.Contracts.Schools;

/// <summary>
/// Request to replace grade offerings for a school (US2).
/// </summary>
public sealed record SetGradesRequest
{
    /// <summary>
    /// Concurrency token for optimistic concurrency control.
    /// </summary>
    [Required(ErrorMessage = "Concurrency stamp is required.")]
    public string ConcurrencyStamp { get; init; } = string.Empty;

    /// <summary>
    /// Complete list of grade selections (selected and unselected grades).
    /// </summary>
    [Required(ErrorMessage = "Grade selections are required.")]
    public IReadOnlyList<GradeSelectionDto> GradeSelections { get; init; } = Array.Empty<GradeSelectionDto>();
}
