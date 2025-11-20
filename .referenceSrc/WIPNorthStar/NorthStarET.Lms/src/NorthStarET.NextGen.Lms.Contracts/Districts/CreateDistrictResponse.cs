namespace NorthStarET.NextGen.Lms.Contracts.Districts;

/// <summary>
/// Response returned after successfully creating a district.
/// </summary>
public sealed record CreateDistrictResponse(
    Guid Id,
    string Name,
    string Suffix,
    DateTime CreatedAtUtc);
