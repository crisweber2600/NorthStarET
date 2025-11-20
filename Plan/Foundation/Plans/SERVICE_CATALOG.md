# NorthStar LMS Microservices - Service Catalog & Specifications

## Overview

This directory contains detailed service specifications and Spec-Kit ready feature prompts for the NorthStar LMS microservices migration. The documentation integrates three complementary planning approaches:

1. **Migration Plan** (MICROSERVICES_MIGRATION_PLAN.md) - Constitutional framework, phased roadmap, and Clean Architecture principles
2. **Legacy Analysis** (.github/prompts/plan-microservicesMigration.prompt.md) - Detailed bounded context analysis from existing monolith
3. **Homeschooling Plan** (HOMESCHOOLING_PLAN.md) - Comprehensive plan for homeschool scenarios, state compliance, and co-op management

**The LMS is the base layer** - The NorthStar Learning Management System (LMS) serves as the **administrative foundation** for system and district administration, school management, staff management, and student management. It provides the platform for educational assessment, intervention management, and student progress tracking. The LMS integrates with **Microsoft Entra ID** (formerly Azure AD) for enterprise authentication and works as a unified system where legacy components operate alongside new microservices during the migration.

## LMS Architecture: Admin Layer Foundation

The NorthStar LMS is fundamentally an **administrative platform** with these core layers:

### Administrative Hierarchy
1. **System Administration** - Platform-wide configuration, multi-district management
2. **District Administration** - District-level settings, calendars, custom attributes
3. **School Management** - School configuration, grade levels, sections
4. **Staff Management** - Teacher and administrator accounts, permissions, teams
5. **Student Management** - Student enrollment, demographics, academic records

### Authentication & Integration
- **Microsoft Entra ID Integration** - Enterprise SSO for staff and administrators
- **Login Management** - User authentication, role-based access control (RBAC)
- **Multi-Tenancy** - Database-per-district isolation with centralized authentication

### Operational Capabilities
- **Manual & Automated Data Entry** - CSV/Excel import, state test data integration, automated workflows
- **Legacy-New Component Interoperability** - Gradual migration with parallel operation
- **Event-Driven Coordination** - Async communication between new microservices and legacy components

## Integration Strategy

### Constitutional Foundation (v1.6.0)
All services adhere to the **NorthStarET NextGen LMS Constitution**:
- Clean Architecture (UI → Application → Domain → Infrastructure)
- .NET Aspire orchestration
- Test-Driven Development (Red → Green with Reqnroll BDD)
- Figma-backed UI development
- Event-driven async-first communication
- Phase review branch workflow

### Bounded Contexts (Legacy Analysis)
The existing prompt file identified 13 bounded contexts through domain-driven design analysis of the .NET Framework 4.6 monolith. These align with and expand upon the 11 microservices in the migration plan:

**Merged Service Mapping**:
1. **Identity & Authentication** = Legacy Context #1 (Authentication & Authorization) + Microsoft Entra ID integration
2. **Student Management** = Legacy Context #3 (Student Management)
3. **Staff Management** = Legacy Context #12 (Staff Management)
4. **Assessment** = Legacy Context #2 (Assessment Management) + #11 (Benchmark Management)
5. **Intervention Management** = Legacy Context #4 (Intervention Management)
6. **Section & Roster** = Legacy Context #5 (Section/Classroom) + #8 (Class Roster)
7. **Data Import** = Legacy Context #10 (Data Entry) + **Automated Data Entry** workflows
8. **Reporting & Analytics** = Legacy Context #6 (Reporting) + #7 (Dashboard/Aggregation)
9. **Content & Media** = Legacy Context #9 (File Storage) + Legacy Context #13 (Video/Content)
10. **Configuration** = Legacy Context (District Configuration) - distributed across services
11. **System Operations** = Monitoring & Health - new service for observability

### Legacy-New Component Interoperability

During the migration, **legacy components will operate alongside new microservices** using a hybrid integration strategy:

**API Gateway Bridge**:
- YARP API Gateway routes requests to both legacy NS4.WebAPI and new microservices
- Legacy endpoints gradually sunset as microservices reach feature parity
- Shared authentication via Identity Service (Microsoft Entra ID with session-based authentication)
- During 90-day parallel run period: Legacy IdentityServer sessions supported read-only (see legacy-identityserver-migration.md)

**Event-Driven Synchronization**:
- New microservices publish domain events to Azure Service Bus
- Legacy components can subscribe to events for data synchronization
- Dual-write pattern during transition (write to both legacy DB and new service DB)

