# NorthStar LMS Feature Specifications

This directory contains detailed feature specifications following a **layered, feature-based architecture** aligned with the project's constitution and Clean Architecture principles.

## Purpose

The **specs** directory organizes all feature-level specifications using an **explicit layered approach** that supports:

- **Clean Architecture boundaries** (UI → Application → Domain ← Infrastructure)
- **Test-Driven Development** (TDD with Red → Green workflow)
- **Multi-tenant data isolation** with PostgreSQL Row-Level Security
- **Event-driven integration** via domain events
- **Traceability** from requirements through implementation

Features are organized into explicit layers that reflect the system's architectural structure, making dependencies and boundaries clear.

## Directory Structure (Explicit Layers)

```
specs/
├── README.md                                    # This file
│
├── Foundation/                                  # Foundation layer (core services)
│   ├── README.md                               # Foundation layer documentation
│   ├── 001-phase1-foundation-services/         # Phase 1 foundation
│   ├── 002-identity-authentication/            # OAuth 2.0 authentication
│   ├── 003-student-enrollment/                 # Student enrollment
│   ├── 004-configuration-service/              # Configuration management
│   ├── 005-api-gateway/                        # API Gateway (YARP)
│   ├── 006-assessment-service/                 # Assessment management
│   ├── 007-staff-management-service/           # Staff management
│   ├── 008-intervention-management-service/    # Interventions
│   ├── 009-data-import-service/                # Data import/ETL
│   ├── 010-section-roster-service/             # Sections & rosters
│   ├── 011-reporting-analytics-service/        # Reporting & analytics
│   ├── 012-content-media-service/              # Content & media
│   ├── 013-homeschool-parent-registration/     # Homeschool features
│   ├── 014-homeschool-student-enrollment/
│   ├── 015-homeschool-annual-report/
│   ├── 016-homeschool-compliance-dashboard/
│   ├── 017-homeschool-activity-logging/
│   ├── 018-homeschool-multigrade/
│   └── 019-homeschool-coop-roster/
│
└── DigitalInk/                                  # Digital Ink layer (specialized)
    ├── README.md                               # Digital Ink layer documentation
    └── 020-digital-ink/                        # Digital ink service

## Feature Structure

Each feature directory (###-feature-name/) contains:

├── spec.md                                  # Feature specification (requirements, user stories)
├── plan.md                                  # Implementation plan (technical approach)
├── data-model.md                            # Domain entities and relationships
├── quickstart.md                            # Setup and getting started guide
├── research.md                              # Phase 0 research and decisions
├── tasks.md                                 # Phase 2 task breakdown
├── SUMMARY.md                               # Executive summary (optional)
├── GENERATION_SUMMARY.md                    # Generation metadata (optional)
│
├── contracts/                               # API contracts and event schemas
│   ├── {service}-api.yaml                  # REST API contracts
│   ├── domain-events.yaml                  # Domain event schemas
│   └── ... (service-specific contracts)
│
├── checklists/                              # Quality checklists
│   ├── requirements.md                     # Requirements validation
│   └── constitution.md                     # Constitution compliance
│
└── features/                                # BDD feature files (Reqnroll)
    ├── user-story-1.feature
    ├── user-story-2.feature
    └── ... (Gherkin scenarios)
```

## Layered Approach

### Phase 0: Research & Analysis
- Document technology choices and alternatives
- Research patterns and best practices
- Create `research.md` with decisions and rationale

### Phase 1: Design
- Define feature specification in `spec.md` (requirements, user stories, success criteria)
- Create implementation plan in `plan.md` (technical context, architecture)
- Model domain entities in `data-model.md`
- Document contracts in `contracts/` directory
- Write setup guide in `quickstart.md`

### Phase 2: Task Breakdown
- Generate actionable tasks in `tasks.md`
- Organize by dependency order
- Map to constitution requirements

### Phase 3: Implementation
- Follow TDD Red → Green workflow
- Implement according to Clean Architecture
- Write BDD scenarios in `features/`
- Achieve ≥80% test coverage

