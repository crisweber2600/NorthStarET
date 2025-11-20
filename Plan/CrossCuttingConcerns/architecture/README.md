# Architecture Patterns

Cross-cutting architecture patterns and principles used across all NorthStarET mono-repo layers.

## Contents

### Domain-Driven Design
- [Bounded Contexts](bounded-contexts.md) - DDD analysis of 11+ bounded contexts, context maps, and integration patterns

### Event-Driven Architecture
- [Domain Events Schema](domain-events-schema.md) - Standardized event schema, naming conventions, and MassTransit integration

### Service Architecture Specifications
See [services/](services/) for technical architecture specifications of all microservices:
- Identity & Authentication Service
- API Gateway (YARP)
- Configuration Service
- System Operations Service
- Student Management Service
- Staff Management Service
- Assessment Service
- Intervention Management Service
- Section & Roster Service
- Data Import & Integration Service
- Reporting & Analytics Service
- Content & Media Service
- Digital Ink Service

## Usage

These architecture patterns are referenced by:
- **Foundation Layer**: Core LMS modernization services
- **DigitalInk Layer**: Stylus input and audio recording capabilities
- **Future Layers**: Analytics, Collaboration, Mobile, etc.

All layers should follow these patterns to maintain consistency and interoperability.
