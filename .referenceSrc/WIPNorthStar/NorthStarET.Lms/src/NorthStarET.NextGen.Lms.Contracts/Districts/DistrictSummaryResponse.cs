namespace NorthStarET.NextGen.Lms.Contracts.Districts;

/// <summary>
/// Summary response for district list views (paginated results).
/// </summary>
public sealed record DistrictSummaryResponse(
    Guid Id,
    string Name,
    string Suffix,
    int AdminCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
