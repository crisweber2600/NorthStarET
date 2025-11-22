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

## Architectural Appendix

### Current State (Legacy)

**Location**: Multiple projects in legacy monolith  
**Framework**: .NET Framework 4.6  
**Database**: Per-district SQL Server databases

**Key Legacy Components**:
- `NS4.WebAPI/Controllers/ImportController.cs` - Manual CSV/Excel upload
- `NS4.BatchProcessor/StateTestImportJob.cs` - State test data batch import
- `NS4.DataEntry/` - Custom data entry forms
- Legacy tables: `ImportJobs`, `ImportErrors`, `ValidationRules`
- Synchronous import processing (blocks UI during large imports)
- Limited error handling and retry logic

**Legacy Limitations**:
- No asynchronous background processing
- Limited file format support (CSV/Excel only)
- Manual error correction required
- No automated data quality checks
- Tight coupling with student/staff services in single database
- No import status tracking or progress notifications

### Target State (.NET 10 Microservice)

#### Architecture

**Clean Architecture Layers**:
```
DataImport.API/                 # UI Layer (REST endpoints)
├── Controllers/
├── Middleware/
└── Program.cs

DataImport.Application/         # Application Layer
├── Commands/                  # Upload file, start import, retry failed
├── Queries/                   # Get import status, list imports
├── DTOs/
├── Interfaces/
└── Services/
    ├── FileParserService/     # Parse CSV, Excel, XML
    ├── ValidationService/     # Data quality checks
    └── MappingService/        # Field mapping and transformation

DataImport.Domain/             # Domain Layer
├── Entities/
│   ├── ImportJob.cs
│   ├── ImportRow.cs
│   ├── ImportError.cs
│   ├── ImportMapping.cs
│   └── ValidationRule.cs
├── Events/
│   ├── ImportStartedEvent.cs
│   ├── ImportCompletedEvent.cs
│   ├── ImportFailedEvent.cs
│   └── RowValidationFailedEvent.cs
└── ValueObjects/
    ├── ImportStatus.cs
    ├── FileFormat.cs
    └── ValidationLevel.cs

DataImport.Infrastructure/     # Infrastructure Layer
├── Data/
│   ├── DataImportDbContext.cs
│   └── Repositories/
├── Integration/
│   ├── EventPublisher.cs
│   ├── StudentServiceClient.cs
│   ├── StaffServiceClient.cs
│   └── AssessmentServiceClient.cs
├── FileStorage/
│   └── AzureBlobStorageService.cs  # Store uploaded files
└── BackgroundJobs/
    └── ImportProcessorJob.cs      # Hangfire background job
```

#### Technology Stack

- **Framework**: .NET 10, ASP.NET Core
- **Data Access**: EF Core 9 with PostgreSQL
- **File Storage**: Azure Blob Storage for uploaded files
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Background Processing**: Hangfire for asynchronous import jobs
- **Parsing**: CsvHelper, EPPlus (Excel), System.Xml (XML)
- **Orchestration**: .NET Aspire hosting

#### Owned Data

**Database**: `NorthStar_DataImport_DB`

**Tables**:
- ImportJobs (Id, TenantId, FileName, FileFormat, TargetEntity, Status, TotalRows, SuccessCount, ErrorCount, StartedAt, CompletedAt, UploadedBy)
- ImportRows (Id, TenantId, JobId, RowNumber, RawData, MappedData, Status, ErrorMessage)
- ImportMappings (Id, TenantId, Name, TargetEntity, FieldMappings, IsDefault)
- ValidationRules (Id, TenantId, TargetEntity, FieldName, RuleType, Parameters)
- ImportTemplates (Id, TenantId, Name, Description, FileFormat, SampleFilePath)

#### Service Boundaries

**Owned Responsibilities**:
- File upload and validation (CSV, Excel, XML, state test formats)
- Data parsing and transformation
- Field mapping configuration
- Data quality validation
- Asynchronous import processing (background jobs)
- Error logging and reporting
- Import job status tracking
- Import template management
- Scheduled/automated imports
- Rollback for failed imports

**Not Owned** (delegated to other services):
- Actual data storage → Student, Staff, Assessment Services
- Data validation rules specific to entities → Target services
- Business logic for created records → Target services

**Integration Pattern**:
- Data Import Service orchestrates imports but delegates actual record creation to target services
- Uses command/event pattern: Sends `CreateStudentCommand` to Student Service
- Target services validate and store data, publish events back

#### Domain Events Published

**Event Schema Version**: 1.0 (follows [domain-events-schema.md](../../CrossCuttingConcerns/architecture/domain-events-schema.md))

- `ImportStartedEvent` - When import job begins
  ```
  - JobId: Guid
  - TenantId: Guid
  - FileName: string
  - TargetEntity: string  // "Student", "Staff", "Assessment"
  - TotalRows: int
  - StartedBy: Guid
  - OccurredAt: DateTime
  ```

