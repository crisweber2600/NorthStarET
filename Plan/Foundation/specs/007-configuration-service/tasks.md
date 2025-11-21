# Tasks: Configuration Service Migration

**Specification Branch**: `Foundation/007-configuration-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/007-configuration-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/007-configuration-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/README.md, quickstart.md

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Configuration`  
**Specification Path**: `Plan/Foundation/specs/007-configuration-service/`

### Layer Consistency Checklist

- [x] Target Layer matches spec.md Layer Identification
- [x] Target Layer matches plan.md Layer Identification
- [x] Implementation path follows layer structure (`Src/Foundation/...`)
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)
- [x] Shared infrastructure dependencies match between spec and plan
- [x] Cross-layer dependencies justified in both spec and plan (Foundation shared infrastructure only)

---

## Layer Compliance Validation

*MANDATORY: Include these validation tasks to ensure mono-repo layer isolation (Constitution Principle 6)*

- [ ] V001 Verify project references ONLY shared infrastructure from approved layers (`Src/Foundation/shared/*`)
- [ ] V002 Verify NO direct service-to-service references across layers (must use events/contracts for cross-layer communication)
- [ ] V003 Verify AppHost orchestration includes Configuration service with correct layer isolation
- [ ] V004 Verify README.md documents layer position and shared infrastructure dependencies
- [ ] V005 Verify no circular dependencies between layers (Foundation cannot depend on higher layers)

---

## Identity & Authentication Compliance

*MANDATORY: Include if this feature requires authentication/authorization*

- [ ] A001 Verify NO references to Duende IdentityServer or custom token issuance
- [ ] A002 Verify Microsoft.Identity.Web used for JWT token validation (NOT custom JWT generation)
- [ ] A003 Verify SessionAuthenticationHandler registered for session-based API authorization
- [ ] A004 Verify Redis configured for session caching (Aspire.Hosting.Redis)
- [ ] A005 Verify identity.sessions table includes tenant_id for multi-tenancy
- [ ] A006 Verify TokenExchangeService implements BFF pattern (Entra tokens → LMS sessions)
- [ ] A007 Verify authentication flow follows `legacy-identityserver-migration.md` architecture

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for Configuration Service

- [ ] T001 Create Configuration service solution structure in `Src/Foundation/services/Configuration/`
- [ ] T002 [P] Initialize Configuration.Domain project with base entities and value objects
- [ ] T003 [P] Initialize Configuration.Application project with MediatR and FluentValidation dependencies
- [ ] T004 [P] Initialize Configuration.Infrastructure project with EF Core, Redis, and MassTransit dependencies
- [ ] T005 [P] Initialize Configuration.Api project with ASP.NET Core and ServiceDefaults reference
- [ ] T006 [P] Initialize Configuration.Tests.Unit project with xUnit and testing dependencies
- [ ] T007 [P] Initialize Configuration.Tests.Integration project with Aspire and WebApplicationFactory
- [ ] T008 [P] Configure centralized package management in `Src/Foundation/services/Configuration/Directory.Packages.props`
- [ ] T009 Create Configuration.AppHost project for local orchestration with PostgreSQL, Redis, RabbitMQ
- [ ] T010 [P] Add Configuration service to root AppHost in `Src/Foundation/AppHost/Program.cs`
- [ ] T011 [P] Create README.md documenting Configuration service architecture and dependencies
- [ ] T012 Configure solution-level EditorConfig and linting rules

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Database and Persistence Foundation

- [ ] T013 Create ConfigurationDbContext in `Src/Foundation/services/Configuration/Infrastructure/Data/ConfigurationDbContext.cs`
- [ ] T014 [P] Configure PostgreSQL connection with Aspire in Configuration.AppHost
- [ ] T015 [P] Implement TenantInterceptor for multi-tenancy enforcement in `Infrastructure/Data/Interceptors/TenantInterceptor.cs`
- [ ] T016 [P] Implement AuditInterceptor for immutable audit records in `Infrastructure/Data/Interceptors/AuditInterceptor.cs`
- [ ] T017 Configure Row-Level Security (RLS) policies for tenant isolation in migration scripts
- [ ] T018 Create initial EF Core migration for Configuration database schema

### Domain Entities (Core Models)

- [ ] T019 [P] Create SettingDefinition entity in `Domain/Entities/SettingDefinition.cs`
- [ ] T020 [P] Create SettingValue entity with hierarchy support in `Domain/Entities/SettingValue.cs`
- [ ] T021 [P] Create ResolvedSetting entity for materialized views in `Domain/Entities/ResolvedSetting.cs`
- [ ] T022 [P] Create AuditRecord entity in `Domain/Entities/AuditRecord.cs`
- [ ] T023 [P] Create SettingScope enum (System, District, School) in `Domain/Enums/SettingScope.cs`
- [ ] T024 Configure entity relationships and constraints in ConfigurationDbContext

