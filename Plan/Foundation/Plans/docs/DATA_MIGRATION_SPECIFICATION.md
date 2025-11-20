# Data Migration Specification

## Overview

This document defines the comprehensive data migration strategy for moving from OldNorthStar (.NET Framework 4.6 with SQL Server) to UpgradedNorthStar (.NET 10 microservices with PostgreSQL).

**Migration Scope**: 383 entity types, ~15 years of historical data, multi-district databases

---

## Migration Principles

1. **Zero Data Loss** - All historical data preserved
2. **Minimal Downtime** - Dual-write pattern during cutover
3. **Incremental Migration** - Service-by-service approach
4. **Validation First** - Extensive reconciliation before cutover
5. **Rollback Ready** - Ability to revert at any phase

---

## Source Database Analysis

### Legacy Schema (SQL Server)

**Database**: Per-district databases (e.g., `NorthStar_District001`, `NorthStar_District002`)

**DbContexts**:
- `DistrictContext` - Main application database (students, staff, assessments, sections)
- `LoginContext` - Cross-district authentication database

**Key Tables** (DistrictContext):
- Students: `Student`, `StudentContact`, `StudentDemographic`, `StudentEnrollment`
- Staff: `Staff`, `StaffDistrict`, `StaffRole`, `Team`, `TeamMember`
- Assessments: `Assessment`, `AssessmentField`, `AssessmentResult`, `Benchmark`
- Sections: `Section`, `SectionStudent`, `SectionStaff`, `SectionSchedule`
- Interventions: `InterventionGroup`, `InterventionStudent`, `InterventionAttendance`
- Configuration: `DistrictSettings`, `Calendar`, `CalendarEvent`, `School`, `Grade`
- Imports: `ImportJob`, `ImportError`, `StateTestData`

**Key Tables** (LoginContext):
- Authentication: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`

**Data Volumes** (estimated per district):
- Students: 5,000-50,000 records
- Staff: 200-2,000 records
- Assessments: 50-200 definitions, 500,000-5,000,000 results
- Sections: 500-5,000 per year, 15 years = 7,500-75,000
- Interventions: 1,000-10,000 groups

---

## Target Database Design (PostgreSQL)

### Database-Per-Service Pattern

Each microservice owns its PostgreSQL database:

| Service | Database Name | Primary Tables | Estimated Size |
|---------|---------------|----------------|----------------|
| Identity | `northstar_identity` | Users, Roles, Claims, RefreshTokens, ExternalProviderLinks | 10,000-100,000 users |
| Student | `northstar_students` | Students, Enrollments, Demographics, Contacts | 5,000-50,000 students |
| Staff | `northstar_staff` | Staff, StaffAssignments, Teams, TeamMembers | 200-2,000 staff |
| Assessment | `northstar_assessments` | Assessments, Fields, Results, Benchmarks | 500K-5M results |
| Intervention | `northstar_interventions` | Interventions, Groups, Attendance | 10,000-100,000 groups |
| Section | `northstar_sections` | Sections, Rosters, Schedules | 7,500-75,000 sections |
| Configuration | `northstar_configuration` | DistrictSettings, Calendars, Schools, Grades | 100-1,000 configs |
| DataImport | `northstar_dataimport` | ImportJobs, ImportErrors, ValidationResults | 1,000-10,000 jobs |
| Reporting | `northstar_reporting` | Reports, ReportTemplates, Aggregations (Read Models) | Variable |
| Media | `northstar_media` | Files, Videos, FileMetadata | 1,000-10,000 files |

### Schema Transformations

**Denormalization → Normalization**:
- Legacy `Student` table has 50+ columns → Split into `Students`, `StudentEnrollments`, `StudentDemographics`
- Legacy `Assessment` has embedded JSON → Extract to `AssessmentFields`, `AssessmentFieldGroups`

**Foreign Key Elimination**:
- Legacy: Database FKs between tables → New: Application-level references via Value Objects
- Example: `SectionStudent.StudentId FK` → `SectionRoster.StudentId (Guid)` with eventual consistency

**Data Type Changes**:
- Legacy: `int` identity columns → New: `uuid` primary keys
- Legacy: `datetime` → New: `timestamptz` (UTC)
- Legacy: `nvarchar(max)` → New: `text` or `jsonb` for structured data

---

## Entity Mapping

### Students Domain

| Legacy Entity | Legacy Table(s) | Target Service | Target Entity | Transformation Notes |
|---------------|-----------------|----------------|---------------|----------------------|
| Student | Student | Student Management | Student | Map int Id → Guid, preserve legacy Id for reconciliation |
| StudentContact | StudentContact | Student Management | StudentContact | Normalize phone/email types |
| StudentDemographic | StudentDemographic | Student Management | StudentDemographics | Extract race, ethnicity as value objects |
| StudentEnrollment | StudentEnrollment | Student Management | StudentEnrollment | Link to School via SchoolId (no FK) |

**Sample Mapping SQL**:
```sql
-- Legacy: Student table
SELECT 
    Id AS LegacyId,
    FirstName,
    MiddleName,
    LastName,
    StateStudentId,
    BirthDate,
    Gender,
    Grade,
    DistrictId,
    SchoolId,
    EnrollmentDate,
    WithdrawalDate,
    CreatedDate,
    ModifiedDate
