# Assessment Service Migration

**Feature**: Migrate Assessment Management from Monolith to Microservice  
**Epic**: Phase 2 - Core Domain Services (Weeks 9-16)  
**Service**: Assessment Service  
**Business Value**: Modern assessment management with analytics and benchmarking

---

## Scenario 1: Create Assessment Definition with Custom Fields

**Given** a district administrator wants to create a new assessment  
**And** they navigate to the "Create Assessment" form  
**When** they define the assessment:
  - Assessment Name: "Fall Reading Benchmark"
  - Subject: "Reading"
  - Grade Levels: K-5
  - Assessment Type: "Benchmark"
  - Scoring Method: "Points"
  - Maximum Score: 100
**And** they add custom fields:
  - "Fluency Score" (numeric, 0-100)
  - "Comprehension Score" (numeric, 0-100)
  - "Phonics Score" (numeric, 0-100)
**And** they submit the form  
**Then** the Assessment Service creates the assessment with `tenant_id`  
**And** publishes an `AssessmentCreatedEvent`  
**And** the assessment is available for assignment to students  
**And** custom fields are stored in the assessment schema

---

## Scenario 2: Assign Assessment to Students

**Given** an assessment "Fall Reading Benchmark" exists  
**And** a teacher wants to assign it to their 3rd grade class  
**When** they select the assessment and their class roster  
**Then** the Assessment Service creates assignment records for each student  
**And** publishes `AssessmentAssignedEvent` for each student  
**And** students appear in the teacher's "Pending Assessments" list  
**And** due dates are set based on the assessment schedule  
**And** the Student Service receives events to update student dashboards

---

## Scenario 3: Record Assessment Results with Automatic Scoring

**Given** a student "Emma Wilson" has been assigned "Fall Reading Benchmark"  
**And** a teacher is recording her results  
**When** they enter scores:
  - Fluency Score: 85
  - Comprehension Score: 78
  - Phonics Score: 92
**And** they submit the scores  
**Then** the Assessment Service calculates the total score: (85+78+92)/3 = 85  
**And** stores the results with `tenant_id` and student linkage  
**And** publishes `AssessmentResultRecordedEvent`  
**And** the result is immediately visible in reports  
**And** the Reporting Service updates performance dashboards

---

## Scenario 4: Benchmark Management and Grade-Level Standards

**Given** districts need to track performance against grade-level benchmarks  
**When** an administrator creates benchmarks for "3rd Grade Reading":
  - Advanced: ≥90
  - Proficient: 70-89
  - Basic: 50-69
  - Below Basic: <50
**And** saves the benchmark configuration  
**Then** the Assessment Service stores benchmarks with tenant isolation  
**And** automatically categorizes student results against benchmarks  
**And** a student scoring 85 is marked as "Proficient"  
**And** benchmark reports show distribution of students across levels

---

## Scenario 5: Assessment Scoring and Grading Calculation

**Given** different assessments use different scoring methods  
**When** a rubric-based assessment is scored:
  - Criteria 1: 4 (Exceeds), weight 30%
  - Criteria 2: 3 (Meets), weight 40%
  - Criteria 3: 4 (Exceeds), weight 30%
**Then** the Assessment Service calculates weighted score: (4×0.3 + 3×0.4 + 4×0.3) / 4 = 91.7%  
**And** converts to letter grade using Configuration Service grading scale  
**And** stores both numeric and letter grade  
**And** the calculation is auditable and explainable

---

## Scenario 6: Assessment Search and Filtering

**Given** a teacher needs to find specific assessments  
**When** they search with filters:
  - Subject: "Mathematics"
  - Grade Level: 5
  - Assessment Type: "Formative"
  - Date Range: "Last 30 days"
**Then** the search returns matching assessments  
**And** only assessments from the same tenant are visible  
**And** results include assessment name, date, and student count  
**And** the search completes within 100ms (P95)  
**And** results are sorted by most recent first

---

## Scenario 7: Assessment Analytics and Student Trends

