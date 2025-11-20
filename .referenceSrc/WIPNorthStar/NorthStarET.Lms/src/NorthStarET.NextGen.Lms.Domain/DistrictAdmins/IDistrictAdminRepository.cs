namespace NorthStarET.NextGen.Lms.Domain.DistrictAdmins;

/// <summary>
/// Repository interface for DistrictAdmin aggregate persistence.
/// All queries are tenant-scoped (filter by DistrictId).
/// </summary>
public interface IDistrictAdminRepository
{
    /// <summary>
    /// Retrieves a district admin by their unique identifier.
    /// </summary>
    /// <param name="id">District admin ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>DistrictAdmin if found, null otherwise</returns>
    Task<DistrictAdmin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a district admin by email within a specific district (case-insensitive).
    /// </summary>
    /// <param name="districtId">District ID for tenant scoping</param>
    /// <param name="email">Admin email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>DistrictAdmin if found, null otherwise</returns>
    Task<DistrictAdmin?> GetByEmailAsync(Guid districtId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all admins for a specific district.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of district admins</returns>
    Task<List<DistrictAdmin>> GetByDistrictIdAsync(Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active (verified) admins for a specific district.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active district admins</returns>
    Task<List<DistrictAdmin>> GetActiveByDistrictIdAsync(Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all admins with pending (unverified, non-expired) invitations for a specific district.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of unverified district admins with valid invitations</returns>
    Task<List<DistrictAdmin>> GetPendingByDistrictIdAsync(Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already associated with an admin in a specific district (case-insensitive).
    /// </summary>
    /// <param name="districtId">District ID for tenant scoping</param>
    /// <param name="email">Admin email address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email exists, false otherwise</returns>
    Task<bool> EmailExistsAsync(Guid districtId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new district admin to the repository.
    /// </summary>
    /// <param name="admin">District admin to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(DistrictAdmin admin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing district admin in the repository.
    /// </summary>
    /// <param name="admin">District admin to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(DistrictAdmin admin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a district admin from the repository (hard delete, used only for testing).
    /// Prefer revocation via DistrictAdmin.Revoke() for production scenarios.
    /// </summary>
    /// <param name="admin">District admin to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(DistrictAdmin admin, CancellationToken cancellationToken = default);
}
