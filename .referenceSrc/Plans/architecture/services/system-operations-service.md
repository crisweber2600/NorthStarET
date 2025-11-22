# System Operations Service

## Overview

The System Operations Service provides health monitoring, diagnostics, system navigation, and operational support for the NorthStar LMS platform.

## Service Classification

- **Type**: Supporting Service
- **Phase**: Phase 4 (Weeks 25-30)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Operations/`
- **Priority**: Low (operational support)
- **LMS Role**: System health, diagnostics, and navigation menu configuration

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` (NavigationController, ProbeController)  
**Framework**: .NET Framework 4.6  
**Database**: Shared `NorthStar` database

**Key Components**:
- Navigation menu configuration
- Health probe endpoints
- System diagnostics
- Operational utilities

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Operations.API/                 # UI Layer (health, diagnostics endpoints)
Operations.Application/         # Application Layer (monitoring, navigation config)
Operations.Domain/              # Domain Layer (Navigation, HealthCheck aggregates)
Operations.Infrastructure/      # Infrastructure Layer (telemetry, EF Core)
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Health Checks**: ASP.NET Core Health Checks
- **Monitoring**: Application Insights, Prometheus metrics
- **Data Access**: EF Core with SQL Server
- **Orchestration**: .NET Aspire hosting
- **Tracing**: OpenTelemetry

### Owned Data

**Database**: `NorthStar_Operations_DB`

**Tables**:
- NavigationMenus (Id, DistrictId, RoleType, MenuStructure JSON, Version, UpdatedBy, UpdatedDate)
- HealthCheckHistory (Id, ServiceName, Status, ResponseTime, Timestamp, Details)
- SystemEvents (Id, EventType, Severity, Message, Source, Timestamp)

### Service Boundaries

**Owned Responsibilities**:
- Health check aggregation across services
- System diagnostics and troubleshooting
- Navigation menu configuration per role
- Operational event logging
- Service discovery information

**Not Owned**:
- Service-specific health → Individual services
- Business logic → Domain services

### API Functional Intent

**Health Monitoring**:
- Aggregate health checks from all services
- Health check dashboard
- Service availability status

**Navigation**:
- Configure navigation menus by role
- Retrieve navigation for authenticated users
- Version navigation configurations

**Diagnostics**:
- System diagnostics endpoints
- Service dependency graph
- Configuration validation

### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime (critical for monitoring)
- **Health Check**: < 100ms p95
- **Navigation Retrieval**: < 50ms p95 (cached)

### Security & Compliance

**Authorization**:
- **System Admin**: Full access to diagnostics
- **District Admin**: Limited health view
- **All Users**: Navigation menu retrieval

### Testing Requirements

**Reqnroll BDD Features**:
- `health-monitoring.feature`
- `navigation-configuration.feature`
- `diagnostics.feature`

### Migration Strategy

**Phase 4, Weeks 25-30**:
1. Health check setup (Weeks 25-26)
2. Navigation configuration (Weeks 27-28)
3. Diagnostics and monitoring (Weeks 29-30)

### Dependencies

**Upstream**: All services (health checks)  
**Downstream**: None

### Implementation Checklist

- [ ] Health check aggregation
- [ ] ASP.NET Core Health Checks UI
- [ ] Navigation configuration API
- [ ] Application Insights integration
- [ ] OpenTelemetry setup
- [ ] Prometheus metrics export

### Open Questions / Risks

1. **Health Check Aggregation**: Polling all services may introduce latency. Consider push-based approach.

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete - Ready for Implementation
