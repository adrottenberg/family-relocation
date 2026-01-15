# MASTER REQUIREMENTS - REQUIRED UPDATES
## Apply These 32 Corrections to MASTER_REQUIREMENTS_FINAL.md

**Current File:** MASTER_REQUIREMENTS_FINAL.md (1,328 lines)  
**Status:** ❌ Outdated - Missing 32 corrections  
**Action Required:** Apply changes below  

---

## 🔄 GLOBAL FIND & REPLACE

### 1. ShabbosLocation → ShabbosShul (Correction #5)

**Find:** `ShabbosLocation`  
**Replace with:** `ShabbosShul`  

**Locations:**
- Line 576: Applicant entity properties
- Any mentions in text descriptions

**Example:**
```diff
- ShabbosLocation
+ ShabbosShul
```

---

## ✏️ ENUM UPDATES

### 2. InterestLevel - Add SomewhatInterested (Correction #3)

**Find:** Section with InterestLevel enum  
**Current:**
```csharp
public enum InterestLevel
{
    VeryInterested,
    Neutral,
    NotVeryInterested,
    NotInterested
}
```

**Replace with:**
```csharp
public enum InterestLevel
{
    VeryInterested,
    SomewhatInterested,     // ← NEW
    Neutral,
    NotVeryInterested,
    NotInterested
}
```

---

### 3. MoveTimeline - Add Never (Correction #4)

**Find:** MoveTimeline enum  
**Add at end:**
```csharp
Never                // For investors, not relocating
```

**Full updated enum:**
```csharp
public enum MoveTimeline
{
    Immediate,           // < 3 months
    ShortTerm,           // 3-6 months
    MediumTerm,          // 6-12 months
    LongTerm,            // 1-2 years
    Extended,            // 2+ years
    Flexible,            // Whenever right property found
    NotSure,             // Haven't decided
    Never                // ← NEW - Investors, not relocating
}
```

---

### 4. ListingStatus - Remove UnderContractThroughUs (Correction #10)

**Find:** ListingStatus enum  
**Current:**
```csharp
public enum ListingStatus
{
    Active,
    UnderContract,
    UnderContractThroughUs,
    Sold,
    OffMarket
}
```

**Replace with:**
```csharp
public enum ListingStatus
{
    Active,
    UnderContract,      // Use for ALL contracts
    Sold,
    OffMarket
}
```

**Note:** Track "our" contracts via Application.ContractPropertyId instead

---

### 5. HouseType - Add New Enum (Correction #6)

**Find:** Section with Property enums  
**Add this new enum:**

```csharp
/// <summary>
/// Type of house structure
/// </summary>
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

---

### 6. ActivityType - Expand (Correction #1)

**Find:** ActivityType enum  
**Add these new values:**

```csharp
PhoneCall,           // Inbound or outbound call
TextMessage,         // SMS sent/received
Meeting,             // In-person meeting
ShowingScheduled,    // Showing scheduled
ShowingCompleted,    // Showing completed
DocumentUploaded,    // Document uploaded
DocumentSigned,      // Agreement signed
ReminderCreated,     // Follow-up reminder created
ReminderCompleted    // Follow-up completed
```

---

### 7. EmailDeliveryStatus - Add New Enum (Correction #14)

**Add this completely new enum:**

```csharp
/// <summary>
/// Email delivery and engagement tracking
/// For email blast system
/// </summary>
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

---

## 🗑️ REMOVE SECTIONS

### 8. Remove Neighborhood Enum (Correction #12)

**Find and DELETE:**
```csharp
public enum Neighborhood
{
    Union_BattleHill,
    Union_Connecticut_Farms,
    RosellePark_Central,
    // ... etc
}
```

**Replace mentions with:** "City (Union or Roselle Park)"

---

## ➕ ADD NEW ENTITIES

### 9. Add OpenHouse Entity (Correction #8)

**Add to Entities section:**

```csharp
/// <summary>
/// Open house schedule for properties
/// </summary>
public class OpenHouse : Entity<Guid>
{
    public Guid OpenHouseId { get; set; }
    public Guid PropertyId { get; set; }
    
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    
    public string Notes { get; set; }
    
    public bool IsCancelled { get; set; }
    public string CancellationReason { get; set; }
    
    // Navigation
    public virtual Property Property { get; set; }
    
    // Validation: No Shabbos or Yom Tov
}
```

**Add to Property entity:**
```csharp
public virtual ICollection<OpenHouse> OpenHouses { get; set; }
```

---

### 10. Add Email Blast Entities (Correction #14)

**Add three new entities:**

