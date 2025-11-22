# Comprehensive Testing Strategy

## Overview

This document defines the testing approach for the NorthStar microservices migration, aligned with the constitutional requirement of ≥80% test coverage and Test-Driven Development (TDD) workflow.

**Testing Philosophy**: Red → Green → Refactor  
**Coverage Target**: ≥80% across all layers  
**Testing Pyramid**: Unit > Integration > BDD > UI

---

## Test-Driven Development (TDD) Workflow

### Constitutional Mandate

Per Constitution v1.6.0, all implementation must follow **Red → Green → Refactor** with evidence capture:

1. **RED Phase**: Write failing test first
   ```bash
   # Create test that fails
   dotnet test > docs/evidence/feature-XXX-red.txt
   ```

2. **GREEN Phase**: Implement minimum code to pass
   ```bash
   # Implement feature
   dotnet test > docs/evidence/feature-XXX-green.txt
   ```

3. **REFACTOR Phase**: Optimize while tests still pass
   ```bash
   # Refactor code
   dotnet test  # Verify still passing
   ```

4. **COMMIT with Evidence**:
   ```bash
   git add .
   git commit -m "Feature: Student enrollment [Phase2]

   - Implemented CreateStudentCommand
   - Added FluentValidation rules
   - Published StudentCreatedEvent
   
   Evidence:
   - docs/evidence/feature-002-red.txt (5 failing tests)
   - docs/evidence/feature-002-green.txt (5 passing tests)
   "
   git push origin HEAD:002review-Phase2
   ```

### Evidence Structure

```
docs/evidence/
├── feature-001-identity-login/
│   ├── phase1-red.txt
│   ├── phase1-green.txt
│   ├── phase2-red.txt
│   └── phase2-green.txt
├── feature-002-student-enrollment/
│   ├── unit-tests-red.txt
│   ├── unit-tests-green.txt
│   ├── bdd-tests-red.txt
│   └── bdd-tests-green.txt
└── ...
```

---

## Testing Pyramid

```
           /\
          /UI\ Playwright (E2E)          ~5%
         /----\ 
        / BDD  \ Reqnroll (Acceptance)   ~10%
       /--------\
      /Integration\ Aspire Tests         ~15%
     /------------\
    /  Unit Tests  \ xUnit                ~70%
   /----------------\
```

**Coverage Distribution**:
- **Unit Tests**: 70% of total tests - Fast, isolated, domain logic
- **Integration Tests**: 15% - Database, message bus, cross-service
- **BDD Tests**: 10% - User acceptance scenarios
- **UI Tests**: 5% - Critical user journeys

---

## Unit Testing Strategy

### Framework: xUnit 2.9.0

**Coverage Target**: ≥80% of all code

**Test Organization**:
```
tests/NorthStar.Students.UnitTests/
├── Domain/
│   ├── StudentTests.cs              # Domain entity tests
│   ├── EnrollmentTests.cs
│   └── ValueObjects/
│       ├── StudentIdTests.cs
│       └── GradeLevelTests.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateStudentCommandHandlerTests.cs
│   │   └── EnrollStudentCommandHandlerTests.cs
│   ├── Queries/
│   │   ├── GetStudentQueryHandlerTests.cs
│   │   └── SearchStudentsQueryHandlerTests.cs
│   └── Validators/
│       └── CreateStudentCommandValidatorTests.cs
└── Infrastructure/
    ├── Repositories/
    │   └── StudentRepositoryTests.cs  # Using in-memory DB
    └── MessageBus/
        └── EventPublisherTests.cs
```

### Unit Test Naming Convention

**Pattern**: `MethodName_Scenario_ExpectedResult`

**Examples**:
- `CreateStudent_WithValidData_ReturnsStudent`
- `CreateStudent_WithInvalidEmail_ThrowsValidationException`
- `EnrollStudent_WhenAlreadyEnrolled_ThrowsDomainException`

### Unit Test Template

