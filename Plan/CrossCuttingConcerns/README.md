# NorthStarET Cross-Cutting Concerns

This directory contains **architecture patterns, technical standards, and workflow guidance** that are reusable across ALL mono-repo layers (Foundation, DigitalInk, future layers).

## Purpose

Cross-cutting concerns are **layer-agnostic** and **evergreen** - they define how services are built, tested, and integrated regardless of which layer they belong to. This prevents duplication and enforces consistency across the mono-repo.

## Directory Structure

```
Plan/CrossCuttingConcerns/
├── README.md                                    # This file
│
├── architecture/
│   ├── README.md                               # Architecture patterns index
│   ├── bounded-contexts.md                     # DDD bounded context analysis
│   ├── domain-events-schema.md                 # Standardized event schema
│   └── services/                               # Technical architecture per service
│       ├── README.md
│       ├── identity-service.md
│       ├── configuration-service.md
│       └── ... (13 service architectures)
│
├── standards/
│   ├── README.md                               # Standards index
│   ├── API_CONTRACTS_SPECIFICATION.md          # RESTful API patterns
│   ├── TESTING_STRATEGY.md                     # TDD/BDD/Playwright approach
│   ├── api-gateway-config.md                   # YARP configuration
│   └── security-compliance.md                  # Security, RBAC, FERPA compliance
│
├── patterns/
│   ├── README.md                               # Patterns catalog
│   ├── aspire-orchestration.md                 # .NET Aspire hosting & service discovery
│   ├── clean-architecture.md                   # Layer isolation & dependency rules
│   ├── dependency-injection.md                 # DI conventions & service lifetimes
│   ├── messaging-integration.md                # MassTransit, RabbitMQ, event-driven
│   ├── multi-tenancy.md                        # Tenant isolation, EF Core, RLS
│   ├── multi-tenant-database.md                # Database-per-service pattern
│   ├── caching-performance.md                  # Redis Stack, idempotency, SLOs
│   └── observability.md                        # Logging, tracing, metrics, health checks
│
├── scenarios/
│   ├── README.md                               # Infrastructure scenarios index
│   ├── 01-identity-service.md                  # Authentication & authorization
│   ├── 02-api-gateway.md                       # YARP routing & cross-cutting concerns
│   └── 03-configuration-service.md             # Multi-tenant configuration
│
└── workflow/
    ├── README.md                               # Workflow guidance index
    └── speckit-coach-guide.md                  # Spec-Kit development workflows
```

## How to Reference

### From Layer Planning Directories
```markdown
# From Plan/Foundation/ or Plan/DigitalInk/
[Bounded Contexts](../CrossCuttingConcerns/architecture/bounded-contexts.md)
[API Standards](../CrossCuttingConcerns/standards/API_CONTRACTS_SPECIFICATION.md)
```

### From Service Implementation Code
```markdown
# From Src/Foundation/services/Identity/ or Src/DigitalInk/
[identity-service.md](../../../../Plan/CrossCuttingConcerns/architecture/services/identity-service.md)
```

### Dependency Declaration
Each layer's README should list cross-cutting dependencies:
```markdown
## Cross-Cutting References
- [Bounded Contexts](../CrossCuttingConcerns/architecture/bounded-contexts.md)
- [Domain Events Schema](../CrossCuttingConcerns/architecture/domain-events-schema.md)
- [Testing Strategy](../CrossCuttingConcerns/standards/TESTING_STRATEGY.md)
```

## Constitution Principle Mapping

This directory provides comprehensive guidance for all 7 constitution principles:

| Principle | Coverage | Documents |
|-----------|----------|-----------|
| **1. Clean Architecture & Aspire Orchestration** | ✅ Complete | [Aspire Orchestration](patterns/aspire-orchestration.md), [Clean Architecture](patterns/clean-architecture.md), [Dependency Injection](patterns/dependency-injection.md), [Observability](patterns/observability.md) |
| **2. Test-Driven Quality Gates** | ✅ Complete | [Testing Strategy](standards/TESTING_STRATEGY.md) |
| **3. UX Traceability & Figma Accountability** | ✅ Complete | [Speckit Coach Guide](workflow/speckit-coach-guide.md) |
| **4. Event-Driven Data Discipline** | ✅ Complete | [Domain Events Schema](architecture/domain-events-schema.md), [Messaging Integration](patterns/messaging-integration.md), [Multi-Tenancy](patterns/multi-tenancy.md), [Multi-Tenant Database](patterns/multi-tenant-database.md), [Caching & Performance](patterns/caching-performance.md) |
| **5. Security & Compliance Safeguards** | ✅ Complete | [Security & Compliance](standards/security-compliance.md), [Identity Service](architecture/services/identity-service.md) |
| **6. Mono-Repo Layer Isolation** | ✅ Complete | [Bounded Contexts](architecture/bounded-contexts.md), [LAYERS.md](../LAYERS.md) |
| **7. Tool-Assisted Development Workflow** | ✅ Complete | [Speckit Coach Guide](workflow/speckit-coach-guide.md) |

## What Belongs Here vs. Layer-Specific Docs

