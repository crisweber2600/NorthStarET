using FluentAssertions;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Tests.Helpers;
using Xunit;

namespace NorthStarET.NextGen.Lms.Domain.Tests.DistrictAdmins;

/// <summary>
/// Tests demonstrating deterministic time-dependent behavior after introducing IDateTimeProvider.
/// Addresses issue from PR #40 about hardcoded DateTime.UtcNow calls.
/// </summary>
public sealed class DistrictAdminTimeProviderTests
{
    [Fact]
    public void Should_UseProvidedTime_When_CreatingAdmin()
    {
        // Arrange
        var specificTime = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc);
        var dateTimeProvider = new FakeDateTimeProvider(specificTime);

        // Act
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "test@demo.org", dateTimeProvider);

        // Assert - This is now deterministic, not relying on DateTime.UtcNow
        admin.InvitedAtUtc.Should().Be(specificTime);
        admin.InvitationExpiresAtUtc.Should().Be(specificTime.AddDays(7));
    }

    [Fact]
    public void Should_UseProvidedTime_When_ResendingInvitation()
    {
        // Arrange
        var initialTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var dateTimeProvider = new FakeDateTimeProvider(initialTime);
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "test@demo.org", dateTimeProvider);

        // Simulate time passing
        var resendTime = new DateTime(2024, 1, 3, 15, 30, 0, DateTimeKind.Utc);
        dateTimeProvider.SetUtcNow(resendTime);

        // Act
        admin.ResendInvitation(dateTimeProvider);

        // Assert - ResendInvitation (line 175 in original issue) now uses abstraction
        admin.InvitedAtUtc.Should().Be(resendTime);
        admin.InvitationExpiresAtUtc.Should().Be(resendTime.AddDays(7));
    }

    [Fact]
    public void Should_CheckExpirationDeterministically_When_TimeIsControlled()
    {
        // Arrange
        var inviteTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var dateTimeProvider = new FakeDateTimeProvider(inviteTime);
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "test@demo.org", dateTimeProvider);

        // Act & Assert - Day 1: Not expired
        admin.IsInvitationExpired(dateTimeProvider).Should().BeFalse();

        // Advance time to day 6: Still not expired
        dateTimeProvider.SetUtcNow(inviteTime.AddDays(6));
        admin.IsInvitationExpired(dateTimeProvider).Should().BeFalse();

        // Advance time to day 7 + 1 second: Now expired
        dateTimeProvider.SetUtcNow(inviteTime.AddDays(7).AddSeconds(1));
        admin.IsInvitationExpired(dateTimeProvider).Should().BeTrue();
    }

    [Fact]
    public void Should_UseProvidedTime_When_VerifyingAdmin()
    {
        // Arrange
        var inviteTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var verifyTime = new DateTime(2024, 1, 2, 14, 30, 0, DateTimeKind.Utc);
        var dateTimeProvider = new FakeDateTimeProvider(inviteTime);
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "test@demo.org", dateTimeProvider);

        dateTimeProvider.SetUtcNow(verifyTime);

        // Act
        admin.Verify(dateTimeProvider);

        // Assert
        admin.VerifiedAtUtc.Should().Be(verifyTime);
    }

    [Fact]
    public void Should_UseProvidedTime_When_RevokingAdmin()
    {
        // Arrange
        var inviteTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var revokeTime = new DateTime(2024, 1, 5, 9, 15, 0, DateTimeKind.Utc);
        var dateTimeProvider = new FakeDateTimeProvider(inviteTime);
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "test@demo.org", dateTimeProvider);

        dateTimeProvider.SetUtcNow(revokeTime);

        // Act
        admin.Revoke("Test reason", dateTimeProvider);

        // Assert
        admin.RevokedAtUtc.Should().Be(revokeTime);
    }
}
