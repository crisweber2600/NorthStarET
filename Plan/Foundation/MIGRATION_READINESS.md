# NorthStarET Foundation Layer: Migration Readiness Checklist

**Version**: 1.0.0  
**Date**: 2025-11-20  
**Purpose**: Validate readiness to begin Phase 1 (Foundation Services) implementation  
**Timeline**: Weeks 1-8 (Identity, API Gateway, Configuration)  
**Status**: Pre-Migration Verification

---

## Overview

This checklist consolidates all prerequisites for beginning Phase 1 of the OldNorthStar-to-Foundation migration. Completing this checklist ensures:

1. **Constitutional Compliance**: All governance principles are understood and enforceable
2. **Architectural Readiness**: Foundation layer structure is prepared
3. **Technical Infrastructure**: Development, testing, and deployment environments are configured
4. **Team Alignment**: Developers understand patterns, standards, and workflows
5. **Data Migration Preparation**: ETL strategy is defined and validated

**References**:
- [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) - Primary migration roadmap (v3.0)
- [INTEGRATED_MIGRATION_PLAN.md](./INTEGRATED_MIGRATION_PLAN.md) - Detailed technical integration (v2.1)
- [Constitution v2.0.0](../../.specify/memory/constitution.md) - Governance principles
- [LAYERS.md](../LAYERS.md) - Mono-repo layer architecture

---

## Checklist Categories

