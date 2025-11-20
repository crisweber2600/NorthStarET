using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services;

public interface ITokenExchangeService
{
    Task<TokenExchangeResult> ExchangeAsync(TokenExchangeCommandContext context, CancellationToken cancellationToken = default);
}
