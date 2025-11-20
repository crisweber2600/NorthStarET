using MediatR;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Infrastructure.Idempotency;

namespace NorthStarET.NextGen.Lms.Infrastructure.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that wraps idempotent commands with 10-minute deduplication windows.
/// Prevents duplicate operations by caching command payloads in Redis.
/// </summary>
internal sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly ICurrentUserService _currentUserService;

    public IdempotencyBehavior(
        IIdempotencyService idempotencyService,
        ICurrentUserService currentUserService)
    {
        _idempotencyService = idempotencyService ?? throw new ArgumentNullException(nameof(idempotencyService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply idempotency to commands marked with IIdempotentCommand
        if (request is not IIdempotentCommand idempotentCommand)
        {
            return await next();
        }

        // Get actor ID from current user service
        var actorId = _currentUserService.UserId?.ToString() ?? "system";

        // Execute with idempotency check (10-minute window)
        return await _idempotencyService.ExecuteWithIdempotencyAsync(
            actorId,
            idempotentCommand.Operation,
            idempotentCommand.EntityId.ToString(),
            request,
            async () => await next(),
            cancellationToken
        );
    }
}
