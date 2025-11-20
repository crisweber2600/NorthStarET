using FluentAssertions;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.Tests.Helpers;
using Xunit;

namespace NorthStarET.NextGen.Lms.Domain.Tests.Districts;

public sealed class DistrictTests
{
    [Fact]
    public void Should_CreateDistrict_When_ValidDataProvided()
    {
        // Arrange
        var name = "Demo District";
        var suffix = "demo";
        var dateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));

        // Act
        var district = District.Create(Guid.NewGuid(), name, suffix, dateTimeProvider);

        // Assert
        district.Should().NotBeNull();
        district.Name.Should().Be(name);
        district.Suffix.Should().Be(suffix);
        district.Id.Should().NotBe(Guid.Empty);
        district.CreatedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        district.DeletedAt.Should().BeNull();
        district.DomainEvents.Should().ContainSingle(e => e is DistrictCreatedEvent);
    }

    [Fact]
    public void Should_TrimName_When_CreatingDistrict()
    {
        // Arrange
        var nameWithWhitespace = "  Test District  ";
        var suffix = "test";
        var dateTimeProvider = new FakeDateTimeProvider();

        // Act
        var district = District.Create(Guid.NewGuid(), nameWithWhitespace, suffix, dateTimeProvider);

        // Assert
        district.Name.Should().Be("Test District");
    }

    [Fact]
    public void Should_UpdateDistrict_When_ValidDataProvided()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));
        var district = District.Create(Guid.NewGuid(), "Old Name", "old", dateTimeProvider);
        district.ClearDomainEvents();

        dateTimeProvider.Advance(TimeSpan.FromHours(1));

        // Act
        district.Update("New Name", "new", dateTimeProvider);

        // Assert
        district.Name.Should().Be("New Name");
        district.Suffix.Should().Be("new");
        district.UpdatedAtUtc.Should().NotBeNull();
        district.UpdatedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        district.DomainEvents.Should().ContainSingle(e => e is DistrictUpdatedEvent);
    }

    [Fact]
    public void Should_TrimName_When_UpdatingDistrict()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var district = District.Create(Guid.NewGuid(), "Old Name", "old", dateTimeProvider);
        var nameWithWhitespace = "  New Name  ";

        // Act
        district.Update(nameWithWhitespace, "new", dateTimeProvider);

        // Assert
        district.Name.Should().Be("New Name");
    }

    [Fact]
    public void Should_SoftDeleteDistrict_When_DeleteCalled()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));
        var district = District.Create(Guid.NewGuid(), "Test District", "test", dateTimeProvider);
        district.ClearDomainEvents();

        dateTimeProvider.Advance(TimeSpan.FromHours(2));

        // Act
        district.Delete(dateTimeProvider);

        // Assert
        district.DeletedAt.Should().NotBeNull();
        district.DeletedAt.Should().Be(dateTimeProvider.UtcNow);
        district.DomainEvents.Should().ContainSingle(e => e is DistrictDeletedEvent);
    }

    [Fact]
    public void Should_NotDeleteTwice_When_AlreadyDeleted()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var district = District.Create(Guid.NewGuid(), "Test District", "test", dateTimeProvider);
        district.Delete(dateTimeProvider);
        var firstDeletedAt = district.DeletedAt;
        district.ClearDomainEvents();

        // Act
        var act = () => district.Delete(dateTimeProvider);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("District is already deleted.");
        district.DeletedAt.Should().Be(firstDeletedAt);
        district.DomainEvents.Should().BeEmpty();
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("A")]
    public void Should_ThrowArgumentException_When_NameTooShort(string name)
    {
        // Act & Assert
        var dateTimeProvider = new FakeDateTimeProvider();
        var act = () => District.Create(Guid.NewGuid(), name, "test", dateTimeProvider);
        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.Message.Contains("District name must be between 3 and 100 characters"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_ThrowArgumentException_When_NameNullOrWhitespace(string? name)
    {
        // Act
        var dateTimeProvider = new FakeDateTimeProvider();
        var act = () => District.Create(Guid.NewGuid(), name!, "test", dateTimeProvider);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "name");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_NameTooLong()
    {
        // Arrange
        var name = new string('X', 101);
        var dateTimeProvider = new FakeDateTimeProvider();

        // Act & Assert
        var act = () => District.Create(Guid.NewGuid(), name, "test", dateTimeProvider);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name must be between 3 and 100 characters*");
    }

    [Theory]
    [InlineData("test_district")]
    [InlineData("test district")]
    [InlineData("test!")]
    public void Should_ThrowArgumentException_When_SuffixInvalidFormat(string suffix)
    {
        // Act & Assert
        var dateTimeProvider = new FakeDateTimeProvider();
        var act = () => District.Create(Guid.NewGuid(), "Test District", suffix, dateTimeProvider);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Suffix must contain only lowercase letters, numbers, dots, and hyphens*");
    }

    [Fact]
    public void Should_NormalizeSuffix_When_UppercaseProvided()
    {
        // Arrange
        var suffix = "TEST";
        var dateTimeProvider = new FakeDateTimeProvider();

        // Act
        var district = District.Create(Guid.NewGuid(), "Test District", suffix, dateTimeProvider);

        // Assert
        district.Suffix.Should().Be("test");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_ThrowArgumentException_When_SuffixNullOrEmpty(string? suffix)
    {
        // Act & Assert
        var dateTimeProvider = new FakeDateTimeProvider();
        var act = () => District.Create(Guid.NewGuid(), "Test District", suffix!, dateTimeProvider);
        act.Should().Throw<ArgumentException>();
    }
}
