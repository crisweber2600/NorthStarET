namespace NorthStarET.NextGen.Lms.Domain.Districts;

/// <summary>
/// Repository interface for District aggregate persistence.
/// All queries are tenant-scoped (filter by DistrictId or exclude deleted).
/// </summary>
public interface IDistrictRepository
{
    /// <summary>
    /// Retrieves a district by its unique identifier.
    /// </summary>
    /// <param name="id">District ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>District if found, null otherwise</returns>
    Task<District?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a district by its unique suffix (case-insensitive).
    /// </summary>
    /// <param name="suffix">Email domain suffix</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>District if found, null otherwise</returns>
    Task<District?> GetBySuffixAsync(string suffix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active (non-deleted) districts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active districts</returns>
    Task<List<District>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a suffix is already in use by an active district (case-insensitive).
    /// </summary>
    /// <param name="suffix">Email domain suffix to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if suffix exists, false otherwise</returns>
    Task<bool> SuffixExistsAsync(string suffix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a suffix is unique (case-insensitive). Returns true if suffix is NOT in use.
    /// </summary>
    /// <param name="suffix">Email domain suffix to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if suffix is unique (not in use), false otherwise</returns>
    Task<bool> IsSuffixUniqueAsync(string suffix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a suffix is unique for updating a district (excludes the district being updated).
    /// </summary>
    /// <param name="suffix">Email domain suffix to check</param>
    /// <param name="excludeDistrictId">District ID to exclude from check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if suffix is unique, false otherwise</returns>
    Task<bool> IsSuffixUniqueAsync(string suffix, Guid excludeDistrictId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list of active districts ordered by name.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of districts for the requested page</returns>
    Task<List<District>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count of active districts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total number of active districts</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count of active district admins for a district.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of active admins</returns>
    Task<int> GetActiveAdminCountAsync(Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count of pending (unverified) district admins for a district.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of pending admins</returns>
    Task<int> GetPendingAdminCountAsync(Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count of revoked district admins for a district.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of revoked admins</returns>
    Task<int> GetRevokedAdminCountAsync(Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new district to the repository.
    /// </summary>
    /// <param name="district">District to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(District district, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing district in the repository.
    /// </summary>
    /// <param name="district">District to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(District district, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a district from the repository (hard delete, used only for testing).
    /// Prefer soft delete via District.Delete() for production scenarios.
    /// </summary>
    /// <param name="district">District to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(District district, CancellationToken cancellationToken = default);
}
