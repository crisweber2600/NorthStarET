using NorthStarET.NextGen.Lms.Domain.Common;

namespace NorthStarET.NextGen.Lms.Domain.Schools;

/// <summary>
/// Domain event raised when a new school is created.
/// </summary>
/// <param name="SchoolId">Unique identifier of the created school</param>
/// <param name="DistrictId">Owning district identifier</param>
/// <param name="SchoolName">Name of the created school</param>
/// <param name="CreatedBy">User who created the school</param>
public sealed record SchoolCreatedEvent(
    Guid SchoolId,
    Guid DistrictId,
    string SchoolName,
    Guid CreatedBy) : IDomainEvent;

/// <summary>
/// Domain event raised when a school is updated.
/// </summary>
/// <param name="SchoolId">Unique identifier of the updated school</param>
/// <param name="DistrictId">Owning district identifier</param>
/// <param name="SchoolName">Updated name of the school</param>
/// <param name="UpdatedBy">User who updated the school</param>
public sealed record SchoolUpdatedEvent(
    Guid SchoolId,
    Guid DistrictId,
    string SchoolName,
    Guid UpdatedBy) : IDomainEvent;

/// <summary>
/// Domain event raised when a school is soft-deleted.
/// Triggers archival of all grade offerings.
/// </summary>
/// <param name="SchoolId">Unique identifier of the deleted school</param>
/// <param name="DistrictId">Owning district identifier</param>
/// <param name="DeletedBy">User who deleted the school</param>
public sealed record SchoolDeletedEvent(
    Guid SchoolId,
    Guid DistrictId,
    Guid DeletedBy) : IDomainEvent;

/// <summary>
/// Domain event raised when a soft-deleted school is restored.
/// </summary>
/// <param name="SchoolId">Unique identifier of the restored school</param>
/// <param name="DistrictId">Owning district identifier</param>
/// <param name="RestoredBy">User who restored the school</param>
public sealed record SchoolRestoredEvent(
    Guid SchoolId,
    Guid DistrictId,
    Guid RestoredBy) : IDomainEvent;

/// <summary>
/// Domain event raised when a school's grade offerings are updated.
/// Contains the new set of grades the school will serve.
/// </summary>
/// <param name="SchoolId">Unique identifier of the school</param>
/// <param name="DistrictId">Owning district identifier</param>
/// <param name="GradeLevels">New set of grade levels offered by the school</param>
/// <param name="UpdatedBy">User who updated the grades</param>
public sealed record SchoolGradesUpdatedEvent(
    Guid SchoolId,
    Guid DistrictId,
    IReadOnlyList<GradeLevel> GradeLevels,
    Guid UpdatedBy) : IDomainEvent;
