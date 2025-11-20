using FluentAssertions;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Tests.Helpers;
using Xunit;

namespace NorthStarET.NextGen.Lms.Domain.Tests.DistrictAdmins;

public sealed class DistrictAdminTests
{
    [Fact]
    public void Should_CreateDistrictAdmin_When_ValidDataProvided()
    {
        // Arrange
        var id = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var email = "admin@demo.org";
        var dateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));

        // Act
        var admin = DistrictAdmin.Create(id, districtId, email, dateTimeProvider);

        // Assert
        admin.Should().NotBeNull();
        admin.Id.Should().Be(id);
        admin.DistrictId.Should().Be(districtId);
        admin.Email.Should().Be(email.ToLowerInvariant());
        admin.Status.Should().Be(DistrictAdminStatus.Unverified);
        admin.InvitedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        admin.VerifiedAtUtc.Should().BeNull();
        admin.RevokedAtUtc.Should().BeNull();
        admin.IsActive.Should().BeFalse();
        admin.DomainEvents.Should().ContainSingle(e => e is DistrictAdminInvitedEvent);
    }

    [Fact]
    public void Should_NormalizeEmailToLowercase_When_CreatingAdmin()
    {
        // Arrange
        var email = "Admin@DEMO.ORG";
        var dateTimeProvider = new FakeDateTimeProvider();

        // Act
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), email, dateTimeProvider);

        // Assert
        admin.Email.Should().Be("admin@demo.org");
    }

    [Fact]
    public void Should_CalculateInvitationExpiry_When_CreatingAdmin()
    {
        // Arrange & Act
        var dateTimeProvider = new FakeDateTimeProvider();
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);

        // Assert
        admin.InvitationExpiresAtUtc.Should().Be(admin.InvitedAtUtc.AddDays(7));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_EmailIsNullOrEmpty(string? invalidEmail)
    {
        // Arrange & Act
        var dateTimeProvider = new FakeDateTimeProvider();
        var act = () => DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), invalidEmail!, dateTimeProvider);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@demo.org")]
    [InlineData("admin@")]
    [InlineData("admin")]
    public void Should_ThrowArgumentException_When_EmailFormatIsInvalid(string invalidEmail)
    {
        // Arrange & Act
        var dateTimeProvider = new FakeDateTimeProvider();
        var act = () => DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), invalidEmail, dateTimeProvider);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid email format.*");
    }

    [Fact]
    public void Should_VerifyAdmin_When_InUnverifiedState()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);
        admin.ClearDomainEvents();

        dateTimeProvider.Advance(TimeSpan.FromHours(1));

        // Act
        admin.Verify(dateTimeProvider);

        // Assert
        admin.Status.Should().Be(DistrictAdminStatus.Verified);
        admin.VerifiedAtUtc.Should().NotBeNull();
        admin.VerifiedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        admin.IsActive.Should().BeTrue();
        admin.DomainEvents.Should().ContainSingle(e => e is DistrictAdminVerifiedEvent);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_VerifyingAlreadyVerifiedAdmin()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);
        admin.Verify(dateTimeProvider);

        // Act
        var act = () => admin.Verify(dateTimeProvider);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Admin is already verified.");
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_VerifyingRevokedAdmin()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);
        admin.Revoke("Test revocation", dateTimeProvider);

        // Act
        var act = () => admin.Verify(dateTimeProvider);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot verify a revoked admin.");
    }

    [Fact]
    public void Should_RevokeAdmin_When_InUnverifiedState()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);
        admin.ClearDomainEvents();
        var reason = "User request";

        dateTimeProvider.Advance(TimeSpan.FromHours(2));

        // Act
        admin.Revoke(reason, dateTimeProvider);

        // Assert
        admin.Status.Should().Be(DistrictAdminStatus.Revoked);
        admin.RevokedAtUtc.Should().NotBeNull();
        admin.RevokedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        admin.IsActive.Should().BeFalse();
        admin.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<DistrictAdminRevokedEvent>();
        var revokedEvent = (DistrictAdminRevokedEvent)admin.DomainEvents.Single();
        revokedEvent.Reason.Should().Be(reason);
    }

    [Fact]
    public void Should_RevokeAdmin_When_InVerifiedState()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);
        admin.Verify(dateTimeProvider);
        admin.ClearDomainEvents();
        var reason = "District deleted";

        // Act
        admin.Revoke(reason, dateTimeProvider);

        // Assert
        admin.Status.Should().Be(DistrictAdminStatus.Revoked);
        admin.RevokedAtUtc.Should().NotBeNull();
        admin.IsActive.Should().BeFalse();
        admin.DomainEvents.Should().ContainSingle(e => e is DistrictAdminRevokedEvent);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_RevokingAlreadyRevokedAdmin()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);
        admin.Revoke("First revocation", dateTimeProvider);

        // Act
        var act = () => admin.Revoke("Second revocation", dateTimeProvider);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Admin is already revoked.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_RevokeReasonIsNullOrEmpty(string? invalidReason)
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);

        // Act
        var act = () => admin.Revoke(invalidReason!, dateTimeProvider);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*reason*");
    }

    [Fact]
    public void Should_ResendInvitation_When_InUnverifiedState()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);
        var originalInvitedAt = admin.InvitedAtUtc;
        admin.ClearDomainEvents();
        dateTimeProvider.Advance(TimeSpan.FromHours(1));

        // Act
        admin.ResendInvitation(dateTimeProvider);

        // Assert
        admin.Status.Should().Be(DistrictAdminStatus.Unverified);
        admin.InvitedAtUtc.Should().BeAfter(originalInvitedAt);
        admin.InvitedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        admin.InvitationExpiresAtUtc.Should().Be(admin.InvitedAtUtc.AddDays(7));
        admin.DomainEvents.Should().ContainSingle(e => e is DistrictAdminInvitedEvent);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_ResendingForVerifiedAdmin()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);
        admin.Verify(dateTimeProvider);

        // Act
        var act = () => admin.ResendInvitation(dateTimeProvider);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Can only resend invitation for unverified admins.");
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_ResendingForRevokedAdmin()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);
        admin.Revoke("Test revocation", dateTimeProvider);

        // Act
        var act = () => admin.ResendInvitation(dateTimeProvider);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Can only resend invitation for unverified admins.");
    }

    [Fact]
    public void Should_IndicateExpired_When_InvitationIsOlderThan7Days()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);

        // Advance time by 8 days
        dateTimeProvider.Advance(TimeSpan.FromDays(8));

        // Act & Assert
        admin.IsInvitationExpired(dateTimeProvider).Should().BeTrue();
    }

    [Fact]
    public void Should_IndicateNotExpired_When_InvitationIsWithin7Days()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var admin = DistrictAdmin.Create(Guid.NewGuid(), Guid.NewGuid(), "admin@demo.org", dateTimeProvider);

        // Advance time by 6 days
        dateTimeProvider.Advance(TimeSpan.FromDays(6));

        // Act & Assert
        admin.IsInvitationExpired(dateTimeProvider).Should().BeFalse();
    }
}
