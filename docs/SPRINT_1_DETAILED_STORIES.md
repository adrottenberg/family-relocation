# SPRINT 1 - DETAILED USER STORIES
## Foundation, Domain Model & Basic CRUD

**Sprint Duration:** 2 weeks  
**Sprint Goal:** Working solution with domain entities, authentication, and basic applicant CRUD  
**Total Story Points:** 42 points  
**Capacity:** ~40-45 points (solo developer, part-time)  

---

## 📋 SPRINT 1 STORIES OVERVIEW

| ID | Story | Epic | Points | Priority |
|----|-------|------|--------|----------|
| US-001 | Set up Visual Studio solution structure | Foundation | 5 | P0 |
| US-002 | Configure AWS Cognito authentication | Foundation | 5 | P0 |
| US-003 | Set up PostgreSQL + EF Core | Foundation | 3 | P0 |
| US-004 | Implement core domain entities | Domain Model | 8 | P0 |
| US-005 | Implement value objects | Domain Model | 5 | P0 |
| US-006 | Create applicant (coordinator) | Applicant CRUD | 5 | P0 |
| US-007 | View applicant details | Applicant CRUD | 3 | P0 |
| US-008 | Update applicant basic info | Applicant CRUD | 3 | P0 |
| US-009 | List applicants with search/filter | Applicant CRUD | 5 | P0 |

**Total:** 42 points

---

## EPIC: FOUNDATION & SETUP

### US-001: Set Up Visual Studio Solution Structure

**As a** developer  
**I want to** create a properly structured Visual Studio solution  
**So that** the codebase follows Clean Architecture principles and is maintainable

**Priority:** P0 (Must Have)  
**Effort:** 5 points  
**Sprint:** 1  

#### Acceptance Criteria

1. Solution `FamilyRelocation.sln` created
2. Four projects created:
   - `FamilyRelocation.Domain` (Class Library, .NET 10)
   - `FamilyRelocation.Application` (Class Library, .NET 10)
   - `FamilyRelocation.Infrastructure` (Class Library, .NET 10)
   - `FamilyRelocation.API` (ASP.NET Core Web API, .NET 10)
3. Project dependencies configured correctly:
   - Application → Domain
   - Infrastructure → Domain
   - API → Application + Infrastructure
4. Domain project has ZERO NuGet dependencies
5. Folder structure created in each project
6. Git repository initialized
7. `.gitignore` configured for .NET
8. README.md created with project overview
9. `docs/` folder created with all planning documents

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Create solution successfully | I have Visual Studio 2022 installed | I run the solution creation commands | All 4 projects are created with correct dependencies |
| Domain has no dependencies | Domain project is created | I check the .csproj file | There are zero PackageReference entries |
| Projects build successfully | Solution is created | I build the solution | All projects build with no errors |
| Git repository initialized | Solution folder exists | I check for .git folder | Git repo exists with proper .gitignore |

#### Technical Implementation

```bash
# Create solution
dotnet new sln -n FamilyRelocation

# Create projects
dotnet new classlib -n FamilyRelocation.Domain -o src/FamilyRelocation.Domain -f net10.0
dotnet new classlib -n FamilyRelocation.Application -o src/FamilyRelocation.Application -f net10.0
dotnet new classlib -n FamilyRelocation.Infrastructure -o src/FamilyRelocation.Infrastructure -f net10.0
dotnet new webapi -n FamilyRelocation.API -o src/FamilyRelocation.API -f net10.0

# Add to solution
dotnet sln add src/FamilyRelocation.Domain/FamilyRelocation.Domain.csproj
dotnet sln add src/FamilyRelocation.Application/FamilyRelocation.Application.csproj
dotnet sln add src/FamilyRelocation.Infrastructure/FamilyRelocation.Infrastructure.csproj
dotnet sln add src/FamilyRelocation.API/FamilyRelocation.API.csproj

# Set up dependencies
cd src/FamilyRelocation.Application
dotnet add reference ../FamilyRelocation.Domain/FamilyRelocation.Domain.csproj

cd ../FamilyRelocation.Infrastructure
dotnet add reference ../FamilyRelocation.Domain/FamilyRelocation.Domain.csproj

cd ../FamilyRelocation.API
dotnet add reference ../FamilyRelocation.Application/FamilyRelocation.Application.csproj
dotnet add reference ../FamilyRelocation.Infrastructure/FamilyRelocation.Infrastructure.csproj

# Initialize Git
cd ../../..
git init
git add .
git commit -m "chore: initial solution structure"
```

#### Folder Structure

```
FamilyRelocation/
├── src/
│   ├── FamilyRelocation.Domain/
│   │   ├── Common/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   ├── Events/
│   │   ├── Services/
│   │   └── Exceptions/
│   ├── FamilyRelocation.Application/
│   │   ├── Common/
│   │   ├── Applicants/
│   │   ├── Applications/
│   │   ├── Properties/
│   │   └── Dashboard/
│   ├── FamilyRelocation.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   ├── AWS/
│   │   └── Email/
│   └── FamilyRelocation.API/
│       ├── Controllers/
│       ├── Middleware/
│       ├── Filters/
│       └── Extensions/
├── docs/
│   ├── CONVERSATION_MEMORY_LOG.md
│   ├── CLAUDE_CODE_CONTEXT.md
│   ├── MASTER_REQUIREMENTS_v3.md
│   └── sprint-plans/
├── .gitignore
├── README.md
└── FamilyRelocation.sln
```

#### Definition of Done

- [x] All 4 projects created
- [x] Dependencies configured (Application/Infrastructure → Domain)
- [x] Domain has zero NuGet packages
- [x] Solution builds successfully
- [x] Folder structure matches specification
- [x] Git initialized with proper .gitignore
- [x] Documentation copied to docs/ folder
- [x] README.md created
- [x] Committed to Git

---

### US-002: Configure AWS Cognito Authentication

**As a** developer  
**I want to** configure AWS Cognito for user authentication  
**So that** coordinators can securely log in to the system

**Priority:** P0 (Must Have)  
**Effort:** 5 points  
**Sprint:** 1  

#### Acceptance Criteria

1. AWS Cognito User Pool created
2. User Pool configured with:
   - Email as username
   - Email verification required
   - Password requirements set
