using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NorthStarET.NextGen.Lms.Api.Controllers;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.CreateDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.UpdateDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.DeleteDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.GetDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.ListDistricts;
using NorthStarET.NextGen.Lms.Contracts.Districts;
using NorthStarET.NextGen.Lms.Domain.Common;
using NSubstitute;

namespace NorthStarET.NextGen.Lms.Api.Tests.Districts;

public sealed class DistrictsControllerTests
{
    private readonly IMediator _mediator;
    private readonly DistrictsController _controller;

    public DistrictsControllerTests()
    {
        _mediator = Substitute.For<IMediator>();
        _controller = new DistrictsController(_mediator);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnCreatedResult_When_CommandSucceeds()
    {
        // Arrange
        var request = new CreateDistrictRequest
        {
            Name = "Demo District",
            Suffix = "demo"
        };
        var response = new CreateDistrictResponse(Guid.NewGuid(), "Demo District", "demo", DateTime.UtcNow);
        var result = Result.Success(response);

        _mediator.Send(Arg.Any<CreateDistrictCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.CreateAsync(request, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<CreatedAtRouteResult>();
        var createdResult = (CreatedAtRouteResult)actionResult;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnBadRequest_When_CommandFails()
    {
        // Arrange
        var request = new CreateDistrictRequest
        {
            Name = "Demo District",
            Suffix = "demo"
        };
        var result = Result.Failure<CreateDistrictResponse>(
            new Error("District.SuffixNotUnique", "Suffix 'demo' is already in use"));

        _mediator.Send(Arg.Any<CreateDistrictCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.CreateAsync(request, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)actionResult;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnOkResult_When_DistrictExists()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var response = new DistrictResponse
        {
            Id = districtId,
            Name = "Test District",
            Suffix = "test",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = null,
            DeletedAt = null,
            IsDeleted = false,
            ActiveAdminCount = 5,
            PendingAdminCount = 2,
            RevokedAdminCount = 1
        };
        var result = Result.Success(response);

        _mediator.Send(Arg.Any<GetDistrictQuery>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.GetByIdAsync(districtId, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNotFound_When_DistrictDoesNotExist()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var result = Result.Failure<DistrictResponse>(
            new Error("District.NotFound", "District not found"));

        _mediator.Send(Arg.Any<GetDistrictQuery>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.GetByIdAsync(districtId, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NotFoundObjectResult>();
        var notFound = (NotFoundObjectResult)actionResult;
        notFound.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ListAsync_Should_ReturnOkResult_When_DistrictsExist()
    {
        // Arrange
        var districts = new List<DistrictSummaryResponse>
        {
            new(Guid.NewGuid(), "District A", "a", 4, DateTime.UtcNow, null),
            new(Guid.NewGuid(), "District B", "b", 2, DateTime.UtcNow, null)
        };
        var pagedResult = new PagedResult<DistrictSummaryResponse>(districts, 1, 20, 2);
        var result = Result.Success(pagedResult);

        _mediator.Send(Arg.Any<ListDistrictsQuery>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.ListAsync(1, 20, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(pagedResult);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNoContent_When_CommandSucceeds()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var request = new UpdateDistrictRequest
        {
            Name = "Updated Name",
            Suffix = "updated"
        };
        var result = Result.Success();

        _mediator.Send(Arg.Any<UpdateDistrictCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.UpdateAsync(districtId, request, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
        var noContent = (NoContentResult)actionResult;
        noContent.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnBadRequest_When_CommandFails()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var request = new UpdateDistrictRequest
        {
            Name = "Updated Name",
            Suffix = "existing"
        };
        var result = Result.Failure(new Error("District.SuffixNotUnique", "Suffix 'existing' is already in use"));

        _mediator.Send(Arg.Any<UpdateDistrictCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.UpdateAsync(districtId, request, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)actionResult;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnNoContent_When_CommandSucceeds()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var result = Result.Success();

        _mediator.Send(Arg.Any<DeleteDistrictCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.DeleteAsync(districtId, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
        var noContent = (NoContentResult)actionResult;
        noContent.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnNotFound_When_DistrictDoesNotExist()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var result = Result.Failure(new Error("District.NotFound", "District not found"));

        _mediator.Send(Arg.Any<DeleteDistrictCommand>(), Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actionResult = await _controller.DeleteAsync(districtId, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NotFoundObjectResult>();
        var notFound = (NotFoundObjectResult)actionResult;
        notFound.StatusCode.Should().Be(404);
    }
}
