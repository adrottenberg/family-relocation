# SPRINT 5 DETAILED USER STORIES
## Family Relocation System - Code Quality, Security Hardening & Performance

**Sprint Duration:** 2 weeks
**Sprint Goal:** Address code review findings, security hardening, and frontend improvements
**Total Points:** ~45 points (20 Backend + 25 Frontend)
**Prerequisites:** Sprint 4 complete

---

## SPRINT 5 OVERVIEW

This sprint focuses on addressing issues identified during the January 21, 2026 comprehensive code review, with emphasis on security hardening, accessibility, and code quality improvements.

### Stories in This Sprint

#### Backend Stories (20 points)

| ID | Story | Points | Priority | Category |
|----|-------|--------|----------|----------|
| CR-008 | Email sending error handling | 2 | HIGH | Reliability |
| CR-009 | Dashboard query optimization | 3 | MEDIUM | Performance |
| CR-010 | Resource-level authorization | 4 | HIGH | Security |
| CR-011 | Token refresh race condition | 2 | MEDIUM | Auth |
| CR-012 | Rate limiting on auth endpoints | 3 | HIGH | Security |
| CR-013 | Domain layer encapsulation fixes | 2 | MEDIUM | Code Quality |
| CR-014 | Email validation consistency | 2 | MEDIUM | Code Quality |
| CR-015 | AuditLogsController pagination limit | 1 | LOW | Security |
| CR-016 | Stream disposal fix in S3 service | 1 | MEDIUM | Bug Fix |

#### Frontend Stories (25 points)

| ID | Story | Points | Priority | Category |
|----|-------|--------|----------|----------|
| CR-F01 | Fix XSS in print function | 3 | CRITICAL | Security |
| CR-F02 | Decode JWT for roles | 3 | CRITICAL | Security |
| CR-F03 | Migrate tokens from localStorage | 4 | HIGH | Security |
| CR-F04 | Keyboard navigation for Kanban | 4 | HIGH | Accessibility |
| CR-F05 | Token refresh race condition fix | 2 | MEDIUM | Reliability |
| CR-F06 | Form validation improvements | 2 | MEDIUM | UX |
| CR-F07 | Protected route loading state | 1 | MEDIUM | UX |
| CR-F08 | React.memo for pipeline components | 2 | MEDIUM | Performance |
| CR-F09 | ARIA labels and accessibility | 2 | MEDIUM | Accessibility |
| CR-F10 | Extract duplicate utilities | 2 | LOW | Code Quality |

**Total: 45 points (20 Backend + 25 Frontend)**

---

## BACKEND STORIES

### CR-008: Email Sending Error Handling (2 points) - HIGH

**Problem:** Email sending calls in command handlers are not wrapped in try-catch. Email service failures crash the entire command.

**Files affected:**
- `CreateApplicantCommandHandler.cs:85-96`
- `SetBoardDecisionCommandHandler.cs:74-85`
- `ChangeStageCommandHandler.cs:84-95`

**Fix:**
```csharp
try
{
    await _emailService.SendTemplatedEmailAsync(
        to: email,
        templateName: "BoardDecision",
        templateData: new { /* ... */ },
        cancellationToken);
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Failed to send email to {Email}", email);
    // Don't throw - email failures shouldn't fail the main operation
}
```

---

### CR-009: Dashboard Query Optimization (3 points) - MEDIUM

**Problem:** `GetDashboardStatsQueryHandler` loads ALL applicants and housing searches into memory, then does LINQ-to-Objects grouping.

**File:** `Application/Dashboard/Queries/GetDashboardStatsQueryHandler.cs:21-30`

**Current (inefficient):**
```csharp
var applicants = await _context.Set<Applicant>()
    .Where(a => !a.IsDeleted)
    .ToListAsync(ct);  // Loads everything

var housingSearches = await _context.Set<HousingSearch>().ToListAsync(ct);
```

