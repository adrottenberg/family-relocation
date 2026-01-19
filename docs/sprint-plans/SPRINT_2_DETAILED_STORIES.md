# SPRINT 2 DETAILED USER STORIES
## Family Relocation System - Application Workflow & Frontend Foundation

**Sprint Duration:** 2 weeks  
**Sprint Goal:** Enable families to apply publicly, coordinators to manage applications through workflow stages, and establish frontend foundation  
**Total Points:** ~34 points (17 Backend + 17 Frontend)  
**Prerequisites:** Sprint 1 complete (Applicant CRUD, Authentication, Domain Model, 291 tests passing)

---

## 📋 SPRINT 2 OVERVIEW

### Stories in This Sprint

#### Backend Stories (17 points)

| ID | Story | Points | Epic |
|----|-------|--------|------|
| US-010 | Modify applicant creation to also create HousingSearch | 3 | Public Application |
| US-014 | View applicant pipeline (Kanban API) | 5 | Application Management |
| US-015 | Change HousingSearch stage (API endpoint) | 2 | Application Management |
| US-016 | Update housing preferences | 2 | Housing Preferences |
| US-018 | Implement audit log feature | 5 | Infrastructure |

#### Frontend Stories (17 points)

| ID | Story | Points | Epic |
|----|-------|--------|------|
| US-F01 | React project setup with design system | 3 | Frontend Foundation |
| US-F02 | Authentication flow (login page) | 5 | Frontend Foundation |
| US-F03 | App shell & navigation | 3 | Frontend Foundation |
| US-F04 | Applicant list page | 3 | Frontend Foundation |
| US-F05 | Applicant detail page | 3 | Frontend Foundation |
| US-F06 | Pipeline Kanban board | 5 | Frontend Foundation |

**Total: 34 points (17 Backend + 17 Frontend)**

### Stories Removed/Deferred from Original Plan

| ID | Story | Reason |
|----|-------|--------|
| US-011 | Application confirmation email | Deferred - Need proper editable email templates (DB-stored, coordinator-editable, variable placeholders) |
| US-012 | Create HousingSearch for applicant | Removed - Now handled automatically in US-010 |
| US-013 | View HousingSearch details | Removed - Already returned as part of Applicant response |
| US-017 | Calculate monthly payment estimate | Deferred to P3 - Nice-to-have feature |

---

## 🔧 TECHNICAL CONTEXT (From Sprint 1)

### Key Patterns Established

1. **Query Object Pattern** - MediatR queries instead of repositories
2. **All handlers in Application layer** - No split with Infrastructure
3. **EF Core ToJson()** - LINQ queries on JSON columns
4. **Generic IApplicationDbContext** - `Set<T>()` method
5. **ApplicantMapper extension methods** - `.ToDto()` syntax
6. **PaginatedList<T>** - For paginated responses
7. **MemberNotNullWhen** - For nullable result types

### Domain Model Reminder

```
Applicant (Aggregate Root)
├── HusbandInfo (Value Object - jsonb)
├── Wife: SpouseInfo (Value Object - jsonb)
├── Address (Owned Entity)
├── Children (List<Child> - jsonb)
├── BoardReview (Value Object - owned)
└── Audit fields

HousingSearch (Aggregate Root)
├── ApplicantId (FK) - 1:1 relationship
├── Stage (HousingSearchStage enum)
├── Preferences: HousingPreferences (Value Object - jsonb)
├── CurrentContract: Contract? (Value Object - jsonb)
├── FailedContracts (List<FailedContractAttempt> - jsonb)
├── MovedInStatus?
└── Audit fields
```

### Enums (Current)

```csharp
public enum HousingSearchStage
{
    Submitted,      // Just applied
    HouseHunting,   // Board approved, actively looking
    UnderContract,  // Offer accepted
    Closed,         // Closing complete
    Paused,         // Temporarily on hold
    Rejected        // Board rejected
}

public enum BoardDecision { Pending, Approved, Rejected, Deferred }
public enum MovedInStatus { MovedIn, RentedOut, Resold, Renovating, Unknown }
public enum MoveTimeline { Immediate, ShortTerm, MediumTerm, LongTerm, Extended, Flexible, NotSure, Never }
```

---

# PART 1: BACKEND STORIES

---

## US-010: Modify Applicant Creation to Also Create HousingSearch

### Story

**As a** prospective family or coordinator  
**I want** a HousingSearch to be automatically created when an applicant is created  
**So that** the applicant immediately appears in the pipeline

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2

### Background

