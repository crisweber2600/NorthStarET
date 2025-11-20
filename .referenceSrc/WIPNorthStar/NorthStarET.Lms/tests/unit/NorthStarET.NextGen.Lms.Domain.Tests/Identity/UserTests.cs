using System;
using FluentAssertions;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;
using Xunit;

namespace NorthStarET.NextGen.Lms.Domain.Tests.Identity;

public class UserTests
{
    [Fact]
    public void CreateUser_WithEmptyEmail_ShouldThrow()
    {
        var act = () => new User(
            Guid.NewGuid(),
            new EntraSubjectId("subject-123"),
            string.Empty,
            "Jane",
            "Doe",
            DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>().WithMessage("*Email*");
    }

    [Fact]
    public void RegisterLogin_ShouldUpdateLastLogin()
    {
        var user = new User(
            Guid.NewGuid(),
            new EntraSubjectId("subject-123"),
            "jane.doe@example.com",
            "Jane",
            "Doe",
            DateTimeOffset.UtcNow.AddDays(-1));

        var loggedInAt = DateTimeOffset.UtcNow;

        user.RegisterLogin(loggedInAt);

        user.LastLoginAt.Should().Be(loggedInAt);
    }

    [Fact]
    public void AddMembership_WithDuplicateTenant_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new User(
            userId,
            new EntraSubjectId("subject-123"),
            "jane.doe@example.com",
            "Jane",
            "Doe",
            DateTimeOffset.UtcNow);

        var membership = new Membership(
            Guid.NewGuid(),
            userId,
            tenantId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null,
            null);

        user.AddMembership(membership);

        var duplicateMembership = new Membership(
            Guid.NewGuid(),
            userId,
            tenantId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null,
            null);

        var act = () => user.AddMembership(duplicateMembership);

        act.Should().Throw<InvalidOperationException>();
    }
}
