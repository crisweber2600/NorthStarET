# Assessment Service

## Overview

The Assessment Service manages educational assessments, benchmarks, and scoring for the NorthStar LMS platform. It provides the core functionality for creating, administering, and scoring assessments while tracking student performance over time.

## Service Classification

- **Type**: Core Domain Service
- **Phase**: Phase 2 (Weeks 9-16)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Assessment/`
- **Priority**: High (core educational functionality)
- **LMS Role**: Assessment management for measuring student progress and performance

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` (multiple assessment-related controllers)  
**Framework**: .NET Framework 4.6  
**Database**: Shared `NorthStar` database (tables: Assessments, AssessmentItems, StudentAssessments, Benchmarks, BenchmarkPeriods)

**Key Components**:
- `AssessmentController.cs` - Assessment CRUD operations
- `AssessmentItemController.cs` - Question/item management
- `BenchmarkController.cs` - Benchmark period management
- `StudentAssessmentController.cs` - Student assessment instances and scoring
- Shared EF6 context accessing monolithic database

**Key Features**:
- Assessment creation and configuration
- Benchmark period management
- Student assessment assignment
- Manual and automated scoring
- Performance tracking across benchmark periods

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Assessment.API/                  # UI Layer (REST endpoints)
├── Controllers/
│   ├── AssessmentsController.cs
│   ├── BenchmarksController.cs
│   └── ScoringController.cs
├── Middleware/
└── Program.cs

Assessment.Application/          # Application Layer
├── Commands/
│   ├── CreateAssessment/
│   ├── AssignAssessment/
│   ├── ScoreAssessment/
│   └── UpdateBenchmarkPeriod/
├── Queries/
│   ├── GetAssessmentById/
│   ├── GetStudentAssessments/
│   └── GetBenchmarkResults/
├── DTOs/
└── Interfaces/

Assessment.Domain/              # Domain Layer
├── Entities/
│   ├── Assessment.cs
│   ├── AssessmentItem.cs
│   ├── StudentAssessment.cs
│   ├── Benchmark.cs
│   ├── BenchmarkPeriod.cs
│   └── Score.cs
├── ValueObjects/
│   ├── AssessmentType.cs
│   └── ScoringCriteria.cs
├── Events/
│   ├── AssessmentCreatedEvent.cs
│   ├── AssessmentAssignedEvent.cs
│   ├── AssessmentScoredEvent.cs
│   └── BenchmarkPeriodClosedEvent.cs
└── Aggregates/
    └── AssessmentAggregate.cs

Assessment.Infrastructure/      # Infrastructure Layer
├── Data/
│   ├── AssessmentDbContext.cs
│   └── Repositories/
│       ├── AssessmentRepository.cs
│       └── StudentAssessmentRepository.cs
├── MessageBus/
└── ExternalServices/
    └── StateTestDataIntegration.cs
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Data Access**: EF Core with SQL Server
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Caching**: Redis for frequently accessed assessment definitions
- **Orchestration**: .NET Aspire hosting
- **Scoring Engine**: Custom business rules engine for automated scoring

### Owned Data

**Database**: `NorthStar_Assessment_DB`

**Tables**:
- Assessments (Id, Name, Type, Subject, GradeLevel, BenchmarkId, CreatedBy, Status)
- AssessmentItems (Id, AssessmentId, QuestionText, QuestionType, Points, Order)
- ItemOptions (Id, ItemId, OptionText, IsCorrect)
- StudentAssessments (Id, AssessmentId, StudentId, AssignedDate, CompletedDate, Status, TotalScore)
- StudentResponses (Id, StudentAssessmentId, ItemId, ResponseValue, Points, IsCorrect)
- Benchmarks (Id, Name, Subject, GradeLevel, DistrictId, IsActive)
- BenchmarkPeriods (Id, BenchmarkId, Name, StartDate, EndDate, IsActive)
- AssessmentTemplates (Id, Name, Configuration, CreatedBy)

### Service Boundaries

**Owned Responsibilities**:
- Assessment definition and configuration (name, type, items, scoring criteria)
- Assessment item (question) management
- Benchmark and benchmark period management
- Student assessment assignment and tracking
- Assessment scoring (manual and automated)
- Performance aggregation within benchmark periods
- Assessment template management

**Not Owned** (delegated to other services):
- Student enrollment data → Student Management Service
- Staff/teacher information → Staff Management Service
- District/school configuration → Configuration Service
- Section/class rosters → Section & Roster Service
- Long-term analytics and reporting → Reporting & Analytics Service

**Cross-Service Coordination**:
- Subscribe to `StudentEnrolledEvent` to track eligible students
- Subscribe to `SectionCreatedEvent` for class-based assignment
- Publish scoring results for intervention triggers

