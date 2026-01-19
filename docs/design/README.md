# Design System & Prototypes

This folder contains the design system documentation and interactive prototypes for the Family Relocation CRM.

## Files

| File | Description |
|------|-------------|
| `crm-design-system-v4.html` | Complete design system with colors, typography, components |
| `prototype-login-page.html` | Interactive login page prototype |
| `prototype-pipeline-kanban.html` | Interactive Kanban board prototype with drag & drop |

## How to Use

Open any HTML file directly in your browser to view and interact with it:

```bash
# From the repository root
open docs/design/crm-design-system-v4.html
```

Or use a local server:

```bash
cd docs/design
npx serve .
```

## Design System Overview

### Colors

**Primary (Interactive Elements)**
- Button background: `#d0e4fc`
- Button text: `#1e40af`
- Focus/active: `#dbeafe`

**Brand (Logo/Success)**
- Green: `#3d9a4a`

**Neutrals**
- Text: `#1a1d1a` to `#7a7e7a`
- Backgrounds: `#f8f9f8` to `#ffffff`
- Borders: `#c4c7c4` to `#e2e4e2`

### Typography

- **Font**: Assistant, Heebo (supports Hebrew)
- **Base size**: 14px
- **Headings**: 30px / 24px / 20px / 18px / 16px

### Button Style

We use "Option B" - light background with dark text for reduced eye strain:

```css
.btn-primary {
  background: #d0e4fc;
  color: #1e40af;
  border: 1px solid #bfdbfe;
}
```

## Prototypes

### Login Page
- Centered card layout
- Logo with Hebrew text "וועד הישוב"
- Form validation states
- Loading and error states

**Demo controls** in bottom-right corner let you test different states.

### Pipeline Kanban
- Full app layout with sidebar
- 4 pipeline stages: Submitted → House Hunting → Under Contract → Closed
- Drag and drop cards between columns
- Click cards to open detail modal
- Search and filter functionality

## Related Documentation

- [Frontend Design Specs](../SPRINT_2_FRONTEND_DESIGN_SPECS.md) - Detailed implementation specs for each frontend story
- [Ant Design Theme](../../src/FamilyRelocation.Web/src/theme/antd-theme.ts) - ConfigProvider theme configuration
