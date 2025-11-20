using NorthStarET.NextGen.Lms.Contracts.Districts;

namespace NorthStarET.NextGen.Lms.Web.Models.Districts;

public sealed class DistrictListViewModel
{
    public IReadOnlyList<DistrictResponse> Districts { get; init; } = Array.Empty<DistrictResponse>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}

public sealed class CreateDistrictViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
}

public sealed class EditDistrictViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
}

public sealed class DeleteDistrictViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public int AdminCount { get; set; }
}
