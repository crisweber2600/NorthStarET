# Intervention Management Service Migration

**Feature**: Migrate Intervention Management from Monolith to Microservice  
**Epic**: Phase 3 - Secondary Domain Services (Weeks 17-22)  
**Service**: Intervention Management Service  
**Business Value**: Track and manage student interventions, RTI/MTSS support

---

## Scenario 1: Create Intervention Group for Tier 2 Reading Support

**Given** a reading specialist identifies students needing additional support  
**And** they want to create a Tier 2 intervention group  
**When** they create an intervention:
  - Intervention Name: "Small Group Reading - Tier 2"
  - Tier Level: "Tier 2"
  - Focus Area: "Reading Fluency"
  - Duration: "8 weeks"
  - Frequency: "3 times per week, 30 minutes"
  - Facilitator: "Ms. Johnson"
**And** they add 6 students to the group  
**And** they submit the form  
**Then** the Intervention Service creates the group with `tenant_id`  
**And** publishes `InterventionCreatedEvent`  
**And** the 6 students are enrolled in the intervention  
**And** calendar entries are created for 24 sessions (8 weeks × 3/week)  
**And** the group appears on the specialist's dashboard

---

## Scenario 2: Assign Students to Intervention Based on Assessment Data

**Given** recent assessment results identify struggling students  
**And** students scoring below 50 on reading benchmark need intervention  
**When** a specialist views the "At-Risk Students" report  
**Then** the system displays students below the threshold  
**And** recommends appropriate intervention tier based on score  
**When** the specialist selects 4 students and creates an intervention  
**Then** students are automatically enrolled  
**And** parent notification emails are sent  
**And** the Student Service receives `StudentEnrolledInInterventionEvent`  
**And** student dashboards show intervention enrollment

---

## Scenario 3: Track Intervention Attendance with Session Notes

**Given** an intervention group has a scheduled session  
**When** the facilitator takes attendance:
  - Emma: Present
  - Liam: Present
  - Sophia: Absent
  - Noah: Present
  - Olivia: Present
  - Ava: Absent
**And** adds session notes: "Focused on phonics blends. Emma and Noah showed great progress."  
**And** saves the attendance record  
**Then** the Intervention Service records attendance with timestamp  
**And** publishes `InterventionAttendanceRecordedEvent`  
**And** calculates attendance rate: 4/6 = 67%  
**And** flags students with multiple absences for follow-up  
**And** the Reporting Service updates intervention reports

---

## Scenario 4: Record Intervention Progress Notes and Observations

**Given** a student is participating in a reading intervention  
**When** the facilitator records progress notes:
  - Student: "Emma Wilson"
  - Date: "October 15, 2025"
  - Observation: "Emma is showing improvement in decoding multisyllabic words"
  - Fluency Level: "Grade 3.2" (up from 2.8)
  - Next Steps: "Continue current strategies, add comprehension focus"
  - Progress Rating: "Making Expected Progress"
**And** saves the note  
**Then** the progress note is stored with tenant isolation  
**And** linked to both student and intervention records  
**And** historical progress is tracked over time  
**And** progress reports show improvement trajectory

---

## Scenario 5: Intervention Scheduling and Calendar Integration

**Given** multiple intervention groups have conflicting schedules  
**When** a specialist schedules a new intervention:
  - Monday, Wednesday, Friday at 10:00 AM  
**Then** the Intervention Service checks for conflicts  
**And** validates room availability  
**And** validates facilitator availability (from Staff Service)  
**And** creates calendar events for all sessions  
**And** sends calendar invites to facilitator and support staff  
**And** integrates with Section Service to avoid pulling students from core classes

---

## Scenario 6: Intervention Effectiveness Tracking and Analytics

**Given** an intervention has been running for 4 weeks  
**When** a specialist views the effectiveness dashboard  
**Then** the system displays:
  - Attendance rate: 85%
  - Pre-intervention average score: 45
  - Mid-intervention average score: 58 (up 13 points)
  - Students showing progress: 5/6 (83%)
  - Projected end-of-intervention score: 68
  - ROI calculation: hours invested vs. improvement gained
