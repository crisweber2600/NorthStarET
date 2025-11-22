# /specify: Multi-Grade Subject Level Tracking

## Feature Title
Track student performance at different grade levels across subjects for individualized homeschool learning

## Goal / Why
Enable homeschool parents to accurately represent students who work at different grade levels in different subjects—a common homeschool scenario where a child might be advanced in math but grade-level in reading. Traditional school systems enforce single-grade enrollment, but homeschoolers need flexibility to track "Emma is doing 5th grade math, 3rd grade reading, and 4th grade science" simultaneously. This ensures appropriate curriculum alignment, assessment assignment, and accurate state standard mapping per subject while respecting each child's unique learning pace.

## Intended Experience / What
A parent enrolls their student Emma in "3rd grade" as the primary grade level. They navigate to Emma's profile and click "Subject Grade Levels". A form displays all subjects (Math, Reading, Science, Social Studies, etc.) with dropdown selectors for grade K-12. The parent sets Math to Grade 5, Reading to Grade 3 (matching primary grade), Science to Grade 4, and leaves others at default (Grade 3). Upon saving, the system updates Emma's subject-level grade tracking and regenerates her learning plan to align Math objectives with 5th grade state standards, Science with 4th grade standards, etc. When the parent later assigns assessments, the Assessment Service recommends grade-appropriate tests per subject—5th grade math benchmarks for Emma's math, 3rd grade reading benchmarks for reading. Progress reports show Emma's mastery per subject at the appropriate grade level rather than a single overall grade.

## Service Boundary Outcomes
The Student Service owns subject-level grade storage in the `StudentSubjectGrades` table and publishes `SubjectGradeUpdatedEvent` when parents change grade assignments. The Configuration Service provides state standards per subject per grade. The Assessment Service subscribes to `SubjectGradeUpdatedEvent` to adjust assessment recommendations. The Reporting Service builds multi-grade progress visualizations from event-sourced data. Subject grade updates are idempotent within 5 minutes—duplicate submissions return existing grades. Changes propagate to downstream services within 2 seconds via event-driven updates. Subject grade update SLO: p95 < 200ms for save, p95 < 2s for full system synchronization.

## Functional Requirements

**Must:**
- Allow parent to set effective grade level (K-12) per subject for any student
- Default subject grade levels to student's primary grade level if not explicitly set
- Update learning plan objectives to align with subject-specific grade-level state standards
- Publish `SubjectGradeUpdatedEvent` with studentId, subject, previousGrade, newGrade, effectiveDate
- Adjust assessment recommendations to match subject-specific grade levels (5th grade math assessments, 3rd grade reading assessments)
- Display subject-level grades on student profile and in parent dashboard
- Support bulk update (set multiple subjects at once) to avoid multiple page refreshes
- Show visual indicator on progress reports when student works at non-standard grade level (e.g., badge "Advanced: Grade 5" on Math progress)

## Constraints / Non-Goals

- **Not** changing student's official "primary grade" for legal/compliance purposes (Emma is enrolled in 3rd grade officially, but works at different levels internally)
- **Not** automatically detecting appropriate grade level based on assessment scores (parent decides grade assignments)
- **Not** limiting grade level changes to adjacent grades (parent can set Math to Grade 8 for gifted student in Grade 3)
- **Not** enforcing grade progressions (student can repeat grade level or skip grades as parent directs)
- Subject-specific grading applies to academic subjects only; electives/specials may use primary grade level

---

**Acceptance Signals (Seed)**:
1. ✅ Parent sets Emma (primary grade 3) to Math Grade 5, and Emma's Math learning plan updates to show 5th grade multiplication/division standards, while Reading plan shows 3rd grade comprehension standards
2. ✅ Parent assigns "Math Benchmark" assessment, system recommends "Grade 5 Math Benchmark", while "Reading Benchmark" recommends "Grade 3 Reading Benchmark"
3. ✅ Progress report shows Emma's Math mastery at 85% (Grade 5 standards) and Reading mastery at 92% (Grade 3 standards) with clear grade-level labels

**Handoff to /plan**:
- Define REST API endpoint `PUT /api/v1/students/{studentId}/subject-grades` with body `{ subjectGrades: [{ subject, gradeLevel }] }`
- Detail database schema: StudentSubjectGrades table (StudentId, Subject, EffectiveGrade, EffectiveDate, PreviousGrade, ChangedBy)
- Implement learning plan regeneration logic: when subject grade changes, query Configuration Service for new grade standards, update LearningPlan objectives
- Set up MassTransit event publishing for `SubjectGradeUpdatedEvent`
- Create query for assessment recommendations: JOIN StudentSubjectGrades ON subject, return grade-appropriate assessments
- Design multi-grade progress report visualization in Figma (separate progress bars per subject with grade labels)
- Write Reqnroll BDD feature file for multi-grade scenarios

**Open Questions for /plan**:
- Should we track historical subject grade changes (audit trail of "Math changed from Grade 3 to Grade 5 on Jan 15")?
- Should we warn parent if grade level jump is extreme (e.g., Grade 2 student assigned Grade 8 math)?
- Do we need to support fractional grades (e.g., "Math: Grade 4.5") or whole grades only?
- Should subject-specific grades affect promotion/graduation logic, or is that entirely parent-driven?
