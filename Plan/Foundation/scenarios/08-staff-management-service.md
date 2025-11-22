# Staff Management Service Migration

**Feature**: Migrate Staff Management from Monolith to Microservice  
**Epic**: Phase 2 - Core Domain Services (Weeks 9-16)  
**Service**: Staff Management Service  
**Business Value**: Modern staff data management with team collaboration

---

## Scenario 1: Create Staff Member with Role Assignment

**Given** a district administrator is adding a new teacher  
**And** they navigate to the "Add Staff" form  
**When** they enter staff details:
  - First Name: "Emily"
  - Last Name: "Rodriguez"
  - Email: "erodriguez@lincolnusd.edu"
  - Role: "Teacher"
  - Subject Specialization: "Mathematics"
  - Hire Date: "August 1, 2025"
**And** they assign the staff member to "Washington Elementary"  
**And** they submit the form  
**Then** the Staff Service creates a new staff record with `tenant_id`  
**And** publishes a `StaffCreatedEvent`  
**And** the Identity Service creates a user account for login  
**And** the staff member receives a welcome email  
**And** they can log in and access their dashboard

---

## Scenario 2: Staff Profile Management and Updates

**Given** a staff member "Emily Rodriguez" exists in the system  
**And** she wants to update her professional information  
**When** she updates her profile:
  - Phone: "(555) 123-4567"
  - Emergency Contact: "Carlos Rodriguez"
  - Certifications: "Math Teaching Credential (CA)"
  - Degree: "M.Ed. in Mathematics Education"
**And** saves the changes  
**Then** the profile is updated with tenant isolation  
**And** a `StaffProfileUpdatedEvent` is published  
**And** the changes are reflected immediately  
**And** the audit log records who made the update

---

## Scenario 3: Team Creation and Member Assignment

**Given** several teachers need to collaborate as a Professional Learning Community  
**When** an administrator creates a team:
  - Team Name: "5th Grade Math PLC"
  - Team Lead: "Emily Rodriguez"
  - Members: 4 math teachers
  - Meeting Schedule: "Every Tuesday 3:00 PM"
  - Purpose: "Curriculum alignment and best practices sharing"
**And** saves the team  
**Then** the team is created with tenant isolation  
**And** all members are notified via email  
**And** the team appears in each member's dashboard  
**And** the team can collaborate and share resources

---

## Scenario 4: Staff Multi-School Assignment

**Given** a staff member works at multiple schools  
**And** "Dr. Martinez" is a district-level curriculum coordinator  
**When** an administrator assigns Dr. Martinez to:
  - Washington Elementary (0.3 FTE)
  - Lincoln Middle School (0.3 FTE)
  - Jefferson High School (0.4 FTE)
**Then** the Staff Service creates multiple `StaffAssignment` records  
**And** each assignment includes school_id and FTE percentage  
**And** Dr. Martinez can switch between school contexts  
**And** her schedule reflects all three school assignments  
**And** each school sees her assigned time

---

## Scenario 5: Staff Role and Permission Management

**Given** different staff roles have different system permissions  
**When** a staff member is assigned the "Department Chair" role  
**Then** they receive additional permissions:
  - View all teachers in their department
  - Approve curriculum materials
  - Access department-level reports
  - Manage department team membership
**And** permissions are enforced via RBAC  
**And** the Identity Service validates permissions on each request  
**And** unauthorized actions are denied

---

## Scenario 6: Staff Search and Filtering

**Given** a principal needs to find staff by specific criteria  
**When** they search for staff with filters:
  - School: "Washington Elementary"
  - Role: "Teacher"
  - Subject: "Mathematics"
  - Status: "Active"
**Then** the search returns matching staff members  
**And** only staff from the same tenant are visible  
**And** results are sorted by last name  
**And** the search completes within 100ms (P95)  
**And** pagination is supported for large result sets

---

## Scenario 7: Staff Schedule and Availability

