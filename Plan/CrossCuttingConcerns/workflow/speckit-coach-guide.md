# Spec-Kit Coach User Guide

> **Interactive Workflow Orchestrator for Spec-Driven Development**

The Spec-Kit Coach is an AI agent that guides you through the Spec-Driven Development workflow by detecting your current state, validating constitution compliance, and orchestrating handoffs to specialized agents.

---

## Quick Start

### Invoke the Coach

```bash
# General guidance (detects current state and suggests next step)
@copilot @speckit.coach what should I do next?

# Start new feature
@copilot @speckit.coach I want to add [feature description]

# Get help with current feature
@copilot @speckit.coach help me with this feature

# Check constitution compliance
@copilot @speckit.coach validate my spec/plan/tasks
```

---

## What the Coach Does

### 1. **Detects Workflow State**
Automatically identifies:
- Current feature branch and directory
- Available artifacts (spec.md, plan.md, tasks.md)
- Current workflow phase (intake → spec → plan → tasks → implementation)
- Git branch status

### 2. **Validates Vertical Slice Scope**
Ensures features are correctly sized:
- ✅ Single user-visible value delivery
- ✅ Fits one git branch (3-7 days typically)
- ✅ Testable with 3-8 acceptance scenarios
- ✅ One clear owner/bounded context

**Proposes splits** when features are too large (>500 words, multiple unrelated capabilities, 8+ scenarios)

### 3. **Validates Constitution Compliance**
Checks artifacts against 7 core principles:
1. Clean Architecture & Aspire Orchestration
2. Test-Driven Quality Gates
3. UX Traceability & Figma Accountability
4. Event-Driven Data Discipline
5. Security & Compliance Safeguards
6. Mono-Repo Layer Isolation
7. Tool-Assisted Development Workflow

**Surfaces violations:**
- 🚨 **CRITICAL** - Blocks progress until resolved
- ⚠️ **WARNING** - Recommendations for improvement
- ✅ **COMPLIANT** - Passing validation

### 4. **Injects MCP Tool Reminders**
Context-aware tool recommendations based on feature type:

**UI Work** → `#figma`, `#playwright`, `#chromedevtools`  
**Service/API** → `#microsoft.docs.mcp`, `#think`  
**Database** → `#microsoft.docs.mcp` (EF Core guidance)  
**Complex** → `#sequential-thinking` (mandatory at start)

### 5. **Orchestrates Agent Handoffs**
Guides you to the right agent with pre-filled prompts:
- `@speckit.specify` - Create feature specification
- `@speckit.clarify` - Resolve unclear requirements
- `@speckit.plan` - Build implementation plan
- `@speckit.tasks` - Generate task breakdown
- `@speckit.analyze` - Validate consistency
- `@speckit.implement` - Execute implementation

---

## User Journeys

### Journey 1: Starting a New Feature

**You:** `@copilot @speckit.coach I want to add district administrator management`

**Coach:**
```
## 🎯 Current State
No feature branch detected. Starting fresh.

## 📐 Intake Questions (4 max)
1. User & Goal: Who is the primary user and what outcome do they need?
2. Location: Where in the product does this live?
3. Layer: Foundation (core migration), DigitalInk (ink features), or Other?
4. Must-Haves: Top 3-6 behaviors this MUST support?

⚠️ Reminder: Have you run #think yet? Mandatory before complex work.
```

**You:** Answers questions:
- User: District admin
- Location: New admin portal page at /admin/districts
- Layer: Foundation
- Must-haves: Create district with name/suffix, list all districts, soft-delete districts

**Coach:**
```
## 📐 Vertical Slice Check
✅ Feature is correctly scoped as a vertical slice:
- Delivers user-visible value: Yes (admin can manage districts)
- Fits single branch: Yes (CRUD operations, no complex workflows)
- 3-8 scenarios: Yes (create, list, delete = 3 core scenarios)
- Single owner: Yes (Foundation layer, admin service)

Target Layer: Foundation
Cross-Layer Dependencies: None

## ⚙️ MCP Tools Required
- #think - Plan domain model and UI components
- #figma/dev-mode-mcp-server - Extract design from [FIGMA_URL needed]
- #microsoft.docs.mcp - EF Core 9 + Aspire patterns
- #mcp_playwright_browser_* - Test admin portal flows

## 🚀 Recommended Next Step
Ready to create specification with @speckit.specify

[Pre-filled handoff prompt provided]
Click "Create Feature Specification" handoff to proceed.
```

