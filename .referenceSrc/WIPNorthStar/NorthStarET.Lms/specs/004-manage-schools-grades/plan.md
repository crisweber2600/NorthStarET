# Implementation Plan: Manage Schools & Grades

**Branch**: `004-manage-schools-grades` | **Date**: 2025-10-28 | **Spec**: `specs/004-manage-schools-grades/spec.md`
**Input**: Feature specification from `specs/004-manage-schools-grades/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Enable district-scoped management of Schools and Grade Offerings allowing district admins (and system admins when explicitly switched) to create, update, and delete schools and to configure which grades each school serves. Implementation will follow the repository's Clean Architecture (.NET 9 / C# 12) pattern: Application-layer commands/queries (MediatR) will orchestrate domain invariants, Infrastructure will persist via EF Core 9 (PostgreSQL), API exposes tenant-aware endpoints, and the Web project (Razor Pages) implements the UI. Idempotency windows (10 minutes) and tenant isolation are enforced in the Application/Infrastructure layers. Tests: xUnit unit tests, Reqnroll BDD features, Aspire integration tests, and Playwright UI journeys (Red→Green evidence required per constitution).

## Technical Context

**Language/Version**: C# 12 / .NET 9 (consistent with repository global.json and project SDKs)
**Primary Dependencies**: MediatR (request/command orchestration), FluentValidation (input validation), Entity Framework Core 9 (Postgres provider), .NET Aspire orchestration packages, Redis Stack (caching/idempotency windows), Microsoft.Identity.Web / custom SessionAuthentication (Entra ID) as already used in the solution.
**Storage**: PostgreSQL 16 via EF Core 9 with DbContext in Infrastructure (Aspire-managed connection strings)
**Testing**: xUnit for unit tests, Reqnroll for BDD feature files, Aspire test projects for orchestration/integration, Playwright for UI journeys (pwsh tests/ui/playwright.ps1)
**Target Platform**: Linux containers/servers orchestrated by Aspire (local dev via dotnet run --project src/NorthStarET.NextGen.Lms.AppHost)
**Project Type**: Web application (API + Razor Web frontend). This feature is implemented across Application, Infrastructure, Api and Web projects in the existing solution.
**Performance Goals**: UI eventual consistency reflecting confirmed changes within 2 seconds; idempotency duration 10 minutes for retries; maintain ≥80% test coverage for the feature.
**Constraints**: Strict tenant isolation per constitution; no UI work without Figma (this feature currently has "Skipped — No Figma" for flows); all changes must pass Red→Green test evidence before merging; no synchronous cross-service calls without documented latency budgets.
**Scale/Scope**: District-level management; expected usage sizes consistent with other district catalog features in the solution (tens to low hundreds of active admin users per district, multi-tenant at scale across many districts).

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

**Red→Green Evidence Requirements**:

- We will capture Red state and Green state transcripts for the following automated suites:
  - Unit tests: `dotnet test tests/unit/NorthStarET.NextGen.Lms.Application.Tests` (and related unit projects). Capture output to `specs/004-manage-schools-grades/checklists/phase-1-evidence/dotnet-test-red.txt` and `dotnet-test-green.txt`.
  - BDD (Reqnroll): `dotnet test tests/bdd/...` executing `features/manage-schools-grades/*.feature`. Capture transcripts to `specs/004-manage-schools-grades/checklists/phase-1-evidence/bdd-red.txt` and `bdd-green.txt`.
  - Aspire integration: `dotnet test tests/aspire/...` capturing orchestration results to `specs/004-manage-schools-grades/checklists/phase-1-evidence/aspire-red.txt` and `aspire-green.txt`.
  - Playwright UI: `pwsh tests/ui/playwright.ps1` with outputs saved to `specs/004-manage-schools-grades/checklists/phase-1-evidence/playwright-red.txt` and `playwright-green.txt`.

- All transcripts and artifacts will be attached to the phase review branch for audit and constitution compliance.

**Tool Usage Expectations** (for AI agents):

- Follow the project's Tool-Assisted Development Workflow in `.specify/memory/constitution.md`:
  - Begin each planning/implementation session with structured thinking (#think equivalent).
  - Use `#microsoft.docs.mcp` (and related Microsoft MCP tools) when making .NET/Azure technology decisions to ground them in official documentation.
  - For UI work (once Figma assets exist), extract specs via `#figma/dev-mode-mcp-server`, validate flows with `#mcp_playwright_browser_*`, and debug via `#chromedevtools/chrome-devtools-mcp`.

**Figma Traceability**:

- Current state: UI flows are labeled "Skipped — No Figma" in the spec.
- Actions:
  - Maintain "Skipped — No Figma" labels until design assets are delivered.
  - Populate `specs/004-manage-schools-grades/figma-prompts/` with prompts so design can supply frames.
  - Defer final Razor UI implementation until Figma references are available.

**Phase Review Branch Strategy**:

- Feature branch (current): `004-manage-schools-grades`. Phase review branches will follow the pattern `004review-PhaseN` when publishing phase artifacts; never push directly to `main` or `master`.

## Complexity Tracking

No constitution violations required for this plan.

## Project Structure

### Documentation (this feature)

```
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
src/
├── NorthStarET.NextGen.Lms.Domain/
├── NorthStarET.NextGen.Lms.Application/
├── NorthStarET.NextGen.Lms.Infrastructure/
├── NorthStarET.NextGen.Lms.Api/
└── NorthStarET.NextGen.Lms.Web/

tests/
├── unit/
├── bdd/
├── aspire/
├── integration/
└── ui/
```

**Structure Decision**: Use the existing Clean Architecture multi-project layout in the repository. Implement feature artifacts across these projects (no new top-level projects):

- `src/NorthStarET.NextGen.Lms.Application` — Application services, MediatR commands/handlers, validators, DTOs.
- `src/NorthStarET.NextGen.Lms.Domain` — Domain entities (School, GradeOffering), domain events, interfaces.
- `src/NorthStarET.NextGen.Lms.Infrastructure` — EF Core DbContext, repository implementations, idempotency via Redis, event publishing hooks.
- `src/NorthStarET.NextGen.Lms.Api` — Tenant-aware Minimal API/Controller endpoints wiring HTTP contracts to Application commands/queries.
- `src/NorthStarET.NextGen.Lms.Web` — Razor Pages and shared components for Schools & Grades UI (implementation gated on Figma assets).

Tests will be added/updated under `tests/unit`, `tests/bdd`, `tests/aspire`, and `tests/ui` following existing patterns.
