# Repository Guidelines

## Project Structure & Module Organization

Solution artifacts live in `NorthStarET.NextGen.Lms.sln`. Production code follows Clean Architecture layers: `src/NorthStarET.NextGen.Lms.UI.*` (presentation), `src/NorthStarET.NextGen.Lms.Application` (application services), `src/NorthStarET.NextGen.Lms.Domain` (domain model), and `src/NorthStarET.NextGen.Lms.Infrastructure` (persistence + integrations). Aspire hosting and service defaults reside in `src/NorthStarET.NextGen.Lms.AppHost` and `src/NorthStarET.NextGen.Lms.ServiceDefaults`. Tests mirror the layers: `tests/unit` (xUnit), `tests/integration` (Aspire test projects), `tests/bdd` (Reqnroll), `tests/ui` (Playwright), and `tests/aspire` for orchestration smoke tests. Feature collateral lives under `specs/<feature-id>/`.

## Architecture & Workflow Mandates

- Enforce Clean Architecture boundaries: UI depends on Application only; Application uses Domain; Infrastructure stays behind interfaces.
- Orchestrate every service through .NET Aspire hosting/client packages with Service Defaults applied.
- Integration tests must be Aspire projects validating hosting, configuration, and cross-service flows.
- Prefer asynchronous eventing; any necessary synchronous call documents latency budgets and fallbacks inside specs and PRs.
- User journeys require Figma-backed Given/When/Then scenarios and Playwright validation; missing Figma links label the task "Skipped — No Figma" and pause implementation until designs and supporting `figma-prompts/` collateral exist.
- Label every UI task as `UI` in plans/tasks and attach its Figma frame before work begins.
- After each phase (plan, implement, verify) run `dotnet build` and the full automated suite; main branch is the single source of truth with CI enforcing gates.
- At phase completion checkpoints, push to phase review branches using `git push origin HEAD:[feature-number]review-Phase[phase-number]` before opening PRs.

## Build, Test, and Development Commands

- `dotnet restore` — install all solution dependencies (run once per environment).
- `dotnet build NorthStarET.NextGen.Lms.sln` — compile every project with Aspire defaults applied.
- `dotnet test tests/unit/NorthStarET.NextGen.Lms.Application.Tests.csproj` (and other test projects) — execute layer-specific suites during Red → Green.
- `dotnet test --collect:"XPlat Code Coverage"` — verify ≥80 % coverage before merging.
- `pwsh tests/ui/playwright.ps1` — run Playwright journeys that validate Figma-backed flows end to end.
- `dotnet test tests/aspire/NorthStarET.NextGen.Lms.AspireTests.csproj` — ensure hosting and client packages stay wired correctly.

## Commit & Pull Request Guidelines

Commit after every completed task with imperative messages such as `feat: add district invite flow` or `test: harden aspire orchestration`. After completing every task, commit, pull, and push to maintain shared visibility. All pushes MUST target phase review branches using the pattern `git push origin HEAD:[feature-number]review-Phase[phase-number]` (e.g., `git push origin HEAD:003review-Phase1` when finishing Phase 1 of feature 003); direct pushes to main/develop are prohibited. Reference linked specs/tasks (`specs/001-feature-title-login/...`) and include Figma URLs for UI changes. Pull requests must show passing CI, attach screenshots or recordings for UI updates, list impacted tests (unit, Reqnroll, Playwright, Aspire), and note any synchronous calls along with latency budgets and fallbacks.

## Coding Style & Naming Conventions

Write C# 12 with four-space indentation and align namespaces with folder paths. Use PascalCase for classes, interfaces (`IInterface`), and public members; camelCase for locals and parameters. Keep UI projects Razor-first; application/domain projects stay class library only. Run `dotnet format` before opening a PR. Aspire assets must apply `ServiceDefaults` in `Program.cs`. Configuration files under `env/` never store secrets—rely on the platform secret store.

## Testing Guidelines

Adhere to TDD: create failing unit, integration, BDD, and Playwright tests before implementation. Reqnroll feature files sit in `specs/<feature-id>/features/`; commit step definition stubs (in `tests/bdd/...Steps.cs`) before writing production logic. Aspire integration and Playwright suites must run at each delivery checkpoint. Name test classes `<TypeUnderTest>Tests` and methods `Should_<behavior>`. Maintain ≥80 % line coverage and capture evidence in CI runs; CI will block merges on failing gates.

## Security & Configuration Tips

Never log or commit secrets; load them through the platform secret store and Aspire-managed configuration. UI projects must depend on Application services only—direct Infrastructure references are constitution violations. When introducing synchronous cross-service calls, document latency budgets and fallback paths inside the PR description and related spec.
