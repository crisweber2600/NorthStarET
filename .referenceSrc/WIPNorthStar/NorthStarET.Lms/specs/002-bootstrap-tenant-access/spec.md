# Feature Specification: Tenant-Isolated District Access

**Feature Branch**: `002-bootstrap-tenant-access`  
**Created**: 2025-10-22  
**Status**: Draft  
**Input**: User description: "Bootstrap the LMS with secure access, strict tenant isolation, and a way for a System Admin to delegate control to a District Admin. Prevent any cross-district data visibility from day one. After sign-in, the Seeded System Admin lands on District Management listing all districts with Create/Edit/Delete. Opening a district shows its District Suffix (email domain matcher) and a District Admins panel. The System Admin can add/edit/remove a district admin email, see Verified/Unverified, and Resend Invite. A District Admin, upon sign-in, never sees platform admin pages and lands on a District Home scoped to their district only. Owns District and DistrictAdmin aggregates, including invite + verification state. Enforces tenant isolation on every query and command. District Suffix is one per district, case-insensitive, and unique platform-wide. Idempotent district/admin create/update within a 10-minute window; invite resend is safe to repeat (latest wins). Emits high-level events (DistrictCreated/Updated/Deleted, DistrictAdminInvited/Verified). Must authenticate users and establish role: System Admin vs District Admin. Must CRUD districts; display and persist unique, case-insensitive suffix. Must CRUD district admin assignments; send invites; track verified/unverified; allow resend. Must restrict District Admins to their district; only System Admin can view all districts and delete districts. Must audit district changes, role assignments, invites, and verification transitions. No cross-district data visibility. Email provider/branding content out of scope. No student/teacher/course features in this slice."

## Clarifications

### Session 2025-10-22

- Q: How long should a District Admin invitation remain valid before expiring? → A: 7 days

### Session 2025-10-25

- Q: What should be the maximum API response time (P95) for district and admin management operations? → A: P95 < 500ms
- Q: What are the maximum expected scale limits for the platform? → A: No hard limits, design for horizontal scaling
- Q: What are the primary security threat scenarios this feature must defend against? → A: All of the above - Comprehensive security posture across all threat categories
- Q: How should the system handle email delivery failures when sending district admin invitations? → A: Retry with exponential backoff (3 attempts), then queue for later
- Q: When a district is deleted, what should happen to associated district admin records and their access? → A: Soft delete district, archive admin assignments, revoke access immediately

### Session 2025-10-28

- Q: What capabilities should the District Admin district home include? → A: Both dashboard + limited management

## User Scenarios & Testing _(mandatory)_

### User Story 1 - System Admin Manages Districts (Priority: P1)

The seeded System Admin signs in, lands on the District Management workspace, and can view, create, edit, and delete districts while maintaining tenant isolation and unique district suffixes.

**Why this priority**: Without district CRUD and isolation, no tenant structure exists, blocking every other workflow.

**Independent Test**: Validate by exercising the District Management page end-to-end with a System Admin persona, ensuring suffix uniqueness, audit capture, and district visibility controls.

**Acceptance Scenarios**:

1. **Given** the System Admin is on District Management https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-2&m=dev , **When** they press Create District https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-43&m=dev a modal to Create New District https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-67&m=dev apprears and the District Name is filled out https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-67&m=dev and the District Suffix is filled out https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-76&m=dev and the Create District button is clicked https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-69&m=dev, **Then** the district appears in the list with the provided suffix https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-31&m=dev and an audit record captures the creation.
2. **Given** a district exists https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-31&m=dev, **When** the pencil icon https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-31&m=dev is pressed and the Edit District Modal https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-278&m=dev shows up and the System Admin updates its name https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-291&m=dev or suffix https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-287&m=dev and the Update District https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-280&m=dev button is pressed, **Then** only one current record exists, the latest values persist, and the change history is logged once for that window.
3. **Given** a district exists with dependent admins, **When** the System Admin attempts to delete it, **Then** the system confirms impact, removes the district, cascades scoped access, and emits a district deletion event.

---

### User Story 2 - System Admin Delegates District Admins (Priority: P2)

The System Admin opens a district detail view, manages district admin email assignments, sends invitations, monitors verification status, and can resend or revoke admin access.

