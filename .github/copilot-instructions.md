# NorthStarET.NextGen.Lms Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-10-25

## Architecture Overview

This is a .NET 9.0 Clean Architecture LMS with strict tenant isolation, orchestrated via .NET Aspire. The solution enforces vertical slice boundaries:

- **Domain** (`src/NorthStarET.NextGen.Lms.Domain`) — Pure business entities, repositories interfaces, domain events. No dependencies.
- **Application** (`src/NorthStarET.NextGen.Lms.Application`) — Use case orchestration via MediatR commands/queries, FluentValidation. Depends on Domain only.
- **Infrastructure** (`src/NorthStarET.NextGen.Lms.Infrastructure`) — EF Core 9 (PostgreSQL 16), Redis Stack, Entra ID integration. Implements Domain interfaces.
- **API** (`src/NorthStarET.NextGen.Lms.Api`) — ASP.NET Core Minimal APIs + Controllers. Custom session authentication. Depends on Application.
- **Web** (`src/NorthStarET.NextGen.Lms.Web`) — Razor Pages frontend. Depends on API via HTTP.
- **AppHost** (`src/NorthStarET.NextGen.Lms.AppHost`) — Aspire orchestration defining PostgreSQL, Redis, API, Web service topology.
- **ServiceDefaults** (`src/NorthStarET.NextGen.Lms.ServiceDefaults`) — Shared telemetry, health checks, resilience policies applied to all services.

**Key Pattern**: Use `DependencyInjection.cs` in Application/Infrastructure to register services; API's `Program.cs` calls `.AddApplication()` and `.AddInfrastructure(configuration)`.

## Constitution Compliance (Version 1.5.0)

**Non-Negotiables - NEVER**:

- Ship UI work without referenced Figma link
- Implement UI stories labeled "Skipped — No Figma" until design asset arrives
- Bypass Red→Green gates or skip recording red-phase outputs for `dotnet test` (unit, Reqnroll, Aspire) or Playwright scripts
- Merge while any test suite is failing
- Couple UI directly to Infrastructure
- Store or log secrets outside approved secret stores
- Introduce synchronous cross-service calls without documented latency budgets and fallbacks

**Mandatory Workflows**:

- Every functional requirement MUST have a Reqnroll feature file before implementation
- Every UI user journey MUST have Playwright test with Figma link
- Every phase MUST capture Red→Green evidence (4 transcript files minimum)
- Every task completion MUST commit, pull, push to phase review branch (NEVER directly to main)
- Every phase boundary MUST run full suite: unit, Reqnroll, Aspire, Playwright with ≥80% coverage

## Active Technologies & Patterns

- **C# 12 / .NET 9.0** — Record types for DTOs/commands, sealed classes, file-scoped namespaces
- **MediatR 11** — CQRS pattern: `sealed record FooCommand : IRequest<Result>`, handled by `FooCommandHandler : IRequestHandler<FooCommand, Result>`
- **FluentValidation** — Validators registered via `AddValidatorsFromAssembly()` in Application layer
- **Entity Framework Core 9** — PostgreSQL provider via Aspire; DbContext in Infrastructure with migrations
- **Redis Stack** — Session caching, idempotency windows (10-minute deduplication envelopes)
- **Entra ID (OIDC)** — Custom session authentication scheme wrapping Microsoft.Identity.Web; see `SessionAuthenticationHandler` and `TokenExchangeService`
- **Aspire Orchestration** — All services reference AppHost project; run via `dotnet run --project src/NorthStarET.NextGen.Lms.AppHost`
- **Centralized Package Management** — `Directory.Packages.props` with `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`

## Critical Workflows

### Running Locally

```bash
# Launch entire stack (PostgreSQL, Redis, API, Web) with Aspire dashboard
dotnet run --project src/NorthStarET.NextGen.Lms.AppHost

# Access Aspire dashboard at http://localhost:15000 (or URL shown in terminal)
# API runs at https://localhost:7001, Web at https://localhost:7002 (ports from launchSettings.json)
```

