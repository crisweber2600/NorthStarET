# Multi-Tenant Database Architecture

**Feature**: Database-Per-Service with Multi-Tenant Isolation  
**Epic**: Phase 1 - Foundation Services  
**Business Value**: Consolidate 100s of district databases into 11 multi-tenant service databases

---

## Scenario 1: Student Record Created with Tenant Isolation

**Given** the Student Management Service uses a multi-tenant database  
**And** a teacher from District A is authenticated  
**And** the session includes `tenant_id = "district-a"`  
**When** the teacher creates a new student record  
**Then** the student record is inserted into the `student.Students` table  
**And** the record includes `tenant_id = "district-a"`  
**And** PostgreSQL Row-Level Security (RLS) is enforced  
**And** the insert succeeds only if session tenant matches record tenant  
**And** the student is only visible to users from District A

---

## Scenario 2: Query Filtering by Tenant Context

**Given** a staff member from District B is viewing the student list  
**And** their session includes `tenant_id = "district-b"`  
**And** the database contains students from District A and District B  
**When** they query the students table  
**Then** the database automatically applies tenant filtering  
**And** only students with `tenant_id = "district-b"` are returned  
**And** students from District A are never visible in the result set  
**And** the query plan shows RLS policy enforcement  
**And** the query completes within performance SLO (<100ms)

---

## Scenario 3: Cross-Tenant Access Prevention

**Given** a malicious user attempts to bypass tenant isolation  
**And** they modify their request to include a different tenant_id  
**When** they attempt to query student records  
**Then** the API Gateway validates the JWT token tenant claim  
**And** rejects requests where token tenant ≠ requested tenant  
**And** the database RLS policy also enforces tenant filtering  
**And** no cross-tenant data is exposed  
**And** the security violation is logged to audit trail

---

## Scenario 4: Database Migration from Per-District to Multi-Tenant

**Given** the legacy system has separate databases for each district  
**And** District A has database `NorthStar_District001`  
**And** District B has database `NorthStar_District002`  
**When** the data migration ETL process runs  
**Then** students from `NorthStar_District001` are migrated to `northstar_students` with `tenant_id = "district-a"`  
**And** students from `NorthStar_District002` are migrated to `northstar_students` with `tenant_id = "district-b"`  
**And** all records include the appropriate tenant identifier  
**And** data integrity is validated via reconciliation queries  
**And** the migration is logged for audit purposes

---

## Scenario 5: LMS Service Creates Base Database Schema

**Given** the LMS Service is responsible for database schema management  
**When** the LMS Service starts for the first time  
**Then** it applies EF Core migrations to create base schema  
**And** it creates schemas: `identity`, `student`, `staff`, `assessment`, etc.  
**And** each table includes a `tenant_id` column  
**And** Row-Level Security policies are created for each table  
**And** appropriate indexes are created including `tenant_id`  
**And** the schema version is recorded in migration history

---

## Scenario 6: Row-Level Security Policy Enforcement

**Given** a PostgreSQL table has RLS enabled  
**And** the table has policy: `CREATE POLICY tenant_isolation ON students USING (tenant_id = current_setting('app.current_tenant')::uuid)`  
**When** a database connection is established for a user session  
**Then** the application sets the session variable: `SET app.current_tenant = '<tenant_uuid>'`  
**And** all queries automatically filter by the session tenant  
**And** INSERT operations require matching tenant_id  
**And** UPDATE operations cannot change tenant_id  
**And** DELETE operations only affect records matching session tenant

---

## Scenario 7: Multi-Service Database Access with Tenant Context

**Given** the Student Service and Assessment Service share tenant context  
**And** a student from District A is enrolled  
**When** the Assessment Service queries student data via API  
**Then** the request includes the tenant context in the JWT token  
**And** the Student Service validates the tenant claim  
**And** returns only students matching the tenant context  
**And** the Assessment Service can then create assessments for those students  
**And** all assessment records include the same tenant_id

---

## Scenario 8: Database Backup and Restore with Multi-Tenancy

**Given** the database contains data for multiple districts  
**When** a database backup is performed  
**Then** all tenant data is included in the backup  
**And** the backup includes all RLS policies and schemas  
**When** a restore is performed  
**Then** all tenant isolation policies are re-enabled  
**And** tenant data remains isolated  
**And** no cross-tenant data leakage occurs

---

## Scenario 9: Performance Optimization for Multi-Tenant Queries

