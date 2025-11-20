# Data Import & Integration Service Migration

**Feature**: Migrate Data Import from Monolith to Microservice  
**Epic**: Phase 3 - Secondary Domain Services (Weeks 17-22)  
**Service**: Data Import & Integration Service  
**Business Value**: Automated data imports, validation, and integration with external systems

---

## Scenario 1: CSV File Upload and Validation

**Given** a district needs to import student data via CSV  
**When** an administrator uploads a CSV file with 200 student records  
**Then** the Data Import Service validates the file format  
**And** checks for required columns: StudentID, FirstName, LastName, DOB, Grade  
**And** validates data types and formats  
**And** identifies 5 records with missing required fields  
**And** identifies 2 records with invalid date formats  
**And** displays validation errors before processing  
**And** allows administrator to fix errors and re-upload  
**And** only proceeds with import if validation passes

---

## Scenario 2: Excel File Import Processing

**Given** staff data is provided in Excel format  
**When** an administrator uploads a .xlsx file with multiple sheets  
**Then** the Data Import Service reads all sheets  
**And** identifies the "Staff" sheet for processing  
**And** maps Excel columns to database fields:
  - Column A "Employee ID" → StaffId
  - Column B "Full Name" → FirstName + LastName (split)
  - Column C "Email" → Email
  - Column D "Hire Date" → HireDate
**And** converts Excel date serial numbers to proper dates  
**And** handles merged cells and formatting  
**And** processes all 50 staff records  
**And** generates import report with success/failure counts

---

## Scenario 3: State Test Data Import from External System

**Given** state assessment results are available via SFTP  
**When** the automated import job runs nightly  
**Then** the Data Import Service connects to state SFTP server  
**And** downloads new assessment result files  
**And** validates file format against state specifications  
**And** parses fixed-width or CSV format as specified  
**And** maps state student IDs to internal IDs  
**And** creates assessment records in Assessment Service  
**And** publishes `StateTestDataImportedEvent`  
**And** moves processed files to archive folder  
**And** sends email notification of import completion

---

## Scenario 4: Data Mapping and Field Transformation

**Given** external systems use different field names and formats  
**When** importing from a SIS (Student Information System)  
**Then** the Data Import Service applies mapping rules:
  - "pupil_id" → StudentId
  - "grade_lvl" → GradeLevel (convert "K" to 0, "1" to 1, etc.)
  - "enroll_status" → Status (map codes: "A"→"Active", "I"→"Inactive")
  - "birth_dt" → DateOfBirth (convert MM/DD/YYYY to ISO format)
**And** transformation rules are configurable per import type  
**And** handles null values and default assignments  
**And** logs all transformations for audit purposes

---

## Scenario 5: Import Error Handling and Detailed Reporting

**Given** a large import file contains some invalid records  
**When** processing 1000 student records  
**And** 950 records are valid  
**And** 50 records have errors:
  - 20 missing required fields
  - 15 duplicate student IDs
  - 10 invalid grade levels
  - 5 invalid date formats
**Then** the Data Import Service:
  - Successfully imports the 950 valid records  
  - Skips the 50 invalid records  
  - Generates detailed error report with row numbers and error descriptions  
  - Exports failed records to separate CSV for correction  
  - Sends summary email: "950 imported, 50 failed"  
  - Allows re-import of corrected failed records

---

## Scenario 6: Scheduled and Automated Import Jobs

**Given** nightly imports are needed from multiple sources  
**When** an administrator configures scheduled imports:
  - "Student Enrollment" - Daily at 2:00 AM from SIS
  - "State Test Results" - Weekly on Mondays at 3:00 AM
  - "Attendance Data" - Daily at 6:00 AM from attendance system
**Then** the Data Import Service schedules jobs using cron expressions  
**And** executes jobs at specified times  
**And** monitors job status and execution time  
**And** retries failed jobs with exponential backoff  
**And** sends alerts for repeated failures  
**And** maintains job execution history

---

## Scenario 7: Import Template Management and Reusability

**Given** districts frequently import similar data  
**When** an administrator creates an import template:
  - Template Name: "Student Demographics Update"
  - Expected Columns: StudentID, Address, Phone, EmergencyContact
  - Validation Rules: Phone must be 10 digits, Address required
  - Target Service: Student Management Service
**And** saves the template  
**Then** the template is stored with tenant isolation  
**And** other administrators in the same district can reuse it  
**And** template includes field mappings and validation rules  
**And** reduces configuration time for repeated imports

---

## Scenario 8: Field Validation Rules and Business Logic

**Given** imported data must meet business requirements  
**When** defining validation rules for student import:
  - Grade level must be K-12  
  - Date of birth must be within reasonable range (5-20 years old for K-12)
  - Email must be valid format
  - Phone must be numeric and 10 digits
  - Student ID must be unique within tenant
**Then** the Data Import Service applies all validation rules  
**And** rejects records that violate any rule  
**And** provides specific error messages for each violation  
**And** allows administrators to override specific validations if authorized

---

## Scenario 9: Duplicate Detection During Import

**Given** import files may contain duplicate records  
**When** importing student data with duplicate detection enabled  
**Then** the Data Import Service checks for duplicates by:
  - Exact match on Student ID
  - Fuzzy match on Name + DOB (for potential duplicates)
**And** for exact duplicates, updates existing record instead of creating new  
**And** for potential duplicates, flags for manual review  
**And** generates duplicate report with match confidence scores  
**And** administrator can choose: Update, Skip, or Create New  
**And** all duplicate resolution decisions are logged

---

## Scenario 10: Import Audit Trail and Compliance