**Given** a teacher has a complex teaching schedule  
**When** they set their availability:
  - Monday-Friday: 8:00 AM - 3:30 PM (Teaching)
  - Tuesday/Thursday: 3:30 PM - 4:30 PM (Tutoring)
  - Wednesday: 2:00 PM - 3:00 PM (PLC Meetings)
  - Planning Period: Daily 1:00 PM - 1:45 PM
**Then** the schedule is stored in the Staff Service  
**And** other systems can query availability  
**And** the Section Service uses it for class scheduling  
**And** conflicts are detected automatically

---

## Scenario 8: Staff Certification and Credential Tracking

**Given** staff members need valid teaching credentials  
**When** a staff member's certificate is about to expire  
**Then** the system sends a notification 60 days before expiration  
**And** administrators receive an alert  
**And** the staff member's status shows "Credential Expiring Soon"  
**When** they upload a renewed certificate  
**Then** the expiration date is updated  
**And** the status changes back to "Active"  
**And** compliance reports reflect current certification status

---

## Scenario 9: Staff Performance Review Workflow

**Given** annual performance reviews are due  
**When** an administrator initiates a review cycle  
**Then** all staff members are notified  
**And** self-assessment forms are distributed  
**And** supervisors complete evaluation forms  
**And** one-on-one meetings are scheduled  
**When** the review is completed  
**Then** the review is stored with historical reviews  
**And** goals for next year are documented  
**And** the staff member can view their review history

---

## Scenario 10: Staff Bulk Import via CSV

**Given** a district is onboarding 50 new staff members  
**And** staff data is provided in CSV format  
**When** an administrator uploads the CSV file  
**Then** the Staff Service validates all rows  
**And** identifies any data quality issues  
**And** if validation passes, imports all 50 staff records  
**And** publishes `StaffCreatedEvent` for each  
**And** creates user accounts in Identity Service  
**And** sends welcome emails to all new staff  
**And** provides a summary report: 50 created, 0 errors

---

## Scenario 11: Staff Directory and Contact Information

**Given** staff need to contact colleagues  
**When** a teacher searches the staff directory  
**Then** they see all staff in their district with:
  - Name and photo
  - Role and department
  - School assignment(s)
  - Email and phone (if permitted)
  - Office location
**And** privacy settings are respected  
**And** personal contact info is hidden based on preferences  
**And** the directory is searchable and filterable

---

## Scenario 12: Staff Audit Trail and Compliance

**Given** staff changes must be audited for compliance  
**When** any staff record is modified  
**Then** an audit log entry is created with:
  - User who made the change
  - Timestamp
  - Fields changed (before/after values)
  - Reason for change (if provided)
**And** the audit trail is immutable  
**And** administrators can generate compliance reports  
**And** the log is retained for required retention period  
**And** all logs are tenant-isolated

---

## Architectural Appendix

### Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` project (legacy monolith)  
**Framework**: .NET Framework 4.6  
**Database**: Per-district SQL Server databases

**Key Legacy Components**:
- `NS4.WebAPI/Controllers/StaffController.cs` - CRUD operations for staff profiles
- `NS4.WebAPI/Controllers/TeamsController.cs` - Professional Learning Community (PLC) teams
- `NS4.WebAPI/Controllers/AssignmentController.cs` - Staff-to-school assignments
- Legacy tables: `Staff`, `StaffAssignments`, `Teams`, `TeamMembers`, `Certifications`
- Tight coupling with identity management in single database
- Limited multi-school assignment support

**Legacy Limitations**:
- No event-driven coordination with other services
- Manual certification expiration tracking
- Limited FTE (full-time equivalent) management
- No real-time collaboration for team management
- Tight database coupling prevents independent scaling

### Target State (.NET 10 Microservice)

#### Architecture

