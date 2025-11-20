using FluentAssertions;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.ListDistricts;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NSubstitute;

namespace NorthStarET.NextGen.Lms.Application.Tests.Districts;

public sealed class ListDistrictsQueryTests
{
    private readonly IDistrictRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ListDistrictsQueryHandler _handler;

    public ListDistrictsQueryTests()
    {
        _repository = Substitute.For<IDistrictRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _handler = new ListDistrictsQueryHandler(_repository);
    }

    [Fact]
    public async Task Should_ReturnPagedDistricts_When_ValidQueryProvided()
    {
        // Arrange
        var districts = new List<District>
        {
            District.Create(Guid.NewGuid(), "District A", "da", _dateTimeProvider),
            District.Create(Guid.NewGuid(), "District B", "db", _dateTimeProvider),
            District.Create(Guid.NewGuid(), "District C", "dc", _dateTimeProvider)
        };

        _repository.GetActiveAdminCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var id = call.Arg<Guid>();
                if (id == districts[0].Id)
                {
                    return Task.FromResult(3);
                }

                if (id == districts[1].Id)
                {
                    return Task.FromResult(2);
                }

                return Task.FromResult(1);
            });

        var query = new ListDistrictsQuery(1, 20);

        _repository.ListAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(districts);
        _repository.CountAsync(Arg.Any<CancellationToken>())
            .Returns(3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalCount.Should().Be(3);
        result.Value.TotalPages.Should().Be(1);
        result.Value.Items.Select(i => i.AdminCount).Should().BeEquivalentTo(new[] { 3, 2, 1 });
    }

    [Fact]
    public async Task Should_ExcludeSoftDeletedDistricts_When_Listing()
    {
        // Arrange
        var activeDistrict = District.Create(Guid.NewGuid(), "Active District", "active", _dateTimeProvider);
        var deletedDistrict = District.Create(Guid.NewGuid(), "Deleted District", "deleted", _dateTimeProvider);
        deletedDistrict.Delete(_dateTimeProvider);

        var districts = new List<District> { activeDistrict };

        var query = new ListDistrictsQuery(1, 20);

        _repository.ListAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(districts);
        _repository.CountAsync(Arg.Any<CancellationToken>())
            .Returns(1);
        _repository.GetActiveAdminCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items.Should().NotContain(d => d.Suffix == "deleted");
    }

    [Fact]
    public async Task Should_CalculateCorrectTotalPages_When_PaginatingResults()
    {
        // Arrange
        var districts = Enumerable.Range(1, 20)
            .Select(i => District.Create(Guid.NewGuid(), $"District {i}", $"d{i}", _dateTimeProvider))
            .ToList();

        var query = new ListDistrictsQuery(2, 20);

        _repository.ListAsync(2, 20, Arg.Any<CancellationToken>())
            .Returns(districts.Skip(20).Take(20).ToList());
        _repository.CountAsync(Arg.Any<CancellationToken>())
            .Returns(100);
        _repository.GetActiveAdminCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalCount.Should().Be(100);
        result.Value.TotalPages.Should().Be(5);
    }

    [Fact]
    public async Task Should_OrderByName_When_ListingDistricts()
    {
        // Arrange
        var districts = new List<District>
        {
            District.Create(Guid.NewGuid(), "Zebra District", "zebra", _dateTimeProvider),
            District.Create(Guid.NewGuid(), "Alpha District", "alpha", _dateTimeProvider),
            District.Create(Guid.NewGuid(), "Beta District", "beta", _dateTimeProvider)
        };

        var query = new ListDistrictsQuery(1, 20);

        _repository.ListAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(districts.OrderBy(d => d.Name).ToList());
        _repository.GetActiveAdminCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.First().Name.Should().Be("Alpha District");
        result.Value.Items.Last().Name.Should().Be("Zebra District");
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoDistrictsExist()
    {
        // Arrange
        var query = new ListDistrictsQuery(1, 20);

        _repository.ListAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns(new List<District>());
        _repository.CountAsync(Arg.Any<CancellationToken>())
            .Returns(0);
        _repository.GetActiveAdminCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }
}