```csharp
public class CreateStudentCommandHandlerTests
{
    private readonly Mock<IStudentRepository> _mockRepository;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly CreateStudentCommandHandler _handler;
    
    public CreateStudentCommandHandlerTests()
    {
        _mockRepository = new Mock<IStudentRepository>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        _handler = new CreateStudentCommandHandler(
            _mockRepository.Object,
            _mockEventPublisher.Object);
    }
    
    [Fact]
    public async Task Handle_WithValidCommand_CreatesStudentAndPublishesEvent()
    {
        // Arrange
        var command = new CreateStudentCommand
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(2010, 5, 15),
            GradeLevel = 7
        };
        
        Student createdStudent = null;
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Student>()))
            .Callback<Student>(s => createdStudent = s)
            .ReturnsAsync((Student s) => s);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Once);
        _mockEventPublisher.Verify(
            p => p.PublishAsync(It.Is<StudentCreatedEvent>(
                e => e.StudentId == createdStudent.Id)),
            Times.Once);
    }
    
    [Theory]
    [InlineData("", "Doe")]  // Empty first name
    [InlineData("John", "")] // Empty last name
    [InlineData(null, "Doe")] // Null first name
    public async Task Handle_WithInvalidName_ThrowsValidationException(
        string firstName, string lastName)
    {
        // Arrange
        var command = new CreateStudentCommand
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = new DateTime(2010, 5, 15),
            GradeLevel = 7
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WithFutureDateOfBirth_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateStudentCommand
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.Now.AddDays(1), // Future date
            GradeLevel = 7
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Date of birth must be in the past", exception.Message);
    }
}
```

### Domain Logic Testing

**Focus**: Business rules, invariants, domain events

```csharp
public class StudentTests
{
    [Fact]
    public void UpdateGrade_WithValidGrade_UpdatesGradeLevelAndPublishesEvent()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            GradeLevel = 7
        };
        
        // Act
        student.UpdateGrade(8);
        
        // Assert
        Assert.Equal(8, student.GradeLevel);
        var domainEvent = Assert.Single(student.DomainEvents);
        Assert.IsType<StudentGradeUpdatedEvent>(domainEvent);
    }
    
    [Theory]
    [InlineData(-1)]  // Below minimum
    [InlineData(13)]  // Above maximum
    public void UpdateGrade_WithInvalidGrade_ThrowsDomainException(int invalidGrade)
    {
        // Arrange
        var student = new Student { GradeLevel = 7 };
        
        // Act & Assert
        var exception = Assert.Throws<DomainException>(
            () => student.UpdateGrade(invalidGrade));
        
        Assert.Equal("Invalid grade level", exception.Message);
    }
}
```

### Mocking Guidelines

**Use Moq 4.x**:
```csharp
// Setup method return
_mockRepository.Setup(r => r.GetByIdAsync(studentId))
    .ReturnsAsync(expectedStudent);

// Setup property
_mockContext.Setup(c => c.CurrentUserId).Returns(userId);

// Verify method called
_mockPublisher.Verify(
    p => p.PublishAsync(It.IsAny<StudentCreatedEvent>()), 
    Times.Once);

// Verify method NOT called
_mockRepository.Verify(
    r => r.DeleteAsync(It.IsAny<Guid>()), 
    Times.Never);

// Capture argument
Student capturedStudent = null;
_mockRepository.Setup(r => r.AddAsync(It.IsAny<Student>()))
    .Callback<Student>(s => capturedStudent = s);
```

---

## Integration Testing Strategy

### Framework: xUnit + Aspire Testing

**Purpose**: Test interactions between layers and external dependencies

**Integration Test Categories**:
1. **Database Integration** - EF Core with test database
2. **Message Bus Integration** - MassTransit with test harness
3. **Service-to-Service** - Aspire integration tests
4. **External APIs** - WireMock for external service stubs

### Database Integration Tests

**Use In-Memory SQLite or Test Containers**:

```csharp
public class StudentRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public StudentRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task AddAsync_WithValidStudent_PersistsToDatabase()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new StudentRepository(context);
        
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = "Integration",
            LastName = "Test",
            DateOfBirth = new DateTime(2010, 1, 1),
            GradeLevel = 5
        };
        
        // Act
        await repository.AddAsync(student);
        
        // Assert
        var retrievedStudent = await repository.GetByIdAsync(student.Id);
        Assert.NotNull(retrievedStudent);
        Assert.Equal("Integration", retrievedStudent.FirstName);
    }
}

public class DatabaseFixture : IDisposable
{
    private readonly DbContextOptions<StudentDbContext> _options;
    
    public DatabaseFixture()
    {
        _options = new DbContextOptionsBuilder<StudentDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        
        // Create database schema
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }
    
    public StudentDbContext CreateContext() => new StudentDbContext(_options);
    
    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }
}
```

### Aspire Integration Tests

**Test Cross-Service Communication**:

