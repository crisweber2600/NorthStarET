# Plan: UI Migration with Preservation Strategy
Version: 0.1.0
Status: Draft (Planning)
Layer: Foundation
Spec Ref: 003-ui-migration-preservation/spec.md

## Objectives
- Replace AngularJS with Angular 18 incrementally without changing UX.
- Guarantee pixel + workflow parity (99%+ similarity threshold).
- Maintain existing API contracts (no backend change dependency).
- Preserve user preferences, accessibility, print/export behavior.

## Architecture Overview
1. Host Shell (Angular 18)
   - Provides routing, layout frame, shared services.
   - Feature flag decides legacy vs migrated component entry.
2. Legacy Bridge
   - Iframe or Web Component wrapper for unmigrated AngularJS modules.
   - Shared auth/session propagated via postMessage or query token.
3. Data Services
   - Angular 18 services replicate legacy service interfaces (method names & signatures).
   - Reuse existing endpoint paths via YARP.
4. Visual Regression Suite
   - Baseline images stored under `tests/ui/baseline/`.
   - Playwright compare: threshold 0.01 (1%).
5. Accessibility & Keyboard Parity
   - Automated axe scans.
   - Shortcut mapping service ensures key bindings identical.

## Component Migration Workflow
1. Capture baseline screenshots (desktop/tablet/mobile states).
2. Extract legacy HTML + CSS class names.
3. Scaffold Angular 18 component with identical structure.
4. Implement data service calls & reactive state.
5. Run functional + visual regression tests.
6. Tag component as migrated; toggle feature flag.

## Data & Preferences
- Preferences stored server-side remain unchanged.
- Mapping: AngularJS local storage keys consumed by new layer on first load.

## Key Interfaces
```typescript
export interface StudentService {
  getStudents(filters: StudentFilters): Observable<Student[]>;
  getStudent(id: string): Observable<Student>;
}
```

## Sample Visual Regression Test
```typescript
test('Student List Parity', async ({ page }) => {
  await page.goto('/students');
  await page.waitForLoadState('networkidle');
  expect(await page.screenshot()).toMatchSnapshot('students-list.png', { threshold: 0.01 });
});
```

## Micro-Frontend Decision
- Angular 18 chosen for direct migration path.
- Blazor deferred (future enhancement / experiments only).

## Performance Considerations
- Remove unused legacy scripts after module migration.
- Lazy-load migrated modules.
- Shared vendor bundle size tracking in CI.

## Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| Mixed routing states | User confusion | Unified router guard + fallback |
| Screenshot instability | False failures | Deterministic test data + viewport lock |
| CSS cascade conflicts | Styling drift | Scoped styles + audit legacy global classes |

## Test Matrix
- Functional parity tests (filters, sorting, pagination, shortcuts).
- Visual regression (desktop, tablet, mobile).
- Accessibility (axe + keyboard traversal script).
- Performance (First Contentful Paint, bundle size regression).

## Completion Criteria
- All spec scenarios green.
- No visual diffs >1%.
- Accessibility equal or improved (no new violations).
- Support tickets unaffected (monitor 2 weeks post-cutover).

---
Draft plan (manual).