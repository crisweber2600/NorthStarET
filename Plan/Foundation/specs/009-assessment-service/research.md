# Research: Assessment Service Migration
Layer: Foundation
Version: 0.1.0

## Decisions
- **Assessment definitions stored with versioning and template library**  
  - Rationale: supports reuse and auditability; aligns with spec requirements for custom fields and corrections.  
  - Alternatives: immutable definitions without version history (blocks corrections).

- **Scoring + benchmarks in domain layer with validation**  
  - Rationale: prevents overlapping benchmark thresholds and ensures consistent rubric calculations.  
  - Alternatives: DB-only constraints or client-side validation (higher risk of invalid configs).

- **Event-driven lifecycle (AssessmentCreated, Assigned, ResultRecorded, ImportCompleted)**  
  - Rationale: aligns with event discipline; notifies analytics/intervention flows.  
  - Alternatives: synchronous callbacks or polling.

- **Imports/exports via shared pipeline**  
  - Rationale: reuse Data Import Service templates for state test/CSV ingestion; exports use background jobs with audit.  
  - Alternatives: inline synchronous uploads (would block UI, harder to resume).

## Open Questions
1. Accepted scoring models beyond rubric/weighted? (e.g., adaptive, partial credit). Default to rubric + weighted now.
2. Storage for attached artifacts (rubrics, resource links)—use blob storage with reference in definition? Likely yes.
3. Benchmark ownership—does Configuration service supply grading scales or are they stored locally and referenced? Prefer referencing Configuration grading scales.
