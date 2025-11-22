# Implementation Plan: Intervention Management Service Migration

**Specification Branch**: `Foundation/010-intervention-management-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/010-intervention-management-service` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/010-intervention-management-service/spec.md

**Note**: Depends on Student and Staff services for enrollment/facilitator data and Configuration for calendars.

## Summary

Create an Intervention Service for MTSS/RTI workflows: define interventions and schedules, enroll students (manual or rules-based), record attendance/notes, evaluate progress and exit criteria, send caregiver communications, and audit all actions. Publish events (InterventionCreated/Enrollment/Attendance/Progress/Exit) and deliver dashboards with p95 <200ms metrics.

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: EF Core 9 + Npgsql, MassTransit + Azure Service Bus, FluentValidation, Redis optional for dashboard caching, templating library for communications  
**Storage**: PostgreSQL (interventions, sessions, enrollments, attendance, progress notes, communications, audit); Redis optional cache; blob storage for attachments if needed  
**Testing**: xUnit + Reqnroll BDD, consumer tests for events, performance tests for dashboards and scheduling conflicts, authorization tests for RBAC  
**Target Platform**: Linux containers via Aspire; API through gateway  
**Project Type**: Backend microservice  
**Performance Goals**: Create/enroll p95 <100ms; attendance events within 2s for 99%; conflict detection >99% accuracy before publish; dashboard p95 <200ms  
**Constraints**: Tenant isolation; RLS; audit logs for all actions/communications; branch naming with layer prefix  
**Scale/Scope**: District-level interventions with recurring sessions and historical preservation

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID; session-based via gateway/BFF  
- Token Validation: Microsoft.Identity.Web in gateway; service uses claims for authorization  
- Session Storage: Managed by Identity; no token storage in service

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Intervention`  
**Specification Path**: `Plan/Foundation/specs/010-intervention-management-service/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting, telemetry, health  
- [x] Domain - Entities/events for interventions, sessions, enrollments  
- [x] Application - CQRS, validation pipeline  
- [x] Infrastructure - PostgreSQL, Redis (optional), messaging, templating

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: Student and Staff services (via events/APIs), Configuration for calendars; ServiceDefaults/messaging  
**Justification**: All dependencies stay within Foundation; no cross-layer coupling.  
**Constitutional Compliance**: Principle 6 upheld.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only OK  
- Multi-tenancy enforced via tenant_id + RLS OK  
- Security: audit and RBAC enforced; secrets in store OK  
- Testing: unit + BDD + consumer + perf + conflict detection coverage planned OK  
- No UI requirement OK

## Project Structure

### Documentation (this feature)

```
specs/010-intervention-management-service/
- plan.md
- research.md
- data-model.md        # Intervention, Session, Enrollment, Attendance, Progress, Communication
- quickstart.md        # Running service locally with Aspire
- contracts/           # API + event schemas
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/services/Intervention/
- Api/
- Application/
- Domain/
- Infrastructure/
- Scheduling/          # Conflict detection, session generation
- Messaging/           # Events/consumers
- Communications/      # Template + delivery tracking
- Tests/               # Unit, integration, BDD, consumer, perf

tests/intervention-service/
- integration/
- contract/
- performance/
```

**Structure Decision**: Single backend microservice with scheduling, messaging, and communications modules.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

