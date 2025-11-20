Feature: Digital Ink Service (Greenfield) (Foundation Layer Phase 4)
Business Value: High-fidelity stroke + audio capture enabling richer assessments, replay for instructional insight, and AI-ready multimodal datasets.

Scenario Summary:
1. Start ink session (PDF background, empty strokes) → InkSessionCreatedEvent.
2. Capture stylus strokes (x,y,pressure,t) ≥60Hz variable width render.
3. Synchronized audio recording → AudioRecordedEvent.
4. Teacher playback (time-aligned stroke revelation).
5. Offline capture & sync queue (local IndexedDB) resilience.
6. Multi-page PDF background alignment & navigation.
7. Eraser/Undo soft delete (deleted flag) respecting playback semantics.
8. LLM raw export (vector + audio metadata, no raster).
9. Auto-save every 30s incremental batch (StrokesCapturedEvent).
10. Teacher feedback annotation overlay (distinct layer).
11. End-of-year archival to cold storage → SessionArchivedEvent.
12. Security & access controls (student own, teacher authorized, SAS tokens short-lived).

NFRs: Create/Save batch <100/<50ms; Playback data <200ms; Audio upload (10MB) <5s; Availability 99.9%; JSONB stroke storage optimized.
Idempotency: Session creation 10m; stroke batch save 5m (client retry).
Events: InkSessionCreatedEvent, StrokesCapturedEvent, AudioRecordedEvent, SessionArchivedEvent.
Subscribed: AssessmentAssignedEvent, StudentWithdrawnEvent.

Risks: Large stroke payload growth → page-level segmentation + compression future; Audio/video sync drift → timestamp normalization.
