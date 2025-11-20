using FluentAssertions;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.UpdateDistrict;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NSubstitute;

namespace NorthStarET.NextGen.Lms.Application.Tests.Districts;

public sealed class UpdateDistrictCommandHandlerTests
{
    private readonly IDistrictRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UpdateDistrictCommandHandler _handler;

    public UpdateDistrictCommandHandlerTests()
    {
        _repository = Substitute.For<IDistrictRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _handler = new UpdateDistrictCommandHandler(_repository, _dateTimeProvider);
    }

    [Fact]
    public async Task Should_UpdateDistrict_When_ValidCommandProvided()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Old Name", "old", _dateTimeProvider);

        var command = new UpdateDistrictCommand(districtId, "New Name", "new");

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);
        _repository.IsSuffixUniqueAsync("new", districtId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        district.Name.Should().Be("New Name");
        district.Suffix.Should().Be("new");
        district.UpdatedAtUtc.Should().NotBeNull();

        command.AuditBeforePayload.Should().NotBeNull();
        command.AuditBeforePayload.Should().Contain("Old Name");
        command.AuditAfterPayload.Should().NotBeNull();
        command.AuditAfterPayload.Should().Contain("New Name");
        ((IIdempotentCommand)command).Operation.Should().Be("Districts.Update");
    ((IAuditableCommand)command).Action.Should().Be("UpdateDistrict");
        ((IIdempotentCommand)command).EntityId.Should().Be(districtId);

        await _repository.Received(1).UpdateAsync(district, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithNotFound_When_DistrictDoesNotExist()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var command = new UpdateDistrictCommand(districtId, "New Name", "new");

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
    public async Task Should_FailWithValidationError_When_SuffixNotUnique()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Old Name", "old", _dateTimeProvider);

        var command = new UpdateDistrictCommand(districtId, "New Name", "existing");

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);
        _repository.IsSuffixUniqueAsync("existing", districtId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("District.SuffixNotUnique");
        result.Error!.Message.Should().Contain("Suffix 'existing' is already in use");
        command.AuditBeforePayload.Should().BeNull();
        command.AuditAfterPayload.Should().BeNull();

        await _repository.DidNotReceive().UpdateAsync(
            Arg.Any<District>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Should_AllowSameSuffix_When_UpdatingOwnDistrict()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Old Name", "same", _dateTimeProvider);

        var command = new UpdateDistrictCommand(districtId, "New Name", "same");

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);
        _repository.IsSuffixUniqueAsync("same", districtId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        district.Suffix.Should().Be("same");
        command.NormalizedSuffix.Should().Be("same");
    }

    [Fact]
    public async Task Should_TrimDistrictName_When_Updating()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Old Name", "old", _dateTimeProvider);

        var command = new UpdateDistrictCommand(districtId, "  New Name  ", "new");

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);
        _repository.IsSuffixUniqueAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        district.Name.Should().Be("New Name");
        command.AuditAfterPayload.Should().NotBeNull();
        command.AuditAfterPayload.Should().Contain("New Name");
    }
}