The existing `POST /api/applicants` endpoint is already `[AllowAnonymous]` and handles applicant creation. Rather than creating a separate public application endpoint, we modify this endpoint to also create a HousingSearch record.

### Acceptance Criteria

1. When an applicant is created, a HousingSearch is automatically created
2. HousingSearch is created in "Submitted" stage
3. HousingSearch.Preferences populated from request (if provided)
4. Both Applicant and HousingSearch created in same transaction
5. Response includes HousingSearchId
6. Self-submitted applicants marked with `CreatedBy = WellKnownIds.SelfSubmittedUserId`

### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Creates both entities | Valid applicant data | POST /api/applicants | Applicant AND HousingSearch created |
| Sets correct stage | Valid submission | POST /api/applicants | HousingSearch.Stage == Submitted |
| Includes preferences | Request has preferences | POST /api/applicants | HousingSearch.Preferences populated |
| Returns both IDs | Valid submission | POST /api/applicants | Response includes ApplicantId AND HousingSearchId |
| Self-submitted tracking | Anonymous request | POST /api/applicants | CreatedBy = SelfSubmittedUserId |

### Technical Implementation

**Modify CreateApplicantCommand:**
```csharp
public record CreateApplicantCommand(CreateApplicantRequest Request) 
    : IRequest<CreateApplicantResponse>;

// Add to request DTO (if not already present)
public class CreateApplicantRequest
{
    // ... existing fields ...
    
    // Optional housing preferences for initial submission
    public HousingPreferencesRequest? HousingPreferences { get; init; }
}

// Modify response DTO
public class CreateApplicantResponse
{
    public required Guid ApplicantId { get; init; }
    public required Guid HousingSearchId { get; init; }  // NEW
}
```

**Modify CreateApplicantCommandHandler:**
```csharp
public class CreateApplicantCommandHandler 
    : IRequestHandler<CreateApplicantCommand, CreateApplicantResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public async Task<CreateApplicantResponse> Handle(
        CreateApplicantCommand request, 
        CancellationToken cancellationToken)
    {
        var req = request.Request;
        
        // Check for duplicate email (existing logic)
        var existingEmail = await _context.Set<Applicant>()
            .AnyAsync(a => a.Husband.Email == req.Husband.Email, cancellationToken);
                
        if (existingEmail)
        {
            throw new ConflictException("An applicant with this email already exists.");
        }

        // Determine CreatedBy
        var createdBy = _currentUserService.IsAuthenticated 
            ? _currentUserService.UserId 
            : WellKnownIds.SelfSubmittedUserId;

        // Create Applicant (existing logic)
        var applicant = new Applicant
        {
            Id = Guid.NewGuid(),
            Husband = req.Husband.ToDomain(),
            Wife = req.Wife?.ToDomain(),
            Address = req.Address.ToDomain(),
            Children = req.Children?.Select(c => c.ToDomain()).ToList() ?? new(),
            CurrentKehila = req.CurrentKehila,
            ShabbosShul = req.ShabbosShul,
            BoardReview = new BoardReview { Decision = BoardDecision.Pending },
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
        };

        // NEW: Create HousingSearch
        var housingSearch = new HousingSearch
        {
            Id = Guid.NewGuid(),
            ApplicantId = applicant.Id,
            Stage = HousingSearchStage.Submitted,
            Preferences = req.HousingPreferences?.ToDomain() ?? new HousingPreferences(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
        };

        _context.Set<Applicant>().Add(applicant);
        _context.Set<HousingSearch>().Add(housingSearch);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateApplicantResponse
        {
            ApplicantId = applicant.Id,
            HousingSearchId = housingSearch.Id,
        };
    }
}
```

**WellKnownIds:**
```csharp
public static class WellKnownIds
{
    public static readonly Guid SelfSubmittedUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
}
```

### Test Cases

```csharp
public class CreateApplicantWithHousingSearchTests
{
    [Fact]
    public async Task ValidApplicant_CreatesApplicantAndHousingSearch()
    
    [Fact]
    public async Task ValidApplicant_HousingSearchStageIsSubmitted()
    
    [Fact]
    public async Task WithPreferences_HousingSearchHasPreferences()
    
    [Fact]
    public async Task AnonymousRequest_SetsCreatedByToSelfSubmitted()
    
    [Fact]
    public async Task AuthenticatedRequest_SetsCreatedByToCurrentUser()
    
    [Fact]
    public async Task ResponseIncludesBothIds()
}
```

### Definition of Done

- [ ] CreateApplicantCommand modified to also create HousingSearch
- [ ] HousingSearch created with Stage = Submitted
- [ ] Response includes HousingSearchId
- [ ] Self-submitted tracked via CreatedBy
- [ ] Transaction ensures both created or neither
- [ ] Existing tests updated
- [ ] New tests for HousingSearch creation

