# Custom Agents Architecture

This document describes the architecture and relationships between custom agents in the NorthStarET repository.

## Agent Dependency Graph

```
┌─────────────────────────────────────────────────────────────────────┐
│                      SCENARIO PROCESSING                            │
│                                                                     │
│  speckit.scenario ──────┐                                          │
│       │                 │                                          │
│       │ For each        │                                          │
│       │ scenario        │                                          │
│       │                 │                                          │
│       ├─────────────────┴───> speckit.specify ──────┐             │
│       │                           │                  │             │
│       │                           │                  │             │
│       │                           ▼                  │             │
│       │                    [Creates Branch]          │             │
│       │                           │                  │             │
│       │                           ▼                  │             │
│       └───────────────────> speckit.plan ───────────┤             │
│                                   │                  │             │
│                                   │                  │             │
│                                   ▼                  │             │
│                         [Generates Artifacts]        │             │
│                                                      │             │
└──────────────────────────────────────────────────────┼─────────────┘
                                                       │
                                                       │
┌──────────────────────────────────────────────────────┼─────────────┐
│                    TASK GENERATION                   │             │
│                                                      │             │
│                         speckit.tasks <──────────────┘             │
│                                │                                   │
│                                ├─────────> speckit.analyze         │
│                                │               │                   │
│                                │               └───> [Quality Report]
│                                │                                   │
│                                └─────────> speckit.taskstoissues   │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
                                │
                                │
┌───────────────────────────────┼─────────────────────────────────────┐
│                    IMPLEMENTATION                   │               │
│                                                     │               │
│                         speckit.implement <─────────┘               │
│                                │                                    │
│                                ├────────> speckit.checklist         │
│                                │               │                    │
│                                │               └─> [Validation Lists]
│                                │                                    │
│                                └────────> speckit.coach             │
│                                                │                    │
│                                                └─> [Guidance]       │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                      CLARIFICATION & REFINEMENT                     │
│                                                                     │
│  speckit.clarify ────────> [Updates Spec]                          │
│                                                                     │
│  speckit.constitution ───> [Project Principles]                    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Agent Types

### 1. Entry Point Agents

These agents are typically invoked directly by users:

- **`speckit.scenario`** (NEW) - Batch processes scenario files
- **`speckit.specify`** - Creates feature specifications
- **`speckit.coach`** - Provides guidance and best practices

### 2. Pipeline Agents

These agents are invoked as part of a workflow:

- **`speckit.plan`** - Generates implementation plans
- **`speckit.tasks`** - Generates task breakdowns
- **`speckit.implement`** - Executes implementation

### 3. Support Agents

These agents provide additional functionality:

- **`speckit.analyze`** - Quality analysis
- **`speckit.clarify`** - Specification refinement
- **`speckit.checklist`** - Validation checklist generation
- **`speckit.constitution`** - Project principles management
- **`speckit.taskstoissues`** - GitHub issue creation

## Workflow Patterns

### Pattern 1: Scenario-Driven Development (NEW)

**Use Case**: Batch process multiple scenarios from legacy documentation

```
┌─────────────┐
│  User       │
│  Invokes    │
└──────┬──────┘
       │ /speckit.scenario Plan/Foundation/Plans/scenarios
       ▼
┌─────────────────────────────────────────────────┐
│  speckit.scenario Agent                         │
│  1. Reads all .md files in directory            │
│  2. For each scenario:                          │
├─────────────────────────────────────────────────┤
│     ┌───────────────────────────────────────┐   │
│     │ A. Extract Given/When/Then scenarios  │   │
│     │ B. Format as feature description      │   │
│     └────────────┬──────────────────────────┘   │
│                  │                              │
│                  ▼                              │
│     ┌───────────────────────────────────────┐   │
│     │ C. Invoke speckit-specify             │   │
│     │    - Generate short name              │   │
│     │    - Create feature branch            │   │
│     │    - Generate spec.md                 │   │
│     └────────────┬──────────────────────────┘   │
│                  │                              │
│                  ▼                              │
│     ┌───────────────────────────────────────┐   │
│     │ D. Switch to feature branch           │   │
│     └────────────┬──────────────────────────┘   │
│                  │                              │
│                  ▼                              │
│     ┌───────────────────────────────────────┐   │
│     │ E. Invoke speckit-plan                │   │
│     │    - Generate plan.md                 │   │
│     │    - Generate research.md             │   │
│     │    - Generate data-model.md           │   │
│     │    - Generate contracts/              │   │
│     └────────────┬──────────────────────────┘   │
│                  │                              │
│                  ▼                              │
│     ┌───────────────────────────────────────┐   │
│     │ F. Record results                     │   │
│     └───────────────────────────────────────┘   │
│                                                 │
│  3. Generate processing report                  │
│  4. Return to original branch                   │
└─────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────┐
│  Output:                                        │
│  - Multiple feature branches created            │
│  - Each with complete spec and plan             │
│  - Processing report generated                  │
│  - Ready for parallel implementation            │
└─────────────────────────────────────────────────┘
```

**Timeline**: 2-5 minutes per scenario (for 10 scenarios: ~30 minutes)

### Pattern 2: Single Feature Development

**Use Case**: Create one feature from description

```
User → /speckit.specify → spec.md created
              ↓
         /speckit.plan → plan.md, research.md, data-model.md created
              ↓
        /speckit.tasks → tasks.md created
              ↓
     /speckit.implement → Feature implemented
