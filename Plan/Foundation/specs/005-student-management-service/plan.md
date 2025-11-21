# Implementation Plan: Student Management Service Migration

**Specification Branch**: `Foundation/005-student-management-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/005-student-management-service` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/005-student-management-service/spec.md

**Note**: Depends on multi-tenant architecture (002) and Configuration/Identity services for validation.

## Summary

Extract student management from the monolith into an event-driven Student Service providing CRUD, enrollment, search, import/export, dashboards, and audit. Enforce tenant isolation, publish domain events (StudentCreated/Enrolled/Withdrawn/Merged), and meet performance targets (search p95 <100ms, import 500 students <2 minutes).

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: EF Core 9 + Npgsql, MassTransit + Azure Service Bus, FluentValidation, Redis cache (optional for dashboard), Azure Blob Storage for photos  
**Storage**: PostgreSQL (student, enrollment, photo metadata, audit); Blob storage for photos; Redis for caching dashboard aggregates  
**Testing**: xUnit + Reqnroll BDD, consumer tests for events, Playwright smoke for dashboard API if exposed via UI, performance harness for search/import  
**Target Platform**: Linux containers via Aspire; API exposed through gateway  
**Project Type**: Backend microservice  
**Performance Goals**: Search p95 <100ms; event publish overhead <50ms; import 500 students <2 minutes; dashboard <200ms  
**Constraints**: Tenant isolation (RLS); idempotent create/import; audit on CRUD and reads; branch naming with layer prefix  
**Scale/Scope**: District-level volumes; millions of students across tenants

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID; SessionAuthenticationHandler  
- Token Validation: Microsoft.Identity.Web; tenant/user claims required  
- Session Storage: PostgreSQL + Redis via Identity service; service trusts gateway/BFF for auth context

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Student`  
**Specification Path**: `Plan/Foundation/specs/005-student-management-service/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting, telemetry, health  
- [x] Domain - Base entities, domain events  
- [x] Application - CQRS patterns, validation pipeline  
- [x] Infrastructure - PostgreSQL/Redis, messaging, blob storage helpers

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: Configuration Service for school/tenant validation; Identity for auth context; ServiceDefaults for hosting; messaging for publishing events  
**Justification**: All dependencies remain within Foundation; no cross-layer service coupling.  
**Constitutional Compliance**: Principle 6 upheld.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only in spec branch OK  
- Multi-tenancy enforced via tenant_id and RLS OK  
- Security: FERPA compliance, secrets in store OK  
- Testing: unit + BDD + consumer + perf gates planned OK  
- No Figma/UI scope in this service OK

## Project Structure

### Documentation (this feature)

```
specs/005-student-management-service/
- plan.md
- research.md
- data-model.md        # Student, Enrollment, Photo, Mapping
- quickstart.md        # Running service locally with Aspire
- contracts/           # API + event schemas
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/services/Student/
- Api/                 # Controllers/endpoints
- Application/         # Commands, queries, validators
- Domain/              # Entities, aggregates, events
- Infrastructure/      # EF Core, blob storage, RLS policies
- Messaging/           # MassTransit publishers/consumers
- Tests/               # Unit, integration, BDD, consumer

tests/student-service/
- integration/
- contract/
- performance/
```

**Structure Decision**: Single backend microservice with layered architecture; no frontend components.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

