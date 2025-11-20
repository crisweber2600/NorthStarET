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

## Mono-Repo Layer Architecture

This specifications directory implements the **Foundation Layer** mono-repo architecture defined in [LAYERS.md](../LAYERS.md). All features here belong to the Foundation layer, with clear boundaries enforced by the constitution.

### Foundation Layer (Features 001-019)

**Purpose**: Core OldNorthStar-to-.NET-10 migration implementation  
**Location**: `Src/Foundation/` (implementation), `Plan/Foundation/specs/Foundation/` (specifications)  
**Status**: Active Development (22-32 week timeline)

Contains all core services and features that form the foundation of the NorthStar LMS:

**Phase 1: Foundation Services (Weeks 1-8)**
- 001: Phase 1 Foundation Services (orchestration)
- 002: Identity & Authentication (Duende IdentityServer + Entra ID)
- 004: Configuration Service (tenants, schools, settings)
- 005: API Gateway (YARP, Strangler Fig pattern)

**Phase 2: Core Domain (Weeks 9-16)**
- 003: Student Enrollment & Management
- 006: Assessment Service
- 007: Staff Management Service

**Phase 3: Secondary Domain (Weeks 17-22)**
- 008: Intervention Management Service
- 009: Data Import & Integration Service
- 010: Section & Roster Service

**Phase 4: Supporting Services (Weeks 23-28)**
- 011: Reporting & Analytics Service
- 012: Content & Media Service

**Homeschool Features (Integrated Throughout)**
- 013: Homeschool Parent Registration
- 014: Homeschool Student Enrollment
- 015: Homeschool Annual Report
- 016: Homeschool Compliance Dashboard
- 017: Homeschool Activity Logging
- 018: Homeschool Multigrade Management
- 019: Homeschool Co-op Roster Management

**Shared Infrastructure** (All Phases):
- `Src/Foundation/shared/ServiceDefaults/` - Aspire orchestration, logging, telemetry
- `Src/Foundation/shared/Domain/` - Shared value objects, base entities
- `Src/Foundation/shared/Application/` - Shared DTOs, interfaces
- `Src/Foundation/shared/Infrastructure/` - Database helpers, caching, messaging

See [Foundation/README.md](./Foundation/README.md) for details.

### DigitalInk Layer (Feature 020)

**Purpose**: Specialized digital ink and handwriting recognition capabilities  
**Location**: `Plan/Foundation/specs/DigitalInk/` (specifications), future `Src/DigitalInk/` (implementation)  
**Status**: Specification Phase

Contains specialized digital ink capture and processing services:

- 020: Digital Ink Service (capture, rendering, recognition, assessment integration)

**Dependencies on Foundation**:
- Identity & Authentication (user context, authorization)
- Configuration Service (tenant settings, feature flags)
- Assessment Service (ink-based assessment integration points)
- Content & Media Service (storage for ink data and artifacts)

**Cross-Layer Communication**: Event-based only (subscribes to Foundation domain events, publishes DigitalInk events)

See [DigitalInk/README.md](./DigitalInk/README.md) for details.

### Layer Dependency Rules

