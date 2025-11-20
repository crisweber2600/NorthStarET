Data Model (Section & Roster Service)

Entities:
- Section(Id, TenantId, SectionNumber, CourseName, GradeLevel, SchoolId, Room, Period, Capacity, AcademicYear, Term)
- TeacherAssignment(Id, TenantId, SectionId, TeacherId, AssignmentType, AssignedDate)
- Roster(Id, TenantId, SectionId, StudentId, EnrollmentDate, DropDate, IsActive, WaitlistPosition)
- Period(Id, TenantId, SchoolId, PeriodNumber, PeriodName, StartTime, EndTime)
- RolloverRecord(Id, TenantId, FromYear, ToYear, ExecutedAt, ExecutedBy, TotalSections, TotalStudents, Status)

Indexes: tenant + school composite; roster by sectionId; active student schedule lookup index.
RLS: tenant_isolation on all tables.
