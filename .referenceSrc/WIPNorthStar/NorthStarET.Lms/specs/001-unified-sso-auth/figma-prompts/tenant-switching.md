# Figma Prompt: Tenant Switching

**Feature**: 001-unified-sso-auth  
**Component**: Tenant Context Switcher  
**Created**: 2025-10-20

## Design Request

Create Figma designs for the tenant switching component that allows users with multiple tenant memberships (e.g., district admin overseeing multiple schools) to switch their active organizational context.

### User Story Reference

**User Story 3** - Seamless Tenant Context Switching (Priority: P3)

A district administrator who oversees multiple schools can switch their active tenant context (from district-wide view to a specific school) instantly without re-authentication. The system updates the user interface to reflect the new tenant scope and adjusts available actions based on the new context.

---

## Component Overview

**Type**: Dropdown menu / selector component  
**Location**: Header/navigation area (always visible when authenticated)  
**Trigger**: Click on current tenant name or dedicated switcher icon

---

## States & Variations

### 1. Default State (Closed)

**Display**:
- Current tenant name: "Lincoln High School"
- Tenant type indicator: Icon or badge (District vs. School)
- Dropdown chevron icon (down arrow)

**Example**:
```
┌────────────────────────────────┐
│  🏫 Lincoln High School  ▼     │
└────────────────────────────────┘
```

**Interaction**:
- Click anywhere in component → Open dropdown (State 2)
- Hover → Highlight/change background color

---

### 2. Open State (Dropdown Expanded)

**Display**:
- Current tenant (highlighted/checked)
- List of other available tenants
- Tenant type icons/badges for each
- Search box (if user has many tenants, e.g., >10)
- Divider between tenant types (Districts, then Schools)

**Example**:
```
┌────────────────────────────────┐
│  Search tenants...             │
├────────────────────────────────┤
│  Districts                     │
├────────────────────────────────┤
│  ✓ Springfield School District │ ← Currently active
├────────────────────────────────┤
│  Schools                       │
├────────────────────────────────┤
│    Lincoln High School         │
│    Washington Middle School    │
│    Roosevelt Elementary        │
└────────────────────────────────┘
```

**Interaction**:
- Click tenant → Switch to that tenant (close dropdown, update UI)
- Click outside → Close dropdown (no change)
- Esc key → Close dropdown
- Type in search → Filter tenant list

---

### 3. Single Tenant (No Switching Available)

**Display**:
- Current tenant name: "Lincoln High School"
- **No dropdown chevron** (not clickable)
- Grayed out or different styling to indicate no switching available

**Example**:
```
┌────────────────────────────────┐
│  🏫 Lincoln High School        │ ← No chevron
└────────────────────────────────┘
```

**Notes**:
- Shown for users with only one tenant membership
- Still displays tenant context for clarity

---

### 4. Loading State (Switching in Progress)

**Display**:
- Dropdown closes
- Spinner/loading indicator appears
- Overlay or subtle animation
- Text: "Switching to [New Tenant Name]..."

**Example**:
```
┌────────────────────────────────┐
│  ⏳ Switching to Washington... │
└────────────────────────────────┘
```

**Duration**: Should be brief (<200ms per requirements)

---

### 5. Error State (Switch Failed)

**Display**:
- Error message: "Failed to switch tenant. Please try again."
- Option to retry
- Option to stay with current tenant

**Example**:
```
┌────────────────────────────────┐
│  ❌ Failed to switch tenant    │
│  [Retry] [Cancel]              │
└────────────────────────────────┘
```

---

## Visual Specifications

### Dropdown List Items

Each tenant item should show:
- **Icon**: Tenant type (District 🏛️ vs. School 🏫)
- **Name**: Tenant display name
- **Hierarchy Indicator**: If school, show parent district (e.g., "Lincoln HS • Springfield District")
- **Checkmark**: If currently active
- **Hover State**: Background color change

