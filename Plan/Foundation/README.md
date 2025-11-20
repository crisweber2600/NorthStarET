# NorthStar LMS Migration Plans

This directory contains high-level migration planning documentation, architecture decisions, and reference materials for the NorthStar LMS modernization project.

## Purpose

The **Plans** directory serves as the strategic planning hub for the migration from .NET Framework 4.6 monolith to .NET 10 microservices. It contains:

- Migration roadmaps and phasing strategies
- Architecture patterns and service boundaries
- Technical specifications and standards
- Implementation scenarios and patterns
- Deployment and development guides

**Note:** For detailed feature specifications and implementation plans, see the `/specs` directory which follows a layered, feature-based organization.

## Directory Structure

### Migration-Critical Documentation (This Directory)

The Plans directory contains **migration-specific** documentation focused on transitioning OldNorthStar to the new .NET 10 Foundation layer. General architecture and standards have been promoted to repository root for cross-layer reuse.

```
Plans/
├── README.md                              # This file
├── MASTER_MIGRATION_PLAN.md               # ⭐ Primary migration roadmap (v3.0)
├── INTEGRATED_MIGRATION_PLAN.md           # Detailed integrated plan (v2.1)
├── MIGRATION_PLAN.md                      # Original migration roadmap (v1.1)
├── INTEGRATION_VALIDATION.md              # Integration testing strategy
├── SERVICE_CATALOG.md                     # Overview of all 11 microservices
├── MIGRATION_READINESS.md                 # Phase 1 readiness checklist
│
├── docs/                                 # Migration-specific technical docs
│   ├── README.md
│   ├── DATA_MIGRATION_SPECIFICATION.md   # ETL strategy for OldNorthStar data
│   ├── deployment-guide.md              # Migration deployment instructions
│   └── development-guide.md             # Migration developer setup
│
└── scenarios/                            # 13 Migration implementation scenarios
    ├── README.md
    ├── SCENARIO_INVENTORY.md
    ├── 01-identity-migration-entra-id.md
    ├── 02-multi-tenant-database-architecture.md
    ├── 03-ui-migration-preservation.md
    ├── 04-data-migration-etl.md
    ├── 05-student-management-service.md
    ├── 06-api-gateway-orchestration.md
    ├── 07-configuration-service.md
    ├── 08-staff-management-service.md
    ├── 09-assessment-service.md
    ├── 10-intervention-management-service.md
    ├── 11-section-roster-service.md
    └── 12-data-import-service.md
```

### General Architecture & Standards (Moved to Repository Root)

The following materials have been **promoted to `/docs/`** at repository root as they are reusable across all mono-repo layers (Foundation, DigitalInk, future layers):

```
/docs/                                     # Repository root documentation
├── architecture/                          # Layer-agnostic architecture patterns
│   ├── bounded-contexts.md               # DDD bounded context analysis
│   └── services/                         # Technical architecture per service
│       ├── identity-service.md
│       ├── student-management-service.md
│       ├── assessment-service.md
│       ├── staff-management-service.md
│       ├── intervention-management-service.md
│       ├── section-roster-service.md
│       ├── data-import-service.md
│       ├── reporting-analytics-service.md
│       ├── content-media-service.md
│       ├── digital-ink-service.md
│       ├── system-operations-service.md
│       ├── assessment-service-detailed.md
│       └── configuration-service.md
│
└── standards/                            # Cross-layer technical standards
    ├── API_CONTRACTS_SPECIFICATION.md    # API design patterns (RESTful, versioning)
    ├── api-gateway-config.md            # YARP configuration patterns
    └── TESTING_STRATEGY.md              # TDD/BDD/Integration/Playwright approach
```

**Rationale for Separation**: Migration plans are Foundation-layer-specific and time-bound (22-32 weeks), while architecture patterns and standards are intended for reuse across the entire mono-repo lifecycle. Promoting general documentation to repository root prevents duplication and enforces consistency across layers.

## Quick Links

### Planning & Strategy (Migration-Specific)
- ⭐ [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) - **Start here** - Complete integrated plan
- [MIGRATION_PLAN.md](./MIGRATION_PLAN.md) - Original migration roadmap
- [INTEGRATED_MIGRATION_PLAN.md](./INTEGRATED_MIGRATION_PLAN.md) - Detailed plan
- [INTEGRATION_VALIDATION.md](./INTEGRATION_VALIDATION.md) - Integration testing strategy
- [SERVICE_CATALOG.md](./SERVICE_CATALOG.md) - All 11 microservices overview
- [MIGRATION_READINESS.md](./MIGRATION_READINESS.md) - Phase 1 readiness checklist