**Given** a student has multiple assessment results over time  
**When** a teacher views the student's assessment history  
**Then** the Assessment Service aggregates all results for that student  
**And** calculates trends: improving, declining, stable  
**And** identifies strengths and weaknesses by subject/skill  
**And** provides comparison to class average and grade-level benchmarks  
**And** generates visual charts showing progress over time  
**And** the analysis updates in real-time as new results are added

---

## Scenario 8: Assessment Result Bulk Export

**Given** a principal needs assessment data for district reporting  
**When** they request an export for "All 5th Grade Math Assessments"  
**Then** the Assessment Service queries all matching results  
**And** formats data according to export specifications:
  - Student ID, Name, School, Grade
  - Assessment Name, Date, Score
  - Benchmark Level, Percentile
**And** generates a CSV file with proper encoding  
**And** the export respects tenant isolation  
**And** includes only authorized data based on user permissions  
**And** the export is logged for audit purposes

---

## Scenario 9: State Test Data Integration and Import

**Given** state test results are provided by the state department  
**And** results are in standardized format (e.g., CALPADS, PEIMS)  
**When** an administrator imports the state test data file  
**Then** the Assessment Service validates the file format  
**And** maps state student IDs to internal student IDs  
**And** creates assessment records for each state test  
**And** imports scores and performance levels  
**And** publishes `StateTestResultsImportedEvent`  
**And** generates import summary: 500 students, 0 errors  
**And** makes results available in state test reports

---

## Scenario 10: Assessment Template Library

**Given** teachers frequently use similar assessments  
**When** a teacher creates an assessment from a template:
  - Template: "Weekly Math Quiz"
  - Includes: Standard fields, rubric criteria, benchmark levels
**Then** the Assessment Service creates a new assessment based on template  
**And** pre-populates all fields from the template  
**And** allows customization before saving  
**And** the teacher can save their customized version as a new template  
**And** templates are shared within the district (tenant-scoped)

---

## Scenario 11: Assessment Scheduling and Reminders

**Given** assessments have scheduled administration dates  
**When** an assessment "Winter Benchmark" is scheduled for "January 15"  
**Then** the Assessment Service sends reminders to teachers:
  - 7 days before: "Upcoming assessment preparation"
  - 1 day before: "Assessment tomorrow"
  - On the day: "Assessment due today"
**And** tracks completion status for each teacher  
**And** sends follow-up reminders for incomplete assessments  
**And** administrators can view completion dashboard  
**And** automated escalation for overdue assessments

---

## Scenario 12: Assessment Audit Trail and Data Compliance

**Given** assessment data must be audited for compliance  
**When** any assessment record is modified  
**Then** an audit log entry is created with:
  - Who made the change
  - What was changed (before/after values)
  - When the change occurred
  - Why (if reason provided)
**And** score changes are flagged for review  
**And** administrators can generate compliance reports  
**And** the audit trail is immutable  
**And** all logs are tenant-isolated  
**And** FERPA compliance is maintained

---

## Architectural Appendix

### Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` project (legacy monolith)  
**Framework**: .NET Framework 4.6  
**Database**: Per-district SQL Server databases

**Key Legacy Components**:
- `NS4.WebAPI/Controllers/AssessmentController.cs` - Assessment definition CRUD
- `NS4.WebAPI/Controllers/ResultsController.cs` - Assessment result entry
- `NS4.WebAPI/Controllers/BenchmarkController.cs` - Benchmark management
- `NS4.WebAPI/Controllers/FieldsController.cs` - Custom assessment fields
- Legacy tables: `Assessments`, `AssessmentFields`, `AssessmentResults`, `Benchmarks`, `AssessmentAvailability`
- Complex custom field architecture with dynamic data types
- Tight coupling with student and reporting in single database

**Legacy Limitations**:
- No event-driven coordination with intervention services
- Limited state test data import automation
- Manual benchmark configuration
- Complex custom field management difficult to maintain
- Performance issues with large result datasets
- No real-time analytics or trend visualization

### Target State (.NET 10 Microservice)

#### Architecture

