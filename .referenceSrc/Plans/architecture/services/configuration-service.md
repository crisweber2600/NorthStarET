# Configuration Service

## Overview

The Configuration Service manages system-wide, district-level, and school-level configuration settings for the NorthStar LMS platform. It provides centralized configuration management for multi-tenant operations, academic calendars, custom attributes, and system preferences.

## Service Classification

- **Type**: Foundation Service
- **Phase**: Phase 1 (Weeks 5-8)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Configuration/`
- **Priority**: Critical (required for all domain services)
- **LMS Role**: System and district administration configuration management

## Current State (Legacy)

**Location**: Distributed across multiple controllers and configuration tables  
**Framework**: .NET Framework 4.6  
**Database**: Shared `NorthStar` database (tables: Districts, Schools, Calendars, CustomAttributes, SystemSettings)

**Key Components**:
- Configuration scattered across multiple controllers
- District and school setup in various tables
- Calendar management embedded in domain logic
- Custom attributes defined per district
- System settings in app.config and database

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Configuration.API/              # UI Layer (REST endpoints)
├── Controllers/
│   ├── DistrictsController.cs
│   ├── SchoolsController.cs
│   ├── CalendarsController.cs
│   └── SettingsController.cs
├── Middleware/
└── Program.cs

Configuration.Application/      # Application Layer
├── Commands/
│   ├── CreateDistrict/
│   ├── CreateSchool/
│   ├── UpdateCalendar/
│   └── UpdateSettings/
├── Queries/
│   ├── GetDistrictById/
│   ├── GetSchoolsByDistrict/
│   ├── GetCalendar/
│   └── GetSettings/
├── DTOs/
└── Interfaces/

Configuration.Domain/           # Domain Layer
├── Entities/
│   ├── District.cs
│   ├── School.cs
│   ├── Calendar.cs
│   ├── CalendarDay.cs
│   ├── GradeLevel.cs
│   ├── CustomAttribute.cs
│   └── SystemSetting.cs
├── ValueObjects/
│   ├── SchoolYear.cs
│   ├── DistrictCode.cs
│   └── SettingValue.cs
├── Events/
│   ├── DistrictCreatedEvent.cs
│   ├── SchoolCreatedEvent.cs
│   ├── CalendarUpdatedEvent.cs
│   └── SettingChangedEvent.cs
└── Aggregates/
    ├── DistrictAggregate.cs
    └── SchoolAggregate.cs

Configuration.Infrastructure/   # Infrastructure Layer
├── Data/
│   ├── ConfigurationDbContext.cs
│   └── Repositories/
│       ├── DistrictRepository.cs
│       └── SchoolRepository.cs
├── MessageBus/
└── Caching/
    └── DistributedCache.cs
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Data Access**: EF Core with SQL Server
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Caching**: Redis for frequently accessed settings (district, school, calendars)
- **Orchestration**: .NET Aspire hosting
- **Configuration**: Distributed configuration with cache invalidation

### Owned Data

**Database**: `NorthStar_Configuration_DB`

**Tables**:
- Districts (Id, Name, Code, State, TimeZone, DatabaseConnectionString, IsActive, CreatedDate)
- Schools (Id, DistrictId, Name, Code, PrincipalId, Address, Phone, IsActive)
- GradeLevels (Id, DistrictId, Name, Sequence, IsActive)
- Calendars (Id, DistrictId, SchoolId, SchoolYear, StartDate, EndDate, TotalDays)
- CalendarDays (Id, CalendarId, Date, DayType, Description)
- CustomAttributes (Id, DistrictId, EntityType, AttributeName, DataType, IsRequired)
- SystemSettings (Id, DistrictId, SchoolId, SettingKey, SettingValue, Category, UpdatedBy, UpdatedDate)
- SchoolYears (Id, DistrictId, YearName, StartDate, EndDate, IsCurrentYear)

### Service Boundaries

**Owned Responsibilities**:
- District creation and management (multi-tenant configuration)
- School creation and management within districts
- Academic calendar management (school years, instructional days, holidays)
- Grade level definitions and sequences
- Custom attribute definitions for extensibility
- System settings and preferences (district, school, user-level)
- Database connection string management per district (multi-tenancy)
- Time zone configuration

**Not Owned** (delegated to other services):
- Staff assignments to schools → Staff Management Service
- Student enrollment in schools → Student Management Service
- Section/class creation → Section & Roster Service
- Authentication and authorization → Identity Service

**Cross-Service Coordination**:
- Publish configuration change events for cache invalidation
- Provide centralized settings for all services

### Domain Events Published

**Event Schema Version**: 1.0

- `DistrictCreatedEvent` - When a new district is added
  ```
  - DistrictId: Guid
  - Name: string
  - Code: string
  - State: string
  - Timestamp: DateTime
  ```

- `SchoolCreatedEvent` - When a new school is created
  ```
  - SchoolId: Guid
  - DistrictId: Guid
  - Name: string
  - Code: string
  - Timestamp: DateTime
  ```

- `CalendarUpdatedEvent` - When academic calendar changes
  ```
  - CalendarId: Guid
  - DistrictId: Guid
  - SchoolId: Guid (optional)
  - SchoolYear: string
  - TotalDays: int
  - Timestamp: DateTime
  ```

- `SettingChangedEvent` - When a system setting is modified
  ```
  - SettingKey: string
  - OldValue: string
  - NewValue: string
  - DistrictId: Guid (optional)
  - SchoolId: Guid (optional)
  - Timestamp: DateTime
  ```

- `GradeLevelCreatedEvent` - When a new grade level is defined
  ```
  - GradeLevelId: Guid
  - DistrictId: Guid
  - Name: string
  - Sequence: int
  - Timestamp: DateTime
  ```

### Domain Events Subscribed

- None (foundation service with no dependencies on domain services)

### API Functional Intent

**District Management**:
- Create and configure districts
- Update district settings
- Manage database connection strings (multi-tenancy)
- Deactivate districts

**School Management**:
- Create schools within districts
- Update school information (address, principal, contact)
- Manage school-specific settings
- Deactivate schools

**Calendar Management**:
- Define school years and academic calendars
- Set instructional days, holidays, early dismissals
- Calculate total instructional days
- Support district-wide or school-specific calendars

**Settings Management**:
- Define and update system settings
- Support district-level, school-level, and user-level preferences
- Custom attribute definitions for extensibility
- Setting versioning and audit trail

**Queries** (read-only):
- Get district by ID or code
- Get schools by district
- Get current academic calendar
- Get settings by key and scope
- Get custom attribute definitions

### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime (critical foundation service)
- **Configuration Retrieval**: < 100ms p95 for cached settings
- **Cache Invalidation**: < 2 seconds to propagate setting changes across services
- **Calendar Calculation**: < 50ms p95 for instructional day calculations
- **Consistency Model**: Strongly consistent for configuration writes; eventually consistent for cross-service caching
- **Idempotency Window**: 5 minutes for duplicate configuration updates

### Security & Compliance

**Authorization**:
- **System Admin**: Full configuration management across all districts
- **District Admin**: Configuration management within their district only
- **School Admin**: Limited school-specific settings
- **Staff/Students**: Read-only access to public settings

**Data Protection**:
- Database connection strings encrypted at rest and in transit
- Audit logging for all configuration changes
- Multi-tenant isolation enforced at database and API level
- No cross-district data leakage

**Secrets Management**:
- Database connection strings in Azure Key Vault
- Per-district connection strings managed securely
- No secrets in configuration files or code

### Testing Requirements

**Unit Tests** (Domain & Application layers):
- District and school validation logic
- Calendar calculations (instructional days, date ranges)
- Setting validation and versioning
- Business rule enforcement

**Reqnroll BDD Features**:
- `district-creation.feature` - Creating new districts with multi-tenancy
- `school-management.feature` - Managing schools within districts
- `calendar-setup.feature` - Defining academic calendars
- `setting-management.feature` - Managing system settings with cache invalidation

**Integration Tests** (Aspire):
- Event publishing to Azure Service Bus
- Database persistence via EF Core
- Redis caching and invalidation
- Multi-tenant database routing

**Playwright UI Tests**:
- District setup wizard (Figma: district-setup-flow)
- School configuration (Figma: school-admin-config)
- Calendar builder (Figma: calendar-management)

**Test Coverage Target**: ≥ 80% for Domain and Application layers

### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 1, Week 5-6**: Foundation
   - Set up Configuration.API with Clean Architecture structure
   - Configure .NET Aspire orchestration
   - Implement EF Core DbContext with migration scripts
   - Set up Redis caching infrastructure

2. **Phase 1, Week 7**: Core Features
   - Migrate district and school management
   - Implement calendar management
   - Set up event publishing for configuration changes
   - API Gateway routes new endpoints

3. **Phase 1, Week 8**: Cutover
   - Validate data consistency between legacy and new DB
   - Configure cache invalidation across services
   - Switch API Gateway to route configuration requests to new service
   - Monitor performance and error rates

**Data Migration**:
- All district, school, and calendar data migrated via ETL scripts
- System settings migrated and validated
- Custom attributes preserved
- Rollback plan: revert API Gateway routing

**Legacy-New Interoperability**:
- Both legacy and new services can query configuration during transition
- Events published to notify all services of configuration changes
- Gradual migration with zero downtime

### Dependencies

**Upstream Services** (this service depends on):
- Identity Service (authentication tokens only)

**Downstream Services** (services that depend on this):
- ALL microservices (require district, school, and configuration data)
- Student Management Service (school assignments)
- Staff Management Service (school assignments)
- Section & Roster Service (grade levels, schools)
- Assessment Service (district/school context)

### Implementation Checklist

**Phase 1, Weeks 5-8**:

- [ ] Set up project structure with Clean Architecture
  - [ ] Configuration.API
  - [ ] Configuration.Application
  - [ ] Configuration.Domain
  - [ ] Configuration.Infrastructure
  - [ ] Configuration.Tests (unit, integration, BDD)

- [ ] Configure .NET Aspire
  - [ ] Add service to AppHost
  - [ ] Configure Aspire Service Defaults
  - [ ] Set up distributed tracing

- [ ] Implement Domain Layer
  - [ ] District and School aggregates
  - [ ] Calendar calculation logic
  - [ ] Value objects (SchoolYear, DistrictCode)
  - [ ] Domain events
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
  - [ ] Redis caching with invalidation

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
  - [ ] Validate multi-tenant data isolation
  - [ ] Test connection string encryption

- [ ] Deployment
  - [ ] Configure Docker container
  - [ ] Set up Kubernetes manifests
  - [ ] Configure API Gateway routing
  - [ ] Deploy to staging environment
  - [ ] Execute smoke tests

- [ ] Production Cutover
  - [ ] Monitor performance and errors
  - [ ] Switch API Gateway to new service
  - [ ] Deprecate legacy configuration logic
  - [ ] Post-migration validation

### Monitoring & Observability

**Application Insights**:
- Track configuration read/write rates
- Monitor cache hit ratios
- Alert on failed cache invalidations

**Custom Metrics**:
- Districts created per month
- Schools created per month
- Configuration reads per second
- Cache hit/miss ratio

**Distributed Tracing**:
- OpenTelemetry for request tracing
- Trace configuration retrieval across services

**Logging**:
- Structured logging to Seq
- Log all configuration changes with audit trail
- Error logging for cache invalidation failures

### Open Questions / Risks

1. **Multi-Tenancy Complexity**: Database-per-district model requires robust connection string management and routing.
2. **Cache Invalidation**: Distributed caching across multiple services needs reliable invalidation mechanism.
3. **Configuration Versioning**: Need strategy for rolling back configuration changes if issues arise.
4. **Calendar Complexity**: Different districts have varying calendar structures (year-round vs. traditional). Need flexible model.

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete - Ready for Implementation
