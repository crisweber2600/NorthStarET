using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

public interface IAuthorizationAuditRepository : IRepository<AuthorizationAuditLog>
{
}
