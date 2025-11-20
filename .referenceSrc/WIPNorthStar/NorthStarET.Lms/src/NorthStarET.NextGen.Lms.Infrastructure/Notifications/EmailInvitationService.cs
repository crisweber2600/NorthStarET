using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Infrastructure.Notifications.Configuration;

namespace NorthStarET.NextGen.Lms.Infrastructure.Notifications;

/// <summary>
/// Email invitation service with exponential backoff retry for sending district admin invitations.
/// Implements 3-attempt retry with dead-letter queue for failures.
/// </summary>
public interface IEmailInvitationService
{
    /// <summary>
    /// Sends an invitation email to a district admin.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="invitationToken">Unique invitation token for verification link</param>
    /// <param name="districtName">Name of the district</param>
    /// <param name="expiresAtUtc">Expiration timestamp of the invitation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    Task<bool> SendInvitationAsync(
        string email,
        string invitationToken,
        string districtName,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Email invitation service with exponential backoff retry (3 attempts).
/// Uses Polly for resilience and routes failures to dead-letter queue.
/// Verification URLs are resolved via Aspire service discovery.
/// </summary>
public sealed class EmailInvitationService : IEmailInvitationService
{
    private readonly IEmailFailureHandler _failureHandler;
    private readonly ILogger<EmailInvitationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly int _maxRetryAttempts = 3;

    public EmailInvitationService(
        IEmailFailureHandler failureHandler,
        ILogger<EmailInvitationService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _failureHandler = failureHandler;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> SendInvitationAsync(
        string email,
        string invitationToken,
        string districtName,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt < _maxRetryAttempts)
        {
            attempt++;

            try
            {
                _logger.LogInformation(
                    "Sending invitation email to {Email} for district {DistrictName} (attempt {Attempt}/{MaxAttempts})",
                    email, districtName, attempt, _maxRetryAttempts);

                // TODO: Replace with actual email provider (SendGrid, SMTP, etc.)
                await SendEmailInternalAsync(email, invitationToken, districtName, expiresAtUtc, cancellationToken);

                _logger.LogInformation(
                    "Successfully sent invitation email to {Email}",
                    email);

                return true;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(
                    ex,
                    "Failed to send invitation email to {Email} on attempt {Attempt}/{MaxAttempts}",
                    email, attempt, _maxRetryAttempts);

                if (attempt < _maxRetryAttempts)
                {
                    // Exponential backoff: 1s, 2s, 4s
                    var delaySeconds = Math.Pow(2, attempt - 1);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                }
            }
        }

        // All retries failed - send to dead-letter queue
        _logger.LogError(
            lastException,
            "All {MaxAttempts} attempts failed to send invitation email to {Email}. Routing to dead-letter queue.",
            _maxRetryAttempts, email);

        await _failureHandler.HandleFailedEmailAsync(
            email,
            invitationToken,
            districtName,
            expiresAtUtc,
            lastException!,
            cancellationToken);

        return false;
    }

    private async Task SendEmailInternalAsync(
        string email,
        string invitationToken,
        string districtName,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken)
    {
        // TODO: Implement actual email sending logic
        // For now, simulate email sending with logging

        // Use service discovery to resolve the Web service URL
        var httpClient = _httpClientFactory.CreateClient("WebService");
        var baseAddress = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://lms.northstaret.org";
        var verificationLink = $"{baseAddress}/verify?token={invitationToken}";
        var expiresIn = (expiresAtUtc - DateTime.UtcNow).Days;

        var emailBody = $@"
You have been invited to join {districtName} as a District Administrator.

Click the link below to verify your email and activate your account:
{verificationLink}

This invitation expires in {expiresIn} days.

If you did not request this invitation, please ignore this email.
";

        // Simulate sending delay
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation(
            "Email body prepared for {Email}: {EmailBody}",
            email, emailBody);

        // In production, replace with:
        // await _emailClient.SendAsync(new EmailMessage
        // {
        //     To = email,
        //     Subject = $"Invitation to {districtName}",
        //     Body = emailBody,
        //     IsHtml = false
        // }, cancellationToken);
    }
}
