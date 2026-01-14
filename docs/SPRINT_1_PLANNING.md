# SPRINT 1 PLANNING
## Foundation, Domain Model & Basic CRUD

**Sprint Duration:** 2 weeks (10 working days)  
**Sprint Dates:** [Set when starting]  
**Team Capacity:** ~40-45 points (solo developer, part-time)  
**Committed Points:** 42 points  

---

## 🎯 SPRINT GOAL

Build a working Visual Studio solution with:
1. Clean Architecture foundation
2. AWS Cognito authentication
3. PostgreSQL database with EF Core
4. Core domain entities (DDD)
5. Complete applicant CRUD API

**Definition of Success:** Can authenticate, create/view/update/list applicants via API

---

## 📊 SPRINT BACKLOG

### Epic 1: Foundation & Setup (13 points)

| ID | Story | Points | Assignee | Status |
|----|-------|--------|----------|--------|
| US-001 | Set up Visual Studio solution structure | 5 | You | Not Started |
| US-002 | Configure AWS Cognito authentication | 5 | You | Not Started |
| US-003 | Set up PostgreSQL + EF Core | 3 | You | Not Started |

**Epic Goal:** Working solution with auth and database

---

### Epic 2: Domain Model (13 points)

| ID | Story | Points | Assignee | Status |
|----|-------|--------|----------|--------|
| US-004 | Implement core domain entities | 8 | You | Not Started |
| US-005 | Implement value objects | 5 | You | Not Started |

**Epic Goal:** DDD foundation with proper entities and value objects

---

### Epic 3: Applicant CRUD (16 points)

| ID | Story | Points | Assignee | Status |
|----|-------|--------|----------|--------|
| US-006 | Create applicant (coordinator) | 5 | You | Not Started |
| US-007 | View applicant details | 3 | You | Not Started |
| US-008 | Update applicant basic info | 3 | You | Not Started |
| US-009 | List applicants with search/filter | 5 | You | Not Started |

**Epic Goal:** Complete applicant management via API

---

## 📅 DAILY BREAKDOWN (Suggested)

### Day 1-2: Foundation
- ✅ Create Visual Studio solution (US-001)
- ✅ Set up Git repository
- ✅ Configure folder structure
- ✅ Install PostgreSQL locally (US-003)
- ✅ Create initial migration

**Deliverable:** Empty solution that builds

---

### Day 3-4: Authentication & Database
- ✅ Create AWS Cognito User Pool (US-002)
- ✅ Configure JWT authentication
- ✅ Create test users
- ✅ Test login endpoint
- ✅ Configure EF Core DbContext (US-003)
- ✅ Test database connection

**Deliverable:** Can log in and connect to database

---

### Day 5-6: Domain Model
- ✅ Create base Entity and ValueObject classes
- ✅ Implement all value objects (US-005)
- ✅ Write value object unit tests
- ✅ Implement Applicant entity (US-004)
- ✅ Implement Application entity (US-004)
- ✅ Write entity unit tests

**Deliverable:** Domain layer complete with tests

---

### Day 7-8: CRUD - Create & Read
- ✅ Create CreateApplicantCommand (US-006)
- ✅ Create GetApplicantQuery (US-007)
- ✅ Implement repositories
- ✅ Create API endpoints
- ✅ Test with Postman
- ✅ Write integration tests

**Deliverable:** Can create and view applicants via API

---

### Day 9-10: CRUD - Update & List
- ✅ Create UpdateApplicantCommand (US-008)
- ✅ Create GetApplicantsQuery with filters (US-009)
- ✅ Test pagination and search
- ✅ Write integration tests
- ✅ Sprint review prep
- ✅ Documentation updates

**Deliverable:** Complete applicant CRUD

---

## 🔧 TECHNICAL SETUP CHECKLIST

### Development Environment
- [ ] Visual Studio 2022 (17.8+)
- [ ] .NET 10 SDK installed
- [ ] PostgreSQL installed (or RDS ready)
- [ ] pgAdmin or DBeaver for database management
- [ ] Git configured
- [ ] AWS account with Cognito access
- [ ] Postman or Thunder Client for API testing

### NuGet Packages Required

**Domain Layer:**
- None (zero dependencies!)

**Application Layer:**
```bash
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```

