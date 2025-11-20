# Digital Ink Layer Implementation Plan

**Version:** 1.0  
**Last Updated:** November 20, 2025  
**Status:** Planning Phase

---

## Executive Summary

The Digital Ink Layer introduces a **greenfield microservice** enabling high-fidelity stylus input, synchronized audio recording, and AI-ready data capture for student assessments. This is a **net-new capability** not present in the legacy NS4 monolith.

**Key Characteristics:**
- Greenfield development (no legacy migration)
- Specialized domain separate from Foundation layer
- Event-driven integration with Foundation services
- Offline-first client architecture
- AI/LLM-optimized data formats

**Timeline:** Week 23-28 (6 weeks)  
**Team Size:** 2-3 developers + 1 UI/UX designer  
**Delivery Model:** Phased rollout with MVP in Phase 4a

---

## Dependency Gates

### Critical Prerequisites

**MANDATORY:** Foundation Phase 3 must be **production-ready** before Digital Ink Phase 4a begins.

| Foundation Service | Required Capability | Gate Status | Target Week |
|-------------------|---------------------|-------------|-------------|
| **Identity Service** | Session token validation, user context | ✅ Phase 1 | Week 8 |
| **Assessment Service** | Assignment CRUD, assignment-to-student mapping | 🔒 **BLOCKING** | Week 20 |
| **Student Management Service** | Student profile retrieval, enrollment validation | 🔒 **BLOCKING** | Week 18 |
| **API Gateway** | YARP routing, rate limiting, JWT validation | ✅ Phase 1 | Week 10 |
| **Azure Blob Storage** | Container provisioning, SAS token generation | ✅ Infrastructure | Week 6 |

**Gate Verification Checklist:**

- [ ] Assessment Service API returns assignment details (GET `/api/v1/assessments/{id}`)
- [ ] Assessment Service publishes `AssessmentAssignedEvent` to message bus
- [ ] Student Management Service API returns student profiles (GET `/api/v1/students/{id}`)
- [ ] Identity Service validates JWT tokens and returns claims including `TenantId`, `UserId`, `Role`
- [ ] API Gateway routes requests to `/api/v1/ink/*` (placeholder routing ready)
- [ ] Azure Blob Storage containers exist: `assignment-pdfs`, `ink-audio-recordings`
- [ ] Foundation Phase 3 integration tests pass at ≥80% coverage
- [ ] Foundation Phase 3 services are deployed to staging environment

**Gate Approval:** Product Owner + Technical Lead must sign off on Phase 3 completion before Digital Ink work begins.

### Optional Dependencies (Nice-to-Have)

- Reporting Service (Phase 4) - For ink session analytics dashboards
- Content Media Service - For PDF asset management (can use Blob Storage directly as fallback)

---

## Implementation Phases

### Phase 4a: Digital Ink Service MVP (Week 23-24)

**Goal:** Core ink capture and basic playback

**Deliverables:**
1. **Digital Ink Service** (.NET 10 microservice)
   - Clean Architecture: Domain → Application → Infrastructure → API
   - PostgreSQL database with JSONB stroke storage
   - EF Core 9 with migrations
   - MassTransit event publishing
   - .NET Aspire AppHost integration

2. **Session Management**
   - Create ink session linked to assignment
   - Save stroke batches (x, y, pressure, timestamp)
   - Retrieve session data for playback
   - Archive session (soft delete)

3. **Stroke Capture**
   - JSONB schema for multi-page stroke data
   - High-fidelity point arrays (60Hz+ sampling)
   - Page-level stroke organization
   - Undo/delete stroke functionality

4. **Avalonia Client Prototype**
   - InkCanvas control with SkiaSharp rendering
   - Pointer event capture (PointerPressed/Moved/Released)
   - Stroke rendering with pressure-based width
   - HTTP client for session API calls

5. **API Endpoints**
   - `POST /api/v1/ink/sessions` - Create session
   - `PUT /api/v1/ink/sessions/{id}/strokes` - Save stroke batch
   - `GET /api/v1/ink/sessions/{id}` - Get session data
   - `DELETE /api/v1/ink/sessions/{id}` - Archive session (soft delete)

