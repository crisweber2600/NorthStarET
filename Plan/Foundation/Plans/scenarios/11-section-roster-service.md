# Section & Roster Service Migration

**Feature**: Migrate Section and Roster Management from Monolith to Microservice  
**Epic**: Phase 3 - Secondary Domain Services (Weeks 17-22)  
**Service**: Section & Roster Service  
**Business Value**: Modern class scheduling and roster management with automated rollover

---

## Scenario 1: Create Class Section with Teacher Assignment

**Given** a school administrator is setting up the new school year  
**And** they need to create a 3rd grade math class  
**When** they create a section:
  - Course: "Mathematics"
  - Grade Level: 3
  - Section Number: "M-301"
  - Teacher: "Ms. Rodriguez"
  - Room: "Room 204"
  - Period: "Period 2 (9:30 AM - 10:30 AM)"
  - Capacity: 25 students
  - Term: "Fall 2025"
**And** they save the section  
**Then** the Section Service creates the section with `tenant_id`  
**And** publishes `SectionCreatedEvent`  
**And** the teacher receives notification of assignment  
**And** the section appears in the master schedule  
**And** the section is ready for student enrollment

---

## Scenario 2: Add Students to Section Roster

**Given** a section "M-301" exists with capacity for 25 students  
**And** a counselor is assigning students to classes  
**When** they add 20 students to the roster  
**Then** each student is added with enrollment date  
**And** publishes `StudentAddedToRosterEvent` for each student  
**And** the Student Service updates student schedules  
**And** students see the class on their schedules  
**And** current enrollment shows 20/25 (80% capacity)  
**And** 5 seats remain available

---

## Scenario 3: Section Scheduling with Period Configuration

**Given** a school uses an 8-period day schedule  
**When** an administrator configures periods:
  - Period 1: 8:00 AM - 8:55 AM
  - Period 2: 9:00 AM - 9:55 AM
  - Period 3: 10:00 AM - 10:55 AM
  - Lunch: 11:00 AM - 11:30 AM
  - Period 4: 11:35 AM - 12:30 PM
  - (continues through Period 8)
**And** creates sections assigned to specific periods  
**Then** the Section Service validates no teacher conflicts  
**And** validates no room conflicts  
**And** validates student schedule doesn't have overlapping periods  
**And** generates master schedule for the school  
**And** identifies any scheduling conflicts for resolution

---

## Scenario 4: Automated Year-End Rollover Process

**Given** the school year is ending  
**And** 500 students need to be promoted to next grade  
**When** an administrator initiates the rollover process:
  - Current Year: 2024-2025
  - New Year: 2025-2026
  - Promotion Rules: Grade N → Grade N+1
  - Retain: Students flagged for retention
**Then** the Section Service:
  - Archives current year sections  
  - Creates template sections for new year based on historical patterns  
  - Promotes students to next grade level  
  - Maintains student groups where appropriate  
  - Publishes `RolloverCompletedEvent`  
  - Generates rollover report: 485 promoted, 15 retained  
  - Allows manual adjustments before finalizing

---

## Scenario 5: Section Capacity Management and Waitlists

**Given** a popular elective class has limited capacity  
**And** "Art 1" section has capacity of 20 students  
**When** 25 students request enrollment  
**Then** the first 20 students are enrolled  
**And** 5 students are added to waitlist  
**When** a student drops the class  
**Then** the first waitlisted student is automatically offered the spot  
**And** notification is sent to waitlisted student and parent  
**And** if accepted, student is moved from waitlist to enrolled  
**And** the next waitlist position is filled

---

## Scenario 6: Co-Teaching and Multiple Instructors

**Given** some classes have multiple teachers  
**When** creating a special education inclusion class:
  - Primary Teacher: "Ms. Johnson" (General Ed)
  - Co-Teacher: "Mr. Davis" (Special Ed)
  - Teaching Assignment: Both teachers have full access
**Then** both teachers are assigned to the section  
**And** both can take attendance and enter grades  
**And** both appear on student schedules  
**And** room scheduling accounts for both teachers' schedules  
**And** parent communications copy both teachers

---

## Scenario 7: Section Search and Filtering for Counselors

**Given** a counselor needs to find appropriate class placements  
**When** they search for sections with filters:
  - Grade Level: 10
  - Subject: "English"
  - Period: "Period 3"
  - Has Available Seats: Yes
  - Teacher: Any