- [Constitutional Compliance](#constitutional-compliance)
- [Architectural Readiness](#architectural-readiness)
- [Phase 1 Service Specifications](#phase-1-service-specifications)
- [Development Environment](#development-environment)
- [Testing Infrastructure](#testing-infrastructure)
- [Data Migration Prerequisites](#data-migration-prerequisites)
- [Deployment Pipeline](#deployment-pipeline)
- [Team Readiness](#team-readiness)

---

## Constitutional Compliance

Verify adherence to [NorthStarET Mono-Repo Constitution v2.0.0](../../.specify/memory/constitution.md):

### Principle 1: Clean Architecture & Aspire Orchestration

- [ ] **Clean Architecture boundaries documented**: UI → Application → Domain ← Infrastructure
- [ ] **.NET Aspire 13.0.0+ installed and configured** for service orchestration
- [ ] **Service templates follow Clean Architecture** with proper project structure:
  - `{ServiceName}.Domain/` - Entities, value objects, domain events
  - `{ServiceName}.Application/` - CQRS handlers, interfaces, DTOs
  - `{ServiceName}.Infrastructure/` - EF Core, repositories, external services
  - `{ServiceName}.Api/` - REST API endpoints
  - `{ServiceName}.Tests/` - Unit, integration, BDD tests
- [ ] **Aspire Service Defaults created** in `Src/Foundation/shared/ServiceDefaults/`
- [ ] **AppHost project created** in `Src/Foundation/AppHost/`

### Principle 2: Test-Driven Quality Gates

- [ ] **TDD workflow documented**: Red → Green → Refactor with evidence capture
- [ ] **≥80% code coverage requirement enforced** via CI/CD pipeline
- [ ] **Reqnroll BDD framework installed** (latest stable version)
- [ ] **Playwright 1.47.0+ installed** for UI testing
- [ ] **Test evidence templates created**:
  - `test-evidence-red.txt` - Failing test output
  - `test-evidence-green.txt` - Passing test output
- [ ] **Aspire integration test projects scaffolded** for Phase 1 services

### Principle 3: UX Traceability & Figma Accountability

- [ ] **UI Migration exemption understood**: OldNorthStar UI preservation does NOT require Figma
- [ ] **Playwright tests prepared for UI preservation validation**
- [ ] **Original OldNorthStar UI screenshots captured** for functional parity reference
- [ ] **New features workflow documented**: Figma-first for post-migration enhancements only

### Principle 4: Event-Driven Data Discipline

- [ ] **MassTransit 8.x installed and configured**
- [ ] **Azure Service Bus connection strings** in Azure Key Vault (production)
- [ ] **RabbitMQ Docker container** available for local development
- [ ] **Multi-tenancy architecture documented**:
  - Database-per-service pattern
  - `tenant_id` column requirement
  - PostgreSQL Row-Level Security (RLS) policies
- [ ] **Event versioning strategy defined**
- [ ] **Idempotency patterns documented** for command handlers

### Principle 5: Security & Compliance Safeguards

- [ ] **Azure Key Vault provisioned** for secrets management
- [ ] **Microsoft Entra ID tenant configured** with application registrations (Web + API)
- [ ] **Microsoft.Identity.Web NuGet packages** added to Identity service
- [ ] **SessionAuthenticationHandler implementation** documented and tested
- [ ] **RBAC roles defined** for Application layer authorization
- [ ] **Secrets management workflow documented** (no secrets in code/config/logs)
- [ ] **Redis Stack configured** for session caching and idempotency windows

### Principle 6: Mono-Repo Layer Isolation

- [ ] **Foundation layer scope documented** in [LAYERS.md](../LAYERS.md)
- [ ] **Shared infrastructure contracts defined**:
  - `ServiceDefaults/` - Aspire orchestration, logging, telemetry
  - `Domain/` - Shared value objects, base entities
  - `Application/` - Shared DTOs, interfaces
  - `Infrastructure/` - Database helpers, caching, messaging abstractions
- [ ] **Cross-layer dependency rules enforced** (no direct service calls across layers)
- [ ] **Layer identification required** in all specs, plans, tasks
- [ ] **Documentation separation validated**:
  - Migration-specific: `Plan/Foundation/Plans/`
  - Layer-agnostic: `/docs/architecture/` and `/docs/standards/`

### Principle 7: Tool-Assisted Development Workflow

- [ ] **AI agents have access to structured thinking tools** (`#think`, `#mcp_sequentialthi_sequentialthinking`)
- [ ] **Microsoft documentation search configured** (`#microsoft.docs.mcp`)
- [ ] **Figma MCP server available** (for post-migration new features)
- [ ] **Playwright browser automation available**
- [ ] **Chrome DevTools MCP available** for debugging

---

## Architectural Readiness

### Foundation Layer Structure

- [ ] **`Src/Foundation/` directory created** with scaffolded structure:
  ```
  Src/Foundation/
  ├── services/
  │   ├── Identity/              # Phase 1
  │   ├── ApiGateway/            # Phase 1
  │   └── Configuration/         # Phase 1
  ├── shared/
  │   ├── ServiceDefaults/
  │   ├── Domain/
  │   ├── Application/
  │   └── Infrastructure/
  └── AppHost/                   # Aspire orchestration
  ```
- [ ] **Service README stubs created** with architecture diagrams and spec cross-references
- [ ] **Shared infrastructure initialized** with base contracts

### Documentation Organization

- [ ] **Migration-critical docs remain in `Plan/Foundation/Plans/`**:
  - ✅ MASTER_MIGRATION_PLAN.md
  - ✅ INTEGRATED_MIGRATION_PLAN.md
  - ✅ MIGRATION_PLAN.md
  - ✅ INTEGRATION_VALIDATION.md
  - ✅ SERVICE_CATALOG.md
  - ✅ MIGRATION_READINESS.md (this file)
  - ✅ `scenarios/` (13 implementation scenarios)
  - ✅ `docs/` (DATA_MIGRATION_SPECIFICATION.md, deployment-guide.md, development-guide.md)
- [ ] **General architecture promoted to `/docs/architecture/`**:
  - ✅ bounded-contexts.md
  - ✅ `services/` (13 service architecture files)
- [ ] **Technical standards promoted to `/docs/standards/`**:
  - ✅ API_CONTRACTS_SPECIFICATION.md
  - ✅ api-gateway-config.md
  - ✅ TESTING_STRATEGY.md
- [ ] **`Plan/Foundation/Plans/README.md` updated** with reorganization details

### Bounded Context Validation

- [ ] **Bounded contexts reviewed**: [docs/architecture/bounded-contexts.md](../../../docs/architecture/bounded-contexts.md)
- [ ] **Phase 1 bounded contexts validated**:
  - Identity & Authentication Context
  - Configuration Context
  - API Gateway Context (infrastructure)
- [ ] **Service boundaries clear** with no overlapping responsibilities
- [ ] **Cross-service event contracts defined**

---

## Phase 1 Service Specifications

### Identity & Authentication Service

**Spec Location**: `Plan/Foundation/specs/Foundation/002-identity-authentication/`

- [ ] **Specification complete**: `spec.md`, `plan.md`, `data-model.md`, `quickstart.md`, `research.md`, `tasks.md`
- [ ] **API contracts defined**: `contracts/identity-api.yaml`
- [ ] **Domain events documented**: `contracts/domain-events.yaml`
- [ ] **Constitution checklist validated**: `checklists/constitution.md`
- [ ] **Reqnroll features created**: `features/*.feature`
- [ ] **Microsoft Entra ID integration scenario reviewed**: [scenarios/01-identity-migration-entra-id.md](./scenarios/01-identity-migration-entra-id.md)
- [ ] **Microsoft Entra ID integration documented** (app registrations, token validation, session management)
- [ ] **JWT token strategy defined** (access tokens, refresh tokens, expiration)
- [ ] **User roles and permissions mapped** from OldNorthStar to Foundation

**Legacy Components to Migrate**:
- `OldNorthStar/IdentityServer/` - Existing IdentityServer setup
- `NS4.WebAPI/Controllers/AuthController.cs` - Authentication endpoints
- `NS4.WebAPI/Controllers/PasswordResetController.cs` - Password management

### API Gateway Service (YARP)

**Spec Location**: `Plan/Foundation/specs/Foundation/005-api-gateway/`

- [ ] **Specification complete**: `spec.md`, `plan.md`, `data-model.md`, `quickstart.md`, `research.md`, `tasks.md`
- [ ] **YARP configuration documented**: [docs/standards/api-gateway-config.md](../../../docs/standards/api-gateway-config.md)
- [ ] **Routing rules defined** for:
  - OldNorthStar legacy routes (Strangler Fig pattern)
  - New Foundation service routes
  - Route priority and fallback strategies
- [ ] **Rate limiting policies configured**
- [ ] **Circuit breaker patterns documented** (Polly)
- [ ] **Gateway orchestration scenario reviewed**: [scenarios/06-api-gateway-orchestration.md](./scenarios/06-api-gateway-orchestration.md)
- [ ] **Health check aggregation configured**
- [ ] **Distributed tracing setup** (OpenTelemetry)
- [ ] **API versioning strategy defined** (`/api/v1/`, `/api/v2/`)

**Legacy Components to Migrate**:
- `NS4.WebAPI/` - Existing API routing and controllers (to be proxied during migration)

### Configuration Service

**Spec Location**: `Plan/Foundation/specs/Foundation/004-configuration-service/`

- [ ] **Specification complete**: `spec.md`, `plan.md`, `data-model.md`, `quickstart.md`, `research.md`, `tasks.md`
- [ ] **API contracts defined**: `contracts/configuration-api.yaml`
- [ ] **Configuration data model documented**:
  - Tenants (districts)
  - Schools
  - Grades/Grade Levels
  - System settings
  - Feature flags
- [ ] **Multi-tenancy isolation validated** (`tenant_id` + RLS policies)
- [ ] **Configuration caching strategy documented** (Redis)
- [ ] **Configuration change event propagation defined**
- [ ] **Configuration service scenario reviewed**: [scenarios/07-configuration-service.md](./scenarios/07-configuration-service.md)

**Legacy Components to Migrate**:
- `NS4.WebAPI/Controllers/DistrictSettingsController.cs` - District configuration
- `NS4.WebAPI/Controllers/NavigationController.cs` - Navigation/menu settings
- `NorthStar.EF6/DistrictSettingsDataService.cs` - Configuration data access

---

## Development Environment

### Local Development Setup

- [ ] **.NET 10 SDK installed** (latest stable version)
- [ ] **Visual Studio 2022 or VS Code** with .NET Aspire workload
- [ ] **Docker Desktop installed and running**
- [ ] **PostgreSQL Docker container available** for local development
- [ ] **Redis Docker container available** for caching
- [ ] **RabbitMQ Docker container available** for messaging (local dev)
- [ ] **Azure CLI installed** for Azure service access
- [ ] **Aspire Dashboard accessible** at `http://localhost:15888` (default)

### IDE and Tooling

- [ ] **C# Dev Kit** (VS Code) or Visual Studio 2022 17.x+
- [ ] **Aspire workload installed**: `dotnet workload install aspire`
- [ ] **EF Core tools installed**: `dotnet tool install --global dotnet-ef`
- [ ] **Reqnroll extension installed** for feature file editing
- [ ] **Playwright CLI available**: `pwsh bin/Debug/net10.0/playwright.ps1 install`
- [ ] **Git configured** with proper `.gitignore` for .NET

### OldNorthStar Reference Access

- [ ] **OldNorthStar codebase available** at `.referenceSrc/OldNorthStar/`
- [ ] **OldNorthStar can build successfully** (verification only, not required for daily dev)
- [ ] **OldNorthStar database backup available** for reference and data migration testing
- [ ] **Legacy API documentation available** (if exists)
- [ ] **Legacy database schema documented**

---

## Testing Infrastructure

### Unit Testing

- [ ] **xUnit 2.9.0+ installed** in all test projects
- [ ] **FluentAssertions** available for readable test assertions
- [ ] **Moq or NSubstitute** available for mocking
- [ ] **Code coverage tool configured** (coverlet, ReportGenerator)
- [ ] **≥80% coverage requirement enforced** in CI pipeline
- [ ] **TDD evidence template prepared**: Red → Green transcripts

### Integration Testing

- [ ] **Aspire integration test projects scaffolded** for each Phase 1 service
- [ ] **Test containers configured** (Testcontainers.PostgreSQL, Testcontainers.Redis)
- [ ] **Integration test database reset strategy** (per-test isolation)
- [ ] **Aspire test host configured** for orchestrating multiple services
- [ ] **Integration test scenarios documented**

### BDD Testing (Reqnroll)

- [ ] **Reqnroll project created** with step definitions
- [ ] **Feature files created** for Phase 1 user stories
- [ ] **SpecFlow Living Doc integration** (optional, for documentation generation)
- [ ] **BDD test evidence capture** configured

### UI Testing (Playwright)

- [ ] **Playwright 1.47.0+ installed** with browsers
- [ ] **Playwright test project created** in C# (not JavaScript)
- [ ] **Page Object Model (POM) patterns established**
- [ ] **Visual regression testing configured** (optional, for UI preservation validation)
- [ ] **UI test scenarios documented** for OldNorthStar functional parity

### Test Automation

- [ ] **GitHub Actions workflows created** for:
  - Unit tests on every commit
  - Integration tests on PR
  - BDD tests on PR
  - UI tests on staging deployment
- [ ] **Test result reporting configured** (GitHub Actions summary, badges)
- [ ] **Parallel test execution enabled** for faster feedback

---

## Data Migration Prerequisites

### ETL Strategy

- [ ] **Data Migration Specification reviewed**: [docs/DATA_MIGRATION_SPECIFICATION.md](./docs/DATA_MIGRATION_SPECIFICATION.md)
- [ ] **Data migration scenario reviewed**: [scenarios/04-data-migration-etl.md](./scenarios/04-data-migration-etl.md)
- [ ] **Multi-tenant architecture scenario reviewed**: [scenarios/02-multi-tenant-database-architecture.md](./scenarios/02-multi-tenant-database-architecture.md)

### Tenant Tagging

- [ ] **Tenant tagging strategy defined**:
  - Add `tenant_id` (GUID) to all records
  - Map legacy district database → tenant ID
  - Validate no cross-tenant data leakage
- [ ] **Tenant ID generation strategy documented** (deterministic vs. random)
- [ ] **Tenant metadata documented**: District name, status, subscription tier, etc.

### Identity Mapping

- [ ] **Identity mapping strategy defined**:
  - Legacy integer IDs → GUIDs
  - ID mapping tables created for reference
  - Foreign key transformation rules documented
- [ ] **User ID mapping**: OldNorthStar user IDs → Entra ID object IDs
- [ ] **Student ID preservation strategy** (maintain legacy IDs for external integrations)

### Data Validation

- [ ] **Data quality rules defined**:
  - Required field validation
  - Data type validation
  - Referential integrity checks
  - Business rule validation
- [ ] **Data reconciliation strategy**: 100% record count matching
- [ ] **Data migration testing plan**: Dry runs, rollback procedures, cutover validation

### Dual-Write Pattern

- [ ] **Dual-write pattern documented** for transition period:
  - Write to both OldNorthStar and Foundation databases
  - Consistency validation
  - Conflict resolution strategy
- [ ] **Dual-write decommission plan**: When/how to stop writing to legacy DB

### Rollback Procedures

- [ ] **Database backup strategy documented**
- [ ] **Rollback procedures tested**: Restore from backup, revert data changes
- [ ] **Rollback decision criteria defined**: Error thresholds, data integrity failures

---

## Deployment Pipeline

### Infrastructure as Code

- [ ] **Azure resource templates created**:
  - Azure Kubernetes Service (AKS) cluster
  - Azure PostgreSQL Flexible Server (11 databases, one per service)
  - Azure Service Bus namespace
  - Azure Blob Storage accounts
  - Azure Key Vault
  - Azure Application Insights
  - Azure Container Registry (ACR)
- [ ] **Infrastructure deployment automation** (Bicep, Terraform, or ARM templates)
- [ ] **Environment separation**: Dev, Staging, Production

### Kubernetes Configuration

- [ ] **Kubernetes manifests created** for Phase 1 services:
  - `deployment.yaml` - Service deployments
  - `service.yaml` - ClusterIP services
  - `ingress.yaml` - External access
  - `configmap.yaml` - Non-sensitive configuration
  - `secret.yaml` - References to Azure Key Vault
- [ ] **Namespace strategy defined**: `northstar-dev`, `northstar-staging`, `northstar-prod`
- [ ] **Resource limits configured** (CPU, memory)
- [ ] **Horizontal Pod Autoscaler (HPA) configured** for scalability

### Docker

- [ ] **Dockerfiles created** for each Phase 1 service
- [ ] **Multi-stage builds configured** for optimized image size
- [ ] **Base image strategy**: Use official Microsoft .NET images
- [ ] **Container security scanning** enabled (Trivy, Snyk)
- [ ] **Image tagging strategy**: Semantic versioning + git commit SHA

### CI/CD Pipeline

- [ ] **GitHub Actions workflows created**:
  - Build and test on commit
  - Build Docker images on PR merge
  - Push to ACR
  - Deploy to dev environment (automatic)
  - Deploy to staging (automatic on main branch)
  - Deploy to production (manual approval)
- [ ] **Deployment smoke tests configured**
- [ ] **Rollback automation** in case of deployment failure
- [ ] **Blue-green deployment strategy** documented (optional, for zero-downtime)

### Monitoring and Observability

- [ ] **Application Insights configured** for distributed tracing
- [ ] **Structured logging implemented** (Serilog with JSON output)
- [ ] **Metrics collection configured** (Prometheus/OpenTelemetry)
- [ ] **Health check endpoints implemented** (`/health`, `/ready`)
- [ ] **Alerting rules defined**: Error rate, latency, availability
- [ ] **Dashboard created**: Service health, request rates, error rates, latency percentiles

---

## Team Readiness

### Training and Documentation

- [ ] **Development guide reviewed**: [docs/development-guide.md](./docs/development-guide.md)
- [ ] **Deployment guide reviewed**: [docs/deployment-guide.md](./docs/deployment-guide.md)
- [ ] **Testing strategy reviewed**: [docs/standards/TESTING_STRATEGY.md](../../../docs/standards/TESTING_STRATEGY.md)
- [ ] **API contracts specification reviewed**: [docs/standards/API_CONTRACTS_SPECIFICATION.md](../../../docs/standards/API_CONTRACTS_SPECIFICATION.md)
- [ ] **Constitution principles understood**: [.specify/memory/constitution.md](../../.specify/memory/constitution.md)
- [ ] **Layer architecture understood**: [LAYERS.md](../LAYERS.md)

### Architecture Review

- [ ] **Architecture team identified** (reviewers for cross-cutting decisions)
- [ ] **Architecture review process documented**:
  - When required: SLO changes, security changes, layer boundary changes
  - How to request: Issue template, review meeting schedule
  - Approval criteria
- [ ] **Architectural Decision Records (ADR) repository created** for tracking decisions

### Communication Channels

- [ ] **Team communication channel established** (Slack/Teams: `#microservices-migration`)
- [ ] **Daily standup scheduled** (or async updates)
- [ ] **Weekly architecture review meeting scheduled**
- [ ] **Incident response plan documented**: On-call rotation, escalation procedures

### Code Review Process

- [ ] **Code review guidelines documented**:
  - Reviewers per PR (minimum 1-2)
  - Review checklist: Constitution compliance, test coverage, documentation
  - Approval requirements before merge
- [ ] **Branch protection rules configured**:
  - Require PR reviews
  - Require status checks (CI passing)
  - No direct commits to `main`
- [ ] **PR templates created** with constitution checklist

### Knowledge Transfer

- [ ] **OldNorthStar walkthrough completed** (architecture, key components)
- [ ] **WIPNorthStar walkthrough completed** (proven patterns, lessons learned)
- [ ] **Clean Architecture training session** (boundaries, dependency rules)
- [ ] **.NET Aspire training session** (orchestration, service defaults, integration tests)
- [ ] **Reqnroll/BDD training session** (feature files, step definitions)
- [ ] **Playwright training session** (page objects, selectors, assertions)

---

## Pre-Migration Validation

### Constitution Compliance Verification

Run through constitution checklist for Phase 1:

1. **Clean Architecture**: Service templates follow UI → Application → Domain ← Infrastructure
2. **Aspire Orchestration**: AppHost and ServiceDefaults configured
3. **TDD Gates**: Red → Green workflow documented, ≥80% coverage enforced
4. **UI Preservation**: Playwright tests prepared, Figma exemption understood
5. **Event-Driven**: MassTransit + Service Bus configured, multi-tenancy enforced
6. **Security**: Key Vault configured, Entra ID integrated, RBAC defined
7. **Layer Isolation**: Shared infrastructure contracts defined, dependency rules validated

### Phase 1 Service Readiness

For each Phase 1 service (Identity, API Gateway, Configuration):

1. **Specification complete**: All documents in `specs/{service}/`
2. **API contracts defined**: OpenAPI YAML in `contracts/`
3. **Domain model documented**: Entities, value objects, aggregates
4. **Database schema designed**: Multi-tenancy enforced (tenant_id + RLS)
5. **Reqnroll features created**: User stories as Given/When/Then
6. **Test projects scaffolded**: Unit, integration, BDD, UI tests
7. **Deployment manifests prepared**: Docker, Kubernetes YAML

### Data Migration Readiness

1. **ETL scripts prepared** for Phase 1 data (users, tenants, configuration)
2. **Tenant tagging tested** on sample data
3. **Identity mapping validated** (OldNorthStar IDs → GUIDs)
4. **Data quality rules enforced** (validation scripts)
5. **Dry run completed** with OldNorthStar backup database
6. **Rollback tested**: Restore from backup successfully

### Deployment Pipeline Validation

1. **Infrastructure provisioned** in dev environment
2. **CI/CD pipeline tested**: Commit → build → test → deploy to dev
3. **Health checks validated**: All services report `/health` = healthy
4. **Aspire Dashboard accessible**: Visualize service orchestration
5. **Monitoring dashboard populated**: Metrics, logs, traces visible

---

## Sign-Off

### Architecture Team Approval

- [ ] **Architecture Lead**: _________________________ Date: _________
- [ ] **Lead Developer**: _________________________ Date: _________
- [ ] **DevOps Lead**: _________________________ Date: _________

### Readiness Assessment

**Overall Readiness Score**: _____ / 100 (minimum 90% to proceed)

**Risk Areas Identified**:
1. _______________________________________________________________
2. _______________________________________________________________
3. _______________________________________________________________

**Mitigation Plans**:
1. _______________________________________________________________
2. _______________________________________________________________
3. _______________________________________________________________

### Go/No-Go Decision

**Decision**: [ ] GO   [ ] NO-GO

**Rationale**:
___________________________________________________________________
___________________________________________________________________
___________________________________________________________________

**Planned Start Date**: _______________

**Expected Phase 1 Completion Date**: _______________ (8 weeks from start)

---

## Next Steps After Approval

1. **Kick-off Meeting**: Schedule with full team to review readiness and Phase 1 plan
2. **Environment Setup**: Provision infrastructure in Azure (if not already done)
3. **Feature Branch Creation**: Create `feature/001-phase1-foundation` branch
4. **Sprint Planning**: Break down Phase 1 tasks into 2-week sprints
5. **Daily Standups**: Begin daily sync meetings for Phase 1 team
6. **Begin Implementation**: Start with Identity Service (highest priority, dependency for others)

---

## References

### Primary Documentation

- [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) - Complete migration roadmap v3.0
- [INTEGRATED_MIGRATION_PLAN.md](./INTEGRATED_MIGRATION_PLAN.md) - Detailed technical plan v2.1
- [Constitution v2.0.0](../../.specify/memory/constitution.md) - Governance principles
- [LAYERS.md](../LAYERS.md) - Mono-repo layer architecture

### Technical Specifications

- [DATA_MIGRATION_SPECIFICATION.md](./docs/DATA_MIGRATION_SPECIFICATION.md) - ETL strategy
- [API_CONTRACTS_SPECIFICATION.md](../../../docs/standards/API_CONTRACTS_SPECIFICATION.md) - API design patterns
- [TESTING_STRATEGY.md](../../../docs/standards/TESTING_STRATEGY.md) - TDD/BDD/Integration approach
- [api-gateway-config.md](../../../docs/standards/api-gateway-config.md) - YARP configuration

### Implementation Scenarios

- [01-identity-migration-entra-id.md](./scenarios/01-identity-migration-entra-id.md)
- [02-multi-tenant-database-architecture.md](./scenarios/02-multi-tenant-database-architecture.md)
- [04-data-migration-etl.md](./scenarios/04-data-migration-etl.md)
- [06-api-gateway-orchestration.md](./scenarios/06-api-gateway-orchestration.md)
- [07-configuration-service.md](./scenarios/07-configuration-service.md)

### Service Architectures

- [identity-service.md](../../../docs/architecture/services/identity-service.md)
- [configuration-service.md](../../../docs/architecture/services/configuration-service.md)

---

**Last Updated**: 2025-11-20  
**Maintained By**: Architecture Team  
**Review Frequency**: Before each phase kickoff  
**Next Review**: Before Phase 2 (Weeks 9-16)
