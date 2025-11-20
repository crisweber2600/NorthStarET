# Data Model: Multi-Tenant Database Architecture
Layer: Foundation
Version: 0.1.0

## Core Interfaces
```csharp
public interface ITenantEntity { Guid TenantId { get; set; } }
public interface IAuditable { DateTime CreatedAt { get; set; } DateTime? UpdatedAt { get; set; } }
public interface ISoftDelete { DateTime? DeletedAt { get; set; } }
```

## Student Aggregate (student schema)
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid | FK (tenant context) |
| LegacyId | int | Legacy mapping (nullable) |
| FirstName | text |  |
| LastName | text |  |
| BirthDate | date |  |
| CreatedAt | timestamptz | Auditable |
| UpdatedAt | timestamptz | Auditable |
| DeletedAt | timestamptz | Soft delete |

## Enrollment
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| StudentId | uuid | FK to Student.Id |
| SchoolId | uuid |  |
| GradeLevel | smallint |  |
| StartDate | date |  |
| EndDate | date | nullable |
| CreatedAt | timestamptz |  |

## AuditRecords (audit schema)
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| UserId | uuid |  |
| Action | text | e.g. INSERT_STUDENT |
| EntityType | text |  |
| EntityId | uuid |  |
| Timestamp | timestamptz |  |
| PayloadHash | text | Integrity check |

## Index Strategy
- Student: `(tenant_id, last_name)`, `(tenant_id, created_at)`.
- Enrollment: `(tenant_id, student_id)`.
- AuditRecords: `(tenant_id, timestamp)`.

## RLS Policy Template
```sql
ALTER TABLE student.students ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_select ON student.students
    USING (tenant_id = current_setting('app.current_tenant')::uuid);
CREATE POLICY tenant_isolation_mod ON student.students
    FOR ALL
    USING (tenant_id = current_setting('app.current_tenant')::uuid)
    WITH CHECK (tenant_id = current_setting('app.current_tenant')::uuid);
```

## Exceptions (Non-Tenant Tables)
- `configuration.GradeLevels` (global) – no TenantId column.

## Mapping Table (migration schema)
| Column | Type | Notes |
|--------|------|-------|
| EntityType | text | e.g. STUDENT |
| LegacyId | int | original PK |
| NewUuid | uuid | new PK |
| TenantId | uuid | context |

---
Draft data model; refine during implementation phase.