**Testing:**
- Unit tests (≥80% coverage)
- Integration tests (PostgreSQL JSONB operations, event publishing)
- BDD: `InkCapture.feature` (Reqnroll)
- Playwright: Canvas interaction simulation

**Exit Criteria:**
- Student can create ink session on assignment
- Student can draw on canvas and see strokes rendered
- Strokes persist to database and reload correctly
- Teacher can view static playback of strokes (no audio yet)

**Estimated Effort:** 2 weeks (80 hours)

---

### Phase 4b: Audio Integration (Week 25)

**Goal:** Synchronized audio recording and playback

**Deliverables:**
1. **Audio Capture**
   - Azure Blob Storage integration for audio files
   - SAS token generation for secure uploads
   - Audio metadata tracking (duration, sample rate, format)
   - `AudioRecordedEvent` domain event

2. **Playback Synchronization**
   - Timeline calculation (stroke timestamps relative to audio start)
   - PlaybackService in Application layer
   - Stroke animation synchronized with audio timeline
   - Pause/rewind/scrub controls

3. **Client Audio Recording**
   - NAudio or platform-specific audio capture
   - WebM/AAC encoding
   - Streaming upload to Blob Storage
   - Visual recording indicator

4. **API Endpoints**
   - `POST /api/v1/ink/sessions/{id}/audio` - Upload audio file
   - `GET /api/v1/ink/sessions/{id}/playback` - Get synchronized playback data

**Testing:**
- Integration tests (Azure Blob upload/download)
- BDD: `AudioSync.feature`
- Playwright: Record audio → playback workflow

**Exit Criteria:**
- Student can record audio while writing
- Teacher sees strokes appear in sync with audio during playback
- Audio files stored securely in Blob Storage with SAS URLs

**Estimated Effort:** 1 week (40 hours)

---

### Phase 4c: Offline Support & Teacher Feedback (Week 26)

**Goal:** Offline-first architecture and teacher annotation

**Deliverables:**
1. **Offline Mode**
   - Client-side storage (IndexedDB or SQLite)
   - Background sync queue
   - Conflict resolution strategy
   - Network connectivity detection
   - "Synced" visual indicator

2. **Teacher Feedback Annotations**
   - FeedbackAnnotations table (linked to InkSession)
   - Teacher-only stroke layer (different color/style)
   - Read-only student view of teacher strokes
   - Feedback overlay rendering

3. **Multi-Page Navigation**
   - Page-level stroke filtering
   - PDF background rendering (per page)
   - Swipe gesture handling
   - Page association tracking

**Testing:**
- BDD: `OfflineMode.feature`, `TeacherFeedback.feature`
- Playwright: Offline workflow, multi-page navigation
- Integration tests: Sync queue conflict resolution

**Exit Criteria:**
- Student can work offline and data syncs when online
- Teacher can annotate student work with red pen
- Student sees teacher feedback overlaid on original work
- Multi-page PDF assignments work correctly

**Estimated Effort:** 1 week (40 hours)

---

### Phase 4d: Polish, Scale & LLM Export (Week 27-28)

**Goal:** Production hardening and AI data export

**Deliverables:**
1. **Archival & Cold Storage**
   - Scheduled job for year-end rollover
   - Move old sessions to Azure Blob Archive tier
   - Database index optimization (active vs archived)
   - Read-only access to archived sessions

2. **LLM Data Export**
   - ExportService in Application layer
   - JSON format optimized for multimodal LLMs
   - Time-series vector export `[x, y, p, t]`
   - Audio URL + metadata inclusion
   - Export API endpoint

3. **Performance Tuning**
   - SignalR real-time stroke sync (future: live monitoring)
   - Database query optimization (JSONB indexing)
   - Blob Storage CDN integration
   - Client-side rendering optimization

4. **Security Hardening**
   - SAS token expiration policies
   - Tenant isolation verification (RLS tests)
   - FERPA compliance audit
   - Audit logging for all session access