**You:** Clicks handoff → `@speckit.specify` runs → spec.md created

---

### Journey 2: Feature Scope Too Large (Split Proposal)

**You:** `@copilot @speckit.coach I want to build a complete student information system with enrollment, grades, attendance, behavior tracking, parent portal, and reporting dashboard`

**Coach:**
```
## 📐 Vertical Slice Check
⚠️ SPLIT REQUIRED - Feature spans too much scope for one branch:
- Multiple unrelated capabilities (enrollment, grades, attendance, behavior, parent, reports)
- Will generate 6 separate bounded contexts
- Will exceed 500-word spec limit by 10x
- Requires 30+ acceptance scenarios

Proposed Split:
1️⃣ "student-enrollment-crud" (P1) - Core student create/edit/list (MVP)
2️⃣ "student-grades-tracking" (P2) - Grade entry and calculation
3️⃣ "student-attendance-tracking" (P2) - Daily attendance recording
4️⃣ "student-behavior-tracking" (P3) - Behavior incident logging
5️⃣ "parent-portal-access" (P3) - Parent view of student data
6️⃣ "student-analytics-dashboard" (P3) - Reporting and analytics

Dependencies:
- All features depend on #1 (enrollment) for student entity
- #2, #3, #4 can be built in parallel after #1
- #5 depends on #2, #3, #4 (needs data to display)
- #6 depends on all others (aggregates all data)

Recommendation: Start with slice #1 (student-enrollment-crud) because it:
- Delivers immediate value (can onboard students)
- Unblocks all other work (provides student entity)
- Is independently testable and deployable
- Fits 150-300 word spec target

Which slice would you like to proceed with?
```

**You:** `Let's start with #1`

**Coach:**
```
✅ Great choice. Proceeding with "student-enrollment-crud" (P1).

[Continues with MCP tool reminders and handoff to @speckit.specify]
```

---

### Journey 3: Existing Spec, What's Next?

**You:** `@copilot @speckit.coach what should I do next?`

**Coach detects:** spec.md exists, no plan.md

**Coach:**
```
## 🎯 Current State
✅ Feature branch: 005-district-admin-crud
✅ Spec complete: specs/Foundation/005-district-admin-crud/spec.md
❌ Plan not started

## 📋 Constitution Validation (v2.0.0)
[Reads spec.md...]

✅ Principle 1: Clean Architecture - COMPLIANT
✅ Principle 3: UX Traceability - COMPLIANT (Figma: figma.com/file/abc/frame-22)
⚠️ Principle 7: Tool-Assisted - WARNING: No MCP usage yet
✅ Principle 6: Layer Isolation - COMPLIANT (Foundation layer)

Clarification Check:
✅ No [NEEDS CLARIFICATION] markers found - spec is clean

## 🚀 Recommended Next Step
Spec is complete. Ready to build implementation plan.

Click "Build Implementation Plan" handoff or run:
@speckit.plan Create implementation plan for district admin CRUD

What plan will generate:
- plan.md (architecture, data model, API contracts)
- data-model.md (EF Core entities, DbContext)
- contracts/ (DTOs)
- quickstart.md (local setup)
```

---

### Journey 4: Constitution Violation Detected

**You:** `@copilot @speckit.coach validate my plan`

**Coach detects:** plan.md exists, reads it, finds violations

