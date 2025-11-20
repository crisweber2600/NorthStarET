using FluentAssertions;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.ResendInvite;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NSubstitute;
using Xunit;

namespace NorthStarET.NextGen.Lms.Application.Tests.DistrictAdmins;

public sealed class ResendInviteCommandHandlerTests
{
    private readonly IDistrictAdminRepository _adminRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ResendInviteCommandHandler _handler;

    public ResendInviteCommandHandlerTests()
    {
        _adminRepository = Substitute.For<IDistrictAdminRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _handler = new ResendInviteCommandHandler(_adminRepository, _dateTimeProvider);
    }

    [Fact]
    public async Task Should_ResendInvitation_When_ValidCommandProvided()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var initialTime = DateTime.UtcNow;
        var resendTime = initialTime.AddMilliseconds(100);
        
        // First call returns initial time, second call returns resend time
        _dateTimeProvider.UtcNow.Returns(initialTime, resendTime);
        
        var admin = DistrictAdmin.Create(adminId, districtId, "admin@demo.org", _dateTimeProvider);
        var originalInvitedAt = admin.InvitedAtUtc;

        var command = new ResendInviteCommand(districtId, adminId);

        _adminRepository.GetByIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(admin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        admin.InvitedAtUtc.Should().BeAfter(originalInvitedAt);
        admin.Status.Should().Be(DistrictAdminStatus.Unverified);

        await _adminRepository.Received(1).UpdateAsync(
            Arg.Is<DistrictAdmin>(a => a.Id == adminId && a.InvitedAtUtc > originalInvitedAt),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithNotFound_When_AdminDoesNotExist()
    {
        // Arrange
        var command = new ResendInviteCommand(Guid.NewGuid(), Guid.NewGuid());

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

        var command = new ResendInviteCommand(districtId, adminId);

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
    public async Task Should_FailWithResendFailed_When_AdminIsVerified()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var admin = DistrictAdmin.Create(adminId, districtId, "admin@demo.org", _dateTimeProvider);
        admin.Verify(_dateTimeProvider); // Transition to Verified state

        var command = new ResendInviteCommand(districtId, adminId);

        _adminRepository.GetByIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(admin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("DistrictAdmin.ResendFailed");
        result.Error!.Message.Should().Contain("Can only resend invitation for unverified admins");

        await _adminRepository.DidNotReceive().UpdateAsync(
            Arg.Any<DistrictAdmin>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailWithResendFailed_When_AdminIsRevoked()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var admin = DistrictAdmin.Create(adminId, districtId, "admin@demo.org", _dateTimeProvider);
        admin.Revoke("Test revocation", _dateTimeProvider); // Transition to Revoked state

        var command = new ResendInviteCommand(districtId, adminId);

        _adminRepository.GetByIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(admin);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("DistrictAdmin.ResendFailed");

        await _adminRepository.DidNotReceive().UpdateAsync(
            Arg.Any<DistrictAdmin>(),
            Arg.Any<CancellationToken>());
    }
}