**Testing:**
- Load testing (100 concurrent sessions)
- Security testing (tenant isolation, SAS token expiration)
- BDD: `Archival.feature`, `LLMExport.feature`
- Performance benchmarks (SLOs validation)

**Exit Criteria:**
- Service meets SLOs (p95 latencies, 99.9% uptime)
- LLM export format validated by AI research team
- Archival process tested with sample year-end data
- Security audit passes (FERPA compliant)

**Estimated Effort:** 2 weeks (80 hours)

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

## Integration Strategy

### Event-Driven Integration

**Digital Ink Service Publishes:**
- `InkSessionCreatedEvent` → Reporting Service (analytics)
- `StrokesCapturedEvent` → Reporting Service (usage tracking)
- `AudioRecordedEvent` → Reporting Service (multimedia usage metrics)
- `SessionArchivedEvent` → Data Retention Service (compliance)

**Digital Ink Service Subscribes:**
- `AssessmentAssignedEvent` (from Assessment Service) → Enable ink session creation
- `StudentWithdrawnEvent` (from Student Service) → Archive student's sessions

### Synchronous Dependencies

Digital Ink Service makes **synchronous HTTP calls** to:
- Identity Service - JWT validation (cached, 5-minute TTL)
- Assessment Service - Assignment details (GET `/api/v1/assessments/{id}`)
- Student Management Service - Student validation (GET `/api/v1/students/{id}`)

**Latency Budgets:**
- Identity Service: 50ms (cached)
- Assessment Service: 100ms
- Student Management Service: 100ms
- Total synchronous latency budget: 250ms

**Fallbacks:**
- Cached assignment metadata (1-hour TTL) for offline resilience
- Graceful degradation if assessment details unavailable (show "Unknown Assignment")

---

## Technology Stack

### Backend
- **Framework:** .NET 10, ASP.NET Core
- **Data Access:** EF Core 9 with PostgreSQL (JSONB for stroke data)
- **Messaging:** MassTransit + Azure Service Bus
- **File Storage:** Azure Blob Storage (PDF backgrounds, audio files)
- **Real-Time:** SignalR (future: live collaboration)
- **Orchestration:** .NET Aspire AppHost

### Client (Avalonia/MAUI)
- **Framework:** Avalonia 11 (cross-platform) or .NET MAUI
- **Rendering:** SkiaSharp for high-performance ink rendering
- **Audio:** NAudio or platform-specific APIs
- **Offline Storage:** IndexedDB (web) or SQLite (desktop/mobile)
- **HTTP Client:** Refit or HttpClient with Polly retry policies

### Infrastructure
- **Database:** PostgreSQL 16 (per-service database: `NorthStar_DigitalInk_DB`)
- **Blob Storage:** Azure Storage Account (Hot tier for active, Archive tier for old sessions)
- **Message Bus:** Azure Service Bus (Standard tier)
- **Monitoring:** Azure Application Insights, Azure Monitor

---

## Testing Strategy

### Test Coverage Requirements

**Constitutional Compliance:** ≥80% code coverage, TDD Red→Green evidence required

1. **Unit Tests** (DigitalInk.Application.UnitTests)
   - Stroke data serialization/deserialization
   - Playback timeline calculation
   - Offline sync queue logic
   - Domain event creation

2. **Integration Tests** (DigitalInk.Infrastructure.IntegrationTests)
   - PostgreSQL JSONB operations
   - Azure Blob Storage upload/download
   - MassTransit event publishing
   - EF Core migrations

3. **BDD Tests** (Reqnroll features in `tests/bdd/`)
   - `InkCapture.feature` - Session creation and stroke capture (Phase 4a)
   - `AudioSync.feature` - Audio recording and playback (Phase 4b)
   - `OfflineMode.feature` - Offline data capture and sync (Phase 4c)
   - `TeacherFeedback.feature` - Annotation overlay (Phase 4c)
   - `Archival.feature` - Year-end rollover (Phase 4d)
   - `LLMExport.feature` - AI data export (Phase 4d)