**Coach:**
```
## 📋 Constitution Validation (v2.0.0)

✅ Principle 1: Clean Architecture - COMPLIANT
🚨 Principle 2: Test-Driven Quality Gates - VIOLATION
   - No Reqnroll BDD feature files defined
   - No Playwright test plan for UI journeys
   - TDD Red→Green workflow not documented
⚠️ Principle 3: UX Traceability - WARNING
   - UI task "Create District Modal" missing Figma link
✅ Principle 4: Event-Driven - COMPLIANT
🚨 Principle 5: Security - VIOLATION
   - Authorization checks not defined in Application layer
   - No RBAC validation before district deletion
✅ Principle 6: Layer Isolation - COMPLIANT
✅ Principle 7: Tool-Assisted - COMPLIANT

Summary:
- 🚨 CRITICAL Issues: 2 (blocks implementation)
- ⚠️ WARNINGS: 1 (recommendation)
- ✅ COMPLIANT: 5 principles

CRITICAL Issues (must fix before proceeding):

1. Principle 2 - Test-Driven Quality Gates:
   Remediation:
   - Create specs/Foundation/005-district-admin-crud/features/district-crud.feature
   - Define Given/When/Then scenarios for create/list/delete
   - Add Playwright test plan section to plan.md
   - Document Red→Green workflow in tasks.md

2. Principle 5 - Security & Compliance:
   Remediation:
   - Add authorization check in Application/Commands/CreateDistrictCommand.cs
   - Validate user has "DistrictAdmin" role before execution
   - Add integration test for unauthorized access (403 expected)

Recommendations:
- Add Figma link to "Create District Modal" task or label "Skipped — No Figma"

❌ Cannot proceed to @speckit.tasks until CRITICAL violations resolved.

Suggested action: Fix plan.md and re-run validation.
```

---

### Journey 5: Implementation in Progress

**You:** `@copilot @speckit.coach status check`

**Coach detects:** tasks.md exists, some marked `[X]` complete

**Coach:**
```
## 🎯 Current State
✅ Feature branch: 005-district-admin-crud
✅ Spec: specs/Foundation/005-district-admin-crud/spec.md
✅ Plan: specs/Foundation/005-district-admin-crud/plan.md
✅ Tasks: specs/Foundation/005-district-admin-crud/tasks.md
🔄 Implementation: IN PROGRESS

Progress: 7 of 15 tasks completed (46%)

Completed Tasks:
- [X] Domain entity: District (Id, Name, Suffix, DeletedAt)
- [X] Repository interface: IDistrictRepository
- [X] EF Core DbContext configuration
- [X] Create district command handler with validation
- [X] Unit tests for domain entity
- [X] Unit tests for create command handler
- [X] Database migration Add_Districts_Table

Next Task:
[ ] [US1] Implement list districts query handler
    - File: Application/Queries/ListDistrictsQueryHandler.cs
    - Dependencies: None (can start now)
    - Acceptance: Returns all non-deleted districts ordered by Name

## 📋 Constitution Check
✅ All principles compliant
✅ TDD Red→Green evidence captured for completed tasks
✅ BDD feature file present: district-crud.feature

## 🚀 Recommended Next Step
Continue implementation with @speckit.implement

Pre-task checklist:
- [ ] Run dotnet test → capture Red state output
- [ ] Implement ListDistrictsQueryHandler
- [ ] Run dotnet test → capture Green state output
- [ ] Commit changes with message: "feat: add list districts query"
- [ ] Mark task [X] in tasks.md

Click "Start Implementation" handoff to continue.
```

---

## Common Scenarios

### Scenario: Parking HOW Details

**You:** `@copilot @speckit.coach I want to add user authentication using Duende IdentityServer with PostgreSQL for token storage and Redis for session caching`

**Coach:**
```
✅ Got it. I'll note the implementation details for /plan.

Feature scope: User authentication

To be detailed in /plan:
- Framework: Duende IdentityServer
- Database: PostgreSQL for token storage
- Caching: Redis for session caching

For specification, I need user-focused information:
1. Who needs to authenticate? (district admin, teacher, student?)
2. What authentication methods? (username/password, SSO, MFA?)
3. What happens after successful login? (redirect to dashboard, return token?)
4. Any session requirements? (timeout duration, remember-me, single sign-on?)
```

