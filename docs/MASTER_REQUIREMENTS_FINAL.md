# FAMILY RELOCATION SYSTEM - MASTER REQUIREMENTS DOCUMENT
## Complete Technical Specifications - Version 2.0 Final

> **Note (January 2026):** This document contains the original business requirements.
> The system is now ~85% implemented. For current implementation status, see:
> - [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) - What's built vs outstanding
> - [SOLUTION_STRUCTURE_AND_CODE_v3.md](SOLUTION_STRUCTURE_AND_CODE_v3.md) - Current code reference
> - Archived corrections have been applied to the codebase

**Date:** January 9, 2026
**Status:** ✅ Original Requirements (Implementation ~85% Complete)
**All Feedback Incorporated:** YES

---

## 📋 EXECUTIVE SUMMARY

### What This Document Contains

This is the **master requirements document** for your custom Family Relocation System. All your feedback has been incorporated. You now have complete technical specifications ready to begin development.

**Total Documentation:** 300+ pages across organized sections  
**User Stories:** 68 stories across 11 epics  
**Development Timeline:** 19-24 weeks (part-time)  
**Estimated Cost:** $0-25/month (AWS)

---

## ✅ ALL YOUR FEEDBACK INCORPORATED

### Technical Stack Updates
- ✅ **.NET 10** (not .NET 8) - latest LTS
- ✅ **AWS only** - all Azure references removed
- ✅ **AWS SES** for email (62k emails/month free)
- ✅ **PostgreSQL** on AWS RDS
- ✅ **React + TypeScript** frontend (NOT Blazor)
- ✅ **Desktop-first** - mobile not priority (no PWA)

### Domain Model Updates (DDD)
- ✅ **"Applicant"** not "Contact" (proper domain language)
- ✅ **"Application"** not "Deal" (proper domain language)
- ✅ **Prospect** entity added (pre-application tracking)
- ✅ **Wife** naming consistent (not "Spouse")
- ✅ **Children** as collection with age + gender
- ✅ **Single Budget field** (max only, removed min)
- ✅ **Board review fields** on Applicant level (not Application)
- ✅ **Phone numbers** as collection with type (Home/Cell/Work)

### Value Objects (Proper DDD)
- ✅ **Address** value object (not string)
- ✅ **PhoneNumber** value object with type
- ✅ **Email** value object with validation
- ✅ **Money** value object with operators
- ✅ **Child** value object (age + gender)
- ✅ **Coordinates** value object (for shul distance)
- ✅ **ShulProximityPreference** value object

### Workflow Updates
- ✅ **Under Contract stage** added (was missing!)
- ✅ **On Hold** scenario fully documented
- ✅ **Failed Contracts** handling added
- ✅ **6 stages** total (+ Rejected)

### Features Added
- ✅ **Google authentication** for applicant portal
- ✅ **Applicant portal** (self-service)
- ✅ **Follow-up reminders** (3 user stories)
- ✅ **Shul proximity** preferences with distance
- ✅ **Property taxes** + monthly payment calculator
- ✅ **Broker entity** (not string name)
- ✅ **Agreement document uploads** required
- ✅ **Union & Roselle Park** specific neighborhoods

### Enums Simplified
- ✅ **Removed ApprovedWithConditions** from BoardDecision
- ✅ **Added UnderContractThroughUs** to ListingStatus  
- ✅ **Shortened MovedInStatus** (removed "LivingThere")
- ✅ **MoveTimeline** now enum (not string)

---

## 📚 DOCUMENT STRUCTURE

This master document organizes all requirements into logical sections. Each section is complete and ready for implementation.

---

## PART 1: OVERVIEW & BUSINESS REQUIREMENTS

### 1.1 Executive Summary

**Purpose:**  
Build custom system to manage Jewish family relocation to Union County, NJ from prospect contact through closing and move-in.

**Why Custom vs CRM:**
- Monday.com nonprofit application rejected
- All CRMs too generic, wrong terminology, expensive
- Custom = $0/month vs $36+/month
- Complete control, exact workflow match
- Own all data, no vendor lock-in

