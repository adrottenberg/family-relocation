# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A custom CRM for managing Orthodox Jewish family relocation to Union County, NJ. Built with Clean Architecture + DDD + CQRS pattern.

---

## MANDATORY WORKFLOW

**CRITICAL: Follow this workflow for ALL changes. No exceptions.**

### 1. Jira Ticket Requirement

**Every code change MUST have a Jira ticket.** Before starting any work:

1. **Check if ticket exists** in Jira project UN (Union Vaad)
2. **If no ticket exists**, create one using the Jira MCP:
   - Use appropriate type: Story (feature), Task (technical), Bug (defect)
   - Link to parent Epic if applicable
   - Add priority label (P0, P1, P2, P3)
3. **Reference the ticket** in branch names and commits (e.g., `feature/UN-87-description`)

### 2. Branching Strategy (Git Flow - STRICTLY ENFORCED)

**NEVER commit directly to `master` or `develop`. Always use feature branches and PRs.**

| Branch | Purpose | Create From | Merge To |
|--------|---------|-------------|----------|
| `master` | Production code | - | - |
| `develop` | Integration branch | - | - |
| `feature/<ticket>-<desc>` | New features | `develop` | `develop` via PR |
| `bugfix/<ticket>-<desc>` | Bug fixes | `develop` | `develop` via PR |
| `hotfix/<ticket>-<desc>` | Production fixes | `master` | `master` + `develop` via PR |
| `release/<version>` | Release prep | `develop` | `master` + `develop` |

**Branch naming examples:**
- `feature/UN-6-aws-production-setup`
- `bugfix/UN-10-fix-login-error`
- `hotfix/UN-99-critical-auth-fix`

### 3. Before Starting Work

1. **Ensure Jira ticket exists** (create if needed)
2. **Check current branch** - must be on correct feature/bugfix branch
3. **Pull latest from develop**: `git checkout develop && git pull`
4. **Create feature branch**: `git checkout -b feature/UN-XXX-description`
5. Check `docs/IMPLEMENTATION_STATUS.md` to understand current state
6. Check `docs/ROADMAP.md` for priorities

### 4. During Development

1. Make commits with conventional format referencing ticket:
   ```
   feat(UN-6): add user authentication
   fix(UN-10): resolve login redirect issue
   docs(UN-15): update API documentation
   ```
2. Keep commits atomic and focused

#### API Endpoint Changes

When creating or modifying API endpoints, you MUST:

1. **Update Postman collection** - Add/update requests in the Postman collection file
2. **Update .http file** - Add/update requests in the API test file
3. **Regenerate Swagger** - Run the API to generate updated OpenAPI spec:
   ```bash
   dotnet run --project src/FamilyRelocation.API --launch-profile https
   ```
   Then access `/swagger` to verify changes

#### Database Migrations

When creating new EF Core migrations:

1. **Create migration:**
   ```bash
   dotnet ef migrations add <MigrationName> --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API
   ```

2. **Apply migration BEFORE running API or deploying:**
   ```bash
   dotnet ef database update --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API
   ```

**Important:** Migrations must be applied before the API runs against that database. For deployments, run migrations as part of the deployment pipeline before starting the application.

### 5. After Completing Work

**You MUST update BOTH Jira AND documentation:**

#### Update Jira:
- Transition ticket status (To Do → In Progress → In Review → Done)
- Add comment summarizing changes if significant

#### Update Documentation:

| Change Type | Update These Files |
|-------------|-------------------|
| Feature complete | `docs/IMPLEMENTATION_STATUS.md`, `docs/ROADMAP.md` |
| Bug fix | `docs/TECHNICAL_DEBT.md` (if listed there) |
| New entity/API | `docs/IMPLEMENTATION_STATUS.md` (inventory sections) |
| Architecture decision | `docs/CONVERSATION_MEMORY_LOG.md` |

### 6. PR Workflow

**Claude will NOT auto-create PRs. User must explicitly request.**