```csharp
public class StudentServiceIntegrationTests : IClassFixture<AspireAppHostFixture>
{
    private readonly AspireAppHostFixture _fixture;
    
    public StudentServiceIntegrationTests(AspireAppHostFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task CreateStudent_IntegrationFlow_Success()
    {
        // Arrange
        var studentsClient = _fixture.CreateHttpClient("students-api");
        var assessmentsClient = _fixture.CreateHttpClient("assessments-api");
        
        var createRequest = new CreateStudentRequest
        {
            FirstName = "Aspire",
            LastName = "Test",
            DateOfBirth = new DateTime(2010, 5, 15),
            GradeLevel = 7
        };
        
        // Act - Create student
        var createResponse = await studentsClient.PostAsJsonAsync(
            "/api/v1/students", createRequest);
        
        createResponse.EnsureSuccessStatusCode();
        var student = await createResponse.Content.ReadFromJsonAsync<StudentDto>();
        
        // Assert - Student created
        Assert.NotNull(student);
        Assert.Equal("Aspire", student.FirstName);
        
        // Verify in database
        var getResponse = await studentsClient.GetAsync($"/api/v1/students/{student.Id}");
        getResponse.EnsureSuccessStatusCode();
        
        // Verify event propagation - Check if Assessment service received StudentEnrolledEvent
        await Task.Delay(2000); // Wait for async event processing
        
        var assessmentsResponse = await assessmentsClient.GetAsync(
            $"/api/v1/assessments/available?studentId={student.Id}&gradeLevel=7");
        
        assessmentsResponse.EnsureSuccessStatusCode();
        // Assessment service should auto-assign grade-appropriate assessments
    }
}

public class AspireAppHostFixture : IAsyncLifetime
{
    private DistributedApplication _app;
    
    public async Task InitializeAsync()
    {
        var builder = DistributedApplication.CreateBuilder();
        
        // Add test services
        var postgres = builder.AddPostgres("test-postgres");
        var redis = builder.AddRedis("test-redis");
        
        builder.AddProject<Projects.NorthStar_Students_Api>("students-api")
            .WithReference(postgres.AddDatabase("studentsdb"))
            .WithReference(redis);
        
        builder.AddProject<Projects.NorthStar_Assessments_Api>("assessments-api")
            .WithReference(postgres.AddDatabase("assessmentsdb"))
            .WithReference(redis);
        
        _app = await builder.BuildAsync();
        await _app.StartAsync();
    }
    
    public HttpClient CreateHttpClient(string serviceName)
    {
        return _app.CreateHttpClient(serviceName);
    }
    
    public async Task DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
```

### Message Bus Integration Tests

**Use MassTransit Test Harness**:

```csharp
public class StudentEventPublisherIntegrationTests
{
    [Fact]
    public async Task PublishStudentCreatedEvent_PublishesToBus()
    {
        // Arrange
        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer<StudentCreatedEventConsumer>();
        
        await harness.Start();
        
        try
        {
            // Act
            await harness.Bus.Publish(new StudentCreatedEvent
            {
                StudentId = Guid.NewGuid(),
                FirstName = "Event",
                LastName = "Test",
                GradeLevel = 6
            });
            
            // Assert
            Assert.True(await harness.Published.Any<StudentCreatedEvent>());
            Assert.True(await consumerHarness.Consumed.Any<StudentCreatedEvent>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
```

---

## BDD Testing Strategy (Reqnroll)

### Framework: Reqnroll (SpecFlow successor for .NET)

**Purpose**: Acceptance testing, executable specifications

**Feature File Structure**:
```
tests/NorthStar.Students.BddTests/
├── Features/
│   ├── StudentEnrollment.feature
│   ├── StudentDashboard.feature
│   └── AssessmentRecording.feature
├── StepDefinitions/
│   ├── StudentEnrollmentSteps.cs
│   ├── StudentDashboardSteps.cs
│   └── AssessmentRecordingSteps.cs
├── Hooks/
│   └── TestHooks.cs  # Before/After scenario setup
└── Support/
    └── TestContext.cs
```

### BDD Feature Example

**File**: `StudentEnrollment.feature`

