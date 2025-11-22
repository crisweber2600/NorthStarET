# Configuration Service: Multi-Tenant Settings Management

**Target Layer**: CrossCuttingConcerns  
**Service**: Configuration Service  
**Pattern**: Multi-Tenant Configuration Hierarchy  
**Architecture Reference**: [Configuration Service Architecture](../architecture/services/configuration-service.md)  
**Business Value**: Centralized multi-tenant configuration management

---

## Scenario 1: District Administrator Creates District Settings

**Given** a system administrator is logged in with district admin privileges  
**And** they are setting up a new school district  
**When** they enter district configuration:
  - District Name: "Lincoln Unified School District"
  - State: "California"
  - Timezone: "America/Los_Angeles"
  - Academic Year Start: "August 15"
  - Contact Email: "admin@lincolnusd.edu"
**And** they save the configuration  
**Then** the Configuration Service creates a new district record with `tenant_id`  
**And** publishes a `DistrictCreatedEvent`  
**And** the Identity Service creates the initial admin account  
**And** the Student Service prepares for student enrollment  
**And** the district appears in the system district list

---

## Scenario 2: Configure Academic Calendar for School Year

**Given** a district exists with `tenant_id = "lincoln-usd"`  
**And** a district administrator is configuring the calendar  
**When** they define the academic calendar:
  - First Day of School: "August 15, 2025"
  - Last Day of School: "June 5, 2026"
  - Fall Break: "November 25-29, 2025"
  - Winter Break: "December 20, 2025 - January 5, 2026"
  - Spring Break: "March 16-20, 2026"
  - Holidays: MLK Day, Presidents Day, Memorial Day
**And** they save the calendar  
**Then** the calendar is stored with tenant isolation  
**And** all schools in the district inherit this calendar  
**And** the Section Service uses the calendar for scheduling  
**And** the Attendance Service uses it for attendance tracking

---

## Scenario 3: Create School Within District

**Given** a district "Lincoln USD" exists  
**And** a district administrator wants to add a new school  
**When** they create a school:
  - School Name: "Washington Elementary"
  - Principal: "Dr. Sarah Johnson"
  - Address: "123 Main St, Lincoln, CA"
  - Grade Levels: K-5
  - Capacity: 500 students
**And** they save the school  
**Then** the school is created with the district's `tenant_id`  
**And** a `SchoolCreatedEvent` is published  
**And** the school appears in the school directory  
**And** teachers can be assigned to this school  
**And** students can be enrolled in this school

---

## Scenario 4: Configure Grade Levels and Subjects

**Given** a school "Washington Elementary" exists  
**When** an administrator configures grade levels:
  - Kindergarten (K)
  - Grade 1, Grade 2, Grade 3, Grade 4, Grade 5
**And** configures subjects per grade level:
  - K-2: Reading, Math, Science, PE, Art
  - 3-5: Reading, Math, Science, Social Studies, PE, Art, Music
**Then** the grade/subject configuration is saved with tenant isolation  
**And** the Assessment Service uses this for assessment assignments  
**And** the Section Service uses this for class scheduling  
**And** teachers see only relevant grades and subjects

---

## Scenario 5: Multi-Tenant Configuration Isolation

**Given** two districts exist: "Lincoln USD" and "Jefferson USD"  
**And** each has different academic calendars and settings  
**When** a Lincoln USD administrator views configuration  
**Then** they only see Lincoln USD settings  
**And** Lincoln USD calendar dates are displayed  
**And** Jefferson USD data is completely hidden  
**And** Row-Level Security enforces the isolation  
**And** no cross-tenant configuration leakage occurs

---

## Scenario 6: System-Wide Settings vs. District-Specific

**Given** there are system-wide default settings  
**And** there are district-specific override settings  
**When** a configuration value is requested  
**Then** the system checks for district-specific value first  
**And** falls back to system default if no override exists  
**And** district admins can override: grading scale, attendance rules, notification preferences  
**And** district admins cannot override: security policies, data retention rules  
**And** the configuration hierarchy is enforced

