# SPRINT 4 DETAILED USER STORIES
## Family Relocation System - Follow-Up Reminders & Audit Log Viewer

**Sprint Duration:** 2 weeks
**Sprint Goal:** Implement follow-up reminder system for coordinator workflow and audit log viewer for compliance
**Total Points:** ~34 points (18 Backend + 16 Frontend)
**Prerequisites:** Sprint 3 complete (PR #19 merged)

---

## SPRINT 4 OVERVIEW

### Stories in This Sprint

#### Backend Stories (18 points)

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-037 | Create follow-up reminder | 5 | NEW |
| US-038 | Get reminders with filters | 3 | NEW |
| US-039 | Complete/snooze/dismiss reminder | 3 | NEW |
| US-040 | Due reminders report (printable) | 3 | NEW |
| US-041 | Audit log viewer API enhancements | 2 | NEW |
| - | Email notification for due reminders | 2 | NEW (optional) |

#### Frontend Stories (16 points)

| ID | Story | Points | Epic |
|----|-------|--------|------|
| US-F12 | Reminders dashboard page | 5 | Reminders |
| US-F13 | Create reminder modal | 3 | Reminders |
| US-F14 | Reminder actions (complete/snooze) | 2 | Reminders |
| US-F15 | Print view for due reminders | 2 | Reminders |
| US-F16 | Audit history tab on detail pages | 4 | Audit Log |

**Total: 34 points (18 Backend + 16 Frontend)**

---

## BACKEND STORIES

### US-037: Create Follow-Up Reminder (5 points)

**As a** coordinator
**I want to** create follow-up reminders on applicants, properties, or general tasks
**So that** I don't forget important follow-up actions

#### Acceptance Criteria

1. Create reminder with required fields: Title, DueDate, EntityType, EntityId
2. Optional fields: Notes, DueTime, Priority, AssignedToUserId, SendEmailNotification
3. Priority levels: Low, Normal (default), High, Urgent
4. EntityType supports: Applicant, Property, HousingSearch, General
5. Validation: DueDate must be today or future, Title required (max 200 chars)
6. Returns created reminder with Id

#### Technical Implementation

**Domain Layer:**

Create `src/FamilyRelocation.Domain/Entities/FollowUpReminder.cs`:
```csharp
public class FollowUpReminder
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Notes { get; private set; }
    public DateTime DueDate { get; private set; }
    public TimeOnly? DueTime { get; private set; }
    public ReminderPriority Priority { get; private set; }
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public ReminderStatus Status { get; private set; }
    public bool SendEmailNotification { get; private set; }
    public DateTime? SnoozedUntil { get; private set; }
    public int SnoozeCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CompletedBy { get; private set; }

    public static FollowUpReminder Create(...) { }
    public void Complete(Guid userId) { }
    public void Snooze(DateTime until, Guid userId) { }
    public void Dismiss(Guid userId) { }
}
```

Create `src/FamilyRelocation.Domain/Enums/ReminderPriority.cs`:
```csharp
public enum ReminderPriority { Low = 0, Normal = 1, High = 2, Urgent = 3 }
```

Create `src/FamilyRelocation.Domain/Enums/ReminderStatus.cs`:
```csharp
public enum ReminderStatus { Open = 0, Completed = 1, Snoozed = 2, Dismissed = 3 }
```

**Application Layer:**

Create `src/FamilyRelocation.Application/Reminders/Commands/CreateReminder/`:
- `CreateReminderCommand.cs` - Command record with all fields
- `CreateReminderCommandHandler.cs` - Handler that creates domain entity
- `CreateReminderCommandValidator.cs` - FluentValidation rules

**API Endpoint:**
```
POST /api/reminders
Body: { title, dueDate, dueTime?, priority?, entityType, entityId, assignedToUserId?, notes?, sendEmailNotification? }
Response: { id, title, dueDate, ... }
```

---

### US-038: Get Reminders with Filters (3 points)

**As a** coordinator
**I want to** view and filter my reminders by status, priority, due date, and entity
**So that** I can focus on the most important tasks

#### Acceptance Criteria

1. List reminders with pagination (page, pageSize)
2. Filter by: status, priority, assignedToUserId, entityType, entityId, dueFrom, dueTo
3. Default sort: DueDate ascending, then Priority descending
4. Include entity display name in response (e.g., "Cohen Family" for Applicant)
5. Overdue reminders flagged with isOverdue: true

#### Technical Implementation

**Application Layer:**

Create `src/FamilyRelocation.Application/Reminders/Queries/GetReminders/`:
- `GetRemindersQuery.cs` - Query with filter parameters
- `GetRemindersQueryHandler.cs` - Handler with pagination and filtering
- `ReminderDto.cs` - Response DTO with computed isOverdue flag

**API Endpoint:**
```
GET /api/reminders?status=Open&priority=High&assignedTo={userId}&dueFrom=2026-01-20&dueTo=2026-01-27&page=1&pageSize=20
Response: { items: [...], page, pageSize, totalCount, totalPages }
```

---

### US-039: Complete/Snooze/Dismiss Reminder (3 points)

**As a** coordinator
**I want to** mark reminders as complete, snooze them, or dismiss them
**So that** I can manage my task list effectively

#### Acceptance Criteria

1. Complete: Sets status to Completed, records completedAt/completedBy
2. Snooze: Sets snoozedUntil date, increments snoozeCount, status = Snoozed
3. Dismiss: Sets status to Dismissed (soft delete, still queryable)
4. Reopen: Can reopen completed/dismissed reminders back to Open
5. Activity logged for all status changes

#### Technical Implementation

**Application Layer:**

Create commands for each action:
- `CompleteReminderCommand.cs` + handler
- `SnoozeReminderCommand.cs` + handler (requires snoozeUntil date)
- `DismissReminderCommand.cs` + handler
- `ReopenReminderCommand.cs` + handler

**API Endpoints:**
```
PATCH /api/reminders/{id}/complete
PATCH /api/reminders/{id}/snooze     Body: { snoozeUntil: "2026-01-25" }
PATCH /api/reminders/{id}/dismiss
PATCH /api/reminders/{id}/reopen
```

---

### US-040: Due Reminders Report (3 points)

**As a** coordinator
**I want to** print a list of all due and overdue reminders
**So that** I can have a physical checklist for my day

#### Acceptance Criteria

1. Get all open reminders due today or overdue
2. Include optional date range parameter
3. Sorted by priority (Urgent first), then due date
4. Response optimized for printing (no pagination, grouped by priority)
5. Include entity context (family name, property address, etc.)

#### Technical Implementation

**Application Layer:**

Create `src/FamilyRelocation.Application/Reminders/Queries/GetDueRemindersReport/`:
- `GetDueRemindersReportQuery.cs` - Query with optional dateRange
- `GetDueRemindersReportQueryHandler.cs` - Handler grouped by priority

**API Endpoint:**
```
GET /api/reminders/due-report?asOfDate=2026-01-20
Response: {
  asOfDate,
  urgent: [...],
  high: [...],
  normal: [...],
  low: [...],
  totalCount
}
```

---

### US-041: Audit Log Viewer API Enhancements (2 points)

**As an** admin
**I want to** view detailed audit history with better formatting
**So that** I can understand exactly what changed and when

#### Acceptance Criteria

1. Existing API already works: GET /api/audit-logs/applicant/{id}
2. Add human-readable change descriptions
3. Add endpoint for any entity: GET /api/audit-logs/{entityType}/{entityId}
4. Add endpoint for recent audit logs across all entities (admin only)

#### Technical Implementation

The backend already has:
- `AuditLogsController` with GET endpoints
- `AuditLogEntry` entity with oldValues/newValues JSON
- `GetAuditLogsQueryHandler` with pagination

Enhancements needed:
- Add `ChangeDescription` computed property to DTO (e.g., "Changed Status from Submitted to Approved")
- Ensure property endpoint exists: GET /api/audit-logs/property/{propertyId}

---

## FRONTEND STORIES

### US-F12: Reminders Dashboard Page (5 points)

**As a** coordinator
**I want to** see all my reminders in a dashboard view
**So that** I can manage my follow-up tasks efficiently

#### Acceptance Criteria

1. Page at route `/reminders`
2. Filter controls: Status (tabs or dropdown), Priority, Due Date Range, Assigned To
3. Reminder cards showing: Title, Due Date/Time, Priority badge, Entity link, Status
4. Visual indicators: Overdue (red), Due Today (orange), Upcoming (default)
5. Click reminder to expand/view details
6. Quick actions: Complete, Snooze, Dismiss from card
7. "Create Reminder" button in header

#### Technical Implementation

Create `src/FamilyRelocation.Web/src/features/reminders/RemindersPage.tsx`:
- Use Ant Design Table or Card list
- Filters in a collapse panel or sidebar
- Status tabs: All | Open | Completed | Snoozed
- Priority badges with colors: Urgent (red), High (orange), Normal (blue), Low (gray)

Create `src/FamilyRelocation.Web/src/api/endpoints/reminders.ts`:
- getReminders(filters, page, pageSize)
- createReminder(data)
- completeReminder(id)
- snoozeReminder(id, snoozeUntil)
- dismissReminder(id)
- getDueReport(asOfDate?)

---

### US-F13: Create Reminder Modal (3 points)

**As a** coordinator
**I want to** create reminders from anywhere in the app
**So that** I can quickly capture follow-up tasks

#### Acceptance Criteria

1. Modal form with fields: Title, Due Date, Due Time (optional), Priority, Notes
2. Entity context auto-filled when opened from detail page
3. Can create "General" reminder with no entity
4. "Assign To" dropdown (optional, defaults to current user)
5. Email notification checkbox
6. Form validation with helpful error messages

#### Technical Implementation

Create `src/FamilyRelocation.Web/src/features/reminders/CreateReminderModal.tsx`:
- Form with Ant Design components
- DatePicker for due date, TimePicker for optional time
- Select for Priority with color indicators
- Can be opened from: Reminders page, Applicant detail, Property detail

Add "Add Reminder" button to:
- ApplicantDetailPage.tsx (in actions dropdown)
- PropertyDetailPage.tsx (in actions dropdown)

---

### US-F14: Reminder Actions (2 points)

**As a** coordinator
**I want to** quickly complete, snooze, or dismiss reminders
**So that** I can manage my task list without friction

#### Acceptance Criteria

1. Complete button with confirmation (optional)
2. Snooze with quick options: Tomorrow, Next Week, Custom Date
3. Dismiss with optional reason
4. Undo capability for accidental actions (within 5 seconds)
5. Optimistic UI updates

#### Technical Implementation

Create `src/FamilyRelocation.Web/src/features/reminders/ReminderActions.tsx`:
- Dropdown menu with actions
- Snooze popover with date options
- Use React Query mutations with optimistic updates

---

### US-F15: Print View for Due Reminders (2 points)

**As a** coordinator
**I want to** print my due reminders list
**So that** I can have a physical checklist

#### Acceptance Criteria

1. "Print" button on Reminders page
2. Opens print-friendly view in new tab/modal
3. Grouped by priority: Urgent, High, Normal, Low
4. Includes: Title, Due Date, Entity name, Notes (truncated)
5. Clean layout optimized for printing (no navigation, minimal styling)
6. Print date/time in header

#### Technical Implementation

Create `src/FamilyRelocation.Web/src/features/reminders/RemindersPrintView.tsx`:
- Fetch due report from API
- CSS @media print styles
- Window.print() trigger
- Or: Open in new window with print-specific layout

---

### US-F16: Audit History Tab on Detail Pages (4 points)

**As an** admin
**I want to** see the complete change history for an applicant or property
**So that** I can audit who changed what and when

#### Acceptance Criteria

1. New "History" or "Audit Log" tab on ApplicantDetailPage
2. Timeline view showing all changes
3. Each entry shows: Timestamp, User, Action, What Changed
4. Expandable to show old/new values
5. Pagination for long histories
6. Also add to PropertyDetailPage

#### Technical Implementation

Create `src/FamilyRelocation.Web/src/components/AuditHistoryTab.tsx`:
- Reusable component accepting entityType and entityId
- Use Ant Design Timeline or Table component
- Fetch from GET /api/audit-logs/{entityType}/{entityId}
- Format old/new values as readable diff

Add tab to:
- `ApplicantDetailPage.tsx` - Add "History" tab to existing Tabs
- `PropertyDetailPage.tsx` - Add "History" tab to existing Tabs

---

## INFRASTRUCTURE

### Database Migration

Create migration `AddFollowUpReminders`:

```sql
CREATE TABLE "FollowUpReminders" (
    "Id" UUID PRIMARY KEY,
    "Title" VARCHAR(200) NOT NULL,
    "Notes" TEXT,
    "DueDate" DATE NOT NULL,
    "DueTime" TIME,
    "Priority" VARCHAR(20) NOT NULL DEFAULT 'Normal',
    "EntityType" VARCHAR(50) NOT NULL,
    "EntityId" UUID NOT NULL,
    "AssignedToUserId" UUID,
    "Status" VARCHAR(20) NOT NULL DEFAULT 'Open',
    "SendEmailNotification" BOOLEAN NOT NULL DEFAULT FALSE,
    "SnoozedUntil" TIMESTAMP,
    "SnoozeCount" INT NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedBy" UUID NOT NULL,
    "CompletedAt" TIMESTAMP,
    "CompletedBy" UUID,

    CONSTRAINT "FK_FollowUpReminders_AssignedTo"
        FOREIGN KEY ("AssignedToUserId") REFERENCES "AspNetUsers"("Id")
);

CREATE INDEX "IX_FollowUpReminders_Status" ON "FollowUpReminders"("Status");
CREATE INDEX "IX_FollowUpReminders_DueDate" ON "FollowUpReminders"("DueDate");
CREATE INDEX "IX_FollowUpReminders_AssignedTo" ON "FollowUpReminders"("AssignedToUserId");
CREATE INDEX "IX_FollowUpReminders_Entity" ON "FollowUpReminders"("EntityType", "EntityId");
CREATE INDEX "IX_FollowUpReminders_Priority" ON "FollowUpReminders"("Priority");
```

### EF Core Configuration

Create `src/FamilyRelocation.Infrastructure/Persistence/Configurations/FollowUpReminderConfiguration.cs`

---

## API ENDPOINTS SUMMARY

After Sprint 4:

```
Reminders:
  POST   /api/reminders                          Create reminder
  GET    /api/reminders                          List with filters/pagination
  GET    /api/reminders/{id}                     Get single reminder
  PATCH  /api/reminders/{id}/complete            Mark complete
  PATCH  /api/reminders/{id}/snooze              Snooze until date
  PATCH  /api/reminders/{id}/dismiss             Dismiss reminder
  PATCH  /api/reminders/{id}/reopen              Reopen reminder
  GET    /api/reminders/due-report               Printable due report

Audit Logs (existing + enhanced):
  GET    /api/audit-logs                         List all (admin, with filters)
  GET    /api/audit-logs/applicant/{id}          Applicant history
  GET    /api/audit-logs/property/{id}           Property history
```

---

## TESTING REQUIREMENTS

### Backend Tests

1. **Domain Tests:**
   - FollowUpReminder.Create with valid/invalid data
   - State transitions (Complete, Snooze, Dismiss, Reopen)
   - Validation rules (future date, required fields)

2. **Integration Tests:**
   - CRUD operations through API
   - Filter combinations
   - Pagination
   - Authorization (only see own reminders or admin sees all)

### Frontend Tests

1. **Component Tests:**
   - RemindersPage renders correctly
   - Filter controls work
   - CreateReminderModal validation
   - AuditHistoryTab displays entries

---

## DEFINITION OF DONE

- [ ] All backend endpoints implemented and tested
- [ ] All frontend pages/components implemented
- [ ] Database migration created and applied
- [ ] API documentation updated (Swagger annotations)
- [ ] Unit tests for domain logic
- [ ] Integration tests for API endpoints
- [ ] Manual testing of full workflow
- [ ] Code reviewed and approved

---

## DEPENDENCIES

- **Sprint 3 (PR #19)** must be merged first
- **Cognito users** - Need user list for "Assigned To" dropdown (may need new endpoint)
- **Activity logging** - Reminder actions should be logged

---

## RISKS & MITIGATIONS

| Risk | Impact | Mitigation |
|------|--------|------------|
| Email notifications complexity | Medium | Make email optional, implement in Phase 2 if needed |
| User assignment without user management | Low | Default to current user, add user list endpoint if needed |
| Print styling cross-browser | Low | Test on Chrome/Edge, use simple CSS |

---

## NOTES

- The Audit Log backend is already complete - this sprint focuses on the frontend viewer
- Reminders are a new feature requiring full stack implementation
- Consider future enhancement: Smart reminder suggestions based on applicant state
