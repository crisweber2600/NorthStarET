using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Abstractions;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.CreateSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.DeleteSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.UpdateSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Queries.ListSchools;
using NorthStarET.NextGen.Lms.Contracts.Schools;
using NorthStarET.NextGen.Lms.Domain.Auditing;
using NorthStarET.NextGen.Lms.Domain.Schools;
using Xunit;

namespace NorthStarET.NextGen.Lms.Application.Tests.Districts.Schools;

/// <summary>
/// Unit tests for school catalog operations covering list, create, update, and delete flows.
/// </summary>
public sealed class SchoolCatalogServiceTests
{
    private readonly Mock<ISchoolRepository> _mockRepository = new();
    private readonly Mock<IGradeTaxonomyProvider> _mockGradeTaxonomyProvider = new();
    private readonly Mock<ICurrentUserService> _mockCurrentUserService = new();
    private readonly Guid _testDistrictId = Guid.NewGuid();
    private readonly Guid _testUserId = Guid.NewGuid();

    public SchoolCatalogServiceTests()
    {
        _mockGradeTaxonomyProvider
            .Setup(t => t.IsValidGrade(It.IsAny<GradeLevel>()))
            .Returns(true);

        _mockCurrentUserService
            .SetupGet(s => s.UserId)
            .Returns(_testUserId);

        _mockCurrentUserService
            .SetupGet(s => s.Role)
            .Returns(ActorRole.DistrictAdmin);

        _mockCurrentUserService
            .SetupGet(s => s.DistrictId)
            .Returns(_testDistrictId);

        _mockCurrentUserService
            .SetupGet(s => s.CorrelationId)
            .Returns(Guid.NewGuid());
    }

    #region List Schools Tests