---

## US-014: View Applicant Pipeline (Kanban API)

### Story

**As a** coordinator  
**I want to** see all applicants grouped by their housing search stage  
**So that** I can view the pipeline and manage workflow

**Priority:** P0  
**Effort:** 5 points  
**Sprint:** 2

### Acceptance Criteria

1. GET endpoint returns applicants grouped by HousingSearch stage
2. Each item includes family name, days in stage, board decision, preferences
3. Supports filtering by city, board decision
4. Supports search by family name
5. Returns counts per stage
6. Pipeline viewed through Applicants (not HousingSearches)

### Technical Implementation

**API Endpoint:**
```
GET /api/applicants/pipeline
[Authorize]
```

**Query Parameters:**
```
?search=cohen&city=Union&boardDecision=Approved
```

**Response DTO:**
```csharp
public class PipelineResponse
{
    public required List<PipelineStageDto> Stages { get; init; }
    public required int TotalCount { get; init; }
}

public class PipelineStageDto
{
    public required string Stage { get; init; }
    public required int Count { get; init; }
    public required List<PipelineItemDto> Items { get; init; }
}

public class PipelineItemDto
{
    public required Guid ApplicantId { get; init; }
    public required Guid HousingSearchId { get; init; }
    public required string FamilyName { get; init; }
    public required string HusbandFirstName { get; init; }
    public string? WifeFirstName { get; init; }
    public required int ChildrenCount { get; init; }
    public required string BoardDecision { get; init; }
    public required string Stage { get; init; }
    public required int DaysInStage { get; init; }
    public decimal? Budget { get; init; }
    public List<string>? PreferredCities { get; init; }
    public string? CurrentContractAddress { get; init; }
}
```

**Query:**
```csharp
public record GetApplicantPipelineQuery(
    string? Search = null,
    string? City = null,
    string? BoardDecision = null
) : IRequest<PipelineResponse>;
```

**Handler:**
```csharp
public class GetApplicantPipelineQueryHandler 
    : IRequestHandler<GetApplicantPipelineQuery, PipelineResponse>
{
    private readonly IApplicationDbContext _context;

    public async Task<PipelineResponse> Handle(
        GetApplicantPipelineQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .Where(a => a.HousingSearch != null)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(a => 
                a.Husband.LastName.ToLower().Contains(search) ||
                a.Husband.FirstName.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(a => 
                a.HousingSearch!.Preferences.PreferredCities.Contains(request.City));
        }

        if (!string.IsNullOrWhiteSpace(request.BoardDecision) && 
            Enum.TryParse<BoardDecision>(request.BoardDecision, out var decision))
        {
            query = query.Where(a => a.BoardReview.Decision == decision);
        }

        // Exclude rejected and paused by default
        query = query.Where(a => 
            a.HousingSearch!.Stage != HousingSearchStage.Rejected && 
            a.HousingSearch!.Stage != HousingSearchStage.Paused);

        var applicants = await query.ToListAsync(cancellationToken);

        // Group by stage
        var activeStages = new[] 
        { 
            HousingSearchStage.Submitted, 
            HousingSearchStage.HouseHunting, 
            HousingSearchStage.UnderContract, 
            HousingSearchStage.Closed 
        };

        var stages = activeStages.Select(stage => new PipelineStageDto
        {
            Stage = stage.ToString(),
            Count = applicants.Count(a => a.HousingSearch!.Stage == stage),
            Items = applicants
                .Where(a => a.HousingSearch!.Stage == stage)
                .OrderByDescending(a => a.HousingSearch!.StageChangedAt ?? a.HousingSearch!.CreatedAt)
                .Select(a => a.ToPipelineItemDto())
                .ToList()
        }).ToList();

        return new PipelineResponse
        {
            Stages = stages,
            TotalCount = applicants.Count
        };
    }
}
```

**Mapper Extension:**
```csharp
public static PipelineItemDto ToPipelineItemDto(this Applicant applicant)
{
    var hs = applicant.HousingSearch!;
    var stageDate = hs.StageChangedAt ?? hs.CreatedAt;
    
    return new PipelineItemDto
    {
        ApplicantId = applicant.Id,
        HousingSearchId = hs.Id,
        FamilyName = applicant.Husband.LastName,
        HusbandFirstName = applicant.Husband.FirstName,
        WifeFirstName = applicant.Wife?.FirstName,
        ChildrenCount = applicant.Children?.Count ?? 0,
        BoardDecision = applicant.BoardReview.Decision.ToString(),
        Stage = hs.Stage.ToString(),
        DaysInStage = (int)(DateTime.UtcNow - stageDate).TotalDays,
        Budget = hs.Preferences?.BudgetAmount,
        PreferredCities = hs.Preferences?.PreferredCities,
        CurrentContractAddress = hs.CurrentContract?.PropertyAddress?.ToString(),
    };
}
```

