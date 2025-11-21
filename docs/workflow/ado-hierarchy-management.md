# Azure DevOps Hierarchy Management Workflow

**Version**: 1.0.0  
**Constitutional Basis**: NorthStarET Constitution v2.2.0  
**Date**: 2025-11-20  
**Status**: Active  

---

## Purpose

This workflow defines the **repeatable, agent-driven process** for managing Azure DevOps work item hierarchies (Epic → Feature → User Story → Task) in alignment with the NorthStarET Constitution v2.2.0.

## Constitutional Compliance

This workflow enforces:
- **Principle 6**: Mono-Repo Layer Isolation (layer identification, specification branch workflow)
- **Principle 7**: Tool-Assisted Development Workflow (structured thinking, documentation grounding)
- **Delivery Workflow**: Specification phase artifacts in dedicated branches
- **Non-Negotiable Constraints**: Layer identification, specification branch usage

---

## File Structure & Responsibilities

### Repository Structure
```
Plan/{LayerName}/specs/{feature-id}/
├── spec.md                          # Feature specification (MUST include ADO frontmatter)
├── tasks.md                         # Task breakdown (mapped to User Stories)
├── plan.md                          # Technical implementation plan
├── .ado-hierarchy.json              # Current ADO work item tracking (version control)
├── .ado-hierarchy-proposed.json     # Proposed restructuring (when applicable)
├── hierarchy-proposal.md            # Executive proposal for restructuring
└── hierarchy-comparison.md          # Visual/analytical comparison
```

### File Responsibilities

**spec.md** - Canonical Source of Truth
- Contains ADO frontmatter: `ado_work_item_id`, `ado_epic_id`, `ado_parent_id`, `sync_status`
- Defines layer, priority, user stories, acceptance criteria
- MUST be in specification branch (`{LayerName}/###-feature-name-spec`)

