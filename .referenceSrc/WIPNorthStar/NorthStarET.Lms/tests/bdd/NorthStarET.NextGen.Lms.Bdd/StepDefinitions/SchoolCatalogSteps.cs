using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NorthStarET.NextGen.Lms.Bdd.Support;
using NorthStarET.NextGen.Lms.Domain.Schools;
using Reqnroll;
using Reqnroll.Assist;

namespace NorthStarET.NextGen.Lms.Bdd.StepDefinitions;

[Binding]
public sealed class SchoolCatalogSteps
{
    private readonly SchoolCatalogScenarioContext _context;

    public SchoolCatalogSteps(SchoolCatalogScenarioContext context)
    {
        _context = context;
    }

    #region Background Steps

    [Given("^a district \"(.*)\" exists with ID \"(.*)\"$")]
    public void GivenADistrictExistsWithID(string districtName, string districtId)
    {
        _context.RegisterDistrict(districtName, Guid.Parse(districtId));
    }

    [Given("^I am authenticated as a district admin for \"(.*)\"$")]
    public void GivenIAmAuthenticatedAsADistrictAdminFor(string districtName)
    {
        _context.SetCurrentDistrict(districtName);
    }

    #endregion

    #region Given Steps - Data Setup

    [Given("^the following schools exist in \"(.*)\"$")]
    public async Task GivenTheFollowingSchoolsExistInAsync(string districtName, Table table)
    {
        foreach (var row in table.Rows)
        {
            var name = row.TryGetValue("Name", out var nameValue) ? nameValue : throw new ArgumentException("Name column is required");
            row.TryGetValue("Code", out var codeValue);
            var status = row.TryGetValue("Status", out var statusValue) ? statusValue : "Active";

            await SeedSchoolAsync(districtName, name, codeValue, status).ConfigureAwait(false);
        }
    }

    [Given("^a school \"(.*)\" exists in \"(.*)\"$")]
    public Task GivenASchoolExistsInAsync(string schoolName, string districtName)
    {
        return SeedSchoolAsync(districtName, schoolName, code: null, status: "Active");
    }

    [Given("^a school \"(.*)\" with code \"(.*)\" exists in \"(.*)\"$")]
    public Task GivenASchoolWithCodeExistsInAsync(string schoolName, string code, string districtName)
    {
        return SeedSchoolAsync(districtName, schoolName, code, "Active");
    }

    #endregion

    #region When Steps - Actions

