# Digital Ink Service Scenarios

**Feature**: Digital Ink and Audio Capture  
**Epic**: Phase 4 - Supporting Services  
**Service**: Digital Ink Service  
**Business Value**: Enables rich, interactive student assessment and feedback with AI-ready data capture.

---

## Scenario 1: Student Starts New Ink Session on Assignment

**Given** a student "Liam" opens an assignment "Math Worksheet 1"  
**And** the assignment has a PDF background  
**When** Liam taps "Start Annotation"  
**Then** the Digital Ink Service creates a new `InkSession` linked to Liam and the Assignment  
**And** retrieves the SAS URL for the PDF background from Blob Storage  
**And** initializes an empty stroke collection  
**And** the UI renders the PDF on the canvas ready for input

---

## Scenario 2: Capturing High-Fidelity Stroke Data

**Given** Liam is writing on the canvas with a stylus  
**When** he draws a circle around an answer  
**Then** the client captures the stroke as a series of points `[x, y, pressure, timestamp]`  
**And** the sampling rate is at least 60Hz  
**And** the pressure data (0.0 to 1.0) is preserved  
**And** the stroke is added to the local session buffer  
**And** the stroke is rendered immediately on screen with variable width based on pressure

---

## Scenario 3: Synchronized Audio Recording

**Given** Liam wants to explain his reasoning  
**When** he taps the "Record Audio" button  
**Then** the client begins capturing audio from the microphone  
**And** records the `audio_start_time` relative to the session start  
**And** subsequent ink strokes are tagged with timestamps relative to the audio  
**And** a visual indicator shows recording is active  
**And** the audio file is streamed or uploaded to Blob Storage upon completion

---

## Scenario 4: Teacher Reviews Student Work with Playback

**Given** a teacher "Ms. Johnson" opens Liam's submitted assignment  
**When** she presses the "Play" button  
**Then** the Digital Ink Service retrieves the stroke data and audio  
**And** the client plays the audio recording  
**And** the ink strokes appear on the canvas in real-time synchronization with the audio  
**And** Ms. Johnson can see exactly when and how Liam wrote his answer  
**And** she can pause, rewind, or scrub through the timeline

---

## Scenario 5: Offline Mode and Sync

**Given** Liam has poor internet connectivity  
**When** he continues working on the assignment offline  
**Then** the client saves all strokes and audio locally  
**And** queues the data for synchronization  
**When** connectivity is restored  
**Then** the client automatically uploads the pending stroke batches and audio files  
**And** the Digital Ink Service updates the session without data loss  
**And** a "Synced" indicator appears in the UI

---

## Scenario 6: PDF Background Rendering

**Given** the assignment is a 5-page PDF  
**When** the session loads  
**Then** the client renders the PDF pages as the background layer  
**And** the ink canvas is overlaid on top of the PDF  
**And** the coordinate system of the ink matches the PDF dimensions  
**And** zooming or panning the PDF keeps the ink perfectly aligned  
**And** the PDF itself is never modified, only overlaid

---

## Scenario 7: Multi-Page Document Navigation

**Given** Liam is working on Page 1  
**When** he swipes to go to Page 2  
**Then** the UI switches the background to Page 2 of the PDF  
**And** the ink canvas clears (visually) to show only strokes for Page 2  
**And** the Digital Ink Service tracks which page each stroke belongs to  
**And** saving the session preserves the page association for all strokes

---

## Scenario 8: Eraser and Undo Functionality

**Given** Liam makes a mistake  
**When** he uses the stylus eraser or taps "Undo"  
**Then** the specific strokes are removed from the visual canvas  
**And** the "Undo" action is recorded in the session history (soft delete)  
**And** the `stroke_data` sent to the server marks these strokes as `deleted` or removes them  
**And** playback respects the deletion (showing the stroke then disappearing, or not showing it at all depending on mode)

---

## Scenario 9: LLM Data Export

**Given** an AI researcher wants to analyze handwriting dynamics  
**When** they request the "Raw Data" export for a session  
**Then** the Digital Ink Service returns the full JSON structure  
**And** it includes the time-series vectors `[x, y, p, t]`  
**And** it includes the audio file URL  
**And** the format is optimized for ingestion by multimodal LLMs  
**And** no raster images are included, only vector data

---

## Scenario 10: Session Auto-Save