```

### Pattern 3: Feature Refinement

**Use Case**: Improve existing specification

```
User → /speckit.clarify → Questions asked, spec updated
              ↓
       /speckit.analyze → Quality issues identified
              ↓
       [Manual fixes]
              ↓
       /speckit.analyze → Quality validated
```

### Pattern 4: Team Collaboration

**Use Case**: Convert tasks to trackable issues

```
User → /speckit.tasks → tasks.md created
              ↓
   /speckit.taskstoissues → GitHub issues created
              ↓
       [Team works on issues in parallel]
```

## Agent Handoffs

Agents can automatically hand off to other agents using the `handoffs` section in front matter:

```yaml
---
description: Agent description
handoffs: 
  - label: Next Action
    agent: speckit.next
    prompt: Default prompt for next agent
    send: true/false  # true = automatic handoff, false = manual
---
```

### Automatic Handoffs

When `send: true`, the agent automatically invokes the next agent after completion:

```
speckit.specify (send: true) → speckit.plan (automatic)
speckit.tasks (send: true) → speckit.analyze (automatic)
```

### Manual Handoffs

When `send: false`, the user must manually invoke the next agent:

```
speckit.scenario (send: false) → User chooses next action
```

## Data Flow

### Scenario Processing Flow

```
Scenario File (Given/When/Then)
       │
       ▼
Feature Description (Natural language)
       │
       ▼
spec.md (Structured specification)
       │
       ├──> User Stories
       ├──> Acceptance Criteria
       ├──> Success Metrics
       └──> Key Entities
       │
       ▼
plan.md (Technical plan)
       │
       ├──> Technology Stack
       ├──> Architecture
       ├──> File Structure
       └──> Implementation Phases
       │
       ▼
tasks.md (Actionable tasks)
       │
       ├──> Setup Tasks
       ├──> Foundation Tasks
       ├──> Feature Tasks (by story)
       └──> Polish Tasks
       │
       ▼
Implementation (Code)
```

### Artifact Dependencies

```
constitution.md ──┬──> spec.md
                  │
                  ├──> plan.md ──┬──> research.md
                  │              ├──> data-model.md
                  │              └──> contracts/
                  │
                  └──> tasks.md ──┬──> checklists/
                                  └──> GitHub Issues
```

## Agent State Management

### Stateless Agents

Most agents are stateless and rely on file artifacts:
- `speckit.specify` - Creates spec.md
- `speckit.plan` - Reads spec.md, creates plan.md
- `speckit.tasks` - Reads spec.md and plan.md, creates tasks.md

### Stateful Context

Agents maintain context through:
1. **Current Git Branch** - Determines which feature is active
2. **File System** - Specs, plans, tasks in `specs/[feature]/`
3. **Git History** - Previous commits and changes
4. **Constitution** - Global project principles in `.specify/memory/constitution.md`

## Agent Configuration

### Front Matter Structure

```yaml
---
description: Agent purpose (shown in tool description)
handoffs: 
  - label: Human-readable action name
    agent: speckit.targetagent  # Target agent name (without "speckit-" prefix)
    prompt: Default prompt to send
    send: true  # Auto-invoke or manual
---
```

### Agent Body Structure

```markdown
## User Input
[Captures $ARGUMENTS from /speckit.agent invocation]

## Outline
[Step-by-step workflow with bash commands and logic]

## Error Handling (optional)
[Error conditions and recovery steps]

