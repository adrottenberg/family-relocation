# CONTEXT FOR CLAUDE CODE CLI
## Quick Reference for AI-Assisted Development

**Project:** Family Relocation System  
**Organization:** Orthodox Jewish community, Union County, NJ  
**Purpose:** Custom CRM for managing families relocating to the community  
**Developer:** Solo developer (experienced .NET)  
**Timeline:** 19-24 weeks part-time  

---

## 🚀 START HERE

**Before doing ANYTHING, read these in order:**

1. **CONVERSATION_MEMORY_LOG.md** - Full context of all design decisions (70 pages)
2. **MASTER_REQUIREMENTS_v3.md** - Complete requirements with all corrections (70 pages)
3. **Current Sprint Plan** - Check `sprint-plans/SPRINT_X.md` for active sprint

**These documents contain the "why" behind every decision. Don't skip them!**

---

## ⚠️ CRITICAL DESIGN DECISIONS

### Technology Stack (FINAL - Don't Suggest Changes)
- ✅ **.NET 10** (NOT .NET 8) - User correction #1
- ✅ **AWS ONLY** (NOT Azure) - User correction #2
- ✅ **React 18** (NOT Blazor) - Market share matters
- ✅ **PostgreSQL** (NOT SQL Server)
- ✅ **Desktop-first** (NO mobile optimization) - Chasidic families use desktop computers

### Architecture (FINAL)
- ✅ Clean Architecture (4 layers)
- ✅ DDD (Domain-Driven Design)
- ✅ CQRS with MediatR
- ✅ Repository Pattern
- ✅ Domain has ZERO dependencies

---

## 🗣️ UBIQUITOUS LANGUAGE (Use Correct Terms!)

### ✅ CORRECT Terms:
- **Applicant** (the family applying)
- **Application** (their application submission)
- **Wife** (not Spouse)
- **ShabbosShul** (where they daven on Shabbos)
- **Children** (with age and gender)
- **Board Review** (at Applicant level, not Application level!)
- **Property** (not Listing)
- **Under Contract** (not "Under Contract Through Us")

### ❌ WRONG Terms (Never Use):
- ~~Contact~~ → Use "Applicant"
- ~~Deal~~ → Use "Application"
- ~~Spouse~~ → Use "Wife"
- ~~ShabbosLocation~~ → Use "ShabbosShul"
- ~~Neighborhood~~ → Use "City" (Union or Roselle Park)

---

## 📐 ARCHITECTURE RULES (Non-Negotiable)

### Domain Layer Rules:
1. **ZERO dependencies** - No NuGet packages, pure C#
2. **Entities use factory methods** - `Applicant.CreateFromApplication(...)`
3. **Value objects are immutable** - All properties `private set`
4. **Domain events for side effects** - `AddDomainEvent(new ApplicantCreated(...))`
5. **No infrastructure concerns** - No DbContext, no repositories, no HTTP

### Value Object Rules:
```csharp
// ✅ CORRECT - Immutable, validation in constructor
public class Email : ValueObject
{
    public string Value { get; private set; }  // private set!
    
    public Email(string value)  // Validation here
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email is required");
        Value = value.ToLowerInvariant();
    }
}

// ❌ WRONG - Public setters
public class Email
{
    public string Value { get; set; }  // NO!
}
```

### Entity Rules:
```csharp
// ✅ CORRECT - Factory method, events
public static Applicant CreateFromApplication(...)
{
    var applicant = new Applicant { ... };
    applicant.AddDomainEvent(new ApplicantCreated(...));
    return applicant;
}

// ❌ WRONG - Public constructor with no validation
public Applicant(string firstName, string lastName) { ... }
```

### CQRS Rules:
```csharp
// ✅ CORRECT - Separate commands and queries
public class CreateApplicantCommand : IRequest<ApplicantDto> { }
public class GetApplicantQuery : IRequest<ApplicantDto> { }

// ❌ WRONG - Single method for both
public ApplicantDto SaveApplicant(...) { }  // Don't do this
```

