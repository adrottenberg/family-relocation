# Technical Specifications

This folder contains detailed technical specifications for features and changes that require more than a simple Jira ticket description.

## When to Create a Spec

Create a spec document when:
- Feature requires architectural decisions
- Multiple components/layers are affected
- API contracts need to be defined
- Complex business logic needs documentation
- Design review is beneficial before coding

## Naming Convention

```
UN-{ticket}-{short-description}.md
```

Examples:
- `UN-86-audit-log-viewer.md`
- `UN-100-open-house-management.md`
- `UN-115-bulk-property-import.md`

## Spec Template

```markdown
# UN-XXX: Feature Title

**Jira Ticket:** [UN-XXX](https://adrottenberg.atlassian.net/browse/UN-XXX)
**Status:** Draft | In Review | Approved | Implemented
**Author:** [Name]
**Date:** YYYY-MM-DD

## Overview

Brief description of what this feature does and why it's needed.

## User Story

As a [role], I want [capability] so that [benefit].

## Acceptance Criteria

- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3

## Technical Approach

### Architecture

[Describe the high-level approach, which layers are affected]

### Domain Changes

[New entities, value objects, domain events]

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/resource | List resources |
| POST | /api/resource | Create resource |

### Request/Response Examples

```json
// Request
{
  "field": "value"
}

// Response
{
  "id": "guid",
  "field": "value"
}
```

### Database Changes

[New tables, columns, migrations needed]

### Frontend Components

[New pages, components, state management]

## Dependencies

- Depends on: [other tickets or features]
- Blocks: [tickets waiting on this]

## Open Questions

- [ ] Question 1?
- [ ] Question 2?

## Decision Log

| Date | Decision | Rationale |
|------|----------|-----------|
| YYYY-MM-DD | Decision made | Why |
```

## Workflow

1. **Create spec** when picking up a complex ticket
2. **Link in Jira** - add spec path to ticket description
3. **Review** - spec can be part of PR or separate review
4. **Update status** - mark as Implemented when done
5. **Keep updated** - if implementation diverges, update spec

## Index

| Spec | Ticket | Status | Description |
|------|--------|--------|-------------|
| *(none yet)* | | | |
