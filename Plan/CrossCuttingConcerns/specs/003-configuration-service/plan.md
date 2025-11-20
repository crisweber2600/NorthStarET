# Implementation Plan - Configuration Service

## Goal
Implement the **Configuration Service** to manage multi-tenant settings, hierarchical configuration (System -> District -> School), and core entity definitions (Districts, Schools, Calendars). This service acts as the source of truth for tenant structure and configuration.

## Architecture
- **Service Type**: ASP.NET Core Web API
- **Project Path**: `Src/Foundation/Services/Configuration/`
- **Database**: PostgreSQL (Database-per-service pattern, but logically multi-tenant)
- **Messaging**: MassTransit (RabbitMQ) for domain events (`DistrictCreated`, `SchoolCreated`)
- **Caching**: Redis for high-frequency configuration lookups

## Proposed Solution

### 1. Domain Model
- **District**: Root tenant entity.
- **School**: Child of District.
- **ConfigurationEntry**: Key-Value pair with hierarchy support (System/District/School scope).
- **AcademicCalendar**: Calendar definitions linked to District/School.
- **GradeLevel/Subject**: Metadata definitions.

### 2. API Design
- `POST /districts` - Create new district (triggers provisioning).
- `GET /districts/{id}/settings` - Get effective settings.
- `PUT /districts/{id}/settings` - Update settings.
- `POST /schools` - Create school.
- `GET /calendars` - Get academic calendar.

### 3. Integration
- **Identity Service**: Listens for `DistrictCreated` to provision admin users.
- **Gateway**: Routes `/api/config/*` to this service.
- **Aspire**: Add to AppHost.

## Tasks

### Phase 1: Scaffolding & Domain
- [ ] Scaffold `NorthStarET.Foundation.Services.Configuration` project
- [ ] Add to `AppHost` (Postgres, Redis)
- [ ] Define Domain Entities (`District`, `School`, `Setting`)
- [ ] Implement EF Core `ConfigurationDbContext`

### Phase 2: Core Features
- [ ] Implement District CRUD & Events
- [ ] Implement School CRUD & Events
- [ ] Implement Hierarchical Settings Logic (Override pattern)
- [ ] Implement Academic Calendar Management

### Phase 3: Integration & UI
- [ ] Configure YARP Routes
- [ ] Add UI pages for District/School management (Razor/Blazor)
- [ ] Verify Event flow to Identity Service

## Verification
- **Unit Tests**: Domain logic (hierarchy resolution).
- **Integration Tests**: API endpoints, Database persistence.
- **E2E**: Create District -> Verify Event -> Verify Admin User.