**Success Criteria:**
- Process 50+ families/year
- Manage 30+ active properties
- Save 8-10 hours/week
- 100% data accuracy
- Desktop-optimized (Chasidic families don't use smartphones)
- Cost: $0-25/month

---

### 1.2 User Roles

**1. Public (Unauthenticated)**
- Submit application form at /apply
- View confirmation page
- No account needed

**2. Applicant (Google OAuth)**
- Self-service portal
- View application status
- Update housing preferences
- View property matches sent to them
- Rate properties (interested/not)
- View scheduled showings
- Upload documents
- Cannot see other families
- Cannot see all properties

**3. Coordinator (Staff - Primary Role)**
- Manage prospects
- Manage applicants & applications
- Manage properties
- Schedule showings
- Create property matches
- Track commissions
- Generate reports
- Create follow-up reminders
- Cannot delete data
- Cannot manage users

**4. Board Member (Staff - Review Only)**
- View applicants (read-only)
- View applications (read-only)
- Approve/reject applications
- Add board review notes
- View reports
- Cannot edit data
- Cannot create properties

**5. Administrator (Staff - Full Access)**
- Everything Coordinators can do
- User management
- System configuration
- Delete data (with confirmation)
- Export all data
- View audit logs
- Manage email templates

---

### 1.3 Core Workflows

#### Workflow 1: Prospect → Applicant
```
PROSPECT CREATED
│
├─ Someone expresses interest (referral, website, event)
├─ Coordinator creates Prospect record
├─ Follow-up conversations (multiple touchpoints)
├─ Status updates: InitialContact → InDiscussion → Interested → ReadyToApply
│
└─ APPLY
   ├─ Prospect submits application
   ├─ Create Applicant record (links to Prospect)
   ├─ Create Application record
   └─ Conversion complete
```

#### Workflow 2: Application → Approval
```
APPLICATION SUBMITTED
│
├─ Form submission creates Applicant + Application
├─ Stage: "Under Review"
├─ Coordinator reviews
├─ Board reviews
├─ Board makes decision
│
├─ If APPROVED:
│  ├─ Stage: "Approved - Waiting For Paperwork"
│  ├─ Send approval email
│  ├─ Request housing preferences
│  └─ Proceed to Workflow 3
│
└─ If REJECTED:
   ├─ Stage: "Rejected" (auto-moved)
   ├─ Send rejection email
   └─ END
```

#### Workflow 3: Approved → House Hunting
```
APPROVED
│
├─ Applicant receives approval email
├─ Logs into portal
├─ Updates housing preferences (13 fields)
├─ Uploads broker agreement (required)
├─ Uploads community agreement (required)
├─ Coordinator validates
│
├─ Stage: "House Hunting"
├─ System auto-generates property matches (algorithm)
├─ Coordinator reviews/sends matches
├─ Applicant rates properties in portal
├─ Coordinator schedules showings
├─ Showings happen, feedback logged
│
└─ Applicant decides on property → Workflow 4
```

#### Workflow 4: House Hunting → Closing
```
OFFER ACCEPTED
│
├─ Stage: "Under Contract"
│  Required: Property address, contract date, price, expected closing
│
├─ Contract period (30-60 days)
│  ├─ Home inspection
│  ├─ Mortgage approval
│  ├─ Attorney review
│  └─ Title search
│
├─ TWO PATHS:
│
├─ PATH A: SUCCESS ✅
│  ├─ Stage: "Closed"
│  │  Required: Actual closing date, final price, commission
│  ├─ Track: Moved In Status, Move-In Date
│  └─ SUCCESS! Family relocated 🎉
│
└─ PATH B: CONTRACT FAILS ❌
   ├─ "Contract Fell Through" button
   ├─ Save failure details (reason, property, impact)
   ├─ Stage: BACK to "House Hunting"
   ├─ Re-generate matches (exclude failed property)
   └─ Try again
```

#### Edge Case: On Hold
```
FROM: Any stage (usually House Hunting)
│
├─ Applicant needs to pause (financial, emergency, timing)
├─ Stage: "On Hold"
├─ Record: Reason, duration, expected resume date
├─ Create reminder for follow-up
│
├─ OUTCOME A: Resume
│  ├─ Stage: Back to "House Hunting"
│  ├─ Re-generate matches
│  └─ Continue
│
└─ OUTCOME B: Permanent
   ├─ Stage: "Rejected/Withdrawn"
   └─ END
```

---

### 1.4 Pipeline Stages

**6 Active Stages + 1 Terminal:**

1. **Under Review** (Blue) - Application submitted, awaiting board
2. **Approved - Waiting For Paperwork** (Green) - Board approved, need agreements
3. **House Hunting** (Orange) - Actively searching for property
4. **On Hold** (Gray) - Paused temporarily
5. **Under Contract** (Light Blue) - Offer accepted, awaiting closing
6. **Closed** (Dark Green) - Successfully closed, family relocated ✅
7. **Rejected/Withdrawn** (Red) - Not approved or withdrew ❌

**Stage Transitions:**
- 1 → 2: Board approves
- 1 → 7: Board rejects (auto)
- 2 → 3: Agreements signed + uploaded
- 3 → 4: Pause search
- 3 → 5: Offer accepted
- 4 → 3: Resume search
- 4 → 7: Permanent withdrawal
- 5 → 6: Closing happens
- 5 → 3: Contract falls through
- 5 → 7: Applicant backs out

---

## PART 2: TECHNICAL ARCHITECTURE

### 2.1 Technology Stack

#### Backend (.NET 10)
```
Platform: .NET 10 (LTS - November 2024)
├── Language: C# 13
├── Framework: ASP.NET Core 10 Web API
├── ORM: Entity Framework Core 10
├── Database: PostgreSQL 16
├── Architecture: Clean Architecture + DDD
├── Patterns: CQRS, Repository, Unit of Work, Domain Events
├── Messaging: MediatR
├── Validation: FluentValidation
├── Mapping: AutoMapper
└── Testing: xUnit, Moq, FluentAssertions
```

#### Frontend (React)
```
Framework: React 18
├── Language: TypeScript 5
├── Build: Vite
├── Routing: React Router v6
├── State: Zustand (not Redux - simpler)
├── API: TanStack Query (React Query)
├── UI: Ant Design (perfect for admin panels)
├── Styling: Tailwind CSS
├── Forms: React Hook Form
├── Kanban: react-beautiful-dnd
├── Charts: Recharts
└── Auth: AWS Amplify (Cognito)
```

#### AWS Services (NO AZURE)
```
Compute: AWS Elastic Beanstalk (.NET 10)
Database: AWS RDS PostgreSQL (db.t3.micro, 750 hrs/month free)
Storage: AWS S3 (5 GB free) + CloudFront (CDN)
Email: AWS SES (62,000 emails/month FREE)
Auth: AWS Cognito (50k MAU free, supports Google OAuth)
Monitoring: AWS CloudWatch
DNS: AWS Route 53 ($0.50/month)
```

#### Cost Estimate
```
First 12 Months: $0.50/month (just DNS)
After 12 Months: $15-25/month (still very cheap)
```

---

### 2.2 Architecture Layers (Clean Architecture)

```
┌─────────────────────────────────────┐
│    FamilyRelocation.API             │  ← HTTP/REST Layer
│    - Controllers                    │
│    - Middleware (auth, logging)     │
│    - DTOs for requests/responses    │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│  FamilyRelocation.Application       │  ← Use Cases
│  - Commands (CreateApplicant, etc)  │
│  - Queries (GetApplicantById, etc)  │
│  - Handlers (MediatR)                │
│  - DTOs                              │
│  - Validators (FluentValidation)    │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│    FamilyRelocation.Domain          │  ← Business Logic
│    - Entities (Applicant, etc)      │
│    - Value Objects (Address, etc)   │
│    - Domain Services                 │
│    - Domain Events                   │
│    - Repository Interfaces           │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│  FamilyRelocation.Infrastructure    │  ← External Services
│  - DbContext (EF Core)               │
│  - Repository Implementations        │
│  - AWS Services (SES, S3, Cognito)  │
│  - Background Jobs                   │
└─────────────────────────────────────┘
```

---

### 2.3 Security Architecture

#### Authentication Flows

**Public (No Auth):**
- `/apply` - application form
- `/application-submitted` - confirmation

**Applicant Portal (Google OAuth via Cognito):**
- Login with Google button
- Cognito handles OAuth flow
- Returns JWT token
- Limited API access: `/api/applicant-portal/*`

**Staff (Email/Password via Cognito):**
- Email + password login
- Optional MFA for admins
- Returns JWT token
- Full API access based on role

**API Security:**
- Every request validates JWT with Cognito
- Extract claims (userId, role, email)
- Role-based authorization on endpoints
- Audit logging for all actions

---

## PART 3: COMPLETE DOMAIN MODEL (DDD)

### 3.1 Ubiquitous Language

**Our Domain (NOT CRM terms):**
- ✅ Applicant (family applying)
- ✅ Application (journey from submission to closing)
- ✅ Prospect (lead before applying)
- ✅ Property (available home)
- ✅ Showing (property viewing)
- ✅ Broker (real estate agent)
- ✅ PropertyMatch (match applicant to property)
- ✅ FailedContract (contract that fell through)
- ✅ Shul (synagogue)
- ✅ FollowUpReminder (task)

**NOT Used:**
- ❌ Contact (CRM term)
- ❌ Deal (CRM term)
- ❌ Lead (CRM term)
- ❌ Opportunity (CRM term)

---

### 3.2 Value Objects

All implemented as proper DDD value objects with:
- Immutability
- Value equality (not reference)
- Validation in constructor
- No identity

#### Address
```csharp
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string? Unit { get; private set; }
    
    public string FullAddress => Unit != null 
        ? $"{Street}, Unit {Unit}, {City}, {State} {ZipCode}"
        : $"{Street}, {City}, {State} {ZipCode}";
}
```

#### PhoneNumber (with Type)
```csharp
public class PhoneNumber : ValueObject
{
    public string Number { get; private set; }  // Formatted: (XXX) XXX-XXXX
    public PhoneType Type { get; private set; } // Home, Cell, Work, Other
    
    public string DisplayName => $"{Number} ({Type})";
}
```

#### Email
```csharp
public class Email : ValueObject
{
    public string Value { get; private set; } // Validated, lowercase
}
```

#### Money
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } // Default: USD
    
    public string Formatted => $"${Amount:N2}";
    
    // Operators: +, -, *, >, <
}
```

#### Child (for Children collection)
```csharp
public class Child : ValueObject
{
    public int Age { get; private set; }
    public Gender Gender { get; private set; } // Male, Female
}
```

#### Coordinates (for shul distance)
```csharp
public class Coordinates : ValueObject
{
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    
    public double DistanceToMiles(Coordinates other)
    {
        // Haversine formula
    }
}
```

#### ShulProximityPreference
```csharp
public class ShulProximityPreference : ValueObject
{
    public ShulPreferenceType PreferenceType { get; private set; }
    // NoPreference, AnyShul, SpecificShuls
    
