# NorthStarET Mono-Repo Constitution

<!--
Sync Impact Report
Version change: TEMPLATE → 2.0.0
Modified principles:
- All principles imported from WIPNorthStar v1.7.0 with mono-repo context
Added sections:
- Principle 6: Mono-Repo Layer Isolation (new architectural constraint)
- Mono-Repo Organization section defining layer boundaries
Removed sections: None
Templates requiring updates:
- ⚠ .specify/templates/plan-template.md (requires mono-repo layer guidance)
- ⚠ .specify/templates/spec-template.md (requires layer identification)
- ⚠ .specify/templates/tasks-template.md (requires layer dependency validation)
Follow-up TODOs: Update all template files with layer isolation guidance
Validation Notes:
- Constitution v2.0.0 complete with no placeholder tokens
- Version bump: MAJOR (breaking architectural constraint - layer isolation principle)
- All principles from WIPNorthStar v1.7.0 preserved
- New Principle 6 enforces mono-repo layer boundaries
- Ratification date: 2025-10-11 (original WIPNorthStar constitution)
- Last amended: 2025-11-20 (mono-repo layer isolation added)
-->

## Core Principles

### 1. Clean Architecture & Aspire Orchestration

- Enforce Clean Architecture boundaries (UI → Application → Domain → Infrastructure) with dependencies pointing inward; UI never talks directly to Infrastructure.
- Orchestrate every service through .NET Aspire hosting and client packages, applying Aspire Service Defaults across the solution.
- Build integration suites as Aspire test projects that validate hosting, configuration, and cross-service flows.
  Rationale: Protects modularity, enforces consistent orchestration, and prevents layer leakage.

### 2. Test-Driven Quality Gates

- Begin every task by running `dotnet test` across impacted unit/application/domain projects to document the failing (Red) state, implement the change, then rerun the same command to confirm the passing (Green) state before committing.
- Author a Reqnroll Feature file for each functional requirement before implementation, commit step definition files before production code, and execute the Reqnroll test project via `dotnet test` in a Red → Green loop with transcripts attached to plans/reviews.
- Capture user journeys as Figma-backed Given/When/Then narratives and execute the corresponding Playwright scripts (`pwsh tests/ui/playwright.ps1`) Red → Green, retaining failure and success logs alongside Aspire integration `dotnet test` runs.
- At each delivery phase boundary, run `dotnet build` plus the full automated suite (unit, integration, BDD, Playwright) and maintain ≥ 80% coverage.
  Rationale: Ensures executable specifications stay aligned with behavior and keeps regressions out of main.

### 3. UX Traceability & Figma Accountability

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

### 4. Event-Driven Data Discipline

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

### 5. Security & Compliance Safeguards

- Enforce least privilege and role-based authorization within the Application layer; Domain and Infrastructure expose minimal contracts.
- Prohibit UI → Infrastructure coupling; all UI interactions flow through Application boundaries.
- Keep secrets solely in the platform secret store, encrypted in transit and at rest; ban secrets from code, configuration files, and logs.
  Rationale: Maintains layered defenses and satisfies compliance expectations.

### 6. Mono-Repo Layer Isolation (NON-NEGOTIABLE)

- The mono-repo contains distinct **layers** (e.g., Foundation, DigitalInk, future extensions) that maintain clear separation boundaries.
- **Foundation Layer** contains the core OldNorthStar-to-.NET-10 migration implementation with 20 feature specifications (001-020) and 11 microservices.
- Future layers MAY be built on top of Foundation but MUST NOT create direct service-to-service dependencies across layers.
- **Shared Infrastructure Only**: Cross-layer dependencies are restricted to approved shared infrastructure:
  - Identity & Authentication (Microsoft Entra ID with custom session authentication)
  - Configuration Service (district/school settings)
  - ServiceDefaults (Aspire orchestration patterns)
  - Domain Primitives (shared value objects, base entities)
- **Layer-Specific Documentation**: Each layer maintains its own `specs/` directory; general architecture and standards reside at repository root in `docs/architecture/` and `docs/standards/`.
- **No Circular Dependencies**: Higher layers may consume Foundation shared infrastructure, but Foundation MUST NOT depend on higher layers.
- **Layer Identification Required**: All specifications, plans, and tasks MUST explicitly identify their target layer.

  Rationale: Enforces modularity at the repository level, prevents tight coupling between distinct business domains, enables independent deployment and scaling of layers, and maintains clear ownership boundaries. Foundation layer remains stable and reusable as new capabilities are added.

### 7. Tool-Assisted Development Workflow

