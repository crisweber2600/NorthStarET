# NorthStar Modernization Migration Plan

**Version**: 1.1  
**Date**: 2025-11-15  
**Status**: Planning - See [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) for complete integration

> **Note**: This document has been superseded by the comprehensive [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) which integrates this plan with all other migration documents, specifications, and architectural guidelines. This document is retained for historical reference.

---

## Executive Summary

This document outlines the comprehensive plan to migrate the legacy OldNorthStar application (.NET Framework 4.6) to a modern microservices architecture (UpgradedNorthStar) based on .NET 10, incorporating the work-in-progress implementation from WIPNorthStar, and adhering to the architectural principles defined in constitution.md.

**For the complete, integrated plan including all service specifications, feature specs, and implementation guides, see [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md).**

## Current State Analysis

### OldNorthStar (Legacy System)
- **Technology**: .NET Framework 4.6, AngularJS
- **Architecture**: Monolithic web application
- **Components**:
  - NS4.WebAPI - Main Web API with 35+ controllers
  - NS4.Angular - AngularJS frontend
  - NorthStar.Core - Core business logic
  - NorthStar.EF6 - Entity Framework 6 data access
  - EntityDto - 383 entity and DTO files
  - IdentityServer - Authentication/authorization
  - NorthStar.BatchProcessor - Background job processing
  - NorthStar4.BatchPrint - Print job processing
  - NorthStar.AutomatedRollover - Automated data rollover
- **Total**: ~729 C# files

### WIPNorthStar (Modern Implementation)
- **Technology**: .NET 10, Aspire orchestration, Clean Architecture
- **Status**: Partially implemented (3 features in progress)
- **Components**:
  - NorthStarET.NextGen.Lms.Domain - Domain entities and events
  - NorthStarET.NextGen.Lms.Application - CQRS handlers and services
  - NorthStarET.NextGen.Lms.Infrastructure - Data access, external services
  - NorthStarET.NextGen.Lms.Api - REST API endpoints
  - NorthStarET.NextGen.Lms.Web - Razor Pages UI
  - NorthStarET.NextGen.Lms.AppHost - Aspire orchestration
  - Test projects (unit, integration, BDD, UI)
- **Total**: ~239 C# files
- **Features Implemented**:
  - Feature 001: Unified SSO & Authorization (partially complete)
  - Feature 002: Bootstrap Tenant Access (partially complete)
  - Feature 004: Manage Schools & Grades (partially complete)
- **Build Status**: ✓ Successful (12 warnings, 0 errors)

### UpgradedNorthStar (Target System)
- **Status**: To be created
- **Purpose**: Final integrated modern microservices system

## Architectural Principles (from constitution.md)

1. **Clean Architecture & Aspire Orchestration**: UI → Application → Domain → Infrastructure
2. **Test-Driven Quality Gates**: TDD, BDD (Reqnroll), Integration (Aspire), UI (Playwright)
3. **UX Traceability & Figma Accountability**: Design-first approach
4. **Event-Driven Data Discipline**: Asynchronous, idempotent operations
5. **Security & Compliance Safeguards**: Least privilege, secrets management
6. **Tool-Assisted Development Workflow**: Documentation-first, structured thinking

## Proposed Microservices Architecture

Based on analysis of OldNorthStar controllers and entities, the following microservices are recommended:

### 1. Identity & Access Service
**Purpose**: Authentication, authorization, session management  
**OldNorthStar Components**: AuthController, PasswordResetController, IdentityServer  
**WIPNorthStar Integration**: Feature 001 (SSO & Authorization)  
**Domain Entities**: User, Role, Permission, Session, Tenant

### 2. Student Information Service
**Purpose**: Student profiles, enrollment, demographics  
**OldNorthStar Components**: StudentController, StudentDashboardController  
**WIPNorthStar Integration**: Foundation from Feature 002  
**Domain Entities**: Student, Enrollment, Guardian, Demographics

### 3. Assessment & Evaluation Service
**Purpose**: Assessments, test results, benchmarks, grading  
**OldNorthStar Components**: AssessmentController, AssessmentAvailabilityController, BenchmarkController  
**WIPNorthStar Integration**: New implementation needed  
**Domain Entities**: Assessment, AssessmentField, AssessmentResult, Benchmark

### 4. Roster & Section Service
**Purpose**: Class sections, rosters, teacher assignments  
**OldNorthStar Components**: SectionController, SectionDataEntryController, SectionReportController, RosterRolloverController  
**WIPNorthStar Integration**: New implementation needed  
**Domain Entities**: Section, Roster, TeacherAssignment, ClassPeriod

### 5. Staff Management Service
**Purpose**: Staff profiles, assignments, district associations  
**OldNorthStar Components**: StaffController  
**WIPNorthStar Integration**: Foundation from Feature 002  
**Domain Entities**: Staff, StaffDistrict, StaffRole, Assignment

