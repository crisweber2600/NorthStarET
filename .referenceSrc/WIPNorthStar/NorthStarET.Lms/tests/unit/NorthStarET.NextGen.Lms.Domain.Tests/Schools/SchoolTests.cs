using FluentAssertions;
using NorthStarET.NextGen.Lms.Domain.Schools;
using Xunit;

namespace NorthStarET.NextGen.Lms.Domain.Tests.Schools;

public sealed class SchoolTests
{
    private static readonly Guid TestDistrictId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    [Fact]
    public void Should_CreateSchool_When_ValidDataProvided()
    {
        // Arrange
        var name = "Lincoln Elementary";
        var code = "LIN-001";
        var notes = "Main campus";

        // Act
        var school = School.Create(TestDistrictId, name, code, notes, TestUserId);

        // Assert
        school.Should().NotBeNull();
        school.Id.Should().NotBe(Guid.Empty);
        school.DistrictId.Should().Be(TestDistrictId);
        school.Name.Should().Be(name);
        school.Code.Should().Be(code);
        school.Notes.Should().Be(notes);
        school.Status.Should().Be(SchoolStatus.Active);
        school.CreatedBy.Should().Be(TestUserId);
        school.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        school.UpdatedBy.Should().BeNull();
        school.UpdatedAtUtc.Should().BeNull();
        school.DeletedAt.Should().BeNull();
        school.IsDeleted.Should().BeFalse();
        school.ConcurrencyStamp.Should().NotBeNullOrEmpty();
        school.DomainEvents.Should().ContainSingle(e => e is SchoolCreatedEvent);
    }

    [Fact]
    public void Should_CreateSchool_When_OptionalFieldsAreNull()
    {
        // Arrange
        var name = "Washington High";

        // Act
        var school = School.Create(TestDistrictId, name, null, null, TestUserId);

        // Assert
        school.Should().NotBeNull();
        school.Name.Should().Be(name);
        school.Code.Should().BeNull();
        school.Notes.Should().BeNull();
    }

    [Fact]
    public void Should_TrimName_When_CreatingSchool()
    {
        // Arrange
        var nameWithWhitespace = "  Jefferson Middle  ";

        // Act
        var school = School.Create(TestDistrictId, nameWithWhitespace, null, null, TestUserId);

        // Assert
        school.Name.Should().Be("Jefferson Middle");
    }

