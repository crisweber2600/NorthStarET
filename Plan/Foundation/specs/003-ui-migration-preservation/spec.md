# Spec: UI Migration with Preservation Strategy

Short Name: ui-migration-preservation
Layer: Foundation
Status: Draft (Specification)
Version: 0.1.0
Created: 2025-11-20

## Feature
Migrate legacy AngularJS UI to modern framework (Angular 18 primary; Blazor optional for future) while preserving existing layouts, workflows, styling, accessibility, and user preferences with pixel‑parity validation.

## Business Value
Modernizes front-end stack without retraining users or disrupting classroom workflows. Reduces technical debt, improves maintainability, enables modern tooling (Playwright, Webpack/Vite, CI visual regression) and sets foundation for future incremental enhancements.

## Target Layer
Foundation

## Actors
- Teachers / Staff Users
- UI Migration Team
- QA / Visual Regression Automation
- API Gateway (unchanged contracts)

## Assumptions
- Existing AngularJS app stable and documented.
- API contracts preserved via YARP gateway.
- Playwright available for automated visual regression tests.
- No redesign scope (strict preservation mandate).

## Constraints
- Must retain identical workflows (steps count, navigation paths).
- No Figma assets required (parity migration, constitution exemption).
- Visual diff tolerance ≤1% pixel difference per screen.
- Coexistence period allows both AngularJS and Angular 18 components.

## Scenarios
(Condensed from source scenario file for parity goals.)

### Scenario 1: Student Dashboard Screen Migration
Given legacy AngularJS dashboard exists
And it shows student info, grades, attendance, assessments
When team analyzes dashboard
Then layout/components/data bindings are documented
And new Angular 18 component matches layout
And same data service calls used
And screenshot comparison verifies parity
And workflows remain identical

### Scenario 2: Teacher Viewing Student List (Before and After Migration)
Given teacher views legacy student list with columns Name, Grade, Enrollment Date, Status
And sorting, filtering, pagination exist
When viewing migrated list
Then layout and column order identical
And interactions behave identically
And visual styling matches
And no retraining required

### Scenario 3: Assessment Entry Form Migration
Given legacy form with validation (score ranges, required fields)
When migrated
Then field positions preserved
And validation + error messages identical
And tab order + keyboard navigation preserved
And submission flow unchanged

### Scenario 4: Incremental Component Migration
Given 150 legacy components
And migration is incremental
When student module migrated first
Then migrated components deploy
And unmigrated remain AngularJS
And micro-frontend coexistence enabled
And navigation seamless between versions

### Scenario 5: Visual Regression Testing During Migration
Given screen migrated
When QA runs visual regression
Then Playwright captures legacy + new screenshots
And pixel comparison executed
And deviations flagged failing test
And discrepancies resolved pre-deploy

### Scenario 6: API Integration Without Backend Changes
Given legacy calls NS4.WebAPI endpoints
When new Angular 18 app built
Then same endpoints consumed via YARP
And JSON formats unchanged
And works with legacy + new microservices transparently

### Scenario 7: User Settings and Preferences Preservation
Given teacher dashboard preferences stored
When accessing migrated UI
Then preferences load identically
And settings panel unchanged
And no data loss occurs

### Scenario 8: Responsive Design Maintenance
Given legacy breakpoints (1920, 768, 375)
When migrated
Then same breakpoints maintained
And layouts adapt identically
And no new mobile views introduced
And touch interactions preserved

### Scenario 9: Navigation and Menu Structure Preservation
Given left sidebar menu with defined items
When migrated
Then structure/order/icons/labels identical
And expand/collapse behavior preserved
And active highlighting same

### Scenario 10: Search and Filter Functionality Preservation
Given student search page filters
When migrated
Then filter positions & algorithms identical
And wildcard + clear all behavior preserved
And results format identical

### Scenario 11: Print and Export Features Preservation
Given print roster feature
When migrated
Then print button location unchanged
And print preview layout identical
And exported PDF matches legacy
And print CSS preserved

### Scenario 12: Accessibility Features Maintained
Given legacy shortcuts and screen reader support
When migrated
Then shortcuts still work
And ARIA labels preserved/improved
And tab order & focus management identical

## Non-Functional Requirements
- Visual parity: ≤1% diff threshold per snapshot.
- Accessibility: Maintain WCAG 2.1 AA, no regressions.
- Performance: Equivalent or faster initial load & interaction.
- Logging: Migration actions tagged; error parity ensured.

## Acceptance Criteria Summary
All scenarios validated via automated functional tests + visual regression + accessibility scan (axe). No increase in support tickets post-cutover.

## Out of Scope
- Design enhancements, new UX patterns, theme overhaul.

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Hidden legacy inline styles | Inventory + stylesheet extraction |
| API subtle contract drift | Contract snapshot tests |
| Visual regression flakiness | Stable wait conditions, deterministic data fixtures |
| Coexistence routing bugs | Clear micro-frontend router adapter tests |

## Initial Roadmap
1. Inventory legacy components + screenshots.
2. Establish visual baseline repository.
3. Build micro-frontend shell (Angular 18 host + legacy iframe/bridge).
4. Migrate student dashboard module (pilot).
5. Implement Playwright visual regression pipeline.
6. Migrate remaining modules per phased schedule.
7. Decommission AngularJS after final parity sign-off.

## Audit & Compliance
Log component migration completion events with timestamp, module, parity result, and reviewer.

---
Generated manually (parity migration – no spec agent invocation).