**Clean Architecture Layers**:
```
Assessment.API/                # UI Layer (REST endpoints)
├── Controllers/
├── Middleware/
└── Program.cs

Assessment.Application/        # Application Layer
├── Commands/                 # Create assessment, record result
├── Queries/                  # Get assessment, student trends
├── DTOs/
├── Interfaces/
└── Services/
    ├── ScoringEngine/        # Calculate weighted scores, rubrics
    ├── TrendAnalyzer/        # Student performance trends
    └── BenchmarkCalculator/  # Grade-level performance distribution

Assessment.Domain/            # Domain Layer
├── Entities/
│   ├── Assessment.cs
│   ├── AssessmentField.cs
│   ├── AssessmentAssignment.cs
│   ├── AssessmentResult.cs
│   ├── Benchmark.cs
│   └── ResultFieldValue.cs
├── Events/
│   ├── AssessmentCreatedEvent.cs
│   ├── AssessmentAssignedEvent.cs
│   ├── AssessmentResultRecordedEvent.cs
│   ├── BenchmarkCreatedEvent.cs
│   └── StateTestResultsImportedEvent.cs
└── ValueObjects/
    ├── Score.cs
    ├── BenchmarkLevel.cs
    ├── GradingScale.cs
    └── FieldType.cs

Assessment.Infrastructure/    # Infrastructure Layer
├── Data/
│   ├── AssessmentDbContext.cs
│   └── Repositories/
├── Integration/
│   ├── EventPublisher.cs
│   ├── StudentServiceClient.cs
│   └── ConfigurationServiceClient.cs  // Grading scales
├── Analytics/
│   └── TrendCalculationService.cs
└── StateTestImport/
    └── StateTestDataParser.cs  // CALPADS, PEIMS, etc.
```

#### Technology Stack

- **Framework**: .NET 10, ASP.NET Core
- **Data Access**: EF Core 9 with PostgreSQL (JSONB for dynamic field values)
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Caching**: Redis Stack for assessment definition lookups
- **Orchestration**: .NET Aspire hosting
- **Analytics**: Custom trend analysis with linear regression

#### Owned Data

**Database**: `NorthStar_Assessment_DB`

**Tables**:
- Assessments (Id, TenantId, AssessmentName, Subject, GradeLevels, AssessmentType, ScoringMethod, MaxScore, CreatedAt, CreatedBy, IsTemplate)
- AssessmentFields (Id, TenantId, AssessmentId, FieldName, FieldType, MaxValue, Weight, SortOrder)
- AssessmentAssignments (Id, TenantId, AssessmentId, StudentId, AssignedDate, DueDate, Status, AssignedBy)
- AssessmentResults (Id, TenantId, AssignmentId, StudentId, AssessmentId, TotalScore, BenchmarkLevel, CompletedDate, RecordedBy, FieldScores JSONB)
- Benchmarks (Id, TenantId, AssessmentId, GradeLevel, Subject, BenchmarkName, MinScore, MaxScore)
- AssessmentTemplates (Id, TenantId, TemplateName, Description, PrebuiltFields JSONB)

**JSONB Field Scores Structure**:
```json
{
  "fieldValues": [
    {
      "fieldId": "uuid",
      "fieldName": "Fluency Score",
      "value": 85,
      "maxValue": 100
    },
    {
      "fieldId": "uuid",
      "fieldName": "Comprehension Score",
      "value": 78,
      "maxValue": 100
    }
  ],
  "calculatedScore": 81.5,
  "benchmarkLevel": "Proficient"
}
```

#### Service Boundaries

**Owned Responsibilities**:
- Assessment definition and configuration
- Custom field management (flexible schema via JSONB)
- Assessment assignment to students
- Result entry and scoring calculation
- Benchmark management and grade-level standards
- Student performance trend analysis
- Assessment template library
- State test data import
- Assessment scheduling and reminders
- Rubric-based scoring
- Assessment analytics and reports

