using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NorthStarET.NextGen.Lms.Api.Controllers;
using NorthStarET.NextGen.Lms.Application.Authentication.Commands;
using NorthStarET.NextGen.Lms.Application.Authentication.Queries;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Contracts;
using NorthStarET.NextGen.Lms.Contracts.Authentication;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;
using Xunit;

namespace NorthStarET.NextGen.Lms.Api.Tests.Controllers;

public class SessionsControllerTests
{
    private readonly Mock<IMediator> mediatorMock = new();
    private readonly Mock<IEntraTokenValidator> entraTokenValidatorMock = new();
    private readonly Mock<ILmsTokenGenerator> lmsTokenGeneratorMock = new();
    private readonly Mock<IUserRepository> userRepositoryMock = new();
    private readonly SessionsController controller;

    public SessionsControllerTests()
    {
        // Setup mock to return a sample JWT token
        lmsTokenGeneratorMock
            .Setup(g => g.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()))
            .Returns("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test.token");

        var settings = new IdentityModuleSettings { SessionSlidingExpirationMinutes = 30 };
        var options = Options.Create(settings);

        controller = new SessionsController(
            mediatorMock.Object,
            NullLogger<SessionsController>.Instance,
            entraTokenValidatorMock.Object,
            lmsTokenGeneratorMock.Object,
            userRepositoryMock.Object,
            options)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task RefreshSession_WithValidEntraToken_ShouldReturnOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var entraSubjectId = "test-subject-123";
        var entraToken = "valid.entra.token";
        var request = new RefreshSessionRequest
        {
            SessionId = sessionId,
            EntraToken = entraToken
        };

        var sessionResult = new ValidateSessionResult(
            sessionId,
            userId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(30),
            DateTimeOffset.UtcNow);

        // Setup ClaimsPrincipal with subject claim
        var claims = new[] { new Claim("sub", entraSubjectId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        entraTokenValidatorMock
            .Setup(v => v.ValidateAsync(entraToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(principal);

        // Setup user with matching EntraSubjectId
        var user = new User(userId, new EntraSubjectId(entraSubjectId), "test@example.com", "Test", "User", DateTimeOffset.UtcNow, true);
        userRepositoryMock
            .Setup(r => r.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<RefreshSessionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<ValidateSessionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);

        // Act
        var actionResult = await controller.RefreshSession(sessionId, request, CancellationToken.None);

        // Assert
        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<RefreshSessionResponse>().Subject;
        response.ExpiresAt.Should().Be(sessionResult.ExpiresAt);
        response.LastActivityAt.Should().Be(sessionResult.LastActivityAt);

        entraTokenValidatorMock.Verify(
            v => v.ValidateAsync(entraToken, It.IsAny<CancellationToken>()),
            Times.Once);
        userRepositoryMock.Verify(
            r => r.GetAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
        mediatorMock.Verify(
            m => m.Send(It.Is<RefreshSessionCommand>(c => c.SessionId == sessionId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshSession_WithTokenSubjectMismatch_ShouldReturnUnauthorized()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var correctEntraSubjectId = "correct-subject-123";
        var wrongEntraSubjectId = "wrong-subject-456";
        var entraToken = "valid.but.wrong.token";
        var request = new RefreshSessionRequest
        {
            SessionId = sessionId,
            EntraToken = entraToken
        };

        var sessionResult = new ValidateSessionResult(
            sessionId,
            userId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(30),
            DateTimeOffset.UtcNow);

        // Setup ClaimsPrincipal with WRONG subject claim
        var claims = new[] { new Claim("sub", wrongEntraSubjectId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        entraTokenValidatorMock
            .Setup(v => v.ValidateAsync(entraToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(principal);

        // Setup user with CORRECT EntraSubjectId (different from token)
        var user = new User(userId, new EntraSubjectId(correctEntraSubjectId), "test@example.com", "Test", "User", DateTimeOffset.UtcNow, true);
        userRepositoryMock
            .Setup(r => r.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        mediatorMock
            .Setup(m => m.Send(It.IsAny<ValidateSessionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionResult);

        // Act
        var actionResult = await controller.RefreshSession(sessionId, request, CancellationToken.None);

        // Assert
        var unauthorized = actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorResponse = unauthorized.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Error.Should().Be("Token does not belong to the session owner.");

        // Verify the refresh command was never called
        mediatorMock.Verify(
            m => m.Send(It.IsAny<RefreshSessionCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshSession_WithEmptyEntraToken_ShouldReturnBadRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new RefreshSessionRequest
        {
            SessionId = sessionId,
            EntraToken = string.Empty
        };

        // Act
        var actionResult = await controller.RefreshSession(sessionId, request, CancellationToken.None);

        // Assert
        var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = badRequest.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Error.Should().Be("Entra token is required for session refresh.");

        entraTokenValidatorMock.Verify(
            v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshSession_WithInvalidEntraToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var entraToken = "invalid.entra.token";
        var request = new RefreshSessionRequest
        {
            SessionId = sessionId,
            EntraToken = entraToken
        };

        entraTokenValidatorMock
            .Setup(v => v.ValidateAsync(entraToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SecurityTokenException("Invalid token"));

        // Act
        var actionResult = await controller.RefreshSession(sessionId, request, CancellationToken.None);

        // Assert
        var unauthorized = actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var errorResponse = unauthorized.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Error.Should().Be("Invalid Entra token.");

        mediatorMock.Verify(
            m => m.Send(It.IsAny<RefreshSessionCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshSession_WhenSessionNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var entraToken = "valid.entra.token";
        var request = new RefreshSessionRequest
        {
            SessionId = sessionId,
            EntraToken = entraToken
        };

        entraTokenValidatorMock
            .Setup(v => v.ValidateAsync(entraToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new System.Security.Claims.ClaimsPrincipal());

        mediatorMock
            .Setup(m => m.Send(It.IsAny<ValidateSessionQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Session not found"));

        // Act
        var actionResult = await controller.RefreshSession(sessionId, request, CancellationToken.None);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RevokeSession_WhenSessionExists_ShouldReturnNoContent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new RevokeSessionRequest { SessionId = sessionId };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<RevokeSessionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var actionResult = await controller.RevokeSession(sessionId, request, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
        mediatorMock.Verify(
            m => m.Send(It.Is<RevokeSessionCommand>(c => c.SessionId == sessionId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RevokeSession_WhenSessionNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new RevokeSessionRequest { SessionId = sessionId };

        mediatorMock
            .Setup(m => m.Send(It.IsAny<RevokeSessionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Session not found"));

        // Act
        var actionResult = await controller.RevokeSession(sessionId, request, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }
}