**Given** all data imports must be audited  
**When** any import job completes  
**Then** the Data Import Service logs:
  - Import job ID and name
  - User who initiated the import
  - Source file name and location
  - Start and end timestamps
  - Records processed, succeeded, failed
  - Error details for failed records
  - Target service and entities affected
**And** the audit log is immutable  
**And** supports compliance reporting  
**And** all logs are tenant-isolated  
**And** retention policies are enforced

---

## Scenario 11: Rollback Failed Imports

**Given** an import job partially completes before failing  
**And** 600 out of 1000 records were imported before error  
**When** administrator triggers rollback  
**Then** the Data Import Service:
  - Identifies all records created by the failed import job  
  - Marks them for deletion or rollback  
  - Publishes `ImportRollbackEvent` to all affected services  
  - Each service removes records created by that import  
  - Restores system to pre-import state  
  - Generates rollback report  
  - Allows re-import after fixing the issue

---

## Scenario 12: Import Job Monitoring and Real-Time Status

**Given** large imports take several minutes  
**When** an import job is running  
**Then** the Data Import Service provides real-time progress:
  - Current Status: "Processing"
  - Records Processed: 750/1000 (75%)
  - Estimated Time Remaining: 2 minutes
  - Success Count: 720
  - Error Count: 30
**And** administrator can view live progress on dashboard  
**And** can cancel long-running imports if needed  
**And** receives notification when job completes  
**And** job history shows all past imports with status and timing

---

## Technical Implementation Notes

**Clean Architecture**:
```
NorthStar.DataImport/
├── Domain/
│   ├── Entities/
│   │   ├── ImportJob.cs
│   │   ├── ImportTemplate.cs
│   │   ├── ValidationRule.cs
│   │   └── ImportError.cs
│   ├── ValueObjects/
│   │   ├── ImportJobId.cs
│   │   └── FileMapping.cs
│   └── Events/
│       ├── ImportStartedEvent.cs
│       ├── ImportCompletedEvent.cs
│       └── ImportFailedEvent.cs
├── Application/
│   ├── Commands/
│   │   ├── StartImportCommand.cs
│   │   └── RollbackImportCommand.cs
│   ├── Queries/
│   │   ├── GetImportJobQuery.cs
│   │   └── GetImportHistoryQuery.cs
│   ├── Importers/
│   │   ├── CsvImporter.cs
│   │   ├── ExcelImporter.cs
│   │   └── SftpImporter.cs
│   └── Validators/
│       └── DataValidator.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── ImportDbContext.cs
│   │   └── ImportRepository.cs
│   ├── FileProcessing/
│   │   └── FileParser.cs
│   └── Integration/
│       └── EventPublisher.cs
└── Api/
    └── Controllers/
        ├── ImportsController.cs
        └── TemplatesController.cs
```

**Database Schema**:
```sql
CREATE TABLE dataimport.import_jobs (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    job_name VARCHAR(200) NOT NULL,
    import_type VARCHAR(100),
    source_file VARCHAR(500),
    status VARCHAR(50),
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    total_records INTEGER,
    success_count INTEGER,
    error_count INTEGER,
    initiated_by UUID,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE dataimport.import_errors (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    job_id UUID NOT NULL,
    row_number INTEGER,
    error_message TEXT,
    field_name VARCHAR(100),
    field_value TEXT,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE dataimport.import_templates (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    template_name VARCHAR(200) NOT NULL,
    target_entity VARCHAR(100),
    field_mappings JSONB,
    validation_rules JSONB,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE dataimport.import_audit_log (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    job_id UUID NOT NULL,
    action VARCHAR(100),
    details JSONB,
    timestamp TIMESTAMPTZ NOT NULL
);

-- Row-Level Security
ALTER TABLE dataimport.import_jobs ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON dataimport.import_jobs
    USING (tenant_id = current_setting('app.current_tenant')::uuid);
```

**API Endpoints**:
- `POST /api/v1/imports` - Start new import
- `GET /api/v1/imports/{id}` - Get import job status
- `GET /api/v1/imports/{id}/errors` - Get import errors
- `POST /api/v1/imports/{id}/rollback` - Rollback import
- `GET /api/v1/imports/history` - Get import history
- `POST /api/v1/imports/templates` - Create import template
- `GET /api/v1/imports/templates` - List templates
- `POST /api/v1/imports/validate` - Validate file before import
- `GET /api/v1/imports/{id}/progress` - Get real-time progress

**Import Processing Pipeline**:
```csharp
public async Task<ImportResult> ProcessImport(ImportJob job)
{
    var records = await _fileParser.ParseFile(job.SourceFile);
    var validationResults = await _validator.ValidateAll(records);
    
    var successCount = 0;
    var errorCount = 0;
    
    foreach (var record in validationResults.ValidRecords)
    {
        try
        {
            await _targetService.CreateRecord(record);
            successCount++;
        }
        catch (Exception ex)
        {
            await LogError(job.Id, record, ex.Message);
            errorCount++;
        }
    }
    
    foreach (var error in validationResults.InvalidRecords)
    {
        await LogError(job.Id, error.Record, error.Message);
        errorCount++;
    }
    
    return new ImportResult
    {
        TotalRecords = records.Count,
        SuccessCount = successCount,
        ErrorCount = errorCount
    };
}
```

**Domain Events**:
- `ImportStartedEvent`
- `ImportCompletedEvent`
- `ImportFailedEvent`
- `ImportRollbackEvent`
- `ValidationErrorsDetectedEvent`

**Performance SLOs**:
- File validation: <5 seconds for 1000 records
- Import processing: 100 records/second
- Real-time progress updates: <1 second latency
- Duplicate detection: <10ms per record

**Security Requirements**:
- Only admins can configure imports
- File uploads scanned for malware
- Sensitive data encrypted at rest
- Import audit trail immutable
- Tenant isolation strictly enforced