**Controller:**
```csharp
[HttpGet("pipeline")]
[Authorize]
public async Task<ActionResult<PipelineResponse>> GetPipeline(
    [FromQuery] string? search,
    [FromQuery] string? city,
    [FromQuery] string? boardDecision,
    CancellationToken cancellationToken)
{
    var query = new GetApplicantPipelineQuery(search, city, boardDecision);
    var result = await _mediator.Send(query, cancellationToken);
    return Ok(result);
}
```

### Definition of Done

- [ ] GET /api/applicants/pipeline endpoint created
- [ ] Returns applicants grouped by HousingSearch stage
- [ ] Search by family name works
- [ ] Filter by city works
- [ ] Filter by board decision works
- [ ] Returns counts per stage
- [ ] Calculates DaysInStage correctly
- [ ] Unit and integration tests

---

## US-015: Change HousingSearch Stage (API Endpoint)

### Story

**As a** coordinator  
**I want to** change an applicant's housing search stage via API  
**So that** I can track their progress through the pipeline

**Priority:** P0  
**Effort:** 2 points  
**Sprint:** 2

### Background

Domain stage transition logic already exists in the HousingSearch entity. This story focuses on creating the API endpoint to expose that functionality.

### Acceptance Criteria

1. PUT endpoint changes stage for an applicant's housing search
2. Uses existing domain transition validation logic
3. Returns 400 for invalid transitions
4. Updates StageChangedAt timestamp
5. Records who made the change (ModifiedBy)

### Technical Implementation

**API Endpoint:**
```
PUT /api/applicants/{id}/housing-search/stage
[Authorize(Roles = "Admin,Coordinator")]
```

**Request:**
```csharp
public class ChangeStageRequest
{
    public required string NewStage { get; init; }
    public string? Notes { get; init; }
}
```

**Command:**
```csharp
public record ChangeApplicantStageCommand(
    Guid ApplicantId, 
    string NewStage,
    string? Notes
) : IRequest<HousingSearchDto>;
```

**Handler:**
```csharp
public class ChangeApplicantStageCommandHandler 
    : IRequestHandler<ChangeApplicantStageCommand, HousingSearchDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public async Task<HousingSearchDto> Handle(
        ChangeApplicantStageCommand request, 
        CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", request.ApplicantId);

        if (applicant.HousingSearch == null)
        {
            throw new NotFoundException("HousingSearch for Applicant", request.ApplicantId);
        }

        if (!Enum.TryParse<HousingSearchStage>(request.NewStage, out var newStage))
        {
            throw new ValidationException($"Invalid stage: {request.NewStage}");
        }

        // Use domain method for transition (includes validation)
        applicant.HousingSearch.TransitionTo(newStage);
        
        applicant.HousingSearch.ModifiedAt = DateTime.UtcNow;
        applicant.HousingSearch.ModifiedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return applicant.HousingSearch.ToDto();
    }
}
```

**Controller:**
```csharp
[HttpPut("{id}/housing-search/stage")]
[Authorize(Roles = "Admin,Coordinator")]
public async Task<ActionResult<HousingSearchDto>> ChangeStage(
    Guid id,
    [FromBody] ChangeStageRequest request,
    CancellationToken cancellationToken)
{
    var command = new ChangeApplicantStageCommand(id, request.NewStage, request.Notes);
    var result = await _mediator.Send(command, cancellationToken);
    return Ok(result);
}
```

### Definition of Done

- [ ] PUT /api/applicants/{id}/housing-search/stage endpoint created
- [ ] Uses existing domain transition logic
- [ ] Returns 400 for invalid transitions
- [ ] Updates StageChangedAt and ModifiedBy
- [ ] Returns updated HousingSearchDto
- [ ] Integration tests for valid and invalid transitions

---

## US-016: Update Housing Preferences

### Story

**As a** coordinator  
**I want to** update a family's housing preferences  
**So that** I can keep their search criteria current

**Priority:** P1  
**Effort:** 2 points  
**Sprint:** 2

### Technical Implementation

**API Endpoint:**
```
PUT /api/applicants/{id}/housing-search/preferences
[Authorize]
```

