# Student Management Service Migration

**Feature**: Migrate Student Management from Monolith to Microservice  
**Epic**: Phase 2 - Core Domain Services (Weeks 9-16)  
**Service**: Student Management Service  
**Business Value**: Modern student data management with event-driven architecture

---

## Scenario 1: Create New Student with Event Publishing

**Given** a school administrator is authenticated in District A  
**And** they navigate to the "Add Student" form  
**When** they enter student details:
  - First Name: "John"
  - Last Name: "Doe"
  - Date of Birth: "2010-05-15"
  - Grade Level: 5
  - School: "Lincoln Elementary"
**And** they submit the form  
**Then** the Student Service validates the input  
**And** creates a new Student record with `tenant_id = district-a`  
**And** publishes a `StudentCreatedEvent` to Azure Service Bus  
**And** the event includes: student_id, tenant_id, school_id, grade_level, timestamp  
**And** returns HTTP 201 Created with student details  
**And** the student appears in the student list immediately

---

## Scenario 2: Other Services React to Student Created Event

**Given** a new student "Jane Smith" was just created  
**And** a `StudentCreatedEvent` was published  
**When** the Assessment Service receives the event  
**Then** it automatically creates default assessment assignments for the student  
**And** assigns grade-appropriate assessments  
**When** the Section Service receives the event  
**Then** it makes the student available for class roster assignment  
**When** the Reporting Service receives the event  
**Then** it updates aggregate student count metrics  
**And** all services process the event within 5 seconds

---

## Scenario 3: Update Student Demographics

**Given** a student record exists with outdated demographic information  
**And** a staff member has permission to update demographics  
**When** they update the student's address and contact information  
**Then** the Student Service validates the changes  
**And** updates the `StudentDemographics` table  
**And** publishes a `StudentDemographicsChangedEvent`  
**And** the event includes what changed (delta)  
**And** the audit log records who made the change  
**And** other services can react to the demographic change if needed

---

## Scenario 4: Search Students with Tenant Isolation

**Given** a teacher from District A is searching for students  
**And** the database contains students from multiple districts  
**When** they search for students with last name "Smith"  
**Then** the Student Service applies tenant filtering automatically  
**And** only returns students from District A with last name "Smith"  
**And** students from other districts are never visible  
**And** the search results are returned within 100ms (P95)  
**And** the search supports wildcards and partial matching

---

## Scenario 5: Enroll Student in School

**Given** a student exists but is not yet enrolled  
**When** an administrator enrolls the student in "Washington Middle School"  
**Then** a `StudentEnrollment` record is created  
**And** the enrollment includes: student_id, school_id, enrollment_date, grade_level, status  
**And** a `StudentEnrolledEvent` is published  
**And** the Section Service receives the event and enables roster assignment  
**And** the Intervention Service receives the event and enables intervention enrollment  
**And** the student's status changes to "Active"

---

## Scenario 6: Bulk Student Import via CSV

**Given** a district has 500 new students to import  
**And** the data is provided in CSV format with required columns  
**When** an administrator uploads the CSV file  
**Then** the Student Service validates all rows  
**And** identifies any data quality issues (missing fields, invalid dates, etc.)  
**And** displays validation errors before import  
**And** if validation passes, imports all 500 students in a single transaction  
**And** publishes `StudentCreatedEvent` for each student  
**And** provides a summary report: 500 created, 0 errors  
**And** the import completes within 2 minutes

---

## Scenario 7: Student Dashboard Query Optimization

**Given** a teacher wants to view a student's complete dashboard  
**And** the dashboard shows: demographics, enrollments, assessments, interventions  
**When** they request the student dashboard  
**Then** the Student Service retrieves student core data  
**And** makes parallel API calls to: Assessment Service, Intervention Service, Section Service  
**And** aggregates all data into a unified dashboard response  
**And** the response is returned within 200ms (P95)  
**And** the dashboard includes data from multiple microservices seamlessly

---

## Scenario 8: Handle Student Withdrawal

**Given** a student is enrolled and active in the system  
**When** an administrator processes a student withdrawal  
**Then** the Student Service updates the enrollment status to "Withdrawn"  
**And** sets the withdrawal_date to today  
**And** publishes a `StudentWithdrawnEvent`  
**And** the Section Service removes the student from active rosters  
**And** the Assessment Service marks pending assessments as cancelled  
**And** the student's data is retained for historical reporting  
**And** the student is hidden from active student lists

---

## Scenario 9: Student Data Privacy and FERPA Compliance

**Given** student data is protected under FERPA regulations  
**And** only authorized staff can access student records  
**When** a teacher attempts to view a student they don't teach  
**Then** the Student Service checks authorization via Identity Service  
**And** validates the teacher has a legitimate educational interest  
**And** denies access if no relationship exists  
**And** logs the access attempt for audit purposes  
**And** all student data access is tracked for compliance reporting

---

## Scenario 10: Student Merge After Duplicate Detection

**Given** two student records exist for the same physical student due to data entry error  
**And** Student A (ID: 123) and Student B (ID: 456) are identified as duplicates  
**When** an administrator initiates a merge operation  
**Then** the Student Service prompts to select the primary record (keep 123, merge 456)  
**And** all enrollments, assessments, and relationships from 456 are reassigned to 123  
**And** Student 456 is marked as merged (soft delete with reference to 123)  
**And** a `StudentMergedEvent` is published  
**And** all other services update their references from 456 to 123  
**And** historical data from both records is preserved

