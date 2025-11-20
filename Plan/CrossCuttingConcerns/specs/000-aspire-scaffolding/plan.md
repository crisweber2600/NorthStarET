# Implementation Plan: Aspire Orchestration & Cross-Cutting Scaffolding

**Specification Branch**: `CrossCuttingConcerns/000-aspire-scaffolding-spec` *(current branch - planning artifacts)*
**Implementation Branch**: `CrossCuttingConcerns/000-aspire-scaffolding` *(created after approval)*
**Date**: 2025-11-20 | **Spec**: [spec.md](spec.md)

**Note**: This template is filled in by the `/speckit.plan` command. See `.github/agents/speckit.plan.agent.md` for the execution workflow.

## Summary

This feature establishes the foundational scaffolding for the NorthStarET platform using .NET Aspire for orchestration. It implements cross-cutting concerns including multi-tenancy enforcement (TenantInterceptor), event-driven messaging (MassTransit), caching & idempotency (Redis), and unified observability (OpenTelemetry). This scaffolding ensures all microservices inherit consistent patterns for security, reliability, and diagnostics from day one.

## Technical Context

**Language/Version**: C# 12 / .NET 10 (GA - November 2025, LTS until 2028)
**Primary Dependencies**: .NET Aspire 9.5, MassTransit 8.x, Entity Framework Core 10, OpenTelemetry 1.x
**Storage**: PostgreSQL (Per-service DBs), Redis (Caching/Idempotency), RabbitMQ (Messaging)
**Testing**: xUnit, Reqnroll (BDD), Aspire.Hosting.Testing
**Target Platform**: Linux Containers (Docker/Podman) via Aspire Orchestration
**Project Type**: Distributed System / Microservices Foundation
**Performance Goals**: AppHost startup < 15s, API overhead < 50ms P95
**Constraints**: Strict multi-tenancy (RLS + Interceptors), Idempotency (10m window)
**Scale/Scope**: Foundation for 11+ microservices

### Identity & Authentication Guidance

*If this feature requires authentication/authorization:*

- **Identity Provider**: Microsoft Entra ID (Azure AD)
- **Authentication Pattern**: Session-based with custom `SessionAuthenticationHandler`
- **Token Validation**: `Microsoft.Identity.Web`
- **Session Storage**: PostgreSQL + Redis
- **Architecture Reference**: `Plan/Foundation/Plans/docs/legacy-identityserver-migration.md`
- **Key Dependencies**: `Microsoft.Identity.Web`, `StackExchange.Redis`, `Aspire.Hosting.Redis`

## Layer Identification (MANDATORY)

*REQUIRED: Declare this feature's position in the mono-repo architecture. Must match layer declared in spec.md.*

**Target Layer**: CrossCuttingConcerns
**Implementation Path**: `Src/Foundation/AppHost`, `Src/Foundation/shared/ServiceDefaults`, `Src/Foundation/shared/Infrastructure`
**Specification Path**: `Plan/CrossCuttingConcerns/specs/000-aspire-scaffolding/`

### Layer Consistency Validation

*Verify layer alignment between spec and plan*

- [x] Target Layer matches specification (spec.md Layer Identification section)
- [x] Implementation path follows layer structure (`Src/Foundation/...` - *Note: CrossCuttingConcerns are implemented in Foundation shared libraries as per architecture*)
- [x] Specification path follows layer structure (`Plan/CrossCuttingConcerns/specs/...`)
- [x] If new layer: Architecture Review completed and documented in `Plan/{LayerName}/README.md`

### Shared Infrastructure Dependencies

*List shared components this feature depends on (from `Src/Foundation/shared/`)*

- [x] **ServiceDefaults** - Defines the defaults this feature implements
- [x] **Domain** - Defines `IEntity`, `IDomainEvent` used by scaffolding
- [x] **Application** - Defines `ICommand`, `IQuery` used by scaffolding
- [x] **Infrastructure** - Defines `DbContext`, `MassTransit` config

### Cross-Layer Dependencies

*CAUTION: Cross-layer dependencies require justification and constitutional approval*

**Depends on layers**: None (This feature *builds* the shared infrastructure)
**Specific Dependencies**: N/A
**Justification**: This feature establishes the base layer for all other features.
**Constitutional Compliance**: Compliant (Principle 6).

### Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Clean Architecture**: Enforces vertical slices and dependency rules.
- [x] **Aspire Orchestration**: Uses AppHost and ServiceDefaults.
- [x] **Test-Driven**: Includes BDD and TDD workflows in scaffolding.
- [x] **Event-Driven**: Configures MassTransit for async integration.
- [x] **Multi-Tenancy**: Implements `TenantInterceptor` and RLS.
- [x] **Security**: Configures Entra ID and secret management.
- [x] **Mono-Repo Isolation**: Respects layer boundaries.

## Project Structure

### Documentation (this feature)

```text
specs/000-aspire-scaffolding/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
Src/Foundation/
├── AppHost/
│   ├── Program.cs       # Aspire orchestration
│   └── AppHost.csproj
├── shared/
│   ├── ServiceDefaults/ # Shared Aspire defaults
│   │   ├── Extensions.cs
│   │   └── ServiceDefaults.csproj
│   ├── Infrastructure/  # Shared Infrastructure
│   │   ├── Persistence/
│   │   │   ├── Interceptors/
│   │   │   │   └── TenantInterceptor.cs
│   │   │   └── ...
│   │   ├── Messaging/
│   │   │   └── MassTransitExtensions.cs
│   │   └── ...
│   └── ...
└── ...
```

## Phases

### Phase 0: Research & Validation

1. **Aspire 9.5 / .NET 10 GA Compatibility**: Verify package versions and confirm no breaking changes.
2. **Multi-Tenancy Pattern**: Implement `TenantInterceptor` with EF Core 10 and `[IgnoreTenantFilter]` attribute for opt-out scenarios.
3. **Idempotency Implementation**: Redis-backed envelope with 10-minute TTL returning 202 Accepted on duplicates.
4. **Observability**: OpenTelemetry configuration for distributed tracing across AppHost, services, and messaging.
5. **Scaffolding Scripts**: PowerShell and Bash scripts for cross-platform service generation.

### Phase 1: Design & Contracts

1. **Data Model**: Define `EntityBase`, `ITenantEntity` interface, and `AuditLog` schema for tenant filter opt-outs.
2. **Contracts**: Health check endpoints (`/health`, `/alive`), idempotency headers (X-Idempotency-Key), audit log structure.
3. **Scaffolding Scripts**: Design PowerShell (`new-service.ps1`) and Bash (`new-service.sh`) scripts with service name parameter.
4. **Quickstart**: Developer guide for running AppHost and using scaffolding scripts.

### Phase 2: Implementation Tasks

1. **AppHost Setup**: Initialize Aspire 9.5 AppHost with Postgres, Redis, RabbitMQ resource definitions and health checks.
2. **ServiceDefaults**: Implement shared OpenTelemetry configuration, health checks, and correlation ID middleware.
3. **TenantInterceptor**: EF Core interceptor with `[IgnoreTenantFilter]` attribute support and automatic audit logging.
4. **IdempotencyService**: Redis-backed with 10-minute TTL, returning 202 Accepted with original entity ID on duplicates.
5. **MassTransit Configuration**: Setup with retry policies, DLQ, and idempotency integration.
6. **Scaffolding Scripts**: PowerShell and Bash scripts generating service structure with AppHost registration.
7. **Validation**: Aspire integration tests verifying resource health, tenant isolation, idempotency, and observability.
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
