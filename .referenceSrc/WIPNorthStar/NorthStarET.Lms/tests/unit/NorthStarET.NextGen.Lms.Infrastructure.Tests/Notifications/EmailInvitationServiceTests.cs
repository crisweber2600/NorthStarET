using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NorthStarET.NextGen.Lms.Infrastructure.Notifications;
using Xunit;

namespace NorthStarET.NextGen.Lms.Infrastructure.Tests.Notifications;

public sealed class EmailInvitationServiceTests
{
    private readonly IEmailFailureHandler _failureHandler;
    private readonly ILogger<EmailInvitationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailInvitationServiceTests()
    {
        _failureHandler = Substitute.For<IEmailFailureHandler>();
        _logger = Substitute.For<ILogger<EmailInvitationService>>();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
    }

    [Fact]
    public async Task SendInvitationAsync_Should_UseServiceDiscoveryToResolveWebServiceUrl()
    {
        // Arrange
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://test.example.com")
        };
        _httpClientFactory.CreateClient("WebService").Returns(httpClient);

        var service = new EmailInvitationService(
            _failureHandler,
            _logger,
            _httpClientFactory);

        var email = "admin@test.com";
        var invitationToken = Guid.NewGuid().ToString();
        var districtName = "Test District";
        var expiresAtUtc = DateTime.UtcNow.AddDays(7);

        // Act
        var result = await service.SendInvitationAsync(
            email,
            invitationToken,
            districtName,
            expiresAtUtc,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        
        // Verify that the logger was called with the correct verification link from service discovery
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"https://test.example.com/verify?token={invitationToken}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [InlineData("https://localhost:7002")]
    [InlineData("https://staging.lms.northstaret.org")]
    [InlineData("https://lms.northstaret.org")]
    [InlineData("http://northstaret-nextgen-lms-web")]
    public async Task SendInvitationAsync_Should_SupportDifferentEnvironmentUrls(string baseUrl)
    {
        // Arrange
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        _httpClientFactory.CreateClient("WebService").Returns(httpClient);

        var service = new EmailInvitationService(
            _failureHandler,
            _logger,
            _httpClientFactory);

        var email = "admin@test.com";
        var invitationToken = Guid.NewGuid().ToString();
        var districtName = "Test District";
        var expiresAtUtc = DateTime.UtcNow.AddDays(7);

        // Act
        var result = await service.SendInvitationAsync(
            email,
            invitationToken,
            districtName,
            expiresAtUtc,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        
        // Verify that the logger was called with the correct base URL for this environment
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"{baseUrl}/verify?token={invitationToken}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
