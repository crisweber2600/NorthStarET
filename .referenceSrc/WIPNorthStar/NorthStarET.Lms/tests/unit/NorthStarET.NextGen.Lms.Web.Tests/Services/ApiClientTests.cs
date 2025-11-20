using FluentAssertions;
using NorthStarET.NextGen.Lms.Web.Services;
using System.Net;

namespace NorthStarET.NextGen.Lms.Web.Tests.Services;

public sealed class ApiClientTests
{
    [Fact]
    public async Task PostAsync_WithNullContent_Should_SendEmptyContent_And_ReturnTrue_When_ApiReturnsSuccess()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.NoContent);
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://localhost:7001")
        };
        var apiClient = new ApiClient(httpClient);

        // Act
        var result = await apiClient.PostAsync<object>("/api/test", null, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        mockHandler.RequestContent.Should().NotBeNull("because null content should be replaced with empty content");
        mockHandler.RequestContent!.Headers.ContentType.Should().BeNull("because bodyless requests should not have Content-Type header");
    }

    [Fact]
    public async Task PostAsync_WithNullContent_Should_ReturnFalse_When_ApiReturnsError()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.BadRequest);
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://localhost:7001")
        };
        var apiClient = new ApiClient(httpClient);

        // Act
        var result = await apiClient.PostAsync<object>("/api/test", null, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PostAsync_WithContent_Should_SendJsonContent_And_ReturnTrue_When_ApiReturnsSuccess()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://localhost:7001")
        };
        var apiClient = new ApiClient(httpClient);
        var content = new { Name = "Test" };

        // Act
        var result = await apiClient.PostAsync("/api/test", content, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        mockHandler.RequestContent.Should().NotBeNull();
    }

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public HttpContent? RequestContent { get; private set; }

        public MockHttpMessageHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestContent = request.Content;
            return Task.FromResult(new HttpResponseMessage(_statusCode));
        }
    }
}
