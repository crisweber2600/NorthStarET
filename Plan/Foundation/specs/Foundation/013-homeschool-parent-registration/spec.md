# /specify: Homeschool Parent Account Registration

## Feature Title
Homeschool parent account registration with state-specific compliance initialization

## Goal / Why
Enable homeschool parents to register for the NorthStar LMS and establish their homeschool organization with appropriate state compliance tracking. Unlike traditional district users, homeschool parents need a unified "educator + administrator + parent" role that gives them full control over their children's education while maintaining compliance with their state's homeschool regulations. This creates the foundation for all homeschool features.

## Intended Experience / What
A parent visits the registration page and selects "I'm a homeschool parent" from user type options. They enter their email, password, full name, and select their state/jurisdiction from a dropdown. The system prompts them to create a homeschool name (e.g., "Weber Family Academy") and enter their home address for compliance records. Upon submission, the system creates their account with "Homeschool Educator" role, establishes a homeschool organization entity, and loads state-specific compliance rules (required subjects, testing requirements, instructional hour minimums). The parent receives a welcome email with onboarding checklist and is redirected to their homeschool dashboard showing the compliance requirements for their state. They can immediately begin enrolling their children and creating learning plans.

## Service Boundary Outcomes
The Identity Service owns homeschool parent authentication and "Homeschool Educator" role assignment. The Configuration Service owns homeschool organization creation and state compliance rule association. These services coordinate via `HomeschoolParentRegisteredEvent` which triggers compliance initialization. Account creation is idempotent within 10 minutes (duplicate email returns existing account). The dashboard loads compliance rules asynchronously with eventual consistency—rules appear within 2 seconds via event-driven rule loading. Registration SLO: p95 < 500ms for account creation, p95 < 2s for full dashboard initialization.

## Functional Requirements

**Must:**
- Create parent account with email/password and assign "Homeschool Educator" role (combines parent, teacher, admin privileges)
- Allow parent to select state/jurisdiction from dropdown of all 50 US states + DC + territories
- Prompt parent to create homeschool organization name (e.g., "Smith Family Academy", "Our Homeschool")
- Collect home address (street, city, state, zip) for compliance documentation
- Load state-specific compliance rules from Configuration Service (required subjects, testing grades, minimum hours)
- Display compliance checklist on initial dashboard (e.g., "Annual notice required", "180 instructional days required")
- Send welcome email with onboarding guide and links to help resources
- Publish `HomeschoolParentRegisteredEvent` to message bus for downstream initialization

## Constraints / Non-Goals

- **Not** collecting payment information during registration (separate subscription flow)
- **Not** verifying parent's legal authority to homeschool (disclaimer: parent responsible for compliance)
- **Not** auto-enrolling children during parent registration (separate "Enroll Student" flow)
- **Not** supporting non-US homeschool regulations in initial release (international expansion deferred)
- Compliance rule loading is asynchronous; dashboard shows "Loading..." state briefly if rules take >500ms

---

**Acceptance Signals (Seed)**:
1. ✅ Parent from Texas registers, creates "Garcia Homeschool", and sees compliance checklist with "Required subjects: Reading, Math, Grammar, Spelling, Citizenship" and "No state testing required"
2. ✅ Parent from New York registers, creates "Chen Family School", and sees compliance checklist with "12 required subjects" and "IHIP submission required" and "Testing in grades 4-8"
3. ✅ Parent attempts to register with duplicate email and receives friendly error "Account already exists with this email"

**Handoff to /plan**:
- Define REST API endpoint `POST /api/v1/auth/register/homeschool`
- Specify request DTO: `{ email, password, firstName, lastName, state, homeschoolName, addressLine1, city, stateProvince, postalCode }`
- Detail database schema: Users table (role: "HomeschoolEducator"), Homeschools table (parentId, name, state, address)
- Implement state compliance rules lookup from Configuration Service via HTTP or message bus
- Create welcome email template with Razor/Liquid syntax
- Set up MassTransit contract for `HomeschoolParentRegisteredEvent`
- Configure RBAC rules for "Homeschool Educator" role (full access to own homeschool, zero access to other homeschools/districts)

**Open Questions for /plan**:
- Should we verify email address before granting full access (email confirmation flow)?
- Should we collect phone number for compliance notifications (SMS alerts)?
- Do we need CAPTCHA to prevent bot registrations?
- Should homeschool name be globally unique or can multiple families use same name?