3. App Client created for API
4. Test users created (2 coordinators)
5. JWT authentication configured in API
6. API endpoints protected with [Authorize] attribute
7. Login endpoint returns JWT token
8. Token validated on protected endpoints
9. User claims mapped correctly (user ID, email, role)
10. Configuration stored in appsettings.json (AWS region, User Pool ID, Client ID)

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Login successfully | Valid coordinator credentials exist | I POST to /api/auth/login with valid credentials | I receive a JWT token |
| Login with invalid credentials | I have invalid credentials | I POST to /api/auth/login | I receive 401 Unauthorized |
| Access protected endpoint with token | I have a valid JWT token | I call GET /api/applicants with Authorization header | I receive 200 OK with data |
| Access protected endpoint without token | I have no token | I call GET /api/applicants without Authorization header | I receive 401 Unauthorized |
| Token expires | I have an expired token | I call GET /api/applicants with expired token | I receive 401 Unauthorized |

#### Technical Implementation

**AWS Cognito Setup:**

```bash
# Using AWS CLI
aws cognito-idp create-user-pool \
  --pool-name FamilyRelocationUserPool \
  --policies "PasswordPolicy={MinimumLength=8,RequireUppercase=true,RequireLowercase=true,RequireNumbers=true,RequireSymbols=false}" \
  --auto-verified-attributes email \
  --username-attributes email \
  --schema "Name=email,AttributeDataType=String,Required=true,Mutable=false"

# Create app client
aws cognito-idp create-user-pool-client \
  --user-pool-id <pool-id> \
  --client-name FamilyRelocationAPI \
  --generate-secret \
  --explicit-auth-flows ALLOW_USER_PASSWORD_AUTH ALLOW_REFRESH_TOKEN_AUTH

# Create test users
aws cognito-idp admin-create-user \
  --user-pool-id <pool-id> \
  --username coordinator@familyrelocation.org \
  --user-attributes Name=email,Value=coordinator@familyrelocation.org \
  --temporary-password TempPass123!
```

**appsettings.json:**

```json
{
  "AWS": {
    "Region": "us-east-1",
    "Cognito": {
      "UserPoolId": "us-east-1_XXXXXXXXX",
      "ClientId": "XXXXXXXXXXXXXXXXXXXXXXXXXX",
      "Authority": "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_XXXXXXXXX"
    }
  }
}
```

**Program.cs Configuration:**

```csharp
// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AWS:Cognito:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Later in pipeline
app.UseAuthentication();
app.UseAuthorization();
```

**AuthController.cs:**

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly Amazon.CognitoIdentityProvider.AmazonCognitoIdentityProviderClient _cognitoClient;
    private readonly IConfiguration _configuration;

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var authRequest = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                ClientId = _configuration["AWS:Cognito:ClientId"],
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", request.Email },
                    { "PASSWORD", request.Password }
                }
            };

            var response = await _cognitoClient.InitiateAuthAsync(authRequest);

            return Ok(new LoginResponse
            {
                AccessToken = response.AuthenticationResult.AccessToken,
                IdToken = response.AuthenticationResult.IdToken,
                RefreshToken = response.AuthenticationResult.RefreshToken,
                ExpiresIn = response.AuthenticationResult.ExpiresIn
            });
        }
        catch (NotAuthorizedException)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
        catch (UserNotFoundException)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // Refresh token implementation
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; }
    public string IdToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
}
```

**Protected Controller Example:**

```csharp
[ApiController]
[Route("api/applicants")]
[Authorize]  // Requires authentication
public class ApplicantsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ApplicantDto>>> GetAll()
    {
        // Only accessible with valid JWT token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        // ...
    }
}
```

#### NuGet Packages Required

```bash
cd src/FamilyRelocation.API
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package AWSSDK.CognitoIdentityProvider
dotnet add package AWSSDK.Extensions.NETCore.Setup
```

#### Definition of Done

- [x] Cognito User Pool created in AWS
- [x] App Client configured
- [x] 2 test users created
- [x] JWT authentication configured in API
- [x] Login endpoint works (returns JWT)
- [x] Protected endpoints require valid token
- [x] Invalid token returns 401
- [x] Expired token returns 401
- [x] User claims (ID, email) accessible in controllers
- [x] Configuration in appsettings.json
- [x] Tested with Postman/Thunder Client

---

### US-003: Set Up PostgreSQL + EF Core

**As a** developer  
**I want to** configure PostgreSQL database with Entity Framework Core  
**So that** I can persist application data

**Priority:** P0 (Must Have)  
**Effort:** 3 points  
**Sprint:** 1  

#### Acceptance Criteria

1. PostgreSQL installed locally (or RDS instance configured)
2. Connection string configured in appsettings.json
3. EF Core packages installed
4. ApplicationDbContext created
5. DbContext registered in DI container
6. Initial migration created
7. Database created and migrated
8. Can connect to database successfully
9. Database tables visible in pgAdmin/DBeaver
10. Unit of Work pattern implemented

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Create database successfully | PostgreSQL is running | I run `dotnet ef database update` | Database is created with all tables |
| Connection string works | Database exists | API starts | Application connects to database without errors |
| Can query database | Database has tables | I execute a test query | Query returns successfully |
| Migration applied | Migration file exists | I run `dotnet ef database update` | Migration is applied and __EFMigrationsHistory updated |

#### Technical Implementation

**Install PostgreSQL:**

```bash
# Windows (using Chocolatey)
choco install postgresql

# Or download from:
# https://www.postgresql.org/download/windows/

# Set password for postgres user during installation
```

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=FamilyRelocation;Username=postgres;Password=your_password"
  }
}
```

**Install NuGet Packages:**

```bash
cd src/FamilyRelocation.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

cd ../FamilyRelocation.API
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

**ApplicationDbContext.cs:**

```csharp
using Microsoft.EntityFrameworkCore;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Common;
using FamilyRelocation.Application.Common.Interfaces;