---

### Scenario: Missing Figma Design

**You:** `@copilot @speckit.coach I need to build a student enrollment form`

**Coach:**
```
⚠️ UI Task Detected: Student enrollment form

## 📋 Constitution Principle 3: UX Traceability
UI tasks require Figma design link BEFORE implementation.

Options:
1. ✅ Provide Figma frame URL → Proceed with implementation
2. ⏸️ Label task "Skipped — No Figma" → Pause until design available
3. 🔄 Is this a migration task? (preserving existing UI) → No Figma required

If this is migrating OldNorthStar's existing enrollment form:
- Mark as migration task in spec.md
- Document original UI behavior
- Use Playwright to capture current UI as baseline
- No Figma required (preserving existing UX)

Which option applies to your scenario?
```

---

### Scenario: Layer Assignment Unclear

**You:** `@copilot @speckit.coach I want to add real-time handwriting recognition`

**Coach:**
```
## 📐 Layer Identification Required

Your feature involves: Real-time handwriting recognition

Layer options:
1. **Foundation** - Core OldNorthStar migration (features 001-020)
   Use if: Replacing existing OldNorthStar handwriting module
   
2. **DigitalInk** - Digital ink capabilities (features 020+)
   Use if: New capability or significant enhancement to ink functionality
   
3. **Other** - New layer being introduced
   Use if: Entirely new business domain (requires architecture review)

Based on "real-time handwriting recognition", this likely belongs to:
→ **DigitalInk** layer (feature 020+)

Reasoning:
- Digital ink is an established layer (specs/DigitalInk/)
- Handwriting recognition is core ink functionality
- Will use Foundation shared infrastructure (Identity, Config)

Cross-layer dependencies allowed:
✅ Identity & Authentication (Entra ID)
✅ Configuration Service (feature flags)
✅ ServiceDefaults (Aspire patterns)
❌ Direct service-to-service calls to Foundation services (use events)

Confirm layer selection: DigitalInk?
```

---

## MCP Tool Integration Examples

### Example 1: UI Development Workflow

```
You: @copilot @speckit.coach I'm implementing the district admin modal

Coach:
⚙️ MCP Tools Required for UI Work:

Step 1: Extract design context
#figma/dev-mode-mcp-server nodeId="22-67" fileKey="abc123"
→ Get spacing, colors, typography, interaction states

Step 2: Implement Razor component
[Build the modal based on Figma specs]

Step 3: Test with Playwright (Red phase)
pwsh tests/ui/playwright.ps1 > phase-red-playwright.txt
→ Capture failing test output

Step 4: Debug if needed
#chromedevtools/chrome-devtools-mcp
- #mcp_chromedevtool_take_snapshot → DOM structure
- #mcp_chromedevtool_list_console_messages → JS errors
- #mcp_chromedevtool_list_network_requests → API calls

Step 5: Verify with Playwright (Green phase)
pwsh tests/ui/playwright.ps1 > phase-green-playwright.txt
→ Capture passing test output

Attach both Red/Green transcripts to phase review.
```

### Example 2: Service Development Workflow

```
You: @copilot @speckit.coach I'm building the district repository with EF Core

Coach:
⚙️ MCP Tools Required for Service Work:

Step 1: Plan approach
#think "Design district repository with soft-delete filtering"

Step 2: Research EF Core patterns
#microsoft.docs.mcp
#mcp_microsoft_doc_microsoft_docs_search query="EF Core 9 PostgreSQL soft delete"
#mcp_microsoft_doc_microsoft_code_sample_search query="IQueryable soft delete filter" language="csharp"

Step 3: Implement with TDD
dotnet test > phase-red-dotnet-test.txt  (Red)
[Write repository implementation]
dotnet test > phase-green-dotnet-test.txt  (Green)

Step 4: Review official examples
#mcp_microsoft_doc_microsoft_docs_fetch url="https://learn.microsoft.com/ef-core/querying/filters"
→ Validate against Microsoft best practices
```

---

## Constitution Principles Quick Reference

