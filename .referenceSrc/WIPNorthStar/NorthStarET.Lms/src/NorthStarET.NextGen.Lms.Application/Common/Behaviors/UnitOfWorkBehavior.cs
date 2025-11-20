namespace NorthStarET.NextGen.Lms.Application.Common.Behaviors;

/// <summary>
/// Marker interface for unit of work pattern.
/// Infrastructure layer will implement behavior that commits DbContext changes.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