**Then** the search returns matching sections  
**And** shows current enrollment vs. capacity  
**And** shows teacher names and room numbers  
**And** sorts by most available seats first  
**And** the search completes within 100ms (P95)  
**And** only sections from the same tenant are visible

---

## Scenario 8: Drop/Add Student from Section with Effective Dates

**Given** a student needs to change from one math class to another  
**When** a counselor processes the schedule change:
  - Drop: "Math-301" effective "October 15"
  - Add: "Math-302" effective "October 16"
  - Reason: "Better fit for student level"
**Then** the Section Service:
  - Marks enrollment in Math-301 as ended on Oct 15  
  - Creates new enrollment in Math-302 starting Oct 16  
  - Publishes `StudentRosterChangedEvent`  
  - Updates student's schedule immediately  
  - Notifies both teachers of the change  
  - Maintains historical record of both enrollments  
  - Ensures no attendance/grade gaps

---

## Scenario 9: Section Attendance Integration

**Given** teachers take attendance daily  
**When** a teacher marks attendance for "Period 2 Math":
  - 18 students present
  - 2 students absent
**Then** the Section Service receives attendance data  
**And** validates all students are on the roster  
**And** publishes `AttendanceRecordedEvent`  
**And** the Attendance Service (if separate) stores detailed records  
**And** absent students trigger automated parent notifications  
**And** attendance rate is calculated: 18/20 = 90%

---

## Scenario 10: Section Grade Book Integration

**Given** section rosters feed into gradebook systems  
**When** a teacher accesses the gradebook for their section  
**Then** the Section Service provides current roster  
**And** includes student names, IDs, and enrollment status  
**And** reflects any mid-term roster changes  
**And** the gradebook displays all currently enrolled students  
**And** historical students are marked as "Dropped" with effective dates  
**And** new students added mid-term appear immediately

---

## Scenario 11: Section Reports and Roster Export

**Given** teachers need class rosters for various purposes  
**When** a teacher exports a roster for "English-401"  
**Then** the Section Service generates a formatted roster with:
  - Student names (Last, First)
  - Student ID numbers
  - Contact information
  - Photo (if available)
  - Special notes (IEP, 504, etc. if permitted)
**And** exports to PDF or Excel format  
**And** respects privacy settings  
**And** includes teacher name and section details  
**And** export is logged for audit purposes

---

## Scenario 12: Historical Section Data Preservation and Transcripts

**Given** section enrollment data must be preserved for transcripts  
**When** a student graduates  
**Then** all historical section enrollments are retained  
**And** includes: course name, teacher, term, grade, credits  
**And** the data is immutable once the term ends  
**And** supports transcript generation  
**And** archived sections remain queryable  
**And** all data is tenant-isolated  
**And** data retention policies are enforced (7+ years)

---

## Architectural Appendix

### Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` project (legacy monolith)  
**Framework**: .NET Framework 4.6  
**Database**: Per-district SQL Server databases

**Key Legacy Components**:
- `NS4.WebAPI/Controllers/SectionController.cs` - Class section CRUD operations
- `NS4.WebAPI/Controllers/RosterController.cs` - Student roster management
- `NS4.WebAPI/Controllers/ScheduleController.cs` - Student schedule generation
- Legacy tables: `Sections`, `ClassRosters`, `TeacherAssignments`, `SchoolPeriods`
- Tight coupling with staff and student data in single database
- Manual year-end rollover process (batch job in NS4.BatchProcessor)

**Legacy Limitations**:
- No automated conflict detection for student schedules
- Limited capacity management and waitlist support
- Manual rollover requiring significant administrative time
- No real-time co-teaching support
- Tight database coupling prevents independent scaling

### Target State (.NET 10 Microservice)

#### Architecture

**Clean Architecture Layers**:
```
Section.API/                    # UI Layer (REST endpoints)
├── Controllers/
├── Middleware/
└── Program.cs

Section.Application/            # Application Layer
├── Commands/                  # Create section, add student, rollover
├── Queries/                   # Get section, student schedule
├── DTOs/
├── Interfaces/
└── Services/
    └── RolloverService/       # Automated year-end rollover

Section.Domain/                # Domain Layer
├── Entities/
│   ├── Section.cs
│   ├── Roster.cs
│   ├── TeacherAssignment.cs
│   ├── Period.cs
│   └── RolloverRecord.cs
├── Events/
│   ├── SectionCreatedEvent.cs
│   ├── StudentAddedToRosterEvent.cs
│   ├── StudentDroppedFromRosterEvent.cs
│   ├── RolloverCompletedEvent.cs
│   └── CapacityReachedEvent.cs
└── ValueObjects/
    ├── AcademicYear.cs
    ├── Term.cs
    └── TimeSlot.cs

Section.Infrastructure/        # Infrastructure Layer
├── Data/
│   ├── SectionDbContext.cs
│   └── Repositories/
├── Integration/
│   ├── EventPublisher.cs
│   ├── StudentServiceClient.cs
│   └── StaffServiceClient.cs
└── MessageBus/
```

