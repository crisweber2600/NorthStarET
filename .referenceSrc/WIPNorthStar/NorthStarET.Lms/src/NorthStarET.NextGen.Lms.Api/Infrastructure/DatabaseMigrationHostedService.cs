using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence;

namespace NorthStarET.NextGen.Lms.Api.Infrastructure;

internal sealed class DatabaseMigrationHostedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IHostEnvironment hostEnvironment;
    private readonly ILogger<DatabaseMigrationHostedService> logger;

    public DatabaseMigrationHostedService(
        IServiceProvider serviceProvider,
        IHostEnvironment hostEnvironment,
        ILogger<DatabaseMigrationHostedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.hostEnvironment = hostEnvironment;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Only run automatic migrations in development; other environments should rely on orchestrated upgrades.
        if (!hostEnvironment.IsDevelopment())
        {
            logger.LogInformation("Skipping automatic database migrations for environment {Environment}", hostEnvironment.EnvironmentName);
            return;
        }

        logger.LogInformation("Applying database migrations for identity and districts contexts");

        using var scope = serviceProvider.CreateScope();
        var identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var districtsContext = scope.ServiceProvider.GetRequiredService<DistrictsDbContext>();

        await ApplyMigrationsAsync(identityContext, nameof(IdentityDbContext), cancellationToken);
        await ApplyMigrationsAsync(districtsContext, nameof(DistrictsDbContext), cancellationToken);

        logger.LogInformation("Database migrations completed successfully");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task ApplyMigrationsAsync(DbContext context, string contextName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Ensuring migrations are applied for {Context}", contextName);
        await context.Database.MigrateAsync(cancellationToken);
    }
}