**Request:**
```csharp
public class UpdatePreferencesRequest
{
    public decimal? BudgetAmount { get; init; }
    public int? MinBedrooms { get; init; }
    public decimal? MinBathrooms { get; init; }
    public List<string>? PreferredCities { get; init; }
    public List<string>? RequiredFeatures { get; init; }
    public string? MoveTimeline { get; init; }
    public ShulProximityRequest? ShulProximity { get; init; }
}
```

**Command:**
```csharp
public record UpdateHousingPreferencesCommand(
    Guid ApplicantId,
    UpdatePreferencesRequest Preferences
) : IRequest<HousingSearchDto>;
```

**Handler:**
```csharp
public class UpdateHousingPreferencesCommandHandler 
    : IRequestHandler<UpdateHousingPreferencesCommand, HousingSearchDto>
{
    public async Task<HousingSearchDto> Handle(
        UpdateHousingPreferencesCommand request, 
        CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", request.ApplicantId);

        if (applicant.HousingSearch == null)
        {
            throw new NotFoundException("HousingSearch for Applicant", request.ApplicantId);
        }

        applicant.HousingSearch.Preferences = request.Preferences.ToDomain();
        applicant.HousingSearch.ModifiedAt = DateTime.UtcNow;
        applicant.HousingSearch.ModifiedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return applicant.HousingSearch.ToDto();
    }
}
```

### Definition of Done

- [ ] PUT /api/applicants/{id}/housing-search/preferences endpoint created
- [ ] Validates preferences
- [ ] Returns updated HousingSearchDto
- [ ] Unit and integration tests

---

## US-018: Implement Audit Log Feature

### Story

**As an** administrator  
**I want** all changes to be automatically logged  
**So that** I can track who changed what and when

**Priority:** P1  
**Effort:** 5 points  
**Sprint:** 2

### Acceptance Criteria

1. AuditLog entity captures all changes to tracked entities
2. Automatic capture via EF Core SaveChanges interceptor
3. Records: EntityType, EntityId, Action, OldValues, NewValues, UserId, Timestamp
4. GET endpoint to query audit logs with filters
5. Audit history viewable on Applicant detail page (frontend)

### Technical Implementation

**AuditLog Entity:**
```csharp
public class AuditLogEntry
{
    public Guid Id { get; set; }
    public required string EntityType { get; set; }
    public required Guid EntityId { get; set; }
    public required string Action { get; set; }  // Created, Updated, Deleted
    public string? OldValues { get; set; }  // JSON
    public string? NewValues { get; set; }  // JSON
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}
```

**EF Core Configuration:**
```csharp
public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Action).HasMaxLength(20).IsRequired();
        builder.Property(e => e.OldValues).HasColumnType("jsonb");
        builder.Property(e => e.NewValues).HasColumnType("jsonb");
        builder.Property(e => e.UserName).HasMaxLength(100);
        builder.Property(e => e.IpAddress).HasMaxLength(50);
        
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => e.EntityId);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.UserId);
    }
}
```

**SaveChanges Interceptor:**
```csharp
public class AuditingInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var auditEntries = new List<AuditLogEntry>();
        var userId = _currentUserService.UserId;
        var userName = _currentUserService.UserName;
        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLogEntry) continue;  // Don't audit the audit log
            if (!ShouldAudit(entry.Entity)) continue;

            var auditEntry = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                EntityType = entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry),
                Action = entry.State.ToString(),
                UserId = userId,
                UserName = userName,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.NewValues = SerializeEntity(entry);
                    break;
                case EntityState.Modified:
                    auditEntry.OldValues = SerializeOriginalValues(entry);
                    auditEntry.NewValues = SerializeCurrentValues(entry);
                    break;
                case EntityState.Deleted:
                    auditEntry.OldValues = SerializeEntity(entry);
                    break;
            }

            auditEntries.Add(auditEntry);
        }

        if (auditEntries.Any())
        {
            context.Set<AuditLogEntry>().AddRange(auditEntries);
        }

        return result;
    }

    private bool ShouldAudit(object entity)
    {
        // Audit Applicant, HousingSearch, and other important entities
        return entity is Applicant or HousingSearch;
    }

    private Guid GetEntityId(EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        return idProperty?.CurrentValue is Guid id ? id : Guid.Empty;
    }

    private string SerializeEntity(EntityEntry entry)
    {
        var dict = entry.Properties
            .Where(p => !p.Metadata.IsShadowProperty())
            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
        return JsonSerializer.Serialize(dict);
    }

    private string SerializeOriginalValues(EntityEntry entry)
    {
        var dict = entry.Properties
            .Where(p => p.IsModified)
            .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
        return JsonSerializer.Serialize(dict);
    }

    private string SerializeCurrentValues(EntityEntry entry)
    {
        var dict = entry.Properties
            .Where(p => p.IsModified)
            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
        return JsonSerializer.Serialize(dict);
    }
}
```