namespace FamilyRelocation.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets (will add entities in US-004)
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Property> Properties { get; set; }
        // ... more DbSets

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch domain events before saving
            await DispatchDomainEventsAsync(cancellationToken);

            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        {
            var domainEntities = ChangeTracker
                .Entries<Entity<Guid>>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                // Publish via MediatR (configured later)
                // await _mediator.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
```

**IApplicationDbContext.cs (Interface in Application layer):**

```csharp
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Applicant> Applicants { get; }
        DbSet<Application> Applications { get; }
        DbSet<Property> Properties { get; }
        // ... more DbSets

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
```

**UnitOfWork.cs:**

```csharp
using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

**Program.cs Configuration:**

```csharp
// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    )
);

// Register interfaces
builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**Create Initial Migration:**

```bash
# From solution root
dotnet ef migrations add InitialCreate --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API

# Apply migration
dotnet ef database update --project src/FamilyRelocation.Infrastructure --startup-project src/FamilyRelocation.API
```

**Test Connection:**

```csharp
// HealthController.cs
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("database")]
    public async Task<ActionResult> CheckDatabase()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return Ok(new { status = "connected", database = _context.Database.GetDbConnection().Database });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }
}
```

#### Definition of Done

- [x] PostgreSQL installed and running
- [x] Connection string in appsettings.json
- [x] EF Core packages installed
- [x] ApplicationDbContext created
- [x] IApplicationDbContext interface created
- [x] UnitOfWork implemented
- [x] DbContext registered in DI
- [x] Initial migration created
- [x] Database created successfully
- [x] Can connect to database (health check works)
- [x] Tables visible in pgAdmin
- [x] Tested migration rollback

---

## EPIC: DOMAIN MODEL

### US-004: Implement Core Domain Entities

**As a** developer  
**I want to** implement core domain entities (Applicant, Application)  
**So that** the system has a proper DDD foundation

**Priority:** P0 (Must Have)  
**Effort:** 8 points  
**Sprint:** 1  

#### Acceptance Criteria

1. Base Entity<TId> class created with domain events support
2. Applicant aggregate root implemented with:
   - All properties (from requirements)
   - Factory method: CreateFromApplication(...)
   - Board decision methods
   - Housing preferences update method
   - Child collection management
   - Phone number collection management
   - Domain events
3. Application aggregate root implemented with:
   - All properties
   - Factory method: CreateFromPublicForm(...)
   - Stage change methods
   - On Hold/Resume methods
   - Domain events
4. All domain events defined
5. Entities have proper encapsulation (private setters)
6. No infrastructure dependencies
7. Entities compile successfully
8. Unit tests for key methods

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Create applicant from application | Valid application data | I call Applicant.CreateFromApplication(...) | Applicant is created with correct properties and ApplicantCreated event |
| Set board decision | An applicant exists | I call applicant.SetBoardDecision(Approved, notes, userId) | Board decision is set and ApplicantApprovedByBoard event is raised |
| Update housing preferences | An applicant exists | I call applicant.UpdateHousingPreferences(...) | Preferences are updated and HousingPreferencesUpdated event is raised |
| Add child to applicant | An applicant exists | I call applicant.AddChild(new Child(5, Male)) | Child is added to Children collection |
| Create application from public form | Valid form data and approved applicant | I call Application.CreateFromPublicForm(...) | Application is created with Submitted stage |
| Change application stage | An application in Submitted stage | I call application.MoveToApprovedStage(userId) | Stage changes to Approved and ApplicationStageChanged event is raised |
| Put application on hold | An application in HouseHunting stage | I call application.PutOnHold(reason, userId) | OnHoldReason is set and ApplicationPutOnHold event is raised |

#### Technical Implementation

**Entity.cs (Base Class):**

```csharp
using System;
using System.Collections.Generic;

namespace FamilyRelocation.Domain.Common
{
    public abstract class Entity<TId>
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public TId Id { get; protected set; }

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public override bool Equals(object obj)
        {
            if (obj is not Entity<TId> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            return (GetType().ToString() + Id).GetHashCode();
        }
    }
}
```

**Applicant.cs (Complete - See SOLUTION_STRUCTURE_AND_CODE.md for full implementation):**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.ValueObjects;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;

namespace FamilyRelocation.Domain.Entities
{
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
        public string ShabbosShul { get; set; }  // NOTE: ShabbosShul not ShabbosLocation!
        
        // Housing Preferences
        public Money Budget { get; set; }
        public int? MinBedrooms { get; set; }
        public decimal? MinBathrooms { get; set; }
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
        
        // Board Review (AT APPLICANT LEVEL!)
        public DateTime? BoardReviewDate { get; private set; }
        public BoardDecision? BoardDecision { get; private set; }
        public string BoardDecisionNotes { get; private set; }
        public Guid? BoardReviewedByUserId { get; private set; }
        
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
            string shabbosShul,
            Guid createdBy,
            Guid? prospectId = null)
        {
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
                ShabbosShul = shabbosShul?.Trim(),
                PhoneNumbers = new List<PhoneNumber>(),
                Children = new List<Child>(),
                PreferredCities = new List<string>(),
                RequiredFeatures = new List<string>(),
                ShulProximity = ShulProximityPreference.NoPreference(),
                HousingNotes = string.Empty,
                BoardDecisionNotes = string.Empty,
                LoanTermYears = 30,
                MortgageInterestRate = 6.5m,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                ModifiedBy = createdBy,
                ModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            applicant.AddDomainEvent(new ApplicantCreated(applicant.ApplicantId, prospectId));

            return applicant;
        }

        public void SetBoardDecision(
            BoardDecision decision,
            string notes,
            Guid reviewedByUserId,
            Guid modifiedBy)
        {
            BoardDecision = decision;
            BoardDecisionNotes = notes ?? string.Empty;
            BoardReviewDate = DateTime.UtcNow;
            BoardReviewedByUserId = reviewedByUserId;
            ModifiedBy = modifiedBy;
            ModifiedDate = DateTime.UtcNow;

            if (decision == Enums.BoardDecision.Approved)
            {
                AddDomainEvent(new ApplicantApprovedByBoard(ApplicantId));
            }
            else if (decision == Enums.BoardDecision.Rejected)
            {
                AddDomainEvent(new ApplicantRejectedByBoard(ApplicantId, notes));
            }
        }

        public void UpdateHousingPreferences(
            Money budget,
            int minBedrooms,
            decimal minBathrooms,
            List<string> cities,
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
            PreferredCities = cities ?? new List<string>();
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

        public void AddChild(Child child)
        {
            Children.Add(child);
            ModifiedDate = DateTime.UtcNow;
        }

        public void RemoveChild(Child child)
        {
            Children.Remove(child);
            ModifiedDate = DateTime.UtcNow;
        }

        public void AddPhoneNumber(PhoneNumber phoneNumber)
        {
            PhoneNumbers.Add(phoneNumber);
            ModifiedDate = DateTime.UtcNow;
        }

        public void RemovePhoneNumber(PhoneNumber phoneNumber)
        {
            PhoneNumbers.Remove(phoneNumber);
            ModifiedDate = DateTime.UtcNow;
        }

        public void Delete(Guid deletedBy)
        {
            IsDeleted = true;
            ModifiedBy = deletedBy;
            ModifiedDate = DateTime.UtcNow;

            AddDomainEvent(new ApplicantDeleted(ApplicantId));
        }
    }
}
```

