# Family Relocation CRM - Documentation

> **Last Updated:** January 27, 2026
> **Version:** 1.0.0-beta

A custom CRM for managing Orthodox Jewish family relocation to Union County, NJ. Built with Clean Architecture + DDD + CQRS pattern.

---

## Quick Start

| If you want to... | Read this |
|-------------------|-----------|
| Understand the codebase quickly | [CLAUDE.md](../CLAUDE.md) (root) |
| See full AI context history | [CONVERSATION_MEMORY_LOG.md](CONVERSATION_MEMORY_LOG.md) |
| Check what's implemented | [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) |
| See upcoming work | [ROADMAP.md](ROADMAP.md) |
| Run tests | [COMPREHENSIVE_TEST_PLAN.md](COMPREHENSIVE_TEST_PLAN.md) |

---

## Documentation Map

### Core Reference (Read First)

| Document | Purpose | When to Use |
|----------|---------|-------------|
| [CLAUDE.md](../CLAUDE.md) | Quick reference for AI/developers | Starting any work session |
| [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) | What's built vs outstanding | Planning work, checking status |
| [ROADMAP.md](ROADMAP.md) | Priorities and upcoming features | Sprint planning |

### Requirements & Specifications

| Document | Purpose | When to Use |
|----------|---------|-------------|
| [MASTER_REQUIREMENTS_FINAL.md](MASTER_REQUIREMENTS_FINAL.md) | Original requirements (with corrections noted) | Understanding business rules |
| [FINAL_CORRECTIONS_JAN_2026.md](FINAL_CORRECTIONS_JAN_2026.md) | All corrections to original requirements | Checking what changed |
| [USER_STORIES_COMPLETE_LIST.md](USER_STORIES_COMPLETE_LIST.md) | All 68 user stories with status | Story reference |

### Technical Architecture

| Document | Purpose | When to Use |
|----------|---------|-------------|
| [SOLUTION_STRUCTURE_AND_CODE_v3.md](SOLUTION_STRUCTURE_AND_CODE_v3.md) | Complete code reference with examples | Writing new code |
| [SECURITY_ARCHITECTURE.md](SECURITY_ARCHITECTURE.md) | Auth, roles, security zones | Auth implementation |
| [BRANCHING_STRATEGY.md](BRANCHING_STRATEGY.md) | Git flow, versioning, CI/CD | Commits, releases |
| [TECHNICAL_DEBT.md](TECHNICAL_DEBT.md) | Known issues to address | Refactoring decisions |

### Testing & Quality

| Document | Purpose | When to Use |
|----------|---------|-------------|
| [COMPREHENSIVE_TEST_PLAN.md](COMPREHENSIVE_TEST_PLAN.md) | 201 manual test cases | QA testing |

### Sprint History

| Document | Purpose | When to Use |
|----------|---------|-------------|
| [sprint-plans/SPRINT_1_DETAILED_STORIES.md](sprint-plans/SPRINT_1_DETAILED_STORIES.md) | Sprint 1 implementation details | Reference for patterns |

### Full Context

| Document | Purpose | When to Use |
|----------|---------|-------------|
| [CONVERSATION_MEMORY_LOG.md](CONVERSATION_MEMORY_LOG.md) | Complete session history (70+ pages) | Resuming after break, understanding decisions |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         FamilyRelocation.API                      │
│                    (Controllers, Middleware)                      │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                     FamilyRelocation.Application                  │
│              (CQRS Commands/Queries, DTOs, Validators)           │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                    FamilyRelocation.Infrastructure                │
│                (EF Core, AWS S3/SES/Cognito, Services)           │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                       FamilyRelocation.Domain                     │
│            (Entities, Value Objects, Domain Events)              │
│                    *** ZERO EXTERNAL DEPENDENCIES ***            │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10, C# 13, MediatR (CQRS) |
| Database | PostgreSQL with EF Core 10 |
| Cloud | AWS (Cognito, S3, SES) |
| Frontend | React 18, Vite, Ant Design, TanStack Query |
| CI/CD | GitHub Actions |

---

## Key Commands

```bash
# Build
dotnet build

# Run API (always use https profile)
dotnet run --project src/FamilyRelocation.API --launch-profile https

# Run frontend
cd src/FamilyRelocation.Web && npm run dev

# Apply migrations
dotnet ef database update --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API

# Run tests
dotnet test
```

---

## Environments

| Environment | URL | Purpose |
|-------------|-----|---------|
| Development | https://dev.unionvaad.com | Testing & development |
| Production | (not yet deployed) | Live system |

---

## Archived Documentation

Old planning documents, superseded versions, and historical reference material is preserved in [_archive/](_archive/).

---

## Contributing

1. Read [CLAUDE.md](../CLAUDE.md) for coding standards
2. Follow [BRANCHING_STRATEGY.md](BRANCHING_STRATEGY.md) for git workflow
3. Update [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) when completing features
4. Add test cases to [COMPREHENSIVE_TEST_PLAN.md](COMPREHENSIVE_TEST_PLAN.md)
