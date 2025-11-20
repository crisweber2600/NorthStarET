# UI Migration with Preservation Strategy

**Feature**: Migrate AngularJS UI to Modern Framework While Preserving Existing Layouts  
**Epic**: Phase 4 - UI Migration (Weeks 20-28)  
**Business Value**: Modernize technology stack without disrupting user workflows

---

## Scenario 1: Student Dashboard Screen Migration

**Given** the OldNorthStar has a student dashboard in AngularJS  
**And** the dashboard displays student info, grades, attendance, and assessments  
**When** the UI migration team analyzes the existing dashboard  
**Then** they document the exact layout, components, and data bindings  
**And** they create a new Angular 18 component matching the layout  
**And** they implement the same data service calls to the new API  
**And** they verify visual parity using screenshot comparison  
**And** the migrated dashboard maintains identical user workflows

---

## Scenario 2: Teacher Viewing Student List (Before and After Migration)

**Given** a teacher is viewing the student list in the legacy AngularJS UI  
**And** the list shows columns: Name, Grade, Enrollment Date, Status  
**And** the list supports sorting, filtering, and pagination  
**When** the same teacher views the migrated Angular 18 student list  
**Then** the layout is identical to the legacy version  
**And** all columns are in the same order  
**And** sorting, filtering, and pagination work identically  
**And** the visual styling matches the legacy UI  
**And** no training is required for the transition

---

## Scenario 3: Assessment Entry Form Migration

**Given** the legacy assessment entry form has specific validation rules  
**And** the form has fields: Student, Assessment Type, Score, Date, Notes  
**And** the form validates score ranges and required fields  
**When** the form is migrated to the modern framework  
**Then** all field layouts remain in the same positions  
**And** validation rules are preserved exactly  
**And** error messages are identical to legacy  
**And** the tab order and keyboard navigation match legacy behavior  
**And** the form submission flow is unchanged

---

## Scenario 4: Incremental Component Migration

**Given** the entire AngularJS application has 150 components  
**And** the migration is performed incrementally  
**When** the student management module is migrated first  
**Then** migrated components are deployed to production  
**And** unmigrated components remain in AngularJS  
**And** both frameworks coexist using micro-frontend pattern  
**And** users navigate seamlessly between old and new components  
**And** the transition is transparent to end users

---

## Scenario 5: Visual Regression Testing During Migration

**Given** a screen has been migrated from AngularJS to Angular 18  
**When** the QA team runs visual regression tests  
**Then** Playwright captures screenshots of both versions  
**And** the screenshots are compared pixel-by-pixel  
**And** differences are highlighted in the test report  
**And** any layout deviations are flagged as test failures  
**And** the team fixes any visual discrepancies before deployment

---

## Scenario 6: API Integration Without Backend Changes

**Given** the legacy AngularJS app calls `NS4.WebAPI` endpoints  
**And** the endpoints return data in specific JSON formats  
**When** the new Angular 18 app is developed  
**Then** it calls the same API endpoints through the YARP gateway  
**And** it consumes the same JSON response formats  
**And** no backend API changes are required for UI migration  
**And** the new UI works with both legacy and new microservices  
**And** the API Gateway handles routing transparently

---

## Scenario 7: User Settings and Preferences Preservation

**Given** a teacher has customized their dashboard preferences in the legacy UI  
**And** preferences include: column visibility, default filters, sort order  
**When** they access the migrated UI  
**Then** their preferences are loaded from the same backend service  
**And** the new UI applies preferences identically  
**And** the settings panel has the same layout as before  
**And** no preference data is lost during migration

---

## Scenario 8: Responsive Design Maintenance

**Given** the legacy AngularJS UI has responsive breakpoints  
**And** the UI adapts for desktop (1920px), tablet (768px), and mobile (375px)  
**When** the screens are migrated to Angular 18  
**Then** the same responsive breakpoints are maintained  
**And** the layouts adapt identically at each breakpoint  
**And** no new mobile views are introduced  
**And** the existing responsive behavior is preserved  
**And** touch interactions work as before on tablets

---

## Scenario 9: Navigation and Menu Structure Preservation

**Given** the legacy UI has a left sidebar navigation menu  
**And** the menu has: Dashboard, Students, Staff, Assessments, Reports, Settings  
**And** each menu item expands to show sub-menus  
**When** the navigation is migrated  
**Then** the menu structure remains identical  
**And** menu item order is unchanged  
**And** icons and labels are preserved  
**And** expand/collapse behavior matches legacy  
**And** active menu highlighting works the same way

---

## Scenario 10: Search and Filter Functionality Preservation

**Given** the student search page has filters: Grade, Status, Enrollment Year  
**And** the search supports: name search, wildcard matching, clear all filters  
**When** the search page is migrated  
**Then** all filter options are available in same positions  
**And** search behavior is identical (same matching algorithm)  
**And** wildcard characters work the same way  
**And** clear all button resets filters identically  
**And** search results display in the same format

