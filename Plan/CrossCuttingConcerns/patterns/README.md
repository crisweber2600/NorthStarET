# Cross-Cutting Architectural Patterns

Infrastructure and architectural patterns that apply across ALL NorthStarET mono-repo layers (Foundation, DigitalInk, future layers).

## Purpose

Patterns provide reusable solutions to common architectural challenges:
- **Infrastructure patterns**: Aspire orchestration, messaging, caching, observability
- **Architecture patterns**: Clean Architecture, CQRS, multi-tenancy, dependency injection
- **Security patterns**: Authentication, authorization, data protection, compliance

## Pattern Catalog

### Infrastructure Orchestration
| Pattern | Priority | Constitution Principle | Status |
|---------|----------|------------------------|--------|
| [Aspire Orchestration](aspire-orchestration.md) | 🔴 Critical | Principle 1 | ✅ Complete |
| [Observability](observability.md) | 🟡 Medium | Principle 1 | ✅ Complete |

### Architecture & Design
| Pattern | Priority | Constitution Principle | Status |
|---------|----------|------------------------|--------|
| [Clean Architecture](clean-architecture.md) | 🟠 High | Principle 1 | ✅ Complete |
| [Dependency Injection](dependency-injection.md) | 🟠 High | Principle 1 | ✅ Complete |

### Data & Persistence
| Pattern | Priority | Constitution Principle | Status |
|---------|----------|------------------------|--------|
| [Multi-Tenant Database](multi-tenant-database.md) | 🟠 High | Principle 4 | ✅ Complete |
| [Multi-Tenancy Patterns](multi-tenancy.md) | 🟠 High | Principle 4 | ✅ Complete |
| [Caching & Performance](caching-performance.md) | 🟡 Medium | Principle 4 | ✅ Complete |

### Integration & Messaging
| Pattern | Priority | Constitution Principle | Status |
|---------|----------|------------------------|--------|
| [Messaging & Integration](messaging-integration.md) | 🟠 High | Principle 4 | ✅ Complete |

### Security & Compliance
| Pattern | Priority | Constitution Principle | Status |
|---------|----------|------------------------|--------|
| [Security & Compliance](../standards/security-compliance.md) | 🔴 Critical | Principle 5 | ✅ Complete |

## Usage

### For Architects
- **Design Validation**: Ensure new services follow established patterns
- **Pattern Selection**: Choose appropriate patterns for specific challenges
- **Architecture Reviews**: Reference patterns in review checklists

### For Developers
- **Implementation Guidance**: Use patterns as templates for new services
- **Code Examples**: Copy/adapt code samples from pattern documents
- **Anti-Patterns**: Avoid documented anti-patterns

### For AI Agents
- **Constitution Compliance**: Validate implementations against patterns
- **Code Generation**: Use patterns to generate compliant code
- **Spec Validation**: Check specs reference appropriate patterns

## Pattern Structure

Each pattern document includes:
1. **Overview**: Purpose, context, and when to use
2. **Code Examples**: C#, SQL, configuration samples
3. **Anti-Patterns**: Common mistakes to avoid
4. **Testing Patterns**: How to test pattern implementation
5. **References**: Links to constitution, service architectures, scenarios

## Related Documentation

- [Service Architectures](../architecture/services/) - Service-specific technical specs
- [Standards](../standards/) - API contracts, testing, security standards
- [Scenarios](../scenarios/) - Infrastructure service behavior specifications
- [Constitution](../../../.specify/memory/constitution.md) - Core principles and constraints

## Pattern Lifecycle

### Creating New Patterns
1. Identify recurring architectural challenge
2. Document solution with code examples
3. Validate against constitution principles
4. Review with architecture team
5. Reference in related service architectures

### Updating Patterns
1. Track implementation feedback
2. Update with lessons learned
3. Version changes (semantic versioning)
4. Communicate to development team
5. Update cross-references

---

**Last Updated**: November 20, 2025  
**Constitution Version**: 2.2.0
