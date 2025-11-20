using MediatR;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Domain.Auditing;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

namespace NorthStarET.NextGen.Lms.Infrastructure.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that wraps all commands/queries with automatic audit record creation.
/// Captures actor context, correlation ID, and before/after state for mutation operations.
/// </summary>
internal sealed class AuditingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly DistrictsDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditingBehavior(
        DistrictsDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Execute the request
        var response = await next();

        // Check if request implements IAuditableCommand
        if (request is IAuditableCommand auditableCommand)
        {
            var auditRecord = AuditRecord.Create(
                Guid.NewGuid(),
                auditableCommand.DistrictId,
                _currentUserService.UserId ?? Guid.Empty,
                _currentUserService.Role,
                auditableCommand.Action,
                auditableCommand.EntityType,
                auditableCommand.EntityId,
                auditableCommand.BeforePayload,
                auditableCommand.AfterPayload,
                _currentUserService.CorrelationId ?? Guid.NewGuid(),
                _dateTimeProvider
            );

            await _context.AuditRecords.AddAsync(auditRecord, cancellationToken);
            // Audit records are persisted as part of the same transaction as the main operation.
        }

        return response;
    }
}
