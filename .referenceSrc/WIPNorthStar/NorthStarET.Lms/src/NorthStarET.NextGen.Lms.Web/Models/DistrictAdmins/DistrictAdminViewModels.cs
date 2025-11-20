using NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;

namespace NorthStarET.NextGen.Lms.Web.Models.DistrictAdmins;

public sealed class ManageDistrictAdminsViewModel
{
    public Guid DistrictId { get; init; }
    public string DistrictName { get; init; } = string.Empty;
    public IReadOnlyList<DistrictAdminResponse> Admins { get; init; } = Array.Empty<DistrictAdminResponse>();
}

public sealed class InviteDistrictAdminViewModel
{
    public Guid DistrictId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