**And** provides recommendation: "Continue for planned duration"  
**And** flags students not responding for tier escalation

---

## Scenario 7: Multi-Tier Intervention Support (MTSS/RTI Framework)

**Given** a school implements a 3-tier MTSS model  
**When** a student progresses through tiers:
  - Starts in Tier 1 (classroom support)
  - Moves to Tier 2 (small group) due to low progress
  - Escalates to Tier 3 (intensive 1-on-1) if still struggling
**Then** the Intervention Service tracks tier history  
**And** maintains complete intervention timeline  
**And** documents decision points for tier changes  
**And** ensures proper documentation for special education referral if needed  
**And** provides parent communication at each tier change

---

## Scenario 8: Parent Communication for Interventions

**Given** parents must be informed about intervention enrollment  
**When** a student is enrolled in an intervention  
**Then** an automated email is sent to parents with:
  - Intervention name and purpose
  - Schedule and duration
  - Expected outcomes
  - Contact information for facilitator
  - Permission to opt-out (if applicable)
**When** progress notes are added  
**Then** parents can view notes via parent portal  
**And** quarterly progress summaries are emailed automatically  
**And** all communications are logged for documentation

---

## Scenario 9: Intervention Resource Management and Materials

**Given** interventions require specific materials and resources  
**When** a specialist creates an intervention  **Then** they can specify required resources:
  - Curriculum: "Reading Mastery Level 3"
  - Materials: "Decodable readers, phonics cards"
  - Location: "Room 104"
  - Equipment: "iPad with reading apps"
**And** the system tracks resource allocation  
**And** prevents double-booking of limited resources  
**And** administrators can view resource utilization reports  
**And** budget impact is calculated per intervention

---

## Scenario 10: Intervention Plan Templates and Best Practices

**Given** certain intervention strategies are proven effective  
**When** a specialist creates a new intervention from template:
  - Template: "Tier 2 Math Facts Fluency"
  - Includes: Recommended duration, frequency, group size, activities
**Then** the intervention is pre-populated with template data  
**And** allows customization before saving  
**And** templates are shared across the district (tenant-scoped)  
**And** usage statistics show which templates are most effective  
**And** specialists can save their own templates

---

## Scenario 11: Intervention Exit Criteria and Success Monitoring

**Given** interventions have defined exit criteria  
**When** a student meets exit criteria:
  - 3 consecutive sessions showing proficiency
  - Benchmark score ≥ grade level
  - Recommendation from facilitator
**Then** the Intervention Service flags student for exit review  
**And** generates exit recommendation report  
**And** if approved, student is exited from intervention  
**And** publishes `StudentExitedInterventionEvent`  
**And** schedules follow-up monitoring for 6 weeks  
**And** celebrates success with student and parents

---

## Scenario 12: Intervention Audit Trail and Compliance Documentation

**Given** intervention data is critical for compliance and special education  
**When** any intervention record is modified  
**Then** an audit log entry is created with:
  - User who made the change
  - What was changed (before/after)
  - Timestamp
  - Reason for change
**And** all decisions are documented  
**And** tier change justifications are recorded  
**And** parent communication is logged  
**And** the audit trail supports IEP development if needed  
**And** compliance reports can be generated  
**And** all logs are tenant-isolated and immutable

---

---

## Architectural Appendix

### Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` project (legacy monolith)  
**Framework**: .NET Framework 4.6  
**Database**: Per-district SQL Server databases

**Key Legacy Components**:
- `NS4.WebAPI/Controllers/InterventionController.cs` - CRUD operations for interventions
- `NS4.WebAPI/Controllers/RTIController.cs` - Response to Intervention (RTI) tracking
- Legacy tables: `Interventions`, `InterventionGroups`, `InterventionAttendance`
- Tight coupling with assessment and student data in single database
- Limited multi-tier intervention support (MTSS/RTI framework)

**Legacy Limitations**:
- No event-driven coordination with other services
- Manual data entry for progress tracking
- Limited analytics and effectiveness metrics
- No automated recommendations based on assessment data

### Target State (.NET 10 Microservice)

#### Architecture