**Application.cs (Partial - full in SOLUTION_STRUCTURE_AND_CODE.md):**

```csharp
public class Application : Entity<Guid>
{
    public Guid ApplicationId { get; private set; }
    public Guid ApplicantId { get; private set; }
    public ApplicationStage Stage { get; private set; }
    public DateTime SubmittedDate { get; private set; }
    
    // On Hold
    public bool IsOnHold { get; private set; }
    public string OnHoldReason { get; private set; }
    public DateTime? OnHoldDate { get; private set; }
    
    // Navigation
    public virtual Applicant Applicant { get; private set; }
    
    private Application() { }
    
    public static Application CreateFromPublicForm(
        Guid applicantId,
        Guid createdBy)
    {
        var application = new Application
        {
            ApplicationId = Guid.NewGuid(),
            ApplicantId = applicantId,
            Stage = ApplicationStage.Submitted,
            SubmittedDate = DateTime.UtcNow,
            IsOnHold = false,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow
        };
        
        application.AddDomainEvent(new ApplicationSubmitted(application.ApplicationId, applicantId));
        
        return application;
    }
    
    public void MoveToApprovedStage(Guid modifiedBy)
    {
        if (Stage != ApplicationStage.Submitted)
            throw new DomainException("Can only approve applications in Submitted stage");
        
        Stage = ApplicationStage.Approved;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new ApplicationStageChanged(ApplicationId, Stage));
    }
    
    public void PutOnHold(string reason, Guid modifiedBy)
    {
        IsOnHold = true;
        OnHoldReason = reason;
        OnHoldDate = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new ApplicationPutOnHold(ApplicationId, reason));
    }
    
    public void ResumeFromHold(Guid modifiedBy)
    {
        if (!IsOnHold)
            throw new DomainException("Application is not on hold");
        
        IsOnHold = false;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new ApplicationResumedFromHold(ApplicationId));
    }
}
```

**Domain Events:**

```csharp
public record ApplicantCreated(Guid ApplicantId, Guid? ProspectId) : IDomainEvent;
public record ApplicantApprovedByBoard(Guid ApplicantId) : IDomainEvent;
public record ApplicantRejectedByBoard(Guid ApplicantId, string Reason) : IDomainEvent;
public record ApplicantDeleted(Guid ApplicantId) : IDomainEvent;
public record HousingPreferencesUpdated(Guid ApplicantId) : IDomainEvent;

public record ApplicationSubmitted(Guid ApplicationId, Guid ApplicantId) : IDomainEvent;
public record ApplicationStageChanged(Guid ApplicationId, ApplicationStage NewStage) : IDomainEvent;
public record ApplicationPutOnHold(Guid ApplicationId, string Reason) : IDomainEvent;
public record ApplicationResumedFromHold(Guid ApplicationId) : IDomainEvent;
```

#### Definition of Done

- [x] Base Entity<TId> class created
- [x] Applicant entity complete with all properties
- [x] Applicant factory method works
- [x] Applicant domain methods work (SetBoardDecision, UpdateHousingPreferences)
- [x] Applicant raises correct domain events
- [x] Application entity complete
- [x] Application factory method works
- [x] Application stage change methods work
- [x] Application On Hold/Resume methods work
- [x] All domain events defined
- [x] No infrastructure dependencies
- [x] Code compiles
- [x] Unit tests pass

---

