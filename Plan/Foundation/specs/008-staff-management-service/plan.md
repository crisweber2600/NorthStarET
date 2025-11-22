# Implementation Plan: Staff Management Service Migration

**Specification Branch**: `Foundation/008-staff-management-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/008-staff-management-service` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/008-staff-management-service/spec.md

**Note**: Depends on Identity for account provisioning and Configuration for school validation.

## Summary

Move staff management out of the monolith into a dedicated service covering profiles, multi-school assignments, schedules, teams/PLCs, certifications, search, bulk import, and audit. Publish events (StaffCreated/ProfileUpdated/AssignmentChanged/CertificationReminder) and enforce tenant isolation, RBAC, and performance targets (search p95 <100ms; import 50 staff <60s).

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: EF Core 9 + Npgsql, MassTransit + Azure Service Bus, FluentValidation, Redis optional for directory caching, Azure Blob Storage for attachments if needed  
**Storage**: PostgreSQL (staff, assignments, teams, certifications, audit), optional Redis cache for directory search, blob storage for documents/photos  
**Testing**: xUnit + Reqnroll BDD, consumer tests for events, performance harness for search/import, authorization tests for RBAC  
**Target Platform**: Linux containers via Aspire; API exposed through gateway  
**Project Type**: Backend microservice  
**Performance Goals**: Search p95 <100ms; certification reminders issued >=60 days pre-expiry; import 50 staff <60s; event publish within 30s for creation  
**Constraints**: Tenant isolation; cumulative FTE <=1.0 per staff; audit for all mutations; branch naming with layer prefix  
**Scale/Scope**: District-wide staff populations with multi-school assignments

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID; account provisioning triggered from service events  
- Authentication Pattern: Session-based via gateway/BFF  
- Token Validation: Microsoft.Identity.Web in gateway; service uses claims for authorization  
- Session Storage: Managed by Identity service; no token storage in service

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Staff`  
**Specification Path**: `Plan/Foundation/specs/008-staff-management-service/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting, telemetry, health  
- [x] Domain - Entities/events for staff/assignments  
- [x] Application - CQRS and validation pipeline  
- [x] Infrastructure - PostgreSQL/Redis, messaging, blob storage helpers

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: Identity for provisioning, Configuration for school/tenant validation, Section/Assessment consume events  
**Justification**: All dependencies remain in Foundation; no cross-layer coupling.  
**Constitutional Compliance**: Principle 6 upheld.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only OK  
- Multi-tenancy enforced via tenant_id + RLS OK  
- Security: FERPA and RBAC enforced; secrets in store OK  
- Testing: unit + BDD + consumer + perf + auth coverage planned OK  
- No UI scope OK

## Project Structure

### Documentation (this feature)

```
specs/008-staff-management-service/
- plan.md
- research.md
- data-model.md        # Staff, Assignment, Team, Certification, ImportJob
- quickstart.md        # Running service locally with Aspire
- contracts/           # API + event schemas
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/services/Staff/
- Api/
- Application/
- Domain/
- Infrastructure/
- Messaging/           # Events/consumers
- Import/              # Bulk import pipeline
- Tests/               # Unit, integration, BDD, consumer, perf

tests/staff-service/
- integration/
- contract/
- performance/
```

**Structure Decision**: Single backend microservice with layered architecture and import pipeline.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

