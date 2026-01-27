# CODE REFERENCE GUIDE
## Where to Find Each Code Sample

**Problem:** SOLUTION_STRUCTURE_AND_CODE_v3.md has an incomplete Table of Contents.

**Solution:** This guide shows you exactly where to find each code sample you need.

---

## ✅ WHAT'S IN SOLUTION_STRUCTURE_AND_CODE_v3.md

### Section 1: Solution Overview ✅
- Architecture diagram
- Project dependencies
- Clean Architecture explanation

### Section 2: Complete Folder Structure ✅
- Every directory documented
- File locations
- Folder purposes

### Section 3: Domain Layer - Complete Code ✅
- Base Entity<TId> class
- Base ValueObject class
- IDomainEvent interface
- IUnitOfWork interface

### Section 4: Enums - Updated ✅
- All 14 enums with corrections
- InterestLevel (with SomewhatInterested)
- MoveTimeline (with Never)
- ListingStatus (without UnderContractThroughUs)
- HouseType (NEW)
- ActivityType (expanded)
- EmailDeliveryStatus (NEW)

### Section 5: Value Objects - Complete Code ✅
- Address (full implementation)
- PhoneNumber (full implementation)
- Email (full implementation)
- Money (full implementation)
- Child (full implementation)
- Coordinates (full implementation with Haversine)
- ShulProximityPreference (full implementation)

### Section 6: Entities - Complete Code ✅
- Applicant (with corrections: ShabbosShul, PreferredCities)
- Property (with corrections: HouseType, City, OpenHouses)
- OpenHouse (NEW entity)
- Shul (with seed data)
- EmailContact (NEW)
- EmailBlast (NEW)
- EmailBlastRecipient (NEW)

### Section 7: Summary of Corrections ✅
- Table showing all 32 corrections applied

---

## ❌ WHAT'S MISSING (Promised in Table of Contents)

### Section 8: Application Layer - CQRS ❌
**Not in v3 document**

### Section 9: Infrastructure Layer ❌
**Not in v3 document**

### Section 10: API Layer ❌
**Not in v3 document**

### Section 11: React Frontend Structure ❌
**Not in v3 document**

### Section 12: NuGet Packages ❌
**Not in v3 document**

### Section 13: Configuration Files ❌
**Not in v3 document**

---

## ✅ WHERE TO FIND THE MISSING SECTIONS

### Application Layer (Commands, Queries, Handlers, DTOs)

**Use:** SPRINT_1_DETAILED_STORIES.md

**Location by Story:**

**US-006: Create Applicant**
- CreateApplicantCommand (full code)
- CreateApplicantCommandValidator (FluentValidation)
- CreateApplicantCommandHandler (full implementation)
- ApplicantDto

**US-007: View Applicant Details**
- GetApplicantQuery
- GetApplicantQueryHandler
- MappingProfile (AutoMapper)

**US-008: Update Applicant**
- UpdateApplicantCommand
- UpdateApplicantCommandHandler

**US-009: List Applicants**
- GetApplicantsQuery (with filters, pagination)
- GetApplicantsQueryHandler
- ApplicantListDto
- PaginatedList<T>

---

### Infrastructure Layer (DbContext, Repositories, AWS Services)

**US-003: PostgreSQL + EF Core**
```csharp
// ApplicationDbContext
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Applicant> Applicants { get; set; }
    public DbSet<Application> Applications { get; set; }
    // ... etc
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

// UnitOfWork
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
```

**Entity Configurations (Example):**
```csharp
// ApplicantConfiguration.cs
public class ApplicantConfiguration : IEntityTypeConfiguration<Applicant>
{
    public void Configure(EntityTypeBuilder<Applicant> builder)
    {
        builder.ToTable("Applicants");
        builder.HasKey(a => a.ApplicantId);
        
        builder.Property(a => a.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(a => a.LastName).IsRequired().HasMaxLength(50);
        
        // Value objects
        builder.OwnsOne(a => a.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").IsRequired();
        });
        
        builder.OwnsOne(a => a.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street");
            address.Property(a => a.City).HasColumnName("City");
            address.Property(a => a.State).HasColumnName("State");
            address.Property(a => a.ZipCode).HasColumnName("ZipCode");
            address.Property(a => a.Unit).HasColumnName("Unit");
        });
        
        // Collections stored as JSON
        builder.Property(a => a.PhoneNumbers)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<PhoneNumber>>(v, (JsonSerializerOptions)null))
            .HasColumnType("jsonb");
        
        builder.Property(a => a.Children)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<Child>>(v, (JsonSerializerOptions)null))
            .HasColumnType("jsonb");
        
        builder.Property(a => a.PreferredCities)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null))
            .HasColumnType("jsonb");
    }
}
```

