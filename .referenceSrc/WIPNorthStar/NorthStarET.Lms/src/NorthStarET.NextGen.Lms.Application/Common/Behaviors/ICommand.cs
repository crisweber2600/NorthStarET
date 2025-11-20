namespace NorthStarET.NextGen.Lms.Application.Common.Behaviors;

/// <summary>
/// Marker interface for commands that mutate state.
/// Used by UnitOfWorkBehavior to identify requests that require database transaction commit.
/// </summary>
public interface ICommand
{
}