**Clean Architecture Layers**:
```
Intervention.API/                # UI Layer (REST endpoints)
├── Controllers/
├── Middleware/
└── Program.cs

Intervention.Application/        # Application Layer
├── Commands/                   # Create intervention, enroll student
├── Queries/                    # Get interventions, effectiveness metrics
├── DTOs/
├── Interfaces/
└── Services/
    └── EffectivenessAnalyzer/ # Calculate intervention outcomes

Intervention.Domain/            # Domain Layer
├── Entities/
│   ├── Intervention.cs
│   ├── InterventionEnrollment.cs
│   ├── InterventionSession.cs
│   ├── SessionAttendance.cs
│   └── ProgressNote.cs
├── Events/
│   ├── InterventionCreatedEvent.cs
│   ├── StudentEnrolledInInterventionEvent.cs
│   ├── InterventionAttendanceRecordedEvent.cs
│   └── ProgressNoteAddedEvent.cs
└── ValueObjects/
    ├── TierLevel.cs  // Tier 1, 2, 3
    ├── ProgressRating.cs
    └── AttendanceStatus.cs

Intervention.Infrastructure/    # Infrastructure Layer
├── Data/
│   ├── InterventionDbContext.cs
│   └── Repositories/
├── Integration/
│   ├── EventPublisher.cs
│   └── StudentServiceClient.cs  // Query student data
└── MessageBus/
```

#### Technology Stack

- **Framework**: .NET 10, ASP.NET Core
- **Data Access**: EF Core 9 with PostgreSQL
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Caching**: Redis Stack for intervention lookups
- **Orchestration**: .NET Aspire hosting
- **Analytics**: Custom effectiveness calculations with trend analysis

#### Owned Data

**Database**: `NorthStar_Intervention_DB`

**Tables**:
- Interventions (Id, TenantId, Name, TierLevel, FocusArea, StartDate, EndDate, FacilitatorId)
- InterventionEnrollments (Id, TenantId, InterventionId, StudentId, EnrollmentDate, ExitDate, IsActive)
- InterventionSessions (Id, TenantId, InterventionId, SessionDate, Notes, FacilitatorId)
- SessionAttendance (Id, TenantId, SessionId, StudentId, AttendanceStatus, RecordedAt)
- ProgressNotes (Id, TenantId, InterventionId, StudentId, NoteDate, Observation, ProgressRating, CreatedBy)
- InterventionResources (Id, TenantId, Name, Description, ResourceUrl) // Toolkit

#### Service Boundaries

**Owned Responsibilities**:
- Intervention group creation and management
- Student enrollment in interventions
- Session scheduling and calendar management
- Attendance tracking per session
- Progress note recording
- Intervention toolkit resource management
- Effectiveness analytics and trend analysis
- Multi-tier intervention support (MTSS/RTI framework)

**Not Owned** (delegated to other services):
- Student enrollment data → Student Management Service
- Assessment results → Assessment Service
- Staff assignments → Staff Management Service
- Calendar integration → Configuration Service
- Reporting dashboards → Reporting & Analytics Service

#### Domain Events Published

**Event Schema Version**: 1.0 (follows domain-events-schema.md)

- `InterventionCreatedEvent` - When new intervention is created
  ```
  - InterventionId: Guid
  - TenantId: Guid
  - InterventionName: string
  - TierLevel: string
  - FocusArea: string
  - CreatedBy: Guid
  - OccurredAt: DateTime
  ```

- `StudentEnrolledInInterventionEvent` - When student enrolled
  ```
  - EnrollmentId: Guid
  - TenantId: Guid
  - InterventionId: Guid
  - StudentId: Guid
  - EnrollmentDate: DateTime
  - OccurredAt: DateTime
  ```

- `InterventionAttendanceRecordedEvent` - When attendance taken
  ```
  - AttendanceId: Guid
  - TenantId: Guid
  - SessionId: Guid
  - InterventionId: Guid
  - StudentId: Guid
  - AttendanceStatus: string
  - RecordedAt: DateTime
  - OccurredAt: DateTime
  ```

