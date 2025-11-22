# Implementation Plan: Multi-Tenant Database Architecture

**Specification Branch**: `Foundation/002-multi-tenant-database-architecture-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/002-multi-tenant-database-architecture` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/002-multi-tenant-database-architecture/spec.md

**Note**: Establishes database-per-service with strict tenant isolation via PostgreSQL RLS and app tenant propagation across all Foundation services.

## Summary

Design and implement multi-tenant database patterns: enforce PostgreSQL RLS on all tenant-scoped tables, propagate tenant context from gateway to DB session, migrate per-district databases into consolidated service DBs, and validate isolation through automated tests and reconciliation scripts. Targets p95 <100ms for tenant-filtered queries and safe onboarding for hundreds of districts.

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: EF Core 9, Npgsql, PostgreSQL 16, MassTransit for eventing, Polly for transient handling  
**Storage**: PostgreSQL per service (multi-tenant schemas), Redis optional for tenant metadata cache  
**Testing**: xUnit integration for RLS, Reqnroll BDD for isolation scenarios, pgTAP/SQL checks for policy enforcement, performance harness for query latency  
**Target Platform**: Linux containers via Aspire; managed Postgres instances  
**Project Type**: Backend infrastructure pattern across services  
**Performance Goals**: Tenant-filtered queries p95 <100ms; tenant context set overhead <50ms; support 500+ districts and millions of rows  
**Constraints**: RLS mandatory on tenant tables; JWT tenant claim validated at gateway; backups must retain policies; branch naming with layer prefix  
**Scale/Scope**: Applies to all Foundation services; migration of existing districts from single-tenant DBs

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID; tenant_id claim required  
- Authentication Pattern: SessionAuthenticationHandler with tenant propagation to DB session variable (`app.current_tenant`)  
- Token Validation: Microsoft.Identity.Web; tenant claim enforced at gateway/API boundaries  
- Session Storage: PostgreSQL + Redis as per Identity architecture

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/shared/Infrastructure/MultiTenancy` plus service-specific migrations under `Src/Foundation/services/*`  
**Specification Path**: `Plan/Foundation/specs/002-multi-tenant-database-architecture/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation paths follow layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Aspire hosting, health, telemetry  
- [x] Domain - Shared value objects for tenant identifiers  
- [x] Application - Middleware for tenant context propagation  
- [x] Infrastructure - PostgreSQL, RLS setup helpers, Redis cache

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: ServiceDefaults, Domain/Application tenant context helpers, Infrastructure (PostgreSQL/RLS tooling)  
**Justification**: Applies shared multi-tenancy to all Foundation services without crossing layers.  
**Constitutional Compliance**: Principle 6 upheld; no cross-layer service coupling.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts scoped to spec branch OK  
- Multi-tenancy enforced via RLS and tenant claims OK  
- Security: no cross-tenant queries; secrets in approved stores OK  
- Testing gates defined (RLS integration, BDD, perf) OK  
- No UI; Figma requirement not applicable OK

## Project Structure

### Documentation (this feature)

```
specs/002-multi-tenant-database-architecture/
- plan.md
- research.md
- data-model.md        # Tenant tables, RLS policy examples
- quickstart.md        # How to enable/verify tenant context in services
- contracts/           # None expected; keep directory for future events
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/shared/Infrastructure/MultiTenancy/
- TenantContextMiddleware/
- RlsPolicyMigrations/
- Testing/IsolationHarness/

Src/Foundation/services/<service>/
- Migrations/          # Includes tenant_id columns + RLS policies
- Application/         # Tenant-aware base handlers
- Tests/               # Service-specific isolation tests

tests/multi-tenancy/
- integration/         # RLS enforcement, policy coverage
- performance/         # Query latency under tenant filters
```

**Structure Decision**: Shared multi-tenancy package plus service-level migrations and tests; no frontend components.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