**Query Endpoint:**

```
GET /api/audit-logs
[Authorize(Roles = "Admin")]
```

**Query Parameters:**
```
?entityType=Applicant&entityId={guid}&userId={guid}&from=2026-01-01&to=2026-01-31&page=1&pageSize=50
```

**Query:**
```csharp
public record GetAuditLogsQuery(
    string? EntityType = null,
    Guid? EntityId = null,
    Guid? UserId = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<PaginatedList<AuditLogDto>>;
```

**Response DTO:**
```csharp
public class AuditLogDto
{
    public Guid Id { get; init; }
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public required string Action { get; init; }
    public Dictionary<string, object?>? OldValues { get; init; }
    public Dictionary<string, object?>? NewValues { get; init; }
    public Guid? UserId { get; init; }
    public string? UserName { get; init; }
    public DateTime Timestamp { get; init; }
}
```

**Handler:**
```csharp
public class GetAuditLogsQueryHandler 
    : IRequestHandler<GetAuditLogsQuery, PaginatedList<AuditLogDto>>
{
    public async Task<PaginatedList<AuditLogDto>> Handle(
        GetAuditLogsQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.Set<AuditLogEntry>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (request.EntityId.HasValue)
            query = query.Where(a => a.EntityId == request.EntityId.Value);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (request.From.HasValue)
            query = query.Where(a => a.Timestamp >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.Timestamp <= request.To.Value);

        query = query.OrderByDescending(a => a.Timestamp);

        return await PaginatedList<AuditLogDto>.CreateAsync(
            query.Select(a => a.ToDto()),
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
```

### Definition of Done

- [ ] AuditLogEntry entity and table created
- [ ] EF Core interceptor captures changes automatically
- [ ] Applicant and HousingSearch changes are audited
- [ ] GET /api/audit-logs endpoint with filters
- [ ] Pagination works correctly
- [ ] Unit tests for interceptor
- [ ] Integration tests for endpoint

---

# PART 2: FRONTEND STORIES

---

## 🎨 DESIGN SYSTEM REFERENCE

All frontend stories should follow the design system documented in:
- **Design System**: `docs/design/crm-design-system-v4.html`
- **Theme Config**: `src/FamilyRelocation.Web/src/theme/antd-theme.ts`
- **Prototypes**: `docs/design/prototype-*.html`

### Key Design Tokens

```typescript
colors: {
  primary: {
    700: '#1e40af',  // Button text, icons
    150: '#d0e4fc',  // Button background
    100: '#dbeafe',  // Button hover
    50: '#eff6ff',   // Sidebar active
  },
  brand: {
    600: '#2d7a3a',  // Logo text color
    500: '#3d9a4a',  // Logo, success
  },
  neutral: {
    900: '#1a1d1a',  // Primary text
    600: '#5c605c',  // Secondary text
    500: '#7a7e7a',  // Tertiary text
    300: '#c4c7c4',  // Borders
    100: '#f1f2f1',  // Backgrounds
    50: '#f8f9f8',   // Page background
  }
}

// Button Style (Option B - Light)
.btn-primary {
  background: #d0e4fc;
  color: #1e40af;
  border: 1px solid #bfdbfe;
}

// Typography
font-family: 'Assistant', 'Heebo', sans-serif;
font-size-base: 14px;
```

---

## US-F01: React Project Setup with Design System

### Story

**As a** developer  
**I want to** set up the React project with proper tooling and design system  
**So that** I have a solid foundation matching our design specifications

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2

### Acceptance Criteria

1. Vite project created with React + TypeScript template
2. Ant Design installed and configured with custom theme
3. React Router configured with basic routes
4. TanStack Query configured for API calls
5. Zustand configured for client state
6. Axios configured with base URL and interceptors
7. Folder structure established
8. Google Fonts (Assistant, Heebo) configured
9. CSS variables from design system injected
10. Proxy to backend API working in development

### Technical Implementation

**Create Project:**
```bash
npm create vite@latest family-relocation-web -- --template react-ts
cd family-relocation-web

# Core dependencies
npm install antd @ant-design/icons
npm install react-router-dom
npm install @tanstack/react-query @tanstack/react-query-devtools
npm install zustand
npm install axios
npm install dayjs

# Dev dependencies
npm install -D @types/node
```