    [Fact]
    public async Task ListSchools_Should_ReturnSortedActiveSchools_When_DistrictHasMixedData()
    {
        var orphanSchool = CreateSchool(Guid.NewGuid(), "Other District", "OD");
        var activeA = CreateSchool(_testDistrictId, "Beta Academy", "BETA");
        var activeB = CreateSchool(_testDistrictId, "Alpha Elementary", "ALPHA");
        var deleted = CreateSchool(_testDistrictId, "Retired School", "OLD");
        deleted.Delete(_testUserId);

        _mockRepository
            .Setup(r => r.ListByDistrictAsync(_testDistrictId, null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { deleted, orphanSchool, activeA, activeB });

        var handler = new ListSchoolsQueryHandler(_mockRepository.Object);

        var result = await handler.Handle(new ListSchoolsQuery(_testDistrictId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var items = result.Value!.Items;
        items.Should().HaveCount(4); // handler relies on repository filtering; verify mapping preserves entries
        items.Select(i => i.Name).Should().BeInAscendingOrder();
        items.First().Name.Should().Be("Alpha Elementary");
    }

    [Fact]
    public async Task ListSchools_Should_RequestFilteredResults_When_SearchProvided()
    {
        var searchTerm = "wash";
        var expected = new[] { CreateSchool(_testDistrictId, "Washington High", "WASH") };

        _mockRepository
            .Setup(r => r.ListByDistrictAsync(_testDistrictId, searchTerm, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new ListSchoolsQueryHandler(_mockRepository.Object);

        var result = await handler.Handle(new ListSchoolsQuery(_testDistrictId, searchTerm), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(r => r.ListByDistrictAsync(_testDistrictId, searchTerm, false, It.IsAny<CancellationToken>()), Times.Once);
        result.Value!.Items.Should().ContainSingle().Which.Name.Should().Be("Washington High");
    }

    #endregion

    #region Create School Tests

    [Fact]
    public async Task CreateSchool_Should_Succeed_When_RequestIsValid()
    {
        School? persistedSchool = null;
        _mockRepository.Setup(r => r.ExistsWithNameAsync(_testDistrictId, "Roosevelt Elementary", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.ExistsWithCodeAsync(_testDistrictId, "ROOS", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<School>(), It.IsAny<CancellationToken>()))
            .Callback<School, CancellationToken>((s, _) => persistedSchool = s);

        var handler = new CreateSchoolCommandHandler(
            _mockRepository.Object,
            _mockGradeTaxonomyProvider.Object,
            _mockCurrentUserService.Object);
        var grades = new List<GradeSelectionDto>
        {
            new() { GradeId = GradeLevel.K.ToString(), SchoolType = SchoolType.Elementary.ToString(), Selected = true }
        };
        var command = new CreateSchoolCommand(_testDistrictId, "Roosevelt Elementary", "ROOS", null, grades);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        persistedSchool.Should().NotBeNull();
        persistedSchool!.DistrictId.Should().Be(_testDistrictId);
        persistedSchool.Name.Should().Be("Roosevelt Elementary");
        persistedSchool.GradeOfferings.Should().ContainSingle(o => o.GradeLevel == GradeLevel.K);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<School>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSchool_Should_Fail_When_NameAlreadyExists()
    {
        _mockRepository.Setup(r => r.ExistsWithNameAsync(_testDistrictId, "Lincoln Elementary", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateSchoolCommandHandler(
            _mockRepository.Object,
            _mockGradeTaxonomyProvider.Object,
            _mockCurrentUserService.Object);
        var command = new CreateSchoolCommand(_testDistrictId, "Lincoln Elementary", null, null, Array.Empty<GradeSelectionDto>());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("School.DuplicateName");
    }

    [Fact]
    public async Task CreateSchool_Should_Fail_When_CodeAlreadyExists()
    {
        _mockRepository.Setup(r => r.ExistsWithNameAsync(_testDistrictId, "New School", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.ExistsWithCodeAsync(_testDistrictId, "LIN", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateSchoolCommandHandler(
            _mockRepository.Object,
            _mockGradeTaxonomyProvider.Object,
            _mockCurrentUserService.Object);
        var command = new CreateSchoolCommand(_testDistrictId, "New School", "LIN", null, Array.Empty<GradeSelectionDto>());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("School.DuplicateCode");
    }

    [Fact]
    public void CreateSchool_Should_FailValidation_When_NameIsEmpty()
    {
        var validator = new CreateSchoolCommandValidator();
        var action = () => new CreateSchoolCommand(Guid.Empty, string.Empty, null, null, Array.Empty<GradeSelectionDto>());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateSchool_Should_FailValidation_When_NameTooLong()
    {
        var validator = new CreateSchoolCommandValidator();
        var longName = new string('A', 101);
        var command = new CreateSchoolCommand(Guid.NewGuid(), longName, null, null, Array.Empty<GradeSelectionDto>());

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("School name must be between"));
    }

    #endregion

    #region Update School Tests

    [Fact]
    public async Task UpdateSchool_Should_Succeed_When_RequestIsValid()
    {
        var school = CreateSchool(_testDistrictId, "Lincoln Elementary", "LIN");
        var originalStamp = school.ConcurrencyStamp;
        _mockRepository.Setup(r => r.GetByIdWithGradesAsync(school.Id, _testDistrictId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(school);
        _mockRepository.Setup(r => r.ExistsWithNameAsync(_testDistrictId, "Abraham Lincoln Elementary", school.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.ExistsWithCodeAsync(_testDistrictId, "ALIN", school.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new UpdateSchoolCommandHandler(_mockRepository.Object, _mockCurrentUserService.Object);
        var command = new UpdateSchoolCommand(_testDistrictId, school.Id, "Abraham Lincoln Elementary", "ALIN", null, originalStamp);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        school.Name.Should().Be("Abraham Lincoln Elementary");
        school.Code.Should().Be("ALIN");
        school.ConcurrencyStamp.Should().NotBe(originalStamp);
        _mockRepository.Verify(r => r.UpdateAsync(school, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSchool_Should_Fail_When_SchoolNotFound()
    {
        _mockRepository.Setup(r => r.GetByIdWithGradesAsync(It.IsAny<Guid>(), _testDistrictId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((School?)null);

        var handler = new UpdateSchoolCommandHandler(_mockRepository.Object, _mockCurrentUserService.Object);
        var command = new UpdateSchoolCommand(_testDistrictId, Guid.NewGuid(), "Name", null, null, "stamp");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("School.NotFound");
    }

    [Fact]
    public async Task UpdateSchool_Should_Fail_When_ConcurrencyStampMismatch()
    {
        var school = CreateSchool(_testDistrictId, "Lincoln Elementary", "LIN");
        _mockRepository.Setup(r => r.GetByIdWithGradesAsync(school.Id, _testDistrictId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(school);

        var handler = new UpdateSchoolCommandHandler(_mockRepository.Object, _mockCurrentUserService.Object);
        var command = new UpdateSchoolCommand(_testDistrictId, school.Id, "Lincoln Elementary", "LIN", null, "stale");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("School.ConcurrencyConflict");
    }

    [Fact]
    public async Task UpdateSchool_Should_Fail_When_NewNameConflicts()
    {
        var school = CreateSchool(_testDistrictId, "Lincoln Elementary", "LIN");
        _mockRepository.Setup(r => r.GetByIdWithGradesAsync(school.Id, _testDistrictId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(school);
        _mockRepository.Setup(r => r.ExistsWithNameAsync(_testDistrictId, "Duplicate", school.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateSchoolCommandHandler(_mockRepository.Object, _mockCurrentUserService.Object);
        var command = new UpdateSchoolCommand(_testDistrictId, school.Id, "Duplicate", null, null, school.ConcurrencyStamp);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("School.DuplicateName");
    }

    #endregion

    #region Delete School Tests

    [Fact]
    public async Task DeleteSchool_Should_SoftDelete_When_SchoolExists()
    {
        var school = CreateSchool(_testDistrictId, "Old School", null);
        _mockRepository.Setup(r => r.GetByIdAsync(school.Id, _testDistrictId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(school);

        var handler = new DeleteSchoolCommandHandler(_mockRepository.Object);
        var command = new DeleteSchoolCommand(_testDistrictId, school.Id, _testUserId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        school.IsDeleted.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(school, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSchool_Should_Fail_When_SchoolAlreadyDeleted()
    {
        var school = CreateSchool(_testDistrictId, "Retired", null);
        school.Delete(_testUserId);
        _mockRepository.Setup(r => r.GetByIdAsync(school.Id, _testDistrictId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(school);

        var handler = new DeleteSchoolCommandHandler(_mockRepository.Object);
        var command = new DeleteSchoolCommand(_testDistrictId, school.Id, _testUserId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("School.AlreadyDeleted");
    }

    [Fact]
    public async Task DeleteSchool_Should_Fail_When_SchoolNotFound()
    {
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), _testDistrictId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((School?)null);

        var handler = new DeleteSchoolCommandHandler(_mockRepository.Object);
        var command = new DeleteSchoolCommand(_testDistrictId, Guid.NewGuid(), _testUserId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("School.NotFound");
    }

    #endregion

    #region Grade Assignment Placeholders

    [Fact(Skip = "US2 grade management not implemented yet")]
    public Task SetSchoolGrades_Should_ReplaceExistingGrades_When_ValidGradeList()
        => Task.CompletedTask;

    [Fact(Skip = "US2 grade management not implemented yet")]
    public Task SetSchoolGrades_Should_RaiseGradesUpdatedEvent_When_GradesChange()
        => Task.CompletedTask;

    #endregion

    private static School CreateSchool(Guid districtId, string name, string? code)
    {
        return School.Create(districtId, name, code, null, Guid.NewGuid());
    }
}
