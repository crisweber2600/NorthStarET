# Implementation Plan: UI Migration with Preservation Strategy

**Specification Branch**: `Foundation/003-ui-migration-preservation-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/003-ui-migration-preservation` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/003-ui-migration-preservation/spec.md

**Note**: Parity migration only; constitution’s Figma requirement is exempt for migrations that preserve existing UI.

## Summary

Migrate the legacy AngularJS LMS UI to Angular 18 while preserving layouts, workflows, accessibility, and data contracts. Use a micro-frontend bridge for coexistence, Playwright visual regression for <=1% pixel drift, and contract tests to ensure API compatibility during strangler rollout.

## Technical Context

**Language/Version**: TypeScript / Angular 18, Node 20 build chain  
**Primary Dependencies**: Angular CLI, RxJS, Nx or workspace tooling, Playwright for visual regression, Jest/Jasmine/Karma for unit tests  
**Storage**: No local data stores beyond browser storage; consumes existing APIs via gateway  
**Testing**: Playwright visual + functional suites, axe accessibility scans, Jest/Karma unit tests, contract tests for API parity  
**Target Platform**: Web (desktop + responsive breakpoints), served via Aspire-hosted backend/gateway  
**Project Type**: Web frontend with migration bridge for AngularJS coexistence  
**Performance Goals**: Equal or better load time than legacy; <=1% visual diff; p95 interactions unchanged or improved  
**Constraints**: Workflow parity (no UX changes), reuse existing API contracts, coexistence period for dual runtimes, branch naming with layer prefix  
**Scale/Scope**: 150+ components/modules; phased migration by module (dashboard first)

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID via existing BFF/session pattern  
- Authentication Pattern: Session-based; frontend relies on cookie/session issued by Identity/BFF  
- Token Validation: Backend responsibility; frontend only forwards requests through gateway  
- Session Storage: Cookies; no token storage in frontend code

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/ui/lms-web` (Angular 18 host + bridge to AngularJS)  
**Specification Path**: `Plan/Foundation/specs/003-ui-migration-preservation/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting for SPA and gateway configuration  
- [x] Domain/Application - None directly, but API contracts enforced via gateway/backends  
- [ ] Infrastructure - Not applicable beyond gateway routing

### Cross-Layer Dependencies

**Depends on layers**: Foundation services via gateway only  
**Specific Dependencies**: Gateway routing configs, existing backend APIs; no direct cross-layer coupling  
**Justification**: UI consumes existing Foundation APIs; no additional layers involved.  
**Constitutional Compliance**: Principle 6 upheld; no cross-layer service calls beyond authorized gateway access.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only; no implementation commits OK  
- Multi-tenancy preserved by honoring tenant headers via gateway OK  
- Security: No token storage in frontend; uses BFF session OK  
- Testing: Visual regression + contract tests planned OK  
- UI migration exemption from Figma acknowledged OK

## Project Structure

### Documentation (this feature)

```
specs/003-ui-migration-preservation/
- plan.md
- research.md
- data-model.md        # UI state and routing maps
- quickstart.md        # Running Angular 18 app with legacy bridge
- contracts/           # API contract snapshots used by UI
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/ui/lms-web/
- apps/host/           # Angular 18 shell
- libs/legacy-bridge/  # Adapters to AngularJS
- libs/features/<module>/
- assets/              # Shared styles, icons
- e2e/playwright/      # Visual + functional tests

Src/Foundation/gateway/   # Route configs unchanged but referenced for testing

tests/ui/
- visual-regression/
- accessibility/
- contract/            # UI contract tests against gateway stubs
```

**Structure Decision**: Web frontend workspace with host app and feature libs; bridge layer for AngularJS coexistence; e2e tests alongside.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

