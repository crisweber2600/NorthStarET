using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NorthStarET.NextGen.Lms.Contracts.Authentication;
using NorthStarET.NextGen.Lms.Web.Services;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace NorthStarET.NextGen.Lms.Web.Tests.Services;

public sealed class TokenExchangeServiceTests
{
    [Fact]
    public async Task ExchangeTokenAsync_Should_ReturnSessionId_When_ApiReturnsSuccess()
    {
        // Arrange
        var expectedSessionId = Guid.NewGuid();
        var response = new TokenExchangeResponse
        {
            SessionId = expectedSessionId,
            LmsAccessToken = "test-lms-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            User = new UserContextDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                DisplayName = "Test User"
            }
        };

        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, response);
        var httpClientFactory = CreateHttpClientFactory(mockHandler);
        var configuration = CreateConfiguration();
        var logger = NullLogger<TokenExchangeService>.Instance;
        var httpContext = CreateHttpContext();

        var service = new TokenExchangeService(httpClientFactory, configuration, logger);

        // Act
        var result = await service.ExchangeTokenAsync("test-access-token", httpContext);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedSessionId.ToString());
        mockHandler.LastRequest.Should().NotBeNull();
        mockHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();
        mockHandler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        mockHandler.LastRequest.Headers.Authorization.Parameter.Should().Be("test-access-token");
    }

    [Fact]
    public async Task ExchangeTokenAsync_Should_ReturnNull_When_ApiReturnsError()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, null);
        var httpClientFactory = CreateHttpClientFactory(mockHandler);
        var configuration = CreateConfiguration();
        var logger = NullLogger<TokenExchangeService>.Instance;
        var httpContext = CreateHttpContext();

        var service = new TokenExchangeService(httpClientFactory, configuration, logger);

        // Act
        var result = await service.ExchangeTokenAsync("test-access-token", httpContext);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExchangeTokenAsync_Should_IncludeClientIpAddress_In_Request()
    {
        // Arrange
        var expectedSessionId = Guid.NewGuid();
        var response = new TokenExchangeResponse
        {
            SessionId = expectedSessionId,
            LmsAccessToken = "test-lms-token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            User = new UserContextDto()
        };

        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, response);
        var httpClientFactory = CreateHttpClientFactory(mockHandler);
        var configuration = CreateConfiguration();
        var logger = NullLogger<TokenExchangeService>.Instance;
        var httpContext = CreateHttpContext("192.168.1.100");

        var service = new TokenExchangeService(httpClientFactory, configuration, logger);

        // Act
        await service.ExchangeTokenAsync("test-access-token", httpContext);

        // Assert
        mockHandler.LastRequestBody.Should().NotBeNull();
        mockHandler.LastRequestBody!.IpAddress.Should().Be("192.168.1.100");
    }

    [Fact]
    public async Task ExchangeTokenAsync_Should_ReturnNull_When_HttpRequestFails()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(throwException: true);
        var httpClientFactory = CreateHttpClientFactory(mockHandler);
        var configuration = CreateConfiguration();
        var logger = NullLogger<TokenExchangeService>.Instance;
        var httpContext = CreateHttpContext();

        var service = new TokenExchangeService(httpClientFactory, configuration, logger);

        // Act
        var result = await service.ExchangeTokenAsync("test-access-token", httpContext);

        // Assert
        result.Should().BeNull();
    }

    private static IHttpClientFactory CreateHttpClientFactory(HttpMessageHandler handler)
    {
        return new TestHttpClientFactory(handler);
    }

    private static IConfiguration CreateConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "DownstreamApi:BaseUrl", "https://localhost:7001" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private static HttpContext CreateHttpContext(string? remoteIpAddress = null)
    {
        var context = new DefaultHttpContext();
        
        if (remoteIpAddress != null)
        {
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(remoteIpAddress);
        }

        context.Request.Headers["User-Agent"] = "Test User Agent";
        
        return context;
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public TestHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handler);
        }
    }

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly TokenExchangeResponse? _response;
        private readonly bool _throwException;

        public HttpRequestMessage? LastRequest { get; private set; }
        public TokenExchangeRequest? LastRequestBody { get; private set; }

        public MockHttpMessageHandler(HttpStatusCode statusCode, TokenExchangeResponse? response)
        {
            _statusCode = statusCode;
            _response = response;
            _throwException = false;
        }

        public MockHttpMessageHandler(bool throwException)
        {
            _throwException = throwException;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            if (_throwException)
            {
                throw new HttpRequestException("Test exception");
            }

            LastRequest = request;

            // Capture request body
            if (request.Content != null)
            {
                var contentString = await request.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                LastRequestBody = JsonSerializer.Deserialize<TokenExchangeRequest>(contentString, options);
            }

            var responseMessage = new HttpResponseMessage(_statusCode);
            
            if (_response != null && _statusCode == HttpStatusCode.OK)
            {
                responseMessage.Content = JsonContent.Create(_response);
            }

            return responseMessage;
        }
    }
}