- `ProgressNoteAddedEvent` - When progress documented
  ```
  - NoteId: Guid
  - TenantId: Guid
  - InterventionId: Guid
  - StudentId: Guid
  - ProgressRating: string
  - CreatedBy: Guid
  - OccurredAt: DateTime
  ```

- `StudentExitedInterventionEvent` - When student exits intervention
  ```
  - EnrollmentId: Guid
  - TenantId: Guid
  - InterventionId: Guid
  - StudentId: Guid
  - ExitDate: DateTime
  - ExitReason: string
  - OccurredAt: DateTime
  ```

#### Domain Events Subscribed

- `StudentEnrolledEvent` (from Student Service) → Enable intervention enrollment
- `AssessmentResultRecordedEvent` (from Assessment Service) → Trigger data-driven recommendations
- `StaffUpdatedEvent` (from Staff Service) → Update facilitator assignments

#### API Endpoints (Functional Intent)

**Intervention Management**:
- Create intervention group → returns intervention details
- Update intervention → modifies intervention settings
- Get intervention details → returns intervention with enrollment count
- List interventions → returns filtered list with pagination

**Enrollment & Attendance**:
- Enroll student in intervention → creates enrollment record
- Record session attendance → tracks attendance with notes
- Exit student from intervention → marks enrollment inactive

**Progress Tracking**:
- Add progress note → documents observation and rating
- Get student intervention history → returns all interventions for student
- Calculate intervention effectiveness → returns metrics and trends

#### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime
- **Create Intervention**: p95 < 100ms
- **Record Attendance**: p95 < 50ms
- **Calculate Effectiveness**: p95 < 200ms
- **Student Intervention History Query**: p95 < 150ms

#### Idempotency & Consistency

**Idempotency Windows**:
- Intervention creation: 10 minutes (duplicate prevention)
- Attendance recording: 5 minutes (same session)

**Consistency Model**:
- Strong consistency for attendance and progress notes
- Eventual consistency for cross-service data (student names, facilitator names)

#### Security Considerations

**Constitutional Requirements**:
- Enforce least privilege principle
- FERPA compliance for student intervention data
- Secrets stored in Azure Key Vault only

**Implementation**:
- Intervention specialists can create/manage interventions
- Teachers can view interventions for their students only
- Progress notes are confidential and audited
- All data access logged for compliance

#### Testing Requirements

**Constitutional Compliance**:
- Reqnroll BDD features before implementation
- TDD Red → Green with test evidence
- ≥ 80% code coverage

**Test Categories**:

1. **Unit Tests** (Intervention.UnitTests):
   - Effectiveness calculation logic
   - Tier escalation rules
   - Attendance rate calculations
   - Progress rating validation

2. **Integration Tests** (Intervention.IntegrationTests):
   - Database operations via EF Core
   - Event publishing to message bus
   - Cross-service queries (Student, Staff)
   - Aspire orchestration validation

3. **BDD Tests** (Reqnroll features):
   - `InterventionCreation.feature` - Create intervention group
   - `StudentEnrollment.feature` - Enroll students
   - `AttendanceTracking.feature` - Record attendance
   - `ProgressMonitoring.feature` - Document progress

4. **UI Tests** (Playwright):
   - Intervention creation form
   - Attendance entry workflow
   - Progress note submission
   - Effectiveness dashboard

#### Dependencies

**External Services**:
- Student Management Service - Student enrollment data
- Assessment Service - Assessment results for recommendations
- Staff Management Service - Facilitator assignments
- Configuration Service - Calendar and schedules
- Azure Service Bus - Event publishing
- Redis - Intervention caching

**Infrastructure Dependencies**:
- PostgreSQL - Intervention database
- .NET Aspire AppHost - Service orchestration

#### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 3a** (Week 17): Deploy new Intervention Service alongside legacy
   - Route new intervention creation to new service
   - Legacy interventions continue in NS4.WebAPI
   - Dual-read from both systems

2. **Phase 3b** (Week 18-19): Data migration
   - Migrate historical intervention records
   - Migrate attendance and progress notes
   - Maintain both systems in parallel

3. **Phase 3c** (Week 20): Complete cutover
   - Route all intervention operations to new service
   - Keep legacy as read-only fallback
   - Monitor error rates

