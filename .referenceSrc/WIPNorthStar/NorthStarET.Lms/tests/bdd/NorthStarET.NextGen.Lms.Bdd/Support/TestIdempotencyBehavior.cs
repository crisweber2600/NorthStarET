using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;

namespace NorthStarET.NextGen.Lms.Bdd.Support;

/// <summary>
/// MediatR pipeline behavior that leverages the fake idempotency service for BDD scenarios.
/// </summary>
public sealed class TestIdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly FakeIdempotencyService _idempotencyService;
    private readonly TestCurrentUserService _currentUserService;

    public TestIdempotencyBehavior(FakeIdempotencyService idempotencyService, TestCurrentUserService currentUserService)
    {
        _idempotencyService = idempotencyService;
        _currentUserService = currentUserService;
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IIdempotentCommand idempotentCommand)
        {
            return next();
        }

        var actorId = _currentUserService.UserId?.ToString() ?? "system";
        var action = idempotentCommand.Operation;
        var resourceId = idempotentCommand.EntityId.ToString();

        return _idempotencyService.ExecuteWithIdempotencyAsync(
            actorId,
            action,
            resourceId,
            request!,
            () => next(),
            cancellationToken);
    }
}
