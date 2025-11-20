using MediatR;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Domain.Auditing;

namespace NorthStarET.NextGen.Lms.Infrastructure.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces tenant isolation for commands/queries.
/// Validates that the user's DistrictId matches the requested tenant context.
/// </summary>
internal sealed class TenantIsolationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;

    public TenantIsolationBehavior(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only enforce for tenant-scoped requests
        if (request is not ITenantScoped tenantScoped)
        {
            return await next();
        }

        // If tenant is not yet initialized (e.g., bootstrap create), skip validation.
        // SECURITY: Only CreateDistrictCommand should reach this code path with Guid.Empty.
        // All other commands implementing ITenantScoped must have a valid DistrictId.
        if (tenantScoped.DistrictId == Guid.Empty)
        {
            return await next();
        }

        // Platform admins can access any tenant (skip validation)
        if (_currentUserService.Role == ActorRole.PlatformAdmin)
        {
            return await next();
        }

        // Validate user's district matches requested district
        if (_currentUserService.DistrictId == null)
        {
            throw new UnauthorizedAccessException("User does not have a district assignment.");
        }

        if (_currentUserService.DistrictId != tenantScoped.DistrictId)
        {
            throw new UnauthorizedAccessException(
                $"Access denied: User's district ({_currentUserService.DistrictId}) does not match requested district ({tenantScoped.DistrictId}).");
        }

        return await next();
    }
}
