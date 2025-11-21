# Research: Data Import & Integration Service Migration
Layer: Foundation
Version: 0.1.0

## Decisions
- **Pipeline built on EF Core + background scheduler (Hangfire or Quartz)**  
  - Rationale: supports recurring jobs (SFTP/nightly) and resumable processing with persisted checkpoints.  
  - Alternatives: Azure Data Factory (new dependency; overkill for app-layer imports).

- **Template-driven validation using FluentValidation + column mapping**  
  - Rationale: flexible per-tenant templates; early failure detection per spec.  
  - Alternatives: static schema per file type (less flexible for districts).

- **Blob storage for file artifacts and error reports**  
  - Rationale: keeps large files out of DB; aligns with other services.  
  - Alternatives: DB bytea (bloats storage), temp filesystem (scales poorly).

- **Event emission for lifecycle (ImportStarted/Completed/Failed/RollbackExecuted/StateTestDataImported)**  
  - Rationale: downstream services subscribe for reconciliation and analytics; matches spec requirements.  
  - Alternatives: polling status only (higher coupling and latency).

## Open Questions
1. Malware scanning step: reuse existing scanning job or integrate third-party? Assume reuse shared scanning hook before validation.
2. Maximum supported file size and chunking strategy; defaults to 50MB? Need firm limit for UI guidance.
3. Retention for file artifacts and row-level error reports; propose 30 days unless compliance requires longer.
