# Feature Specification: Configuration Service Migration

**Specification Branch**: `Foundation/007-configuration-service-migration-spec`  
**Implementation Branch**: `Foundation/007-configuration-service-migration`  
**Created**: 2025-11-20  
**Status**: Draft  
**Input**: User description: "Centralize multi-tenant district, school, calendar, settings, grading scales, compliance, custom attributes, and notification template management with hierarchical overrides and auditability."

---

## Layer Identification (MANDATORY)

**Target Layer**: Foundation

**Layer Validation Checklist**:
- [x] Layer explicitly identified (Foundation)
- [x] Layer exists in mono-repo structure (`Plan/Foundation/` and `Src/Foundation/`)
- [x] If new layer: Architecture Review documented in `Plan/{LayerName}/README.md` (Not a new layer)
- [x] Cross-layer dependencies justified and limited to approved shared infrastructure

**Cross-Layer Dependencies**: Foundation shared infrastructure (ServiceDefaults for events, shared Domain/Application patterns)  
**Justification**: Configuration is a core shared capability; only shared messaging and persistence patterns are needed to serve downstream services.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Manage tenant configuration with override hierarchy (Priority: P1)

District admins define or update tenant settings with system defaults and school-level overrides while ensuring downstream services consume resolved values.

**Why this priority**: Baseline capability that unblocks dependent services and ensures consistent behavior.  
**Independent Test**: Update a district-level setting, verify school inheritance or override, and confirm downstream resolution uses the expected value via API or event.

**Acceptance Scenarios**:

1. Given system defaults and no district overrides, when a district enables a setting, then the resolved value for all schools reflects the district change and is cached for reads.
2. Given a district override and an existing school override, when the school override is removed, then resolution falls back to the district value and publishes a change notification.
3. Given an update that fails validation (duplicate key), when saving settings, then the change is rejected with reasons and no audit record is written.

---

### User Story 2 - Configure calendars and academic structures (Priority: P2)

Administrators manage academic calendars, terms, grading periods, and non-instructional days tied to tenant and school context.

**Why this priority**: Calendars drive scheduling, attendance, and grading timelines.  
**Independent Test**: Create a calendar with terms and blackout dates, verify conflicts are detected, and ensure calendar resolutions are available to consuming services.

**Acceptance Scenarios**:

1. Given an existing school calendar, when adding a blackout date overlapping a session, then the system flags the conflict and prevents publishing until resolved.
2. Given district-wide term dates, when a school creates a custom term, then downstream consumers receive the school-specific term set while other schools remain unchanged.

---

### User Story 3 - Manage grading scales, custom attributes, and templates (Priority: P3)

District or school admins define grading scales, custom attributes, and notification templates with validation and audit history.

**Why this priority**: Enables assessment and intervention services to apply consistent grading and extend entities safely.  
**Independent Test**: Create a grading scale, add a custom student attribute, and publish a notification template; verify retrieval, validation rules, and audit entries.

**Acceptance Scenarios**:

1. Given a new grading scale, when score thresholds overlap, then the system blocks publication and returns a validation error.
2. Given a custom attribute name already used at district level, when a school tries to create the same key, then creation is rejected to prevent collisions.
3. Given a template update, when saved, then audit history stores before/after values with actor and timestamp.

### Edge Cases

- What happens when cache contains stale values after concurrent updates? -> Cache is invalidated on write and resolves from source on next read.  
- How does the system handle missing hierarchical values? -> Falls back deterministically from school to district to system default or returns an explicit "not configured" error.  
- What if a school is reassigned to another district? -> Triggers re-evaluation of applicable settings and purges inherited overrides for the previous district.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow tenant-scoped creation and update of configuration settings with validation of keys and value types.
- **FR-002**: System MUST resolve effective settings using the hierarchy system default -> district -> school with deterministic precedence.
- **FR-003**: System MUST provide APIs for read-mostly access returning resolved values within p95 <50ms under normal load.
- **FR-004**: System MUST publish change notifications when settings, calendars, grading scales, or templates are created or updated.
- **FR-005**: System MUST enforce tenant isolation on all configuration read/write operations.
- **FR-006**: System MUST support calendar management (terms, sessions, blackout dates) with conflict detection before activation.
- **FR-007**: System MUST validate grading scales to prevent overlapping ranges and ensure alignment with district policies.
- **FR-008**: System MUST allow definition of custom attributes per tenant with uniqueness across district and schools.
- **FR-009**: System MUST provide versioned notification/email templates with merge-field validation and preview.
- **FR-010**: System MUST record immutable audit entries for create/update/delete actions including actor, timestamp, previous value, and new value.
- **FR-011**: System MUST expose configuration search and filter capabilities scoped by tenant and entity type.
- **FR-012**: System MUST invalidate caches on writes and ensure eventual consistency for consuming services.

### Key Entities

- **Tenant Configuration**: Collection of resolved settings (system, district, school) including metadata such as scope and version.
- **Academic Calendar**: Terms, sessions, and blackout dates tied to district or school context.
- **Grading Scale**: Ordered bands with thresholds and labels, scoped to district or school.
- **Custom Attribute Definition**: Tenant-defined field metadata (name, scope, validation rules).
- **Notification Template**: Communication templates with merge fields and version history.
- **Audit Record**: Immutable change history capturing actor, timestamp, entity, and before/after values.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% of configuration read requests return in under 50ms with correct resolved value.
- **SC-002**: 100% of configuration writes generate corresponding audit records with complete metadata.
- **SC-003**: Cache invalidation propagates within 5 seconds for 99% of write operations, verified by downstream reads.
- **SC-004**: Validation prevents conflicting calendars or grading scales with under 1% configuration errors reported post-launch.
- **SC-005**: Downstream services receive configuration change notifications within 30 seconds for 99% of updates.
