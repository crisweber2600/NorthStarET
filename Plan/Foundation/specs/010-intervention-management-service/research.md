# Research: Intervention Management Service Migration
Layer: Foundation
Version: 0.1.0

## Decisions
- **Model interventions with template + schedule generation**  
  - Rationale: supports recurring sessions and consistent configuration; aligns with spec for session generation on create.  
  - Alternatives: ad-hoc sessions only (more manual work, harder to audit).

- **Conflict detection using combined indexes and domain validation**  
  - Rationale: prevents overlapping sessions for facilitator/room/students before persistence; necessary for compliance.  
  - Alternatives: detect conflicts post-write (too late) or rely solely on DB constraints (complex).

- **Event-driven lifecycle (InterventionCreated, StudentEnrolled, AttendanceRecorded, ProgressUpdated, InterventionExited)**  
  - Rationale: downstream analytics and communication flows rely on timely updates; consistent with constitution.  
  - Alternatives: polling dashboards (laggy).

- **Communications via templates from Configuration service**  
  - Rationale: reuse approved templates and audit coverage; reduces duplication.  
  - Alternatives: inline templating (inconsistent with configuration governance).

## Open Questions
1. Auto-enrollment criteria from risk feeds—what signals and thresholds? Assume Assessment/Attendance feeds event triggers with filters defined per tenant.
2. Do caregiver communications require multi-channel support (email + SMS)? Templates should include channel metadata.
3. Are progress ratings standardized (Likert) or per-intervention custom? Default to standardized set with optional overrides.