| Principle | Key Validations | CRITICAL Violations |
|-----------|----------------|---------------------|
| **1. Clean Architecture** | UI→App→Domain→Infra, Aspire hosting | UI directly calling Infrastructure |
| **2. Test-Driven Quality** | Red→Green transcripts, BDD features, Playwright tests, ≥80% coverage | Missing test strategy, no Red→Green evidence |
| **3. UX Traceability** | Figma links for UI tasks (new features only), migration tasks marked | UI task without Figma or migration marker |
| **4. Event-Driven Data** | Async events, idempotent commands, tenant isolation (RLS + tenant_id) | Sync cross-service calls without fallback |
| **5. Security** | RBAC in Application layer, no secrets in code/logs | Hardcoded secrets, authorization bypass |
| **6. Layer Isolation** | Layer identified, shared infra only, no circular deps | Cross-layer service coupling, circular dependency |
| **7. Tool-Assisted** | MCP tools used per workflow (`#think`, `#figma`, `#microsoft.docs.mcp`) | Complex work without `#think` at start |

---

## Tips & Best Practices

### When to Invoke the Coach

✅ **Do invoke:**
- Starting any new feature
- Feeling stuck or unsure what's next
- Need constitution validation
- Before phase boundaries (spec→plan, plan→tasks, tasks→implementation)
- After completing a phase (to confirm next step)
- When feature scope feels too large

❌ **Don't invoke:**
- For implementation details during coding (use `@speckit.implement`)
- To clarify specific requirements (use `@speckit.clarify`)
- For consistency analysis (use `@speckit.analyze`)
- During active TDD Red→Green cycles (focus on implementation)

### Handling Split Proposals

When the coach proposes a split:
1. **Review the reasoning** - Why is it too large?
2. **Evaluate proposed slices** - Do they make sense for your product?
3. **Choose starting slice** - Which delivers core value first?
4. **Document dependencies** - What must be built in order?
5. **Proceed with chosen slice** - Coach will guide to `@speckit.specify`

### Constitution Violations

**CRITICAL violations:**
- Block all forward progress
- Must be fixed before next phase
- Often require spec/plan/tasks updates
- Coach provides specific remediation steps

**WARNINGS:**
- Recommendations for improvement
- Don't block progress but should be addressed
- May become CRITICAL in future phases

### MCP Tool Adoption

The coach will remind you of required tools, but it's your responsibility to:
- Run `#think` or `#sequential-thinking` at session start (mandatory)
- Use `#figma/dev-mode-mcp-server` before UI implementation
- Use `#microsoft.docs.mcp` for .NET/Azure research
- Use `#playwright` and `#chromedevtools` for UI testing/debugging

**Tool Chain Example:**
```
Session Start → #think (plan)
Research → #microsoft.docs.mcp (documentation)
UI Design → #figma/dev-mode-mcp-server (extract specs)
UI Testing → #playwright (automate) + #chromedevtools (debug)
```

---

## Troubleshooting

### Coach says "Could not detect workflow state"

**Cause:** Not in git repository root or `.specify/` directory missing

**Fix:**
```bash
# Ensure you're in repository root
cd d:\source\NorthStarET-1

# Verify .specify/ exists
ls .specify/
```

### Coach says "Constitution file missing"

**Cause:** `.specify/memory/constitution.md` not found

**Fix:**
```bash
# Verify constitution exists
cat .specify/memory/constitution.md

# If missing, restore from template or git
git checkout .specify/memory/constitution.md
```

### Coach keeps proposing splits for small features

**Cause:** Feature description contains multiple unrelated concepts

**Fix:** Be more specific in your feature description:
- ❌ "I want to add user management and reporting"
- ✅ "I want to add user creation form"

### Coach doesn't suggest correct MCP tools

**Cause:** Feature keywords not recognized

**Fix:** Provide more context:
- ❌ "I want to build a thing"
- ✅ "I want to build a Razor page with form validation (UI work)"

---

## Advanced Usage

### Custom Layer Creation

