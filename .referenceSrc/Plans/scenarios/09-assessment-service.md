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
