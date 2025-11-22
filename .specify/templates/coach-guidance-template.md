# Coach Guidance Output Template

Use this template structure when providing coaching guidance to users in the Spec-Driven Development workflow.

---

## 🎯 Current State

**Feature Branch**: `[###-feature-name]` or `None detected`  
**Current Phase**: `[none | spec | plan | tasks | implementation]`  
**Available Artifacts**:
- ✅ spec.md: `[path or N/A]`
- ✅ plan.md: `[path or N/A]`
- ✅ tasks.md: `[path or N/A]`
- ✅ Other: `[list any research.md, data-model.md, contracts/, etc.]`

**Branch Status**: `[clean | uncommitted changes | not on feature branch]`

---

## 📐 Vertical Slice Check

**Scope Assessment**: `[Right-sized ✅ | Needs Split ⚠️]`

### [If Right-Sized]
✅ **Feature is correctly scoped as a vertical slice:**
- Delivers user-visible value: `[Yes/No - explain]`
- Fits single git branch: `[Yes/No - explain]`
- Testable with 3-8 scenarios: `[Yes/No - explain]`
- Single owner/bounded context: `[Yes/No - explain]`

**Target Layer**: `[Foundation | DigitalInk | Other]`  
**Cross-Layer Dependencies**: `[None | Foundation shared infrastructure only]`

### [If Needs Split]
⚠️ **SPLIT REQUIRED** - Feature spans too much scope:

**Reasons**:
- [ ] Multiple unrelated capabilities or personas
- [ ] Multiple pages/screens with independent value
- [ ] Feature + global platform change
- [ ] Will exceed 500-700 words
- [ ] More than 8 acceptance scenarios
- [ ] Spans multiple layers without clear justification

**Proposed Split**:
1️⃣ **"[short-name-1]"** (P1) - `[Description - why this delivers core value first]`
2️⃣ **"[short-name-2]"** (P2) - `[Description - independent enhancement]`
3️⃣ **"[short-name-3]"** (P3) - `[Description - advanced capability]`

**Dependencies**: `[List any ordering constraints between slices]`

**Recommendation**: Start with slice #`[N]` because `[reasoning]`.

**Which slice would you like to proceed with?**

---

## 📋 Constitution Validation (v2.0.0)

**Constitution Version**: 2.0.0 (2025-11-20)  
**Artifacts Validated**: `[spec.md | plan.md | tasks.md | none yet]`

### Principle 1: Clean Architecture & Aspire Orchestration
`[✅ COMPLIANT | ⚠️ WARNING | 🚨 CRITICAL]`  
`[Details: what was checked, what was found, what needs fixing if non-compliant]`

### Principle 2: Test-Driven Quality Gates
`[✅ COMPLIANT | ⚠️ WARNING | 🚨 CRITICAL]`  
`[Details: TDD workflow, BDD features, Playwright tests, coverage]`

### Principle 3: UX Traceability & Figma Accountability
`[✅ COMPLIANT | ⚠️ WARNING | 🚨 CRITICAL]`  
`[Details: Figma links present, migration task exemptions, figma-prompts/]`

### Principle 4: Event-Driven Data Discipline
`[✅ COMPLIANT | ⚠️ WARNING | 🚨 CRITICAL]`  
`[Details: async patterns, idempotency, multi-tenant isolation]`

### Principle 5: Security & Compliance Safeguards
`[✅ COMPLIANT | ⚠️ WARNING | 🚨 CRITICAL]`  
`[Details: RBAC, secret management, layer boundaries]`

### Principle 6: Mono-Repo Layer Isolation
`[✅ COMPLIANT | ⚠️ WARNING | 🚨 CRITICAL]`  
`[Details: layer identified, cross-layer dependencies validated, shared infrastructure only]`

### Principle 7: Tool-Assisted Development Workflow
`[✅ COMPLIANT | ⚠️ WARNING | 🚨 CRITICAL]`  
`[Details: MCP tool usage documented, #think at session start]`

---

**Summary**:
- 🚨 **CRITICAL Issues**: `[count]` - `[blocks progress until resolved]`
- ⚠️ **WARNINGS**: `[count]` - `[recommendations for improvement]`
- ✅ **COMPLIANT**: `[count]` principles passing

**CRITICAL Issues (must fix before proceeding)**:
1. `[Principle N: Specific violation - how to remediate]`
2. `[Principle N: Specific violation - how to remediate]`

**Recommendations**:
1. `[Principle N: Suggestion for improvement]`
2. `[Principle N: Suggestion for improvement]`