**Why this priority**: Delegation enables districts to self-manage; without it the System Admin remains a bottleneck.

**Independent Test**: Simulate invite lifecycle for a district from the detail view, covering add/edit/remove/resend behaviors and invite status transitions.

**Acceptance Scenarios**:

1. **Given** a System Admin on the District Management page https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-2&m=dev and they click the Manage Admins button https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-33&m=dev it takes you to the Manage Admins page https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-181&m=dev, **When** the System Admin adds a new First Name https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-285&m=dev Last Name https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-282&m=dev and email address https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-288&m=dev and the Send Invitiation button is pressed https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-314&m=dev , **Then** the system validates suffix alignment, stores the assignment with Unverified status https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-410&m=dev, sends an invitation, and records an audit entry.
2. **Given** an Unverified district admin exists https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-410&m=dev , **When** the System Admin resends an invite within 10 minutes https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=340-96&m=dev, **Then** the most recent invite supersedes prior ones, a resend event records, and the status remains Unverified until the admin accepts.
3. **Given** a verified district admin https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-403&m=dev, **When** the System Admin removes the assignment https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=340-99&m=dev, **Then** the admin loses access immediately, a removal event emits, and the audit log captures the change.


### User Story 4 - Platform Captures Governance Signals (Priority: P4)

The platform records auditable events and emits high-level domain events for district lifecycle changes and admin invite transitions to support downstream observability and compliance.

**Why this priority**: Traceability and downstream integrations rely on trustworthy events and audit trails from the first release.

**Independent Test**: Trigger create/update/delete and invite lifecycle actions, verify audit entries, and confirm the expected event notifications exist for each action.

**Acceptance Scenarios**:

1. **Given** any district or admin change occurs, **When** the action completes, **Then** an audit record captures actor, action, entity, timestamp, and scope.
2. **Given** district lifecycle or admin invite state changes, **When** the change is saved, **Then** the corresponding domain event (created, updated, deleted, invited, verified) emits with the correct payload for downstream consumers.

---

### Edge Cases

- Attempting to create or update a district with a suffix that matches an existing suffix (case-insensitive) must be rejected with guidance to choose another suffix.
- Rapid duplicate submissions (manual refresh, double-click, or API retry) within 10 minutes for district or admin creation/update must resolve to a single persisted outcome without conflicting records.
- Resending an invite multiple times must always mark the latest invite as active and prevent stale links from succeeding.
- District Admin attempting to access global or other-district routes must receive an access denied response without revealing restricted data.
- Removing the last District Admin from a district must warn the System Admin and require confirmation to avoid orphaned districts.
- Deleting a district with active admins must soft-delete the district record, archive all admin assignments with deletion timestamp, immediately revoke all district admin access tokens/sessions, and emit DistrictDeleted and DistrictAdminRevoked events for each affected admin.
- SQL injection attempts in district name or suffix fields must be sanitized and rejected with validation errors.
- XSS payloads in user-supplied content must be encoded before rendering to prevent script execution.
- CSRF attacks on state-changing operations must be blocked via anti-forgery tokens.
- Rate limiting must prevent abuse of invite resend and district creation endpoints (max 10 requests per minute per user).
- Concurrent update attempts on the same district or admin record must use optimistic concurrency control to prevent data loss.

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: The platform MUST authenticate users and determine whether they hold the System Admin or District Admin role before granting access.
- **FR-002**: The platform MUST route the System Admin to the District Management workspace immediately after sign-in.
- **FR-003**: The platform MUST present a paginated list of all districts to the System Admin, including district name, suffix, admin count, and verification summary.
- **FR-004**: The platform MUST allow the System Admin to create, edit, and delete districts, enforcing a single, case-insensitive suffix per district across the platform.
- **FR-005**: The platform MUST ensure district create and update actions are idempotent for 10 minutes by collapsing repeated submissions with the same payload into a single persisted district record.
- **FR-006**: The platform MUST restrict district deletion to the System Admin, require confirmation when admins or dependent data exist, soft-delete the district record, archive all associated district admin assignments, and immediately revoke access for all district admins while preserving audit trail and enabling potential recovery.
- **FR-007**: The platform MUST show district details, including suffix and District Admin assignments, when the System Admin opens a district.
- **FR-008**: The platform MUST allow the System Admin to add, edit, remove, and resend invitations for District Admin email addresses from the district detail view.
- **FR-009**: The platform MUST track each District Admin assignment’s status (Unverified, Verified, Revoked) and surface it in the UI.
- **FR-010**: The platform MUST ensure District Admin invitations align with the district’s suffix before sending and reject mismatched email domains.
- **FR-011**: The platform MUST treat District Admin invitation create and update actions as idempotent within a 10-minute window to prevent duplicate invitations.
- **FR-012**: The platform MUST send invitation or resend notifications to District Admins with resilient delivery handling (exponential backoff retry up to 3 attempts, then dead-letter queue), enforce a 7-day absolute expiration from initial invite creation on outstanding invites, and record the action for auditing.
- **FR-013**: The platform MUST restrict District Admin access to data and actions scoped strictly to their assigned district, blocking views of other districts or platform-wide administration.
- **FR-014**: The platform MUST provide a district home experience for District Admins that combines district dashboard insights (KPIs, pending invites, recent audit activity) with limited self-service management (invite/resend/revoke within their district only) while keeping System Admin tooling hidden.
- **FR-015**: The platform MUST emit domain events for DistrictCreated, DistrictUpdated, DistrictDeleted, DistrictAdminInvited, DistrictAdminVerified, and DistrictAdminRevoked actions.
- **FR-016**: The platform MUST maintain an audit trail capturing actor, action, entity, old/new values, timestamp, and district scope for district lifecycle, admin role assignment, invite, and verification activities.
- **FR-017**: The platform MUST prevent cross-district data visibility in queries, reports, and UI components for every authenticated user.
- **FR-018**: The platform MUST surface meaningful error messages to System Admins when actions are blocked (e.g., suffix conflicts, orphaning districts) without exposing sensitive details.

