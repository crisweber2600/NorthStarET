using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorthStarET.Foundation.Identity.Infrastructure.Caching;
using NorthStarET.Foundation.Identity.Infrastructure.Data;

namespace NorthStarET.Foundation.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("IdentityDb")
                ?? throw new InvalidOperationException("IdentityDb connection string not configured");
                
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });
            
            // Enable sensitive data logging in development
            if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }
        });
        
        // Add Redis distributed cache (configured via Aspire)
        services.AddStackExchangeRedisCache(options =>
        {
            var redisConfig = configuration["Redis:Configuration"];
            if (!string.IsNullOrEmpty(redisConfig))
            {
                options.Configuration = redisConfig;
            }
        });
        
        // Add session caching service
        services.AddScoped<ISessionCacheService, SessionCacheService>();
        
        // TODO: Add MassTransit messaging
        // TODO: Add repositories
        
        return services;
    }
}