#### Technology Stack

- **Framework**: .NET 10, ASP.NET Core
- **Data Access**: EF Core 9 with PostgreSQL
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Caching**: Redis Stack for section and roster lookups
- **Orchestration**: .NET Aspire hosting
- **Background Jobs**: Hangfire for automated rollover scheduling

#### Owned Data

**Database**: `NorthStar_Section_DB`

**Tables**:
- Sections (Id, TenantId, SectionNumber, CourseName, GradeLevel, SchoolId, Room, Period, Capacity, AcademicYear, Term)
- TeacherAssignments (Id, TenantId, SectionId, TeacherId, AssignmentType, AssignedDate)
- Rosters (Id, TenantId, SectionId, StudentId, EnrollmentDate, DropDate, IsActive, WaitlistPosition)
- Periods (Id, TenantId, SchoolId, PeriodNumber, PeriodName, StartTime, EndTime)
- RolloverRecords (Id, TenantId, FromYear, ToYear, ExecutedAt, ExecutedBy, TotalSections, TotalStudents, Status)

#### Service Boundaries

**Owned Responsibilities**:
- Class section creation and configuration
- Student roster management (add/drop)
- Teacher-to-section assignments (including co-teaching)
- School period configuration
- Automated year-end rollover
- Capacity management and waitlist tracking
- Student schedule generation
- Section search and reporting

**Not Owned** (delegated to other services):
- Student enrollment data → Student Management Service
- Teacher availability → Staff Management Service
- Academic calendar → Configuration Service
- Grades and attendance → Assessment Service
- Section analytics → Reporting & Analytics Service

#### Domain Events Published

**Event Schema Version**: 1.0 (follows domain-events-schema.md)

- `SectionCreatedEvent` - When new section is created
  ```
  - SectionId: Guid
  - TenantId: Guid
  - SchoolId: Guid
  - SectionNumber: string
  - CourseName: string
  - GradeLevel: int
  - AcademicYear: int
  - TeacherId: Guid
  - OccurredAt: DateTime
  ```

- `StudentAddedToRosterEvent` - When student enrolled in section
  ```
  - RosterId: Guid
  - TenantId: Guid
  - SectionId: Guid
  - StudentId: Guid
  - EnrollmentDate: DateTime
  - OccurredAt: DateTime
  ```

- `StudentDroppedFromRosterEvent` - When student dropped from section
  ```
  - RosterId: Guid
  - TenantId: Guid
  - SectionId: Guid
  - StudentId: Guid
  - DropDate: DateTime
  - Reason: string
  - OccurredAt: DateTime
  ```

- `RolloverCompletedEvent` - When year-end rollover finishes
  ```
  - RolloverId: Guid
  - TenantId: Guid
  - FromAcademicYear: int
  - ToAcademicYear: int
  - TotalSections: int
  - TotalStudents: int
  - PromotedCount: int
  - RetainedCount: int
  - CompletedAt: DateTime
  - OccurredAt: DateTime
  ```

- `CapacityReachedEvent` - When section reaches max capacity
  ```
  - SectionId: Guid
  - TenantId: Guid
  - SectionNumber: string
  - CurrentEnrollment: int
  - MaxCapacity: int
  - OccurredAt: DateTime
  ```

#### Domain Events Subscribed

- `StudentEnrolledEvent` (from Student Service) → Enable section enrollment
- `StaffCreatedEvent` (from Staff Service) → Enable teacher assignment
- `CalendarUpdatedEvent` (from Configuration Service) → Trigger automated rollover

#### API Endpoints (Functional Intent)

**Section Management**:
- Create section → returns section details
- Update section → modifies section configuration
- Get section details → returns section with roster count
- Search sections → returns filtered list with pagination

**Roster Management**:
- Add student to roster → creates enrollment record
- Drop student from roster → marks inactive with reason
- Get section roster → returns student list with enrollment dates
- Get student schedule → returns all sections for student

