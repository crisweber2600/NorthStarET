Technical Plan (Intervention Service)

Slices:
1. Core domain entities & DbContext.
2. CreateInterventionCommand (session pre-generation logic) + event publish.
3. EnrollStudentCommand (validation: active, not duplicated) + event.
4. RecordAttendanceCommand + absence streak analyzer.
5. ProgressNoteCommand + rating validation.
6. EffectivenessQuery: aggregates (attendance %, pre/post score delta from Assessment Service via query client).
7. TierHistory tracking (escalations) future slice.
8. Resource management (simple CRUD + conflict checks).
9. Template creation & instantiation.
10. ExitCriteriaEvaluator background job weekly.
11. Audit interceptor & communication logging.

Data Migration: Legacy interventions copied; attendance & notes imported; map facilitator legacy IDs.
Performance: Index (tenant_id, tier_level, focus_area); attendance queries by sessionId composite index.
Security: Policy per role (specialist, teacher read-only, admin full); RLS enforced.
Testing: Unit (effectiveness calc, absence flags), Integration (event publish), BDD (creation, enrollment, attendance, progress), UI (forms + dashboard).
