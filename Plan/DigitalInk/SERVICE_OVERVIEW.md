# Digital Ink Service Overview

**Version:** 1.0  
**Last Updated:** November 20, 2025  
**Service Name:** Digital Ink Service  
**Service Type:** Greenfield microservice (no legacy migration)

---

## Purpose

The Digital Ink Service enables **high-fidelity stylus input capture** and **synchronized audio recording** for student assessments, providing rich, multimodal data for teacher review and AI/LLM analysis.

**Business Value:**
- Modern assessment capture beyond text and multiple-choice
- AI-ready time-series data for handwriting and reasoning analysis
- Rich teacher-student feedback with annotation and playback
- Offline-first support for real-world classroom tablet use

---

## Architecture

### Clean Architecture Layers

```
DigitalInk.API/                # Presentation Layer
├── Controllers/
│   ├── InkSessionsController.cs
│   ├── StrokesController.cs
│   └── PlaybackController.cs
├── SignalR/
│   └── InkSessionHub.cs       # Real-time stroke sync
├── Middleware/
│   ├── TenantMiddleware.cs
│   └── ExceptionHandlingMiddleware.cs
└── Program.cs

DigitalInk.Application/         # Application Layer
├── Commands/
│   ├── CreateInkSession/
│   │   ├── CreateInkSessionCommand.cs
│   │   ├── CreateInkSessionCommandHandler.cs
│   │   └── CreateInkSessionCommandValidator.cs
│   ├── SaveStrokeBatch/
│   ├── UploadAudio/
│   └── ArchiveSession/
├── Queries/
│   ├── GetInkSession/
│   ├── GetPlaybackData/
│   └── ExportLLMFormat/
├── DTOs/
│   ├── InkSessionDto.cs
│   ├── StrokeDto.cs
│   └── PlaybackDataDto.cs
├── Interfaces/
│   ├── IInkSessionRepository.cs
│   ├── IBlobStorageService.cs
│   └── IEventPublisher.cs
└── Services/
    ├── PlaybackService.cs     # Timeline synchronization
    └── ExportService.cs       # LLM data format

DigitalInk.Domain/              # Domain Layer
├── Entities/
│   ├── InkSession.cs
│   ├── Stroke.cs
│   ├── Page.cs
│   ├── AudioAsset.cs
│   └── FeedbackAnnotation.cs
├── Events/
│   ├── InkSessionCreatedEvent.cs
│   ├── StrokesCapturedEvent.cs
│   ├── AudioRecordedEvent.cs
│   └── SessionArchivedEvent.cs
├── ValueObjects/
│   ├── Point.cs               # [x, y, pressure, timestamp]
│   ├── ToolType.cs            # Pen, Highlighter, Eraser
│   └── StrokeData.cs          # JSONB structure
└── Aggregates/
    └── InkSessionAggregate.cs # Root aggregate

DigitalInk.Infrastructure/      # Infrastructure Layer
├── Data/
│   ├── InkDbContext.cs
│   ├── Repositories/
│   │   ├── InkSessionRepository.cs
│   │   └── FeedbackRepository.cs
│   ├── Configurations/
│   │   ├── InkSessionConfiguration.cs
│   │   └── FeedbackConfiguration.cs
│   └── Migrations/
├── Storage/
│   └── AzureBlobStorageService.cs
├── Integration/
│   ├── EventPublisher.cs      # MassTransit
│   └── Consumers/
│       ├── AssessmentAssignedConsumer.cs
│       └── StudentWithdrawnConsumer.cs
└── SignalR/
    └── StrokeSyncService.cs   # Future: real-time collaboration
```

---

## Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **API** | ASP.NET Core 10 | REST endpoints, SignalR |
| **Data Access** | EF Core 9 + PostgreSQL | JSONB stroke storage |
| **File Storage** | Azure Blob Storage | PDF backgrounds, audio files |
| **Messaging** | MassTransit + Azure Service Bus | Domain events |
| **Real-Time** | SignalR | Future: live stroke collaboration |
| **Orchestration** | .NET Aspire | Service hosting and discovery |
| **Validation** | FluentValidation | Command/query validation |
| **Testing** | xUnit, Reqnroll, Playwright | Unit, BDD, UI tests |

See [docs/technology-stack.md](./docs/technology-stack.md) for detailed specifications.

---

## Database Schema

**Database:** `NorthStar_DigitalInk_DB` (PostgreSQL 16)

### Tables

