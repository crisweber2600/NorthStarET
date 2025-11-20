Technical Plan (Section & Roster Service)

Slices:
1. Core entities & DbContext (Sections, Rosters, TeacherAssignments, Periods, RolloverRecords).
2. CreateSectionCommand + validation (room, teacher availability via Staff client).
3. AddStudentToRosterCommand (capacity check, waitlist fallback).
4. DropStudentCommand with effective dates, history retention.
5. RolloverService (dry-run + execute; promotion rules; template creation).
6. SearchSectionsQuery (filter builder + index usage, seat availability calculation precomputed).
7. CoTeaching assignments (AddCoTeacherCommand).
8. ExportRosterQuery (PDF via reporting adapter → out of scope for initial stub, Excel via library).
9. Attendance event listener (validate roster membership pre-publish).
10. Audit interceptor.

Performance: Redis cache for section capacity & current enrollment counts; invalidated on roster changes.
Migration: Import legacy sections, rosters, assignments; map periods to new schema.
Testing Focus: Capacity waitlist edge cases; rollover promotion vs retention; conflict detection.
