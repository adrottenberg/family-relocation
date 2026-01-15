# FINAL CORRECTIONS - January 2026
## Before Starting Development

**Date:** January 9, 2026  
**Status:** Pre-Development Refinements  
**Total Corrections:** 11 new items  

---

## 🔄 NEW CORRECTIONS

### 1. Add Activity/Interaction Tracking

**Issue:** No system for tracking coordinator interactions with families

**Solution:** Add Activity/Timeline system (partially exists but needs expansion)

**Implementation:**
```csharp
// Already have Activity entity, but expand ActivityType enum:
public enum ActivityType
{
    // Existing
    Note,
    EmailSent,
    EmailReceived,
    StageChange,
    StatusChange,
    
    // ADD THESE:
    PhoneCall,           // Inbound or outbound call
    TextMessage,         // SMS sent/received
    Meeting,             // In-person meeting
    ShowingScheduled,    // Showing scheduled
    ShowingCompleted,    // Showing completed
    DocumentUploaded,    // Document uploaded
    DocumentSigned,      // Agreement signed
    ReminderCreated,     // Follow-up reminder created
    ReminderCompleted    // Follow-up completed
}

// Activity entity includes:
public class Activity : Entity<Guid>
{
    public ActivityType Type { get; set; }
    public EntityType EntityType { get; set; }  // Prospect, Applicant, Application, Property
    public Guid EntityId { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime ActivityDate { get; set; }
    public Guid CreatedBy { get; set; }
    
    // Navigation
    public virtual User CreatedByUser { get; set; }
}
```

**UI:** 
- Timeline view on Applicant/Prospect/Application pages
- Filter by activity type
- Add activity button with type selector
- Shows all interactions in chronological order

**Priority:** P0 (MVP) - Essential for tracking

---

### 2. Add Walking Distance for Shul Proximity

**Issue:** Haversine (straight-line) distance doesn't account for streets/walkability

**Solution:** Support BOTH metrics

**Implementation:**
```csharp
public class ShulProximityResult
{
    public Guid ShulId { get; set; }
    public string ShulName { get; set; }
    
    // Straight-line distance (quick calculation)
    public double StraightLineDistanceMiles { get; set; }
    
    // Walking distance via streets (requires API)
    public double? WalkingDistanceMiles { get; set; }
    public int? WalkingTimeMinutes { get; set; }
    
    // Use walking distance if available, otherwise straight-line
    public double EffectiveDistance => WalkingDistanceMiles ?? StraightLineDistanceMiles;
}

// Integration options:
// Option 1: Google Maps Distance Matrix API (paid, $5/1000 requests)
// Option 2: MapBox Directions API (50k free requests/month)
// Option 3: OpenStreetMap with OSRM (free, self-hosted or public)

// Recommended: MapBox (free tier sufficient)
public interface IDistanceCalculationService
{
    Task<double> CalculateStraightLineDistance(Coordinates from, Coordinates to);
    Task<(double miles, int minutes)> CalculateWalkingDistance(Coordinates from, Coordinates to);
}
```

**Display in UI:**
- Show both metrics: "0.4 miles straight line, 0.6 miles walking (12 min)"
- Use walking distance for matching algorithm if available
- Fall back to straight-line if API unavailable

**Cost:** MapBox free tier = 50k requests/month (plenty for this use case)

**Priority:** P1 (Nice to have, not blocking MVP)

---

### 3. Add "SomewhatInterested" to Interest Level

**Issue:** Prospect.InterestLevel enum missing "SomewhatInterested"

**Correction:**
```csharp
public enum InterestLevel
{
    VeryInterested,
    SomewhatInterested,     // ADD THIS (was missing)
    Neutral,
    NotVeryInterested,
    NotInterested
}
```

**Priority:** P0 (Simple fix before starting)

---

### 4. Add "Never" to MoveTimeline (For Investors)

**Issue:** Investors have no move timeline - they're buying for investment

**Correction:**
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
    Never                // ADD THIS - Investors, not relocating
}
```

**Display:** "Investment Property (Not Relocating)"

**Use Case:** Track investor applicants separately, different matching criteria

**Priority:** P1 (Low priority, few investors)

---

### 5. Rename ShabbosLocation → ShabbosShul

**Issue:** Terminology inconsistency

**Correction:**
```csharp
// Applicant entity
public class Applicant : Entity<Guid>
{
    // OLD:
    // public string ShabbosLocation { get; set; }
    
    // NEW:
    public string ShabbosShul { get; set; }
}

// Same in Application form
// Frontend label: "Where do you daven on Shabbos?"
```

**Priority:** P0 (Simple rename before starting)

---

### 6. Add HouseType + Expand Feature System

**Issue:** Need house type (Colonial, Cape Cod, etc.) + better feature system

**Part A: Add HouseType Enum**
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

// Add to Property entity
public class Property : Entity<Guid>
{
    public PropertyType PropertyType { get; set; }  // Existing: SingleFamily, MultiFamily, etc.
    public HouseType? HouseType { get; set; }       // NEW: Colonial, Cape Cod, etc.
}
```

**Part B: Enhanced Feature System (DEFER TO PHASE 2)**
```csharp
// Current: Simple string list
public List<string> Features { get; set; }

// Future enhancement (Phase 2):
public class PropertyFeature
{
    public string FeatureName { get; set; }     // From predefined list
    public string Value { get; set; }            // Optional: "2-car", "Finished", etc.
    public bool IsNegative { get; set; }         // Exclude listings with this
}

// Admin maintains feature list
public class FeatureDefinition
{
    public string Name { get; set; }             // "Garage", "Basement", "Kitchen"
    public FeatureValueType ValueType { get; set; } // None, Text, Number, Dropdown
    public List<string> AllowedValues { get; set; } // If dropdown
}

// Examples:
// - Garage: Type=Dropdown, Values=["None", "1-car", "2-car", "3-car", "Attached", "Detached"]
// - Basement: Type=Dropdown, Values=["None", "Unfinished", "Partially Finished", "Fully Finished"]
// - Kitchen: Type=Dropdown, Values=["Original", "Updated", "Renovated", "Gourmet"]
```

**Decision:** 
- **Phase 1 (MVP):** Simple string list (current approach)
- **Phase 2:** Enhanced feature system with admin maintenance

