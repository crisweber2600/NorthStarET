# Plan: Staff Management Service Migration
Version: 0.1.0
Status: Draft (Planning)
Layer: Foundation
Spec Ref: 008-staff-management-service/spec.md

## Objectives
- Deliver staff microservice with profile, assignment, team, certification, schedule management.
- Integrate identity provisioning & event-driven downstream updates.
- Support multi-school FTE allocation & conflict detection.

## Architecture Components
1. API Layer – Controllers: Staff, Assignments, Teams, Certifications, Import, Directory.
2. Application – Commands (CreateStaff, UpdateStaffProfile, AssignToSchool, CreateTeam, AddTeamMember, AddCertification, RenewCertification, ImportStaff), Queries (GetStaff, SearchStaff, GetAssignments, GetSchedule, GetDirectory).
3. Domain – Entities: StaffMember, StaffAssignment, Team, TeamMember, Certification, Schedule; Events: StaffCreated, StaffAssignedToSchool, TeamCreated, TeamMemberAdded, CertificationExpiring.
4. Infrastructure – EF Core DbContext; Repositories; EventPublisher; IdentityServiceClient; CertificationMonitorJob (Hangfire or hosted service); AuditInterceptor.

## Multi-School Assignment Logic
- Validation ensures total FTE across active assignments ≤ 1.0.
- Switching context uses selected school assignment for UI scoping.

## Certification Monitoring
- Daily job queries certifications expiring within 60 days; publishes CertificationExpiringEvent & sends notification.

## Data Model Highlights
- StaffMember(TenantId, FirstName, LastName, Email, Role, Subject, EmploymentStatus, HireDate,...)
- StaffAssignment(StaffId, SchoolId, FtePercentage, StartDate, EndDate, IsActive)
- Certification(StaffId, Type, IssueDate, ExpirationDate, Status)

## Import Workflow
1. Parse CSV -> DTO list.
2. Validate each row (email uniqueness, required fields).
3. On success bulk insert + publish events.
4. Trigger identity provisioning asynchronous.

## Directory Privacy
- Exclude phone/email if privacy preference set; require admin override for sensitive fields.

## Event Payload Minimal Fields
- StaffCreated: StaffId, TenantId, Email, Role, Timestamp.
- StaffAssignedToSchool: AssignmentId, StaffId, SchoolId, FtePercentage, TenantId, Timestamp.

## Observability
- Metrics: staff_create_latency, staff_import_count, certification_expiring_count.
- Tracing: command handler spans; event publishing spans.

## Security
- RBAC check per endpoint; policy names (ViewStaff, ManageAssignments, ManageTeams, ManageCertifications).
- FERPA: teacher-student context enforced in Section Service; Staff Service provides assignment data.

## Testing Strategy
- Unit: FTE validation, certification expiration calculation, schedule conflict logic.
- Integration: CRUD + event publication, identity client mock.
- BDD: map all scenarios before implementation.
- Performance: search under load, import throughput test.

## Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| Identity provisioning lag | Delayed access | Poll & status endpoint |
| Large team membership updates | Slow writes | Batch operations + indexing |
| Schedule conflict complexity | Incorrect availability | Progressive enhancement after MVP |

## Completion Criteria
- All spec scenarios green.
- FTE and certification jobs passing tests.
- p95 search <100ms; import completes <60s for 50 staff.

---
Draft plan.