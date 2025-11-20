# /specify: Daily Learning Activity Logging

## Feature Title
Log daily homeschool learning activities with time tracking and portfolio evidence

## Goal / Why
Enable homeschool parents and students to document daily learning activities to satisfy state-mandated instructional hour requirements and build portfolio evidence for annual compliance reporting. States like Pennsylvania require 180 days or 900 hours of instruction with portfolio documentation. Parents need a quick, mobile-friendly way to log "what we did today" with time spent and optional photos/work samples while the LMS automatically aggregates hours for compliance tracking.

## Intended Experience / What
A parent opens the activity log page for their child (or the child logs their own activity on an age-appropriate interface). They select today's date (or a past date if catching up), choose the subject from a dropdown (Math, Reading, Science, etc.), enter a brief description of the activity (e.g., "Read chapters 3-5 of Charlotte's Web, discussed themes"), and record time spent in hours and minutes (e.g., "1 hour 30 minutes"). Optionally, they upload evidence like a photo of the child's work, a scanned worksheet, or a video of a science experiment. Upon submission, the system saves the activity, updates the student's total instructional hours for compliance tracking, tags the activity to relevant learning plan objectives, and stores the evidence in the portfolio. The parent sees a confirmation message and can view a weekly summary showing total hours by subject. The activity and evidence automatically appear in the student's portfolio report for annual state submission.

## Service Boundary Outcomes
The Student Service owns learning activity storage and instructional hour aggregation. The Media Service owns portfolio evidence file uploads (photos, videos, PDFs) to Azure Blob Storage. These services coordinate via `LearningActivityLoggedEvent` which triggers compliance hour recalculation in the Reporting Service. Activity logging is idempotent within 1 hour (duplicate submissions with same studentId+date+subject+description return existing activity ID). Evidence upload uses async processing—file URL appears in activity record within 5 seconds after upload completes. Activity logging SLO: p95 < 200ms for save (excluding file upload), evidence upload p95 < 3s for files < 5MB.

## Functional Requirements

**Must:**
- Allow parent/student to select activity date (today or past date within current school year)
- Provide subject dropdown with state-required subjects + custom subjects added by parent
- Accept activity description (text field, 500 char limit) describing what was learned
- Record time spent in hours and minutes (converted to decimal hours for storage, e.g., 1h 30m = 1.5 hours)
- Optionally allow upload of 1-5 evidence files (photos, PDFs, videos) with 10MB per file limit
- Tag activity to learning plan objectives if parent selects from checkbox list (optional)
- Publish `LearningActivityLoggedEvent` with studentId, subject, hoursSpent, activityDate, evidenceUrls
- Update student's total instructional hours immediately (no need for batch processing)
- Display weekly summary view showing hours by subject and progress toward state requirement (e.g., "120/180 days logged")

## Constraints / Non-Goals

- **Not** requiring evidence upload for every activity (parent discretion; some states require portfolio, others don't)
- **Not** auto-tagging activities to learning objectives (manual parent selection to avoid incorrect assumptions)
- **Not** supporting bulk import of activities via CSV (defer to Phase 3 Data Import Service)
- **Not** tracking attendance in traditional sense (homeschool doesn't have "absent" days; focus on hours/days logged)
- Evidence upload is asynchronous; activity saves immediately, files process in background

---

**Acceptance Signals (Seed)**:
1. ✅ Parent logs "Math: Completed multiplication worksheet, practiced times tables" for 1 hour, uploads photo of worksheet, and student's total math hours increase from 45 to 46 hours
2. ✅ Student (age 12) logs their own "Science: Built volcano model, researched chemical reactions" for 2.5 hours, uploads video, and activity appears in their portfolio
3. ✅ Parent views weekly summary for week of Nov 4-8 and sees: Math 5 hours, Reading 7 hours, Science 4 hours, Writing 3 hours, Social Studies 2 hours, Total 21 hours

**Handoff to /plan**:
- Define REST API endpoint `POST /api/v1/students/{studentId}/activities`
- Specify request DTO: `{ activityDate, subject, description, hoursSpent, objectiveIds?: [], evidenceFiles?: [File] }`
- Detail database schema: LearningActivities table (Id, StudentId, ActivityDate, Subject, Description, HoursSpent, EvidenceUrls (JSON array), ObjectivesAddressed (JSON array), LoggedBy, CreatedAt)
- Implement time aggregation query: SELECT SUM(HoursSpent) FROM LearningActivities WHERE StudentId = ? AND ActivityDate >= schoolYearStart AND ActivityDate <= schoolYearEnd GROUP BY Subject
- Integrate with Media Service for file upload: POST /api/v1/media/portfolio-evidence with multipart/form-data
- Create weekly summary query/view for dashboard widget
- Set up MassTransit event publishing for `LearningActivityLoggedEvent`
- Write Reqnroll BDD feature file for activity logging scenarios

**Open Questions for /plan**:
- Should we support voice-to-text for description field (mobile accessibility)?
- Should we auto-suggest activities based on recent history (e.g., "Reading: Charlotte's Web" appears in dropdown)?
- Should we validate that activity date is not in the future or more than 30 days in past?
- Do we need approval workflow for student-logged activities (parent review before counting toward compliance)?
