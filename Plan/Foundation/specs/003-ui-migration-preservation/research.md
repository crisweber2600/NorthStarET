# Research: UI Migration Preservation
Layer: Foundation
Version: 0.1.0

## AngularJS → Angular 18
- Direct migration path supported; incremental strategy reduces risk.
- Maintain legacy CSS classes to minimize styling drift.

## Visual Regression
- Playwright + pixelmatch (built into toMatchSnapshot) for diffing.
- Stabilization: fixed viewport, mock dates, deterministic data.

## Micro-Frontend Pattern
| Option | Pros | Cons |
|--------|------|------|
| Iframe bridge | Isolation | Harder deep linking, resizing quirks |
| Web component wrapper | Better integration | Build complexity |
| Hybrid router (Chosen) | Seamless navigation | Requires adapter code |

## Accessibility
- Maintain ARIA attributes; test with axe.
- Keyboard shortcuts preserved via centralized mapping service.

## Tooling
- ESLint + Prettier to enforce consistency.
- CI: visual + functional + accessibility gates.

## Open Questions
1. Duration of coexistence window? (Define cutoff date.)
2. Strategy for legacy preference migration (one-time import vs on-demand)?
3. Handling of custom user scripts (if any) in legacy environment.

---
Manual research artifact.