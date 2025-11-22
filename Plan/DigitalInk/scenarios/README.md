# Digital Ink Scenarios

This directory contains Given-When-Then scenarios for the Digital Ink Service implementation.

## Scenario Files

### 01. Digital Ink Capture & Playback
**File:** [01-digital-ink-capture-playback.md](./01-digital-ink-capture-playback.md)  
**Status:** ✅ Complete with Architectural Appendix (2025-11-20)  
**Scenarios:** 12 Given-When-Then scenarios

**Coverage:**
1. Student starts new ink session on assignment
2. Capturing high-fidelity stroke data (x, y, pressure, timestamp)
3. Synchronized audio recording
4. Teacher reviews student work with playback
5. Offline mode and sync
6. PDF background rendering
7. Multi-page document navigation
8. Eraser and undo functionality
9. LLM data export
10. Session auto-save
11. Teacher annotation on student work
12. Archiving old sessions

**Architectural Appendix Includes:**
- Clean Architecture layers (Domain → Application → Infrastructure → API)
- Technology stack (.NET 10, EF Core 9, PostgreSQL JSONB, Azure Blob Storage)
- Database schema with JSONB stroke data structure
- Domain events (published and subscribed)
- API endpoints (RESTful + SignalR)
- Service Level Objectives (SLOs)
- Testing requirements (Unit, Integration, BDD, Playwright)
- Migration strategy (Phase 4a-4d greenfield development)
- Client SDK architecture (Avalonia/MAUI with SkiaSharp)

---

## Scenario Development Process

### 1. Specification Phase (Current)
- ✅ Functional scenarios written in Given-When-Then format
- ✅ Architectural Appendix completed
- ✅ Service boundaries defined
- ✅ Integration patterns documented

### 2. BDD Implementation (Phase 4a-4d)
Each scenario will be implemented as Reqnroll `.feature` files:

```gherkin
Feature: Ink Session Creation
  As a student
  I want to create an ink session on an assignment
  So that I can annotate and record my work

Scenario: Student starts new ink session on assignment
  Given a student "Liam" is logged in
  And an assignment "Math Worksheet 1" exists with a PDF background
  When Liam taps "Start Annotation"
  Then a new ink session is created
  And the session is linked to Liam and the assignment
  And the PDF background SAS URL is retrieved
  And the ink canvas is initialized with an empty stroke collection
```

### 3. Test-Driven Development
- Write BDD feature file BEFORE implementation (Red phase)
- Run `dotnet test` → capture Red output
- Implement production code
- Run `dotnet test` → capture Green output
- Attach both transcripts to phase review

---

## Scenario Mapping to Phases

| Scenario | Phase | Implementation Priority |
|----------|-------|------------------------|
| 1. Session Creation | 4a | P0 (MVP blocker) |
| 2. Stroke Capture | 4a | P0 (MVP blocker) |
| 10. Auto-Save | 4a | P1 (MVP enhancement) |
| 3. Audio Recording | 4b | P0 (Audio integration) |
| 4. Teacher Playback | 4b | P0 (Audio integration) |
| 5. Offline Sync | 4c | P0 (Offline support) |
| 11. Teacher Annotation | 4c | P1 (Feedback feature) |
| 6. PDF Rendering | 4a | P0 (MVP blocker) |
| 7. Multi-Page Navigation | 4c | P2 (Enhancement) |
| 8. Eraser/Undo | 4a | P1 (MVP enhancement) |
| 12. Archival | 4d | P2 (Polish) |
| 9. LLM Export | 4d | P2 (Polish) |

---

## Related Documentation

- **Implementation Plan:** [../IMPLEMENTATION_PLAN.md](../IMPLEMENTATION_PLAN.md) - Phase 4a-4d roadmap
- **Service Overview:** [../SERVICE_OVERVIEW.md](../SERVICE_OVERVIEW.md) - Architecture and boundaries
- **Technical Docs:** [../docs/](../docs/) - API, events, technology stack

---

**Last Updated:** November 20, 2025  
**Total Scenarios:** 12  
**Status:** Specification complete, implementation starts Week 23
