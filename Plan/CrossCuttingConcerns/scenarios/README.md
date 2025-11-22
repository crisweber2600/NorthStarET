# Cross-Cutting Infrastructure Scenarios

This directory contains comprehensive Given-When-Then (Gherkin-style) scenarios for infrastructure services and patterns that are used across ALL mono-repo layers (Foundation, DigitalInk, future layers).

## Purpose

These scenario files provide a **business and technical view** into infrastructure capabilities, making it easy for stakeholders to understand:
- How foundational services operate
- How cross-cutting patterns are applied
- What integration points exist between services
- How infrastructure supports business functionality

## Scenario Files

### 01. Identity Service: Microsoft Entra ID Authentication
**File**: [01-identity-service.md](./01-identity-service.md)  
**Scenarios**: 10  
**Coverage**:
- Staff member SSO login
- Administrator MFA authentication
- User provisioning and legacy migration
- Token refresh and session management
- Cross-district access with tenant switching
- Password reset via Entra ID
- Role-based authorization
- Session termination and logout
- Service-to-service authentication
- Failed authentication handling

**Key Business Value**: Modern cloud authentication, reduced maintenance, enterprise SSO  
**Related Architecture**: [Identity Service Architecture](../architecture/services/identity-service.md)

---

### 02. API Gateway: YARP Service Orchestration
**File**: [02-api-gateway.md](./02-api-gateway.md)  
**Scenarios**: 12  
**Coverage**:
- Route requests to new microservices
- Route requests to legacy monolith (Strangler Fig)
- Authentication validation at gateway
- Rate limiting per tenant
- CORS policy management
- Request logging and correlation IDs
- Health check aggregation
- Circuit breaker for failing services
- Request transformation and header injection
- API versioning support
- Load balancing across service instances
- Request size limits and validation

**Key Business Value**: Unified entry point, authentication enforcement, traffic management  
**Related Architecture**: [API Gateway Configuration](../standards/api-gateway-config.md)

---

### 03. Configuration Service: Multi-Tenant Settings
**File**: [03-configuration-service.md](./03-configuration-service.md)  
**Scenarios**: 12  
**Coverage**:
- District administrator creates district settings
- Configure academic calendar
- Create schools within district
- Configure grade levels and subjects
- Multi-tenant configuration isolation
- System-wide vs. district-specific settings
- Custom attributes and fields
- Grading scale configuration
- State-specific compliance settings
- Navigation menu customization
- Notification and email templates
- Configuration change audit trail

**Key Business Value**: Centralized multi-tenant configuration, district customization  
**Related Architecture**: [Configuration Service Architecture](../architecture/services/configuration-service.md)

---

## Related Patterns

These scenarios demonstrate the practical application of cross-cutting patterns:

- **[Multi-Tenant Database](../patterns/multi-tenant-database.md)** - Database-per-service with RLS isolation
- **[Multi-Tenancy](../patterns/multi-tenancy.md)** - Application-level tenant isolation
- **[Clean Architecture](../patterns/clean-architecture.md)** - Domain-driven design boundaries
- **[Aspire Orchestration](../patterns/aspire-orchestration.md)** - Service discovery and hosting
- **[Messaging Integration](../patterns/messaging-integration.md)** - Event-driven communication
- **[Caching & Performance](../patterns/caching-performance.md)** - Redis caching strategies
- **[Observability](../patterns/observability.md)** - Logging, tracing, metrics

---

## Usage Guidelines

### For Developers
Use these scenarios to:
1. Understand how infrastructure services operate
2. Learn integration patterns between services
3. Validate technical implementation against business requirements
4. Create Reqnroll BDD tests based on scenarios

### For Architects
Use these scenarios to:
1. Validate architectural decisions
2. Identify cross-cutting concerns
3. Document service boundaries and contracts
4. Plan integration strategies

### For Product Owners
Use these scenarios to:
1. Understand infrastructure capabilities
2. Validate business value delivery
3. Communicate technical capabilities to stakeholders
4. Plan feature dependencies on infrastructure

---

## Scenario Writing Standards

All scenarios follow Gherkin-style Given-When-Then format:

```gherkin
Given [precondition/context]
And [additional context]
When [action/event]
And [additional action]
Then [expected outcome]
And [additional outcome]
And [performance/security requirement]
```

**Key Characteristics**:
- **Business-focused language**: Avoid technical jargon where possible
- **Testable**: Each scenario can be automated via Reqnroll or Playwright
- **Performance-aware**: Include SLO expectations where relevant
- **Security-conscious**: Highlight tenant isolation and authorization
- **Observable**: Include logging and monitoring expectations

---

## Related Documentation

**Architecture**:
- [Service Architecture Catalog](../architecture/services/) - Technical specs for all services
- [Domain Events Schema](../architecture/domain-events-schema.md) - Event definitions
- [Bounded Contexts](../architecture/bounded-contexts.md) - DDD analysis

**Standards**:
- [API Contracts Specification](../standards/API_CONTRACTS_SPECIFICATION.md) - RESTful API patterns
- [Testing Strategy](../standards/TESTING_STRATEGY.md) - TDD/BDD/Playwright approach
- [Security & Compliance](../standards/security-compliance.md) - FERPA, RBAC

**Workflow**:
- [Speckit Coach Guide](../workflow/speckit-coach-guide.md) - Spec-driven development

---

**Last Updated**: November 20, 2025  
**Version**: 1.0.0  
**Maintainer**: NorthStarET Architecture Team
