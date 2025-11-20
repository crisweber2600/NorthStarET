using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Authorization.Services;
using System.Reflection;

namespace NorthStarET.NextGen.Lms.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR 12.x registration (downgraded from 13.x to avoid licensing requirements)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<ITokenExchangeService, TokenExchangeService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        return services;
    }
}