**Clean Architecture Layers**:
```
Staff.API/                     # UI Layer (REST endpoints)
├── Controllers/
├── Middleware/
└── Program.cs

Staff.Application/             # Application Layer
├── Commands/                 # Create staff, assign to school, create team
├── Queries/                  # Get staff, search, get schedule
├── DTOs/
├── Interfaces/
└── Services/
    └── CertificationTracker/ # Monitor expiring credentials

Staff.Domain/                 # Domain Layer
├── Entities/
│   ├── StaffMember.cs
│   ├── StaffAssignment.cs
│   ├── Team.cs
│   ├── TeamMember.cs
│   └── Certification.cs
├── Events/
│   ├── StaffCreatedEvent.cs
│   ├── StaffAssignedToSchoolEvent.cs
│   ├── TeamCreatedEvent.cs
│   ├── TeamMemberAddedEvent.cs
│   └── CertificationExpiringEvent.cs
└── ValueObjects/
    ├── StaffId.cs
    ├── FtePercentage.cs
    └── Schedule.cs

Staff.Infrastructure/         # Infrastructure Layer
├── Data/
│   ├── StaffDbContext.cs
│   └── Repositories/
├── Integration/
│   ├── EventPublisher.cs
│   ├── IdentityServiceClient.cs  // Create user accounts
│   └── ConfigurationServiceClient.cs  // Validate schools
└── BackgroundJobs/
    └── CertificationMonitorJob.cs  // Alert on expiring credentials
```

#### Technology Stack

- **Framework**: .NET 10, ASP.NET Core
- **Data Access**: EF Core 9 with PostgreSQL
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Caching**: Redis Stack for staff profile lookups
- **Orchestration**: .NET Aspire hosting
- **Background Jobs**: Hangfire for certification expiration monitoring

#### Owned Data

**Database**: `NorthStar_Staff_DB`

**Tables**:
- StaffMembers (Id, TenantId, FirstName, LastName, Email, Phone, HireDate, EmploymentStatus, Role, SubjectSpecialization, CreatedAt, UpdatedAt, DeletedAt)
- StaffAssignments (Id, TenantId, StaffId, SchoolId, FtePercentage, StartDate, EndDate, IsActive)
- Teams (Id, TenantId, TeamName, TeamLeadId, Purpose, MeetingSchedule, CreatedAt)
- TeamMembers (Id, TenantId, TeamId, StaffId, JoinedDate, Role)
- Certifications (Id, TenantId, StaffId, CertificationType, IssueDate, ExpirationDate, IssuingAuthority, Status)
- StaffSchedules (Id, TenantId, StaffId, DayOfWeek, StartTime, EndTime, ActivityType)

#### Service Boundaries

**Owned Responsibilities**:
- Staff profile creation and management
- Multi-school assignment tracking (FTE percentages)
- Team creation and member management
- Certification and credential tracking
- Staff schedule and availability management
- Staff search and directory
- Certification expiration notifications
- Performance review workflow (future)

**Not Owned** (delegated to other services):
- User account creation → Identity Service (but Staff Service triggers it)
- School validation → Configuration Service
- Section/class assignments → Section & Roster Service
- Intervention facilitation → Intervention Service
- Staff analytics → Reporting & Analytics Service

#### Domain Events Published

**Event Schema Version**: 1.0 (follows [domain-events-schema.md](../../CrossCuttingConcerns/architecture/domain-events-schema.md))

- `StaffCreatedEvent` - When new staff member added
  ```
  - StaffId: Guid
  - TenantId: Guid
  - Email: string
  - Role: string
  - CreatedBy: Guid
  - OccurredAt: DateTime
  ```

- `StaffAssignedToSchoolEvent` - When staff assigned to school
  ```
  - AssignmentId: Guid
  - TenantId: Guid
  - StaffId: Guid
  - SchoolId: Guid
  - FtePercentage: decimal
  - StartDate: DateTime
  - OccurredAt: DateTime
  ```

- `TeamCreatedEvent` - When PLC team formed
  ```
  - TeamId: Guid
  - TenantId: Guid
  - TeamName: string
  - TeamLeadId: Guid
  - MemberIds: Guid[]
  - OccurredAt: DateTime
  ```

- `TeamMemberAddedEvent` - When staff added to team
  ```
  - TeamId: Guid
  - TenantId: Guid
  - StaffId: Guid
  - Role: string
  - OccurredAt: DateTime
  ```

