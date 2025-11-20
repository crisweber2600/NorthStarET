Feature: Configuration Service: Multi-Tenant Settings Management
Short Name: configuration-service
Target Layer: CrossCuttingConcerns
Business Value: Centralize hierarchical multi-tenant configuration (system/district/school) enabling consistent isolation, inheritance, auditing, and extensibility for calendars, grading, compliance and custom attributes across districts.

Scenarios:
Scenario 1: District Administrator Creates District Settings
Given a system administrator with district admin privileges
And setting up a new school district
When entering configuration (name, state, timezone, academic year start, contact email)
Then a district record created with tenant_id
And DistrictCreatedEvent published
And Identity Service creates initial admin
And Student Service prepares enrollment
And district appears in list.

Scenario 2: Configure Academic Calendar for School Year
Given district "lincoln-usd" exists
And admin defines academic calendar (first day, last day, breaks, holidays)
Then calendar stored with tenant isolation
And schools inherit calendar
And Section Service uses for scheduling
And Attendance Service uses for tracking.

Scenario 3: Create School Within District
Given district exists
When creating school (name, principal, address, grade levels, capacity)
Then school created with district tenant_id
And SchoolCreatedEvent published
And school appears in directory
And teachers assignable, students enrollable.

Scenario 4: Configure Grade Levels and Subjects
Given school exists
When administrator configures grade levels and subjects per range
Then configuration saved with tenant isolation
And Assessment Service uses for assignment
And Section Service uses for scheduling
And teachers see relevant sets.

Scenario 5: Multi-Tenant Configuration Isolation
Given two districts with distinct settings
When Lincoln admin views configuration
Then only Lincoln settings visible
And Jefferson data hidden
And RLS enforces isolation
And no leakage occurs.

Scenario 6: System-Wide Settings vs. District-Specific
Given system defaults and district overrides
When retrieving configuration value
Then district-specific returned if present else system default
And certain keys overridable (grading scale, attendance rules, notifications)
And protected keys (security policies, retention) non-overridable
And hierarchy enforced.

Scenario 7: Custom Attributes and Fields
Given districts have unique data needs
When admin creates custom student attributes (transportation, dietary, after-school program)
Then attributes stored
And Student Service extends records
And visibility restricted to district
And reports filter by attributes.

Scenario 8: Grading Scale Configuration
Given district configures grading scales by level ranges
When scales defined (elementary mastery codes, numeric for middle, A+/- for high school)
Then scales stored per range
And Assessment Service uses for scoring
And Reporting Service for report cards
And conversion centralized.

Scenario 9: State-Specific Compliance Settings
Given states differ in compliance
When configuring California district
Then CA-specific fields enabled (CALPADS, CELDT, schedules)
When configuring Texas district
Then TX fields enabled (PEIMS, STAAR, TEA tracking)
And each district sees only relevant compliance settings.

Scenario 10: Navigation Menu Customization
Given roles need tailored menus
When configuring teacher menu
Then teachers see Students, Assessments, Gradebook, Attendance
When configuring admin menu
Then admins see Dashboard, Reports, Settings, User Management
And menus role-based & tenant-specific
And UI renders dynamically.

Scenario 11: Notification and Email Templates
Given districts customize communications
When admin defines templates (welcome, enrollment, parent invite, report card)
Then templates stored with branding & merge fields
And emails sent using district-specific templates
And SMTP settings per district.

Scenario 12: Configuration Change Audit Trail
Given configuration changes are critical
When admin alters a critical setting
Then change logged (user, timestamp, old, new, name)
And audit trail maintained & immutable
And history viewable and revertable
And tenant isolation preserved.

Acceptance Criteria:
1. Hierarchical resolution: school override > district override > system default.
2. RLS + global filters enforce tenant isolation across tables.
3. Domain events emitted (DistrictCreated, SchoolCreated, ConfigurationChanged, CalendarUpdated, GradingScaleChanged).
4. Custom attributes extensible without schema change for other tenants.
5. Caching strategy (Redis per-tenant keys) reduces config read latency <50ms P95.
6. Audit trail immutable with revert capability where allowed.
7. Compliance flags enable state-specific configuration surfaces.
8. Navigation & templates delivered dynamically from configuration store.
9. Performance: create district <200ms, get settings <50ms P95.
10. Security: role-based modification controls, protected keys guarded, RLS enforced.