### 6. Intervention & Support Service
**Purpose**: Interventions, support programs, toolkits  
**OldNorthStar Components**: InterventionGroupController, InterventionDashboardController, InterventonToolkitController  
**WIPNorthStar Integration**: New implementation needed  
**Domain Entities**: Intervention, InterventionGroup, InterventionTool, InterventionFramework

### 7. District & School Administration Service
**Purpose**: District settings, school management, organizational hierarchy  
**OldNorthStar Components**: DistrictSettingsController, NavigationController  
**WIPNorthStar Integration**: Feature 002 (Bootstrap Tenant Access), Feature 004 (Manage Schools & Grades)  
**Domain Entities**: District, School, DistrictSettings, Calendar

### 8. Reporting & Analytics Service
**Purpose**: Reports, dashboards, data visualization, exports  
**OldNorthStar Components**: LineGraphController, StackedBarGraphController, ExportDataController, PrintController  
**WIPNorthStar Integration**: New implementation needed  
**Domain Entities**: Report, Dashboard, ExportRequest, Chart

### 9. Data Import & Integration Service
**Purpose**: Batch imports, state test data, file uploads  
**OldNorthStar Components**: ImportStateTestDataController, FileUploaderController, DataEntryController  
**WIPNorthStar Integration**: New implementation needed  
**Domain Entities**: ImportJob, DataSource, FileUpload, ValidationResult

### 10. Content & Media Service
**Purpose**: Videos, presentations, help content  
**OldNorthStar Components**: VideoController, HelpController  
**WIPNorthStar Integration**: New implementation needed  
**Domain Entities**: Video, Presentation, HelpContent, MediaAsset

### 11. Personal Settings & Preferences Service
**Purpose**: User preferences, personal settings, calendar  
**OldNorthStar Components**: PersonalSettingsController, CalendarController  
**WIPNorthStar Integration**: New implementation needed  
**Domain Entities**: UserPreference, PersonalSetting, UserCalendar

### 12. Background Jobs & Processing Service
**Purpose**: Batch processing, scheduled tasks, automated workflows  
**OldNorthStar Components**: NorthStar.BatchProcessor, NorthStar4.BatchPrint, NorthStar.AutomatedRollover  
**WIPNorthStar Integration**: New implementation needed  
**Domain Entities**: Job, Schedule, BatchProcess, Workflow

## Integration Strategy

### Phase 1: Foundation Setup (Week 1-2)
1. Create UpgradedNorthStar folder structure
2. Set up Aspire orchestration project
3. Create shared infrastructure:
   - Service Defaults project
   - Contracts project (shared DTOs)
   - Common domain project (shared value objects, base entities)
4. Set up centralized configuration management
5. Establish CI/CD pipeline structure

### Phase 2: Migrate WIPNorthStar Components (Week 3-4)
1. Copy and adapt existing WIPNorthStar projects into UpgradedNorthStar:
   - Identity & Access Service (Feature 001)
   - District & School Administration Service (Features 002, 004)
2. Update namespaces and project references
3. Ensure all tests pass in new location
4. Complete partially implemented features
5. Update Aspire orchestration to include migrated services

### Phase 3: Core Services Implementation (Week 5-8)
Priority order based on dependencies:
1. **Student Information Service** (depends on Identity, District/School)
2. **Staff Management Service** (depends on Identity, District/School)
3. **Roster & Section Service** (depends on Student, Staff, District/School)
4. **Assessment & Evaluation Service** (depends on Student, Section)

For each service:
- Create Clean Architecture project structure
- Define domain entities and events
- Implement CQRS commands and queries
- Build API controllers
- Write comprehensive tests (unit, integration, BDD)
- Integrate with Aspire orchestration

### Phase 4: Advanced Services Implementation (Week 9-12)
1. **Intervention & Support Service**
2. **Reporting & Analytics Service**
3. **Data Import & Integration Service**
4. **Content & Media Service**

### Phase 5: Supporting Services (Week 13-14)
1. **Personal Settings & Preferences Service**
2. **Background Jobs & Processing Service**

### Phase 6: UI Migration (Week 15-18)
1. Create modern Blazor/Razor Pages UI
2. Migrate AngularJS components to modern framework
3. Implement Figma-driven design system
4. Create Playwright UI tests

### Phase 7: Integration & Testing (Week 19-20)
1. End-to-end integration testing
2. Performance testing and optimization
3. Security audit
4. Load testing
5. Data migration scripts and validation

### Phase 8: Deployment & Cutover (Week 21-22)
1. Production environment setup
2. Data migration execution
3. Parallel run period
4. Final cutover
5. Decommission OldNorthStar

## Data Migration Strategy

