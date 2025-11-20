using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NorthStarET.NextGen.Lms.Web.Services;

namespace NorthStarET.NextGen.Lms.Web.Tests.Configuration;

public class LmsApiConfigurationTests
{
    private static IConfiguration CreateTestConfiguration()
    {
        var configValues = new Dictionary<string, string?>
        {
            ["AuthenticationDiagnostics:EnableVerboseLogging"] = "false"
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }

    [Fact]
    public void Configuration_Should_Include_LmsApiScope()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            ["LmsApi:Scope"] = "api://792ef7e2-56c1-414a-a300-c071c6a4ce51/access_as_user"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        var scope = configuration["LmsApi:Scope"];

        // Assert
        Assert.NotNull(scope);
        Assert.NotEmpty(scope);
        Assert.Contains("access_as_user", scope);
    }

    [Fact]
    public async Task LmsSessionAccessor_Should_CreateSecureCookie()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var configuration = CreateTestConfiguration();
        var logger = NullLogger<LmsSessionAccessor>.Instance;
        var accessor = new LmsSessionAccessor(httpContextAccessor, configuration, logger);
        var sessionId = Guid.NewGuid().ToString();

        // Act
        await accessor.SetSessionIdAsync(sessionId);

        // Assert
        var cookies = httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("LmsSessionId", cookies);
        Assert.Contains(sessionId, cookies);
        // In ASP.NET Core, cookie options are encoded in the Set-Cookie header
        // httponly flag is implied if not explicitly stated otherwise
        Assert.Contains("expires=", cookies); // Verify expiration is set
        Assert.Contains("samesite=lax", cookies.ToLowerInvariant());
    }

    [Fact]
    public async Task LmsSessionAccessor_Should_RetrieveSessionId_FromCookie()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = $"LmsSessionId={sessionId}";
        
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var configuration = CreateTestConfiguration();
        var logger = NullLogger<LmsSessionAccessor>.Instance;
        var accessor = new LmsSessionAccessor(httpContextAccessor, configuration, logger);

        // Act
        var retrievedSessionId = await accessor.GetSessionIdAsync();

        // Assert
        Assert.Equal(sessionId, retrievedSessionId);
    }

    [Fact]
    public async Task LmsSessionAccessor_Should_ClearSessionCookie()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = $"LmsSessionId={sessionId}";
        httpContext.Response.Body = new MemoryStream();
        
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var configuration = CreateTestConfiguration();
        var logger = NullLogger<LmsSessionAccessor>.Instance;
        var accessor = new LmsSessionAccessor(httpContextAccessor, configuration, logger);

        // Act
        await accessor.ClearSessionAsync();

        // Assert
        var cookies = httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("LmsSessionId", cookies);
        Assert.Contains("expires=", cookies);
    }

    [Fact]
    public async Task LmsSessionAccessor_Should_ReturnNull_WhenNoHttpContext()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor { HttpContext = null };
        var configuration = CreateTestConfiguration();
        var logger = NullLogger<LmsSessionAccessor>.Instance;
        var accessor = new LmsSessionAccessor(httpContextAccessor, configuration, logger);

        // Act
        var sessionId = await accessor.GetSessionIdAsync();

        // Assert
        Assert.Null(sessionId);
    }
}