```csharp
/// <summary>
/// Email marketing contact (not yet prospect/applicant)
/// </summary>
public class EmailContact : Entity<Guid>
{
    public Guid EmailContactId { get; set; }
    public Email EmailAddress { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Source { get; set; }
    
    // Lifecycle
    public DateTime AddedDate { get; set; }
    public bool IsSubscribed { get; set; }
    public DateTime? UnsubscribedDate { get; set; }
    
    // Link when converted
    public Guid? ProspectId { get; set; }
    public Guid? ApplicantId { get; set; }
    public DateTime? ConvertedDate { get; set; }
    
    // Navigation
    public virtual ICollection<EmailBlastRecipient> EmailsReceived { get; set; }
}

/// <summary>
/// Email blast campaign
/// </summary>
public class EmailBlast : Entity<Guid>
{
    public Guid EmailBlastId { get; set; }
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
    
    public DateTime SentDate { get; set; }
    public Guid SentByUserId { get; set; }
    
    // Statistics
    public int TotalSent { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    
    // Navigation
    public virtual ICollection<EmailBlastRecipient> Recipients { get; set; }
}

/// <summary>
/// Individual recipient tracking
/// </summary>
public class EmailBlastRecipient : Entity<Guid>
{
    public Guid RecipientId { get; set; }
    public Guid EmailBlastId { get; set; }
    public Guid EmailContactId { get; set; }
    
    public string MessageId { get; set; }  // SES message ID
    
    // Tracking
    public EmailDeliveryStatus Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? FirstOpenedAt { get; set; }
    public int OpenCount { get; set; }
    public DateTime? FirstClickedAt { get; set; }
    public int ClickCount { get; set; }
    
    // Navigation
    public virtual EmailBlast EmailBlast { get; set; }
    public virtual EmailContact Contact { get; set; }
}
```

---

## 📝 UPDATE ENTITY PROPERTIES

### 11. Applicant Entity Updates

**Find Applicant entity, update these properties:**

```diff
- public List<Neighborhood> PreferredNeighborhoods { get; set; }
+ public List<string> PreferredCities { get; set; }  // Union, Roselle Park
```

**Note in description:** "Removed Neighborhood enum, use City strings instead (Correction #12)"

---

### 12. Property Entity Updates

**Add to Property entity:**

```csharp
public HouseType? HouseType { get; set; }  // Colonial, Cape Cod, etc.
```

**Change:**
```diff
- public Neighborhood Neighborhood { get; set; }
+ public string City { get; set; }  // Union or Roselle Park
```

---

## 📊 UPDATE MATCHING ALGORITHM

### 13. Property Matching Algorithm

**Find the property matching algorithm section**  

**Update City/Neighborhood scoring:**

```diff
- // 4. Neighborhood (20 points)
- if (applicant.PreferredNeighborhoods.Any())
- {
-     var propertyNeighborhood = DetermineNeighborhood(property.Address);
-     if (applicant.PreferredNeighborhoods.Contains(propertyNeighborhood))
-     {
-         score += 20;
-     }
- }

+ // 4. City (20 points)
+ if (applicant.PreferredCities.Any())
+ {
+     if (applicant.PreferredCities.Contains(property.City))
+     {
+         score += 20;
+     }
+ }
```

---

## 🆕 ADD NEW SECTIONS

### 14. Shul Walking Distance (Correction #2)

**Add new subsection under "Shul Proximity":**

```markdown
#### Walking Distance Calculation

The system calculates BOTH straight-line distance AND walking distance:

**Straight-Line Distance (Haversine):**
- Fast calculation using GPS coordinates
- "As the crow flies"
- Used for initial filtering

**Walking Distance (MapBox API):**
- Actual walking route via streets
- Includes walking time in minutes
- More accurate for real-world proximity
- Cost: MapBox free tier = 50k requests/month

**Display:**
"0.4 miles straight line, 0.6 miles walking (12 min)"

**Matching Algorithm:**
Uses walking distance if available, falls back to straight-line.
```

---

### 15. Shul Seed Data (Correction #13)

**Update Shul section with addresses:**

```markdown
#### Initial Shuls (Seed Data)

1. **Bobov**
   - Address: 212 New Jersey Ave, Elizabeth, NJ 07202
   - Coordinates: (to be geocoded)

2. **Nassad**
   - Address: 433 Bailey Avenue, Elizabeth, NJ 07208
   - Coordinates: (to be geocoded)

3. **Yismach Yisroel**
   - Address: 547 Salem Road, Union, NJ 07083
   - Coordinates: (to be geocoded)
```

---

### 16. Reminders Dashboard (Correction #16)

**Add new section:**

```markdown
### Follow-Up Reminders Dashboard

Coordinators need to see all due reminders in one view.

**Features:**
- View all open reminders
- Filter by: Overdue, Due Today, This Week, Assigned To Me
- Sort by: Priority, Due Date
- Quick actions: Complete, Snooze
- Print view for paper workflow

**Screen View:**
- Shows reminder with clickable link to applicant/prospect
- Summary stats: Overdue count, Due Today count
- Colored highlighting (red=overdue, yellow=today)

**Print View:**
- All contact information visible (name, phone, email)
- No links (printer-friendly)
- Checkboxes for manual completion tracking
- Space for handwritten notes
```

