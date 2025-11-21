# Azure DevOps Sync Agent

**Version**: 1.0.1  
**Author**: GitHub Copilot  
**Last Updated**: November 21, 2025

## Purpose

This agent maintains bidirectional synchronization between repository specification files (`Plan/Foundation/specs/**/*.md`) and Azure DevOps work items in the NorthStarET project. It uses the `#microsoft/azure-devops-mcp` tool to perform all Azure DevOps operations.

## Core Capabilities

### Execution Ordering & Naming (Features & Stories)

To enforce a predictable delivery sequence, all Features and their User Stories for the current Aspire Scaffolding Epic (Epic 1436) are numbered. The numbering is semantic (dependency + foundational layering), not just creation order. The agent MUST apply these naming conventions on future push/update cycles (renaming ADO work item titles when they drift).

Feature Title Format:
```
F{FeatureNumber}: {Concise Feature Name}
```
User Story Title Format (two acceptable variants, prefer first):
```
F{FeatureNumber}-S{StoryNumber}: {Full Story Name}
F{FeatureNumber}.S{StoryNumber}: {Full Story Name}   # fallback when hyphen conflicts with external tooling
```

Numbering Rationale:
1. Foundation precedes orchestration; infrastructure assets must exist before orchestration glue.
2. Orchestration precedes messaging so events can route across registered services.
3. Messaging precedes performance so baseline flows are optimized later.
4. Performance & caching precede advanced DevEx tooling to avoid premature optimization of tools.
5. Developer Experience precedes Quality polish so automation targets a mostly stable architecture.
6. Quality & Documentation last to consolidate test coverage & narrative after core capabilities.

Ordered Feature & Story List (Current State – Epic 1436):
```
F1: Foundation Infrastructure & Core Setup (Feature 1437)
  F1-S1: Aspire Project Initialization & Shared Infrastructure (Story 1443)
  F1-S2: Domain Entities & EF Core Configuration (Story 1445)

F2: Orchestration & Service Discovery (Feature 1438)
  F2-S1: US1: AppHost Boot with Resource Orchestration (Story 1446)
  F2-S2: US3: Tenant Isolation with TenantInterceptor (Story 1444)

F3: Event-Driven Messaging (Feature 1441)
  F3-S1: US4: Event Publication with MassTransit (Story 1447)
  F3-S2: US8: Resilient Messaging with Circuit Breaker (Story 1450)

F4: Performance & Caching (Feature 1442)
  F4-S1: US5: Redis Caching & Idempotency (Story 1451)
  F4-S2: US6: Observability with OpenTelemetry (Story 1452)

F5: Developer Experience & Tooling (Feature 1439)
  F5-S1: US2: Service Scaffolding Scripts (Story 1453)
  F5-S2: US7: API Gateway with Legacy Routing (Story 1448)

F6: Testing & Documentation (Feature 1440)
  F6-S1: Testing Infrastructure & Documentation Polish (Story 1449)
```

Agent Renaming Rules:
- When pushing, if a work item title does not start with its canonical prefix (`F{N}:` for Features, `F{N}-S{M}:` for Stories), update the title.
- Preserve original descriptive portion after the prefix; do not alter acceptance criteria or description body.
- Log all renames in commit message: `rename: [old] -> [new]`.
- Do NOT renumber automatically; numbering changes require explicit human directive in this file.

Story Number Assignment Guidance:
- Order stories within a feature by dependency (setup/config before integration, integration before resilience).
- If two stories have no dependency relationship, preserve original creation order.
- New stories appended as next S{M+1} for that feature; update list above in same commit.

Hierarchy Registry Impact:
- `.ado-hierarchy.json` should store the renamed titles verbatim after next sync.
- If registry titles differ, treat as drift and schedule update.

