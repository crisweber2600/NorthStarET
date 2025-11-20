# Data Migration from Legacy to Multi-Tenant Architecture

**Feature**: Migrate 383 Entities from Per-District Databases to Multi-Tenant Service Databases  
**Epic**: Data Migration Strategy (Parallel with Phases 1-4)  
**Business Value**: Consolidate data, reduce infrastructure costs, enable modern architecture

---

## Scenario 1: Student Records Migration with Tenant Tagging

**Given** District A has 5,000 students in `NorthStar_District001` database  
**And** District B has 3,000 students in `NorthStar_District002` database  
**When** the student migration ETL process runs  
**Then** all 5,000 students from District A are inserted into `northstar_students.Students` with `tenant_id = 'district-a-uuid'`  
**And** all 3,000 students from District B are inserted with `tenant_id = 'district-b-uuid'`  
**And** each student retains their original `legacy_id` for reconciliation  
**And** student demographics, contacts, and enrollments are also migrated  
**And** all related records maintain the same `tenant_id`  
**And** the migration completes without data loss

---

## Scenario 2: Assessment Results Migration (Large Volume)

**Given** the legacy system has 5 million assessment results across all districts  
**And** the results span 15 years of historical data  
**When** the assessment migration runs in batches of 10,000 records  
**Then** results are migrated with proper `tenant_id` tagging  
**And** batch processing completes at rate of 5,000 records/second  
**And** PostgreSQL COPY command is used for bulk insert optimization  
**And** progress is logged after each batch  
**And** the migration can be resumed if interrupted  
**And** all 5 million records are migrated successfully

---

## Scenario 3: Foreign Key Relationship Transformation

**Given** the legacy database uses database foreign keys between Student and Enrollment  
**And** FK constraint: `FOREIGN KEY (StudentId) REFERENCES Students(Id)`  
**When** data is migrated to multi-tenant architecture  
**Then** foreign keys are replaced with application-level references  
**And** `Enrollments.StudentId` stores the student's UUID  
**And** the application validates referential integrity  
**And** Row-Level Security ensures same-tenant references only  
**And** cross-tenant references are prevented at application level

---

## Scenario 4: Dual-Write Pattern During Transition

**Given** the migration is in progress for District A  
**And** District A users are still using the legacy system  
**When** a teacher creates a new student in the legacy UI  
**Then** the record is written to legacy database `NorthStar_District001`  
**And** simultaneously written to new `northstar_students` with tenant tagging  
**And** both writes must succeed or both rollback  
**And** nightly reconciliation job validates data consistency  
**And** any discrepancies are logged and alerted

---

## Scenario 5: Data Validation and Reconciliation

**Given** migration has completed for all districts  
**When** the validation script runs  
**Then** it counts records in legacy database vs. new database per tenant  
**And** it samples 1,000 random records per district for field-by-field comparison  
**And** it validates all critical fields match (names, dates, IDs)  
**And** it checks for orphaned records (missing FK references)  
**And** it generates a reconciliation report showing:
  - Total records migrated per entity type
  - Count mismatches (if any)
  - Sample validation results
  - Orphaned record count
**And** the report confirms 100% data integrity

---

## Scenario 6: Historical Data Preservation

**Given** some student records date back 15 years  
**And** these records include archived enrollments and old assessments  
**When** historical data is migrated  
**Then** all timestamps are preserved accurately  
**And** created_at and updated_at values match legacy exactly  
**And** deleted/archived records are flagged appropriately  
**And** audit trail information is migrated  
**And** no historical context is lost

---

## Scenario 7: Identity and GUID Mapping

**Given** legacy uses integer identity columns (StudentId: 1, 2, 3...)  
**And** new system uses UUIDs for primary keys  
**When** migration creates new records  
**Then** a new UUID is generated for each record  
**And** the legacy ID is stored in `legacy_id` column  
**And** a mapping table is created: `migration.LegacyIdMapping(entity_type, legacy_id, new_uuid)`  
**And** this mapping supports bi-directional lookups during transition  
**And** external systems can still reference legacy IDs temporarily

---

## Scenario 8: Handling Data Type Conversions

**Given** legacy SQL Server uses `datetime` type  
**And** new PostgreSQL database uses `timestamptz` (with timezone)  
**When** datetime fields are migrated  
**Then** all times are converted to UTC  
**And** timezone offset is applied based on district location  
**And** the conversion preserves the original moment in time  
**And** date-only fields are migrated to `date` type  
**And** no precision is lost in the conversion

---

## Scenario 9: Incremental Migration by Service

**Given** the migration happens service by service over 20 weeks  
**When** Phase 1 completes (Identity, Configuration services)  
**Then** only Identity and Configuration data is migrated  
**And** Student data remains in legacy until Phase 2  
**When** Phase 2 begins (Student, Staff, Assessment)  
**Then** Student, Staff, and Assessment data is migrated  
**And** each service can begin using new database after its migration  
**And** unmigrated services continue using legacy databases

---

## Scenario 10: Rollback Scenario for Failed Migration

