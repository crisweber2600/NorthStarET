using FluentAssertions;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.GetDistrict;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NSubstitute;

namespace NorthStarET.NextGen.Lms.Application.Tests.Districts;

public sealed class GetDistrictQueryTests
{
    private readonly IDistrictRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly GetDistrictQueryHandler _handler;

    public GetDistrictQueryTests()
    {
        _repository = Substitute.For<IDistrictRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _handler = new GetDistrictQueryHandler(_repository);
    }

    [Fact]
    public async Task Should_ReturnDistrict_When_DistrictExists()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Test District", "test", _dateTimeProvider);

        var query = new GetDistrictQuery(districtId);

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(districtId);
        result.Value.Name.Should().Be("Test District");
        result.Value.Suffix.Should().Be("test");
        result.Value.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_FailWithNotFound_When_DistrictDoesNotExist()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var query = new GetDistrictQuery(districtId);

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns((District?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("District.NotFound");
        result.Error!.Message.Should().Contain("District with ID");
    }

    [Fact]
    public async Task Should_IncludeAdminCounts_When_RetrievingDistrict()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Test District", "test", _dateTimeProvider);

        var query = new GetDistrictQuery(districtId);

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);
        _repository.GetActiveAdminCountAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(5);
        _repository.GetPendingAdminCountAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(2);
        _repository.GetRevokedAdminCountAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ActiveAdminCount.Should().Be(5);
        result.Value.PendingAdminCount.Should().Be(2);
        result.Value.RevokedAdminCount.Should().Be(3);
    }

    [Fact]
    public async Task Should_ReturnDeletedStatus_When_DistrictSoftDeleted()
    {
        // Arrange
        var districtId = Guid.NewGuid();
        var district = District.Create(districtId, "Test District", "test", _dateTimeProvider);
        district.Delete(_dateTimeProvider);

        var query = new GetDistrictQuery(districtId);

        _repository.GetByIdAsync(districtId, Arg.Any<CancellationToken>())
            .Returns(district);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsDeleted.Should().BeTrue();
        result.Value.DeletedAt.Should().NotBeNull();
    }
}
