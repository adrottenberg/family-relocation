# VISUAL STUDIO SOLUTION STRUCTURE & CODE
## Complete Implementation Guide for Family Relocation System

**Version:** 3.0 Final (With All 32 Corrections)  
**Last Updated:** January 14, 2026  
**Tech Stack:** .NET 10, C# 13, ASP.NET Core, EF Core 10, PostgreSQL, React 18  
**Architecture:** Clean Architecture + DDD + CQRS  

---

## ⚠️ IMPORTANT: LATEST CORRECTIONS APPLIED

This document includes ALL 32 corrections:
- **16 original corrections** (from initial planning)
- **16 new corrections** (from FINAL_CORRECTIONS_JAN_2026.md)

Key changes:
- ✅ ShabbosLocation → ShabbosShul
- ✅ InterestLevel.SomewhatInterested added
- ✅ MoveTimeline.Never added
- ✅ HouseType enum added
- ✅ Neighborhood enum removed (use City: Union/Roselle Park)
- ✅ Activity tracking expanded
- ✅ OpenHouse entity added
- ✅ UnderContractThroughUs removed
- ✅ Walking distance for shuls
- ✅ Reminders dashboard
- ✅ Email blast system (Phase 3)
- ✅ And more...

See **FINAL_CORRECTIONS_JAN_2026.md** for complete details.

---

## 📋 TABLE OF CONTENTS