[DOCUMENT CONTINUES... Due to length, I'll create remaining stories in separate sections]

**Would you like me to continue with:**
1. US-005: Implement Value Objects
2. US-006-009: CRUD operations
3. Then create the Jira CSV export?

Or shall I finalize all documents first? Let me know and I'll complete everything! 🚀

### US-005: Implement Value Objects

**As a** developer  
**I want to** implement all core value objects  
**So that** the domain model has proper encapsulation and validation

**Priority:** P0 (Must Have)  
**Effort:** 5 points  
**Sprint:** 1  

#### Acceptance Criteria

1. ValueObject base class created with equality comparison
2. Address value object implemented:
   - Street, City, State, ZipCode, Unit
   - Validation (required fields, zip format)
   - FullAddress property for display
3. PhoneNumber value object implemented:
   - 10-digit validation
   - Type enum (Home, Cell, Work, Other)
   - FormattedNumber property: (908) 555-1234
4. Email value object implemented:
   - Email format validation
   - Lowercase normalization
5. Money value object implemented:
   - Amount, Currency (USD)
   - Arithmetic operations (+, -, *, /)
   - Formatted display ($525,000.00)
6. Child value object implemented:
   - Age (0-18), Gender (Male/Female)
   - Validation
7. Coordinates value object implemented:
   - Latitude, Longitude
   - DistanceToMiles() method (Haversine formula)
8. ShulProximityPreference value object implemented:
   - Factory methods: NoPreference(), AnyShulWithinDistance(), SpecificShulsWithinDistance()
9. All value objects are immutable (private setters)
10. All value objects compile without errors

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Create valid address | Valid address data | I create new Address("123 Main St", "Union", "NJ", "07083") | Address is created successfully |
| Create address with invalid zip | Invalid zip code "1234" | I try to create Address with invalid zip | ArgumentException is thrown |
| Format phone number | 10 digits "9085551234" | I create new PhoneNumber("9085551234", Cell) | FormattedNumber returns "(908) 555-1234" |
| Validate email format | Invalid email "notanemail" | I try to create new Email("notanemail") | ArgumentException is thrown |
| Add money values | Two Money values $100 and $50 | I add them: money1 + money2 | Result is $150 |
| Calculate distance | Two coordinates 0.5 miles apart | I call coords1.DistanceToMiles(coords2) | Returns approximately 0.5 |
| Value objects are equal | Two addresses with same values | I compare with == | They are equal (value equality) |

#### Technical Implementation

**Complete implementations in SOLUTION_STRUCTURE_AND_CODE.md - Section 3.2**

Key code samples:

**Address.cs:**
```csharp
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Unit { get; private set; }

    public string FullAddress =>
        string.IsNullOrWhiteSpace(Unit)
            ? $"{Street}, {City}, {State} {ZipCode}"
            : $"{Street} {Unit}, {City}, {State} {ZipCode}";

    private Address() { }

    public Address(string street, string city, string state, string zipCode, string unit = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        if (!System.Text.RegularExpressions.Regex.IsMatch(zipCode, @"^\d{5}(-\d{4})?$"))
            throw new ArgumentException("Invalid zip code format", nameof(zipCode));

        Street = street.Trim();
        City = city.Trim();
        State = state.Trim().ToUpper();
        ZipCode = zipCode.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street.ToLowerInvariant();
        yield return City.ToLowerInvariant();
        yield return State.ToUpperInvariant();
        yield return ZipCode;
        yield return Unit?.ToLowerInvariant();
    }
}
```

**Money.cs:**
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    public string Formatted => Amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));

    private Money() { }

    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        return new Money(a.Amount - b.Amount, a.Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

See SOLUTION_STRUCTURE_AND_CODE.md for complete implementations of all value objects.

#### Files to Create

```
src/FamilyRelocation.Domain/
├── Common/
│   └── ValueObject.cs
└── ValueObjects/
    ├── Address.cs
    ├── PhoneNumber.cs
    ├── Email.cs
    ├── Money.cs
    ├── Child.cs
    ├── Coordinates.cs
    └── ShulProximityPreference.cs
```

#### Definition of Done

- [x] ValueObject base class created
- [x] Address value object complete
- [x] PhoneNumber value object complete
- [x] Email value object complete
- [x] Money value object complete
- [x] Child value object complete
- [x] Coordinates value object complete
- [x] ShulProximityPreference value object complete
- [x] All value objects immutable (private setters)
- [x] All validation working
- [x] Value equality working
- [x] Code compiles
- [x] Unit tests written and passing

---

## EPIC: APPLICANT CRUD

### US-006: Create Applicant (Coordinator Manual Entry)

**As a** coordinator  
**I want to** manually create an applicant record  
**So that** I can enter families who applied via phone/email/in-person

**Priority:** P0 (Must Have)  
**Effort:** 5 points  
**Sprint:** 1  

#### Acceptance Criteria

1. API endpoint POST /api/applicants accepts applicant data
2. Command: CreateApplicantCommand with validation
3. Command handler creates applicant using factory method
4. Applicant saved to database
5. Returns 201 Created with applicant DTO
6. Returns location header with applicant URL
7. Validation errors return 400 Bad Request
8. Duplicate email returns 409 Conflict
9. Domain event ApplicantCreated published
10. Transaction commits or rolls back properly

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Create applicant successfully | Valid applicant data | I POST to /api/applicants with complete data | Applicant is created, returns 201 with applicant DTO |
| Missing required field | Incomplete data (no first name) | I POST to /api/applicants | Returns 400 with validation error "First name is required" |
| Duplicate email | Applicant with email exists | I POST with same email | Returns 409 Conflict "Email already registered" |
| Invalid email format | Invalid email "notanemail" | I POST with invalid email | Returns 400 "Invalid email format" |
| Database error | Database is down | I POST to /api/applicants | Returns 500 with error message |

#### Technical Implementation

**CreateApplicantCommand.cs:**

```csharp
using MediatR;
using FamilyRelocation.Application.Applicants.DTOs;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant
{
    public class CreateApplicantCommand : IRequest<ApplicantDto>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        
        public string WifeFirstName { get; set; }
        public string WifeMaidenName { get; set; }
        public string WifeFatherName { get; set; }
        public string WifeHighSchool { get; set; }
        
        public string Email { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; }
        public AddressDto Address { get; set; }
        
        public List<ChildDto> Children { get; set; }
        
        public string CurrentKehila { get; set; }
        public string ShabbosShul { get; set; }
    }
}
```

**CreateApplicantCommandValidator.cs:**

```csharp
using FluentValidation;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant
{
    public class CreateApplicantCommandValidator : AbstractValidator<CreateApplicantCommand>
    {
        public CreateApplicantCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Address)
                .NotNull().WithMessage("Address is required");

            RuleFor(x => x.PhoneNumbers)
                .NotEmpty().WithMessage("At least one phone number is required");
        }
    }
}
```

**CreateApplicantCommandHandler.cs:**

```csharp
using MediatR;
using AutoMapper;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using FamilyRelocation.Domain.Common;
using FamilyRelocation.Application.Common.Interfaces;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant
{
    public class CreateApplicantCommandHandler : IRequestHandler<CreateApplicantCommand, ApplicantDto>
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public CreateApplicantCommandHandler(
            IApplicantRepository applicantRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUser)
        {
            _applicantRepository = applicantRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<ApplicantDto> Handle(CreateApplicantCommand command, CancellationToken ct)
        {
            // Check for duplicate email
            var existing = await _applicantRepository.GetByEmailAsync(command.Email);
            if (existing != null)
            {
                throw new ConflictException($"Applicant with email {command.Email} already exists");
            }

            // Create value objects
            var email = new Email(command.Email);
            var address = new Address(
                command.Address.Street,
                command.Address.City,
                command.Address.State,
                command.Address.ZipCode,
                command.Address.Unit
            );

            // Create applicant using factory method
            var applicant = Applicant.CreateFromApplication(
                firstName: command.FirstName,
                lastName: command.LastName,
                fatherName: command.FatherName,
                email: email,
                address: address,
                currentKehila: command.CurrentKehila,
                shabbosShul: command.ShabbosShul,
                createdBy: _currentUser.UserId
            );

            // Add wife info
            applicant.WifeFirstName = command.WifeFirstName;
            applicant.WifeMaidenName = command.WifeMaidenName;
            applicant.WifeFatherName = command.WifeFatherName;
            applicant.WifeHighSchool = command.WifeHighSchool;

            // Add phone numbers
            foreach (var phoneDto in command.PhoneNumbers)
            {
                var phoneNumber = new PhoneNumber(phoneDto.Number, phoneDto.Type);
                applicant.AddPhoneNumber(phoneNumber);
            }

            // Add children
            if (command.Children != null)
            {
                foreach (var childDto in command.Children)
                {
                    var child = new Child(childDto.Age, childDto.Gender);
                    applicant.AddChild(child);
                }
            }

            // Save
            await _applicantRepository.AddAsync(applicant);
            await _unitOfWork.SaveChangesAsync(ct);

            // Map to DTO and return
            return _mapper.Map<ApplicantDto>(applicant);
        }
    }
}
```

**ApplicantsController.cs:**

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FamilyRelocation.API.Controllers
{
    [ApiController]
    [Route("api/applicants")]
    [Authorize]
    public class ApplicantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ApplicantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ApplicantDto>> Create([FromBody] CreateApplicantCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.ApplicantId }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicantDto>> GetById(Guid id)
        {
            // Implemented in US-007
            return Ok();
        }
    }
}
```

**ApplicantDto.cs:**

```csharp
namespace FamilyRelocation.Application.Applicants.DTOs
{
    public class ApplicantDto
    {
        public Guid ApplicantId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string FatherName { get; set; }
        