### Domain Events Published

**Event Schema Version**: 1.0

- `AssessmentCreatedEvent` - When a new assessment is defined
  ```
  - AssessmentId: Guid
  - Name: string
  - Type: string (Benchmark, Formative, Summative)
  - Subject: string
  - GradeLevel: string
  - CreatedBy: Guid
  - Timestamp: DateTime
  ```

- `AssessmentAssignedEvent` - When an assessment is assigned to students
  ```
  - AssignmentId: Guid
  - AssessmentId: Guid
  - StudentIds: Guid[]
  - SectionId: Guid (optional)
  - AssignedBy: Guid
  - DueDate: DateTime
  - Timestamp: DateTime
  ```

- `AssessmentScoredEvent` - When a student assessment is scored
  ```
  - StudentAssessmentId: Guid
  - AssessmentId: Guid
  - StudentId: Guid
  - TotalScore: decimal
  - PercentageScore: decimal
  - ScoredBy: Guid (null for automated)
  - ScoredDate: DateTime
  - Timestamp: DateTime
  ```

- `BenchmarkPeriodClosedEvent` - When a benchmark period ends
  ```
  - BenchmarkPeriodId: Guid
  - BenchmarkId: Guid
  - ClosedDate: DateTime
  - TotalAssessments: int
  - CompletedAssessments: int
  - Timestamp: DateTime
  ```

### Domain Events Subscribed

- `StudentEnrolledEvent` (from Student Management) - Track students eligible for assessments
- `StudentWithdrawnEvent` (from Student Management) - Update assessment eligibility
- `SectionCreatedEvent` (from Section & Roster) - Enable class-based assignment
- `StateTestDataImportedEvent` (from Data Import) - Integrate external assessment results

### API Functional Intent

**Assessment Management**:
- Create and configure assessments (items, scoring, metadata)
- Update assessment definitions
- Deactivate/archive assessments
- Clone assessments from templates

**Student Assessment Operations**:
- Assign assessments to individual students or sections
- Record student responses
- Score assessments (manual or automated)
- Retrieve student assessment history

**Benchmark Management**:
- Create and manage benchmarks
- Define benchmark periods
- Close benchmark periods and finalize results
- Query benchmark performance across cohorts

**Reporting Queries** (read-only):
- Get assessment results by student
- Get assessment results by section/class
- Get benchmark period summaries
- Get item-level analysis (difficulty, discrimination)

### Service Level Objectives (SLOs)

- **Availability**: 99.5% uptime during instructional hours (6 AM - 6 PM local time)
- **Assessment Retrieval**: < 200ms p95 for assessment definition queries
- **Scoring Latency**: 
  - Automated scoring: < 500ms p95
  - Manual score entry: < 300ms p95
- **Assignment Creation**: < 1 second p95 for bulk student assignment
- **Consistency Model**: Eventually consistent for cross-service data; strongly consistent for scoring
- **Idempotency Window**: 10 minutes for duplicate score submissions

### Security & Compliance

**Authorization**:
- **District Admin**: Full assessment management within district
- **School Admin**: Assessment management within school
- **Teachers**: Create/assign/score assessments for their sections
- **Students**: View assigned assessments and own results (read-only)

**Data Protection**:
- Assessment items encrypted at rest (prevent cheating)
- Student responses encrypted at rest
- FERPA compliance for student assessment results
- Audit logging for all score modifications

**Secrets Management**:
- Database connection strings in Azure Key Vault
- External API keys (state test integration) in Key Vault
- No secrets in configuration files or code

### Testing Requirements

**Unit Tests** (Domain & Application layers):
- Assessment creation and validation logic
- Scoring algorithm accuracy
- Benchmark period calculations
- Business rule enforcement (e.g., score ranges, required fields)

**Reqnroll BDD Features**:
- `assessment-creation.feature` - Creating different assessment types
- `student-assessment-assignment.feature` - Assigning to students/sections
- `automated-scoring.feature` - Scoring logic for different question types
- `benchmark-period-management.feature` - Opening/closing periods

**Integration Tests** (Aspire):
- Event publishing to Azure Service Bus
- Database persistence via EF Core
- Cross-service event subscription (Student, Section services)
- Redis caching behavior

**Playwright UI Tests**:
- Assessment builder workflow (Figma: assessment-builder-flow)
- Student assessment assignment flow (Figma: assign-assessment-flow)
- Scoring interface (Figma: scoring-interface)

**Test Coverage Target**: ≥ 80% for Domain and Application layers

### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 2, Week 9-10**: Foundation
   - Set up Assessment.API with Clean Architecture structure
   - Configure .NET Aspire orchestration
   - Implement EF Core DbContext with migration scripts
   - Dual-write to legacy DB and new Assessment_DB