**Repository Pattern:**
```csharp
// IApplicantRepository (in Domain layer)
public interface IApplicantRepository
{
    Task<Applicant> GetByIdAsync(Guid id);
    Task<Applicant> GetByEmailAsync(string email);
    Task<List<Applicant>> GetAllAsync();
    Task AddAsync(Applicant applicant);
    Task UpdateAsync(Applicant applicant);
    Task DeleteAsync(Guid id);
}

// ApplicantRepository (in Infrastructure layer)
public class ApplicantRepository : IApplicantRepository
{
    private readonly ApplicationDbContext _context;
    
    public ApplicantRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Applicant> GetByIdAsync(Guid id)
    {
        return await _context.Applicants
            .Include(a => a.Applications)
            .FirstOrDefaultAsync(a => a.ApplicantId == id);
    }
    
    public async Task<Applicant> GetByEmailAsync(string email)
    {
        return await _context.Applicants
            .FirstOrDefaultAsync(a => a.Email.Value == email.ToLowerInvariant());
    }
    
    public async Task<List<Applicant>> GetAllAsync()
    {
        return await _context.Applicants
            .Where(a => !a.IsDeleted)
            .ToListAsync();
    }
    
    public async Task AddAsync(Applicant applicant)
    {
        await _context.Applicants.AddAsync(applicant);
    }
    
    public async Task UpdateAsync(Applicant applicant)
    {
        _context.Applicants.Update(applicant);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var applicant = await GetByIdAsync(id);
        if (applicant != null)
        {
            applicant.Delete(Guid.Empty); // Set current user ID here
        }
    }
}
```

---

### API Layer (Controllers)

**Use:** SPRINT_1_DETAILED_STORIES.md

**US-006, 007, 008, 009 contain:**
- ApplicantsController (full code)
- AuthController (US-002)
- HealthController (US-003)

**Example:**
```csharp
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
        var query = new GetApplicantQuery { ApplicantId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApplicantDto>> Update(Guid id, [FromBody] UpdateApplicantCommand command)
    {
        if (id != command.ApplicantId)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<ApplicantListDto>>> GetAll([FromQuery] GetApplicantsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

---

### NuGet Packages

**Use:** SPRINT_1_DETAILED_STORIES.md - US-001 & US-003

**Domain Layer:**
```
None! Domain has ZERO dependencies.
```

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
dotnet add package AWSSDK.S3
dotnet add package AWSSDK.SimpleEmail
```

**API Layer:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

### Configuration Files

**Program.cs (API Layer):**

**Use:** SPRINT_1_DETAILED_STORIES.md - US-002, US-003

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    )
);

// Add MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateApplicantCommand).Assembly));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateApplicantCommand).Assembly);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add JWT Authentication
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

// Register repositories
builder.Services.AddScoped<IApplicantRepository, ApplicantRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=FamilyRelocation;Username=postgres;Password=your_password"
  },
  "AWS": {
    "Region": "us-east-1",
    "Cognito": {
      "UserPoolId": "us-east-1_XXXXXXXXX",
      "ClientId": "XXXXXXXXXXXXXXXXXXXXXXXXXX",
      "Authority": "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_XXXXXXXXX"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

### React Frontend Structure

**Status:** Coming in Sprint 2

**Will include:**
- Component structure
- React Router setup
- Ant Design integration
- API client setup
- State management (Zustand)

**For now:** Focus on backend (Sprint 1)

---

## 🎯 RECOMMENDED WORKFLOW

### For Sprint 1 Development:

1. **Domain Entities & Value Objects:**
   - Use: SOLUTION_STRUCTURE_AND_CODE_v3.md Sections 5-6
   - Copy complete implementations

2. **Commands, Queries, Handlers:**
   - Use: SPRINT_1_DETAILED_STORIES.md
   - Each story (US-006 through US-009) has complete code

3. **Infrastructure (DbContext, Repositories):**
   - Use: This guide (sections above)
   - Plus: SPRINT_1_DETAILED_STORIES.md US-003

4. **Controllers:**
   - Use: SPRINT_1_DETAILED_STORIES.md US-006 through US-009

5. **Configuration:**
   - Use: This guide (Program.cs, appsettings.json)
   - Plus: SPRINT_1_DETAILED_STORIES.md US-002, US-003

---

## 📚 DOCUMENT PRIORITY

**For Sprint 1 Development, use in this order:**

1. ✅ **SPRINT_1_DETAILED_STORIES.md** (MOST IMPORTANT - has all working code)
2. ✅ **SOLUTION_STRUCTURE_AND_CODE_v3.md** (Domain layer - Sections 1-7)
3. ✅ **CODE_REFERENCE_GUIDE.md** (This file - fills in the gaps)
4. ✅ **CLAUDE_CODE_CONTEXT.md** (Quick reference, patterns, decisions)

**Don't waste time looking for missing sections in v3 - everything you need is in Sprint 1 stories!**

---

## ✅ SUMMARY

**SOLUTION_STRUCTURE_AND_CODE_v3.md is complete for:**
- ✅ Domain layer (Entities, Value Objects, Enums) - ALL CORRECTED
- ✅ Architecture overview
- ✅ Folder structure
- ✅ All 32 corrections applied

**For everything else, use:**
- ✅ SPRINT_1_DETAILED_STORIES.md (Commands, Queries, Controllers)
- ✅ This guide (Infrastructure, Configuration)

**You have EVERYTHING you need to complete Sprint 1! 🚀**
