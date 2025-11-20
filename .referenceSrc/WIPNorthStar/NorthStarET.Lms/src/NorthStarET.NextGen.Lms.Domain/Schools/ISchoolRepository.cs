namespace NorthStarET.NextGen.Lms.Domain.Schools;

/// <summary>
/// Repository abstraction for School aggregate persistence and retrieval.
/// Enforces tenant isolation and supports query operations for the Schools & Grades feature.
/// </summary>
public interface ISchoolRepository
{
    /// <summary>
    /// Gets a school by ID with tenant validation.
    /// </summary>
    /// <param name="schoolId">School identifier</param>
    /// <param name="districtId">District identifier for tenant validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>School if found and belongs to district, null otherwise</returns>
    Task<School?> GetByIdAsync(Guid schoolId, Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a school by ID with grade offerings eagerly loaded.
    /// </summary>
    /// <param name="schoolId">School identifier</param>
    /// <param name="districtId">District identifier for tenant validation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>School with grades if found and belongs to district, null otherwise</returns>
    Task<School?> GetByIdWithGradesAsync(Guid schoolId, Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all schools for a specific district with optional search and sorting.
    /// Excludes soft-deleted schools by default.
    /// </summary>
    /// <param name="districtId">District identifier (tenant scope)</param>
    /// <param name="searchTerm">Optional search term for name/code filtering</param>
    /// <param name="includeDeleted">Whether to include soft-deleted schools</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of schools matching criteria</returns>
    Task<IReadOnlyList<School>> ListByDistrictAsync(
        Guid districtId,
        string? searchTerm = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a school name already exists in the district (case-insensitive).
    /// Used for uniqueness validation during create/update.
    /// </summary>
    /// <param name="districtId">District identifier</param>
    /// <param name="name">School name to check</param>
    /// <param name="excludeSchoolId">Optional school ID to exclude from check (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if name exists, false otherwise</returns>
    Task<bool> ExistsWithNameAsync(
        Guid districtId,
        string name,
        Guid? excludeSchoolId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a school code already exists in the district (case-insensitive).
    /// Only checks non-null codes.
    /// </summary>
    /// <param name="districtId">District identifier</param>
    /// <param name="code">School code to check</param>
    /// <param name="excludeSchoolId">Optional school ID to exclude from check (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if code exists, false otherwise</returns>
    Task<bool> ExistsWithCodeAsync(
        Guid districtId,
        string code,
        Guid? excludeSchoolId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new school to the repository.
    /// </summary>
    /// <param name="school">School to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(School school, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing school.
    /// Handles optimistic concurrency via ConcurrencyStamp.
    /// </summary>
    /// <param name="school">School to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(School school, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a school and archives its grade offerings.
    /// </summary>
    /// <param name="school">School to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(School school, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the database and publishes domain events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities affected</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