### Cache Infrastructure

- [ ] T025 Configure Redis Stack with Aspire in Configuration.AppHost
- [ ] T026 Implement IConfigurationCacheService interface in `Application/Interfaces/IConfigurationCacheService.cs`
- [ ] T027 Implement ConfigurationCacheService with Redis client in `Infrastructure/Caching/ConfigurationCacheService.cs`
- [ ] T028 Implement cache key generation strategy (`cfg:{tenant}:{school}:{key}`) in ConfigurationCacheService
- [ ] T029 Implement cache invalidation on write operations in ConfigurationCacheService

### Resolution Engine

- [ ] T030 Create IHierarchyResolver interface in `Application/Interfaces/IHierarchyResolver.cs`
- [ ] T031 Implement HierarchyResolver with deterministic precedence (System → District → School) in `Infrastructure/Resolution/HierarchyResolver.cs`
- [ ] T032 Implement ResolvedSetting materialization logic in HierarchyResolver
- [ ] T033 Add unit tests for hierarchy resolution edge cases in `Tests.Unit/Resolution/HierarchyResolverTests.cs`

### Event Publishing Infrastructure

- [ ] T034 Configure MassTransit with Azure Service Bus in `Infrastructure/DependencyInjection.cs`
- [ ] T035 [P] Define ConfigurationChanged event contract in `Domain/Events/ConfigurationChanged.cs`
- [ ] T036 [P] Define CalendarPublished event contract in `Domain/Events/CalendarPublished.cs`
- [ ] T037 [P] Define GradingScaleUpdated event contract in `Domain/Events/GradingScaleUpdated.cs`
- [ ] T038 [P] Implement IEventPublisher interface in `Application/Interfaces/IEventPublisher.cs`
- [ ] T039 Implement EventPublisher with MassTransit in `Infrastructure/Messaging/EventPublisher.cs`

### API Infrastructure

- [ ] T040 Configure API routing and middleware pipeline in `Api/Program.cs`
- [ ] T041 [P] Add ServiceDefaults with telemetry and health checks in `Api/Program.cs`
- [ ] T042 [P] Implement global exception handler in `Api/Middleware/ExceptionHandlingMiddleware.cs`
- [ ] T043 [P] Configure Swagger/OpenAPI with authentication in `Api/Program.cs`
- [ ] T044 Register authentication using SessionAuthenticationHandler in `Api/Program.cs`
- [ ] T045 [P] Implement tenant context extraction from gateway headers in `Api/Middleware/TenantContextMiddleware.cs`
- [ ] T046 [P] Configure CORS policies in `Api/Program.cs`

### Validation and Error Handling