**Data Entry Continuity**:
- **Manual Data Entry**: CSV/Excel import works with both legacy and new Data Import Service
- **Automated Data Entry**: Background workflows process state test data, automated rollover, batch operations
- Legacy batch processors (NorthStar.BatchProcessor, NorthStar4.BatchPrint) migrate to .NET 8 Worker Services

**Administrative Workflows**:
- System/district admin functions accessible through both legacy NS4.Angular and new UI
- Staff and student management operations gradually migrate to new services
- Configuration changes propagate via events to maintain consistency

### Implementation Structure

```
NewDesign/NorthStarET.Lms/          # .NET 8 LMS implementation (base layer)
├── src/
│   ├── services/                   # 11 microservices
│   │   ├── Identity/              # Foundation - Phase 1
│   │   ├── ApiGateway/            # Foundation - Phase 1
│   │   ├── Configuration/         # Foundation - Phase 1
│   │   ├── Student/               # Core Domain - Phase 2
│   │   ├── Staff/                 # Core Domain - Phase 2
│   │   ├── Assessment/            # Core Domain - Phase 2
│   │   ├── Intervention/          # Secondary - Phase 3
│   │   ├── Section/               # Secondary - Phase 3
│   │   ├── DataImport/            # Secondary - Phase 3
│   │   ├── Reporting/             # Supporting - Phase 4
│   │   ├── Media/                 # Supporting - Phase 4
│   │   └── Operations/            # Supporting - Phase 4
│   ├── shared/                    # Shared libraries
│   │   ├── Contracts/            # Event contracts, DTOs
│   │   ├── Domain/               # Common domain primitives
│   │   └── Infrastructure/       # Aspire Service Defaults
│   └── AppHost/                   # .NET Aspire orchestration
├── tests/
│   ├── unit/                      # TDD unit tests per service
│   ├── integration/               # Aspire integration tests
│   ├── bdd/                       # Reqnroll BDD features
│   └── ui/                        # Playwright UI tests (Figma-backed)
└── specs/                         # Spec-Kit feature specifications
    └── [feature]/figma-prompts/  # Design collaboration prompts

microservices/                      # Documentation layer
├── services/                      # Detailed service docs (this directory)
│   ├── identity-service.md
│   ├── student-management-service.md
│   ├── assessment-service.md
│   └── ... (9 more services)
├── specs/                         # Spec-Kit ready prompts
│   ├── identity-authentication-spec.md
│   ├── student-enrollment-spec.md
│   └── ... (feature specifications)
├── architecture/
│   └── bounded-contexts.md       # Comprehensive context definitions
└── docs/
    ├── development-guide.md
    ├── api-gateway-config.md
    └── deployment-guide.md
```

## Service Documentation Index

Each service has two complementary documents:

### Detailed Service Specifications (architecture/services/)

Comprehensive technical documentation including:
- Current state (legacy) and target state (.NET 8)
- Clean Architecture layer structure
- Owned data and service boundaries
- Domain events published/subscribed
- API functional intent (no implementation details)
- SLOs, idempotency, consistency models
- Security, testing, and monitoring requirements
- Migration strategy and implementation checklist

**Available Services**:
1. [Identity & Authentication Service](./services/identity-service.md) - OAuth 2.0/OIDC, user management
2. [Student Management Service](./services/student-management-service.md) - Student enrollment, demographics
3. [Assessment Service](./services/assessment-service.md) - Benchmarks, testing, scoring
4. [Staff Management Service](./services/staff-management-service.md) - Teacher/Admin accounts, teams
5. [Intervention Management Service](./services/intervention-management-service.md) - Tiers, progress monitoring
6. [Section & Roster Service](./services/section-roster-service.md) - Class rosters, scheduling
7. [Data Import Service](./services/data-import-service.md) - State test ETL, manual import
8. [Reporting & Analytics Service](./services/reporting-analytics-service.md) - Dashboards, CQRS read models
9. [Content & Media Service](./services/content-media-service.md) - File storage, video streaming
10. [Configuration Service](./services/configuration-service.md) - District/School settings, calendars
11. [System Operations Service](./services/system-operations-service.md) - Health, diagnostics, observability
12. [Digital Ink Service](./services/digital-ink-service.md) - High-fidelity stylus input, audio sync, and playback

### Feature Specs Coverage