**Example Item**:
```
┌────────────────────────────────────────┐
│ ✓ 🏫 Lincoln High School               │
│      Springfield School District       │ ← Smaller, muted
└────────────────────────────────────────┘
```

### Search Box (for many tenants)

- Placeholder: "Search tenants..."
- Icon: Magnifying glass
- Clear button (X) when text entered
- Auto-focus when dropdown opens

---

## Responsive Behavior

### Desktop (1024px+)

- Dropdown width: 300px - 400px
- Position: Below trigger, aligned left or right
- Max height: 400px (scroll if more items)

### Tablet (768px - 1023px)

- Dropdown width: 280px - 350px
- Position: Below trigger or centered if space limited

### Mobile (< 768px)

- Full-width modal/sheet from bottom
- Larger touch targets (48px min height)
- Swipe down to close

---

## Accessibility Requirements

- **Keyboard Navigation**:
  - Tab to focus switcher
  - Enter/Space to open dropdown
  - Arrow keys to navigate tenants
  - Enter to select
  - Esc to close

- **ARIA Attributes**:
  - `role="combobox"`
  - `aria-expanded="true|false"`
  - `aria-haspopup="listbox"`
  - `aria-label="Select active tenant"`
  - Each tenant: `role="option"`, `aria-selected="true|false"`

- **Screen Reader**:
  - Announce current tenant on load
  - Announce selected tenant on switch
  - Announce loading state

---

## Integration with Page Context

When tenant is switched:
1. **Header**: Update displayed tenant name immediately
2. **Breadcrumbs**: Update if tenant is shown in breadcrumb trail
3. **Page Content**: Reload or update to show data for new tenant
4. **Notifications**: Optional toast/snackbar: "Switched to [Tenant Name]"
5. **Navigation**: Update available menu items based on new tenant permissions

---

## Edge Cases

### Many Tenants (50+)

- **Search is required** (show by default, not collapsed)
- **Virtualized scrolling** for performance
- **Group by type** (Districts, Schools) with collapsible sections

### Tenant with Long Names

- **Truncate with ellipsis** after ~40 characters
- **Tooltip on hover** showing full name
- Example: "Springfield School District Sou..."

### Slow Network

- **Show loading indicator** after 200ms
- **Timeout after 5 seconds** → Error state
- **Cached tenant list** to avoid delay on open

---

## Animation & Transitions

- **Dropdown Open**: Slide down + fade in (150ms)
- **Dropdown Close**: Slide up + fade out (150ms)
- **Tenant Switch**: Subtle fade or slide transition for page content (200ms)
- **Loading Spinner**: Rotate animation

---

## Success Criteria

Designs should show:
1. All 5 states listed above (default, open, single tenant, loading, error)
2. Dropdown with 3-5 example tenants (mix of districts and schools)
3. Search box variation (for many tenants)
4. Responsive layouts (desktop + mobile)
5. Interactive prototype showing:
   - Open dropdown
   - Select new tenant
   - Loading state
   - Page refresh with new tenant context

---

## Deliverables

1. **Figma Component**: Reusable tenant switcher component
2. **States**: All 5 states as component variants
3. **Prototype**: Interactive flow for switching tenant
4. **Responsive Versions**: Desktop, tablet, mobile
5. **Specs**: Measurements, colors, typography for developers

---

## Questions for Designer

1. Should we show user's role in each tenant (e.g., "District Admin" badge)?
2. Preferred position for switcher in header (left, center, right)?
3. Should we show count of available tenants (e.g., "3 tenants")?
4. Any specific icons for tenant types or use emoji placeholders?

---

## Next Steps After Design

1. Update this prompt with Figma link
2. Implement as Razor component (`TenantSwitcher.razor`)
3. Integrate with Application layer `SwitchTenantContextCommand`
4. Write Playwright test for tenant switching flow
5. Test with users who have 1, 5, and 20+ tenants

---

**Status**: ⚠️ AWAITING FIGMA DESIGNS

*Implementation of tenant switcher component is blocked until this design is available.*
