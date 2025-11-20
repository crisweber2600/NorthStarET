# Figma Prompt: District Admin Management (User Story 2)

**For contributors without the Figma MCP plugin**

## Design Frame References

- **Manage Admins Button**: [Manage Admins Button](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-33&m=dev)
- **Manage Admins Page**: [Manage Admins](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-181&m=dev)
- **First Name Field**: [First Name Input](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-285&m=dev)
- **Last Name Field**: [Last Name Input](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-282&m=dev)
- **Email Field**: [Email Input](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-288&m=dev)
- **Send Invitation Button**: [Send Invitation](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-314&m=dev)
- **Unverified Status Badge**: [Unverified Status](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-410&m=dev)
- **Verified Status Badge**: [Verified Status](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-403&m=dev)
- **Resend Invite Action**: [Resend Button](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=340-96&m=dev)
- **Remove Admin Action**: [Remove Button](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=340-99&m=dev)

## Manual Extraction Steps (if Figma MCP unavailable)

1. Open each design frame URL in Figma
2. Switch to **Dev Mode** (top-right toggle)
3. For each component, extract:
   - **Layout**: Spacing values (padding, margins, gaps)
   - **Typography**: Font family, size, weight, line-height, color
   - **Colors**: Background, text, border colors (use design tokens if available)
   - **Sizing**: Width, height, min/max constraints
   - **Interactive States**: Hover, focus, active, disabled styles
   - **Badge Variants**: Colors for Unverified, Verified, Revoked statuses

## Key Design Details to Capture

### Manage Admins Page (24-181)

- Page title showing district name/context
- Invitation form section
- Admin list/table layout
- Column headers: Name, Email, Status, Invited Date, Actions
- Empty state message (when no admins exist)

### Invitation Form

- Form field grouping and spacing
- Label positioning (above/beside inputs)
- Input field dimensions and styling
- Validation error placement
- Success feedback message styling
- Button arrangement (Clear, Send Invitation)

### Admin Status Badges

- **Unverified (24-410)**: Background color, text color, icon (if any)
- **Verified (24-403)**: Background color, text color, icon (if any)
- Badge dimensions, border radius, padding
- Font size and weight for badge text

### Action Buttons

- **Resend Invite (340-96)**: Icon, color scheme, sizing
- **Remove Admin (340-99)**: Icon, color scheme, sizing
- Hover/focus states
- Disabled state (e.g., when resend is on cooldown)

### Email Validation Feedback

- Inline error message styling for suffix mismatch
- Example text showing expected format (@[suffix].\*)
- Error icon and color palette

## Implementation Notes

Use these prompts when the Figma MCP plugin is unavailable. Once extracted, ensure:

- Status badge colors align with design system (consider accessibility - sufficient contrast)
- Form validation messages are clear and actionable
- Action buttons have appropriate confirmation dialogs (especially for Remove)

**Related Acceptance Criteria**: User Story 2 - System Admin Delegates District Admins (spec.md lines 60-73)

## Validation Rules to Display

Per FR-010, the email field should:

- Show helper text: "Email must match district suffix: @[suffix].\*"
- Validate on blur and on form submit
- Display clear error if suffix doesn't match
- Clear error when corrected

Per FR-012, the resend button should:

- Show countdown or disable for brief period after resend
- Display "Last sent: [time]" timestamp
- Show expiration date: "Expires: [date]"