FROM dbo.Student
WHERE IsActive = 1

-- Target: Students table (PostgreSQL)
INSERT INTO students (
    id, legacy_id, first_name, middle_name, last_name,
    state_student_id, date_of_birth, gender, grade_level,
    district_id, school_id, created_at, updated_at
)
VALUES (
    gen_random_uuid(), @LegacyId, @FirstName, @MiddleName, @LastName,
    @StateStudentId, @BirthDate, @Gender, @Grade,
    @DistrictId, @SchoolId, @CreatedDate, @ModifiedDate
);
```

### Staff Domain

| Legacy Entity | Legacy Table(s) | Target Service | Target Entity | Transformation Notes |
|---------------|-----------------|----------------|---------------|----------------------|
| Staff | Staff | Staff Management | Staff | Map int Id → Guid |
| StaffDistrict | StaffDistrict | Staff Management | StaffAssignment | Many-to-many → Assignment aggregate |
| Team | Team | Staff Management | Team | Preserve team structure |
| TeamMember | TeamMember | Staff Management | TeamMember | Link to Staff via StaffId |

### Assessment Domain

| Legacy Entity | Legacy Table(s) | Target Service | Target Entity | Transformation Notes |
|---------------|-----------------|----------------|---------------|----------------------|
| Assessment | Assessment | Assessment | Assessment | Extract field definitions to separate table |
| AssessmentField | AssessmentField | Assessment | AssessmentField | Normalize field types |
| AssessmentResult | AssessmentResult | Assessment | AssessmentResult | Preserve all historical results |
| Benchmark | Benchmark, BenchmarkGrade | Assessment | Benchmark | Merge benchmark/grade tables |

### Authentication Domain

| Legacy Entity | Legacy Table(s) | Target Service | Target Entity | Transformation Notes |
|---------------|-----------------|----------------|---------------|----------------------|
| AspNetUsers | AspNetUsers (LoginContext) | Identity | User | Migrate to Duende IdentityServer schema |
| AspNetRoles | AspNetRoles | Identity | Role | Map to new role structure |
| AspNetUserClaims | AspNetUserClaims | Identity | Claim | Preserve claims for authorization |

---

## Migration Tools & Technology

### ETL Framework

**Tool**: Custom .NET 10 console application

**Architecture**:
```
DataMigration.Console/
├── Program.cs                      # Main orchestration
├── Configuration/
│   └── MigrationSettings.json     # Connection strings, batch sizes
├── Jobs/
│   ├── StudentMigrationJob.cs
│   ├── StaffMigrationJob.cs
│   ├── AssessmentMigrationJob.cs
│   └── ... (one per service)
├── Mappers/
│   ├── StudentMapper.cs           # Legacy → New entity mapping
│   ├── StaffMapper.cs
│   └── ...
├── Validators/
│   ├── DataIntegrityValidator.cs  # Post-migration validation
│   └── ReconciliationReport.cs
└── Infrastructure/
    ├── LegacyDbContext.cs         # EF6 → EF Core adapter
    ├── TargetDbContexts/          # Per-service EF Core contexts
    └── BatchProcessor.cs          # Batch insert optimization