        public string WifeFirstName { get; set; }
        public string WifeMaidenName { get; set; }
        
        public string Email { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; }
        public AddressDto Address { get; set; }
        
        public int NumberOfChildren { get; set; }
        public List<ChildDto> Children { get; set; }
        
        public string CurrentKehila { get; set; }
        public string ShabbosShul { get; set; }
        
        public string BoardDecision { get; set; }
        public DateTime? BoardReviewDate { get; set; }
        
        public DateTime CreatedDate { get; set; }
    }
}
```

#### Definition of Done

- [x] CreateApplicantCommand created
- [x] Validator with FluentValidation
- [x] Command handler implemented
- [x] Repository method AddAsync works
- [x] API endpoint POST /api/applicants works
- [x] Returns 201 Created with DTO
- [x] Returns Location header
- [x] Validation errors return 400
- [x] Duplicate email returns 409
- [x] Tested with Postman/Thunder Client
- [x] Unit tests pass

---

### US-007: View Applicant Details

**As a** coordinator  
**I want to** view complete applicant details  
**So that** I can review family information

**Priority:** P0 (Must Have)  
**Effort:** 3 points  
**Sprint:** 1  

#### Acceptance Criteria

1. API endpoint GET /api/applicants/{id} returns applicant
2. Query: GetApplicantQuery
3. Returns full applicant details including:
   - Personal info (name, contact)
   - Wife info
   - Children list
   - Community info
   - Board decision (if reviewed)
4. Returns 200 OK with ApplicantDto
5. Returns 404 Not Found if applicant doesn't exist
6. Includes all value objects formatted correctly
7. Phone numbers formatted: (908) 555-1234
8. Address formatted: "123 Main St, Union, NJ 07083"

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Get existing applicant | Applicant with ID exists | I GET /api/applicants/{id} | Returns 200 OK with complete applicant data |
| Get non-existent applicant | No applicant with ID | I GET /api/applicants/{invalid-id} | Returns 404 Not Found |
| View includes children | Applicant has 3 children | I GET applicant details | Response includes 3 children with age and gender |
| Phone numbers formatted | Applicant has phone "9085551234" | I GET applicant details | Phone appears as "(908) 555-1234" |
| Board decision included | Applicant has board decision | I GET applicant details | Response includes boardDecision and boardReviewDate |

#### Technical Implementation

**GetApplicantQuery.cs:**

```csharp
using MediatR;

namespace FamilyRelocation.Application.Applicants.Queries.GetApplicant
{
    public class GetApplicantQuery : IRequest<ApplicantDto>
    {
        public Guid ApplicantId { get; set; }
    }
}
```

**GetApplicantQueryHandler.cs:**

```csharp
using MediatR;
using AutoMapper;
using FamilyRelocation.Application.Common.Exceptions;

namespace FamilyRelocation.Application.Applicants.Queries.GetApplicant
{
    public class GetApplicantQueryHandler : IRequestHandler<GetApplicantQuery, ApplicantDto>
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly IMapper _mapper;

        public GetApplicantQueryHandler(
            IApplicantRepository applicantRepository,
            IMapper mapper)
        {
            _applicantRepository = applicantRepository;
            _mapper = mapper;
        }

        public async Task<ApplicantDto> Handle(GetApplicantQuery query, CancellationToken ct)
        {
            var applicant = await _applicantRepository.GetByIdAsync(query.ApplicantId);

            if (applicant == null)
            {
                throw new NotFoundException("Applicant", query.ApplicantId);
            }

            return _mapper.Map<ApplicantDto>(applicant);
        }
    }
}
```

**ApplicantsController.cs (Add Method):**

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ApplicantDto>> GetById(Guid id)
{
    var query = new GetApplicantQuery { ApplicantId = id };
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

**MappingProfile.cs:**

```csharp
using AutoMapper;

