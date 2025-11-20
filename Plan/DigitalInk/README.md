# Digital Ink Layer Planning

This directory contains planning documentation, architecture decisions, and implementation scenarios for the **Digital Ink Layer** of the NorthStar LMS mono-repo.

## Purpose

The **DigitalInk** directory serves as the strategic planning hub for the Digital Ink Service - a greenfield capability enabling high-fidelity stylus input, synchronized audio recording, and AI-ready data capture for student assessments.

This layer represents a **distinct microservice domain** separate from the Foundation layer's LMS modernization work. It contains:

- Implementation roadmaps and phasing strategies
- Service architecture and domain modeling
- Technical specifications for ink capture and playback
- Implementation scenarios with Given-When-Then patterns
- Integration guides and client SDK documentation

**Note:** For detailed feature specifications following Speckit workflows, see the `/specs` directory at repository root.

## Directory Structure

```
Plan/DigitalInk/
├── README.md                              # This file
├── IMPLEMENTATION_PLAN.md                 # ⭐ Phase 4a-4d implementation roadmap
├── SERVICE_OVERVIEW.md                    # Digital Ink Service architecture
│
├── docs/                                  # Technical documentation
│   ├── README.md                         # Technical docs index
│   ├── domain-events.md                  # Event schema and integration patterns
│   ├── api-specification.md              # REST API and SignalR endpoints
│   └── technology-stack.md               # PostgreSQL JSONB, Azure Blob, client SDKs
│
└── scenarios/                             # Implementation scenarios
    ├── README.md                         # Scenarios overview
    └── 01-digital-ink-capture-playback.md # 12 Given-When-Then scenarios
```

## Quick Links

### Planning & Architecture
- ⭐ [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) - **Start here** - Phase 4a-4d roadmap with dependency gates
- [SERVICE_OVERVIEW.md](./SERVICE_OVERVIEW.md) - Service architecture, boundaries, and domain model
- [scenarios/01-digital-ink-capture-playback.md](./scenarios/01-digital-ink-capture-playback.md) - Complete scenarios with Architectural Appendix

### Technical Documentation
- [docs/domain-events.md](./docs/domain-events.md) - Domain events schema and integration patterns
- [docs/api-specification.md](./docs/api-specification.md) - REST API endpoints and SignalR hubs
- [docs/technology-stack.md](./docs/technology-stack.md) - JSONB storage, Azure Blob, Avalonia/MAUI client SDKs

### General Architecture (Repository Root)
- [Bounded Contexts](../../docs/architecture/bounded-contexts.md) - DDD analysis (cross-layer)
- [Digital Ink Service Architecture](../../docs/architecture/services/digital-ink-service.md) - Technical specification
- [API Standards](../../docs/standards/API_CONTRACTS_SPECIFICATION.md) - RESTful API patterns
- [Testing Strategy](../../docs/standards/TESTING_STRATEGY.md) - TDD/BDD/Playwright approach

## Layer Context

### Mono-Repo Position

The Digital Ink Layer is one of multiple independent layers in the NorthStar mono-repo:

- **Foundation Layer** (`Src/Foundation/`, `Plan/Foundation/`) - Core LMS modernization (11 microservices)
- **DigitalInk Layer** (`Src/DigitalInk/`, `Plan/DigitalInk/`) - Specialized ink capture and playback service
- **Future Layers** - Additional specialized domains as needed

See [Plan/LAYERS.md](../LAYERS.md) for complete mono-repo layer architecture.

### Dependencies

**Digital Ink Service depends on Foundation Layer services:**
- **Identity Service** - Authentication and authorization (session tokens)
- **Assessment Service** - Assignment context for ink sessions
- **Student Management Service** - Student validation and enrollment context

**Dependency Gate:** Foundation Phase 3 (Assessment Service) must be **production-ready** before Digital Ink Phase 4a implementation begins.

### Scope

**What Digital Ink Layer Owns:**
- High-fidelity stroke data capture (x, y, pressure, timestamp)
- Synchronized audio recording and playback
- PDF background rendering and multi-page navigation
- Teacher annotation and feedback overlays
- Offline-first data synchronization
- LLM-ready data export formats
- Session lifecycle management and archival

**What Digital Ink Layer Does NOT Own:**
- Assignment creation and distribution → Assessment Service
- Student enrollment and demographics → Student Management Service
- Grading and scoring logic → Assessment Service
- Access control and authentication → Identity Service
- Reporting and analytics → Reporting Service

## Development Status

**Current Phase:** Specification and planning  
**Target Start:** Week 23 (after Foundation Phase 3 completion)  
**Target Completion:** Week 28 (Phase 4d)

**Recent Updates:**
- 2025-11-20: Scenario 01 completed with full Architectural Appendix (12 Given-When-Then scenarios)
- 2025-11-20: Digital Ink layer planning documentation created and separated from Foundation layer

## Getting Started

1. **Review the Implementation Plan**: Start with [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) to understand the phased rollout strategy
2. **Understand Service Boundaries**: Read [SERVICE_OVERVIEW.md](./SERVICE_OVERVIEW.md) for architecture and domain model
3. **Explore Scenarios**: Review [scenarios/01-digital-ink-capture-playback.md](./scenarios/01-digital-ink-capture-playback.md) for functional requirements
4. **Check Dependencies**: Verify Foundation Phase 3 completion before starting Digital Ink implementation
5. **Consult Technical Docs**: Reference `docs/` for detailed technical specifications

## Related Documentation

- **Constitution**: `.specify/memory/constitution.md` - Project-wide development standards
- **Copilot Instructions**: `.github/copilot-instructions.md` - AI agent coding guidelines
- **Layer Architecture**: `Plan/LAYERS.md` - Mono-repo layer definitions
- **Foundation Plans**: `Plan/Foundation/` - Core LMS modernization plans (dependencies)
- **Service Catalog**: `Plan/Foundation/SERVICE_CATALOG.md` - Foundation services overview

---

**Last Updated:** November 20, 2025  
**Version:** 1.0  
**Status:** Active Planning Phase
