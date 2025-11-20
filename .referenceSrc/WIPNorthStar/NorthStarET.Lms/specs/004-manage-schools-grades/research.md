# Research Findings

## Catalog Isolation & API Routing

- Decision: Preserve tenant context in session claims and propagate via headers to Application layer commands, keeping tenancy outside resource URIs.
- Rationale: Microsoft REST API guidance recommends header or token-based tenant resolution to protect data isolation without polluting routes ([Best practices for RESTful web API design](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design#multitenant-web-apis)). This aligns with the existing session authentication scheme.
- Alternatives considered: Path-based tenant segments were rejected because they complicate routing and caching per Microsoft guidance, and would diverge from the platform-wide pattern already in use.

## EF Core Tenant Partitioning & Concurrency

- Decision: Extend EF Core entities with tenant filters and concurrency tokens to ensure per-district isolation and stale write detection during school edits.
- Rationale: EF Core multitenancy documentation demonstrates configuring per-tenant filters and optimistic concurrency to prevent cross-tenant leakage ([EF Core Multi-tenancy sample](https://learn.microsoft.com/en-us/ef/core/miscellaneous/multitenancy)). This matches the Clean Architecture boundary requirements.
- Alternatives considered: Introducing an external multitenancy library (e.g., Finbuckle) was rejected because the solution already enforces tenant context in Application layer services and adding another framework would add overhead without incremental benefit.

## Event Emission Strategy

- Decision: Emit school catalog change events through the existing Aspire-hosted messaging pipeline with versioned schemas to support downstream consumers.
- Rationale: Azure multitenant architecture guidance emphasizes asynchronous integration to maintain isolation and scalability ([Tenancy models for a multitenant solution](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/considerations/tenancy-models#tenant-isolation)). Versioned events allow parallel evolution.
- Alternatives considered: Synchronous API callbacks to downstream systems were rejected because they would violate the constitution's event-driven principle and introduce latency coupling.

## Additional Decisions & Rationale

- Language / Runtime: Use C# 12 / .NET 9 to match repository standards and minimize cross-version maintenance. This aligns with `global.json` and project SDKs.
- Primary Dependencies: MediatR, FluentValidation, EF Core 9 (Postgres), Redis Stack for idempotency, and Aspire orchestration. These match existing project patterns and simplify integration.
- Storage: PostgreSQL via EF Core 9 for relational constraints (unique school name per district) and audit durability.
- Testing & Evidence: Use xUnit (unit), Reqnroll (BDD), Aspire integration tests, and Playwright (UI). Store Red/Green transcripts in the feature spec folder for phase reviews.

## Next Steps (Phase 1 Inputs)

1. Produce `data-model.md` detailing entities: `School`, `GradeOffering`, associated fields, validation rules, and state transitions.
2. Create API contract drafts under `contracts/` (OpenAPI snippets) for listing, creating, updating, deleting schools and updating grade assignments.
3. Author Reqnroll feature files in `specs/004-manage-schools-grades/features/` and Playwright test stubs in `tests/ui/` so the Red phase is capturable.
4. Prepare `figma-prompts/` placeholder prompts to request the missing Figma frames from design; do not implement final UI until frames are available.

## Notes / Remaining Unknowns

- Figma designs remain unresolved — UI implementation is gated until assets are provided. Stories are labeled "Skipped — No Figma" per constitution.
- Confirm central grade taxonomy (PK–12) with product; default is to consume a read-only central taxonomy.