### Building & Testing

```bash
# Full solution build (warnings as errors)
dotnet build --configuration Debug --verbosity normal /warnaserror

# Run all tests (unit, integration, Aspire orchestration)
dotnet test --configuration Debug --verbosity normal

# Coverage (≥80% required for PR approval)
dotnet test --collect:"XPlat Code Coverage"

# Playwright UI tests (requires pwsh)
pwsh tests/ui/playwright.ps1
```

### Database Migrations

```bash
# Add migration from Infrastructure project
dotnet ef migrations add MigrationName --project src/NorthStarET.NextGen.Lms.Infrastructure --startup-project src/NorthStarET.NextGen.Lms.Api

# Apply migrations (Aspire auto-applies on startup in dev)
dotnet ef database update --project src/NorthStarET.NextGen.Lms.Infrastructure --startup-project src/NorthStarET.NextGen.Lms.Api
```

## Git Workflow & Quality Gates

**Branch Strategy**: Feature branches named `[###]-feature-name` (e.g., `002-bootstrap-tenant-access`). Phase review branches for checkpoints: `[###]review-Phase[N]`.

**Commit Discipline**:

- Commit after every task: `git add . && git commit -m "feat: add district soft-delete"`
- Pull before push: `git pull origin [branch-name]`
- Push to phase review branch: `git push origin HEAD:002review-Phase1`
- **Never push directly to main/master**

**Phase Gates** (mandatory before advancing):

```bash
# BEFORE implementation - capture Red state:
dotnet test --configuration Debug --verbosity normal > phase-red-dotnet-test.txt
pwsh tests/ui/playwright.ps1 > phase-red-playwright.txt

# Build and implement changes
dotnet build --configuration Debug --verbosity normal /warnaserror

# AFTER implementation - capture Green state:
dotnet test --configuration Debug --verbosity normal > phase-green-dotnet-test.txt
pwsh tests/ui/playwright.ps1 > phase-green-playwright.txt

# Attach all 4 transcript files to phase review artifacts before pushing
```

## Project-Specific Conventions

### Dependency Injection Pattern

```csharp
// Application/DependencyInjection.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddMediatR(Assembly.GetExecutingAssembly());
    services.AddScoped<IMyService, MyService>();
    return services;
}

// Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<MyDbContext>(/* Aspire-managed connection */);
    services.AddScoped<IMyRepository, MyRepository>();
    return services;
}
```

### Command/Query Pattern

```csharp
// Application layer
public sealed record CreateDistrictCommand(string Name, string Suffix) : IRequest<Guid>;

public sealed class CreateDistrictCommandHandler : IRequestHandler<CreateDistrictCommand, Guid>
{
    private readonly IDistrictRepository _repository;

    public async Task<Guid> Handle(CreateDistrictCommand request, CancellationToken cancellationToken)
    {
        // Domain logic here
    }
}
```

### Aspire Service Registration

```csharp
// AppHost/Program.cs
var postgres = builder.AddPostgres("MyPostgres").AddDatabase("MyDb");
var redis = builder.AddRedis("MyRedis");

builder.AddProject<Projects.MyApi>("myapi")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);
```

### Session Authentication (Custom)

- Entra ID tokens exchanged via `ITokenExchangeService`
- Sessions stored in `IdentityDbContext.Sessions` (PostgreSQL)
- Cached in Redis with sliding expiration
- Custom `SessionAuthenticationHandler` reads session from cache/DB
- Controllers use `[Authorize]` with `SessionAuthenticationDefaults.AuthenticationScheme`

### Tenant Isolation

- Every query/command MUST include tenant context (`DistrictId`)
- Repository methods filter by tenant: `dbContext.Districts.Where(d => d.Id == tenantId)`
- Authorization service validates user belongs to tenant before allowing operations

### Idempotency Windows

