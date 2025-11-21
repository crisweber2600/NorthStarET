# Implementation Plan: Legacy IdentityServer to Microsoft Entra ID Migration

**Specification Branch**: `Foundation/001-entra-id-migration-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/001-entra-id-migration` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/001-entra-id-migration/spec.md

**Note**: Planning aligns with the parent Identity Service specification under CrossCuttingConcerns; scope is the migration execution slice.

## Summary

Migrate legacy IdentityServer users to Microsoft Entra ID with email-based matching, creation of ExternalProviderLinks, preservation of roles and tenant associations, and staged dry-runs. Use reconciliation reports, rollback switches, and session cleanup to ensure zero data loss and at least 99% post-migration login success.

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: Microsoft.Identity.Web, EF Core 9, MassTransit + Azure Service Bus, Polly for retries, Microsoft Graph for validation checks  
**Storage**: Legacy SQL Server (source), PostgreSQL Identity DB (target) with ExternalProviderLinks, role tables, audit tables  
**Testing**: xUnit + Reqnroll BDD for migration paths; scripted dry-runs with reconciliation; smoke Playwright for post-login verification  
**Target Platform**: Linux containers orchestrated via Aspire  
**Project Type**: Backend migration service plus admin scripts  
**Performance Goals**: Automated match rate >=95%; overnight cutover window; login success >=99% after cutover  
**Constraints**: Zero data loss; reversible rollback; no new auth flows beyond Entra; branch naming must include layer prefix  
**Scale/Scope**: All active districts/users in NorthStar; multi-tenant role preservation required

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID only; no custom issuers  
- Authentication Pattern: SessionAuthenticationHandler per parent spec; session tokens validated via Microsoft.Identity.Web  
- Session Storage: PostgreSQL identity.sessions plus Redis cache  
- Architecture Reference: `Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/spec.md`

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Identity/Migration`  
**Specification Path**: `Plan/Foundation/specs/001-entra-id-migration/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix per constitution

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Aspire hosting, logging, health checks  
- [x] Domain - Identity aggregates and value objects  
- [x] Application - Commands/queries for migration orchestration  
- [x] Infrastructure - PostgreSQL/Redis providers, MassTransit wiring

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: ServiceDefaults, Domain/Application plumbing, Infrastructure for database and messaging, parent Identity service contracts  
**Justification**: Migration stays within Identity domain, publishing events and updating identity data while honoring Foundation shared components.  
**Constitutional Compliance**: Principle 6 satisfied; no cross-layer service coupling.

### Constitution Check

- Layer-prefixed branch pattern in use: OK  
- Planning artifacts only in spec branch; no implementation code: OK  
- Multi-tenancy preserved via tenant role mapping; no cross-tenant access: OK  
- Security: Entra-only auth, secrets in platform store: OK  
- Testing gates planned (unit, BDD, dry-run evidence): OK  
- No UI; Figma requirement not applicable: OK

## Project Structure

### Documentation (this feature)

```
specs/001-entra-id-migration/
- plan.md          # This file
- research.md      # Migration unknowns, reconciliation approach
- data-model.md    # Mapping tables, state diagram
- quickstart.md    # How to run dry-run and production cutover
- contracts/       # Migration report/event schemas
- tasks.md         # Generated via /speckit.tasks
```

### Source Code (repository root)

```
Src/Foundation/services/Identity/
- Migration/              # Orchestrator, scripts, job runner
- Application/            # Commands/queries for migration operations
- Infrastructure/         # EF Core contexts, mappings, repositories
- Contracts/              # Events for migration reports (if needed)
- Tests/                  # Unit, integration, BDD

tests/identity-migration/
- integration/
- bdd/
- smoke/                  # Post-cutover login validation
```

**Structure Decision**: Single backend service with migration module; no frontend components.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

