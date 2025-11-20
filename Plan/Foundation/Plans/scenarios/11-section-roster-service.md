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
