namespace NorthStarET.NextGen.Lms.Domain.Schools;

/// <summary>
/// Operational status of a school within a district.
/// </summary>
public enum SchoolStatus
{
    /// <summary>
    /// School is actively operating and can be assigned grades.
    /// </summary>
    Active = 0,

    /// <summary>
    /// School is inactive and cannot accept new grade assignments.
    /// Existing grade offerings are retained for historical purposes.
    /// </summary>
    Inactive = 1
}