### 1. Push Sync (Repository → Azure DevOps)
- Creates new work items from spec.md files without `ado_work_item_id`
- Updates existing work items when spec content changes
- Maintains parent-child hierarchy (Epic → Feature → User Story)
- User Stories use intelligent grouping strategy:
  1. **Requirement Key Mapping** (preferred): Extract [US#] markers from tasks, match to spec.md User Scenarios, use full user story titles
  2. **Functional Grouping** (fallback): Group related tasks by functional area (Setup, Database, API, Auth, Testing, Documentation)
  3. **Priority Grouping** (last resort): Group by P1/P2/P3 when functional grouping unclear
- Embeds task checklists in User Story descriptions with markdown checkboxes

### 2. Pull Sync (Azure DevOps → Repository)
- Detects work item changes in ADO
- Updates spec.md frontmatter and content
- Preserves local markdown formatting
- Handles conflict detection

### 3. Conflict Resolution
- Repository-first strategy (local changes take precedence)
- Generates conflict reports for manual review
- Provides resolution commands

## Prerequisites

### Required Tools
- `#microsoft/azure-devops-mcp` - Azure DevOps MCP server
- Access to NorthStarET project (ID: 80541855-4202-425b-ac5b-e38d5537f6bb)

### Required Files
- `.ado-hierarchy.json` - Epic/Feature to folder mapping
- `spec.md` - Feature specification with YAML frontmatter
- `tasks.md` - Task breakdown with checklist items

## Agent Commands

### Push Changes to Azure DevOps

**Command**: `@workspace /sync-ado push [spec-filter]`

**Description**: Creates or updates Azure DevOps work items from spec.md files.

**Example**:
```
@workspace /sync-ado push
@workspace /sync-ado push identity-service
@workspace /sync-ado push phase1-*
```

**Agent Workflow**:

1. **Load Hierarchy Registry**
   ```
   Read Plan/Foundation/specs/.ado-hierarchy.json
   Parse Epic and Feature mappings
   ```

2. **Discover Specs**
   ```
   Use #filesystem to find all spec.md files matching pattern:
   - Plan/Foundation/specs/**/spec.md
   - Filter by spec-filter if provided
   ```

3. **Parse Each Spec**
   ```
   For each spec.md:
   - Extract YAML frontmatter
   - Parse markdown content (title, description, acceptance criteria)
   - Check for ado_work_item_id
   ```

4. **Parse Tasks**
   ```
   For each tasks.md in same directory:
   - Extract tasks by phase
   - Group tasks by priority markers ([P], [P2], [P3])
   - Tasks without markers default to P1
   - Phase-level priority in header overrides individual task priorities
   - Track task ranges (e.g., T001-T020) for reference to tasks.md
   ```

5. **Resolve Parent ID**
   ```
   Use folder path to lookup Feature ID in hierarchy registry:
   - Extract phase from folder name (e.g., "phase1-setup")
   - Match to Feature work item ID in registry
   - If not found, use Epic ID as parent
   ```

6. **Group Tasks by Requirement Key and Create User Stories**
   ```
   Strategy 1 - Requirement Key Mapping (preferred):
     - Scan tasks for [US#] markers (e.g., [US1], [US2])
     - Load spec.md User Scenarios section
     - Extract full user story title matching [US#]
     - Create User Story: "E#-F#-S#: [Full User Story Title]"
     - Group all tasks with same [US#] marker
     - Story Points: Estimate based on task complexity (not 1:1 count)
     - Priority: Extract from spec.md user scenario priority or default from task markers
   
   Strategy 2 - Functional Grouping (fallback when no [US#] markers):
     - Analyze task descriptions to identify functional areas
     - Common areas: Setup, Database, API, Authentication, Testing, Documentation
     - Create User Story: "E#-F#-S#: [Functional Area Name]"
     - Group tasks by functional area
     - Story Points: Estimate based on functional area complexity
     - Priority: Most common priority among grouped tasks (P1 default)
   
   Strategy 3 - Scenario-Based Grouping (from spec.md scenarios):
     - Map tasks to spec.md scenarios by analyzing descriptions
     - Create User Story per scenario: "E#-F#-S#: [Scenario Title]"
     - Include task range reference (e.g., T001-T020)
     - Story Points: Estimate based on scenario scope
     - Priority: From spec.md scenario priority
   
   For all strategies:
     - Description Format: "<strong>Goal:</strong> [goal text]<br><br><strong>Acceptance Criteria:</strong><ul><li>criterion 1</li></ul><br><strong>Tasks:</strong> T###-T### ([task summary])"
     - Acceptance Criteria: From spec.md user scenario or aggregate from tasks
     - Tasks Reference: Include task ID ranges without individual checkboxes
   ```

7. **Create or Update Work Item**
   ```
   If no ado_work_item_id exists:
     Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_create_work_item
     Parameters:
       - project: "NorthStarET"
       - workItemType: "User Story"
       - title: "E#-F#-S#: [Title from spec.md scenario]"
       - description: HTML formatted with Goal, Acceptance Criteria, Tasks sections
       - storyPoints: Estimated from task complexity analysis
       - priority: From spec.md scenario priority
       - tags: From spec.md frontmatter
       - parentId: Resolved Feature ID from hierarchy
     
     Description HTML Format:
       "<strong>Goal:</strong> [goal text]<br><br>
        <strong>Acceptance Criteria:</strong>
        <ul><li>criterion 1</li><li>criterion 2</li></ul><br>
        <strong>Tasks:</strong> T###-T### ([functional summary from tasks.md])"
     
     After User Story creation, create 4 child Task work items:
       Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_create_work_item (4 times)
       Parameters for each:
         - project: "NorthStarET"
         - workItemType: "Task"
         - title: "Coded" | "Tested" | "Reviewed" | "Merged"
         - state: "New" (Azure DevOps default)
         - priority: 2
         - remainingWork: 0 (hours)
         - description: 
           * Coded: "Implementation complete for all tasks in E#-F#-S# (T###-T###)"
           * Tested: "Unit, integration, and BDD tests passing for E#-F#-S#"
           * Reviewed: "Code review completed and approved for E#-F#-S#"
           * Merged: "Pull request merged to main branch for E#-F#-S#"
       
       CRITICAL - Task Linking:
         Tasks CANNOT be linked to User Stories during creation using parentId field.
         After all 4 Tasks are created, link them separately using:
         
         Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_work_items_link
         Parameters:
           - updates: Array of 4 link operations
           - type: "parent" (NOT "child" - this is the child-to-parent direction)
           - linkToId: [User Story work_item_id]
           - id: [Task work_item_id]
         
         Example:
           [
             {"type": "parent", "linkToId": 2267, "id": 2302},
             {"type": "parent", "linkToId": 2267, "id": 2303},
             {"type": "parent", "linkToId": 2267, "id": 2306},
             {"type": "parent", "linkToId": 2267, "id": 2307}
           ]
         
         Why this approach:
           - Azure DevOps API does NOT support System.Parent field during Task creation
           - Attempting to use parentId parameter during creation silently fails
           - Must use separate work_items_link call with type="parent" after creation
           - Using type="child" will fail with TF201036 error (wrong direction)
           - Verify linking success by checking for relations in Task work item
     
     After all creations and linking:
       - Add User Story to .ado-hierarchy.json under appropriate epic/feature
       - Record: work_item_id, title, url, story_points, priority
       - Record child task IDs: [coded_id, tested_id, reviewed_id, merged_id]
       - No spec.md frontmatter updates (specs don't have individual work item IDs)
   
   Else if work item exists:
     Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_get_work_item
     Query existing work item by ID
     
     Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_update_work_item
     Parameters:
       - id: work_item_id from hierarchy
       - title: Updated E#-F#-S# title if needed
       - description: Updated HTML with Goal/Criteria/Tasks
       - storyPoints: Updated estimate
       - priority: Updated priority
       - tags: Updated tags
     
     Update .ado-hierarchy.json:
       - Refresh story_points, priority if changed
       - Update last_updated timestamp
   ```

7. **Commit Hierarchy Updates**
   ```
   Use #filesystem to write updated .ado-hierarchy.json
   Commit with message: "feat: Add Features and User Stories for [Epic Name] ([work-item-ids])"
   ```

**Output**:
```
✅ Synced 170 work items to Azure DevOps (34 User Stories + 136 child Tasks)

Created for E1 Identity Service (Epic #1455):
- Story #1463: E1-F1-S1: US1: Staff Login via Entra ID SSO (8 pts)
  ├── Task #1510: Coded
  ├── Task #1511: Tested
  ├── Task #1512: Reviewed
  └── Task #1513: Merged

- Story #1465: E1-F1-S2: US2: Admin Login with MFA (5 pts)
  ├── Task #1514: Coded
  ├── Task #1515: Tested
  ├── Task #1516: Reviewed
  └── Task #1517: Merged

Created for E2 API Gateway (Epic #1456):
- Story #1475: E2-F3-S3: Service Health Monitoring (8 pts)
  ├── Task #1518: Coded
  ├── Task #1519: Tested
  ├── Task #1520: Reviewed
  └── Task #1521: Merged

...

Summary:
- E1: 10 User Stories (40 child Tasks) - 66 story points
- E2: 12 User Stories (48 child Tasks) - 83 story points
- E3: 12 User Stories (48 child Tasks) - 89 story points
- Total: 34 User Stories, 136 Tasks, 238 story points
```

---

### Pull Changes from Azure DevOps

**Command**: `@workspace /sync-ado pull [spec-filter]`

**Description**: Updates spec.md files from Azure DevOps work items.

**Example**:
```
@workspace /sync-ado pull
@workspace /sync-ado pull identity-service
```

**Agent Workflow**:

1. **Load Hierarchy Registry**
   ```
   Read Plan/Foundation/specs/.ado-hierarchy.json
   Parse Epic and Feature mappings
   ```

2. **Query Modified Work Items**
   ```
   Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_get_query
   
   WIQL Query:
   SELECT [System.Id], [System.Title], [System.ChangedDate]
   FROM WorkItems
   WHERE [System.TeamProject] = 'NorthStarET'
     AND [System.WorkItemType] = 'User Story'
     AND [System.ChangedDate] > @LastSyncDate
     AND [System.Parent] IN (1377, 1379, 1378, 1380)
   ORDER BY [System.ChangedDate] DESC
   
   @LastSyncDate = earliest last_synced timestamp from all spec.md files
   ```

3. **Match Work Items to Specs**
   ```
   For each work item ID:
   - Use #filesystem to search for spec.md with matching ado_work_item_id in frontmatter
   - If found, proceed to update
   - If not found, log as orphaned work item
   ```

4. **Fetch Work Item Details**
   ```
   Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_get_work_item
   Parameters:
     - id: work item ID
     - expand: "Relations" (to get parent links)
   
   Extract fields:
     - System.Title
     - System.Description (HTML format)
     - Microsoft.VSTS.Common.AcceptanceCriteria (HTML format)
     - Microsoft.VSTS.Scheduling.StoryPoints
     - Microsoft.VSTS.Common.Priority
     - System.Tags
     - System.ChangedDate
     - System.Parent (from relations)
   ```

5. **Conflict Detection**
   ```
   Read spec.md last_synced timestamp
   
   If spec.md modified after last_synced:
     CONFLICT DETECTED
     Generate conflict report (see Conflict Resolution section)
   Else:
     Proceed to update spec.md
   ```

6. **Update Spec Content**
   ```
   Convert HTML description to markdown:
     - Parse HTML content
     - Extract task checklist (convert back to tasks.md format)
     - Convert remaining description to markdown
   
   Update spec.md:
     - Replace title
     - Replace description section
     - Replace acceptance criteria section
     - Update frontmatter:
         ado_work_item_id: (no change)
         ado_parent_id: From System.Parent
         ado_work_item_type: "User Story"
         ado_url: https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/{id}
         last_synced: System.ChangedDate
         sync_status: "synced"
         story_points: From work item
         priority: From work item
         tags: From work item (array)
   
   Update tasks.md:
     - Parse checklist from description
     - Regenerate task structure with checklist states
   ```

7. **Commit Updates**
   ```
   Use #filesystem to write updated spec.md and tasks.md
   Commit with message: "sync: Pull updates from ADO [work-item-ids]"
   ```

**Output**:
```
✅ Pulled 3 updates from Azure DevOps

Updated:
- Identity Service (spec.md) ← User Story #1382
  - Story points: 13 → 21
  - Priority: 1 → 2
  - 2 tasks marked as Coded
  
- Configuration Service (spec.md) ← User Story #1383
  - Description updated
  - 1 task marked as Tested

- API Gateway (spec.md) ← User Story #1381
  - Tags added: authentication, security
  - 3 tasks marked as Reviewed

No changes:
- Student Management (User Story #1390)
- Staff Management (User Story #1389)
```

---

### Validate Sync State

**Command**: `@workspace /sync-ado validate`

**Description**: Validates frontmatter and detects sync drift.

**Example**:
```
@workspace /sync-ado validate
```

**Agent Workflow**:

1. **Check Frontmatter Schema**
   ```
   For each spec.md:
   - Verify YAML frontmatter exists
   - Check required fields after 7 days: ado_work_item_id
   - Validate field types (ado_work_item_id: number, tags: array, etc.)
   - Check last_synced is valid ISO timestamp
   ```

2. **Verify Work Item Existence**
   ```
   For each spec.md with ado_work_item_id:
   - Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_get_work_item
   - Verify work item exists in ADO
   - Check work item state (Active vs Closed)
   ```

3. **Detect Orphaned Work Items**
   ```
   Query all User Story work items in Features 1377, 1379, 1378, 1380
   For each work item:
   - Search for matching spec.md with ado_work_item_id
   - If not found, mark as orphaned
   ```

4. **Check Sync Drift**
   ```
   For each spec.md with ado_work_item_id:
   - Compare spec.md last modified time with last_synced
   - Query work item ChangedDate
   - If either is newer than last_synced, mark as needs sync
   ```

**Output**:
```
✅ Validation Report

Schema Issues:
- specs/005-reporting-service/spec.md: Missing ado_work_item_id (created 8 days ago)
- specs/006-content-media/spec.md: Invalid tags format (expected array)

Work Item Issues:
- User Story #1399: Not found in Azure DevOps (spec: identity-service/spec.md)
- User Story #1385: State is Closed (spec: data-migration/spec.md)

Orphaned Work Items:
- User Story #1401: "Legacy Auth Cleanup" - No matching spec.md

Sync Drift:
- specs/001-identity-service/spec.md: Modified locally, needs push
- User Story #1390: Modified in ADO, needs pull
- User Story #1383: Conflict detected (both sides modified)

Summary:
- Total specs: 18
- Synced: 13
- Needs push: 2
- Needs pull: 1
- Conflicts: 1
- Schema errors: 2
- Orphaned: 1
```

---

### Resolve Conflicts

**Command**: `@workspace /sync-ado resolve <spec-path> --strategy=[repo-wins|ado-wins|manual]`

**Description**: Resolves sync conflicts with specified strategy.

**Example**:
```
@workspace /sync-ado resolve specs/001-identity-service/spec.md --strategy=repo-wins
@workspace /sync-ado resolve specs/002-student-management/spec.md --strategy=ado-wins
@workspace /sync-ado resolve specs/003-staff-management/spec.md --strategy=manual
```

**Agent Workflow**:

1. **Detect Conflict**
   ```
   Read spec.md:
     - Extract last_synced timestamp
     - Extract ado_work_item_id
     - Check file last modified time
   
   Query work item:
     Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_get_work_item
     - Get System.ChangedDate
   
   Confirm conflict:
     - File modified after last_synced
     - Work item changed after last_synced
   ```

2. **Repository Wins Strategy**
   ```
   Force push spec.md content to ADO:
     Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_update_work_item
     Overwrite all fields from spec.md
     
   Update spec.md frontmatter:
     - Set last_synced: current timestamp
     - Set sync_status: "synced"
   ```

3. **ADO Wins Strategy**
   ```
   Force pull work item to spec.md:
     Fetch work item with mcp_microsoft_azu_wit_get_work_item
     Overwrite spec.md content
     Overwrite tasks.md checklist
     
   Update frontmatter:
     - Set last_synced: System.ChangedDate
     - Set sync_status: "synced"
   ```

4. **Manual Strategy**
   ```
   Generate detailed conflict report:
   
   Create specs/001-identity-service/spec.conflict file:
   ---
   # Conflict Report: Identity Service
   
   **Spec Modified**: 2025-11-20T14:30:00Z
   **ADO Modified**: 2025-11-20T15:45:00Z
   **Last Synced**: 2025-11-19T10:00:00Z
   
   ## Repository Version
   
   ### Title
   Identity Service Migration
   
   ### Description
   Migrate IdentityServer4 to Duende Identity + Entra ID...
   
   ### Story Points
   21
   
   ### Tasks
   - [ ] Coded: Setup Duende package
   - [x] Tested: Integration tests pass
   
   ## Azure DevOps Version
   
   ### Title
   Identity Service & Auth Migration
   
   ### Description
   Migrate IdentityServer4 to Duende Identity with Entra ID integration...
   
   ### Story Points
   25
   
   ### Tasks
   - [x] Coded: Setup Duende package
   - [x] Tested: Integration tests pass
   - [ ] Reviewed: Security review pending
   
   ## Resolution Options
   
   1. Keep repository version:
      @workspace /sync-ado resolve specs/001-identity-service/spec.md --strategy=repo-wins
   
   2. Use Azure DevOps version:
      @workspace /sync-ado resolve specs/001-identity-service/spec.md --strategy=ado-wins
   
   3. Merge manually:
      - Edit spec.md with desired final state
      - Delete spec.conflict file
      - Run: @workspace /sync-ado push identity-service
   ---
   ```

**Output**:
```
✅ Conflict resolved: Identity Service

Strategy: repo-wins
Updated ADO User Story #1382 with repository content

Changes pushed:
- Title: "Identity Service Migration" (was "Identity Service & Auth Migration")
- Story points: 21 (was 25)
- Tasks: 2 items (was 3 items)

Next: Verify changes in ADO
https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1382
```

---

## Conflict Resolution

### Conflict Detection Logic

```
is_conflict = (
    spec_file_modified_time > last_synced AND
    ado_work_item.ChangedDate > last_synced
)
```

### Conflict Report Format

When conflicts are detected during push or pull, the agent generates a `.conflict` file:

**Location**: Same directory as `spec.md`  
**Filename**: `spec.conflict`

**Content**:
```markdown
# Conflict Report: [Service Name]

**Spec Modified**: [ISO timestamp]
**ADO Modified**: [ISO timestamp]
**Last Synced**: [ISO timestamp]

## Repository Version

[Full spec.md content including frontmatter]

## Azure DevOps Version

[Converted work item content to markdown format]

## Difference Summary

- Title changed: Yes/No
- Description changed: Yes/No
- Story points changed: [old] → [new]
- Priority changed: [old] → [new]
- Tags changed: Yes/No
- Tasks changed: [old count] → [new count]

## Resolution Commands

1. Keep repository version (overwrite ADO):
   @workspace /sync-ado resolve [spec-path] --strategy=repo-wins

2. Use Azure DevOps version (overwrite repo):
   @workspace /sync-ado resolve [spec-path] --strategy=ado-wins

3. Merge manually:
   - Edit spec.md with desired final state
   - Delete spec.conflict file
   - Run: @workspace /sync-ado push [spec-filter]
```

### Resolution Workflow

1. **Automatic Detection**: Conflicts detected during push/pull operations
2. **Generate Report**: Create `.conflict` file with both versions
3. **Notify User**: Output conflict summary in agent response
4. **Manual Review**: User examines `.conflict` file
5. **Choose Strategy**: User runs resolve command with chosen strategy
6. **Clean Up**: Delete `.conflict` file after resolution
7. **Verify**: Run validate command to confirm sync state

---

## Data Schema

### Spec.md Frontmatter (Legacy - Not Used for CrossCuttingConcerns)

**Note**: CrossCuttingConcerns specs do NOT have individual work item IDs in frontmatter. The .ado-hierarchy.json file is the single source of truth for work item mappings. Each spec folder maps to an Epic, and User Stories are created from spec scenarios and tracked in the hierarchy file.

**Legacy Format** (used for Foundation layer feature specs):
```yaml
---
ado_work_item_id: 1382           # Azure DevOps work item ID (deprecated for CrossCuttingConcerns)
ado_parent_id: 1377              # Parent Feature work item ID
ado_work_item_type: "User Story" # Work item type
ado_url: "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1382"
last_synced: "2025-11-20T10:30:00Z"  # Last successful sync timestamp (ISO 8601)
sync_status: "synced"            # Sync state: synced | needs_push | needs_pull | conflict
story_points: 21                 # Effort estimate
priority: 1                      # Priority (1=High, 2=Medium, 3=Low)
tags:                            # Tags for categorization
  - identity
  - authentication
  - entra-id
  - migration
---
```

### Hierarchy Registry (.ado-hierarchy.json)

**Location**: `Plan/CrossCuttingConcerns/specs/.ado-hierarchy.json`

**Purpose**: Single source of truth for Azure DevOps work item IDs mapped to repository spec folders. Tracks Epics (specs), Features (functional domains), and User Stories (scenarios) with their IDs, titles, URLs, story points, and priorities. The agent creates User Stories from spec.md scenarios and tasks.md analysis, recording the results in this registry.

```json
{
  "version": "7.0.0",
  "last_updated": "2025-11-21T17:15:00Z",
  "hierarchy": "Parent Epic (1454) → Epic (Spec) → Feature (Functional Domain) → User Story (Requirement Key/Functional Group)",
  "description": "Tracking file mapping Azure DevOps work item IDs to repository spec folders. All CrossCuttingConcerns epics (E0-E3) are children of parent Epic 1454. Agent analyzes spec.md and tasks.md to determine User Story groupings: requirement keys (preferred), functional groups (fallback). Features organized by functional domains instead of phases for better cross-phase tracking. Each task = 1 story point.",
  "grouping_note": "The grouping_strategy field records how the agent created each User Story: 'requirement_key' when [US#] markers found in tasks.md, 'functional_group' when tasks grouped by functional area. Features represent functional domains (Foundation, Orchestration, Messaging, Performance, DevEx, Quality) rather than phases to allow cross-phase work item tracking.",
  "parent_epic": {
    "work_item_id": 1454,
    "title": "CrossCuttingConcerns Specifications",
    "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1454",
    "child_epics": [1436, 1455, 1456, 1457]
  },
  "epics": {
    "000-aspire-scaffolding": {
      "work_item_id": 1436,
      "title": "E0: Aspire Orchestration & Cross-Cutting Scaffolding",
      "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1436",
      "folder": "Plan/CrossCuttingConcerns/specs/000-aspire-scaffolding",
      "layer": "foundation",
      "story_points": 93,
      "tags": ["foundation", "cross-cutting", "aspire", "orchestration", "multi-tenancy"],
      "features": {
        "feature1-foundation": {
          "work_item_id": 1437,
          "title": "F1: Foundation Infrastructure & Core Setup",
          "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1437",
          "description": "Core Aspire project setup, shared infrastructure, domain entities",
          "effort_points": 11,
          "user_stories": {
            "aspire-initialization": {
              "work_item_id": 1443,
              "title": "F1-S1: Aspire Project Initialization & Shared Infrastructure",
              "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1443",
              "grouping_strategy": "functional_group",
              "priority": 1,
              "story_points": 7
            }
          }
        }
      }
    },
    "001-identity-service-entra-id": {
      "work_item_id": 1455,
      "title": "E1: Identity Service with Microsoft Entra ID",
      "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1455",
      "folder": "Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id",
      "layer": "foundation",
      "tags": ["foundation", "identity", "authentication", "entra-id", "security"],
      "features": {
        "feature1-authentication": {
          "work_item_id": 1458,
          "title": "F1: Authentication & Session Management",
          "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1458",
          "description": "Core authentication flows, session management, role-based authorization",
          "user_stories": {
            "us1-staff-login": {
              "work_item_id": 1463,
              "title": "E1-F1-S1: US1: Staff Login via Entra ID SSO",
              "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1463",
              "grouping_strategy": "requirement_key",
              "requirement_key": "US1",
              "priority": 1,
              "story_points": 8
            }
          }
        }
      }
    }
  }
}
```

### Task Reference Format

**In tasks.md**:
```markdown
### Phase 1: Setup

- [ ] T001 Initialize Aspire AppHost project with service defaults
- [ ] T002 Configure PostgreSQL container with Aspire hosting
- [ ] T003 Configure Redis Stack container for caching
- [ ] T004 Setup EF Core migrations infrastructure
```

**In User Story Description (HTML)**:
```html
<strong>Goal:</strong> Establish project structure, app configuration, and hosting resources.<br><br>
<strong>Acceptance Criteria:</strong>
<ul>
  <li>Aspire AppHost orchestrates PostgreSQL and Redis containers</li>
  <li>Service defaults configured for all projects</li>
  <li>EF Core migrations infrastructure ready</li>
  <li>Health checks operational</li>
</ul><br>
<strong>Tasks:</strong> T001-T004 (Project Setup, Container Configuration)
```

**Child Task Work Items (in Azure DevOps Board)**:
```
User Story 1493: E3-F1-S1: District & School Provisioning (13 story points)
├── Task 1504: Coded (State: To Do, Remaining: 0h)
│   Description: "Implementation complete for all tasks T001-T020"
├── Task 1505: Tested (State: To Do, Remaining: 0h)
│   Description: "Unit, integration, and BDD tests passing"
├── Task 1506: Reviewed (State: To Do, Remaining: 0h)
│   Description: "Code review completed and approved"
└── Task 1507: Merged (State: To Do, Remaining: 0h)
    Description: "Pull request merged to main branch"
```

**Workflow**:
1. **Developer Implementation**: Works through T001-T020 in tasks.md, checking off items as completed
2. **Coded Milestone**: When all T001-T020 implementation tasks done, move Task 1504 "Coded" to "Done"
3. **Tested Milestone**: When tests written and passing, move Task 1505 "Tested" to "Done"
4. **Reviewed Milestone**: After PR review approved, move Task 1506 "Reviewed" to "Done"
5. **Merged Milestone**: After PR merged to main, move Task 1507 "Merged" to "Done" and User Story to "Closed"

**Key Points**:
- **tasks.md** = Detailed technical checklist (T001-T020 for this example, tracked locally in repo)
- **User Story Description** = References task range (T001-T020) for traceability to tasks.md
- **ADO Task Work Items** = 4 high-level milestone tasks (Coded/Tested/Reviewed/Merged) tracking implementation progress on board
- **Story Points** = Based on complexity estimate, not 1:1 count of tasks.md items
- Every User Story MUST have exactly 4 child Task work items created

---

## MCP Tool Usage

### Required MCP Tools

From `#microsoft/azure-devops-mcp`:

1. **mcp_microsoft_azu_wit_create_work_item**
   - Purpose: Create new work items
   - Parameters: project, workItemType, title, description, acceptanceCriteria, storyPoints, priority, tags, parentId
   - Returns: Created work item ID

2. **mcp_microsoft_azu_wit_update_work_item**
   - Purpose: Update existing work items
   - Parameters: id, title, description, acceptanceCriteria, storyPoints, priority, tags
   - Returns: Updated work item details

3. **mcp_microsoft_azu_wit_get_work_item**
   - Purpose: Retrieve work item by ID
   - Parameters: id, expand (e.g., "Relations")
   - Returns: Full work item JSON

4. **mcp_microsoft_azu_wit_get_query**
   - Purpose: Execute WIQL queries
   - Parameters: project, query (WIQL string)
   - Returns: Array of work item IDs

5. **mcp_microsoft_azu_wit_work_item_link**
   - Purpose: Create parent-child links
   - Parameters: sourceId, targetId, linkType
   - Returns: Link details

### Example MCP Calls

**Create User Story**:
```
Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_create_work_item with:
{
  "project": "NorthStarET",
  "workItemType": "User Story",
  "title": "Identity Service Migration",
  "description": "<h2>Description</h2><p>Migrate IdentityServer4...</p><h2>Tasks</h2><ul>...</ul>",
  "acceptanceCriteria": "<ul><li>Entra ID authentication works</li>...</ul>",
  "storyPoints": 21,
  "priority": 1,
  "tags": ["identity", "authentication", "entra-id"],
  "parentId": 1377
}
```

**Query Modified Work Items**:
```
Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_get_query with:
{
  "project": "NorthStarET",
  "query": "SELECT [System.Id], [System.Title], [System.ChangedDate] FROM WorkItems WHERE [System.TeamProject] = 'NorthStarET' AND [System.WorkItemType] = 'User Story' AND [System.ChangedDate] > '2025-11-19T10:00:00Z' ORDER BY [System.ChangedDate] DESC"
}
```

**Update Work Item**:
```
Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_update_work_item with:
{
  "id": 1382,
  "title": "Identity Service Migration",
  "description": "<h2>Updated Description</h2>...",
  "storyPoints": 25,
  "priority": 2,
  "tags": ["identity", "authentication", "entra-id", "security"]
}
```

---

## Error Handling

### Common Errors

**1. Work Item Not Found**
- Error: `TF401232: Work item 1399 does not exist`
- Resolution: Remove `ado_work_item_id` from spec.md frontmatter, run push sync to recreate

**2. Parent Work Item Invalid**
- Error: `TF201036: You cannot add a Child link between work items 1382 and 9999`
- Resolution: Verify `.ado-hierarchy.json` has correct Feature IDs, update parent mappings

**3. Permission Denied**
- Error: `TF400499: You do not have permission to access this resource`
- Resolution: Verify Azure DevOps PAT has work item read/write permissions

**4. Rate Limit Exceeded**
- Error: `TF200016: The following request has exceeded the allowed threshold`
- Resolution: Agent automatically retries with exponential backoff (1s, 2s, 4s, 8s)

**5. Invalid Field Value**
- Error: `TF401320: Rule violation: Value for field 'Story Points' cannot be negative`
- Resolution: Validate spec.md frontmatter, ensure story_points > 0

### Retry Strategy

```
Max retries: 3
Backoff: Exponential (1s, 2s, 4s)
Retryable errors:
  - HTTP 429 (Rate limit)
  - HTTP 500 (Server error)
  - HTTP 503 (Service unavailable)
  - Network timeout

Non-retryable errors:
  - HTTP 400 (Bad request - fix data)
  - HTTP 401 (Unauthorized - fix credentials)
  - HTTP 403 (Forbidden - fix permissions)
  - HTTP 404 (Not found - work item deleted)
```

---

## Bootstrap Process

### Initial Setup (One-Time)

**Goal**: Add frontmatter to existing 18 spec.md files for already-created User Stories.

**Steps**:

1. **Create Hierarchy Registry**
   ```
   @workspace Create .ado-hierarchy.json in Plan/Foundation/specs/ with:
   - Epic 1375: Foundation Layer
   - Epic 1376: DigitalInk Layer
   - Feature 1377: Phase 1 (parent: 1375)
   - Feature 1379: Phase 2 (parent: 1375)
   - Feature 1378: Phase 3 (parent: 1375)
   - Feature 1380: Phase 4 (parent: 1375)
   ```

2. **Map Work Items to Specs**
   ```
   Phase 1 (Feature 1377):
   - 1382 → Identity Service
   - 1381 → API Gateway
   - 1383 → Configuration Service
   - 1384 → Infrastructure Setup
   - 1388 → Multi-Tenant Database
   - 1386 → UI Migration
   - 1385 → Data Migration
   - 1387 → DevOps Pipeline
   
   Phase 2 (Feature 1379):
   - 1390 → Student Management
   - 1389 → Staff Management
   - 1391 → Assessment Service
   
   Phase 3 (Feature 1378):
   - 1392 → Intervention Management
   - 1393 → Section Roster Service
   - 1394 → Data Import Service
   
   Phase 4 (Feature 1380):
   - 1395 → Reporting & Analytics
   - 1397 → Content & Media Service
   - 1398 → System Operations Service
   - 1396 → Digital Ink Service
   ```

3. **Add Frontmatter to Each Spec**
   ```
   For each spec.md:
   - Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_get_work_item to fetch current state
   - Extract: title, story points, priority, tags, ChangedDate
   - Add YAML frontmatter block at top of file:
     ---
     ado_work_item_id: [work item ID]
     ado_parent_id: [Feature ID]
     ado_work_item_type: "User Story"
     ado_url: "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/[id]"
     last_synced: "[System.ChangedDate]"
     sync_status: "synced"
     story_points: [value from ADO]
     priority: [value from ADO]
     tags: [array from ADO]
     ---
   ```

4. **Verify Bootstrap**
   ```
   @workspace /sync-ado validate
   
   Expected output:
   - 18 specs with valid frontmatter
   - 18 work items found in ADO
   - 0 orphaned work items
   - All sync_status: "synced"
   ```

---

## Usage Examples

### Example 0: Requirement Key Extraction (Identity Service)

**Scenario**: Agent analyzes Identity Service spec with explicit user stories and creates requirement key-based User Stories.

**Input Files**:

**spec.md** (User Scenarios section):
```markdown
## User Scenarios

### US1: Staff Member Logs In Using Microsoft Entra ID SSO
**Priority**: [P1]

**Description**: A staff member navigates to the NorthStarET application...

**Acceptance Scenarios**:
- Staff clicks "Sign in with Microsoft"
- Redirected to Entra ID login page
- Successfully authenticated and redirected back with session

### US2: Administrator Logs In Using Entra ID with MFA
**Priority**: [P1]

**Description**: A district administrator with elevated privileges...
```

**tasks.md** (Phase 3):
```markdown
### Phase 3: Authentication Flow (US1, US2)

#### T025: [US1] Implement Entra ID OIDC flow [P1]
Add Microsoft.Identity.Web package and configure authentication...

#### T026: [US1] Create SessionAuthenticationHandler [P1]
Custom authentication handler that validates sessions...

#### T027: [US1] Add login/logout endpoints [P1]
API endpoints for initiating Entra ID login...

#### T028: [US2] Configure MFA requirements [P1]
Set up conditional access policies for admin roles...

#### T029: [US2] Admin role claim mapping [P1]
Map Entra ID groups to application admin roles...

#### T030: [US1] Session storage in PostgreSQL [P1]
Create Sessions table and repository...
```

**Agent Analysis Process**:
```
1. Agent scans Phase 3 tasks and finds [US1] and [US2] markers
2. Agent loads spec.md and searches for "### US1:" and "### US2:"
3. Agent extracts full titles:
   - US1: "Staff Member Logs In Using Microsoft Entra ID SSO"
   - US2: "Administrator Logs In Using Entra ID with MFA"
4. Agent groups tasks:
   - US1: T025, T026, T027, T030 (4 tasks = 4 story points)
   - US2: T028, T029 (2 tasks = 2 story points)
5. Agent creates User Stories in ADO:
   - Title: "Phase 3: US1 - Staff Member Logs In Using Microsoft Entra ID SSO"
   - Title: "Phase 3: US2 - Administrator Logs In Using Entra ID with MFA"
```

**Hierarchy Registry Result**:
```json
"phase3-authentication": {
  "work_item_id": 1404,
  "title": "Phase 3: Authentication Flow",
  "user_stories": {
    "us1-staff-login": {
      "work_item_id": 1425,
      "title": "Phase 3: US1 - Staff Member Logs In Using Microsoft Entra ID SSO",
      "grouping_strategy": "requirement_key",
      "requirement_key": "US1",
      "requirement_name": "Staff Member Logs In Using Microsoft Entra ID SSO",
      "story_points": 4,
      "priority": 1
    },
    "us2-admin-login": {
      "work_item_id": 1426,
      "title": "Phase 3: US2 - Administrator Logs In Using Entra ID with MFA",
      "grouping_strategy": "requirement_key",
      "requirement_key": "US2",
      "requirement_name": "Administrator Logs In Using Entra ID with MFA",
      "story_points": 2,
      "priority": 1
    }
  }
}
```

**Fallback Example** (Phase 1 with no requirement keys):
```markdown
### Phase 1: Project Setup

#### T001: Initialize .NET Aspire AppHost project
Create new .NET 9 solution with Aspire.AppHost...

#### T002: Configure PostgreSQL container
Add PostgreSQL resource to AppHost...

#### T003: Configure Redis container
Add Redis Stack resource for caching...

#### T004: Setup EF Core migrations infrastructure
Install EF Core tools and configure migration paths...
```

**Agent Fallback Analysis**:
```
1. Agent scans Phase 1 tasks, finds no [US#] markers
2. Agent analyzes task descriptions for functional patterns:
   - T001: "Initialize", "project" → Setup
   - T002-T003: "Configure", "container", "PostgreSQL", "Redis" → Infrastructure
   - T004: "migrations", "EF Core" → Database
3. Agent decides functional grouping unclear, falls back to single group
4. Agent creates User Story: "Phase 1: Project Setup & Infrastructure"
5. Grouping strategy: "functional_group"
```

---

### Example 1: Create New Service Spec

**Scenario**: Developer creates new service spec and wants to create ADO work item.

**Steps**:
```
1. Create Plan/Foundation/specs/005-reporting-service/spec.md:
   ---
   story_points: 13
   priority: 1
   tags:
     - reporting
     - analytics
   ---
   
   # Reporting & Analytics Service
   
   ## Description
   ...
   
   ## Acceptance Criteria
   - [ ] Real-time dashboard
   - [ ] Export to PDF
   ...

2. Create Plan/Foundation/specs/005-reporting-service/tasks.md:
   ### Phase 1: API Development
   #### Task 1.1: Create report endpoints
   ...

3. Run sync:
   @workspace /sync-ado push reporting-service

4. Agent creates User Story #1399 in ADO under Feature 1380 (Phase 4)

5. Agent updates spec.md with:
   ---
   ado_work_item_id: 1399
   ado_parent_id: 1380
   ado_work_item_type: "User Story"
   ado_url: "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1399"
   last_synced: "2025-11-20T16:00:00Z"
   sync_status: "synced"
   ...
```

### Example 2: Update Story Points in ADO

**Scenario**: PM updates story points in Azure DevOps, developer pulls changes.

**Steps**:
```
1. PM changes User Story #1382 (Identity Service) story points from 21 → 34 in ADO

2. Developer runs sync:
   @workspace /sync-ado pull identity-service

3. Agent detects change (System.ChangedDate > last_synced)

4. Agent updates spec.md frontmatter:
   story_points: 34  # Changed from 21
   last_synced: "2025-11-20T17:30:00Z"

5. Developer commits:
   git add specs/001-identity-service/spec.md
   git commit -m "sync: Pull updates from ADO [1382]"
```

### Example 3: Resolve Conflict

**Scenario**: Developer updates spec locally, PM updates ADO simultaneously.

**Steps**:
```
1. Developer edits specs/001-identity-service/spec.md:
   - Changes description
   - Updates story_points: 34 → 55

2. PM edits User Story #1382 in ADO:
   - Changes priority: 1 → 2
   - Adds tag: "security"

3. Developer runs sync:
   @workspace /sync-ado push identity-service

4. Agent detects conflict:
   - spec.md modified: 2025-11-20T14:30:00Z
   - ADO modified: 2025-11-20T15:45:00Z
   - last_synced: 2025-11-20T10:00:00Z

5. Agent creates specs/001-identity-service/spec.conflict with both versions

6. Agent outputs:
   ⚠️ Conflict detected: Identity Service (User Story #1382)
   
   Both repository and Azure DevOps have changes since last sync.
   Review conflict file: specs/001-identity-service/spec.conflict
   
   Resolve with:
   - Repository wins: @workspace /sync-ado resolve specs/001-identity-service/spec.md --strategy=repo-wins
   - ADO wins: @workspace /sync-ado resolve specs/001-identity-service/spec.md --strategy=ado-wins
   - Manual merge: Edit spec.md, delete spec.conflict, then push

7. Developer reviews spec.conflict, decides to keep ADO priority/tags but repo story points

8. Developer manually edits spec.md:
   story_points: 55  # From repo
   priority: 2       # From ADO
   tags:             # From ADO
     - identity
     - security

9. Developer deletes spec.conflict

10. Developer runs sync:
    @workspace /sync-ado push identity-service

11. Agent updates ADO with merged content
```

### Example 4: Validate Before PR

**Scenario**: Developer wants to ensure all specs are synced before creating PR.

**Steps**:
```
1. Developer runs validation:
   @workspace /sync-ado validate

2. Agent reports:
   ⚠️ Validation Issues Found
   
   Needs Push (2):
   - specs/001-identity-service/spec.md (modified locally)
   - specs/005-reporting-service/spec.md (no ado_work_item_id)
   
   Needs Pull (1):
   - User Story #1390 (Student Management) - ADO modified
   
   All other specs: synced ✅

3. Developer syncs:
   @workspace /sync-ado push
   @workspace /sync-ado pull

4. Developer re-validates:
   @workspace /sync-ado validate
   
   ✅ All specs synced!

5. Developer creates PR with confidence
```

---

## Performance Considerations

### Batch Operations
- Process specs in batches of 10 to avoid rate limits
- Add 1-second delay between batches
- Azure DevOps API limit: ~200 requests/minute

### Incremental Sync
- Use `last_synced` timestamps to query only modified items
- Skip specs with `sync_status: "synced"` and no local changes
- Only pull work items changed since earliest `last_synced`

### Caching
- Load `.ado-hierarchy.json` once per sync operation
- Cache work item queries within single sync session
- Reuse MCP connections

---

## Monitoring & Alerting

### Health Checks
- Run `@workspace /sync-ado validate` nightly
- Check for orphaned work items monthly
- Verify hierarchy registry accuracy quarterly

### Drift Detection
- Alert when `sync_status: "conflict"` persists > 24 hours
- Notify when work items exist without spec.md > 7 days
- Track average sync latency (should be < 5 minutes)

### Metrics
- Sync success rate (target: >95%)
- Conflict resolution time (target: <1 hour)
- Spec coverage (target: 100% have ado_work_item_id)

---

## Troubleshooting

### Problem: "Work item not found"
**Symptom**: `TF401232: Work item 1399 does not exist`  
**Diagnosis**: Work item deleted in ADO but spec.md still references it  
**Fix**: Remove `ado_work_item_id` from frontmatter, run `@workspace /sync-ado push` to recreate

### Problem: "Parent link invalid"
**Symptom**: `TF201036: You cannot add a Child link between work items`  
**Diagnosis**: Either using wrong link type ("child" instead of "parent") or Parent Feature ID in hierarchy registry is incorrect  
**Fix**: 
- For Task linking: Use `type: "parent"` in work_items_link call (Tasks link TO parents, not parents link TO children)
- For Feature linking: Update `.ado-hierarchy.json` with correct Feature IDs, run `@workspace /sync-ado push --force`

### Problem: "Tasks not showing under User Story"
**Symptom**: Tasks created but don't appear as children of User Story in Azure DevOps  
**Diagnosis**: Tasks were created without proper parent linking (System.Parent field doesn't work during creation)  
**Fix**: 
1. Get Task IDs that need linking
2. Use `mcp_microsoft_azu_wit_work_items_link` with `type: "parent"` and `linkToId: [User Story ID]`
3. Verify by querying Task with `expand: "relations"` to see parent link in relations array

### Problem: "Conflict loop"
**Symptom**: Every sync detects conflicts  
**Diagnosis**: Timestamps not updating correctly  
**Fix**: Manually set `last_synced` to current time, delete `.conflict` files, run `@workspace /sync-ado push --force`

### Problem: "Rate limit exceeded"
**Symptom**: `TF200016: The following request has exceeded the allowed threshold`  
**Diagnosis**: Too many API calls in short time  
**Fix**: Agent automatically retries with backoff. If persistent, reduce batch size in agent code.

### Problem: "Permission denied"
**Symptom**: `TF400499: You do not have permission`  
**Diagnosis**: Azure DevOps PAT lacks work item permissions  
**Fix**: Regenerate PAT with "Work Items (Read, Write, & Manage)" scope

---

## Future Enhancements

### Planned Features
1. **Automated Pull Requests**: Generate PRs when ADO changes detected
2. **Task State Sync**: Sync individual checklist states to ADO Comments
3. **Attachment Sync**: Upload spec diagrams as ADO attachments
4. **Multi-Project Support**: Extend beyond NorthStarET project
5. **Webhook Integration**: Real-time sync on ADO changes (no polling)
6. **Conflict Merge UI**: Interactive conflict resolution in VS Code

### Not Planned (Out of Scope)
- Custom ADO fields (per user requirement)
- Task work items (using checklist approach instead)
- Two-way git history sync (ADO discussion → git commits)
- Automated story point estimation
- Time tracking sync

---

## Version History

### 1.0.0 (November 20, 2025)
- Initial release
- Bidirectional sync (push/pull)
- Conflict detection and resolution
- Task checklist embedding
- Hierarchy registry
- Bootstrap process
- Validation command

---

## Support

**Issues**: Create GitHub Issue with `sync-agent` label  
**Questions**: Ask in GitHub Discussions  
**Documentation**: See `Plan/Foundation/agents/README.md`

---

**End of Agent Documentation**
