using MediatR;

namespace NorthStarET.Foundation.Identity.Application.Commands;

/// <summary>
/// Command to exchange an Entra ID token for a session
/// </summary>
public sealed record ExchangeTokenCommand(
    string AccessToken,
    string? IpAddress = null,
    string? UserAgent = null) : IRequest<ExchangeTokenResult>;

/// <summary>
/// Result of token exchange operation
/// </summary>
public sealed record ExchangeTokenResult(
    Guid SessionId,
    Guid UserId,
    string UserPrincipalName,
    DateTime ExpiresAt,
    string[] Roles);