### Key Entities

- **District**: Represents a tenant grouping with attributes such as name, unique case-insensitive suffix, current admins, lifecycle timestamps (including soft-delete timestamp when archived), audit references, and deletion status for compliance and recovery.
- **DistrictAdmin Assignment**: Represents the relationship between a district and an admin email, storing invite status, verification timestamp, role scope, archived status (for soft-deleted districts), and audit linkage.
- **System Admin User**: Represents the seeded platform-wide administrator with privileges across all districts and visibility into audit logs and events.
- **Audit Record**: Captures immutable entries describing who performed an action, what changed, when it occurred, and which district or admin was affected.
- **Domain Event**: Abstract representation of high-level notifications (e.g., DistrictCreated) emitted for downstream processing and observability.

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: 100% of authenticated users land on the correct workspace for their role (System Admin or assigned District Admin) immediately after sign-in.
- **SC-002**: System Admins can create and publish a new district, including suffix and initial admin invite, in under 3 minutes end-to-end.
- **SC-003**: District Admin invite acceptance rate reaches at least 80% within 48 hours due to clear messaging and reliable resend flows, measured by tracking invite-sent events against verification-completed events in the audit trail.
- **SC-004**: No verified District Admin is able to access data or functions belonging to other districts during verification and isolation testing.
- **SC-005**: 100% of district lifecycle and admin invite actions produce corresponding audit entries and domain events available for review within 1 minute of the action.
- **SC-006**: 95th percentile API response time for district and admin management operations (create, update, delete, invite, resend) remains below 500ms under normal load conditions.

## Assumptions

- Seeded System Admin credentials already exist and are managed outside this feature's scope.
- Invitation delivery occurs via email using an existing messaging capability; template branding is deferred to future work.
- Verification flow includes an email link that District Admins complete without additional identity proofing in this slice.
- Downstream systems consuming domain events will be instrumented later; this feature ensures events are published with the required payloads.
- Platform architecture supports horizontal scaling with no hard limits on districts or admins per district; database and caching layers designed for cloud-native growth.

## Dependencies

- Centralized authentication service capable of issuing role-aware tokens or claims for System Admin and District Admin personas.
- Notification infrastructure capable of delivering email invitations and resends with traceability; must support retry with exponential backoff (3 attempts) and dead-letter queue for failed deliveries requiring manual intervention.
- Audit storage mechanism accessible to authorized administrators for compliance review.