Per [Constitution v2.0.0 Principle 6: Mono-Repo Layer Isolation](../../.specify/memory/constitution.md#principle-6-mono-repo-layer-isolation):

✅ **Permitted Cross-Layer Dependencies**:
- **Shared Infrastructure**: Identity, Configuration, ServiceDefaults, Domain primitives
- **Event Bus**: Asynchronous event-based communication (publish/subscribe)

❌ **Prohibited Cross-Layer Dependencies**:
- **Direct Service Calls**: No synchronous API calls between layers
- **Database Access**: No cross-layer database queries
- **Internal Domain Logic**: No access to another layer's domain models

**Example**: DigitalInk may NOT call Student Management Service directly. Instead, DigitalInk subscribes to `StudentEnrolledEvent` and maintains its own read model.

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

All feature specifications must align with the [NorthStarET Mono-Repo Constitution v2.0.0](../../.specify/memory/constitution.md):

### 1. Clean Architecture & Aspire Orchestration
- Enforce layer boundaries: UI → Application → Domain ← Infrastructure
- Orchestrate via .NET Aspire 13.0.0 hosting
- No direct UI → Infrastructure coupling
- Aspire test projects for integration validation

### 2. Test-Driven Quality Gates
- Red → Green TDD workflow mandatory with evidence capture
- Reqnroll BDD scenarios for acceptance criteria (before implementation)
- Playwright UI tests for user journeys (with red/green logs)
- ≥80% test coverage at phase boundaries
- `dotnet test` transcripts required: `test-evidence-red.txt` and `test-evidence-green.txt`

### 3. UX Traceability & Figma Accountability
- **New Features**: Require Figma links before UI implementation, maintain `figma-prompts/` directory
- **UI Migration** (OldNorthStar): Preserve existing layouts, NO Figma required
- Document original UI behavior with Playwright tests for functional parity
- Technology stack upgrade (AngularJS → modern) while maintaining UX continuity

### 4. Event-Driven Data Discipline
- Prefer asynchronous event-driven integration via Azure Service Bus
- Idempotent commands with retry safety
- Version event schemas with deprecation windows
- **Multi-tenancy**: Database-per-service with `tenant_id` columns and PostgreSQL Row-Level Security (RLS)
- Document latency budgets for any synchronous cross-service calls

### 5. Security & Compliance Safeguards
- Least privilege authorization in Application layer via RBAC
- Secrets only in Azure Key Vault (never in code, config, logs)
- Audit logging for security events and tenant access
- No UI → Infrastructure coupling

### 6. Mono-Repo Layer Isolation (NON-NEGOTIABLE)
- **Layer Identification Required**: All specs must identify target layer (Foundation, DigitalInk, etc.)
- **Shared Infrastructure Only**: Cross-layer dependencies restricted to Identity, Configuration, ServiceDefaults, Domain primitives
- **No Direct Service Calls**: Event-based communication between layers only
- **Documentation Boundaries**: Layer-specific specs in `specs/{LayerName}/`, general architecture in `/docs/`
- See [LAYERS.md](../LAYERS.md) for complete dependency rules

### 7. Tool-Assisted Development Workflow
- Use structured thinking tools (`#think`) for planning
- Query official documentation (`#microsoft.docs.mcp`) for .NET/Azure patterns
- UI development pipeline: Figma (new features) → Playwright → Chrome DevTools
- Research-first pattern for technology decisions

## Creating a New Feature

1. **Identify Target Layer**: Determine if feature belongs to Foundation, DigitalInk, or future layer
2. **Verify Layer Dependencies**: Ensure feature only depends on approved shared infrastructure (see [LAYERS.md](../LAYERS.md))
3. **Create feature directory**: `mkdir specs/{LayerName}/###-feature-name`
4. **Copy template structure**: 
   ```bash
   cp .specify/templates/spec-template.md specs/{LayerName}/###-feature-name/spec.md
   cp .specify/templates/plan-template.md specs/{LayerName}/###-feature-name/plan.md
   mkdir -p specs/{LayerName}/###-feature-name/{contracts,checklists,features}
   ```
5. **Fill out spec.md**: 
   - **MUST include layer identification** (Foundation, DigitalInk, etc.)
   - Define requirements, user stories, success criteria
   - Document cross-layer dependencies (must be shared infrastructure only)
6. **Run /speckit.plan**: Generate implementation plan
7. **Run /speckit.tasks**: Generate task breakdown
8. **Validate Constitution Compliance**: Complete `checklists/constitution.md` including layer isolation checks
9. **Implement with TDD**: Follow Red → Green workflow with evidence capture

**Layer Isolation Validation**:
- [ ] Feature identifies target layer
- [ ] No direct service calls to other layers (event-based only)
- [ ] Dependencies limited to shared infrastructure (Identity, Configuration, ServiceDefaults, Domain)
- [ ] Constitution checklist includes layer boundary verification

## Related Documentation

### Governance & Architecture
- [Constitution v2.0.0](../../.specify/memory/constitution.md) - Governing principles including mono-repo layer isolation
- [LAYERS.md](../LAYERS.md) - Mono-repo layer architecture, dependency rules, shared infrastructure
- [Master Migration Plan](../Plans/MASTER_MIGRATION_PLAN.md) - Complete migration roadmap (v3.0, 22-32 weeks)
- [Migration Readiness Checklist](../Plans/MIGRATION_READINESS.md) - Phase 1 readiness validation

### Migration Planning
- [Plans Directory](../Plans/) - Migration-specific plans, scenarios, technical docs
- [Service Catalog](../Plans/SERVICE_CATALOG.md) - All 11 Foundation microservices
- [Scenarios](../Plans/scenarios/) - 13 implementation scenarios (identity, multi-tenancy, UI, data, services)

### Technical Standards (Repository Root)
- [Architecture Patterns](../../../docs/architecture/) - Bounded contexts, service architectures (layer-agnostic)
- [API Contracts](../../../docs/standards/API_CONTRACTS_SPECIFICATION.md) - RESTful API design patterns
- [Testing Strategy](../../../docs/standards/TESTING_STRATEGY.md) - TDD/BDD/Integration/Playwright approach
- [API Gateway Config](../../../docs/standards/api-gateway-config.md) - YARP configuration patterns

### Reference Implementation
- [WIPNorthStar](../../.referenceSrc/WIPNorthStar/) - Proven patterns from features 001/002/004
- [OldNorthStar](../../.referenceSrc/OldNorthStar/) - Legacy codebase to migrate

## Standards & Templates

- [Spec Template](../../.specify/templates/spec-template.md) - Feature specification template
- [Plan Template](../../.specify/templates/plan-template.md) - Implementation plan template
- [Tasks Template](../../.specify/templates/tasks-template.md) - Task breakdown template
- [Constitution Agent](../../.github/agents/speckit.constitution.agent.md) - Constitution update workflow

---

**Note**: This layered approach ensures each feature is independently specifiable, testable, and implementable while maintaining consistency across the entire migration project.