**Given** Liam is working for an extended period  
**When** 30 seconds elapse since the last save  
**Then** the client automatically sends a `PUT` request to the Digital Ink Service  
**And** the payload contains the new strokes since the last save  
**And** the service appends these strokes to the `stroke_data` in the database  
**And** the `updated_at` timestamp is refreshed  
**And** Liam sees a subtle "Saved" notification

---

## Scenario 11: Teacher Annotation on Student Work

**Given** Ms. Johnson is reviewing Liam's submission  
**When** she selects the "Red Pen" tool  
**Then** she can write on top of Liam's work  
**And** her strokes are saved to the same session (or a linked "Feedback" session)  
**And** her strokes are visually distinct (different color/layer)  
**And** Liam can view her feedback overlaid on his original work  
**And** he cannot erase her strokes

---

## Scenario 12: Archiving Old Sessions

**Given** the school year has ended  
**When** the system runs the "Year End Rollover" process  
**Then** old Ink Sessions are marked as `archived`  
**And** the raw stroke data is moved to cold storage (if configured)  
**And** the PDF/Audio assets are retained in Blob Storage (Archive Tier)  
**And** the data remains accessible for historical reporting but is read-only  
**And** the database index is optimized for active sessions

---

## Architectural Appendix

### Current State (Legacy)

**Location**: Not present in legacy NS4 monolith  
**Framework**: N/A - This is a **new capability** for NorthStar LMS  
**Database**: N/A

**Legacy State**:
- Legacy NS4 system has **no digital ink capture** functionality
- Traditional text-only or multiple-choice assessments
- No audio recording synchronized with student work
- Limited multimedia assessment capabilities

**Why New Service Needed**:
- Modern education requires richer student work capture (especially for math/science)
- AI/LLM analysis requires high-fidelity time-series data
- Offline-first mobile and tablet support essential for real-world classroom use
- Teacher feedback workflows benefit from annotation and playback capabilities

### Target State (.NET 10 Microservice)

#### Architecture

**Clean Architecture Layers**:
```
DigitalInk.API/                # UI Layer (REST endpoints)
├── Controllers/
├── SignalR/
│   └── InkSessionHub.cs      # Real-time stroke sync
├── Middleware/
└── Program.cs

DigitalInk.Application/        # Application Layer
├── Commands/                 # Create session, save strokes
├── Queries/                  # Get session, export data
├── DTOs/
├── Interfaces/
└── Services/
    ├── PlaybackService/      # Stroke replay synchronization
    └── ExportService/        # LLM-ready data export

DigitalInk.Domain/            # Domain Layer
├── Entities/
│   ├── InkSession.cs
│   ├── Stroke.cs
│   ├── Page.cs
│   └── AudioAsset.cs
├── Events/
│   ├── InkSessionCreatedEvent.cs
│   ├── StrokesCapturedEvent.cs
│   ├── AudioRecordedEvent.cs
│   └── SessionArchivedEvent.cs
└── ValueObjects/
    ├── Point.cs           // [x, y, pressure, timestamp]
    ├── ToolType.cs        // Pen, Highlighter, Eraser
    └── StrokeData.cs      // JSONB structure

DigitalInk.Infrastructure/    # Infrastructure Layer
├── Data/
│   ├── InkDbContext.cs
│   └── Repositories/
├── Storage/
│   └── AzureBlobStorageService.cs  # PDF, Audio assets
├── Integration/
│   └── EventPublisher.cs
└── SignalR/
    └── StrokeSyncService.cs      # Real-time collaboration
```

#### Technology Stack

- **Framework**: .NET 10, ASP.NET Core
- **Data Access**: EF Core 9 with PostgreSQL (JSONB for stroke data)
- **File Storage**: Azure Blob Storage (PDF backgrounds, audio files)
- **Real-Time**: SignalR for live stroke synchronization (future: teacher live monitoring)
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Orchestration**: .NET Aspire hosting
- **Offline Support**: Client-side IndexedDB + background sync

#### Owned Data

**Database**: `NorthStar_DigitalInk_DB`

**Tables**:
- InkSessions (Id, TenantId, OwnerId, EntityId, EntityType, BackgroundAssetUrl, AudioAssetUrl, StrokeData JSONB, CreatedAt, UpdatedAt, IsArchived)
- SessionMetadata (Id, TenantId, SessionId, TotalStrokes, TotalPages, AudioDurationMs, LastSyncedAt)
- FeedbackAnnotations (Id, TenantId, SessionId, TeacherId, FeedbackStrokeData JSONB, CreatedAt)

