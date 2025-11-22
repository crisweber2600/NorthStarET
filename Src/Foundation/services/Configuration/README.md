# Configuration Service

**Phase**: 1 (Weeks 1-8)  
**Priority**: Critical (Blocking - Required by all services for tenant/school settings)  
**Status**: To Be Implemented

---

## Overview

The Configuration Service provides centralized configuration management for all Foundation services:

- **Tenant (District) Configuration**: District settings, subscriptions, feature flags
- **School Configuration**: School profiles, calendars, grade levels
- **System Settings**: Application-wide configuration
- **Feature Flags**: Enable/disable features per tenant or globally

**Bounded Context**: Configuration  
**Database**: `Configuration_DB` (PostgreSQL with multi-tenancy)

---

## Responsibilities

1. **Tenant Management**
   - CRUD operations for districts (tenants)
   - Tenant activation/suspension
   - Subscription tiers and limits
   - Tenant-specific feature flags

2. **School Management**
   - CRUD operations for schools within tenants
   - School profiles (name, address, principal, etc.)
   - School calendars (start/end dates, holidays)
   - Grade level configuration

3. **System Settings**
   - Application-wide configuration key-value pairs
   - Email templates
   - Notification preferences
   - Integration settings (APIs, webhooks)

4. **Feature Flags**
   - Global feature flags (enable/disable features for all)
   - Tenant-level feature flags (enable/disable per district)
   - School-level feature flags (enable/disable per school)
   - User-level feature flags (enable/disable for specific users)

---

## API Endpoints

### Tenants

- `GET /api/configuration/tenants` - List all tenants (admin only)
- `GET /api/configuration/tenants/{id}` - Get tenant by ID
- `POST /api/configuration/tenants` - Create tenant (admin only)
- `PUT /api/configuration/tenants/{id}` - Update tenant
- `DELETE /api/configuration/tenants/{id}` - Deactivate tenant (soft delete)

### Schools

- `GET /api/configuration/schools` - List schools (filtered by tenant)
- `GET /api/configuration/schools/{id}` - Get school by ID
- `POST /api/configuration/schools` - Create school
- `PUT /api/configuration/schools/{id}` - Update school
- `DELETE /api/configuration/schools/{id}` - Deactivate school (soft delete)

### Grade Levels

- `GET /api/configuration/schools/{schoolId}/grade-levels` - List grade levels for school
- `POST /api/configuration/schools/{schoolId}/grade-levels` - Add grade level to school
- `DELETE /api/configuration/schools/{schoolId}/grade-levels/{id}` - Remove grade level

### System Settings

- `GET /api/configuration/settings` - Get all system settings
- `GET /api/configuration/settings/{key}` - Get setting by key
- `PUT /api/configuration/settings/{key}` - Update setting value (admin only)

### Feature Flags

- `GET /api/configuration/features` - List all feature flags
- `GET /api/configuration/features/{featureName}` - Check if feature enabled (tenant-aware)
- `PUT /api/configuration/features/{featureName}` - Update feature flag (admin only)

---

## Domain Model

### Entities

- **Tenant**: District entity (Id, Name, Status, SubscriptionTier, CreatedAt, etc.)
- **School**: School entity (Id, TenantId, Name, Address, Principal, etc.)
- **GradeLevel**: Grade level enum (PreK, K, 1-12)
- **SchoolGradeLevel**: Many-to-many relationship (SchoolId, GradeLevel)
- **SystemSetting**: Key-value configuration (Key, Value, Description)
- **FeatureFlag**: Feature toggle (Name, Enabled, TenantId nullable, SchoolId nullable)

### Value Objects

- `TenantId` - Tenant/district identifier
- `Address` - School address (street, city, state, zip)
- `SchoolCalendar` - School year dates (start, end, holidays)

### Domain Events

- `TenantCreatedEvent` - New tenant created
- `TenantActivatedEvent` - Tenant activated
- `TenantSuspendedEvent` - Tenant suspended
- `SchoolCreatedEvent` - New school created
- `FeatureFlagChangedEvent` - Feature flag enabled/disabled

---

## Technology Stack

- **ASP.NET Core Web API** (.NET 10)
- **Entity Framework Core 10** (PostgreSQL)
- **MediatR** (CQRS)
- **FluentValidation** (Input validation)
- **Redis** (Feature flag caching)

---

## Legacy Components to Migrate

From `OldNorthStar`:

- `NS4.WebAPI/Controllers/DistrictSettingsController.cs` - District configuration
- `NS4.WebAPI/Controllers/NavigationController.cs` - Navigation/menu settings
- `NorthStar.EF6/DistrictSettingsDataService.cs` - Configuration data access
- `EntityDto/Entity/District.cs`, `School.cs` - District/school entities

---

## Dependencies

### Phase 1 Dependencies

- **PostgreSQL**: Configuration_DB database
- **Redis**: Feature flag caching, settings caching
- **Azure Service Bus**: Domain event publishing