**Priority:** HouseType = P1, Feature System = P2

---

### 7. Move Basic Property Management to Phase 1 (MVP)

**Issue:** Need property tracking for contracts, can't wait until Phase 2

**Change:**
- **OLD:** Property Management = Phase 2 (Sprint 5-6)
- **NEW:** Basic Property Management = Phase 1 (Sprint 3-4)

**What moves to Phase 1:**
- ✅ Add Property (basic fields + photos)
- ✅ View Property List
- ✅ View Property Details
- ✅ Update Property Status (Active, Under Contract, Sold)
- ✅ Link properties to applications (contract tracking)

**What stays in Phase 2:**
- Property Matching Algorithm
- Bulk Import
- Advanced filtering
- Monthly payment calculator

**Updated Timeline:**
- Sprint 1-2: Core application flow
- Sprint 3-4: Property tracking + Board review + Email
- Sprint 5+: Property matching, showings, etc.

**Priority:** P0 (MVP) - Must track properties for contracts

---

### 8. Add Open House Schedule to Property

**Issue:** Coordinators schedule open houses, need to track

**Implementation:**
```csharp
// Add to Property entity
public class Property : Entity<Guid>
{
    // ... existing fields
    
    public virtual ICollection<OpenHouse> OpenHouses { get; private set; } = new List<OpenHouse>();
}

// New entity
public class OpenHouse : Entity<Guid>
{
    public Guid OpenHouseId { get; set; }
    public Guid PropertyId { get; set; }
    
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    
    public string Notes { get; set; }  // "Bring ID", "Parking on street", etc.
    
    public bool IsCancelled { get; set; }
    public string CancellationReason { get; set; }
    
    // Navigation
    public virtual Property Property { get; set; }
    
    // Validation
    public void Validate()
    {
        // Exclude Shabbos (Friday sunset to Saturday night)
        // Exclude Yom Tov
        var dayOfWeek = StartDateTime.DayOfWeek;
        
        if (dayOfWeek == DayOfWeek.Saturday)
            throw new DomainException("Cannot schedule open house on Shabbos");
        
        if (dayOfWeek == DayOfWeek.Friday && StartDateTime.Hour >= 16)
            throw new DomainException("Cannot schedule open house Friday after 4pm (Shabbos)");
        
        // TODO: Check against Yom Tov calendar
    }
}
```

**UI Features:**
- Calendar view of open houses
- Filter properties with upcoming open houses
- Show on property details page
- Alert if scheduling on Shabbos/Yom Tov

**Priority:** P1 (Phase 2) - Useful but not blocking MVP

---

### 9. Simplify Broker Tracking + Add Default Broker

**Issue:** Currently working with 1 broker, full tracking might be overkill

**Decision:** Keep Broker entity but simplify

**Implementation:**
```csharp
public class Broker : Entity<Guid>
{
    public Guid BrokerId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    
    public PhoneNumber Phone { get; set; }
    public Email Email { get; set; }
    public string Company { get; set; }
    
    public bool IsDefault { get; set; }  // NEW: Mark default broker
    public bool IsActive { get; set; }
    
    // Simplified - removed:
    // - LicenseNumber
    // - TaxId (for 1099)
    // - TotalCommissionPaid tracking
    
    // Can add these later if needed
}

// Application entity
public class Application : Entity<Guid>
{
    public Guid? AssignedBrokerId { get; set; }  // Nullable, can be null initially
    public virtual Broker AssignedBroker { get; set; }
}

// On application approval, auto-assign default broker
public void MoveToApprovedStage(Guid modifiedBy)
{
    Stage = ApplicationStage.Approved;
    
    // Auto-assign default broker if not assigned
    if (!AssignedBrokerId.HasValue)
    {
        var defaultBroker = _brokerRepository.GetDefault();
        if (defaultBroker != null)
        {
            AssignedBrokerId = defaultBroker.BrokerId;
        }
    }
    
    ModifiedBy = modifiedBy;
    ModifiedDate = DateTime.UtcNow;
}
```

**Admin Settings:**
- System Settings page
- Set default broker (dropdown)
- Can override per application if needed

**Priority:** P1 (Phase 2) - Can hardcode broker initially, add UI later

---

### 10. Remove UnderContractThroughUs Status

**Issue:** Redundant with existing tracking

**Correction:**
```csharp
// ListingStatus enum - REMOVE THIS:
public enum ListingStatus
{
    Active,
    UnderContract,
    UnderContractThroughUs,  // ❌ REMOVE - Redundant
    Sold,
    OffMarket
}

// NEW:
public enum ListingStatus
{
    Active,
    UnderContract,  // Use this for ALL contracts
    Sold,
    OffMarket
}

// Track "through us" separately on Application
public class Application : Entity<Guid>
{
    // If this application has contract on a property:
    public Guid? ContractPropertyId { get; set; }
    
    // This already tells us it's "through us"
}

// To find "our" contracts:
var ourContracts = applications
    .Where(a => a.Stage == ApplicationStage.UnderContract)
    .Select(a => a.ContractPropertyId)
    .Distinct();
```

**Priority:** P0 (Simple fix before starting)

---

### 11. Add SMS Notifications (Optional)

**Issue:** Need SMS for families with feature phones (no smartphones)

**Correction:**
- SMS is needed (not removed)
- But OPTIONAL (not everyone has it)

**Implementation:**
```csharp
// Applicant entity
public class Applicant : Entity<Guid>
{
    // ... existing
    
    // Add SMS preferences
    public bool AllowSmsNotifications { get; set; }  // Opt-in
    public PhoneNumber SmsPhoneNumber { get; set; }  // Which number to use for SMS
}

// AWS SNS for SMS
public interface ISmsService
{
    Task SendSms(string phoneNumber, string message);
}

public class AwsSnsService : ISmsService
{
    // AWS SNS pricing: $0.00645 per SMS (US)
    // Much cheaper than email
    
    public async Task SendSms(string phoneNumber, string message)
    {
        var request = new PublishRequest
        {
            PhoneNumber = phoneNumber,
            Message = message,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                { "AWS.SNS.SMS.SMSType", new MessageAttributeValue 
                    { StringValue = "Transactional", DataType = "String" } }
            }
        };
        
        await _snsClient.PublishAsync(request);
    }
}
```

