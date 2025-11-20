# Implementation Plan: [FEATURE]

**Specification Branch**: `[LayerName]/[###-feature-name-spec]` *(current branch - planning artifacts)*  
**Implementation Branch**: `[LayerName]/[###-feature-name]` *(created after approval)*  
**Date**: [DATE] | **Spec**: [link to spec.md]

**Note**: This template is filled in by the `/speckit.plan` command. See `.github/agents/speckit.plan.agent.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]  
**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]  
**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]  
**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]  
**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]
**Project Type**: [single/web/mobile - determines source structure]  
**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]  
**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]  
**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]

### Identity & Authentication Guidance

*If this feature requires authentication/authorization:*

- **Identity Provider**: Microsoft Entra ID (Azure AD) - Do NOT use Duende IdentityServer or custom token issuance
- **Authentication Pattern**: Session-based with custom SessionAuthenticationHandler
- **Token Validation**: Microsoft.Identity.Web for JWT validation (tokens issued by Entra ID)
- **Session Storage**: PostgreSQL (identity.sessions table) + Redis caching
- **Architecture Reference**: See `Plan/Foundation/Plans/docs/legacy-identityserver-migration.md` for BFF token exchange pattern
- **Key Dependencies**: Microsoft.Identity.Web (3.x), StackExchange.Redis (2.x), Aspire.Hosting.Redis

## Layer Identification (MANDATORY)

*REQUIRED: Declare this feature's position in the mono-repo architecture. Must match layer declared in spec.md.*

**Target Layer**: [e.g., Foundation, DigitalInk, or future layer name - MUST match spec.md]  
**Implementation Path**: [e.g., `Src/Foundation/services/[ServiceName]` or `Src/DigitalInk/modules/[ModuleName]`]  
**Specification Path**: [e.g., `Plan/Foundation/specs/[###-feature-name]/` - where this plan.md resides]

### Layer Consistency Validation

*Verify layer alignment between spec and plan*

- [ ] Target Layer matches specification (spec.md Layer Identification section)
- [ ] Implementation path follows layer structure (`Src/{TargetLayer}/...`)
- [ ] Specification path follows layer structure (`Plan/{TargetLayer}/specs/...`)
- [ ] If new layer: Architecture Review completed and documented in `Plan/{LayerName}/README.md`

### Shared Infrastructure Dependencies

*List shared components this feature depends on (from `Src/Foundation/shared/`)*

- [ ] **ServiceDefaults** - Aspire orchestration, OpenTelemetry, health checks
- [ ] **Domain** - Domain primitives (EntityBase, ValueObject, DomainEvent)
- [ ] **Application** - CQRS abstractions (ICommand, IQuery, Result pattern)
- [ ] **Infrastructure** - Multi-tenancy, caching, messaging, Azure services

### Cross-Layer Dependencies

*CAUTION: Cross-layer dependencies require justification and constitutional approval*

**Depends on layers**: [e.g., None (self-contained) or Foundation shared infrastructure only]  
**Specific Dependencies**: [If Foundation dependency, list specific shared components: ServiceDefaults, Domain, Application, Infrastructure]  
**Justification**: [Explain WHY this cross-layer dependency is necessary]  
**Constitutional Compliance**: See Principle 6 (Mono-Repo Layer Isolation) - layers must remain independently deployable. Direct service-to-service dependencies across layers are prohibited.

### Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

[Gates determined based on constitution file]

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