4. **UI Tests** (Playwright in `tests/ui/`)
   - Ink canvas interaction (simulated touch/stylus)
   - Audio recording workflow
   - Playback synchronization
   - Multi-page navigation
   - Teacher annotation workflow

### Red→Green Evidence

**MANDATORY:** Capture terminal output for each phase:
- `phase-4a-red-dotnet-test.txt` - Unit/Integration/BDD tests BEFORE implementation
- `phase-4a-red-playwright.txt` - UI tests BEFORE implementation
- `phase-4a-green-dotnet-test.txt` - All tests AFTER implementation
- `phase-4a-green-playwright.txt` - UI tests AFTER implementation

Repeat for Phase 4b, 4c, 4d.

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Foundation Phase 3 delays | Medium | High | Gate enforcement, fallback to Phase 4d if needed |
| Stylus input cross-platform issues | Medium | Medium | Early prototyping on Windows/iPad/Android |
| Audio sync accuracy < 50ms | Low | Medium | Use high-resolution timers, test on real devices |
| Offline sync conflicts | Medium | Medium | Last-write-wins with timestamp resolution |
| PostgreSQL JSONB query performance | Low | High | Index optimization, load testing with 10k sessions |
| Blob Storage SAS token leaks | Low | High | Short expiration (15 min), audit logging |

---

## Rollout Strategy

### Phase 4a: Internal Testing
- Deploy to dev environment
- QA team testing with synthetic assignments
- Performance baseline established

### Phase 4b: Pilot School (1 classroom)
- 30 students, 1 teacher
- Math worksheet assignments with audio
- Weekly feedback sessions

### Phase 4c: Pilot Expansion (3 schools)
- 200 students, 10 teachers
- Monitor offline sync reliability
- Collect teacher feedback on annotation tools

### Phase 4d: General Availability
- All districts with Foundation Phase 3 deployed
- Feature flag controlled rollout
- Gradual scale-up monitoring

---

## Success Metrics

**Phase 4a (MVP):**
- ✅ 100% of pilot students successfully create ink sessions
- ✅ 95% of stroke data persists without loss
- ✅ Teachers can view playback for all submitted assignments

**Phase 4b (Audio):**
- ✅ 90% of students record audio at least once
- ✅ Audio-stroke sync accuracy within ±100ms (measured)

**Phase 4c (Offline + Feedback):**
- ✅ 95% of offline sessions sync successfully when online
- ✅ 80% of teachers use annotation feature within first week

**Phase 4d (Production):**
- ✅ 99.9% uptime over 30-day period
- ✅ All SLOs met for p95 latencies
- ✅ Zero FERPA compliance violations

---

## Open Questions & Decisions Needed

1. **Client Platform Priority:** Avalonia (desktop-first) vs MAUI (mobile-first)? → **Decision needed by Week 22**
2. **Audio Format:** WebM (web-friendly) vs AAC (higher quality)? → **Decision: WebM for browser, AAC for native apps**
3. **Sync Conflict Resolution:** Last-write-wins vs prompt user? → **Decision: Last-write-wins with audit log**
4. **LLM Export Format:** Custom JSON vs standardized InkML? → **Research: Consult AI team by Week 26**

---

## Related Documentation

- **Scenarios:** [scenarios/01-digital-ink-capture-playback.md](./scenarios/01-digital-ink-capture-playback.md)
- **Service Overview:** [SERVICE_OVERVIEW.md](./SERVICE_OVERVIEW.md)
- **Technical Docs:** [docs/](./docs/)
- **Foundation Dependencies:** [Plan/Foundation/MASTER_MIGRATION_PLAN.md](../Foundation/MASTER_MIGRATION_PLAN.md)
- **Layer Architecture:** [Plan/LAYERS.md](../LAYERS.md)

---

**Last Updated:** November 20, 2025  
**Next Review:** Week 22 (pre-Phase 4a kickoff)  
**Owner:** Digital Ink Team Lead