---

## ⚙️ MCP Tools Required

**Detected Feature Type**: `[UI | Service/API | Database/Migration | Complex/Architectural]`

**Mandatory Tools** (based on feature type):

### [For UI Work]
```
- #think - Plan vertical slice and component architecture
- #figma/dev-mode-mcp-server - Extract design specs from [FIGMA_URL]
  (⚠️ Figma link required before implementation - label "Skipped — No Figma" if missing)
- #mcp_playwright_browser_navigate - Navigate to pages
- #mcp_playwright_browser_click - Simulate user interactions
- #mcp_playwright_browser_fill_form - Test form submissions
- #mcp_playwright_browser_take_screenshot - Visual verification
- #chromedevtools/chrome-devtools-mcp - Debug live UI issues
  - #mcp_chromedevtool_take_snapshot - DOM inspection
  - #mcp_chromedevtool_list_console_messages - JavaScript errors
  - #mcp_chromedevtool_list_network_requests - API call inspection
```

### [For Service/API Work]
```
- #think - Plan domain model and service boundaries
- #microsoft.docs.mcp - Search .NET/EF Core/Aspire documentation
- #mcp_microsoft_doc_microsoft_docs_search - Find relevant guides
- #mcp_microsoft_doc_microsoft_code_sample_search - Get code examples (language: csharp)
- #mcp_microsoft_doc_microsoft_docs_fetch - Full documentation pages
```

### [For Database/Migration Work]
```
- #think - Plan schema evolution and migration strategy
- #microsoft.docs.mcp - EF Core 9 migration best practices
- #mcp_microsoft_doc_microsoft_docs_search - Query: "EF Core 9 PostgreSQL migrations"
```

### [For Complex/Architectural Work]
```
- #think or #sequential-thinking - MANDATORY at session start
- #microsoft.docs.mcp - .NET Aspire orchestration patterns
- #mcp_microsoft_doc_microsoft_docs_search - Multi-service coordination
```

**Session Start Reminder**:
```
⚠️ Have you run #think or #sequential-thinking yet?
It's MANDATORY for planning complex work (per Constitution Principle 7).
```

---

## 🚀 Recommended Next Step

**Current Phase**: `[Detected from state - none/spec/plan/tasks/implementation]`  
**Next Agent**: `@speckit.[specify|clarify|plan|tasks|analyze|implement]`

### [Phase: No Feature Detected]
**Action**: Create Feature Specification

You need to start with `@speckit.specify` to create your feature branch and spec.md.

**Pre-Flight Checklist**:
- [ ] Ran `#think` to plan this feature? (MANDATORY)
- [ ] Feature is a vertical slice (single page/flow/service capability)?
- [ ] Know which layer this belongs to? (Foundation/DigitalInk/Other)
- [ ] Answered intake questions: user, goal, location, must-haves?

**Handoff Prompt** (click "Create Feature Specification" handoff):
```
Create specification for: [FEATURE TITLE]

Layer: [Foundation | DigitalInk | Other]
Primary User: [WHO]
Goal: [WHAT OUTCOME]
Location: [WHERE IN PRODUCT]

Must-Have Behaviors:
1. [BEHAVIOR 1]
2. [BEHAVIOR 2]
3. [BEHAVIOR 3]

Boundary Guarantees:
- Idempotency: [e.g., "10-minute deduplication for creates"]
- Consistency: [e.g., "eventual, 5-second SLO"]
- Latency: [e.g., "confirmation within 800ms"]

Non-Goals:
- [EXCLUSION 1]
- [EXCLUSION 2]

To be detailed in /plan:
- [HOW DETAIL 1]
- [HOW DETAIL 2]
```

### [Phase: Spec Exists, No Plan]
**Action**: Check Clarifications or Build Plan

Spec found at: `[FEATURE_SPEC path]`

**Clarification Check**:
- `[NEEDS CLARIFICATION]` markers found: `[count]`
  - `[MARKER 1: question]`
  - `[MARKER 2: question]`

**If clarifications needed**:
⚠️ Run `@speckit.clarify` first to resolve open questions (max 10 questions, 5 at a time).

**If spec is clean**:
✅ Ready to build implementation plan with `@speckit.plan`.

**Handoff**: Click "Build Implementation Plan" or run:
```
@speckit.plan Create implementation plan for [feature name]
```

**What plan will generate**:
- `plan.md` - Architecture decisions, data model, contracts
- `data-model.md` - EF Core entities, DbContext, migrations
- `contracts/` - Request/response DTOs, API contracts
- `quickstart.md` - Local development setup instructions

