# CrossCuttingConcerns Specifications - Resolution Action Plan

**Generated**: 2025-11-20T22:55:00Z  
**Related Report**: `SPECIFICATION_STATUS_REPORT.md`  
**Status**: Awaiting Stakeholder Decisions

---

## Overview

All 4 CrossCuttingConcerns scenarios have been processed into specifications, but there are structural issues that need resolution before proceeding to implementation. This document provides a clear action plan to resolve these issues.

---

## Critical Issues Requiring Decisions

### Issue #1: Missing Specification Branch (000-aspire-scaffolding)

**Problem**: Feature 000-aspire-scaffolding has a proposed branch but no specification branch, violating Constitution v2.1.0 workflow.

**Impact**: 
- Cannot follow standard workflow for modifications
- Cannot run `/speckit.tasks` or `/speckit.implement` from spec branch
- Stakeholders reviewing proposed branch have no working branch to reference

**Options**:

#### Option A: Create Spec Branch from Proposed (Recommended)
```bash
# Manual action required (coding agent cannot push directly)
git checkout CrossCuttingConcerns/000-aspire-scaffolding-proposed
git checkout -b CrossCuttingConcerns/000-aspire-scaffolding-spec
git push -u origin CrossCuttingConcerns/000-aspire-scaffolding-spec
```
**Pros**: Preserves all work, follows Constitution v2.1.0  
**Cons**: Requires manual git operation

#### Option B: Accept Proposed-Only Structure
Keep only the proposed branch and document the exception.

**Pros**: No work needed  
**Cons**: Violates established workflow, may confuse future work

**Recommendation**: **Option A** - Create the spec branch to maintain consistency.

---

### Issue #2: Duplicate Identity Service Specifications

**Problem**: Two identity service specifications exist with different naming:
- `001-identity-service` (complete: spec.md, plan.md, research.md, data-model.md)
- `01-identity-service-entra-id` (partial: only spec.md)

**Impact**:
- Confusion about which version is canonical
- Wasted effort if both are maintained
- Potential for divergent implementations

**Options**:

#### Option A: Keep 001-identity-service, Archive 01-entra-id (Recommended)
Use `001-identity-service` as the canonical version since it's more complete.

**Actions**:
```bash
# Archive the incomplete version
git branch -m CrossCuttingConcerns/01-identity-service-entra-id-spec CrossCuttingConcerns/ARCHIVED-01-identity-service-entra-id-spec
git branch -m CrossCuttingConcerns/01-identity-service-entra-id-proposed CrossCuttingConcerns/ARCHIVED-01-identity-service-entra-id-proposed
# Push renamed branches
```

**Pros**: Clear canonical version, eliminates confusion  
**Cons**: Loses alternative approach (if intentional)

#### Option B: Complete 01-entra-id, Archive 001-identity
Complete the `01-identity-service-entra-id` specification if it represents a different or better approach.

**Actions**:
```bash
git checkout CrossCuttingConcerns/01-identity-service-entra-id-spec
/speckit.plan  # Re-run to generate missing artifacts
```

**Pros**: Preserves potentially better approach  
**Cons**: More work, unclear why two versions exist

#### Option C: Merge Both Approaches
Review both specifications and create a single consolidated version combining the best of both.

**Pros**: Comprehensive solution  
**Cons**: Most work, requires deep review

**Recommendation**: **Option A** - Keep 001-identity-service (more complete) unless there's a specific reason for the alternative version.

---

### Issue #3: API Gateway Version Conflict

**Problem**: Two API Gateway specifications exist with conflicting numbers:
- `001-api-gateway-yarp` (YARP-specific, newer, better documented in processing report)
- `002-api-gateway` (generic naming, older)

**Impact**:
- Feature number 001 conflicts with identity service
- Unclear which version to implement
- Numbering sequence broken

**Options**:

#### Option A: Use 002-api-gateway, Renumber 001-yarp to 004 (Recommended)
Keep the original numbering sequence clean.

**Actions**:
```bash
# Renumber YARP version to 004
git branch -m CrossCuttingConcerns/001-api-gateway-yarp-spec CrossCuttingConcerns/004-api-gateway-yarp-spec
git branch -m CrossCuttingConcerns/001-api-gateway-yarp-proposed CrossCuttingConcerns/004-api-gateway-yarp-proposed

# Update spec files to reflect new number
# Push renamed branches
```

**Pros**: Clean numbering, preserves both versions  
**Cons**: Work to renumber and update references

#### Option B: Use 001-yarp, Archive 002-gateway
Use the newer YARP-specific version as canonical (based on better documentation).

**Actions**:
```bash
# Archive old version
git branch -m CrossCuttingConcerns/002-api-gateway-spec CrossCuttingConcerns/ARCHIVED-002-api-gateway-spec
git branch -m CrossCuttingConcerns/002-api-gateway-proposed CrossCuttingConcerns/ARCHIVED-002-api-gateway-proposed

# Note: 001 still conflicts with identity service number
```

**Pros**: Uses newer/better specification  
**Cons**: Doesn't resolve numbering conflict with identity service

#### Option C: Compare and Merge
Review both specifications and decide which is superior or merge them.

**Actions**:
- Compare `Plan/CrossCuttingConcerns/specs/001-api-gateway-yarp/spec.md`
- Compare `Plan/CrossCuttingConcerns/specs/002-api-gateway/spec.md`
- Decide on canonical version
- Archive or renumber the other

**Pros**: Best solution based on content  
**Cons**: Requires detailed review

**Recommendation**: **Option C** followed by **Option A** - Compare both, then renumber YARP to 004 if keeping both, or archive 002 if YARP is superior.

---

### Issue #4: Incomplete Artifacts

