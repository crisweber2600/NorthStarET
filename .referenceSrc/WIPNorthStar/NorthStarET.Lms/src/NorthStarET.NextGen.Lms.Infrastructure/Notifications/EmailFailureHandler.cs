using Microsoft.Extensions.Logging;

namespace NorthStarET.NextGen.Lms.Infrastructure.Notifications;

/// <summary>
/// Dead-letter queue handler for failed email deliveries.
/// Stores failures for retry or manual intervention.
/// </summary>
public interface IEmailFailureHandler
{
    /// <summary>
    /// Handles a failed email delivery by storing it in the dead-letter queue.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="invitationToken">Invitation token</param>
    /// <param name="districtName">District name</param>
    /// <param name="expiresAtUtc">Invitation expiration</param>
    /// <param name="exception">The exception that caused the failure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandleFailedEmailAsync(
        string email,
        string invitationToken,
        string districtName,
        DateTime expiresAtUtc,
        Exception exception,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all failed email records for manual intervention.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of failed email records</returns>
    Task<IReadOnlyList<FailedEmailRecord>> GetFailedEmailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a failed email as retried.
    /// </summary>
    /// <param name="recordId">Failed record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MarkAsRetriedAsync(Guid recordId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Dead-letter queue implementation using in-memory storage (for demonstration).
/// In production, replace with Azure Storage Queue, RabbitMQ, or database table.
/// </summary>
public sealed class EmailFailureHandler : IEmailFailureHandler
{
    private readonly ILogger<EmailFailureHandler> _logger;
    // In-memory storage for demonstration - replace with persistent storage in production
    private readonly List<FailedEmailRecord> _failedEmails = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public EmailFailureHandler(ILogger<EmailFailureHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleFailedEmailAsync(
        string email,
        string invitationToken,
        string districtName,
        DateTime expiresAtUtc,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        var record = new FailedEmailRecord
        {
            Id = Guid.NewGuid(),
            Email = email,
            InvitationToken = invitationToken,
            DistrictName = districtName,
            ExpiresAtUtc = expiresAtUtc,
            FailedAtUtc = DateTime.UtcNow,
            ExceptionMessage = exception.Message,
            ExceptionStackTrace = exception.StackTrace ?? string.Empty,
            RetryCount = 0,
            IsRetried = false
        };

        await _lock.WaitAsync(cancellationToken);
        try
        {
            _failedEmails.Add(record);

            _logger.LogError(
                exception,
                "Email failure recorded in dead-letter queue. RecordId={RecordId}, Email={Email}, District={DistrictName}",
                record.Id, email, districtName);

            // In production, persist to database or queue:
            // await _dbContext.FailedEmails.AddAsync(record, cancellationToken);
            // await _dbContext.SaveChangesAsync(cancellationToken);
            // OR
            // await _queueClient.SendMessageAsync(JsonSerializer.Serialize(record), cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<FailedEmailRecord>> GetFailedEmailsAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            // In production:
            // return await _dbContext.FailedEmails
            //     .Where(e => !e.IsRetried)
            //     .OrderByDescending(e => e.FailedAtUtc)
            //     .ToListAsync(cancellationToken);

            return _failedEmails
                .Where(e => !e.IsRetried)
                .OrderByDescending(e => e.FailedAtUtc)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task MarkAsRetriedAsync(Guid recordId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var record = _failedEmails.FirstOrDefault(e => e.Id == recordId);
            if (record != null)
            {
                record.IsRetried = true;
                record.RetryCount++;
                record.RetriedAtUtc = DateTime.UtcNow;

                _logger.LogInformation(
                    "Failed email marked as retried. RecordId={RecordId}, Email={Email}",
                    recordId, record.Email);

                // In production:
                // var record = await _dbContext.FailedEmails.FindAsync(recordId);
                // if (record != null)
                // {
                //     record.IsRetried = true;
                //     record.RetryCount++;
                //     record.RetriedAtUtc = DateTime.UtcNow;
                //     await _dbContext.SaveChangesAsync(cancellationToken);
                // }
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}

/// <summary>
/// Record of a failed email delivery for dead-letter queue.
/// </summary>
public sealed class FailedEmailRecord
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string InvitationToken { get; init; } = string.Empty;
    public string DistrictName { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public DateTime FailedAtUtc { get; init; }
    public string ExceptionMessage { get; init; } = string.Empty;
    public string ExceptionStackTrace { get; init; } = string.Empty;
    public int RetryCount { get; set; }
    public bool IsRetried { get; set; }
    public DateTime? RetriedAtUtc { get; set; }
}