If introducing a new layer beyond Foundation/DigitalInk:

```
You: @copilot @speckit.coach I want to add a payment processing layer

Coach:
⚠️ New layer detected: "payment processing"

This requires architecture review because:
- Not part of existing Foundation (001-020) or DigitalInk (020+)
- Introduces new business domain with distinct boundaries
- May need new shared infrastructure contracts

Recommended process:
1. Document layer purpose and scope in docs/architecture/layers.md
2. Define shared infrastructure contracts (if any)
3. Update constitution.md to add layer to approved list
4. Create specs/PaymentProcessing/ directory
5. Start with feature 030 (next available after DigitalInk)

Would you like to proceed with architecture review?
```

### Multi-Feature Coordination

When building features that depend on each other:

```
You: @copilot @speckit.coach I'm building "student-grades-tracking" which depends on "student-enrollment-crud" (currently in progress)

Coach:
📋 Dependency Detected:
- Your feature: student-grades-tracking (P2)
- Depends on: student-enrollment-crud (P1) - IN PROGRESS

Coordination Options:
1. ⏸️ Wait for P1 completion (recommended)
   - Ensures Student entity contract is stable
   - Avoids rework if P1 changes
   
2. 🔄 Proceed with assumed contract (risky)
   - Mock Student entity based on expected shape
   - Risk: May need refactoring when P1 merges
   
3. 🤝 Coordinate with P1 author
   - Ask for early contract definition in P1 plan
   - Build against stable interface, flexible implementation

Recommendation: Option 1 (wait) unless P1 completion is >1 week away.

If proceeding, document assumption in spec.md:
"Assumes Student entity from P1 (student-enrollment-crud) with properties: Id, FirstName, LastName, Grade, EnrollmentDate"
```

---

## FAQ

**Q: Do I need to invoke the coach for every feature?**  
A: Recommended but not mandatory. The coach helps enforce constitution compliance and catch scope issues early. Experienced contributors may go directly to `@speckit.specify`.

**Q: Can I skip the coach and go straight to implementation?**  
A: Technically yes, but risky. The coach validates vertical slice scope and constitution compliance before you invest time in implementation that might need refactoring.

**Q: What if I disagree with a split proposal?**  
A: Explain your reasoning to the coach. If you believe the feature is genuinely a single vertical slice, provide justification (e.g., "these capabilities are inseparable for user value"). The coach will re-evaluate.

**Q: How do I update the constitution if I think a principle is wrong?**  
A: Use `@speckit.constitution` to propose changes. Constitution updates require team discussion and approval (not unilateral changes).

**Q: Can the coach write code for me?**  
A: No. The coach is a workflow orchestrator, not an implementation agent. For implementation, use `@speckit.implement` which executes tasks with TDD Red→Green workflow.

**Q: What if my feature genuinely has no Figma design (not a migration)?**  
A: Label the task "Skipped — No Figma" and pause UI implementation until design arrives. Focus on service/domain logic first, or use `specs/{feature}/figma-prompts/` to document what design details are needed.

---

## Related Agents

- **@speckit.specify** - Creates feature specification (spec.md)
- **@speckit.clarify** - Resolves unclear requirements via Q&A
- **@speckit.plan** - Builds implementation plan and artifacts
- **@speckit.tasks** - Generates actionable task breakdown
- **@speckit.analyze** - Validates spec/plan/tasks consistency
- **@speckit.implement** - Executes implementation with TDD
- **@speckit.constitution** - Updates project constitution
- **@speckit.checklist** - Generates custom quality checklists
- **@speckit.taskstoissues** - Converts tasks to GitHub Issues

---

## Feedback & Improvements

This guide reflects Constitution v2.0.0 (2025-11-20). If you encounter:
- Missing scenarios not covered in this guide
- Constitution principles that seem contradictory
- Coach behavior that doesn't match this documentation

Please open a GitHub Issue or discuss in team channels.

---

**Last Updated:** November 20, 2025  
**Constitution Version:** 2.0.0  
**Coach Agent Version:** 1.0.0
