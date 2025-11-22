# Assessment Service Specification

## Service Overview

The Assessment Service manages all aspects of educational assessments, benchmarks, scoring, and assessment availability within the NorthStar LMS. It provides data-driven insights for intervention planning and student progress tracking.

## Service Classification

- **Type**: Core Domain Service
- **Phase**: Phase 2 (Weeks 9-16)
- **Implementation Path**: `UpgradedNorthStar/src/services/Assessment/`
- **Priority**: High
- **Dependencies**: Student Management (for test takers), Configuration (for district benchmarks)

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/Assessment*`

**Controllers**:
- `AssessmentController.cs` - Assessment CRUD, scoring
- `BenchmarkAssessmentController.cs` - Benchmark definitions
- `AssessmentAvailabilityController.cs` - Scheduling
- `AssessmentFieldController.cs` - Dynamic field management
- `DataValidationController.cs` - Assessment data validation

**Database Tables** (`NorthStar.EF6/DistrictContext`):
- Assessment, AssessmentField, AssessmentFieldCategory, AssessmentFieldGroup
- Benchmark, BenchmarkGrade, BenchmarkDistrict
- AssessmentResult, AssessmentScore
- AssessmentAvailability

**Estimated Entities**: ~40 from `EntityDto`

## Target State (.NET 10 Microservice)

### Clean Architecture Structure

```
Assessment.Api/                     # UI Layer
├── Controllers/
│   ├── AssessmentsController.cs
│   ├── BenchmarksController.cs
│   ├── AssessmentResultsController.cs
│   └── AvailabilityController.cs
├── Middleware/
└── Program.cs

Assessment.Application/             # Application Layer
├── Commands/
│   ├── CreateAssessment/
│   ├── UpdateAssessment/
│   ├── DefineB enchmark/
│   ├── RecordAssessmentResult/
│   └── SetAvailability/
├── Queries/
│   ├── GetAssessment/
│   ├── SearchAssessments/
│   ├── GetBenchmarks/
│   ├── GetAssessmentResults/
│   └── GetStudentAssessmentHistory/
├── DTOs/
├── Validators/ (FluentValidation)
└── Interfaces/

Assessment.Domain/                  # Domain Layer
├── Entities/
│   ├── Assessment.cs
│   ├── AssessmentField.cs
│   ├── AssessmentFieldGroup.cs
│   ├── Benchmark.cs
│   ├── AssessmentResult.cs
│   └── AssessmentAvailability.cs
├── ValueObjects/
│   ├── AssessmentId.cs
│   ├── Score.cs
│   └── GradeLevel.cs
├── Events/
│   ├── AssessmentCreatedEvent.cs
│   ├── AssessmentCompletedEvent.cs
│   ├── BenchmarkDefinedEvent.cs
│   └── AssessmentAvailabilityChangedEvent.cs
├── Aggregates/
│   └── AssessmentAggregate.cs
└── Interfaces/