---

### 17. Email Blast System (Correction #14) - Phase 3

**Add new section:**

```markdown
### Email Marketing Blast System (Phase 3)

Send marketing emails to contacts who haven't applied yet.

**Features:**
- Maintain email list (separate from prospects)
- Create blast campaigns (HTML email editor)
- Track engagement:
  - Delivery confirmation
  - Opens (tracking pixel)
  - Clicks (wrapped links)
  - Bounces
  - Spam complaints
- Auto-link email history when contact applies
- CAN-SPAM compliance (unsubscribe, physical address)

**Implementation:**
- AWS SES for sending ($0.10 per 1,000 emails)
- AWS SNS for event tracking
- Email templates in HTML

**Use Cases:**
- Community newsletters
- Event announcements
- Property highlights
- Move-in support tips
```

---

### 18. SMS Notifications (Correction #11) - Phase 2

**Add note in Email Notifications section:**

```markdown
#### SMS Notifications (Optional - Phase 2)

Some families use feature phones (not smartphones).

**Features:**
- Opt-in SMS notifications
- AWS SNS for sending
- Use cases:
  - Showing reminders: "Showing at 123 Main St today at 3pm"
  - Application updates: "Your application has been approved!"
  - Contract updates: "Congratulations! Your offer was accepted"

**Cost:** $0.00645 per SMS (very affordable)

**Fields Added to Applicant:**
```csharp
public bool AllowSmsNotifications { get; set; }
public PhoneNumber SmsPhoneNumber { get; set; }
```

---

### 19. Default Broker (Correction #9)

**Update Broker section:**

```markdown
#### Default Broker Support

Currently working with one primary broker.

**Features:**
- Mark one broker as "IsDefault"
- Auto-assign default broker when application approved
- Can override per application if needed
- System settings page to change default

**Simplified Broker Entity:**
- Removed: LicenseNumber, TaxId, commission tracking (can add later)
- Kept: Basic contact info, IsDefault flag
```

---

## 📋 UPDATED ENTITY COUNT

**Update entity count in overview:**

```diff
- 12 entities
+ 15 entities

New entities:
- OpenHouse
- EmailContact
- EmailBlast
- EmailBlastRecipient
```

**Update enum count:**

```diff
- X enums
+ Add: HouseType, EmailDeliveryStatus
+ Remove: Neighborhood
```

---

## ✅ VERIFICATION CHECKLIST

After applying all changes, verify:

- [ ] ShabbosLocation → ShabbosShul (all occurrences)
- [ ] InterestLevel has SomewhatInterested
- [ ] MoveTimeline has Never
- [ ] ListingStatus removed UnderContractThroughUs
- [ ] HouseType enum added
- [ ] ActivityType expanded with 9 new types
- [ ] EmailDeliveryStatus enum added
- [ ] Neighborhood enum removed
- [ ] Applicant.PreferredNeighborhoods → PreferredCities
- [ ] Property has HouseType property
- [ ] Property.Neighborhood → City
- [ ] OpenHouse entity added
- [ ] EmailContact entity added
- [ ] EmailBlast entity added
- [ ] EmailBlastRecipient entity added
- [ ] Walking distance section added
- [ ] Shul addresses added
- [ ] Reminders dashboard section added
- [ ] Email blast system section added
- [ ] SMS notifications noted (Phase 2)
- [ ] Default broker noted
- [ ] Property matching algorithm updated (City not Neighborhood)
- [ ] Entity count updated (12 → 15)

---

## 🎯 PRIORITY ORDER

If updating incrementally, do in this order:

**Priority 1 (P0 - Needed for Sprint 1):**
1. ShabbosLocation → ShabbosShul
2. InterestLevel.SomewhatInterested
3. MoveTimeline.Never
4. Remove Neighborhood, use City
5. Remove UnderContractThroughUs
6. ActivityType expansion
7. Shul addresses

**Priority 2 (P1 - Needed for Sprint 2-3):**
8. HouseType enum
9. OpenHouse entity
10. Reminders dashboard
11. Walking distance

**Priority 3 (P2 - Phase 3 features):**
12. Email blast entities
13. EmailDeliveryStatus enum
14. SMS notifications note
15. Default broker note

---

## 📄 ALTERNATIVE: USE UPDATED VERSION

Rather than manually applying all changes, you can:

**Option A:** I can create a complete new MASTER_REQUIREMENTS_v4.md with all corrections  
**Option B:** Use the correction documents as supplements to the original

**Recommendation:** Keep MASTER_REQUIREMENTS_FINAL.md as reference, use:
- FINAL_CORRECTIONS_JAN_2026.md for detailed corrections
- SOLUTION_STRUCTURE_AND_CODE_v3.md for code with corrections
- This document for specific changes needed

---

END OF UPDATE GUIDE
