# /specify: Homeschool Student Enrollment

## Feature Title
Enroll homeschool student with flexible grade assignment and learning plan initialization

## Goal / Why
Enable homeschool parents to add their children to the LMS with appropriate grade level and subject-specific tracking. Unlike traditional school enrollment, homeschool students may work at different grade levels across subjects (e.g., advanced in math, grade-level in reading) and don't follow fixed school calendars. Parents need to quickly enroll students and begin tracking their individualized learning without district-imposed constraints.

## Intended Experience / What
A homeschool parent navigates to "My Students" and clicks "Enroll Student". They enter the child's first name, last name, date of birth, and select a primary grade level (K-12). The system optionally prompts them to assign subject-specific grade levels if the student works at different levels (e.g., "Math: Grade 5, Reading: Grade 3"). The parent confirms enrollment and the system creates the student profile, affiliates them with the parent's homeschool organization, and generates a skeleton learning plan template based on their state's required subjects. The student appears immediately in the parent's student list and dashboard. The parent receives a confirmation message with the student's unique ID and can proceed to customize the learning plan, add subjects, or log activities.

## Service Boundary Outcomes
The Student Service owns student identity, homeschool affiliation, and subject-level grade tracking. It publishes `HomeschoolStudentEnrolledEvent` which triggers learning plan template creation in the Student Service and assessment eligibility updates in the Assessment Service. Enrollment is idempotent within 30 days based on name+DOB+homeschoolId combination (prevents accidental duplicates). The student appears in the parent's dashboard within 500ms via direct database insert; downstream services synchronize asynchronously. Enrollment SLO: p95 < 300ms for creation, p99 < 500ms.

## Functional Requirements

**Must:**
- Create student record with first name, last name, date of birth, and primary grade level (K-12)
- Affiliate student with parent's homeschool organization (HomeschoolId from parent account)
- Optionally allow parent to set subject-specific grade levels (e.g., Math: 5, Reading: 3, Science: 4)
- Generate learning plan template with state-required subjects (loaded from Configuration Service)
- Assign unique student ID (GUID) and set enrollment status to "Active"
- Publish `HomeschoolStudentEnrolledEvent` with studentId, homeschoolId, parentId, enrollmentDate
- Prevent duplicate enrollment if student with same name, DOB, and homeschoolId exists within past 365 days
- Display enrolled student in parent's student list immediately (no page refresh required)

## Constraints / Non-Goals

- **Not** requiring SSN or government-issued ID (homeschool students may not have these)
- **Not** enforcing district residency or boundary checks (homeschools are location-independent)
- **Not** assigning students to traditional "sections" or "classrooms" during enrollment (separate co-op enrollment flow)
- **Not** auto-creating assessments or assignments (parent chooses when to assign assessments)
- Subject-specific grade levels are optional; default to primary grade level if not specified
- Learning plan template is skeleton only (state-required subjects); parent customizes objectives later

---

**Acceptance Signals (Seed)**:
1. ✅ Parent enrolls 8-year-old "Emma" in grade 3, and Emma appears in student list with auto-generated learning plan showing Pennsylvania's required subjects (Math, Reading, Writing, Science, Social Studies, Arts, Health, PE, Safety, Library)
2. ✅ Parent enrolls "Lucas" in grade K but sets Math to grade 2 and Reading to grade K, and Lucas's assessment recommendations vary by subject (grade 2 math benchmarks, grade K reading benchmarks)
3. ✅ Parent attempts to enroll "Emma Weber" (already enrolled 2 months ago) and system returns existing student record with message "Emma Weber is already enrolled"

**Handoff to /plan**:
- Define REST API endpoint `POST /api/v1/students/homeschool`
- Specify request DTO: `{ firstName, lastName, dateOfBirth, primaryGrade, subjectGrades: [{ subject, effectiveGrade }], homeschoolId }`
- Detail database schema: Students table (Id, FirstName, LastName, DOB, PrimaryGrade), HomeschoolStudents table (StudentId, HomeschoolId, ParentEducatorId, EnrollmentDate), StudentSubjectGrades table (StudentId, Subject, EffectiveGrade)
- Implement duplicate detection logic: SELECT WHERE firstName = ? AND lastName = ? AND dateOfBirth = ? AND homeschoolId = ? AND enrollmentDate > (NOW() - 365 days)
- Create learning plan template generator: fetch state-required subjects from Configuration Service, create LearningPlan records
- Set up MassTransit event publishing for `HomeschoolStudentEnrolledEvent`
- Write Reqnroll BDD feature file for enrollment scenarios

**Open Questions for /plan**:
- Should we validate date of birth (e.g., must be between 4-18 years old)?
- Should we allow parents to upload student photo during enrollment or defer to later?
- Do we need to track immunization records or defer to parent's own record-keeping?
- Should learning plan template include generic objectives or leave blank for parent customization?