- AI agents MUST use structured thinking tools (`#think` or `#mcp_sequentialthi_sequentialthinking`) at the start of every interaction to plan approach and validate assumptions.
- When working with .NET, Azure, or Microsoft technologies, agents MUST query official documentation (`#microsoft.docs.mcp`) to ensure implementation aligns with current best practices and API contracts.
- UI development MUST follow a tool-assisted pipeline: extract design context via `#figma/dev-mode-mcp-server`, implement based on specifications, test with `#mcp_playwright_browser_*` automation, and debug with `#chromedevtools/chrome-devtools-mcp` live inspection.
- Research-first pattern: search documentation before implementation, retrieve code samples with language filters, fetch complete guides for complex scenarios.
  Rationale: Ensures AI-assisted development maintains consistency with project patterns, reduces hallucination risk through official documentation grounding, and enforces design-driven UI fidelity through systematic tool chains.

## Mono-Repo Organization

### Layer Structure
- **Foundation Layer**: `Src/Foundation/`, `Plan/Foundation/`, specifications in `Plan/Foundation/specs/Foundation/`
- **DigitalInk Layer**: Specifications in `Plan/Foundation/specs/DigitalInk/`, future implementation in `Src/DigitalInk/`
- **Future Layers**: Follow pattern `Src/{LayerName}/`, `Plan/{LayerName}/`, `specs/{LayerName}/`

### Shared Infrastructure Location
- **ServiceDefaults**: `Src/Foundation/shared/ServiceDefaults/` (Aspire defaults, logging, telemetry)
- **Domain Primitives**: `Src/Foundation/shared/Domain/` (common value objects, base entities, domain events)
- **Application Contracts**: `Src/Foundation/shared/Application/` (shared DTOs, common interfaces)
- **Infrastructure Utilities**: `Src/Foundation/shared/Infrastructure/` (database helpers, caching, messaging abstractions)

### Documentation Organization
- **Migration Plans**: `Plan/Foundation/Plans/` (migration-specific roadmaps, scenarios, data migration specs)
- **Architecture**: `docs/architecture/` (bounded contexts, service architectures - layer-agnostic)
- **Standards**: `docs/standards/` (API contracts, testing strategy, gateway configuration - reusable across layers)
- **Layer Specifications**: `Plan/Foundation/specs/{LayerName}/` (feature specs, plans, tasks, contracts per layer)

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
- create direct service-to-service dependencies across mono-repo layers (use shared infrastructure only).
- implement features without explicit layer identification in specifications, plans, and tasks.
- place layer-specific architecture docs in repository root (use `docs/` for cross-layer standards only).

## Delivery Workflow Requirements

- Main branch is the single source of truth; CI enforces constitution gates before merge.
- After completing every task, developers commit, pull, and push to keep shared history consistent. All pushes MUST target phase review branches using the refspec pattern `git push origin HEAD:[feature-number]review-Phase[phase-number]` (e.g., `git push origin HEAD:003review-Phase1` when finishing Phase 1 of feature 003); direct upstream pushes to main/develop are prohibited. Open pull requests from phase review branches into the integration or feature branches as governed.
- Each test phase MUST capture terminal output from `dotnet test` (unit, Reqnroll, Aspire) and Playwright scripts showing the Red state before implementation and the Green state after implementation; attach evidence to the relevant phase or review artifacts.
- Changes that weaken SLOs, security posture, architectural separation, multi-tenant isolation, or layer boundaries require an Architecture Review approval prior to merge.
- `/specify`, `/plan`, and `/tasks` outputs MUST map each requirement, journey, and task back to this constitution, including layer identification, Figma links (for new UI features), Reqnroll Feature identifiers, test gates (TDD/BDD/Playwright), Aspire touchpoints, multi-tenancy validation, and any `figma-prompts/` collateral for stories awaiting design assets.

## Governance

- **Amendments**: Require documented impact analysis, Architecture Review sign-off, and synchronized template updates before merge. Backward-incompatible principle changes trigger a MAJOR version bump; new or materially expanded guidance triggers MINOR; clarifications are PATCH.
- **Versioning Policy**: Follow semantic versioning (`MAJOR.MINOR.PATCH`) and record rationale within the Sync Impact Report.
- **Compliance Reviews**: Feature plans, specs, and task lists must pass constitution checks for layer identification, Figma evidence, testing gates, Aspire orchestration, and security controls; CI blocks merges without proof.
- **Record Keeping**: Preserve historical constitution revisions with rationale for deprecation; never delete prior guidance.
- **Layer Governance**: New layer proposals require Architecture Review documenting: business justification, dependency contracts with Foundation shared infrastructure, independent deployment strategy, and specification organization plan.

**Version**: 2.0.0 | **Ratified**: 2025-10-11 | **Last Amended**: 2025-11-20
