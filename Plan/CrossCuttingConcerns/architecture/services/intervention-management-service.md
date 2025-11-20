# Intervention Management Service

## Overview

The Intervention Management Service manages student intervention plans, intervention groups, and progress monitoring for the NorthStar LMS platform. It helps educators identify at-risk students, assign interventions, track progress, and measure intervention effectiveness.

## Service Classification

- **Type**: Secondary Domain Service
- **Phase**: Phase 3 (Weeks 17-24)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Intervention/`
- **Priority**: Medium (supports Response to Intervention - RTI frameworks)
- **LMS Role**: Student intervention planning and progress monitoring

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` (Intervention-related controllers)  
**Framework**: .NET Framework 4.6  
**Database**: Shared `NorthStar` database (tables: Interventions, InterventionGroups, InterventionAssignments, ProgressMonitoring)

**Key Components**:
- `InterventionController.cs` - Intervention CRUD operations
- `InterventionGroupController.cs` - Group management
- `InterventionAssignmentController.cs` - Student-intervention assignments
- Shared EF6 context accessing monolithic database

**Key Features**:
- Intervention plan creation and management
- Intervention group formation
- Student-intervention assignments
- Progress monitoring and data tracking
- Intervention effectiveness reporting

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Intervention.API/               # UI Layer (REST endpoints)
├── Controllers/
│   ├── InterventionsController.cs
│   ├── GroupsController.cs
│   └── ProgressController.cs
├── Middleware/
└── Program.cs

Intervention.Application/       # Application Layer
├── Commands/
│   ├── CreateIntervention/
│   ├── AssignStudentToIntervention/
│   ├── CreateInterventionGroup/
│   └── RecordProgress/
├── Queries/
│   ├── GetInterventionsByStudent/
│   ├── GetInterventionGroups/
│   └── GetProgressReports/
├── DTOs/
└── Interfaces/

Intervention.Domain/            # Domain Layer
├── Entities/
│   ├── Intervention.cs
│   ├── InterventionGroup.cs
│   ├── InterventionAssignment.cs
│   ├── ProgressMonitoring.cs
│   └── InterventionTier.cs
├── ValueObjects/
│   ├── InterventionType.cs
│   └── ProgressMetric.cs
├── Events/
│   ├── InterventionCreatedEvent.cs
│   ├── StudentAssignedToInterventionEvent.cs
│   ├── ProgressRecordedEvent.cs
│   └── InterventionCompletedEvent.cs
└── Aggregates/
    └── InterventionAggregate.cs

Intervention.Infrastructure/    # Infrastructure Layer
├── Data/
│   ├── InterventionDbContext.cs
│   └── Repositories/
│       ├── InterventionRepository.cs
│       └── ProgressRepository.cs
├── MessageBus/
└── ExternalServices/
    └── AssessmentDataIntegration.cs
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Data Access**: EF Core with SQL Server
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Caching**: Redis for active intervention lookups
- **Orchestration**: .NET Aspire hosting

### Owned Data

**Database**: `NorthStar_Intervention_DB`

**Tables**:
- Interventions (Id, Name, Description, Type, TierLevel, DistrictId, CreatedBy, IsActive)
- InterventionGroups (Id, Name, InterventionId, StaffId, ScheduledDays, StartDate, EndDate)
- GroupMembers (Id, GroupId, StudentId, JoinedDate, ExitDate, ExitReason)
- InterventionAssignments (Id, StudentId, InterventionId, GroupId, StartDate, EndDate, Status, AssignedBy)
- ProgressMonitoring (Id, AssignmentId, MonitoringDate, MetricType, MetricValue, Notes, RecordedBy)
- InterventionTiers (Id, DistrictId, TierLevel, Description, MaxStudentsPerGroup)
- InterventionGoals (Id, AssignmentId, GoalDescription, TargetDate, MeasurementCriteria, Status)

### Service Boundaries

**Owned Responsibilities**:
- Intervention plan definition and management
- Intervention group creation and scheduling
- Student-intervention assignments
- Progress monitoring data collection
- Intervention goal setting and tracking
- Tier management (RTI Tier 1, 2, 3)
- Intervention effectiveness analysis

**Not Owned** (delegated to other services):
- Student enrollment data → Student Management Service
- Assessment scores (triggers) → Assessment Service
- Staff assignments → Staff Management Service
- Reporting and analytics → Reporting & Analytics Service

**Cross-Service Coordination**:
- Subscribe to `AssessmentScoredEvent` to trigger intervention assignments
- Subscribe to `StudentEnrolledEvent` for new students
- Publish progress data for reporting

### Domain Events Published

**Event Schema Version**: 1.0

- `InterventionCreatedEvent` - When a new intervention is defined
  ```
  - InterventionId: Guid
  - Name: string
  - Type: string
  - TierLevel: int
  - CreatedBy: Guid
  - Timestamp: DateTime
  ```

- `StudentAssignedToInterventionEvent` - When a student is assigned
  ```
  - AssignmentId: Guid
  - StudentId: Guid
  - InterventionId: Guid
  - GroupId: Guid (optional)
  - StartDate: DateTime
  - AssignedBy: Guid
  - Timestamp: DateTime
  ```

- `ProgressRecordedEvent` - When progress is monitored
  ```
  - ProgressId: Guid
  - AssignmentId: Guid
  - StudentId: Guid
  - MetricType: string
  - MetricValue: decimal
  - MonitoringDate: DateTime
  - Timestamp: DateTime
  ```