4. **Phase 3d** (Week 21-22): Decommission legacy
   - Verify all data migrated
   - Archive legacy intervention tables
   - Remove legacy controllers

---

## Technical Implementation Notes

**Clean Architecture**:
```
NorthStar.Interventions/
├── Domain/
│   ├── Entities/
│   │   ├── Intervention.cs
│   │   ├── InterventionGroup.cs
│   │   ├── InterventionEnrollment.cs
│   │   ├── InterventionSession.cs
│   │   └── ProgressNote.cs
│   ├── ValueObjects/
│   │   ├── InterventionId.cs
│   │   ├── TierLevel.cs
│   │   └── ProgressRating.cs
│   └── Events/
│       ├── InterventionCreatedEvent.cs
│       ├── StudentEnrolledEvent.cs
│       └── InterventionCompletedEvent.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateInterventionCommand.cs
│   │   ├── EnrollStudentCommand.cs
│   │   └── RecordAttendanceCommand.cs
│   ├── Queries/
│   │   ├── GetInterventionQuery.cs
│   │   └── GetEffectivenessQuery.cs
│   └── Validators/
│       └── InterventionValidator.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── InterventionDbContext.cs
│   │   └── InterventionRepository.cs
│   └── Integration/
│       └── EventPublisher.cs
└── Api/
    └── Controllers/
        └── InterventionsController.cs
```

**Database Schema**:
```sql
CREATE TABLE intervention.interventions (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    intervention_name VARCHAR(200) NOT NULL,
    tier_level VARCHAR(20),
    focus_area VARCHAR(100),
    start_date DATE,
    end_date DATE,
    frequency VARCHAR(100),
    facilitator_id UUID,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE intervention.intervention_enrollments (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    intervention_id UUID NOT NULL,
    student_id UUID NOT NULL,
    enrollment_date DATE,
    exit_date DATE,
    exit_reason VARCHAR(200),
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE intervention.intervention_sessions (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    intervention_id UUID NOT NULL,
    session_date DATE NOT NULL,
    notes TEXT,
    facilitator_id UUID
);

CREATE TABLE intervention.session_attendance (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    session_id UUID NOT NULL,
    student_id UUID NOT NULL,
    attendance_status VARCHAR(20),
    recorded_at TIMESTAMPTZ
);

CREATE TABLE intervention.progress_notes (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    intervention_id UUID NOT NULL,
    student_id UUID NOT NULL,
    note_date DATE NOT NULL,
    observation TEXT,
    progress_rating VARCHAR(50),
    created_by UUID,
    created_at TIMESTAMPTZ NOT NULL
);

-- Row-Level Security
ALTER TABLE intervention.interventions ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON intervention.interventions
    USING (tenant_id = current_setting('app.current_tenant')::uuid);
```

**API Endpoints**:
- `POST /api/v1/interventions` - Create intervention
- `GET /api/v1/interventions/{id}` - Get intervention details
- `PUT /api/v1/interventions/{id}` - Update intervention
- `POST /api/v1/interventions/{id}/enroll` - Enroll student
- `POST /api/v1/interventions/{id}/sessions` - Create session
- `POST /api/v1/sessions/{id}/attendance` - Record attendance
- `POST /api/v1/interventions/{id}/notes` - Add progress note
- `GET /api/v1/interventions/{id}/effectiveness` - Get effectiveness metrics
- `GET /api/v1/students/{id}/interventions` - Student intervention history

**Domain Events**:
- `InterventionCreatedEvent`
- `StudentEnrolledInInterventionEvent`
- `InterventionAttendanceRecordedEvent`
- `ProgressNoteAddedEvent`
- `StudentExitedInterventionEvent`
- `TierEscalatedEvent`

**Performance SLOs**:
- Create intervention: <100ms (P95)
- Record attendance: <50ms (P95)
- Effectiveness calculation: <200ms (P95)

**Security Requirements**:
- Intervention specialists can create/manage interventions
- Teachers can view interventions for their students
- All progress notes are confidential
- FERPA compliance for student data
