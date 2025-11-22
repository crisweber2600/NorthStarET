# Clarification Decisions — 2025-11-21

Run of `@speckit.clarify` yielded the automatic choices below.

## 1. Primary data store
- **Options**
  - **A.** PostgreSQL schema with JSONB values.
  - **B.** Azure Cosmos DB (document store).
  - **C.** Azure Table Storage / key-value pairs.
- **Decision**: **A** — keeps parity with other services, supports transactions, and lets us leverage row-level security.

## 2. Hierarchy modeling
- **Options**
  - **A.** Single table tracking `scope_type` and `scope_id` with precedence logic.
  - **B.** Separate tables per scope (system/district/school).
  - **C.** Static configuration files merged at build time.
- **Decision**: **A** — simplifies overrides and enables atomic updates when promoting defaults to districts/schools.

## 3. Custom attributes pattern
- **Options**
  - **A.** JSON schema definitions stored per tenant with validation on write.
  - **B.** Generate bespoke tables/columns per tenant.
  - **C.** Store arbitrary key/value blobs with no schema.
- **Decision**: **A** — balances flexibility with validation so downstream services know how to render/edit the data.

## 4. Consumer access method
- **Options**
  - **A.** REST API with service-to-service Entra tokens plus client SDK.
  - **B.** gRPC-only API.
  - **C.** Direct database reads from each service.
- **Decision**: **A** — HTTP endpoints are easiest for .NET + front-end clients, and the SDK can hide caching concerns.

## 5. Audit log retention & revert mechanism
- **Options**
  - **A.** Append-only PostgreSQL table retained 90 days hot / 7 years cold, powering revert flows.
  - **B.** Write-only application logs in Blob Storage.
  - **C.** No dedicated audit (rely on events only).
- **Decision**: **A** — satisfies compliance requirements and gives deterministic rollback data.