2. **Phase 2, Week 11-12**: Core Features
   - Migrate assessment CRUD operations
   - Implement automated scoring engine
   - Set up event publishing for assessment lifecycle events
   - API Gateway routes new endpoints

3. **Phase 2, Week 13-14**: Advanced Features
   - Migrate benchmark management
   - Implement bulk assignment workflows
   - Add state test data integration
   - Performance optimization (caching, indexing)

4. **Phase 2, Week 15-16**: Cutover
   - Validate data consistency between legacy and new DB
   - Switch API Gateway to route all traffic to new service
   - Deprecate legacy assessment controllers
   - Monitor performance and error rates

**Data Migration**:
- Historical assessments migrated via ETL scripts
- Student assessment results migrated in batches
- Benchmark periods and historical data preserved
- Rollback plan: revert API Gateway routing

**Legacy-New Interoperability**:
- During transition, both legacy and new services can query assessment data
- Events published to support dependent services (Intervention, Reporting)
- Gradual feature migration (core features first, advanced features later)

### Dependencies

**Upstream Services** (this service depends on):
- Identity Service (authentication tokens)
- Configuration Service (district/school settings)
- Student Management Service (student enrollment status)
- Section & Roster Service (class sections for bulk assignment)

**Downstream Services** (services that depend on this):
- Intervention Management Service (assessment scores trigger interventions)
- Reporting & Analytics Service (consumes assessment results for dashboards)
- Student Management Service (may display recent assessment summary)

### Implementation Checklist

**Phase 2, Weeks 9-16**:

- [ ] Set up project structure with Clean Architecture
  - [ ] Assessment.API
  - [ ] Assessment.Application
  - [ ] Assessment.Domain
  - [ ] Assessment.Infrastructure
  - [ ] Assessment.Tests (unit, integration, BDD)

- [ ] Configure .NET Aspire
  - [ ] Add service to AppHost
  - [ ] Configure Aspire Service Defaults
  - [ ] Set up distributed tracing

- [ ] Implement Domain Layer
  - [ ] Assessment aggregate with business rules
  - [ ] Value objects (AssessmentType, ScoringCriteria)
  - [ ] Domain events (Created, Assigned, Scored)
  - [ ] Unit tests for domain logic

- [ ] Implement Application Layer
  - [ ] CQRS commands and queries
  - [ ] DTOs and mapping
  - [ ] Event handlers
  - [ ] Application service tests

- [ ] Implement Infrastructure Layer
  - [ ] EF Core DbContext and configurations
  - [ ] Repository implementations
  - [ ] MassTransit event publishing
  - [ ] Database migrations
  - [ ] Redis caching

- [ ] Implement API Layer
  - [ ] REST controllers
  - [ ] Authentication/authorization middleware
  - [ ] API documentation (Swagger)
  - [ ] Health checks

- [ ] Testing
  - [ ] Write Reqnroll BDD features
  - [ ] Execute TDD Red → Green → Refactor cycles
  - [ ] Write Playwright UI tests (with Figma designs)
  - [ ] Aspire integration tests
  - [ ] Achieve ≥ 80% code coverage

- [ ] Data Migration
  - [ ] Design ETL scripts for historical data
  - [ ] Test dual-write mechanism
  - [ ] Validate data consistency

- [ ] Deployment
  - [ ] Configure Docker container
  - [ ] Set up Kubernetes manifests
  - [ ] Configure API Gateway routing
  - [ ] Deploy to staging environment
  - [ ] Execute smoke tests

- [ ] Production Cutover
  - [ ] Monitor performance and errors
  - [ ] Gradually increase traffic to new service
  - [ ] Deprecate legacy controllers
  - [ ] Post-migration validation

### Monitoring & Observability

**Application Insights**:
- Track assessment creation rate
- Monitor scoring latency
- Alert on failed score calculations

**Custom Metrics**:
- Assessments created per day
- Assessments assigned per day
- Average scoring time
- Benchmark period completion rate

**Distributed Tracing**:
- OpenTelemetry for request tracing across services
- Trace assessment assignment to scoring workflow

**Logging**:
- Structured logging to Seq
- Log all score modifications with audit trail
- Error logging for scoring failures

### Open Questions / Risks

1. **Scoring Engine Complexity**: Legacy system has complex scoring rules for different question types (multiple choice, short answer, essay). Need to ensure parity.
2. **Performance at Scale**: Benchmark periods may have thousands of concurrent assessments. Load testing required.
3. **State Test Integration**: External APIs for state test data may have rate limits or downtime. Need fallback strategy.
4. **Data Migration Volume**: Historical assessment data spans multiple years. ETL process may take extended time.

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete - Ready for Implementation
