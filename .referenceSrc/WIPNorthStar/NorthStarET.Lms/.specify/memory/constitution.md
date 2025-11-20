# NorthStarET NextGen LMS Constitution

<!--
Sync Impact Report
Version change: 1.6.0 → 1.7.0
Modified principles:
- UX Traceability & Figma Accountability → Refined to clarify UI migration vs. new feature development workflows
- Event-Driven Data Discipline → Added multi-tenancy architecture guidance
Added sections:
- Multi-Tenancy & Data Isolation subsection under Event-Driven Data Discipline
- UI Preservation Strategy clarification under UX Traceability
Removed sections: None
Templates requiring updates:
- ✅ .specify/templates/plan-template.md (updated Constitution Check section for multi-tenancy and UI migration context)
- ✅ .specify/templates/spec-template.md (no changes required - remains technology-agnostic)
- ✅ .specify/templates/tasks-template.md (updated testing notes for UI migration context)
Follow-up TODOs: None - all placeholders resolved
Validation Notes:
- Constitution v1.7.0 complete with no placeholder tokens
- Principles now aligned with Master Migration Plan v3.0 architectural decisions
- Multi-tenancy architecture (database-per-service with tenant_id isolation) formally recognized
- UI workflow clarified: migration preserves existing layouts (no Figma), new features require Figma
- All dates in ISO format (YYYY-MM-DD)
- Version bump: MINOR (material expansion of existing principles with new guidance)
-->

## Core Principles

### Clean Architecture & Aspire Orchestration

- Enforce Clean Architecture boundaries (UI → Application → Domain → Infrastructure) with dependencies pointing inward; UI never talks directly to Infrastructure.
- Orchestrate every service through .NET Aspire hosting and client packages, applying Aspire Service Defaults across the solution.
- Build integration suites as Aspire test projects that validate hosting, configuration, and cross-service flows.
  Rationale: Protects modularity, enforces consistent orchestration, and prevents layer leakage.

### Test-Driven Quality Gates

- Begin every task by running `dotnet test` across impacted unit/application/domain projects to document the failing (Red) state, implement the change, then rerun the same command to confirm the passing (Green) state before committing.
- Author a Reqnroll Feature file for each functional requirement before implementation, commit step definition files before production code, and execute the Reqnroll test project via `dotnet test` in a Red → Green loop with transcripts attached to plans/reviews.
- Capture user journeys as Figma-backed Given/When/Then narratives and execute the corresponding Playwright scripts (`pwsh tests/ui/playwright.ps1`) Red → Green, retaining failure and success logs alongside Aspire integration `dotnet test` runs.
- At each delivery phase boundary, run `dotnet build` plus the full automated suite (unit, integration, BDD, Playwright) and maintain ≥ 80% coverage.
  Rationale: Ensures executable specifications stay aligned with behavior and keeps regressions out of main.

### UX Traceability & Figma Accountability


**For New Features & UI Enhancements**:
- Tag all UI tasks with `UI` and include the exact Figma frame or flow link before starting work.
- When a Figma artifact is unavailable, label the story "Skipped — No Figma", pause implementation, and surface the gap in plans/tasks instead of attempting UI delivery.
- For every UI story, maintain `specs/<feature>/figma-prompts/` prompts using `#figma/dev-mode-mcp-server` so contributors without the MCP plugin can gather required design details once assets arrive.
- Link every user journey to its Figma flow (when available), owning Reqnroll Feature, and automated tests (TDD, BDD, Playwright) for full traceability.

**For UI Migration & Modernization**:
- When migrating existing UI (e.g., OldNorthStar AngularJS to modern framework), Figma designs are NOT required.
- Preserve existing UI layouts, workflows, and visual design during technology modernization.
- Document original UI behavior and maintain functional parity through Playwright tests.
- Focus on technology stack upgrade while maintaining user experience continuity.
- New features added during or after migration MUST follow the Figma-first workflow above.

  Rationale: Locks design fidelity into the workflow for new development while enabling practical UI modernization that preserves existing user experiences. Ensures work halts safely when designs are missing for genuinely new features, and enables asynchronous collaboration once assets exist.

### Event-Driven Data Discipline

- Prefer asynchronous event-driven integration; exceptions that require synchronous calls MUST document latency budgets and fallback strategies.
- Make all commands idempotent so retries and partial failures are safe.
- Version every event schema and provide deprecation windows for breaking changes.