**InkSessions**
```sql
CREATE TABLE ink_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    owner_id UUID NOT NULL,           -- StudentId
    entity_id UUID NOT NULL,          -- AssignmentId
    entity_type VARCHAR(50) NOT NULL, -- "Assignment"
    background_asset_url TEXT,        -- SAS URL to PDF
    audio_asset_url TEXT,             -- SAS URL to audio file
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    is_archived BOOLEAN DEFAULT FALSE,
    stroke_data JSONB NOT NULL DEFAULT '{}'::jsonb,
    CONSTRAINT fk_tenant FOREIGN KEY (tenant_id) REFERENCES tenants(id)
);

CREATE INDEX idx_ink_sessions_tenant ON ink_sessions(tenant_id);
CREATE INDEX idx_ink_sessions_entity ON ink_sessions(entity_id);
CREATE INDEX idx_ink_sessions_owner ON ink_sessions(owner_id);
CREATE INDEX idx_ink_sessions_stroke_data ON ink_sessions USING GIN (stroke_data);
```

**SessionMetadata** (for analytics)
```sql
CREATE TABLE session_metadata (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    session_id UUID NOT NULL REFERENCES ink_sessions(id),
    total_strokes INT DEFAULT 0,
    total_pages INT DEFAULT 1,
    audio_duration_ms INT DEFAULT 0,
    last_synced_at TIMESTAMPTZ
);
```

**FeedbackAnnotations** (teacher strokes)
```sql
CREATE TABLE feedback_annotations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    session_id UUID NOT NULL REFERENCES ink_sessions(id),
    teacher_id UUID NOT NULL,
    feedback_stroke_data JSONB NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
```

### JSONB Stroke Data Structure

```json
{
  "pages": [
    {
      "pageNumber": 1,
      "strokes": [
        {
          "strokeId": "550e8400-e29b-41d4-a716-446655440000",
          "tool": "pen",
          "color": "#000000",
          "width": 2.0,
          "points": [
            [10.5, 20.3, 0.75, 0],      // [x, y, pressure, timestamp_ms]
            [11.2, 21.1, 0.80, 16],
            [12.8, 22.5, 0.78, 32]
          ],
          "deleted": false
        }
      ],
      "audioStartOffset": 1500  // ms into audio recording
    }
  ],
  "audioMetadata": {
    "durationMs": 45000,
    "sampleRate": 44100,
    "format": "webm"
  }
}
```

---

## Service Boundaries

### Owned Responsibilities

✅ **Digital Ink Service owns:**
- Ink session lifecycle (create, update, archive)
- High-fidelity stroke data capture (x, y, pressure, timestamp)
- Synchronized audio recording and playback
- PDF background asset management
- Multi-page document support
- Stroke-level undo/redo
- Teacher annotation and feedback overlay
- Playback synchronization (audio + ink)
- LLM-ready data export
- Offline-first data sync
- Cold storage archival for old sessions

### Not Owned (Delegated)

❌ **Digital Ink Service does NOT own:**
- Assignment/assessment creation → **Assessment Service**
- Student enrollment and demographics → **Student Management Service**
- Grading and scoring → **Assessment Service**
- Access control and authentication → **Identity Service**
- Reporting and analytics → **Reporting Service**

### Integration Pattern

**Lightweight, Event-Driven Service:**
- Publishes domain events when sessions are created/completed
- Subscribes to assessment and student events
- Other services query Digital Ink Service for playback data
- Synchronous calls only for session creation context (assignment details)

---

## Domain Events

See [docs/domain-events.md](./docs/domain-events.md) for complete schema definitions.

### Published Events

| Event | When | Consumers |
|-------|------|-----------|
| `InkSessionCreatedEvent` | New session starts | Reporting Service |
| `StrokesCapturedEvent` | Strokes saved (batch) | Reporting Service (usage analytics) |
| `AudioRecordedEvent` | Audio uploaded | Reporting Service (multimedia metrics) |
| `SessionArchivedEvent` | Session archived | Data Retention Service |

### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `AssessmentAssignedEvent` | Assessment Service | Enable ink session creation |
| `StudentWithdrawnEvent` | Student Service | Archive student's sessions |

---

## API Endpoints

See [docs/api-specification.md](./docs/api-specification.md) for complete API documentation.

### Endpoint Summary

**Session Management:**
- `POST /api/v1/ink/sessions` - Create session, returns session ID + SAS URL
- `GET /api/v1/ink/sessions/{id}` - Get session details
- `PUT /api/v1/ink/sessions/{id}` - Update session metadata
- `DELETE /api/v1/ink/sessions/{id}` - Archive session (soft delete)

**Stroke Operations:**
- `PUT /api/v1/ink/sessions/{id}/strokes` - Save stroke batch (append to JSONB)
- `DELETE /api/v1/ink/sessions/{id}/strokes/{strokeId}` - Delete stroke (undo)
- `GET /api/v1/ink/sessions/{id}/pages/{pageNum}/strokes` - Get page strokes

**Audio Operations:**
- `POST /api/v1/ink/sessions/{id}/audio` - Upload audio file
- `GET /api/v1/ink/sessions/{id}/audio` - Get audio SAS URL

**Playback & Export:**
- `GET /api/v1/ink/sessions/{id}/playback` - Get synchronized timeline
- `GET /api/v1/ink/sessions/{id}/export` - Export LLM format (JSON)