---

## Scenario 11: Print and Export Features Preservation

**Given** teachers can print student rosters from the legacy UI  
**And** the print view has a specific layout with school logo  
**When** the roster page is migrated  
**Then** the print button remains in the same location  
**And** the print preview shows identical layout  
**And** the exported PDF matches legacy format  
**And** all print-specific CSS is preserved  
**And** page breaks occur in the same positions

---

## Scenario 12: Accessibility Features Maintained

**Given** the legacy UI has keyboard shortcuts: Ctrl+S (save), Ctrl+P (print), etc.  
**And** the UI supports screen reader navigation  
**When** the UI is migrated  
**Then** all keyboard shortcuts continue to work  
**And** screen reader compatibility is maintained or improved  
**And** ARIA labels are preserved  
**And** tab order follows the same sequence  
**And** focus management works identically

---

## Technical Implementation Notes

**Migration Strategy**:
1. **Analyze Legacy Components**: Document all AngularJS components, services, directives
2. **Screenshot Baseline**: Capture screenshots of all screens in multiple states
3. **Component Mapping**: Map AngularJS components to Angular 18 equivalents
4. **Service Layer**: Preserve API contracts, create new services with same interfaces
5. **Style Preservation**: Copy CSS/SCSS from legacy, maintain class names
6. **Incremental Rollout**: Migrate module by module, maintain both frameworks
7. **Visual Testing**: Automated screenshot comparison in CI/CD pipeline
8. **User Acceptance**: Get feedback from actual users before broader rollout

**Technology Options**:

**Option A: Angular 18 (Incremental)**
```typescript
// Preserve legacy service interface
@Injectable()
export class StudentService {
  // Same method signatures as AngularJS service
  getStudents(filters: StudentFilters): Observable<Student[]> {
    // Call same API endpoints
    return this.http.get<Student[]>('/api/v1/students', {params: filters});
  }
}
```

**Option B: Blazor Web App**
```csharp
@page "/students"
@inject StudentService StudentService

<!-- Preserve exact HTML structure -->
<div class="student-list-container">
  <div class="filters-panel">
    <!-- Same filter layout as AngularJS -->
  </div>
  <div class="student-grid">
    <!-- Same grid layout as AngularJS -->
  </div>
</div>
```

**Visual Regression Test Example**:
```typescript
test('Student Dashboard Visual Regression', async ({ page }) => {
  await page.goto('/students/dashboard');
  await page.waitForLoadState('networkidle');
  
  // Capture screenshot
  const screenshot = await page.screenshot();
  
  // Compare with baseline (legacy UI)
  expect(screenshot).toMatchSnapshot('student-dashboard.png', {
    threshold: 0.01, // Allow 1% pixel difference
  });
});
```

**CSS Preservation Strategy**:
```scss
// Copy legacy class names exactly
.student-list-container {
  // Preserve exact spacing, colors, fonts
  padding: 20px;
  background: #f5f5f5;
  font-family: 'Segoe UI', sans-serif;
}

.student-row {
  // Maintain exact row height and borders
  height: 48px;
  border-bottom: 1px solid #e0e0e0;
}
```

**Coexistence Pattern (Micro-Frontend)**:
```html
<!-- Root App Shell (Angular 18) -->
<div id="app-container">
  <!-- New Angular 18 Components -->
  <router-outlet *ngIf="useNewComponents"></router-outlet>
  
  <!-- Legacy AngularJS iframe for unmigrated components -->
  <iframe 
    *ngIf="!useNewComponents" 
    src="/legacy/students"
    seamless>
  </iframe>
</div>
```

**Migration Phases**:
1. **Phase 1**: Student Management module (Weeks 20-22)
2. **Phase 2**: Assessment module (Weeks 22-24)
3. **Phase 3**: Staff & Intervention modules (Weeks 24-26)
4. **Phase 4**: Reporting & Settings (Weeks 26-28)
5. **Cutover**: Decommission AngularJS completely (Week 28)

**No Figma Required Because**:
- Existing UI layouts are well-documented
- Visual regression tests ensure pixel-perfect preservation
- User training overhead is minimized
- Business process workflows remain unchanged
- Focus is on technology upgrade, not redesign

**Quality Gates**:
- ✅ Visual regression tests passing (99% similarity)
- ✅ All functional tests passing
- ✅ User acceptance testing with actual teachers
- ✅ Performance metrics match or exceed legacy
- ✅ Accessibility compliance maintained (WCAG 2.1 AA)
- ✅ Cross-browser testing (Chrome, Edge, Firefox, Safari)

**Success Criteria**:
- Users cannot distinguish between legacy and migrated screens
- No increase in support tickets during migration
- No negative user feedback about UI changes
- All workflows complete in same or fewer steps
- Performance equal to or better than legacy