**Rollover Operations**:
- Execute year-end rollover → promotes students and creates template sections
- Get rollover status → returns progress and results
- Revert rollover → rolls back to previous year (safety feature)

**Teacher Management**:
- Assign teacher to section → creates assignment record
- Add co-teacher → supports multiple instructors per section
- Get teacher sections → returns all sections for teacher

#### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime
- **Create Section**: p95 < 100ms
- **Add Student to Roster**: p95 < 50ms
- **Search Sections**: p95 < 100ms
- **Student Schedule Query**: p95 < 150ms
- **Year-End Rollover (500 students)**: < 5 minutes

#### Idempotency & Consistency

**Idempotency Windows**:
- Section creation: 10 minutes (duplicate prevention)
- Roster add: 5 minutes (prevent duplicate enrollment)

**Consistency Model**:
- Strong consistency for roster changes (immediate visibility)
- Eventual consistency for cross-service data (student names, teacher names)

#### Security Considerations

**Constitutional Requirements**:
- Enforce least privilege principle
- FERPA compliance for student section data
- Secrets stored in Azure Key Vault only

**Implementation**:
- Counselors and administrators can modify rosters
- Teachers can view their section rosters only
- Students can view their own schedules only
- Historical data is immutable after term ends
- All roster changes audited for compliance

#### Testing Requirements

**Constitutional Compliance**:
- Reqnroll BDD features before implementation
- TDD Red → Green with test evidence
- ≥ 80% code coverage

**Test Categories**:

1. **Unit Tests** (Section.UnitTests):
   - Capacity validation logic
   - Rollover promotion rules
   - Schedule conflict detection
   - Waitlist ordering

2. **Integration Tests** (Section.IntegrationTests):
   - Database operations via EF Core
   - Event publishing to message bus
   - Cross-service queries (Student, Staff)
   - Aspire orchestration validation

3. **BDD Tests** (Reqnroll features):
   - `SectionManagement.feature` - Create and configure sections
   - `RosterManagement.feature` - Add/drop students
   - `YearEndRollover.feature` - Automated rollover
   - `ScheduleGeneration.feature` - Student schedule conflicts

4. **UI Tests** (Playwright):
   - Section creation form
   - Roster management workflow
   - Rollover execution dashboard
   - Student schedule display

#### Dependencies

**External Services**:
- Student Management Service - Student enrollment data
- Staff Management Service - Teacher assignments
- Configuration Service - Academic calendar and school configuration
- Azure Service Bus - Event publishing
- Redis - Section and roster caching

**Infrastructure Dependencies**:
- PostgreSQL - Section database
- .NET Aspire AppHost - Service orchestration
- Hangfire - Background job scheduling for rollover

#### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 3a** (Week 17): Deploy new Section Service alongside legacy
   - Route new section creation to new service
   - Legacy sections continue in NS4.WebAPI
   - Dual-read from both systems

2. **Phase 3b** (Week 18-19): Data migration
   - Migrate historical section records
   - Migrate roster enrollment history
   - Migrate teacher assignments
   - Maintain both systems in parallel

3. **Phase 3c** (Week 20): Complete cutover
   - Route all section operations to new service
   - Keep legacy as read-only fallback
   - Monitor error rates

4. **Phase 3d** (Week 21): Automated rollover
   - Execute first automated rollover in new service
   - Verify promotion logic and template creation
   - Validate cross-service event propagation

5. **Phase 3e** (Week 22): Decommission legacy
   - Verify all data migrated
   - Archive legacy section tables
   - Remove legacy controllers and batch processors

---

## Technical Implementation Notes

**Clean Architecture**:
```
NorthStar.Sections/
├── Domain/
│   ├── Entities/
│   │   ├── Section.cs
│   │   ├── Roster.cs
│   │   ├── Period.cs
│   │   └── TeacherAssignment.cs
│   ├── ValueObjects/
│   │   ├── SectionId.cs
│   │   ├── TimeSlot.cs
│   │   └── Capacity.cs
│   └── Events/
│       ├── SectionCreatedEvent.cs
│       ├── StudentAddedToRosterEvent.cs
│       └── RolloverCompletedEvent.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateSectionCommand.cs
│   │   ├── AddStudentToRosterCommand.cs
│   │   └── RolloverSectionsCommand.cs
│   ├── Queries/
│   │   ├── GetSectionQuery.cs
│   │   └── SearchSectionsQuery.cs
│   └── Validators/
│       └── SectionValidator.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── SectionDbContext.cs
│   │   └── SectionRepository.cs
│   └── Integration/
│       └── EventPublisher.cs
└── Api/
    └── Controllers/
        ├── SectionsController.cs
        └── RostersController.cs
```

