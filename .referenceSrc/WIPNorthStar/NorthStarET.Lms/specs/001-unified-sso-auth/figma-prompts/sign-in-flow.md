# Figma Prompt: Sign-In Flow

**Feature**: 001-unified-sso-auth  
**Component**: User Authentication Flow  
**Created**: 2025-10-20

## Design Request

Create Figma designs for the complete sign-in flow from initial landing to authenticated dashboard.

### User Story Reference

**User Story 1** - Single Sign-On Access Across All Portals (Priority: P1)

Any user (admin, teacher, parent, or student) navigates to any LMS or admin UI and is automatically redirected to Microsoft Entra for authentication. After successful sign-in, the user receives a session that grants access to all authorized portals and services without requiring additional login prompts.

---

## Screens Needed

### 1. Initial Landing / Unauthenticated State

**Purpose**: First screen users see when navigating to LMS without a session

**Elements**:
- Application logo and branding
- "Sign In" button (primary CTA)
- Brief welcome message
- Footer with support links

**Interaction**:
- Click "Sign In" → Redirect to Entra ID (screen will navigate away)

**Notes**:
- This is a very brief screen (users typically see <1 second before redirect)
- Keep it simple and branded
- No username/password fields (handled by Entra)

---

### 2. Entra ID Sign-In Page (Reference Only)

**Purpose**: Microsoft-hosted authentication page (not designed by us)

**What Users See**:
- Microsoft Entra branding
- Email/username field
- Password field
- "Sign In" button
- "Forgot password" link
- MFA prompts (if configured)

**Notes**:
- We don't design this page; it's Microsoft's
- Include in flow diagram for completeness
- Can customize branding via Entra tenant settings (Microsoft's docs)

---

### 3. Post-Authentication Landing / Dashboard

**Purpose**: First screen users see after successful Entra authentication

**Elements**:
- **Header/Navigation**:
  - User name and avatar/initials
  - Active tenant name and type (e.g., "Lincoln High School - School")
  - Tenant switcher dropdown (if user has multiple tenants)
  - Sign out button
  
- **Main Content**:
  - Welcome message: "Welcome back, [First Name]!"
  - Role indicator: "You are signed in as: [Role Name]"
  - Quick access cards/tiles for common actions (role-specific)
  - Recent activity or notifications

- **Footer**:
  - Support links
  - Privacy policy

**Interaction**:
- Click tenant switcher → Show dropdown with available tenants (see `tenant-switching.md`)
- Click user avatar/name → User profile menu
- Click sign out → Confirmation modal → Redirect to sign-in

**Role-Specific Variations**:

**System Admin**:
- Quick access: Manage Districts, Manage Users, View Reports, System Settings

**District Admin**:
- Quick access: Manage Schools, View District Reports, Manage Educators, Settings

**School Admin**:
- Quick access: Manage Classes, View School Reports, Manage Teachers, Settings

**Teacher**:
- Quick access: My Classes, Grade Book, Assignments, Students

**Parent**:
- Quick access: My Children, Grades, Assignments, Messages

---

### 4. Session Active Indicator

**Purpose**: Persistent UI element showing user is authenticated

**Elements**:
- Small badge or indicator in header
- Session expiration countdown (optional, for transparency)
- "Your session expires in X minutes" (optional)

**Interaction**:
- Auto-refresh session on user activity (no user action needed)
- Show subtle notification if session renewed

**Notes**:
- Should be non-intrusive
- Reassures users their session is active

---

## User Flow Diagram

```
┌─────────────────────────────────┐
│  User navigates to LMS URL      │
└───────────┬─────────────────────┘
            │
            v
┌─────────────────────────────────┐
│  Has active session?            │
└───────────┬─────────────────────┘
            │
     ┌──────┴──────┐
     │             │
    Yes           No
     │             │
     │             v
     │   ┌─────────────────────────┐
     │   │ Initial Landing Page    │
     │   │ (Show "Sign In" button) │
     │   └───────────┬─────────────┘
     │               │
     │               v
     │   ┌─────────────────────────┐
     │   │ Redirect to Entra ID    │
     │   └───────────┬─────────────┘
     │               │
     │               v
     │   ┌─────────────────────────┐
     │   │ User authenticates      │
     │   │ (Entra sign-in page)    │
     │   └───────────┬─────────────┘
     │               │
     │               v
     │   ┌─────────────────────────┐
     │   │ Entra redirects back    │
     │   │ with auth token         │
     │   └───────────┬─────────────┘
     │               │
     └───────────────┘
                     │
                     v
       ┌─────────────────────────┐
       │ Dashboard / Home Page   │
       │ (Personalized by role)  │
       └─────────────────────────┘
```

---

## Accessibility Requirements

- **Keyboard Navigation**: All interactive elements must be keyboard-accessible (Tab, Enter, Esc)
- **Screen Reader Support**: All icons must have `aria-label` attributes
- **Color Contrast**: WCAG AA minimum (4.5:1 for normal text, 3:1 for large text)
- **Focus Indicators**: Clear visual focus states for all interactive elements
- **Responsive**: Mobile-first design, works on 320px - 1920px+ screens

---

## Responsive Breakpoints

- **Mobile**: 320px - 767px (single column, hamburger menu)
- **Tablet**: 768px - 1023px (adapt layout, possibly stacked)
- **Desktop**: 1024px+ (full layout with sidebar navigation)

---

## Branding & Style Notes

- Use NorthStarET brand colors (primary, secondary, accent)
- Typography: [Specify font family, e.g., Inter, Roboto]
- Spacing: Follow 8px grid system
- Border radius: Consistent across buttons, cards (e.g., 4px or 8px)
- Shadows: Subtle elevation for cards (e.g., `0 2px 4px rgba(0,0,0,0.1)`)

---

## Success Criteria

When Figma designs are complete, they should:
1. Show all 4 screens listed above
2. Include role-specific dashboard variations (at least 2-3 examples)
3. Demonstrate responsive layouts (mobile + desktop)
4. Include interactive prototype showing flow from landing → sign-in → dashboard
5. Provide component library for reusable elements (buttons, headers, cards)

---

## Deliverables

1. **Figma Link**: Share link to Figma file with view access
2. **Prototype Link**: Share interactive prototype link
3. **Component Library**: Exportable design system components
4. **Specs**: Design specs for developers (spacing, colors, typography)

---

## Questions for Designer

1. Do we have existing brand guidelines or should this establish them?
2. Are there preferred Microsoft Entra branding guidelines we should follow for consistency?
3. Should we include dark mode designs?
4. Any specific accessibility certifications we're targeting (e.g., WCAG AAA)?

---

## Next Steps After Design

Once Figma designs are ready:
1. Update this prompt with the Figma link
2. Tag UI implementation tasks with the specific Figma frames
3. Validate designs with stakeholders (product, UX, accessibility)
4. Implement in Razor components and MVC views
5. Run Playwright tests against implemented UI to verify flow

---

**Status**: ⚠️ AWAITING FIGMA DESIGNS

*Implementation of sign-in UI is blocked until this design is available. Meanwhile, backend authentication logic and API contracts can proceed.*
