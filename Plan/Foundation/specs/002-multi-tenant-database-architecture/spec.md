# Spec: Multi-Tenant Database Architecture

Short Name: multi-tenant-db-architecture
Layer: Foundation
Status: Draft (Specification)
Version: 0.1.0
Created: 2025-11-20

## Feature
Database-per-service with strict multi-tenant isolation enforced via PostgreSQL Row-Level Security (RLS) + application tenant context propagation.

## Business Value
Consolidates hundreds of per-district databases into 11 multi-tenant service databases, reducing infrastructure complexity and cost while enabling modern observability and performance optimizations.

## Target Layer
Foundation

## Actors
- District Staff (Teachers / Admins)
- Assessment Service
- Student Service
- Configuration Service
- Migration ETL Subsystem

## Assumptions
- PostgreSQL 16+ available via Aspire orchestration.
- EF Core 9 used with per-service DbContext.
- Tenant UUID established during district onboarding and stored in `configuration.Tenants`.
- JWT includes `tenant_id` claim validated at API Gateway.
- Redis available for caching tenant metadata (optional).

## Constraints
- No cross-tenant data leakage (audit enforced).
- RLS policies mandatory on all tenant-scoped tables.
- Performance SLO: P95 <100ms filtered queries, P95 <50ms tenant context set.
- Backups must preserve RLS policies.

## Scenarios

### Scenario 1: Student Record Created with Tenant Isolation
Given the Student Management Service uses a multi-tenant database
And a teacher from District A is authenticated
And the session includes `tenant_id = "district-a"`
When the teacher creates a new student record
Then the student record is inserted into the `student.Students` table
And the record includes `tenant_id = "district-a"`
And PostgreSQL Row-Level Security (RLS) is enforced
And the insert succeeds only if session tenant matches record tenant
And the student is only visible to users from District A

### Scenario 2: Query Filtering by Tenant Context
Given a staff member from District B is viewing the student list
And their session includes `tenant_id = "district-b"`
And the database contains students from District A and District B
When they query the students table
Then the database automatically applies tenant filtering
And only students with `tenant_id = "district-b"` are returned
And students from District A are never visible in the result set
And the query plan shows RLS policy enforcement
And the query completes within performance SLO (<100ms)

### Scenario 3: Cross-Tenant Access Prevention
Given a malicious user attempts to bypass tenant isolation
And they modify their request to include a different tenant_id
When they attempt to query student records
Then the API Gateway validates the JWT token tenant claim
And rejects requests where token tenant ≠ requested tenant
And the database RLS policy also enforces tenant filtering
And no cross-tenant data is exposed
And the security violation is logged to audit trail

### Scenario 4: Database Migration from Per-District to Multi-Tenant
Given the legacy system has separate databases for each district
And District A has database `NorthStar_District001`
And District B has database `NorthStar_District002`
When the data migration ETL process runs
Then students from `NorthStar_District001` are migrated to `northstar_students` with `tenant_id = "district-a"`
And students from `NorthStar_District002` are migrated to `northstar_students` with `tenant_id = "district-b"`
And all records include the appropriate tenant identifier
And data integrity is validated via reconciliation queries
And the migration is logged for audit purposes

### Scenario 5: LMS Service Creates Base Database Schema
Given the LMS Service is responsible for database schema management
When the LMS Service starts for the first time
Then it applies EF Core migrations to create base schema
And it creates schemas: `identity`, `student`, `staff`, `assessment`, etc.
And each table includes a `tenant_id` column
And Row-Level Security policies are created for each table
And appropriate indexes are created including `tenant_id`
And the schema version is recorded in migration history

### Scenario 6: Row-Level Security Policy Enforcement
Given a PostgreSQL table has RLS enabled
And the table has policy: `CREATE POLICY tenant_isolation ON students USING (tenant_id = current_setting('app.current_tenant')::uuid)`
When a database connection is established for a user session
Then the application sets the session variable: `SET app.current_tenant = '<tenant_uuid>'`
And all queries automatically filter by the session tenant
And INSERT operations require matching tenant_id
And UPDATE operations cannot change tenant_id
And DELETE operations only affect records matching session tenant