**Multi-Tenancy & Data Isolation**:
- Follow database-per-service pattern with strict multi-tenant isolation within each service database.
- Every table MUST include `tenant_id` (district identifier) as part of composite keys for tenant-scoped data.
- Enforce Row-Level Security (RLS) policies in PostgreSQL to guarantee tenant boundaries at the database level.
- Application layer MUST set tenant context via claims/session for automatic query filtering.
- Connection pooling uses single pool per service database with tenant context in session variables.
- LMS service owns base database layers, schema evolution, and cross-tenant administrative features.

  Rationale: Sustains resilience, predictability, and compatibility across evolving services while enforcing strict data sovereignty and tenant isolation. Database-per-service with multi-tenancy consolidates infrastructure (11 databases vs. hundreds) while maintaining security and compliance through multiple isolation layers.

### Security & Compliance Safeguards

- Enforce least privilege and role-based authorization within the Application layer; Domain and Infrastructure expose minimal contracts.
- Prohibit UI → Infrastructure coupling; all UI interactions flow through Application boundaries.
- Keep secrets solely in the platform secret store, encrypted in transit and at rest; ban secrets from code, configuration files, and logs.
  Rationale: Maintains layered defenses and satisfies compliance expectations.

### Tool-Assisted Development Workflow

- AI agents MUST use structured thinking tools (`#think` or `#mcp_sequentialthi_sequentialthinking`) at the start of every interaction to plan approach and validate assumptions.
- When working with .NET, Azure, or Microsoft technologies, agents MUST query official documentation (`#microsoft.docs.mcp`) to ensure implementation aligns with current best practices and API contracts.
- UI development MUST follow a tool-assisted pipeline: extract design context via `#figma/dev-mode-mcp-server`, implement based on specifications, test with `#mcp_playwright_browser_*` automation, and debug with `#chromedevtools/chrome-devtools-mcp` live inspection.
- Research-first pattern: search documentation before implementation, retrieve code samples with language filters, fetch complete guides for complex scenarios.
  Rationale: Ensures AI-assisted development maintains consistency with project patterns, reduces hallucination risk through official documentation grounding, and enforces design-driven UI fidelity through systematic tool chains.

## Non-Negotiable Constraints

NEVER:


- ship NEW UI features without the referenced Figma link (UI migration work preserving existing layouts is exempt).
- implement UI stories labeled "Skipped — No Figma" until the design asset and supporting prompt resolve the gap (applies only to new features, not migration work).
- bypass Red → Green gates, skip recording the red-phase outputs for `dotnet test` (unit, Reqnroll, Aspire) or Playwright scripts, or merge while any suite is failing.
- couple UI directly to Infrastructure.
- store or log secrets outside approved secret stores.
- introduce synchronous cross-service calls without documented latency budgets and fallbacks.
- create service databases without tenant_id columns and Row-Level Security policies for multi-tenant data.
- query across tenant boundaries without explicit cross-tenant authorization documented in Architecture Review.

## Delivery Workflow Requirements

- Main branch is the single source of truth; CI enforces constitution gates before merge.
- After completing every task, developers commit, pull, and push to keep shared history consistent. All pushes MUST target phase review branches using the refspec pattern `git push origin HEAD:[feature-number]review-Phase[phase-number]` (e.g., `git push origin HEAD:003review-Phase1` when finishing Phase 1 of feature 003); direct upstream pushes to main/develop are prohibited. Open pull requests from phase review branches into the integration or feature branches as governed.
- Each test phase MUST capture terminal output from `dotnet test` (unit, Reqnroll, Aspire) and Playwright scripts showing the Red state before implementation and the Green state after implementation; attach evidence to the relevant phase or review artifacts.
- Changes that weaken SLOs, security posture, architectural separation, or multi-tenant isolation require an Architecture Review approval prior to merge.
- `/specify`, `/plan`, and `/tasks` outputs MUST map each requirement, journey, and task back to this constitution, including Figma links (for new UI features), Reqnroll Feature identifiers, test gates (TDD/BDD/Playwright), Aspire touchpoints, multi-tenancy validation, and any `figma-prompts/` collateral for stories awaiting design assets.

## Governance

- **Amendments**: Require documented impact analysis, Architecture Review sign-off, and synchronized template updates before merge. Backward-incompatible principle changes trigger a MAJOR version bump; new or materially expanded guidance triggers MINOR; clarifications are PATCH.
- **Versioning Policy**: Follow semantic versioning (`MAJOR.MINOR.PATCH`) and record rationale within the Sync Impact Report.
- **Compliance Reviews**: Feature plans, specs, and task lists must pass constitution checks for Figma evidence, testing gates, Aspire orchestration, and security controls; CI blocks merges without proof.
- **Record Keeping**: Preserve historical constitution revisions with rationale for deprecation; never delete prior guidance.

**Version**: 1.7.0 | **Ratified**: 2025-10-11 | **Last Amended**: 2025-11-18