- `CertificationExpiringEvent` - When credential expires soon
  ```
  - StaffId: Guid
  - TenantId: Guid
  - CertificationType: string
  - ExpirationDate: DateTime
  - DaysUntilExpiration: int
  - OccurredAt: DateTime
  ```

#### Domain Events Subscribed

- `UserCreatedEvent` (from Identity Service) → Link user account to staff profile
- `SchoolCreatedEvent` (from Configuration Service) → Enable staff assignment to new school

#### API Endpoints (Functional Intent)

**Staff Management**:
- Create staff member → returns staff details, triggers user account creation
- Update staff profile → modifies staff data
- Get staff details → returns full profile with assignments
- Search staff → returns filtered list with pagination

**Assignment Management**:
- Assign to school → creates multi-school assignment record
- Update FTE percentage → adjusts workload allocation
- Get staff assignments → returns all school assignments

**Team Management**:
- Create team → forms PLC or collaboration team
- Add team member → enrolls staff in team
- Remove team member → removes from team
- Get team details → returns team with member list

**Certification Management**:
- Add certification → records credential
- Update expiration date → renews credential
- Get expiring certifications → returns list for proactive renewal

#### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime
- **Create Staff**: p95 < 100ms (triggers async user account creation)
- **Search Staff**: p95 < 100ms
- **Get Staff Schedule**: p95 < 150ms
- **Bulk Import (50 staff)**: < 60 seconds

#### Idempotency & Consistency

**Idempotency Windows**:
- Staff creation: 10 minutes (duplicate email prevention)
- Assignment creation: 5 minutes (prevent duplicate school assignment)

**Consistency Model**:
- Strong consistency for staff profile updates
- Eventual consistency for user account creation (Identity Service)
- Eventual consistency for cross-service data (school names)

#### Security Considerations

**Constitutional Requirements**:
- Enforce least privilege principle
- FERPA compliance for staff-student relationships
- Secrets stored in Azure Key Vault only

**Implementation**:
- Staff can view own profile
- Principals can view staff at their schools
- District admins can view all staff
- HR role required for certification data access
- All profile changes audited for compliance

#### Testing Requirements

**Constitutional Compliance**:
- Reqnroll BDD features before implementation
- TDD Red → Green with test evidence
- ≥ 80% code coverage

**Test Categories**:

1. **Unit Tests** (Staff.UnitTests):
   - FTE percentage validation
   - Certification expiration logic
   - Schedule conflict detection
   - Team membership rules

2. **Integration Tests** (Staff.IntegrationTests):
   - Database operations via EF Core
   - Event publishing to message bus
   - Identity Service integration (user creation)
   - Configuration Service integration (school validation)
   - Aspire orchestration validation

3. **BDD Tests** (Reqnroll features):
   - `StaffManagement.feature` - Create and update staff
   - `MultiSchoolAssignment.feature` - FTE tracking
   - `TeamManagement.feature` - PLC collaboration
   - `CertificationTracking.feature` - Credential monitoring

4. **UI Tests** (Playwright):
   - Staff creation form
   - Assignment management workflow
   - Team creation interface
   - Certification renewal workflow

#### Dependencies

**External Services**:
- Identity Service - User account creation (async)
- Configuration Service - School validation and lookup
- Section & Roster Service - Teacher-section assignments (consumes events)
- Intervention Service - Facilitator assignments (consumes events)
- Azure Service Bus - Event publishing

**Infrastructure Dependencies**:
- PostgreSQL - Staff database
- .NET Aspire AppHost - Service orchestration
- Hangfire - Background job for certification monitoring

#### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 2a** (Week 9): Deploy new Staff Service alongside legacy
   - Route new staff creation to new service
   - Legacy staff continue in NS4.WebAPI
   - Dual-read from both systems

2. **Phase 2b** (Week 10-11): Data migration
   - Migrate historical staff records
   - Migrate multi-school assignments
   - Migrate team memberships and certifications
   - Maintain both systems in parallel