| Service | Phase | Core Feature Specs | Status |
|---------|-------|-------------------|--------|
| **Identity** | 1 | User Authentication | ✅ Existing |
| | | Password Reset & Recovery | 📝 Planned |
| | | Role/Permission Admin | 📝 Planned |
| **Configuration** | 1 | District/School Provisioning | 📝 Planned |
| | | Academic Calendar & Rules | 📝 Planned |
| **Student** | 2 | Enrollment Management | ✅ Existing |
| | | Grade Promotion & Rollover | 📝 Planned |
| **Staff** | 2 | Entra ID Sync & Deactivation | 📝 Planned |
| **Assessment** | 2 | Benchmark Period Lifecycle | 📝 Planned |
| | | Assignment & Scheduling | 📝 Planned |
| **Intervention** | 3 | Risk Trigger Workflow | ⏳ Backlog |
| **Section** | 3 | Automated Rollover | ⏳ Backlog |
| **Data Import** | 3 | State Test Integration | ⏳ Backlog |
| **Reporting** | 4 | Dashboard Projections | ⏳ Backlog |
| **Media** | 4 | Secure Attachments | ⏳ Backlog |
| **Operations** | 4 | Health Aggregation | ⏳ Backlog |
| **Digital Ink** | 4 | Ink Session Recording | 📝 Planned |
| | | Audio Synchronization | 📝 Planned |

### Spec-Kit Ready Feature Prompts (../specs/)

Optimized `/specify` prompts (150-300 words) following Spec-Driven Development:
- **WHAT & WHY**: User outcomes and experience (not HOW)
- **Vertical Slice**: Single feature, single owner, clear value
- **Boundary Guarantees**: SLOs, idempotency windows, consistency models
- **Functional Requirements**: 3-6 "Must..." bullets
- **Constraints / Non-Goals**: What's explicitly excluded
- **Acceptance Signals**: 3 short scenarios (pre-Gherkin)
- **Handoff to /plan**: What to detail in implementation planning

**Available Specifications**:

**Core Platform Features**:
1. [Identity: User Authentication](./specs/identity-authentication-spec.md) - OAuth 2.0 login flow
2. [Student: Enrollment Management](./specs/student-enrollment-spec.md) - Student registration and demographics

**Homeschool-Specific Features** (see [HOMESCHOOLING_PLAN.md](../HOMESCHOOLING_PLAN.md)):
3. [Homeschool: Parent Registration](./specs/homeschool-parent-registration-spec.md) - Homeschool parent account with state compliance
4. [Homeschool: Student Enrollment](./specs/homeschool-student-enrollment-spec.md) - Flexible grade assignment and learning plans
5. [Homeschool: Activity Logging](./specs/homeschool-activity-logging-spec.md) - Daily learning with time tracking and portfolio evidence
6. [Homeschool: Multi-Grade Tracking](./specs/homeschool-multigrade-spec.md) - Subject-level grade assignments
7. [Homeschool: Compliance Dashboard](./specs/homeschool-compliance-dashboard-spec.md) - Real-time state requirement tracking
8. [Homeschool: Annual Report](./specs/homeschool-annual-report-spec.md) - State-specific compliance report generation
9. [Homeschool: Co-op Roster](./specs/homeschool-coop-roster-spec.md) - Multi-family co-op class management

*(Additional specs to be created for each key feature)*

## Usage Guide

### For Product Managers & Architects

**Planning a New Feature**:
1. Start with the **Spec-Kit prompt** (specs/) for high-level definition
2. Reference the **detailed service doc** (services/) for technical boundaries
3. Ensure alignment with **constitution** (Clean Architecture, TDD, Figma accountability)
4. Create Figma designs before implementation (constitutional requirement)

### For Developers

**Implementing a Service**:
1. Read the **detailed service specification** (services/) for architecture
2. Review **Spec-Kit prompts** (specs/) for feature requirements
3. Set up **Clean Architecture** project structure in `NewDesign/`
4. Write **Reqnroll BDD features** before code (Red state)
5. Implement following **TDD** (Red → Green → Refactor)
6. Configure **.NET Aspire** orchestration in AppHost
7. Run **integration tests** to validate cross-service flows
8. Create **Playwright UI tests** for Figma-backed components
9. Deploy and monitor following **phase review branch** workflow

**Constitutional Workflow**:
```bash
# Document Red state
dotnet test > test-evidence-red.txt

# Implement feature
# (following Clean Architecture, Aspire orchestration)

# Document Green state
dotnet test > test-evidence-green.txt

# Commit with evidence
git add .
git commit -m "Feature: Student enrollment [Phase2]"
git push origin HEAD:002review-Phase2  # Phase review branch
```