**Given** the students table contains 500,000 records across 100 districts  
**And** the table has an index on `(tenant_id, created_at)`  
**When** a user queries students for their district (5,000 records)  
**Then** the query uses the tenant_id index  
**And** only scans rows for the specific tenant  
**And** the query completes in <50ms (P95)  
**And** the query plan shows efficient index usage  
**And** no full table scan occurs

---

## Scenario 10: New District Onboarding

**Given** a new school district (District C) is being onboarded  
**When** the district administrator creates the district in the system  
**Then** a new tenant UUID is generated for District C  
**And** the tenant is registered in the `configuration.Tenants` table  
**And** initial configuration data is created with `tenant_id = "district-c"`  
**And** RLS policies automatically isolate District C data  
**And** no schema changes are required for the new tenant  
**And** the district can immediately start using the system

---

## Scenario 11: Audit Trail with Tenant Context

**Given** all database operations are logged for compliance  
**When** a staff member from District A creates a student record  
**Then** an audit log entry is created  
**And** the log includes: `tenant_id`, `user_id`, `action`, `entity_type`, `entity_id`, `timestamp`  
**And** the audit log is also tenant-isolated  
**And** only District A administrators can view District A audit logs  
**And** the audit log is immutable and tamper-proof

---

## Scenario 12: Database Connection Pooling with Tenant Isolation

**Given** the application uses connection pooling  
**And** multiple requests from different tenants are being processed  
**When** a connection is retrieved from the pool  
**Then** the `app.current_tenant` session variable is set for the request  
**And** the connection is used only for that tenant's request  
**And** the session variable is reset before returning to the pool  
**And** no tenant data leaks between pooled connection uses  
**And** connection pool efficiency is maintained

---

## Technical Implementation Notes

**Database Structure**:
```
northstar_students (PostgreSQL database)
├── Schema: student
│   ├── Students (tenant_id, id, first_name, last_name, ...)
│   ├── Enrollments (tenant_id, student_id, school_id, ...)
│   └── Demographics (tenant_id, student_id, ...)
├── RLS Policies: tenant_isolation_policy ON EACH TABLE
└── Indexes: (tenant_id, ...) on all tables
```

**Service Databases**:
1. `northstar_identity` - Users, Roles, Claims
2. `northstar_students` - Students, Enrollments, Demographics
3. `northstar_staff` - Staff, Teams, Assignments
4. `northstar_assessments` - Assessments, Results, Benchmarks
5. `northstar_interventions` - Groups, Attendance
6. `northstar_sections` - Sections, Rosters, Schedules
7. `northstar_configuration` - District Settings, Calendars, Schools
8. `northstar_dataimport` - Import Jobs, Validation Results
9. `northstar_reporting` - Reports, Aggregations
10. `northstar_media` - Files, Videos, Metadata

**Row-Level Security Example**:
```sql
-- Enable RLS on students table
ALTER TABLE student.students ENABLE ROW LEVEL SECURITY;

-- Create policy for tenant isolation
CREATE POLICY tenant_isolation ON student.students
  USING (tenant_id = current_setting('app.current_tenant')::uuid);

-- Create policy for INSERT
CREATE POLICY tenant_isolation_insert ON student.students
  FOR INSERT
  WITH CHECK (tenant_id = current_setting('app.current_tenant')::uuid);
```

**Application Code Pattern**:
```csharp
public async Task<Student> GetStudentAsync(Guid studentId)
{
    var tenantId = _httpContextAccessor.HttpContext.User
        .FindFirst("tenant_id").Value;
    
    // Set tenant context for this database connection
    await _dbContext.Database
        .ExecuteSqlRawAsync($"SET app.current_tenant = '{tenantId}'");
    
    // Query automatically filtered by RLS
    return await _dbContext.Students
        .FirstOrDefaultAsync(s => s.Id == studentId);
}
```

**Migration Impact**:
- **Before**: 100 districts × 11 databases = 1,100 databases to manage
- **After**: 11 multi-tenant databases (one per service)
- **Benefit**: 99% reduction in database infrastructure
- **Tenant Isolation**: Enforced at database layer via RLS + application layer via JWT validation

**Performance SLOs**:
- Query with tenant filter: <100ms (P95)
- Tenant context switch: <50ms
- RLS policy evaluation overhead: <5ms
- Multi-tenant insert: <50ms (P95)

**Security Requirements**:
- RLS enabled on all tables
- Tenant_id in composite primary keys where applicable
- Audit logging includes tenant context
- No cross-tenant joins permitted
- Regular security audits of RLS policies
