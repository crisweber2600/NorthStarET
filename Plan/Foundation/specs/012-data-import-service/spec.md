# Feature Specification: Data Import & Integration Service Migration

**Specification Branch**: `Foundation/012-data-import-service-migration-spec`  
**Implementation Branch**: `Foundation/012-data-import-service-migration`  
**Created**: 2025-11-20  
**Status**: Draft  
**Input**: User description: "Provide reliable, asynchronous, auditable ingestion of external data (CSV, Excel, SFTP/state tests) with validation, transformation, scheduling, rollback, and progress reporting."

---

## Layer Identification (MANDATORY)

**Target Layer**: Foundation

**Layer Validation Checklist**:
- [x] Layer explicitly identified (Foundation)
- [x] Layer exists in mono-repo structure (`Plan/Foundation/` and `Src/Foundation/`)
- [x] If new layer: Architecture Review documented in `Plan/{LayerName}/README.md` (Not a new layer)
- [x] Cross-layer dependencies justified and limited to approved shared infrastructure

**Cross-Layer Dependencies**: Foundation shared infrastructure (ServiceDefaults for messaging, shared Domain/Application patterns).  
**Justification**: Import capabilities depend only on shared messaging, scheduling, and persistence patterns; no extra cross-layer coupling.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Upload and validate files before import (Priority: P1)

Admins upload CSV/Excel files, select templates, and run pre-validation to catch schema and rule errors before execution.

**Why this priority**: Prevents bad data from entering the system and saves rework.  
**Independent Test**: Upload a file, map to a template, run validation, and confirm pass/fail results with actionable errors.

**Acceptance Scenarios**:

1. Given a CSV missing required columns, when validation runs, then the job is blocked with a clear error report and no data is ingested.
2. Given an Excel file with multiple sheets, when mapped to a template, then all sheets are processed according to mappings and type conversions.
3. Given a validated file, when the user confirms import, then the job transitions to scheduled/processing with a unique job identifier.

---

### User Story 2 - Schedule, monitor, and resume imports (Priority: P2)

Admins schedule recurring imports (e.g., nightly SFTP), monitor progress, and resume interrupted jobs safely.

**Why this priority**: Ensures timely data delivery and operational resilience.  
**Independent Test**: Configure a nightly SFTP import, observe job progress metrics, simulate an interruption, and verify resumable processing with correct status.

**Acceptance Scenarios**:

1. Given a scheduled job, when the trigger time arrives, then the service fetches the file, starts processing, and exposes real-time progress (processed/total, ETA).
2. Given a transient failure mid-job, when retried, then processing resumes from last committed offset without duplicating previously ingested rows.
3. Given a completed job, when reviewing history, then the job shows duration, success counts, failure counts, and links to error files.

---

### User Story 3 - Handle errors, duplicates, and rollback (Priority: P3)

Operators manage duplicates, validation failures, and rollback partial imports with auditability.

**Why this priority**: Reduces risk of corrupt data and supports operational recovery.  
**Independent Test**: Run an import with duplicate records and validation failures, resolve duplicates, generate an error export, and perform a rollback that restores prior state.

**Acceptance Scenarios**:

1. Given detected duplicates, when review occurs, then operators can accept, merge, or reject records, and the chosen action is auditable.
2. Given validation failures, when exporting error rows, then a CSV with failures and reasons is available and sent via notification.
3. Given a rollback request, when invoked within the retention window, then the import's effects are reversed and a rollback event is published.

### Edge Cases

- What happens when file size exceeds configured limits? -> Upload is rejected with guidance on chunking or alternate transfer methods.  
- How does the system handle template changes between scheduling and execution? -> The job pins the template version at scheduling time and warns if a newer version exists.  
- What if the SFTP source is unavailable during a scheduled run? -> The job retries with backoff and alerts after retry budget is exhausted.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support CSV and Excel uploads with template-based column mapping and type conversion.
- **FR-002**: System MUST validate incoming files for required fields, data types, and business rules prior to import execution.
- **FR-003**: System MUST allow configuration of recurring imports (e.g., nightly SFTP) with schedules, credentials, and source locations stored securely.
- **FR-004**: System MUST provide real-time progress tracking (processed/total, success/failure counts, ETA) for each import job.
- **FR-005**: System MUST support resumable processing so partial progress persists across restarts without duplicating committed rows.
- **FR-006**: System MUST detect duplicate records using configurable keys and provide resolution actions (accept, merge, reject).
- **FR-007**: System MUST generate detailed error reports (CSV) for failed rows with reasons and make them available for download and notification.
- **FR-008**: System MUST enforce tenant isolation for all import templates, jobs, files, and resulting records.
- **FR-009**: System MUST emit events for job started, completed, failed, rollback executed, and state test imports processed.
- **FR-010**: System MUST support rollback of recent imports within a defined window, restoring previous state and auditing the action.
- **FR-011**: System MUST provide audit logs for uploads, validations, imports, duplicate resolutions, and rollbacks including actor and timestamp.
- **FR-012**: System MUST expose history and analytics for imports (volume, failure reasons, duration) to inform tuning and operational practices.

### Key Entities

- **Import Template**: Mapping and validation rules for a specific file type, including versioning.
- **Import Job**: Execution instance with status, progress metrics, source reference, and history.
- **File Artifact**: Uploaded or retrieved file with metadata (size, checksum, source).
- **Row Result**: Per-row validation and processing outcome with success/failure reasons.
- **Duplicate Candidate**: Records flagged for potential duplication with resolution decision.
- **Schedule**: Recurring configuration for fetching and running imports.
- **Error Report**: Generated CSV detailing failed rows and reasons.
- **Audit Record**: Immutable log of actions taken during import lifecycle.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% of uploads of up to 10MB complete validation in under 5 seconds.
- **SC-002**: Scheduled imports start within 2 minutes of their trigger time for 99% of runs.
- **SC-003**: Import processing sustains at least 100 records per second with an overall error rate below 2% for valid files.
- **SC-004**: 99% of rollback requests execute successfully within 5 minutes and restore pre-import state.
- **SC-005**: 100% of import jobs produce audit entries and, when applicable, downloadable error reports.
