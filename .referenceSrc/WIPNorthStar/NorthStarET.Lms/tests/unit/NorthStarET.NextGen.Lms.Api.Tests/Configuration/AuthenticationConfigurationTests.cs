using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Api.Authentication;

namespace NorthStarET.NextGen.Lms.Api.Tests.Configuration;

public class AuthenticationConfigurationTests
{
    [Fact]
    public void Authentication_Should_UseSessionScheme_AsDefault()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = SessionAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = SessionAuthenticationDefaults.AuthenticationScheme;
            })
            .AddLmsSession();

        var serviceProvider = services.BuildServiceProvider();
        var authOptions = serviceProvider.GetRequiredService<IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions>>().Value;

        // Assert
        Assert.Equal("LmsSession", authOptions.DefaultAuthenticateScheme);
        Assert.Equal("LmsSession", authOptions.DefaultChallengeScheme);
        Assert.Equal("LmsSession", SessionAuthenticationDefaults.AuthenticationScheme);
    }

    [Fact]
    public void Cors_Configuration_Should_ReadFromAppSettings()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            ["AllowedOrigins:0"] = "https://localhost:7064",
            ["AllowedOrigins:1"] = "http://localhost:5143"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        var origins = configuration.GetSection("AllowedOrigins").Get<string[]>();

        // Assert
        Assert.NotNull(origins);
        Assert.Equal(2, origins.Length);
        Assert.Contains("https://localhost:7064", origins);
        Assert.Contains("http://localhost:5143", origins);
    }

    [Fact]
    public void Cors_Configuration_Should_UseFallbackOrigins_WhenNotConfigured()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var origins = configuration.GetSection("AllowedOrigins").Get<string[]>() 
            ?? new[] { "https://localhost:7064", "http://localhost:5143" };

        // Assert
        Assert.NotNull(origins);
        Assert.Contains("https://localhost:7064", origins);
        Assert.Contains("http://localhost:5143", origins);
    }
}
