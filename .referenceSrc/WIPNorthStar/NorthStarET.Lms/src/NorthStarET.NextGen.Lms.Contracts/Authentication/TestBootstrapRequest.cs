namespace NorthStarET.NextGen.Lms.Contracts.Authentication;

public sealed class TestBootstrapRequest
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid ActiveTenantId { get; set; } = Guid.Empty; // Optional: API will auto-select if empty
}
