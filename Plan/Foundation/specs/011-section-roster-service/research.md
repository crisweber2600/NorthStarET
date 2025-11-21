# Research: Section & Roster Service Migration
Layer: Foundation
Version: 0.1.0

## Decisions
- **Normalized schedule model with conflict detection cache**  
  - Rationale: spec calls for fast conflict checks (teacher, room, student); use Redis-backed schedule map keyed by period/time.  
  - Alternatives: DB-only validation (slower under load), client-side checks (unsafe).

- **Event-driven roster changes**  
  - Rationale: downstream attendance, gradebook, analytics require roster events (`StudentAddedToRoster`, `StudentDroppedFromRoster`, `RolloverCompleted`).  
  - Alternatives: batch exports (laggy).

- **Rollover via templating + dry-run**  
  - Rationale: reduces risk; supports preview of next-year rosters per spec.  
  - Alternatives: direct copy without dry-run (higher error risk).

- **Waitlist with FIFO auto-fill**  
  - Rationale: meets fairness requirement; triggers auto-fill events when seats free.  
  - Alternatives: manual waitlist management (more admin effort).

## Open Questions
1. How are period definitions sourced—Configuration service calendars or local table? Assume Configuration provides period catalog per school.
2. Gradebook integration format—events only or dedicated API feed? Default to events + contract snapshot endpoint.
3. Does rollover include teacher assignment carry-over or re-evaluation each year? Spec implies templating; confirm default behavior.
