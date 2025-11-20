# /specify: Compliance Dashboard with Real-Time Status

## Feature Title
Real-time homeschool compliance status dashboard with state-specific requirements tracking

## Goal / Why
Enable homeschool parents to instantly see their compliance status with state regulations and identify missing requirements before annual report deadlines. States have complex, varying requirements (instructional hours, testing, subject coverage, portfolio evidence) that parents must track throughout the year. A visual, real-time dashboard prevents last-minute scrambling when the annual report is due and gives parents confidence they're meeting legal obligations. This reduces compliance risk and parental stress.

## Intended Experience / What
A homeschool parent logs into their dashboard and immediately sees a "Compliance Status" widget showing their state's requirements (e.g., "Pennsylvania: 180 days, Grade 3 testing, Portfolio required"). Visual progress bars show completion: "Instructional Days: 120/180 (67%)", "Required Testing: Not due until May 2025", "Portfolio Evidence: 45 items uploaded". Color coding indicates status—green for on track, yellow for approaching deadline, red for overdue. The parent can click into any requirement to see details (e.g., clicking "Instructional Days" shows daily activity log with hours by subject). A countdown timer displays days until the annual report deadline. If any requirement is incomplete or approaching deadline, the dashboard shows actionable alerts like "Testing deadline in 30 days: Register for standardized test" with a button to schedule testing. Parents can filter the dashboard by child if they have multiple students.

## Service Boundary Outcomes
The Reporting Service owns compliance status aggregation from multiple sources: instructional hours from Student Service (via `LearningActivityLoggedEvent`), test results from Assessment Service (via `AssessmentCompletedEvent`), portfolio evidence count from Media Service (via `PortfolioEvidenceUploadedEvent`), and state requirements from Configuration Service. The dashboard uses CQRS read models with eventual consistency—status updates appear within 2 seconds of underlying data changes via event-driven materialized views. The Configuration Service publishes `ComplianceDeadlineApproachingEvent` 30 days before deadlines, triggering dashboard alerts. Dashboard load SLO: p95 < 500ms, real-time updates p95 < 2s.

## Functional Requirements

**Must:**
- Display state-specific compliance requirements loaded from Configuration Service (e.g., "New York: 180 days, 12 subjects, testing grades 4-8")
- Show visual progress indicators (progress bars, percentages) for quantitative requirements (instructional days/hours, subject coverage)
- Display status badges (green/yellow/red) for each requirement based on completion and deadline proximity
- Aggregate instructional hours by subject from daily activity logs (via Student Service data)
- Show testing status: completed tests with scores, upcoming required tests, overdue tests
- Display portfolio evidence count and link to portfolio gallery
- Present deadline countdown for annual report submission (e.g., "89 days until June 30 deadline")
- Generate actionable alerts for incomplete requirements (e.g., "Action needed: Grade 3 testing required by May 15")
- Allow parent to switch between students if managing multiple children (student selector)
- Auto-refresh status every 30 seconds or update via real-time event push (SignalR/WebSockets)

## Constraints / Non-Goals

- **Not** providing legal advice or guaranteeing compliance (disclaimer: parents responsible for interpreting laws)
- **Not** auto-filing reports with state agencies (parent downloads/submits reports manually)
- **Not** supporting multi-state compliance (if family moves mid-year, defer to manual adjustment)
- **Not** enforcing minimum requirements (dashboard shows status, doesn't block activities if behind)
- Dashboard shows aggregated status; drilling into details requires navigation to detailed reports
- Real-time updates are eventual consistency (2-second lag acceptable)

---

**Acceptance Signals (Seed)**:
1. ✅ Parent in California with 120 days logged sees dashboard: "Instructional Days: 120/180 (67%)" in yellow, "Required Subjects: 5/5 covered" in green, "Testing: Optional" in gray, and alert "60 days remaining—log 60 more days by June 1"
2. ✅ Parent in Pennsylvania with testing due in 15 days sees red alert "Action Required: Grade 5 testing due May 15—only 15 days remaining" with button "Schedule Test"
3. ✅ Parent switches from Emma's dashboard (grade 3, on track) to Lucas's dashboard (grade K, 90 days logged) and sees different compliance status per child

**Handoff to /plan**:
- Define REST API endpoint `GET /api/v1/compliance/dashboard/{studentId}` returning compliance summary DTO
- Specify response DTO: `{ state, requirements: [{ type, description, status, progress, deadline, actionNeeded, actionUrl }] }`
- Implement CQRS read model: ComplianceSummary table materialized from events (LearningActivityLoggedEvent, AssessmentCompletedEvent, PortfolioEvidenceUploadedEvent)
- Create event subscriber in Reporting Service to update read model on relevant events
- Implement deadline calculation logic based on state rules (e.g., "Annual report due June 30 if enrolled Sep 1")
- Design dashboard UI in Figma with progress bars, status badges, alert cards
- Set up SignalR hub for real-time dashboard updates (optional: poll every 30s if WebSockets not available)
- Write Reqnroll BDD feature file for compliance dashboard scenarios

**Open Questions for /plan**:
- Should we send proactive email/SMS alerts when deadlines approach (not just show in dashboard)?
- Should we provide compliance "score" (e.g., "85% compliant") or just individual requirement status?
- Do we need multi-year historical compliance view (e.g., show last 3 years' annual reports)?
- Should we recommend specific actions (e.g., "Suggestion: Log 2 hours/day for next 30 days to reach goal")?