```gherkin
Feature: Student Enrollment
  As a school administrator
  I want to enroll students in the system
  So that they can access educational services

  Background:
    Given I am authenticated as a school administrator
    And Lincoln Middle School exists in the system
    And 7th grade is available for enrollment

  Scenario: Enroll a new student successfully
    Given I have a valid student enrollment form
      | Field         | Value         |
      | FirstName     | Michael       |
      | LastName      | Johnson       |
      | DateOfBirth   | 2010-05-15    |
      | GradeLevel    | 7             |
      | SchoolId      | {school-id}   |
    When I submit the student enrollment
    Then a new student record is created
    And the student ID is returned
    And a StudentCreatedEvent is published
    And the student appears in the 7th grade roster
    And I see a success confirmation message

  Scenario: Attempt to enroll a student with missing required fields
    Given I have an incomplete student enrollment form
      | Field         | Value         |
      | FirstName     | Sarah         |
      | LastName      |               |
      | DateOfBirth   | 2010-03-20    |
    When I submit the student enrollment
    Then I receive a validation error
    And the error message indicates "Last name is required"
    And no student record is created

  Scenario: Enroll a student who is already in the system
    Given a student "Alex Martinez" already exists with state ID "12345678"
    And I have a student enrollment form with the same state ID
    When I submit the student enrollment
    Then I receive a conflict error
    And the error message indicates "Student with this State ID already exists"
    And I am shown the existing student record

  Scenario: Auto-assign grade-appropriate assessments upon enrollment
    Given 7th grade has 3 active assessments
    When I enroll a new student in 7th grade
    Then the student is auto-assigned to all 3 assessments
    And a StudentEnrolledEvent is published
    And the Assessment service receives the event
    And the student can view available assessments in their dashboard
```

### Step Definitions

```csharp
[Binding]
public class StudentEnrollmentSteps
{
    private readonly TestContext _context;
    private CreateStudentRequest _enrollmentRequest;
    private HttpResponseMessage _response;
    private StudentDto _createdStudent;
    
    public StudentEnrollmentSteps(TestContext context)
    {
        _context = context;
    }
    
    [Given(@"I am authenticated as a school administrator")]
    public async Task GivenIAmAuthenticatedAsSchoolAdministrator()
    {
        var loginResponse = await _context.ApiClient.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@school.edu",
            password = "AdminPassword123!"
        });
        
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _context.AuthToken = authResult.AccessToken;
        _context.ApiClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _context.AuthToken);
    }
    
    [Given(@"I have a valid student enrollment form")]
    public void GivenIHaveAValidStudentEnrollmentForm(Table table)
    {
        var row = table.Rows[0];
        _enrollmentRequest = new CreateStudentRequest
        {
            FirstName = row["FirstName"],
            LastName = row["LastName"],
            DateOfBirth = DateTime.Parse(row["DateOfBirth"]),
            GradeLevel = int.Parse(row["GradeLevel"]),
            SchoolId = _context.GetSchoolId(row["SchoolId"])
        };
    }
    
    [When(@"I submit the student enrollment")]
    public async Task WhenISubmitTheStudentEnrollment()
    {
        _response = await _context.ApiClient.PostAsJsonAsync(
            "/api/v1/students", _enrollmentRequest);
    }
    
    [Then(@"a new student record is created")]
    public async Task ThenANewStudentRecordIsCreated()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Created);
        _createdStudent = await _response.Content.ReadFromJsonAsync<StudentDto>();
        _createdStudent.Should().NotBeNull();
    }
    
    [Then(@"the student ID is returned")]
    public void ThenTheStudentIdIsReturned()
    {
        _createdStudent.Id.Should().NotBeEmpty();
    }
    
    [Then(@"a StudentCreatedEvent is published")]
    public async Task ThenAStudentCreatedEventIsPublished()
    {
        // Wait for event processing
        await Task.Delay(1000);
        
        // Verify event was published (check message bus test harness or event log)
        var publishedEvents = await _context.GetPublishedEvents<StudentCreatedEvent>();
        publishedEvents.Should().ContainSingle(e => e.StudentId == _createdStudent.Id);
    }
    
    [Then(@"I receive a validation error")]
    public void ThenIReceiveAValidationError()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Then(@"the error message indicates ""(.*)""")]
    public async Task ThenTheErrorMessageIndicates(string expectedMessage)
    {
        var error = await _response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Errors.Values.SelectMany(v => v).Should().Contain(expectedMessage);
    }
}
```

### BDD Test Configuration

**reqnroll.json**:
```json
{
  "language": {
    "feature": "en"
  },
  "bindingCulture": {
    "name": "en-US"
  },
  "runtime": {
    "stopAtFirstError": false,
    "missingOrPendingStepsOutcome": "Inconclusive"
  },
  "trace": {
    "traceSuccessfulSteps": false,
    "traceTimings": true,
    "minTracedDuration": "0:0:0.1"
  }
}
```

