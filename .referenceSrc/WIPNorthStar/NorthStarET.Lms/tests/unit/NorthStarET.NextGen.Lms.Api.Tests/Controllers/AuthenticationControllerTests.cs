using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NorthStarET.NextGen.Lms.Api.Controllers;
using NorthStarET.NextGen.Lms.Application.Authentication.Commands;
using NorthStarET.NextGen.Lms.Application.Authentication.Queries;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Contracts.Authentication;
using Xunit;

namespace NorthStarET.NextGen.Lms.Api.Tests.Controllers;

public class AuthenticationControllerTests
{
    private readonly Mock<IMediator> mediatorMock = new();
    private readonly AuthenticationController controller;

    public AuthenticationControllerTests()
    {
        controller = new AuthenticationController(mediatorMock.Object, NullLogger<AuthenticationController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task ExchangeToken_ShouldReturnOkWithMappedResponse()
    {
        var request = new TokenExchangeRequest
        {
            EntraToken = "entra-token",
            ActiveTenantId = Guid.NewGuid(),
            IpAddress = "127.0.0.1",
            UserAgent = "agent"
        };

        var result = BuildTokenExchangeResult(request.ActiveTenantId);

        mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<ExchangeEntraTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var actionResult = await controller.ExchangeToken(request, CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<TokenExchangeResponse>().Subject;
        response.SessionId.Should().Be(result.SessionId);
        response.LmsAccessToken.Should().Be(result.LmsAccessToken);
        response.ExpiresAt.Should().Be(result.ExpiresAt);
        response.User.Id.Should().Be(result.User.Id);
        response.User.ActiveTenantId.Should().Be(result.User.ActiveTenantId);
        response.User.ActiveTenantName.Should().Be(result.User.ActiveTenantName);
        response.User.ActiveTenantType.Should().Be(result.User.ActiveTenantType);
        response.User.Role.Should().Be(result.User.Role);
        response.User.AvailableTenants.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCurrentUser_WhenHeaderMissing_ShouldReturnBadRequest()
    {
        var result = await controller.GetCurrentUser(CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUserContext()
    {
        var sessionId = Guid.NewGuid();
        controller.HttpContext.Request.Headers[AuthenticationHeaders.SessionId] = sessionId.ToString();

        var now = DateTimeOffset.UtcNow.AddMinutes(30);
        var queryResult = new GetCurrentUserContextResult(
            sessionId,
            now,
            new UserContextModel(Guid.NewGuid(), "user@example.com", "User Example", Guid.NewGuid(), "Tenant", "District", "Teacher"),
            new[] { new TenantSummaryModel(Guid.NewGuid(), "Tenant", "District") });

        mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetCurrentUserContextQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var actionResult = await controller.GetCurrentUser(CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<UserContextDto>().Subject;
        dto.Id.Should().Be(queryResult.User.Id);
        dto.Email.Should().Be(queryResult.User.Email);
        dto.DisplayName.Should().Be(queryResult.User.DisplayName);
        dto.ActiveTenantId.Should().Be(queryResult.User.ActiveTenantId);
        dto.Role.Should().Be(queryResult.User.Role);
        dto.AvailableTenants.Should().HaveCount(1);
    }

    [Fact]
    public async Task ValidateSession_ShouldReturnResponse()
    {
        var sessionId = Guid.NewGuid();
        controller.HttpContext.Request.Headers[AuthenticationHeaders.SessionId] = sessionId.ToString();

        var queryResult = new ValidateSessionResult(sessionId, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(30), DateTimeOffset.UtcNow);

        mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<ValidateSessionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var actionResult = await controller.ValidateSession(CancellationToken.None);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<ValidateSessionResponse>().Subject;
        dto.UserId.Should().Be(queryResult.UserId);
        dto.ActiveTenantId.Should().Be(queryResult.ActiveTenantId);
        dto.ExpiresAt.Should().Be(queryResult.ExpiresAt);
        dto.IsValid.Should().BeTrue();
    }

    private static TokenExchangeResult BuildTokenExchangeResult(Guid activeTenantId)
    {
        var userId = Guid.NewGuid();
        var tenantSummary = new TenantSummaryModel(activeTenantId, "Tenant", "District");

        return new TokenExchangeResult(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(30),
            "lms-token",
            new UserContextModel(userId, "user@example.com", "User Example", activeTenantId, "Tenant", "District", "Teacher"),
            new List<TenantSummaryModel> { tenantSummary });
    }
}
