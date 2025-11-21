# Contracts: Staff Management Service
Layer: Foundation

## REST APIs (proposed)
- `POST /api/staff` — create staff profile; triggers identity provisioning event.
- `GET /api/staff?search=...` — directory search with tenant/privacy filters.
- `GET /api/staff/{id}` — profile with assignments, teams, certifications.
- `PUT/PATCH /api/staff/{id}` — update profile fields.
- `POST /api/staff/{id}/assignments` — add/update assignment with FTE validation.
- `POST /api/staff/{id}/availability` — manage availability blocks.
- `POST /api/staff/{id}/teams/{teamId}` — add/remove memberships.
- `POST /api/staff/{id}/certifications` — create/update certifications.
- `POST /api/staff/import` — start bulk import; returns job id.

## Events
- `StaffCreated`, `StaffUpdated`, `StaffAssignmentChanged`, `StaffAvailabilityChanged`, `TeamMembershipChanged`, `CertificationExpiring`, `StaffImported`.
- Headers: `tenant_id`, `correlation_id`, `actor`.

## Validation/Contracts
- Tenant context required; RLS enforced in DB.
- Certification payload includes `expiry_date`, `reminder_window_days`.
- Import schema columns: `staff_number`, `first_name`, `last_name`, `email`, `role`, `school_id`, `fte`, `status`.

## Consumers
- Section & Roster: consumes assignments and availability for scheduling.
- Assessment/Intervention: consumes StaffCreated/Updated for roster linking and permissions.
