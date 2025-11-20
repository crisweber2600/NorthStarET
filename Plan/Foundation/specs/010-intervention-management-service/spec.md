Feature: Intervention Management Service Migration (Foundation Layer)
Business Value: Structured MTSS/RTI intervention tracking, attendance, progress analytics, enabling data-driven tier adjustments and improved student outcomes.

Key Scenarios (Condensed):
1. Create Tier 2 group (calendar sessions auto-generated) → InterventionCreatedEvent.
2. Auto enrollment from assessment risk list → StudentEnrolledInInterventionEvent.
3. Session attendance + notes, absence flagging → InterventionAttendanceRecordedEvent.
4. Progress notes with ratings & trajectory.
5. Scheduling conflict detection (room, facilitator, class).
6. Effectiveness dashboard (attendance %, score delta, projected outcome, ROI).
7. Tier transitions with documented decision points (TierEscalatedEvent future).
8. Parent communication emails (enrollment, progress summaries) audited.
9. Resource allocation (curriculum, materials) & utilization.
10. Templates for common interventions (district shared).
11. Exit criteria evaluation & StudentExitedInterventionEvent.
12. Immutable audit trail of modifications & communications.

NFRs:
- Performance: Create/Attendance p95 <100/<50ms; Effectiveness calc <200ms.
- Availability 99.9%; Security FERPA; Idempotency windows (creation 10m, attendance 5m).
- Observability: metrics intervention_effectiveness_calc_ms, attendance_rate, structured logs.

Events Published v1.0: InterventionCreatedEvent, StudentEnrolledInInterventionEvent, InterventionAttendanceRecordedEvent, ProgressNoteAddedEvent, StudentExitedInterventionEvent.
Subscribed: StudentEnrolledEvent, AssessmentResultRecordedEvent, StaffUpdatedEvent.

Risks: Complex effectiveness metrics → incremental approach; scheduling conflicts across services → rely on Section & Configuration service APIs.
