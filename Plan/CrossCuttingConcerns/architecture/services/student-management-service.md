# Student Management Service

## Overview

The Student Management Service is the core domain service responsible for managing student information, enrollment, demographics, and academic records within the NorthStar LMS platform.

## Service Classification

- **Type**: Core Domain Service
- **Phase**: Phase 2 (Weeks 9-16)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Student/`
- **Priority**: High (foundational for assessment and intervention features)

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/StudentController.cs`, `NS4.WebAPI/Controllers/StudentDashboardController.cs`  
**Data Services**: `StudentDataService`  
**Database**: `DistrictContext` (shared monolithic database)

**Key Tables**:
- Student (demographics, enrollment status)
- StudentSection (class enrollment)
- StudentAttributeData (custom attributes)
- StudentContact (guardian information)

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Student.API/                    # UI Layer
├── Controllers/
│   ├── StudentsController.cs
│   └── StudentDashboardController.cs
├── Middleware/
└── Program.cs

Student.Application/            # Application Layer
├── Commands/
│   ├── EnrollStudentCommand.cs
│   ├── UpdateStudentCommand.cs
│   └── WithdrawStudentCommand.cs
├── Queries/
│   ├── GetStudentByIdQuery.cs
│   ├── GetStudentsByGradeQuery.cs
│   └── GetStudentDashboardQuery.cs
├── DTOs/
└── Validators/

Student.Domain/                # Domain Layer
├── Entities/
│   ├── Student.cs             # Aggregate root
│   ├── StudentEnrollment.cs
│   ├── StudentDemographics.cs
│   └── StudentContact.cs
├── Events/
│   ├── StudentCreatedEvent.cs
│   ├── StudentEnrolledEvent.cs
│   ├── StudentWithdrawnEvent.cs
│   └── StudentDemographicsChangedEvent.cs
├── ValueObjects/
│   ├── StudentId.cs
│   ├── Grade.cs
│   └── EnrollmentStatus.cs
└── Repositories/
    └── IStudentRepository.cs

Student.Infrastructure/        # Infrastructure Layer
├── Data/
│   ├── StudentDbContext.cs
│   ├── Repositories/
│   │   └── StudentRepository.cs
│   └── Migrations/
└── MessageBus/
    └── EventPublisher.cs
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core Web API
- **Data Access**: EF Core with SQL Server
- **Messaging**: MassTransit + Azure Service Bus
- **Caching**: Redis for frequently accessed student data
- **Orchestration**: .NET Aspire hosting

### Owned Data

**Database**: `NorthStar_Student_DB`

**Tables**:
- Students (Id, FirstName, LastName, DateOfBirth, Grade, EnrollmentStatus, DistrictId)
- StudentEnrollments (Id, StudentId, SchoolId, EnrollDate, WithdrawDate, Status)
- StudentDemographics (StudentId, Gender, Ethnicity, ELL, SPED, FreeReducedLunch)
- StudentContacts (Id, StudentId, ContactName, Relationship, Phone, Email, IsPrimary)
- StudentAttributes (StudentId, AttributeKey, AttributeValue) - for custom district data
- StudentNotes (Id, StudentId, NoteText, CreatedBy, CreatedDate)

### Service Boundaries

**Owned Responsibilities**:
- Student CRUD operations (create, read, update, deactivate)
- Student enrollment management (enroll, transfer, withdraw)
- Student demographics management
- Student contact information
- Student custom attributes
- Student search and filtering
- Student dashboard aggregation

**Not Owned** (delegated):
- Assessment scores → Assessment Service
- Intervention assignments → Intervention Service
- Section/class rosters → Section Service
- Attendance records → Attendance Service (future)

### Domain Events Published

**Event Schema Version**: 1.0

- `StudentCreatedEvent`
  ```
  - StudentId: Guid
  - FirstName: string
  - LastName: string
  - Grade: int
  - DistrictId: Guid
  - CreatedAt: DateTime
  ```

- `StudentEnrolledEvent`
  ```
  - StudentId: Guid
  - SchoolId: Guid
  - EnrollmentDate: DateTime
  - Grade: int
  ```

- `StudentWithdrawnEvent`
  ```
  - StudentId: Guid
  - WithdrawDate: DateTime
  - Reason: string
  ```

- `StudentDemographicsChangedEvent`
  ```
  - StudentId: Guid
  - Changes: Dictionary<string, object>
  - ChangedBy: Guid
  - ChangedAt: DateTime
  ```

- `StudentGradePromotedEvent`
  ```
  - StudentId: Guid
  - FromGrade: int
  - ToGrade: int
  - PromotionDate: DateTime
  ```

### Domain Events Subscribed

- `SectionCreatedEvent` (from Section Service) - for roster suggestions
- `AssessmentCompletedEvent` (from Assessment Service) - for dashboard updates

### API Endpoints (Functional Intent)

**Student Management**:
- Create new student → registers student in system
- Get student by ID → retrieves student details
- Update student information → modifies demographics
- Search students → finds students by criteria
- Deactivate student → marks as inactive (soft delete)

