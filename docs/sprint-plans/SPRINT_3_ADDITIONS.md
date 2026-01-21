# Sprint 3 Additions - User Stories

**Added:** January 20, 2026
**Total Points:** ~40 points

These stories extend Sprint 3 to include Property Management, Email Notifications, Dashboard, and Activity Tracking.

---

## Epic: Property Management (~21 points)

### US-024: Create Property Entity and API
**As a** coordinator
**I want to** add new properties to the system
**So that** I can track available listings for families

**Priority:** P0
**Points:** 5

**Acceptance Criteria:**
- Property entity with: Address, Price, Beds, Baths, SqFt, LotSize, YearBuilt, Features, ListingStatus
- `POST /api/properties` endpoint creates new property
- `GET /api/properties/{id}` returns property details
- Validation for required fields (address, price, beds, baths)
- ListingStatus enum: Active, UnderContract, Sold, OffMarket

**Technical Notes:**
```csharp
public class Property : AuditableEntity
{
    public Guid Id { get; private set; }
    public Address Address { get; private set; }
    public Money Price { get; private set; }
    public int Bedrooms { get; private set; }
    public decimal Bathrooms { get; private set; }
    public int? SquareFeet { get; private set; }
    public decimal? LotSize { get; private set; }
    public int? YearBuilt { get; private set; }
    public List<string> Features { get; private set; }
    public ListingStatus Status { get; private set; }
    public string? MlsNumber { get; private set; }
    public string? Notes { get; private set; }
}
```

---

### US-025: Update Property API
**As a** coordinator
**I want to** update property details
**So that** I can keep listing information current

**Priority:** P0
**Points:** 3

**Acceptance Criteria:**
- `PUT /api/properties/{id}` updates property
- Can update all fields including status
- Returns 404 if property not found
- Audit trail updated (ModifiedBy, ModifiedAt)

---

### US-026: List Properties with Search/Filter
**As a** coordinator
**I want to** search and filter properties
**So that** I can find suitable listings for families

**Priority:** P0
**Points:** 5

**Acceptance Criteria:**
- `GET /api/properties` with pagination
- Search by address, MLS number
- Filter by: status, min/max price, min beds, min baths, city
- Sort by: price, beds, createdDate
- Returns PropertyListDto (lightweight)

**Query Parameters:**
| Parameter | Description |
|-----------|-------------|
| `page` | Page number (default: 1) |
| `pageSize` | Items per page (default: 20) |
| `search` | Search address or MLS |
| `status` | Filter by ListingStatus |
| `minPrice` | Minimum price |
| `maxPrice` | Maximum price |
| `minBeds` | Minimum bedrooms |
| `city` | Filter by city |

---

### US-027: Property List Page (Frontend)
**As a** coordinator
**I want to** view all properties in a list
**So that** I can browse available listings

**Priority:** P0
**Points:** 3

**Acceptance Criteria:**
- Table view with columns: Address, Price, Beds/Baths, Status, Added Date
- Search input for address/MLS
- Filters for status, price range, bedrooms
- Click row to navigate to detail page
- "Add Property" button opens create form

---

### US-028: Property Detail Page (Frontend)
**As a** coordinator
**I want to** view and edit property details
**So that** I can manage listing information

**Priority:** P0
**Points:** 3

**Acceptance Criteria:**
- Display all property fields
- Edit button opens drawer/modal
- Status badge with color coding
- Show created/modified dates
- Back button to list

---

### US-029: Property Photos Upload
**As a** coordinator
**I want to** upload photos for properties
**So that** families can see what listings look like

**Priority:** P1
**Points:** 2

**Acceptance Criteria:**
- `POST /api/properties/{id}/photos` uploads to S3
- `GET /api/properties/{id}/photos` returns photo URLs
- `DELETE /api/properties/{id}/photos/{photoId}` removes photo
- Support multiple photos per property
- Max 10 photos per property
- Accepted formats: JPEG, PNG

---

## Epic: Email Notifications (~8 points)

### US-030: AWS SES Configuration
**As a** developer
**I want to** configure AWS SES for email sending
**So that** the system can send notifications

**Priority:** P0
**Points:** 2

**Acceptance Criteria:**
- SES configured in Infrastructure layer
- `IEmailService` interface in Application
- `SesEmailService` implementation in Infrastructure
- Configuration via appsettings (region, from address)
- Verified sender email address

**Technical Notes:**
```csharp
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> data, CancellationToken ct = default);
}
```

---

### US-031: Application Received Email
**As an** applicant
**I want to** receive confirmation when I submit my application
**So that** I know it was received

**Priority:** P0
**Points:** 2