    public decimal? MaxDistanceMiles { get; private set; }
    public List<Guid> SpecificShulIds { get; private set; }
}
```

---

### 3.3 Core Entities

#### Prospect (Pre-Application)

**Purpose:** Track potential applicants before formal application

**Key Fields:**
- FirstName, LastName
- PhoneNumbers (collection)
- Email (optional)
- Address (optional)
- Source (Referral, Website, Event, etc.)
- Status (InitialContact → InDiscussion → Interested → ReadyToApply → Applied)
- InterestLevel (VeryInterested → NotInterested)
- ApplicantId (when converted)

**Key Methods:**
- `Create()` - Create new prospect
- `UpdateStatus()` - Track progress
- `UpdateInterestLevel()` - Track interest
- `ConvertToApplicant()` - Create applicant when they apply

---

#### Applicant (Family)

**Purpose:** Family that has formally applied

**Husband Info:**
- FirstName, LastName, FatherName

**Wife Info:**
- WifeFirstName, WifeMaidenName, WifeFatherName, WifeHighSchool

**Contact:**
- Email (value object)
- PhoneNumbers (collection of PhoneNumber value objects)
- Address (value object)

**Children:**
- NumberOfChildren
- Children (collection of Child value objects with age + gender)

**Community:**
- CurrentKehila
- ShabbosLocation

**Housing Preferences:**
- Budget (Money - max budget only)
- MinBedrooms, MinBathrooms
- PreferredNeighborhoods (List<Neighborhood> enum)
- RequiredFeatures (List<string>)
- ShulProximity (ShulProximityPreference value object)
- MoveTimeline (enum: Immediate, ShortTerm, MediumTerm, etc.)
- EmploymentStatus (enum)
- HousingNotes

**Mortgage Info (for calculator):**
- DownPayment (Money)
- MortgageInterestRate (decimal)
- LoanTermYears (int, default 30)

**Board Review (at Applicant level):**
- BoardReviewDate
- BoardDecision (Pending, Approved, Rejected, Deferred)
- BoardDecisionNotes
- BoardReviewedByUserId

**Audit:**
- CreatedBy, CreatedDate
- ModifiedBy, ModifiedDate
- IsDeleted

**Key Methods:**
- `CreateFromApplication()` - Create applicant from form
- `UpdateHousingPreferences()` - Update preferences
- `SetBoardDecision()` - Board approves/rejects

---

#### Application (Journey)

**Purpose:** Track applicant's journey from submission to closing

**Identity:**
- ApplicationId
- ApplicantId (FK)
- Name (computed: "Application - [Applicant Name]")

**Journey:**
- Stage (enum: UnderReview, Approved, HouseHunting, OnHold, UnderContract, Closed, Rejected)
- ApplicationDate

**Agreements:**
- BrokerAgreementSigned (bool)
- BrokerAgreementDocumentId (FK to Document)
- CommunityAgreementSigned (bool)
- CommunityAgreementDocumentId (FK to Document)
- AssignedBrokerId (FK to Broker)

**Contract (when Under Contract):**
- ContractPropertyAddress (Address)
- ContractPropertyId (FK to Property)
- ContractDate
- ExpectedClosingDate
- PurchasePrice (Money)

**Closing:**
- ActualClosingDate
- MovedInStatus (MovedIn, RentedOut, Resold, Renovating, Unknown)
- MoveInDate

**Commission:**
- CommissionFromBroker (Money)
- CommissionFromBuyer (Money)
- CommissionStatus (Pending, Due, Paid, Disputed)
- CommissionPaidDate

**On Hold:**
- IsOnHold (bool)
- OnHoldDate
- OnHoldReason
- OnHoldDuration (Temporary, Extended, Indefinite)
- ExpectedResumeDate
- OnHoldNotes

**Calculated:**
- PropertiesViewed (from Showings count)
- FailedContractCount (from FailedContracts count)

**Key Methods:**
- `CreateForApplicant()` - Create new application
- `MoveToStage()` - Change stage (with validation)
- `PutOnHold()` - Pause search
- `ResumeFromHold()` - Resume search
- `RecordFailedContract()` - Save failed contract, move back to House Hunting
- `SignAgreement()` - Mark agreement signed + document uploaded

---

#### Property (Listing)

**Purpose:** Available property for applicants

**Location:**
- Address (value object)
- Coordinates (value object - for distance calc)

**Pricing:**
- ListingPrice (Money)
- AnnualPropertyTax (Money)

**Size:**
- Bedrooms, Bathrooms
- SquareFootage, LotSize
- YearBuilt
- PropertyType (SingleFamily, MultiFamily, Townhouse, Condo)
- Features (List<string>)

**Listing:**
- ListingStatus (Active, UnderContract, UnderContractThroughUs, Sold, OffMarket)
- DateListed, DateSold
- SoldPrice (Money)
- ListingAgent
- ZillowLink
- Notes

**Key Methods:**
- `Create()` - Add new property
- `EstimateMonthlyPayment()` - Calculate PITI (Principal, Interest, Tax, Insurance)
- `UpdateStatus()` - Change listing status

---

#### PropertyMatch

**Purpose:** Link applicant to matched property with score

**Key Fields:**
- PropertyMatchId
- PropertyId (FK)
- ApplicantId (FK)
- ApplicationId (FK)
- MatchScore (0-100, from algorithm)
- MatchExplanation (text: why it matches)
- IsManual (bool - algorithm vs manual)
- Status (NotSent, SentToFamily, Interested, NotInterested, ShowingScheduled)
- SentDate

---

#### Showing

**Purpose:** Schedule property viewings

**Key Fields:**
- ShowingId
- PropertyId (FK)
- ApplicantId (FK)
- ApplicationId (FK)
- ShowingDate (DateTime)
- ShowingType (InPerson, Virtual, DriveBy, OpenHouse)
- Status (Scheduled, Confirmed, Completed, Cancelled, NoShow)
- BrokerId (FK to Broker entity - NOT string)
- BrokerConfirmed (bool)
- ProspectFeedback (text)
- InterestLevel (VeryInterested, SomewhatInterested, NotInterested, MakingOffer)
- FollowUpNeeded (bool)
- CancellationReason

---

#### Broker

**Purpose:** Real estate agent/broker (proper entity, not string)

**Key Fields:**
- BrokerId
- FirstName, LastName, FullName
- Phone (PhoneNumber)
- Email (Email)
- Company
- LicenseNumber
- OfficeAddress (Address)
- TaxId (for 1099)
- PaymentMethod
- IsActive
- TotalCommissionPaid (Money)

**Calculated:**
- TotalDeals (from Showings/Applications)

---

#### FailedContract

**Purpose:** Track contracts that fell through

**Key Fields:**
- FailedContractId
- ApplicationId (FK)
- ContractAttemptNumber (1st, 2nd, 3rd try)
- PropertyAddress (Address)
- PropertyId (FK to Property - if in our system)
- ContractDate
- ExpectedClosingDate
- FellThroughDate
- FailureReason (enum: HomeInspection, Financing, SellerBackedOut, BuyerBackedOut, TitleIssues, AppraisalTooLow, CoopBoardRejected, AttorneyReview, Other)
- FailureDetails (text)
- PurchasePrice (Money)
- ExpectedCommission (Money)
- CommissionLost (Money)

---

#### Shul

**Purpose:** Synagogue for proximity matching

**Key Fields:**
- ShulId
- Name
- Address (Address)
- Location (Coordinates - for distance calc)

---

#### FollowUpReminder

**Purpose:** Tasks for coordinator to follow up

**Key Fields:**
- ReminderId
- Title ("Follow up on budget review")
- Notes
- DueDate, DueTime
- Priority (Low, Normal, High, Urgent)
- EntityType (Applicant, Application, Property, Showing, General)
- EntityId
- AssignedToUserId
- Status (Open, Completed, Snoozed, Dismissed)
- SendEmailNotification (bool)
- SnoozedUntil, SnoozeCount

---

#### Document

**Purpose:** Uploaded files (agreements, etc.)

**Key Fields:**
- DocumentId
- FileName
- BlobUrl (S3)
- FileSizeBytes
- ContentType ("application/pdf")
- Type (BrokerAgreement, CommunityAgreement, IdentificationDocument, FinancialDocument, Other)
- EntityType (Application, Applicant, Property)
- EntityId
- UploadedBy, UploadedDate

---

### 3.4 Enums

#### Neighborhoods (Union & Roselle Park specific)
```csharp
public enum Neighborhood
{
    // Union Township
    Union_BattleHill,
    Union_Connecticut_Farms,
    Union_Putnam_Manor,
    Union_Vauxhall,
    Union_Other,
    