- `ImportCompletedEvent` - When import finishes successfully
  ```
  - JobId: Guid
  - TenantId: Guid
  - TotalRows: int
  - SuccessCount: int
  - ErrorCount: int
  - DurationSeconds: int
  - CompletedAt: DateTime
  - OccurredAt: DateTime
  ```

- `ImportFailedEvent` - When import fails
  ```
  - JobId: Guid
  - TenantId: Guid
  - FailureReason: string
  - FailedAt: DateTime
  - OccurredAt: DateTime
  ```

- `RowValidationFailedEvent` - When individual row fails validation
  ```
  - JobId: Guid
  - TenantId: Guid
  - RowNumber: int
  - ErrorMessage: string
  - RawData: Dictionary<string, string>
  - OccurredAt: DateTime
  ```

#### Domain Events Subscribed

- None (Data Import Service is a workflow orchestrator, doesn't react to domain events)

#### API Endpoints (Functional Intent)

**File Upload**:
- Upload CSV file → stores file, returns job ID
- Upload Excel file → parses and stores file
- Upload state test data → processes state-specific format

**Import Management**:
- Start import job → begins background processing
- Get import status → returns job progress
- List import jobs → returns job history with filters
- Retry failed import → reprocesses failed rows
- Cancel import job → stops running import

**Configuration**:
- Create import mapping → defines field mappings
- Get import templates → returns available templates
- Download sample template → returns template file

**Validation**:
- Validate file before import → dry-run validation
- Get validation errors → returns error list for job

#### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime
- **File Upload**: p95 < 2 seconds (for files up to 10MB)
- **Import Start**: p95 < 100ms (async job queued)
- **Import Processing** (500 rows): < 2 minutes
- **Import Processing** (5000 rows): < 10 minutes
- **Validation Only**: p95 < 30 seconds (for files up to 1000 rows)

#### Idempotency & Consistency

**Idempotency Windows**:
- Import job creation: 10 minutes (prevent duplicate uploads)
- Row processing: Uses unique identifiers (StudentID, StaffID) to prevent duplicates

**Consistency Model**:
- Eventually consistent (import is asynchronous)
- Strong consistency for job status updates
- Target services handle transactional consistency

#### Security Considerations

**Constitutional Requirements**:
- Enforce least privilege principle
- Sensitive student data encrypted at rest and in transit
- Secrets stored in Azure Key Vault only

**Implementation**:
- Only district administrators can import data
- Uploaded files stored in Azure Blob Storage with encryption
- Temporary files deleted after processing
- All imports logged for audit trail
- FERPA compliance for student data imports

#### Testing Requirements

**Constitutional Compliance**:
- Reqnroll BDD features before implementation
- TDD Red → Green with test evidence
- ≥ 80% code coverage

**Test Categories**:

1. **Unit Tests** (DataImport.UnitTests):
   - File parsing logic (CSV, Excel, XML)
   - Field mapping transformations
   - Validation rule execution
   - Error handling scenarios

2. **Integration Tests** (DataImport.IntegrationTests):
   - Database operations via EF Core
   - Azure Blob Storage file upload/download
   - Event publishing to message bus
   - Background job execution (Hangfire)
   - Cross-service integration (Student, Staff, Assessment)

3. **BDD Tests** (Reqnroll features):
   - `FileUpload.feature` - Upload and validate files
   - `DataImport.feature` - Execute import jobs
   - `ErrorHandling.feature` - Handle validation errors
   - `StateTestImport.feature` - Import state test data

4. **UI Tests** (Playwright):
   - File upload form
   - Import status dashboard
   - Error report display
   - Template download workflow

#### Dependencies

**External Services**:
- Student Management Service - Student record creation
- Staff Management Service - Staff record creation
- Assessment Service - Assessment result import
- Configuration Service - Validation rules
- Azure Blob Storage - File storage
- Azure Service Bus - Event publishing

**Infrastructure Dependencies**:
- PostgreSQL - Import job tracking database
- .NET Aspire AppHost - Service orchestration
- Hangfire - Background job processing

#### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 3a** (Week 17): Deploy new Data Import Service alongside legacy
   - Route new CSV imports to new service
   - Legacy imports continue in NS4.WebAPI and BatchProcessor
   - Both systems operational in parallel

2. **Phase 3b** (Week 18-19): Expand format support
   - Add state test data import
   - Add Excel import support
   - Migrate import templates and field mappings

3. **Phase 3c** (Week 20): Complete cutover
   - Route all import operations to new service
   - Decommission legacy NS4.BatchProcessor import jobs
   - Monitor error rates and performance

4. **Phase 3d** (Week 21-22): Enhanced features
   - Add automated scheduled imports
   - Add advanced validation rules
   - Add import analytics dashboard

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
