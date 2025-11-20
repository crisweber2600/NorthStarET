# Digital Ink Service

## Overview

The Digital Ink Service provides the backend infrastructure for capturing, storing, and replaying high-fidelity digital handwriting (ink) and synchronized audio. It enables teachers and students to annotate PDF documents (assignments, assessments) using a stylus, preserving the temporal and pressure data required for future AI analysis and "movie-mode" playback.

## Service Classification

- **Type**: Supporting Service
- **Phase**: Phase 4 (Weeks 23-28)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/DigitalInk/`
- **Priority**: Medium (Enhancement for iPad/Tablet users)
- **LMS Role**: Rich media capture for student work and teacher feedback

## Current State (Legacy)

**Status**: **New Capability**
- No equivalent service exists in the legacy NorthStar 4.6 monolith.
- Current system relies on standard file uploads without interactive annotation.

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
DigitalInk.API/                  # UI Layer (REST endpoints)
├── Controllers/
│   ├── SessionsController.cs
│   ├── StrokesController.cs
│   └── AudioController.cs
├── Middleware/
└── Program.cs

DigitalInk.Application/          # Application Layer
├── Commands/
│   ├── CreateInkSession/
│   ├── SaveStrokeData/
│   └── LinkAudioRecording/
├── Queries/
│   ├── GetSessionById/
│   └── GetSessionsByEntity/
├── DTOs/
└── Interfaces/

DigitalInk.Domain/              # Domain Layer
├── Entities/
│   ├── InkSession.cs
│   └── StrokeBatch.cs
├── ValueObjects/
│   ├── StrokePoint.cs
│   └── CanvasDimensions.cs
├── Events/
│   ├── InkSessionCreatedEvent.cs
│   └── InkSessionCompletedEvent.cs
└── Aggregates/
    └── InkSessionAggregate.cs

DigitalInk.Infrastructure/      # Infrastructure Layer
├── Data/
│   ├── DigitalInkDbContext.cs (PostgreSQL)
│   └── Repositories/
├── Storage/
│   └── BlobStorageService.cs (Azure Blobs)
└── MessageBus/
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Data Access**: EF Core with **PostgreSQL** (using `JSONB` for stroke data)
- **File Storage**: Azure Blob Storage (PDF backgrounds, Audio files)
- **Messaging**: MassTransit + Azure Service Bus
- **Orchestration**: .NET Aspire hosting

### Owned Data

**Database**: `NorthStar_DigitalInk_DB` (PostgreSQL)

**Tables**:
- `ink_sessions` (Id, TenantId, OwnerId, EntityId, EntityType, BackgroundAssetUrl, AudioAssetUrl, CreatedAt)
- `stroke_data` (SessionId, JsonData) - *Stored as JSONB for flexibility and analytics*

### Service Boundaries

**Owned Responsibilities**:
- Managing the lifecycle of an "Ink Session" (start, update, complete).
- Storing raw vector data (X, Y, Pressure, Timestamp) for strokes.
- Managing links to binary assets (PDF backgrounds, Audio recordings).
- Providing playback-ready data structures to clients.

**Not Owned**:
- **User Identity**: Validated via Identity Service tokens.
- **Entity Management**: The service doesn't know what an "Assignment" is, only that it has an `EntityId`.
- **File Hosting**: Actual binaries reside in Blob Storage; this service manages the URLs/SAS tokens.

### Domain Events Published

- `InkSessionCreatedEvent`
  - Triggered when a user opens a document for annotation.
- `InkSessionUpdatedEvent`
  - Triggered when new strokes or audio are saved.
- `InkSessionCompletedEvent`
  - Triggered when the user submits the work.

### Domain Events Subscribed

- `StudentWithdrawnEvent`: To archive/cleanup ink data for withdrawn students.
- `AssignmentDeletedEvent`: To cleanup orphaned ink sessions.

### API Functional Intent

- **POST /sessions**: Initialize a new annotation session.
- **PUT /sessions/{id}/strokes**: Append or replace stroke data (JSON).
- **POST /sessions/{id}/audio**: Link an uploaded audio file to the session.
- **GET /sessions/{id}**: Retrieve full session data for playback.
- **GET /sessions?entityId={id}**: List all sessions for a specific assignment/assessment.

### Service Level Objectives (SLOs)

- **Write Latency**: < 100ms for saving stroke batches.
- **Read Latency**: < 200ms for loading a full session.
- **Data Integrity**: 100% preservation of timestamp and pressure data.

### Security & Compliance

- **Authorization**:
  - Users can only access sessions they own (Students) or manage (Teachers).
  - Tenant isolation enforced at the database query level.
- **Data Privacy**:
  - Audio recordings are sensitive student data (FERPA).
  - Access to Blob Storage is secured via short-lived SAS tokens.

### Testing Requirements

- **Unit Tests**: Validate JSON serialization/deserialization of stroke data.
- **Integration Tests**: Verify PostgreSQL JSONB queries and Blob Storage linking.
- **Load Tests**: Simulate concurrent uploads of audio and stroke data from a classroom of students.

### Migration Strategy

- **Greenfield**: Since this is a new service, no data migration is required.
- **Integration**: Will be integrated into the new Avalonia client and potentially a web-based canvas in the future.

### Dependencies

- **Identity Service**: For JWT validation.
- **Content & Media Service**: For uploading the initial PDF backgrounds.
- **Azure Blob Storage**: For storing large assets.

### Implementation Checklist

- [ ] Scaffold `DigitalInk.API` with PostgreSQL support.
- [ ] Implement `InkSession` aggregate and JSONB mapping.
- [ ] Create Azure Blob Storage integration for SAS token generation.
- [ ] Implement REST endpoints for session management.
- [ ] Develop Avalonia client library for capturing ink (separate task).
