using FluentAssertions;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.InviteDistrictAdmin;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NSubstitute;
using Xunit;

namespace NorthStarET.NextGen.Lms.Application.Tests.DistrictAdmins;

public sealed class InviteDistrictAdminCommandHandlerTests
{
    private readonly IDistrictRepository _districtRepository;
    private readonly IDistrictAdminRepository _adminRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly InviteDistrictAdminCommandHandler _handler;

    public InviteDistrictAdminCommandHandlerTests()
    {
        _districtRepository = Substitute.For<IDistrictRepository>();
        _adminRepository = Substitute.For<IDistrictAdminRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _handler = new InviteDistrictAdminCommandHandler(_districtRepository, _adminRepository, _dateTimeProvider);
    }

    [Fact]
    public async Task Should_InviteDistrictAdmin_When_ValidCommandProvided()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Demo District", "demo.org", _dateTimeProvider);
        var command = new InviteDistrictAdminCommand(
            districtId,
            "John",
            "Doe",
            "john.doe@demo.org");

        _districtRepository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);
        _adminRepository.GetByEmailAsync(districtId, command.Email, Arg.Any<CancellationToken>())
            .Returns((DistrictAdmin?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().NotBe(Guid.Empty);
        result.Value.DistrictId.Should().Be(districtId);
        result.Value.Email.Should().Be("john.doe@demo.org");
        result.Value.Status.Should().Be("Unverified");
        result.Value.InvitedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.InvitationExpiresAtUtc.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(7),
            TimeSpan.FromSeconds(5));

        await _adminRepository.Received(1).AddAsync(
            Arg.Is<DistrictAdmin>(a =>
                a.DistrictId == districtId &&
                a.Email == "john.doe@demo.org" &&
                a.Status == DistrictAdminStatus.Unverified),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithNotFound_When_DistrictDoesNotExist()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var command = new InviteDistrictAdminCommand(
            districtId,
            "John",
            "Doe",
            "john.doe@demo.org");

        _districtRepository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns((District?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("District.NotFound");

        await _adminRepository.DidNotReceive().AddAsync(
            Arg.Any<DistrictAdmin>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithInvalidSuffix_When_EmailDomainDoesNotMatchDistrict()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Demo District", "demo.org", _dateTimeProvider);
        var command = new InviteDistrictAdminCommand(
            districtId,
            "John",
            "Doe",
            "john.doe@other.org");

        _districtRepository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("DistrictAdmin.InvalidSuffix");
        result.Error!.Message.Should().Contain("Email domain does not match district suffix");

        await _adminRepository.DidNotReceive().AddAsync(
            Arg.Any<DistrictAdmin>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithDuplicateEmail_When_AdminAlreadyExists()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Demo District", "demo.org", _dateTimeProvider);
        var existingAdmin = DistrictAdmin.Create(Guid.NewGuid(), districtId, "john.doe@demo.org", _dateTimeProvider);
        var command = new InviteDistrictAdminCommand(
            districtId,
            "John",
            "Doe",
            "john.doe@demo.org");

        _districtRepository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);
        _adminRepository.GetByEmailAsync(districtId, command.Email, Arg.Any<CancellationToken>())
            .Returns(existingAdmin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("DistrictAdmin.DuplicateEmail");
        result.Error!.Message.Should().Contain("Admin with this email already exists");

        await _adminRepository.DidNotReceive().AddAsync(
            Arg.Any<DistrictAdmin>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_NormalizeEmailToLowercase_When_InvitingAdmin()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Demo District", "demo.org", _dateTimeProvider);
        var command = new InviteDistrictAdminCommand(
            districtId,
            "John",
            "Doe",
            "John.Doe@DEMO.ORG");

        _districtRepository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);
        _adminRepository.GetByEmailAsync(districtId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((DistrictAdmin?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("john.doe@demo.org");

        await _adminRepository.Received(1).AddAsync(
            Arg.Is<DistrictAdmin>(a => a.Email == "john.doe@demo.org"),
            Arg.Any<CancellationToken>());
    }
}
