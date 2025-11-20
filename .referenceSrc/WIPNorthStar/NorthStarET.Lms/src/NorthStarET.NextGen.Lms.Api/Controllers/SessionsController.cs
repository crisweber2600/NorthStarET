using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Application.Authentication.Commands;
using NorthStarET.NextGen.Lms.Application.Authentication.Queries;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Contracts;
using NorthStarET.NextGen.Lms.Contracts.Authentication;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Api.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public sealed class SessionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SessionsController> _logger;
    private readonly IEntraTokenValidator _entraTokenValidator;
    private readonly ILmsTokenGenerator _lmsTokenGenerator;
    private readonly IUserRepository _userRepository;
    private readonly IdentityModuleSettings _settings;

    public SessionsController(
        IMediator mediator,
        ILogger<SessionsController> logger,
        IEntraTokenValidator entraTokenValidator,
        ILmsTokenGenerator lmsTokenGenerator,
        IUserRepository userRepository,
        IOptions<IdentityModuleSettings> settings)
    {
        _mediator = mediator;
        _logger = logger;
        _entraTokenValidator = entraTokenValidator;
        _lmsTokenGenerator = lmsTokenGenerator;
        _userRepository = userRepository;
        _settings = settings.Value;
    }

    /// <summary>
    /// Refreshes an active session, extending its expiration time.
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="request">Refresh session request containing the Entra token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated session information</returns>
    [HttpPost("{sessionId}/refresh")]
    public async Task<ActionResult<RefreshSessionResponse>> RefreshSession(
        [FromRoute] Guid sessionId,
        [FromBody] RefreshSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate the provided Entra token
            if (string.IsNullOrWhiteSpace(request.EntraToken))
            {
                _logger.LogWarning("Refresh session request missing Entra token for session {SessionId}", sessionId);
                return BadRequest(new ErrorResponse { Error = "Entra token is required for session refresh." });
            }

            // Validate the Entra token with Microsoft Entra ID and get the principal
            ClaimsPrincipal principal;
            try
            {
                principal = await _entraTokenValidator.ValidateAsync(request.EntraToken, cancellationToken);
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Failed to validate Entra token for session {SessionId} refresh", sessionId);
                return Unauthorized(new ErrorResponse { Error = "Invalid Entra token." });
            }

            // Extract the subject (user ID) from the Entra token
            var entraSubjectId = ExtractEntraSubject(principal);

            // Retrieve current session to get user ID and calculate new expiration
            var currentSession = await _mediator.Send(
                new ValidateSessionQuery(sessionId),
                cancellationToken);

            // Retrieve the user to verify the token belongs to the session owner
            var user = await _userRepository.GetAsync(currentSession.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found for session {SessionId}", currentSession.UserId, sessionId);
                return BadRequest(new ErrorResponse { Error = "Session user not found." });
            }

            // Verify the Entra token's subject matches the session's user
            if (user.EntraSubjectId != entraSubjectId)
            {
                _logger.LogWarning(
                    "Entra token subject mismatch for session {SessionId}. Token subject: {TokenSubject}, Session user: {SessionUser}",
                    sessionId, entraSubjectId.Value, user.EntraSubjectId.Value);
                return Unauthorized(new ErrorResponse { Error = "Token does not belong to the session owner." });
            }

            var now = DateTimeOffset.UtcNow;
            var newExpiresAt = now.AddMinutes(_settings.SessionSlidingExpirationMinutes);

            // Generate a new JWT LMS access token with session metadata
            var newToken = _lmsTokenGenerator.GenerateAccessToken(
                currentSession.UserId,
                sessionId,
                newExpiresAt);

            await _mediator.Send(
                new RefreshSessionCommand(sessionId, newToken),
                cancellationToken);

            // Retrieve the updated session to return accurate information
            var session = await _mediator.Send(
                new ValidateSessionQuery(sessionId),
                cancellationToken);

            return Ok(new RefreshSessionResponse
            {
                ExpiresAt = session.ExpiresAt,
                LastActivityAt = session.LastActivityAt
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to refresh session {SessionId}", sessionId);
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    private static EntraSubjectId ExtractEntraSubject(ClaimsPrincipal principal)
    {
        const string SubjectClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        const string ObjectIdClaim = "oid";

        var subject = principal.FindFirst(SubjectClaim)?.Value
            ?? principal.FindFirst(ObjectIdClaim)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException("Subject claim missing from Entra token.");
        }

        return new EntraSubjectId(subject);
    }

    /// <summary>
    /// Revokes an active session, removing it from cache and marking it as invalid.
    /// </summary>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="request">Revoke session request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPost("{sessionId}/revoke")]
    public async Task<IActionResult> RevokeSession(
        [FromRoute] Guid sessionId,
        [FromBody] RevokeSessionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(
                new RevokeSessionCommand(sessionId),
                cancellationToken);

            _logger.LogInformation("Session {SessionId} revoked successfully", sessionId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to revoke session {SessionId}", sessionId);
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }
}