**Not Owned** (delegated to other services):
- Student enrollment data → Student Management Service
- District grading scales → Configuration Service (reused for score conversion)
- Intervention recommendations → Intervention Service (consumes assessment events)
- Comprehensive reporting dashboards → Reporting & Analytics Service

#### Domain Events Published

**Event Schema Version**: 1.0 (follows domain-events-schema.md)

- `AssessmentCreatedEvent` - When new assessment defined
  ```
  - AssessmentId: Guid
  - TenantId: Guid
  - AssessmentName: string
  - Subject: string
  - GradeLevels: int[]
  - CreatedBy: Guid
  - OccurredAt: DateTime
  ```

- `AssessmentAssignedEvent` - When assigned to students
  ```
  - AssignmentId: Guid
  - TenantId: Guid
  - AssessmentId: Guid
  - StudentId: Guid
  - DueDate: DateTime
  - AssignedBy: Guid
  - OccurredAt: DateTime
  ```

- `AssessmentResultRecordedEvent` - When result entered
  ```
  - ResultId: Guid
  - TenantId: Guid
  - AssessmentId: Guid
  - StudentId: Guid
  - TotalScore: decimal
  - BenchmarkLevel: string
  - CompletedDate: DateTime
  - RecordedBy: Guid
  - OccurredAt: DateTime
  ```

- `BenchmarkCreatedEvent` - When benchmark configured
  ```
  - BenchmarkId: Guid
  - TenantId: Guid
  - GradeLevel: int
  - Subject: string
  - BenchmarkName: string
  - MinScore: decimal
  - MaxScore: decimal
  - OccurredAt: DateTime
  ```

- `StateTestResultsImportedEvent` - When state test data loaded
  ```
  - ImportJobId: Guid
  - TenantId: Guid
  - StateTestType: string  // e.g., "CALPADS", "PEIMS"
  - TotalResults: int
  - OccurredAt: DateTime
  ```

#### Domain Events Subscribed

- `StudentEnrolledEvent` (from Student Service) → Enable assessment assignment
- `DistrictSettingsUpdatedEvent` (from Configuration Service) → Update grading scales

#### API Endpoints (Functional Intent)

**Assessment Management**:
- Create assessment → returns assessment with custom fields
- Update assessment → modifies definition
- Get assessment details → returns full configuration
- Search assessments → returns filtered list

**Assignment Management**:
- Assign to students → creates assignments
- Get student assignments → returns pending/completed
- Batch assign → assigns to class roster

**Result Management**:
- Record result → calculates score, assigns benchmark level
- Update result → modifies existing result
- Get results for assessment → returns all student results
- Get student assessment history → returns all results for student

**Analytics**:
- Get student trends → calculates performance trajectory
- Get class analytics → aggregate performance metrics
- Get benchmark distribution → counts by performance level

**Benchmark & Configuration**:
- Create benchmark → defines grade-level standards
- Get benchmarks → returns for grade/subject
- Import state test data → processes state-specific format

#### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime
- **Create Assessment**: p95 < 100ms
- **Record Result**: p95 < 100ms
- **Search Assessments**: p95 < 100ms
- **Student Trends Calculation**: p95 < 200ms
- **Bulk Export (1000 results)**: < 30 seconds

#### Idempotency & Consistency

**Idempotency Windows**:
- Assessment creation: 10 minutes (duplicate name prevention)
- Result recording: 5 minutes (prevent duplicate entry)

**Consistency Model**:
- Strong consistency for result recording
- Eventual consistency for cross-service data (student names)
- Trend calculations use materialized views (eventual consistency)

#### Security Considerations

**Constitutional Requirements**:
- FERPA compliance for assessment results
- Secrets stored in Azure Key Vault only
- Enforce least privilege principle

**Implementation**:
- Teachers can record results for their students
- Principals can view all assessments at their schools
- District admins can view all assessments
- Score changes audited and flagged for review
- Historical results immutable after grading period closes

#### Testing Requirements

**Constitutional Compliance**:
- Reqnroll BDD features before implementation
- TDD Red → Green with test evidence
- ≥ 80% code coverage