3. **Phase 2c** (Week 12): Complete cutover
   - Route all staff operations to new service
   - Keep legacy as read-only fallback
   - Monitor error rates

4. **Phase 2d** (Week 13-14): Enhanced features
   - Add certification expiration monitoring
   - Add advanced team collaboration features
   - Integrate with Section Service for class assignments

5. **Phase 2e** (Week 15-16): Decommission legacy
   - Verify all data migrated
   - Archive legacy staff tables
   - Remove legacy controllers

---

## Technical Implementation Notes

**Clean Architecture**:
```
NorthStar.Staff/
├── Domain/
│   ├── Entities/
│   │   ├── StaffMember.cs
│   │   ├── StaffAssignment.cs
│   │   ├── Team.cs
│   │   └── TeamMember.cs
│   ├── ValueObjects/
│   │   ├── StaffId.cs
│   │   ├── Certification.cs
│   │   └── Schedule.cs
│   └── Events/
│       ├── StaffCreatedEvent.cs
│       ├── StaffAssignedEvent.cs
│       └── TeamCreatedEvent.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateStaffCommand.cs
│   │   ├── UpdateStaffCommand.cs
│   │   └── AssignStaffToSchoolCommand.cs
│   ├── Queries/
│   │   ├── GetStaffQuery.cs
│   │   ├── SearchStaffQuery.cs
│   │   └── GetStaffScheduleQuery.cs
│   └── Validators/
│       └── StaffValidator.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── StaffDbContext.cs
│   │   └── StaffRepository.cs
│   └── Integration/
│       └── EventPublisher.cs
└── Api/
    └── Controllers/
        ├── StaffController.cs
        └── TeamsController.cs
```

**Database Schema**:
```sql
CREATE TABLE staff.staff_members (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    legacy_id INTEGER,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(200) UNIQUE NOT NULL,
    phone VARCHAR(20),
    hire_date DATE,
    employment_status VARCHAR(50),
    role VARCHAR(100),
    subject_specialization VARCHAR(100),
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE staff.staff_assignments (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    staff_id UUID NOT NULL,
    school_id UUID NOT NULL,
    fte_percentage DECIMAL(3,2),
    start_date DATE,
    end_date DATE,
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE staff.teams (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    team_name VARCHAR(200) NOT NULL,
    team_lead_id UUID,
    purpose TEXT,
    meeting_schedule VARCHAR(200),
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE staff.team_members (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    team_id UUID NOT NULL,
    staff_id UUID NOT NULL,
    joined_date DATE,
    role VARCHAR(50)
);

-- Row-Level Security
ALTER TABLE staff.staff_members ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON staff.staff_members
    USING (tenant_id = current_setting('app.current_tenant')::uuid);
```

**API Endpoints**:
- `POST /api/v1/staff` - Create staff member
- `GET /api/v1/staff/{id}` - Get staff details
- `PUT /api/v1/staff/{id}` - Update staff
- `DELETE /api/v1/staff/{id}` - Soft delete staff
- `GET /api/v1/staff` - Search staff with filters
- `POST /api/v1/staff/{id}/assignments` - Assign to school
- `GET /api/v1/staff/{id}/schedule` - Get staff schedule
- `POST /api/v1/teams` - Create team
- `GET /api/v1/teams` - List teams
- `POST /api/v1/teams/{id}/members` - Add team member
- `POST /api/v1/staff/import` - Bulk import from CSV

**Domain Events**:
- `StaffCreatedEvent`
- `StaffUpdatedEvent`
- `StaffAssignedToSchoolEvent`
- `StaffCertificationExpiringEvent`
- `TeamCreatedEvent`
- `TeamMemberAddedEvent`

**Performance SLOs**:
- Create staff: <100ms (P95)
- Search staff: <100ms (P95)
- Bulk import 50 staff: <60 seconds
- Event publishing: <50ms

**Security Requirements**:
- Staff can only view own profile (unless admin)
- Principals can view staff at their schools
- District admins can view all staff
- FERPA compliance for staff-student relationships
- Certification data access restricted to HR
