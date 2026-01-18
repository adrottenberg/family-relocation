# CONVERSATION MEMORY LOG
## Family Relocation CRM System - Complete Session Context

**Session Date:** January 6-9, 2026  
**Total Duration:** 4 days, multiple sessions  
**Project:** Custom Family Relocation System for Jewish Community in Union County, NJ  
**User Role:** Experienced software developer, limited time, building solo

---

## 📋 TABLE OF CONTENTS

1. [Original Problem & Evolution](#original-problem--evolution)
2. [User's Working Style & Preferences](#users-working-style--preferences)
3. [Key Decisions Made](#key-decisions-made)
4. [Technical Architecture Established](#technical-architecture-established)
5. [Domain Model Developed](#domain-model-developed)
6. [All Corrections & Refinements](#all-corrections--refinements)
7. [Effective Collaboration Patterns](#effective-collaboration-patterns)
8. [Project Context & Requirements](#project-context--requirements)
9. [Templates & Frameworks Created](#templates--frameworks-created)
10. [Next Steps Identified](#next-steps-identified)
11. [Quick Reference for Future Sessions](#quick-reference-for-future-sessions)

---

## 1. ORIGINAL PROBLEM & EVOLUTION

### Initial Request (Day 1)
**User said:** "I need a CRM to manage Jewish families relocating to Union County, NJ"

**Started with:** CRM research phase
- Evaluated HubSpot, Airtable, Calendly, Zapier (550+ pages of research)
- HubSpot rejected: Only 2 users + 10 custom fields in free tier (needed 31 fields)
- Bitrix24 selected initially

### First Pivot (Day 2)
**After 3+ hours testing Bitrix24:**
- User: "This UI is completely unusable"
- Completely rejected Bitrix24
- Started fresh evaluation with NEW priority: **UI/usability is #1**

**New candidates:**
- Freshsales
- Monday CRM (nonprofit program)

### Second Pivot (Day 3)
**User chose Monday CRM, requested implementation guides:**
- Created 110+ pages of Monday CRM documentation:
  - 80-page complete setup guide
  - 30-page required fields & validation guide
- User applied for Monday.com nonprofit program
- Started 14-day trial

### Final Decision (Day 4)
**Monday.com nonprofit application REJECTED**

**User's conclusion:**
> "All CRMs overcomplicate simple tasks. Features locked behind expensive tiers. Data split between systems. I'm an experienced software developer with limited time - I'll build exactly what I need."

**Benefits identified:**
- No automation costs
- No record limits
- Exact UI/workflow match
- Unified data (no CRM + Airtable split)
- Own all data
- Could sell/license to other communities later

### Evolution to Custom System
**This is when the real work began:**
1. Gathered comprehensive user stories
2. Defined complete domain model (DDD approach)
3. Selected tech stack (AWS + .NET 10 + React)
4. Designed security architecture
5. Created 300+ pages of technical specifications

**Key insight:** User knew what they wanted all along but needed to prove off-the-shelf wouldn't work.

---

## 2. USER'S WORKING STYLE & PREFERENCES

### Communication Style

**Direct & Efficient:**
- User doesn't want fluff or excessive explanation
- Prefers succinct responses with action items
- Values: "Here's what I found" over "Let me explain why..."
- Example correction: "Don't update any documentation yet, I am still reviewing"

**Detail-Oriented:**
- Caught missing "Under Contract" stage in workflow
- Noticed inconsistent wife/spouse naming
- Identified need for failed contract tracking
- Spotted missing follow-up reminder functionality
- Always thinking about edge cases

**Technical Expertise:**
- Experienced software developer (knows .NET, architecture patterns)
- Understands DDD, Clean Architecture, CQRS
- Familiar with AWS, PostgreSQL, React
- Knew immediately that .NET 8 was wrong (latest is .NET 10)
- Makes informed technical decisions quickly

**Iterative Review Process:**
> "I will send my feedback as I go along, when I am done you can send me a final updated document"

- Reviews thoroughly before committing
- Sends corrections incrementally
- Wants ONE consolidated update at end (not piecemeal updates)
- Doesn't want documents updated until complete review done

### Decision-Making Patterns

**Practical Over Perfect:**
- Chose AWS over Azure (simpler, more generous free tier)
- Chose React over Blazor (bigger community, more resources, market share matters)
- Desktop-first approach (no mobile optimization needed)
- "Chasidic families don't use smartphones" - practical cultural consideration

**Domain-Driven:**
- Insisted on proper domain terminology:
  - "Applicant" not "Contact"
  - "Application" not "Deal"
  - "Wife" not "Spouse" (matches community language)
- Value objects for all key concepts (Address, PhoneNumber, Money, etc.)
- Board review at Applicant level (not Application) - proper domain boundary

**Cost-Conscious:**
- AWS free tier approach ($0-25/month vs $36+/month for CRMs)
- Removed unnecessary features (SMS, PWA, mobile optimization)
- Focus on what actually adds value

**User-Centric:**
- "Most Chasidic families don't use smartphones" → Desktop-first design
- Understood applicant portal needed to be simple
- Google OAuth for applicants (no password management hassle)
- Monthly payment calculator for families

### Working Preferences

**Documentation:**
- Wants complete, comprehensive specs upfront
- Prefers organized, structured documents
- Values tables of contents and clear sections
- Likes executive summaries
- Code samples very important ("show me the code")

**Tooling:**
- Plans to use Jira for project management
- Wants user stories in proper format for import
- Visual Studio for backend development
- Familiar with Git/GitHub

**Process:**
- Review first, implement later
- Get full picture before starting
- Make informed decisions with complete information
- Doesn't rush - takes time to review properly

---

## 3. KEY DECISIONS MADE

### Technology Stack

**Backend: .NET 10 (NOT .NET 8)**
- User corrected me: "Latest LTS is .NET 10 as of a month ago"
- ASP.NET Core 10 Web API
- C# 13
- Clean Architecture + DDD
- CQRS with MediatR
- Entity Framework Core 10
- PostgreSQL 16

**Frontend: React (NOT Blazor)**
- User was RIGHT to be concerned about Blazor's market share (2-3%)
- React: 42% market share, massive community, resources everywhere
- TypeScript for type safety
- Ant Design (perfect for admin panels)
- Vite build tool
- TanStack Query for API calls

**Cloud: AWS (NOT Azure)**
- User preference: "I already decided to go with AWS"
- More generous free tier
- Better .NET support than before
- RDS PostgreSQL free tier (750 hrs/month forever)
- SES: 62k emails/month FREE (vs SendGrid's 100/day)
- S3: 5GB free forever
- Cognito: 50k MAU free

**Why these decisions were right:**
- .NET 10: Latest features, better performance
- React: Market share = maximum help available (critical for solo developer)
- AWS: Better free tier = lower costs long-term

### Architecture Patterns

**Clean Architecture (4 Layers):**
1. API Layer (Controllers, DTOs)
2. Application Layer (Commands, Queries, Handlers)
3. Domain Layer (Entities, Value Objects, Domain Services)
4. Infrastructure Layer (EF Core, Repositories, AWS Services)

**CQRS Pattern:**
- Commands for writes (CreateApplicant, ApproveApplication)
- Queries for reads (GetApplicantById, GetApplications)
- MediatR for decoupling

**Domain-Driven Design:**
- Proper aggregates (Applicant, Application, Property)
- Value objects (Address, PhoneNumber, Email, Money, Child, Coordinates)
- Domain events (ApplicationApproved, ContractFellThrough)
- Ubiquitous language from the domain (not CRM terms)

### Feature Decisions

**Desktop-First (No Mobile Priority):**
- User: "Most Chasidic families don't use smartphones"
- Removed mobile optimization from priorities
- Removed PWA/offline support
- Saved 1-2 weeks of development time
- Still works on mobile (Ant Design is responsive by default)

**No SMS Notifications:**
- Not needed if families don't use smartphones
- Email sufficient for all communication
- Removed from scope

**Google OAuth for Applicant Portal:**
- User's idea to add limited applicant portal
- Google sign-in (no password management)
- Self-service features for families
- View application status, update preferences, view matches

**Follow-Up Reminders:**
- User asked: "Do you have a user story for setting follow up reminders?"
- Added 3 new user stories (create, view, smart suggestions)
- Critical for coordinator workflow

---

## 4. TECHNICAL ARCHITECTURE ESTABLISHED

### Domain Model (Final)

**Core Entities (Aggregates):**

1. **Prospect** (Pre-Application)
   - Tracks leads before formal application
   - Status: InitialContact → InDiscussion → Interested → ReadyToApply → Applied
   - Links to Applicant when they apply

2. **Applicant** (Family)
   - Husband info: FirstName, LastName, FatherName
   - Wife info: WifeFirstName, WifeMaidenName, WifeFatherName, WifeHighSchool
   - Contact: Email, PhoneNumbers (collection with type), Address
   - Children: Collection of Child value objects (age + gender)
   - Housing preferences: Budget, Bedrooms, Bathrooms, Neighborhoods, Features, Shul Proximity
   - Board review: BoardReviewDate, BoardDecision, BoardDecisionNotes (at Applicant level!)
   - Mortgage info: DownPayment, InterestRate, LoanTerm

3. **Application** (Journey)
   - Tracks applicant's journey from submission to closing
   - 6 stages: UnderReview → Approved → HouseHunting → OnHold → UnderContract → Closed
   - Agreements: BrokerAgreementSigned, CommunityAgreementSigned (with document uploads required)
   - Contract details: PropertyAddress, ContractDate, PurchasePrice, ExpectedClosingDate
   - Commission tracking: From broker, from buyer, status, paid date
   - On hold tracking: Reason, duration, expected resume date
   - Failed contracts: Collection of contracts that fell through

4. **Property**
   - Address, Price, Tax, Beds, Baths, Square Footage, Lot Size, Year Built
   - Features (collection)
   - Photos (collection with descriptions)
   - ListingStatus: Active, UnderContract, UnderContractThroughUs, Sold, OffMarket
   - Monthly payment calculator method (PITI)

5. **PropertyMatch**
   - Links Property to Applicant with score (0-110)
   - MatchExplanation text
   - Status: NotSent, SentToFamily, Interested, NotInterested

6. **Showing**
   - Property viewing appointment
   - BrokerId (FK to Broker entity, NOT string!)
   - ShowingDate, Status, InterestLevel, Feedback

7. **Broker**
   - Real estate agent (proper entity)
   - Name, Phone, Email, Company, LicenseNumber
   - TaxId for 1099
   - TotalCommissionPaid tracked

8. **Shul**
   - Synagogue for proximity matching
   - Name, Address, Coordinates (for distance calculation)

9. **FailedContract**
   - Tracks contracts that fell through
   - ContractAttemptNumber (1st, 2nd, 3rd try)
   - FailureReason enum
   - Alerts if 2+ failures for same family

10. **FollowUpReminder**
    - Tasks for coordinator
    - Title, DueDate, Priority, EntityType, EntityId
    - Email notification option
    - Snooze capability

11. **Document**
    - File uploads (broker agreements, community agreements)
    - BlobUrl (S3), ContentType, Type
    - Links to Application/Applicant/Property

**Value Objects (Proper DDD):**

All immutable, value equality, validation in constructor:

- **Address**: Street, City, State, ZipCode, Unit (optional)
- **PhoneNumber**: Number (formatted), Type (Home/Cell/Work/Other)
- **Email**: Value (validated, lowercase)
- **Money**: Amount, Currency (with operators +, -, *, >, <)
- **Child**: Age, Gender
- **Coordinates**: Latitude, Longitude (with DistanceToMiles method)
- **ShulProximityPreference**: PreferenceType, MaxDistanceMiles, SpecificShulIds

**Key Enums:**

- **Neighborhood**: Union_BattleHill, Union_Connecticut_Farms, RosellePark_East, etc.
- **ApplicationStage**: UnderReview, Approved, HouseHunting, OnHold, UnderContract, Closed, Rejected
- **BoardDecision**: Pending, Approved, Rejected, Deferred (removed ApprovedWithConditions)
- **ListingStatus**: Active, UnderContract, UnderContractThroughUs, Sold, OffMarket
- **MovedInStatus**: MovedIn, RentedOut, Resold, Renovating, Unknown (shortened names)
- **MoveTimeline**: Immediate, ShortTerm, MediumTerm, LongTerm, Extended, Flexible, NotSure
- **ContractFailureReason**: HomeInspectionIssues, FinancingFellThrough, etc.

### Database Schema (12+ Tables)

**Main Tables:**
1. Prospects
2. Applicants (with 30+ fields)
3. Applications
4. Properties
5. PropertyPhotos
6. PropertyMatches
7. Showings
8. Brokers
9. Shuls
10. FailedContracts
11. FollowUpReminders
12. Documents

**Supporting:**
- Users (minimal, mostly Cognito)
- Activities (timeline)
- AuditLog (all changes)
- EmailTemplates
- Notifications

**Value Objects Storage:**
- Address: Owned entity type in EF Core
- PhoneNumbers: JSON column (collection)
- Children: JSON column (collection)
- Money: Owned entity type
- Coordinates: Owned entity type

### API Endpoints (50+)

**Public (No Auth):**
- POST /api/applications (submit application)
- GET /api/health

**Applicant Portal (Google OAuth):**
- GET /api/applicant-portal/application
- PUT /api/applicant-portal/housing-preferences
- GET /api/applicant-portal/property-matches
- POST /api/applicant-portal/property-match/{id}/rate
- GET /api/applicant-portal/showings

**Staff (Cognito + Role-Based):**
- /api/prospects (CRUD)
- /api/applicants (CRUD)
- /api/applications (CRUD + pipeline operations)
- /api/properties (CRUD + bulk import)
- /api/showings (CRUD)
- /api/brokers (CRUD)
- /api/property-matches (generate, send)
- /api/reminders (CRUD)
- /api/reports (various)
- /api/users (admin only)
- /api/audit-log (admin only)

### Security Architecture

**Authentication:**
- Public: No auth for /apply
- Applicants: Google OAuth via Cognito Social Identity Provider
- Staff: Email/password via Cognito User Pool (with optional MFA)

**Authorization:**
- Public: Submit application only
- Applicant: View own data, update preferences, rate properties
- Board Member: Read-only, approve/reject applications
- Coordinator: Manage all except delete, user management
- Admin: Full access

**Data Protection:**
- HTTPS everywhere
- reCAPTCHA v3 on public form
- Rate limiting (5 submissions/hour per IP)
- CSRF protection
- Input validation
- SQL injection prevention (EF Core parameterized)
- Audit logging (7-year retention)
- Document uploads require signed agreements

### Property Matching Algorithm

**Scoring (0-110 points):**

1. **Budget (30 points):**
   - Price ≤ budget → 30 points
   - Price > budget → Penalty (-5 per $10k over)

2. **Bedrooms (20 points):**
   - Property beds ≥ min beds → 20 points

3. **Bathrooms (15 points):**
   - Property baths ≥ min baths → 15 points

4. **Neighborhood (20 points):**
   - In preferred list → 20 points

5. **Features (15 points):**
   - (Matching / Total required) × 15

6. **Shul Proximity (Bonus 10 points):**
   - Meets proximity requirement → +10

**Threshold:** Only show matches ≥ 60 by default

**When Generated:**
- Housing preferences updated
- New property added
- Property details changed
- Manual trigger

---

## 5. DOMAIN MODEL DEVELOPED

### Critical User Corrections

**1. Contact → Applicant**
> "Contact is also not exactly the best DDD match, we need an applicant"

**Rationale:** "Contact" is CRM terminology. In this domain, we have families applying to relocate - they're "Applicants."

**2. Deal → Application**
> "We should get rid of the CRM terminology such as deal, and focus on our domain specific terminology"

**Rationale:** "Deal" is sales terminology. In this domain, we track an applicant's journey from submission to closing - it's their "Application" or journey through the program.

**3. Board Review at Applicant Level**
> "Board review fields should be on the contact level (but we should probably rename contact to be more in line with our domain)"

**Rationale:** Board reviews the FAMILY (Applicant), not their journey (Application). A family is approved/rejected once, then their application progresses through stages.

**4. Wife vs Spouse**
> "Option A for 1" (use Wife consistently)

**Rationale:** Matches the community's language. In the Orthodox Jewish community, terminology is specific.

**5. Children as Collection with Gender**
> "Children ages should be a collection? Maybe also include gender of each kid?"

**Rationale:** Need structured data for matching (family with boys needs different house than family with girls). Also useful for school proximity matching.

**6. Phone Numbers as Collection with Type**
> "For Phone number we should also have a value type which identifies the type of phone number and the number. and just allow an applicant contact to have as many as needed."

**Rationale:** Families have multiple contact methods (home, cell, work). Need to track which is which.

**7. Address as Value Object**
> "All address fields should use a proper address object instead of a string"

**Rationale:** Address is a value object - it has no identity, it's immutable, equality is by value. Same for Email, PhoneNumber, Money, etc.

### Complete Ubiquitous Language

**Domain Terms (NOT CRM):**
- ✅ Prospect (lead before applying)
- ✅ Applicant (family)
- ✅ Application (journey)
- ✅ Property (listing)
- ✅ Showing (viewing)
- ✅ Broker (agent)
- ✅ Shul (synagogue)
- ✅ PropertyMatch (algorithmic match)
- ✅ FailedContract (contract that fell through)

**Rejected CRM Terms:**
- ❌ Lead → Use Prospect
- ❌ Contact → Use Applicant
- ❌ Deal → Use Application
- ❌ Opportunity → Use Application
- ❌ Won/Lost → Use Closed/Rejected

This is proper Domain-Driven Design.

---

## 6. ALL CORRECTIONS & REFINEMENTS

### Workflow Corrections

**User caught missing stage:**
> "You have workflow 3 as house hunting to closing, but there is an under contract stage in between."

**Fixed:**
- ❌ OLD: House Hunting → Closing
- ✅ NEW: House Hunting → Under Contract → Closing → Closed

**Under Contract is 30-60 days of:**
- Home inspection
- Mortgage approval
- Attorney review
- Title search

**User identified edge cases:**
> "There are 2 additional situations that we didn't cover, 1 an house hunting family puts their search on hold (temporarily or permanent), 2 a contract falls through prior to closing"

**Added:**
1. **On Hold stage** - For families who need to pause (financial, emergency, timing)
2. **Failed Contract tracking** - When contracts fall through (happens 10-15% of time)

### Data Model Corrections

**User provided detailed feedback list:**

1. ✅ Wife/Spouse naming - Use "Wife" consistently
2. ✅ Remove FamilySize - Only need NumberOfChildren
3. ✅ Children as collection with age + gender
4. ✅ Single Budget field (max only, removed min)
5. ✅ Neighborhoods specific to Union & Roselle Park
6. ✅ Add Shul proximity preference
7. ✅ Remove TargetMoveDate
8. ✅ MoveTimeline as enum (not string)
9. ✅ Board review on Applicant level (not Application)
10. ✅ Remove PropertiesViewed field (calculate from Showings)
11. ✅ Address as value object (not string)
12. ✅ All value objects (Phone, Email, Money, Coordinates)
13. ✅ Audit fields on Applicant
14. ✅ Property taxes + monthly payment calculator
15. ✅ Photo descriptions
16. ✅ Broker as entity (not string name)
17. ✅ Prospect/Lead entity
18. ✅ Remove ApprovedWithConditions from BoardDecision
19. ✅ Add UnderContractThroughUs to ListingStatus
20. ✅ Shorten MovedInStatus names
21. ✅ Require agreement document uploads

**Every single correction was valid and improved the design.**

### Technical Stack Corrections

**User corrected .NET version:**
> "As of a month ago, the latest LTS version of .net is 10, not 8."

**User clarified cloud provider:**
> "I already decided to go with AWS, so we can ignore any of the azure options"

**User confirmed email service:**
> "We can also utilize SES for sending emails"

**User asked about authentication:**
> "I would also maybe consider adding google authentication, and have a limited applicant portal"

All incorporated immediately.

---

## 7. EFFECTIVE COLLABORATION PATTERNS

### What Worked Well

**1. Iterative Review Process**

User's process:
1. Ask for comprehensive documentation
2. Review thoroughly (takes time)
3. Send feedback incrementally: "I will send my feedback as I go along"
4. Request consolidated update at end: "When I am done you can send me a final updated document"

**My role:**
- Create comprehensive initial docs
- Track all feedback in a list
- DON'T update docs until user finishes review
- Consolidate all changes into one final update

**Why it works:**
- User gets complete picture before deciding
- Avoids confusion from multiple versions
- User can review at own pace
- Final product incorporates everything

**2. Assumption Checking**

Several times I made assumptions that user corrected:

- Assumed .NET 8 → User: ".NET 10 is latest"
- Assumed Azure → User: "I already decided AWS"
- Assumed Blazor might be okay → User: "Market share matters" (was right)
- Assumed mobile needed → User: "Chasidic families don't use smartphones"

**Lesson:** When user corrects, they're providing domain knowledge I don't have. Accept and incorporate immediately.

**3. Domain Expert Deference**

User knows their domain better than I do:
- Community culture (no smartphones)
- Terminology (Wife not Spouse, Applicant not Contact)
- Workflow details (contracts fall through, families go on hold)
- Cost sensitivity (nonprofit organization)

**My role:** Capture requirements accurately, suggest technical solutions, defer to user on domain decisions.

**4. Technical Discussion**

User is experienced developer, so:
- Can discuss architecture patterns (CQRS, DDD, Clean Architecture)
- Appreciates code samples
- Understands trade-offs (React vs Blazor, AWS vs Azure)
- Makes informed decisions

**What works:**
- Explain technical options with pros/cons
- Provide code examples
- Discuss at appropriate technical level
- Trust user to make right choice

**5. Comprehensive Documentation**

User wants:
- Complete specs upfront
- Well-organized documents
- Table of contents
- Code samples
- Clear structure

**Not just:** "Here's an overview..."
**But:** "Here's 300 pages of complete technical specifications"

**Why:** User is building solo with limited time. Needs to see complete picture to plan effectively.

### What Didn't Work

**1. Premature Updates**

Early on, I'd update documents immediately after each correction.

User stopped me:
> "Don't update any documentation yet, I am still reviewing your documentation. I will send my feedback as I go along, when I am done you can send me a final updated document"

**Lesson:** Wait for complete review, then one consolidated update.

**2. Incomplete File Creation**

First attempt at consolidated document hit size limits and failed mid-creation.

**Lesson:** Break large docs into focused sections. Create index + separate detailed docs.

**3. Assuming Context**

Initially provided Azure alongside AWS options, thinking user wanted comparison.

User:
> "I already decided to go with AWS, so we can ignore any of the azure options"

**Lesson:** When user has made a decision, don't second-guess. Move forward with that decision.

---

## 8. PROJECT CONTEXT & REQUIREMENTS

### Business Context

**Organization:**
- Nonprofit Jewish community organization
- Helps Orthodox/Chasidic families relocate to Union County, NJ
- Board-led approval process
- Works with brokers (commission income)
- ~50 families/year
- ~30 active property listings

**Cultural Considerations:**
- Orthodox Jewish community
- Specific terminology (Wife, Shul, Kehila)
- Most families don't use smartphones (desktop-first)
- Community agreements required
- Shul proximity very important
- References/background checks via board

**Current Process:**
- Google Forms for applications (manual, error-prone)
- Spreadsheets for tracking (data scattered)
- No property matching (manual)
- No showing tracking
- No commission tracking

**Pain Points:**
- 8-10 hours/week wasted on manual work
- Lost applications
- No visibility into pipeline
- Can't match families to properties efficiently
- No self-service for families

### Functional Requirements

**Core Workflows:**

1. **Prospect → Applicant**
   - Lead expresses interest
   - Follow up conversations
   - Eventually apply

2. **Application → Approval**
   - Family submits application
   - Board reviews
   - Approve or reject

3. **Approved → House Hunting**
   - Collect housing preferences
   - Sign agreements (broker, community)
   - Generate property matches
   - Schedule showings

4. **House Hunting → Closing**
   - Make offer
   - Under contract (30-60 days)
   - Either close OR contract falls through
   - If close: Track move-in
   - If fail: Back to house hunting

**Edge Cases:**
- On Hold (financial, emergency, timing)
- Failed Contracts (track attempts, reasons)
- Multiple tries (alert if 2+ failures)

### Non-Functional Requirements

**Performance:**
- Support 3-5 concurrent staff users
- Response time < 2 seconds
- Dashboard loads quickly

**Scalability:**
- 200 applicants/year
- 30-50 active properties
- 500+ showings/year
- Grows slowly (not viral startup)

**Availability:**
- Normal business hours (9am-5pm ET)
- Downtime acceptable for maintenance
- Not mission-critical 24/7

**Security:**
- PII protection (names, addresses, SSNs if collected)
- Role-based access
- Audit logging
- HTTPS only
- SOC 2 not required (nonprofit)

**Usability:**
- Desktop-optimized (primary use case)
- Intuitive for non-technical users
- Board members (older, less tech-savvy)
- Works on mobile but not optimized

**Cost:**
- Target: $0-25/month
- AWS free tier Year 1: ~$0.50/month (DNS)
- AWS Year 2+: ~$24/month
- Must be cheaper than CRM ($36+/month)

---

## 9. TEMPLATES & FRAMEWORKS CREATED

### Documentation Templates

**1. User Story Template**
```
### User Story X.Y: [Title]

**As a** [role]
**I want to** [action]
**So that** [benefit]

**Priority:** P0/P1/P2
**Effort:** [points]
**Sprint:** [number]

**Acceptance Criteria:**
- [Criterion 1]
- [Criterion 2]
- ...

**Technical Notes:**
```
[API endpoint, request/response, domain method code]
```

**Domain Event:** [EventName]
**Email Template:** [Template name]
```

**2. Entity Documentation Template**
```
### [Entity Name]

**Purpose:** [What it represents]

**Key Fields:**
- [Field 1]: [Description]
- [Field 2]: [Description]

**Key Methods:**
- `MethodName()` - [Description]

**Technical Notes:**
```
[C# code]
```
```

**3. PRD Template**
```
# [Project Name] - Product Requirements Document

## Executive Summary
- Purpose
- Why Custom
- Core Functions
- Success Criteria

## System Overview
- Architecture diagram
- User roles
- Workflows

## Technical Architecture
- Tech stack
- Database schema
- API design

## Domain Model
- Entities
- Value Objects
- Aggregates

## User Stories
- All stories grouped by epic
- Prioritized

## Implementation Plan
- Phases
- Sprints
- Timeline
```

### Code Templates

**1. Value Object Base Class**
```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;
        
        var other = (ValueObject)obj;
        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
}
```

**2. Entity Base Class**
```csharp
public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**3. Command Handler Template**
```csharp
public class [CommandName]Handler : IRequestHandler<[CommandName], [ResponseType]>
{
    private readonly I[Entity]Repository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public async Task<[ResponseType]> Handle([CommandName] command, CancellationToken ct)
    {
        // 1. Validate (FluentValidation handles this)
        
        // 2. Create/update domain entity
        var entity = [Entity].Create(...);
        
        // 3. Save
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // 4. Return DTO
        return _mapper.Map<[ResponseType]>(entity);
    }
}
```

**4. Repository Template**
```csharp
public class [Entity]Repository : I[Entity]Repository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<[Entity]> GetByIdAsync(Guid id)
    {
        return await _context.[Entities]
            .Include(e => e.[Navigation1])
            .Include(e => e.[Navigation2])
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    
    public async Task AddAsync([Entity] entity)
    {
        await _context.[Entities].AddAsync(entity);
    }
}
```

### Process Frameworks

**1. Sprint Planning Framework**
```
Sprint [N] ([Week X-Y])
Goal: [What we're achieving]

Stories (Points: [X]):
- Story A.B: [Title] ([Points] points)
- Story C.D: [Title] ([Points] points)

Tasks:
- [ ] Task 1
- [ ] Task 2
- [ ] Task 3

Definition of Done:
- Code complete
- Tests passing
- Code reviewed
- Deployed to dev
- Documented
```

**2. Architecture Decision Record**
```
# ADR [N]: [Title]

Date: [YYYY-MM-DD]
Status: Accepted

## Context
[What decision needs to be made and why]

## Options Considered
1. Option A: [Description]
   Pros: [...]
   Cons: [...]
   
2. Option B: [Description]
   Pros: [...]
   Cons: [...]

## Decision
[Option chosen]

## Rationale
[Why this option]

## Consequences
[Impact of decision]
```

---

## 10. NEXT STEPS IDENTIFIED

### Immediate Next Steps

**1. Finalize User Story Priorities**
User said:
> "I have not yet fully reviewed the priorities, but I will do that when I have updated requirement"

**Action:** User to review PRIORITIZED_USER_STORIES.md and adjust P0/P1/P2 as needed.

**2. Provide Domain-Specific Details**

Need from user:
- **Exact neighborhood names** for Union & Roselle Park (for Neighborhood enum)
- **List of shuls** to include in Shul table (name, address)
- Any other community-specific terminology or requirements

**3. Create Jira CSV Export**

Once priorities finalized:
- Export all 68 user stories to CSV
- Format for Jira import
- Include: Epic, Story, Priority, Points, Acceptance Criteria

**4. Review Complete Documentation**

User requested:
> "Show me the full user story listing updated with the latest changes and sample code. I want the technical specs of the entire project not only sprint 1."

Creating:
- USER_STORIES_DETAILED.md (all 68 stories)
- TECHNICAL_SPECS_COMPLETE.md (complete project specs)
- SOLUTION_STRUCTURE_AND_CODE.md (Visual Studio solution + code)

### Short-Term Next Steps (Week 1)

**1. Set Up Development Environment**
- Install .NET 10 SDK
- Install Visual Studio 2022 or Rider
- Install PostgreSQL (local dev)
- Install Node.js + npm (for React)

**2. Set Up AWS Account**
- Create AWS account
- Set up billing alerts ($1, $5, $10)
- Create IAM user (don't use root)
- Configure AWS CLI

**3. Create GitHub Repository**
- Initialize repo
- Set up .gitignore (.NET + React)
- Create initial README
- Set up branch protection

**4. Create Visual Studio Solution**
- Follow SOLUTION_STRUCTURE_AND_CODE.md
- Create 4 projects (Domain, Application, Infrastructure, API)
- Set up project references
- Install NuGet packages

### Medium-Term Next Steps (Weeks 2-8)

**Sprint 1-2: MVP Core**
- Domain entities + value objects
- EF Core DbContext + migrations
- Public application form endpoint
- Applicant CRUD endpoints
- Basic authentication (Cognito)

**Sprint 3-4: MVP Complete**
- Application pipeline (Kanban)
- Board review workflow
- Email notifications (SES)
- Basic dashboard
- Deploy to AWS

**Sprint 5-6: Phase 2 Start**
- Property management
- Property matching algorithm
- Applicant portal (Google OAuth)
- Showing scheduler

### Long-Term Next Steps (Weeks 9-24)

**Phase 2 Complete:**
- Broker management
- Commission tracking
- Follow-up reminders
- On Hold + Failed Contracts
- Prospect management

**Phase 3 Polish:**
- Advanced reports
- Bulk import
- Performance optimization
- User training

---

## 11. QUICK REFERENCE FOR FUTURE SESSIONS

### User Profile Summary

**Background:**
- Experienced software developer
- Building solo, limited time
- Nonprofit organization (cost-sensitive)
- Serving Orthodox Jewish community in Union County, NJ

**Technical Preferences:**
- .NET 10 + C# 13
- AWS (not Azure)
- React + TypeScript (not Blazor)
- PostgreSQL
- Clean Architecture + DDD + CQRS
- Desktop-first (no mobile optimization)

**Working Style:**
- Direct, efficient communication
- Reviews thoroughly before committing
- Sends incremental feedback
- Wants consolidated final update (not piecemeal)
- Values comprehensive documentation with code samples
- Detail-oriented, catches edge cases
- Makes practical, informed decisions

**Domain Preferences:**
- Proper domain terminology (Applicant, Application, not Contact, Deal)
- "Wife" not "Spouse"
- Value objects for all key concepts
- Board review at Applicant level
- Desktop-first (Chasidic families don't use smartphones)

### Project Quick Facts

**What:** Custom CRM for Jewish family relocation to Union County, NJ

**Why Custom:** All off-the-shelf CRMs rejected (Monday.com nonprofit denied, others too expensive/complex)

**Tech Stack:**
- Backend: .NET 10, ASP.NET Core, PostgreSQL, EF Core, Clean Architecture
- Frontend: React 18, TypeScript, Ant Design, Vite
- Cloud: AWS (RDS, S3, SES, Cognito, CloudWatch)
- Cost: $0-25/month

**Core Features:**
- Prospect tracking
- Application workflow (6 stages)
- Board approval
- Property matching algorithm
- Showing scheduler
- Broker management
- Commission tracking
- Applicant portal (Google OAuth)
- Follow-up reminders
- On Hold + Failed Contracts

**Timeline:**
- MVP: 6-8 weeks
- Phase 2: 10-12 weeks
- Phase 3: 3-4 weeks
- Total: 19-24 weeks

**User Stories:** 68 stories, 11 epics, ~280 points

### Key Documents Created

**Requirements:**
1. MASTER_REQUIREMENTS_FINAL.md (70 pages)
2. PRIORITIZED_USER_STORIES.md (60 pages)
3. SECURITY_ARCHITECTURE.md (30 pages)
4. FRONTEND_FRAMEWORK_COMPARISON.md (40 pages)
5. UPDATED_WORKFLOWS_EDGE_CASES.md (40 pages)

**In Progress:**
6. USER_STORIES_DETAILED.md (all 68 stories with code)
7. TECHNICAL_SPECS_COMPLETE.md (complete project specs)
8. SOLUTION_STRUCTURE_AND_CODE.md (VS solution + code samples)

### Critical Design Decisions

**Domain Model:**
- Applicant (not Contact) - the family
- Application (not Deal) - the journey
- Proper value objects (Address, PhoneNumber, Email, Money, Child, Coordinates)
- Board review on Applicant level
- Phone numbers as collection with type
- Children as collection with age + gender

**Workflow:**
- 6 stages: UnderReview → Approved → HouseHunting → OnHold → UnderContract → Closed
- Edge case: On Hold (can resume or withdraw)
- Edge case: Failed Contracts (track attempts, back to HouseHunting)
- Under Contract is separate stage (was missing initially)

**Technical:**
- .NET 10 (latest LTS)
- AWS only (more generous free tier)
- React (42% market share > Blazor's 2-3%)
- Desktop-first (no mobile optimization)
- No SMS (families don't use smartphones)
- Google OAuth for applicants
- AWS SES for email (62k/month free)

### Common Patterns

**When User Says:**
- "I will send my feedback as I go along" → Don't update docs yet, wait for complete review
- "Can you include [X]" → Add to requirement list, incorporate in final update
- "As of [time], latest is [version]" → User knows current state, update immediately
- "I already decided [X]" → Don't second-guess, proceed with that decision
- "Most Chasidic families..." → Cultural context, trust user's domain knowledge

**What User Values:**
- Comprehensive documentation (not summaries)
- Code samples (show don't tell)
- Organized structure (TOCs, sections)
- Practical solutions (not theoretical)
- Cost-effectiveness
- Proper DDD/architecture
- Complete picture before starting

**What to Avoid:**
- Updating docs mid-review
- Assuming without asking
- CRM terminology (Contact, Deal, Lead)
- Mobile-first assumptions
- Azure over AWS
- Blazor over React
- Fluff or excessive explanation

---

## 12. LESSONS LEARNED

### Technical Lessons

**1. Market Share Matters for Solo Developers**

User was RIGHT about Blazor concern:
- Blazor: 2-3% market share, 15k Stack Overflow questions
- React: 42% market share, 500k Stack Overflow questions
- 10-20 hours saved over project finding React answers vs Blazor

**Lesson:** For solo developer with limited time, community size = time saved = money saved.

**2. Cultural Context Changes Everything**

"Most Chasidic families don't use smartphones"
→ Desktop-first design
→ No mobile optimization needed
→ No SMS notifications
→ No PWA/offline
→ Saved 1-2 weeks development

**Lesson:** Don't assume. Ask about user context.

**3. Domain Language is Non-Negotiable**

"Contact" → Applicant
"Deal" → Application
"Spouse" → Wife

**Lesson:** Use the language of the domain experts, not generic software terms.

**4. Latest != Best (But Often Is)**

.NET 10 over .NET 8:
- Latest LTS
- Better performance
- New features
- No downside

**Lesson:** For greenfield projects, use latest stable.

**5. Free Tier Research Pays Off**

AWS SES: 62k emails/month free
vs SendGrid: 100/day free

**Lesson:** Research free tiers thoroughly. Differences can be huge.

### Process Lessons

**1. Wait for Complete Review**

User said:
> "Don't update any documentation yet"

Early on, I updated after each correction. Wrong approach.

**Lesson:** Let user finish review, then ONE consolidated update.

**2. Comprehensive > Iterative (For This User)**

User wanted:
- Complete PRD upfront
- All 68 user stories detailed
- Complete technical specs
- Full solution structure

Not:
- "Let's start with basics and iterate"

**Lesson:** Different users have different preferences. This user wants full picture.

**3. Code Samples Are Essential**

User specifically requested:
> "Sample code. I want the technical specs of the entire project"

**Lesson:** For technical users, code samples clarify what words cannot.

**4. Track All Feedback in One Place**

Created running list:
1. .NET 10 (not 8)
2. AWS only
3. Wife not Spouse
4. Under Contract stage missing
... (29 corrections total)

**Lesson:** Makes consolidated update easier and ensures nothing forgotten.

### Collaboration Lessons

**1. User is Domain Expert**

User knows:
- Their community's culture
- Their workflow better than any CRM
- What terminology is correct
- Which features matter

**Lesson:** Defer to domain expertise.

**2. Technical Discussion at Right Level**

User knows CQRS, DDD, Clean Architecture.

Could discuss:
- Aggregate boundaries
- Value object design
- Repository pattern
- Domain events

**Lesson:** Match technical level to user.

**3. Practical Over Perfect**

User chose:
- AWS over Azure (simpler)
- React over Blazor (bigger community)
- Desktop over mobile (matches usage)

Not chasing perfection, chasing "good enough that works."

**Lesson:** Pragmatism is valuable.

---

## FINAL SUMMARY

### What We Built Together

**From:** "I need a CRM"

**To:** Complete technical specifications for custom system:
- 300+ pages of documentation
- 68 user stories across 11 epics
- Complete domain model (12+ entities, 6+ value objects)
- Full technical architecture (.NET 10 + React + AWS)
- Database schema (12+ tables)
- API design (50+ endpoints)
- Security architecture
- Implementation roadmap (19-24 weeks)
- Cost estimate ($0-25/month)

### Success Factors

**1. User's Clarity**
- Knew what they wanted
- Made decisions quickly
- Provided detailed feedback
- Corrected assumptions

**2. Iterative Refinement**
- Started with research (CRMs)
- Tested real options (Monday.com)
- Got rejected (nonprofit denied)
- Made final decision (build custom)
- Refined requirements (29 corrections)

**3. Proper DDD**
- Domain language
- Value objects
- Proper aggregates
- Bounded contexts
- Ubiquitous language

**4. Practical Choices**
- AWS free tier
- React (community size)
- Desktop-first
- No unnecessary features

### What Makes This Collaboration Work

**User Brings:**
- Domain expertise
- Technical knowledge
- Clear decisions
- Detailed feedback
- Practical focus

**I Bring:**
- Comprehensive documentation
- Code samples
- Architecture guidance
- Technology research
- Organization/structure

**Together:**
- User makes informed decisions
- I capture requirements accurately
- We build proper technical solution
- Everything documented thoroughly

---

## SESSION: January 15-16, 2026 - Domain Entity Refactoring

### Context
Continued development on Sprint 1 (UV-11: Core Domain Entities). Major refactoring of domain model based on user feedback.

### Key Refactorings Completed

**1. Application → HousingSearch Rename**
- "Application" implied one-time application; "HousingSearch" better represents the journey
- Single HousingSearch per applicant (1:1 relationship)
- Failed contracts preserved in `FailedContracts` collection

**2. HusbandInfo and SpouseInfo Value Objects**
- Extracted from Applicant to properly encapsulate person info
- Contains: FirstName, LastName/MaidenName, FatherName, Email, PhoneNumbers, Occupation, EmployerName
- Stored as jsonb columns in PostgreSQL
- Name formatting: `HusbandInfo.FullNameWithFather` → "Moshe Cohen (ben Yaakov)"
- Name formatting: `SpouseInfo.FullName` → "Sarah (Goldstein)" with maiden name in parens

**3. Contract and HousingPreferences Value Objects**
- `Contract`: PropertyId, Price, ContractDate, ExpectedClosingDate, ActualClosingDate
- `HousingPreferences`: Budget, MinBedrooms, MinBathrooms, RequiredFeatures, ShulProximity, MoveTimeline
- `FailedContractAttempt` refactored to contain `Contract` + FailedDate + Reason

**4. HousingSearchStage State Machine**
- Added `Rejected` stage (Submitted → Rejected transition)
- Renamed `Closing` → `Closed` (represents completed closing, not in-progress)
- Full transitions: Submitted → HouseHunting/Rejected, HouseHunting → UnderContract/Paused, etc.

**5. Child Value Object Simplified**
- Age and Gender required (from application form)
- Name and School optional
- Removed Grade and Notes properties

**6. Cleanup**
- Removed `ProspectId` from Applicant (future feature)
- Removed 12 unused enums (EntityType, ListingStatus, PropertyType, etc.)
- Removed `SearchNumber` from HousingSearch (redundant with 1:1 relationship)

### Current Domain Model

**Aggregate Roots:**
- `Applicant`: HusbandInfo, Wife (SpouseInfo), Address, Children, BoardReview, audit fields
- `HousingSearch`: Stage, CurrentContract, FailedContracts, Preferences, MovedInStatus, audit fields

**Value Objects (13 total):**
- HusbandInfo, SpouseInfo, BoardReview
- Contract, HousingPreferences, FailedContractAttempt
- Address, Email, PhoneNumber, Money, Child, Coordinates, ShulProximityPreference

**Enums (4 active):**
- BoardDecision, HousingSearchStage, MovedInStatus, MoveTimeline

**Domain Events (5):**
- ApplicantCreated, ApplicantBoardDecisionMade
- HousingSearchStarted, HousingSearchStageChanged, HousingPreferencesUpdated

### Test Results
All 231 tests pass (194 Domain + 25 API + 12 Integration)

### EF Core Configuration
- HusbandInfo/SpouseInfo stored as jsonb columns
- Contract/HousingPreferences/FailedContracts stored as jsonb columns
- Address/BoardReview as owned entity types (flattened columns)
- Children as jsonb array

### PR Created
UV-11: Implement core domain entities - https://github.com/adrottenberg/family-relocation/pull/4

---

## FOR NEXT SESSION

### To Quickly Re-Establish Context

**Just say:**
> "I'm the developer building the Family Relocation CRM for the Jewish community in Union County. We documented everything in January 2026."

**I'll know:**
- Complete domain model (Applicant, HousingSearch, etc.)
- Tech stack (.NET 10, React, AWS)
- Your working style (comprehensive docs, wait for complete review)
- All 68 user stories and priorities
- The 29 corrections we made
- Cultural context (Orthodox community, no smartphones, desktop-first)
- HousingSearch represents the house-hunting journey with failed contract history

**And we can pick up exactly where we left off.**

---

## SESSION: January 16, 2026 - UV-13 Create Applicant Endpoint & Infrastructure Refactoring

### Context
Implemented UV-13 (US-006: Create Applicant endpoint) and performed major infrastructure refactoring from traditional repository pattern to query object pattern.

### Create Applicant Endpoint (UV-13)

**Implemented:**
- `POST /api/applicants` endpoint with `[AllowAnonymous]` for public board approval applications
- `CreateApplicantCommand` and `CreateApplicantCommandHandler`
- `CreateApplicantCommandValidator` with FluentValidation rules
- Full DTO structure: ApplicantDto, HusbandInfoDto, SpouseInfoDto, AddressDto, ChildDto, PhoneNumberDto

**Key Design Decisions:**

1. **Anonymous Access with Audit Trail**
   - Endpoint is publicly accessible for self-submitted board approval applications
   - `WellKnownIds.SelfSubmittedUserId` (GUID: `AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA`) used for CreatedBy
   - `IsSelfSubmitted` computed property on Applicant entity returns true when CreatedBy matches well-known ID
   - Well-known GUID cannot collide with UUID v4 (version bits make it structurally impossible)

2. **Duplicate Email Checking**
   - Checks both husband AND wife emails for duplicates
   - Uses PostgreSQL JSONB query since emails are stored in jsonb columns
   - `ExistsByEmailQuery` handles the duplicate check

3. **Auto-Demote Multiple Primary Phone Numbers**
   - Instead of rejecting multiple primary phones, first one marked as primary wins
   - `NormalizePhoneNumbers` helper method in handler demotes subsequent primaries
   - More user-friendly than validation rejection

### Infrastructure Refactoring: Query Object Pattern

**Replaced traditional repository pattern with Mark Seemann's query object approach:**

**Before:**
```
IApplicantRepository interface with methods like:
- ExistsWithEmailAsync(string email)
- AddAsync(Applicant applicant)
- etc. (constantly growing interface)
```

**After:**
```
Query objects via MediatR:
- ExistsByEmailQuery record in Application layer
- ExistsByEmailQueryHandler in Application layer
- IApplicationDbContext with generic Set<T>() method
```

**Why Query Object Pattern:**
- No constantly evolving IRepository interfaces
- Each query is explicit and self-documenting
- All handlers in Application layer (where they belong - handlers ARE the application)
- MediatR handles dispatch automatically

### Additional Refactoring: EF Core ToJson() for LINQ on JSON Columns

**Problem:** Initially handlers that needed to query JSONB columns required raw SQL, forcing them into Infrastructure layer. User didn't like split between Application and Infrastructure handlers.

**Solution:** Configure HusbandInfo/SpouseInfo with EF Core's `ToJson()`:
```csharp
builder.OwnsOne(a => a.Husband, husband =>
{
    husband.ToJson();
    husband.OwnsOne(h => h.Email);
    husband.OwnsMany(h => h.PhoneNumbers);
});
```

**Result:** LINQ queries work on JSON columns - no raw SQL needed:
```csharp
// This now works!
await _context.Set<Applicant>().AnyAsync(a =>
    a.Husband.Email != null && a.Husband.Email.Value.ToLower() == email);
```

### IApplicationDbContext Design (Open/Closed Principle)

**Problem:** Original design had entity-specific properties (`Applicants`, `HousingSearches`). Adding new entities would require modifying the interface.

**Solution:** Generic `Set<T>()` method:
```csharp
public interface IApplicationDbContext
{
    IQueryable<T> Set<T>() where T : class;
    void Add<T>(T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### ICurrentUserService.UserId is Nullable

**Problem:** Originally returned `WellKnownIds.SelfSubmittedUserId` for anonymous users, coupling the service to a specific use case.

**Solution:** Return `Guid?` - let consumers decide the fallback:
```csharp
public interface ICurrentUserService
{
    Guid? UserId { get; }  // null if not authenticated
    // ...
}

// Consumer decides:
createdBy: _currentUserService.UserId ?? WellKnownIds.SelfSubmittedUserId
```

### Final Files Structure

```
Application/
  Applicants/
    Commands/
      CreateApplicant/
        CreateApplicantCommand.cs
        CreateApplicantCommandHandler.cs
        CreateApplicantCommandValidator.cs
    DTOs/
      AddressDto.cs, ApplicantDto.cs, ChildDto.cs,
      HusbandInfoDto.cs, PhoneNumberDto.cs, SpouseInfoDto.cs
    Queries/
      ExistsByEmail/
        ExistsByEmailQuery.cs
        ExistsByEmailQueryHandler.cs  (ALL handlers in Application)
  Common/
    Exceptions/
      DuplicateEmailException.cs
    Interfaces/
      IApplicationDbContext.cs  (generic Set<T>, Add<T>, SaveChangesAsync)
      ICurrentUserService.cs    (nullable UserId)

Domain/
  Common/
    WellKnownIds.cs  (SelfSubmittedUserId constant)

Infrastructure/
  Persistence/
    Configurations/
      ApplicantConfiguration.cs  (uses ToJson() for HusbandInfo/SpouseInfo)
```

### Test Results
All 259 tests pass (196 Domain + 51 API + 12 Integration)

### PR Created
UV-13: Implement Create Applicant endpoint - https://github.com/adrottenberg/family-relocation/pull/5

### Key Takeaways

1. **Query Object Pattern** - MediatR queries instead of repository interfaces
2. **ALL handlers in Application** - handlers ARE the application logic
3. **EF Core ToJson()** - enables LINQ queries on JSON columns, no raw SQL
4. **Generic IApplicationDbContext** - `Set<T>()` follows Open/Closed principle
5. **Nullable ICurrentUserService.UserId** - consumers decide fallback for anonymous
6. **Well-known GUIDs** - for system-level identifiers (cannot collide with UUID v4)

---

## FOR NEXT SESSION

### To Quickly Re-Establish Context

**Just say:**
> "I'm the developer building the Family Relocation CRM for the Jewish community in Union County. We documented everything in January 2026."

**I'll know:**
- Complete domain model (Applicant, HousingSearch, etc.)
- Tech stack (.NET 10, React, AWS)
- Your working style (comprehensive docs, wait for complete review)
- All 68 user stories and priorities
- The 29 corrections we made
- Cultural context (Orthodox community, no smartphones, desktop-first)
- HousingSearch represents the house-hunting journey with failed contract history
- **Query object pattern** instead of repository pattern (Mark Seemann's approach)
- **All handlers in Application layer** - no split with Infrastructure
- **EF Core ToJson()** for LINQ queries on JSON columns
- **Generic IApplicationDbContext** with `Set<T>()` (Open/Closed principle)

**And we can pick up exactly where we left off.**

---

---

## SESSION: January 17, 2026 - UV-14 & UV-15 Implementation + Authentication Refactoring

### Context
Continued Sprint 1 development. Implemented UV-14 (View Applicant Details) and UV-15 (Update Applicant Basic Info), plus AWS Cognito authentication refactoring plan.

### UV-14: View Applicant Details (US-007)

**Implemented:**
- `GET /api/applicants/{id}` endpoint
- `GetApplicantByIdQuery` and `GetApplicantByIdQueryHandler`
- Added `BoardReviewDto` for complete response
- Added `FullAddress` computed property to `AddressDto`

**Key Design Decisions:**

1. **FullAddress as Computed Property**
   - User feedback: "The full address should be a calculated property, no point in having it as a separate data field"
   - Changed from stored field to: `public string FullAddress => $"{Street}, {City}, {State} {ZipCode}";`

2. **MemberNotNullWhen Attribute for Nullable Contracts**
   - User questioned null-forgiving operators (`!`) in AuthController
   - Added `[MemberNotNullWhen(true, nameof(Property))]` to result types
   - Applied to: `AuthResult`, `TokenRefreshResult`, `RegisterUserResult`
   - Compiler now knows properties are non-null when `Success == true`

### UV-15: Update Applicant Basic Info (US-008)

**Implemented:**
- `PUT /api/applicants/{id}` endpoint with `[Authorize]` (admin/coordinator only)
- `UpdateApplicantCommand` and `UpdateApplicantCommandHandler`
- `UpdateApplicantCommandValidator` with FluentValidation (20 tests)
- `NotFoundException` for 404 responses
- Email uniqueness validation (excluding current applicant)
- 5 controller integration tests

**Key Implementation Details:**
- Handler validates email uniqueness against all other applicants
- Uses `ICurrentUserService.UserId` for audit trail
- Full CRUD cycle now complete for Applicants

### Mapper Consolidation Refactoring

**Problem:** Duplicate mapping code across handlers (600+ lines total):
- CreateApplicantCommandHandler: 228 lines
- UpdateApplicantCommandHandler: 262 lines
- GetApplicantByIdQueryHandler: 118 lines

**Solution:** Centralized `ApplicantMapper` with extension methods:
```csharp
// Before (static method)
ApplicantMapper.ToDto(applicant)
ApplicantMapper.ToDomain(request.Husband)

// After (extension method)
applicant.ToDto()
request.Husband.ToDomain()
```

**Results:**
- CreateApplicantCommandHandler: 228 → 68 lines
- UpdateApplicantCommandHandler: 262 → 97 lines
- GetApplicantByIdQueryHandler: 118 → 25 lines
- Single source of truth for all mappings
- Auto-demote multiple primary phone numbers in one place

### Authentication Architecture Plan (UV-9)

**Created detailed refactoring plan:**
1. Extract `ComputeSecretHash` to Infrastructure as `HmacHelper`
2. Create `IAuthenticationService` interface in Application layer
3. Implement `CognitoAuthenticationService` in Infrastructure layer
4. Support multiple challenge types with user-friendly field names:
   - `newPassword` → `NEW_PASSWORD` (for NEW_PASSWORD_REQUIRED)
   - `mfaCode` → `SMS_MFA_CODE` (for SMS_MFA)
   - `totpCode` → `SOFTWARE_TOKEN_MFA_CODE` (for SOFTWARE_TOKEN_MFA)
5. Create `ChallengeMetadata` for mapping user-friendly names to Cognito keys

**Plan file:** `C:\Users\adrot\.claude\plans\typed-herding-lighthouse.md`

### API Testing Discovery

**JWT Authentication Requires HTTPS:**
- HTTP calls to `http://localhost:5267` returned 401 even with valid JWT
- HTTPS calls to `https://localhost:7267` worked correctly
- Cognito tokens validated properly only over HTTPS
- Used `curl -k` to bypass SSL certificate validation for local testing

### Test Results
All 287 tests pass (196 Domain + 79 API + 12 Integration)

### PR Created
UV-15: Update applicant basic info - https://github.com/adrottenberg/family-relocation/pull/new/feature/UV-15_update_applicant_basic_info

### Key Takeaways

1. **Extension Methods for Mappers** - Cleaner syntax: `applicant.ToDto()` vs `ApplicantMapper.ToDto(applicant)`
2. **MemberNotNullWhen** - Compiler-enforced nullable contracts eliminate null-forgiving operators
3. **Computed Properties** - Use for derived values, not stored fields
4. **HTTPS Required for JWT** - Cognito token validation requires secure transport
5. **Centralized Mapping** - Single source of truth prevents drift and reduces maintenance

---

## FOR NEXT SESSION

### To Quickly Re-Establish Context

**Just say:**
> "I'm the developer building the Family Relocation CRM for the Jewish community in Union County. We documented everything in January 2026."

**I'll know:**
- Complete domain model (Applicant, HousingSearch, etc.)
- Tech stack (.NET 10, React, AWS)
- Your working style (comprehensive docs, wait for complete review)
- All 68 user stories and priorities
- The 29 corrections we made
- Cultural context (Orthodox community, no smartphones, desktop-first)
- HousingSearch represents the house-hunting journey with failed contract history
- **Query object pattern** instead of repository pattern (Mark Seemann's approach)
- **All handlers in Application layer** - no split with Infrastructure
- **EF Core ToJson()** for LINQ queries on JSON columns
- **Generic IApplicationDbContext** with `Set<T>()` (Open/Closed principle)
- **ApplicantMapper extension methods** for DTO conversions
- **MemberNotNullWhen** for nullable result types

**And we can pick up exactly where we left off.**

---

**END OF CONVERSATION MEMORY LOG**

This document captures our complete collaboration. Use it to quickly re-establish context in future sessions.
