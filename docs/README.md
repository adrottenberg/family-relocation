# Family Relocation CRM - Documentation

> **Last Updated:** January 27, 2026
> **Implementation Status:** ~85% Complete

A custom CRM for managing Orthodox Jewish family relocation to Union County, NJ. Built with Clean Architecture + DDD + CQRS pattern.

---

## Quick Start

| If you want to... | Read this |
|-------------------|-----------|
| Understand the codebase quickly | [CLAUDE.md](../CLAUDE.md) (root) |
| Check what's implemented | [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) |
| See upcoming work | [ROADMAP.md](ROADMAP.md) |
| Run tests | [COMPREHENSIVE_TEST_PLAN.md](COMPREHENSIVE_TEST_PLAN.md) |

---

## Core Documentation (7 Files)

### Status & Planning

| Document | Purpose |
|----------|---------|
| [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) | What's built vs outstanding (~85% complete) |
| [ROADMAP.md](ROADMAP.md) | Priorities P0-P3, upcoming work |
| [USER_STORIES_COMPLETE_LIST.md](USER_STORIES_COMPLETE_LIST.md) | All 68 user stories with status |

### Technical Reference

| Document | Purpose |
|----------|---------|
| [SOLUTION_STRUCTURE_AND_CODE_v3.md](SOLUTION_STRUCTURE_AND_CODE_v3.md) | Authoritative code reference with examples |
| [SECURITY_ARCHITECTURE.md](SECURITY_ARCHITECTURE.md) | Auth, roles, security zones |
| [BRANCHING_STRATEGY.md](BRANCHING_STRATEGY.md) | Git flow, versioning, CI/CD |

### Quality & Operations

| Document | Purpose |
|----------|---------|
| [COMPREHENSIVE_TEST_PLAN.md](COMPREHENSIVE_TEST_PLAN.md) | 201 manual test cases |
| [TECHNICAL_DEBT.md](TECHNICAL_DEBT.md) | Known issues to address |

### Historical Reference

| Document | Purpose |
|----------|---------|
| [MASTER_REQUIREMENTS_FINAL.md](MASTER_REQUIREMENTS_FINAL.md) | Original business requirements |
| [CONVERSATION_MEMORY_LOG.md](CONVERSATION_MEMORY_LOG.md) | Full planning session history |
| [sprint-plans/](sprint-plans/) | Sprint implementation details |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         FamilyRelocation.API                     │
│                    (Controllers, Middleware)                     │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                     FamilyRelocation.Application                 │
│              (CQRS Commands/Queries, DTOs, Validators)          │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                    FamilyRelocation.Infrastructure               │
│                (EF Core, AWS S3/SES/Cognito, Services)          │
└─────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────┐
│                       FamilyRelocation.Domain                    │
│            (Entities, Value Objects, Domain Events)             │
│                    *** ZERO EXTERNAL DEPENDENCIES ***           │
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

Planning documents, superseded versions, and historical reference material is preserved in [_archive/](_archive/).

Archived files include:
- Old sprint plans (Sprint 2-5 detailed stories)
- Superseded requirement versions
- Applied corrections (now in codebase)
- Old code reviews
