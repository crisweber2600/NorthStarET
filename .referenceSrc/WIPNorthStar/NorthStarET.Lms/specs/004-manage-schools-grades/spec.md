# Feature Specification: Manage Schools & Grades

**Feature Branch**: `004-manage-schools-grades`  
**Created**: October 28, 2025  
**Status**: Draft  
**Input**: User description: "District: Manage Schools & Grades

Goal / Why

Enable district admins to define the organizational structure of their district by managing schools and the grades each school serves. System admins need equivalent controls across all districts for setup, audits, and support.

Intended Experience / What

Within the district admin portal, a \"Schools & Grades\" section lists schools scoped to the current district (system admins can switch districts). Admins can add/edit/remove a school (name, optional code/notes) and open a school detail to manage its grades via a simple checklist/toggles. Lists support search and basic sort. After a change, the list/detail reflects updates within seconds; success and error states are clearly surfaced. Deleting a school asks for confirmation.

Service Boundary Outcomes

This feature owns the District->School->Grades catalog (no enrollment/rosters). All operations are hard-tenant isolated: district admins can only see/manage their district; system admins may select any district explicitly. Writes are idempotent for 10 minutes (retries return the same outcome). UI reflects eventual consistency within 2 seconds for create/update/delete. The service emits high-level events on school/grade changes and consumes none in this slice.

Functional Requirements

Must list schools for the active district; system admin must be able to change the active district.

Must create, update, and delete a school with validation (required name; unique per district).

Must display and modify the set of grades for a school.

Must show success/failure messages and prevent accidental deletes via confirmation.

Must enforce tenant isolation on every read/write.

Constraints / Non-Goals

No student/teacher/class section data or enrollment effects.

No bulk import/export in this slice.

No cross-district views for district admins."

## User Scenarios & Testing _(mandatory)_

### User Story 1 - Maintain District Schools (Priority: P1)

District admins open the Schools & Grades workspace for their district, review the school catalog, and create, edit, or remove entries while receiving immediate feedback.

**Why this priority**: Without an accurate school roster, the district cannot configure staff or grade coverage; this is prerequisite for all downstream onboarding.

**Figma Flow**: Skipped — No Figma (prompt to be authored)  
**Figma Prompt**: `specs/004-manage-schools-grades/figma-prompts/p1-maintain-district-schools-prompt.md`  
**Reqnroll Feature**: `features/manage-schools-grades/district-admin-manages-schools.feature`  
**Test Gates**:

- TDD: `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/Schools/SchoolCatalogServiceTests.cs`
- BDD: `features/manage-schools-grades/district-admin-manages-schools.feature`
- Playwright: `tests/ui/DistrictAdmin/manage-schools.spec.ts`  
  **Aspire Touchpoints**: District catalog application service, tenant-aware gateway, DistrictsPostgres resource, Service Defaults telemetry hooks

**Independent Test**: Execute the catalog TDD suite, run the Reqnroll feature for district school maintenance, and complete the Playwright journey of creating, updating, and deleting schools as a district admin.

**Acceptance Scenarios**:
1. **Given** a district admin with catalog permissions, **When** they open “School Management”:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-37&m=dev  **Then** they see the full list of schools: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-64&m=dev scoped to their district with search:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-124&m=dev and sort controls:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-138&m=dev.

2. **Given** the list view:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-37&m=dev, **When** they click "create a school":https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-76&m=dev, a create school modal shows up:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=231-162&m=dev and they fill out the school name:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=237-263&m=dev with a unique name and they check the grade levels the school has:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=237-260&m=dev, and they click "Create School" button:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=231-165&m=dev, **Then** the new school appears in the list:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-37&m=dev within 2 seconds with a success confirmation:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=254-1163&m=dev.

3. **Given** an existing school:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-64&m=dev, **When** they click on Edit icon of the school list:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=297-90&m=dev, a modal shows up:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=237-289&m=dev and they update the name:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=237-367&m=dev or optional code/notes:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=237-370&m=dev and they click "Update School" button:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=237-292&m=dev, **Then** a success confirmation appears:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=297-82&m=dev and the detail saves successfully and the list reflects the changes.

4. **Given** a school selected for removal (when they click the delete icon:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=297-91&m=dev of the school list:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-64&m=dev), and a delete confirmation modal appears:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=237-643&m=dev, **When** they confirm deletion by clicking the "Delete School" button:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=237-646&m=dev, **Then** the school is removed, the list refreshes within 2 seconds, and a success message is displayed:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=297-148&m=dev; when they click cancel button:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=237-734&m=dev, cancellations leave the school unchanged and go back to the list page:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=200-37&m=dev.

---

### User Story 2 - Configure School Grades (Priority: P1)