**Fix:** Use database-side aggregation:
```csharp
var stats = await _context.Set<Applicant>()
    .Where(a => !a.IsDeleted)
    .GroupBy(a => a.Status)
    .Select(g => new { Status = g.Key, Count = g.Count() })
    .ToListAsync(ct);

var stageStats = await _context.Set<HousingSearch>()
    .GroupBy(h => h.Stage)
    .Select(g => new { Stage = g.Key, Count = g.Count() })
    .ToListAsync(ct);
```

---

### CR-010: Resource-Level Authorization (4 points) - HIGH

**Problem:** Any authenticated user can view any applicant's details. No check that user has permission to access specific resources.

**Example:** `ApplicantsController.GetById` doesn't verify user's permission.

**Implementation:**
1. Create `IAuthorizationService` interface
2. Add resource-based authorization checks
3. Optionally: Brokers can only see applicants in Searching+ stages
4. Optionally: Coordinators can only see their assigned applicants

```csharp
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetById(Guid id)
{
    var applicant = await _mediator.Send(new GetApplicantByIdQuery(id));

    if (!await _authService.CanViewApplicant(User, applicant))
        return Forbid();

    return Ok(applicant);
}
```

---

### CR-011: Backend Token Refresh Improvements (2 points) - MEDIUM

**Problem:** `AuthController.Refresh` requires username + refresh token but doesn't fully validate token ownership.

**File:** `AuthController.cs:157`

**Fix:**
1. Validate refresh token belongs to the specified user
2. Add logging for suspicious activity
3. Consider using token family approach

---

### CR-012: Rate Limiting on Auth Endpoints (3 points) - HIGH

**Problem:** No rate limiting on login/password reset endpoints allows brute force attacks.

**Implementation:**
```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// AuthController.cs
[EnableRateLimiting("auth")]
[HttpPost("login")]
public async Task<IActionResult> Login(...) { }

[EnableRateLimiting("auth")]
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword(...) { }
```

---

### CR-013: Domain Layer Encapsulation Fixes (2 points) - MEDIUM

**Issues:**
1. `Property.Features` returns mutable `List<string>` - should be `IReadOnlyList<string>`
2. `AuditableEntity` has `protected set` allowing subclasses to modify audit fields
3. `Applicant.Children` should return `IReadOnlyCollection<Child>`

**Files:**
- `Domain/Entities/Property.cs:18`
- `Domain/Common/AuditableEntity.cs:8-11`
- `Domain/Entities/Applicant.cs:32`

---

### CR-014: Email Validation Consistency (2 points) - MEDIUM

**Problem:** Email case sensitivity is inconsistent across queries. `ExistsByEmailQuery` normalizes but comparison may fail.

**Files:**
- `HusbandInfo.cs:48` - Silent email validation failure
- `SpouseInfo.cs:46` - Silent email validation failure
- `ExistsByEmailQueryHandler.cs:26` - No null check

**Fix:**
1. Always store emails in lowercase in domain value objects
2. Add explicit validation instead of silent failure
3. Add null check in query handler

---

### CR-015: AuditLogsController Pagination Limit (1 point) - LOW

**Problem:** No maximum pageSize limit - could request millions of records.

**File:** `AuditLogsController.cs:48-49`

**Fix:**
```csharp
[HttpGet]
public async Task<IActionResult> GetAll(int page = 1, int pageSize = 50)
{
    pageSize = Math.Clamp(pageSize, 1, 100);  // Add limit
    // ...
}
```

---

### CR-016: Stream Disposal Fix in S3 Service (1 point) - MEDIUM

**Problem:** `fileStream.Length` accessed after async `PutObjectAsync` - stream may be disposed.

**File:** `Infrastructure/AWS/S3DocumentStorageService.cs:46`

