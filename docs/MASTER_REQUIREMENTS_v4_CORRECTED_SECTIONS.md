# MASTER REQUIREMENTS v4 - CORRECTED SECTIONS
## Use This With MASTER_REQUIREMENTS_FINAL.md

**Purpose:** This document contains ONLY the corrected sections that differ from MASTER_REQUIREMENTS_FINAL.md. Use this alongside the original document.

**How to Use:**
1. Read MASTER_REQUIREMENTS_FINAL.md for full requirements
2. When you hit a section listed below, use the CORRECTED VERSION from this file
3. Or, apply the find/replace changes listed in MASTER_REQUIREMENTS_UPDATES.md

---

## ✅ GLOBAL CORRECTIONS APPLIED

### Correction #5: ShabbosLocation → ShabbosShul
**Find in original:** `ShabbosLocation`  
**Replace with:** `ShabbosShul`  

**Affects:**
- Applicant entity property name
- All references in forms and UI
- Database column name

---

### Correction #12: Neighborhood → City  
**Removed:** Neighborhood enum entirely  
**Replaced with:** City as string (Union or Roselle Park)

**Affects:**
- Applicant.PreferredNeighborhoods → Applicant.PreferredCities
- Property.Neighborhood → Property.City
- Matching algorithm scoring

---

## 📝 CORRECTED SECTIONS

### Section 5.1: Public Application Form (CORRECTED)

**Section 4: Current Situation**
```
- Current Address (Street, City, State, Zip)
- Current Kehila/Community
- ShabbosShul (where they daven on Shabbos) ← CORRECTED
```

**Section 5: Housing Preferences**
```
- Preferred Cities (multi-select checkboxes: Union, Roselle Park) ← CORRECTED
- Move Timeline dropdown: ← CORRECTED - Added "Never"
  * Immediate (< 3 months)
  * Short Term (3-6 months)
  * Medium Term (6-12 months)
  * Long Term (1-2 years)
  * Extended (2+ years)
  * Flexible
  * Not Sure
  * Never (for investors) ← NEW
```

---

### Section 5.2.3: View Applicant Details (CORRECTED)

**Overview Tab - Community Section:**
```markdown
- Current Kehila
- ShabbosShul ← CORRECTED (was ShabbosLocation)
```

**Housing Preferences Tab:**
```markdown
- Preferred Cities (Union, Roselle Park) ← CORRECTED (was Neighborhoods)
- Move Timeline ← Now includes "Never" option
```

---

### Section 5.2.6: Update Housing Preferences (CORRECTED)

**Form Fields:**
```markdown
- Preferred Cities (multi-select checkboxes: Union, Roselle Park) ← CORRECTED
  * Union
  * Roselle Park
  
  Note: Removed Neighborhood enum. Now using simple city selection.

- Move Timeline (dropdown): ← CORRECTED
  * Immediate (< 3 months)
  * Short Term (3-6 months)
  * Medium Term (6-12 months)
  * Long Term (1-2 years)
  * Extended (2+ years)
  * Flexible
  * Not Sure
  * Never (for investors) ← NEW
```

---

### Section 5.4.1: Create Property (CORRECTED)

**Form Fields:**
```markdown
- House Type (dropdown): ← NEW (Correction #6)
  * Colonial
  * Cape Cod
  * Flat (ranch)
  * Split Level
  * Bi-Level
  * Townhouse
  * Duplex
  * Condo
  * Victorian
  * Contemporary
  * Other

- City (text: Union or Roselle Park) ← CORRECTED (was Neighborhood enum)
```

---

### Section 5.4.4: Update Property Status (CORRECTED)

**Status Dropdown:**
```markdown
- Active
- Under Contract ← CORRECTED (removed "UnderContractThroughUs")
- Sold
- Off Market

Note: Track "our contracts" via Application.ContractPropertyId instead of separate status.
```

---

### NEW Section 5.4.5: Open House Scheduling