**Problem**: Some specifications are missing planning documents:
- `01-identity-service-entra-id`: Missing plan.md, research.md, data-model.md
- `001-api-gateway-yarp`: Missing research.md, data-model.md

**Impact**:
- Harder to implement without complete planning
- Inconsistent specification quality

**Options**:

#### Option A: Generate Missing Artifacts
Run `/speckit.plan` again on incomplete specifications.

**Actions**:
```bash
# For each incomplete spec branch:
git checkout CrossCuttingConcerns/01-identity-service-entra-id-spec
/speckit.plan

git checkout CrossCuttingConcerns/001-api-gateway-yarp-spec
/speckit.plan
```

**Pros**: Complete specifications  
**Cons**: More work, may be unnecessary if archiving these versions

#### Option B: Accept Partial Specifications
Document that spec.md is sufficient for implementation.

**Pros**: No additional work  
**Cons**: Inconsistent standards

**Recommendation**: **Option A** if keeping those versions, **Option B** if archiving them (resolve Issues #2 and #3 first).

---

## Recommended Resolution Sequence

### Step 1: Fix Missing Spec Branch (High Priority)
**Issue**: #1 - 000-aspire-scaffolding missing spec branch  
**Action**: Create spec branch from proposed branch  
**Who**: DevOps or team member with git push access  
**Estimated Time**: 5 minutes

### Step 2: Resolve Identity Service Duplication (Medium Priority)
**Issue**: #2 - Duplicate identity service specifications  
**Action**: Keep 001-identity-service, archive 01-identity-service-entra-id  
**Who**: Product Owner or Lead Developer (needs decision)  
**Estimated Time**: 15 minutes (after decision)

### Step 3: Resolve API Gateway Version Conflict (Medium Priority)
**Issue**: #3 - API Gateway version conflict  
**Action**: Compare specifications, then either:
  - Renumber 001-yarp to 004-yarp and keep both, OR
  - Archive 002-gateway and use 001-yarp (renumber to 002 to fix conflict)  
**Who**: Product Owner or Lead Developer (needs decision + comparison)  
**Estimated Time**: 30-60 minutes (comparison + action)

### Step 4: Complete Missing Artifacts (Low Priority)
**Issue**: #4 - Incomplete specifications  
**Action**: Run `/speckit.plan` on incomplete specs (only if keeping those versions)  
**Who**: Development team  
**Estimated Time**: 10-15 minutes per specification

---

## Decision Matrix

To help guide decisions, here's a quick reference:

| Issue | Quick Fix | Best Fix | Effort |
|-------|-----------|----------|--------|
| Missing spec branch | Create from proposed | Create from proposed | Low |
| Duplicate identity | Archive 01-entra | Compare & choose best | Med-High |
| API Gateway conflict | Archive one version | Compare, renumber cleanly | High |
| Incomplete artifacts | Accept as-is | Generate missing files | Medium |

---

## Implementation Readiness After Resolution

Once issues are resolved, the specifications will be ready for:

### Phase 1: Task Generation
```bash
# For each specification branch:
git checkout CrossCuttingConcerns/[###]-feature-name-spec
/speckit.tasks
```

This generates `tasks.md` with phased breakdown of implementation work.

### Phase 2: Begin Implementation
```bash
# Creates implementation branch and begins development:
git checkout CrossCuttingConcerns/[###]-feature-name-spec
/speckit.implement
```

This creates branch `CrossCuttingConcerns/[###]-feature-name` for actual code development.

### Phase 3: Development
Follow Constitution v2.1.0:
- TDD Red→Green workflow
- Capture evidence at phase boundaries
- Maintain ≥80% test coverage
- Review at phase checkpoints

---

## Stakeholder Actions Required

### Product Owner / Tech Lead
- [ ] **Decision**: Choose canonical identity service version (Issue #2)
- [ ] **Decision**: Choose canonical API gateway version (Issue #3)
- [ ] **Decision**: Determine if incomplete artifacts are acceptable (Issue #4)
- [ ] Review all proposed branches and provide feedback

### DevOps / Git Administrator
- [ ] **Action**: Create missing spec branch for 000-aspire-scaffolding (Issue #1)
- [ ] **Action**: Execute branch renames/archives based on decisions above
- [ ] **Action**: Update branch protections if needed

### Development Team
- [ ] Review specifications on proposed branches
- [ ] Provide technical feedback on specifications
- [ ] After resolution, run `/speckit.tasks` on each spec branch
- [ ] Prepare for implementation phase

---

## Timeline Estimate

**Best Case** (quick decisions, straightforward fixes):
- Day 1: Decisions made, spec branch created
- Day 2: Branches renamed/archived, artifacts generated
- Day 3: Begin task generation and implementation planning

**Realistic Case** (some discussion needed):
- Week 1: Stakeholder review and decisions
- Week 2: Execute fixes, verify completeness
- Week 3: Begin implementation phase

---

## References

- **Status Report**: `Plan/CrossCuttingConcerns/scenarios/SPECIFICATION_STATUS_REPORT.md`
- **Constitution**: `.specify/memory/constitution.md` (v2.1.0)
- **Processing Reports**: 
  - `Plan/CrossCuttingConcerns/scenarios/PROCESSING_REPORT_2025-11-20T00-00-00Z.md`
  - `Plan/CrossCuttingConcerns/scenarios/PROCESSING_REPORT_2025-11-20T16-25-00Z.md`

---

## Questions?

For questions or clarifications about this action plan, please:
1. Review the detailed STATUS_REPORT.md
2. Check individual specification branches
3. Contact the development team lead

---

**Next Update**: After stakeholder decisions are made, this document will be updated with execution status.