**Acceptance Criteria:**
- Email sent after successful application creation
- Includes: Family name, submission date, what to expect next
- Triggered by `ApplicantCreatedEvent` domain event
- Uses email template (stored in code for now)

---

### US-032: Board Decision Email
**As an** applicant
**I want to** be notified of the board's decision
**So that** I know my application status

**Priority:** P0
**Points:** 2

**Acceptance Criteria:**
- Email sent when board decision is recorded
- Different templates for: Approved, Rejected, Deferred
- Approved email includes next steps (agreements to sign)
- Triggered by `ApplicantBoardDecisionMade` domain event

---

### US-033: Stage Change Email
**As an** applicant
**I want to** be notified when my status changes
**So that** I stay informed about my progress

**Priority:** P1
**Points:** 2

**Acceptance Criteria:**
- Email sent on key stage transitions
- Stages that trigger email: Searching, UnderContract, Closed
- Brief status update with relevant next steps
- Can be disabled per-applicant (future enhancement)

---

## Epic: Dashboard (~8 points)

### US-034: Dashboard API Endpoints
**As a** coordinator
**I want to** see key metrics via API
**So that** the dashboard can display statistics

**Priority:** P0
**Points:** 3

**Acceptance Criteria:**
- `GET /api/dashboard/stats` returns:
  - Total applicants count
  - Applicants by board decision (Pending, Approved, Rejected, Deferred)
  - Applicants by stage (Submitted, AwaitingAgreements, Searching, UnderContract, Closed)
  - Total properties count
  - Properties by status
- `GET /api/dashboard/recent-activity` returns last 10 activities

**Response Example:**
```json
{
  "applicants": {
    "total": 45,
    "byBoardDecision": {
      "pending": 12,
      "approved": 28,
      "rejected": 3,
      "deferred": 2
    },
    "byStage": {
      "submitted": 12,
      "awaitingAgreements": 5,
      "searching": 18,
      "underContract": 3,
      "closed": 7
    }
  },
  "properties": {
    "total": 32,
    "byStatus": {
      "active": 25,
      "underContract": 4,
      "sold": 3
    }
  }
}
```

---

### US-035: Dashboard Page (Frontend)
**As a** coordinator
**I want to** see an overview dashboard
**So that** I can quickly understand system status

**Priority:** P0
**Points:** 5

**Acceptance Criteria:**
- Stat cards showing key metrics
- Pie/donut chart for applicants by stage
- Bar chart for board decisions
- Recent activity list (last 10 items)
- Quick links to common actions
- Responsive layout with Ant Design cards

**Layout:**
```
+------------------+------------------+------------------+
|  Total Applicants|  Active Searching|  Under Contract  |
|       45         |        18        |         3        |
+------------------+------------------+------------------+
|                                     |                  |
|  Applicants by Stage (Pie Chart)   |  Recent Activity |
|                                     |  - Activity 1    |
|                                     |  - Activity 2    |
+-------------------------------------+------------------+
```

---

## Epic: Activity Tracking (~3 points)

### US-036: Activity Log Entity and API
**As a** coordinator
**I want to** track all activities in the system
**So that** I can see a history of actions

**Priority:** P1
**Points:** 3

**Acceptance Criteria:**
- `ActivityLog` entity: EntityType, EntityId, Action, Description, UserId, Timestamp
- Auto-log on: Applicant created, Board decision, Stage change, Property created
- `GET /api/activities` with filters (entityType, entityId, dateRange)
- `GET /api/applicants/{id}/activities` returns activities for specific applicant
- Stored separately from audit log (user-facing vs system-facing)

**Technical Notes:**
```csharp
public class ActivityLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; }  // "Applicant", "Property"
    public Guid EntityId { get; set; }
    public string Action { get; set; }       // "Created", "BoardDecision", "StageChanged"
    public string Description { get; set; }  // Human-readable description
    public Guid? UserId { get; set; }
    public DateTime Timestamp { get; set; }
}
```

---

## Summary

| Epic | Stories | Points |
|------|---------|--------|
| Property Management | US-024 to US-029 | 21 |
| Email Notifications | US-030 to US-033 | 8 |
| Dashboard | US-034 to US-035 | 8 |
| Activity Tracking | US-036 | 3 |
| **Total** | **13 stories** | **40** |

---

## Implementation Order

1. **Property Management Backend** (US-024, US-025, US-026) - Foundation
2. **Property Management Frontend** (US-027, US-028) - UI
3. **Activity Tracking** (US-036) - Needed for dashboard
4. **Dashboard** (US-034, US-035) - Uses activity data
5. **Email Notifications** (US-030, US-031, US-032, US-033) - Can run in parallel
6. **Property Photos** (US-029) - Enhancement