### ✅ Cross-Cutting (Belongs Here)
- **Infrastructure patterns**: Aspire orchestration, messaging, caching, observability that apply to ALL layers
- **Architecture patterns**: Clean Architecture, CQRS, DDD, multi-tenancy, dependency injection
- **Technical standards**: API contracts, testing strategy, security compliance, gateway configuration
- **Workflow guidance**: Spec-Kit Coach, TDD/BDD workflows, tool-assisted development
- **Service architectures**: Technical specifications for infrastructure and domain services (13 services)
- **Infrastructure scenarios**: Identity, API Gateway, Configuration service behaviors

### ❌ Layer-Specific (Belongs in Plan/{Layer}/)
- **Migration plans** → `Plan/Foundation/` (time-bound, Foundation-specific)
- **Implementation scenarios** → `Plan/Foundation/scenarios/` (domain service behaviors: Student, Staff, Assessment, etc.)
- **Layer-specific technical docs** → `Plan/DigitalInk/docs/` (DigitalInk API specs, technology stacks)
- **Feature specifications** → `Plan/Foundation/specs/` or `Plan/DigitalInk/specs/`

## How to Use Cross-Cutting Concerns

### For New Service Development

When creating a new service (in any layer):

1. **Architecture Phase**:
   - Review [Bounded Contexts](architecture/bounded-contexts.md) for domain boundaries
   - Check [Service Architectures](architecture/services/) for similar service patterns
   - Follow [Clean Architecture](patterns/clean-architecture.md) for layer structure

2. **Infrastructure Setup**:
   - Configure Aspire orchestration using [Aspire Orchestration](patterns/aspire-orchestration.md)
   - Setup dependency injection per [Dependency Injection](patterns/dependency-injection.md)
   - Implement multi-tenancy per [Multi-Tenancy](patterns/multi-tenancy.md)

3. **Integration**:
   - Configure messaging using [Messaging Integration](patterns/messaging-integration.md)
   - Add caching per [Caching & Performance](patterns/caching-performance.md)
   - Setup observability using [Observability](patterns/observability.md)

4. **Security**:
   - Implement authentication/authorization per [Security & Compliance](standards/security-compliance.md)
   - Enforce tenant isolation per [Multi-Tenant Database](patterns/multi-tenant-database.md)

5. **Testing**:
   - Follow TDD workflow in [Testing Strategy](standards/TESTING_STRATEGY.md)
   - Write Reqnroll BDD scenarios
   - Create Aspire integration tests

6. **API Design**:
   - Follow [API Contracts Specification](standards/API_CONTRACTS_SPECIFICATION.md)
   - Configure YARP routes per [API Gateway Config](standards/api-gateway-config.md)

### For Feature Development

When implementing a new feature:

1. **Specification** (via `/speckit.specify`):
   - Reference relevant patterns in `spec.md`
   - Map to constitution principles
   - Identify cross-cutting concerns

2. **Planning** (via `/speckit.plan`):
   - Include pattern implementation steps
   - Reference code examples from patterns
   - Validate against standards

3. **Implementation**:
   - Copy/adapt code from pattern documents
   - Follow anti-patterns guidance to avoid common mistakes
   - Use testing patterns from relevant docs

4. **Validation**:
   - Verify constitution compliance via [Speckit Coach](workflow/speckit-coach-guide.md)
   - Run full test suite (unit, BDD, integration, UI)
   - Check ≥80% coverage

### For AI Agents

When generating code or specifications:

1. **Context Gathering**:
   ```
   @copilot #think "What cross-cutting concerns apply to this feature?"
   @copilot #microsoft.docs.mcp search for ".NET Aspire service discovery"
   ```

2. **Pattern Application**:
   - Reference appropriate pattern documents
   - Include code samples adapted to feature context
   - Validate against anti-patterns

3. **Constitution Validation**:
   - Check feature against all 7 principles
   - Link to specific pattern documents in `spec.md`
   - Validate Figma links for UI features (new features only)

4. **Code Generation**:
   - Use patterns as templates
   - Ensure Aspire orchestration for all services
   - Include multi-tenancy and security by default

### For Architects & Reviewers

When reviewing specifications, plans, or code:

1. **Architecture Review Checklist**:
   - [ ] Clean Architecture boundaries respected
   - [ ] Aspire orchestration configured correctly
   - [ ] Multi-tenancy enforced (TenantId on all entities)
   - [ ] Event-driven integration preferred over synchronous
   - [ ] Security & compliance requirements met
   - [ ] Testing strategy includes TDD/BDD/integration
   - [ ] Observability configured (logging, tracing, metrics)

2. **Pattern Compliance**:
   - Verify patterns referenced in `spec.md`
   - Check code examples match pattern templates
   - Validate anti-patterns are avoided

3. **Constitution Compliance**:
   - Map feature to constitution principles
   - Validate non-negotiable constraints
   - Check Figma links for new UI features
   - Verify Red→Green evidence attached

## References

- [Plan/LAYERS.md](../LAYERS.md) - Mono-repo layer architecture
- [.specify/memory/constitution.md](../../../.specify/memory/constitution.md) - Constitution v2.2.0
- [Foundation Layer](../Foundation/) - Core LMS migration layer
- [DigitalInk Layer](../DigitalInk/) - Digital ink capabilities layer

---

**Last Updated**: November 20, 2025  
**Constitution Version**: 2.2.0  
**Status**: Complete - All 7 principles covered
