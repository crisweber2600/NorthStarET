# /specify: Annual State Homeschool Report Generation

## Feature Title
Generate state-specific annual homeschool compliance report for submission to education authorities

## Goal / Why
Enable homeschool parents to automatically generate the annual report required by their state education department, saving hours of manual document preparation. Each state has unique reporting requirements—New York requires an IHIP (Individualized Home Instruction Plan), Pennsylvania requires a portfolio evaluation, California requires a Private School Affidavit. Parents must submit these reports annually to remain in legal compliance. Automating report generation from LMS data (learning plans, activity logs, test scores, portfolio evidence) ensures accuracy, completeness, and reduces parent burden during stressful annual reporting season.

## Intended Experience / What
A homeschool parent navigates to "Compliance Reports" in May and selects "Generate Annual Report". They choose their state from a dropdown (pre-populated with their registered state), select the student and school year (e.g., "Emma Weber, 2024-2025"). The system displays a compliance checklist showing which requirements are complete (green checkmarks) and which are missing (red X's). If all requirements are met, the parent clicks "Generate Report" and the system produces a PDF formatted per their state's template, including student demographics, learning plan with state standard alignments, subject-by-subject instructional hours, assessment results, and selected portfolio evidence samples. The parent previews the PDF, optionally edits custom text sections (e.g., parent's narrative statement), and downloads the final report or emails it directly to their school district. The system archives the report for future reference and marks the compliance year as "Submitted".

## Service Boundary Outcomes
The Reporting Service owns report generation by aggregating data from Student Service (demographics, learning plans), Assessment Service (test scores), Media Service (portfolio evidence), and Configuration Service (state templates and standards). Report generation is an asynchronous process triggered by command—the parent receives a "Your report is being generated" message and gets notified via email when the PDF is ready (typically 10-30 seconds). Generated reports are stored in Azure Blob Storage with SAS token URLs expiring in 7 days. Report generation SLO: p95 < 30s for standard report with < 50 portfolio items, p99 < 60s.

## Functional Requirements

**Must:**
- Allow parent to select state, student, and school year for report generation
- Display compliance checklist showing completion status of state requirements (instructional hours, testing, subjects, portfolio)
- Load state-specific report template from Configuration Service (e.g., NY IHIP template, PA portfolio template)
- Populate report with student demographics (name, DOB, grade, address) from Student Service
- Include learning plan with subject objectives aligned to state standards (from LearningPlan entities)
- Aggregate and display instructional hours by subject and total for the year (from LearningActivities aggregation)
- Include assessment results: standardized tests, benchmark scores, portfolio assessment scores (from Assessment Service)
- Embed selected portfolio evidence samples (3-5 photos/work samples per subject, parent-selected or auto-selected)
- Generate PDF report with print-ready formatting, state logo/header, page numbers
- Allow parent to preview report, optionally edit narrative sections, and finalize
- Provide download link and optional email delivery to school district
- Archive report in parent's account for 7 years with retrieval capability

## Constraints / Non-Goals

- **Not** auto-submitting reports to state agencies electronically (parent responsible for manual submission per state process)
- **Not** validating legal compliance (disclaimer: report provided as-is, parent reviews for accuracy)
- **Not** supporting mid-year or custom date range reports in initial release (defer to advanced reporting features)
- **Not** translating reports to other languages (English only in Phase 1)
- Report generation is asynchronous; parent waits for completion notification (no real-time streaming)
- Portfolio evidence selection is limited to 50 items total per report (prevent PDF size explosion)

---

**Acceptance Signals (Seed)**:
1. ✅ Parent in New York generates IHIP report for Emma (grade 3, 2024-2025), sees compliance checklist with all items green, downloads 12-page PDF with learning plan (12 subjects), 180 days logged, standardized test results, and parent certification statement
2. ✅ Parent in Pennsylvania generates annual report for Lucas (grade K, 2024-2025), system auto-selects 5 portfolio evidence items per subject (Math, Reading, Science, Art, PE), generates 25-page PDF with photos, and parent emails PDF to evaluator
3. ✅ Parent in Texas (no state reporting required) sees message "Texas does not require annual reports. You may generate an optional summary report for your records." and generates simplified summary

**Handoff to /plan**:
- Define REST API endpoint `POST /api/v1/reports/annual-homeschool` with body `{ state, studentId, schoolYear }`
- Implement async report generation command: enqueue job to background worker (Hangfire, Azure Functions)
- Create report template library: Razor templates or HTML-to-PDF library (iTextSharp, PuppeteerSharp) with state-specific layouts
- Aggregate data from multiple services via query: Student demographics (Student Service), learning plan (Student Service), hours (LearningActivities aggregation), tests (Assessment Service), evidence (Media Service)
- Implement portfolio evidence selection: parent-selected OR auto-select most recent 5 items per subject
- Generate PDF and upload to Azure Blob Storage with secure SAS URL (7-day expiration)
- Send completion email with download link
- Create report archive table: GeneratedReports (Id, StudentId, SchoolYear, State, GeneratedAt, BlobUrl, Status)
- Write Reqnroll BDD feature file for report generation scenarios

**Open Questions for /plan**:
- Should we allow parent to customize report template (e.g., change font, add logo)?
- Should we include signature field for parent's digital signature?
- Do we need watermark or timestamp to prevent report tampering?
- Should we generate Word document (.docx) in addition to PDF for parent editing?
- How do we handle missing data (e.g., student has only 120 days logged but report requires 180—show warning but still generate)?
