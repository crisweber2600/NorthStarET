# Figma Prompt: Session Expiration

**Feature**: 001-unified-sso-auth  
**Component**: Session Expiration Modal & Notifications  
**Created**: 2025-10-20

## Design Request

Create Figma designs for the session expiration flow, including warning notifications and the final "Session expired" modal that prompts users to re-authenticate.

### User Story Reference

**User Story 4** - Graceful Session Expiration and Renewal (Priority: P2)

When a user's session token expires (due to time limits or inactivity), the system detects the expiration and presents a single, unified "Session expired" prompt. The user is redirected to Entra for re-authentication, and upon successful re-authentication, they are returned to their original location with their work context preserved.

---

## Component Overview

**Components Needed**:
1. **Warning Notification** (5 minutes before expiration)
2. **Session Expired Modal** (when session actually expires)
3. **Auto-Refresh Success Indicator** (optional, for transparency)

---

## 1. Warning Notification (5 Minutes Before Expiration)

### Purpose
Warn active users that their session is about to expire, giving them a chance to extend it.

### Display

**Type**: Toast notification / banner at top or bottom of page

**Content**:
- Icon: ⏰ (clock or warning icon)
- Message: "Your session will expire in 5 minutes."
- Actions:
  - **Primary Button**: "Stay Signed In" (extends session)
  - **Secondary Button**: "Sign Out Now" (optional)

**Example**:
```
┌────────────────────────────────────────────────┐
│ ⏰ Your session will expire in 5 minutes.      │
│                                                │
│  [Stay Signed In]  [Sign Out Now]             │
└────────────────────────────────────────────────┘
```

### Behavior

- **Appears**: 5 minutes before session expiration
- **Position**: Top center or bottom right (non-blocking)
- **Auto-dismiss**: After 30 seconds if user doesn't interact (but session still auto-extends on activity)
- **Click "Stay Signed In"**: Refresh session, dismiss notification
- **Click "Sign Out Now"**: Sign user out immediately, redirect to sign-in page
- **Ignore notification**: Session auto-extends if user is active; expires if inactive

### Variations

**Desktop**:
- Toast in top-right or bottom-right corner
- 400px wide
- Subtle drop shadow

**Mobile**:
- Banner at top of screen (pushes content down slightly)
- Full width
- Slide down animation

---

## 2. Session Expired Modal

### Purpose
Inform user that their session has expired and they must re-authenticate to continue.

### Display

**Type**: Modal dialog (blocks interaction with page)

**Content**:
- Icon: 🔒 (lock icon)
- Heading: "Your session has expired"
- Message: "For your security, you've been signed out due to inactivity. Please sign in again to continue."
- **Primary Button**: "Sign In Again" (redirects to Entra)
- **Secondary Link**: "Return to Home" (optional)

**Example**:
```
┌────────────────────────────────────────────────┐
│                                                │
│              🔒                                │
│                                                │
│        Your session has expired                │
│                                                │
│  For your security, you've been signed out     │
│  due to inactivity. Please sign in again       │
│  to continue.                                  │
│                                                │
│  [Sign In Again]   [Return to Home]            │
│                                                │
└────────────────────────────────────────────────┘
```

### Behavior

- **Appears**: When session expires (timeout or manual revocation)
- **Blocks interaction**: User must click button (cannot dismiss or click outside)
- **No close button (X)**: User must take action
- **Click "Sign In Again"**: Redirect to Entra sign-in, preserve return URL
- **Click "Return to Home"**: Redirect to public home page (if applicable)
- **On re-authentication**: Return user to the page they were on before expiration

### Variations

**Timeout Expiration** (most common):
- Message focuses on inactivity
- Tone: Reassuring (security feature)