Assessment.Infrastructure/          # Infrastructure Layer
├── Data/
│   ├── AssessmentDbContext.cs
│   ├── Migrations/
│   └── Configurations/
├── Repositories/
│   ├── AssessmentRepository.cs
│   ├── BenchmarkRepository.cs
│   └── ResultRepository.cs
├── MessageBus/
│   ├── EventPublisher.cs
│   └── EventConsumers/
└── Integration/
```

### Technology Stack

- **.NET 10** with ASP.NET Core
- **EF Core 10** with PostgreSQL
- **MediatR 12.4** for CQRS
- **FluentValidation** for input validation
- **MassTransit 8.x** for event publishing
- **Aspire 13.0** orchestration

### Owned Data

**Database**: `northstar_assessments`

**Tables**:
- `Assessments` - Assessment definitions (Id, Name, Description, Type, CreatedDate)
- `AssessmentFields` - Dynamic fields (Id, AssessmentId, Name, Type, Category, Order)
- `AssessmentFieldGroups` - Field grouping (Id, Name, AssessmentId, DisplayOrder)
- `AssessmentFieldCategories` - Field categorization (Id, Name, AssessmentId)
- `Benchmarks` - Performance benchmarks (Id, AssessmentId, GradeLevel, BenchmarkValue, ComparisonOperator)
- `AssessmentResults` - Student test results (Id, AssessmentId, StudentId, Score, DateTaken, RecordedBy)
- `AssessmentAvailability` - Scheduling (Id, AssessmentId, SchoolId, GradeLevel, AvailableFrom, AvailableTo)
- `AssessmentScores` - Detailed scoring (Id, ResultId, FieldId, Value)

### Service Boundaries

**Owned Responsibilities**:
- Assessment definition and configuration
- Assessment field management (dynamic fields)
- Benchmark definition per district/grade
- Assessment availability scheduling
- Assessment result recording and scoring
- Assessment data validation

**NOT Owned** (Cross-Service Coordination):
- Student enrollment data (Student Management Service)
- District/school configuration (Configuration Service)
- Intervention recommendations (Intervention Service consumes events)

### Domain Events

**Published Events**:
- `AssessmentCreatedEvent` - When new assessment is defined
  ```json
  {
    "assessmentId": "guid",
    "name": "string",
    "type": "string",
    "createdBy": "guid",
    "timestamp": "datetime"
  }
  ```

- `AssessmentCompletedEvent` - When student completes assessment
  ```json
  {
    "assessmentId": "guid",
    "studentId": "guid",
    "score": "decimal",
    "dateTaken": "datetime",
    "recordedBy": "guid",
    "correlationId": "guid"
  }
  ```

- `BenchmarkDefinedEvent` - When benchmark criteria is set
  ```json
  {
    "benchmarkId": "guid",
    "assessmentId": "guid",
    "gradeLevel": "int",
    "benchmarkValue": "decimal",
    "comparisonOperator": "string"
  }
  ```

- `AssessmentAvailabilityChangedEvent` - When availability window updated
  ```json
  {
    "availabilityId": "guid",
    "assessmentId": "guid",
    "schoolId": "guid",
    "availableFrom": "datetime",
    "availableTo": "datetime"
  }
  ```

**Subscribed Events**:
- `StudentEnrolledEvent` (from Student Service) - Auto-assign grade-appropriate assessments
- `DistrictSettingsUpdatedEvent` (from Configuration Service) - Update district-level benchmarks
- `SchoolCreatedEvent` (from Configuration Service) - Initialize assessment availability

### API Endpoints (Functional Intent)

**Assessment Management**:
- Create new assessment with configurable fields
- Update assessment definition and field structure
- Search assessments by type, grade, subject
- Get assessment details including fields and benchmarks

**Benchmark Management**:
- Define benchmarks for assessment/grade combinations
- Update benchmark thresholds
- Query benchmarks by district, assessment, grade

**Assessment Results**:
- Record student assessment results with field-level scores
- Bulk import assessment results (CSV/API)
- Query student assessment history
- Get assessment results for reporting

**Availability Management**:
- Set assessment availability windows per school/grade
- Query available assessments for students
- Update availability schedules

### Service Level Objectives (SLOs)

**Performance**:
- Assessment query: P95 <100ms
- Assessment result recording: P95 <200ms
- Bulk result import: Process 1000 records in <30s
- Availability check: P99 <50ms

**Availability**:
- Service uptime: 99.9% (excluding planned maintenance)
- Database availability: 99.95%

**Data Consistency**:
- Event publication: At-least-once delivery
- Idempotency window: 24 hours for result recording
- Eventual consistency: Assessment results visible in reporting within 5 seconds

### Idempotency Strategy

**Result Recording**:
- Idempotency key: `{assessmentId}:{studentId}:{dateTaken}:{recordedBy}`
- Duplicate detection: Check if result exists within same day
- Handling: Return existing result ID if duplicate detected

**Event Publishing**:
- Outbox pattern: Store events in database, publish asynchronously
- Retry policy: Exponential backoff, max 3 retries
- Dead-letter queue: After retry exhaustion

### Data Consistency Model

**Internal (Within Service)**:
- Strong consistency: Assessment definitions, results
- Transactions: Unit of Work pattern for multi-table updates

**External (Cross-Service)**:
- Eventual consistency: Accept student enrollments may lag
- Compensating transactions: If student enrollment invalidated after result recorded, publish `AssessmentResultInvalidatedEvent`

### Security Requirements

**Authentication**:
- JWT token validation (from Identity Service)
- Service-to-service authentication via mutual TLS (Aspire)

**Authorization**:
- Role-based access control (RBAC):
  - `Admin` - Full access
  - `Teacher` - Record results for assigned students
  - `Staff` - Read-only assessment definitions
  - `Student` - View own results only

**Data Protection**:
- Encrypt PII in database (student assessment results)
- Audit logging for result modifications
- GDPR compliance: Support data export and deletion

### Testing Requirements

**Unit Tests** (≥80% coverage):
- Domain logic: Benchmark comparison, score calculation
- Validation: Assessment field types, required fields
- Business rules: Availability window logic

**BDD Tests** (Reqnroll):
```gherkin
Feature: Assessment Result Recording
  Scenario: Teacher records student assessment result
    Given a teacher is authenticated
    And the teacher has an assigned student
    And an assessment is available for the student's grade
    When the teacher records the assessment result
    Then the result is saved
    And an AssessmentCompletedEvent is published
    And the result is visible in the student's history
