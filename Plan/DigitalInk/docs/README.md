# Digital Ink Technical Documentation

This directory contains detailed technical specifications for the Digital Ink Service.

## Documentation Index

### Architecture & Design

- **[domain-events.md](./domain-events.md)** - Domain event schema and integration patterns
  - Published events (InkSessionCreated, StrokesCaptured, AudioRecorded, SessionArchived)
  - Subscribed events (AssessmentAssigned, StudentWithdrawn)
  - Event versioning and backward compatibility

- **[api-specification.md](./api-specification.md)** - REST API and SignalR endpoints
  - Session management endpoints
  - Stroke and audio operations
  - Playback and export APIs
  - Teacher feedback endpoints
  - Request/response schemas
  - Error handling and status codes

- **[technology-stack.md](./technology-stack.md)** - Technology choices and implementation details
  - PostgreSQL JSONB for stroke data
  - Azure Blob Storage for PDF and audio assets
  - Avalonia/MAUI client SDK architecture
  - SignalR for real-time collaboration (future)
  - Performance optimization techniques

## Quick Reference

| Document | Purpose | Audience |
|----------|---------|----------|
| domain-events.md | Event-driven integration patterns | Backend developers, integration team |
| api-specification.md | REST API contract | Frontend developers, API consumers |
| technology-stack.md | Technology choices and rationale | Architects, developers, DevOps |

## Cross-References

- **Service Overview:** [../SERVICE_OVERVIEW.md](../SERVICE_OVERVIEW.md) - High-level architecture
- **Implementation Plan:** [../IMPLEMENTATION_PLAN.md](../IMPLEMENTATION_PLAN.md) - Phase 4a-4d roadmap
- **Scenarios:** [../scenarios/01-digital-ink-capture-playback.md](../scenarios/01-digital-ink-capture-playback.md) - Functional requirements

## Contributing

When adding new technical documentation:

1. Follow existing document structure (Purpose → Schema → Examples → Testing)
2. Include code samples and JSON schemas where applicable
3. Cross-reference related documents
4. Update this README index
5. Version major schema changes

---

**Last Updated:** November 20, 2025  
**Version:** 1.0