**Purpose:** Track open houses for properties ← NEW (Correction #8)

**Requirements:**

1. **Schedule Open House:**
   - Button on property details: "Schedule Open House"
   - Modal form:
     * Start Date/Time (datetime picker)
     * End Date/Time (datetime picker)
     * Notes (textarea, optional)
   
   - **Validation:**
     * Cannot be Saturday (Shabbos)
     * Cannot be Friday after 4pm (Shabbos)
     * Cannot be on Yom Tov (future: integrate calendar)
   
   - **Save:**
     * Creates OpenHouse entity
     * Links to Property

2. **View Open Houses:**
   - List on property details page
   - Shows: Date/Time, Status (Scheduled/Cancelled)
   - Upcoming highlighted

3. **Cancel Open House:**
   - "Cancel" button per open house
   - Requires: Cancellation Reason
   - Sets IsCancelled = true

**OpenHouse Entity:**
```csharp
public class OpenHouse : Entity<Guid>
{
    public Guid OpenHouseId { get; set; }
    public Guid PropertyId { get; set; }
    
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    
    public string Notes { get; set; }
    
    public bool IsCancelled { get; set; }
    public string CancellationReason { get; set; }
    
    public virtual Property Property { get; set; }
}
```

---

### Section 5.5.1: Property Matching Algorithm (CORRECTED)

**4. City Match (20 points max)** ← CORRECTED
```
IF applicant.PreferredCities.Contains(property.City):
    score = 20
ELSE:
    score = 0
```

**6. Shul Proximity Bonus (up to 10 points)** ← UPDATED (Correction #2 - Walking Distance)

```
FOR EACH shul IN shuls:
    // Try walking distance first (MapBox API)
    walkingDistance = CalculateWalkingDistance(property.Coordinates, shul.Coordinates)
    
    IF walkingDistance IS NULL:
        // Fallback to straight-line distance (Haversine)
        walkingDistance = CalculateStraightLineDistance(property.Coordinates, shul.Coordinates)
    
    IF walkingDistance < nearestDistance:
        nearestDistance = walkingDistance

IF nearestDistance <= applicant.ShulProximity.MaxDistanceMiles:
    score = 10
ELSE IF nearestDistance <= applicant.ShulProximity.MaxDistanceMiles * 1.2:
    score = 5
ELSE:
    score = 0
```

---

### NEW Section 5.5.2: Walking Distance Calculation

**MapBox Integration:** ← NEW (Correction #2)

**Purpose:** Calculate actual walking distance via streets (more accurate than straight-line)

**Implementation:**
```csharp
public interface IDistanceCalculationService
{
    Task<double> CalculateStraightLineDistance(Coordinates from, Coordinates to);
    Task<(double miles, int minutes)?> CalculateWalkingDistance(Coordinates from, Coordinates to);
}

public class MapBoxDistanceService : IDistanceCalculationService
{
    public async Task<(double miles, int minutes)?> CalculateWalkingDistance(
        Coordinates from, 
        Coordinates to)
    {
        var url = $"https://api.mapbox.com/directions/v5/mapbox/walking/" +
                  $"{from.Longitude},{from.Latitude};{to.Longitude},{to.Latitude}" +
                  $"?access_token={_apiKey}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;  // Fallback to straight-line

        var json = await response.Content.ReadFromJsonAsync<MapBoxDirectionsResponse>();
        var route = json.Routes.FirstOrDefault();
        if (route == null)
            return null;

        var miles = route.Distance * 0.000621371;  // meters to miles
        var minutes = (int)(route.Duration / 60);   // seconds to minutes

        return (miles, minutes);
    }
}
```

**Display:**
```
Shul Proximity:
  Bobov: 0.4 mi straight-line, 0.6 mi walking (12 min)
  Nassad: 0.8 mi straight-line, 1.1 mi walking (22 min)
```

**Cost:** MapBox free tier = 50,000 requests/month

---

### Section 5.6.1: Activity Types (CORRECTED)

**Expanded List:** ← UPDATED (Correction #1)

```
- Note (general note)
- Email Sent
- Email Received
- Phone Call (inbound or outbound) ← NEW
- Text Message (SMS sent/received) ← NEW
- Meeting (in-person) ← NEW
- Showing Scheduled ← NEW
- Showing Completed ← NEW
- Document Uploaded ← NEW
- Document Signed ← NEW
- Stage Change
- Status Change
- Reminder Created ← NEW
- Reminder Completed ← NEW
```

---

### NEW Section 5.7.2: Reminders Dashboard

**Purpose:** Centralized view of all due reminders ← UPDATED (Correction #16)

**Display:**

1. **Stats Cards:**
   - Overdue (red count)
   - Due Today (yellow count)
   - Total Open

2. **Filters:**
   - Dropdown: Overdue, Due Today, Due This Week, Due This Month, All
   - Checkbox: "My Reminders Only"

3. **Table Columns:**
   - Priority (icon with color)
   - Due Date (with "X days overdue" if late)
   - Task (title + notes preview)
   - Contact (clickable link to applicant/prospect)
   - Contact Phone
   - Assigned To
   - Actions (Complete, Snooze)

4. **Row Styling:**
   - Red background: Overdue
   - Yellow background: Due today

5. **Actions:**
   - **Complete:** Modal for completion notes, sets Status=Completed
   - **Snooze:** Modal for new date + reason, updates DueDate

**NEW: Print View**

**Trigger:** "Print Task List" button

**Features:**
- Printer-friendly page (auto-triggers print dialog)
- **Header:** Date generated, filter used, total count
- **Table:** Priority, Due Date, Task, Contact Name, Phone, Email
- **No Links:** All info visible for paper
- **Overdue Marked:** ⚠️ symbol
- **Bottom Space:** For handwritten notes
- **Checkbox List:** Manual completion tracking
- **Optimized:** Black & white, 12pt font, 8.5×11 paper

---

### NEW Section 5.9: Email Marketing Blasts (Phase 3)

**Purpose:** Send marketing emails to contacts who haven't applied yet ← NEW (Correction #14)

#### 5.9.1 Email Contact Management

**EmailContact Entity:**
- Email address
- First name, Last name
- Source (how we got them)
- Subscription status
- Auto-links to Applicant when they apply

**Features:**
- Import CSV of emails
- Add manually
- Track subscriptions/unsubscribes
- Filter by source

#### 5.9.2 Create Email Blast

**Form:**
- Subject line
- HTML body (WYSIWYG editor)
- Recipients selection:
  * All subscribed
  * Filter by source
  * Exclude converters (already applicants)

**Send:**
- Queue via AWS SES
- Track delivery, opens, clicks
- Unsubscribe link (CAN-SPAM compliance)

#### 5.9.3 Email Tracking

**Track via AWS SES + SNS:**
- ✅ Delivery confirmation
- ✅ Opens (tracking pixel)
- ✅ Clicks (wrapped links)
- ✅ Bounces
- ✅ Spam complaints

**EmailBlastRecipient tracks:**
- Sent timestamp
- First opened, open count
- First clicked, click count
- Delivery status

#### 5.9.4 Auto-Link Email History

**When contact applies:**
1. Find existing EmailContact by email
2. Link EmailContact.ApplicantId = new applicant
3. Set ConvertedDate
4. Create Activity:
   ```
   "Received 5 marketing emails before applying.
    Last opened: Dec 15, 2025"
   ```

**Cost:** $0.10 per 1,000 emails

---

### NEW Section 5.10: SMS Notifications (Optional - Phase 2)

**Purpose:** Text notifications for feature phones ← NEW (Correction #11)

**Requirements:**

1. **Opt-In:**
   - Checkbox: "Allow SMS notifications"
   - Select phone number for SMS

2. **SMS Types:**
   - Showing reminders
   - Application updates
   - Contract updates

3. **Implementation:**
   - AWS SNS
   - Cost: $0.00645 per SMS

4. **Applicant Fields:**
   ```csharp
   public bool AllowSmsNotifications { get; set; }
   public PhoneNumber SmsPhoneNumber { get; set; }
   ```

---

## 📊 CORRECTED DOMAIN MODEL

### Section 6.1: Entities

**Total: 15 entities** (12 original + 3 email blast)

#### Applicant (CORRECTED)
```csharp
// CORRECTED properties:
public string ShabbosShul { get; set; }  // was ShabbosLocation
public List<string> PreferredCities { get; set; }  // was PreferredNeighborhoods
public MoveTimeline? MoveTimeline { get; set; }  // now includes Never

// NEW (Phase 2):
public bool AllowSmsNotifications { get; set; }
public PhoneNumber SmsPhoneNumber { get; set; }
```

#### Property (CORRECTED)
```csharp
// NEW:
public HouseType? HouseType { get; set; }  // Colonial, Cape Cod, etc.

// CORRECTED:
public string City { get; set; }  // was Neighborhood enum

// CORRECTED:
public ListingStatus Status { get; set; }  // removed UnderContractThroughUs

// NEW:
public virtual ICollection<OpenHouse> OpenHouses { get; set; }
```

#### NEW: OpenHouse Entity
```csharp
public class OpenHouse : Entity<Guid>
{
    public Guid OpenHouseId { get; set; }
    public Guid PropertyId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Notes { get; set; }
    public bool IsCancelled { get; set; }
    public string CancellationReason { get; set; }
    public virtual Property Property { get; set; }
}
```

#### NEW: Email Blast Entities

**EmailContact:**
```csharp
public class EmailContact : Entity<Guid>
{
    public Guid EmailContactId { get; set; }
    public Email EmailAddress { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Source { get; set; }
    public DateTime AddedDate { get; set; }
    public bool IsSubscribed { get; set; }
    public DateTime? UnsubscribedDate { get; set; }
    public Guid? ApplicantId { get; set; }  // When converted
    public DateTime? ConvertedDate { get; set; }
    public virtual ICollection<EmailBlastRecipient> EmailsReceived { get; set; }
}
```

**EmailBlast:**
```csharp
public class EmailBlast : Entity<Guid>
{
    public Guid EmailBlastId { get; set; }
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
    public DateTime SentDate { get; set; }
    public Guid SentByUserId { get; set; }
    public int TotalSent { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    public virtual ICollection<EmailBlastRecipient> Recipients { get; set; }
}
```

**EmailBlastRecipient:**
```csharp
public class EmailBlastRecipient : Entity<Guid>
{
    public Guid RecipientId { get; set; }
    public Guid EmailBlastId { get; set; }
    public Guid EmailContactId { get; set; }
    public string MessageId { get; set; }
    public EmailDeliveryStatus Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? FirstOpenedAt { get; set; }
    public int OpenCount { get; set; }
    public DateTime? FirstClickedAt { get; set; }
    public int ClickCount { get; set; }
    public virtual EmailBlast EmailBlast { get; set; }
    public virtual EmailContact Contact { get; set; }
}
```

#### Activity (CORRECTED)
```csharp
// NEW properties for expanded activity types:
public string CallDirection { get; set; }  // Inbound/Outbound
public int? CallDurationMinutes { get; set; }
public string MeetingLocation { get; set; }
public string Attendees { get; set; }
```

#### Prospect (CORRECTED)
```csharp
// CORRECTED:
public InterestLevel InterestLevel { get; set; }  // now includes SomewhatInterested
public MoveTimeline? MoveTimeline { get; set; }  // now includes Never
```

#### Broker (CORRECTED - Simplified)
```csharp
// NEW:
public bool IsDefault { get; set; }  // Mark default broker

// REMOVED (for Phase 1):
// - LicenseNumber
// - TaxId
// - Commission tracking (handled in Application)
```

#### Shul (CORRECTED - With Seed Data)
```csharp
// Seed data:
1. Bobov - 212 New Jersey Ave, Elizabeth, NJ 07202
2. Nassad - 433 Bailey Avenue, Elizabeth, NJ 07208
3. Yismach Yisroel - 547 Salem Road, Union, NJ 07083
```

---

### Section 6.3: Enums (CORRECTED)

#### InterestLevel (CORRECTED)
```csharp
public enum InterestLevel
{
    VeryInterested,
    SomewhatInterested,     // ← NEW (Correction #3)
    Neutral,
    NotVeryInterested,
    NotInterested
}
```

#### MoveTimeline (CORRECTED)
```csharp
public enum MoveTimeline
{
    Immediate,
    ShortTerm,
    MediumTerm,
    LongTerm,
    Extended,
    Flexible,
    NotSure,
    Never                // ← NEW (Correction #4) - For investors
}
```

#### ListingStatus (CORRECTED)
```csharp
public enum ListingStatus
{
    Active,
    UnderContract,      // ← Use for ALL contracts (removed UnderContractThroughUs)
    Sold,
    OffMarket
}
```

#### HouseType (NEW)
```csharp
public enum HouseType
{
    Colonial,
    CapeCod,
    Flat,              // Single-level ranch
    SplitLevel,
    BiLevel,
    Townhouse,
    Duplex,
    Condo,
    Victorian,
    Contemporary,
    Other
}
```

#### ActivityType (CORRECTED - Expanded)
```csharp
public enum ActivityType
{
    // Original:
    Note,
    EmailSent,
    EmailReceived,
    StageChange,
    StatusChange,
    
    // NEW (Correction #1):
    PhoneCall,
    TextMessage,
    Meeting,
    ShowingScheduled,
    ShowingCompleted,
    DocumentUploaded,
    DocumentSigned,
    ReminderCreated,
    ReminderCompleted
}
```

#### EmailDeliveryStatus (NEW)
```csharp
public enum EmailDeliveryStatus
{
    Pending,
    Sent,
    Delivered,
    Opened,
    Clicked,
    Bounced,
    Complained       // Spam complaint
}
```

#### REMOVED: Neighborhood Enum
```
// This enum has been REMOVED (Correction #12)
// Use City string instead (Union or Roselle Park)
```

---

## 🎯 HOW TO USE THIS DOCUMENT

**Option 1: Reference Both**
- Read MASTER_REQUIREMENTS_FINAL.md for general requirements
- When you hit a corrected section, refer to this document

**Option 2: Mental Find/Replace**
- Read MASTER_REQUIREMENTS_FINAL.md
- Mentally replace:
  * ShabbosLocation → ShabbosShul
  * Neighborhood → City
  * InterestLevel (add SomewhatInterested)
  * MoveTimeline (add Never)
  * ListingStatus (remove UnderContractThroughUs)

**Option 3: Use Code Documents**
- Use **SOLUTION_STRUCTURE_AND_CODE_v3.md** for all code (already corrected)
- Use **SPRINT_1_DETAILED_STORIES.md** for implementation
- Use original requirements only for high-level context

---

**END OF CORRECTED SECTIONS**

For complete details on each correction, see:
- **FINAL_CORRECTIONS_JAN_2026.md** - All 16 corrections explained
- **SOLUTION_STRUCTURE_AND_CODE_v3.md** - Complete code with corrections
- **MASTER_REQUIREMENTS_UPDATES.md** - Step-by-step update guide