**Test Categories**:

1. **Unit Tests** (Assessment.UnitTests):
   - Scoring calculation logic (weighted, rubric)
   - Benchmark categorization
   - Trend analysis calculations (linear regression)
   - Custom field validation

2. **Integration Tests** (Assessment.IntegrationTests):
   - Database operations via EF Core (including JSONB)
   - Event publishing to message bus
   - Cross-service queries (Student, Configuration)
   - State test data import parsing

3. **BDD Tests** (Reqnroll features):
   - `AssessmentCreation.feature` - Define assessment with fields
   - `ResultRecording.feature` - Enter and score results
   - `BenchmarkManagement.feature` - Configure standards
   - `StudentTrends.feature` - Performance analytics

4. **UI Tests** (Playwright):
   - Assessment definition form
   - Result entry workflow
   - Benchmark configuration interface
   - Student trend dashboard

#### Dependencies

**External Services**:
- Student Management Service - Student enrollment data
- Configuration Service - Grading scales and district settings
- Intervention Service - Consumes assessment events for RTI triggers
- Data Import Service - State test data import orchestration
- Azure Service Bus - Event publishing

**Infrastructure Dependencies**:
- PostgreSQL - Assessment database (with JSONB for flexibility)
- .NET Aspire AppHost - Service orchestration
- Redis - Assessment definition caching

#### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 2a** (Week 9): Deploy new Assessment Service alongside legacy
   - Route new assessment creation to new service
   - Legacy assessments continue in NS4.WebAPI
   - Dual-read from both systems

2. **Phase 2b** (Week 10-11): Data migration
   - Migrate historical assessment definitions
   - Migrate custom field configurations
   - Migrate assessment results (15 years of data)
   - Migrate benchmarks
   - Maintain both systems in parallel

3. **Phase 2c** (Week 12): Complete cutover
   - Route all assessment operations to new service
   - Keep legacy as read-only fallback
   - Monitor error rates and performance

4. **Phase 2d** (Week 13-14): Enhanced features
   - Add state test data import automation
   - Add advanced trend analysis
   - Integrate with Intervention Service for RTI recommendations

5. **Phase 2e** (Week 15-16): Decommission legacy
   - Verify all data migrated
   - Archive legacy assessment tables
   - Remove legacy controllers

---

## Technical Implementation Notes

**Clean Architecture**:
```
NorthStar.Assessments/
├── Domain/
│   ├── Entities/
│   │   ├── Assessment.cs
│   │   ├── AssessmentField.cs
│   │   ├── AssessmentAssignment.cs
│   │   ├── AssessmentResult.cs
│   │   └── Benchmark.cs
│   ├── ValueObjects/
│   │   ├── AssessmentId.cs
│   │   ├── Score.cs
│   │   └── BenchmarkLevel.cs
│   └── Events/
│       ├── AssessmentCreatedEvent.cs
│       ├── AssessmentAssignedEvent.cs
│       └── AssessmentResultRecordedEvent.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateAssessmentCommand.cs
│   │   ├── AssignAssessmentCommand.cs
│   │   └── RecordResultCommand.cs
│   ├── Queries/
│   │   ├── GetAssessmentQuery.cs
│   │   ├── SearchAssessmentsQuery.cs
│   │   └── GetStudentTrendsQuery.cs
│   └── Validators/
│       └── AssessmentValidator.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── AssessmentDbContext.cs
│   │   └── AssessmentRepository.cs
│   └── Integration/
│       └── EventPublisher.cs
└── Api/
    └── Controllers/
        ├── AssessmentsController.cs
        ├── BenchmarksController.cs
        └── ResultsController.cs
```

