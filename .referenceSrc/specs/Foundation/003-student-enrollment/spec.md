# /specify: Student Enrollment Management

## Feature Title
Student enrollment and demographic management for NorthStar LMS

## Goal / Why
Enable school administrators and registrars to enroll new students, manage student demographics, and track enrollment history across the district. This replaces the legacy student management functionality from NS4.WebAPI while establishing a dedicated Student Management microservice with clean boundaries. Teachers and administrators need accurate, up-to-date student rosters to deliver assessments and interventions effectively.

## Intended Experience / What
An administrator navigates to the "Students" page and clicks "Enroll New Student". They enter the student's first name, last name, date of birth, grade level, and school assignment in a form. The system validates the information (no duplicate students with same name+DOB+school within the past year), creates the student record, and assigns a unique student ID. The administrator sees a confirmation message with the student ID and is redirected to the student detail page where they can add contact information, demographics (ethnicity, ELL status, SPED status), and custom district attributes. The student immediately appears in relevant teacher rosters and can be assigned to assessments.

## Service Boundary Outcomes
The Student Service owns student identity, enrollment status, demographics, and contact information. It publishes `StudentCreatedEvent` (consumed by Section Service for roster management and Reporting Service for analytics) and `StudentEnrolledEvent` (consumed by Assessment Service for assignment eligibility). Student creation is idempotent within a 10-minute window based on name+DOB+school combination (returns existing student if duplicate detected). Dashboard queries aggregate data via eventual consistency—student appears in teacher rosters within 2 seconds of enrollment via event propagation. Enrollment SLO: p95 < 300ms for creation, p95 < 100ms for retrieval.

## Functional Requirements

**Must:**
- Create student record with first name, last name, date of birth, grade (K-12), and school assignment
- Validate no duplicate student exists with same name, DOB, and school within past 12 months (return existing student ID if found)
- Generate unique student ID (GUID) and assign enrollment status (Active, Inactive, Withdrawn)
- Allow administrators to add/update student demographics (gender, ethnicity, ELL, SPED, free/reduced lunch eligibility)
- Allow administrators to add multiple contacts (name, relationship, phone, email, primary flag) per student
- Publish `StudentCreatedEvent` and `StudentEnrolledEvent` to message bus for cross-service coordination
- Support soft delete (mark as Inactive rather than hard delete) to preserve historical data

## Constraints / Non-Goals

- **Not** managing student assessment scores (owned by Assessment Service)
- **Not** managing student intervention assignments (owned by Intervention Service)
- **Not** managing class section rosters (owned by Section Service)
- **Not** implementing student photo uploads in initial release (deferred to Phase 4 Media Service)
- Dashboard aggregation happens via eventual consistency; real-time cross-service joins not supported

---

**Acceptance Signals (Seed)**:
1. ✅ Administrator creates a new student and sees confirmation with student ID within 300ms
2. ✅ Attempting to create duplicate student (same name+DOB+school) returns existing student record without error
3. ✅ Student appears in teacher's roster within 2 seconds after enrollment (via event propagation to Section Service)

**Handoff to /plan**:
- Define REST API endpoints (`POST /api/v1/students`, `GET /api/v1/students/{id}`, `PUT /api/v1/students/{id}`)
- Detail EF Core entity models for Student, StudentEnrollment, StudentDemographics, StudentContact tables
- Specify MassTransit event schema for `StudentCreatedEvent`, `StudentEnrolledEvent`, `StudentWithdrawnEvent`
- Implement Redis caching strategy for frequently accessed student records
- Design search/filter API for student lookup by name, grade, school
- Create Reqnroll BDD features for enrollment scenarios
- Set up authorization rules (teachers view only assigned students, admins view all in district)

**Open Questions for /plan**:
- Should we implement student transfer workflow (withdraw from School A, enroll in School B) as a single transaction or two separate operations?
- What's the retention policy for withdrawn students (archive after X years)?
- Do we need to track enrollment history (multiple schools over time) or just current enrollment?
