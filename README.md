# Family Relocation System

A custom CRM built specifically for managing Orthodox Jewish family relocation to Union County, NJ.

## 🎯 Project Overview

This system manages the complete family relocation process from initial application through move-in, including:

- Public application form (no authentication required)
- Board review and approval workflow
- Property matching and management
- Showing scheduling
- Application pipeline tracking (Kanban)
- Email notifications
- Activity tracking and follow-up reminders

**Status:** In Development - Sprint 1  
**Timeline:** 19-24 weeks part-time development  
**Current Sprint:** Foundation, Domain Model & Basic CRUD  

## 🏗️ Architecture

**Clean Architecture + Domain-Driven Design (DDD) + CQRS**

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  - React 18 Frontend (Ant Design)      │
│  - ASP.NET Core 10 API (Controllers)   │
└─────────────┬───────────────────────────┘
              │
┌─────────────▼───────────────────────────┐
│      Application Layer (CQRS)           │
│  - Commands (Write operations)          │
│  - Queries (Read operations)            │
│  - Handlers (MediatR)                   │
│  - DTOs & Validators                    │
└─────────────┬───────────────────────────┘
              │
┌─────────────▼───────────────────────────┐
│         Domain Layer (Pure DDD)         │
│  - 15 Entities                          │
│  - 7 Value Objects                      │
│  - Domain Events                        │
│  - ZERO Dependencies                    │
└─────────────┬───────────────────────────┘
              │
