---
description: "Task list for Section & Roster Service Migration"
---

# Tasks: Section & Roster Service

**Specification Branch**: `Foundation/011-section-roster-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/011-section-roster-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/011-section-roster-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/SectionRosterService/`  
**Specification Path**: `Plan/Foundation/specs/011-section-roster-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification (Foundation)
- [ ] Target Layer matches plan.md Layer Identification (Foundation)
- [ ] Implementation path follows layer structure (`Src/Foundation/services/SectionRosterService/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/011-section-roster-service/`)
- [ ] Shared infrastructure dependencies match between spec and plan
- [ ] Cross-layer dependencies use events/contracts only (Staff service client, Student events)

---

## Layer Compliance Validation

*MANDATORY: Include these validation tasks to ensure mono-repo layer isolation (Constitution Principle 6)*

- [ ] Verify project references ONLY shared infrastructure from `Src/Foundation/shared/{Domain,Application,Infrastructure,ServiceDefaults}`
- [ ] Verify NO direct service-to-service references (Staff/Student integration via HTTP clients/events only)
- [ ] Verify AppHost orchestration includes SectionRosterService with correct dependencies (PostgreSQL, Redis, RabbitMQ)
- [ ] Verify README.md documents Foundation layer position and shared infrastructure dependencies
- [ ] Verify no circular dependencies between layers

---

## Identity & Authentication Compliance

*MANDATORY: This service requires authentication/authorization*

- [ ] Verify NO references to Duende IdentityServer or custom token issuance
- [ ] Verify Microsoft.Identity.Web used for JWT token validation (NOT custom JWT generation)
- [ ] Verify SessionAuthenticationHandler registered for session-based API authorization
- [ ] Verify Redis configured for session caching (Aspire.Hosting.Redis)
- [ ] Verify identity.sessions table includes tenant_id for multi-tenancy enforcement
- [ ] Verify TokenExchangeService implements BFF pattern (Entra tokens → LMS sessions)
- [ ] Verify authentication flow follows `Plan/Foundation/docs/legacy-identityserver-migration.md` architecture

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for Section & Roster Service

- [ ] T001 Create SectionRosterService project structure at `Src/Foundation/services/SectionRosterService/` with Clean Architecture layers (API, Application, Domain, Infrastructure)
- [ ] T002 Initialize .NET 9 Web API project with dependencies: Aspire.Hosting, MediatR 11, FluentValidation, EF Core 9 (PostgreSQL), MassTransit (RabbitMQ), Microsoft.Identity.Web, ServiceDefaults
- [ ] T003 [P] Configure centralized package versions in `Src/Foundation/services/SectionRosterService/Directory.Packages.props`
- [ ] T004 [P] Setup project references: API → Application → {Domain, Infrastructure}; Infrastructure → Domain
- [ ] T005 [P] Create README.md documenting service purpose, layer position, shared infrastructure dependencies, and local development setup
- [ ] T006 Configure launchSettings.json with HTTPS ports and Aspire integration settings

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story implementation

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T007 Create Domain entities in `Src/Foundation/services/SectionRosterService/Domain/Entities/`: Section, TeacherAssignment, Roster, Period, RolloverRecord (all with TenantId, soft delete support)
- [ ] T008 [P] Create value objects in `Src/Foundation/services/SectionRosterService/Domain/ValueObjects/`: SectionNumber, Capacity, WaitlistPosition
- [ ] T009 [P] Define domain events in `Src/Foundation/services/SectionRosterService/Domain/Events/`: SectionCreatedEvent, StudentAddedToRosterEvent, StudentDroppedFromRosterEvent, RolloverCompletedEvent, CapacityReachedEvent
- [ ] T010 Create SectionRosterDbContext in `Src/Foundation/services/SectionRosterService/Infrastructure/Persistence/SectionRosterDbContext.cs` with entity configurations, tenant filters, audit interceptor
- [ ] T011 Setup EF Core migrations framework and initial migration for all entities at `Src/Foundation/services/SectionRosterService/Infrastructure/Persistence/Migrations/`
- [ ] T012 [P] Create composite indexes: (TenantId, SchoolId, AcademicYear), (TenantId, SectionId) on Roster, (TenantId, StudentId, AcademicYear, IsActive) for active schedule lookup
- [ ] T013 [P] Implement TenantInterceptor for automatic tenant_id injection in `Src/Foundation/services/SectionRosterService/Infrastructure/Persistence/Interceptors/TenantInterceptor.cs`
- [ ] T014 [P] Implement AuditInterceptor for CreatedAt/ModifiedAt timestamps in `Src/Foundation/services/SectionRosterService/Infrastructure/Persistence/Interceptors/AuditInterceptor.cs`
- [ ] T015 Configure Redis caching in `Src/Foundation/services/SectionRosterService/Infrastructure/Caching/RedisCacheService.cs` for section capacity/enrollment counts
- [ ] T016 [P] Setup MassTransit with RabbitMQ in `Src/Foundation/services/SectionRosterService/Infrastructure/Messaging/MassTransitConfiguration.cs` for event publishing
- [ ] T017 [P] Create base Result/Result<T> types in shared Application layer (if not exists) for command/query responses
- [ ] T018 Register Application layer services in `Src/Foundation/services/SectionRosterService/Application/DependencyInjection.cs`: MediatR, FluentValidation, AutoMapper
- [ ] T019 Register Infrastructure layer services in `Src/Foundation/services/SectionRosterService/Infrastructure/DependencyInjection.cs`: DbContext, Repositories, Redis, MassTransit
- [ ] T020 Configure API Program.cs at `Src/Foundation/services/SectionRosterService/API/Program.cs`: builder.AddServiceDefaults(), AddApplication(), AddInfrastructure(), authentication/authorization middleware
- [ ] T021 [P] Create HTTP client for Staff Service in `Src/Foundation/services/SectionRosterService/Infrastructure/ExternalServices/StaffServiceClient.cs` to validate teacher availability
- [ ] T022 [P] Setup global exception handling middleware in `Src/Foundation/services/SectionRosterService/API/Middleware/ExceptionHandlingMiddleware.cs`
- [ ] T023 Add SectionRosterService to AppHost at `Src/Foundation/AppHost/Program.cs` with PostgreSQL, Redis, RabbitMQ references and WaitFor dependencies

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Create Section with Teacher Assignment (Priority: P1) 🎯 MVP

**Goal**: Enable school admins to create sections with course details, capacity, period/room assignments, and assign primary teacher

**Independent Test**: POST /api/v1/sections with valid data → 201 Created → SectionCreatedEvent published → section appears in database with teacher assignment

### Implementation for User Story 1

- [ ] T024 [P] [US1] Create CreateSectionCommand record in `Src/Foundation/services/SectionRosterService/Application/Sections/Commands/CreateSection/CreateSectionCommand.cs` implementing IRequest<Result<Guid>>
- [ ] T025 [P] [US1] Create CreateSectionCommandValidator in `Src/Foundation/services/SectionRosterService/Application/Sections/Commands/CreateSection/CreateSectionCommandValidator.cs` with FluentValidation rules (capacity > 0, valid academic year, required fields)
- [ ] T026 [US1] Implement CreateSectionCommandHandler in `Src/Foundation/services/SectionRosterService/Application/Sections/Commands/CreateSection/CreateSectionCommandHandler.cs`: validate teacher availability via StaffServiceClient, create Section entity, assign primary teacher (TeacherAssignment), save to DB, publish SectionCreatedEvent via MassTransit
- [ ] T027 [US1] Create ISectionRepository interface in `Src/Foundation/services/SectionRosterService/Domain/Repositories/ISectionRepository.cs` with AddAsync, GetByIdAsync, ExistsAsync methods
- [ ] T028 [US1] Implement SectionRepository in `Src/Foundation/services/SectionRosterService/Infrastructure/Persistence/Repositories/SectionRepository.cs` with tenant filtering and EF Core queries
- [ ] T029 [US1] Create SectionsController POST endpoint in `Src/Foundation/services/SectionRosterService/API/Controllers/SectionsController.cs`: [Authorize], validate tenant claim, send CreateSectionCommand via MediatR, return 201 with Location header
- [ ] T030 [US1] Add conflict detection for teacher/room/period in CreateSectionCommandHandler: query existing sections for same tenant/school/period with same teacher or room → return 409 Conflict if collision
- [ ] T031 [US1] Implement SectionCreatedEventPublisher in `Src/Foundation/services/SectionRosterService/Infrastructure/Messaging/Publishers/SectionCreatedEventPublisher.cs` using MassTransit IPublishEndpoint
- [ ] T032 [US1] Add logging for section creation in CreateSectionCommandHandler using ILogger with correlation ID from X-Correlation-Id header

**Checkpoint**: At this point, User Story 1 should be fully functional - sections can be created with teacher assignments and events published

---

## Phase 4: User Story 2 - Add Students to Roster with Capacity Tracking (Priority: P1) 🎯 MVP

**Goal**: Enable adding students to section rosters with automatic capacity validation, waitlist fallback when full, and event notification

**Independent Test**: POST /api/v1/sections/{id}/roster with studentId → 200 OK (enrolled) or 202 Accepted (waitlisted) → StudentAddedToRosterEvent published → roster/waitlist updated, capacity count cached in Redis

### Implementation for User Story 2

- [ ] T033 [P] [US2] Create AddStudentToRosterCommand record in `Src/Foundation/services/SectionRosterService/Application/Rosters/Commands/AddStudent/AddStudentToRosterCommand.cs` implementing IRequest<Result<RosterStatus>>
- [ ] T034 [P] [US2] Create AddStudentToRosterCommandValidator in `Src/Foundation/services/SectionRosterService/Application/Rosters/Commands/AddStudent/AddStudentToRosterCommandValidator.cs`: validate SectionId and StudentId not empty
- [ ] T035 [US2] Implement AddStudentToRosterCommandHandler in `Src/Foundation/services/SectionRosterService/Application/Rosters/Commands/AddStudent/AddStudentToRosterCommandHandler.cs`: load section, check current enrollment count from Redis cache (fallback to DB query if cache miss), if under capacity → add Roster record with IsActive=true/WaitlistPosition=null, else → add with IsActive=false/WaitlistPosition=next, publish StudentAddedToRosterEvent, update Redis capacity cache
- [ ] T036 [US2] Create IRosterRepository interface in `Src/Foundation/services/SectionRosterService/Domain/Repositories/IRosterRepository.cs` with AddAsync, GetActiveBySectionAsync, CountActiveBySectionAsync, GetWaitlistBySectionAsync methods
- [ ] T037 [US2] Implement RosterRepository in `Src/Foundation/services/SectionRosterService/Infrastructure/Persistence/Repositories/RosterRepository.cs` with tenant filtering and optimized queries
- [ ] T038 [US2] Extend SectionsController with POST /api/v1/sections/{id}/roster endpoint in `Src/Foundation/services/SectionRosterService/API/Controllers/SectionsController.cs`: return 200 for enrolled, 202 for waitlisted, include waitlist position in response
- [ ] T039 [US2] Implement capacity caching in RedisCacheService: cache key pattern `section:{tenantId}:{sectionId}:capacity` with enrollment count, TTL 1 hour, invalidate on roster changes
- [ ] T040 [US2] Create StudentAddedToRosterEventPublisher in `Src/Foundation/services/SectionRosterService/Infrastructure/Messaging/Publishers/StudentAddedToRosterEventPublisher.cs` including roster status (active/waitlisted) in event payload
- [ ] T041 [US2] Add duplicate enrollment check in AddStudentToRosterCommandHandler: query existing roster entry for student+section+tenant → return 409 Conflict if already enrolled or waitlisted
- [ ] T042 [US2] Emit CapacityReachedEvent when section reaches full capacity (first waitlist addition) in AddStudentToRosterCommandHandler

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - sections can be created and students can be enrolled with automatic waitlist handling

---

## Phase 5: User Story 3 - Period Configuration & Conflict Validation (Priority: P1) 🎯 MVP

**Goal**: Define school periods with times, validate teacher/room/student schedule conflicts when creating sections or enrolling students

**Independent Test**: POST /api/v1/periods → 201 Created; POST /api/v1/sections with conflicting teacher/period → 409 Conflict; POST /api/v1/sections/{id}/roster with student already scheduled in period → 409 Conflict

### Implementation for User Story 3

- [ ] T043 [P] [US3] Create CreatePeriodCommand record in `Src/Foundation/services/SectionRosterService/Application/Periods/Commands/CreatePeriod/CreatePeriodCommand.cs` implementing IRequest<Result<Guid>>
- [ ] T044 [P] [US3] Create CreatePeriodCommandValidator in `Src/Foundation/services/SectionRosterService/Application/Periods/Commands/CreatePeriod/CreatePeriodCommandValidator.cs`: validate StartTime < EndTime, PeriodNumber unique per school
- [ ] T045 [US3] Implement CreatePeriodCommandHandler in `Src/Foundation/services/SectionRosterService/Application/Periods/Commands/CreatePeriod/CreatePeriodCommandHandler.cs`: create Period entity, save to DB
- [ ] T046 [US3] Create IPeriodRepository interface in `Src/Foundation/services/SectionRosterService/Domain/Repositories/IPeriodRepository.cs` with AddAsync, GetBySchoolAsync, ExistsAsync methods
- [ ] T047 [US3] Implement PeriodRepository in `Src/Foundation/services/SectionRosterService/Infrastructure/Persistence/Repositories/PeriodRepository.cs` with tenant filtering
- [ ] T048 [US3] Create PeriodsController POST endpoint in `Src/Foundation/services/SectionRosterService/API/Controllers/PeriodsController.cs` with [Authorize] and tenant validation
- [ ] T049 [US3] Implement teacher schedule conflict detection in CreateSectionCommandHandler: build in-memory schedule map from Redis cache (key: `teacher:{tenantId}:{teacherId}:schedule:{academicYear}`, value: array of {period, sectionId}), check for period collision before creating section → return 409 if conflict
- [ ] T050 [US3] Implement room conflict detection in CreateSectionCommandHandler: similar Redis cache strategy for room schedules (key: `room:{tenantId}:{schoolId}:{room}:schedule:{academicYear}`), check for period collision → return 409 if conflict
- [ ] T051 [US3] Implement student schedule conflict detection in AddStudentToRosterCommandHandler: load student's active rosters for academic year, check if student already enrolled in section with same period → return 409 Conflict if collision
- [ ] T052 [US3] Create schedule cache warming service in `Src/Foundation/services/SectionRosterService/Infrastructure/Caching/ScheduleCacheService.cs`: background task (IHostedService) to precompute teacher/room schedules on startup and refresh hourly
- [ ] T053 [US3] Add cache invalidation on section creation/deletion/period change in CreateSectionCommandHandler: clear affected teacher/room schedule cache entries

**Checkpoint**: All conflict validation working - no double-bookings for teachers, rooms, or students

---

## Phase 6: User Story 4 - Year-End Rollover with Archive & Template Generation (Priority: P2)

**Goal**: Automate year-end section rollover: archive current year sections (make immutable), create next year section templates based on enrollment patterns, emit promotion statistics

**Independent Test**: POST /api/v1/sections/rollover with fromYear=2024, toYear=2025, dryRun=true → 200 OK with preview statistics (no DB changes); dryRun=false → sections archived, templates created, RolloverCompletedEvent published

### Implementation for User Story 4

- [ ] T054 [P] [US4] Create RolloverSectionsCommand record in `Src/Foundation/services/SectionRosterService/Application/Sections/Commands/RolloverSections/RolloverSectionsCommand.cs` implementing IRequest<Result<RolloverSummary>> with fromYear, toYear, dryRun properties
- [ ] T055 [P] [US4] Create RolloverSectionsCommandValidator in `Src/Foundation/services/SectionRosterService/Application/Sections/Commands/RolloverSections/RolloverSectionsCommandValidator.cs`: validate toYear = fromYear + 1, fromYear not already rolled over (check RolloverRecord)
- [ ] T056 [US4] Implement RolloverSectionsCommandHandler in `Src/Foundation/services/SectionRosterService/Application/Sections/Commands/RolloverSections/RolloverSectionsCommandHandler.cs`: load all sections for fromYear/tenant, for each section → set IsImmutable=true (archive), calculate promotion rule (increment grade level or retain for kindergarten), create new Section template for toYear with estimated capacity (avg enrollment from previous year), if dryRun=false → save changes and create RolloverRecord, publish RolloverCompletedEvent with statistics (total sections, total students, promotion vs retention counts)
- [ ] T057 [US4] Add IsImmutable boolean flag to Section entity in `Src/Foundation/services/SectionRosterService/Domain/Entities/Section.cs` to prevent modifications post-term
- [ ] T058 [US4] Create IRolloverRecordRepository interface in `Src/Foundation/services/SectionRosterService/Domain/Repositories/IRolloverRecordRepository.cs` with AddAsync, ExistsAsync methods
- [ ] T059 [US4] Implement RolloverRecordRepository in `Src/Foundation/services/SectionRosterService/Infrastructure/Persistence/Repositories/RolloverRecordRepository.cs`
- [ ] T060 [US4] Extend SectionsController with POST /api/v1/sections/rollover endpoint in `Src/Foundation/services/SectionRosterService/API/Controllers/SectionsController.cs`: support dryRun query parameter, return RolloverSummary DTO (totalSections, totalStudents, promotedCount, retainedCount)
- [ ] T061 [US4] Implement immutability guard in all update/delete handlers: check Section.IsImmutable flag → return 403 Forbidden if attempting to modify archived section
- [ ] T062 [US4] Add background job support in `Src/Foundation/services/SectionRosterService/Infrastructure/Jobs/RolloverSchedulerService.cs` (IHostedService) to schedule rollover at configurable date (e.g., June 1st annually) with notification to admins
- [ ] T063 [US4] Create RolloverCompletedEventPublisher in `Src/Foundation/services/SectionRosterService/Infrastructure/Messaging/Publishers/RolloverCompletedEventPublisher.cs` with detailed statistics payload
- [ ] T064 [US4] Add logging and correlation tracking for rollover operations (long-running transaction, 5min timeout for 500 students per spec)

**Checkpoint**: Rollover automation working - sections archived, templates created, events published

---

## Phase 7: User Story 5 - Capacity & Waitlist Auto-Fill on Drop (Priority: P2)

**Goal**: When student drops from active roster, automatically promote first waitlisted student (FIFO), update capacity cache, notify via events

**Independent Test**: DELETE /api/v1/sections/{id}/roster/{studentId} → 204 No Content → StudentDroppedFromRosterEvent published → if waitlist exists, first student promoted to active roster → StudentAddedToRosterEvent published for promoted student

### Implementation for User Story 5

- [ ] T065 [P] [US5] Create DropStudentFromRosterCommand record in `Src/Foundation/services/SectionRosterService/Application/Rosters/Commands/DropStudent/DropStudentFromRosterCommand.cs` implementing IRequest<Result> with SectionId, StudentId, EffectiveDate (optional)
- [ ] T066 [P] [US5] Create DropStudentFromRosterCommandValidator in `Src/Foundation/services/SectionRosterService/Application/Rosters/Commands/DropStudent/DropStudentFromRosterCommandValidator.cs`: validate student exists in roster, EffectiveDate not in future
- [ ] T067 [US5] Implement DropStudentFromRosterCommandHandler in `Src/Foundation/services/SectionRosterService/Application/Rosters/Commands/DropStudent/DropStudentFromRosterCommandHandler.cs`: load roster entry, if EffectiveDate provided → set DropDate=EffectiveDate, IsActive=false (preserves history), else → set DropDate=now, publish StudentDroppedFromRosterEvent, query waitlist ordered by WaitlistPosition ASC, if exists → promote first (set IsActive=true, WaitlistPosition=null, renumber remaining waitlist), publish StudentAddedToRosterEvent for promoted student, update Redis capacity cache
- [ ] T068 [US5] Extend IRosterRepository with UpdateAsync, GetFirstWaitlistedAsync, RenumberWaitlistAsync methods
- [ ] T069 [US5] Extend SectionsController with DELETE /api/v1/sections/{id}/roster/{studentId} endpoint supporting optional effectiveDate query parameter
- [ ] T070 [US5] Create StudentDroppedFromRosterEventPublisher in `Src/Foundation/services/SectionRosterService/Infrastructure/Messaging/Publishers/StudentDroppedFromRosterEventPublisher.cs`
- [ ] T071 [US5] Implement waitlist promotion logic in DropStudentFromRosterCommandHandler: transaction-safe (EF SaveChanges atomic with waitlist queries to prevent race conditions)
- [ ] T072 [US5] Add audit logging for drops and promotions with effective dates and initiator (user claim)

**Checkpoint**: Automatic waitlist promotion working - no manual intervention needed when students drop

---

## Phase 8: User Story 6 - Co-Teaching Assignments & Shared Permissions (Priority: P2)

**Goal**: Support multiple teachers per section (primary + co-teachers), all have equal access to roster/gradebook, indicated in UI

**Independent Test**: POST /api/v1/sections/{id}/teachers with teacherId and assignmentType=CoTeacher → 201 Created → TeacherAssignment record added → GET /api/v1/sections/{id} shows multiple teachers

### Implementation for User Story 6

- [ ] T073 [P] [US6] Create AddCoTeacherCommand record in `Src/Foundation/services/SectionRosterService/Application/Sections/Commands/AddCoTeacher/AddCoTeacherCommand.cs` implementing IRequest<Result<Guid>> with SectionId, TeacherId
- [ ] T074 [P] [US6] Create AddCoTeacherCommandValidator in `Src/Foundation/services/SectionRosterService/Application/Sections/Commands/AddCoTeacher/AddCoTeacherCommandValidator.cs`: validate teacher not already assigned to section
- [ ] T075 [US6] Implement AddCoTeacherCommandHandler in `Src/Foundation/services/SectionRosterService/Application/Sections/Commands/AddCoTeacher/AddCoTeacherCommandHandler.cs`: validate teacher availability via StaffServiceClient, create TeacherAssignment with AssignmentType=CoTeacher, save to DB, update teacher schedule cache
- [ ] T076 [US6] Create ITeacherAssignmentRepository interface in `Src/Foundation/services/SectionRosterService/Domain/Repositories/ITeacherAssignmentRepository.cs` with AddAsync, GetBySectionAsync, ExistsAsync methods
- [ ] T077 [US6] Implement TeacherAssignmentRepository in `Src/Foundation/services/SectionRosterService/Infrastructure/Persistence/Repositories/TeacherAssignmentRepository.cs`
- [ ] T078 [US6] Extend SectionsController with POST /api/v1/sections/{id}/teachers endpoint
- [ ] T079 [US6] Update conflict detection in Phase 3 to include co-teachers: when checking teacher schedule conflicts, query all TeacherAssignments (primary + co-teachers) for period collisions
- [ ] T080 [US6] Update GetSectionByIdQuery to include all TeacherAssignments in response DTO (primary flag + co-teachers list)
- [ ] T081 [US6] Add DELETE /api/v1/sections/{id}/teachers/{teacherId} endpoint to remove co-teacher assignments

**Checkpoint**: Co-teaching support complete - multiple teachers can be assigned and validated

---

## Phase 9: User Story 7 - Search/Filter Sections with Performance (Priority: P2)

**Goal**: Enable fast search/filter by grade level, subject, period, available seats with p95 latency < 100ms

**Independent Test**: GET /api/v1/sections/search?gradeLevel=9&subject=Math&hasSeats=true → 200 OK with matching sections, response time < 100ms at p95

### Implementation for User Story 7

- [ ] T082 [P] [US7] Create SearchSectionsQuery record in `Src/Foundation/services/SectionRosterService/Application/Sections/Queries/SearchSections/SearchSectionsQuery.cs` implementing IRequest<Result<PagedResult<SectionSummaryDto>>> with GradeLevel, Subject, Period, HasSeats, PageNumber, PageSize parameters
- [ ] T083 [P] [US7] Create SearchSectionsQueryValidator in `Src/Foundation/services/SectionRosterService/Application/Sections/Queries/SearchSections/SearchSectionsQueryValidator.cs`: validate pagination limits (max 100 per page)
- [ ] T084 [US7] Implement SearchSectionsQueryHandler in `Src/Foundation/services/SectionRosterService/Application/Sections/Queries/SearchSections/SearchSectionsQueryHandler.cs`: build filtered EF query with tenant filter + provided filters, join with cached enrollment counts from Redis (fallback to DB aggregate if cache miss), calculate AvailableSeats = Capacity - EnrollmentCount, apply HasSeats filter if true (AvailableSeats > 0), paginate results, project to SectionSummaryDto
- [ ] T085 [US7] Extend ISectionRepository with SearchAsync method supporting optional filters and pagination
- [ ] T086 [US7] Create composite index on Sections (TenantId, SchoolId, GradeLevel, AcademicYear, IsActive) for fast filtered queries
- [ ] T087 [US7] Extend SectionsController with GET /api/v1/sections/search endpoint supporting query parameters: gradeLevel, subject, period, hasSeats, pageNumber, pageSize
- [ ] T088 [US7] Implement enrollment count precomputation in ScheduleCacheService: cache key `section:{tenantId}:{sectionId}:enrollment`, refresh on roster changes and hourly background task
- [ ] T089 [US7] Add full-text search support for CourseName using PostgreSQL tsvector/tsquery in SearchSectionsQueryHandler (optional, if search query parameter provided)
- [ ] T090 [US7] Add response time logging and monitoring for search queries to validate < 100ms p95 target

**Checkpoint**: Fast search/filter working with Redis-cached enrollment counts

---

## Phase 10: User Story 8 - Drop/Add with Effective Dates & History Preservation (Priority: P3)

**Goal**: Support retroactive drops and adds with effective dates, preserve full history for transcripts, never delete Roster records

**Independent Test**: DELETE /api/v1/sections/{id}/roster/{studentId}?effectiveDate=2024-09-15 → 204 No Content → Roster.DropDate=2024-09-15, IsActive=false, record preserved in DB → StudentRosterChangedEvent published with effective date

### Implementation for User Story 8

- [ ] T091 [P] [US8] Extend DropStudentFromRosterCommand (from Phase 7 T065) to support EffectiveDate parameter (already implemented in Phase 7)
- [ ] T092 [P] [US8] Extend AddStudentToRosterCommand (from Phase 4 T033) to support EffectiveDate parameter for retroactive adds
- [ ] T093 [US8] Update AddStudentToRosterCommandHandler to set EnrollmentDate = EffectiveDate if provided, else DateTime.UtcNow
- [ ] T094 [US8] Create GetStudentRosterHistoryQuery in `Src/Foundation/services/SectionRosterService/Application/Rosters/Queries/GetStudentRosterHistory/GetStudentRosterHistoryQuery.cs` implementing IRequest<Result<List<RosterHistoryDto>>> with StudentId, AcademicYear parameters
- [ ] T095 [US8] Implement GetStudentRosterHistoryQueryHandler in `Src/Foundation/services/SectionRosterService/Application/Rosters/Queries/GetStudentRosterHistory/GetStudentRosterHistoryQueryHandler.cs`: load all Roster entries for student (including dropped ones with IsActive=false), order by EnrollmentDate DESC, project to RosterHistoryDto with section details, enrollment date, drop date
- [ ] T096 [US8] Extend SectionsController with GET /api/v1/students/{studentId}/roster-history endpoint with academicYear query parameter
- [ ] T097 [US8] Create StudentRosterChangedEvent in `Src/Foundation/services/SectionRosterService/Domain/Events/StudentRosterChangedEvent.cs` with ChangeType (Added/Dropped), EffectiveDate fields
- [ ] T098 [US8] Update event publishers to emit StudentRosterChangedEvent with effective dates instead of generic add/drop events (unifies audit trail)
- [ ] T099 [US8] Add soft delete guard: prevent physical deletion of Roster records, enforce soft delete only (IsActive=false, DropDate set)

**Checkpoint**: Full roster history preserved with effective dates for transcript accuracy

---

## Phase 11: User Story 9 - Attendance Integration & Roster Validation (Priority: P3)

**Goal**: Consume AttendanceRecordedEvent, validate student is on active roster before allowing attendance submission, reject if not enrolled

**Independent Test**: Publish AttendanceRecordedEvent with studentId + sectionId + date → consumer validates roster membership → if not found, publish AttendanceRejectedEvent with reason "Student not enrolled"

### Implementation for User Story 9

- [ ] T100 [P] [US9] Create AttendanceRecordedEventConsumer in `Src/Foundation/services/SectionRosterService/Infrastructure/Messaging/Consumers/AttendanceRecordedEventConsumer.cs` implementing MassTransit IConsumer<AttendanceRecordedEvent>
- [ ] T101 [US9] Implement AttendanceRecordedEventConsumer logic: extract studentId, sectionId, attendanceDate from event, query Roster with tenant filter for active enrollment (IsActive=true, EnrollmentDate <= attendanceDate, (DropDate IS NULL OR DropDate > attendanceDate)), if not found → publish AttendanceRejectedEvent with reason, else → log success (no action needed, validation passed)
- [ ] T102 [US9] Define AttendanceRecordedEvent contract in shared Domain events (if not already in Student/Attendance service): studentId, sectionId, tenantId, attendanceDate, status
- [ ] T103 [US9] Define AttendanceRejectedEvent in `Src/Foundation/services/SectionRosterService/Domain/Events/AttendanceRejectedEvent.cs`: studentId, sectionId, tenantId, attendanceDate, rejectionReason
- [ ] T104 [US9] Register AttendanceRecordedEventConsumer in MassTransit configuration in Infrastructure DependencyInjection
- [ ] T105 [US9] Add idempotency handling in consumer using event correlation ID to prevent duplicate validation on retries
- [ ] T106 [US9] Add logging and monitoring for rejected attendance events with alert threshold (> 5% rejection rate signals roster sync issue)

**Checkpoint**: Attendance validation integrated - only enrolled students can have attendance recorded

---

## Phase 12: User Story 10 - Gradebook Roster Feed with Status (Priority: P3)

**Goal**: Provide GET endpoint for gradebook service to fetch current roster with student status (active/dropped), includes drop dates for grade exclusion logic

**Independent Test**: GET /api/v1/sections/{id}/gradebook-roster → 200 OK with array of {studentId, enrollmentDate, dropDate, isActive, status} for all roster entries

### Implementation for User Story 10

- [ ] T107 [P] [US10] Create GetGradebookRosterQuery record in `Src/Foundation/services/SectionRosterService/Application/Rosters/Queries/GetGradebookRoster/GetGradebookRosterQuery.cs` implementing IRequest<Result<GradebookRosterDto>> with SectionId
- [ ] T108 [US10] Implement GetGradebookRosterQueryHandler in `Src/Foundation/services/SectionRosterService/Application/Rosters/Queries/GetGradebookRoster/GetGradebookRosterQueryHandler.cs`: load all Roster entries for section (including dropped), project to GradebookRosterDto with studentId, enrollmentDate, dropDate, isActive, status (Active/Dropped/Waitlisted)
- [ ] T109 [US10] Create GradebookRosterDto in `Src/Foundation/services/SectionRosterService/Application/Rosters/Queries/GetGradebookRoster/GradebookRosterDto.cs` with properties: StudentId, EnrollmentDate, DropDate?, IsActive, Status enum
- [ ] T110 [US10] Extend SectionsController with GET /api/v1/sections/{id}/gradebook-roster endpoint
- [ ] T111 [US10] Add caching layer for gradebook roster queries in Redis: cache key `section:{tenantId}:{sectionId}:gradebook-roster`, TTL 1 hour, invalidate on roster changes
- [ ] T112 [US10] Document endpoint in contracts/README.md with expected response format and caching behavior for gradebook service integration

**Checkpoint**: Gradebook roster feed available with status and drop date information

---

## Phase 13: User Story 11 - Roster Export (PDF/Excel) with Audit Log (Priority: P3)

**Goal**: Enable export of section rosters to Excel/PDF formats for printing/distribution, log all export operations for compliance

**Independent Test**: GET /api/v1/sections/{id}/export?format=excel → 200 OK with Excel file attachment → audit log entry created with timestamp and user

### Implementation for User Story 11

- [ ] T113 [P] [US11] Create ExportRosterQuery record in `Src/Foundation/services/SectionRosterService/Application/Rosters/Queries/ExportRoster/ExportRosterQuery.cs` implementing IRequest<Result<FileResult>> with SectionId, Format (Excel/PDF)
- [ ] T114 [P] [US11] Create ExportRosterQueryValidator in `Src/Foundation/services/SectionRosterService/Application/Rosters/Queries/ExportRoster/ExportRosterQueryValidator.cs`: validate format is Excel or PDF
- [ ] T115 [US11] Implement ExportRosterQueryHandler in `Src/Foundation/services/SectionRosterService/Application/Rosters/Queries/ExportRoster/ExportRosterQueryHandler.cs`: load section with all rosters and teacher assignments, if Excel → generate using EPPlus library (columns: Student Name, Student ID, Enrollment Date, Status), if PDF → generate using reporting adapter (stub for now, log "PDF export not implemented"), log audit entry to AuditRecords table (user, section, timestamp, format), return FileResult with stream and content type
- [ ] T116 [US11] Install EPPlus NuGet package in Infrastructure project for Excel generation
- [ ] T117 [US11] Create IExportService interface in `Src/Foundation/services/SectionRosterService/Application/Services/IExportService.cs` with GenerateExcel and GeneratePdf methods
- [ ] T118 [US11] Implement ExcelExportService in `Src/Foundation/services/SectionRosterService/Infrastructure/Services/ExcelExportService.cs` using EPPlus to create formatted workbook with headers, data rows, section metadata
- [ ] T119 [US11] Create stub PdfExportService in `Src/Foundation/services/SectionRosterService/Infrastructure/Services/PdfExportService.cs` throwing NotImplementedException with "PDF export requires reporting service integration"
- [ ] T120 [US11] Extend SectionsController with GET /api/v1/sections/{id}/export endpoint: accept format query parameter (default Excel), return File result with appropriate content type and filename
- [ ] T121 [US11] Add audit logging to ExportRosterQueryHandler: insert AuditRecord with ActionType=RosterExport, UserId from claims, ResourceId=SectionId, Metadata json with format and timestamp
- [ ] T122 [US11] Add rate limiting for export endpoints: max 10 exports per user per minute (configured in API Program.cs middleware)

**Checkpoint**: Excel export working with audit trail, PDF stubbed for future implementation

---

## Phase 14: User Story 12 - Historical Preservation for Transcripts (Priority: P3)

**Goal**: Make sections immutable post-term (IsImmutable flag), prevent any roster/assignment changes to archived sections, enforce read-only access

**Independent Test**: Archive section via rollover → attempt PUT /api/v1/sections/{id} or POST /api/v1/sections/{id}/roster → 403 Forbidden with message "Cannot modify archived section"

### Implementation for User Story 12

- [ ] T123 [P] [US12] Extend Section entity IsImmutable flag implementation (already added in Phase 6 T057)
- [ ] T124 [US12] Create immutability guard middleware in `Src/Foundation/services/SectionRosterService/API/Middleware/ImmutabilityGuardMiddleware.cs`: intercept PUT/POST/DELETE requests to /api/v1/sections/{id}/* endpoints, load section, if IsImmutable=true → return 403 Forbidden
- [ ] T125 [US12] Register ImmutabilityGuardMiddleware in API Program.cs before controller routing
- [ ] T126 [US12] Update all command handlers (UpdateSection, AddStudent, DropStudent, AddCoTeacher) to check Section.IsImmutable flag and return Forbidden result if true (defense in depth)
- [ ] T127 [US12] Create GetArchivedSectionsQuery in `Src/Foundation/services/SectionRosterService/Application/Sections/Queries/GetArchivedSections/GetArchivedSectionsQuery.cs` for read-only access to historical sections
- [ ] T128 [US12] Implement GetArchivedSectionsQueryHandler with filters for academicYear and schoolId
- [ ] T129 [US12] Extend SectionsController with GET /api/v1/sections/archived endpoint for transcript/reporting access
- [ ] T130 [US12] Add database constraint: CHECK (IsImmutable = false OR (IsImmutable = true AND DeletedAt IS NULL)) to prevent soft-deleting archived sections
- [ ] T131 [US12] Document immutability rules in README.md: sections become immutable after rollover, read-only access preserved forever

**Checkpoint**: Historical sections immutable - transcript data integrity guaranteed

---

## Phase 15: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T132 [P] Add comprehensive logging to all command/query handlers with correlation IDs in `Src/Foundation/services/SectionRosterService/Application/*/` handlers
- [ ] T133 [P] Add OpenTelemetry instrumentation for distributed tracing in API Program.cs
- [ ] T134 [P] Implement health checks in `Src/Foundation/services/SectionRosterService/API/HealthChecks/` for DbContext, Redis, RabbitMQ
- [ ] T135 [P] Create API documentation using Swagger/OpenAPI in API Program.cs with XML comments
- [ ] T136 [P] Add response compression middleware in API Program.cs for large roster export responses
- [ ] T137 Performance testing: Load test search endpoint with 10k sections, verify p95 < 100ms latency target
- [ ] T138 Performance testing: Load test rollover endpoint with 500 students, verify completion < 5 minutes
- [ ] T139 Security audit: Verify all endpoints enforce tenant isolation via claims validation
- [ ] T140 Security audit: Verify all database queries include tenant_id filter (no cross-tenant data leaks)
- [ ] T141 [P] Add input sanitization for all text fields (SectionNumber, CourseName, Room) to prevent XSS
- [ ] T142 [P] Implement circuit breaker pattern for StaffServiceClient HTTP calls in Infrastructure
- [ ] T143 Create migration scripts for legacy data import in `Src/Foundation/services/SectionRosterService/Infrastructure/Migrations/SeedData/` for sections, rosters, assignments, periods
- [ ] T144 Update AppHost configuration to include environment-specific settings (dev/staging/prod) for connection strings and Redis cache sizing
- [ ] T145 [P] Write README.md sections: API overview, architecture diagram, setup instructions, testing guide
- [ ] T146 [P] Document event contracts in `Plan/Foundation/specs/011-section-roster-service/contracts/events.md` with schemas for all published/consumed events
- [ ] T147 Run full integration test suite: create section → add students → rollover → verify immutability → export roster → validate end-to-end flow

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-14)**: All depend on Foundational phase completion
  - US1, US2, US3 are P1 priority (MVP) - should be completed first in order
  - US4-US7 are P2 priority - can start after P1 stories complete
  - US8-US12 are P3 priority - can start after P2 stories complete
- **Polish (Phase 15)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Depends on US1 (needs sections to exist) - sequential dependency
- **User Story 3 (P1)**: Can start after Foundational - Integrates with US1 (period validation in section creation)
- **User Story 4 (P2)**: Depends on US1, US2 (needs sections and rosters to rollover)
- **User Story 5 (P2)**: Depends on US2 (extends drop functionality with waitlist promotion)
- **User Story 6 (P2)**: Depends on US1, US3 (co-teacher assignments with conflict validation)
- **User Story 7 (P2)**: Depends on US1, US2 (search needs sections and enrollment counts)
- **User Story 8 (P3)**: Extends US2, US5 (effective dates for add/drop)
- **User Story 9 (P3)**: Depends on US2 (validates roster membership)
- **User Story 10 (P3)**: Depends on US2 (provides roster feed)
- **User Story 11 (P3)**: Depends on US1, US2 (exports sections with rosters)
- **User Story 12 (P3)**: Depends on US4 (immutability set during rollover)

### Within Each User Story

- Command/Query definitions before handlers
- Validators before handlers
- Repository interfaces before implementations
- Handlers before controllers
- Event publishers after handlers
- Caching and performance optimizations after core functionality

### Parallel Opportunities

- **Phase 1 (Setup)**: Tasks T003, T004, T005, T006 can run in parallel after T001-T002 complete
- **Phase 2 (Foundational)**: Tasks T008, T009, T013, T014, T016, T017, T021, T022 can all run in parallel after T007, T010-T012 complete
- **User Story 1**: Tasks T024, T025 can run in parallel; T027 and T029 can run in parallel after T026 completes
- **User Story 2**: Tasks T033, T034 can run in parallel; T036, T037, T039, T040 can run in parallel after T035 completes
- **User Story 3**: Tasks T043, T044 can run in parallel; T046, T047, T052 can run in parallel after T045 completes
- **After MVP (US1-3)**: US4, US6, US7 can be developed in parallel by different team members (no direct dependencies)
- **Phase 15 (Polish)**: Tasks T132, T133, T134, T135, T136, T141, T142, T145, T146 can all run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch command and validator in parallel:
Task T024: "Create CreateSectionCommand record"
Task T025: "Create CreateSectionCommandValidator"

# After handler completes, launch repository interface and controller in parallel:
Task T027: "Create ISectionRepository interface"
Task T029: "Create SectionsController POST endpoint"
```

---

## Implementation Strategy

### MVP First (User Stories 1-3 Only - P1 Priority)

1. Complete Phase 1: Setup (T001-T006)
2. Complete Phase 2: Foundational (T007-T023) - CRITICAL blocking phase
3. Complete Phase 3: User Story 1 - Create Section (T024-T032)
4. Complete Phase 4: User Story 2 - Add Students to Roster (T033-T042)
5. Complete Phase 5: User Story 3 - Period Configuration & Conflict Validation (T043-T053)
6. **STOP and VALIDATE**: Test MVP independently (create sections → assign teachers → add students → validate conflicts)
7. Deploy/demo if ready

**MVP Delivers**: Core scheduling functionality - create sections with teachers, enroll students with capacity tracking and waitlist, prevent scheduling conflicts for teachers/rooms/students

### Incremental Delivery (Add P2 Stories)

1. Complete MVP (Phases 1-5)
2. Add Phase 6: User Story 4 - Year-End Rollover (T054-T064) → Test rollover automation
3. Add Phase 7: User Story 5 - Waitlist Auto-Fill (T065-T072) → Test automatic promotion
4. Add Phase 8: User Story 6 - Co-Teaching (T073-T081) → Test multiple teachers
5. Add Phase 9: User Story 7 - Search/Filter (T082-T090) → Test performance at scale
6. **VALIDATE**: Test P1 + P2 stories together, measure search performance, verify rollover at 500 students < 5min

### Full Feature Set (Add P3 Stories)

1. Complete MVP + P2 stories (Phases 1-9)
2. Add Phase 10: User Story 8 - Effective Dates (T091-T099) → Test history preservation
3. Add Phase 11: User Story 9 - Attendance Integration (T100-T106) → Test event-driven validation
4. Add Phase 12: User Story 10 - Gradebook Feed (T107-T112) → Test gradebook integration
5. Add Phase 13: User Story 11 - Roster Export (T113-T122) → Test Excel/PDF generation
6. Add Phase 14: User Story 12 - Historical Preservation (T123-T131) → Test immutability enforcement
7. Add Phase 15: Polish (T132-T147) → Performance testing, security audit, documentation
8. **FINAL VALIDATION**: Run full integration test suite, load testing, security audit

### Parallel Team Strategy

With multiple developers after Foundational phase (Phase 2) completes:

- **Developer A**: User Story 1 → User Story 4 → User Story 7 → User Story 11
- **Developer B**: User Story 2 → User Story 5 → User Story 9 → User Story 10
- **Developer C**: User Story 3 → User Story 6 → User Story 8 → User Story 12

Stories complete and integrate independently, reducing merge conflicts.

---

## Notes

- [P] tasks = different files, no dependencies, can run in parallel
- [Story] label maps task to specific user story for traceability (US1-US12)
- Each user story delivers independently testable functionality
- MVP (US1-3) provides core value: section creation, enrollment, conflict validation
- P2 stories add automation (rollover, waitlist) and co-teaching
- P3 stories add integration (attendance, gradebook), export, and compliance (immutability)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Redis caching is critical for performance targets (search < 100ms, rollover < 5min for 500 students)
- All handlers use Result/Result<T> pattern for consistent error handling
- All entities enforce tenant isolation via TenantId with global query filters
- All events include correlation IDs for distributed tracing
- Immutability enforced at multiple layers (middleware, handlers, constraints) for transcript integrity

---

## Summary

- **Total Tasks**: 147
- **MVP Tasks (P1 - US1-3)**: T001-T053 (53 tasks)
- **P2 Tasks (US4-7)**: T054-T090 (37 tasks)
- **P3 Tasks (US8-12)**: T091-T131 (41 tasks)
- **Polish Tasks**: T132-T147 (16 tasks)

**Task Count per User Story**:
- Setup (Phase 1): 6 tasks
- Foundational (Phase 2): 17 tasks
- US1 (Create Section): 9 tasks
- US2 (Add Students to Roster): 10 tasks
- US3 (Period & Conflict Validation): 11 tasks
- US4 (Rollover): 11 tasks
- US5 (Waitlist Auto-Fill): 8 tasks
- US6 (Co-Teaching): 9 tasks
- US7 (Search/Filter): 9 tasks
- US8 (Effective Dates): 9 tasks
- US9 (Attendance Integration): 7 tasks
- US10 (Gradebook Feed): 6 tasks
- US11 (Roster Export): 10 tasks
- US12 (Historical Preservation): 9 tasks
- Polish: 16 tasks

**Parallel Opportunities Identified**:
- Phase 1: 4 tasks can run in parallel
- Phase 2: 8 tasks can run in parallel
- Each user story has 2-5 tasks that can run in parallel
- After Foundational phase, all user stories can start in parallel if team capacity allows

**Independent Test Criteria**:
- US1: Create section via API → section exists in DB with teacher assignment → event published
- US2: Add student via API → roster entry created → capacity cached → event published
- US3: Create section with conflict → 409 error; create valid section → success
- US4: Run rollover dry-run → preview returned; run execute → sections archived, templates created
- US5: Drop student with waitlist → first waitlisted student promoted automatically
- US6: Add co-teacher → both teachers listed in section details
- US7: Search sections with filters → results match criteria, response time < 100ms
- US8: Drop student with effective date → history preserved, drop date recorded
- US9: Publish attendance event for non-enrolled student → rejection event published
- US10: Get gradebook roster → all students with status and drop dates returned
- US11: Export roster to Excel → file downloaded, audit log entry created
- US12: Attempt to modify archived section → 403 Forbidden error

**Suggested MVP Scope**: User Stories 1-3 (Phases 1-5, tasks T001-T053) delivers core scheduling functionality with conflict validation.

**Format Validation**: ✅ All tasks follow the checklist format with checkbox, sequential ID, [P] marker for parallel tasks, [Story] label for user story phases, and file paths in descriptions.