### [Phase: Plan Exists, No Tasks]
**Action**: Generate Task Breakdown

Plan found at: `[IMPL_PLAN path]`

Your plan defines the HOW. Now break it into actionable tasks with `@speckit.tasks`.

**Handoff**: Click "Generate Task Breakdown" or run:
```
@speckit.tasks Generate task breakdown for [feature name]
```

**What tasks will generate**:
- `tasks.md` - User-story-based task organization
- Parallel execution markers `[P]`
- Task-to-user-story mapping `[US1]`, `[US2]`, etc.
- Acceptance criteria for each task

### [Phase: Tasks Exist]
**Action**: Implementation or Consistency Check

Tasks found at: `[TASKS path]`

**Options**:
1. **Start Implementation** (`@speckit.implement`)
   - Execute tasks sequentially with TDD Red→Green
   - Mark completed tasks as `[X]`
   - Capture Red→Green transcripts (4 files minimum per phase)

2. **Analyze Consistency** (`@speckit.analyze`)
   - Read-only validation across spec/plan/tasks
   - Identify gaps, duplications, constitution violations
   - Get remediation recommendations before coding

**Recommendation**: `[Based on constitution violations, suggest analyze or implement]`

**Pre-Implementation Checklist**:
- [ ] Ran `#think` to plan implementation approach?
- [ ] BDD feature files created in `specs/{feature}/features/`?
- [ ] UI tasks have Figma links (or labeled "Skipped — No Figma")?
- [ ] Test environment ready (Aspire AppHost running, PostgreSQL, Redis)?
- [ ] Git branch clean and up-to-date?

**Handoff**: Click `[appropriate handoff button]`

### [Phase: Implementation In Progress]
**Action**: Continue Implementation or Review

`[Analyze tasks.md to see which are marked [X] completed]`

**Progress**: `[N]` of `[M]` tasks completed (`[percentage]%`)

**Next Task**: `[Task title and description]`

**Constitution Compliance Check**:
`[Run validation on current state]`

**If violations found**: Address before continuing
**If clean**: Proceed with next task using TDD Red→Green workflow

---

## 📝 Notes & Questions

### Open Questions
`[List any questions that need user clarification before proceeding]`

### Assumptions Made
`[List any assumptions you're making based on context]`

### HOW Details Parked for /plan
`[List implementation details user mentioned that belong in plan, not spec]`

**Examples**:
- Framework: "use MediatR for CQRS"
- Database: "PostgreSQL jsonb column for metadata"
- Events: "publish DistrictCreated event to school-sync topic"
- APIs: "POST /api/districts endpoint"
- DTOs: "DistrictDto with Id, Name, DeletedAt fields"

### Dependencies & Blockers
`[List any external dependencies or blockers identified]`

**Examples**:
- Waiting on Figma design for admin portal modal
- Need Entra ID app registration before testing OAuth flow
- Blocked on Foundation layer event schema finalization

### Next Interaction
`[Set expectations for what happens next - user click handoff, answer question, provide more context]`

---

## Template Usage Notes

**When to use each section:**

1. **Current State** - ALWAYS show (detect via check-prerequisites.sh --json)
2. **Vertical Slice Check** - Show when:
   - New feature being scoped
   - User provides feature description during intake
   - Split proposal needed
3. **Constitution Validation** - ALWAYS show if artifacts exist (spec/plan/tasks)
4. **MCP Tools Required** - ALWAYS show (context-aware based on feature type)
5. **Recommended Next Step** - ALWAYS show (state-based guidance)
6. **Notes & Questions** - Show when:
   - Open questions need user input
   - Assumptions made that should be validated
   - HOW details captured for parking in /plan
   - Dependencies or blockers identified

**Tone Guidelines:**
- Concise and actionable
- Use emojis for visual scanning (🎯 ✅ ⚠️ 🚨 📋 ⚙️ 🚀 📝)
- Provide specific file paths and commands
- Always end with clear next action

**Constitution Validation Frequency:**
- At intake (if artifacts exist)
- Before every handoff
- When user asks "what's next?"
- After implementation milestones

**MCP Tool Reminder Triggers:**
- UI keywords: Razor, component, page, form, modal, layout, UI, frontend
- Service keywords: service, API, domain, repository, command, query, CQRS
- Database keywords: migration, schema, entity, DbContext, EF Core, PostgreSQL
- Complex keywords: architecture, refactor, multi-service, orchestration, Aspire