**Database Schema**:
```sql
CREATE TABLE section.sections (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    legacy_id INTEGER,
    section_number VARCHAR(50) NOT NULL,
    course_name VARCHAR(200) NOT NULL,
    grade_level INTEGER,
    school_id UUID NOT NULL,
    room VARCHAR(50),
    period VARCHAR(50),
    capacity INTEGER,
    academic_year INTEGER,
    term VARCHAR(50),
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE section.teacher_assignments (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    section_id UUID NOT NULL,
    teacher_id UUID NOT NULL,
    assignment_type VARCHAR(50), -- Primary, Co-Teacher
    assigned_date DATE
);

CREATE TABLE section.rosters (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    section_id UUID NOT NULL,
    student_id UUID NOT NULL,
    enrollment_date DATE NOT NULL,
    drop_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    waitlist_position INTEGER
);

CREATE TABLE section.periods (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    school_id UUID NOT NULL,
    period_number INTEGER,
    period_name VARCHAR(50),
    start_time TIME,
    end_time TIME
);

-- Row-Level Security
ALTER TABLE section.sections ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON section.sections
    USING (tenant_id = current_setting('app.current_tenant')::uuid);

-- Indexes
CREATE INDEX idx_sections_tenant ON section.sections(tenant_id);
CREATE INDEX idx_sections_school ON section.sections(tenant_id, school_id);
CREATE INDEX idx_rosters_section ON section.rosters(tenant_id, section_id);
CREATE INDEX idx_rosters_student ON section.rosters(tenant_id, student_id, is_active);
```

**API Endpoints**:
- `POST /api/v1/sections` - Create section
- `GET /api/v1/sections/{id}` - Get section details
- `PUT /api/v1/sections/{id}` - Update section
- `POST /api/v1/sections/{id}/roster` - Add student to roster
- `DELETE /api/v1/sections/{id}/roster/{studentId}` - Drop student
- `GET /api/v1/sections/{id}/roster` - Get current roster
- `POST /api/v1/sections/rollover` - Execute year-end rollover
- `GET /api/v1/sections/search` - Search sections
- `GET /api/v1/students/{id}/schedule` - Get student schedule
- `GET /api/v1/teachers/{id}/sections` - Get teacher sections
- `POST /api/v1/periods` - Configure school periods
- `GET /api/v1/sections/{id}/export` - Export roster

**Domain Events**:
- `SectionCreatedEvent`
- `StudentAddedToRosterEvent`
- `StudentDroppedFromRosterEvent`
- `RolloverCompletedEvent`
- `CapacityReachedEvent`

**Rollover Algorithm**:
```csharp
public async Task<RolloverResult> ExecuteRollover(int currentYear, int newYear)
{
    var students = await _studentService.GetActiveStudents();
    var promotionResults = new List<PromotionResult>();
    
    foreach (var student in students)
    {
        if (student.ShouldRetain)
        {
            // Keep in same grade
            promotionResults.Add(new PromotionResult
            {
                StudentId = student.Id,
                FromGrade = student.GradeLevel,
                ToGrade = student.GradeLevel,
                Status = "Retained"
            });
        }
        else
        {
            // Promote to next grade
            await PromoteStudent(student.Id, student.GradeLevel + 1);
            promotionResults.Add(new PromotionResult
            {
                StudentId = student.Id,
                FromGrade = student.GradeLevel,
                ToGrade = student.GradeLevel + 1,
                Status = "Promoted"
            });
        }
    }
    
    await CreateTemplateNextYearSections(newYear);
    await PublishRolloverCompletedEvent(promotionResults);
    
    return new RolloverResult
    {
        TotalStudents = students.Count,
        Promoted = promotionResults.Count(r => r.Status == "Promoted"),
        Retained = promotionResults.Count(r => r.Status == "Retained")
    };
}
```

**Performance SLOs**:
- Create section: <100ms (P95)
- Add student to roster: <50ms (P95)
- Search sections: <100ms (P95)
- Rollover 500 students: <5 minutes

**Security Requirements**:
- Only counselors and admins can modify rosters
- Teachers can view their section rosters
- Students can view their own schedules
- Historical data is immutable after term ends