### Scenario 7: Multi-Service Database Access with Tenant Context
Given the Student Service and Assessment Service share tenant context
And a student from District A is enrolled
When the Assessment Service queries student data via API
Then the request includes the tenant context in the JWT token
And the Student Service validates the tenant claim
And returns only students matching the tenant context
And the Assessment Service can then create assessments for those students
And all assessment records include the same tenant_id

### Scenario 8: Database Backup and Restore with Multi-Tenancy
Given the database contains data for multiple districts
When a database backup is performed
Then all tenant data is included in the backup
And the backup includes all RLS policies and schemas
When a restore is performed
Then all tenant isolation policies are re-enabled
And tenant data remains isolated
And no cross-tenant data leakage occurs

### Scenario 9: Performance Optimization for Multi-Tenant Queries
Given the students table contains 500,000 records across 100 districts
And the table has an index on `(tenant_id, created_at)`
When a user queries students for their district (5,000 records)
Then the query uses the tenant_id index
And only scans rows for the specific tenant
And the query completes in <50ms (P95)
And the query plan shows efficient index usage
And no full table scan occurs

### Scenario 10: New District Onboarding
Given a new school district (District C) is being onboarded
When the district administrator creates the district in the system
Then a new tenant UUID is generated for District C
And the tenant is registered in the `configuration.Tenants` table
And initial configuration data is created with `tenant_id = "district-c"`
And RLS policies automatically isolate District C data
And no schema changes are required for the new tenant
And the district can immediately start using the system

### Scenario 11: Audit Trail with Tenant Context
Given all database operations are logged for compliance
When a staff member from District A creates a student record
Then an audit log entry is created
And the log includes: `tenant_id`, `user_id`, `action`, `entity_type`, `entity_id`, `timestamp`
And the audit log is also tenant-isolated
And only District A administrators can view District A audit logs
And the audit log is immutable and tamper-proof

### Scenario 12: Database Connection Pooling with Tenant Isolation
Given the application uses connection pooling
And multiple requests from different tenants are being processed
When a connection is retrieved from the pool
Then the `app.current_tenant` session variable is set for the request
And the connection is used only for that tenant's request
And the session variable is reset before returning to the pool
And no tenant data leaks between pooled connection uses
And connection pool efficiency is maintained

## Non-Functional Requirements
- Security: RLS + JWT validation + audit logging mandatory.
- Performance: Tenant-filtered queries optimized via composite indexes.
- Scalability: Support 500+ districts, millions of rows per table.
- Observability: Traces include tenant context; metrics grouped by tenant.
- Recovery: Backups restorable without losing isolation guarantees.

## Acceptance Criteria Summary
All scenarios above must pass automated integration tests: tenant isolation, performance benchmarks, audit integrity, backup/restore validation.

## Out of Scope
- Cross-district reporting aggregation (future reporting service feature).
- Data lake export pipelines.

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| RLS misconfiguration | Automated migration scripts + validation queries |
| Performance degradation | Index strategy + query plan monitoring |
| Connection leakage of tenant var | Middleware resets `app.current_tenant` post-request |
| Data migration mismatch | Reconciliation and sampling reports |

## Initial Roadmap
1. Implement tenant context propagation middleware.
2. Scaffold service DbContexts and migrations with `tenant_id` columns.
3. Add RLS enablement + policy creation migrations.
4. Implement repository pattern enforcing tenant filters.
5. Add performance tests + query plan baselines.
6. Implement audit logging with tenant context.
7. Validate backup/restore flow retains policies.

## Audit & Compliance
All tenant-affecting operations must emit audit events with tenant UUID, operation type, timestamp, and actor ID.

---
Generated manually (no speckit agent invocation available in this environment).