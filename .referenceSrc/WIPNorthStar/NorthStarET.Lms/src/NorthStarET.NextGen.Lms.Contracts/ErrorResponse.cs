namespace NorthStarET.NextGen.Lms.Contracts;

/// <summary>
/// Standard error response for API endpoints.
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// The error message describing what went wrong.
    /// </summary>
    public required string Error { get; init; }

    /// <summary>
    /// Optional error code for programmatic error handling.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Optional additional details about the error.
    /// </summary>
    public string? Details { get; init; }
}