**Use Cases:**
- Showing reminders: "Showing at 123 Main St today at 3pm"
- Application updates: "Your application has been approved!"
- Contract updates: "Congratulations! Your offer was accepted"

**Cost:** $0.00645/SMS (very affordable for low volume)

**Priority:** P2 (Phase 3) - Nice to have, not blocking

---

### 12. Skip Neighborhoods (Defer)

**Issue:** Neighborhood boundaries complex, not well-defined

**Decision:** Remove Neighborhood enum for now

**Correction:**
```csharp
// Applicant entity - REMOVE:
public List<Neighborhood> PreferredNeighborhoods { get; private set; } = new();

// Property entity - REMOVE:
public Neighborhood Neighborhood { get; set; }

// REPLACE WITH:
// Applicant
public List<string> PreferredCities { get; private set; } = new();  // Just Union, Roselle Park

// Property
public string City { get; set; }  // Union or Roselle Park

// Matching algorithm uses city instead of neighborhood
if (applicant.PreferredCities.Contains(property.City))
{
    score += 20;  // City match
}
```

**Later (if needed):** Can add neighborhood as free-text field, not enum

**Priority:** P0 (Simple fix before starting)

---

### 13. Define Shuls

**Shul List for System:**

```csharp
// Seed data for Shul table
public static class ShulSeeder
{
    public static List<Shul> GetShuls()
    {
        return new List<Shul>
        {
            new Shul
            {
                ShulId = Guid.NewGuid(),
                Name = "Bobov",
                // Address TBD - get from user
                // Coordinates TBD - will geocode
                IsActive = true
            },
            new Shul
            {
                ShulId = Guid.NewGuid(),
                Name = "Nassad",
                // Address TBD
                IsActive = true
            },
            new Shul
            {
                ShulId = Guid.NewGuid(),
                Name = "Yismach Yisroel",
                // Address TBD
                IsActive = true
            }
        };
    }
}
```

**Need from user:**
- Full addresses for each shul
- Will geocode to get coordinates for distance calculation

**Priority:** P0 (Need addresses to start)

---

## 📋 UPDATED PRIORITIES

### Phase 1 (MVP) - 6-8 weeks

**Sprint 1-2: Core Application Flow**
- Domain entities + value objects
- Public application form
- Applicant CRUD
- Basic authentication (Cognito)

**Sprint 3-4: Property Tracking + Board Review**
- ✅ **MOVED TO MVP:** Basic property management (CRUD, status tracking)
- Board review workflow
- Application pipeline (Kanban)
- Email notifications (SES)
- Activity/interaction tracking

**Deliverable:** Can receive applications, track board review, track properties under contract

---

### Phase 2 (Full Features) - 10-12 weeks

**Sprint 5-6:**
- Property matching algorithm
- Shul proximity (with walking distance API)
- Applicant portal (Google OAuth)

**Sprint 7-8:**
- Showing scheduler
- Open house tracking
- Broker assignment (with default)

**Sprint 9-10:**
- Follow-up reminders
- On Hold + Failed Contracts
- Prospect management
- Commission tracking

---

### Phase 3 (Polish) - 3-4 weeks

**Sprint 11-13:**
- Advanced reports
- SMS notifications (optional)
- Bulk property import
- Enhanced feature system
- Performance optimization

---

## 🔄 WHAT NEEDS TO BE UPDATED

### Documents to Update:

1. ✅ **MASTER_REQUIREMENTS_FINAL.md**
   - Add Activity tracking section
   - Add InterestLevel.SomewhatInterested
   - Add MoveTimeline.Never
   - Rename ShabbosLocation → ShabbosShul
   - Add HouseType enum
   - Add OpenHouse entity
   - Remove Neighborhood enum
   - Update Shul list
   - Remove UnderContractThroughUs
   - Add SMS (optional)

2. ✅ **SOLUTION_STRUCTURE_AND_CODE.md**
   - Update value objects
   - Update enums
   - Add OpenHouse entity
   - Update Property entity
   - Update Applicant entity
   - Simplify Broker entity

3. ✅ **PRIORITIZED_USER_STORIES.md**
   - Move basic property management to P0
   - Add open house user stories (P1)
   - Add activity tracking user stories (P0)
   - Add SMS notification stories (P2)

---

## ✅ ACTION ITEMS

**Before starting development:**

1. ✅ Incorporate these 13 corrections into all documents
2. ⏳ Get shul addresses from user:
   - Bobov: [address]
   - Nassad: [address]
   - Yismach Yisroel: [address]
3. ⏳ User confirms final priorities
4. ⏳ Create Jira CSV export with updated stories

---

## 📝 SUMMARY OF CHANGES

**Added:**
- Activity/interaction tracking (P0)
- Walking distance calculation (P1)
- InterestLevel.SomewhatInterested (P0)
- MoveTimeline.Never (P1)
- HouseType enum (P1)
- OpenHouse entity (P1)
- Default broker support (P1)
- SMS notifications - optional (P2)

**Removed:**
- Neighborhood enum (deferred)
- UnderContractThroughUs status (redundant)

**Renamed:**
- ShabbosLocation → ShabbosShul (P0)

**Moved:**
- Basic property management → Phase 1 MVP (P0)

**Simplified:**
- Broker entity (removed complex tracking for now)

**Defined:**
- Shul list: Bobov, Nassad, Yismach Yisroel (need addresses)

---

---

## 14. Email Marketing / Blast System (Phase 3)

**Issue:** Need to send marketing emails to people who haven't applied yet

