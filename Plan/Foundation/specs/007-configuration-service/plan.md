# Implementation Plan: Configuration Service Migration

**Specification Branch**: `Foundation/007-configuration-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/007-configuration-service` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/007-configuration-service/spec.md

**Note**: Provides tenant-scoped settings, calendars, grading scales, custom attributes, and templates with hierarchical overrides.

## Summary

Deliver a dedicated Configuration Service with deterministic resolution (system -> district -> school), calendar management, grading scales, custom attributes, and notification templates. Enforce tenant isolation, caching with invalidation, validation rules for conflicts/overlaps, and audit for all writes. Publish change events for downstream consumers.

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: EF Core 9 + Npgsql, Redis for caching, MassTransit + Azure Service Bus, FluentValidation, Polly for retries  
**Storage**: PostgreSQL (configuration, calendars, grading scales, templates, audit); Redis cache for resolved settings  
**Testing**: xUnit + Reqnroll BDD, consumer tests for change events, cache correctness tests, performance tests for read p95 <50ms  
**Target Platform**: Linux containers via Aspire; API through gateway  
**Project Type**: Backend microservice  
**Performance Goals**: Read p95 <50ms cached; cache invalidation propagation <5s for 99% writes; validation prevents overlap conflicts  
**Constraints**: Tenant isolation; deterministic hierarchy resolution; audit for all writes; branch naming with layer prefix  
**Scale/Scope**: All districts/schools; high read volume, moderate write volume

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID via gateway/BFF  
- Authentication Pattern: Session-based; service trusts upstream auth context  
- Token Validation: Microsoft.Identity.Web; tenant/user claims required  
- Session Storage: Managed by Identity; service uses claims for authorization

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Configuration`  
**Specification Path**: `Plan/Foundation/specs/007-configuration-service/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting, telemetry, health  
- [x] Domain - Entities/value objects for settings, calendars, grading scales  
- [x] Application - CQRS/validation pipeline  
- [x] Infrastructure - PostgreSQL, Redis cache, messaging

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: ServiceDefaults, Domain/Application, Redis cache, messaging for change events; downstream services consume events via gateway  
**Justification**: Provides shared settings to Foundation services without cross-layer coupling.  
**Constitutional Compliance**: Principle 6 upheld.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only in spec branch OK  
- Multi-tenancy enforced via tenant_id + RLS OK  
- Security: RBAC, audit, secrets in store OK  
- Testing: unit + BDD + cache correctness + consumer tests planned OK  
- No UI/Figma requirement OK

## Project Structure

### Documentation (this feature)

```
specs/007-configuration-service/
- plan.md
- research.md
- data-model.md        # Settings, calendar, grading scale, templates
- quickstart.md        # Running service with cache + events
- contracts/           # API + change event schemas
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/services/Configuration/
- Api/
- Application/
- Domain/
- Infrastructure/      # EF Core, Redis cache, messaging
- Resolution/          # Hierarchy resolution engine
- Tests/               # Unit, integration, BDD, consumer, cache correctness

tests/configuration-service/
- integration/
- contract/
- performance/
```

**Structure Decision**: Single backend microservice with layered architecture and cache layer.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