**Given** the Student migration for District C encounters errors  
**And** only 60% of student records were migrated  
**When** the rollback procedure is triggered  
**Then** the transaction is rolled back  
**And** all partially migrated records are deleted from new database  
**And** the legacy database remains unchanged  
**And** error logs are captured for investigation  
**And** the migration can be retried after fixing the issue  
**And** no data corruption occurs

---

## Scenario 11: Reference Data Migration (Lookup Tables)

**Given** the system has lookup tables (Grade Levels, Assessment Types, etc.)  
**And** some lookups are district-specific, others are system-wide  
**When** reference data is migrated  
**Then** system-wide lookups are created once without `tenant_id`  
**And** district-specific lookups include `tenant_id`  
**And** the application logic distinguishes between shared and tenant-specific  
**And** no duplicate lookups exist across tenants  
**And** reference data integrity is maintained

---

## Scenario 12: Handling Denormalized Data

**Given** the legacy Student table has denormalized demographics (50+ columns)  
**And** the new schema normalizes this into: Students, StudentDemographics, StudentContacts  
**When** denormalized data is migrated  
**Then** the ETL process splits data into appropriate tables  
**And** `Students` table has core fields only  
**And** `StudentDemographics` table has demographic fields  
**And** `StudentContacts` table has contact information  
**And** all three tables link via `student_id`  
**And** all three share the same `tenant_id`  
**And** no data is lost during normalization

---

## Technical Implementation Notes

**ETL Tool Architecture**:
```
DataMigration.Console/
├── Program.cs                          # Orchestration
├── Configuration/
│   └── MigrationSettings.json         # DB connections, batch sizes
├── Jobs/
│   ├── StudentMigrationJob.cs
│   ├── StaffMigrationJob.cs
│   ├── AssessmentMigrationJob.cs
│   └── ...
├── Mappers/
│   ├── StudentMapper.cs               # Legacy → New mapping
│   └── ...
├── Validators/
│   ├── DataIntegrityValidator.cs
│   └── ReconciliationReport.cs
└── Infrastructure/
    ├── LegacyDbContext.cs             # EF6 → EF Core
    ├── TargetDbContexts/              # Per-service contexts
    └── BatchProcessor.cs
```

**Migration Job Example**:
```csharp
public class StudentMigrationJob : IMigrationJob
{
    public async Task<MigrationResult> ExecuteAsync(string tenantId)
    {
        var batchSize = 1000;
        var offset = 0;
        
        while (true)
        {
            var batch = await _legacyContext.Students
                .OrderBy(s => s.Id)
                .Skip(offset)
                .Take(batchSize)
                .ToListAsync();
            
            if (!batch.Any()) break;
            
            var newStudents = batch.Select(s => new Student
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.Parse(tenantId),
                LegacyId = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                // ... map all fields
            }).ToList();
            
            await BulkInsertAsync(newStudents);
            offset += batchSize;
            
            _logger.LogInformation($"Migrated {offset} students");
        }
    }
}
```

**Bulk Insert Optimization (PostgreSQL COPY)**:
```csharp
public async Task BulkInsertAsync(List<Student> students)
{
    using var connection = new NpgsqlConnection(_connectionString);
    await connection.OpenAsync();
    
    using var writer = connection.BeginBinaryImport(
        "COPY student.students (id, tenant_id, legacy_id, first_name, last_name, ...) " +
        "FROM STDIN (FORMAT BINARY)");
    
    foreach (var student in students)
    {
        writer.StartRow();
        writer.Write(student.Id, NpgsqlDbType.Uuid);
        writer.Write(student.TenantId, NpgsqlDbType.Uuid);
        writer.Write(student.LegacyId, NpgsqlDbType.Integer);
        writer.Write(student.FirstName, NpgsqlDbType.Text);
        writer.Write(student.LastName, NpgsqlDbType.Text);
        // ...
    }
    
    await writer.CompleteAsync();
}
```

**Validation Query Example**:
```sql
-- Count validation
SELECT 
  'District A' as district,
  COUNT(*) as legacy_count
FROM [NorthStar_District001].dbo.Students
WHERE IsActive = 1;

SELECT 
  'District A' as district,
  COUNT(*) as new_count
FROM northstar_students.student.students
WHERE tenant_id = 'district-a-uuid'
  AND deleted_at IS NULL;

-- Should match!
```

**Migration Timeline**:
- **Weeks 6-10**: Identity & Configuration data
- **Weeks 9-16**: Student, Staff, Assessment data
- **Weeks 17-22**: Intervention, Section, Data Import historical data
- **Weeks 23-28**: Reporting, Media metadata

**Performance Targets**:
- Students: 10,000 records/second
- Assessment Results: 5,000 records/second
- Sections/Rosters: 15,000 records/second
- Total migration time: <72 hours per district

**Data Quality Requirements**:
- Zero data loss (100% record count match)
- 99.99% field accuracy (sample validation)
- Complete referential integrity
- All timestamps preserved accurately
- Audit trail maintained
