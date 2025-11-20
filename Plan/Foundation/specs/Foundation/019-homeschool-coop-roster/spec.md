# /specify: Co-op Class Roster Management

## Feature Title
Manage homeschool co-op class rosters with multi-family enrollment and attendance tracking

## Goal / Why
Enable homeschool co-op administrators to create and manage classes (like art, science lab, PE) that serve students from multiple homeschool families in a community. Homeschool co-ops are parent-run organizations where families share teaching responsibilities and resources. Unlike traditional classrooms, co-op classes meet weekly or bi-weekly, have flexible enrollment, and instructors are often parents themselves. Co-op administrators need tools to manage class rosters, track attendance, and share resources while parents need visibility into their children's co-op participation for compliance hour tracking.

## Intended Experience / What
A co-op administrator creates a co-op organization in the LMS called "Springfield Homeschool Co-op" and defines a class "Art Appreciation (Ages 8-12)". They set the instructor (another homeschool parent with limited "co-op instructor" permissions), class description, subject (Art), meeting schedule (Thursdays 1-3pm), and max enrollment (12 students). The administrator publishes the class to member families. Parent Emily searches for local co-ops, finds "Springfield Homeschool Co-op", requests membership, and gets approved by the admin. Emily enrolls her daughter Emma in "Art Appreciation". On Thursdays, the instructor marks Emma as present/absent via a mobile-friendly attendance page. Emma's 2-hour co-op class attendance automatically logs to her activity log as "Art: Co-op class participation, 2 hours" and counts toward her instructional hour requirements. Emily views Emma's co-op participation in her dashboard and includes it in the annual compliance report.

## Service Boundary Outcomes
The Section Service owns co-op classes and roster management. The Student Service owns co-op attendance logging (via `CoopAttendanceRecordedEvent`) which triggers automatic activity log entries. The Staff Service owns co-op instructor permissions. The Configuration Service owns co-op organization setup. Event-driven coordination: when instructor marks attendance, `CoopAttendanceRecordedEvent` → Student Service creates `LearningActivity` → Reporting Service updates compliance hours. Roster enrollment is idempotent within 24 hours (prevents duplicate enrollments). Attendance marking SLO: p95 < 300ms, activity log creation p95 < 2s via async event.

## Functional Requirements

**Must:**
- Allow co-op administrator to create co-op organization (name, location, contact info)
- Allow co-op admin to create classes with name, subject, instructor, grade range, max enrollment, meeting schedule (flexible/recurring)
- Allow parents to search for co-ops by name, location, or zip code radius
- Implement membership approval workflow: parent requests → admin approves → parent can enroll students
- Allow parent to enroll student in co-op class if within grade range and enrollment cap not reached
- Provide co-op instructor with attendance page showing class roster and date selector
- Allow instructor to mark each student present/absent for a specific class session
- Publish `CoopAttendanceRecordedEvent` with studentId, coopClassId, date, hoursAttended (from class duration)
- Auto-create LearningActivity when attendance recorded: subject from co-op class, hours from class duration
- Display co-op participation in student dashboard and compliance reports

## Constraints / Non-Goals

- **Not** managing co-op payments or fee collection (defer to external tools like PayPal, Venmo)
- **Not** supporting co-op messaging/forum in initial release (defer to Phase 5 community features)
- **Not** allowing parents to be instructors for their own children in same class (conflict of interest; defer validation to Phase 3)
- **Not** enforcing strict schedules (co-op classes may meet irregularly, skip weeks; flexible scheduling)
- Co-op enrollment is per class, not org-wide (student enrolls in "Art" but not necessarily "Science Lab")
- Attendance is binary (present/absent); partial attendance not supported in Phase 1

---

**Acceptance Signals (Seed)**:
1. ✅ Co-op admin creates "Springfield Homeschool Co-op" and class "Science Lab (Ages 10-14)" with instructor Ms. Johnson, and 8 parents enroll their students
2. ✅ Instructor marks Emma present for Nov 7 Science Lab (2 hours), and Emma's activity log auto-creates entry "Science: Co-op Science Lab, 2 hours" tagged to Springfield Co-op
3. ✅ Parent Emily views Emma's dashboard and sees "Co-op Participation: 16 hours (8 sessions)" and this appears in annual compliance report

**Handoff to /plan**:
- Define REST API endpoints:
  - `POST /api/v1/coops` - Create co-op organization
  - `POST /api/v1/coops/{coopId}/classes` - Create co-op class
  - `POST /api/v1/coops/{coopId}/members/request` - Request membership
  - `POST /api/v1/coops/classes/{classId}/enroll` - Enroll student
  - `POST /api/v1/coops/classes/{classId}/attendance` - Record attendance
- Detail database schema:
  - CoopOrganizations (Id, Name, Location, AdminUserId)
  - CoopClasses (Id, CoopOrgId, Name, Subject, InstructorId, GradeMin, GradeMax, MaxEnrollment, Schedule (JSON))
  - CoopMembers (CoopOrgId, ParentUserId, Status, JoinDate)
  - CoopClassEnrollments (ClassId, StudentId, EnrollDate)
  - CoopAttendance (Id, ClassId, StudentId, SessionDate, Status, HoursAttended)
- Implement attendance → activity log workflow via MassTransit: CoopAttendanceRecordedEvent → Student Service handler creates LearningActivity
- Create co-op search API with geolocation filtering (zip code radius search)
- Design co-op roster UI in Figma (class list, enrollment, attendance marking)
- Write Reqnroll BDD feature file for co-op scenarios

**Open Questions for /plan**:
- Should we support waitlist when class is full (auto-enroll when slot opens)?
- Should we allow mid-session enrollment or require start-of-session only?
- Do we need co-op resource library (shared curriculum, handouts) or defer to Phase 4 Media Service?
- Should instructor be able to assign homework/assessments via co-op class or keep that in parent's domain?