### For QA Engineers

**Testing a Feature**:
1. Review **Acceptance Signals** in Spec-Kit prompt
2. Expand into **Reqnroll Given/When/Then** scenarios
3. Execute **BDD tests** via `dotnet test` on Reqnroll project
4. Run **Playwright UI tests** with Figma design validation
5. Verify **Aspire integration tests** for cross-service flows
6. Validate **SLOs** from service specification

## Spec-Kit Specify Coach Integration

When creating new feature specifications, use the **Spec-Kit Specify Coach** protocol (from comment):

**Intake Questions** (≤5 targeted questions):
1. Primary user & goal? (Who, what outcome?)
2. Where in product? (page/flow/component)
3. Must-have behaviors (top 3-6)? Non-goals?
4. Boundary guarantees? (idempotency, consistency window, SLO)
5. Ownership & dependencies? (service boundaries)

**Output Structure**:
- Feature Title
- Goal / Why (2-3 sentences)
- Intended Experience / What (3-5 sentences, user flows)
- Service Boundary Outcomes (ownership, SLOs, events)
- Functional Requirements (3-6 "Must..." bullets)
- Constraints / Non-Goals (1-4 bullets)
- Acceptance Signals (3 short scenarios)
- Handoff to /plan (what to detail in implementation)

**Quality Checklist**:
- ✅ 150-300 words; clear WHAT/WHY; no HOW
- ✅ Vertical slice (one feature, one owner, single branch)
- ✅ 3-6 FRs; 1-4 constraints
- ✅ Service SLOs present
- ✅ No endpoints/DTOs/framework names
- ✅ Acceptance signals stated

## Service Dependency Graph

```
┌─────────────────────────┐
│  Identity Service       │ ← Foundation (no dependencies)
│  (Phase 1)              │
└───────────┬─────────────┘
            │ provides auth tokens
            ↓
┌───────────────────────────────────────────┐
│  API Gateway (YARP)                       │
│  (Phase 1)                                │
└───────────┬───────────────────────────────┘
            │ routes & validates
            ↓
┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│ Student Service   │  │ Staff Service     │  │ Assessment Service│
│ (Phase 2)         │  │ (Phase 2)         │  │ (Phase 2)         │
└─────────┬─────────┘  └─────────┬─────────┘  └─────────┬─────────┘
          │ publishes events      │                      │
          └───────────────────────┼──────────────────────┘
                                  ↓
┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│ Intervention Svc  │  │ Section Service   │  │ Data Import Svc   │
│ (Phase 3)         │  │ (Phase 3)         │  │ (Phase 3)         │
└───────────────────┘  └───────────────────┘  └───────────────────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 ↓
┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│ Reporting Service │  │ Media Service     │  │ Operations Svc    │
│  (Phase 4 - CQRS)  │  │ (Phase 4)         │  │ (Phase 4)         │
└───────────────────┘  └───────────────────┘  └───────────────────┘
                                  │
                                  ↓
                       ┌───────────────────┐
                       │ Digital Ink Svc   │
                       │ (Phase 4)         │
                       └───────────────────┘
```

**Event Flow** (Async via Azure Service Bus):
- Core services publish domain events (StudentCreated, AssessmentCompleted)
- Downstream services subscribe to relevant events
- Reporting service aggregates data via event sourcing (CQRS read models)
- No synchronous cross-service calls (constitutional requirement)

## Next Steps

### Immediate Actions
1. Complete remaining service documentation (9 services)
2. Create Spec-Kit prompts for key features per service
3. Review and approve specifications with stakeholders
4. Begin Phase 1 implementation (Identity, API Gateway, Configuration)

### Template Usage
When documenting a new service:
1. Copy structure from `identity-service.md` or `student-management-service.md`
2. Fill in service-specific details from legacy analysis
3. Define Clean Architecture layers
4. Map domain events (published/subscribed)
5. Specify SLOs and consistency models
6. Create migration strategy

When creating a new feature spec:
1. Follow Spec-Kit Specify Coach protocol
2. Keep to 150-300 words (max 500)
3. Focus on WHAT/WHY, not HOW
4. Include service boundary outcomes and SLOs
5. Defer implementation details to /plan

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Maintained By**: Architecture Team  
**Status**: Service catalog established; ongoing specification development
