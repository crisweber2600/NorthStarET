# Data Model: Staff Management Service
Layer: Foundation
Version: 0.1.0

## StaffMember
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid | Isolation |
| LegacyId | int | nullable |
| FirstName | text |  |
| LastName | text |  |
| Email | text | unique |
| Phone | text | nullable |
| Role | text | Teacher, DeptChair, etc. |
| SubjectSpecialization | text | nullable |
| EmploymentStatus | text | Active, Inactive |
| HireDate | date |  |
| CreatedAt | timestamptz |  |
| UpdatedAt | timestamptz |  |
| DeletedAt | timestamptz | soft delete |

## StaffAssignment
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| StaffId | uuid |  |
| SchoolId | uuid |  |
| FtePercentage | decimal(3,2) | 0.00–1.00 |
| StartDate | date |  |
| EndDate | date | nullable |
| IsActive | bool |  |

## Team
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| TeamName | text |  |
| TeamLeadId | uuid |  |
| Purpose | text | nullable |
| MeetingSchedule | text | nullable |
| CreatedAt | timestamptz |  |

## TeamMember
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| TeamId | uuid |  |
| StaffId | uuid |  |
| JoinedDate | date |  |
| Role | text | Member/Lead |

## Certification
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| StaffId | uuid |  |
| CertificationType | text |  |
| IssueDate | date |  |
| ExpirationDate | date |  |
| IssuingAuthority | text | nullable |
| Status | text | Active / ExpiringSoon / Expired |

## Schedule (availability rows)
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| StaffId | uuid |  |
| DayOfWeek | int | 1-7 |
| StartTime | time |  |
| EndTime | time |  |
| ActivityType | text | Teaching / Planning / Meeting / Tutoring |

Indexes: StaffMember(TenantId, LastName), StaffAssignment(TenantId, StaffId), Team(TenantId, TeamName), Certification(TenantId, ExpirationDate), Schedule(TenantId, StaffId, DayOfWeek).

---
Draft model.