### Architecture (General - Repository Root)
- [Bounded Contexts](../../../docs/architecture/bounded-contexts.md) - DDD analysis
- [Service Architectures](../../../docs/architecture/services/) - Technical specs per service

### Technical Standards (General - Repository Root)
- [API Contracts](../../../docs/standards/API_CONTRACTS_SPECIFICATION.md) - API design patterns
- [API Gateway Config](../../../docs/standards/api-gateway-config.md) - YARP configuration
- [Testing Strategy](../../../docs/standards/TESTING_STRATEGY.md) - TDD/BDD/Integration approach

### Migration-Specific Technical Docs
- [Data Migration](./docs/DATA_MIGRATION_SPECIFICATION.md) - ETL strategy for OldNorthStar
- [Development Guide](./docs/development-guide.md) - Migration developer setup
- [Deployment Guide](./docs/deployment-guide.md) - Migration deployment instructions

### Implementation Scenarios
- [Scenarios](./scenarios/) - 13 migration implementation patterns
- [Scenario Inventory](./scenarios/SCENARIO_INVENTORY.md) - Complete scenario listing

### Feature Specifications
- [/specs directory](../specs/) - Detailed feature specs in layered format (Foundation, DigitalInk)
- [Layer Documentation](../LAYERS.md) - Mono-repo layer structure and dependency rules

## Migration Phases

| Phase | Duration | Services | Status |
|-------|----------|----------|--------|
| **Phase 1: Foundation** | Weeks 1-8 | Identity, API Gateway, Configuration | Not Started |
| **Phase 2: Core Domain** | Weeks 9-16 | Student, Staff, Assessment | Not Started |
| **Phase 3: Secondary Domain** | Weeks 17-22 | Intervention, Section, Data Import | Not Started |
| **Phase 4: Supporting** | Weeks 23-28 | Reporting, Media, Operations | Not Started |
| **UI Migration** | Weeks 20-28 | Blazor Web App (parallel) | In Progress |
| **Iterative Cutover** | Weeks 29-32 | Production rollout | Not Started |

## Getting Started

### For Architects
1. Review [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md)
2. Study [Service Catalog](./SERVICE_CATALOG.md)
3. Examine [Bounded Contexts](./architecture/bounded-contexts.md)
4. Validate service boundaries and dependencies

### For Developers
1. Review [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md)
2. Read [Development Guide](./docs/development-guide.md)
3. Study [WIPNorthStar implementation](../Src/WIPNorthStar/) for patterns
4. Review feature specs in [/specs directory](../specs/)
5. Follow TDD Red→Green workflow (≥80% coverage)

### For Product Managers
1. Review [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md)
2. Track progress against phased timeline
3. Review feature specifications in [/specs directory](../specs/)

### For QA Engineers
1. Review [Testing Strategy](./docs/TESTING_STRATEGY.md)
2. Study feature specs in [/specs directory](../specs/) for acceptance criteria
3. Prepare Reqnroll BDD scenarios
4. Setup Playwright for UI testing

1. Set up container registry
2. Configure CI/CD pipelines
3. Provision Kubernetes cluster
4. Set up monitoring infrastructure
5. Configure API Gateway

## Key Principles

### 1. Single Responsibility
Each microservice should have one clear business responsibility.

### 2. Independent Deployment
Services must be deployable independently without affecting others.

### 3. Database per Service
Each service owns its data. No shared databases.

### 4. API-First Design
Define and document APIs before implementation.

### 5. Resilience
Implement circuit breakers, retries, and fallback mechanisms.

### 6. Observability
Comprehensive logging, metrics, and tracing from day one.

### 7. Security
Security is built in, not bolted on.

### 8. Testing
Automated testing at all levels (unit, integration, contract, e2e).

## Communication Patterns

### Synchronous Communication
- Use for user-facing operations
- REST APIs with HTTP/2
- gRPC for internal high-performance needs
- Implement timeouts and circuit breakers

### Asynchronous Communication
- Use for domain events
- Message bus (RabbitMQ/Azure Service Bus)
- Pub/Sub pattern for events
- Event sourcing for audit trails

## Data Management

### Owned Data
Each service manages its own database schema and data.

### Shared Reference Data
- Replicate via events
- Cache frequently accessed data
- Accept eventual consistency

### Transactions
- Use Saga pattern for distributed transactions
- Implement compensating transactions
- Event sourcing for complex workflows

## Technology Stack

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- MassTransit for messaging

### API Gateway
- YARP or Ocelot
- Authentication via JWT
- Rate limiting and throttling

### Data Stores
- SQL Server (per service)
- Redis (caching)
- Blob Storage (files)

### Infrastructure
- Docker containers
- Kubernetes orchestration
- GitHub Actions for CI/CD

### Monitoring
- Application Insights
- Prometheus + Grafana
- Seq for logging
- OpenTelemetry for tracing