---

## Scenario 7: Custom Attributes and Fields

**Given** different districts have unique data requirements  
**When** a district administrator creates custom student attributes:
  - "Transportation Mode" (Bus, Car, Walk)
  - "Dietary Restrictions" (None, Vegetarian, Vegan, Allergies)
  - "After-School Program" (Yes/No)
**Then** the custom attributes are stored in the Configuration Service  
**And** the Student Service extends student records with custom fields  
**And** the custom fields are only visible to that district  
**And** reports can filter/group by custom attributes

---

## Scenario 8: Grading Scale Configuration

**Given** a district wants to configure grading scales  
**When** an administrator defines:
  - Elementary (K-5): E (Exceeds), M (Meets), P (Progressing), N (Needs Support)
  - Middle School (6-8): A (90-100), B (80-89), C (70-79), D (60-69), F (<60)
  - High School (9-12): A+ through F with +/- modifiers
**Then** the grading scales are stored per grade-level range  
**And** the Assessment Service uses these scales for scoring  
**And** the Reporting Service uses them for report cards  
**And** grade conversion logic is centralized

---

## Scenario 9: State-Specific Compliance Settings

**Given** different states have different compliance requirements  
**When** a district in California is configured  
**Then** California-specific fields are enabled:
  - CALPADS reporting requirements
  - CELDT language assessment tracking
  - California state testing schedules
**When** a district in Texas is configured  
**Then** Texas-specific fields are enabled:
  - PEIMS reporting requirements
  - STAAR testing schedules
  - TEA compliance tracking
**And** each district only sees relevant compliance settings

---

## Scenario 10: Navigation Menu Customization

**Given** different user roles need different menu options  
**When** a district admin configures navigation for teachers  
**Then** teachers see: Students, Assessments, Gradebook, Attendance  
**When** configured for administrators  
**Then** administrators see: Dashboard, Reports, Settings, User Management  
**And** the navigation is role-based and tenant-specific  
**And** the UI renders menus dynamically from configuration

---

## Scenario 11: Notification and Email Templates

**Given** districts want to customize communications  
**When** an administrator configures email templates:
  - Welcome Email for new staff
  - Student enrollment confirmation
  - Parent portal invitation
  - Report card notification
**Then** templates are stored with district branding  
**And** templates include merge fields: {StudentName}, {ParentName}, {School}  
**And** emails are sent using district-specific templates  
**And** SMTP settings are configured per district

---

## Scenario 12: Configuration Change Audit Trail

**Given** configuration changes affect entire district operations  
**When** an administrator changes a critical setting  
**Then** the change is logged with: user_id, timestamp, old_value, new_value, setting_name  
**And** an audit trail is maintained for compliance  
**And** administrators can view configuration history  
**And** changes can be reverted if needed  
**And** the audit log is immutable and tenant-isolated

---

## Related Architecture

**Service Architecture**: [Configuration Service Technical Specification](../architecture/services/configuration-service.md)  
**Multi-Tenancy Pattern**: [Multi-Tenant Database Pattern](../patterns/multi-tenant-database.md)  
**Caching Strategy**: [Caching & Performance](../patterns/caching-performance.md)  
**Domain Events**: [Domain Events Schema](../architecture/domain-events-schema.md)

---

## Technical Implementation Notes

