Feature: Section & Roster Service Migration (Foundation Layer)
Business Value: Modern scheduling, enrollment & rollover automation reducing admin effort and enabling accurate, conflict-free rosters.

Scenario Summary:
1. Create section with teacher assignment → SectionCreatedEvent.
2. Add students to roster (capacity tracking) → StudentAddedToRosterEvent.
3. Period configuration & conflict validation (teacher, room, student).
4. Year-end rollover: archive, template next year, promotion stats → RolloverCompletedEvent.
5. Capacity & waitlist auto-fill on drop.
6. Co-teaching assignments; shared permissions.
7. Search/filter (grade, subject, period, available seats) p95 <100ms.
8. Drop/Add with effective dates preserving history (StudentRosterChangedEvent).
9. Attendance integration (AttendanceRecordedEvent) validation roster.
10. Gradebook roster feed with current & dropped status.
11. Roster export (PDF/Excel) with audit log.
12. Historical preservation for transcripts (immutable post-term).

NFRs: Create/AddStudent <100/<50ms; Rollover 500 students <5m; Availability 99.9%; RLS enforced; Waitlist fairness FIFO.
Events Published: SectionCreatedEvent, StudentAddedToRosterEvent, StudentDroppedFromRosterEvent, RolloverCompletedEvent, CapacityReachedEvent.
Subscribed: StudentEnrolledEvent, StaffCreatedEvent, CalendarUpdatedEvent.

Risks: Rollover complexity → dry-run mode; conflict detection performance → precomputed teacher schedule map in Redis.