**Fix:**
```csharp
public async Task<DocumentUploadResult> UploadAsync(...)
{
    var fileSize = fileStream.Length;  // Capture BEFORE async operation

    await _s3Client.PutObjectAsync(request, cancellationToken);

    return new DocumentUploadResult(
        StorageUrl: url,
        StorageKey: storageKey,
        FileSize: fileSize,  // Use captured value
        // ...
    );
}
```

---

## FRONTEND STORIES

### CR-F01: Fix XSS Vulnerability in Print Function (3 points) - CRITICAL

**Problem:** HTML built from untrusted data without escaping in print view.

**File:** `Web/src/features/applicants/ApplicantDetailPage.tsx:267-344`

**Current (vulnerable):**
```typescript
const html = `<h1>${husband.lastName} Family</h1>`;
```

**Fix Option 1 - DOMPurify:**
```typescript
import DOMPurify from 'dompurify';

const html = `<h1>${DOMPurify.sanitize(husband.lastName)} Family</h1>`;
```

**Fix Option 2 - Safe DOM Creation:**
```typescript
const container = document.createElement('div');
const h1 = document.createElement('h1');
h1.textContent = `${husband.lastName} Family`;  // textContent is safe
container.appendChild(h1);
```

**Fix Option 3 - Use react-to-print library** (recommended for long-term maintenance)

---

### CR-F02: Decode JWT for Roles (3 points) - CRITICAL

**Problem:** All users get hardcoded 'Admin' role instead of actual roles from JWT.

**File:** `Web/src/features/auth/LoginPage.tsx:114-116`

**Current:**
```typescript
// TODO: Extract roles from JWT token (idToken) in production
setUser({ email: userEmail, roles: ['Admin'] });
```

**Fix:**
```typescript
import { jwtDecode } from 'jwt-decode';

interface CognitoToken {
  'cognito:groups'?: string[];
  email: string;
  sub: string;
}

const handleLoginSuccess = (tokens: Tokens) => {
  const decoded = jwtDecode<CognitoToken>(tokens.idToken);
  const roles = decoded['cognito:groups'] || [];

  setTokens(tokens);
  setUser({
    email: decoded.email,
    roles: roles,
    id: decoded.sub,
  });
};
```

---

### CR-F03: Migrate Tokens from localStorage (4 points) - HIGH

**Problem:** Refresh tokens stored in localStorage are vulnerable to XSS attacks.

**File:** `Web/src/store/authStore.ts:27-69`

**Options:**
1. **Memory-only storage** (tokens cleared on page refresh - requires re-login)
2. **HttpOnly cookies** (requires backend changes)
3. **SessionStorage** (better than localStorage, cleared on tab close)

**Recommended approach - Memory + Short-lived tokens:**
```typescript
// Don't persist tokens
export const useAuthStore = create<AuthState>()((set, get) => ({
  tokens: null,
  user: null,
  // ... remove persist middleware
}));

// Implement silent refresh on app load if user was previously logged in
```

---

### CR-F04: Keyboard Navigation for Kanban Board (4 points) - HIGH

**Problem:** Pipeline Kanban board is mouse-only. Screen readers and keyboard users cannot navigate.

**File:** `Web/src/features/pipeline/PipelinePage.tsx:160-242`

**Implementation:**
```typescript
const KanbanCard = ({ item, ...props }) => {
  const handleKeyDown = (e: React.KeyboardEvent) => {
    switch (e.key) {
      case 'Enter':
      case ' ':
        onClick(item.applicantId);
        break;
      case 'ArrowRight':
        e.preventDefault();
        // Move to next stage
        const nextStage = getNextStage(item.stage);
        if (nextStage) initiateTransition(item, nextStage);
        break;
      case 'ArrowLeft':
        e.preventDefault();
        // Move to previous stage (if allowed)
        break;
    }
  };

  return (
    <div
      role="listitem"
      tabIndex={0}
      aria-label={`${item.familyName} - ${formatStage(item.stage)}`}
      onKeyDown={handleKeyDown}
      // ...
    >
      {/* ... */}
    </div>
  );
};

const KanbanColumn = ({ stage, items }) => (
  <div role="list" aria-label={`${formatStage(stage)} applicants`}>
    {items.map(item => <KanbanCard key={item.id} item={item} />)}
  </div>
);
```