**JSONB Stroke Data Structure**:
```json
{
  "pages": [
    {
      "pageNumber": 1,
      "strokes": [
        {
          "strokeId": "uuid",
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

#### Service Boundaries

**Owned Responsibilities**:
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

**Not Owned** (delegated to other services):
- Assignment/assessment creation → Assessment Service
- Student enrollment and demographics → Student Management Service
- Grading and scoring → Assessment Service
- Access control → Identity Service
- Reporting and analytics → Reporting Service

**Integration Pattern**:
- Lightweight service focused on ink/audio capture
- Publishes events when sessions are created/completed
- Other services query Digital Ink Service for playback data

#### Domain Events Published

**Event Schema Version**: 1.0 (follows domain-events-schema.md)

- `InkSessionCreatedEvent` - When new ink session starts
  ```
  - SessionId: Guid
  - TenantId: Guid
  - OwnerId: Guid
  - EntityId: Guid        // AssignmentId or AssessmentId
  - EntityType: string
  - BackgroundAssetUrl: string
  - CreatedAt: DateTime
  - OccurredAt: DateTime
  ```

- `StrokesCapturedEvent` - When strokes are saved (batch)
  ```
  - SessionId: Guid
  - TenantId: Guid
  - StrokeCount: int
  - PageNumber: int
  - CapturedAt: DateTime
  - OccurredAt: DateTime
  ```

- `AudioRecordedEvent` - When audio recording uploaded
  ```
  - SessionId: Guid
  - TenantId: Guid
  - AudioAssetUrl: string
  - DurationMs: int
  - OccurredAt: DateTime
  ```

- `SessionArchivedEvent` - When session moved to cold storage
  ```
  - SessionId: Guid
  - TenantId: Guid
  - ArchivedAt: DateTime
  - OccurredAt: DateTime
  ```

#### Domain Events Subscribed

- `AssessmentAssignedEvent` (from Assessment Service) → Enable ink session creation for assignment
- `StudentWithdrawnEvent` (from Student Service) → Archive student's ink sessions

#### API Endpoints (Functional Intent)

**Session Management**:
- Create ink session → returns session ID and SAS URL for PDF
- Update session (save strokes) → appends strokes to JSONB
- Get session details → returns full stroke data and audio URL
- Archive session → moves to cold storage

**Stroke Operations**:
- Save stroke batch → efficient bulk append
- Delete strokes (undo) → marks strokes as deleted
- Get strokes for page → returns filtered stroke data

**Audio Operations**:
- Upload audio file → stores in Blob Storage, links to session
- Get audio URL → returns SAS URL for playback

**Playback & Export**:
- Get playback data → returns synchronized stroke+audio timeline
- Export LLM format → returns JSON optimized for AI analysis

**Teacher Feedback**:
- Add feedback annotation → creates linked annotation session
- Get feedback overlay → returns teacher's strokes

#### Service Level Objectives (SLOs)

- **Availability**: 99.9% uptime
- **Create Session**: p95 < 100ms
- **Save Stroke Batch (100 strokes)**: p95 < 50ms
- **Audio Upload (10MB file)**: p95 < 5 seconds
- **Get Playback Data**: p95 < 200ms
- **Export LLM Format**: p95 < 1 second

#### Idempotency & Consistency

**Idempotency Windows**:
- Session creation: 10 minutes (prevent duplicate sessions)
- Stroke save: 5 minutes (client retry protection)

**Consistency Model**:
- Eventual consistency for stroke batches (offline sync)
- Strong consistency for playback queries
- Audio upload asynchronous (success callback to UI)

#### Security Considerations

**Constitutional Requirements**:
- FERPA compliance for student work
- Secrets stored in Azure Key Vault only
- Enforce least privilege principle

**Implementation**:
- Students can only access their own sessions
- Teachers can view sessions for their students
- SAS tokens with short expiration for Blob Storage URLs
- All session access logged for audit trail
- Audio files encrypted at rest in Blob Storage

#### Testing Requirements

**Constitutional Compliance**:
- Reqnroll BDD features before implementation
- TDD Red → Green with test evidence
- ≥ 80% code coverage

**Test Categories**:

1. **Unit Tests** (DigitalInk.UnitTests):
   - Stroke data serialization/deserialization
   - Playback timeline calculation
   - Offline sync queue logic
   - Undo/redo stroke deletion

2. **Integration Tests** (DigitalInk.IntegrationTests):
   - PostgreSQL JSONB operations
   - Azure Blob Storage upload/download
   - Event publishing to message bus
   - SignalR real-time stroke sync

3. **BDD Tests** (Reqnroll features):
   - `InkCapture.feature` - Create session and capture strokes
   - `AudioSync.feature` - Record and playback audio
   - `OfflineMode.feature` - Offline data capture and sync
   - `TeacherFeedback.feature` - Annotation overlay

4. **UI Tests** (Playwright):
   - Ink canvas interaction (simulated touch/stylus)
   - Audio recording workflow
   - Playback synchronization
   - Multi-page navigation

#### Dependencies

**External Services**:
- Assessment Service - Assignment context
- Student Management Service - Student validation
- Identity Service - Authentication and authorization
- Azure Blob Storage - PDF and audio assets
- Azure Service Bus - Event publishing

**Infrastructure Dependencies**:
- PostgreSQL - Session and stroke data (JSONB)
- .NET Aspire AppHost - Service orchestration

#### Client SDK

**Avalonia/MAUI Client Implementation**:
```csharp
// InkCanvas control
public class InkCaptureService
{
    private readonly List<StrokePoint> _currentStroke = new();
    private DateTime _sessionStartTime;
    