- `InterventionCompletedEvent` - When an intervention is completed
  ```
  - AssignmentId: Guid
  - StudentId: Guid
  - InterventionId: Guid
  - CompletionDate: DateTime
  - Outcome: string
  - Timestamp: DateTime
  ```

### Domain Events Subscribed

- `AssessmentScoredEvent` (from Assessment Service) - Identify at-risk students for intervention
- `StudentEnrolledEvent` (from Student Management) - Track eligible students
- `StudentWithdrawnEvent` (from Student Management) - Update intervention status

### API Functional Intent

**Intervention Management**:
- Create and configure interventions
- Update intervention definitions
- Deactivate interventions

**Group Management**:
- Create intervention groups
- Assign students to groups
- Schedule group sessions
- Manage group membership

**Progress Monitoring**:
- Record progress monitoring data
- Track intervention goals
- Document observations and notes

**Queries** (read-only):
- Get interventions by student
- Get active intervention groups
- Get progress reports
- Get intervention effectiveness metrics

### Service Level Objectives (SLOs)

- **Availability**: 99% uptime during school hours
- **Intervention Retrieval**: < 200ms p95 for intervention queries
- **Progress Recording**: < 300ms p95 for progress data entry
- **Group Assignment**: < 500ms p95 for bulk student assignments
- **Consistency Model**: Eventually consistent for cross-service data
- **Idempotency Window**: 10 minutes for duplicate progress entries

### Security & Compliance

**Authorization**:
- **District Admin**: Full intervention management within district
- **School Admin**: Intervention management within school
- **Teachers/Interventionists**: Manage assigned groups and record progress
- **Parents**: View own child's interventions (future)

**Data Protection**:
- Student intervention data encrypted at rest
- FERPA compliance for intervention records
- Audit logging for intervention assignments and progress

**Secrets Management**:
- Database connection strings in Azure Key Vault
- No secrets in configuration files or code

### Testing Requirements

**Unit Tests** (Domain & Application layers):
- Intervention assignment logic
- Progress calculation rules
- Tier level validation
- Group capacity constraints

**Reqnroll BDD Features**:
- `intervention-assignment.feature` - Assigning students to interventions
- `progress-monitoring.feature` - Recording and tracking progress
- `intervention-group-management.feature` - Managing groups
- `tier-management.feature` - RTI tier configuration

**Integration Tests** (Aspire):
- Event publishing to Azure Service Bus
- Event subscription from Assessment Service
- Database persistence via EF Core
- Redis caching behavior

**Playwright UI Tests**:
- Intervention assignment workflow (Figma: intervention-assignment-flow)
- Progress monitoring interface (Figma: progress-monitoring-ui)
- Group management (Figma: group-management-flow)

**Test Coverage Target**: ≥ 80% for Domain and Application layers

### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 3, Week 17-18**: Foundation
   - Set up Intervention.API with Clean Architecture
   - Configure .NET Aspire orchestration
   - Implement EF Core DbContext
   - Dual-write to legacy and new DB

2. **Phase 3, Week 19-20**: Core Features
   - Migrate intervention CRUD operations
   - Implement group management
   - Set up event publishing
   - API Gateway routes new endpoints

3. **Phase 3, Week 21-22**: Advanced Features
   - Implement progress monitoring
   - Add assessment score triggers
   - Performance optimization

4. **Phase 3, Week 23-24**: Cutover
   - Validate data consistency
   - Switch API Gateway routing
   - Deprecate legacy controllers
   - Monitor performance

**Data Migration**:
- Historical interventions migrated via ETL
- Active assignments and progress preserved
- Rollback plan: revert API Gateway routing

### Dependencies

**Upstream Services** (this service depends on):
- Identity Service (authentication)
- Student Management Service (student data)
- Assessment Service (assessment scores for triggers)
- Staff Management Service (interventionist information)

**Downstream Services** (services that depend on this):
- Reporting & Analytics Service (intervention effectiveness reports)

### Implementation Checklist

**Phase 3, Weeks 17-24**:

- [ ] Set up project structure with Clean Architecture
- [ ] Configure .NET Aspire orchestration
- [ ] Implement Domain Layer with RTI business rules
- [ ] Implement Application Layer (CQRS, event handlers)
- [ ] Implement Infrastructure Layer (EF Core, MassTransit)
- [ ] Implement API Layer (REST controllers, auth)
- [ ] Write Reqnroll BDD features
- [ ] Execute TDD Red → Green cycles
- [ ] Write Playwright UI tests
- [ ] Aspire integration tests
- [ ] Achieve ≥ 80% code coverage
- [ ] Data migration scripts
- [ ] Deploy to staging
- [ ] Production cutover

### Monitoring & Observability

**Application Insights**:
- Track intervention assignments
- Monitor progress recording rate
- Alert on failed event processing

**Custom Metrics**:
- Active interventions per school
- Students in interventions by tier
- Progress monitoring frequency
- Intervention completion rate

**Distributed Tracing**:
- OpenTelemetry for cross-service traces
- Trace assessment-to-intervention workflow

**Logging**:
- Structured logging to Seq
- Audit trail for interventions
- Error logging

### Open Questions / Risks

1. **RTI Framework Complexity**: Different districts may use different RTI tier structures. Need flexible configuration.
2. **Assessment Integration**: Real-time triggers from assessment scores require reliable event processing.
3. **Progress Monitoring Flexibility**: Different intervention types may require different progress metrics.

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete - Ready for Implementation
