# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A custom CRM for managing Orthodox Jewish family relocation to Union County, NJ. Built with Clean Architecture + DDD + CQRS pattern.

---

## MANDATORY WORKFLOW

**IMPORTANT: Follow this workflow for ALL code changes.**

### Before Starting Work

1. Check `docs/IMPLEMENTATION_STATUS.md` to understand current state
2. Check `docs/ROADMAP.md` for priorities
3. If task involves a new feature, verify it's in the roadmap

### After Completing Work

**You MUST update documentation after completing significant work:**

1. **After completing a feature:**
   - Update `docs/IMPLEMENTATION_STATUS.md` - mark feature/component as complete
   - Update `docs/ROADMAP.md` - move completed items, update priorities

2. **After fixing bugs:**
   - Update `docs/TECHNICAL_DEBT.md` if the bug was listed there

3. **After adding new entities/APIs:**
   - Update `docs/IMPLEMENTATION_STATUS.md` entity/API inventory sections

4. **After architectural decisions:**
   - Add to `docs/CONVERSATION_MEMORY_LOG.md` if significant

### Commit Workflow

When creating commits:
1. Use conventional commit format: `feat:`, `fix:`, `docs:`, `refactor:`
2. If the commit completes a feature, include doc updates in the same commit

### PR Workflow

When the user asks to create a PR:
1. Ensure all doc updates are included
2. Add documentation changes to PR description if applicable

---

## Build & Run Commands

```bash
# Build solution
dotnet build

# Run API (from solution root) - ALWAYS use https profile for frontend compatibility
dotnet run --project src/FamilyRelocation.API --launch-profile https

# Run with watch for development
dotnet watch run --project src/FamilyRelocation.API --launch-profile https

# Run frontend (from src/FamilyRelocation.Web)
npm run dev

# Apply EF Core migrations
dotnet ef database update --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API

# Create new migration
dotnet ef migrations add <MigrationName> --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API

# Run tests
dotnet test
```

## Architecture

Four-layer Clean Architecture with strict dependency rules:

```
FamilyRelocation.API          → Controllers, middleware, Program.cs
    ↓
FamilyRelocation.Application  → CQRS commands/queries, DTOs, validators (MediatR)
    ↓
FamilyRelocation.Infrastructure → EF Core, query handlers, AWS services (S3, SES, Cognito)
    ↓
FamilyRelocation.Domain       → Entities, value objects, domain events (ZERO external dependencies)
```

**Key constraint**: Domain layer has NO NuGet packages - pure C# only.

## Query Object Pattern (Mark Seemann's Approach)

We use query objects instead of traditional repository interfaces:

```
Application Layer:
  - Query/Command records (e.g., ExistsByEmailQuery, CreateApplicantCommand)
  - ALL handlers live here (queries and commands)
  - IApplicationDbContext for data access
```

**Why this pattern:**
- No constantly evolving IRepository interfaces
- Each query is explicit and self-documenting
- All handlers in one place (Application layer)
- MediatR handles dispatch automatically

**IApplicationDbContext provides:**
- `IQueryable<T> Set<T>()` - generic queryable access (Open/Closed principle)
- `void Add<T>(T entity)` - for adding entities
- `Task<int> SaveChangesAsync()` - for persistence

**JSON Column Queries:**
EF Core's `ToJson()` configuration enables LINQ queries into JSON columns (HusbandInfo, SpouseInfo).
No raw SQL needed - queries like `a.Husband.Email.Value == email` work directly.

## Ubiquitous Language (Required Terms)

| Correct | Incorrect |
|---------|-----------|
| Applicant | Contact |
| HousingSearch | Application, Deal |
| Wife | Spouse |
| ShabbosShul | ShabbosLocation |
| City (Union/Roselle Park) | Neighborhood |
| Property | Listing |

## Critical Design Decisions

1. **Board review at APPLICANT level** - Not HousingSearch level. Board approves the family once.

2. **HousingSearch represents the journey** - Single record per active effort. Failed contracts are preserved in `FailedContracts` collection, then search continues from HouseHunting stage.

3. **Value objects use C# records** - Modern approach with built-in value equality. No ValueObject base class.

4. **Entities use factory methods** - e.g., `Applicant.Create(...)` not public constructors.

5. **Domain events for side effects** - `AddDomainEvent(new ApplicantCreated(...))`.

6. **Desktop-first** - No mobile optimization required.

## Tech Stack

- .NET 10 (C# 13) - NOT .NET 8
- AWS services (Cognito, S3, SES) - NOT Azure
- PostgreSQL with EF Core 10
- React 18 + Vite + Ant Design

## Documentation Reference

| Document | Purpose |
|----------|---------|
| `docs/README.md` | Documentation index |
| `docs/IMPLEMENTATION_STATUS.md` | What's built vs outstanding |
| `docs/ROADMAP.md` | Priorities and upcoming work |
| `docs/SOLUTION_STRUCTURE_AND_CODE_v3.md` | Code patterns and examples |
| `docs/COMPREHENSIVE_TEST_PLAN.md` | Test cases |
| `docs/CONVERSATION_MEMORY_LOG.md` | Historical decisions |

Before making architectural changes, check these docs for prior decisions.

## After Context Compaction

When resuming from a compacted session (indicated by "This session is being continued from a previous conversation"):

1. Read `docs/IMPLEMENTATION_STATUS.md` for current state
2. Check `docs/ROADMAP.md` for priorities
3. Update `docs/CONVERSATION_MEMORY_LOG.md` with:
   - Key decisions made in the previous session
   - Files created or significantly modified
   - Current task status and next steps

This ensures continuity across long development sessions.
