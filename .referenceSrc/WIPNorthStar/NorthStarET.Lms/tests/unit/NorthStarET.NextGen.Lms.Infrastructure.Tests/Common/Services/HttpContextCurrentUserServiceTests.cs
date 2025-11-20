using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NorthStarET.NextGen.Lms.Domain.Auditing;
using NorthStarET.NextGen.Lms.Infrastructure.Common.Services;
using NSubstitute;
using Xunit;

namespace NorthStarET.NextGen.Lms.Infrastructure.Tests.Common.Services;

public class HttpContextCurrentUserServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContextCurrentUserService _service;

    public HttpContextCurrentUserServiceTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _service = new HttpContextCurrentUserService(_httpContextAccessor);
    }

    [Fact]
    public void UserId_ReturnsUserId_WhenClaimExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _service.UserId;

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void UserId_ReturnsNull_WhenClaimDoesNotExist()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _service.UserId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UserId_ReturnsNull_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = _service.UserId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Role_ReturnsSystemService_WhenUserNotAuthenticated()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _service.Role;

        // Assert
        Assert.Equal(ActorRole.SystemService, result);
    }

    [Fact]
    public void Role_ReturnsPlatformAdmin_WhenRoleClaimIndicatesSystemAdmin()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "SystemAdmin")
        };
        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _service.Role;

        // Assert
        Assert.Equal(ActorRole.PlatformAdmin, result);
    }

    [Fact]
    public void Role_ReturnsDistrictAdmin_WhenRoleClaimIndicatesDistrictAdmin()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "DistrictAdmin")
        };
        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _service.Role;

        // Assert
        Assert.Equal(ActorRole.DistrictAdmin, result);
    }

    [Fact]
    public void Role_ReturnsDistrictAdmin_WhenTenantClaimPresentWithoutRole()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("lms:tenant_id", districtId.ToString())
        };
        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _service.Role;

        // Assert
        Assert.Equal(ActorRole.DistrictAdmin, result);
    }

    [Fact]
    public void Role_ReturnsPlatformAdmin_WhenNoTenantClaimOrRoleClaim()
    {
        // Arrange
        var identity = new ClaimsIdentity(authenticationType: "Test");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _service.Role;

        // Assert
        Assert.Equal(ActorRole.PlatformAdmin, result);
    }

    [Fact]
    public void DistrictId_ReturnsDistrictId_WhenClaimExists()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("lms:tenant_id", districtId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _service.DistrictId;

        // Assert
        Assert.Equal(districtId, result);
    }

    [Fact]
    public void DistrictId_ReturnsNull_WhenClaimDoesNotExist()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _service.DistrictId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DistrictId_ReturnsNull_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = _service.DistrictId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CorrelationId_ReturnsDeterministicGuid_WhenTraceIdentifierExists()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-trace-id";
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result1 = _service.CorrelationId;
        var result2 = _service.CorrelationId;

        // Assert
        Assert.NotNull(result1);
        Assert.Equal(result1, result2); // Should be deterministic for same trace ID
    }

    [Fact]
    public void CorrelationId_ReturnsNull_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = _service.CorrelationId;

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHttpContextAccessorIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HttpContextCurrentUserService(null!));
    }
}
