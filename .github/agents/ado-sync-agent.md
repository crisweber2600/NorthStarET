# Azure DevOps Sync Agent

**Version**: 1.0.0  
**Author**: GitHub Copilot  
**Last Updated**: November 20, 2025

## Purpose

This agent maintains bidirectional synchronization between repository specification files (`Plan/Foundation/specs/**/*.md`) and Azure DevOps work items in the NorthStarET project. It uses the `#microsoft/azure-devops-mcp` tool to perform all Azure DevOps operations.

## Core Capabilities

### 1. Push Sync (Repository → Azure DevOps)
- Creates new work items from spec.md files without `ado_work_item_id`
- Updates existing work items when spec content changes
- Maintains parent-child hierarchy (Epic → Feature → User Story)
- Embeds task checklists in User Story descriptions

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
   - Generate 4 checklist items per task:
     * [ ] Coded
     * [ ] Tested
     * [ ] Reviewed
     * [ ] Merged
   ```

5. **Resolve Parent ID**
   ```
   Use folder path to lookup Feature ID in hierarchy registry:
   - Extract phase from folder name (e.g., "001-phase1-foundation-services")
   - Match to Feature work item ID in registry
   - If not found, use Epic ID as parent
   ```

6. **Create or Update Work Item**
   ```
   If no ado_work_item_id exists:
     Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_create_work_item
     Parameters:
       - project: "NorthStarET"
       - workItemType: "User Story"
       - title: From spec.md title
       - description: Markdown content + embedded task checklist (HTML)
       - acceptanceCriteria: From spec.md acceptance criteria section
       - storyPoints: From spec.md frontmatter
       - priority: From spec.md frontmatter
       - tags: From spec.md frontmatter
       - parentId: Resolved Feature ID
     
     After creation:
       - Update spec.md frontmatter with new ado_work_item_id
       - Set ado_parent_id, ado_work_item_type, ado_url
       - Set last_synced: current ISO timestamp
       - Set sync_status: "synced"
   
   Else if ado_work_item_id exists:
     Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_get_work_item
     Query existing work item by ID
     
     Compare ChangedDate with last_synced:
       If ChangedDate > last_synced:
         CONFLICT DETECTED
         Generate conflict report (see Conflict Resolution section)
       Else:
         Use #microsoft/azure-devops-mcp mcp_microsoft_azu_wit_update_work_item
         Parameters:
           - id: ado_work_item_id
           - title: From spec.md
           - description: Updated markdown + task checklist
           - acceptanceCriteria: Updated from spec.md
           - storyPoints: Updated from frontmatter
           - priority: Updated from frontmatter
           - tags: Updated from frontmatter
         
         Update spec.md frontmatter:
           - Set last_synced: current ISO timestamp
           - Set sync_status: "synced"
   ```

7. **Commit Frontmatter Updates**
   ```
   Use #filesystem to write updated spec.md files
   Commit with message: "sync: Push updates to ADO [work-item-ids]"
   ```

**Output**:
```
✅ Synced 8 work items to Azure DevOps

Created:
- Identity Service (User Story #1399) → Phase 1 Foundation Services
- API Gateway (User Story #1400) → Phase 1 Foundation Services

Updated:
- Configuration Service (User Story #1383) - Updated story points 8 → 13
- Student Management (User Story #1390) - Updated description

No changes:
- Staff Management (User Story #1389)
- Assessment Service (User Story #1391)
- Infrastructure Setup (User Story #1384)
- Multi-Tenant Database (User Story #1388)
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

### Spec.md Frontmatter

```yaml
---
ado_work_item_id: 1382           # Azure DevOps work item ID (required after sync)
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

**Location**: `Plan/Foundation/specs/.ado-hierarchy.json`

```json
{
  "version": "1.0.0",
  "last_updated": "2025-11-20T10:00:00Z",
  "epics": {
    "foundation-layer": {
      "work_item_id": 1375,
      "title": "Foundation Layer",
      "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1375",
      "folder": "Plan/Foundation"
    },
    "digital-ink-layer": {
      "work_item_id": 1376,
      "title": "DigitalInk Layer",
      "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1376",
      "folder": "Plan/Foundation/specs/DigitalInk"
    }
  },
  "features": {
    "phase1-foundation-services": {
      "work_item_id": 1377,
      "parent_epic": "foundation-layer",
      "parent_work_item_id": 1375,
      "title": "Phase 1: Foundation Services",
      "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1377",
      "folder_pattern": "001-phase1-*"
    },
    "phase2-core-domain-services": {
      "work_item_id": 1379,
      "parent_epic": "foundation-layer",
      "parent_work_item_id": 1375,
      "title": "Phase 2: Core Domain Services",
      "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1379",
      "folder_pattern": "002-phase2-*"
    },
    "phase3-secondary-domain-services": {
      "work_item_id": 1378,
      "parent_epic": "foundation-layer",
      "parent_work_item_id": 1375,
      "title": "Phase 3: Secondary Domain Services",
      "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1378",
      "folder_pattern": "003-phase3-*"
    },
    "phase4-supporting-services": {
      "work_item_id": 1380,
      "parent_epic": "foundation-layer",
      "parent_work_item_id": 1375,
      "title": "Phase 4: Supporting Services",
      "url": "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1380",
      "folder_pattern": "004-phase4-*"
    }
  }
}
```

### Task Checklist Format

**In tasks.md**:
```markdown
### Phase 1: Setup

#### Task 1.1: Install Duende IdentityServer
- Description: Add Duende.IdentityServer NuGet package and configure services
- Estimated: 5 story points

#### Task 1.2: Configure Entra ID integration
- Description: Setup Azure AD authentication flow
- Estimated: 8 story points
```

**Embedded in User Story Description (HTML)**:
```html
<h2>Tasks</h2>
<h3>Phase 1: Setup</h3>
<ul>
<li><strong>Task 1.1: Install Duende IdentityServer</strong>
  <ul>
    <li>☐ Coded</li>
    <li>☐ Tested</li>
    <li>☐ Reviewed</li>
    <li>☐ Merged</li>
  </ul>
</li>
<li><strong>Task 1.2: Configure Entra ID integration</strong>
  <ul>
    <li>☑ Coded</li>
    <li>☑ Tested</li>
    <li>☐ Reviewed</li>
    <li>☐ Merged</li>
  </ul>
</li>
</ul>
```

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
**Diagnosis**: Parent Feature ID in hierarchy registry is incorrect  
**Fix**: Update `.ado-hierarchy.json` with correct Feature IDs, run `@workspace /sync-ado push --force`

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