### Database Schema Evolution
1. **Analyze OldNorthStar schema**: Map existing tables to new domain entities
2. **Design new schema**: Clean Architecture-aligned, normalized, event-sourced where appropriate
3. **Create migration scripts**: ETL processes using EF Core migrations
4. **Validation strategy**: Data integrity checks, reconciliation reports

### Migration Approach
- **Incremental**: Migrate service by service, not big-bang
- **Dual-write period**: Write to both old and new systems during transition
- **Reconciliation**: Automated checks to ensure data consistency
- **Rollback plan**: Ability to revert if issues discovered

## Quality Assurance Strategy

### Test Coverage Requirements (per constitution.md)
- ≥80% code coverage across all services
- Unit tests for all domain logic and application services
- Integration tests (Aspire) for all cross-service interactions
- BDD tests (Reqnroll) for all user stories
- UI tests (Playwright) for all user workflows

### Test Execution Gates
1. **Red Phase**: Document failing tests before implementation
2. **Green Phase**: Implement and verify all tests pass
3. **Refactor Phase**: Optimize while maintaining test coverage
4. **CI/CD Gates**: No merge without passing all test suites

## Risk Management

### High Risks
1. **Data Migration Complexity**: 383 entities to migrate
   - *Mitigation*: Incremental migration, extensive validation
2. **Feature Parity**: Ensuring all OldNorthStar functionality preserved
   - *Mitigation*: Comprehensive feature mapping, acceptance criteria
3. **Performance**: Meeting <50ms authorization requirements
   - *Mitigation*: Early performance testing, caching strategy
4. **Integration**: WIPNorthStar components may need refactoring
   - *Mitigation*: Thorough code review, alignment with constitution.md

### Medium Risks
1. **Learning Curve**: Team adoption of Clean Architecture, CQRS, Aspire
   - *Mitigation*: Training, documentation, pair programming
2. **Timeline**: 22-week estimate is aggressive
   - *Mitigation*: Buffer time, MVP approach, prioritization

## Success Criteria

1. All OldNorthStar functionality available in UpgradedNorthStar
2. All services follow Clean Architecture principles
3. ≥80% test coverage across all layers
4. Authorization decisions <50ms (P95)
5. All services orchestrated through Aspire
6. Zero direct UI → Infrastructure coupling
7. All secrets in platform secret store
8. Comprehensive documentation and ADRs
9. Successful production deployment
10. User acceptance testing passed

## Deliverables

1. UpgradedNorthStar solution with 12 microservices
2. Comprehensive test suite (unit, integration, BDD, UI)
3. Aspire orchestration configuration
4. Data migration scripts and validation tools
5. API documentation (OpenAPI/Swagger)
6. Architecture Decision Records (ADRs)
7. Deployment guides and runbooks
8. User documentation
9. Training materials

## Next Steps

1. **Review Master Plan**: See [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) for complete integrated approach
2. **Immediate**: Create UpgradedNorthStar folder structure
3. **Week 1**: Set up foundation projects and Aspire orchestration
4. **Week 2**: Complete infrastructure setup and CI/CD
5. **Week 3**: Begin WIPNorthStar component migration

## Additional Resources

**Comprehensive Planning**:
- [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) - Complete integrated plan (v3.0) ⭐
- [INTEGRATED_MIGRATION_PLAN.md](./INTEGRATED_MIGRATION_PLAN.md) - Detailed v2.1 plan
- [microservices/SERVICE_CATALOG.md](./microservices/SERVICE_CATALOG.md) - All 11 service specifications
- [../specs/](./../specs/) - Feature specifications
- [docs/DATA_MIGRATION_SPECIFICATION.md](./docs/DATA_MIGRATION_SPECIFICATION.md) - Data migration
- [Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md](./Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md) - Constitution v1.6.0

## Appendices

### A. OldNorthStar Controller Inventory
- 35 controllers identified
- Full mapping to microservices in section above

### B. WIPNorthStar Feature Status
- Feature 001: Unified SSO & Authorization - 70% complete
- Feature 002: Bootstrap Tenant Access - 60% complete
- Feature 004: Manage Schools & Grades - 50% complete

### C. Technology Stack
- .NET 10.0
- ASP.NET Core
- Entity Framework Core
- .NET Aspire
- MediatR (CQRS)
- xUnit (unit tests)
- Reqnroll (BDD tests)
- Playwright (UI tests)
- PostgreSQL or SQL Server (TBD)
- Azure (hosting platform)

### D. References
- [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) - Complete integrated migration plan ⭐
- Constitution.md (v1.6.0) - [Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md](./Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md)
- [AGENTS.md](./Src/WIPNorthStar/NorthStarET.Lms/AGENTS.md)
- [microservices/SERVICE_CATALOG.md](./microservices/SERVICE_CATALOG.md) - Service documentation
- WIPNorthStar implementation status documents