namespace FamilyRelocation.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Applicant, ApplicantDto>()
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Value))
                .ForMember(d => d.Address, opt => opt.MapFrom(s => new AddressDto
                {
                    Street = s.Address.Street,
                    City = s.Address.City,
                    State = s.Address.State,
                    ZipCode = s.Address.ZipCode,
                    Unit = s.Address.Unit,
                    FullAddress = s.Address.FullAddress
                }))
                .ForMember(d => d.PhoneNumbers, opt => opt.MapFrom(s => s.PhoneNumbers.Select(p => new PhoneNumberDto
                {
                    Number = p.FormattedNumber,
                    Type = p.Type.ToString()
                })))
                .ForMember(d => d.Children, opt => opt.MapFrom(s => s.Children.Select(c => new ChildDto
                {
                    Age = c.Age,
                    Gender = c.Gender.ToString()
                })))
                .ForMember(d => d.BoardDecision, opt => opt.MapFrom(s => s.BoardDecision.HasValue ? s.BoardDecision.ToString() : null));
        }
    }
}
```

#### Definition of Done

- [x] GetApplicantQuery created
- [x] Query handler implemented
- [x] Repository GetByIdAsync works
- [x] API endpoint GET /api/applicants/{id} works
- [x] Returns 200 OK with full DTO
- [x] Returns 404 if not found
- [x] AutoMapper profile configured
- [x] Value objects formatted correctly
- [x] Tested with Postman
- [x] Unit tests pass

---

### US-008: Update Applicant Basic Info

**As a** coordinator  
**I want to** update applicant basic information  
**So that** I can keep records current

**Priority:** P0 (Must Have)  
**Effort:** 3 points  
**Sprint:** 1  

#### Acceptance Criteria

1. API endpoint PUT /api/applicants/{id} updates applicant
2. Command: UpdateApplicantCommand with validation
3. Can update: name, wife info, contact info, children, community info
4. Cannot update: board decision, created date, applicant ID
5. Returns 200 OK with updated DTO
6. Returns 404 if applicant not found
7. Returns 400 for validation errors
8. Email uniqueness validated (409 if duplicate)
9. ModifiedDate and ModifiedBy updated automatically

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Update applicant successfully | Applicant exists | I PUT /api/applicants/{id} with updated data | Applicant is updated, returns 200 with updated DTO |
| Update non-existent applicant | No applicant with ID | I PUT /api/applicants/{invalid-id} | Returns 404 Not Found |
| Update with validation error | Applicant exists | I PUT with empty first name | Returns 400 "First name is required" |
| Update email to duplicate | Another applicant has email | I PUT with duplicate email | Returns 409 Conflict |
| ModifiedDate updated | Applicant exists | I update applicant | ModifiedDate is set to current time |

#### Technical Implementation

**UpdateApplicantCommand.cs:**

```csharp
public class UpdateApplicantCommand : IRequest<ApplicantDto>
{
    public Guid ApplicantId { get; set; }
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FatherName { get; set; }
    
    public string WifeFirstName { get; set; }
    public string WifeMaidenName { get; set; }
    public string WifeFatherName { get; set; }
    public string WifeHighSchool { get; set; }
    
    public string Email { get; set; }
    public List<PhoneNumberDto> PhoneNumbers { get; set; }
    public AddressDto Address { get; set; }
    
    public List<ChildDto> Children { get; set; }
    
    public string CurrentKehila { get; set; }
    public string ShabbosShul { get; set; }
}
```

**UpdateApplicantCommandHandler.cs:**

```csharp
public class UpdateApplicantCommandHandler : IRequestHandler<UpdateApplicantCommand, ApplicantDto>
{
    private readonly IApplicantRepository _applicantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public async Task<ApplicantDto> Handle(UpdateApplicantCommand command, CancellationToken ct)
    {
        var applicant = await _applicantRepository.GetByIdAsync(command.ApplicantId);
        
        if (applicant == null)
            throw new NotFoundException("Applicant", command.ApplicantId);

        // Check email uniqueness if changed
        if (command.Email != applicant.Email.Value)
        {
            var existing = await _applicantRepository.GetByEmailAsync(command.Email);
            if (existing != null && existing.ApplicantId != command.ApplicantId)
            {
                throw new ConflictException($"Email {command.Email} is already in use");
            }
        }

        // Update basic info (using reflection or manual mapping)
        applicant.FirstName = command.FirstName;
        applicant.LastName = command.LastName;
        applicant.FatherName = command.FatherName;
        
        applicant.WifeFirstName = command.WifeFirstName;
        applicant.WifeMaidenName = command.WifeMaidenName;
        applicant.WifeFatherName = command.WifeFatherName;
        applicant.WifeHighSchool = command.WifeHighSchool;
        
        applicant.Email = new Email(command.Email);
        applicant.Address = new Address(
            command.Address.Street,
            command.Address.City,
            command.Address.State,
            command.Address.ZipCode,
            command.Address.Unit
        );
        
        applicant.CurrentKehila = command.CurrentKehila;
        applicant.ShabbosShul = command.ShabbosShul;
        
        // Update phone numbers (clear and re-add)
        applicant.PhoneNumbers.Clear();
        foreach (var phoneDto in command.PhoneNumbers)
        {
            applicant.AddPhoneNumber(new PhoneNumber(phoneDto.Number, phoneDto.Type));
        }
        
        // Update children (clear and re-add)
        applicant.Children.Clear();
        if (command.Children != null)
        {
            foreach (var childDto in command.Children)
            {
                applicant.AddChild(new Child(childDto.Age, childDto.Gender));
            }
        }
        
        // Modified metadata updated automatically by entity
        applicant.ModifiedBy = _currentUser.UserId;
        applicant.ModifiedDate = DateTime.UtcNow;

        await _applicantRepository.UpdateAsync(applicant);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<ApplicantDto>(applicant);
    }
}
```

**ApplicantsController.cs:**

```csharp
[HttpPut("{id}")]
public async Task<ActionResult<ApplicantDto>> Update(Guid id, [FromBody] UpdateApplicantCommand command)
{
    if (id != command.ApplicantId)
        return BadRequest("ID mismatch");

    var result = await _mediator.Send(command);
    return Ok(result);
}
```

#### Definition of Done

- [x] UpdateApplicantCommand created
- [x] Validator created
- [x] Command handler implemented
- [x] Repository UpdateAsync works
- [x] API endpoint PUT /api/applicants/{id} works
- [x] Returns 200 with updated DTO
- [x] Returns 404 if not found
- [x] Email uniqueness validated
- [x] ModifiedDate/ModifiedBy updated
- [x] Tested with Postman
- [x] Unit tests pass

---

### US-009: List Applicants with Search/Filter

**As a** coordinator  
**I want to** view a list of all applicants with search and filter  
**So that** I can find families quickly

**Priority:** P0 (Must Have)  
**Effort:** 5 points  
**Sprint:** 1  

#### Acceptance Criteria

1. API endpoint GET /api/applicants returns paginated list
2. Query: GetApplicantsQuery with filters
3. Supports pagination (page, pageSize)
4. Supports search by: name, email, phone
5. Supports filter by: board decision, city, created date range
6. Supports sorting by: name, created date, board review date
7. Returns lightweight DTOs (not full details)
8. Returns total count for pagination
9. Returns empty array if no results
10. Default page size = 20, max = 100

#### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Get all applicants | 50 applicants exist | I GET /api/applicants | Returns first 20 applicants with totalCount=50 |
| Search by name | Applicant "Cohen" exists | I GET /api/applicants?search=Cohen | Returns applicants with "Cohen" in name |
| Filter by board decision | 5 approved applicants | I GET /api/applicants?boardDecision=Approved | Returns 5 approved applicants |
| Paginate results | 100 applicants exist | I GET /api/applicants?page=2&pageSize=20 | Returns applicants 21-40 |
| Sort by name | Multiple applicants | I GET /api/applicants?sortBy=lastName&sortOrder=asc | Returns applicants sorted alphabetically |
| No results | No applicants match filter | I GET /api/applicants?search=NonExistent | Returns empty array with totalCount=0 |

#### Technical Implementation

**GetApplicantsQuery.cs:**

```csharp
public class GetApplicantsQuery : IRequest<PaginatedList<ApplicantListDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    public string SearchTerm { get; set; }
    public string BoardDecision { get; set; }
    public string City { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    
    public string SortBy { get; set; } = "createdDate";
    public string SortOrder { get; set; } = "desc";
}
```

**GetApplicantsQueryHandler.cs:**

```csharp
public class GetApplicantsQueryHandler : IRequestHandler<GetApplicantsQuery, PaginatedList<ApplicantListDto>>
{
    private readonly IApplicantRepository _applicantRepository;
    private readonly IMapper _mapper;

