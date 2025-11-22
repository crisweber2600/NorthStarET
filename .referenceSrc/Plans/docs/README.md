# NorthStar Migration Documentation Index

## Overview

This directory contains the comprehensive documentation for migrating OldNorthStar (.NET Framework 4.6 monolith) to UpgradedNorthStar (.NET 10 microservices architecture).

**Documentation Status**: ✅ Complete  
**Last Updated**: November 15, 2025  
**Version**: 2.0

---

## Primary Migration Plan

📋 **[INTEGRATED_MIGRATION_PLAN.md](../INTEGRATED_MIGRATION_PLAN.md)**

The master migration plan integrating:
- Original MIGRATION_PLAN.md (constitutional framework, phased approach)
- plan-monolithToMicroservicesMigration.prompt.md (concrete 10-step execution)
- microservices/* documentation (architecture, services, specifications)

**Contents**:
- Constitutional compliance requirements
- 11 microservices architecture
- 4-phase delivery roadmap (32 weeks)
- 10-step migration execution plan
- Risk management and success criteria

---

## Supporting Documentation

### Architecture & Design

1. **[../architecture/bounded-contexts.md](../architecture/bounded-contexts.md)**
   - Detailed bounded context definitions
   - Service dependency graph
   - Event flows and integration patterns
   - Anti-corruption layers

2. **[microservices/SERVICE_CATALOG.md](../microservices/SERVICE_CATALOG.md)**
   - Service inventory and status tracking
   - Implementation vs documentation structure
   - Spec-Kit ready feature prompts index

### Service Specifications

**Detailed Service Specs** (../architecture/services/):

3. **[identity-service.md](../architecture/services/identity-service.md)**
   - OAuth 2.0/OIDC authentication
   - Microsoft Entra ID integration
   - User management and claims

4. **[student-management-service.md](../architecture/services/student-management-service.md)**
   - Student enrollment and demographics
   - Domain model and events
   - API contracts

5. **[assessment-service-detailed.md](../architecture/services/assessment-service-detailed.md)** ✨ NEW
   - Assessment definitions and scoring
   - Benchmark management
   - Result recording with idempotency
   - Complete Clean Architecture structure

6. **Additional Service Specs**:
   - staff-management-service.md
   - intervention-management-service.md
   - section-roster-service.md
   - data-import-service.md
   - reporting-analytics-service.md
   - content-media-service.md
   - configuration-service.md
   - system-operations-service.md

### Technical Specifications

7. **[docs/DATA_MIGRATION_SPECIFICATION.md](DATA_MIGRATION_SPECIFICATION.md)** ✨ NEW
   - Entity mapping (383 entities from OldNorthStar)
   - ETL tool architecture
   - Migration phases with dual-write strategy
   - Data validation and reconciliation
   - PostgreSQL optimization (bulk insert, parallel processing)
   - Rollback procedures

8. **[docs/API_CONTRACTS_SPECIFICATION.md](API_CONTRACTS_SPECIFICATION.md)** ✨ NEW
   - RESTful API standards (URI structure, HTTP methods, response codes)
   - API contracts for all 11 services
   - YARP Gateway configuration
   - Pagination, error handling, versioning
   - OpenAPI/Swagger documentation requirements

9. **[docs/TESTING_STRATEGY.md](TESTING_STRATEGY.md)** ✨ NEW
   - TDD workflow (Red → Green → Refactor) with evidence capture
   - Testing pyramid (Unit 70%, Integration 15%, BDD 10%, UI 5%)
   - xUnit unit testing patterns
   - Aspire integration testing
   - Reqnroll BDD scenarios
   - Playwright UI testing
   - Performance testing (NBomber)
   - Coverage requirements (≥80%)

### Implementation Guides

10. **[microservices/docs/development-guide.md](../microservices/docs/development-guide.md)**
    - Development environment setup
    - Service development workflow
    - Coding standards (C# conventions, Clean Architecture)
    - Data access patterns (Repository, Unit of Work)
    - Event-driven communication
    - Security best practices

11. **[microservices/docs/deployment-guide.md](../microservices/docs/deployment-guide.md)**
    - Local development deployment (Docker Compose)
    - Staging deployment (Azure AKS)
    - Production deployment (Blue-Green, Rolling)
    - CI/CD pipeline (GitHub Actions)
    - Database migrations (EF Core)
    - Monitoring setup (Application Insights, Prometheus)
    - Rollback procedures

12. **[microservices/docs/api-gateway-config.md](../microservices/docs/api-gateway-config.md)**
    - YARP reverse proxy configuration
    - Routing rules
    - Authentication/authorization
    - Rate limiting

---

## Feature Specifications (Spec-Kit Ready)

### Core Platform Features

13. **[../../specs/identity-authentication-spec.md](../../specs/identity-authentication-spec.md)**
    - OAuth 2.0 login flow
    - Microsoft Entra ID SSO
    - Spec-Kit format (150-300 words, WHAT/WHY, no HOW)

14. **[../../specs/student-enrollment-spec.md](../../specs/student-enrollment-spec.md)**
    - Student registration and demographics
    - Acceptance scenarios

### Homeschool-Specific Features

15-22. **Homeschool Spec Suite** (../../specs/):
   - homeschool-parent-registration-spec.md
   - homeschool-student-enrollment-spec.md
   - homeschool-activity-logging-spec.md
   - homeschool-multigrade-spec.md
   - homeschool-compliance-dashboard-spec.md
   - homeschool-annual-report-spec.md
   - homeschool-coop-roster-spec.md

---

## How to Use This Documentation

### For Project Managers

**Planning Phase**:
1. Start with **INTEGRATED_MIGRATION_PLAN.md** for timeline and phases
2. Review **SERVICE_CATALOG.md** for service dependencies
3. Use **bounded-contexts.md** for architecture discussions
4. Track progress via migration checklist in main plan

### For Architects

**Architecture Design**:
1. Study **bounded-contexts.md** for DDD principles
2. Review service specs for Clean Architecture patterns
3. Validate **API_CONTRACTS_SPECIFICATION.md** for consistency
4. Use **DATA_MIGRATION_SPECIFICATION.md** for schema design

### For Developers

**Implementation**:
1. Read service-specific spec (e.g., **assessment-service-detailed.md**)
2. Follow **development-guide.md** for environment setup
3. Apply **TESTING_STRATEGY.md** for TDD workflow:
   - Write failing test (Red)
   - Implement feature (Green)
   - Capture evidence
   - Commit with phase review branch
4. Use **API_CONTRACTS_SPECIFICATION.md** for endpoint design
5. Reference **deployment-guide.md** for local testing

**Example Workflow**:
```bash
# 1. Read spec
cat ../architecture/services/student-management-service.md

# 2. Write failing test (RED)
dotnet test > docs/evidence/student-enrollment-red.txt

# 3. Implement feature

# 4. Tests pass (GREEN)
dotnet test > docs/evidence/student-enrollment-green.txt

# 5. Commit with evidence
git add .
git commit -m "Feature: Student enrollment [Phase2]"
git push origin HEAD:002review-Phase2
```

### For QA Engineers

**Testing Strategy**:
1. Review **TESTING_STRATEGY.md** for testing pyramid
2. Write Reqnroll BDD scenarios from feature specs
3. Implement Playwright UI tests (after Figma designs)
4. Validate coverage ≥80% requirement

### For DevOps

**Deployment**:
1. Follow **deployment-guide.md** for infrastructure setup
2. Configure CI/CD via GitHub Actions templates
3. Setup monitoring per service specs
4. Use rollback procedures from **DATA_MIGRATION_SPECIFICATION.md**

---

## Document Relationships

```
INTEGRATED_MIGRATION_PLAN.md (Master Plan)
├── bounded-contexts.md (Architecture Foundation)
├── SERVICE_CATALOG.md (Service Index)
├── Service Specs (11 detailed specifications)
│   ├── identity-service.md
│   ├── student-management-service.md
│   ├── assessment-service-detailed.md
│   └── ... (8 more)
├── DATA_MIGRATION_SPECIFICATION.md (Data Strategy)
├── API_CONTRACTS_SPECIFICATION.md (API Design)
├── TESTING_STRATEGY.md (Quality Assurance)
├── development-guide.md (Implementation)
└── deployment-guide.md (Operations)
```

---

## Key Decisions & Patterns

### Architectural Patterns

**Clean Architecture**:
- UI → Application → Domain ← Infrastructure
- Domain independence from frameworks
- MediatR CQRS for commands/queries

**Event-Driven**:
- Azure Service Bus for async communication
- Eventual consistency across services
- Outbox pattern for reliable publishing

**Database-Per-Service**:
- PostgreSQL per microservice
- Value objects for cross-service references (no FKs)
- Dual-write during migration

**API Gateway (YARP)**:
- Single entry point
- JWT authentication
- Routes to legacy + new services during migration

### Technology Choices

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Backend Framework | .NET 10 | Modern, performant, constitutional requirement |
| Database | PostgreSQL | Open-source, robust, better than SQL Server for cloud |
| Message Bus | Azure Service Bus | Managed, reliable, integrates with Aspire |
| Caching | Redis | Industry standard, fast, Aspire support |
| Authentication | Duende IdentityServer + Entra ID | OAuth 2.0/OIDC standard, enterprise SSO |
| API Gateway | YARP | Microsoft-supported, .NET native, simple config |
| Orchestration | .NET Aspire | Simplifies microservices development, built-in observability |
| Testing | xUnit + Reqnroll + Playwright | Best-in-class .NET testing tools |
| CI/CD | GitHub Actions | Git-native, extensive marketplace |

### Migration Strategy

**Strangler Fig Pattern**:
- Incrementally replace legacy functionality
- Both systems run in parallel during transition
- Feature flags for gradual traffic shifting

**Phased Approach**:
- Phase 1 (8 weeks): Foundation (Identity, Gateway, Config)
- Phase 2 (8 weeks): Core Domain (Student, Staff, Assessment)
- Phase 3 (6 weeks): Secondary Domain (Intervention, Section, Data Import)
- Phase 4 (6 weeks): Supporting (Reporting, Media, Operations)
- Phase 5 (8 weeks): UI Migration (blocked on Figma)

**Dual-Write**:
- Write to both legacy and new databases during cutover
- Nightly reconciliation jobs
- Validation dashboards

---

## Success Metrics

✅ **Functional**: All 383 entities migrated, zero data loss, feature parity  
✅ **Architectural**: 11 microservices following Clean Architecture, Aspire orchestration  
✅ **Quality**: ≥80% test coverage, all BDD scenarios passing, Playwright E2E tests  
✅ **Performance**: <50ms auth (P95), <100ms API queries (P95), <5s reports  
✅ **Security**: Zero hardcoded secrets, OAuth 2.0/OIDC, RBAC on all endpoints  
✅ **Operations**: 99.9% uptime, distributed tracing, centralized logging, automated deployments

---

## Next Steps

### Immediate Actions (Week 1)

1. ✅ Review **INTEGRATED_MIGRATION_PLAN.md** with stakeholders
2. ✅ Approve architecture per **bounded-contexts.md**
3. **Setup project tracking**:
   - Create GitHub project
   - Import issues from migration checklist
   - Assign teams to services
4. **Begin Step 1** (Component Inventory):
   - Analyze OldNorthStar controllers
   - Create COMPONENT_INVENTORY.md
   - Map entities to services

### Week 2-4 (Foundation Setup)

1. Setup UpgradedNorthStar solution structure
2. Configure Aspire AppHost
3. Create shared libraries (Contracts, Common)
4. Setup CI/CD pipeline templates

### Week 5-8 (Phase 1 Implementation)

1. Implement Identity Service (with Entra ID)
2. Implement API Gateway (YARP)
3. Implement Configuration Service
4. Deploy to development environment

### Blockers to Resolve

⚠️ **UI Migration Blocked**: Requires Figma design assets (constitutional requirement)  
⚠️ **WIPNorthStar Integration**: Complete Features 001/002/004 before merging

---

## Documentation Maintenance

**Ownership**:
- INTEGRATED_MIGRATION_PLAN.md - Architecture Team
- Service Specs - Service Teams (per service)
- DATA_MIGRATION_SPECIFICATION.md - Data Migration Team
- API_CONTRACTS_SPECIFICATION.md - API Architecture Team
- TESTING_STRATEGY.md - QA Team
- development-guide.md, deployment-guide.md - DevOps Team

**Update Frequency**:
- Master plan: Weekly during active migration
- Service specs: When service architecture changes
- Technical specs: When standards change
- Implementation guides: When tools/frameworks update

**Review Process**:
- All documentation changes via PR
- Architecture team approval for spec changes
- Update "Last Updated" timestamp on changes

---

**Document Version**: 1.0  
**Created**: November 15, 2025  
**Maintained By**: Documentation Team  
**Status**: Complete - Ready for Migration Start
