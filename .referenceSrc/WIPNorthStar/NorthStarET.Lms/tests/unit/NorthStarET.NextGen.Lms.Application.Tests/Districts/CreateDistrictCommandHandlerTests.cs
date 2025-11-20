using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.CreateDistrict;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NSubstitute;

namespace NorthStarET.NextGen.Lms.Application.Tests.Districts;

public sealed class CreateDistrictCommandHandlerTests
{
    private readonly IDistrictRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CreateDistrictCommandHandler _handler;

    public CreateDistrictCommandHandlerTests()
    {
        _repository = Substitute.For<IDistrictRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _handler = new CreateDistrictCommandHandler(_repository, _dateTimeProvider);
    }

    [Fact]
    public async Task Should_CreateDistrict_When_ValidCommandProvided()
    {
        // Arrange
        var command = new CreateDistrictCommand("Demo District", "demo");
        _repository.IsSuffixUniqueAsync(command.NormalizedSuffix, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().NotBe(Guid.Empty);
        result.Value.Name.Should().Be("Demo District");
        result.Value.Suffix.Should().Be("demo");

        command.DistrictId.Should().Be(result.Value.Id);
        command.IdempotencyEntityId.Should().Be(ComputeDeterministicGuid("demo"));
        command.AuditAfterPayload.Should().NotBeNull();
        command.AuditAfterPayload.Should().Contain("\"name\":\"Demo District\"");
        command.AuditAfterPayload.Should().Contain("\"suffix\":\"demo\"");

        ((IIdempotentCommand)command).Operation.Should().Be("Districts.Create");
    ((IAuditableCommand)command).Action.Should().Be("CreateDistrict");

        await _repository.Received(1).AddAsync(
            Arg.Is<District>(d => d.Name == "Demo District" && d.Suffix == "demo"),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Should_FailWithValidationError_When_SuffixNotUnique()
    {
        // Arrange
        var command = new CreateDistrictCommand("Demo District", "demo");
        _repository.IsSuffixUniqueAsync(command.NormalizedSuffix, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("District.SuffixNotUnique");
        result.Error!.Message.Should().Contain("Suffix 'demo' is already in use");
        command.AuditAfterPayload.Should().BeNull();
        command.DistrictId.Should().Be(Guid.Empty);

        await _repository.DidNotReceive().AddAsync(
            Arg.Any<District>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Should_CheckCaseInsensitiveSuffix_When_Validating()
    {
        // Arrange
        var command = new CreateDistrictCommand("Demo District", "DEMO");
        _repository.IsSuffixUniqueAsync(command.NormalizedSuffix, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _repository.Received(1).IsSuffixUniqueAsync(
            Arg.Is<string>(s => s.ToLowerInvariant() == "demo"),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Should_TrimDistrictName_When_Creating()
    {
        // Arrange
        var command = new CreateDistrictCommand("  Test District  ", "test");
        _repository.IsSuffixUniqueAsync(command.NormalizedSuffix, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Test District");

        await _repository.Received(1).AddAsync(
            Arg.Is<District>(d => d.Name == "Test District"),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public void Should_GenerateDeterministicIdempotencyEntityId_BasedOnNormalizedSuffix()
    {
        // Arrange
        var commandA = new CreateDistrictCommand("Demo A", "MixedCase");
        var commandB = new CreateDistrictCommand("Demo B", "mixedcase");

        // Act / Assert
        commandA.IdempotencyEntityId.Should().Be(commandB.IdempotencyEntityId);
        commandA.IdempotencyEntityId.Should().Be(ComputeDeterministicGuid("mixedcase"));
    }

    private static Guid ComputeDeterministicGuid(string value)
    {
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = md5.ComputeHash(bytes);
        return new Guid(hash);
    }
}