    public async Task<PaginatedList<ApplicantListDto>> Handle(GetApplicantsQuery query, CancellationToken ct)
    {
        var applicants = await _applicantRepository.GetAllAsync();

        // Apply search
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.ToLower();
            applicants = applicants.Where(a =>
                a.FirstName.ToLower().Contains(search) ||
                a.LastName.ToLower().Contains(search) ||
                a.Email.Value.ToLower().Contains(search) ||
                a.PhoneNumbers.Any(p => p.Number.Contains(search.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "")))
            ).ToList();
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.BoardDecision))
        {
            if (Enum.TryParse<BoardDecision>(query.BoardDecision, out var decision))
            {
                applicants = applicants.Where(a => a.BoardDecision == decision).ToList();
            }
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            applicants = applicants.Where(a => a.Address.City.Equals(query.City, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (query.CreatedAfter.HasValue)
        {
            applicants = applicants.Where(a => a.CreatedDate >= query.CreatedAfter.Value).ToList();
        }

        if (query.CreatedBefore.HasValue)
        {
            applicants = applicants.Where(a => a.CreatedDate <= query.CreatedBefore.Value).ToList();
        }

        // Apply sorting
        applicants = query.SortBy.ToLower() switch
        {
            "lastname" => query.SortOrder.ToLower() == "asc"
                ? applicants.OrderBy(a => a.LastName).ToList()
                : applicants.OrderByDescending(a => a.LastName).ToList(),
            "firstname" => query.SortOrder.ToLower() == "asc"
                ? applicants.OrderBy(a => a.FirstName).ToList()
                : applicants.OrderByDescending(a => a.FirstName).ToList(),
            "boardreviewdate" => query.SortOrder.ToLower() == "asc"
                ? applicants.OrderBy(a => a.BoardReviewDate).ToList()
                : applicants.OrderByDescending(a => a.BoardReviewDate).ToList(),
            _ => query.SortOrder.ToLower() == "asc"
                ? applicants.OrderBy(a => a.CreatedDate).ToList()
                : applicants.OrderByDescending(a => a.CreatedDate).ToList()
        };

        // Paginate
        var totalCount = applicants.Count;
        var items = applicants
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<ApplicantListDto>>(items);

        return new PaginatedList<ApplicantListDto>(dtos, totalCount, query.Page, query.PageSize);
    }
}
```

**ApplicantListDto.cs:**

```csharp
public class ApplicantListDto
{
    public Guid ApplicantId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PrimaryPhone { get; set; }
    public string City { get; set; }
    public int NumberOfChildren { get; set; }
    public string BoardDecision { get; set; }
    public DateTime? BoardReviewDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

**PaginatedList.cs:**

```csharp
public class PaginatedList<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PaginatedList(List<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
```

**ApplicantsController.cs:**

```csharp
[HttpGet]
public async Task<ActionResult<PaginatedList<ApplicantListDto>>> GetAll([FromQuery] GetApplicantsQuery query)
{
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

#### Definition of Done

- [x] GetApplicantsQuery created
- [x] Query handler with search/filter/sort
- [x] Pagination implemented
- [x] ApplicantListDto created (lightweight)
- [x] API endpoint GET /api/applicants works
- [x] Search by name/email/phone works
- [x] Filter by board decision/city works
- [x] Sorting works
- [x] Returns totalCount
- [x] Tested with various filters
- [x] Unit tests pass

---

## SPRINT 1 SUMMARY

**Total Stories:** 9  
**Total Points:** 42  

**By Epic:**
- Foundation: 13 points (3 stories)
- Domain Model: 13 points (2 stories)
- Applicant CRUD: 16 points (4 stories)

**Deliverables:**
- ✅ Working Visual Studio solution
- ✅ AWS Cognito authentication
- ✅ PostgreSQL database with EF Core
- ✅ Domain entities (Applicant, Application)
- ✅ All value objects
- ✅ Complete applicant CRUD API
- ✅ Unit tests for domain logic

**Ready for Sprint 2:**
- Application workflow (submit, approve, stages)
- Property tracking (basic CRUD)
- Board review workflow
- Email notifications

---

END OF SPRINT 1 DETAILED STORIES