- [ ] T047 Create Result and Result<T> types for command/query responses in `Domain/Common/Result.cs`
- [ ] T048 [P] Create ValidationError type in `Domain/Common/ValidationError.cs`
- [ ] T049 [P] Configure FluentValidation pipeline behavior in `Application/Behaviors/ValidationBehavior.cs`
- [ ] T050 Create base validator classes in `Application/Validators/BaseValidator.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Manage tenant configuration with override hierarchy (Priority: P1) 🎯 MVP

**Goal**: District admins can create/update tenant-scoped settings with hierarchical overrides, downstream services consume resolved values via API

**Independent Test**: Update a district-level setting, verify school inheritance or override, confirm downstream resolution returns expected value and publishes change event

### Domain and Data Models for US1

- [ ] T051 [P] [US1] Add unique constraint on (definition_id, tenant_id, scope, scope_ref_id) in SettingValue entity
- [ ] T052 [P] [US1] Implement validation rules for SettingValue in `Domain/Entities/SettingValue.cs`
- [ ] T053 [P] [US1] Create migration for SettingValue constraints

### Application Layer for US1

- [ ] T054 [P] [US1] Create CreateSettingCommand in `Application/Features/Settings/Commands/CreateSetting/CreateSettingCommand.cs`
- [ ] T055 [P] [US1] Create UpdateSettingCommand in `Application/Features/Settings/Commands/UpdateSetting/UpdateSettingCommand.cs`
- [ ] T056 [P] [US1] Create GetResolvedSettingsQuery in `Application/Features/Settings/Queries/GetResolvedSettings/GetResolvedSettingsQuery.cs`
- [ ] T057 [P] [US1] Create GetSettingByIdQuery in `Application/Features/Settings/Queries/GetSettingById/GetSettingByIdQuery.cs`
- [ ] T058 [US1] Implement CreateSettingCommandHandler with audit in `Application/Features/Settings/Commands/CreateSetting/CreateSettingCommandHandler.cs`
- [ ] T059 [US1] Implement UpdateSettingCommandHandler with audit in `Application/Features/Settings/Commands/UpdateSetting/UpdateSettingCommandHandler.cs`
- [ ] T060 [US1] Implement GetResolvedSettingsQueryHandler with cache-first strategy in `Application/Features/Settings/Queries/GetResolvedSettings/GetResolvedSettingsQueryHandler.cs`
- [ ] T061 [US1] Implement GetSettingByIdQueryHandler in `Application/Features/Settings/Queries/GetSettingById/GetSettingByIdQueryHandler.cs`
- [ ] T062 [P] [US1] Create CreateSettingValidator in `Application/Features/Settings/Commands/CreateSetting/CreateSettingValidator.cs`
- [ ] T063 [P] [US1] Create UpdateSettingValidator in `Application/Features/Settings/Commands/UpdateSetting/UpdateSettingValidator.cs`

### Infrastructure Layer for US1

- [ ] T064 [US1] Implement ISettingRepository in `Infrastructure/Repositories/SettingRepository.cs` with tenant filtering
- [ ] T065 [US1] Implement cache write-through on CreateSetting in CreateSettingCommandHandler
- [ ] T066 [US1] Implement cache invalidation on UpdateSetting in UpdateSettingCommandHandler
- [ ] T067 [US1] Implement ConfigurationChanged event publishing in command handlers

### API Endpoints for US1

- [ ] T068 [P] [US1] Create SettingsController in `Api/Controllers/SettingsController.cs`
- [ ] T069 [US1] Implement POST /api/config/settings endpoint for CreateSettingCommand
- [ ] T070 [US1] Implement PUT /api/config/settings/{id} endpoint for UpdateSettingCommand
- [ ] T071 [US1] Implement GET /api/config/resolved endpoint for GetResolvedSettingsQuery
- [ ] T072 [US1] Implement GET /api/config/settings/{id} endpoint for GetSettingByIdQuery
- [ ] T073 [US1] Add request/response DTOs in `Api/Models/Settings/`
- [ ] T074 [US1] Add API documentation and examples to SettingsController

### Testing for US1

- [ ] T075 [P] [US1] Unit tests for CreateSettingCommandHandler in `Tests.Unit/Features/Settings/CreateSettingCommandHandlerTests.cs`
- [ ] T076 [P] [US1] Unit tests for UpdateSettingCommandHandler in `Tests.Unit/Features/Settings/UpdateSettingCommandHandlerTests.cs`
- [ ] T077 [P] [US1] Unit tests for GetResolvedSettingsQueryHandler in `Tests.Unit/Features/Settings/GetResolvedSettingsQueryHandlerTests.cs`
- [ ] T078 [P] [US1] Unit tests for hierarchy resolution with override scenarios in `Tests.Unit/Resolution/HierarchyResolverTests.cs`
- [ ] T079 [US1] Integration test for create setting flow in `Tests.Integration/Settings/CreateSettingTests.cs`
- [ ] T080 [US1] Integration test for update setting with cache invalidation in `Tests.Integration/Settings/UpdateSettingTests.cs`
- [ ] T081 [US1] Integration test for resolved settings with school override in `Tests.Integration/Settings/ResolvedSettingsTests.cs`
- [ ] T082 [US1] Contract test for ConfigurationChanged event schema in `Tests.Integration/Events/ConfigurationChangedTests.cs`
- [ ] T083 [US1] Performance test for GET /api/config/resolved p95 <50ms target in `Tests.Integration/Performance/ResolvedSettingsPerformanceTests.cs`

**Checkpoint**: User Story 1 complete - setting management with hierarchical resolution functional and tested

---

## Phase 4: User Story 2 - Configure calendars and academic structures (Priority: P2)

**Goal**: Administrators manage academic calendars, terms, grading periods, and blackout dates with conflict detection

**Independent Test**: Create a calendar with terms and blackout dates, verify conflicts are detected, ensure calendar resolutions are available to consuming services

### Domain Entities for US2

- [ ] T084 [P] [US2] Create AcademicCalendar entity in `Domain/Entities/AcademicCalendar.cs`
- [ ] T085 [P] [US2] Create Term entity in `Domain/Entities/Term.cs`
- [ ] T086 [P] [US2] Create Session entity in `Domain/Entities/Session.cs`
- [ ] T087 [P] [US2] Create GradingPeriod entity in `Domain/Entities/GradingPeriod.cs`
- [ ] T088 [P] [US2] Create BlackoutDate entity in `Domain/Entities/BlackoutDate.cs`
- [ ] T089 [P] [US2] Create CalendarStatus enum (Draft, Published, Archived) in `Domain/Enums/CalendarStatus.cs`
- [ ] T090 [US2] Configure calendar entity relationships in ConfigurationDbContext
- [ ] T091 [US2] Add exclusion constraints for date range overlaps in calendar migration
- [ ] T092 [US2] Create migration for calendar schema

### Application Layer for US2

- [ ] T093 [P] [US2] Create CreateCalendarCommand in `Application/Features/Calendars/Commands/CreateCalendar/CreateCalendarCommand.cs`
- [ ] T094 [P] [US2] Create UpdateCalendarCommand in `Application/Features/Calendars/Commands/UpdateCalendar/UpdateCalendarCommand.cs`
- [ ] T095 [P] [US2] Create PublishCalendarCommand in `Application/Features/Calendars/Commands/PublishCalendar/PublishCalendarCommand.cs`
- [ ] T096 [P] [US2] Create AddTermCommand in `Application/Features/Calendars/Commands/AddTerm/AddTermCommand.cs`
- [ ] T097 [P] [US2] Create AddBlackoutDateCommand in `Application/Features/Calendars/Commands/AddBlackoutDate/AddBlackoutDateCommand.cs`
- [ ] T098 [P] [US2] Create GetCalendarQuery in `Application/Features/Calendars/Queries/GetCalendar/GetCalendarQuery.cs`
- [ ] T099 [P] [US2] Create GetCalendarsByTenantQuery in `Application/Features/Calendars/Queries/GetCalendarsByTenant/GetCalendarsByTenantQuery.cs`
- [ ] T100 [US2] Implement CreateCalendarCommandHandler with tenant validation in `Application/Features/Calendars/Commands/CreateCalendar/CreateCalendarCommandHandler.cs`
- [ ] T101 [US2] Implement UpdateCalendarCommandHandler in `Application/Features/Calendars/Commands/UpdateCalendar/UpdateCalendarCommandHandler.cs`
- [ ] T102 [US2] Implement PublishCalendarCommandHandler with conflict detection in `Application/Features/Calendars/Commands/PublishCalendar/PublishCalendarCommandHandler.cs`
- [ ] T103 [US2] Implement AddTermCommandHandler with overlap validation in `Application/Features/Calendars/Commands/AddTerm/AddTermCommandHandler.cs`
- [ ] T104 [US2] Implement AddBlackoutDateCommandHandler with conflict detection in `Application/Features/Calendars/Commands/AddBlackoutDate/AddBlackoutDateCommandHandler.cs`
- [ ] T105 [US2] Implement GetCalendarQueryHandler in `Application/Features/Calendars/Queries/GetCalendar/GetCalendarQueryHandler.cs`
- [ ] T106 [US2] Implement GetCalendarsByTenantQueryHandler in `Application/Features/Calendars/Queries/GetCalendarsByTenant/GetCalendarsByTenantQueryHandler.cs`
- [ ] T107 [P] [US2] Create calendar validators in `Application/Features/Calendars/Validators/`
- [ ] T108 [US2] Implement date range conflict detection service in `Application/Services/CalendarConflictDetectionService.cs`

### Infrastructure Layer for US2

- [ ] T109 [US2] Implement ICalendarRepository in `Infrastructure/Repositories/CalendarRepository.cs`
- [ ] T110 [US2] Implement CalendarPublished event publishing in PublishCalendarCommandHandler

### API Endpoints for US2

- [ ] T111 [P] [US2] Create CalendarsController in `Api/Controllers/CalendarsController.cs`
- [ ] T112 [US2] Implement POST /api/config/calendars endpoint
- [ ] T113 [US2] Implement PUT /api/config/calendars/{id} endpoint
- [ ] T114 [US2] Implement POST /api/config/calendars/{id}/publish endpoint
- [ ] T115 [US2] Implement POST /api/config/calendars/{id}/terms endpoint
- [ ] T116 [US2] Implement POST /api/config/calendars/{id}/blackout-dates endpoint
- [ ] T117 [US2] Implement GET /api/config/calendars/{id} endpoint
- [ ] T118 [US2] Implement GET /api/config/calendars endpoint with tenant filtering
- [ ] T119 [US2] Add calendar DTOs in `Api/Models/Calendars/`

### Testing for US2

- [ ] T120 [P] [US2] Unit tests for calendar command handlers in `Tests.Unit/Features/Calendars/`
- [ ] T121 [P] [US2] Unit tests for conflict detection service in `Tests.Unit/Services/CalendarConflictDetectionServiceTests.cs`
- [ ] T122 [US2] Integration test for calendar creation with terms in `Tests.Integration/Calendars/CreateCalendarTests.cs`
- [ ] T123 [US2] Integration test for term overlap validation in `Tests.Integration/Calendars/TermOverlapTests.cs`
- [ ] T124 [US2] Integration test for blackout date conflict detection in `Tests.Integration/Calendars/BlackoutDateConflictTests.cs`
- [ ] T125 [US2] Integration test for calendar publish with downstream notification in `Tests.Integration/Calendars/PublishCalendarTests.cs`
- [ ] T126 [US2] Contract test for CalendarPublished event schema in `Tests.Integration/Events/CalendarPublishedTests.cs`

**Checkpoint**: User Story 2 complete - calendar management with conflict detection functional

---

## Phase 5: User Story 3 - Manage grading scales, custom attributes, and templates (Priority: P3)

**Goal**: District or school admins define grading scales, custom attributes, and notification templates with validation and audit history

**Independent Test**: Create a grading scale, add a custom student attribute, publish a notification template; verify retrieval, validation rules, and audit entries

### Domain Entities for US3

- [ ] T127 [P] [US3] Create GradingScale entity in `Domain/Entities/GradingScale.cs`
- [ ] T128 [P] [US3] Create GradingScaleBand value object in `Domain/ValueObjects/GradingScaleBand.cs`
- [ ] T129 [P] [US3] Create CustomAttributeDefinition entity in `Domain/Entities/CustomAttributeDefinition.cs`
- [ ] T130 [P] [US3] Create NotificationTemplate entity in `Domain/Entities/NotificationTemplate.cs`
- [ ] T131 [P] [US3] Create AttributeScope enum (Student, Staff, Section, Intervention, Assessment) in `Domain/Enums/AttributeScope.cs`
- [ ] T132 [US3] Configure grading scale and template relationships in ConfigurationDbContext
- [ ] T133 [US3] Create migration for grading scales, custom attributes, and templates

### Application Layer for US3 - Grading Scales

- [ ] T134 [P] [US3] Create CreateGradingScaleCommand in `Application/Features/GradingScales/Commands/CreateGradingScale/CreateGradingScaleCommand.cs`
- [ ] T135 [P] [US3] Create UpdateGradingScaleCommand in `Application/Features/GradingScales/Commands/UpdateGradingScale/UpdateGradingScaleCommand.cs`
- [ ] T136 [P] [US3] Create GetGradingScaleQuery in `Application/Features/GradingScales/Queries/GetGradingScale/GetGradingScaleQuery.cs`
- [ ] T137 [US3] Implement CreateGradingScaleCommandHandler with overlap validation in `Application/Features/GradingScales/Commands/CreateGradingScale/CreateGradingScaleCommandHandler.cs`
- [ ] T138 [US3] Implement UpdateGradingScaleCommandHandler in `Application/Features/GradingScales/Commands/UpdateGradingScale/UpdateGradingScaleCommandHandler.cs`
- [ ] T139 [US3] Implement GetGradingScaleQueryHandler in `Application/Features/GradingScales/Queries/GetGradingScale/GetGradingScaleQueryHandler.cs`
- [ ] T140 [P] [US3] Create grading scale validators in `Application/Features/GradingScales/Validators/`
- [ ] T141 [US3] Implement threshold overlap validation in grading scale validator

### Application Layer for US3 - Custom Attributes

- [ ] T142 [P] [US3] Create CreateCustomAttributeCommand in `Application/Features/CustomAttributes/Commands/CreateCustomAttribute/CreateCustomAttributeCommand.cs`
- [ ] T143 [P] [US3] Create UpdateCustomAttributeCommand in `Application/Features/CustomAttributes/Commands/UpdateCustomAttribute/UpdateCustomAttributeCommand.cs`
- [ ] T144 [P] [US3] Create DeleteCustomAttributeCommand in `Application/Features/CustomAttributes/Commands/DeleteCustomAttribute/DeleteCustomAttributeCommand.cs`
- [ ] T145 [P] [US3] Create GetCustomAttributesQuery in `Application/Features/CustomAttributes/Queries/GetCustomAttributes/GetCustomAttributesQuery.cs`
- [ ] T146 [US3] Implement CreateCustomAttributeCommandHandler with uniqueness check in `Application/Features/CustomAttributes/Commands/CreateCustomAttribute/CreateCustomAttributeCommandHandler.cs`
- [ ] T147 [US3] Implement UpdateCustomAttributeCommandHandler in `Application/Features/CustomAttributes/Commands/UpdateCustomAttribute/UpdateCustomAttributeCommandHandler.cs`
- [ ] T148 [US3] Implement DeleteCustomAttributeCommandHandler with soft delete in `Application/Features/CustomAttributes/Commands/DeleteCustomAttribute/DeleteCustomAttributeCommandHandler.cs`
- [ ] T149 [US3] Implement GetCustomAttributesQueryHandler with scope filtering in `Application/Features/CustomAttributes/Queries/GetCustomAttributes/GetCustomAttributesQueryHandler.cs`
- [ ] T150 [P] [US3] Create custom attribute validators with collision prevention in `Application/Features/CustomAttributes/Validators/`

### Application Layer for US3 - Notification Templates

- [ ] T151 [P] [US3] Create CreateTemplateCommand in `Application/Features/Templates/Commands/CreateTemplate/CreateTemplateCommand.cs`
- [ ] T152 [P] [US3] Create UpdateTemplateCommand in `Application/Features/Templates/Commands/UpdateTemplate/UpdateTemplateCommand.cs`
- [ ] T153 [P] [US3] Create PublishTemplateCommand in `Application/Features/Templates/Commands/PublishTemplate/PublishTemplateCommand.cs`
- [ ] T154 [P] [US3] Create PreviewTemplateQuery in `Application/Features/Templates/Queries/PreviewTemplate/PreviewTemplateQuery.cs`
- [ ] T155 [P] [US3] Create GetTemplateQuery in `Application/Features/Templates/Queries/GetTemplate/GetTemplateQuery.cs`
- [ ] T156 [US3] Implement CreateTemplateCommandHandler in `Application/Features/Templates/Commands/CreateTemplate/CreateTemplateCommandHandler.cs`
- [ ] T157 [US3] Implement UpdateTemplateCommandHandler with version history in `Application/Features/Templates/Commands/UpdateTemplate/UpdateTemplateCommandHandler.cs`
- [ ] T158 [US3] Implement PublishTemplateCommandHandler in `Application/Features/Templates/Commands/PublishTemplate/PublishTemplateCommandHandler.cs`
- [ ] T159 [US3] Implement PreviewTemplateQueryHandler with merge field substitution in `Application/Features/Templates/Queries/PreviewTemplate/PreviewTemplateQueryHandler.cs`
- [ ] T160 [US3] Implement GetTemplateQueryHandler in `Application/Features/Templates/Queries/GetTemplate/GetTemplateQueryHandler.cs`
- [ ] T161 [P] [US3] Create template validators with merge field validation in `Application/Features/Templates/Validators/`
- [ ] T162 [US3] Implement merge field validation service in `Application/Services/MergeFieldValidationService.cs`

### Infrastructure Layer for US3

- [ ] T163 [P] [US3] Implement IGradingScaleRepository in `Infrastructure/Repositories/GradingScaleRepository.cs`
- [ ] T164 [P] [US3] Implement ICustomAttributeRepository in `Infrastructure/Repositories/CustomAttributeRepository.cs`
- [ ] T165 [P] [US3] Implement ITemplateRepository in `Infrastructure/Repositories/TemplateRepository.cs`
- [ ] T166 [US3] Implement GradingScaleUpdated event publishing in command handlers
- [ ] T167 [US3] Implement CustomAttributeUpdated event publishing in command handlers
- [ ] T168 [US3] Implement TemplatePublished event publishing in command handlers

### API Endpoints for US3

- [ ] T169 [P] [US3] Create GradingScalesController in `Api/Controllers/GradingScalesController.cs`
- [ ] T170 [US3] Implement POST /api/config/grading-scales endpoint
- [ ] T171 [US3] Implement PUT /api/config/grading-scales/{id} endpoint
- [ ] T172 [US3] Implement GET /api/config/grading-scales/{id} endpoint
- [ ] T173 [P] [US3] Create CustomAttributesController in `Api/Controllers/CustomAttributesController.cs`
- [ ] T174 [US3] Implement POST /api/config/custom-attributes endpoint
- [ ] T175 [US3] Implement PUT /api/config/custom-attributes/{id} endpoint
- [ ] T176 [US3] Implement DELETE /api/config/custom-attributes/{id} endpoint
- [ ] T177 [US3] Implement GET /api/config/custom-attributes endpoint
- [ ] T178 [P] [US3] Create TemplatesController in `Api/Controllers/TemplatesController.cs`
- [ ] T179 [US3] Implement POST /api/config/templates endpoint
- [ ] T180 [US3] Implement PUT /api/config/templates/{id} endpoint
- [ ] T181 [US3] Implement POST /api/config/templates/{id}/publish endpoint
- [ ] T182 [US3] Implement GET /api/config/templates/{id}/preview endpoint
- [ ] T183 [US3] Implement GET /api/config/templates/{id} endpoint
- [ ] T184 [US3] Add grading scale, custom attribute, and template DTOs in `Api/Models/`

### Testing for US3

- [ ] T185 [P] [US3] Unit tests for grading scale command handlers in `Tests.Unit/Features/GradingScales/`
- [ ] T186 [P] [US3] Unit tests for custom attribute command handlers in `Tests.Unit/Features/CustomAttributes/`
- [ ] T187 [P] [US3] Unit tests for template command handlers in `Tests.Unit/Features/Templates/`
- [ ] T188 [P] [US3] Unit tests for threshold overlap validation in `Tests.Unit/Validators/GradingScaleValidatorTests.cs`
- [ ] T189 [P] [US3] Unit tests for merge field validation in `Tests.Unit/Services/MergeFieldValidationServiceTests.cs`
- [ ] T190 [US3] Integration test for grading scale creation with overlap prevention in `Tests.Integration/GradingScales/CreateGradingScaleTests.cs`
- [ ] T191 [US3] Integration test for custom attribute uniqueness enforcement in `Tests.Integration/CustomAttributes/CustomAttributeUniquenessTests.cs`
- [ ] T192 [US3] Integration test for template versioning in `Tests.Integration/Templates/TemplateVersioningTests.cs`
- [ ] T193 [US3] Integration test for template preview with merge fields in `Tests.Integration/Templates/TemplatePreviewTests.cs`
- [ ] T194 [US3] Contract tests for US3 events in `Tests.Integration/Events/`

**Checkpoint**: User Story 3 complete - grading scales, custom attributes, and templates functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and finalization

### Search and Filtering

- [ ] T195 [P] Create SearchConfigurationQuery in `Application/Features/Search/Queries/SearchConfiguration/SearchConfigurationQuery.cs`
- [ ] T196 Implement SearchConfigurationQueryHandler with entity type filtering in `Application/Features/Search/Queries/SearchConfiguration/SearchConfigurationQueryHandler.cs`
- [ ] T197 Create SearchController in `Api/Controllers/SearchController.cs`
- [ ] T198 Implement GET /api/config/search endpoint with query parameters
- [ ] T199 [P] Add search integration tests in `Tests.Integration/Search/SearchConfigurationTests.cs`

### Audit and Compliance

- [ ] T200 Verify all write operations generate AuditRecord entries
- [ ] T201 [P] Add audit query endpoints in `Api/Controllers/AuditController.cs`
- [ ] T202 [P] Implement GET /api/config/audit endpoint for audit history retrieval
- [ ] T203 Add audit history integration tests in `Tests.Integration/Audit/AuditHistoryTests.cs`

### Performance and Observability

- [ ] T204 Add OpenTelemetry instrumentation for cache operations
- [ ] T205 [P] Add distributed tracing for hierarchy resolution
- [ ] T206 [P] Add metrics for cache hit/miss rates
- [ ] T207 [P] Add metrics for event publishing latency
- [ ] T208 Add health checks for PostgreSQL, Redis, and RabbitMQ in `Api/Program.cs`
- [ ] T209 Verify p95 read latency <50ms target in load tests

### Documentation

- [ ] T210 [P] Update README.md with complete API documentation
- [ ] T211 [P] Add architecture diagrams to documentation
- [ ] T212 [P] Document cache invalidation strategy
- [ ] T213 [P] Document event contracts and consumer patterns
- [ ] T214 Update quickstart.md with complete setup instructions

### Code Quality

- [ ] T215 Run code coverage analysis and ensure ≥80% coverage
- [ ] T216 [P] Run static analysis and address warnings
- [ ] T217 [P] Perform security scan for vulnerabilities
- [ ] T218 Code review and refactoring for maintainability
- [ ] T219 Update CHANGELOG.md with feature summary

### Integration Testing

- [ ] T220 End-to-end test for complete setting lifecycle (create → update → resolve → cache invalidation)
- [ ] T221 End-to-end test for calendar with terms and conflicts
- [ ] T222 End-to-end test for grading scale with validation
- [ ] T223 Validate all success criteria from spec.md
- [ ] T224 Run quickstart.md validation with AppHost

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User Story 1 (P1): Can start after Foundational - No dependencies on other stories
  - User Story 2 (P2): Can start after Foundational - Independent of US1
  - User Story 3 (P3): Can start after Foundational - Independent of US1 and US2
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundation only - Core setting management with hierarchy
- **User Story 2 (P2)**: Foundation only - Calendar management independent of settings
- **User Story 3 (P3)**: Foundation only - Grading scales, attributes, templates independent of settings and calendars

### Within Each User Story

- Domain entities before application layer
- Application layer (commands/queries) before infrastructure
- Infrastructure (repositories) before API endpoints
- API endpoints before tests (though tests should be written first in TDD)
- Core implementation before integration

### Parallel Opportunities

#### Setup Phase (Phase 1)
- T002-T008 (all project initialization) can run in parallel
- T010-T012 (documentation and configuration) can run in parallel

#### Foundational Phase (Phase 2)
- Database: T014-T017 (database configuration) can run in parallel after T013
- Entities: T019-T023 (all domain entities) can run in parallel
- Cache: T026-T029 (cache service) can run in parallel after T025
- Events: T035-T037 (event contracts) can run in parallel
- API: T041-T043, T045-T046 (middleware and configuration) can run in parallel after T040
- Validation: T048-T050 (validation types) can run in parallel after T047

#### User Story 1 (Phase 3)
- T051-T053 (domain constraints) can run in parallel
- T054-T057 (commands and queries) can run in parallel
- T062-T063 (validators) can run in parallel
- T068, T073-T074 (API setup) can run in parallel
- T075-T078 (unit tests) can run in parallel after implementation

#### User Story 2 (Phase 4)
- T084-T089 (calendar entities) can run in parallel
- T093-T099 (commands and queries) can run in parallel
- T120-T121 (unit tests) can run in parallel after implementation

#### User Story 3 (Phase 5)
- T127-T131 (domain entities) can run in parallel
- T134-T136, T142-T145, T151-T155 (commands and queries per feature) can run in parallel
- T163-T165 (repositories) can run in parallel
- T169, T173, T178 (controllers) can run in parallel
- T185-T189 (unit tests) can run in parallel after implementation

#### Polish Phase (Phase 6)
- T210-T214 (documentation) can run in parallel
- T215-T217 (quality checks) can run in parallel

---

## Parallel Example: User Story 1

```bash
# After Foundational phase completes, launch User Story 1 model creation in parallel:
Task T051: "Add unique constraint on SettingValue"
Task T052: "Implement validation rules for SettingValue"

