# SPRINT 4 DETAILED USER STORIES
## Family Relocation System - Reminders, Audit Viewer, User Management & RBAC

**Sprint Duration:** 2 weeks
**Sprint Goal:** Implement follow-up reminders, audit log viewer, user management, and complete role-based access control
**Total Points:** ~73 points (46 Backend + 27 Frontend)
**Prerequisites:** Sprint 3 complete (PR #19 merged)

---

## SPRINT 4 OVERVIEW

### Stories in This Sprint

#### Backend Stories (30 points)

| ID | Story | Points | Status | Epic |
|----|-------|--------|--------|------|
| US-037 | Create follow-up reminder | 5 | NEW | Reminders |
| US-038 | Get reminders with filters | 3 | NEW | Reminders |
| US-039 | Complete/snooze/dismiss reminder | 3 | NEW | Reminders |
| US-040 | Due reminders report (printable) | 3 | NEW | Reminders |
| US-041 | Audit log viewer API enhancements | 2 | NEW | Audit Log |
| US-042 | List users API | 3 | NEW | User Mgmt |
| US-043 | Get user details API | 2 | NEW | User Mgmt |
| US-044 | Update user roles API | 3 | NEW | User Mgmt |
| US-045 | Deactivate/reactivate user API | 2 | NEW | User Mgmt |
| US-046 | Role-based access control review | 4 | NEW | RBAC |
| US-047 | Communication Logging via Activity Log | 4 | NEW | Activity |
| US-048 | SES Email Verification | 2 | NEW | Email |
| US-049 | Automated Agreement Follow-up | 3 | NEW | Email |
| US-050 | Board Approval Report | 3 | NEW | Reports |
| US-051 | Broker Role | 2 | NEW | RBAC |
| US-052 | Expand Activity Logging Coverage | 2 | NEW | Audit Log |
| - | Email notification for due reminders | 2 | OPTIONAL | Reminders |

#### Frontend Stories (22 points)

| ID | Story | Points | Epic |
|----|-------|--------|------|
| US-F12 | Reminders dashboard page | 5 | Reminders |
| US-F13 | Create reminder modal | 3 | Reminders |
| US-F14 | Reminder actions (complete/snooze) | 2 | Reminders |
| US-F15 | Print view for due reminders | 2 | Reminders |
| US-F16 | Audit history tab on detail pages | 4 | Audit Log |
| US-F17 | User management page | 4 | User Mgmt |
| US-F18 | Create/edit user modal | 2 | User Mgmt |
| US-F19 | Log Activity Modal | 3 | Activity |
| US-F20 | Board Approval Report Print View | 2 | Reports |

**Total: 73 points (46 Backend + 27 Frontend)**

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

### US-042: List Users API (3 points)

**As an** admin
**I want to** view all users in the system
**So that** I can manage user access and roles

#### Acceptance Criteria

1. List all Cognito users with pagination
2. Include: email, name, roles, status (active/disabled), created date, last login
3. Filter by: role, status, search by name/email
4. Sort by name, email, or created date
5. Admin-only access

#### Technical Implementation

**Application Layer:**

Create `src/FamilyRelocation.Application/Users/Queries/GetUsers/`:
- `GetUsersQuery.cs` - Query with filters (role, status, search, page, pageSize)
- `GetUsersQueryHandler.cs` - Uses IAuthenticationService to fetch from Cognito
- `UserDto.cs` - Response DTO

**API Endpoint:**
```
GET /api/users?role=Coordinator&status=active&search=cohen&page=1&pageSize=20
Response: { items: [...], page, pageSize, totalCount, totalPages }
```

**IAuthenticationService extension:**
```csharp
Task<PaginatedList<UserInfo>> ListUsersAsync(
    string? roleFilter,
    string? statusFilter,
    string? search,
    int page,
    int pageSize,
    CancellationToken ct);
```

---

### US-043: Get User Details API (2 points)

**As an** admin
**I want to** view detailed information about a specific user
**So that** I can review their access and activity

#### Acceptance Criteria

1. Get user by ID (Cognito sub)
2. Include: email, name, all roles, status, created date, last login, MFA status
3. Include recent activity count (optional)
4. Admin-only access

#### Technical Implementation

**API Endpoint:**
```
GET /api/users/{userId}
Response: { id, email, name, roles: [], status, createdAt, lastLogin, mfaEnabled }
```

---

### US-044: Update User Roles API (3 points)

**As an** admin
**I want to** assign or remove roles from users
**So that** I can control what features users can access

#### Acceptance Criteria

1. Add role(s) to user
2. Remove role(s) from user
3. Set complete role list (replace all)
4. Available roles: Admin, Coordinator, BoardMember
5. Cannot remove own Admin role (prevent lockout)
6. Activity logged for all role changes
7. Admin-only access

#### Technical Implementation

**Application Layer:**

Create `src/FamilyRelocation.Application/Users/Commands/UpdateUserRoles/`:
- `UpdateUserRolesCommand.cs` - Command with userId and roles list
- `UpdateUserRolesCommandHandler.cs` - Uses IAuthenticationService to update Cognito groups

**API Endpoint:**
```
PUT /api/users/{userId}/roles
Body: { roles: ["Coordinator", "BoardMember"] }
Response: { userId, roles: [...], message: "Roles updated" }
```

**IAuthenticationService extension:**
```csharp
Task UpdateUserRolesAsync(Guid userId, IEnumerable<string> roles, CancellationToken ct);
```

---

### US-045: Deactivate/Reactivate User API (2 points)

**As an** admin
**I want to** disable or re-enable user accounts
**So that** I can manage access when staff leave or return

#### Acceptance Criteria

1. Disable user (prevents login, preserves data)
2. Re-enable disabled user
3. Cannot disable own account (prevent lockout)
4. Activity logged for status changes
5. Admin-only access

#### Technical Implementation

**Application Layer:**

Create commands:
- `DeactivateUserCommand.cs` + handler
- `ReactivateUserCommand.cs` + handler

**API Endpoints:**
```
POST /api/users/{userId}/deactivate
POST /api/users/{userId}/reactivate
Response: { userId, status, message }
```

**IAuthenticationService extension:**
```csharp
Task DisableUserAsync(Guid userId, CancellationToken ct);
Task EnableUserAsync(Guid userId, CancellationToken ct);
```

---

### US-046: Role-Based Access Control Review (4 points)

**As a** system architect
**I want to** ensure all endpoints have appropriate role-based access
**So that** users can only access features appropriate for their role

#### Current State Analysis

Current role usage in the codebase:
- **Admin**: AuditLogsController, RegisterUser endpoint
- **Generic [Authorize]**: All other controllers (any authenticated user)

Defined roles in the system:
- **Admin**: Full system access, user management, audit logs
- **Coordinator**: Day-to-day operations, manage applicants/properties/searches
- **BoardMember**: View applicants for review, set board decisions

#### Required Access Control Changes

| Controller | Current | Should Be |
|------------|---------|-----------|
| ApplicantsController | [Authorize] | POST/PUT/DELETE: Coordinator+Admin only |
| PropertiesController | [Authorize] | POST/PUT/DELETE: Coordinator+Admin only |
| HousingSearchesController | [Authorize] | POST/PUT/DELETE: Coordinator+Admin only |
| DocumentsController | [Authorize] | POST/DELETE: Coordinator+Admin only |
| DocumentTypesController | [Authorize] | POST/PUT/DELETE: Admin only |
| StageRequirementsController | [Authorize] | POST/PUT/DELETE: Admin only |
| AuditLogsController | Admin | ✓ Correct |
| DashboardController | [Authorize] | ✓ Correct (read-only) |
| ActivitiesController | [Authorize] | ✓ Correct (read-only) |

#### Acceptance Criteria

1. Review all controllers and endpoints
2. Apply appropriate `[Authorize(Roles = "...")]` attributes
3. Settings endpoints (DocumentTypes, StageRequirements) require Admin
4. Write operations (POST, PUT, DELETE) require Coordinator or Admin
5. Read operations available to all authenticated users
6. Board decision endpoint requires BoardMember or Admin
7. Update frontend to show/hide features based on user roles
8. Document the complete RBAC matrix

#### Technical Implementation

**API Changes:**

Apply role-based authorization to controllers:

```csharp
// ApplicantsController.cs
[Authorize]
public class ApplicantsController : ControllerBase
{
    [HttpGet] // All authenticated users can read
    public async Task<IActionResult> GetAll(...) { }

    [HttpPost]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<IActionResult> Create(...) { }

    [HttpPost("{id}/board-decision")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> SetBoardDecision(...) { }
}
```

**Frontend Changes:**

Update authStore to properly handle roles:
```typescript
// authStore.ts
interface AuthState {
  user: User | null;
  roles: string[];
  hasRole: (role: string) => boolean;
  canWrite: () => boolean; // Admin or Coordinator
  isBoardMember: () => boolean;
  isAdmin: () => boolean;
}
```

Conditionally render UI elements based on roles.

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

### US-F17: User Management Page (4 points)

**As an** admin
**I want to** view and manage all system users
**So that** I can control access to the application

#### Acceptance Criteria

1. Page at route `/admin/users` (admin-only)
2. Table showing: Name, Email, Roles (badges), Status, Last Login, Actions
3. Filter by role and status
4. Search by name or email
5. Quick actions: Edit roles, Deactivate/Reactivate
6. Visual indicators for disabled users (grayed out)
7. Link to create new user (existing RegisterUser endpoint)
8. Show in sidebar only for Admin users

#### Technical Implementation

Create `src/FamilyRelocation.Web/src/features/admin/UsersPage.tsx`:
- Use Ant Design Table component
- Role badges with colors: Admin (red), Coordinator (blue), BoardMember (green)
- Status column with Active/Disabled tag
- Actions column with dropdown menu

Create `src/FamilyRelocation.Web/src/api/endpoints/users.ts`:
- getUsers(filters, page, pageSize)
- getUser(userId)
- updateUserRoles(userId, roles)
- deactivateUser(userId)
- reactivateUser(userId)

Update sidebar navigation:
- Add "Users" link under Settings section
- Only visible when user.roles.includes('Admin')

---

### US-F18: Create/Edit User Modal (2 points)

**As an** admin
**I want to** create new users and edit existing user roles
**So that** I can onboard staff and manage permissions

#### Acceptance Criteria

1. Create user form: Email, Name, Temporary Password, Role selection
2. Edit user modal: Role checkboxes, status toggle
3. Role selection with descriptions:
   - Admin: Full system access
   - Coordinator: Manage applicants, properties, searches
   - BoardMember: Review applicants, make board decisions
4. Validation: Email required, valid format
5. Success/error notifications
6. Cannot remove own Admin role (show warning)

#### Technical Implementation

Create `src/FamilyRelocation.Web/src/features/admin/CreateUserModal.tsx`:
- Form using Ant Design Form component
- Email input with validation
- Name input
- Password input with requirements
- Checkbox group for roles

Create `src/FamilyRelocation.Web/src/features/admin/EditUserModal.tsx`:
- Role checkboxes
- Status toggle (Active/Disabled)
- Show warning when editing own roles

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

User Management (Admin only):
  GET    /api/users                              List users with filters/pagination
  GET    /api/users/{userId}                     Get user details
  PUT    /api/users/{userId}/roles               Update user roles
  POST   /api/users/{userId}/deactivate          Disable user account
  POST   /api/users/{userId}/reactivate          Enable user account
  POST   /api/auth/register                      Create new user (existing)

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

- **Sprint 3 (PR #19)** - ✅ MERGED
- **Cognito groups** - Roles managed via Cognito groups (Admin, Coordinator, BoardMember)
- **Activity logging** - Reminder and user actions should be logged

---

## RISKS & MITIGATIONS

| Risk | Impact | Mitigation |
|------|--------|------------|
| Email notifications complexity | Medium | Make email optional, implement in Phase 2 if needed |
| Cognito API complexity | Medium | Use existing CognitoAuthenticationService patterns |
| Print styling cross-browser | Low | Test on Chrome/Edge, use simple CSS |
| RBAC breaking existing functionality | Medium | Careful testing, apply role restrictions incrementally |

---

## ADDITIONAL STORIES (Added from User Feedback)

### US-047: Communication Logging via Activity Log (4 points)

**As a** coordinator
**I want to** record phone calls, notes, and other communications with applicants
**So that** I have a complete history of all interactions in one place

#### Acceptance Criteria

1. Extend ActivityLog to support manual entries (not just system-generated)
2. Activity types: `System` (auto), `PhoneCall`, `Email`, `SMS`, `Note`
3. Phone calls include: Duration (minutes), Outcome (Connected, Voicemail, NoAnswer, Busy)
4. Option to create follow-up reminder directly from any communication log
5. All communications appear in unified activity timeline
6. Extensible for future email/SMS integrations

#### Technical Implementation

**Extend ActivityLog entity:**
```csharp
public class ActivityLog
{
    // Existing fields...
    public ActivityType Type { get; private set; } = ActivityType.System;
    public int? DurationMinutes { get; private set; }  // For phone calls
    public string? Outcome { get; private set; }        // Call outcome
    public Guid? FollowUpReminderId { get; private set; }

    public static ActivityLog CreatePhoneCall(...) { }
    public static ActivityLog CreateNote(...) { }
}

public enum ActivityType { System = 0, PhoneCall = 1, Email = 2, SMS = 3, Note = 4 }
```

**New endpoint:**
```
POST /api/activities
Body: {
  entityType: "Applicant",
  entityId: "...",
  type: "PhoneCall",
  description: "Discussed housing preferences",
  durationMinutes: 15,
  outcome: "Connected",
  createFollowUp: true,
  followUpDate: "2026-01-28"
}
```

---

### US-048: SES Email Verification for Applicants (2 points)

**As a** system
**I want to** verify applicant emails in SES
**So that** we can send them transactional emails

#### Acceptance Criteria

1. When new applicant is created, send SES verification email
2. Track verification status on applicant record
3. Only send emails to verified addresses
4. Admin can trigger re-verification if needed

---

### US-049: Automated Agreement Follow-up Emails (3 points)

**As a** coordinator
**I want** the system to automatically send weekly follow-up emails
**When** applicants haven't submitted signed agreements after board approval

#### Acceptance Criteria

1. Background job runs daily to check for overdue agreements
2. Send follow-up email 7 days after board approval if agreements not received
3. Send weekly reminders until agreements received (max 4 reminders)
4. Track reminder count and dates
5. Coordinator can disable auto-reminders per applicant

---

### US-050: Board Approval Report (3 points)

**As a** board member
**I want** a printable report of applicants pending board approval
**So that** I can review applications before the board meeting

#### Acceptance Criteria

1. GET /api/reports/pending-board-review
2. Each applicant on a separate page with:
   - Family information (husband, wife, children)
   - Current address and community details
   - Application date and time in queue
   - Any notes or special circumstances
3. Print-optimized layout
4. Ability to upload signed board approval document after meeting

---

### US-051: Broker Role (2 points)

**As an** admin
**I want** a Broker role with limited access
**So that** real estate brokers can view relevant information

#### Acceptance Criteria

1. New role: Broker
2. Broker can view:
   - Applicants who are Searching or later stages
   - Housing preferences and budget
   - Property listings
3. Broker cannot view:
   - Personal contact information (masked)
   - Financial documents
   - Board review details
4. Update RBAC matrix to include Broker role

---

### US-F19: Log Activity Modal (3 points)

**As a** coordinator
**I want** to log communications from the applicant detail page
**So that** I can keep track of all interactions

#### Acceptance Criteria

1. "Log Activity" button on applicant detail page
2. Activity type selector: Phone Call, Note (Email/SMS disabled for now)
3. **Phone Call fields:** Duration (minutes), Outcome dropdown, Notes
4. **Note fields:** Just description/notes
5. Optional: Create follow-up reminder checkbox with date picker
6. Activity appears immediately in Activity tab timeline
7. Phone icon, note icon, etc. to distinguish types in timeline

---

### US-F20: Board Approval Report Print View (2 points)

**As a** board member
**I want** to print the pending board approval report
**So that** I can review applications during meetings

#### Acceptance Criteria

1. "Print Board Report" button on Pipeline page (for BoardMember/Admin)
2. Opens print-friendly view with one applicant per page
3. Includes all relevant details for review
4. Page breaks between applicants

---

### US-052: Expand Activity Logging Coverage (2 points)

**As a** coordinator
**I want** all significant actions to be logged in the activity log
**So that** I have a complete audit trail of what happened

#### Current Coverage

| Handler | Entity | Action | Status |
|---------|--------|--------|--------|
| `CreatePropertyCommandHandler` | Property | Created | ✅ Implemented |
| `UpdatePropertyCommandHandler` | Property | Updated | ✅ Implemented |
| `UpdatePropertyStatusCommandHandler` | Property | StatusChanged | ✅ Implemented |
| `DeletePropertyCommandHandler` | Property | Deleted | ✅ Implemented |
| `ChangeHousingSearchStageCommandHandler` | HousingSearch | StageChanged | ✅ Implemented |

#### Missing (To Be Added)

| Handler | Entity | Action | Priority |
|---------|--------|--------|----------|
| `CreateApplicantCommandHandler` | Applicant | Created | High |
| `UpdateApplicantCommandHandler` | Applicant | Updated | High |
| `SetBoardDecisionCommandHandler` | Applicant | BoardDecisionMade | High |
| `DocumentsController.Upload` | Document | Uploaded | Medium |
| `DocumentsController.Delete` | Document | Deleted | Medium |
| `UpdateHousingPreferencesCommandHandler` | HousingSearch | PreferencesUpdated | Medium |

#### Acceptance Criteria

1. Add `IActivityLogger` to all handlers listed above
2. Log appropriate action with meaningful description
3. Include entity name/identifier in description for context
4. Ensure all activities appear in activity feed and audit tabs

#### Technical Implementation

Each handler follows the same pattern:
```csharp
// Add to constructor
private readonly IActivityLogger _activityLogger;

// Add after SaveChangesAsync
await _activityLogger.LogAsync(
    entityType: "Applicant",
    entityId: applicant.Id,
    action: "Created",
    description: $"Applicant {familyName} created",
    cancellationToken);
```

---

## UPDATED STORY POINTS

| Category | Original | Added | New Total |
|----------|----------|-------|-----------|
| Backend | 30 | 16 | 46 |
| Frontend | 22 | 5 | 27 |
| **Total** | **52** | **21** | **73** |

*Notes:*
- *US-047 increased to 4 points due to unified communication logging approach*
- *US-052 (2 points) added for expanding activity logging coverage*

---

## GIT BRANCHING AND VERSIONING STRATEGY

### Branch Naming Convention

```
feature/[story-id]-[short-description]  e.g., feature/US-047-phone-call-logging
bugfix/[issue-id]-[short-description]   e.g., bugfix/BUG-123-fix-stage-transition
hotfix/[version]-[description]          e.g., hotfix/1.0.1-critical-auth-fix
release/[version]                       e.g., release/1.0.0
```

### Workflow

1. **Feature Development**: Create branch from `master`, PR back to `master`
2. **Releases**: Tag releases on master (e.g., `v1.0.0`, `v1.1.0`)
3. **Hotfixes**: Branch from release tag, merge to master and backport if needed

### Version Numbering (SemVer)

- **MAJOR**: Breaking API changes or major feature overhauls
- **MINOR**: New features, backward compatible
- **PATCH**: Bug fixes, minor improvements

Current: `v0.1.0` (Pre-release/development)

---

## NOTES

- The Audit Log backend is already complete - this sprint focuses on the frontend viewer
- Reminders are a new feature requiring full stack implementation
- User Management leverages existing Cognito integration
- RBAC review will document the complete access control matrix
- Consider future enhancement: Smart reminder suggestions based on applicant state
- **Activity logging for stage changes**: Fixed - added IActivityLogger to ChangeHousingSearchStageCommandHandler
- **SES sandbox mode**: Requires email verification for each recipient in sandbox; consider requesting production access
- **Broker role**: Consider privacy implications - may need data masking layer