**Teacher Feedback:**
- `POST /api/v1/ink/sessions/{id}/feedback` - Add teacher annotation
- `GET /api/v1/ink/sessions/{id}/feedback` - Get feedback overlay

---

## Service Level Objectives (SLOs)

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Availability** | 99.9% uptime | Azure Monitor alerts |
| **Create Session** | p95 < 100ms | Application Insights |
| **Save Stroke Batch (100 strokes)** | p95 < 50ms | Application Insights |
| **Audio Upload (10MB file)** | p95 < 5s | Blob Storage metrics |
| **Get Playback Data** | p95 < 200ms | Application Insights |
| **Export LLM Format** | p95 < 1s | Application Insights |

---

## Security & Compliance

### FERPA Compliance

- All student work data encrypted at rest (PostgreSQL + Blob Storage)
- Access logs maintained for all session queries
- Data retention policies enforced (7 years)
- Tenant isolation via RLS and query filters

### Authentication & Authorization

- JWT tokens validated via Identity Service
- Students can only access their own sessions
- Teachers can view sessions for enrolled students only
- SAS tokens with 15-minute expiration for Blob Storage URLs
- Rate limiting: 100 requests/min per user

### Data Protection

- Secrets stored in Azure Key Vault only
- TLS 1.3 for all API communication
- Blob Storage with private endpoints
- Audit logging for all mutations

---

## Dependencies

### External Services

| Service | Purpose | Latency Budget |
|---------|---------|----------------|
| **Identity Service** | JWT validation, user context | 50ms (cached) |
| **Assessment Service** | Assignment details | 100ms |
| **Student Management Service** | Student validation | 100ms |
| **Azure Blob Storage** | PDF/audio assets | 200ms (upload), 50ms (SAS token) |
| **Azure Service Bus** | Event publishing | Asynchronous |

### Infrastructure Dependencies

- **PostgreSQL 16** - Session and stroke data (JSONB)
- **.NET Aspire AppHost** - Service orchestration and discovery
- **Azure Application Insights** - Monitoring and telemetry

---

## Client SDK Architecture

**Avalonia/MAUI Client Components:**

```
Client.UI/
├── InkCanvas.cs              # Custom SkiaSharp control
├── InkCaptureService.cs      # Stroke capture logic
├── AudioRecordingService.cs  # NAudio wrapper
├── OfflineSyncService.cs     # Background sync queue
└── PlaybackController.cs     # Timeline synchronization

Client.Data/
├── InkSessionRepository.cs   # HTTP client wrapper
├── OfflineStorage.cs         # IndexedDB/SQLite
└── Models/
    ├── StrokePoint.cs
    └── InkSessionModel.cs
```

See [docs/technology-stack.md](./docs/technology-stack.md) for client SDK details.

---

## Testing Strategy

### Test Coverage Requirements

- **Unit Tests:** ≥80% coverage (Application + Domain layers)
- **Integration Tests:** All repository and Blob Storage operations
- **BDD Tests:** All 12 scenarios from [scenarios/01-digital-ink-capture-playback.md](./scenarios/01-digital-ink-capture-playback.md)
- **UI Tests:** Playwright for canvas interaction and playback

### Test Artifacts (Per Phase)

- `phase-{4a|4b|4c|4d}-red-dotnet-test.txt` - BEFORE implementation
- `phase-{4a|4b|4c|4d}-green-dotnet-test.txt` - AFTER implementation
- `phase-{4a|4b|4c|4d}-red-playwright.txt` - UI tests BEFORE
- `phase-{4a|4b|4c|4d}-green-playwright.txt` - UI tests AFTER

---

## Performance Considerations

### Optimization Strategies

1. **JSONB Indexing:** GIN index on `stroke_data` for fast queries
2. **Stroke Batching:** Client sends 100-stroke batches (not per-stroke)
3. **CDN for PDFs:** Azure CDN in front of Blob Storage (future)
4. **Redis Caching:** Session metadata cached (5-minute TTL)
5. **Blob Streaming:** Large audio files streamed, not buffered

### Scalability

- **Horizontal Scaling:** Stateless API layer, scale via Aspire replicas
- **Database Sharding:** Future: shard by `tenant_id` if needed
- **Cold Storage:** Archive old sessions to Blob Archive tier

---

## Related Documentation

- **Implementation Plan:** [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md)
- **Scenarios:** [scenarios/01-digital-ink-capture-playback.md](./scenarios/01-digital-ink-capture-playback.md)
- **Domain Events:** [docs/domain-events.md](./docs/domain-events.md)
- **API Specification:** [docs/api-specification.md](./docs/api-specification.md)
- **Technology Stack:** [docs/technology-stack.md](./docs/technology-stack.md)

---

**Last Updated:** November 20, 2025  
**Version:** 1.0  
**Owner:** Digital Ink Team Lead