## Layers

### Foundation Layer

Contains all core services and features that form the foundation of the NorthStar LMS (19 features: 001-019).

See [Foundation/README.md](./Foundation/README.md) for details.

**Includes:**
- Phase 1: Foundation Services (Identity, API Gateway, Configuration)
- Phase 2: Core Domain (Assessment, Staff, Intervention)
- Phase 3: Secondary Domain (Data Import, Sections, Reporting, Content)
- Homeschool Features (7 specialized features)

### Digital Ink Layer

Contains specialized digital ink capture and processing services (1 feature: 020).

See [DigitalInk/README.md](./DigitalInk/README.md) for details.

**Includes:**
- Digital ink capture and processing
- Real-time ink rendering
- Integration with assessment and student work

## Feature Numbering Convention

Features are numbered sequentially with a 3-digit prefix within each layer:

- **Foundation Layer (001-099)**: Core services and domain features
  - 001-005: Phase 1 Foundation Services
  - 006-008: Phase 2 Core Domain
  - 009-012: Phase 3 Secondary Domain
  - 013-019: Homeschool Features
  
- **Digital Ink Layer (020-099)**: Specialized ink services
  - 020: Digital ink service

- **Reserved Ranges**:
  - **100-199**: Domain services (future expansion)
  - **200-299**: Supporting services (future expansion)
  - **300-399**: UI and presentation features (future expansion)

## Constitution Alignment

All feature specifications must align with the [NorthStarET Constitution](../Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md):

### Clean Architecture & Aspire Orchestration
- Enforce layer boundaries: UI → Application → Domain ← Infrastructure
- Orchestrate via .NET Aspire hosting
- No direct UI → Infrastructure coupling

### Test-Driven Quality Gates
- Red → Green TDD workflow mandatory
- Reqnroll BDD scenarios for acceptance criteria
- Playwright UI tests for user journeys
- ≥80% test coverage at phase boundaries

### UX Traceability & Figma Accountability
- **New Features**: Require Figma links before UI implementation
- **UI Migration**: Preserve existing layouts, no Figma required
- Document UI behavior with Playwright tests

### Event-Driven Data Discipline
- Prefer asynchronous event-driven integration
- Idempotent commands with retry safety
- Version event schemas with deprecation windows
- Enforce multi-tenant isolation via `tenant_id` and Row-Level Security

### Security & Compliance Safeguards
- Least privilege authorization in Application layer
- Secrets only in platform secret store
- Audit logging for security events

## Creating a New Feature

1. **Create feature directory**: `mkdir specs/###-feature-name`
2. **Copy template structure**: 
   ```bash
   cp .specify/templates/spec-template.md specs/###-feature-name/spec.md
   cp .specify/templates/plan-template.md specs/###-feature-name/plan.md
   mkdir -p specs/###-feature-name/{contracts,checklists,features}
   ```
3. **Fill out spec.md**: Define requirements, user stories, success criteria
4. **Run /speckit.plan**: Generate implementation plan
5. **Run /speckit.tasks**: Generate task breakdown
6. **Implement with TDD**: Follow Red → Green workflow

## Related Documentation

- [Plans Directory](../Plans/) - High-level migration planning and architecture
- [Constitution](../Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md) - Governing principles
- [Master Migration Plan](../Plans/MASTER_MIGRATION_PLAN.md) - Complete migration roadmap
- [Service Catalog](../Plans/SERVICE_CATALOG.md) - All 11 microservices
- [WIPNorthStar](../Src/WIPNorthStar/) - Reference implementation

## Standards & Templates

- [Spec Template](./.specify/templates/spec-template.md) - Feature specification template
- [Plan Template](./.specify/templates/plan-template.md) - Implementation plan template
- [Tasks Template](./.specify/templates/tasks-template.md) - Task breakdown template
- [Checklist Template](./.specify/templates/checklist-template.md) - Quality checklist template

---

**Note**: This layered approach ensures each feature is independently specifiable, testable, and implementable while maintaining consistency across the entire migration project.