---

## UI Testing Strategy (Playwright)

### Framework: Playwright 1.47.0 (C#)

**Purpose**: End-to-end testing of user workflows

**UI Test Structure**:
```
tests/NorthStar.UiTests/
├── PageObjects/
│   ├── LoginPage.cs
│   ├── StudentDashboardPage.cs
│   ├── StudentEnrollmentPage.cs
│   └── AssessmentEntryPage.cs
├── Tests/
│   ├── AuthenticationTests.cs
│   ├── StudentWorkflowTests.cs
│   └── AssessmentWorkflowTests.cs
├── Fixtures/
│   └── PlaywrightFixture.cs
└── playwright.config.json
```

### Page Object Pattern

```csharp
public class StudentEnrollmentPage
{
    private readonly IPage _page;
    
    private ILocator FirstNameInput => _page.Locator("#firstName");
    private ILocator LastNameInput => _page.Locator("#lastName");
    private ILocator DateOfBirthInput => _page.Locator("#dateOfBirth");
    private ILocator GradeLevelSelect => _page.Locator("#gradeLevel");
    private ILocator SubmitButton => _page.Locator("button[type='submit']");
    private ILocator SuccessMessage => _page.Locator(".success-message");
    private ILocator ErrorMessage => _page.Locator(".error-message");
    
    public StudentEnrollmentPage(IPage page)
    {
        _page = page;
    }
    
    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/students/enroll");
    }
    
    public async Task FillEnrollmentFormAsync(
        string firstName, string lastName, DateTime dateOfBirth, int gradeLevel)
    {
        await FirstNameInput.FillAsync(firstName);
        await LastNameInput.FillAsync(lastName);
        await DateOfBirthInput.FillAsync(dateOfBirth.ToString("yyyy-MM-dd"));
        await GradeLevelSelect.SelectOptionAsync(gradeLevel.ToString());
    }
    
    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
    }
    
    public async Task<string> GetSuccessMessageAsync()
    {
        return await SuccessMessage.TextContentAsync();
    }
    
    public async Task<bool> HasErrorAsync(string errorText)
    {
        var errors = await ErrorMessage.AllTextContentsAsync();
        return errors.Any(e => e.Contains(errorText));
    }
}
```

### UI Test Example

```csharp
[Collection("Playwright")]
public class StudentWorkflowTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;
    
    public StudentWorkflowTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task EnrollStudent_CompleteWorkflow_Success()
    {
        // Arrange
        await using var browser = await _fixture.Playwright.Chromium.LaunchAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        var loginPage = new LoginPage(page);
        var enrollmentPage = new StudentEnrollmentPage(page);
        
        // Act - Login
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync("admin@school.edu", "AdminPassword123!");
        await page.WaitForURLAsync("/dashboard");
        
        // Act - Navigate to enrollment
        await enrollmentPage.NavigateAsync();
        
        // Act - Fill form
        await enrollmentPage.FillEnrollmentFormAsync(
            firstName: "UITest",
            lastName: "Student",
            dateOfBirth: new DateTime(2010, 6, 1),
            gradeLevel: 7);
        
        // Act - Submit
        await enrollmentPage.SubmitAsync();
        
        // Assert - Success message
        await page.WaitForSelectorAsync(".success-message");
        var successMessage = await enrollmentPage.GetSuccessMessageAsync();
        Assert.Contains("Student enrolled successfully", successMessage);
        
        // Assert - Redirected to student list
        await page.WaitForURLAsync("/students");
        
        // Assert - New student appears in list
        var studentRow = page.Locator("tr:has-text('UITest Student')");
        await Expect(studentRow).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task EnrollStudent_WithInvalidData_ShowsValidationErrors()
    {
        // Arrange
        await using var browser = await _fixture.Playwright.Chromium.LaunchAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        var loginPage = new LoginPage(page);
        var enrollmentPage = new StudentEnrollmentPage(page);
        
        // Act - Login and navigate
        await loginPage.NavigateAsync();
        await loginPage.LoginAsync("admin@school.edu", "AdminPassword123!");
        await enrollmentPage.NavigateAsync();
        
        // Act - Fill form with invalid data (empty last name)
        await enrollmentPage.FirstNameInput.FillAsync("Invalid");
        await enrollmentPage.LastNameInput.FillAsync("");  // Empty - should error
        
        // Act - Submit
        await enrollmentPage.SubmitAsync();
        
        // Assert - Validation error shown
        var hasError = await enrollmentPage.HasErrorAsync("Last name is required");
        Assert.True(hasError);
        
        // Assert - Form not submitted (still on enrollment page)
        Assert.Contains("/students/enroll", page.Url);
    }
}
```

