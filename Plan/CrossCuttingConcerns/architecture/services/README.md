# Service Architecture Specifications

Technical architecture specifications for all NorthStarET microservices. Each specification defines:
- Clean Architecture layers (Domain, Application, Infrastructure, API/UI)
- Technology stack and dependencies
- Data ownership and schema design
- Domain events published/consumed
- API contracts and integration patterns
- Security and multi-tenancy considerations

## Foundation Layer Services

### Phase 1: Core Infrastructure (Weeks 1-6)
- [Identity Service](identity-service.md) - Entra ID integration, session management, RBAC
- [Configuration Service](configuration-service.md) - Tenant/district/school settings, feature flags
- API Gateway - YARP routing, Strangler Fig pattern (see [standards/api-gateway-config.md](../../standards/api-gateway-config.md))

### Phase 2: Core Domain (Weeks 7-14)
- [Student Management Service](student-management-service.md) - Student enrollment, demographics, family contacts
- [Staff Management Service](staff-management-service.md) - Teacher/admin accounts, district assignments
- [Assessment Service](assessment-service.md) - Benchmarks, scoring, progress tracking
- [Assessment Service (Detailed)](assessment-service-detailed.md) - Extended architecture specification

### Phase 3: Secondary Domain (Weeks 15-22)
- [Intervention Management Service](intervention-management-service.md) - RTI tiers, progress monitoring
- [Section & Roster Service](section-roster-service.md) - Class sections, enrollment, scheduling
- [Data Import Service](data-import-service.md) - CSV/Excel import, state test integration

### Phase 4: Supporting Services (Weeks 23-32)
- [Reporting & Analytics Service](reporting-analytics-service.md) - Dashboards, CQRS read models
- [Content & Media Service](content-media-service.md) - File storage, video streaming
- [System Operations Service](system-operations-service.md) - Health monitoring, diagnostics

## DigitalInk Layer Services

### Phase 4a-4d: Digital Ink Capabilities (Weeks 23-32)
- [Digital Ink Service](digital-ink-service.md) - Stylus input capture, audio recording, playback

## Usage

Service architectures are layer-agnostic technical specifications. Implementation plans and scenarios live in layer-specific directories:
- **Foundation implementation**: `Plan/Foundation/scenarios/`
- **DigitalInk implementation**: `Plan/DigitalInk/scenarios/`

## References
- [Bounded Contexts](../bounded-contexts.md) - DDD context map
- [Domain Events Schema](../domain-events-schema.md) - Event standards
- [API Contracts](../../standards/API_CONTRACTS_SPECIFICATION.md) - RESTful patterns
- [Testing Strategy](../../standards/TESTING_STRATEGY.md) - TDD/BDD approach
