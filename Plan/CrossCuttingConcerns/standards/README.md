# Technical Standards

Cross-cutting technical standards and conventions applied across all NorthStarET services and layers.

## Contents

### API Standards
- [API Contracts Specification](API_CONTRACTS_SPECIFICATION.md) - RESTful API patterns, versioning, OpenAPI, error schemas, pagination

### Testing Standards
- [Testing Strategy](TESTING_STRATEGY.md) - TDD Red→Green→Refactor, test pyramid, ≥80% coverage requirements, BDD with Reqnroll, Playwright UI testing

### Infrastructure Standards
- [API Gateway Configuration](api-gateway-config.md) - YARP configuration, routing patterns, rate limiting, Strangler Fig migration pattern

### Security & Compliance Standards
- [Security & Compliance](security-compliance.md) - Authentication (Entra ID), authorization (RBAC), multi-tenant isolation, FERPA compliance, secret management, audit logging

## Usage

All services across all layers (Foundation, DigitalInk, future) MUST comply with these standards:
- **API design**: Follow RESTful conventions in API_CONTRACTS_SPECIFICATION.md
- **Testing approach**: Implement TDD/BDD/Playwright per TESTING_STRATEGY.md
- **Gateway integration**: Configure YARP routes per api-gateway-config.md
- **Security**: Implement authentication/authorization per security-compliance.md
- **Compliance**: Enforce FERPA compliance and audit logging per security-compliance.md

## Compliance

These standards are enforced through:
- **Constitution v2.2.0**: 
  - Principle 1 (Clean Architecture & Aspire Orchestration)
  - Principle 2 (Test-Driven Quality Gates)
  - Principle 3 (UX Traceability & Figma Accountability)
  - Principle 5 (Security & Compliance Safeguards)
- **CI/CD pipelines**: Build warnings as errors, ≥80% coverage gates, Playwright test execution, security scanning
- **Code reviews**: Phase review branches verify compliance before merge
- **Architecture reviews**: Security and compliance validation for sensitive changes

## References
- [Architecture Patterns](../architecture/) - Bounded contexts, event schemas
- [Patterns](../patterns/) - Aspire orchestration, multi-tenancy, messaging, caching
- [Workflow Guidance](../workflow/) - Spec-Kit Coach development workflows
- [Constitution](../../../.specify/memory/constitution.md) - v2.2.0 Core principles