- 10-minute deduplication for district/admin create/update
- Redis key: `idempotency:{operation}:{entityId}:{payloadHash}`
- Infrastructure handles automatically via `IdempotencyService`

### Soft Delete Pattern

- Districts/Admins have `DeletedAt` timestamp (nullable)
- Queries filter: `.Where(x => x.DeletedAt == null)`
- Deletion emits `DistrictDeleted` event, archives admin assignments

## Testing Guidelines

- **TDD Mandatory**: Write failing test → implement → test passes (Red-Green-Refactor)
- **Red→Green Evidence Required**: EVERY phase MUST capture terminal output showing:
  - `dotnet test` (unit, Reqnroll BDD, Aspire) in Red state BEFORE implementation
  - `pwsh tests/ui/playwright.ps1` in Red state BEFORE implementation
  - Same commands in Green state AFTER implementation
  - Attach transcripts to phase review artifacts
- **Unit Tests**: `tests/unit/NorthStarET.NextGen.Lms.Application.Tests`, naming: `FooServiceTests.Should_DoSomething_When_Condition()`
- **BDD Tests (MANDATORY)**: Reqnroll feature files in `specs/[feature-id]/features/` for EVERY functional requirement; commit step definition stubs in `tests/bdd/` BEFORE production code
- **Integration Tests**: Aspire test projects validating full hosting stack
- **UI Tests (MANDATORY)**: Playwright in `tests/ui/` for EVERY user journey with Figma links; Red→Green evidence required before merge
- **Coverage**: Maintain ≥80% line coverage; CI blocks merges below threshold

## Tool Usage Patterns (MANDATORY)

### Every Chat Session

- **ALWAYS use #think or #mcp_sequentialthi_sequentialthinking** at the start of every interaction to plan your approach
- **ALWAYS use #microsoft.docs.mcp** when working with .NET, Azure, or Microsoft technologies to get official documentation and code samples

### UI Development Workflow

1. **Extract Design Context**: Use **#figma/dev-mode-mcp-server** to gather component specs, spacing, colors, and interaction patterns from Figma frames linked in `spec.md`
2. **Implement UI**: Build the Razor Pages or components based on Figma specifications
3. **Test UI**: Use **#mcp_playwright_browser_click** and related Playwright tools to automate user interaction testing:
   - `#mcp_playwright_browser_navigate` - Navigate to pages
   - `#mcp_playwright_browser_snapshot` - Capture accessibility snapshots
   - `#mcp_playwright_browser_click` - Click buttons/links
   - `#mcp_playwright_browser_fill_form` - Fill form fields
   - `#mcp_playwright_browser_take_screenshot` - Visual verification
4. **Debug UI**: Use **#chromedevtools/chrome-devtools-mcp** for live debugging:
   - `#mcp_chromedevtool_take_snapshot` - DOM inspection
   - `#mcp_chromedevtool_list_console_messages` - JavaScript errors
   - `#mcp_chromedevtool_list_network_requests` - API call inspection
   - `#mcp_chromedevtool_evaluate_script` - Runtime diagnostics

### Research & Documentation

- Before implementing .NET/Azure features: `#microsoft.docs.mcp` to search official docs
- For code samples: `#mcp_microsoft_doc_microsoft_code_sample_search` with language filter (csharp)
- For complete guides: `#mcp_microsoft_doc_microsoft_docs_fetch` for full documentation pages

### Example Tool Chain for UI Task

```
1. #think "Planning district creation modal implementation"
2. #figma/dev-mode-mcp-server get_design_context nodeId="22-67" (Create New District modal)
3. Implement Razor component based on Figma specs
4. #mcp_playwright_browser_navigate url="https://localhost:7002/districts"
5. #mcp_playwright_browser_click element="Create District button"
6. #mcp_playwright_browser_fill_form fields=[{name: "District Name", value: "Test"}]
7. #mcp_chromedevtool_list_console_messages to verify no errors
8. #mcp_playwright_browser_take_screenshot for visual validation
```