┌─────────────▼───────────────────────────┐
│      Infrastructure Layer               │
│  - EF Core + PostgreSQL                 │
│  - Repositories                         │
│  - AWS Services (S3, SES, Cognito)     │
└─────────────────────────────────────────┘
```

## 🛠️ Technology Stack

### Backend
- **.NET 10** (C# 13)
- **ASP.NET Core 10** Web API
- **Entity Framework Core 10**
- **PostgreSQL 16**
- **MediatR** (CQRS pattern)
- **FluentValidation** (input validation)
- **AutoMapper** (object mapping)

### Frontend
- **React 18** with TypeScript
- **Vite** (build tool)
- **Ant Design** (UI components)
- **TanStack Query** (API state management)
- **Zustand** (client state)
- **React Router** (routing)

### Cloud Infrastructure (AWS)
- **EC2** (t3.micro) - API hosting
- **RDS PostgreSQL** (db.t3.micro) - Database
- **S3** - File storage (photos, documents)
- **SES** - Email notifications
- **Cognito** - User authentication
- **SNS** - SMS notifications (optional)
- **Route 53** - DNS

### Development Tools
- **Visual Studio 2022** (.NET development)
- **VS Code** (React development)
- **Git** / **GitHub** (version control)
- **Postman** (API testing)

## 📁 Solution Structure

```
FamilyRelocation/
├── docs/                                    # Documentation
│   ├── CONVERSATION_MEMORY_LOG.md          # Full project context
│   ├── CLAUDE_CODE_CONTEXT.md              # Quick reference
│   ├── MASTER_REQUIREMENTS_v4_CORRECTED_SECTIONS.md
│   ├── SOLUTION_STRUCTURE_AND_CODE_v3.md
│   ├── FINAL_CORRECTIONS_JAN_2026.md
│   └── sprint-plans/
│       └── SPRINT_1_DETAILED_STORIES.md
│
├── src/
│   ├── FamilyRelocation.Domain/            # Core business logic (ZERO dependencies)
│   │   ├── Common/                         # Base classes, interfaces
│   │   ├── Entities/                       # 15 entities
│   │   ├── ValueObjects/                   # 7 value objects
│   │   ├── Enums/                          # 14 enums
│   │   ├── Events/                         # Domain events
│   │   └── Services/                       # Domain services
│   │
│   ├── FamilyRelocation.Application/       # Use cases & orchestration
│   │   ├── Common/                         # Shared interfaces, mappings
│   │   ├── Applicants/                     # Applicant CRUD (Commands/Queries)
│   │   ├── Applications/                   # Application workflow
│   │   ├── Properties/                     # Property management
│   │   ├── Reminders/                      # Follow-up reminders
│   │   └── Dashboard/                      # Dashboard queries
│   │
│   ├── FamilyRelocation.Infrastructure/    # External concerns
│   │   ├── Persistence/                    # EF Core DbContext, configurations
│   │   ├── Repositories/                   # Repository implementations
│   │   ├── Services/                       # External services
│   │   ├── AWS/                            # S3, SES, SNS, Cognito
│   │   └── Email/                          # Email templates
│   │
│   └── FamilyRelocation.API/               # Web API
│       ├── Controllers/                    # REST endpoints
│       ├── Middleware/                     # Auth, error handling
│       └── Program.cs                      # Startup configuration
│
├── .gitignore
├── README.md                                # This file
└── FamilyRelocation.sln                     # Visual Studio solution
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 16](https://www.postgresql.org/download/)
- [Node.js 20+](https://nodejs.org/) (for React frontend)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code
- [Git](https://git-scm.com/)
- AWS Account (for deployment)

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/family-relocation-system.git
cd family-relocation-system
```

### 2. Set Up the Database

**Install PostgreSQL locally:**
```bash
# Windows (using Chocolatey)
choco install postgresql

# Or download from https://www.postgresql.org/download/
```

**Create database:**
```sql
CREATE DATABASE FamilyRelocation;
```

**Update connection string in `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=FamilyRelocation;Username=postgres;Password=your_password"
  }
}
```

### 3. Apply Migrations

```bash
# From solution root
dotnet ef database update --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API
```

### 4. Configure AWS Services

**AWS Cognito (Authentication):**
1. Create User Pool in AWS Console
2. Create App Client
3. Update `appsettings.json`:
```json
{
  "AWS": {
    "Region": "us-east-1",
    "Cognito": {
      "UserPoolId": "us-east-1_XXXXXXXXX",
      "ClientId": "XXXXXXXXXXXXXXXXXXXXXXXXXX"
    }
  }
}
```

**Create test users:**
```bash
aws cognito-idp admin-create-user \
  --user-pool-id <pool-id> \
  --username coordinator@familyrelocation.org \
  --user-attributes Name=email,Value=coordinator@familyrelocation.org \
  --temporary-password TempPass123!
```

### 5. Run the Application

**Backend (API):**
```bash
cd src/FamilyRelocation.API
dotnet run
```

API will be available at: `https://localhost:5001`

**Frontend (React) - Coming in Sprint 2**
```bash
cd src/FamilyRelocation.Web
npm install
npm run dev
```

Frontend will be available at: `http://localhost:5173`

### 6. Verify Setup

**Check database connection:**
```bash
curl https://localhost:5001/api/health/database
```

**Login:**
```bash
POST https://localhost:5001/api/auth/login
{
  "email": "coordinator@familyrelocation.org",
  "password": "TempPass123!"
}
```

## 📚 Documentation

### For Developers

- **[SPRINT_1_DETAILED_STORIES.md](docs/sprint-plans/SPRINT_1_DETAILED_STORIES.md)** - Current sprint stories with full code samples
- **[SOLUTION_STRUCTURE_AND_CODE_v3.md](docs/SOLUTION_STRUCTURE_AND_CODE_v3.md)** - Complete code reference
- **[CLAUDE_CODE_CONTEXT.md](docs/CLAUDE_CODE_CONTEXT.md)** - Quick reference for AI-assisted development

### For Project Understanding

- **[MASTER_REQUIREMENTS_v4_CORRECTED_SECTIONS.md](docs/MASTER_REQUIREMENTS_v4_CORRECTED_SECTIONS.md)** - Product requirements
- **[CONVERSATION_MEMORY_LOG.md](docs/CONVERSATION_MEMORY_LOG.md)** - Full project context and decisions
- **[FINAL_CORRECTIONS_JAN_2026.md](docs/FINAL_CORRECTIONS_JAN_2026.md)** - All corrections applied

## 🎯 Current Sprint: Sprint 1

**Duration:** 2 weeks  
**Goal:** Foundation + Domain Model + Basic CRUD  
**Points:** 42 points  

### Sprint 1 Stories

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-001 | Set up Visual Studio solution structure | 5 | ✅ In Progress |
| US-002 | Configure AWS Cognito authentication | 5 | 🔲 Not Started |
| US-003 | Set up PostgreSQL + EF Core | 3 | 🔲 Not Started |
| US-004 | Implement core domain entities | 8 | 🔲 Not Started |
| US-005 | Implement value objects | 5 | 🔲 Not Started |
| US-006 | Create applicant (coordinator) | 5 | 🔲 Not Started |
| US-007 | View applicant details | 3 | 🔲 Not Started |
| US-008 | Update applicant basic info | 3 | 🔲 Not Started |
| US-009 | List applicants with search/filter | 5 | 🔲 Not Started |

**See:** [SPRINT_1_DETAILED_STORIES.md](docs/sprint-plans/SPRINT_1_DETAILED_STORIES.md) for complete details.

## 🔑 Key Design Decisions

### Domain Language (Ubiquitous Language)

**Use these terms consistently:**
- ✅ **Applicant** (NOT Contact) - A family applying to relocate
- ✅ **Application** (NOT Deal) - Their application submission
- ✅ **Wife** (NOT Spouse) - Reflecting community terminology
- ✅ **ShabbosShul** (NOT ShabbosLocation) - Where family davens on Shabbos
- ✅ **City** (NOT Neighborhood) - Union or Roselle Park

### Architecture Principles

1. **Domain has ZERO dependencies** - Pure C#, no NuGet packages
2. **Value objects are immutable** - All properties have `private set`
3. **Entities use factory methods** - `Applicant.CreateFromApplication(...)`
4. **Domain events for side effects** - `AddDomainEvent(new ApplicantCreated(...))`
5. **CQRS separation** - Commands for writes, Queries for reads

### Board Review Location

**CRITICAL:** Board review happens at **APPLICANT level**, NOT Application level.

Why? One applicant can have multiple applications (if first contract fails). Board decision stays with the applicant.

```csharp
// ✅ CORRECT
applicant.SetBoardDecision(BoardDecision.Approved, notes, reviewerId);

// ❌ WRONG
application.SetBoardDecision(...);  // DON'T DO THIS
```

## 🧪 Testing

### Run Unit Tests
```bash
dotnet test
```

### API Testing with Postman

Import the Postman collection from `docs/postman/` (coming soon).

### Test Coverage Goals
- Domain Layer: 90%+
- Application Layer: 80%+
- Controllers: 70%+

## 🚢 Deployment

### Cost Estimates

**Year 1 (Free Tier):**
- EC2 t3.micro: Free (12 months)
- RDS db.t3.micro: Free (12 months)
- S3: ~$0.50/month
- SES: $0 (included)
- **Total: ~$6/year**

**Year 2+ (Post Free Tier):**
- EC2: ~$8/month
- RDS: ~$15/month
- S3: ~$1/month
- **Total: ~$288/year**

**5-Year Total Cost: < $1,500**

Compare to:
- Salesforce: $75-150/user/month = $54K-108K over 5 years
- HubSpot: $45-120/month = $2.7K-7.2K over 5 years
- Custom solution on existing platform: $50K-100K upfront

### Deployment Steps

See `docs/DEPLOYMENT.md` (coming in Sprint 4)

## 🤝 Contributing

This is a private project for the Union County community. If you're working on this project:

1. **Read the docs first** - Especially `CLAUDE_CODE_CONTEXT.md`
2. **Follow the patterns** - Check `SOLUTION_STRUCTURE_AND_CODE_v3.md`
3. **Use correct terminology** - See "Key Design Decisions" above
4. **Write tests** - Domain logic must have unit tests
5. **Update documentation** - Keep docs in sync with code

### Git Workflow

```bash
# Create feature branch
git checkout -b feature/US-006-create-applicant

# Make changes, commit often
git add .
git commit -m "feat: implement CreateApplicantCommand with validation"

# Push and create PR
git push origin feature/US-006-create-applicant
```

**Commit Message Format:**
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `refactor:` Code refactoring
- `test:` Adding tests
- `chore:` Build/config changes

## 📞 Support

For questions or issues:
- **Email:** coordinator@familyrelocation.org
- **Documentation:** Check `docs/` folder first
- **Context:** See `CONVERSATION_MEMORY_LOG.md` for design decisions

## 📄 License

Private/Proprietary - Union County Orthodox Jewish Community

---

## 🎓 Learning Resources

**New to Clean Architecture?**
- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)

**New to CQRS?**
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR GitHub](https://github.com/jbogard/MediatR)

**New to .NET 10?**
- [What's New in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)

---

**Last Updated:** January 14, 2026  
**Version:** 1.0  
**Sprint:** 1 (Foundation)  

**Built with ❤️ for the Union County Orthodox Jewish Community**
