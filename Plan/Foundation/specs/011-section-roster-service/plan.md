# Implementation Plan: Section & Roster Service Migration

**Specification Branch**: `Foundation/011-section-roster-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/011-section-roster-service` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/011-section-roster-service/spec.md

**Note**: Depends on Student, Staff, and Configuration services for roster validation and calendars.

## Summary

Build a Section & Roster Service to manage sections, teacher assignments, student rosters, conflicts, capacity/waitlists, rollover, exports, and audit. Publish events (SectionCreated/StudentAddedToRoster/StudentDropped/RolloverCompleted/CapacityReached) and ensure attendance/gradebook consumers receive accurate feeds. Meet performance targets (create <100ms, add student <50ms, rollover 500 students <5m).

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: EF Core 9 + Npgsql, MassTransit + Azure Service Bus, FluentValidation, Redis optional for schedule cache, CsvHelper for exports  
**Storage**: PostgreSQL (sections, rosters, assignments, waitlists, rollover history, audit); optional Redis cache for schedule maps; blob storage for exports  
**Testing**: xUnit + Reqnroll BDD, consumer tests for events, conflict detection tests, performance harness for roster operations and rollover, authorization tests for RBAC  
**Target Platform**: Linux containers via Aspire; API through gateway  
**Project Type**: Backend microservice  
**Performance Goals**: Create/add student p95 <100/<50ms; rollover 500 students <5m; search p95 <100ms; conflict detection accuracy >99%  
**Constraints**: Tenant isolation; immutable history post-term; FIFO waitlist; branch naming with layer prefix  
**Scale/Scope**: District schedule volumes with yearly rollover

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID via gateway/BFF  
- Authentication Pattern: Session-based; service uses claims for authorization  
- Token Validation: Microsoft.Identity.Web in gateway; service honors tenant/user claims  
- Session Storage: Managed by Identity; no token storage in service

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/SectionRoster`  
**Specification Path**: `Plan/Foundation/specs/011-section-roster-service/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting, telemetry, health  
- [x] Domain - Entities/events for sections and rosters  
- [x] Application - CQRS, validation pipeline  
- [x] Infrastructure - PostgreSQL, Redis optional, messaging, export helpers

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: Student/Staff services (via events/APIs), Configuration calendars; ServiceDefaults/messaging  
**Justification**: All dependencies remain inside Foundation; no cross-layer coupling.  
**Constitutional Compliance**: Principle 6 upheld.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only OK  
- Multi-tenancy enforced via tenant_id + RLS OK  
- Security: audit + RBAC; secrets in store OK  
- Testing: unit + BDD + consumer + perf + conflict accuracy planned OK  
- No UI requirement OK

## Project Structure

### Documentation (this feature)

```
specs/011-section-roster-service/
- plan.md
- research.md
- data-model.md        # Section, Assignment, Roster, Waitlist, Rollover
- quickstart.md        # Running service locally with Aspire
- contracts/           # API + event schemas
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/services/SectionRoster/
- Api/
- Application/
- Domain/
- Infrastructure/
- Scheduling/          # Conflict and period validation
- Messaging/           # Events/consumers
- Rollover/            # Archive + template + promotion
- Tests/               # Unit, integration, BDD, consumer, perf

tests/section-roster-service/
- integration/
- contract/
- performance/
```

**Structure Decision**: Single backend microservice with scheduling, rollover, and messaging modules.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

