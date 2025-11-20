# Research: Configuration Service
Layer: Foundation
Version: 0.1.0

## Hierarchical Config Patterns
- Common approach: layered lookup chain (site → org → global).
- Precompute flattened view for performance; store version for invalidation.

## Redis Usage
- TTL 1 hour; may adopt sliding refresh.
- Consider storing dependency graph for selective invalidation.

## State-Specific Compliance
- Feature flags per state (CA, TX) controlling required fields.
- Extend via JSON schema definitions for validation future phase.

## Custom Attributes
- Avoid dynamic schema migration; store attributes as key/value set in JSONB or separate table referencing entity.

## Open Questions
1. Should grading scale share a common canonical representation for conversions? – Proposed yes.
2. Do templates need multi-language early? – Not initial; backlog.
3. Need partial update vs full replace semantics? – Full replace except small patch endpoint.

---
Manual research artifact.