**Infrastructure Layer:**
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package AWSSDK.CognitoIdentityProvider
dotnet add package AWSSDK.Extensions.NETCore.Setup
```

**API Layer:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## 📝 DEFINITION OF DONE

### For Each User Story:
- [ ] Code written following patterns in SOLUTION_STRUCTURE_AND_CODE.md
- [ ] Unit tests written and passing
- [ ] Integration tests written and passing (where applicable)
- [ ] Code compiles with no warnings
- [ ] API tested with Postman
- [ ] Code reviewed (self-review against docs)
- [ ] Committed to Git with meaningful message
- [ ] Documentation updated if needed

### For the Sprint:
- [ ] All 9 stories completed
- [ ] All tests passing
- [ ] API fully functional (can CRUD applicants)
- [ ] Authentication working
- [ ] Database migrations applied
- [ ] Code committed and pushed
- [ ] Sprint demo prepared
- [ ] Sprint retrospective conducted

---

## 🚀 GETTING STARTED

### Step 1: Review Documentation
```bash
# Read these first:
docs/CLAUDE_CODE_CONTEXT.md           # Quick reference
docs/SPRINT_1_DETAILED_STORIES.md     # Full story details
docs/SOLUTION_STRUCTURE_AND_CODE.md   # Code samples
```

### Step 2: Create Solution
```bash
# Follow US-001 instructions:
dotnet new sln -n FamilyRelocation
# ... (full commands in US-001)
```

### Step 3: Set Up Database
```bash
# Install PostgreSQL
# Configure connection string in appsettings.json
# Run migrations
dotnet ef migrations add InitialCreate --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API
dotnet ef database update --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API
```

### Step 4: Configure AWS Cognito
```bash
# Follow US-002 instructions
# Create User Pool
# Create test users
# Configure JWT in API
```

### Step 5: Start Coding
```bash
# Follow stories US-004 through US-009
# Copy code from SPRINT_1_DETAILED_STORIES.md
# Test as you go
```

---

## 🧪 TESTING STRATEGY

### Unit Tests (Domain Layer)
- Test value object validation
- Test value object equality
- Test entity factory methods
- Test entity domain methods
- Test domain events raised

**Example:**
```csharp
[Fact]
public void Address_WithInvalidZip_ThrowsException()
{
    // Arrange & Act & Assert
    Assert.Throws<ArgumentException>(() => 
        new Address("123 Main St", "Union", "NJ", "1234"));
}
```

### Integration Tests (API Layer)
- Test API endpoints
- Test authentication
- Test database interactions
- Test validation

**Example:**
```csharp
[Fact]
public async Task CreateApplicant_WithValidData_Returns201()
{
    // Arrange
    var client = _factory.CreateClient();
    var command = new CreateApplicantCommand { /* ... */ };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/applicants", command);
    
    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

---

## 📊 SPRINT METRICS

### Velocity Tracking
- **Planned Points:** 42
- **Completed Points:** ___ (fill at sprint end)
- **Velocity:** ___ (completed / planned)

### Story Completion
- **Day 2:** Should complete US-001, US-003 (8 points)
- **Day 4:** Should complete US-002 (13 points total)
- **Day 6:** Should complete US-004, US-005 (26 points total)
- **Day 8:** Should complete US-006, US-007 (34 points total)
- **Day 10:** Should complete US-008, US-009 (42 points total)

### Burn Down Chart
Track daily:
- Points remaining
- Stories completed
- Blockers encountered

---

## 🚧 RISK MANAGEMENT

### Identified Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| AWS Cognito setup issues | High | Medium | Have fallback to simple JWT, research Cognito docs first |
| PostgreSQL connection issues | High | Low | Test connection early, have local and RDS options |
| EF Core migration issues | Medium | Medium | Test migrations incrementally, keep backups |
| Underestimated complexity | Medium | Medium | Focus on MVP, defer nice-to-haves to Sprint 2 |
| Time availability | High | Medium | Adjust story points if needed, prioritize P0 stories |

---

## 📚 RESOURCES

### Documentation
- CONVERSATION_MEMORY_LOG.md - Full context
- MASTER_REQUIREMENTS_v3.md - Complete requirements
- SOLUTION_STRUCTURE_AND_CODE.md - Code samples
- SPRINT_1_DETAILED_STORIES.md - Story details

### External Resources
- AWS Cognito Docs: https://docs.aws.amazon.com/cognito/
- EF Core Docs: https://learn.microsoft.com/en-us/ef/core/
- PostgreSQL Docs: https://www.postgresql.org/docs/
- MediatR Docs: https://github.com/jbogard/MediatR
- FluentValidation: https://docs.fluentvalidation.net/

### Support
- Claude Code CLI: For quick coding help
- Web Console: For design decisions and complex questions
- GitHub Issues: Track bugs and technical debt

---

## 🎉 SPRINT REVIEW (End of Sprint)

### Demo Checklist
- [ ] Show working authentication (login)
- [ ] Show database connection (pgAdmin)
- [ ] Show create applicant API call (Postman)
- [ ] Show view applicant details
- [ ] Show update applicant
- [ ] Show list with search/filter
- [ ] Show code structure (Clean Architecture)

### What Went Well
- ___ (fill at sprint end)

### What Could Be Improved
- ___ (fill at sprint end)

### Action Items for Sprint 2
- ___ (fill at sprint end)

---

## 🔄 SPRINT RETROSPECTIVE

### Continue Doing
- ___ (fill at sprint end)

### Start Doing
- ___ (fill at sprint end)

### Stop Doing
- ___ (fill at sprint end)

---

## ➡️ NEXT SPRINT PREVIEW

**Sprint 2 Focus:**
- Application workflow (submit, approve, stages)
- Property management (basic CRUD)
- Board review workflow
- Email notifications (AWS SES)
- Application pipeline (Kanban view)

**Estimated Points:** ~40 points

---

**Sprint 1 Planning Complete - Ready to Start! 🚀**
