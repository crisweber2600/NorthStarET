using FluentAssertions;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.DeleteDistrict;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NSubstitute;

namespace NorthStarET.NextGen.Lms.Application.Tests.Districts;

public sealed class DeleteDistrictCommandHandlerTests
{
    private readonly IDistrictRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly DeleteDistrictCommandHandler _handler;

    public DeleteDistrictCommandHandlerTests()
    {
        _repository = Substitute.For<IDistrictRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _handler = new DeleteDistrictCommandHandler(_repository, _dateTimeProvider);
    }

    [Fact]
    public async Task Should_SoftDeleteDistrict_When_ValidCommandProvided()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Test District", "test", _dateTimeProvider);

        var command = new DeleteDistrictCommand(districtId);

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        district.DeletedAt.Should().NotBeNull();
        district.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        command.AuditBeforePayload.Should().NotBeNull();
        command.AuditBeforePayload.Should().Contain("Test District");
        command.AuditAfterPayload.Should().NotBeNull();
        command.AuditAfterPayload.Should().Contain("deletedAtUtc");
    ((IAuditableCommand)command).Action.Should().Be("DeleteDistrict");

        await _repository.Received(1).UpdateAsync(district, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithNotFound_When_DistrictDoesNotExist()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var command = new DeleteDistrictCommand(districtId);

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns((District?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("District.NotFound");
        result.Error!.Message.Should().Contain("District with ID");
        command.AuditBeforePayload.Should().BeNull();
        command.AuditAfterPayload.Should().BeNull();

        await _repository.DidNotReceive().UpdateAsync(
            Arg.Any<District>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Should_EmitDistrictDeletedEvent_When_Deleting()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Test District", "test", _dateTimeProvider);
        district.ClearDomainEvents();

        var command = new DeleteDistrictCommand(districtId);

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        district.DomainEvents.Should().ContainSingle(e => e is DistrictDeletedEvent);
    }

    [Fact]
    public async Task Should_BeIdempotent_When_DistrictAlreadyDeleted()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Test District", "test", _dateTimeProvider);
        district.Delete(_dateTimeProvider);
        var firstDeletedAt = district.DeletedAt;

        var command = new DeleteDistrictCommand(districtId);

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        district.DeletedAt.Should().Be(firstDeletedAt);
        command.AuditAfterPayload.Should().NotBeNull();
        command.AuditAfterPayload.Should().Contain(firstDeletedAt!.Value.ToUniversalTime().ToString("yyyy"));
    }
}
