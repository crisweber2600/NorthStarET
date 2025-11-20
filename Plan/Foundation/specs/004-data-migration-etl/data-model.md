# Data Model: Data Migration ETL
Layer: Foundation
Version: 0.1.0

## Mapping Table
| Column | Type | Notes |
|--------|------|-------|
| EntityType | text | STUDENT, ASSESSMENT_RESULT, etc. |
| LegacyId | int | Original integer identity |
| NewUuid | uuid | Generated primary key |
| TenantId | uuid | Tenant context |
| CreatedAt | timestamptz | Timestamp |

## BatchState
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| EntityType | text |  |
| TenantId | uuid |  |
| LastLegacyId | int | High-water mark |
| Status | text | PENDING / RUNNING / COMPLETE / FAILED |
| StartedAt | timestamptz |  |
| CompletedAt | timestamptz | nullable |

## Staging Table Example (Students)
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | Final PK |
| TenantId | uuid |  |
| LegacyId | int |  |
| FirstName | text |  |
| LastName | text |  |
| BirthDate | date |  |
| CreatedAt | timestamptz |  |

Indexes: Mapping(NewUuid), Mapping(LegacyId, TenantId), BatchState(EntityType, TenantId).

## Integrity Validation Views
- View: `validation.student_count_parity` compares legacy vs new per tenant.
- View: `validation.orphan_enrollments` lists enrollments with missing student.

---
Draft model.