---

## 🎯 KEY BUSINESS RULES

### Board Review (IMPORTANT!)
- Board review happens at **APPLICANT level** (User correction #11)
- NOT at Application level (this was wrong initially)
- One applicant can have multiple applications (if first fails)
- Board decision stays with applicant

### Application Workflow:
```
Submitted → Approved → House Hunting → Under Contract → Closing → Moved In

Edge cases:
- On Hold: Can pause/resume from any stage
- Failed Contract: Can fail from Under Contract → back to House Hunting
```

### Property Matching:
- 0-110 points total
- Budget: 30 points
- Bedrooms: 20 points
- Bathrooms: 15 points
- City: 20 points (Union or Roselle Park)
- Features: 15 points
- BONUS: Shul proximity (up to +10 points)

### Shul Proximity:
- 3 shuls: Bobov, Nassad, Yismach Yisroel
- Calculate BOTH straight-line AND walking distance (User correction #2)
- Use walking distance for matching if available

---

## 📝 16 CORRECTIONS SUMMARY

The user made 16 corrections during planning. **Read CONVERSATION_MEMORY_LOG.md** for full details:

1. ✅ .NET 10 (not .NET 8)
2. ✅ AWS only (not Azure)
3. ✅ Added "Under Contract" stage
4. ✅ Added On Hold workflow
5. ✅ Added Failed Contracts tracking
6. ✅ Google OAuth for applicant portal
7. ✅ Domain language (Applicant/Application not Contact/Deal)
8. ✅ DDD value objects required
9. ✅ Broker entity added
10. ✅ Prospect tracking added
11. ✅ Board review at APPLICANT level (not Application)
12. ✅ Desktop-first (no mobile)
13. ✅ Phone numbers as collection with type
14. ✅ Children with gender
15. ✅ Single budget field (not min/max)
16. ✅ ShabbosLocation → ShabbosShul

**Plus 16 MORE corrections in FINAL_CORRECTIONS_JAN_2026.md:**
- Activity/interaction tracking
- Walking distance for shuls
- InterestLevel.SomewhatInterested
- MoveTimeline.Never
- HouseType enum
- OpenHouse entity
- Skip neighborhoods (use City)
- Shul addresses
- Default broker
- Remove UnderContractThroughUs
- SMS notifications (optional)
- Email marketing blasts (Phase 3)
- Reminders dashboard with print view

---

## 🛠️ COMMON TASKS

### Adding a New Entity:
1. Check CONVERSATION_MEMORY_LOG.md - Was this discussed?
2. Create in Domain layer first (ZERO dependencies)
3. Add factory method
4. Add domain events
5. Create EF Core configuration
6. Create repository interface in Domain
7. Implement repository in Infrastructure
8. Add to DbContext

### Adding a New Feature:
1. Check sprint plan - Is this in scope?
2. Create Command/Query in Application layer
3. Create Handler with business logic
4. Create Validator (FluentValidation)
5. Add API endpoint in Controllers
6. Test manually
7. Update documentation if design changed

### Modifying Domain Model:
1. **STOP!** Check CONVERSATION_MEMORY_LOG.md first
2. Was this decision already discussed and finalized?
3. If yes, follow the documented decision
4. If no, note that user should review change

---

## 📚 DOCUMENT STRUCTURE

```
docs/
├── CONVERSATION_MEMORY_LOG.md          # ← Full context (READ FIRST!)
├── CLAUDE_CODE_CONTEXT.md              # ← This file (quick reference)
├── MASTER_REQUIREMENTS_v3.md           # ← Complete requirements
├── FINAL_CORRECTIONS_JAN_2026.md       # ← All 32 corrections
├── ARCHITECTURE_DECISIONS.md           # ← ADRs
├── DOMAIN_MODEL.md                     # ← Domain explanation
├── sprint-plans/
│   ├── SPRINT_1.md                     # ← Current: Foundation
│   ├── SPRINT_2.md                     # ← TBD
│   └── ...
└── user-stories/
    ├── P0_STORIES_SUMMARY.md           # ← All MVP stories
    ├── SPRINT_1_DETAILED_STORIES.md    # ← Sprint 1 stories with code
    └── ...
```

---

## 🚦 WORKFLOW WITH USER

### When User Asks You To:

**"Implement [feature]"**
→ Check sprint plan first
→ Read relevant user story
→ Follow established patterns
→ Create code
→ User reviews and commits

**"Why did we decide to [X]?"**
→ Search CONVERSATION_MEMORY_LOG.md
→ Explain the reasoning
→ Reference correction number if applicable

**"Can we change [X] to [Y]?"**
→ Check if [X] was a finalized decision
→ If yes: "This was decided in correction #N because [reason]. Want to reconsider?"
→ If no: "No prior decision, here's the tradeoff..."

**"Debug this error"**
→ Check code against patterns in docs
→ Verify domain rules from MASTER_REQUIREMENTS_v3.md
→ Suggest fix

---

## 🎨 CODE STYLE PREFERENCES

### User Prefers:
- ✅ Explicit over implicit
- ✅ Clear variable names
- ✅ Guard clauses at top of methods
- ✅ Domain events for side effects
- ✅ Comprehensive comments on complex logic
- ✅ Validation in constructors/factory methods

### User Dislikes:
- ❌ Magic strings (use enums/constants)
- ❌ Anemic domain models
- ❌ God classes
- ❌ Tight coupling
- ❌ Shortcuts that sacrifice maintainability

---

## 🔍 BEFORE SUGGESTING CHANGES

**Ask yourself:**
1. Was this already decided? (Check CONVERSATION_MEMORY_LOG.md)
2. Does this follow DDD principles?
3. Does this maintain domain purity?
4. Is this in the current sprint scope?
5. Does this use correct ubiquitous language?

**If uncertain, say:**
> "I see this could be done differently. Should I check the conversation log to see if there was a prior decision on this?"

---

## ⚡ QUICK COMMANDS FOR YOU

```bash
# Give me context when starting:
> Read docs/CONVERSATION_MEMORY_LOG.md and docs/CLAUDE_CODE_CONTEXT.md 
  to understand the project

# Before implementing a feature:
> Read docs/sprint-plans/SPRINT_1.md and docs/user-stories/SPRINT_1_DETAILED_STORIES.md 
  for User Story US-006: Create Applicant

# When stuck on a domain decision:
> Search docs/CONVERSATION_MEMORY_LOG.md for "board review" to understand 
  why it's at Applicant level

# Before changing architecture:
> Review docs/ARCHITECTURE_DECISIONS.md before proposing changes to 
  the domain model
```

---

## 🎯 SUCCESS CRITERIA

**You're doing it right if:**
- ✅ You check docs before making changes
- ✅ You use correct domain language
- ✅ You follow established patterns
- ✅ You maintain domain purity
- ✅ You ask before changing finalized decisions

**Red flags:**
- ❌ Suggesting .NET 8 instead of .NET 10
- ❌ Using "Contact" instead of "Applicant"
- ❌ Adding dependencies to Domain layer
- ❌ Changing decisions that were already corrected
- ❌ Ignoring the conversation history

---

## 📞 NEED MORE CONTEXT?

**Full details in:**
- CONVERSATION_MEMORY_LOG.md (70 pages) - Complete session history
- MASTER_REQUIREMENTS_v3.md (70 pages) - All requirements
- FINAL_CORRECTIONS_JAN_2026.md - All 32 corrections explained

**Don't be shy - read the docs! They contain months of planning and refinement.**

---

**Last Updated:** January 2026  
**Version:** 3.0 (After 32 corrections)  
**Status:** Ready for Sprint 1 development  

**Remember: When in doubt, check the docs. The user spent a lot of time documenting everything for a reason! 🎯**