When user requests PR creation:
1. Ensure all doc updates are committed
2. Push branch to remote
3. **Transition Jira ticket to "In Review"**
4. Ask user for confirmation before creating PR
5. Include in PR description:
   - Jira ticket reference
   - Summary of changes
   - Documentation updates made

### Jira Status Transitions

| Action | Jira Status |
|--------|-------------|
| Start working on ticket | **In Progress** |
| Push branch / Create PR | **In Review** |
| PR merged | **Done** |

### 7. Version Management

Version updates happen automatically on merge to `develop` via CI/CD.
- Follows Semantic Versioning: `MAJOR.MINOR.PATCH`
- See `docs/BRANCHING_STRATEGY.md` for details

---

## Quick Reference: Starting a New Task

```bash
# 1. Ensure Jira ticket exists (UN-XXX)
# 2. Update local develop
git checkout develop
git pull origin develop

# 3. Create feature branch
git checkout -b feature/UN-XXX-short-description

# 4. Do work, commit with ticket reference
git add .
git commit -m "feat(UN-XXX): description of change"

# 5. Push and request PR (Claude will ask for confirmation)
git push -u origin feature/UN-XXX-short-description
```

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
| `docs/BRANCHING_STRATEGY.md` | Git flow and versioning |
| `docs/specs/` | Detailed technical specifications |
| `docs/SOLUTION_STRUCTURE_AND_CODE_v3.md` | Code patterns and examples |
| `docs/COMPREHENSIVE_TEST_PLAN.md` | Test cases |
| `docs/CONVERSATION_MEMORY_LOG.md` | Historical decisions |

Before making architectural changes, check these docs for prior decisions.

## Documentation Architecture (Jira + Docs Hybrid)

**Jira is the source of truth for task tracking.** Detailed specs live in the codebase.

| Content Type | Location | When to Use |
|--------------|----------|-------------|
| User stories (short) | Jira ticket description | Always |
| Acceptance criteria | Jira ticket | Always |
| Technical specs | `docs/specs/UN-XXX-*.md` | Complex features |
| Architecture decisions | `docs/CONVERSATION_MEMORY_LOG.md` | Significant decisions |
| Implementation status | `docs/IMPLEMENTATION_STATUS.md` | Milestone updates |

### When to Create a Spec

Create a `docs/specs/UN-XXX-feature-name.md` when:
- Feature affects multiple layers (Domain, API, Frontend)
- API contracts need definition
- Complex business logic requires documentation
- Design review is beneficial before coding

### Spec Workflow

1. Create Jira ticket with user story + acceptance criteria
2. If complex, create `docs/specs/UN-XXX-description.md`
3. Add link to spec in Jira ticket description
4. Spec can be reviewed as part of PR or separately
5. Update spec status when implemented

## After Context Compaction

When resuming from a compacted session (indicated by "This session is being continued from a previous conversation"):

1. Read `docs/IMPLEMENTATION_STATUS.md` for current state
2. Check `docs/ROADMAP.md` for priorities
3. Check Jira for any in-progress tickets
4. Update `docs/CONVERSATION_MEMORY_LOG.md` with:
   - Key decisions made in the previous session
   - Files created or significantly modified
   - Current task status and next steps

This ensures continuity across long development sessions.

## Jira Project Reference

- **Project Key:** UN (Union Vaad)
- **Board:** https://adrottenberg.atlassian.net/jira/software/projects/UN/boards/34
- **Epics:**
  - UN-4: Production Launch (P0)
  - UN-5: MVP Polish (P1)
  - UN-2: Feature Enhancements (P2)
  - UN-3: Future Features (P3)
  - UN-1: Technical Debt

### Kanban Workflow Stages
1. **Backlog** - Unprioritized items
2. **Selected for Development** - Ready for current sprint
3. **In Progress** - Actively being coded
4. **In Review** - PR created, awaiting review
5. **Done** - Merged and complete