```

**Integration Tests** (Aspire):
- Database operations (EF Core)
- Event publication (MassTransit)
- Cross-service integration (Student Service)

**Performance Tests**:
- Load testing: 1000 concurrent result submissions
- Bulk import: 10,000 results via CSV

### Monitoring & Observability

**Metrics** (Prometheus):
- `assessments_created_total` - Counter
- `assessment_results_recorded_total` - Counter
- `assessment_query_duration_seconds` - Histogram
- `benchmark_comparisons_total` - Counter (tagged by result)

**Health Checks**:
- `/health` - Liveness probe (service running)
- `/health/ready` - Readiness probe (database connected, message bus ready)
- Custom: Assessment result queue depth

**Distributed Tracing** (OpenTelemetry):
- Trace assessment result recording flow
- Trace event publication to downstream services

**Logging**:
- Structured logging (Serilog)
- Log levels: Information (normal), Warning (validation errors), Error (system errors)
- Correlation IDs for request tracking

### Migration Strategy

**Phase 1: Data Analysis** (Week 9)
- Map legacy tables to new schema
- Identify data quality issues in legacy assessments
- Design migration scripts for historical results

**Phase 2: Service Implementation** (Week 10-12)
- Implement Clean Architecture layers
- TDD: Write tests first (Red → Green)
- Configure Aspire integration

**Phase 3: Data Migration** (Week 13)
- Migrate assessment definitions
- Migrate historical results (ETL)
- Dual-write: Write to both legacy and new DB

**Phase 4: Testing** (Week 14)
- Run BDD scenarios
- Performance testing
- Integration testing with Student/Intervention services

**Phase 5: Deployment** (Week 15-16)
- Deploy to staging
- Feature flag rollout: 10% → 50% → 100%
- Monitor for errors, performance

### Migration Checklist

- [ ] Schema design reviewed
- [ ] EF Core migrations created
- [ ] Domain entities implemented with business logic
- [ ] MediatR command/query handlers
- [ ] FluentValidation rules
- [ ] API controllers with Swagger documentation
- [ ] Event publishing configured (MassTransit)
- [ ] Event consumers for subscribed events
- [ ] Unit tests (≥80% coverage)
- [ ] BDD scenarios (Reqnroll)
- [ ] Aspire integration tests
- [ ] Performance tests passed
- [ ] Security review completed
- [ ] Data migration scripts tested
- [ ] Monitoring dashboards created
- [ ] API documentation complete
- [ ] Deployment to staging successful
- [ ] Feature flag configuration
- [ ] Production deployment

---

**Version**: 1.0  
**Last Updated**: November 15, 2025  
**Owner**: Core Domain Team  
**Status**: Specification Complete - Ready for Implementation
