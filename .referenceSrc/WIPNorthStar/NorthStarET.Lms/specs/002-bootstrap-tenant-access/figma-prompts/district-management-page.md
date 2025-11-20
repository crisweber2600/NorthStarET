# Figma Prompt: District Management Page (User Story 1)

**For contributors without the Figma MCP plugin**

## Design Frame References

- **Main Page**: [District Management](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-2&m=dev)
- **Create Button**: [Create District Button](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-43&m=dev)
- **Create Modal**: [Create New District Modal](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-67&m=dev)
- **District Name Field**: [District Name Input](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-67&m=dev)
- **District Suffix Field**: [District Suffix Input](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-76&m=dev)
- **Create Action Button**: [Create District Submit](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-69&m=dev)
- **District List Row**: [District Row](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-31&m=dev)
- **Edit Icon**: [Edit Button](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-31&m=dev)
- **Edit Modal**: [Edit District Modal](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-278&m=dev)
- **Edit Name Field**: [Edit District Name](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-291&m=dev)
- **Edit Suffix Field**: [Edit District Suffix](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-287&m=dev)
- **Update Action Button**: [Update District Submit](https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-280&m=dev)

## Manual Extraction Steps (if Figma MCP unavailable)

1. Open each design frame URL in Figma
2. Switch to **Dev Mode** (top-right toggle)
3. For each component, extract:
   - **Layout**: Spacing values (padding, margins, gaps)
   - **Typography**: Font family, size, weight, line-height, color
   - **Colors**: Background, text, border colors (use design tokens if available)
   - **Sizing**: Width, height, min/max constraints
   - **Interactive States**: Hover, focus, active, disabled styles
   - **Responsive Behavior**: Breakpoints and layout shifts

## Key Design Details to Capture

### District Management Page (22-2)

- Page title and header styling
- Table/list layout for districts
- Column headers: Name, Suffix, Admin Count, Actions
- Pagination controls (if present)
- Empty state message (when no districts exist)

### Create District Modal (22-67)

- Modal dimensions and positioning
- Header with close button
- Form field labels and input styling
- Validation error message styling
- Button arrangement (Cancel, Create)
- Focus trap behavior

### Edit District Modal (22-278)

- Same structure as Create modal
- Pre-filled form values styling
- Update vs Create button differences

### District List Row (22-31)

- Row height and padding
- Column alignment
- Edit icon size and color
- Delete icon (if present)
- Manage Admins button styling
- Hover state styling

## Implementation Notes

Use these prompts when the Figma MCP plugin is unavailable. Once extracted, document design tokens in a shared stylesheet to maintain consistency across the application.

**Related Acceptance Criteria**: User Story 1 - System Admin Manages Districts (spec.md lines 49-58)