**Requirements:**
1. Maintain email list (contacts who aren't yet prospects/applicants)
2. Send blast emails to list
3. Track email opens/clicks
4. Auto-link email history when contact becomes prospect/applicant

**Implementation:**

```csharp
// New entity: EmailContact (marketing list)
public class EmailContact : Entity<Guid>
{
    public Guid EmailContactId { get; set; }
    public Email EmailAddress { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Source { get; set; }  // "Website signup", "Referral", etc.
    
    // Lifecycle tracking
    public DateTime AddedDate { get; set; }
    public bool IsSubscribed { get; set; }
    public DateTime? UnsubscribedDate { get; set; }
    
    // Link when they convert
    public Guid? ProspectId { get; set; }  // Set when becomes prospect
    public Guid? ApplicantId { get; set; }  // Set when becomes applicant
    public DateTime? ConvertedDate { get; set; }
    
    // Email history
    public virtual ICollection<EmailBlastRecipient> EmailsReceived { get; set; }
}

// Email blast campaign
public class EmailBlast : Entity<Guid>
{
    public Guid EmailBlastId { get; set; }
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
    public string PlainTextBody { get; set; }
    
    public DateTime SentDate { get; set; }
    public Guid SentByUserId { get; set; }
    
    // Statistics
    public int TotalSent { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    public int TotalUnsubscribed { get; set; }
    
    // Recipients
    public virtual ICollection<EmailBlastRecipient> Recipients { get; set; }
}

// Individual recipient tracking
public class EmailBlastRecipient : Entity<Guid>
{
    public Guid RecipientId { get; set; }
    public Guid EmailBlastId { get; set; }
    public Guid EmailContactId { get; set; }
    
    // AWS SES tracking via SNS
    public string MessageId { get; set; }  // SES message ID
    
    // Delivery tracking
    public EmailDeliveryStatus Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? BouncedAt { get; set; }
    public string BounceReason { get; set; }
    
    // Engagement tracking
    public DateTime? FirstOpenedAt { get; set; }
    public int OpenCount { get; set; }
    public DateTime? LastOpenedAt { get; set; }
    
    public DateTime? FirstClickedAt { get; set; }
    public int ClickCount { get; set; }
    public DateTime? LastClickedAt { get; set; }
    
    public bool Unsubscribed { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
    
    // Navigation
    public virtual EmailBlast EmailBlast { get; set; }
    public virtual EmailContact Contact { get; set; }
}

public enum EmailDeliveryStatus
{
    Pending,
    Sent,
    Delivered,
    Opened,
    Clicked,
    Bounced,
    Complained  // Spam complaint
}
```

**Email Tracking Implementation (AWS SES + SNS):**

```csharp
// AWS SES Configuration Events
// Set up SNS topic to receive:
// - Delivery confirmations
// - Opens (requires tracking pixel)
// - Clicks (requires link wrapping)
// - Bounces
// - Complaints

public class EmailTrackingService : IEmailTrackingService
{
    // 1. Send email with tracking
    public async Task SendBlastEmail(EmailBlast blast, List<EmailContact> recipients)
    {
        foreach (var contact in recipients)
        {
            var htmlBody = blast.HtmlBody;
            
            // Add tracking pixel for opens
            var trackingPixel = $"<img src='https://yourdomain.com/track/open/{recipientId}' width='1' height='1' />";
            htmlBody += trackingPixel;
            
            // Wrap links for click tracking
            htmlBody = WrapLinksForTracking(htmlBody, recipientId);
            
            // Send via SES
            var request = new SendEmailRequest
            {
                Source = "Family Relocation <noreply@familyrelocation.org>",
                Destination = new Destination { ToAddresses = new List<string> { contact.EmailAddress } },
                Message = new Message
                {
                    Subject = new Content(blast.Subject),
                    Body = new Body { Html = new Content(htmlBody) }
                },
                ConfigurationSetName = "email-tracking"  // Links to SNS topic
            };
            
            var response = await _sesClient.SendEmailAsync(request);
            
            // Save recipient record with message ID
            var recipient = new EmailBlastRecipient
            {
                RecipientId = recipientId,
                EmailBlastId = blast.EmailBlastId,
                EmailContactId = contact.EmailContactId,
                MessageId = response.MessageId,
                Status = EmailDeliveryStatus.Sent,
                SentAt = DateTime.UtcNow
            };
            
            await _repository.AddAsync(recipient);
        }
    }
    
    // 2. Handle open tracking (via pixel request)
    [HttpGet("/track/open/{recipientId}")]
    public async Task<IActionResult> TrackOpen(Guid recipientId)
    {
        var recipient = await _repository.GetByIdAsync(recipientId);
        
        if (recipient.FirstOpenedAt == null)
        {
            recipient.FirstOpenedAt = DateTime.UtcNow;
        }
        
        recipient.OpenCount++;
        recipient.LastOpenedAt = DateTime.UtcNow;
        recipient.Status = EmailDeliveryStatus.Opened;
        
        await _repository.UpdateAsync(recipient);
        
        // Return 1x1 transparent pixel
        return File(Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"), 
            "image/gif");
    }
    
    // 3. Handle click tracking (via wrapped links)
    [HttpGet("/track/click/{recipientId}/{linkId}")]
    public async Task<IActionResult> TrackClick(Guid recipientId, string linkId)
    {
        var recipient = await _repository.GetByIdAsync(recipientId);
        
        if (recipient.FirstClickedAt == null)
        {
            recipient.FirstClickedAt = DateTime.UtcNow;
        }
        
        recipient.ClickCount++;
        recipient.LastClickedAt = DateTime.UtcNow;
        recipient.Status = EmailDeliveryStatus.Clicked;
        
        await _repository.UpdateAsync(recipient);
        
        // Get original URL and redirect
        var originalUrl = GetOriginalUrl(linkId);
        return Redirect(originalUrl);
    }
    
    // 4. Handle SNS notifications from SES
    [HttpPost("/webhooks/ses")]
    public async Task<IActionResult> HandleSesNotification([FromBody] SnsNotification notification)
    {
        var message = JsonSerializer.Deserialize<SesNotification>(notification.Message);
        
        switch (message.NotificationType)
        {
            case "Delivery":
                await HandleDelivery(message.Mail.MessageId);
                break;
                
            case "Bounce":
                await HandleBounce(message.Mail.MessageId, message.Bounce);
                break;
                
            case "Complaint":
                await HandleComplaint(message.Mail.MessageId);
                break;
        }
        
        return Ok();
    }
}
```

**Auto-Link Email History When Converting:**

```csharp
// When prospect created from public form
public class CreateApplicantCommandHandler : IRequestHandler<CreateApplicantCommand, ApplicantDto>
{
    public async Task<ApplicantDto> Handle(CreateApplicantCommand command, CancellationToken ct)
    {
        // 1. Check for existing prospect by email (already doing this)
        var prospect = await _prospectRepository.GetByEmail(command.Email);
        
        // 2. NEW: Check for existing email contact
        var emailContact = await _emailContactRepository.GetByEmail(command.Email);
        
        // 3. Create applicant
        var applicant = Applicant.CreateFromApplication(...);
        await _applicantRepository.AddAsync(applicant);
        
        // 4. Link prospect if exists
        if (prospect != null)
        {
            prospect.ConvertToApplicant(applicant.ApplicantId, command.CreatedBy);
        }
        
        // 5. NEW: Link email contact if exists
        if (emailContact != null)
        {
            emailContact.ApplicantId = applicant.ApplicantId;
            emailContact.ConvertedDate = DateTime.UtcNow;
            await _emailContactRepository.UpdateAsync(emailContact);
            
            // Create activity showing email history
            var activity = Activity.Create(
                EntityType.Applicant,
                applicant.ApplicantId,
                ActivityType.Note,
                "Email Marketing History",
                $"Received {emailContact.EmailsReceived.Count} marketing emails before applying. " +
                $"Last opened: {emailContact.EmailsReceived.OrderByDescending(e => e.LastOpenedAt).FirstOrDefault()?.LastOpenedAt}",
                command.CreatedBy
            );
            await _activityRepository.AddAsync(activity);
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
        return _mapper.Map<ApplicantDto>(applicant);
    }
}
```

**UI Features:**

1. **Email Contacts Page**
   - Import CSV of email addresses
   - Add manually
   - View list with subscription status
   - Filter: Subscribed, Unsubscribed, Converted

2. **Create Email Blast Page**
   - Subject line
   - HTML email editor (WYSIWYG)
   - Select recipients (all subscribed, or filtered list)
   - Preview before sending
   - Schedule send (or send immediately)

3. **Email Blast Dashboard**
   - List all past campaigns
   - Stats: Sent, Delivered, Opened (%), Clicked (%), Unsubscribed
   - Click to view individual recipient tracking

4. **Applicant/Prospect Details - Email History Tab**
   - Shows all marketing emails they received
   - When opened, when clicked
   - Before they applied

**Cost Analysis:**

- **AWS SES:** $0.10 per 1,000 emails (very cheap!)
- **AWS SNS:** $0.50 per 1 million notifications (essentially free)
- **Example:** 500 contacts × 2 blasts/month = 1,000 emails/month = $0.10/month

**Compliance:**

- ✅ CAN-SPAM Act compliance
  - Unsubscribe link in every email
  - Physical address in footer
  - Accurate subject lines
  - Honor unsubscribe within 10 days
  
- ✅ GDPR compliance (if applicable)
  - Explicit consent to subscribe
  - Right to be forgotten
  - Export data on request

**Priority:** P2 (Phase 3 - Nice to have, not blocking MVP)

**User Stories (for later):**
- US: Import email list from CSV
- US: Create and send email blast
- US: View email blast statistics
- US: View recipient-level tracking
- US: Manage subscriptions/unsubscribes
- US: View email history on applicant record
- US: Auto-link email contact when they apply

---

## 15. Shul Addresses (FINAL)

**Shul Data for System:**

```csharp
public static class ShulSeeder
{
    public static List<Shul> GetShuls()
    {
        return new List<Shul>
        {
            new Shul
            {
                ShulId = Guid.NewGuid(),
                Name = "Bobov",
                Address = new Address(
                    street: "212 New Jersey Ave",
                    city: "Elizabeth",
                    state: "NJ",
                    zipCode: "07202",
                    unit: null
                ),
                // Coordinates will be geocoded
                Coordinates = null,  // TODO: Geocode
                IsActive = true
            },
            new Shul
            {
                ShulId = Guid.NewGuid(),
                Name = "Nassad",
                Address = new Address(
                    street: "433 Bailey Avenue",
                    city: "Elizabeth",
                    state: "NJ",
                    zipCode: "07208",
                    unit: null
                ),
                Coordinates = null,  // TODO: Geocode
                IsActive = true
            },
            new Shul
            {
                ShulId = Guid.NewGuid(),
                Name = "Yismach Yisroel",
                Address = new Address(
                    street: "547 Salem Road",
                    city: "Union",
                    state: "NJ",
                    zipCode: "07083",
                    unit: null
                ),
                Coordinates = null,  // TODO: Geocode
                IsActive = true
            }
        };
    }
}
```

**Geocoding Service (to get coordinates):**

```csharp
public interface IGeocodingService
{
    Task<Coordinates> GeocodeAddress(Address address);
}

public class MapBoxGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    public async Task<Coordinates> GeocodeAddress(Address address)
    {
        // MapBox Geocoding API (50k requests/month free)
        var query = Uri.EscapeDataString(address.FullAddress);
        var url = $"https://api.mapbox.com/geocoding/v5/mapbox.places/{query}.json?access_token={_apiKey}";
        
        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MapBoxGeocodeResponse>(json);
        
        if (result.Features?.Any() == true)
        {
            var coords = result.Features[0].Center;
            return new Coordinates(
                latitude: (decimal)coords[1],
                longitude: (decimal)coords[0]
            );
        }
        
        throw new Exception($"Could not geocode address: {address}");
    }
}

// Run once to populate shul coordinates:
public async Task SeedShulsWithCoordinates()
{
    var shuls = ShulSeeder.GetShuls();
    
    foreach (var shul in shuls)
    {
        shul.Coordinates = await _geocodingService.GeocodeAddress(shul.Address);
        await _shulRepository.AddAsync(shul);
    }
    
    await _unitOfWork.SaveChangesAsync();
}
```

**Priority:** P0 (Need for shul proximity matching)

---

---

## 16. Follow-Up Reminders Dashboard with Print View

**Issue:** Coordinators need to see all due reminders in one place and print as task list

**Requirements:**
1. Dashboard view showing all currently due reminders
2. Filter by: overdue, today, this week, assigned to me, assigned to anyone
3. Click reminder → go directly to applicant/prospect page
4. Print view with full contact information (no links, printer-friendly)
5. Mark reminders as complete from dashboard
6. Snooze reminder (reschedule for later)

**Implementation:**

```csharp
// Query to get reminders dashboard
public class GetRemindersQuery : IRequest<RemindersDashboardDto>
{
    public ReminderFilter Filter { get; set; }
    public Guid? AssignedToUserId { get; set; }  // null = all coordinators
}

public enum ReminderFilter
{
    Overdue,        // Due date < today
    DueToday,       // Due date = today
    DueThisWeek,    // Due date within 7 days
    DueThisMonth,   // Due date within 30 days
    All             // All open reminders
}

// Query Handler
public class GetRemindersQueryHandler : IRequestHandler<GetRemindersQuery, RemindersDashboardDto>
{
    private readonly IFollowUpReminderRepository _reminderRepository;
    private readonly IApplicantRepository _applicantRepository;
    private readonly IProspectRepository _prospectRepository;
    private readonly IMapper _mapper;

    public async Task<RemindersDashboardDto> Handle(GetRemindersQuery query, CancellationToken ct)
    {
        var reminders = await _reminderRepository.GetOpenRemindersAsync();

        // Filter by assigned user
        if (query.AssignedToUserId.HasValue)
        {
            reminders = reminders.Where(r => r.AssignedToUserId == query.AssignedToUserId.Value).ToList();
        }

        // Filter by date
        var now = DateTime.UtcNow;
        var today = now.Date;
        var endOfWeek = today.AddDays(7);
        var endOfMonth = today.AddDays(30);

        reminders = query.Filter switch
        {
            ReminderFilter.Overdue => reminders.Where(r => r.DueDate < today).ToList(),
            ReminderFilter.DueToday => reminders.Where(r => r.DueDate.Date == today).ToList(),
            ReminderFilter.DueThisWeek => reminders.Where(r => r.DueDate.Date <= endOfWeek).ToList(),
            ReminderFilter.DueThisMonth => reminders.Where(r => r.DueDate.Date <= endOfMonth).ToList(),
            ReminderFilter.All => reminders,
            _ => reminders
        };

        // Sort by priority then due date
        var sorted = reminders
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.DueDate)
            .ToList();

        // Map to DTOs with entity information
        var reminderDtos = new List<ReminderWithEntityDto>();
        
        foreach (var reminder in sorted)
        {
            var dto = _mapper.Map<ReminderWithEntityDto>(reminder);
            
            // Load related entity details (Applicant or Prospect)
            if (reminder.EntityType == EntityType.Applicant)
            {
                var applicant = await _applicantRepository.GetByIdAsync(reminder.EntityId);
                if (applicant != null)
                {
                    dto.EntityName = applicant.FullName;
                    dto.EntityEmail = applicant.Email?.Value;
                    dto.EntityPhone = applicant.PhoneNumbers.FirstOrDefault()?.FormattedNumber;
                    dto.EntityAddress = applicant.Address?.FullAddress;
                }
            }
            else if (reminder.EntityType == EntityType.Prospect)
            {
                var prospect = await _prospectRepository.GetByIdAsync(reminder.EntityId);
                if (prospect != null)
                {
                    dto.EntityName = prospect.FullName;
                    dto.EntityEmail = prospect.Email?.Value;
                    dto.EntityPhone = prospect.PhoneNumbers.FirstOrDefault()?.FormattedNumber;
                    dto.EntityAddress = prospect.Address?.FullAddress;
                }
            }
            
            reminderDtos.Add(dto);
        }

        return new RemindersDashboardDto
        {
            Reminders = reminderDtos,
            TotalCount = reminderDtos.Count,
            OverdueCount = reminderDtos.Count(r => r.IsOverdue),
            DueTodayCount = reminderDtos.Count(r => r.IsDueToday)
        };
    }
}

// DTOs
public class RemindersDashboardDto
{
    public List<ReminderWithEntityDto> Reminders { get; set; }
    public int TotalCount { get; set; }
    public int OverdueCount { get; set; }
    public int DueTodayCount { get; set; }
}

public class ReminderWithEntityDto
{
    // Reminder info
    public Guid ReminderId { get; set; }
    public string Title { get; set; }
    public DateTime DueDate { get; set; }
    public ReminderPriority Priority { get; set; }
    public string Notes { get; set; }
    public string AssignedToName { get; set; }
    
    // Calculated
    public bool IsOverdue => DueDate.Date < DateTime.UtcNow.Date;
    public bool IsDueToday => DueDate.Date == DateTime.UtcNow.Date;
    public int DaysOverdue => IsOverdue ? (DateTime.UtcNow.Date - DueDate.Date).Days : 0;
    
    // Entity info (Applicant or Prospect)
    public EntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string EntityName { get; set; }
    public string EntityEmail { get; set; }
    public string EntityPhone { get; set; }
    public string EntityAddress { get; set; }
}

// Commands
public class CompleteReminderCommand : IRequest<Unit>
{
    public Guid ReminderId { get; set; }
    public string CompletionNotes { get; set; }
}

public class SnoozeReminderCommand : IRequest<Unit>
{
    public Guid ReminderId { get; set; }
    public DateTime NewDueDate { get; set; }
    public string SnoozeReason { get; set; }
}
```

**API Endpoints:**

```csharp
[ApiController]
[Route("api/reminders")]
public class RemindersController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("dashboard")]
    public async Task<ActionResult<RemindersDashboardDto>> GetDashboard(
        [FromQuery] ReminderFilter filter = ReminderFilter.DueThisWeek,
        [FromQuery] Guid? assignedToUserId = null)
    {
        var query = new GetRemindersQuery 
        { 
            Filter = filter,
            AssignedToUserId = assignedToUserId
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteReminder(Guid id, [FromBody] CompleteReminderRequest request)
    {
        var command = new CompleteReminderCommand
        {
            ReminderId = id,
            CompletionNotes = request.Notes
        };
        
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/snooze")]
    public async Task<ActionResult> SnoozeReminder(Guid id, [FromBody] SnoozeReminderRequest request)
    {
        var command = new SnoozeReminderCommand
        {
            ReminderId = id,
            NewDueDate = request.NewDueDate,
            SnoozeReason = request.Reason
        };
        
        await _mediator.Send(command);
        return NoContent();
    }
}
```

**React UI - Dashboard View (Screen):**

```tsx
// RemindersPage.tsx
export const RemindersPage: React.FC = () => {
    const [filter, setFilter] = useState<ReminderFilter>('DueThisWeek');
    const [showMyOnly, setShowMyOnly] = useState(true);
    const { data, refetch } = useQuery(['reminders', filter], () => 
        remindersApi.getDashboard(filter, showMyOnly ? currentUser.userId : null)
    );

    const handleComplete = async (reminderId: string, notes: string) => {
        await remindersApi.complete(reminderId, notes);
        message.success('Reminder completed');
        refetch();
    };

    const handleSnooze = async (reminderId: string, newDate: Date, reason: string) => {
        await remindersApi.snooze(reminderId, newDate, reason);
        message.success('Reminder snoozed');
        refetch();
    };

    return (
        <PageContainer title="Follow-Up Reminders">
            <Space direction="vertical" style={{ width: '100%' }} size="large">
                {/* Stats Cards */}
                <Row gutter={16}>
                    <Col span={8}>
                        <Card>
                            <Statistic 
                                title="Overdue" 
                                value={data?.overdueCount || 0}
                                valueStyle={{ color: '#cf1322' }}
                                prefix={<ClockCircleOutlined />}
                            />
                        </Card>
                    </Col>
                    <Col span={8}>
                        <Card>
                            <Statistic 
                                title="Due Today" 
                                value={data?.dueTodayCount || 0}
                                valueStyle={{ color: '#faad14' }}
                                prefix={<CalendarOutlined />}
                            />
                        </Card>
                    </Col>
                    <Col span={8}>
                        <Card>
                            <Statistic 
                                title="Total Open" 
                                value={data?.totalCount || 0}
                                prefix={<BellOutlined />}
                            />
                        </Card>
                    </Col>
                </Row>

                {/* Filters */}
                <Card>
                    <Space>
                        <Select 
                            value={filter} 
                            onChange={setFilter}
                            style={{ width: 200 }}
                        >
                            <Select.Option value="Overdue">Overdue</Select.Option>
                            <Select.Option value="DueToday">Due Today</Select.Option>
                            <Select.Option value="DueThisWeek">Due This Week</Select.Option>
                            <Select.Option value="DueThisMonth">Due This Month</Select.Option>
                            <Select.Option value="All">All Open</Select.Option>
                        </Select>

                        <Checkbox 
                            checked={showMyOnly}
                            onChange={(e) => setShowMyOnly(e.target.checked)}
                        >
                            My Reminders Only
                        </Checkbox>

                        <Button 
                            icon={<PrinterOutlined />}
                            onClick={() => window.open('/reminders/print', '_blank')}
                        >
                            Print Task List
                        </Button>
                    </Space>
                </Card>

                {/* Reminders Table */}
                <Card>
                    <Table
                        dataSource={data?.reminders || []}
                        rowKey="reminderId"
                        pagination={false}
                        rowClassName={(record) => {
                            if (record.isOverdue) return 'reminder-overdue';
                            if (record.isDueToday) return 'reminder-due-today';
                            return '';
                        }}
                    >
                        <Table.Column
                            title="Priority"
                            dataIndex="priority"
                            width={100}
                            render={(priority: ReminderPriority) => (
                                <Tag color={getPriorityColor(priority)}>
                                    {priority}
                                </Tag>
                            )}
                        />

                        <Table.Column
                            title="Due Date"
                            dataIndex="dueDate"
                            width={120}
                            render={(date: string, record: ReminderWithEntityDto) => (
                                <Space direction="vertical" size={0}>
                                    <Text>{formatDate(date)}</Text>
                                    {record.isOverdue && (
                                        <Text type="danger" style={{ fontSize: 12 }}>
                                            {record.daysOverdue} days overdue
                                        </Text>
                                    )}
                                </Space>
                            )}
                        />

                        <Table.Column
                            title="Task"
                            dataIndex="title"
                            render={(title: string, record: ReminderWithEntityDto) => (
                                <Space direction="vertical" size={0}>
                                    <Text strong>{title}</Text>
                                    {record.notes && (
                                        <Text type="secondary" style={{ fontSize: 12 }}>
                                            {record.notes}
                                        </Text>
                                    )}
                                </Space>
                            )}
                        />

                        <Table.Column
                            title="Contact"
                            dataIndex="entityName"
                            render={(name: string, record: ReminderWithEntityDto) => (
                                <Space direction="vertical" size={0}>
                                    <Button 
                                        type="link" 
                                        onClick={() => navigate(getEntityUrl(record))}
                                        style={{ padding: 0, height: 'auto' }}
                                    >
                                        {name}
                                    </Button>
                                    <Text type="secondary" style={{ fontSize: 12 }}>
                                        {record.entityPhone}
                                    </Text>
                                </Space>
                            )}
                        />

                        <Table.Column
                            title="Assigned To"
                            dataIndex="assignedToName"
                            width={120}
                        />

                        <Table.Column
                            title="Actions"
                            width={200}
                            render={(_, record: ReminderWithEntityDto) => (
                                <Space>
                                    <Button
                                        size="small"
                                        type="primary"
                                        icon={<CheckOutlined />}
                                        onClick={() => showCompleteModal(record)}
                                    >
                                        Complete
                                    </Button>
                                    <Button
                                        size="small"
                                        icon={<ClockCircleOutlined />}
                                        onClick={() => showSnoozeModal(record)}
                                    >
                                        Snooze
                                    </Button>
                                </Space>
                            )}
                        />
                    </Table>
                </Card>
            </Space>
        </PageContainer>
    );
};

// Helper to get entity URL
const getEntityUrl = (reminder: ReminderWithEntityDto): string => {
    if (reminder.entityType === 'Applicant') {
        return `/applicants/${reminder.entityId}`;
    } else if (reminder.entityType === 'Prospect') {
        return `/prospects/${reminder.entityId}`;
    }
    return '#';
};
```

**Print View (Printer-Friendly):**

```tsx
// RemindersPrintPage.tsx
export const RemindersPrintPage: React.FC = () => {
    const [filter] = useQueryParams();
    const { data } = useQuery(['reminders-print', filter], () => 
        remindersApi.getDashboard(filter)
    );

    useEffect(() => {
        // Auto-print when page loads
        window.print();
    }, []);

    return (
        <div className="print-container">
            <style>{`
                @media print {
                    body { 
                        font-size: 12pt;
                        color: #000;
                    }
                    .print-container {
                        width: 100%;
                        padding: 20px;
                    }
                    .page-break {
                        page-break-after: always;
                    }
                    table {
                        width: 100%;
                        border-collapse: collapse;
                        margin-bottom: 20px;
                    }
                    th, td {
                        border: 1px solid #ddd;
                        padding: 8px;
                        text-align: left;
                    }
                    th {
                        background-color: #f5f5f5;
                    }
                    .overdue {
                        background-color: #fff1f0;
                    }
                    .due-today {
                        background-color: #fffbe6;
                    }
                }
                @media screen {
                    .print-container {
                        max-width: 8.5in;
                        margin: 20px auto;
                        padding: 20px;
                        background: white;
                        box-shadow: 0 0 10px rgba(0,0,0,0.1);
                    }
                }
            `}</style>

            {/* Header */}
            <div style={{ marginBottom: 20, borderBottom: '2px solid #000', paddingBottom: 10 }}>
                <h1 style={{ margin: 0 }}>Follow-Up Task List</h1>
                <p style={{ margin: '5px 0 0 0' }}>
                    Generated: {formatDateTime(new Date())} | 
                    Filter: {filter} | 
                    Total Tasks: {data?.totalCount || 0}
                </p>
            </div>

            {/* Reminders Table */}
            <table>
                <thead>
                    <tr>
                        <th style={{ width: '8%' }}>Priority</th>
                        <th style={{ width: '12%' }}>Due Date</th>
                        <th style={{ width: '25%' }}>Task</th>
                        <th style={{ width: '20%' }}>Contact Name</th>
                        <th style={{ width: '15%' }}>Phone</th>
                        <th style={{ width: '20%' }}>Email</th>
                    </tr>
                </thead>
                <tbody>
                    {data?.reminders.map((reminder) => (
                        <tr 
                            key={reminder.reminderId}
                            className={
                                reminder.isOverdue ? 'overdue' :
                                reminder.isDueToday ? 'due-today' : ''
                            }
                        >
                            <td>
                                <strong>{reminder.priority}</strong>
                            </td>
                            <td>
                                {formatDate(reminder.dueDate)}
                                {reminder.isOverdue && (
                                    <div style={{ color: '#cf1322', fontSize: '10pt' }}>
                                        ⚠️ {reminder.daysOverdue}d overdue
                                    </div>
                                )}
                            </td>
                            <td>
                                <strong>{reminder.title}</strong>
                                {reminder.notes && (
                                    <div style={{ fontSize: '10pt', color: '#666', marginTop: 4 }}>
                                        {reminder.notes}
                                    </div>
                                )}
                            </td>
                            <td>
                                <strong>{reminder.entityName}</strong>
                                <div style={{ fontSize: '10pt', color: '#666' }}>
                                    {reminder.entityType}
                                </div>
                            </td>
                            <td>{reminder.entityPhone || '-'}</td>
                            <td style={{ fontSize: '10pt' }}>{reminder.entityEmail || '-'}</td>
                        </tr>
                    ))}
                </tbody>
            </table>

            {/* Footer with space for notes */}
            <div style={{ marginTop: 30, borderTop: '1px solid #ddd', paddingTop: 10 }}>
                <p><strong>Notes:</strong></p>
                <div style={{ 
                    minHeight: 100, 
                    border: '1px solid #ddd', 
                    padding: 10,
                    marginTop: 5 
                }}>
                    {/* Empty space for handwritten notes */}
                </div>
            </div>

            {/* Checkbox list for completion */}
            <div style={{ marginTop: 20 }}>
                <p><strong>Task Completion Checklist:</strong></p>
                {data?.reminders.map((reminder) => (
                    <div key={reminder.reminderId} style={{ marginBottom: 8 }}>
                        <label>
                            <input type="checkbox" style={{ marginRight: 8 }} />
                            {reminder.title} - {reminder.entityName}
                        </label>
                    </div>
                ))}
            </div>
        </div>
    );
};
```

**CSS for Screen View:**

```css
/* RemindersPage.css */
.reminder-overdue {
    background-color: #fff1f0 !important;
}

.reminder-due-today {
    background-color: #fffbe6 !important;
}
```

**User Stories:**

```markdown
### US: View Follow-Up Reminders Dashboard

**As a** coordinator
**I want to** see all my due reminders in one dashboard
**So that** I know what follow-ups I need to do today

**Acceptance Criteria:**
1. Dashboard shows summary stats: Overdue, Due Today, Total Open
2. Can filter by: Overdue, Due Today, This Week, This Month, All
3. Can toggle "My Reminders Only" vs "All Team Reminders"
4. Table shows: Priority, Due Date, Task, Contact Name, Contact Phone, Assigned To
5. Overdue reminders highlighted in red
6. Due today reminders highlighted in yellow
7. Shows how many days overdue
8. Click contact name → navigate to applicant/prospect page
9. Can mark reminder complete (with optional notes)
10. Can snooze reminder (reschedule with reason)
11. Can print task list

**Priority:** P0 (MVP - Essential tool for coordinators)
**Effort:** 5 points
**Sprint:** 3

---

### US: Print Follow-Up Task List

**As a** coordinator
**I want to** print my task list on paper
**So that** I can work through it during the day without being on the computer

**Acceptance Criteria:**
1. "Print Task List" button opens print-friendly view
2. Print view includes:
   - Header with date generated and filter used
   - Table with: Priority, Due Date, Task, Contact Name, Phone, Email
   - No clickable links (all information visible)
   - Overdue items visually marked
3. Print view automatically triggers print dialog
4. Includes space at bottom for handwritten notes
5. Includes checkbox list for manual completion tracking
6. Printer-friendly formatting:
   - Black and white
   - Clear borders
   - Readable 12pt font
   - Fits on standard 8.5x11 paper
7. Can print specific filter (e.g., "Due Today" only)

**Priority:** P0 (MVP - Many coordinators prefer paper)
**Effort:** 3 points
**Sprint:** 3
```

**Priority:** P0 (MVP) - Essential coordination tool

**Implementation Notes:**
- Dashboard page part of Sprint 3 (same sprint as reminders)
- Print functionality critical for coordinators who work with paper
- On-screen version optimized for quick actions (complete, snooze)
- Print version optimized for all-day reference with full contact info
- Both views use same data source (query with different filters)

---

**END OF CORRECTIONS LOG**
