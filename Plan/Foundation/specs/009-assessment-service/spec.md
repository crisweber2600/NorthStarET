# Feature Specification: Assessment Service Migration

**Specification Branch**: `Foundation/009-assessment-service-migration-spec`  
**Implementation Branch**: `Foundation/009-assessment-service-migration`  
**Created**: 2025-11-20  
**Status**: Draft  
**Input**: User description: "Modernize the assessment lifecycle (definition, assignment, scoring, benchmarks, analytics, imports/exports, audit) with tenant isolation and event-driven integration."

---

## Layer Identification (MANDATORY)

**Target Layer**: Foundation

**Layer Validation Checklist**:
- [x] Layer explicitly identified (Foundation)
- [x] Layer exists in mono-repo structure (`Plan/Foundation/` and `Src/Foundation/`)
- [x] If new layer: Architecture Review documented in `Plan/{LayerName}/README.md` (Not a new layer)
- [x] Cross-layer dependencies justified and limited to approved shared infrastructure

**Cross-Layer Dependencies**: Foundation shared infrastructure (ServiceDefaults for messaging, shared Domain/Application patterns).  
**Justification**: Assessment is a core domain service; integration is limited to shared messaging and established tenant isolation patterns.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create assessments and assign to rosters (Priority: P1)

Educators or district admins create assessments with custom fields and assign them to rosters or groups.

**Why this priority**: Foundational flow enabling all subsequent scoring and analytics.  
**Independent Test**: Create an assessment with custom fields, assign it to a roster, and verify assignment events and visibility for targeted students.

**Acceptance Scenarios**:

1. Given a new assessment definition with required metadata, when saved, then it is persisted with versioning and an AssessmentCreated notification is emitted.
2. Given a roster selection, when the assessment is assigned, then each student receives an assignment entry and an assignment event is published.
3. Given invalid configuration (missing grading scale link), when saving, then the operation fails with validation errors and no assignments are created.

---

### User Story 2 - Record results and apply benchmarks (Priority: P2)

Authorized staff record results with rubric/weighted scoring and benchmark classification per student.

**Why this priority**: Delivers instructional insight and is key for compliance reporting.  
**Independent Test**: Record results for assigned students, verify benchmark category, and ensure auditing and notifications for score changes.

**Acceptance Scenarios**:

1. Given a student assignment, when a score is recorded with required fields, then the result is persisted, benchmark level is calculated, and a result event is published.
2. Given overlapping benchmark thresholds, when configuring benchmarks, then the system rejects the configuration until ranges are non-overlapping.
3. Given a corrected score, when updated, then the change is audited and downstream consumers receive the updated result event.

---

### User Story 3 - Analyze trends, imports, and exports (Priority: P3)

Staff view assessment trends, import state test data, and export authorized results for analysis.

**Why this priority**: Enables broader reporting, compliance, and data-sharing workflows.  
**Independent Test**: Import a state test file, generate a trend view for a student group, and export results while confirming performance targets and audit coverage.

**Acceptance Scenarios**:

1. Given a valid state test file, when imported, then records are validated, ingested, and a completed import event is emitted with a summary report.
2. Given a request for student trend data, when processed, then the service returns direction (improving/declining/stable) with supporting metrics within the defined latency.
3. Given an export request with filters, when executed, then only authorized, tenant-scoped results are included and the export is logged with requester details.

### Edge Cases

- What happens when assignment duplication is attempted for the same student and assessment? -> The system enforces idempotency and prevents duplicate active assignments.  
- How does the system handle partial imports with failures? -> Failed rows are reported with reasons, successful rows commit, and the job status reflects partial completion.  
- What if scoring configuration references a retired grading scale? -> The scoring attempt is blocked and prompts the user to select an active grading scale.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow creation and update of assessment definitions with custom fields, templates, and validation of required metadata.
- **FR-002**: System MUST support assigning assessments to rosters/groups and enforce idempotency to avoid duplicate assignments.
- **FR-003**: System MUST enforce tenant isolation for all assessment definitions, assignments, and results.
- **FR-004**: System MUST record assessment results with rubric/weighted scoring and compute benchmark classifications per student.
- **FR-005**: System MUST validate benchmark configurations to avoid overlapping ranges and ensure alignment with grading scales.
- **FR-006**: System MUST publish events for creation, assignment, result recording, and import completion/failure.
- **FR-007**: System MUST provide search and filtering for assessments and results by name, subject, grade, roster, date, and benchmark status.
- **FR-008**: System MUST support correction of scores with full audit history and change notifications.
- **FR-009**: System MUST ingest state test or external assessment files with schema validation, error reporting, and partial success handling.
- **FR-010**: System MUST provide export of authorized assessment results with requester metadata and audit logging.
- **FR-011**: System MUST calculate trend metrics (improving/declining/stable) and return them within defined performance targets.
- **FR-012**: System MUST provide configurable reminders for upcoming assessments and overdue submissions.

### Key Entities

- **Assessment Definition**: Template containing metadata, scoring rules, and optional custom fields.
- **Assessment Assignment**: Link between an assessment and a student or roster with due dates and status.
- **Assessment Result**: Recorded score with rubric/weighted details, benchmark classification, and audit info.
- **Benchmark**: Threshold definitions mapping scores to performance levels for a given assessment.
- **Assessment Template Library**: Reusable definitions scoped to a tenant.
- **Import Job**: External assessment ingestion run with status, success metrics, and error details.
- **Export Job**: Authorized export request with filters, requester info, and delivery details.
- **Audit Record**: Immutable history of changes to assessments, assignments, and results.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% of assessment creation and assignment operations complete in under 100ms.
- **SC-002**: 99% of result recordings compute benchmark classification and emit events within 2 seconds.
- **SC-003**: Trend queries for a class or roster return in under 200ms for 95% of requests.
- **SC-004**: Imports process at least 1,000 assessment records with an error rate below 2% per job and provide row-level error reporting.
- **SC-005**: 100% of create/update/delete operations on assessments, assignments, and results generate audit entries with actor and before/after values.
