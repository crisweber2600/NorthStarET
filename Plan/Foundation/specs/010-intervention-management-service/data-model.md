Data Model (Intervention Service)

Entities:
- Intervention(Id, TenantId, Name, TierLevel, FocusArea, StartDate, EndDate, Frequency, FacilitatorId)
- InterventionEnrollment(Id, TenantId, InterventionId, StudentId, EnrollmentDate, ExitDate, ExitReason, IsActive)
- InterventionSession(Id, TenantId, InterventionId, SessionDate, Notes, FacilitatorId)
- SessionAttendance(Id, TenantId, SessionId, StudentId, AttendanceStatus, RecordedAt)
- ProgressNote(Id, TenantId, InterventionId, StudentId, NoteDate, Observation, ProgressRating, CreatedBy, CreatedAt)
- InterventionResource(Id, TenantId, Name, Description, ResourceUrl)

Indexes: idx_interventions_tenant, idx_enrollments_student, idx_sessions_intervention_date, idx_attendance_session.
RLS: tenant_isolation policy per table.