### Services Depending on Configuration

- **ALL Services**: Every Foundation service reads tenant/school configuration

---

## Clean Architecture Structure

```
Configuration/
├── Configuration.Domain/             # Domain entities, events, interfaces
│   ├── Entities/
│   │   ├── Tenant.cs
│   │   ├── School.cs
│   │   ├── SchoolGradeLevel.cs
│   │   ├── SystemSetting.cs
│   │   └── FeatureFlag.cs
│   ├── ValueObjects/
│   │   ├── Address.cs
│   │   └── SchoolCalendar.cs
│   ├── Events/
│   │   ├── TenantCreatedEvent.cs
│   │   └── SchoolCreatedEvent.cs
│   └── Interfaces/
│       ├── ITenantRepository.cs
│       └── ISchoolRepository.cs
│
├── Configuration.Application/        # CQRS handlers, DTOs, validators
│   ├── Commands/
│   │   ├── CreateTenant/
│   │   │   ├── CreateTenantCommand.cs
│   │   │   ├── CreateTenantCommandHandler.cs
│   │   │   └── CreateTenantValidator.cs
│   │   └── CreateSchool/
│   │       ├── CreateSchoolCommand.cs
│   │       └── CreateSchoolCommandHandler.cs
│   ├── Queries/
│   │   ├── GetTenant/
│   │   │   ├── GetTenantQuery.cs
│   │   │   └── GetTenantQueryHandler.cs
│   │   └── ListSchools/
│   │       ├── ListSchoolsQuery.cs
│   │       └── ListSchoolsQueryHandler.cs
│   └── DTOs/
│       ├── TenantDto.cs
│       ├── SchoolDto.cs
│       └── FeatureFlagDto.cs
│
├── Configuration.Infrastructure/     # EF Core, repositories, caching
│   ├── Persistence/
│   │   ├── ConfigurationDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── TenantConfiguration.cs
│   │   │   └── SchoolConfiguration.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── TenantRepository.cs
│   │   └── SchoolRepository.cs
│   ├── Caching/
│   │   └── FeatureFlagCacheService.cs
│   └── DependencyInjection.cs
│
├── Configuration.Api/                # REST API endpoints
│   ├── Program.cs
│   ├── Controllers/
│   │   ├── TenantsController.cs
│   │   ├── SchoolsController.cs
│   │   ├── SettingsController.cs
│   │   └── FeaturesController.cs
│   └── appsettings.json
│
└── Configuration.Tests/              # Tests
    ├── Domain.Tests/
    ├── Application.Tests/
    ├── Infrastructure.Tests/
    └── Integration.Tests/
```

---

## Multi-Tenancy Considerations

### Tenant Table (No tenant_id)

The `Tenants` table itself does NOT have a `tenant_id` column (it defines tenants). All other tables (`Schools`, `SchoolGradeLevels`, etc.) have `tenant_id` foreign key.

### Cross-Tenant Queries

- **SuperAdmin**: Can query all tenants
- **DistrictAdmin**: Can only query their own tenant
- **SchoolAdmin**: Can only query schools within their tenant

Authorization logic in Application layer enforces these rules.

---

## Feature Flag Strategy

### Hierarchical Feature Flags

1. **Global**: Applies to all tenants (`TenantId = null`, `SchoolId = null`)
2. **Tenant-Level**: Overrides global for specific tenant
3. **School-Level**: Overrides tenant-level for specific school
4. **User-Level**: Overrides school-level for specific user (future)

### Caching Strategy

- Feature flags cached in Redis with 5-minute TTL
- Cache invalidation on feature flag update
- Fast checks: `await _featureService.IsEnabledAsync("new-assessment-ui")`

---

## Configuration

```json
{
  "ConnectionStrings": {
    "ConfigurationDb": "Host=postgres;Database=Configuration_DB;Username=postgres;Password=postgres"
  },
  "Redis": {
    "ConnectionString": "redis:6379"
  }
}
```

---

## Testing Requirements

### Unit Tests (≥80% coverage)

- Domain entities and value objects
- Command/query handlers
- Validators

### Integration Tests (Aspire)

- CRUD operations for tenants/schools
- Feature flag resolution (hierarchical)
- Multi-tenancy isolation (no cross-tenant data leakage)

### BDD Tests (Reqnroll)

Feature files in `specs/004-configuration-service/features/`:
- `tenant-management.feature`
- `school-management.feature`
- `feature-flags.feature`

---

## References

- **Specification**: [004-configuration-service](../../../Plan/Foundation/specs/Foundation/004-configuration-service/)
- **Architecture**: [configuration-service.md](../../../../Plan/CrossCuttingConcerns/architecture/services/configuration-service.md)
- **Scenario**: [07-configuration-service.md](../../../Plan/Foundation/Plans/scenarios/07-configuration-service.md)

---

**Status**: Specification Complete, Implementation Pending  
**Start Date**: TBD (Phase 1, Week 3)  
**Completion Target**: Week 6
