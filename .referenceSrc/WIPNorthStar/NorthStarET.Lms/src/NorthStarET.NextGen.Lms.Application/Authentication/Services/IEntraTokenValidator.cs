using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services;

public interface IEntraTokenValidator
{
    Task<ClaimsPrincipal> ValidateAsync(string entraToken, CancellationToken cancellationToken = default);
}