    public void OnPointerPressed(PointerEventArgs e)
    {
        _currentStroke.Clear();
        _sessionStartTime = DateTime.UtcNow;
    }
    
    public void OnPointerMoved(PointerEventArgs e)
    {
        var point = new StrokePoint
        {
            X = e.GetPosition(canvas).X,
            Y = e.GetPosition(canvas).Y,
            Pressure = e.GetPressure(),  // 0.0 - 1.0
            Timestamp = (DateTime.UtcNow - _sessionStartTime).TotalMilliseconds
        };
        _currentStroke.Add(point);
        RenderStroke(point);  // Immediate visual feedback
    }
    
    public async Task OnPointerReleased()
    {
        // Save stroke batch to server
        await _inkService.SaveStrokeBatchAsync(_sessionId, _currentStroke);
        _currentStroke.Clear();
    }
}
```

#### Migration Strategy

**Greenfield Development** (No legacy migration needed):

1. **Phase 4a** (Week 23-24): Build Digital Ink Service MVP
   - Implement session creation and stroke capture
   - Build basic playback functionality
   - Avalonia client prototype

2. **Phase 4b** (Week 25): Audio integration
   - Add audio recording and upload
   - Synchronize audio with stroke playback
   - Build teacher feedback annotations

3. **Phase 4c** (Week 26): Offline support
   - Implement client-side storage
   - Build background sync queue
   - Handle conflict resolution

4. **Phase 4d** (Week 27-28): Polish and scale
   - Add archival and cold storage
   - LLM export format optimization
   - Performance tuning for high-volume sessions

---

## Technical Implementation Notes

**Database Schema (PostgreSQL)**:
```sql
CREATE TABLE ink_sessions (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    owner_id UUID NOT NULL,
    entity_id UUID NOT NULL, -- AssignmentId
    entity_type VARCHAR(50),
    background_asset_url TEXT,
    audio_asset_url TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    stroke_data JSONB -- The heavy lifting
);

CREATE INDEX idx_ink_sessions_entity ON ink_sessions(entity_id);
```

**JSON Structure**:
```json
{
  "pages": [
    {
      "pageNumber": 1,
      "strokes": [
        {
          "tool": "pen",
          "color": "#000000",
          "width": 2.0,
          "points": [[10, 10, 0.5, 0], [12, 12, 0.6, 16], ...]
        }
      ]
    }
  ]
}
```

**API Endpoints**:
- `POST /api/v1/ink/sessions`
- `PUT /api/v1/ink/sessions/{id}/strokes`
- `POST /api/v1/ink/sessions/{id}/audio`
- `GET /api/v1/ink/sessions/{id}`

**Client-Side (Avalonia)**:
- Use `InkCanvas` or custom `SkiaSharp` control for rendering.
- Implement `PointerPressed`, `PointerMoved`, `PointerReleased` events to capture high-frequency data.
- Use `NAudio` or similar for cross-platform audio recording.
