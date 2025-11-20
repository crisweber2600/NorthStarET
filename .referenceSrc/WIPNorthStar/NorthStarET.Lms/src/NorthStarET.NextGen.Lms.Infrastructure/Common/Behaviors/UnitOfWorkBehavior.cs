using MediatR;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

namespace NorthStarET.NextGen.Lms.Infrastructure.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that commits all database changes after successful command execution.
/// Ensures domain events are published as part of SaveChangesAsync.
/// </summary>
internal sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly DistrictsDbContext _context;

    public UnitOfWorkBehavior(DistrictsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply UnitOfWork pattern to commands (mutations)
        if (!IsCommand(request))
        {
            return await next();
        }

        // Execute the request
        var response = await next();

        // Commit changes (publishes domain events automatically)
        await _context.SaveChangesAsync(cancellationToken);

        return response;
    }

    private static bool IsCommand(TRequest request)
    {
        // Use marker interface to identify commands
        return request is ICommand;
    }
}
