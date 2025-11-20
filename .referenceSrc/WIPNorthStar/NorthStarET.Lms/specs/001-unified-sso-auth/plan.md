ios/ or android/
# Implementation Plan: Unified SSO & Authorization via Entra (LMS Identity Module)

**Branch**: `001-unified-sso-auth` | **Date**: 2025-10-21 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/001-unified-sso-auth/spec.md`

## Summary

Deliver single sign-on and fast authorization across NorthStarET portals by federating authentication to Microsoft Entra ID and hosting the LMS Identity module inside the existing LMS API. The module validates Entra tokens, exchanges them for LMS sessions, persists users/tenants/memberships in a dedicated `identity` schema, and serves sub-50 ms authorization decisions via Redis-backed caching. Session continuity, tenant switching, and graceful expiration complete the MVP while UI tasks remain blocked pending Figma assets.

## Technical Context

**Language/Version**: C# 12 / .NET 9.0  
**Primary Dependencies**: Microsoft.Identity.Web, ASP.NET Core Identity, Entity Framework Core (Npgsql), StackExchange.Redis, Polly, MassTransit, .NET Aspire 9.0  
**Storage**: PostgreSQL (`identity` schema) for identity data; Redis for authorization/session caching  
**Testing**: xUnit, Reqnroll (BDD), Playwright, Aspire integration test projects  
**Target Platform**: Linux containers orchestrated via .NET Aspire AppHost  
**Project Type**: Multi-project Clean Architecture solution (UI MVC, API, Application, Domain, Infrastructure, ServiceDefaults, AppHost)  
**Performance Goals**: Authorization decisions <50 ms P95; full login flow <800 ms P95; tenant switching <200 ms; session refresh transparent  
**Constraints**: Enforce Clean Architecture boundaries, Aspire orchestration, ≥80 % test coverage, Redis/DB access via Service Defaults, synchronous identity queries must document fallback/resilience  
**Scale/Scope**: 5 personas, multi-tenant (50+ memberships), distributed caching, session renewal, event-driven updates for role/membership changes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Clean Architecture & Aspire**: ✅ projects already separated (UI → Application → Domain ← Infrastructure) with AppHost orchestrating Web/API plus PostgreSQL & Redis resources. No direct UI → Infrastructure access planned.
- **Test-Driven Quality Gates**: ✅ plan mandates red-phase tests (unit, BDD, Aspire, Playwright). UI journeys flagged “Skipped — No Figma” until assets exist. Full `dotnet build` + complete test suite scheduled at phase boundaries.
- **UX Traceability & Figma**: ⚠️ UI deliverables remain blocked; figma prompts exist (`figma-prompts/*.md`) and UI tasks in `tasks.md` tagged “Skipped — No Figma”. Must secure frames before implementing UI changes.
- **Event-Driven Discipline**: ✅ identity module emits/consumes domain events; synchronous queries stay intra-service with documented cache fallback. Redis cache TTL (10 min) plus circuit breaker captured.
- **Security & Compliance**: ✅ application-layer authorization, secrets via platform store, bearer-secured identity endpoints, audit logging required.

Gate verdict: ✅ PASS with noted UI/Figma blocker (acceptable as no UI implementation planned yet).

## Project Structure

### Documentation (this feature)

```
specs/001-unified-sso-auth/
├── plan.md              # This file (updated by /speckit.plan)
├── research.md          # Phase 0 decisions & alternatives
├── data-model.md        # Phase 1 domain/entity design
├── quickstart.md        # Phase 1 setup & workflows
├── contracts/
│   ├── authentication-api.yaml
│   ├── authorization-api.yaml
│   └── domain-events.yaml
├── figma-prompts/
│   ├── sign-in-flow.md
│   ├── tenant-switching.md
│   └── session-expiration.md
└── tasks.md             # Phase 2 task breakdown (/speckit.tasks)
```

### Source Code (repository root)

```
src/
├── NorthStarET.NextGen.Lms.Web/                # ASP.NET Core MVC UI (depends on Application)
├── NorthStarET.NextGen.Lms.Api/                # Web API hosting LMS Identity module endpoints
├── NorthStarET.NextGen.Lms.Application/        # Application layer (Commands/Queries/Services)
├── NorthStarET.NextGen.Lms.Domain/             # Domain entities, value objects, events
├── NorthStarET.NextGen.Lms.Infrastructure/     # EF Core (identity schema), Redis caching, integrations
├── NorthStarET.NextGen.Lms.Contracts/          # Shared DTOs for API/clients
├── NorthStarET.NextGen.Lms.AppHost/            # .NET Aspire host wiring API/Web/Postgres/Redis
└── NorthStarET.NextGen.Lms.ServiceDefaults/    # Service default extensions (resilience, telemetry)

tests/
├── unit/
│   ├── NorthStarET.NextGen.Lms.Application.Tests/
│   └── NorthStarET.NextGen.Lms.Domain.Tests/
├── integration/
│   └── NorthStarET.NextGen.Lms.Identity.IntegrationTests/   # Aspire-backed flows (Postgres/Redis)
├── bdd/
│   └── NorthStarET.NextGen.Lms.Bdd/                         # Reqnroll features & steps
├── ui/
│   └── NorthStarET.NextGen.Lms.Playwright/                  # Browser journeys (blocked pending Figma)
└── aspire/
    └── NorthStarET.NextGen.Lms.AspireTests/                 # AppHost orchestration smoke tests
```

**Structure Decision**: Maintain Clean Architecture multi-project solution with LMS Identity module integrated inside `NorthStarET.NextGen.Lms.Api` + Infrastructure persistence. Tests mirror layers; Aspire orchestrates resources to honor constitution gates.

## Complexity Tracking

No constitution violations requiring justification beyond documented synchronous access to the LMS Identity module (intra-service DB/cache lookups with fallbacks). Current architecture stays within approved constraints, so no additional entries needed.