```

**Technology Stack**:
- .NET 10 Console Application
- EF Core 10 for database access
- Npgsql for PostgreSQL
- Serilog for logging
- Polly for retry policies
- Bogus for synthetic test data (validation)

### Migration Job Example

```csharp
public class StudentMigrationJob : IMigrationJob
{
    private readonly LegacyDbContext _legacyContext;
    private readonly StudentDbContext _targetContext;
    private readonly IMapper<LegacyStudent, Student> _mapper;
    private readonly ILogger<StudentMigrationJob> _logger;
    
    private const int BatchSize = 1000;
    
    public async Task<MigrationResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new MigrationResult { JobName = "Student Migration" };
        var totalProcessed = 0;
        var errors = new List<MigrationError>();
        
        try
        {
            var totalStudents = await _legacyContext.Students.CountAsync(cancellationToken);
            _logger.LogInformation("Starting migration of {Count} students", totalStudents);
            
            var offset = 0;
            while (true)
            {
                var batch = await _legacyContext.Students
                    .Include(s => s.Contacts)
                    .Include(s => s.Demographics)
                    .Include(s => s.Enrollments)
                    .OrderBy(s => s.Id)
                    .Skip(offset)
                    .Take(BatchSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                if (!batch.Any()) break;
                
                foreach (var legacyStudent in batch)
                {
                    try
                    {
                        var student = _mapper.Map(legacyStudent);
                        await _targetContext.Students.AddAsync(student, cancellationToken);
                        totalProcessed++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new MigrationError
                        {
                            EntityType = "Student",
                            LegacyId = legacyStudent.Id,
                            ErrorMessage = ex.Message
                        });
                        _logger.LogError(ex, "Failed to migrate student {Id}", legacyStudent.Id);
                    }
                }
                
                await _targetContext.SaveChangesAsync(cancellationToken);
                offset += BatchSize;
                
                _logger.LogInformation("Migrated {Processed}/{Total} students", totalProcessed, totalStudents);
            }
            
            result.TotalRecords = totalProcessed;
            result.Errors = errors;
            result.Success = errors.Count == 0;
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Student migration job failed");
            result.Success = false;
            result.FatalError = ex.Message;
            return result;
        }
    }
}
```

---

## Migration Phases

### Phase 1: Preparation (Week 1-2)

**Activities**:
1. **Schema Analysis**
   - Generate ER diagrams of legacy databases
   - Document table relationships
   - Identify data quality issues

2. **Migration Tool Development**
   - Build ETL console application
   - Create entity mappers for all domains
   - Implement validation logic

3. **Test Environment Setup**
   - Clone production database to staging
   - Setup target PostgreSQL databases
   - Configure connection strings

**Deliverables**:
- `docs/LEGACY_SCHEMA_ANALYSIS.md`
- ETL console application
- Test environment ready

### Phase 2: Identity & Configuration Migration (Week 3-4)

**Services**: Identity, Configuration

**Data Migration**:
1. Migrate `AspNetUsers` → `Users` (Identity DB)
2. Migrate `AspNetRoles` → `Roles`
3. Migrate `DistrictSettings` → `DistrictSettings` (Configuration DB)
4. Migrate `Calendar`, `School`, `Grade` → Configuration DB

**Validation**:
- Verify all users can authenticate
- Confirm district settings match legacy

**Dual-Write**: Not required (foundation services)

### Phase 3: Core Domain Migration (Week 5-12)

**Services**: Student, Staff, Assessment

**Data Migration Order**:
1. **Students** (Week 5-6)
   - Migrate Students table → `northstar_students`
   - Migrate StudentContacts, StudentDemographics
   - Migrate StudentEnrollments (link to School via Guid)

2. **Staff** (Week 7-8)
   - Migrate Staff table → `northstar_staff`
   - Migrate Teams, TeamMembers
   - Migrate StaffDistrict → StaffAssignments

3. **Assessments** (Week 9-12)
   - Migrate Assessment definitions → `northstar_assessments`
   - Migrate AssessmentFields, AssessmentFieldGroups
   - Migrate historical AssessmentResults (BULK - millions of records)
   - Migrate Benchmarks

**Dual-Write Strategy**:
- Activate dual-write after initial historical migration
- Write to both legacy SQL Server and new PostgreSQL
- Run nightly reconciliation jobs
- Monitor for discrepancies

**Dual-Write Implementation**:
```csharp
public class DualWriteStudentRepository : IStudentRepository
{
    private readonly StudentDbContext _newContext;
    private readonly LegacyDistrictContext _legacyContext;
    private readonly ILogger<DualWriteStudentRepository> _logger;
    