## Usage Examples (optional)
[Example invocations]
```

## Security Considerations

### Git Operations

Agents perform git operations that require:
- Clean working directory
- Proper branch management
- No uncommitted changes before switching branches

### File System Access

Agents have full filesystem access to:
- Read scenario files
- Create/update specifications
- Generate artifacts
- Commit changes

### Agent Invocation

Custom agents are invoked through GitHub Copilot's tool system:
- No direct shell execution
- Sandboxed environment
- Validated inputs

## Performance Characteristics

### Agent Processing Times

| Agent | Typical Duration | Depends On |
|-------|-----------------|------------|
| `speckit.scenario` | 2-5 min per scenario | Number of scenarios, scenario complexity |
| `speckit.specify` | 30-90 seconds | Feature complexity |
| `speckit.plan` | 60-120 seconds | Tech stack research needs |
| `speckit.tasks` | 30-60 seconds | Number of user stories |
| `speckit.implement` | Variable | Task complexity and count |
| `speckit.analyze` | 20-40 seconds | Artifact size |
| `speckit.clarify` | 30-60 seconds | Ambiguity count |

### Optimization Strategies

1. **Batch Processing** - Use `speckit.scenario` for multiple scenarios
2. **Parallel Implementation** - Work on multiple feature branches
3. **Incremental Processing** - Process one scenario to validate before batch
4. **Caching** - Git branch caching reduces repetitive operations

## Extensibility

### Adding New Agents

1. Create `.github/agents/speckit.newagent.agent.md`
2. Follow front matter structure
3. Implement workflow in `## Outline` section
4. Document in `agents/README.md`
5. Add to this architecture diagram

### Agent Template

```markdown
---
description: What this agent does
handoffs: 
  - label: Next Step
    agent: speckit.next
    prompt: Default prompt
    send: false
---

## User Input
```text
$ARGUMENTS
```

## Outline

1. **Setup**: Describe prerequisites
2. **Process**: Step-by-step workflow
3. **Output**: What gets generated
4. **Report**: Status and next steps

## Error Handling

- **Error Type**: Solution

## Usage Examples

```
/speckit.newagent arg1 arg2
```
```

## Integration Points

### GitHub Integration

- Branch creation and management
- Issue creation (`speckit.taskstoissues`)
- Commit and push operations

### MCP Servers

Agents can leverage MCP servers:
- `filesystem` - File operations
- `github` - GitHub API access
- `sequential-thinking` - Complex reasoning
- `microsoft-docs` - .NET documentation

### External Tools

Agents invoke external tools:
- `git` - Version control
- Bash scripts in `.specify/scripts/bash/`
- Language-specific tools (dotnet, npm, etc.)

## Monitoring and Debugging

### Agent Execution Logs

When agents run, they:
1. Log progress to terminal
2. Generate status reports
3. Record errors and warnings

### Debugging Failed Agents

1. Check agent output for error messages
2. Verify prerequisites (clean git state, files exist)
3. Check git branch and filesystem state
4. Review generated artifacts for partial completion
5. Re-run with corrected inputs

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| "Scenario file not found" | Invalid path | Verify file exists |
| "Git state not clean" | Uncommitted changes | Commit or stash changes |
| "Branch already exists" | Previous run | Use `--force` or different scenario |
| "Agent not found" | Typo in agent name | Check `.github/agents/` for correct name |

## Future Architecture Improvements

### Planned Enhancements

1. **Parallel Agent Execution** - Process multiple scenarios simultaneously
2. **Agent Pipelines** - Compose agents into reusable pipelines
3. **Agent Versioning** - Track agent versions and changes
4. **Agent Testing Framework** - Automated testing of agent workflows
5. **Agent Metrics** - Performance tracking and optimization
6. **Agent Dependencies** - Explicit dependency management
7. **Agent Templates** - Standardized agent creation

### Research Areas

1. **Distributed Agent Execution** - Run agents across multiple machines
2. **Agent Learning** - Improve agents based on usage patterns
3. **Agent Composition** - Build complex workflows from simple agents
4. **Agent Validation** - Static analysis of agent definitions

## Summary

The custom agent architecture provides:

✅ **Modular Design** - Each agent has a single responsibility
✅ **Composable Workflows** - Agents can be chained together
✅ **Extensible Framework** - Easy to add new agents
✅ **Automated Processes** - Reduce manual specification and planning work
✅ **Scalable Approach** - Handle multiple features in parallel

The new `speckit.scenario` agent extends this architecture to support batch processing of scenario files, enabling efficient migration and feature development workflows.
