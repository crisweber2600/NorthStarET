using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence;

internal sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine("src", "NorthStarET.NextGen.Lms.Api", "appsettings.json"), optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddJsonFile(Path.Combine("src", "NorthStarET.NextGen.Lms.Api", $"appsettings.{environment}.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("IdentityDb")
            ?? configuration.GetConnectionString("IdentityPostgres")
            ?? configuration["Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:ConnectionString"]
            ?? configuration[$"Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:{nameof(IdentityDbContext)}:ConnectionString"]
            ?? configuration.GetSection("PostgreSQL")["ConnectionString"]
            ?? throw new InvalidOperationException("PostgreSQL connection string configuration is required to create the IdentityDbContext.");

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString, builder =>
        {
            builder.MigrationsHistoryTable("__EFMigrationsHistory", IdentityDbContext.SchemaName);
        });

        return new IdentityDbContext(optionsBuilder.Options);
    }
}