    [When("^I navigate to \"(.*)\"$")]
    public async Task WhenINavigateToAsync(string pageName)
    {
        if (!pageName.Equals("School Management", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Unknown page '{pageName}' requested in scenario.");
        }

        await _context.ExecuteListQueryAsync().ConfigureAwait(false);
    }

    [When("^I click \"(.*)\" button$")]
    public async Task WhenIClickButtonAsync(string buttonName)
    {
        switch (buttonName)
        {
            case "Create School":
                if (_context.CreateDraft is null)
                {
                    _context.BeginCreateDraft();
                    return;
                }

                await _context.SubmitCreateAsync().ConfigureAwait(false);
                return;

            case "Update School":
                await _context.SubmitUpdateAsync().ConfigureAwait(false);
                return;

            default:
                throw new ArgumentException($"Unsupported button '{buttonName}' in scenario.");
        }
    }

    [When("^I click the edit icon for \"(.*)\"$")]
    public void WhenIClickTheEditIconFor(string schoolName)
    {
        var school = _context.Repository.FindByName(_context.CurrentDistrictId, schoolName)
                     ?? throw new InvalidOperationException($"School '{schoolName}' not found for edit.");
        _context.BeginUpdateDraft(school);
    }

    [When("^I click the delete icon for \"(.*)\"$")]
    public void WhenIClickTheDeleteIconFor(string schoolName)
    {
        var school = _context.Repository.FindByName(_context.CurrentDistrictId, schoolName)
                     ?? throw new InvalidOperationException($"School '{schoolName}' not found for delete.");
        _context.SetPendingDelete(school);
    }

    [When("^I fill in school name with \"(.*)\"$")]
    public void WhenIFillInSchoolNameWith(string schoolName)
    {
        if (_context.CreateDraft is null)
        {
            throw new InvalidOperationException("Create school draft has not been started.");
        }

        _context.CreateDraft.Name = schoolName;
    }

    [When("^I fill in school code with \"(.*)\"$")]
    public void WhenIFillInSchoolCodeWith(string code)
    {
        if (_context.CreateDraft is null)
        {
            throw new InvalidOperationException("Create school draft has not been started.");
        }

        _context.CreateDraft.Code = string.IsNullOrWhiteSpace(code) ? null : code;
    }

    [When("^I change the school name to \"(.*)\"$")]
    public void WhenIChangeTheSchoolNameTo(string newName)
    {
        if (_context.UpdateDraft is null)
        {
            throw new InvalidOperationException("Update school draft has not been started.");
        }

        _context.UpdateDraft.Name = newName;
    }

    [When("^I change the code to \"(.*)\"$")]
    public void WhenIChangeTheCodeTo(string newCode)
    {
        if (_context.UpdateDraft is null)
        {
            throw new InvalidOperationException("Update school draft has not been started.");
        }

        _context.UpdateDraft.Code = string.IsNullOrWhiteSpace(newCode) ? null : newCode;
    }

    [When("^I select grades \"(.*)\"$")]
    public void WhenISelectGrades(string grades)
    {
        if (_context.CreateDraft is null)
        {
            throw new InvalidOperationException("Create school draft has not been started.");
        }

        _context.CreateDraft.Grades.Clear();
        foreach (var gradeLevel in ParseGrades(grades))
        {
            if (!_context.CreateDraft.Grades.Contains(gradeLevel))
            {
                _context.CreateDraft.Grades.Add(gradeLevel);
            }
        }
    }

    [When("^I enter \"(.*)\" in the search box$")]
    public async Task WhenIEnterInTheSearchBoxAsync(string searchTerm)
    {
        _context.ActiveSearchTerm = searchTerm;
        await _context.ExecuteListQueryAsync().ConfigureAwait(false);
    }

    [When("^I click \"(.*)\" button in the modal$")]
    public async Task WhenIClickButtonInTheModalAsync(string buttonName)
    {
        switch (buttonName)
        {
            case "Delete School":
                await _context.SubmitDeleteAsync().ConfigureAwait(false);
                break;
            case "Cancel":
                _context.CancelPendingDelete();
                break;
            default:
                throw new ArgumentException($"Unsupported modal button '{buttonName}'.");
        }
    }

    #endregion

    #region Then Steps - Assertions

    [Then("^I should see (.*) schools? in the (?:list|filtered list)$")]
    public void ThenIShouldSeeSchoolsInTheList(int expectedCount)
    {
        _context.LastListItems.Should().HaveCount(expectedCount);
    }

    [Then("^I should see \"(.*)\" in the school list$")]
    public void ThenIShouldSeeInTheSchoolList(string schoolName)
    {
        _context.LastListItems.Select(i => i.Name).Should().Contain(schoolName);
    }

    [Then("^I should not see \"(.*)\" in the school list$")]
    public void ThenIShouldNotSeeInTheSchoolList(string schoolName)
    {
        _context.LastListItems.Select(i => i.Name).Should().NotContain(schoolName);
    }

    [Then("^I should see search and sort controls$")]
    public void ThenIShouldSeeSearchAndSortControls()
    {
        _context.SearchControlsVisible.Should().BeTrue();
    }

    [Then("^I should see \"(.*)\" confirmation$")]
    public void ThenIShouldSeeConfirmation(string message)
    {
        _context.LastSuccessMessage.Should().Be(message);
    }

    [Then("^\"(.*)\" should appear in the school list within (.*) seconds$")]
    public async Task ThenShouldAppearInTheSchoolListWithinSecondsAsync(string schoolName, int seconds)
    {
        await _context.ExecuteListQueryAsync().ConfigureAwait(false);
        _context.LastListItems.Select(i => i.Name).Should().Contain(schoolName);
    }

    [Then("^the school should have code \"(.*)\"$")]
    public void ThenTheSchoolShouldHaveCode(string expectedCode)
    {
        var school = _context.Repository.FindByName(_context.CurrentDistrictId, _context.LastSchoolDetail?.Name ?? string.Empty)
                     ?? throw new InvalidOperationException("Expected recently accessed school to exist in repository.");
        school.Code.Should().Be(expectedCode);
    }

    [Then("^I should see a delete confirmation modal$")]
    public void ThenIShouldSeeADeleteConfirmationModal()
    {
        _context.PendingDeleteSchoolId.Should().NotBeNull();
    }

    [Then("^\"(.*)\" should disappear from the school list within (.*) seconds$")]
    public async Task ThenShouldDisappearFromTheSchoolListWithinSecondsAsync(string schoolName, int seconds)
    {
        await _context.ExecuteListQueryAsync().ConfigureAwait(false);
        _context.LastListItems.Select(i => i.Name).Should().NotContain(schoolName);
    }

    [Then("^the modal should close$")]
    public void ThenTheModalShouldClose()
    {
        _context.PendingDeleteSchoolId.Should().BeNull();
    }

    [Then("^\"(.*)\" should remain in the school list$")]
    public async Task ThenShouldRemainInTheSchoolListAsync(string schoolName)
    {
        await _context.ExecuteListQueryAsync().ConfigureAwait(false);
        _context.LastListItems.Select(i => i.Name).Should().Contain(schoolName);
    }

    [Then("^I should see an error \"(.*)\"$")]
    public void ThenIShouldSeeAnError(string errorMessage)
    {
        _context.LastErrorMessage.Should().Be(errorMessage);
    }

    [Then("^the school should not be created$")]
    public void ThenTheSchoolShouldNotBeCreated()
    {
        _context.LastCommandResult.Should().NotBeNull();
        _context.LastCommandResult!.IsSuccess.Should().BeFalse();
    }

    [Then("^I should only see schools from \"(.*)\"$")]
    public void ThenIShouldOnlySeeSchoolsFrom(string districtName)
    {
        var expectedDistrictId = _context.GetDistrictId(districtName);
        var otherDistrictNames = _context.Repository.Schools
            .Where(s => s.DistrictId != expectedDistrictId)
            .Select(s => s.Name)
            .ToList();

        _context.LastListItems.Select(i => i.Name).Should().NotIntersectWith(otherDistrictNames);
    }

    [Then("^the school list should show \"(.*)\" within (.*) seconds$")]
    public async Task ThenTheSchoolListShouldShowWithinSecondsAsync(string schoolName, int seconds)
    {
        await _context.ExecuteListQueryAsync().ConfigureAwait(false);
        _context.LastListItems.Select(i => i.Name).Should().Contain(schoolName);
    }

    #endregion

    #region Grade Configuration Steps (US2)

    [Given("^the school currently serves grades \"(.*)\"$")]
    public async Task GivenTheSchoolCurrentlyServesGradesAsync(string grades)
    {
        if (_context.UpdateDraft is null)
        {
            throw new InvalidOperationException("No school is being edited; cannot set existing grades.");
        }

        var gradeList = ParseGrades(grades).ToList();
        await _context.SetSchoolGradesAsync(gradeList).ConfigureAwait(false);
    }

    [When("^I navigate to the \"(.*)\" tab$")]
    public void WhenINavigateToTheTab(string tabName)
    {
        // UI navigation - for BDD we just track the context
        _context.ActiveTab = tabName;
    }

    [When("^I select grade range from \"(.*)\" to \"(.*)\"$")]
    public void WhenISelectGradeRangeFromTo(string startGrade, string endGrade)
    {
        if (_context.UpdateDraft is null && _context.CreateDraft is null)
        {
            throw new InvalidOperationException("No draft is active for grade range selection.");
        }

        var start = Enum.Parse<GradeLevel>(startGrade, ignoreCase: false);
        var end = Enum.Parse<GradeLevel>(endGrade, ignoreCase: false);
        var range = GetGradeRange(start, end).ToList();

        if (_context.UpdateDraft is not null)
        {
            _context.UpdateDraft.Grades.Clear();
            foreach (var grade in range)
            {
                _context.UpdateDraft.Grades.Add(grade);
            }
        }
        else if (_context.CreateDraft is not null)
        {
            _context.CreateDraft.Grades.Clear();
            foreach (var grade in range)
            {
                _context.CreateDraft.Grades.Add(grade);
            }
        }
    }

    [When("^I click \"(.*)\" button$")]
    public async Task WhenIClickSpecificButtonAsync(string buttonName)
    {
        switch (buttonName)
        {
            case "Save Grades":
                await _context.SubmitGradeAssignmentAsync().ConfigureAwait(false);
                break;
            case "Select All Grades":
                SelectAllGrades();
                break;
            default:
                await WhenIClickButtonAsync(buttonName).ConfigureAwait(false);
                break;
        }
    }

    [When("^I deselect all grades$")]
    public void WhenIDeselectAllGrades()
    {
        if (_context.UpdateDraft is not null)
        {
            _context.UpdateDraft.Grades.Clear();
        }
        else if (_context.CreateDraft is not null)
        {
            _context.CreateDraft.Grades.Clear();
        }
    }

    [When("^I deselect grade \"(.*)\"$")]
    public void WhenIDeselectGrade(string grade)
    {
        var gradeLevel = Enum.Parse<GradeLevel>(grade, ignoreCase: false);
        
        if (_context.UpdateDraft is not null)
        {
            _context.UpdateDraft.Grades.Remove(gradeLevel);
        }
        else if (_context.CreateDraft is not null)
        {
            _context.CreateDraft.Grades.Remove(gradeLevel);
        }
    }

    [When("^I select grade \"(.*)\"$")]
    public void WhenISelectGrade(string grade)
    {
        var gradeLevel = Enum.Parse<GradeLevel>(grade, ignoreCase: false);
        
        if (_context.UpdateDraft is not null)
        {
            if (!_context.UpdateDraft.Grades.Contains(gradeLevel))
            {
                _context.UpdateDraft.Grades.Add(gradeLevel);
            }
        }
        else if (_context.CreateDraft is not null)
        {
            if (!_context.CreateDraft.Grades.Contains(gradeLevel))
            {
                _context.CreateDraft.Grades.Add(gradeLevel);
            }
        }
    }

    [Then("^the school should serve grades \"(.*)\"$")]
    public void ThenTheSchoolShouldServeGrades(string expectedGrades)
    {
        var expected = ParseGrades(expectedGrades).OrderBy(g => g).ToList();
        var school = _context.Repository.FindByName(_context.CurrentDistrictId, _context.LastSchoolDetail?.Name ?? string.Empty)
                     ?? throw new InvalidOperationException("Expected school not found for grade verification.");
        
        var actual = school.GradeOfferings.Select(o => o.GradeLevel).OrderBy(g => g).ToList();
        actual.Should().BeEquivalentTo(expected);
    }

    [Then("^the school should serve all available grades$")]
    public void ThenTheSchoolShouldServeAllAvailableGrades()
    {
        var school = _context.Repository.FindByName(_context.CurrentDistrictId, _context.LastSchoolDetail?.Name ?? string.Empty)
                     ?? throw new InvalidOperationException("Expected school not found for grade verification.");
        
        var allGrades = Enum.GetValues<GradeLevel>();
        var actualGrades = school.GradeOfferings.Select(o => o.GradeLevel).ToList();
        actualGrades.Should().HaveCount(allGrades.Length);
    }

    [Then("^I should see an error message \"(.*)\"$")]
    public void ThenIShouldSeeAnErrorMessage(string expectedMessage)
    {
        _context.LastErrorMessage.Should().Contain(expectedMessage);
    }

    [Then("^the grades should not be saved$")]
    public void ThenTheGradesShouldNotBeSaved()
    {
        _context.LastCommandResult.Should().NotBeNull();
        _context.LastCommandResult!.IsSuccess.Should().BeFalse();
    }

    [Then("^the school detail should display grades \"(.*)\"$")]
    public void ThenTheSchoolDetailShouldDisplayGrades(string expectedGrades)
    {
        ThenTheSchoolShouldServeGrades(expectedGrades);
    }

    [Then("^the grade list should be ordered sequentially$")]
    public void ThenTheGradeListShouldBeOrderedSequentially()
    {
        var school = _context.Repository.FindByName(_context.CurrentDistrictId, _context.LastSchoolDetail?.Name ?? string.Empty)
                     ?? throw new InvalidOperationException("Expected school not found for grade verification.");
        
        var grades = school.GradeOfferings.Select(o => o.GradeLevel).ToList();
        grades.Should().BeInAscendingOrder();
    }

    private void SelectAllGrades()
    {
        if (_context.UpdateDraft is not null)
        {
            _context.UpdateDraft.Grades.Clear();
            foreach (var grade in Enum.GetValues<GradeLevel>())
            {
                _context.UpdateDraft.Grades.Add(grade);
            }
        }
        else if (_context.CreateDraft is not null)
        {
            _context.CreateDraft.Grades.Clear();
            foreach (var grade in Enum.GetValues<GradeLevel>())
            {
                _context.CreateDraft.Grades.Add(grade);
            }
        }
    }

    private static IEnumerable<GradeLevel> GetGradeRange(GradeLevel start, GradeLevel end)
    {
        var allGrades = Enum.GetValues<GradeLevel>().ToList();
        var startIndex = allGrades.IndexOf(start);
        var endIndex = allGrades.IndexOf(end);

        if (startIndex == -1 || endIndex == -1 || startIndex > endIndex)
        {
            throw new ArgumentException($"Invalid grade range: {start} to {end}");
        }

        for (int i = startIndex; i <= endIndex; i++)
        {
            yield return allGrades[i];
        }
    }

    #endregion

    private async Task SeedSchoolAsync(string districtName, string schoolName, string? code, string status)
    {
        var districtId = _context.GetDistrictId(districtName);
        var creator = _context.CurrentUserId != Guid.Empty ? _context.CurrentUserId : Guid.NewGuid();

        var school = School.Create(districtId, schoolName, code, null, creator);
        await _context.Repository.AddAsync(school).ConfigureAwait(false);

        if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            school.Delete(creator);
            await _context.Repository.UpdateAsync(school).ConfigureAwait(false);
        }
    }

    private static IEnumerable<GradeLevel> ParseGrades(string grades)
    {
        if (string.IsNullOrWhiteSpace(grades))
        {
            yield break;
        }

        foreach (var token in grades.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return Enum.Parse<GradeLevel>(token, ignoreCase: false);
        }
    }
}
