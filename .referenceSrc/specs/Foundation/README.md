# Foundation Layer

This layer contains all foundational services and features for the NorthStar LMS migration project.

## Purpose

The Foundation layer establishes the core infrastructure and services that all other layers depend on, including:

- **Authentication & Authorization** - Identity services, SSO, role-based access control
- **Configuration Management** - District, school, and system settings
- **Core Domain Services** - Student, staff, assessment, intervention management
- **Integration Services** - Data import, section/roster management
- **Supporting Services** - Reporting, analytics, content/media management
- **Homeschool Features** - Specialized homeschool functionality

## Features

This layer contains 19 numbered feature specifications (001-019):

### Phase 1: Foundation Services (001-005)
- `001-phase1-foundation-services` - Identity, API Gateway, Configuration
- `002-identity-authentication` - OAuth 2.0 authentication
- `003-student-enrollment` - Student enrollment workflows
- `004-configuration-service` - District/school configuration
- `005-api-gateway` - YARP API Gateway

### Phase 2: Core Domain Services (006-008)
- `006-assessment-service` - Assessment management
- `007-staff-management-service` - Staff profiles and teams
- `008-intervention-management-service` - Student interventions

### Phase 3: Secondary Domain (009-012)
- `009-data-import-service` - Data import and ETL
- `010-section-roster-service` - Class sections and rosters
- `011-reporting-analytics-service` - Reports and dashboards
- `012-content-media-service` - File storage and media

### Homeschool Features (013-019)
- `013-homeschool-parent-registration` - Homeschool parent onboarding
- `014-homeschool-student-enrollment` - Homeschool student enrollment
- `015-homeschool-annual-report` - Homeschool annual reports
- `016-homeschool-compliance-dashboard` - Homeschool compliance tracking
- `017-homeschool-activity-logging` - Homeschool activity logs
- `018-homeschool-multigrade` - Homeschool multi-grade support
- `019-homeschool-coop-roster` - Homeschool co-op rosters

## Architecture Principles

All features in this layer follow:

- **Clean Architecture** boundaries (UI → Application → Domain ← Infrastructure)
- **Test-Driven Development** with Red → Green workflow (≥80% coverage)
- **Multi-tenant isolation** using PostgreSQL Row-Level Security
- **Event-driven integration** via domain events
- **.NET Aspire orchestration** for all services

## Standard Feature Structure

Each feature directory contains:

```
###-feature-name/
├── spec.md              # Feature specification (requirements, user stories)
├── plan.md              # Implementation plan (technical approach)
├── data-model.md        # Domain entities and relationships
├── quickstart.md        # Setup guide
├── research.md          # Phase 0 research
├── tasks.md             # Phase 2 task breakdown
├── contracts/           # API contracts and event schemas
├── checklists/          # Quality checklists
└── features/            # BDD feature files (Reqnroll)
```

## Related Documentation

- [Specs Overview](../README.md) - Layered architecture documentation
- [Plans](../../Plans/) - High-level migration planning
- [Master Migration Plan](../../Plans/MASTER_MIGRATION_PLAN.md) - Complete roadmap
- [Constitution](../../Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md) - Governing principles

---

**Layer Status**: Active Development  
**Features**: 19 specifications  
**Phase Coverage**: Phases 1-4 of migration