**Manual Revocation** (admin signed user out):
- Message: "Your session was ended by an administrator. Please sign in again."
- Tone: Neutral (not user's fault)

**Security Policy** (e.g., password change required):
- Message: "Your session was ended for security reasons. Please sign in again."
- Additional info: Link to security policy or help article

---

## 3. Auto-Refresh Success Indicator (Optional)

### Purpose
Subtly indicate that the session was automatically refreshed (transparency for power users).

### Display

**Type**: Subtle toast or icon animation in header

**Content**:
- Icon: ✓ (checkmark or refresh icon)
- Message: "Session refreshed"

**Example**:
```
┌─────────────────────────┐
│ ✓ Session refreshed     │
└─────────────────────────┘
```

### Behavior

- **Appears**: When background refresh succeeds (every ~25 minutes)
- **Duration**: 2 seconds, then auto-dismiss
- **Style**: Very subtle, low visual weight (don't distract user)
- **Optional**: Can be disabled or hidden for most users

**Note**: This is a "nice to have" for transparency but not required.

---

## User Flow Diagram

### Happy Path (Active User)

```
┌──────────────────────────┐
│ User is active           │
│ (clicking, typing)       │
└────────┬─────────────────┘
         │
         v
┌──────────────────────────┐
│ Session auto-refreshes   │
│ every ~25 minutes        │
└────────┬─────────────────┘
         │
         v
┌──────────────────────────┐
│ Optional: Show "Session  │
│ refreshed" indicator     │
└──────────────────────────┘
```

### Warning Path (Inactive User)

```
┌──────────────────────────┐
│ User inactive for 25 min │
└────────┬─────────────────┘
         │
         v
┌──────────────────────────┐
│ Show warning: "Session   │
│ will expire in 5 min"    │
└────────┬─────────────────┘
         │
    ┌────┴────┐
    │         │
   User      User
   acts    ignores
    │         │
    v         v
┌─────────┐ ┌──────────────────────────┐
│ Session │ │ Session expires          │
│ extends │ │ Show "Session expired"   │
│ Warning │ │ modal                    │
│ dismiss │ └────────┬─────────────────┘
└─────────┘          │
                     v
          ┌──────────────────────────┐
          │ User clicks "Sign In     │
          │ Again"                   │
          └────────┬─────────────────┘
                   │
                   v
          ┌──────────────────────────┐
          │ Redirect to Entra        │
          │ Preserve return URL      │
          └────────┬─────────────────┘
                   │
                   v
          ┌──────────────────────────┐
          │ User re-authenticates    │
          └────────┬─────────────────┘
                   │
                   v
          ┌──────────────────────────┐
          │ Return to original page  │
          │ with new session         │
          └──────────────────────────┘
```

---

## Visual Specifications

### Warning Notification

- **Width**: 400px (desktop), 100% (mobile)
- **Background**: Warning color (e.g., amber/yellow tint)
- **Border**: Optional subtle border or shadow
- **Icon**: 24px × 24px
- **Text**: Body text size (14px - 16px)
- **Buttons**: Standard button styles, 16px padding
- **Animation**: Slide in from top or bottom

### Session Expired Modal

- **Overlay**: Semi-transparent dark background (rgba(0,0,0,0.5))
- **Modal Width**: 480px (desktop), 90% max-width (mobile)
- **Padding**: 32px - 48px
- **Icon**: 64px × 64px, centered
- **Heading**: H2 size (24px - 28px), bold, centered
- **Body Text**: 16px - 18px, centered
- **Buttons**: Full-width on mobile, inline on desktop
- **Border Radius**: Match app style (e.g., 8px)
- **Shadow**: Elevated (e.g., `0 8px 16px rgba(0,0,0,0.2)`)

---

## Accessibility Requirements

### Warning Notification

- **ARIA Live Region**: `aria-live="polite"` (announce to screen readers)
- **Role**: `role="alert"`
- **Keyboard**: Tab to buttons, Enter to activate, Esc to dismiss (if allowed)
- **Focus**: Auto-focus "Stay Signed In" button

### Session Expired Modal

- **ARIA Modal**: `role="dialog"`, `aria-modal="true"`
- **Focus Trap**: User cannot tab outside modal
- **Initial Focus**: "Sign In Again" button
- **Esc Key**: Disabled (user must choose action)
- **Screen Reader**: Announce heading and message on open

---

## Edge Cases

### Multiple Tabs/Windows

**Problem**: User has LMS open in 3 tabs. Session expires.

**Solution**:
- All tabs detect expiration (via shared storage event)
- All tabs show "Session expired" modal simultaneously
- When user re-authenticates in one tab, other tabs detect new session and auto-refresh

**Design Consideration**: Modal should not confuse user if they see it in multiple tabs.

### Mid-Action Expiration

**Problem**: User is filling out a long form. Session expires before submission.

**Solution**:
- Auto-save form data to local storage (if applicable)
- After re-authentication, restore form data
- Show message: "Your session expired, but we saved your work."

**Design Consideration**: Reassure user they won't lose work.

### Network Disconnect

**Problem**: User's network disconnects. Session might expire but they don't see modal.

**Solution**:
- Detect offline state (via `navigator.onLine`)
- Show "You're offline" message instead of session expiration
- When back online, check session and show expiration modal if needed

**Design Consideration**: Different message for offline vs. expired.

---

## Success Criteria

Designs should show:
1. Warning notification (desktop + mobile)
2. Session expired modal (all 3 variations: timeout, revocation, security)
3. Auto-refresh indicator (optional)
4. Flow diagram showing warning → expired → re-auth → return
5. Interactive prototype demonstrating:
   - Warning appears
   - User clicks "Stay Signed In" → Notification dismisses
   - Warning ignored → Modal appears
   - User clicks "Sign In Again" → Redirect flow

---

## Deliverables

1. **Figma Components**: Warning notification, session expired modal
2. **Variants**: Different expiration reasons (timeout, revocation, security)
3. **Responsive**: Desktop and mobile layouts
4. **Prototype**: Interactive flow for expiration and re-authentication
5. **Specs**: Measurements, colors, typography, animations

---

## Questions for Designer

1. Should warning notification be dismissible or auto-dismiss only?
2. Preferred position for warning (top vs. bottom)?
3. Should we show countdown timer in warning (e.g., "4:32 remaining")?
4. Any branding requirements for modal (logo, colors)?
5. Should "Return to Home" link be included or only "Sign In Again"?

---

## Next Steps After Design

1. Update this prompt with Figma link
2. Implement warning notification component (Razor)
3. Implement session expired modal component (Razor)
4. Integrate with session management service (JavaScript timer + C# backend)
5. Write Playwright test for expiration flow (force timeout, verify modal, re-auth)

---

**Status**: ⚠️ AWAITING FIGMA DESIGNS

*Implementation of session expiration UI is blocked until this design is available. Backend session expiration logic can proceed independently.*
