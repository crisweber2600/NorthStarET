using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NorthStarET.NextGen.Lms.Api.Controllers;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.InviteDistrictAdmin;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.ResendInvite;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.RevokeDistrictAdmin;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Queries.ListDistrictAdmins;
using NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NSubstitute;
using Xunit;

namespace NorthStarET.NextGen.Lms.Api.Tests.DistrictAdmins;

public sealed class DistrictAdminsControllerTests
{
    private readonly IMediator _mediator;
    private readonly DistrictAdminsController _controller;

    public DistrictAdminsControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _controller = new DistrictAdminsController(_mediator);
    }

    [Fact]
    public async Task InviteAsync_Should_ReturnCreatedResult_When_CommandSucceeds()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var request = new InviteDistrictAdminRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@demo.org"
        };
        var response = new InviteDistrictAdminResponse
        {
            Id = Guid.NewGuid(),
            DistrictId = districtId,
            Email = "john.doe@demo.org",
            Status = "Unverified",
            InvitedAtUtc = DateTime.UtcNow,
            InvitationExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };
        var result = Result.Success(response);

        _mediator.Send(Arg.Any<InviteDistrictAdminCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.InviteAsync(districtId, request, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)actionResult;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task InviteAsync_Should_ReturnBadRequest_When_CommandFails()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var request = new InviteDistrictAdminRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@other.org"
        };
        var result = Result.Failure<InviteDistrictAdminResponse>(
            new Error("DistrictAdmin.InvalidSuffix", "Email domain does not match district suffix"));

        _mediator.Send(Arg.Any<InviteDistrictAdminCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.InviteAsync(districtId, request, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)actionResult;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ListAsync_Should_ReturnOkResult_When_QuerySucceeds()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var admins = new List<DistrictAdminResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DistrictId = districtId,
                Email = "admin1@demo.org",
                Status = DistrictAdminStatus.Verified.ToString(),
                InvitedAtUtc = DateTime.UtcNow.AddDays(-5),
                VerifiedAtUtc = DateTime.UtcNow.AddDays(-4)
            },
            new()
            {
                Id = Guid.NewGuid(),
                DistrictId = districtId,
                Email = "admin2@demo.org",
                Status = DistrictAdminStatus.Unverified.ToString(),
                InvitedAtUtc = DateTime.UtcNow.AddDays(-2),
                VerifiedAtUtc = null
            }
        };
        var result = Result.Success<IReadOnlyList<DistrictAdminResponse>>(admins);

        _mediator.Send(Arg.Any<ListDistrictAdminsQuery>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.ListAsync(districtId, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(admins);
    }

    [Fact]
    public async Task ResendInviteAsync_Should_ReturnNoContent_When_CommandSucceeds()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var result = Result.Success();

        _mediator.Send(Arg.Any<ResendInviteCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.ResendInviteAsync(districtId, adminId, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
        var noContent = (NoContentResult)actionResult;
        noContent.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task ResendInviteAsync_Should_ReturnBadRequest_When_CommandFails()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var result = Result.Failure(
            new Error("DistrictAdmin.ResendFailed", "Can only resend invitation for unverified admins"));

        _mediator.Send(Arg.Any<ResendInviteCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.ResendInviteAsync(districtId, adminId, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)actionResult;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RevokeAsync_Should_ReturnNoContent_When_CommandSucceeds()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var result = Result.Success();

        _mediator.Send(Arg.Any<RevokeDistrictAdminCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.RevokeAsync(districtId, adminId, "User request", CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
        var noContent = (NoContentResult)actionResult;
        noContent.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task RevokeAsync_Should_ReturnBadRequest_When_CommandFails()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var result = Result.Failure(
            new Error("DistrictAdmin.RevokeFailed", "Admin is already revoked"));

        _mediator.Send(Arg.Any<RevokeDistrictAdminCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.RevokeAsync(districtId, adminId, "User request", CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)actionResult;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RevokeAsync_Should_ReturnNotFound_When_AdminDoesNotExist()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var result = Result.Failure(
            new Error("DistrictAdmin.NotFound", "District admin not found"));

        _mediator.Send(Arg.Any<RevokeDistrictAdminCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.RevokeAsync(districtId, adminId, "User request", CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NotFoundObjectResult>();
        var notFound = (NotFoundObjectResult)actionResult;
        notFound.StatusCode.Should().Be(404);
    }
}