1. [Solution Overview](#solution-overview)
2. [Complete Folder Structure](#complete-folder-structure)
3. [Domain Layer - Complete Code](#domain-layer---complete-code)
4. [Enums - Updated](#enums---updated)
5. [Value Objects - Complete Code](#value-objects---complete-code)
6. [Entities - Complete Code](#entities---complete-code)
7. [Application Layer - CQRS](#application-layer---cqrs)
8. [Infrastructure Layer](#infrastructure-layer)
9. [API Layer](#api-layer)
10. [React Frontend Structure](#react-frontend-structure)
11. [NuGet Packages](#nuget-packages)
12. [Configuration Files](#configuration-files)

---

## 1. SOLUTION OVERVIEW

### Architecture (Clean Architecture + DDD + CQRS)

```
Domain Layer (Core - ZERO dependencies)
├── Entities (12 entities)
├── Value Objects (7 value objects)
├── Enums (14 enums - UPDATED)
├── Events (Domain events)
└── Services (Domain services)

Application Layer (CQRS)
├── Commands (Write operations)
├── Queries (Read operations)
├── Handlers (MediatR)
├── DTOs (Data transfer objects)
└── Validators (FluentValidation)

Infrastructure Layer
├── Persistence (EF Core)
├── Repositories
├── AWS Services (S3, SES, Cognito)
└── External Services

API Layer
├── Controllers (REST endpoints)
├── Middleware
└── Configuration
```

---

## 2. COMPLETE FOLDER STRUCTURE

```
FamilyRelocation/
├── docs/                                    # ← Documentation
│   ├── CONVERSATION_MEMORY_LOG.md          # Full context
│   ├── CLAUDE_CODE_CONTEXT.md              # Quick reference for Claude Code CLI
│   ├── MASTER_REQUIREMENTS_v3.md           # Complete requirements
│   ├── FINAL_CORRECTIONS_JAN_2026.md       # All 32 corrections
│   ├── sprint-plans/
│   │   └── SPRINT_1_DETAILED_STORIES.md
│   └── README.md
│
├── src/
│   ├── FamilyRelocation.Domain/
│   │   ├── Common/
│   │   │   ├── Entity.cs
│   │   │   ├── ValueObject.cs
│   │   │   ├── IDomainEvent.cs
│   │   │   └── IUnitOfWork.cs
│   │   │
│   │   ├── Entities/
│   │   │   ├── Applicant.cs
│   │   │   ├── Application.cs
│   │   │   ├── Property.cs
│   │   │   ├── PropertyPhoto.cs
│   │   │   ├── PropertyMatch.cs
│   │   │   ├── Showing.cs
│   │   │   ├── Prospect.cs
│   │   │   ├── Broker.cs
│   │   │   ├── Shul.cs
│   │   │   ├── FailedContract.cs
│   │   │   ├── FollowUpReminder.cs
│   │   │   ├── Document.cs
│   │   │   ├── Activity.cs
│   │   │   ├── OpenHouse.cs                # ← NEW (Correction #8)
│   │   │   ├── EmailContact.cs             # ← NEW (Correction #14)
│   │   │   ├── EmailBlast.cs               # ← NEW (Correction #14)
│   │   │   ├── EmailBlastRecipient.cs      # ← NEW (Correction #14)
│   │   │   └── User.cs
│   │   │
│   │   ├── ValueObjects/
│   │   │   ├── Address.cs
│   │   │   ├── PhoneNumber.cs
│   │   │   ├── Email.cs
│   │   │   ├── Money.cs
│   │   │   ├── Child.cs
│   │   │   ├── Coordinates.cs
│   │   │   └── ShulProximityPreference.cs
│   │   │
│   │   ├── Enums/
│   │   │   ├── ApplicationStage.cs
│   │   │   ├── BoardDecision.cs
│   │   │   ├── ListingStatus.cs            # ← UPDATED (Removed UnderContractThroughUs)
│   │   │   ├── MovedInStatus.cs
│   │   │   ├── MoveTimeline.cs             # ← UPDATED (Added Never)
│   │   │   ├── ProspectStatus.cs
│   │   │   ├── InterestLevel.cs            # ← UPDATED (Added SomewhatInterested)
│   │   │   ├── PropertyType.cs
│   │   │   ├── HouseType.cs                # ← NEW (Correction #6)
│   │   │   ├── ShowingStatus.cs
│   │   │   ├── ContractFailureReason.cs
│   │   │   ├── ReminderPriority.cs
│   │   │   ├── ReminderStatus.cs
│   │   │   ├── EntityType.cs
│   │   │   ├── ActivityType.cs             # ← UPDATED (Expanded)
│   │   │   └── EmailDeliveryStatus.cs      # ← NEW (Correction #14)
│   │   │
│   │   ├── Events/
│   │   │   ├── ApplicantCreated.cs
│   │   │   ├── ApplicantApprovedByBoard.cs
│   │   │   ├── ApplicationSubmitted.cs
│   │   │   ├── PropertyMatchCreated.cs
│   │   │   └── ... (all domain events)
│   │   │
│   │   └── Services/
│   │       ├── IPropertyMatchingService.cs
│   │       └── PropertyMatchingService.cs
│   │
│   ├── FamilyRelocation.Application/
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   ├── Mappings/
│   │   │   └── Behaviors/
│   │   │
│   │   ├── Applicants/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateApplicant/
│   │   │   │   ├── UpdateApplicant/
│   │   │   │   ├── UpdateHousingPreferences/
│   │   │   │   └── SetBoardDecision/
│   │   │   ├── Queries/
│   │   │   │   ├── GetApplicant/
│   │   │   │   ├── GetApplicants/
│   │   │   │   └── GetApplicantTimeline/
│   │   │   └── DTOs/
│   │   │       └── ApplicantDto.cs
│   │   │
│   │   ├── Applications/
│   │   ├── Properties/
│   │   ├── Reminders/                     # ← Dashboard query (Correction #16)
│   │   │   ├── Queries/
│   │   │   │   └── GetRemindersQuery.cs
│   │   │   └── Commands/
│   │   │       ├── CompleteReminderCommand.cs
│   │   │       └── SnoozeReminderCommand.cs
│   │   │
│   │   └── EmailBlasts/                   # ← NEW (Correction #14)
│   │       ├── Commands/
│   │       └── Queries/
│   │
│   ├── FamilyRelocation.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── ApplicantConfiguration.cs
│   │   │   │   ├── OpenHouseConfiguration.cs      # ← NEW
│   │   │   │   ├── EmailContactConfiguration.cs   # ← NEW
│   │   │   │   └── ... (all entity configs)
│   │   │   └── Migrations/
│   │   │
│   │   ├── Repositories/
│   │   ├── Services/
│   │   │   ├── DistanceCalculationService.cs     # ← NEW (Walking distance)
│   │   │   └── GeocodingService.cs               # ← NEW (Shul geocoding)
│   │   │
│   │   ├── AWS/
│   │   │   ├── S3Service.cs
│   │   │   ├── SESService.cs
│   │   │   ├── SNSService.cs                     # ← NEW (SMS support)
│   │   │   └── CognitoService.cs
│   │   │
│   │   └── Email/
│   │       └── EmailTrackingService.cs           # ← NEW (Email blasts)
│   │
│   └── FamilyRelocation.API/
│       ├── Controllers/
│       │   ├── RemindersController.cs            # ← Dashboard endpoints
│       │   └── ... (all controllers)
│       └── Program.cs
│
└── FamilyRelocation.sln
```

---

## 3. DOMAIN LAYER - COMPLETE CODE

### 3.1 Updated Enums

#### ListingStatus.cs (UPDATED - Removed UnderContractThroughUs)

```csharp
namespace FamilyRelocation.Domain.Enums
{
    /// <summary>
    /// Property listing status
    /// NOTE: Removed UnderContractThroughUs (Correction #10)
    /// Use Application.ContractPropertyId to track "our" contracts
    /// </summary>
    public enum ListingStatus
    {
        Active,
        UnderContract,      // Use for ALL contracts
        Sold,
        OffMarket
    }
}
```

#### InterestLevel.cs (UPDATED - Added SomewhatInterested)

```csharp
namespace FamilyRelocation.Domain.Enums
{
    /// <summary>
    /// Prospect interest level
    /// Updated: Added SomewhatInterested (Correction #3)
    /// </summary>
    public enum InterestLevel
    {
        VeryInterested,
        SomewhatInterested,     // ← NEW
        Neutral,
        NotVeryInterested,
        NotInterested
    }
}
```

#### MoveTimeline.cs (UPDATED - Added Never)

```csharp
namespace FamilyRelocation.Domain.Enums
{
    /// <summary>
    /// When family plans to move
    /// Updated: Added Never for investors (Correction #4)
    /// </summary>
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
}
```

#### HouseType.cs (NEW - Correction #6)

```csharp
namespace FamilyRelocation.Domain.Enums
{
    /// <summary>
    /// Type of house structure
    /// NEW: Added per Correction #6
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
}
```

#### ActivityType.cs (UPDATED - Expanded)

```csharp
namespace FamilyRelocation.Domain.Enums
{
    /// <summary>
    /// Types of activities/interactions
    /// Updated: Expanded per Correction #1
    /// </summary>
    public enum ActivityType
    {
        // Existing
        Note,
        EmailSent,
        EmailReceived,
        StageChange,
        StatusChange,
        
        // NEW (Correction #1)
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
}
```

#### EmailDeliveryStatus.cs (NEW - For Email Blasts)

```csharp
namespace FamilyRelocation.Domain.Enums
{
    /// <summary>
    /// Email delivery and engagement tracking
    /// NEW: For email blast system (Correction #14)
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
}
```

---

### 3.2 Updated Entities

#### Applicant.cs (UPDATED - ShabbosShul, Cities)

```csharp
using System;
using System.Collections.Generic;
using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.ValueObjects;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;

namespace FamilyRelocation.Domain.Entities
{
    /// <summary>
    /// Applicant aggregate root
    /// UPDATED: ShabbosLocation → ShabbosShul (Correction #5)
    /// UPDATED: Neighborhood → Cities (Correction #12)
    /// </summary>
    public class Applicant : Entity<Guid>
    {
        public Guid ApplicantId
        {
            get => Id;
            private set => Id = value;
        }

        public Guid? ProspectId { get; private set; }
        
        // Husband Info
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FatherName { get; private set; }
        public string FullName => $"{FirstName} {LastName}";
        
        // Wife Info
        public string WifeFirstName { get; set; }
        public string WifeMaidenName { get; set; }
        public string WifeFatherName { get; set; }
        public string WifeHighSchool { get; set; }
        
        // Contact
        public Email Email { get; private set; }
        public List<PhoneNumber> PhoneNumbers { get; private set; } = new();
        public Address Address { get; private set; }
        
        // Children
        public int NumberOfChildren => Children?.Count ?? 0;
        public List<Child> Children { get; private set; } = new();
        
        // Community
        public string CurrentKehila { get; set; }
        public string ShabbosShul { get; set; }  // ← UPDATED from ShabbosLocation (Correction #5)
        
        // Housing Preferences
        public Money Budget { get; set; }
        public int? MinBedrooms { get; set; }
        public decimal? MinBathrooms { get; set; }
        
        // UPDATED: Removed Neighborhood enum, use Cities (Correction #12)
        public List<string> PreferredCities { get; private set; } = new();  // Union, Roselle Park
        
        public List<string> RequiredFeatures { get; private set; } = new();
        public ShulProximityPreference ShulProximity { get; set; }
        public MoveTimeline? MoveTimeline { get; set; }
        public string EmploymentStatus { get; set; }
        public string HousingNotes { get; set; }
        
        // Mortgage
        public Money DownPayment { get; set; }
        public decimal? MortgageInterestRate { get; set; }
        public int LoanTermYears { get; set; } = 30;
        
        // Board Review (AT APPLICANT LEVEL - Correction #11)
        public DateTime? BoardReviewDate { get; private set; }
        public BoardDecision? BoardDecision { get; private set; }
        public string BoardDecisionNotes { get; private set; }
        public Guid? BoardReviewedByUserId { get; private set; }
        
        // Navigation
        public virtual ICollection<Application> Applications { get; private set; } = new List<Application>();
        
        // Audit
        public Guid CreatedBy { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public Guid ModifiedBy { get; private set; }
        public DateTime ModifiedDate { get; private set; }
        public bool IsDeleted { get; private set; }

        private Applicant() { }

        public static Applicant CreateFromApplication(
            string firstName,
            string lastName,
            string fatherName,
            Email email,
            Address address,
            string currentKehila,
            string shabbosShul,          // ← UPDATED parameter name
            Guid createdBy,
            Guid? prospectId = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required", nameof(lastName));

            var applicant = new Applicant
            {
                ApplicantId = Guid.NewGuid(),
                ProspectId = prospectId,
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                FatherName = fatherName?.Trim(),
                Email = email,
                Address = address,
                CurrentKehila = currentKehila?.Trim(),
                ShabbosShul = shabbosShul?.Trim(),  // ← UPDATED
                PhoneNumbers = new List<PhoneNumber>(),
                Children = new List<Child>(),
                PreferredCities = new List<string>(),  // ← UPDATED
                RequiredFeatures = new List<string>(),
                ShulProximity = ShulProximityPreference.NoPreference(),
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                ModifiedBy = createdBy,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            applicant.AddDomainEvent(new ApplicantCreated(applicant.ApplicantId, prospectId));

            return applicant;
        }

        public void UpdateHousingPreferences(
            Money budget,
            int minBedrooms,
            decimal minBathrooms,
            List<string> cities,              // ← UPDATED from neighborhoods
            List<string> features,
            ShulProximityPreference shulProximity,
            MoveTimeline? moveTimeline,
            string employmentStatus,
            Money downPayment,
            decimal? mortgageInterestRate,
            int? loanTermYears,
            string notes,
            Guid modifiedBy)
        {
            Budget = budget;
            MinBedrooms = minBedrooms;
            MinBathrooms = minBathrooms;
            PreferredCities = cities ?? new List<string>();  // ← UPDATED
            RequiredFeatures = features ?? new List<string>();
            ShulProximity = shulProximity ?? ShulProximityPreference.NoPreference();
            MoveTimeline = moveTimeline;
            EmploymentStatus = employmentStatus;
            DownPayment = downPayment;
            MortgageInterestRate = mortgageInterestRate ?? 6.5m;
            LoanTermYears = loanTermYears ?? 30;
            HousingNotes = notes ?? string.Empty;
            ModifiedBy = modifiedBy;
            ModifiedDate = DateTime.UtcNow;

            AddDomainEvent(new HousingPreferencesUpdated(ApplicantId));
        }

        // ... other methods unchanged
    }
}
```

#### Property.cs (UPDATED - HouseType, OpenHouses, City)

```csharp
namespace FamilyRelocation.Domain.Entities
{
    /// <summary>
    /// Property aggregate root
    /// UPDATED: Added HouseType (Correction #6)
    /// UPDATED: Added OpenHouses collection (Correction #8)
    /// UPDATED: Neighborhood → City (Correction #12)
    /// </summary>
    public class Property : Entity<Guid>
    {
        public Guid PropertyId
        {
            get => Id;
            private set => Id = value;
        }

        // Basic Info
        public Address Address { get; private set; }
        public PropertyType PropertyType { get; private set; }
        
        // NEW: House type (Correction #6)
        public HouseType? HouseType { get; private set; }
        
        public int Bedrooms { get; private set; }
        public decimal Bathrooms { get; private set; }
        public int? SquareFeet { get; private set; }
        public decimal? LotSize { get; private set; }
        
        // Pricing
        public Money ListPrice { get; private set; }
        public Money? SoldPrice { get; private set; }
        
        // Location
        // UPDATED: Use City instead of Neighborhood enum (Correction #12)
        public string City { get; private set; }  // Union or Roselle Park
        public Coordinates Coordinates { get; private set; }
        
        // Status
        public ListingStatus Status { get; private set; }
        public DateTime ListedDate { get; private set; }
        public DateTime? SoldDate { get; private set; }
        
        // Features
        public List<string> Features { get; private set; } = new();
        public string Description { get; private set; }
        public string Notes { get; private set; }
        
        // Broker
        public Guid? ListingBrokerId { get; private set; }
        public virtual Broker ListingBroker { get; private set; }
        
        // Photos
        public virtual ICollection<PropertyPhoto> Photos { get; private set; } = new List<PropertyPhoto>();
        
        // NEW: Open houses (Correction #8)
        public virtual ICollection<OpenHouse> OpenHouses { get; private set; } = new List<OpenHouse>();
        
        // Navigation
        public virtual ICollection<PropertyMatch> Matches { get; private set; } = new List<PropertyMatch>();
        public virtual ICollection<Showing> Showings { get; private set; } = new List<Showing>();
        
        // Audit
        public Guid CreatedBy { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public Guid ModifiedBy { get; private set; }
        public DateTime ModifiedDate { get; private set; }

        private Property() { }

        public static Property Create(
            Address address,
            PropertyType propertyType,
            HouseType? houseType,           // ← NEW parameter
            int bedrooms,
            decimal bathrooms,
            Money listPrice,
            string city,                     // ← UPDATED from neighborhood
            Coordinates coordinates,
            Guid createdBy)
        {
            var property = new Property
            {
                PropertyId = Guid.NewGuid(),
                Address = address,
                PropertyType = propertyType,
                HouseType = houseType,       // ← NEW
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                ListPrice = listPrice,
                City = city,                 // ← UPDATED
                Coordinates = coordinates,
                Status = ListingStatus.Active,
                ListedDate = DateTime.UtcNow,
                Features = new List<string>(),
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                ModifiedBy = createdBy,
                ModifiedDate = DateTime.UtcNow
            };

            property.AddDomainEvent(new PropertyListed(property.PropertyId));

            return property;
        }

        // ... other methods
    }
}
```

#### OpenHouse.cs (NEW - Correction #8)

```csharp
namespace FamilyRelocation.Domain.Entities
{
    /// <summary>
    /// Open house schedule
    /// NEW: Added per Correction #8
    /// </summary>
    public class OpenHouse : Entity<Guid>
    {
        public Guid OpenHouseId
        {
            get => Id;
            private set => Id = value;
        }

        public Guid PropertyId { get; private set; }
        
        public DateTime StartDateTime { get; private set; }
        public DateTime EndDateTime { get; private set; }
        
        public string Notes { get; private set; }
        
        public bool IsCancelled { get; private set; }
        public string CancellationReason { get; private set; }
        
        // Navigation
        public virtual Property Property { get; private set; }

        private OpenHouse() { }

        public static OpenHouse Schedule(
            Guid propertyId,
            DateTime startDateTime,
            DateTime endDateTime,
            string notes = null)
        {
            // Validate no Shabbos or Yom Tov
            ValidateNotShabbos(startDateTime, endDateTime);

            var openHouse = new OpenHouse
            {
                OpenHouseId = Guid.NewGuid(),
                PropertyId = propertyId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Notes = notes,
                IsCancelled = false
            };

            return openHouse;
        }

        private static void ValidateNotShabbos(DateTime start, DateTime end)
        {
            // Saturday
            if (start.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Saturday)
                throw new DomainException("Cannot schedule open house on Shabbos (Saturday)");

            // Friday after 4pm
            if (start.DayOfWeek == DayOfWeek.Friday && start.Hour >= 16)
                throw new DomainException("Cannot schedule open house Friday after 4pm (Shabbos)");

            // TODO: Check against Yom Tov calendar
        }

        public void Cancel(string reason)
        {
            IsCancelled = true;
            CancellationReason = reason;
        }
    }
}
```

#### Shul.cs (UPDATED - Seed Data with Addresses)

```csharp
namespace FamilyRelocation.Domain.Entities
{
    /// <summary>
    /// Shul entity
    /// UPDATED: Seed data with addresses (Correction #13)
    /// </summary>
    public class Shul : Entity<Guid>
    {
        public Guid ShulId
        {
            get => Id;
            private set => Id = value;
        }

        public string Name { get; private set; }
        public Address Address { get; private set; }
        public Coordinates Coordinates { get; private set; }
        public bool IsActive { get; private set; }

        private Shul() { }

        public static Shul Create(string name, Address address, Coordinates coordinates = null)
        {
            return new Shul
            {
                ShulId = Guid.NewGuid(),
                Name = name,
                Address = address,
                Coordinates = coordinates,
                IsActive = true
            };
        }

        /// <summary>
        /// Seed data for initial shuls (Correction #13)
        /// </summary>
        public static List<Shul> GetSeedData()
        {
            return new List<Shul>
            {
                Create(
                    "Bobov",
                    new Address("212 New Jersey Ave", "Elizabeth", "NJ", "07202"),
                    null  // Geocode after creation
                ),
                Create(
                    "Nassad",
                    new Address("433 Bailey Avenue", "Elizabeth", "NJ", "07208"),
                    null
                ),
                Create(
                    "Yismach Yisroel",
                    new Address("547 Salem Road", "Union", "NJ", "07083"),
                    null
                )
            };
        }
    }
}
```

#### EmailContact.cs (NEW - Email Blast System, Correction #14)

```csharp
namespace FamilyRelocation.Domain.Entities
{
    /// <summary>
    /// Email marketing contact (not yet prospect/applicant)
    /// NEW: Email blast system (Correction #14)
    /// </summary>
    public class EmailContact : Entity<Guid>
    {
        public Guid EmailContactId
        {
            get => Id;
            private set => Id = value;
        }

        public Email EmailAddress { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Source { get; private set; }  // "Website signup", "Referral", etc.
        
        // Lifecycle
        public DateTime AddedDate { get; private set; }
        public bool IsSubscribed { get; private set; }
        public DateTime? UnsubscribedDate { get; private set; }
        
        // Link when they convert
        public Guid? ProspectId { get; private set; }
        public Guid? ApplicantId { get; private set; }
        public DateTime? ConvertedDate { get; private set; }
        
        // Navigation
        public virtual ICollection<EmailBlastRecipient> EmailsReceived { get; private set; }

        private EmailContact() { }

        public static EmailContact Create(Email email, string firstName, string lastName, string source)
        {
            return new EmailContact
            {
                EmailContactId = Guid.NewGuid(),
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName,
                Source = source,
                AddedDate = DateTime.UtcNow,
                IsSubscribed = true,
                EmailsReceived = new List<EmailBlastRecipient>()
            };
        }

        public void LinkToApplicant(Guid applicantId)
        {
            ApplicantId = applicantId;
            ConvertedDate = DateTime.UtcNow;
        }

        public void Unsubscribe()
        {
            IsSubscribed = false;
            UnsubscribedDate = DateTime.UtcNow;
        }
    }
}
```

---

## 4. INFRASTRUCTURE SERVICES

### 4.1 Distance Calculation Service (NEW - Walking Distance, Correction #2)

```csharp
using FamilyRelocation.Domain.ValueObjects;

namespace FamilyRelocation.Infrastructure.Services
{
    /// <summary>
    /// Calculate both straight-line AND walking distance
    /// NEW: Correction #2 - Walking distance via MapBox API
    /// </summary>
    public interface IDistanceCalculationService
    {
        Task<double> CalculateStraightLineDistance(Coordinates from, Coordinates to);
        Task<(double miles, int minutes)> CalculateWalkingDistance(Coordinates from, Coordinates to);
    }

    public class MapBoxDistanceService : IDistanceCalculationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public async Task<double> CalculateStraightLineDistance(Coordinates from, Coordinates to)
        {
            // Use Haversine formula from Coordinates value object
            return from.DistanceToMiles(to);
        }

        public async Task<(double miles, int minutes)> CalculateWalkingDistance(Coordinates from, Coordinates to)
        {
            // MapBox Directions API
            var url = $"https://api.mapbox.com/directions/v5/mapbox/walking/" +
                      $"{from.Longitude},{from.Latitude};{to.Longitude},{to.Latitude}" +
                      $"?access_token={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadFromJsonAsync<MapBoxDirectionsResponse>();

            var route = json.Routes.FirstOrDefault();
            if (route == null)
                return (from.DistanceToMiles(to), 0);  // Fallback to straight-line

            var miles = route.Distance * 0.000621371;  // meters to miles
            var minutes = (int)(route.Duration / 60);   // seconds to minutes

            return (miles, minutes);
        }
    }

    // MapBox response models
    public class MapBoxDirectionsResponse
    {
        public List<MapBoxRoute> Routes { get; set; }
    }

    public class MapBoxRoute
    {
        public double Distance { get; set; }  // in meters
        public double Duration { get; set; }  // in seconds
    }
}
```

### 4.2 Shul Proximity Result (UPDATED)

```csharp
namespace FamilyRelocation.Application.Properties.DTOs
{
    /// <summary>
    /// Shul proximity with BOTH straight-line and walking distance
    /// UPDATED: Correction #2
    /// </summary>
    public class ShulProximityResult
    {
        public Guid ShulId { get; set; }
        public string ShulName { get; set; }
        
        // Straight-line distance (quick calculation)
        public double StraightLineDistanceMiles { get; set; }
        
        // Walking distance via streets (MapBox API)
        public double? WalkingDistanceMiles { get; set; }
        public int? WalkingTimeMinutes { get; set; }
        
        // Use walking if available, otherwise straight-line
        public double EffectiveDistance => WalkingDistanceMiles ?? StraightLineDistanceMiles;
        
        public string DisplayDistance =>
            WalkingDistanceMiles.HasValue
                ? $"{StraightLineDistanceMiles:F2} mi straight-line, {WalkingDistanceMiles:F2} mi walking ({WalkingTimeMinutes} min)"
                : $"{StraightLineDistanceMiles:F2} mi straight-line";
    }
}
```

---

## 5. APPLICATION LAYER - REMINDERS DASHBOARD (NEW - Correction #16)

See complete implementation in SPRINT_1_DETAILED_STORIES.md and FINAL_CORRECTIONS_JAN_2026.md section 16.

Key files:
- `GetRemindersQuery.cs` - Query with filters (overdue, today, this week, assigned to me)
- `ReminderWithEntityDto.cs` - DTO with full contact info for print view
- `RemindersController.cs` - Dashboard and print endpoints

---

## 6. COMPLETE ENUM REFERENCE

All enums with latest corrections:

```csharp
// ApplicationStage - No changes
public enum ApplicationStage
{
    Submitted, Approved, HouseHunting, UnderContract, Closing, MovedIn
}

// BoardDecision - No changes
public enum BoardDecision
{
    Pending, Approved, Rejected, Deferred
}

// ListingStatus - UPDATED (Removed UnderContractThroughUs)
public enum ListingStatus
{
    Active, UnderContract, Sold, OffMarket
}

// MovedInStatus - No changes
public enum MovedInStatus
{
    MovedIn, LeftCommunity, StillInOriginalLocation
}

// MoveTimeline - UPDATED (Added Never)
public enum MoveTimeline
{
    Immediate, ShortTerm, MediumTerm, LongTerm, Extended, Flexible, NotSure, Never
}

// InterestLevel - UPDATED (Added SomewhatInterested)
public enum InterestLevel
{
    VeryInterested, SomewhatInterested, Neutral, NotVeryInterested, NotInterested
}

// HouseType - NEW
public enum HouseType
{
    Colonial, CapeCod, Flat, SplitLevel, BiLevel, Townhouse, Duplex, Condo, Victorian, Contemporary, Other
}

// ActivityType - UPDATED (Expanded)
public enum ActivityType
{
    Note, EmailSent, EmailReceived, StageChange, StatusChange,
    PhoneCall, TextMessage, Meeting, ShowingScheduled, ShowingCompleted,
    DocumentUploaded, DocumentSigned, ReminderCreated, ReminderCompleted
}

// EmailDeliveryStatus - NEW
public enum EmailDeliveryStatus
{
    Pending, Sent, Delivered, Opened, Clicked, Bounced, Complained
}
```

---

## 7. SUMMARY OF ALL 32 CORRECTIONS IN CODE

| # | Correction | Code Location | Status |
|---|------------|---------------|--------|
| 1 | Activity tracking expanded | ActivityType enum, Activity entity | ✅ Updated |
| 2 | Walking distance calculation | DistanceCalculationService, ShulProximityResult | ✅ Added |
| 3 | InterestLevel.SomewhatInterested | InterestLevel enum | ✅ Added |
| 4 | MoveTimeline.Never | MoveTimeline enum | ✅ Added |
| 5 | ShabbosLocation → ShabbosShul | Applicant entity | ✅ Updated |
| 6 | HouseType enum | HouseType enum, Property entity | ✅ Added |
| 7 | Basic property to Phase 1 | Sprint planning | ✅ Noted |
| 8 | OpenHouse entity | OpenHouse.cs, Property.OpenHouses | ✅ Added |
| 9 | Default broker | Broker entity, settings | ✅ Noted |
| 10 | Remove UnderContractThroughUs | ListingStatus enum | ✅ Removed |
| 11 | SMS notifications | SNS service | ✅ Noted (P2) |
| 12 | Skip neighborhoods | Neighborhood enum removed, use City | ✅ Updated |
| 13 | Shul addresses | Shul.GetSeedData() | ✅ Added |
| 14 | Email blasts | EmailContact, EmailBlast entities | ✅ Added |
| 15 | (Same as 13) | - | - |
| 16 | Reminders dashboard | GetRemindersQuery, print view | ✅ Added |

---

## 8. NEXT STEPS

1. **Use this document** as your code reference
2. **Copy code samples** from sections above
3. **Follow Sprint 1 stories** in SPRINT_1_DETAILED_STORIES.md
4. **Reference corrections** in FINAL_CORRECTIONS_JAN_2026.md for details

**All corrections are now incorporated! Ready to build! 🚀**

---

END OF DOCUMENT (Version 3.0 with ALL 32 Corrections)
