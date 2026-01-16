# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A custom CRM for managing Orthodox Jewish family relocation to Union County, NJ. Built with Clean Architecture + DDD + CQRS pattern.

## Build & Run Commands

```bash
# Build solution
dotnet build

# Run API (from solution root)
dotnet run --project src/FamilyRelocation.API

# Run with watch for development
dotnet watch run --project src/FamilyRelocation.API

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
  - Command handlers that don't need EF Core specifics
  - IApplicationDbContext for simple operations (Add, SaveChanges, IQueryable access)

Infrastructure Layer:
  - Query handlers that need EF Core (e.g., FromSqlRaw for JSONB queries)
  - Located in Infrastructure/QueryHandlers/
```

**Why this pattern:**
- No constantly evolving IRepository interfaces
- Each query is explicit and self-documenting
- Handlers can live where their dependencies are (EF Core in Infrastructure)
- MediatR handles dispatch automatically

**IApplicationDbContext provides:**
- `IQueryable<Applicant> Applicants` - for LINQ queries
- `IQueryable<HousingSearch> HousingSearches` - for LINQ queries
- `void Add<TEntity>(TEntity entity)` - for adding entities
- `Task<int> SaveChangesAsync()` - for persistence

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
- React 18 + Vite + Ant Design (frontend, not yet implemented)

## Documentation Reference

- `docs/CONVERSATION_MEMORY_LOG.md` - Full project context and design decisions
- `docs/CLAUDE_CODE_CONTEXT.md` - Quick reference for AI development
- `docs/SOLUTION_STRUCTURE_AND_CODE_v3.md` - Complete code patterns and entity definitions
- `docs/sprint-plans/SPRINT_1_DETAILED_STORIES.md` - Current sprint implementation details

Before making architectural changes, check these docs for prior decisions.
