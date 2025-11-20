# Development Guide - NorthStar Microservices

## Table of Contents
1. [Getting Started](#getting-started)
2. [Development Environment Setup](#development-environment-setup)
3. [Service Development Workflow](#service-development-workflow)
4. [Coding Standards](#coding-standards)
5. [Testing Guidelines](#testing-guidelines)
6. [API Design Guidelines](#api-design-guidelines)
7. [Data Access Patterns](#data-access-patterns)
8. [Event-Driven Communication](#event-driven-communication)
9. [Logging and Monitoring](#logging-and-monitoring)
10. [Security Best Practices](#security-best-practices)
11. [Troubleshooting](#troubleshooting)

## Getting Started

### Prerequisites

**Required Software:**
- .NET 8 SDK or later
- Docker Desktop
- Visual Studio 2022 or VS Code
- SQL Server 2019+ or Docker SQL Server
- Git
- Postman or similar API testing tool

**Optional but Recommended:**
- Kubernetes (Docker Desktop includes it)
- Azure CLI (for cloud deployments)
- Redis (for caching)
- RabbitMQ or Azure Service Bus (for messaging)

### Initial Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/crisweber2600/northstaretOld.git
   cd northstaretOld
   ```

2. **Install .NET Templates**
   ```bash
   # Install service template (when available)
   dotnet new -i ./microservices/templates/service-template
   ```

3. **Set Up Development Database**
   ```bash
   # Using Docker
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
      -p 1433:1433 --name northstar-sql -d mcr.microsoft.com/mssql/server:2019-latest
   ```

4. **Set Up Message Bus (RabbitMQ)**
   ```bash
   docker run -d --name northstar-rabbitmq \
      -p 5672:5672 -p 15672:15672 \
      rabbitmq:3-management
   ```

5. **Set Up Redis Cache**
   ```bash
   docker run -d --name northstar-redis -p 6379:6379 redis:latest
   ```

## Development Environment Setup

### Visual Studio 2022

1. Open `NorthStar4_Framework46.sln` (legacy monolith) or individual microservice solutions
2. Restore NuGet packages
3. Set up multiple startup projects for local microservices development
4. Configure user secrets for connection strings

### VS Code

1. Install recommended extensions:
   - C# Dev Kit
   - Docker
   - Kubernetes
   - REST Client
   - GitLens

2. Open workspace:
   ```bash
   code northstaretOld.code-workspace
   ```

### Configuration

Each service uses `appsettings.json` for configuration with environment-specific overrides.

**appsettings.Development.json example:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NorthStar_Student_DB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Secret": "your-development-secret-key-min-32-chars",
    "Issuer": "https://localhost:5001",
    "Audience": "northstar-api",
    "ExpirationMinutes": 60
  },
  "MessageBus": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Use User Secrets for sensitive data:**
```bash
cd src/StudentManagement.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
dotnet user-secrets set "JwtSettings:Secret" "your-secret-key"
```

## Service Development Workflow

### Creating a New Service

1. **Use the Service Template**
   ```bash
   cd microservices/services
   dotnet new northstar-service -n StudentManagement
   ```

2. **Project Structure**
   ```
   StudentManagement/
   ├── src/
   │   ├── StudentManagement.API/
   │   │   ├── Controllers/
   │   │   ├── Middleware/
   │   │   ├── Program.cs
   │   │   ├── appsettings.json
   │   │   └── StudentManagement.API.csproj
   │   ├── StudentManagement.Core/
   │   │   ├── Entities/
   │   │   ├── Events/
   │   │   ├── Interfaces/
   │   │   ├── Services/
   │   │   └── StudentManagement.Core.csproj
   │   ├── StudentManagement.Infrastructure/
   │   │   ├── Data/
   │   │   ├── Repositories/
   │   │   ├── MessageBus/
   │   │   └── StudentManagement.Infrastructure.csproj
   │   └── StudentManagement.Contracts/
   │       ├── DTOs/
   │       ├── Requests/
   │       ├── Responses/
   │       └── StudentManagement.Contracts.csproj
   └── tests/
       ├── StudentManagement.UnitTests/
       ├── StudentManagement.IntegrationTests/
       └── StudentManagement.ContractTests/
   ```

3. **Define Domain Models**
   ```csharp
   // StudentManagement.Core/Entities/Student.cs
   public class Student : BaseEntity
   {
       public string FirstName { get; set; }
       public string LastName { get; set; }
       public string StudentId { get; set; }
       public DateTime DateOfBirth { get; set; }
       public int GradeLevel { get; set; }
       
       // Domain logic
       public void UpdateGrade(int newGrade)
       {
           if (newGrade < 0 || newGrade > 12)
               throw new DomainException("Invalid grade level");
           
           GradeLevel = newGrade;
           AddDomainEvent(new StudentGradeUpdatedEvent(Id, newGrade));
       }
   }
   ```

4. **Implement Repository**
   ```csharp
   // StudentManagement.Infrastructure/Repositories/StudentRepository.cs
   public class StudentRepository : IStudentRepository
   {
       private readonly ApplicationDbContext _context;
       
       public StudentRepository(ApplicationDbContext context)
       {
           _context = context;
       }
       
       public async Task<Student> GetByIdAsync(Guid id)
       {
           return await _context.Students
               .FirstOrDefaultAsync(s => s.Id == id);
       }
       
       public async Task<Student> AddAsync(Student student)
       {
           await _context.Students.AddAsync(student);
           await _context.SaveChangesAsync();
           return student;
       }
   }
   ```

5. **Create API Controller**
   ```csharp
   // StudentManagement.API/Controllers/StudentsController.cs
   [ApiController]
   [Route("api/v1/[controller]")]
   [Authorize]
   public class StudentsController : ControllerBase
   {
       private readonly IStudentRepository _repository;
       private readonly ILogger<StudentsController> _logger;
       
       public StudentsController(
           IStudentRepository repository,
           ILogger<StudentsController> logger)
       {
           _repository = repository;
           _logger = logger;
       }
       
       [HttpGet("{id}")]
       [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
       [ProducesResponseType(StatusCodes.Status404NotFound)]
       public async Task<ActionResult<StudentDto>> GetStudent(Guid id)
       {
           var student = await _repository.GetByIdAsync(id);
           if (student == null)
               return NotFound();
           
           return Ok(student.ToDto());
       }
       
       [HttpPost]
       [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
       [ProducesResponseType(StatusCodes.Status400BadRequest)]
       public async Task<ActionResult<StudentDto>> CreateStudent(
           [FromBody] CreateStudentRequest request)
       {
           var student = new Student
           {
               FirstName = request.FirstName,
               LastName = request.LastName,
               StudentId = request.StudentId,
               DateOfBirth = request.DateOfBirth,
               GradeLevel = request.GradeLevel
           };
           
           var created = await _repository.AddAsync(student);
           
           return CreatedAtAction(
               nameof(GetStudent),
               new { id = created.Id },
               created.ToDto());
       }
   }
   ```

### Running Services Locally

**Single Service:**
```bash
cd src/StudentManagement.API
dotnet run
```

**Multiple Services with Docker Compose:**
```bash
docker-compose up
```

**With Hot Reload:**
```bash
dotnet watch run
```

## Coding Standards

### General Principles

1. **SOLID Principles** - Follow SOLID design principles
2. **Clean Code** - Write self-documenting code
3. **DRY** - Don't Repeat Yourself
4. **YAGNI** - You Aren't Gonna Need It
5. **KISS** - Keep It Simple, Stupid

### C# Coding Conventions

```csharp
// Use PascalCase for class names and public members
public class StudentService
{
    // Use camelCase for private fields with underscore prefix
    private readonly IStudentRepository _repository;
    
    // Use PascalCase for properties
    public string ServiceName { get; set; }
    
    // Use camelCase for parameters and local variables
    public async Task<Student> GetStudentAsync(Guid studentId)
    {
        var student = await _repository.GetByIdAsync(studentId);
        return student;
    }
    
    // Use async/await for I/O operations
    public async Task SaveStudentAsync(Student student)
    {
        await _repository.UpdateAsync(student);
    }
}
```

### Project Structure Guidelines

- **API Layer**: Controllers, middleware, filters, program configuration
- **Core Layer**: Domain entities, business logic, interfaces, domain events
- **Infrastructure Layer**: Data access, external services, message bus
- **Contracts Layer**: DTOs, requests, responses (can be shared)

### Dependency Injection

Register services in `Program.cs`:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

// Business Services
builder.Services.AddScoped<IStudentService, StudentService>();

// Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:Configuration");
});

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Testing Guidelines

### Unit Tests

```csharp
public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _mockRepository;
    private readonly StudentService _service;
    
    public StudentServiceTests()
    {
        _mockRepository = new Mock<IStudentRepository>();
        _service = new StudentService(_mockRepository.Object);
    }
    
    [Fact]
    public async Task GetStudent_WithValidId_ReturnsStudent()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedStudent = new Student
        {
            Id = studentId,
            FirstName = "John",
            LastName = "Doe"
        };
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(studentId))
            .ReturnsAsync(expectedStudent);
        
        // Act
        var result = await _service.GetStudentAsync(studentId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStudent.Id, result.Id);
        Assert.Equal(expectedStudent.FirstName, result.FirstName);
    }
}
```

### Integration Tests

```csharp
public class StudentsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public StudentsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetStudent_ReturnsSuccessStatusCode()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        
        // Act
        var response = await _client.GetAsync($"/api/v1/students/{studentId}");
        
        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

## API Design Guidelines

### REST Conventions

- Use nouns for resources: `/students`, `/assessments`
- Use HTTP verbs: GET, POST, PUT, DELETE, PATCH
- Version APIs: `/api/v1/students`
- Use plural nouns: `/students` not `/student`
- Use hierarchical structure: `/students/{id}/assessments`

### Response Codes

- `200 OK` - Successful GET, PUT, PATCH
- `201 Created` - Successful POST
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not authorized
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

### Pagination

```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<StudentDto>>> GetStudents(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var students = await _repository.GetPagedAsync(page, pageSize);
    
    return Ok(new PagedResult<StudentDto>
    {
        Items = students.Items.Select(s => s.ToDto()),
        TotalCount = students.TotalCount,
        Page = page,
        PageSize = pageSize
    });
}
```

## Data Access Patterns

### Repository Pattern

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
```

### Unit of Work Pattern

```csharp
public interface IUnitOfWork : IDisposable
{
    IStudentRepository Students { get; }
    IAssessmentRepository Assessments { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

## Event-Driven Communication

### Publishing Events

```csharp
public class StudentService
{
    private readonly IEventPublisher _eventPublisher;
    
    public async Task CreateStudentAsync(Student student)
    {
        await _repository.AddAsync(student);
        
        await _eventPublisher.PublishAsync(new StudentCreatedEvent
        {
            StudentId = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Timestamp = DateTime.UtcNow
        });
    }
}
```

### Consuming Events

```csharp
public class StudentCreatedEventConsumer : IConsumer<StudentCreatedEvent>
{
    public async Task Consume(ConsumeContext<StudentCreatedEvent> context)
    {
        var student = context.Message;
        
        // Process the event
        await ProcessStudentCreatedAsync(student);
    }
}
```

## Logging and Monitoring

### Structured Logging

```csharp
_logger.LogInformation(
    "Student {StudentId} was created by {UserId} at {Timestamp}",
    student.Id,
    userId,
    DateTime.UtcNow);

_logger.LogError(
    exception,
    "Failed to create student {StudentId}",
    student.Id);
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRedis(builder.Configuration.GetValue<string>("Redis:Configuration"))
    .AddRabbitMQ();

app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");
```

## Security Best Practices

### Authentication

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
        };
    });
```

### Authorization

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteStudent(Guid id)
{
    await _service.DeleteStudentAsync(id);
    return NoContent();
}
```

## Troubleshooting

### Common Issues

**Connection String Issues:**
- Verify SQL Server is running
- Check user secrets configuration
- Validate connection string format

**Message Bus Issues:**
- Ensure RabbitMQ container is running
- Check credentials
- Verify queue/exchange configuration

**Authentication Issues:**
- Validate JWT secret is consistent
- Check token expiration
- Verify issuer and audience settings

### Debugging Tips

1. Enable detailed logging in `appsettings.Development.json`
2. Use Swagger for API testing
3. Check health endpoints
4. Review container logs: `docker logs <container-name>`
5. Use Application Insights locally

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Maintained By**: Development Team