# Launch command/query definitions in parallel:
Task T054: "Create CreateSettingCommand"
Task T055: "Create UpdateSettingCommand"
Task T056: "Create GetResolvedSettingsQuery"
Task T057: "Create GetSettingByIdQuery"

# Launch validators in parallel:
Task T062: "Create CreateSettingValidator"
Task T063: "Create UpdateSettingValidator"

# Launch unit tests in parallel after implementation:
Task T075: "Unit tests for CreateSettingCommandHandler"
Task T076: "Unit tests for UpdateSettingCommandHandler"
Task T077: "Unit tests for GetResolvedSettingsQueryHandler"
Task T078: "Unit tests for hierarchy resolution"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T012)
2. Complete Phase 2: Foundational (T013-T050) - CRITICAL
3. Complete Phase 3: User Story 1 (T051-T083)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Verify success criteria SC-001, SC-002, SC-003 from spec.md
6. Deploy/demo if ready

**Why User Story 1 is MVP**:
- Provides core configuration capability needed by all downstream services
- Establishes hierarchical resolution pattern used throughout the system
- Proves cache-first read performance (<50ms p95)
- Demonstrates event-driven notification to consumers
- Validates tenant isolation and audit trail

### Incremental Delivery

1. **Foundation** (Phase 1 + 2) → Infrastructure ready for any user story
2. **MVP** (Phase 3) → Setting management with hierarchy ✅ Deploy
3. **Calendar Support** (Phase 4) → Academic structures available ✅ Deploy
4. **Extended Configuration** (Phase 5) → Grading, attributes, templates ✅ Deploy
5. **Production Ready** (Phase 6) → Search, monitoring, documentation ✅ Deploy