## Naming Conventions

### Services
- Use kebab-case: `student-management-service`
- Include service suffix: `-service`

### Docker Images
- Format: `northstar/[service-name]:[version]`
- Example: `northstar/student-service:1.0.0`

### Databases
- Format: `NorthStar_[ServiceName]_DB`
- Example: `NorthStar_Student_DB`

### API Routes
- Format: `/api/[version]/[resource]`
- Example: `/api/v1/students`

### Events
- Format: `[Entity][Action]Event`
- Example: `StudentEnrolledEvent`

## Folder Structure (Per Service)

```
[service-name]/
├── src/
│   ├── [ServiceName].API/           # Web API project
│   ├── [ServiceName].Core/          # Domain logic
│   ├── [ServiceName].Infrastructure/ # Data access, external services
│   └── [ServiceName].Contracts/     # DTOs, contracts
├── tests/
│   ├── [ServiceName].UnitTests/
│   ├── [ServiceName].IntegrationTests/
│   └── [ServiceName].ContractTests/
├── docker/
│   └── Dockerfile
├── k8s/
│   ├── deployment.yaml
│   ├── service.yaml
│   └── configmap.yaml
└── README.md
```

## API Documentation Standards

All services must:
- Use Swagger/OpenAPI 3.0
- Document all endpoints
- Include request/response examples
- Specify error codes and messages
- Version APIs explicitly

## Coding Standards

### General
- Follow C# coding conventions
- Use async/await for I/O operations
- Implement proper exception handling
- Log structured data

### API Controllers
- Keep controllers thin
- Use MediatR for CQRS pattern
- Return ActionResult types
- Use data annotations for validation

### Domain Logic
- Keep in Core project
- No dependencies on infrastructure
- Use domain events
- Implement domain validations

### Data Access
- Repository pattern
- Unit of Work pattern
- Use EF Core migrations
- Avoid N+1 queries

## Testing Requirements

### Unit Tests
- Minimum 80% code coverage
- Test business logic thoroughly
- Mock external dependencies
- Fast execution (< 1 minute for all tests)

### Integration Tests
- Test database interactions
- Test message bus integration
- Use test containers
- Clean up after each test

### Contract Tests
- Use Pact or similar
- Provider and consumer tests
- Run in CI pipeline
- Prevent breaking changes

### E2E Tests
- Critical user journeys only
- Run against staging environment
- Automated in deployment pipeline
- Monitor for flaky tests

## Deployment Process

### Development
1. Create feature branch
2. Implement changes
3. Write tests
4. Submit PR
5. Code review
6. Merge to main

### CI/CD Pipeline
1. Build service
2. Run unit tests
3. Build Docker image
4. Run integration tests
5. Push to dev registry
6. Deploy to dev environment
7. Run smoke tests
8. Deploy to staging (on main branch)
9. Run E2E tests
10. Deploy to production (manual approval)

## Monitoring Checklist

Each service must implement:
- [ ] Health check endpoint (`/health`)
- [ ] Ready check endpoint (`/ready`)
- [ ] Structured logging with correlation IDs
- [ ] Metrics collection (Prometheus format)
- [ ] Distributed tracing (OpenTelemetry)
- [ ] Custom business metrics
- [ ] Error rate monitoring
- [ ] Performance monitoring
- [ ] Dependency health checks

## Security Checklist

Each service must implement:
- [ ] JWT authentication
- [ ] Role-based authorization
- [ ] Input validation
- [ ] SQL injection prevention
- [ ] XSS protection
- [ ] CORS configuration
- [ ] HTTPS only
- [ ] Secrets from Key Vault
- [ ] Dependency vulnerability scanning
- [ ] Container image scanning

## Resources

### Learning Materials
- [Microservices Patterns](https://microservices.io/patterns/)
- [.NET Microservices Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Building Microservices by Sam Newman](https://samnewman.io/books/building_microservices/)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)

### Tools
- [Docker](https://www.docker.com/)
- [Kubernetes](https://kubernetes.io/)
- [MassTransit](https://masstransit-project.com/)
- [Polly](https://github.com/App-vNext/Polly)
- [Swagger/OpenAPI](https://swagger.io/)

## Support & Questions

- Architecture questions: Tag `@architecture-team`
- DevOps questions: Tag `@devops-team`
- General questions: Post in `#microservices-migration` channel

## Contributing

When adding new services:
1. Follow the service template structure
2. Document in service catalog
3. Define API contracts first
4. Update architecture diagrams
5. Create deployment manifests
6. Set up monitoring and alerts
7. Write comprehensive tests
8. Update this README with service status

---

**Last Updated**: 2025-11-13  
**Maintained By**: Architecture Team
