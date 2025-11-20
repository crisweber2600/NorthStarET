using System;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Microsoft.IdentityModel.Protocols;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Authorization.Services;
using NorthStarET.NextGen.Lms.Application.Common.Abstractions;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Application.Common.Caching;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Domain.Auditing;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.Schools;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Infrastructure.Auditing.Persistence;
using NorthStarET.NextGen.Lms.Infrastructure.Common;
using NorthStarET.NextGen.Lms.Infrastructure.Common.Services;
using NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Caching;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Configuration;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.ExternalServices;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Repositories;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Services;
using NorthStarET.NextGen.Lms.Infrastructure.Notifications;
using NorthStarET.NextGen.Lms.Infrastructure.Notifications.Configuration;
using StackExchange.Redis;

namespace NorthStarET.NextGen.Lms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdentityModuleSettings>(configuration.GetSection("IdentityModule"));
        services.Configure<EntraIdOptions>(configuration.GetSection("EntraId"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
        {
            var connectionString = ResolvePostgresConnectionString(serviceProvider, configuration, nameof(IdentityDbContext));
            ConfigurePostgresOptions(options, connectionString, typeof(IdentityDbContext));
        });

        // Districts schema DbContext (feature 002)
        services.AddDbContext<DistrictsDbContext>((serviceProvider, options) =>
        {
            var connectionString = ResolvePostgresConnectionString(serviceProvider, configuration, nameof(DistrictsDbContext));
            ConfigurePostgresOptions(options, connectionString, typeof(DistrictsDbContext));
        });

        services.AddSingleton<IDocumentRetriever>(_ => new HttpDocumentRetriever());

        // Core services
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IGradeTaxonomyProvider, GradeTaxonomyProvider>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISessionStore, RedisSessionStore>();
        services.AddScoped<IEntraTokenValidator, EntraTokenValidator>();

        // Session-based token generator (no JWT generation needed with Entra ID)
        services.AddScoped<ILmsTokenGenerator, SessionTokenGenerator>();

        services.AddScoped<IAuthorizationCache, AuthorizationCacheService>();
        services.AddScoped<IIdentityAuthorizationDataService, IdentityAuthorizationDataService>();
        services.AddScoped<IAuthorizationAuditRepository, AuthorizationAuditRepository>();

        // Feature 002 repositories
        services.AddScoped<IDistrictRepository, DistrictRepository>();
        services.AddScoped<IDistrictAdminRepository, DistrictAdminRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        // Feature 004 repositories
        services.AddScoped<ISchoolRepository, SchoolRepository>();

        // Feature 002 email notification service with retry and dead-letter queue
        services.AddScoped<IEmailInvitationService, EmailInvitationService>();
        services.AddSingleton<IEmailFailureHandler, EmailFailureHandler>();

        // HttpClient for Web service URL resolution via Aspire service discovery
        services.AddHttpClient("WebService", (serviceProvider, client) =>
        {
            var emailSettings = configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
            client.BaseAddress = new Uri(emailSettings.WebServiceName);
        });

        // Domain event publisher
        services.AddScoped<IDomainEventPublisher, MediatRDomainEventPublisher>();

        // MediatR pipeline behaviors (order matters: Tenant→Idempotency→Auditing→UnitOfWork)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.TenantIsolationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.IdempotencyBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.AuditingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.UnitOfWorkBehavior<,>));

        // Idempotency service for district management (feature 002)
        services.AddSingleton<Idempotency.IIdempotencyService, Idempotency.IdempotencyService>();

        // Register background token refresh service
        services.AddHostedService<TokenRefreshService>();

        services.AddOptions<IdentityModuleSettings>()
            .ValidateDataAnnotations();

        services.AddOptions<EntraIdOptions>()
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId) && !string.IsNullOrWhiteSpace(options.TenantId),
                "EntraId settings must include ClientId and TenantId.");

        services.AddOptions<EmailSettings>()
            .ValidateDataAnnotations();

        return services;
    }

    private static string ResolvePostgresConnectionString(IServiceProvider serviceProvider, IConfiguration configuration, string contextName)
    {
        var connectionString = configuration.GetConnectionString("identity-db")
            ?? configuration.GetConnectionString("IdentityDb")
            ?? configuration["PostgreSQL:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory?
                .CreateLogger("NorthStarET.NextGen.Lms.Infrastructure.PostgresConfiguration")
                .LogInformation(
                    "{Context} configured to use PostgreSQL host {Host} port {Port} database {Database}",
                    contextName,
                    builder.Host,
                    builder.Port,
                    builder.Database);

            return builder.ConnectionString;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Failed to parse PostgreSQL connection string.", exception);
        }
    }

    private static void ConfigurePostgresOptions(DbContextOptionsBuilder options, string connectionString, Type migrationsAssemblySource)
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(migrationsAssemblySource.Assembly.FullName);
        });
    }
}