**Database Schema**:
```sql
CREATE TABLE assessment.assessments (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    legacy_id INTEGER,
    assessment_name VARCHAR(200) NOT NULL,
    subject VARCHAR(100),
    grade_levels INTEGER[],
    assessment_type VARCHAR(50),
    scoring_method VARCHAR(50),
    max_score DECIMAL(10,2),
    created_at TIMESTAMPTZ NOT NULL,
    created_by UUID,
    is_template BOOLEAN DEFAULT FALSE
);

CREATE TABLE assessment.assessment_fields (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    assessment_id UUID NOT NULL,
    field_name VARCHAR(100) NOT NULL,
    field_type VARCHAR(50),
    max_value DECIMAL(10,2),
    weight DECIMAL(5,2),
    sort_order INTEGER
);

CREATE TABLE assessment.assessment_assignments (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    assessment_id UUID NOT NULL,
    student_id UUID NOT NULL,
    assigned_date DATE,
    due_date DATE,
    status VARCHAR(50),
    assigned_by UUID
);

CREATE TABLE assessment.assessment_results (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    assignment_id UUID NOT NULL,
    student_id UUID NOT NULL,
    assessment_id UUID NOT NULL,
    total_score DECIMAL(10,2),
    benchmark_level VARCHAR(50),
    completed_date DATE,
    recorded_by UUID,
    field_scores JSONB
);

CREATE TABLE assessment.benchmarks (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    assessment_id UUID,
    grade_level INTEGER,
    subject VARCHAR(100),
    benchmark_name VARCHAR(100),
    min_score DECIMAL(10,2),
    max_score DECIMAL(10,2)
);

-- Row-Level Security
ALTER TABLE assessment.assessments ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON assessment.assessments
    USING (tenant_id = current_setting('app.current_tenant')::uuid);

-- Indexes
CREATE INDEX idx_assessments_tenant ON assessment.assessments(tenant_id);
CREATE INDEX idx_results_student ON assessment.assessment_results(tenant_id, student_id);
CREATE INDEX idx_results_assessment ON assessment.assessment_results(tenant_id, assessment_id);
```

**API Endpoints**:
- `POST /api/v1/assessments` - Create assessment
- `GET /api/v1/assessments/{id}` - Get assessment details
- `PUT /api/v1/assessments/{id}` - Update assessment
- `GET /api/v1/assessments` - Search assessments
- `POST /api/v1/assessments/{id}/assign` - Assign to students
- `POST /api/v1/assessments/{id}/results` - Record results
- `GET /api/v1/assessments/{id}/results` - Get all results
- `GET /api/v1/students/{id}/assessments` - Student assessment history
- `GET /api/v1/students/{id}/trends` - Student performance trends
- `POST /api/v1/benchmarks` - Create benchmark
- `GET /api/v1/benchmarks` - List benchmarks
- `POST /api/v1/assessments/import` - Import state test data
- `GET /api/v1/assessments/export` - Export assessment data

**Domain Events**:
- `AssessmentCreatedEvent`
- `AssessmentAssignedEvent`
- `AssessmentResultRecordedEvent`
- `BenchmarkCreatedEvent`
- `StateTestResultsImportedEvent`

**Performance SLOs**:
- Create assessment: <100ms (P95)
- Record result: <100ms (P95)
- Search assessments: <100ms (P95)
- Student trends calculation: <200ms (P95)
- Bulk export 1000 results: <30 seconds

**Analytics Calculations**:
```csharp
public class TrendAnalyzer
{
    public StudentTrend AnalyzeTrend(List<AssessmentResult> results)
    {
        // Calculate linear regression
        var trend = results
            .OrderBy(r => r.CompletedDate)
            .Select((r, i) => new { Score = r.TotalScore, Index = i })
            .LinearRegression();
        
        return new StudentTrend
        {
            Direction = trend.Slope > 0 ? "Improving" : 
                       trend.Slope < 0 ? "Declining" : "Stable",
            AverageScore = results.Average(r => r.TotalScore),
            HighestScore = results.Max(r => r.TotalScore),
            LowestScore = results.Min(r => r.TotalScore),
            TrendStrength = Math.Abs(trend.Slope)
        };
    }
}
```

**Security Requirements**:
- Teachers can only view assessments for their students
- Principals can view all assessments at their schools
- District admins can view all assessments in district
- Assessment results are FERPA-protected
- Score changes audited and flagged
