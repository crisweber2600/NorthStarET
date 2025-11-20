using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Web.Models;

public sealed class ClaimsDiagnosticViewModel
{
    public string? UserName { get; init; }

    public string? AuthenticationType { get; init; }

    public IReadOnlyCollection<ClaimSummary> Claims { get; init; } = new List<ClaimSummary>();
}

public sealed class ClaimSummary
{
    public string Type { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;

    public string ValueType { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;
}