**Enrollment**:
- Enroll student in school → creates enrollment record
- Transfer student → withdraws from old school, enrolls in new
- Withdraw student → ends enrollment
- Get enrollment history → lists all enrollments

**Dashboard**:
- Get student dashboard → aggregates student data, recent assessments, interventions
- Get students by grade → lists all students in grade level
- Get students by teacher → lists teacher's students

### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime
- **Read Operations**: p95 < 100ms (with Redis caching)
- **Write Operations**: p95 < 300ms
- **Search Operations**: p95 < 500ms
- **Dashboard Aggregation**: p95 < 800ms (async event-driven)

### Idempotency & Consistency

**Idempotency Windows**:
- Student creation: 10 minutes (prevent duplicate entries by name+DOB)
- Enrollment operations: 5 minutes
- Demographics updates: 1 minute

**Consistency Model**:
- Strong consistency for student CRUD operations within service
- Eventual consistency for cross-service data (dashboards updated via events)

### Security Considerations

**Constitutional Requirements**:
- RBAC enforcement in Application layer
- Teachers can only view their assigned students
- Administrators can view all students in their district
- No direct UI → Infrastructure coupling

**Data Privacy**:
- FERPA compliance for student data
- PII protection (names, DOB, contacts)
- Audit logging for all student data access
- Data retention policies for withdrawn students

### Testing Requirements

**BDD Features** (Reqnroll):
- `StudentEnrollment.feature` - New student registration
- `StudentTransfer.feature` - Student transfer between schools
- `StudentDashboard.feature` - Dashboard data aggregation
- `StudentSearch.feature` - Search and filtering

**Unit Tests**:
- Student aggregate business rules
- Grade promotion logic
- Enrollment validation
- Search query building

**Integration Tests**:
- EF Core database operations
- Event publishing to message bus
- Redis caching behavior
- Aspire service orchestration

**UI Tests** (Playwright):
- Student registration form (Figma-backed)
- Student search page
- Student detail view
- Enrollment workflow

### Dependencies

**Upstream Services**:
- Identity Service - User authentication
- Configuration Service - District/school data

**Downstream Services** (consumers of events):
- Assessment Service - Student context for assessments
- Intervention Service - Student selection for interventions
- Section Service - Roster management
- Reporting Service - Student analytics

**Infrastructure**:
- SQL Server - Student database
- Azure Service Bus - Event publishing
- Redis - Student data caching
- .NET Aspire - Orchestration

### Migration Strategy

**Data Migration**:
1. Extract Student tables from `DistrictContext`
2. Create new `NorthStar_Student_DB` per district
3. Migrate existing student data with transformation
4. Maintain referential integrity

**Strangler Fig Approach**:
1. **Week 9**: Deploy Student Service, read-only mode
   - API Gateway routes GET requests to new service
   - POST/PUT still go to legacy monolith
   
2. **Week 10**: Enable write operations
   - Route student creation to new service
   - Publish events to message bus
   - Dual-write to legacy DB for compatibility

3. **Week 11**: Gradual migration
   - Batch migrate existing students during low-traffic hours
   - Validate data integrity
   - Update API Gateway routing

4. **Week 12**: Complete cutover
   - All operations routed to Student Service
   - Remove dual-write to legacy DB
   - Monitor and optimize

### Configuration

**Aspire Service Defaults**:
```csharp
builder.AddServiceDefaults();
builder.AddSqlServerDbContext<StudentDbContext>("StudentDb");
builder.AddRedis("cache");
builder.AddRabbitMQ("messaging");
```

**Environment Variables**:
- `ConnectionStrings__StudentDb`
- `Redis__ConnectionString`
- `MessageBus__ConnectionString`
- `DistrictId` - For multi-tenant isolation

### Monitoring & Observability

**Metrics**:
- Student operations per second
- Search query latency
- Dashboard load time
- Event publishing lag
- Cache hit ratio

**Alerts**:
- Student creation failure rate > 1%
- Search response time > 1 second
- Event publishing failures
- Database connection pool exhaustion

## Implementation Checklist

- [ ] Create Clean Architecture project structure
- [ ] Define Student aggregate and domain events
- [ ] Implement EF Core DbContext and migrations
- [ ] Create command and query handlers
- [ ] Configure MassTransit event publishing
- [ ] Add Redis caching for read operations
- [ ] Write Reqnroll BDD features
- [ ] Implement TDD unit tests (Red → Green)
- [ ] Create Aspire integration tests
- [ ] Set up API controllers with authorization
- [ ] Configure health checks and metrics
- [ ] Deploy to staging and test
- [ ] Execute data migration from legacy
- [ ] Monitor production deployment

## Related Documentation

- [Identity Service](./identity-service.md) - Authentication integration
- [Assessment Service](./assessment-service.md) - Assessment data relationship
- [Bounded Contexts](../architecture/bounded-contexts.md) - Service boundaries

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete, Ready for Implementation