Each phase adds value without breaking previous functionality.

### Parallel Team Strategy

With multiple developers after Foundational phase:

1. **Team completes Phase 1 + 2 together** (foundation is critical)
2. **Once Foundational is done**:
   - Developer A: User Story 1 (Settings) - P1 priority
   - Developer B: User Story 2 (Calendars) - P2 priority
   - Developer C: User Story 3 (Grading/Templates) - P3 priority
3. Stories complete independently and integrate via events

---

## Notes

- **[P] tasks**: Different files, no dependencies, can run in parallel
- **[Story] label**: Maps task to specific user story for traceability
- **Each user story**: Independently completable and testable
- **Cache strategy**: Write-through on create/update, cache-first on read
- **Event publishing**: All writes must publish change notifications
- **Tenant isolation**: Enforced via interceptors, RLS, and query filters
- **Audit trail**: Every write operation generates immutable audit record
- **Performance target**: Read p95 <50ms (requires cache optimization)
- **Validation**: Prevent overlaps, conflicts, and duplicates before persistence
- **Testing**: Unit tests for logic, integration tests for flows, contract tests for events
- **Commit strategy**: Commit after each task or logical group
- **Stop at any checkpoint**: Validate story independently before proceeding
- **Avoid**: Vague tasks, same-file conflicts, cross-story dependencies that break independence

---

## Success Validation Checklist

After implementation, verify all success criteria from spec.md:

- [ ] SC-001: 95% of configuration read requests return in under 50ms
- [ ] SC-002: 100% of configuration writes generate corresponding audit records
- [ ] SC-003: Cache invalidation propagates within 5 seconds for 99% of writes
- [ ] SC-004: Validation prevents conflicting calendars/grading scales
- [ ] SC-005: Downstream services receive change notifications within 30 seconds

---

**Total Tasks**: 224 (excluding validation tasks V001-V005 and auth tasks A001-A007)
- Phase 1 (Setup): 12 tasks
- Phase 2 (Foundational): 38 tasks
- Phase 3 (User Story 1): 33 tasks
- Phase 4 (User Story 2): 43 tasks
- Phase 5 (User Story 3): 68 tasks
- Phase 6 (Polish): 30 tasks

**Parallel Opportunities**: 89 tasks marked with [P]
**MVP Scope**: Phase 1 + Phase 2 + Phase 3 = 83 tasks