## Spec-Driven Development

Feature specs live in `specs/[###-feature-name]/`:

- `spec.md` — User stories (P1/P2/P3 priorities), acceptance criteria, Figma links
- `plan.md` — Generated by `/speckit.plan`, technical approach
- `tasks.md` — Generated by `/speckit.tasks`, phased task breakdown
- `research.md`, `data-model.md`, `contracts/` — Supporting artifacts
- `features/` — Reqnroll BDD feature files (MANDATORY for every functional requirement)
- `figma-prompts/` — Prompts using `#figma/dev-mode-mcp-server` for contributors without MCP plugin

**UI Tasks**: MUST have Figma frame URL before implementation. Label task "Skipped — No Figma" if missing and HALT work until design arrives.

**BDD Workflow**:

1. Write Reqnroll `.feature` file in `specs/[feature-id]/features/` for each functional requirement
2. Commit step definition stubs in `tests/bdd/...Steps.cs` BEFORE production code
3. Run `dotnet test` on BDD project → capture Red output
4. Implement production code
5. Run `dotnet test` on BDD project → capture Green output
6. Attach both transcripts to phase review

**UI Workflow**:

1. Extract Figma design context using `#figma/dev-mode-mcp-server`
2. Write Playwright test in `tests/ui/` for user journey
3. Run `pwsh tests/ui/playwright.ps1` → capture Red output
4. Implement UI components
5. Run `pwsh tests/ui/playwright.ps1` → capture Green output
6. Attach both transcripts to phase review

## Security & Configuration

- **No Secrets in Code**: Use Aspire-managed configuration or platform secret store
- **Rate Limiting**: 10 requests/min for invite/create endpoints (configured in API `Program.cs`)
- **CSRF Protection**: Anti-forgery tokens on state-changing operations
- **SQL Injection**: EF Core parameterized queries only
- **XSS**: Razor encodes by default; manual encoding for raw HTML
- **Audit Trail**: All tenant operations logged to `AuditRecords` table

## Debugging & Troubleshooting

### Aspire Dashboard

- Access at `http://localhost:15000` after running AppHost
- **Resources Tab**: Check PostgreSQL/Redis health status, view connection strings
- **Console Logs**: Real-time service logs filtered by resource
- **Traces**: Distributed tracing across API/Web/Infrastructure calls
- **Metrics**: Performance counters, request rates, error rates

### Session Authentication Issues

1. Check Redis cache: `#mcp_chromedevtool_list_network_requests` for `/api/auth/*` calls
2. Verify session in database: Query `IdentityDbContext.Sessions` table
3. Inspect claims: Add breakpoint in `SessionAuthenticationHandler.HandleAuthenticateAsync()`
4. Token refresh: Check `TokenRefreshService` logs for background refresh attempts

### Tenant Isolation Verification

- Every query MUST filter by `DistrictId` - use SQL logging to verify:
  ```csharp
  options.EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information);
  ```
- Test with multiple districts: Ensure no cross-tenant data leaks in UI or API responses

### Redis Cache Debugging

- Use Aspire dashboard to connect to Redis: View keys matching `session:*` or `idempotency:*`
- Check expiration: `TTL session:{sessionId}` should show sliding window
- Clear cache during dev: `FLUSHDB` in Redis CLI (Aspire provides connection details)

### Common Error Patterns

- **404 on API calls**: Check Aspire service discovery, ensure API is running and healthy
- **401 Unauthorized**: Session expired or not found in cache/DB; check `SessionAuthenticationHandler` logs
- **403 Forbidden**: Tenant authorization failed; verify `DistrictId` in claims matches requested resource
- **Idempotency conflicts**: Redis key collision; check payload hash generation in `IdempotencyService`

## Common Pitfalls