**Folder Structure:**
```
src/
├── api/
│   ├── client.ts           # Axios instance with interceptors
│   ├── endpoints/
│   │   ├── applicants.ts
│   │   ├── housingSearches.ts
│   │   └── auth.ts
│   └── types/
├── components/
│   ├── common/
│   │   ├── StatusTag.tsx    # Board decision tags
│   │   ├── StageTag.tsx     # Pipeline stage tags
│   │   ├── LoadingSpinner.tsx
│   │   └── PageHeader.tsx
│   └── layout/
│       ├── AppLayout.tsx
│       ├── Sidebar.tsx
│       └── Header.tsx
├── features/
│   ├── auth/
│   ├── applicants/
│   └── pipeline/
├── hooks/
├── store/
│   ├── authStore.ts
│   └── uiStore.ts
├── theme/
│   └── antd-theme.ts        # Copy from docs/design/
├── utils/
├── App.tsx
├── main.tsx
└── routes.tsx
```

### Definition of Done

- [ ] Vite project created with TypeScript
- [ ] All dependencies installed
- [ ] Ant Design configured with `antd-theme.ts`
- [ ] Google Fonts loading (Assistant, Heebo)
- [ ] CSS variables injected
- [ ] Folder structure created
- [ ] Axios client with interceptors
- [ ] Proxy to backend working
- [ ] `npm run dev` starts without errors
- [ ] `npm run build` succeeds
- [ ] Primary button renders with light blue style

---

## US-F02: Authentication Flow (Login Page)

### Story

**As a** coordinator  
**I want to** log in to the system  
**So that** I can access the CRM features

**Priority:** P0  
**Effort:** 5 points  
**Sprint:** 2

### Design Reference

**Prototype:** `docs/design/prototype-login-page.html`

### Layout Specifications

| Element | Specification |
|---------|---------------|
| Container | Centered, max-width 420px |
| Background | Gradient: `brand-50` to `primary-50` |
| Card | White, border-radius 16px, shadow-lg, padding 40px |
| Logo | Tree image (64px height) + "וועד הישוב" text |

### States to Implement

1. **Default** - Form ready for input
2. **Loading** - Button shows spinner, inputs disabled
3. **Error** - Red alert shown above form
4. **Success** - Redirect to `/dashboard`

### Definition of Done

- [ ] Login page matches prototype design
- [ ] Logo with Hebrew text displays correctly
- [ ] Form validation works (email format, required fields)
- [ ] Loading state shows spinner in button
- [ ] Error state shows alert with message
- [ ] Successful login stores tokens and redirects
- [ ] Protected routes redirect to login if not authenticated

---

## US-F03: App Shell & Navigation

### Story

**As a** coordinator  
**I want to** navigate between different sections of the CRM  
**So that** I can access all features easily

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2

### Design Reference

**Prototype:** `docs/design/prototype-pipeline-kanban.html` (sidebar section)

### Sidebar Specifications