### Playwright Configuration

**playwright.config.json**:
```json
{
  "use": {
    "baseURL": "https://localhost:5001",
    "trace": "on-first-retry",
    "screenshot": "only-on-failure",
    "video": "retain-on-failure"
  },
  "projects": [
    {
      "name": "chromium",
      "use": { "browserName": "chromium" }
    },
    {
      "name": "firefox",
      "use": { "browserName": "firefox" }
    },
    {
      "name": "webkit",
      "use": { "browserName": "webkit" }
    }
  ],
  "reporter": [
    ["html", { "outputFolder": "playwright-report" }],
    ["junit", { "outputFile": "test-results/junit.xml" }]
  ]
}
```

---

## Performance Testing

### Framework: NBomber (load testing)

**Performance Test Example**:

```csharp
public class StudentServiceLoadTests
{
    [Fact]
    public void CreateStudent_LoadTest_MeetsP95Target()
    {
        var scenario = Scenario.Create("create_student", async context =>
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", GetTestToken());
            
            var request = new CreateStudentRequest
            {
                FirstName = $"Load{context.InvocationNumber}",
                LastName = "Test",
                DateOfBirth = new DateTime(2010, 1, 1),
                GradeLevel = 7
            };
            
            var response = await httpClient.PostAsJsonAsync(
                "https://localhost:5002/api/v1/students", request);
            
            return response.IsSuccessStatusCode 
                ? Response.Ok() 
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), 
                during: TimeSpan.FromMinutes(1))
        );
        
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
        
        var p95 = stats.ScenarioStats[0].StepStats[0].Ok.Latency.Percent95;
        
        // Assert P95 < 200ms (SLO)
        Assert.True(p95 < 200, $"P95 latency {p95}ms exceeds 200ms target");
    }
}
```

---

## Test Coverage Reporting

### Tool: Coverlet + ReportGenerator

**Generate Coverage Report**:

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage/

# Generate HTML report
reportgenerator \
  -reports:./coverage/coverage.opencover.xml \
  -targetdir:./coverage/report \
  -reporttypes:Html

# Open report
start ./coverage/report/index.html
```

**Coverage Thresholds** (fail build if not met):

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <Threshold>80</Threshold>
  <ThresholdType>line</ThresholdType>
  <ThresholdStat>total</ThresholdStat>
</PropertyGroup>
```

---

## CI/CD Integration

**GitHub Actions Workflow**:

```yaml
name: Test & Coverage

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Run Unit Tests
        run: dotnet test tests/**/*.UnitTests --no-build --verbosity normal
      
      - name: Run Integration Tests
        run: dotnet test tests/**/*.IntegrationTests --no-build
      
      - name: Run BDD Tests
        run: dotnet test tests/**/*.BddTests --no-build
      
      - name: Generate Coverage Report
        run: |
          dotnet test /p:CollectCoverage=true \
            /p:CoverletOutputFormat=opencover \
            /p:Threshold=80 \
            /p:ThresholdType=line
      
      - name: Upload Coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          file: ./coverage/coverage.opencover.xml
      
      - name: Fail if coverage < 80%
        run: |
          COVERAGE=$(grep -oP 'line-rate="\K[0-9.]+' coverage/coverage.opencover.xml | head -1)
          if (( $(echo "$COVERAGE < 0.8" | bc -l) )); then
            echo "Coverage $COVERAGE is below 80% threshold"
            exit 1
          fi
```

---

## Testing Checklist

### Per Feature
- [ ] Unit tests written (TDD Red phase)
- [ ] Unit tests passing (TDD Green phase)
- [ ] Code refactored while tests pass
- [ ] Integration tests for database operations
- [ ] Integration tests for event publishing
- [ ] BDD scenario written in Gherkin
- [ ] BDD step definitions implemented
- [ ] BDD test passing
- [ ] UI test for critical user journey (if applicable)
- [ ] Performance test if SLO-critical endpoint
- [ ] Test evidence captured (red/green)
- [ ] Coverage ≥80% verified
- [ ] All tests passing in CI/CD

---

**Version**: 1.0  
**Last Updated**: November 15, 2025  
**Owner**: Quality Assurance Team  
**Status**: Strategy Complete - Ready for Implementation