1. **Singleton vs Scoped**: Hosted services (singleton) cannot directly inject scoped services (e.g., `ISessionRepository`). Use `IServiceScopeFactory`:

   ```csharp
   using var scope = _scopeFactory.CreateScope();
   var repo = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
   ```

2. **Aspire Connection Strings**: Use Aspire resource names, not hardcoded strings:

   ```csharp
   var connectionString = configuration["PostgreSQL:ConnectionString"]; // Aspire injects this
   ```

3. **Clean Architecture Violations**: UI projects depend on Application ONLY. Infrastructure references forbidden in UI/API.

4. **Phase Review Branches**: Always push to `[###]review-Phase[N]`, never directly to main.

5. **Missing WaitFor**: Aspire services must `WaitFor` dependencies:

   ```csharp
   .AddProject<Projects.Api>("api").WaitFor(postgres).WaitFor(redis)
   ```

6. **UI Testing Without Figma**: Never implement UI tasks without Figma frame URL in spec. Use `#figma/dev-mode-mcp-server` to extract design tokens first.

## Recent Changes
- 004-manage-schools-grades: Added C# 12 / .NET 9 (consistent with repository global.json and project SDKs) + MediatR (request/command orchestration), FluentValidation (input validation), Entity Framework Core 9 (Postgres provider), .NET Aspire orchestration packages, Redis Stack (caching/idempotency windows), Microsoft.Identity.Web / custom SessionAuthentication (Entra ID) as already used in the solution.

- **002-bootstrap-tenant-access** (2025-10-25): Added district/admin CRUD, tenant isolation, soft-delete with archive, email retry with exponential backoff
- **001-unified-sso-auth** (2025-10-09): Entra ID OIDC integration, custom session authentication, token refresh background service

---

**Feedback Requested**: Are there specific integration patterns, debugging techniques, or Aspire-specific workflows that need more detail?


---

## GitHub Copilot Coding Agent

This repository is configured for **GitHub Copilot Coding Agent** to execute code on self-hosted ARC runners.

### Quick Reference

**Runner Label**: `arc-runner-set` (use this in workflows that need code execution)

**Available Environment**:
- ✅ .NET 10 SDK with Aspire workload
- ✅ Docker-in-Docker for container testing  
- ✅ Node.js LTS for MCP servers
- ✅ Full repository filesystem access

**MCP Servers** (use with # prefix in Copilot Chat):
- `#think` or `#sequential-thinking` - Plan and reason through complex problems
- `#microsoft.docs.mcp` - Search .NET and Azure documentation
- `#filesystem` - Navigate and modify repository files
- `#github` - Interact with GitHub API

### Using Coding Agent

**Execute Code**:
```
@copilot Run dotnet build and show me any warnings
@copilot Run all tests and generate coverage report
@copilot Start the Aspire AppHost and verify services are healthy
```

**Plan with Sequential Thinking**:
```
@copilot #think "How should I refactor the district repository to support soft deletes?"
```

**Research .NET APIs**:
```
@copilot #microsoft.docs.mcp
Find examples of using EF Core 9 with PostgreSQL in .NET Aspire applications
```

**Work with Aspire**:
```
@copilot Can you:
1. Build the Aspire AppHost project
2. Start the Aspire dashboard
3. Verify PostgreSQL and Redis containers are running
4. Show me the connection strings
```

### Configuration Files

See `.github/copilot/` for complete configuration:
- `environment.yml` - Runner environment specification
- `mcp-servers.json` - MCP server configurations
- `README.md` - Full documentation
- `../workflows/copilot-environment.yml` - Verification workflow

### Testing the Setup

Run the verification workflow:
```bash
gh workflow run copilot-environment.yml --ref main
```

Or through GitHub UI: **Actions** → **Enable Copilot Coding Agent** → **Run workflow**

---

**Configuration Location**: `.github/copilot/`  
**Runner Image**: `ghcr.io/crisweber2600/arc-runner-aspire:latest`  
**Last Updated**: November 12, 2025