| Element | Specification |
|---------|---------------|
| Width | 220px fixed |
| Background | White |
| Border | 1px solid `neutral-200` on right |
| Logo area | Padding 16px, border-bottom |
| Nav items | Padding 12px 14px, border-radius 8px |
| **Active item** | Background `primary-50` (#eff6ff), color `primary-700` (#1e40af) |
| Hover item | Background `neutral-100`, color `neutral-900` |
| User section | Bottom, border-top, padding 16px |

### Definition of Done

- [ ] Sidebar renders with logo and Hebrew text
- [ ] Navigation items highlight when active (light blue)
- [ ] User info displays at bottom of sidebar
- [ ] Header shows page title
- [ ] Routes work: /, /applicants, /pipeline
- [ ] Layout is fixed (sidebar doesn't scroll with content)

---

## US-F04: Applicant List Page

### Story

**As a** coordinator  
**I want to** see a list of all applicants  
**So that** I can manage family applications

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2

### Table Specifications

| Element | Specification |
|---------|---------------|
| Header bg | `neutral-50` |
| Header text | 12px, uppercase, `neutral-500` |
| Row hover | `primary-50` background |
| Row padding | 16px |

### Definition of Done

- [ ] Page header with title, count, and "Add Applicant" button
- [ ] Filters row with search, status, and stage dropdowns
- [ ] Table displays applicant data with custom tags
- [ ] Row hover shows light blue background
- [ ] Clicking row navigates to detail page
- [ ] Loading state shows spinner
- [ ] Empty state shows message

---

## US-F05: Applicant Detail Page

### Story

**As a** coordinator  
**I want to** view detailed information about an applicant  
**So that** I can review their application and housing search

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2

### Tabs

1. **Overview** - Husband, Wife, Address, Preferences
2. **Housing Search** - Stage, preferences, contracts
3. **Children** - List with ages and genders
4. **Activity** - Audit log history (calls US-018 API)

### Definition of Done

- [ ] Back link navigates to applicant list
- [ ] Header shows family name, status tags, and action buttons
- [ ] Tabs switch between Overview, Housing Search, Children, Activity
- [ ] Info sections display in 2-column grid
- [ ] Activity tab shows audit history
- [ ] Data loads from API with loading state
- [ ] 404 handling if applicant not found

---

## US-F06: Pipeline Kanban Board

### Story

**As a** coordinator  
**I want to** view families in a Kanban board by housing search stage  
**So that** I can visualize and manage the pipeline

**Priority:** P0  
**Effort:** 5 points  
**Sprint:** 2

### Design Reference

**Prototype:** `docs/design/prototype-pipeline-kanban.html`

### Stage Colors

| Stage | Dot/Border Color | Background |
|-------|------------------|------------|
| Submitted | `#3b82f6` | `#dbeafe` |
| House Hunting | `#f59e0b` | `#fef3c7` |
| Under Contract | `#8b5cf6` | `#ede9fe` |
| Closed | `#10b981` | `#d1fae5` |

### Card Specifications

| Element | Specification |
|---------|---------------|
| Background | White |
| Border-left | 4px solid (stage color) |
| Border-radius | 10px |
| Padding | 16px |
| Shadow | `shadow-sm` |
| Hover | Lift effect (`shadow-md`, translateY -2px) |

### Definition of Done

- [ ] Four columns render with correct colors
- [ ] Cards display family info with stage-colored border
- [ ] Drag and drop changes stage (calls PUT /api/applicants/{id}/housing-search/stage)
- [ ] Click on card opens detail modal or navigates
- [ ] Search filters cards across all columns
- [ ] City/Status filters work
- [ ] Loading state shows skeletons
- [ ] Error handling for failed stage changes

---

## 📋 SPRINT 2 COMPLETE SUMMARY

### All Stories

| ID | Story | Points | Type |
|----|-------|--------|------|
| US-010 | Modify applicant creation to also create HousingSearch | 3 | Backend |
| US-014 | View applicant pipeline (Kanban API) | 5 | Backend |
| US-015 | Change HousingSearch stage (API endpoint) | 2 | Backend |
| US-016 | Update housing preferences | 2 | Backend |
| US-018 | Implement audit log feature | 5 | Backend |
| US-F01 | React project setup with design system | 3 | Frontend |
| US-F02 | Authentication flow (login page) | 5 | Frontend |
| US-F03 | App shell & navigation | 3 | Frontend |
| US-F04 | Applicant list page | 3 | Frontend |
| US-F05 | Applicant detail page | 3 | Frontend |
| US-F06 | Pipeline Kanban board | 5 | Frontend |

**Total: 34 points (17 Backend + 17 Frontend)**

### Stories Deferred

| ID | Story | Deferred To | Reason |
|----|-------|-------------|--------|
| US-011 | Email notifications | Sprint 4+ | Need editable templates with DB storage |
| US-017 | Monthly payment calculator | P3 | Nice-to-have feature |

---

### 🎯 DAILY BREAKDOWN

**Week 1: Backend Focus**

| Day | Tasks |
|-----|-------|
| Day 1 | US-010: Modify CreateApplicant to create HousingSearch |
| Day 2 | US-014: Pipeline query endpoint |
| Day 3 | US-015: Stage change endpoint + US-016: Preferences |
| Day 4-5 | US-018: Audit log feature |

**Week 2: Frontend Focus**

| Day | Tasks |
|-----|-------|
| Day 6 | US-F01: React project setup with theme |
| Day 7 | US-F02: Login page + Auth store |
| Day 8 | US-F03: App shell + navigation |
| Day 9 | US-F04: Applicant list page |
| Day 10 | US-F05: Applicant detail + US-F06: Pipeline Kanban |

---

### What You'll Have at End of Sprint 2

**Backend:**
- ✅ Applicant creation automatically creates HousingSearch
- ✅ Pipeline (Kanban) data API via /api/applicants/pipeline
- ✅ Stage transition API endpoint
- ✅ Housing preferences update
- ✅ Audit log with automatic change tracking

**Frontend:**
- ✅ Working React application with design system
- ✅ Login page with Cognito auth
- ✅ App shell with navigation
- ✅ Applicant list with search/filter
- ✅ Applicant detail page with audit history
- ✅ Pipeline Kanban board with drag & drop

---

**Sprint 2 Detailed Stories Complete! 🚀**
