using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Commands;

internal sealed class ExchangeEntraTokenCommandHandler : IRequestHandler<ExchangeEntraTokenCommand, TokenExchangeResult>
{
    private readonly ITokenExchangeService tokenExchangeService;

    public ExchangeEntraTokenCommandHandler(ITokenExchangeService tokenExchangeService)
    {
        this.tokenExchangeService = tokenExchangeService;
    }

    public async Task<TokenExchangeResult> Handle(ExchangeEntraTokenCommand request, CancellationToken cancellationToken)
    {
        var context = new TokenExchangeCommandContext(
            request.EntraToken,
            request.ActiveTenantId,
            request.IpAddress,
            request.UserAgent);

        return await tokenExchangeService.ExchangeAsync(context, cancellationToken);
    }
}
