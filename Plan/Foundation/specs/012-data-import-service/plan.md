# Implementation Plan: Data Import & Integration Service Migration

**Specification Branch**: `Foundation/012-data-import-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/012-data-import-service` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/012-data-import-service/spec.md

**Note**: Supports CSV/Excel uploads, SFTP/state test feeds, validation, transformation, scheduling, progress, duplicate handling, and rollback.

## Summary

Deliver a Data Import Service that validates and maps uploaded or scheduled files, runs resumable jobs with progress telemetry, handles duplicates and validation errors with exportable reports, and supports rollback. Publish events (ImportStarted/Completed/Failed/RowValidationFailed/ImportRollbackExecuted/StateTestDataImported) and meet performance targets (100 records/sec, validation <5s for 1k rows).

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: EF Core 9 + Npgsql, CsvHelper/ExcelDataReader, Hangfire or Quartz for scheduling, MassTransit + Azure Service Bus, FluentValidation, Polly for retries  
**Storage**: PostgreSQL (templates, jobs, runs, row results, audit, schedule), blob storage for uploaded files and error reports, Redis optional for progress cache  
**Testing**: xUnit + Reqnroll BDD, consumer tests for events, performance tests for throughput/validation times, contract tests for template schemas, resumability tests  
**Target Platform**: Linux containers via Aspire; operator UI (if any) behind gateway  
**Project Type**: Backend service with background workers  
**Performance Goals**: 95% of uploads <=10MB validated in <5s; scheduled jobs start within 2 minutes; processing >=100 records/sec with <2% error rate for valid files; rollback <5 minutes for 99% of requests  
**Constraints**: Tenant isolation; resumable jobs; error exports required; branch naming with layer prefix  
**Scale/Scope**: District-level recurring imports and ad-hoc uploads

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID; operator access via gateway/BFF  
- Authentication Pattern: Session-based; service uses claims for authorization  
- Token Validation: Microsoft.Identity.Web in gateway; service honors tenant/user claims  
- Session Storage: Managed by Identity; no token storage in service

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/DataImport`  
**Specification Path**: `Plan/Foundation/specs/012-data-import-service/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting, telemetry, health  
- [x] Domain - Entities/value objects for templates, jobs, row results  
- [x] Application - CQRS, validation pipeline  
- [x] Infrastructure - PostgreSQL, blob storage, messaging, scheduler, Redis optional

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: Configuration for tenant metadata; messaging to downstream services; ServiceDefaults for hosting  
**Justification**: All dependencies remain within Foundation; no cross-layer coupling.  
**Constitutional Compliance**: Principle 6 upheld.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only OK  
- Multi-tenancy enforced via tenant_id + RLS OK  
- Security: malware scan hook, secrets in store, audit logs OK  
- Testing: unit + BDD + consumer + perf + resumability planned OK  
- No UI requirement (operator UI optional) OK

## Project Structure

### Documentation (this feature)

```
specs/012-data-import-service/
- plan.md
- research.md
- data-model.md        # Template, Job, RowResult, DuplicateCandidate
- quickstart.md        # Running imports locally with Aspire
- contracts/           # API + event schemas
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/services/DataImport/
- Api/
- Application/
- Domain/
- Infrastructure/      # EF Core, blob storage, scheduler, messaging
- Workers/             # Import processors, duplicate resolution, rollback
- Validation/          # Template and business rule validation
- Reports/             # Error export and summaries
- Tests/               # Unit, integration, BDD, consumer, perf, resumability

tests/data-import-service/
- integration/
- contract/
- performance/
```

**Structure Decision**: Backend service with worker pipeline for imports; no frontend components in scope.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