---

## Scenario 11: Student Photo Upload and Management

**Given** schools need to store student photos for ID cards  
**When** a staff member uploads a student photo  
**Then** the photo is uploaded to Azure Blob Storage  
**And** the file is stored with path: `{tenant_id}/students/{student_id}/photo.jpg`  
**And** the Student Service stores the blob URL in the database  
**And** only users from the same tenant can access the photo  
**And** the photo is displayed in the student profile  
**And** photos are automatically resized to standard dimensions (200x200px)

---

## Scenario 12: Student Export for State Reporting

**Given** the state requires annual student enrollment data submission  
**When** a district administrator requests a state report export  
**Then** the Student Service queries all active students for the tenant  
**And** formats the data according to state specifications  
**And** includes required fields: state_student_id, demographics, enrollment status  
**And** excludes confidential fields not required by state  
**And** generates a CSV file with proper encoding  
**And** logs the export for audit purposes  
**And** provides download link valid for 24 hours

---

## Technical Implementation Notes

**Clean Architecture Structure**:
```
NorthStar.Students/
├── Domain/
│   ├── Entities/
│   │   ├── Student.cs
│   │   ├── StudentEnrollment.cs
│   │   └── StudentDemographics.cs
│   ├── ValueObjects/
│   │   ├── StudentId.cs
│   │   └── GradeLevel.cs
│   └── Events/
│       ├── StudentCreatedEvent.cs
│       ├── StudentEnrolledEvent.cs
│       └── StudentWithdrawnEvent.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateStudentCommand.cs
│   │   ├── UpdateStudentCommand.cs
│   │   └── EnrollStudentCommand.cs
│   ├── Queries/
│   │   ├── GetStudentQuery.cs
│   │   ├── SearchStudentsQuery.cs
│   │   └── GetStudentDashboardQuery.cs
│   ├── Validators/
│   │   └── CreateStudentValidator.cs
│   └── Interfaces/
│       └── IStudentRepository.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── StudentDbContext.cs
│   │   └── StudentRepository.cs
│   └── Integration/
│       └── EventPublisher.cs
└── Api/
    └── Controllers/
        └── StudentsController.cs
```

**CQRS Command Example**:
```csharp
public record CreateStudentCommand(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    int GradeLevel,
    Guid SchoolId
) : IRequest<StudentDto>;

public class CreateStudentCommandHandler 
    : IRequestHandler<CreateStudentCommand, StudentDto>
{
    private readonly IStudentRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ITenantContext _tenantContext;
    
    public async Task<StudentDto> Handle(
        CreateStudentCommand command, 
        CancellationToken ct)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            FirstName = command.FirstName,
            LastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            GradeLevel = command.GradeLevel
        };
        
        await _repository.AddAsync(student);
        
        await _eventPublisher.PublishAsync(new StudentCreatedEvent
        {
            StudentId = student.Id,
            TenantId = student.TenantId,
            SchoolId = command.SchoolId,
            GradeLevel = command.GradeLevel,
            Timestamp = DateTime.UtcNow
        });
        
        return MapToDto(student);
    }
}
```

**Event Publishing**:
```csharp
public class EventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    
    public async Task PublishAsync<T>(T @event) where T : class
    {
        await _publishEndpoint.Publish(@event);
    }
}
```

**Database Schema**:
```sql
CREATE TABLE student.students (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    legacy_id INTEGER,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    middle_name VARCHAR(100),
    date_of_birth DATE NOT NULL,
    grade_level INTEGER,
    state_student_id VARCHAR(50),
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

-- Row-Level Security
ALTER TABLE student.students ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON student.students
    USING (tenant_id = current_setting('app.current_tenant')::uuid);

-- Indexes
CREATE INDEX idx_students_tenant ON student.students(tenant_id);
CREATE INDEX idx_students_name ON student.students(tenant_id, last_name, first_name);
CREATE INDEX idx_students_grade ON student.students(tenant_id, grade_level);
```

**API Endpoints**:
- `POST /api/v1/students` - Create student
- `GET /api/v1/students/{id}` - Get student by ID
- `PUT /api/v1/students/{id}` - Update student
- `DELETE /api/v1/students/{id}` - Soft delete student
- `GET /api/v1/students` - Search students (with filters)
- `GET /api/v1/students/{id}/dashboard` - Get student dashboard
- `POST /api/v1/students/import` - Bulk import from CSV
- `GET /api/v1/students/export` - Export for state reporting

**Domain Events Published**:
- `StudentCreatedEvent`
- `StudentUpdatedEvent`
- `StudentEnrolledEvent`
- `StudentWithdrawnEvent`
- `StudentDemographicsChangedEvent`
- `StudentMergedEvent`

**Events Subscribed**:
- `SectionCreatedEvent` (from Section Service)
- `SchoolClosedEvent` (from Configuration Service)

**Performance SLOs**:
- Create student: <100ms (P95)
- Search students: <100ms (P95)
- Get student dashboard: <200ms (P95) including cross-service calls
- Bulk import 500 students: <120 seconds
- Event publishing: <50ms

**Testing Requirements**:
- Unit tests: ≥80% coverage
- Integration tests: All repository methods, event publishing
- BDD tests: All user scenarios (Reqnroll)
- Performance tests: Load testing for search and bulk operations
