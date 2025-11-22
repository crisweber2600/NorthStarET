# Implementation Plan: Assessment Service Migration

**Specification Branch**: `Foundation/009-assessment-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/009-assessment-service` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/009-assessment-service/spec.md

**Note**: Depends on Configuration (grading scales), Student, and Section services for roster context; multi-tenant foundations required.

## Summary

Build a dedicated Assessment Service to handle definitions, assignments, scoring with benchmarks, trends, imports, exports, and audit. Publish events (AssessmentCreated/Assigned/ResultRecorded/ImportCompleted) and enforce tenant isolation, idempotent assignments, and performance targets (creation/assignment p95 <100ms; trend queries <200ms; events within 2s for 99%).

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: EF Core 9 + Npgsql, MassTransit + Azure Service Bus, FluentValidation, Redis optional for trend caches, CsvHelper for exports/imports  
**Storage**: PostgreSQL (assessments, assignments, results, benchmarks, imports, audit); Blob storage for export files; Redis optional cache  
**Testing**: xUnit + Reqnroll BDD, consumer tests for events, performance tests for trend queries and imports, contract tests for API schemas  
**Target Platform**: Linux containers via Aspire; API through gateway  
**Project Type**: Backend microservice  
**Performance Goals**: Create/assign p95 <100ms; result recording benchmarks computed with event emission <2s for 99%; trend queries p95 <200ms; imports 1k records <30s with <2% errors  
**Constraints**: Tenant isolation; benchmark overlap validation; audit on score changes; branch naming with layer prefix  
**Scale/Scope**: Millions of historical results across districts

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID via gateway/BFF  
- Authentication Pattern: Session-based; service uses claims for authorization  
- Token Validation: Microsoft.Identity.Web in gateway; service honors tenant/user claims  
- Session Storage: Managed by Identity; no token storage in service

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Assessment`  
**Specification Path**: `Plan/Foundation/specs/009-assessment-service/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting, telemetry, health  
- [x] Domain - Assessment aggregates/events  
- [x] Application - CQRS, validation pipeline  
- [x] Infrastructure - PostgreSQL, Redis (optional), messaging, blob storage

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: Configuration (grading scales), Student (rosters), Section (roster context) via events/APIs through gateway; ServiceDefaults/messaging  
**Justification**: All dependencies remain within Foundation; no cross-layer coupling.  
**Constitutional Compliance**: Principle 6 upheld.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only OK  
- Multi-tenancy enforced via tenant_id + RLS OK  
- Security: audit for score changes; secrets in store OK  
- Testing: unit + BDD + consumer + perf + contract planned OK  
- No UI requirement OK

## Project Structure

### Documentation (this feature)

```
specs/009-assessment-service/
- plan.md
- research.md
- data-model.md        # Assessment, Assignment, Result, Benchmark, ImportJob
- quickstart.md        # Running service locally with Aspire
- contracts/           # API + event schemas
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/services/Assessment/
- Api/
- Application/
- Domain/
- Infrastructure/
- Messaging/
- Trends/              # Trend calculation + caching
- ImportsExports/      # File ingest/export pipelines
- Tests/               # Unit, integration, BDD, consumer, perf

tests/assessment-service/
- integration/
- contract/
- performance/
```

**Structure Decision**: Single backend microservice with layered architecture and trend/import modules.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

