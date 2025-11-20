# Quickstart

## Prerequisites

- .NET 9 SDK installed (see `global.json`).
- Node.js/Playwright dependencies already provisioned by repository bootstrap scripts.
- Aspire resources (PostgreSQL, Redis) launched via AppHost.
- Access to Entra ID dev tenant and seeded district/system admin accounts.

## Run the Stack

1. `dotnet run --project src/NorthStarET.NextGen.Lms.AppHost` to start Aspire orchestrated services.
2. Open the Aspire dashboard (typically http://localhost:15000) to verify PostgreSQL, Redis, API, and Web resources are healthy.
3. Browse to https://localhost:7002 to confirm the Web front end loads with session authentication.

## Execute Tests (Red → Green)

1. Capture Red state before implementation:
   - `dotnet test --configuration Debug --verbosity normal > specs/004-manage-schools-grades/checklists/phase-1-evidence/dotnet-test-red.txt`
   - `pwsh tests/ui/playwright.ps1 > specs/004-manage-schools-grades/checklists/phase-1-evidence/playwright-red.txt`
2. After implementing changes, rerun the suites and capture Green state using the same commands with `*-green.txt` filenames.
3. Include Aspire orchestration tests (`dotnet test tests/aspire/...`) in the same loop when district services are touched.

## Development Workflow

1. Follow TDD: create failing unit/BDD tests in `tests/unit/...Schools/` and `specs/004-manage-schools-grades/features/` before production code.
2. Use Application handlers (MediatR) for school catalog commands/queries; expose API endpoints via Minimal APIs or controllers under `NorthStarET.NextGen.Lms.Api/Controllers/Districts`.
3. Persist changes through Infrastructure repositories implementing tenant-aware EF Core contexts with idempotency envelopes.
4. Emit `SchoolChangeEvent` domain events and wire Infrastructure publishers to existing messaging pipeline.
5. Update Razor pages in `NorthStarET.NextGen.Lms.Web/Pages/Districts/Schools/` when Figma assets become available; until then, flag UI tasks as blocked.

## Verification & Review

1. Run `dotnet build --configuration Debug --verbosity normal /warnaserror` before committing.
2. Ensure coverage remains >=80% (use `dotnet test --collect:"XPlat Code Coverage"`).
3. Commit with descriptive message, pull latest feature branch, and push to `004review-Phase[n]` branch.
4. Attach Red/Green transcripts and any Aspire dashboard evidence to phase review artifacts in `specs/004-manage-schools-grades/checklists/`.
