# Data Model: Staff Management Service

**Feature ID**: 008-staff-management-service  
**Database**: PostgreSQL (tenant-scoped with RLS)  
**Last Updated**: 2025-11-20

## Core Entities
- **StaffProfile**
  - Fields: `id`, `tenant_id`, `identity_user_id`, `staff_number`, `first_name`, `last_name`, `email`, `phone`, `role`, `status`, `privacy_flags`, `created_at`, `updated_at`.
  - Notes: `staff_number` unique per tenant; status controls visibility.

- **StaffAssignment**
  - Fields: `id`, `tenant_id`, `staff_id`, `school_id`, `role`, `fte` (decimal), `start_date`, `end_date`, `status`.
  - Notes: check constraint ensures `fte` >0 and cumulative FTE across active assignments <=1.0.

- **AvailabilityBlock**
  - Fields: `id`, `tenant_id`, `staff_id`, `day_of_week`, `start_time`, `end_time`, `location`, `notes`.
  - Notes: unique constraint prevents overlapping blocks per staff/day.

- **Team**
  - Fields: `id`, `tenant_id`, `name`, `scope` (District|School|CrossSchool), `school_id` (nullable), `created_at`.

- **TeamMembership**
  - Fields: `id`, `tenant_id`, `team_id`, `staff_id`, `role`, `start_date`, `end_date`.
  - Notes: history preserved; prevent duplicate active membership.

- **Certification**
  - Fields: `id`, `tenant_id`, `staff_id`, `name`, `issuing_body`, `issue_date`, `expiry_date`, `reminder_window_days`, `status`.
  - Notes: reminder jobs emitted when within reminder window.

- **ImportJob**
  - Fields: `id`, `tenant_id`, `source_file_uri`, `status`, `total_rows`, `succeeded_rows`, `failed_rows`, `error_report_uri`, `started_at`, `completed_at`.
  - Notes: coordinates with Data Import Service templates.

- **AuditRecord**
  - Fields: `id`, `tenant_id`, `entity_type`, `entity_id`, `action`, `actor`, `before`, `after`, `created_at`.

## Events
- `StaffCreated`, `StaffUpdated`, `StaffAssignmentChanged`, `StaffAvailabilityChanged`, `TeamMembershipChanged`, `CertificationExpiring`, `StaffImported`.

## Access Patterns
- Search index on `(tenant_id, last_name, first_name, email)`.
- Assignment lookups on `(tenant_id, staff_id, status, start_date)`.
- Team membership queries on `(tenant_id, team_id, end_date nulls last)` to fetch active members.

## Validation Rules
- Total active FTE per staff <= 1.0.
- Availability blocks cannot overlap for the same staff/day.
- Certification expiry must be in the future when status is Active; reminders scheduled at `expiry_date - reminder_window_days`.