**tasks.md** - Task-to-Story Mapping
- Phase-based task organization with `[US#]` markers
- Defines grouping strategy: `requirement_key`, `functional_group`, or `priority_fallback`
- Task IDs (T###) map to User Stories via grouping markers

**.ado-hierarchy.json** - Work Item Registry
- Tracks Epic → Feature → User Story → Task relationships
- Records work item IDs, URLs, story points, dependencies
- Version controlled; updated only after successful ADO sync
- Empty `"epics": {}` indicates pending initial sync

**.ado-hierarchy-proposed.json** - Restructuring Proposal
- Created when proposing hierarchy changes (e.g., phase-based → domain-based)
- Includes `changes_from_v{old}` documentation
- Subject to stakeholder approval before adoption

**hierarchy-proposal.md** - Human-Readable Proposal
- Executive summary for stakeholders
- Feature details, milestones, risk analysis
- Approval checklist

**hierarchy-comparison.md** - Decision Support
- Visual comparison (ASCII art, side-by-side)
- Dependency graphs, parallelization strategies
- Decision matrix with weighted scoring

---

## Agent Workflow: Initial Hierarchy Creation

### Phase 1: Analyze Specification

**Agent**: `/speckit.plan` or `/speckit.tasks`  
**Input**: `spec.md` (user stories, requirements) + `tasks.md` (task breakdown)  
**Constitutional Check**: Layer identification present, specification branch active

**Actions**:
1. Use `#think` to analyze spec structure and task groupings
2. Identify User Story grouping strategy:
   - **Preferred**: `requirement_key` (tasks tagged with `[US#]` in tasks.md)
   - **Fallback**: `functional_group` (tasks grouped by functional area)
   - **Last Resort**: `priority_fallback` (tasks grouped by priority P1/P2/P3)
3. Determine Feature grouping:
   - **Phase-based**: One feature per implementation phase (simple, sequential)
   - **Domain-based**: Features group related functional domains (better parallelization)
4. Query `#microsoft.docs.mcp` for Azure DevOps API best practices if needed

**Output**: Proposed hierarchy structure with:
- Epic title from spec.md
- Feature titles (phases or domains)
- User Story titles with story point estimates
- Task mappings (T### → User Story)

### Phase 2: Generate Hierarchy Files

**Agent**: Hierarchy generation agent or manual creation  
**Tool Use**: `#think` for structure planning  

**Actions**:
1. Create `.ado-hierarchy.json` with empty `"epics": {}`
2. Document hierarchy structure:
   ```json
   {
     "version": "5.0.0",
     "hierarchy": "Epic → Feature → User Story → Task",
     "description": "Tracking file for ADO sync",
     "epics": {}
   }
   ```
3. Update `spec.md` frontmatter:
   ```yaml
   ado_work_item_id: null
   ado_epic_id: null
   sync_status: "pending"
   layer: "{LayerName}"
   ```

**Constitutional Compliance**:
- ✅ Layer explicitly identified in spec.md
- ✅ Files created in specification branch (`{LayerName}/###-feature-name-spec`)
- ✅ No implementation code in specification branch

### Phase 3: Sync to Azure DevOps

**Agent**: ADO sync agent with `#microsoft/azure-devops-mcp`  
**Prerequisite**: Specification approved, hierarchy files reviewed  

**Actions**:
1. Read `.ado-hierarchy.json` (if `epics: {}`, this is initial sync)
2. Create Epic work item:
   ```typescript
   mcp_microsoft_azu_wit_create_work_item({
     project: "NorthStarET",
     workItemType: "Epic",
     fields: [
       { name: "System.Title", value: epicTitle },
       { name: "System.Description", value: epicDescription },
       { name: "System.Tags", value: tags.join("; ") }
     ]
   })
   ```
3. Create Feature work items (link to Epic as parent)
4. Create User Story work items (link to Features as parent)
5. Create Task work items (link to User Stories as parent)
6. Update `.ado-hierarchy.json` with work item IDs and URLs
7. Update `spec.md` frontmatter:
   ```yaml
   ado_work_item_id: {epic_id}
   ado_epic_id: {epic_id}
   sync_status: "synced"
   last_synced: "2025-11-20T23:45:00Z"
   ```

**Constitutional Compliance**:
- ✅ Work items created in NorthStarET project
- ✅ Hierarchy reflects layer organization
- ✅ Links enable traceability (spec → ADO → implementation)

**Error Handling**:
- If ADO tools disabled: Document work item structure, defer sync
- If work item creation fails: Roll back, log error, preserve `.ado-hierarchy.json` state
- If partial sync: Mark `sync_status: "partial"`, document missing items

### Phase 4: Commit & Version Control

**Agent**: Git automation or developer  

**Actions**:
1. Stage updated files:
   ```bash
   git add Plan/{LayerName}/specs/{feature-id}/.ado-hierarchy.json
   git add Plan/{LayerName}/specs/{feature-id}/spec.md
   ```
2. Commit with message:
   ```bash
   git commit -m "feat(ado): sync {feature-id} hierarchy to Azure DevOps

   - Created Epic {epic_id}: {epic_title}
   - Created {n} Features with {m} User Stories
   - Total story points: {total_sp}
   - Sync status: synced
   
   Constitutional: Principle 6 (layer isolation), Delivery Workflow (spec branch)"
   ```
3. Push to specification branch:
   ```bash
   git push origin {LayerName}/###-feature-name-spec
   ```

**Constitutional Compliance**:
- ✅ Commits reference constitutional principles
- ✅ Specification branch workflow preserved
- ✅ Version control tracks ADO sync history

---

## Agent Workflow: Hierarchy Restructuring

### When to Restructure

Restructure hierarchy when:
- ❌ Features are overly fragmented (too many 1:1 Feature:Story mappings)
- ❌ Phase-based grouping prevents team parallelization
- ❌ Feature sizes are imbalanced (some 4 SP, others 13+ SP)
- ❌ Business value unclear from feature titles
- ✅ Better domain-based grouping identified
- ✅ Improved milestone clarity possible

### Phase 1: Analysis & Proposal Generation

**Agent**: Architecture analysis agent  
**Tool Use**: `#think` for multi-step reasoning  

**Actions**:
1. Use `#mcp_sequentialthi_sequentialthinking` to analyze current hierarchy:
   ```
   Thought 1: Identify issues with current structure
   Thought 2: Propose alternative groupings (domain-based vs phase-based)
   Thought 3: Validate dependencies and parallelization
   Thought 4: Calculate milestone progression
   Thought 5: Assess risks and mitigation strategies
   ```

2. Generate `.ado-hierarchy-proposed.json`:
   - New version number (e.g., `"version": "6.0.0"`)
   - Document `changes_from_v{old}` array
   - Preserve all story points and tasks (93 SP, 100 tasks)
   - Map old features to new features

3. Create `hierarchy-proposal.md`:
   - Executive summary with "why restructure?"
   - Feature-by-feature breakdown
   - 4 delivery milestones
   - Risk analysis
   - Approval checklist

4. Create `hierarchy-comparison.md`:
   - ASCII art visualizations
   - Side-by-side metrics table
   - Dependency graphs
   - Decision matrix with scoring

**Constitutional Compliance**:
- ✅ Uses tool-assisted development (Principle 7)
- ✅ Documents rationale (Governance: Amendments)
- ✅ Preserves all deliverables (no work lost)

### Phase 2: Stakeholder Review

**Agent**: None (human review process)  
**Artifacts**: `hierarchy-proposal.md`, `hierarchy-comparison.md`, `.ado-hierarchy-proposed.json`  

**Actions**:
1. Product Owner reviews business value alignment
2. Engineering Lead reviews technical feasibility
3. Architecture Review Board validates dependencies
4. Scrum Master assesses team parallelization

**Approval Criteria**:
- ✅ Improved business value clarity
- ✅ Better team parallelization
- ✅ Reduced coordination overhead
- ✅ No loss of deliverables
- ✅ Constitutional compliance maintained

### Phase 3: Adoption & Re-Sync

**Agent**: ADO sync agent  
**Prerequisite**: Stakeholder approval obtained  

**Actions**:
1. **If work items NOT yet created** (easy path):
   - Replace `.ado-hierarchy.json` content with `.ado-hierarchy-proposed.json`
   - Update version number
   - Follow "Initial Hierarchy Creation" workflow above

2. **If work items ALREADY created** (migration path):
   - Archive old work items in ADO (don't delete - preserve history)
   - Clear `.ado-hierarchy.json`: `"epics": {}`
   - Copy `.ado-hierarchy-proposed.json` to `.ado-hierarchy.json`
   - Run sync agent to create new hierarchy
   - Update `spec.md` frontmatter with new Epic ID

3. Update commit message:
   ```bash
   git commit -m "refactor(ado): restructure {feature-id} hierarchy to domain-based

   - Reduced features from 11 to 6 (45% reduction)
   - Improved parallelization (4-5 features concurrent)
   - Clearer milestones (MVP→Production→DevEx→Hardened)
   - Preserved all 93 SP and 100 tasks
   - Approved by: [Product Owner, Eng Lead, Architecture Review]
   
   Constitutional: Principle 6 (layer isolation), Governance (Amendments)
   
   Changes from v5.0:
   - Phase-based → Domain-based feature grouping
   - Features now represent functional domains
   - Better team autonomy and ownership"
   ```

**Constitutional Compliance**:
- ✅ Amendment process followed (Governance)
- ✅ Impact analysis documented (hierarchy-proposal.md)
- ✅ Approval obtained before merge
- ✅ Version history preserved

---

## Agent Workflow: Ongoing Sync & Maintenance

### Phase 1: Detect Changes

**Agent**: Git diff agent or manual detection  

**Triggers**:
- Spec.md updated (new user stories added)
- Tasks.md updated (new tasks, SP estimates changed)
- ADO work items updated externally (status changes, assignments)

**Actions**:
1. Compare spec.md frontmatter `last_synced` with current date
2. Check `.ado-hierarchy.json` version against spec.md version
3. Detect drift: work items in ADO not in hierarchy file or vice versa

### Phase 2: Incremental Sync

**Agent**: ADO sync agent  
**Tool Use**: `#microsoft/azure-devops-mcp`  

**Actions**:
1. For **new user stories**:
   - Determine parent feature (based on grouping strategy)
   - Create User Story work item in ADO
   - Update `.ado-hierarchy.json` with new story
   
2. For **new tasks**:
   - Identify parent User Story via `[US#]` marker
   - Create Task work item in ADO
   - Update `.ado-hierarchy.json` task list

3. For **story point changes**:
   - Update work item in ADO
   - Update `.ado-hierarchy.json` SP totals
   - Recalculate feature/epic totals

4. For **status changes** (ADO → Spec):
   - Query work item status via `mcp_microsoft_azu_wit_get_work_item`
   - Update spec.md frontmatter if status changed
   - Update `.ado-hierarchy.json` if needed

**Constitutional Compliance**:
- ✅ Maintains sync between spec and ADO
- ✅ Preserves version control history
- ✅ Documents changes in commit messages

### Phase 3: Validation & Health Check

**Agent**: Validation agent  

**Actions**:
1. Verify all work items in `.ado-hierarchy.json` exist in ADO
2. Verify all tasks in `tasks.md` mapped to User Stories
3. Verify story points sum correctly (task → story → feature → epic)
4. Verify parent-child relationships intact
5. Verify layer identification consistent (spec.md layer = ADO tags)

**Output**: Health report
```markdown
# ADO Hierarchy Health Report

**Feature**: 000-aspire-scaffolding
**Date**: 2025-11-20
**Status**: ✅ Healthy | ⚠️ Warnings | ❌ Errors

## Summary
- Epic ID: 1399
- Features: 6/6 synced
- User Stories: 11/11 synced
- Tasks: 100/100 mapped
- Story Points: 93 (spec) = 93 (ADO)

## Warnings
- None

## Errors
- None

## Next Actions
- None required
```

---

## Constitutional Checkpoints

### Every Hierarchy Operation MUST:

1. **Layer Identification** (Principle 6)
   - [ ] spec.md declares `layer: "{LayerName}"`
   - [ ] Files in correct specification path: `Plan/{LayerName}/specs/{feature-id}/`
   - [ ] Branch follows pattern: `{LayerName}/###-feature-name-spec`

2. **Specification Branch Workflow** (Principle 6, Delivery Workflow)
   - [ ] Hierarchy files created in specification branch, not implementation branch
   - [ ] No source code in specification branch
   - [ ] Branch name includes layer prefix

3. **Tool-Assisted Development** (Principle 7)
   - [ ] Agent uses `#think` or `#mcp_sequentialthi_sequentialthinking` for planning
   - [ ] Agent queries `#microsoft.docs.mcp` for ADO API guidance if needed
   - [ ] Structured reasoning documented in commit messages

4. **Version Control & Governance** (Governance)
   - [ ] All hierarchy changes committed with rationale
   - [ ] Restructuring follows amendment process (analysis → approval → adoption)
   - [ ] Version numbers follow semantic versioning (MAJOR.MINOR.PATCH)

5. **Traceability** (Principle 2, Delivery Workflow)
   - [ ] spec.md ↔ .ado-hierarchy.json ↔ ADO work items all consistent
   - [ ] Work item IDs and URLs captured in hierarchy file
   - [ ] Sync status tracked in spec.md frontmatter

---

## Error Recovery

### Scenario 1: ADO Sync Fails Mid-Process

**Actions**:
1. Do NOT update `.ado-hierarchy.json` or `spec.md`
2. Log error with work items created before failure
3. Mark `sync_status: "failed"` in spec.md
4. Document manual recovery steps:
   - Delete partially created work items in ADO, OR
   - Complete sync manually and update hierarchy file

### Scenario 2: Work Item IDs Change (ADO Reorganization)

**Actions**:
1. Query ADO for Epic by title (unique identifier)
2. Update `.ado-hierarchy.json` with new work item IDs
3. Update `spec.md` frontmatter
4. Commit with message: `fix(ado): update work item IDs after ADO reorganization`

### Scenario 3: Hierarchy File Deleted or Corrupted

**Actions**:
1. Restore from Git history: `git checkout HEAD~1 -- .ado-hierarchy.json`
2. If unavailable, reconstruct from ADO:
   - Query Epic by title
   - Query child Features, User Stories, Tasks
   - Rebuild hierarchy file
   - Mark `sync_status: "reconstructed"`

---

## Metrics & Observability

### Success Metrics
- **Sync Success Rate**: % of sync operations completing without errors
- **Hierarchy Health Score**: % of work items correctly mapped
- **Drift Detection Time**: Time between spec change and sync trigger
- **Approval Cycle Time**: Time from proposal to restructuring approval

### Monitoring
- CI/CD pipeline validates hierarchy files on every commit
- Daily cron job validates ADO ↔ Spec consistency
- Slack/email alerts on sync failures

### Reporting
- Weekly hierarchy health report per layer
- Monthly trend analysis (features added, restructured, completed)
- Quarterly review of grouping strategies (requirement_key vs functional_group usage)

---

## Version History

### v1.0.0 (2025-11-20)
- Initial workflow definition
- Aligned with Constitution v2.2.0
- Covers: initial creation, restructuring, ongoing sync, error recovery
- Defines agent responsibilities and constitutional checkpoints

---

## References

- NorthStarET Constitution v2.2.0: `.specify/memory/constitution.md`
- Azure DevOps MCP Tools: `#microsoft/azure-devops-mcp`
- Specification Templates: `.specify/templates/spec-template.md`, `plan-template.md`, `tasks-template.md`
- Hierarchy Examples: `Plan/CrossCuttingConcerns/specs/000-aspire-scaffolding/`