District admins open a school detail drawer, adjust the grades the school serves via checklist toggles, and save changes with clear status feedback.

**Why this priority**: Grade coverage drives downstream enrollment, reporting, and compliance; districts must finalize grade assignments before inviting staff.

**Figma Flow**: Skipped — No Figma (prompt to be authored)  
**Figma Prompt**: `specs/004-manage-schools-grades/figma-prompts/p1-configure-school-grades-prompt.md`  
**Reqnroll Feature**: `features/manage-schools-grades/district-admin-configures-grades.feature`  
**Test Gates**:

- TDD: `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/Schools/GradeAssignmentsServiceTests.cs`
- BDD: `features/manage-schools-grades/district-admin-configures-grades.feature`
- Playwright: `tests/ui/DistrictAdmin/configure-school-grades.spec.ts`  
  **Aspire Touchpoints**: District catalog application service, grade change domain events, DistrictsPostgres resource

**Independent Test**: Run unit tests covering grade assignment rules, execute the paired Reqnroll feature, and walk through the Playwright scenario toggling grades and saving updates.

**Acceptance Scenarios**:

1. **Given** a school detail view:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-982&m=dev **When** the admin selects or clears grade checkboxes:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-1005&m=dev and clicks "Save Changes" button:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-1006&m=dev, **Then** the displayed checklist updates instantly and the persisted grade set matches the selection within 2 seconds.
2. **Given** an attempted save:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-1006&m=dev, **When** no grades are selected, **Then** the admin receives a prompt to confirm serving no grades or cancel to restore the previous state:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-796&m=dev.
3. **Given** a successful save:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-1006&m=dev, **When** another admin opens the same school detail within 10 minutes:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-982&m=dev, **Then** they see the latest grade selections.
4. **Given** a school detail view:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-982&m=dev and no grades are selected, **When** a user clicks the first checkbox that becomes the minimum grade and the next checkbox clicked becomes the max grade for that school, **Then** all of the checkboxes between the min and max grades are checked.
5. **Given** a school detail view:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-982&m=dev, **When** a user looks at the grades that can be edited:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-1005&m=dev, **Then** they see a select all button next to each school type:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=254-1120&m=dev.
6. **Given** a school detail view:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-982&m=dev, **When** the user clicks on the "Select All Grades" button for a school type:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=313-143&m=dev, **Then** All of the grades in that school type are selected. 
---

### User Story 3 - System Admin District Oversight (Priority: P2)

System admins switch between districts to review and adjust school and grade configurations with the same safeguards provided to district admins.

**Why this priority**: Centralized teams must assist districts during onboarding, audits, and support escalations without violating tenant boundaries.

**Figma Flow**: Skipped — No Figma (prompt to be authored)  
**Figma Prompt**: `specs/004-manage-schools-grades/figma-prompts/p2-system-admin-oversight-prompt.md`  
**Reqnroll Feature**: `features/manage-schools-grades/system-admin-oversees-districts.feature`  
**Test Gates**:

- TDD: `tests/unit/NorthStarET.NextGen.Lms.Application.Tests/Districts/Schools/SystemAdminScopeServiceTests.cs`
- BDD: `features/manage-schools-grades/system-admin-oversees-districts.feature`
- Playwright: `tests/ui/SystemAdmin/manage-district-schools.spec.ts`  
  **Aspire Touchpoints**: District selector widget, tenant-aware gateway, DistrictsPostgres resource, audit logging pipeline

**Independent Test**: Validate cross-district access rules through the TDD suite, execute the Reqnroll scenarios for district switching, and run the Playwright flow that changes districts and edits schools.

**Acceptance Scenarios**:
1. **Given** a system admin user clicking "System Administrator":https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=313-59&m=dev on the top right nav in the school management page:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=313-44&m=dev, and going to the system admin view:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-816&m=dev **When** they switch the active district:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=313-194&m=dev by clicking the district selector dropdown:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=313-203&m=dev, **Then** the schools list and detail screens reload with the chosen district’s data in under 2 seconds:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-819&m=dev.
2. **Given** a system admin editing a school in another district:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-982&m=dev, **When** they save valid changes:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=247-1006&m=dev, **Then** the updates honor the same validations and success messaging as district admins:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=297-82&m=dev.
3. **Given** a system admin without selection:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=317-119&m=dev, **When** they attempt to access the school management page:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=313-44&m=dev, **Then** a district selection screen shows up:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=317-114&m=dev and they are prompted to choose a district before continuing:https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=317-50&m=dev

---

### User Story 4 - District Admin Operates Within Their District (Priority: P4)