---

### CR-F05: Token Refresh Race Condition Fix (2 points) - MEDIUM

**Problem:** Multiple 401 responses can trigger multiple refresh attempts simultaneously.

**File:** `Web/src/api/client.ts:27-69`

**Fix - Implement refresh queue:**
```typescript
let refreshPromise: Promise<string> | null = null;

const refreshAccessToken = async (): Promise<string> => {
  if (refreshPromise) {
    return refreshPromise;  // Return existing promise if refresh in progress
  }

  refreshPromise = (async () => {
    try {
      const { tokens } = useAuthStore.getState();
      const response = await axios.post('/api/auth/refresh', {
        refreshToken: tokens?.refreshToken,
      });

      const newTokens = response.data;
      useAuthStore.getState().setTokens(newTokens);
      return newTokens.accessToken;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
};
```

---

### CR-F06: Form Validation Improvements (2 points) - MEDIUM

**Issues:**
1. Phone number validation missing
2. ZIP code format validation missing
3. Server-side errors not mapped to form fields

**Files:**
- `CreateApplicantDrawer.tsx:192-217` (phone validation)
- `CreateApplicantDrawer.tsx:333-340` (ZIP validation)

**Fix:**
```typescript
// Phone validation
<Form.Item
  name={[name, 'number']}
  rules={[
    { pattern: /^\d{10}$/, message: 'Phone must be 10 digits' }
  ]}
>
  <Input placeholder="Phone number" />
</Form.Item>

// ZIP validation
<Form.Item
  name={['address', 'zipCode']}
  rules={[
    { required: true, message: 'ZIP code is required' },
    { pattern: /^\d{5}(-\d{4})?$/, message: 'Invalid ZIP code format' }
  ]}
>
  <Input />
</Form.Item>

// Server error mapping
const mutation = useMutation({
  onError: (error) => {
    const apiError = getApiError(error);
    if (apiError.fieldErrors) {
      form.setFields(
        Object.entries(apiError.fieldErrors).map(([name, errors]) => ({
          name: name.split('.'),
          errors: errors as string[],
        }))
      );
    }
  },
});
```

---

### CR-F07: Protected Route Loading State (1 point) - MEDIUM

**Problem:** Brief flash of login page before auth state loads from storage.

**File:** `Web/src/App.tsx:26-34`

**Fix:**
```typescript
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuthenticated, isHydrated } = useAuthStore((state) => ({
    isAuthenticated: state.isAuthenticated,
    isHydrated: state._hasHydrated,
  }));

  if (!isHydrated) {
    return <Spin size="large" className="full-page-loader" />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};
```

---

### CR-F08: React.memo for Pipeline Components (2 points) - MEDIUM

**Problem:** KanbanCard and KanbanColumn re-render unnecessarily.

**File:** `Web/src/features/pipeline/PipelinePage.tsx:396-511`

**Fix:**
```typescript
const KanbanCard = React.memo(({ item, borderColor, onDragStart, onClick }: KanbanCardProps) => {
  // ... component implementation
});

const KanbanColumn = React.memo(({ stage, config, items, onDragStart, onDrop, onClick }: KanbanColumnProps) => {
  // ... component implementation
});

// Also wrap event handlers in useCallback
const handleDragStart = useCallback((e: DragEvent, item: PipelineItem) => {
  // ...
}, []);

const handleDrop = useCallback((e: DragEvent, targetStage: PipelineStage) => {
  // ...
}, [/* dependencies */]);
```

---

### CR-F09: ARIA Labels and Accessibility (2 points) - MEDIUM

**Issues:**
1. Missing ARIA labels on interactive elements
2. Missing accessible names for Kanban cards
3. Form field labels not properly associated

