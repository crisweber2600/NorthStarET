# Plan: Student Management Service Migration
Version: 0.1.0
Status: Draft (Planning)
Layer: Foundation
Spec Ref: 005-student-management-service/spec.md

## Objectives
- Deliver isolated student domain microservice with event-driven integration.
- Ensure tenant isolation + FERPA-compliant access.
- Optimize search & dashboard queries for latency targets.

## Architecture Components
1. API Layer (ASP.NET Core) – Controllers: Students, Enrollments, Import, Export, Dashboard.
2. Application Layer – Commands (CreateStudent, UpdateDemographics, EnrollStudent, WithdrawStudent, MergeStudent, ImportStudents), Queries (GetStudent, SearchStudents, GetDashboard, ExportStudents).
3. Domain Layer – Aggregates: Student, Enrollment; Events: StudentCreated, StudentEnrolled, StudentWithdrawn, StudentDemographicsChanged, StudentMerged.
4. Infrastructure – EF Core DbContext + repositories; MassTransit publisher; Blob storage client; CSV import parser; Export generator.
5. Cross-Cutting – Authorization service (educational interest validation); Audit interceptor; Idempotency service.

## Event Model
- Publish events after successful transaction commit.
- Ensure payload includes tenant_id, entity id, minimal necessary data.

## Data Model Highlights
- Student(Id, TenantId, FirstName, LastName, DOB, GradeLevel, Status, PhotoUrl, CreatedAt, UpdatedAt, DeletedAt, LegacyId optional).
- Enrollment(Id, TenantId, StudentId, SchoolId, GradeLevel, EnrollmentDate, WithdrawalDate nullable, Status).

## Key Interfaces
```csharp
public interface IStudentRepository {
    Task AddAsync(Student student);
    Task<Student?> GetAsync(Guid id, CancellationToken ct);
    IQueryable<Student> Query();
}

public interface IEventPublisher { Task PublishAsync<T>(T @event); }
```

## Idempotency Strategy
- CreateStudent: Hash (FirstName, LastName, DOB, SchoolId, TenantId) stored in Redis for 10 minutes.
- Import: Row-level check against existing (Email or external student id) + batch idempotency token.

## Dashboard Aggregation
- Parallel calls via Task.WhenAll to Assessment, Intervention, Section services with cancellation token.
- Use circuit breakers; fallback partial data if a downstream fails.

## Merge Workflow
1. Validate candidate pair not already merged.
2. Reassign foreign references (enrollments -> primary).
3. Soft-delete secondary with MergeReferenceId.
4. Publish StudentMergedEvent.

## Export Workflow
- Build query filtered by active students; map to DTO; stream CSV using `TextWriter` + asynchronous flush.
- Store file in blob with 24h SAS token.

## Photo Handling
- Client uploads -> Virus scan (placeholder) -> Resize (ImageSharp) -> Save -> Update Student.PhotoUrl.

## Performance Tactics
- Composite indexes: (TenantId, LastName), (TenantId, CreatedAt), (TenantId, GradeLevel).
- Pagination implemented using `ORDER BY Id` stable ordering.
- Cache frequently accessed photo metadata; avoid large blob retrieval during search.

## Security & Privacy
- Authorization service validates teacher-student relationship (Enrollment + Section membership) for sensitive read.
- Access audit log appended per successful read where actor ≠ record owner.

## Observability
- Add OpenTelemetry instrumentation: spans for create/search/dashboard; events emit semantic attributes (student.id, tenant.id).
- Metrics: `student_create_latency`, `student_search_latency`, `event_publish_failures`.

## Testing Strategy
- Unit: Domain events, merge logic, idempotency hash generation.
- Integration: Repository CRUD, event publishing, dashboard aggregator with mocked downstream.
- BDD: All scenarios mapped to feature files BEFORE implementation.
- Performance: Load test search (concurrent queries), import throughput simulation.

## Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| Downstream latency affects dashboard | Slow responses | Parallel + timeout + partial fallback |
| Merge logic corrupts relationships | Data inconsistency | Transaction scope + pre/post comparison validation |
| Large imports degrade DB | Lock contention | Batch size tuning + COPY ingestion (future) |

## Completion Criteria
- All spec scenarios implemented + green tests.
- p95 metrics within SLO for create/search/dashboard.
- Events consumed successfully by subscribed services in staging test harness.

---
Draft plan.