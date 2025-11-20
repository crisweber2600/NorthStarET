# Data Model: Student Management Service
Layer: Foundation
Version: 0.1.0

## Student
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid | Tenant isolation |
| LegacyId | int | Legacy mapping nullable |
| FirstName | text |  |
| LastName | text |  |
| DateOfBirth | date |  |
| GradeLevel | int | Normalized code |
| Status | text | Active, Withdrawn, Merged |
| PhotoUrl | text | Blob path |
| CreatedAt | timestamptz |  |
| UpdatedAt | timestamptz |  |
| DeletedAt | timestamptz | Soft delete |
| MergeReferenceId | uuid | Points to primary if merged |

## Enrollment
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| StudentId | uuid | FK to Student.Id |
| SchoolId | uuid |  |
| GradeLevel | int |  |
| EnrollmentDate | date |  |
| WithdrawalDate | date | nullable |
| Status | text | Active / Withdrawn |

## Idempotency Hash
| Column | Type | Notes |
|--------|------|-------|
| Hash | text | Unique creation fingerprint |
| TenantId | uuid |  |
| CreatedAt | timestamptz |  |

## Indexes
- Student: (TenantId, LastName), (TenantId, CreatedAt), (TenantId, Status)
- Enrollment: (TenantId, StudentId), (TenantId, SchoolId)

## Event Payload Minimal Fields
- StudentCreated: StudentId, TenantId, SchoolId, GradeLevel, Timestamp
- StudentMerged: PrimaryStudentId, SecondaryStudentId, TenantId, Timestamp

---
Draft model.