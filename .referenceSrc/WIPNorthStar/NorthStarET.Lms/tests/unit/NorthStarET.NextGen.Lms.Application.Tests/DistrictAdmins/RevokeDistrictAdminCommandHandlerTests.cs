using FluentAssertions;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.RevokeDistrictAdmin;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NSubstitute;
using Xunit;

namespace NorthStarET.NextGen.Lms.Application.Tests.DistrictAdmins;

public sealed class RevokeDistrictAdminCommandHandlerTests
{
    private readonly IDistrictAdminRepository _adminRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly RevokeDistrictAdminCommandHandler _handler;

    public RevokeDistrictAdminCommandHandlerTests()
    {
        _adminRepository = Substitute.For<IDistrictAdminRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _handler = new RevokeDistrictAdminCommandHandler(_adminRepository, _dateTimeProvider);
    }

    [Fact]
    public async Task Should_RevokeAdmin_When_ValidCommandProvided()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var admin = DistrictAdmin.Create(adminId, districtId, "admin@demo.org", _dateTimeProvider);
        var reason = "User request";

        var command = new RevokeDistrictAdminCommand(districtId, adminId, reason);

        _adminRepository.GetByIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(admin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        admin.Status.Should().Be(DistrictAdminStatus.Revoked);
        admin.RevokedAtUtc.Should().NotBeNull();
        admin.IsActive.Should().BeFalse();

        await _adminRepository.Received(1).UpdateAsync(
            Arg.Is<DistrictAdmin>(a =>
                a.Id == adminId &&
                a.Status == DistrictAdminStatus.Revoked),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_RevokeVerifiedAdmin_When_ValidCommandProvided()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var admin = DistrictAdmin.Create(adminId, districtId, "admin@demo.org", _dateTimeProvider);
        admin.Verify(_dateTimeProvider); // Transition to Verified state
        var reason = "District deleted";

        var command = new RevokeDistrictAdminCommand(districtId, adminId, reason);

        _adminRepository.GetByIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(admin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        admin.Status.Should().Be(DistrictAdminStatus.Revoked);
        admin.RevokedAtUtc.Should().NotBeNull();

        await _adminRepository.Received(1).UpdateAsync(
            Arg.Is<DistrictAdmin>(a => a.Status == DistrictAdminStatus.Revoked),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithNotFound_When_AdminDoesNotExist()
    {
        // Arrange
        var command = new RevokeDistrictAdminCommand(Guid.NewGuid(), Guid.NewGuid(), "Test reason");

        _adminRepository.GetByIdAsync(command.AdminId, Arg.Any<CancellationToken>())
            .Returns((DistrictAdmin?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("DistrictAdmin.NotFound");

        await _adminRepository.DidNotReceive().UpdateAsync(
            Arg.Any<DistrictAdmin>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithAccessDenied_When_AdminBelongsToDifferentDistrict()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var otherDistrictId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var admin = DistrictAdmin.Create(adminId, otherDistrictId, "admin@other.org", _dateTimeProvider);

        var command = new RevokeDistrictAdminCommand(districtId, adminId, "Test reason");

        _adminRepository.GetByIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(admin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("DistrictAdmin.AccessDenied");
        result.Error!.Message.Should().Contain("Admin does not belong to this district");

        await _adminRepository.DidNotReceive().UpdateAsync(
            Arg.Any<DistrictAdmin>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithRevokeFailed_When_AdminIsAlreadyRevoked()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var admin = DistrictAdmin.Create(adminId, districtId, "admin@demo.org", _dateTimeProvider);
        admin.Revoke("First revocation", _dateTimeProvider);

        var command = new RevokeDistrictAdminCommand(districtId, adminId, "Second revocation");

        _adminRepository.GetByIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(admin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("DistrictAdmin.RevokeFailed");
        result.Error!.Message.Should().Contain("Admin is already revoked");

        await _adminRepository.DidNotReceive().UpdateAsync(
            Arg.Any<DistrictAdmin>(),
            Arg.Any<CancellationToken>());
    }
}