**Implementation:**
```typescript
// Kanban card
<div
  role="button"
  tabIndex={0}
  aria-label={`${item.familyName} family - ${item.stage} stage. Press Enter to view details, arrow keys to change stage.`}
>

// Action buttons
<Button
  aria-label={`Complete reminder for ${reminder.title}`}
  icon={<CheckOutlined />}
/>

// Form fields - ensure all have htmlFor
<Form.Item
  name="email"
  label={<label htmlFor="email">Email Address</label>}
>
  <Input id="email" />
</Form.Item>
```

---

### CR-F10: Extract Duplicate Utilities (2 points) - LOW

**Problem:** Formatting functions duplicated across components.

**Files:**
- `ApplicantListPage.tsx:43-70`
- `ApplicantDetailPage.tsx:110-137`

**Fix - Create shared utilities:**
```typescript
// src/utils/formatting.ts
export const formatStageName = (stage: string): string => {
  const names: Record<string, string> = {
    Submitted: 'Submitted',
    AwaitingAgreements: 'Awaiting Agreements',
    Searching: 'Searching',
    UnderContract: 'Under Contract',
    Closed: 'Closed',
    MovedIn: 'Moved In',
  };
  return names[stage] || stage;
};

export const getStatusTagStyle = (decision: string) => {
  switch (decision) {
    case 'Approved': return { color: 'green' };
    case 'Rejected': return { color: 'red' };
    case 'Deferred': return { color: 'orange' };
    default: return { color: 'default' };
  }
};

export const getStageTagStyle = (stage: string) => {
  // ... implementation
};
```

Then import in components:
```typescript
import { formatStageName, getStatusTagStyle } from '@/utils/formatting';
```

---

## TESTING REQUIREMENTS

### Backend Tests
1. Rate limiting integration tests
2. Resource authorization tests
3. Email error handling tests
4. Dashboard query performance tests

### Frontend Tests
1. XSS prevention in print function
2. JWT decoding and role extraction
3. Keyboard navigation for Kanban
4. Form validation rules
5. Token refresh race condition handling

---

## DEFINITION OF DONE

- [ ] All security vulnerabilities addressed
- [ ] Accessibility audit passes (WCAG 2.1 AA)
- [ ] Performance improvements verified
- [ ] Unit tests for new functionality
- [ ] Code reviewed and approved
- [ ] Manual testing completed
- [ ] Documentation updated

---

## DEPENDENCIES

- **Sprint 4** - Must be complete (especially CR-001 through CR-007)
- **DOMPurify** - New npm dependency for XSS prevention
- **jwt-decode** - May already be installed, verify
- **react-to-print** - Optional, for better print handling

---

## RISKS & MITIGATIONS

| Risk | Impact | Mitigation |
|------|--------|------------|
| Token migration breaks existing sessions | High | Implement gradual migration with fallback |
| Rate limiting affects legitimate users | Medium | Start with lenient limits, adjust based on monitoring |
| Accessibility changes affect existing UI | Low | Test thoroughly, maintain visual appearance |
| Performance optimizations add complexity | Low | Keep changes minimal and well-documented |

---

## NOTES

- This sprint originated from the January 21, 2026 comprehensive code review
- Critical security issues (CR-F01, CR-F02) should be prioritized
- Consider security penetration testing after this sprint
- Frontend accessibility improvements align with WCAG 2.1 AA compliance goal
- Performance optimizations target the pipeline page which has most complexity
- **Property Matches Cleanup**: When a property is no longer actively on the market (status changed to Sold, OffMarket, etc.), all property matches associated with that property should be automatically removed
- **User Documentation**: Create extensive user documentation consisting of: (1) A brief overview of the entire system for new users, and (2) A comprehensive guide covering all features and workflows in detail
- **Contextual Help**: Implement contextual help throughout the website - tooltips, info icons, and inline guidance to help users understand features without leaving the current page