    public async Task<Student> AddAsync(Student student)
    {
        // Write to new PostgreSQL database
        await _newContext.Students.AddAsync(student);
        await _newContext.SaveChangesAsync();
        
        // Also write to legacy SQL Server
        try
        {
            var legacyStudent = MapToLegacy(student);
            _legacyContext.Students.Add(legacyStudent);
            await _legacyContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dual-write to legacy DB failed for student {Id}", student.Id);
            // Don't fail the request - new DB is source of truth
        }
        
        return student;
    }
}
```

### Phase 4: Secondary Domain Migration (Week 13-18)

**Services**: Intervention, Section, Data Import

**Data Migration**:
- Interventions: Migrate InterventionGroups, InterventionStudents, Attendance
- Sections: Migrate Sections, SectionStudents, SectionStaff, Schedules (15 years of history)
- Data Import: Migrate ImportJobs, ImportErrors (historical audit trail)

**Challenges**:
- Section history: 75,000+ records per large district
- Many-to-many relationships: SectionStudent, SectionStaff

**Optimization**:
- Bulk insert using `COPY` command (PostgreSQL)
- Partition large tables by academic year
- Archive old sections (>10 years) to cold storage

### Phase 5: Supporting Services Migration (Week 19-22)

**Services**: Reporting, Media

**Data Migration**:
- Reporting: Build read models from events (no historical data migration - regenerate from events)
- Media: Migrate file metadata, copy blobs from legacy Azure Storage to new storage account

**Media Migration**:
```bash
# Use AzCopy for blob migration
azcopy copy \
  "https://legacystorage.blob.core.windows.net/northstar-files?<SAS>" \
  "https://newstorage.blob.core.windows.net/media?<SAS>" \
  --recursive=true
```

---

## Data Validation Strategy

### Reconciliation Queries

**Student Count Validation**:
```sql
-- Legacy SQL Server
SELECT COUNT(*) AS LegacyCount FROM dbo.Student WHERE IsActive = 1;

-- New PostgreSQL
SELECT COUNT(*) AS NewCount FROM students WHERE deleted_at IS NULL;

-- Compare counts (should match)
```

**Assessment Results Validation**:
```sql
-- Compare total results by student
SELECT s.legacy_id, COUNT(ar.id) AS ResultCount
FROM students s
JOIN assessment_results ar ON ar.student_id = s.id
GROUP BY s.legacy_id
ORDER BY s.legacy_id;

-- Compare with legacy
SELECT StudentId, COUNT(*) AS ResultCount
FROM AssessmentResult
GROUP BY StudentId
ORDER BY StudentId;
```

### Automated Validation Tool

```csharp
public class DataIntegrityValidator
{
    public async Task<ValidationReport> ValidateStudentMigrationAsync()
    {
        var report = new ValidationReport();
        
        // Count validation
        var legacyCount = await _legacyContext.Students.CountAsync();
        var newCount = await _targetContext.Students.CountAsync();
        
        report.AddCheck("Student Count", legacyCount == newCount, 
            $"Legacy: {legacyCount}, New: {newCount}");
        
        // Sample data validation
        var sampleSize = 1000;
        var samples = await _legacyContext.Students
            .OrderBy(s => Guid.NewGuid())
            .Take(sampleSize)
            .ToListAsync();
        
        foreach (var legacyStudent in samples)
        {
            var newStudent = await _targetContext.Students
                .FirstOrDefaultAsync(s => s.LegacyId == legacyStudent.Id);
            
            if (newStudent == null)
            {
                report.AddError($"Student {legacyStudent.Id} not found in new database");
                continue;
            }
            
            // Field-by-field validation
            if (newStudent.FirstName != legacyStudent.FirstName)
                report.AddError($"Student {legacyStudent.Id}: FirstName mismatch");
            
            if (newStudent.LastName != legacyStudent.LastName)
                report.AddError($"Student {legacyStudent.Id}: LastName mismatch");
            
            // ... validate all critical fields
        }
        
        return report;
    }
}
```

---

## Rollback Plan

### Rollback Triggers

**Automatic Rollback Conditions**:
- Data validation failure rate >1%
- Migration errors >5% of records
- Critical service downtime >30 minutes

**Manual Rollback Decision**:
- Stakeholder approval required
- Based on business impact assessment

### Rollback Procedures

**Phase 1-2 Rollback** (Foundation services):
```bash
# 1. Stop new services
kubectl scale deployment identity-service --replicas=0
kubectl scale deployment configuration-service --replicas=0

