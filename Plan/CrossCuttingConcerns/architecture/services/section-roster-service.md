# Section & Roster Service

## Overview

The Section & Roster Service manages class sections, student rosters, and automated roster rollover for the NorthStar LMS platform. It handles section creation, student enrollment in classes, teacher assignments, and year-end rollover automation.

## Service Classification

- **Type**: Secondary Domain Service
- **Phase**: Phase 3 (Weeks 17-24)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Section/`
- **Priority**: Medium (core classroom management)
- **LMS Role**: Section and roster management with automated rollover workflows

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` (SectionController, RosterRolloverController)  
**Framework**: .NET Framework 4.6  
**Database**: Shared `NorthStar` database

**Key Components**:
- Section CRUD operations
- Student roster management
- Automated roster rollover (year-end batch process)
- Teacher-section assignments

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Section.API/                    # UI Layer
Section.Application/            # Application Layer (commands, queries, rollover orchestrator)
Section.Domain/                 # Domain Layer (Section, Roster, RolloverRule aggregates)
Section.Infrastructure/         # Infrastructure Layer (EF Core, MassTransit, Worker Service for rollover)
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Data Access**: EF Core with SQL Server
- **Messaging**: MassTransit + Azure Service Bus
- **Caching**: Redis for active sections
- **Orchestration**: .NET Aspire hosting
- **Background Jobs**: .NET Worker Service for automated rollover

### Owned Data

**Database**: `NorthStar_Section_DB`

**Tables**:
- Sections (Id, SchoolId, CourseId, SectionName, GradeLevel, SchoolYear, TeacherId, MaxStudents, IsActive)
- Rosters (Id, SectionId, StudentId, EnrollmentDate, WithdrawalDate, Status)
- RolloverRules (Id, DistrictId, SourceGrade, TargetGrade, AutoPromote, RequireTeacherReview)
- RolloverJobs (Id, SchoolYearFrom, SchoolYearTo, TriggeredBy, StartDate, CompletionDate, Status)
- RolloverLogs (Id, JobId, StudentId, SourceSectionId, TargetSectionId, Action, Timestamp)

### Service Boundaries

**Owned Responsibilities**:
- Section creation and management
- Student roster enrollment and withdrawals
- Teacher-section assignments
- Automated roster rollover configuration
- Rollover job execution and monitoring
- Section capacity management

**Not Owned**:
- Student enrollment data → Student Management Service
- Staff/teacher data → Staff Management Service
- School/grade configurations → Configuration Service
- Assessment assignments → Assessment Service

**Cross-Service Coordination**:
- Subscribe to `StudentEnrolledEvent` for roster eligibility
- Subscribe to `StaffAssignedToSchoolEvent` for teacher availability
- Publish section creation events for dependent services

### Domain Events Published

**Event Schema Version**: 1.0

- `SectionCreatedEvent` - New section created
- `StudentEnrolledInSectionEvent` - Student added to roster
- `StudentWithdrawnFromSectionEvent` - Student removed from roster
- `RolloverJobCompletedEvent` - Year-end rollover finished
- `SectionCapacityReachedEvent` - Section at maximum capacity

### Domain Events Subscribed

- `StudentEnrolledEvent` (from Student Management)
- `StudentWithdrawnEvent` (from Student Management)
- `StaffAssignedToSchoolEvent` (from Staff Management)
- `SchoolYearCreatedEvent` (from Configuration Service)

### API Functional Intent

**Section Management**:
- Create sections for courses/grades
- Update section details
- Assign teachers to sections
- Close/archive sections

**Roster Management**:
- Enroll students in sections
- Withdraw students from sections
- Transfer students between sections
- Query roster by section

**Rollover Management**:
- Configure rollover rules
- Trigger automated rollover jobs
- Review and approve rollover results
- Query rollover job status

### Service Level Objectives (SLOs)

- **Availability**: 99% uptime during school hours
- **Section Retrieval**: < 150ms p95
- **Roster Updates**: < 300ms p95
- **Rollover Job**: Complete within 2 hours for 10,000 students
- **Consistency Model**: Eventually consistent
- **Idempotency Window**: 10 minutes

### Security & Compliance

**Authorization**:
- **District Admin**: Full section/roster management
- **School Admin**: Sections within school
- **Teachers**: View/manage assigned sections
- **Students**: View own schedule (read-only)

**Data Protection**:
- Student roster data encrypted at rest
- FERPA compliance
- Audit logging for enrollments

**Secrets Management**:
- Connection strings in Azure Key Vault

### Testing Requirements

**Reqnroll BDD Features**:
- `section-creation.feature`
- `student-enrollment.feature`
- `roster-rollover.feature` - Automated year-end rollover
- `capacity-management.feature`

**Test Coverage Target**: ≥ 80%

### Migration Strategy

**Phase 3, Weeks 17-24**:
1. Foundation setup (Weeks 17-18)
2. Core features (Weeks 19-20)
3. Rollover automation (Weeks 21-22)
4. Cutover (Weeks 23-24)

**Data Migration**:
- Historical sections and rosters via ETL
- Rollover rules configuration
- Active sections migrated first

### Dependencies

**Upstream**: Identity, Student Management, Staff Management, Configuration  
**Downstream**: Assessment, Intervention, Reporting

### Implementation Checklist

**Phase 3, Weeks 17-24**:
- [ ] Clean Architecture project structure
- [ ] .NET Aspire orchestration
- [ ] Domain Layer (Section, Roster, Rollover aggregates)
- [ ] Application Layer (CQRS, rollover orchestrator)
- [ ] Infrastructure Layer (EF Core, Worker Service)
- [ ] API Layer
- [ ] Reqnroll BDD features
- [ ] TDD Red → Green cycles
- [ ] Playwright UI tests
- [ ] Data migration scripts
- [ ] Deploy and cutover

### Monitoring & Observability

**Metrics**:
- Sections created per day
- Roster changes per day
- Rollover job duration and success rate
- Section capacity utilization

**Logging**: Structured logging to Seq with audit trail

### Open Questions / Risks

1. **Rollover Complexity**: Automated rollover has complex business rules. Extensive testing required.
2. **Peak Load**: Year-end rollover processes thousands of students. Performance testing critical.
3. **Rollback**: Failed rollover jobs need clear rollback procedures.

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete - Ready for Implementation