An invited and verified District Admin signs in and lands on a district-scoped home experience that combines actionable insights with the ability to manage their district’s administrators, all without exposing platform-wide controls.

**Why this priority**: District Admins need immediate situational awareness and limited self-service to keep their district operational without relying on the System Admin for day-to-day changes.

**Independent Test**: Log in with a District Admin persona, confirm the landing dashboard surfaces district metrics, pending invites, and recent activity, while the limited management panel enables invite/resend/revoke within the district. Verify navigation, API calls, and data access are constrained to the assigned district.

**Acceptance Scenarios**:

1. **Given** a District Admin has completed verification and signs in, **When** the session completes authentication, **Then** they bypass System Admin pages and land on the District Admin Home where they see district-level KPIs (active admins, invite status counts, recent audit entries) and quick links confined to their district.
2. **Given** a District Admin is viewing the District Admin Home, **When** they use the limited management panel to invite a new admin, resend an invite, or revoke an existing admin, **Then** the action executes against their district only, enforces suffix validation, records an audit entry, and updates the dashboard summaries in real time.
3. **Given** a District Admin attempts to access another district’s management endpoint or global admin tooling, **When** the request is made, **Then** the system denies access, logs an audit event for the isolation violation attempt, and leaves their session on the district-scoped home.

---

### Edge Cases

- Duplicate school names within the same district trigger a descriptive validation error and preserve the draft input.
- Deleting a school with assigned grades surfaces a warning summarizing the impact and requires explicit confirmation.
- Concurrent edits to the same school detect stale versions, honor the most recent confirmed save, and notify later editors about the change.
- Grade configuration gracefully handles newly added grades in the master taxonomy by defaulting them to unchecked until explicitly enabled.
- Any request with tampered identifiers or cross-tenant access attempts returns an access error without exposing other district data.

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: The Schools & Grades workspace MUST list schools scoped to the active district and provide search by name/code plus alphabetical sort.
- **FR-002**: Authorized district admins MUST create a school by entering a required name (unique per district) and optional code/notes, with inline validation feedback.
- **FR-003**: Authorized users MUST edit school details and see the catalog update within 2 seconds of confirmation.
- **FR-004**: Deleting a school MUST require explicit confirmation and remove the school from the catalog within 2 seconds when confirmed.
- **FR-005**: Grade management MUST present a standardized checklist of available grades, apply changes immediately upon confirmation, and persist selections idempotently for 10 minutes.
- **FR-006**: The experience MUST surface clear success, warning, and error states for create, update, delete, and grade actions without exposing internal codes.
- **FR-007**: Tenant isolation MUST ensure district admins interact only with their district while system admins explicitly select a district before any data loads.
- **FR-008**: All catalog operations MUST emit a high-level change event (created, updated, deleted, grades-updated) for downstream consumers.
- **FR-009**: UI refresh mechanisms MUST honor the 2-second eventual consistency target for reflecting confirmed changes.
- **FR-010**: The system MUST capture who performed each catalog change, the timestamp, and the acting tenant for compliance audits.
- **FR-011**: Retry logic MUST prevent duplicate schools or grade assignments when operations are retried within the 10-minute idempotency window.
- **FR-012**: Access controls MUST block and log any cross-tenant or unauthorized attempts without leaking other tenant metadata.

### Key Entities

- **District**: The tenant context representing an education organization; stores display name and identifiers used for isolation and auditing.
- **School**: A district-managed site with name, optional code/notes, status, audit metadata, and associations to its owning district.
- **Grade Offering**: The relationship between a school and each grade level it serves, including grade identifier, effective timestamp, and audit trail for changes.

## Assumptions & Dependencies

- A centrally managed grade taxonomy (e.g., PK–12) exists and is consumed read-only by this feature.
- Identity, authorization, and tenant context switching are already provided by the platform (per Microsoft multitenancy guidance).
- Messaging infrastructure and audit pipelines are available to receive catalog change events.
- Existing eventual consistency refresh mechanisms can deliver list/detail updates within the 2-second target.
- No enrollment, roster, or invitation flows change in this feature; downstream systems react to emitted events.

## Success Criteria _(mandatory)_

### Measurable Outcomes

- **SC-001**: 95% of catalog changes reflect in the UI within 2 seconds during user acceptance testing.
- **SC-002**: 100% of catalog writes retried inside the 10-minute idempotency window complete without duplicate records in integration testing.
- **SC-003**: District admins can complete school and grade setup for a new district in under 15 minutes during pilot onboarding sessions.
- **SC-004**: Support tickets related to incorrect school or grade assignments drop by 60% within one month of release.
- **SC-005**: Automated unit, BDD, Playwright, and Aspire suites covering this feature sustain ≥80% code coverage with recorded Red → Green evidence.