    [Fact]
    public void Should_TrimCodeAndNotes_When_CreatingSchool()
    {
        // Arrange
        var code = "  CODE-001  ";
        var notes = "  Some notes  ";

        // Act
        var school = School.Create(TestDistrictId, "Test School", code, notes, TestUserId);

        // Assert
        school.Code.Should().Be("CODE-001");
        school.Notes.Should().Be("Some notes");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_DistrictIdIsEmpty()
    {
        // Act
        var act = () => School.Create(Guid.Empty, "Test School", null, null, TestUserId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("District ID cannot be empty*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_NameIsNullOrWhitespace()
    {
        // Act
        var actNull = () => School.Create(TestDistrictId, null!, null, null, TestUserId);
        var actEmpty = () => School.Create(TestDistrictId, "", null, null, TestUserId);
        var actWhitespace = () => School.Create(TestDistrictId, "   ", null, null, TestUserId);

        // Assert
        actNull.Should().Throw<ArgumentException>().WithMessage("School name is required*");
        actEmpty.Should().Throw<ArgumentException>().WithMessage("School name is required*");
        actWhitespace.Should().Throw<ArgumentException>().WithMessage("School name is required*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_NameExceeds200Characters()
    {
        // Arrange
        var longName = new string('A', 201);

        // Act
        var act = () => School.Create(TestDistrictId, longName, null, null, TestUserId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("School name cannot exceed 200 characters*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_CodeExceeds50Characters()
    {
        // Arrange
        var longCode = new string('B', 51);

        // Act
        var act = () => School.Create(TestDistrictId, "Test School", longCode, null, TestUserId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("School code cannot exceed 50 characters*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_NotesExceed1000Characters()
    {
        // Arrange
        var longNotes = new string('C', 1001);

        // Act
        var act = () => School.Create(TestDistrictId, "Test School", null, longNotes, TestUserId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("School notes cannot exceed 1000 characters*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_CreatedByIsEmpty()
    {
        // Act
        var act = () => School.Create(TestDistrictId, "Test School", null, null, Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("CreatedBy cannot be empty*");
    }

    [Fact]
    public void Should_UpdateSchool_When_ValidDataProvided()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Old Name", "OLD-001", "Old notes", TestUserId);
        school.ClearDomainEvents();
        var originalStamp = school.ConcurrencyStamp;
        var updatedBy = Guid.NewGuid();

        // Act
        school.Update("New Name", "NEW-001", "New notes", SchoolStatus.Inactive, updatedBy);

        // Assert
        school.Name.Should().Be("New Name");
        school.Code.Should().Be("NEW-001");
        school.Notes.Should().Be("New notes");
        school.Status.Should().Be(SchoolStatus.Inactive);
        school.UpdatedBy.Should().Be(updatedBy);
        school.UpdatedAtUtc.Should().NotBeNull();
        school.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        school.ConcurrencyStamp.Should().NotBe(originalStamp);
        school.DomainEvents.Should().ContainSingle(e => e is SchoolUpdatedEvent);
    }

    [Fact]
    public void Should_TrimFields_When_UpdatingSchool()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Old Name", null, null, TestUserId);

        // Act
        school.Update("  New Name  ", "  CODE  ", "  Notes  ", SchoolStatus.Active, TestUserId);

        // Assert
        school.Name.Should().Be("New Name");
        school.Code.Should().Be("CODE");
        school.Notes.Should().Be("Notes");
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_UpdatingDeletedSchool()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);
        school.Delete(TestUserId);

        // Act
        var act = () => school.Update("New Name", null, null, SchoolStatus.Active, TestUserId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot update a deleted school");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_UpdateWithInvalidName()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);

        // Act
        var actEmpty = () => school.Update("", null, null, SchoolStatus.Active, TestUserId);
        var actTooLong = () => school.Update(new string('A', 201), null, null, SchoolStatus.Active, TestUserId);

        // Assert
        actEmpty.Should().Throw<ArgumentException>().WithMessage("School name is required*");
        actTooLong.Should().Throw<ArgumentException>().WithMessage("School name cannot exceed 200 characters*");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_UpdatedByIsEmpty()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);

        // Act
        var act = () => school.Update("New Name", null, null, SchoolStatus.Active, Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("UpdatedBy cannot be empty*");
    }

    [Fact]
    public void Should_SoftDeleteSchool_When_DeleteCalled()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);
        school.ClearDomainEvents();
        var originalStamp = school.ConcurrencyStamp;
        var deletedBy = Guid.NewGuid();

        // Act
        school.Delete(deletedBy);

        // Assert
        school.DeletedAt.Should().NotBeNull();
        school.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        school.IsDeleted.Should().BeTrue();
        school.UpdatedBy.Should().Be(deletedBy);
        school.UpdatedAtUtc.Should().NotBeNull();
        school.ConcurrencyStamp.Should().NotBe(originalStamp);
        school.DomainEvents.Should().ContainSingle(e => e is SchoolDeletedEvent);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_DeletingAlreadyDeletedSchool()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);
        school.Delete(TestUserId);

        // Act
        var act = () => school.Delete(TestUserId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("School is already deleted");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_DeletedByIsEmpty()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);

        // Act
        var act = () => school.Delete(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("DeletedBy cannot be empty*");
    }

    [Fact]
    public void Should_RestoreSchool_When_RestoreCalled()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);
        school.Delete(TestUserId);
        school.ClearDomainEvents();
        var restoredBy = Guid.NewGuid();

        // Act
        school.Restore(restoredBy);

        // Assert
        school.DeletedAt.Should().BeNull();
        school.IsDeleted.Should().BeFalse();
        school.UpdatedBy.Should().Be(restoredBy);
        school.UpdatedAtUtc.Should().NotBeNull();
        school.DomainEvents.Should().ContainSingle(e => e is SchoolRestoredEvent);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_RestoringNonDeletedSchool()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);

        // Act
        var act = () => school.Restore(TestUserId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("School is not deleted");
    }

    [Fact]
    public void Should_ThrowArgumentException_When_RestoredByIsEmpty()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);
        school.Delete(TestUserId);

        // Act
        var act = () => school.Restore(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("RestoredBy cannot be empty*");
    }

    [Fact]
    public void Should_RaiseSchoolCreatedEvent_When_SchoolIsCreated()
    {
        // Act
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);

        // Assert
        var domainEvent = school.DomainEvents.Should().ContainSingle(e => e is SchoolCreatedEvent)
            .Which as SchoolCreatedEvent;

        domainEvent.Should().NotBeNull();
        domainEvent!.SchoolId.Should().Be(school.Id);
        domainEvent.DistrictId.Should().Be(TestDistrictId);
        domainEvent.SchoolName.Should().Be("Test School");
        domainEvent.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Should_RaiseSchoolUpdatedEvent_When_SchoolIsUpdated()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Old Name", null, null, TestUserId);
        school.ClearDomainEvents();
        var updatedBy = Guid.NewGuid();

        // Act
        school.Update("New Name", null, null, SchoolStatus.Active, updatedBy);

        // Assert
        var domainEvent = school.DomainEvents.Should().ContainSingle(e => e is SchoolUpdatedEvent)
            .Which as SchoolUpdatedEvent;

        domainEvent.Should().NotBeNull();
        domainEvent!.SchoolId.Should().Be(school.Id);
        domainEvent.DistrictId.Should().Be(TestDistrictId);
        domainEvent.SchoolName.Should().Be("New Name");
        domainEvent.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void Should_RaiseSchoolDeletedEvent_When_SchoolIsDeleted()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);
        school.ClearDomainEvents();
        var deletedBy = Guid.NewGuid();

        // Act
        school.Delete(deletedBy);

        // Assert
        var domainEvent = school.DomainEvents.Should().ContainSingle(e => e is SchoolDeletedEvent)
            .Which as SchoolDeletedEvent;

        domainEvent.Should().NotBeNull();
        domainEvent!.SchoolId.Should().Be(school.Id);
        domainEvent.DistrictId.Should().Be(TestDistrictId);
        domainEvent.DeletedBy.Should().Be(deletedBy);
    }

    [Fact]
    public void Should_RaiseSchoolRestoredEvent_When_SchoolIsRestored()
    {
        // Arrange
        var school = School.Create(TestDistrictId, "Test School", null, null, TestUserId);
        school.Delete(TestUserId);
        school.ClearDomainEvents();
        var restoredBy = Guid.NewGuid();

        // Act
        school.Restore(restoredBy);

        // Assert
        var domainEvent = school.DomainEvents.Should().ContainSingle(e => e is SchoolRestoredEvent)
            .Which as SchoolRestoredEvent;

        domainEvent.Should().NotBeNull();
        domainEvent!.SchoolId.Should().Be(school.Id);
        domainEvent.DistrictId.Should().Be(TestDistrictId);
        domainEvent.RestoredBy.Should().Be(restoredBy);
    }
}
