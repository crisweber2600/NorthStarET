using System;
using System.Text.Json;
using FluentAssertions;
using NorthStarET.NextGen.Lms.Contracts.Authentication;

namespace NorthStarET.NextGen.Lms.Application.Tests.Contracts;

public class AuthenticationContractTests
{
    [Fact]
    public void RefreshSessionRequest_ShouldCaptureSessionId()
    {
        var sessionId = Guid.NewGuid();
        var request = new RefreshSessionRequest { SessionId = sessionId };

        request.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public void RefreshSessionRequest_ShouldCaptureEntraToken()
    {
        var sessionId = Guid.NewGuid();
        var entraToken = "valid.jwt.token";
        var request = new RefreshSessionRequest
        {
            SessionId = sessionId,
            EntraToken = entraToken
        };

        request.SessionId.Should().Be(sessionId);
        request.EntraToken.Should().Be(entraToken);
    }

    [Fact]
    public void RefreshSessionResponse_ShouldSerializeToCamelCase()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        var lastActivity = expiresAt.AddMinutes(-5);
        var response = new RefreshSessionResponse
        {
            ExpiresAt = expiresAt,
            LastActivityAt = lastActivity
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        json.Should().Contain("\"expiresAt\"");
        json.Should().Contain("\"lastActivityAt\"");
    }

    [Fact]
    public void RevokeSessionRequest_ShouldCaptureSessionId()
    {
        var sessionId = Guid.NewGuid();
        var request = new RevokeSessionRequest { SessionId = sessionId };

        request.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public void ValidateSessionResponse_ShouldSerializeExpectedProperties()
    {
        var response = new ValidateSessionResponse
        {
            IsValid = true,
            UserId = Guid.NewGuid(),
            ActiveTenantId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        json.Should().Contain("\"isValid\"");
        json.Should().Contain("\"userId\"");
        json.Should().Contain("\"activeTenantId\"");
        json.Should().Contain("\"expiresAt\"");
    }
}