    // Roselle Park
    RosellePark_East,
    RosellePark_West,
    RosellePark_Central,
    RosellePark_Other,
    
    Other
}
```

**Note:** You'll need to provide exact neighborhood names for Union & Roselle Park.

#### ApplicationStage
```csharp
public enum ApplicationStage
{
    UnderReview,
    Approved,
    HouseHunting,
    OnHold,
    UnderContract,
    Closed,
    Rejected
}
```

#### BoardDecision
```csharp
public enum BoardDecision
{
    Pending,
    Approved,
    // Removed: ApprovedWithConditions
    Rejected,
    Deferred
}
```

#### ListingStatus
```csharp
public enum ListingStatus
{
    Active,
    UnderContract,
    UnderContractThroughUs, // New!
    Sold,
    OffMarket
}
```

#### MovedInStatus
```csharp
public enum MovedInStatus
{
    MovedIn,        // Shortened from "MovedInLivingThere"
    RentedOut,
    Resold,         // Shortened from "FlippedResold"
    Renovating,
    Unknown
}
```

#### MoveTimeline
```csharp
public enum MoveTimeline
{
    Immediate,      // 0-1 month
    ShortTerm,      // 1-3 months
    MediumTerm,     // 3-6 months
    LongTerm,       // 6-12 months
    Extended,       // 12+ months
    Flexible,
    NotSure
}
```

---

### 3.5 Property Matching Algorithm

**Scoring Algorithm (0-100 points):**

```csharp
public int CalculateMatchScore(Property property, Applicant applicant)
{
    int score = 0;
    
    // 1. Budget Match (30 points)
    if (applicant.Budget != null)
    {
        if (property.ListingPrice <= applicant.Budget)
        {
            score += 30;
        }
        else
        {
            // Penalty for over budget
            var overBy = property.ListingPrice.Amount - applicant.Budget.Amount;
            var penalty = (int)(overBy / 10000) * 5; // -5 points per 10k over
            score += Math.Max(0, 30 - penalty);
        }
    }
    
    // 2. Bedrooms (20 points)
    if (applicant.MinBedrooms.HasValue)
    {
        if (property.Bedrooms >= applicant.MinBedrooms)
            score += 20;
    }
    
    // 3. Bathrooms (15 points)
    if (applicant.MinBathrooms.HasValue)
    {
        if (property.Bathrooms >= applicant.MinBathrooms)
            score += 15;
    }
    
    // 4. Neighborhood (20 points)
    if (applicant.PreferredNeighborhoods.Any())
    {
        var propertyNeighborhood = DetermineNeighborhood(property.Address);
        if (applicant.PreferredNeighborhoods.Contains(propertyNeighborhood))
            score += 20;
    }
    
    // 5. Features (15 points)
    if (applicant.RequiredFeatures.Any() && property.Features.Any())
    {
        var matchingFeatures = property.Features
            .Intersect(applicant.RequiredFeatures, StringComparer.OrdinalIgnoreCase)
            .Count();
        var totalRequired = applicant.RequiredFeatures.Count;
        
        score += (int)(15.0 * matchingFeatures / totalRequired);
    }
    
    // 6. Shul Proximity (Bonus: up to 10 points)
    if (applicant.ShulProximity.PreferenceType != ShulPreferenceType.NoPreference)
    {
        if (property.Location != null)
        {
            bool meetsShulRequirement = CheckShulProximity(
                property.Location, 
                applicant.ShulProximity);
            
            if (meetsShulRequirement)
                score += 10;
        }
    }
    
    return Math.Min(score, 100);
}
```

**Only show matches with score ≥ 60 by default.**

---

## PART 4: USER STORIES SUMMARY

**Total User Stories:** 68  
**Total Epics:** 11

### Epic Breakdown:

**EPIC 1: Prospect Management (NEW)** - 5 stories
- Create prospect
- Track prospect status
- Follow up with prospect
- Convert prospect to applicant
- View prospect pipeline

**EPIC 2: Application Management** - 5 stories
- Submit application (public form)
- View applications under review
- Board review application
- Auto-reject on disapproval
- Approve and send paperwork

**EPIC 3: Applicant Management** - 7 stories
- View applicant list
- View applicant details
- Edit applicant information
- Collect housing preferences
- Add notes
- Create follow-up reminder
- View my reminders

**EPIC 4: Application Pipeline** - 7 stories
- View pipeline (Kanban)
- Change application stage
- View application details
- Track commission
- Close application
- Put on hold
- Handle failed contract

**EPIC 5: Property Management** - 5 stories
- Add property listing
- View property list
- View property details
- Bulk import from CSV
- Update property status

**EPIC 6: Property Matching** - 5 stories
- Auto-generate matches
- View matches for applicant
- Send matches to applicant
- Manual match override
- Update scores when preferences change

**EPIC 7: Showing Management** - 5 stories
- Schedule showing
- View showing calendar
- Log showing feedback
- Cancel/reschedule showing
- Track broker attendance

**EPIC 8: Dashboard & Reporting** - 6 stories
- Main dashboard
- Pipeline report
- Commission report
- Monthly activity report
- Failed contract report
- Custom reports

**EPIC 9: Notifications** - 4 stories
- Email templates
- Automated notifications
- In-app notifications
- (SMS removed - not needed)

**EPIC 10: System Administration** - 5 stories
- User management
- Audit logging
- Data backup & export
- System configuration
- System health monitoring

**EPIC 11: Applicant Portal (NEW)** - 4 stories
- Applicant login (Google OAuth)
- View application status
- Update housing preferences in portal
- View property matches in portal

**EPIC 12: Broker Management (NEW)** - 3 stories
- Add/edit broker
- View broker list
- Broker performance report

---

## PART 5: PRIORITIES (To Be Finalized After Your Review)

**Note:** You haven't fully reviewed priorities yet. Current breakdown:

**P0 (MVP) - 15 stories, 79 points, 6-8 weeks:**
- Core application flow
- Basic applicant management
- Simple pipeline
- Dashboard
- Email notifications

**P1 (Phase 2) - 24 stories, 130 points, 10-12 weeks:**
- Property management
- Property matching
- Showings
- Commission tracking
- Follow-up reminders
- On Hold functionality
- Failed contracts

**P2 (Phase 3) - 16 stories, 70 points, 3-4 weeks:**
- Advanced reports
- Bulk import
- In-app notifications
- Admin features

**P3 (Future) - 1 story:**
- SMS notifications (not needed)

**Total: 19-24 weeks part-time development**

---

## PART 6: IMPLEMENTATION ROADMAP

### Phase 1: MVP (6-8 weeks)
**Goal:** Replace Google Forms, enable basic workflow

**Deliverables:**
- Public application form
- Applicant management (CRUD)
- Application pipeline (6 stages)
- Board approval workflow
- Basic dashboard
- Email notifications
- Staff authentication (AWS Cognito)

**Can Launch After This Phase!**

---

### Phase 2: Full Features (10-12 weeks)
**Goal:** Complete property matching and showing management

**Deliverables:**
- Property management
- Automated matching algorithm
- Applicant portal (Google OAuth)
- Showing scheduler
- Broker management
- Commission tracking
- Follow-up reminders
- On Hold + Failed Contracts handling

**Full System Complete!**

---

### Phase 3: Polish (3-4 weeks)
**Goal:** Advanced features and optimization

**Deliverables:**
- Advanced reports
- Bulk property import
- In-app notifications
- Performance optimization
- User training materials

---

## PART 7: DATABASE SCHEMA

**12 Main Tables:**
1. Prospects
2. Applicants
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

**Plus Supporting Tables:**
- Users (from Cognito)
- Activities (timeline/notes)
- AuditLog
- EmailTemplates
- Notifications

**Value Objects stored as:**
- JSON columns in PostgreSQL (Children, PhoneNumbers, Features)
- Owned types in EF Core (Address, Money, Email, etc.)

---

## PART 8: API ENDPOINTS SUMMARY

**Public Endpoints (No Auth):**
- POST /api/applications (submit application)
- GET /api/health (health check)

**Applicant Portal Endpoints (Google OAuth):**
- GET /api/applicant-portal/application
- PUT /api/applicant-portal/housing-preferences
- GET /api/applicant-portal/property-matches
- POST /api/applicant-portal/property-match/{id}/rate
- GET /api/applicant-portal/showings
- POST /api/applicant-portal/documents/upload

**Staff Endpoints (Role-Based):**
- /api/prospects (CRUD)
- /api/applicants (CRUD)
- /api/applications (CRUD + pipeline)
- /api/properties (CRUD)
- /api/showings (CRUD)
- /api/brokers (CRUD)
- /api/property-matches (generate, send)
- /api/reminders (CRUD)
- /api/reports (various)
- /api/users (admin only)
- /api/audit-log (admin only)

---

## PART 9: DEPLOYMENT

### AWS Infrastructure

**Elastic Beanstalk (.NET 10):**
- Deploy backend API
- Auto-scaling (if needed)
- Load balancer
- Monitoring via CloudWatch

**S3 + CloudFront:**
- Host React frontend
- CDN for fast delivery
- SSL via AWS Certificate Manager

**RDS PostgreSQL:**
- Managed database
- Automated backups
- Point-in-time recovery
- Encryption at rest

**Cognito:**
- Staff user pool (email/password)
- Applicant identity provider (Google OAuth)
- JWT token management

**SES:**
- Send all emails
- Template management
- Bounce/complaint handling

**Deployment Process:**
1. GitHub Actions CI/CD
2. Push to main → Auto-deploy
3. EF migrations auto-applied
4. Zero-downtime deployments

---

## PART 10: NEXT STEPS

### Before Development Starts:

1. **✅ Review This Document** - You're doing this now
2. **Finalize Priorities** - You mentioned you'll review priorities separately
3. **Provide Exact Neighborhood Names** - For Union & Roselle Park
4. **Confirm Shul List** - Which shuls to include in system
5. **Create Jira Project** - Import user stories
6. **Set Up AWS Account** - Free tier to start
7. **Set Up GitHub Repo** - Version control

### Development Starts:

**Sprint 1 (Week 1-2):**
- Project structure setup
- Database design in EF Core
- Basic entities and value objects
- User authentication (Cognito)
- First API endpoint

**You're Ready To Build!** 🚀

---

## DOCUMENT VERSION HISTORY

**Version 1.0** - Initial draft (superseded)  
**Version 2.0 Final** - This document, with all feedback:
- .NET 10, AWS only, AWS SES
- Domain terminology (Applicant, Application)
- Complete DDD with value objects
- All workflow corrections
- All field corrections
- Google auth + Applicant portal
- Follow-up reminders
- Desktop-first

---

## QUESTIONS TO FINALIZE

1. **Exact neighborhood names** for Union & Roselle Park?
2. **Which shuls** to include in Shul table?
3. **Ready for CSV export** of user stories to Jira?
4. **Want technical specs** for Phase 1 Sprint 1?
5. **Need sample code** for key features?

**This document is your complete technical specification. You now have everything needed to begin development!**

---

**END OF MASTER REQUIREMENTS DOCUMENT**
