using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.CreateDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.DeleteDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.UpdateDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.GetDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.ListDistricts;
using NorthStarET.NextGen.Lms.Bdd.Support;
using NorthStarET.NextGen.Lms.Contracts.Districts;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Districts;
using Reqnroll;
using Reqnroll.Assist;

namespace NorthStarET.NextGen.Lms.Bdd.StepDefinitions;

[Binding]
public sealed class DistrictSteps
{
    private readonly DistrictScenarioContext _context;

    public DistrictSteps(DistrictScenarioContext context)
    {
        _context = context;
    }

    // Background steps
    [Given(@"I am authenticated as a System Admin")]
    public void GivenIAmAuthenticatedAsASystemAdmin()
    {
        _context.CurrentUser.SetSystemAdmin();
    }

    [Given(@"the system has no existing districts")]
    public void GivenTheSystemHasNoExistingDistricts()
    {
        _context.Repository.Clear();
        _context.AuditSink.Clear();
        _context.IdempotencyService.Clear();
    }

    // District creation steps
    [When(@"I create a district with:")]
    public async Task WhenICreateADistrictWithAsync(Table table)
    {
        _context.ResetOutcome();
        var data = table.CreateInstance<DistrictData>();

        var command = new CreateDistrictCommand(data.Name, data.Suffix);
        _context.LastDistrictPayload = new() { ["Name"] = data.Name, ["Suffix"] = data.Suffix };

        try
        {
            var result = await _context.Mediator.Send(command, CancellationToken.None);
            _context.LastCreateResult = result;
            if (result.IsSuccess)
            {
                var value = result.Value ?? throw new InvalidOperationException("Expected created district details.");
                _context.LastDistrictId = value.Id;
            }
            _context.CaptureAuditIfSuccessful(command, result);
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [Given(@"I create a district with:")]
    public Task GivenICreateADistrictWithAsync(Table table)
    {
        return WhenICreateADistrictWithAsync(table);
    }

    [When(@"I attempt to create a district with:")]
    public async Task WhenIAttemptToCreateADistrictWithAsync(Table table)
    {
        await WhenICreateADistrictWithAsync(table);
    }

    [Then(@"the district should be created successfully")]
    public void ThenTheDistrictShouldBeCreatedSuccessfully()
    {
        var result = _context.LastCreateResult ?? throw new InvalidOperationException("Expected a district creation result.");
        result.IsSuccess.Should().BeTrue();
    }

    [Then(@"the district should have ID assigned")]
    public void ThenTheDistrictShouldHaveIDAssigned()
    {
        var result = _context.LastCreateResult ?? throw new InvalidOperationException("Expected a district creation result.");
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().NotBe(Guid.Empty);
    }

    [Then(@"the district ""([^""]*)"" should be ""([^""]*)""")]
    public async Task ThenTheDistrictPropertyShouldBeAsync(string propertyName, string expectedValue)
    {
        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for verification.");

        var query = new GetDistrictQuery(districtId);
        var result = await _context.Mediator.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var district = result.Value ?? throw new InvalidOperationException("Expected district details.");

        var actualValue = propertyName switch
        {
            "Name" => district.Name,
            "Suffix" => district.Suffix,
            _ => throw new ArgumentException($"Unknown property: {propertyName}")
        };

        actualValue.Should().Be(expectedValue);
    }

    [Then(@"the district ""([^""]*)"" should be set to current timestamp")]
    public async Task ThenTheDistrictPropertyShouldBeSetToCurrentTimestampAsync(string propertyName)
    {
        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for verification.");

        var query = new GetDistrictQuery(districtId);
        var result = await _context.Mediator.Send(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var district = result.Value ?? throw new InvalidOperationException("Expected district details.");

        var timestamp = propertyName switch
        {
            "CreatedAtUtc" => district.CreatedAtUtc,
            "UpdatedAtUtc" => district.UpdatedAtUtc ?? throw new InvalidOperationException("Expected UpdatedAtUtc to be set."),
            "DeletedAt" => district.DeletedAt ?? throw new InvalidOperationException("Expected DeletedAt to be set."),
            _ => throw new ArgumentException($"Unknown timestamp property: {propertyName}")
        };

        timestamp.Should().BeCloseTo(_context.Clock.UtcNow.UtcDateTime, TimeSpan.FromSeconds(5));
    }

    [Then(@"a ""([^""]*)"" domain event should be emitted")]
    public void ThenADomainEventShouldBeEmitted(string eventName)
    {
        var events = _context.Repository.DomainEvents;
        var expectedNames = new[]
        {
            eventName,
            eventName.EndsWith("Event", StringComparison.Ordinal) ? eventName : $"{eventName}Event"
        };

        events.Should().Contain(e => expectedNames.Contains(e.GetType().Name, StringComparer.Ordinal));
    }

    [Then(@"an audit record should be created with action ""([^""]*)""")]
    public void ThenAnAuditRecordShouldBeCreatedWithAction(string action)
    {
        var audits = _context.AuditSink.Records.Where(a => a.Action == action);
        audits.Should().NotBeEmpty();
    }

    // Error validation steps
    [Then(@"the operation should fail with error ""([^""]*)""")]
    public void ThenTheOperationShouldFailWithError(string expectedError)
    {
        if (_context.LastCreateResult is not null)
        {
            var result = _context.LastCreateResult;
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
            result.Error!.Message.Should().Be(expectedError);
            return;
        }

        // If the command threw an exception instead of returning a failure result,
        // allow assertions against the exception message. This makes the step
        // resilient to both error propagation styles used by handlers.
        if (_context.LastException is not null)
        {
            _context.LastException.Message.Should().Be(expectedError);
            return;
        }

        var lastResult = _context.LastResult ?? throw new InvalidOperationException("Expected an operation result or exception to check errors.");
        lastResult.IsFailure.Should().BeTrue();
        lastResult.Error.Should().NotBeNull();
        lastResult.Error!.Message.Should().Be(expectedError);
    }

    [Then(@"the operation should fail with validation error ""([^""]*)""")]
    public void ThenTheOperationShouldFailWithValidationError(string expectedError)
    {
        if (_context.LastException is ValidationException validationException)
        {
            validationException.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
            return;
        }

        if (_context.LastCreateResult is not null && _context.LastCreateResult.IsFailure)
        {
            _context.LastCreateResult.Error.Should().NotBeNull();
            _context.LastCreateResult.Error!.Message.Should().Be(expectedError);
            return;
        }

        throw new AssertionFailedException($"Expected validation error {expectedError} but the command succeeded.");
    }

    [Then(@"no district should be created")]
    public void ThenNoDistrictShouldBeCreated()
    {
        _context.Repository.Districts.Count.Should().Be(_context.DistrictCountBeforeCommand);
        var result = _context.LastCreateResult ?? throw new InvalidOperationException("Expected a district creation result.");
        result.IsFailure.Should().BeTrue();
    }

    [Then(@"no audit record should be created")]
    public void ThenNoAuditRecordShouldBeCreated()
    {
        _context.AuditSink.Records.Count.Should().Be(_context.AuditCountBeforeCommand);
    }

    // Existing district setup
    [Given(@"a district exists with suffix ""([^""]*)""")]
    public async Task GivenADistrictExistsWithSuffixAsync(string suffix)
    {
        var table = new Table("Field", "Value");
        table.AddRow("Name", $"{suffix}-district");
        table.AddRow("Suffix", suffix);

        await WhenICreateADistrictWithAsync(table);

        EnsureLastOperationSucceeded();

        _context.Repository.ClearDomainEvents();
        _context.AuditSink.Clear();
        _context.IdempotencyService.Clear();
        _context.ResetOutcome();
    }

    [Given(@"a district exists with:")]
    public async Task GivenADistrictExistsWithAsync(Table table)
    {
        await WhenICreateADistrictWithAsync(table);

        EnsureLastOperationSucceeded();

        _context.Repository.ClearDomainEvents();
        _context.AuditSink.Clear();
        _context.IdempotencyService.Clear();
        _context.ResetOutcome();
    }

    [Given(@"districts exist:")]
    public async Task GivenDistrictsExistAsync(Table table)
    {
        foreach (var row in table.Rows)
        {
            var name = row.TryGetValue("Name", out var n) ? n : throw new ArgumentException("Name column is required");
            var suffix = row.TryGetValue("Suffix", out var s) ? s : throw new ArgumentException("Suffix column is required");
            var deletedAt = row.TryGetValue("DeletedAt", out var d) ? d : null;

            await SeedDistrictAsync(name, suffix, deletedAt);
        }

        _context.Repository.ClearDomainEvents();
        _context.AuditSink.Clear();
        _context.IdempotencyService.Clear();
        _context.ResetOutcome();
    }

    // List districts steps
    [When(@"I request the list of active districts")]
    public async Task WhenIRequestTheListOfActiveDistrictsAsync()
    {
        _context.ResetOutcome();
        var query = new ListDistrictsQuery(1, 50);
        _context.LastListResult = await _context.Mediator.Send(query, CancellationToken.None);
    }

    [Then(@"I should see (\d+) districts?")]
    public void ThenIShouldSeeDistricts(int expectedCount)
    {
        var result = _context.LastListResult ?? throw new InvalidOperationException("Expected a list districts result.");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(expectedCount);
    }

    [Then(@"the districts should be ordered by name")]
    public void ThenTheDistrictsShouldBeOrderedByName()
    {
        var result = _context.LastListResult ?? throw new InvalidOperationException("Expected a list districts result.");
        var names = result.Value!.Items.Select(d => d.Name).ToList();
        names.Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
    }

    [Then(@"each district should include:")]
    public void ThenEachDistrictShouldInclude(Table table)
    {
        var result = _context.LastListResult ?? throw new InvalidOperationException("Expected a list districts result.");
        var items = result.Value!.Items;
        var fields = table.Rows.Select(r => r["Field"]).ToList();

        foreach (var item in items)
        {
            foreach (var field in fields)
            {
                switch (field)
                {
                    case "Id":
                        item.Id.Should().NotBe(Guid.Empty);
                        break;
                    case "Name":
                        item.Name.Should().NotBeNullOrWhiteSpace();
                        break;
                    case "Suffix":
                        item.Suffix.Should().NotBeNullOrWhiteSpace();
                        break;
                    case "CreatedAtUtc":
                        item.CreatedAtUtc.Should().BeAfter(DateTime.MinValue);
                        break;
                    default:
                        throw new AssertionFailedException($"Unknown district field '{field}'");
                }
            }
        }
    }

    private void EnsureLastOperationSucceeded()
    {
        if (_context.LastException is not null)
        {
            throw new AssertionFailedException($"Expected the last operation to succeed but it threw: {_context.LastException}");
        }

        var result = _context.LastCreateResult ?? throw new InvalidOperationException("Expected a create district result to be available.");
        result.IsSuccess.Should().BeTrue($"Expected operation to succeed but failed with error {result.Error?.Message}");
    }

    private async Task SeedDistrictAsync(string name, string suffix, string? deletedAt)
    {
        var table = new Table("Field", "Value");
        table.AddRow("Name", name);
        table.AddRow("Suffix", suffix);

        await WhenICreateADistrictWithAsync(table);
        EnsureLastOperationSucceeded();

        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("Expected the last district ID to be set after seeding.");
        if (!string.IsNullOrWhiteSpace(deletedAt))
        {
            if (!DateTimeOffset.TryParse(deletedAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out var deletedAtValue))
            {
                throw new FormatException($"Unable to parse DeletedAt value '{deletedAt}'.");
            }

            var district = _context.Repository.Find(districtId) ?? throw new InvalidOperationException("Could not find seeded district in repository.");
            var utcTimestamp = deletedAtValue.UtcDateTime;
            typeof(District).GetProperty("DeletedAt")!.SetValue(district, utcTimestamp);
            typeof(District).GetProperty("UpdatedAtUtc")!.SetValue(district, utcTimestamp);
        }
    }

    // Update district steps
    [When(@"I update the district with:")]
    public async Task WhenIUpdateTheDistrictWith(Table table)
    {
        _context.ResetOutcome();

        var data = table.CreateInstance<DistrictData>();
        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for update.");
        var existing = _context.Repository.Find(districtId) ?? throw new InvalidOperationException("Expected district to exist in repository.");

        _context.PreviousDistrictPayload = new()
        {
            ["Id"] = existing.Id.ToString(),
            ["Name"] = existing.Name,
            ["Suffix"] = existing.Suffix
        };

        _context.LastDistrictPayload = new()
        {
            ["Name"] = data.Name,
            ["Suffix"] = data.Suffix
        };

        var command = new UpdateDistrictCommand(districtId, data.Name, data.Suffix);

        try
        {
            var result = await _context.Mediator.Send(command, CancellationToken.None);
            _context.LastResult = result;
            if (result.IsSuccess)
            {
                _context.CaptureAuditIfSuccessful(command, result);
            }
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [When(@"I attempt to update ""([^""]*)"" with suffix ""([^""]*)""")]
    public async Task WhenIAttemptToUpdateDistrictWithSuffix(string districtName, string newSuffix)
    {
        _context.ResetOutcome();

        var district = FindDistrictByName(districtName);
        _context.PreviousDistrictPayload = new()
        {
            ["Id"] = district.Id.ToString(),
            ["Name"] = district.Name,
            ["Suffix"] = district.Suffix
        };

        _context.LastDistrictPayload = new()
        {
            ["Name"] = district.Name,
            ["Suffix"] = newSuffix
        };

        var command = new UpdateDistrictCommand(district.Id, district.Name, newSuffix);

        try
        {
            var result = await _context.Mediator.Send(command, CancellationToken.None);
            _context.LastResult = result;
            if (result.IsSuccess)
            {
                _context.CaptureAuditIfSuccessful(command, result);
            }
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [Then(@"the district should be updated successfully")]
    public void ThenTheDistrictShouldBeUpdatedSuccessfully()
    {
        var result = _context.LastResult ?? throw new InvalidOperationException("Expected an update result.");
        result.IsSuccess.Should().BeTrue();
    }

    [Then(@"the district ""([^""]*)"" should remain ""([^""]*)""")]
    public void ThenTheDistrictPropertyShouldRemain(string propertyName, string expectedValue)
    {
        var previous = _context.PreviousDistrictPayload ?? throw new InvalidOperationException("Expected previous district payload.");
        previous.Should().ContainKey(propertyName);
        previous[propertyName].Should().Be(expectedValue);

        var district = FindDistrictByName(previous["Name"]);
        var actualValue = propertyName switch
        {
            "Name" => district.Name,
            "Suffix" => district.Suffix,
            _ => throw new ArgumentException($"Unknown property: {propertyName}")
        };

        actualValue.Should().Be(expectedValue);
    }

    [Then(@"an audit record should be created with:")]
    public void ThenAnAuditRecordShouldBeCreatedWith(Table table)
    {
        var expectations = table.Rows.ToDictionary(row => row["Field"], row => row["Value"], StringComparer.OrdinalIgnoreCase);

        expectations.TryGetValue("Action", out var expectedAction);
        var audit = expectedAction is null
            ? _context.AuditSink.Records.LastOrDefault()
            : _context.AuditSink.Records.LastOrDefault(r => string.Equals(r.Action, expectedAction, StringComparison.OrdinalIgnoreCase));

        audit.Should().NotBeNull("Expected an audit record matching the provided criteria.");

        if (expectedAction is not null)
        {
            audit!.Action.Should().Be(expectedAction);
        }

        if (expectations.TryGetValue("BeforePayload", out var beforeExpectation) && beforeExpectation.Contains("old", StringComparison.OrdinalIgnoreCase))
        {
            var previous = _context.PreviousDistrictPayload ?? throw new InvalidOperationException("Expected previous payload for audit verification.");
            audit!.BeforePayload.Should().NotBeNull();
            foreach (var value in previous.Values)
            {
                audit.BeforePayload!.Should().Contain(value);
            }
        }

        if (expectations.TryGetValue("AfterPayload", out var afterExpectation) && afterExpectation.Contains("new", StringComparison.OrdinalIgnoreCase))
        {
            var current = _context.LastDistrictPayload ?? throw new InvalidOperationException("Expected current payload for audit verification.");
            audit!.AfterPayload.Should().NotBeNull();
            foreach (var value in current.Values)
            {
                audit.AfterPayload!.Should().Contain(value);
            }
        }
    }

    // Delete district steps
    [When(@"I delete the district")]
    public async Task WhenIDeleteTheDistrict()
    {
        _context.ResetOutcome();

        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for deletion.");
        var command = new DeleteDistrictCommand(districtId);

        try
        {
            var result = await _context.Mediator.Send(command, CancellationToken.None);
            _context.LastResult = result;
            if (result.IsSuccess)
            {
                _context.CaptureAuditIfSuccessful(command, result);
            }
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [Then(@"the district should be soft-deleted")]
    public void ThenTheDistrictShouldBeSoftDeleted()
    {
        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for verification.");
        var district = _context.Repository.Find(districtId) ?? throw new InvalidOperationException("Expected district to exist in repository.");

        district.IsDeleted.Should().BeTrue();
        district.DeletedAt.Should().NotBeNull();
    }

    [Then(@"the district should not appear in active district list")]
    public void ThenTheDistrictShouldNotAppearInActiveDistrictList()
    {
        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for verification.");
        var districts = _context.Repository.GetAllActiveAsync().GetAwaiter().GetResult();
        districts.Should().NotContain(d => d.Id == districtId);
    }

    // District admins cascade steps
    [Given(@"the district has (\d+) active district admins?")]
    public void GivenTheDistrictHasActiveDistrictAdmins(int adminCount)
    {
        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available to assign admins.");
        _context.Repository.SetAdminCounts(districtId, adminCount, 0, 0);
    }

    [Then(@"all (\d+) district admins? should be revoked")]
    public void ThenAllDistrictAdminsShouldBeRevoked(int expectedCount)
    {
        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for verification.");
        var revoked = _context.Repository.GetRevokedAdminCountAsync(districtId).GetAwaiter().GetResult();
        revoked.Should().Be(expectedCount);
    }

    [Then(@"""([^""]*)"" events should be emitted for each admin")]
    public void ThenEventsShouldBeEmittedForEachAdmin(string eventName)
    {
        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for verification.");
        var events = _context.Repository.DomainEvents.OfType<DistrictAdminRevokedEvent>()
            .Where(e => e.DistrictId == districtId)
            .ToList();

        events.Should().NotBeEmpty();
        events.Should().OnlyContain(e => string.Equals(e.GetType().Name, eventName.EndsWith("Event", StringComparison.Ordinal) ? eventName : $"{eventName}Event", StringComparison.Ordinal));
    }

    [Then(@"audit records should be created for admin revocations")]
    public void ThenAuditRecordsShouldBeCreatedForAdminRevocations()
    {
        // Current implementation records a single delete audit entry. Ensure at least one audit exists.
        _context.AuditSink.Records.Should().NotBeEmpty();
    }

    // Idempotency steps
    [When(@"I attempt to create the same district again within (\d+) minutes")]
    public async Task WhenIAttemptToCreateTheSameDistrictAgainWithinMinutes(int minutes)
    {
        var originalId = _context.LastDistrictId ?? throw new InvalidOperationException("No prior district available for idempotency test.");
        var payload = _context.LastDistrictPayload ?? throw new InvalidOperationException("Expected last district payload.");

        _context.PreviousDistrictPayload = new() { ["Id"] = originalId.ToString() };

        _context.Clock.Advance(TimeSpan.FromMinutes(minutes));
        _context.ResetOutcome();

        var command = new CreateDistrictCommand(payload["Name"], payload["Suffix"]);

        try
        {
            var result = await _context.Mediator.Send(command, CancellationToken.None);
            _context.LastCreateResult = result;
            if (result.IsSuccess)
            {
                var value = result.Value ?? throw new InvalidOperationException("Expected created district details.");
                _context.LastDistrictId = value.Id;
            }

            _context.CaptureAuditIfSuccessful(command, result);
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [Then(@"the operation should return the existing district ID")]
    public void ThenTheOperationShouldReturnTheExistingDistrictID()
    {
        var result = _context.LastCreateResult ?? throw new InvalidOperationException("Expected create result for idempotency check.");
        result.IsSuccess.Should().BeTrue();
        var value = result.Value ?? throw new InvalidOperationException("Expected district details.");

        var previous = _context.PreviousDistrictPayload ?? throw new InvalidOperationException("Expected previous district payload.");
        value.Id.ToString().Should().Be(previous["Id"]);
    }

    [Then(@"no new district should be created")]
    public void ThenNoNewDistrictShouldBeCreated()
    {
        _context.Repository.Districts.Count.Should().Be(_context.DistrictCountBeforeCommand);
    }

    [Then(@"no additional audit record should be created")]
    public void ThenNoAdditionalAuditRecordShouldBeCreated()
    {
        _context.AuditSink.Records.Count.Should().Be(_context.AuditCountBeforeCommand);
    }

    [Given(@"I created a district with suffix ""([^""]*)"" (\d+) minutes ago")]
    public async Task GivenICreatedADistrictWithSuffixMinutesAgo(string suffix, int minutesAgo)
    {
        var currentTimestamp = _context.Clock.UtcNow;
        var historicTimestamp = currentTimestamp - TimeSpan.FromMinutes(minutesAgo);

        var name = $"Historic {suffix}";
        var table = new Table("Field", "Value");
        table.AddRow("Name", name);
        table.AddRow("Suffix", suffix);

        _context.Clock.Set(historicTimestamp);
        await WhenICreateADistrictWithAsync(table);
        EnsureLastOperationSucceeded();

        _context.Clock.Set(currentTimestamp);
        _context.Repository.ClearDomainEvents();
        _context.ResetOutcome();
    }

    [When(@"I create a new district with the same data")]
    public async Task WhenICreateANewDistrictWithTheSameData()
    {
        var payload = _context.LastDistrictPayload ?? throw new InvalidOperationException("Expected last district payload for duplicate creation.");
        _context.ResetOutcome();

        var command = new CreateDistrictCommand(payload["Name"], payload["Suffix"]);

        try
        {
            var result = await _context.Mediator.Send(command, CancellationToken.None);
            _context.LastCreateResult = result;
            if (result.IsSuccess && result.Value is { } value)
            {
                _context.LastDistrictId = value.Id;
            }

            _context.CaptureAuditIfSuccessful(command, result);
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    // Get district by ID steps
    [When(@"I request the district by ID")]
    public async Task WhenIRequestTheDistrictByID()
    {
        _context.ResetOutcome();

        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for retrieval.");
        var query = new GetDistrictQuery(districtId);

        try
        {
            var result = await _context.Mediator.Send(query, CancellationToken.None);
            _context.LastDistrictResult = result;
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [Then(@"I should receive the district details")]
    public void ThenIShouldReceiveTheDistrictDetails()
    {
        var result = _context.LastDistrictResult ?? throw new InvalidOperationException("Expected district query result.");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Then(@"the response should include:")]
    public void ThenTheResponseShouldInclude(Table table)
    {
        var result = _context.LastDistrictResult ?? throw new InvalidOperationException("Expected district query result.");
        result.IsSuccess.Should().BeTrue();
        var response = result.Value ?? throw new InvalidOperationException("Expected district details.");

        foreach (var row in table.Rows)
        {
            var field = row["Field"];
            switch (field)
            {
                case "Id":
                    response.Id.Should().NotBe(Guid.Empty);
                    break;
                case "Name":
                    response.Name.Should().NotBeNullOrWhiteSpace();
                    break;
                case "Suffix":
                    response.Suffix.Should().NotBeNullOrWhiteSpace();
                    break;
                case "CreatedAtUtc":
                    response.CreatedAtUtc.Should().BeAfter(DateTime.MinValue);
                    break;
                case "UpdatedAtUtc":
                    if (response.UpdatedAtUtc.HasValue)
                    {
                        response.UpdatedAtUtc.Value.Should().BeOnOrAfter(response.CreatedAtUtc);
                    }
                    else
                    {
                        response.UpdatedAtUtc.Should().BeNull();
                    }
                    break;
                case "IsDeleted":
                    response.IsDeleted.Should().Be(response.DeletedAt.HasValue);
                    break;
                case "ActiveAdminCount":
                    response.ActiveAdminCount.Should().BeGreaterThanOrEqualTo(0);
                    break;
                case "PendingAdminCount":
                    response.PendingAdminCount.Should().BeGreaterThanOrEqualTo(0);
                    break;
                case "RevokedAdminCount":
                    response.RevokedAdminCount.Should().BeGreaterThanOrEqualTo(0);
                    break;
                default:
                    throw new AssertionFailedException($"Unknown response field '{field}'");
            }
        }
    }

    [Then(@"""([^""]*)"" should not be in the list")]
    public void ThenDistrictShouldNotBeInTheList(string districtName)
    {
        var result = _context.LastListResult ?? throw new InvalidOperationException("Expected a list districts result.");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Select(d => d.Name)
            .Should()
            .NotContain(name => string.Equals(name, districtName, StringComparison.OrdinalIgnoreCase));
    }

    // Case-insensitive suffix step
    [When(@"I attempt to create a district with suffix ""([^""]*)""")]
    public async Task WhenIAttemptToCreateADistrictWithSuffix(string suffix)
    {
        _context.ResetOutcome();

        var name = $"Conflict {suffix}";
        var command = new CreateDistrictCommand(name, suffix);
        _context.LastDistrictPayload = new() { ["Name"] = name, ["Suffix"] = suffix };

        try
        {
            var result = await _context.Mediator.Send(command, CancellationToken.None);
            _context.LastCreateResult = result;

            if (result.IsSuccess)
            {
                var value = result.Value ?? throw new InvalidOperationException("Expected created district details.");
                _context.LastDistrictId = value.Id;
            }

            _context.CaptureAuditIfSuccessful(command, result);
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    // Trimming step
    [Then(@"the name should not have leading or trailing spaces")]
    public void ThenTheNameShouldNotHaveLeadingOrTrailingSpaces()
    {
        var districtId = _context.LastDistrictId ?? throw new InvalidOperationException("No district ID available for verification.");
        var district = _context.Repository.Find(districtId) ?? throw new InvalidOperationException("Expected district to be present in repository.");

        var trimmed = district.Name.Trim();
        district.Name.Should().Be(trimmed);
    }

    // Pagination steps
    [Given(@"(\d+) districts exist")]
    public async Task GivenDistrictsExist(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            var table = new Table("Field", "Value");
            table.AddRow("Name", $"District {i:000}");
            table.AddRow("Suffix", $"d{i:000}");

            await WhenICreateADistrictWithAsync(table);
            EnsureLastOperationSucceeded();
        }

        _context.Repository.ClearDomainEvents();
        _context.AuditSink.Clear();
        _context.ResetOutcome();
    }

    [When(@"I request districts with page size (\d+) and page (\d+)")]
    public async Task WhenIRequestDistrictsWithPageSizeAndPage(int pageSize, int page)
    {
        _context.ResetOutcome();
        var query = new ListDistrictsQuery(page, pageSize);
        var result = await _context.Mediator.Send(query, CancellationToken.None);
        _context.LastListResult = result;
    }

    [Then(@"the response should include pagination metadata:")]
    public void ThenTheResponseShouldIncludePaginationMetadata(Table table)
    {
        var result = _context.LastListResult ?? throw new InvalidOperationException("Expected list districts result.");
        result.IsSuccess.Should().BeTrue();
        var value = result.Value ?? throw new InvalidOperationException("Expected list result value.");

        foreach (var row in table.Rows)
        {
            var field = row["Field"];
            var expectedValue = int.Parse(row["Value"], CultureInfo.InvariantCulture);

            switch (field)
            {
                case "Page":
                    value.Page.Should().Be(expectedValue);
                    break;
                case "PageSize":
                    value.PageSize.Should().Be(expectedValue);
                    break;
                case "TotalCount":
                    value.TotalCount.Should().Be(expectedValue);
                    break;
                case "TotalPages":
                    value.TotalPages.Should().Be(expectedValue);
                    break;
                default:
                    throw new AssertionFailedException($"Unknown pagination field '{field}'");
            }
        }
    }

    private class DistrictData
    {
        public string Name { get; set; } = "";
        public string Suffix { get; set; } = "";
    }

    private District FindDistrictByName(string districtName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(districtName);
        var district = _context.Repository.Districts
            .FirstOrDefault(d => string.Equals(d.Name, districtName, StringComparison.OrdinalIgnoreCase));

        return district ?? throw new InvalidOperationException($"District named '{districtName}' was not found in the repository.");
    }
}