# 2. Re-enable legacy services
# (No database changes needed - legacy still active)

# 3. Update API Gateway routes back to legacy
kubectl apply -f gateway-config-legacy.yaml
```

**Phase 3+ Rollback** (With dual-write active):
```bash
# 1. Disable dual-write in application
# (Set feature flag: UseDualWrite=false)

# 2. Route all traffic to legacy via Gateway
kubectl apply -f gateway-config-legacy.yaml

# 3. Stop new services
kubectl scale deployment student-service --replicas=0

# 4. Verify legacy database is up-to-date
# (Run reconciliation queries)

# 5. Archive new databases (don't delete - keep for forensics)
pg_dump northstar_students > rollback_backup_students.sql
```

---

## Performance Optimization

### Bulk Insert Optimization

**PostgreSQL COPY Command**:
```csharp
public async Task BulkInsertStudentsAsync(List<Student> students)
{
    var connectionString = _configuration.GetConnectionString("StudentsDb");
    
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    
    using var writer = connection.BeginBinaryImport(
        "COPY students (id, legacy_id, first_name, last_name, date_of_birth, grade_level, created_at) " +
        "FROM STDIN (FORMAT BINARY)");
    
    foreach (var student in students)
    {
        writer.StartRow();
        writer.Write(student.Id, NpgsqlDbType.Uuid);
        writer.Write(student.LegacyId, NpgsqlDbType.Integer);
        writer.Write(student.FirstName, NpgsqlDbType.Text);
        writer.Write(student.LastName, NpgsqlDbType.Text);
        writer.Write(student.DateOfBirth, NpgsqlDbType.Date);
        writer.Write(student.GradeLevel, NpgsqlDbType.Integer);
        writer.Write(student.CreatedAt, NpgsqlDbType.TimestampTz);
    }
    
    await writer.CompleteAsync();
}
```

**Performance Targets**:
- Students: 10,000 records/second
- Assessment Results: 5,000 records/second (more complex)
- Sections: 15,000 records/second

### Parallel Processing

```csharp
public async Task MigrateStudentsInParallelAsync()
{
    var totalStudents = await _legacyContext.Students.CountAsync();
    var batchSize = 1000;
    var parallelism = 4; // 4 concurrent batches
    
    var batches = Enumerable.Range(0, (int)Math.Ceiling((double)totalStudents / batchSize))
        .Select(i => new { Offset = i * batchSize, Limit = batchSize });
    
    await Parallel.ForEachAsync(batches, 
        new ParallelOptions { MaxDegreeOfParallelism = parallelism },
        async (batch, ct) =>
        {
            await MigrateBatchAsync(batch.Offset, batch.Limit, ct);
        });
}
```

---

## Migration Checklist

### Pre-Migration
- [ ] Legacy database backup created
- [ ] Target PostgreSQL databases provisioned
- [ ] ETL tool tested on sample data
- [ ] Validation queries prepared
- [ ] Dual-write code implemented and tested
- [ ] Rollback procedures documented and rehearsed
- [ ] Stakeholder approval obtained

### During Migration
- [ ] Migration started (log timestamp)
- [ ] Real-time monitoring dashboard active
- [ ] Error rate within acceptable threshold (<1%)
- [ ] Performance metrics captured
- [ ] Validation queries executed on sample
- [ ] Team on standby for issues

### Post-Migration
- [ ] Full data validation completed
- [ ] Reconciliation report generated
- [ ] Performance testing passed
- [ ] User acceptance testing passed
- [ ] Dual-write activated
- [ ] Monitoring dashboards showing health
- [ ] Rollback readiness confirmed

---

**Version**: 1.0  
**Last Updated**: November 15, 2025  
**Owner**: Data Migration Team  
**Status**: Specification Complete - Ready for Tool Development
