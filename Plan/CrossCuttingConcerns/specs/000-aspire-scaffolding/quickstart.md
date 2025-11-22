# Quickstart: Aspire Orchestration & Cross-Cutting Scaffolding

**Feature**: aspire-scaffolding
**Date**: 2025-11-20

## Prerequisites

- .NET 10 SDK (GA - November 2025)
- .NET Aspire 9.5 workload: `dotnet workload install aspire`
- Docker Desktop (or Podman)
- Visual Studio 2022 17.12+ / VS Code with C# DevKit

## Running the Full Stack

### 1. Navigate to AppHost

```bash
cd Src/Foundation/AppHost
```

### 2. Run with Aspire

```bash
dotnet run
```

### 3. Access Dashboard

- Open the URL displayed in the terminal (e.g., `http://localhost:15000`)
- View status of `postgres`, `redis`, `rabbitmq`, and all microservices
- Check resource health in the **Resources** tab
- View distributed traces in the **Traces** tab

## Scaffolding a New Service

### PowerShell (Windows)

```powershell
# Navigate to scripts directory
cd .specify/scripts/powershell

# Generate new service
.\new-service.ps1 -ServiceName "StudentManagement"
```

### Bash (Linux/macOS)

```bash
# Navigate to scripts directory
cd .specify/scripts/bash

# Generate new service
./new-service.sh StudentManagement
```

### What Gets Created

```text
Src/Foundation/services/StudentManagement/
├── StudentManagement.Api/
│   ├── Program.cs
│   ├── StudentManagement.Api.csproj
│   └── Controllers/
├── StudentManagement.Application/
│   ├── DependencyInjection.cs
│   └── StudentManagement.Application.csproj
├── StudentManagement.Domain/
│   └── StudentManagement.Domain.csproj
└── StudentManagement.Infrastructure/
    ├── DependencyInjection.cs
    ├── Persistence/
    │   └── StudentManagementDbContext.cs
    └── StudentManagement.Infrastructure.csproj

tests/StudentManagement.Tests/
├── Unit/
└── Integration/
```

### Manual Steps After Scaffolding

1. **Register in AppHost** (auto-added by script):
   ```csharp
   // Src/Foundation/AppHost/Program.cs
   var studentDb = postgres.AddDatabase("StudentManagementDb");
   
   builder.AddProject<Projects.StudentManagement_Api>("studentmanagement")
          .WithReference(studentDb)
          .WithReference(redis)
          .WithReference(rabbit)
          .WaitFor(studentDb);
   ```

2. **Implement Domain Logic**:
   - Add entities to `Domain/` project
   - Add commands/queries to `Application/` project
   - Add repositories to `Infrastructure/Persistence/` project

3. **Run Tests**:
   ```bash
   dotnet test tests/StudentManagement.Tests
   ```

## Verifying Cross-Cutting Concerns

### Tenant Isolation

```csharp
// Example query - automatically filtered by TenantId
var students = await _context.Students.ToListAsync();
// Returns only students for current tenant

// Opt-out example (creates audit log)
[IgnoreTenantFilter]
public async Task<List<Student>> GetAllStudentsAcrossTenantsAsync()
{
    return await _context.Students.ToListAsync();
    // Audit log automatically created with user context
}
```

### Idempotency

```csharp
// Send command with idempotency key
var command = new CreateStudentCommand 
{
    IdempotencyKey = "create-student-123",
    Name = "John Doe"
};

var result = await _mediator.Send(command);
// First call: 201 Created with new student ID

// Duplicate call within 10 minutes
var duplicate = await _mediator.Send(command);
// Returns: 202 Accepted with original student ID (not reprocessed)
```

### Observability

```bash
# View traces in Aspire Dashboard
# Navigate to http://localhost:15000/traces

# Filter by service
# View full request chain across API Gateway → Service → Database → Message Bus
```

## Troubleshooting

### Containers not starting

```bash
# Check Docker is running
docker ps

# View Aspire resource logs
# Go to Dashboard → Resources → Click resource → View Logs
```

### Connection Refused

```bash
# Check connection strings in Dashboard
# Resources → [Resource Name] → Details → Connection String
```

### Missing TenantId in Queries

```bash
# Ensure TenantInterceptor is registered in Infrastructure/DependencyInjection.cs
services.AddDbContext<MyDbContext>(options =>
{
    options.UseNpgsql(connectionString)
           .AddInterceptors(new TenantInterceptor(tenantContext));
});
```

### Idempotency Not Working

```bash
# Check Redis is healthy in Dashboard
# Verify IdempotencyService is registered
services.AddSingleton<IIdempotencyService, IdempotencyService>();
```

## Next Steps

1. **Implement Feature**: Add domain logic to generated service
2. **Add Tests**: Write unit tests, integration tests, BDD scenarios
3. **Run Full Suite**: `dotnet test && pwsh tests/ui/playwright.ps1`
4. **View Traces**: Monitor distributed traces in Aspire Dashboard
5. **Deploy**: Follow deployment guide for production environment