**Clean Architecture**:
```
NorthStar.Configuration/
├── Domain/
│   ├── Entities/
│   │   ├── District.cs
│   │   ├── School.cs
│   │   ├── Calendar.cs
│   │   ├── GradeLevel.cs
│   │   └── ConfigurationSetting.cs
│   ├── ValueObjects/
│   │   ├── AcademicYear.cs
│   │   └── GradingScale.cs
│   └── Events/
│       ├── DistrictCreatedEvent.cs
│       ├── SchoolCreatedEvent.cs
│       └── ConfigurationChangedEvent.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateDistrictCommand.cs
│   │   ├── CreateSchoolCommand.cs
│   │   └── UpdateConfigurationCommand.cs
│   ├── Queries/
│   │   ├── GetDistrictSettingsQuery.cs
│   │   ├── GetSchoolsQuery.cs
│   │   └── GetCalendarQuery.cs
│   └── Validators/
│       └── DistrictValidator.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── ConfigurationDbContext.cs
│   │   └── ConfigurationRepository.cs
│   └── Integration/
│       └── EventPublisher.cs
└── Api/
    └── Controllers/
        ├── DistrictsController.cs
        ├── SchoolsController.cs
        └── ConfigurationController.cs
```

**Database Schema**:
```sql
CREATE TABLE configuration.districts (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL UNIQUE,
    district_name VARCHAR(200) NOT NULL,
    state VARCHAR(2) NOT NULL,
    timezone VARCHAR(50),
    academic_year_start DATE,
    contact_email VARCHAR(200),
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ
);

CREATE TABLE configuration.schools (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    district_id UUID NOT NULL,
    school_name VARCHAR(200) NOT NULL,
    principal_name VARCHAR(200),
    address TEXT,
    grade_levels INTEGER[],
    capacity INTEGER,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE configuration.calendars (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    district_id UUID,
    school_id UUID,
    academic_year INTEGER NOT NULL,
    first_day DATE NOT NULL,
    last_day DATE NOT NULL,
    holidays JSONB,
    breaks JSONB
);

CREATE TABLE configuration.settings (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    setting_key VARCHAR(100) NOT NULL,
    setting_value JSONB NOT NULL,
    setting_type VARCHAR(50),
    is_system_default BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL,
    UNIQUE(tenant_id, setting_key)
);

-- Row-Level Security
ALTER TABLE configuration.districts ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON configuration.districts
    USING (tenant_id = current_setting('app.current_tenant')::uuid);

ALTER TABLE configuration.schools ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON configuration.schools
    USING (tenant_id = current_setting('app.current_tenant')::uuid);
```

**API Endpoints**:
- `POST /api/v1/districts` - Create district
- `GET /api/v1/districts/{id}` - Get district details
- `PUT /api/v1/districts/{id}` - Update district
- `GET /api/v1/districts/{id}/schools` - List schools in district
- `POST /api/v1/schools` - Create school
- `GET /api/v1/schools/{id}` - Get school details
- `GET /api/v1/calendars` - Get academic calendar
- `PUT /api/v1/calendars` - Update calendar
- `GET /api/v1/settings` - Get all settings for tenant
- `PUT /api/v1/settings/{key}` - Update specific setting
- `GET /api/v1/grades` - Get grade level configuration
- `GET /api/v1/subjects` - Get subject configuration

**Domain Events**:
- `DistrictCreatedEvent`
- `SchoolCreatedEvent`
- `SchoolClosedEvent`
- `CalendarUpdatedEvent`
- `ConfigurationChangedEvent`
- `GradingScaleChangedEvent`

**Configuration Hierarchy**:
1. **System Defaults** (no tenant_id) - Lowest priority
2. **District Overrides** (tenant_id) - Medium priority
3. **School Overrides** (tenant_id + school_id) - Highest priority

**Performance SLOs**:
- Get settings: <50ms (P95) - heavily cached
- Create district: <200ms
- Update configuration: <100ms
- List schools: <100ms

**Caching Strategy**:
- Configuration settings cached in Redis for 1 hour
- Cache invalidation on ConfigurationChangedEvent
- Per-tenant cache keys
- Distributed cache across all API instances

**Security Requirements**:
- Only district admins can modify district settings
- School admins can only view (not modify) district settings
- School admins can modify school-level settings
- All changes audited
- Row-Level Security enforced
