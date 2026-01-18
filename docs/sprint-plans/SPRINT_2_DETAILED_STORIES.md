# SPRINT 2 DETAILED USER STORIES
## Family Relocation System - Application Workflow & Public Form

**Sprint Duration:** 2 weeks  
**Sprint Goal:** Enable families to apply publicly and coordinators to manage applications through workflow stages  
**Total Points:** ~38 points (8 stories)  
**Prerequisites:** Sprint 1 complete (Applicant CRUD, Authentication, Domain Model)

---

## 📋 SPRINT 2 OVERVIEW

### Stories in This Sprint

| ID | Story | Points | Epic |
|----|-------|--------|------|
| US-010 | Public application form submission | 8 | Public Application |
| US-011 | Application confirmation email | 3 | Public Application |
| US-012 | Create HousingSearch for applicant | 3 | Application Management |
| US-013 | View HousingSearch details | 3 | Application Management |
| US-014 | View HousingSearch pipeline (Kanban) | 8 | Application Management |
| US-015 | Change HousingSearch stage | 5 | Application Management |
| US-016 | Update housing preferences | 5 | Housing Preferences |
| US-017 | Calculate monthly payment estimate | 3 | Housing Preferences |

**Total: 38 points**

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
├── ApplicantId (FK)
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

## US-010: Public Application Form Submission

### Story

**As a** prospective family  
**I want to** submit an application without creating an account  
**So that** I can apply to relocate to the community easily

**Priority:** P0  
**Effort:** 8 points  
**Sprint:** 2

### Acceptance Criteria

1. Public endpoint accepts application without authentication
2. Creates Applicant with all required information
3. Creates HousingSearch in "Submitted" stage
4. Validates all required fields (husband name, email, at least one phone)
5. Checks for duplicate email (husband or wife)
6. Returns confirmation with application ID
7. Marks applicant as self-submitted (CreatedBy = WellKnownIds.SelfSubmittedUserId)

### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Valid submission | Valid application data | POST /api/applications/public | 201 Created with ApplicantId and HousingSearchId |
| Duplicate email | Email already exists | POST /api/applications/public | 409 Conflict with error message |
| Missing required | No husband first name | POST /api/applications/public | 400 Bad Request with validation errors |
| Missing phone | No phone numbers provided | POST /api/applications/public | 400 Bad Request "At least one phone required" |
| Creates both entities | Valid submission | Check database | Applicant AND HousingSearch created |

### Technical Implementation

**API Endpoint:**
```
POST /api/applications/public
[AllowAnonymous]
Content-Type: application/json
```

**Request DTO:**
```csharp
public class PublicApplicationRequest
{
    // Husband (Required)
    public required HusbandInfoRequest Husband { get; init; }
    
    // Wife (Optional but common)
    public SpouseInfoRequest? Wife { get; init; }
    
    // Address (Required)
    public required AddressRequest Address { get; init; }
    
    // Children (Optional)
    public List<ChildRequest>? Children { get; init; }
    
    // Community (Optional on initial form)
    public string? CurrentKehila { get; init; }
    public string? ShabbosShul { get; init; }
    
    // Initial Housing Preferences (Optional)
    public HousingPreferencesRequest? HousingPreferences { get; init; }
}

public class HusbandInfoRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? FatherName { get; init; }
    public required string Email { get; init; }
    public required List<PhoneNumberRequest> PhoneNumbers { get; init; }
    public string? Occupation { get; init; }
    public string? EmployerName { get; init; }
}

public class SpouseInfoRequest
{
    public required string FirstName { get; init; }
    public string? MaidenName { get; init; }
    public string? FatherName { get; init; }
    public string? Email { get; init; }
    public List<PhoneNumberRequest>? PhoneNumbers { get; init; }
    public string? HighSchool { get; init; }
    public string? Occupation { get; init; }
    public string? EmployerName { get; init; }
}

public class HousingPreferencesRequest
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
public record SubmitPublicApplicationCommand(PublicApplicationRequest Application) 
    : IRequest<PublicApplicationResult>;

public class PublicApplicationResult
{
    public required Guid ApplicantId { get; init; }
    public required Guid HousingSearchId { get; init; }
    public required string ConfirmationMessage { get; init; }
}
```

**Handler:**
```csharp
public class SubmitPublicApplicationCommandHandler 
    : IRequestHandler<SubmitPublicApplicationCommand, PublicApplicationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public async Task<PublicApplicationResult> Handle(
        SubmitPublicApplicationCommand request, 
        CancellationToken ct)
    {
        var app = request.Application;
        
        // Check for duplicate email (husband)
        var husbandEmailExists = await _mediator.Send(
            new ExistsByEmailQuery(app.Husband.Email), ct);
        if (husbandEmailExists)
            throw new DuplicateEmailException(app.Husband.Email);
        
        // Check for duplicate email (wife, if provided)
        if (!string.IsNullOrWhiteSpace(app.Wife?.Email))
        {
            var wifeEmailExists = await _mediator.Send(
                new ExistsByEmailQuery(app.Wife.Email), ct);
            if (wifeEmailExists)
                throw new DuplicateEmailException(app.Wife.Email);
        }
        
        // Create Applicant
        var applicant = Applicant.Create(
            husband: app.Husband.ToDomain(),
            wife: app.Wife?.ToDomain(),
            address: app.Address.ToDomain(),
            children: app.Children?.Select(c => c.ToDomain()).ToList() ?? new(),
            currentKehila: app.CurrentKehila,
            shabbosShul: app.ShabbosShul,
            createdBy: WellKnownIds.SelfSubmittedUserId
        );
        
        _context.Add(applicant);
        
        // Create HousingSearch with optional preferences
        var preferences = app.HousingPreferences?.ToDomain() 
            ?? HousingPreferences.Empty();
        
        var housingSearch = HousingSearch.Create(
            applicantId: applicant.Id,
            preferences: preferences,
            createdBy: WellKnownIds.SelfSubmittedUserId
        );
        
        _context.Add(housingSearch);
        
        await _context.SaveChangesAsync(ct);
        
        // Raise domain event for email notification
        applicant.AddDomainEvent(new ApplicationSubmittedEvent(
            applicant.Id, 
            housingSearch.Id,
            applicant.Husband.Email.Value));
        
        return new PublicApplicationResult
        {
            ApplicantId = applicant.Id,
            HousingSearchId = housingSearch.Id,
            ConfirmationMessage = "Application received! You will receive a confirmation email shortly."
        };
    }
}
```

**Validator:**
```csharp
public class SubmitPublicApplicationCommandValidator 
    : AbstractValidator<SubmitPublicApplicationCommand>
{
    public SubmitPublicApplicationCommandValidator()
    {
        RuleFor(x => x.Application.Husband)
            .NotNull().WithMessage("Husband information is required");
        
        RuleFor(x => x.Application.Husband.FirstName)
            .NotEmpty().WithMessage("Husband first name is required")
            .MaximumLength(50);
        
        RuleFor(x => x.Application.Husband.LastName)
            .NotEmpty().WithMessage("Husband last name is required")
            .MaximumLength(50);
        
        RuleFor(x => x.Application.Husband.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
        
        RuleFor(x => x.Application.Husband.PhoneNumbers)
            .NotEmpty().WithMessage("At least one phone number is required")
            .Must(phones => phones.Count <= 5)
            .WithMessage("Maximum 5 phone numbers allowed");
        
        RuleForEach(x => x.Application.Husband.PhoneNumbers)
            .ChildRules(phone =>
            {
                phone.RuleFor(p => p.Number)
                    .NotEmpty()
                    .Matches(@"^\d{10}$").WithMessage("Phone must be 10 digits");
            });
        
        RuleFor(x => x.Application.Address)
            .NotNull().WithMessage("Address is required");
        
        RuleFor(x => x.Application.Address.Street)
            .NotEmpty().WithMessage("Street is required");
        
        RuleFor(x => x.Application.Address.City)
            .NotEmpty().WithMessage("City is required");
        
        RuleFor(x => x.Application.Address.State)
            .NotEmpty().WithMessage("State is required")
            .Length(2).WithMessage("State must be 2 characters");
        
        RuleFor(x => x.Application.Address.ZipCode)
            .NotEmpty().WithMessage("Zip code is required")
            .Matches(@"^\d{5}(-\d{4})?$").WithMessage("Invalid zip code format");
        
        // Optional wife validation (if provided)
        When(x => x.Application.Wife != null, () =>
        {
            RuleFor(x => x.Application.Wife!.FirstName)
                .NotEmpty().WithMessage("Wife first name required when wife info provided");
            
            RuleFor(x => x.Application.Wife!.Email)
                .EmailAddress().WithMessage("Invalid wife email format")
                .When(x => !string.IsNullOrWhiteSpace(x.Application.Wife?.Email));
        });
        
        // Optional children validation
        RuleForEach(x => x.Application.Children)
            .ChildRules(child =>
            {
                child.RuleFor(c => c.Age)
                    .InclusiveBetween(0, 18)
                    .WithMessage("Child age must be 0-18");
                
                child.RuleFor(c => c.Gender)
                    .IsInEnum().WithMessage("Invalid gender");
            });
    }
}
```

**Controller:**
```csharp
[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicApplicationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PublicApplicationResult>> SubmitPublicApplication(
        [FromBody] PublicApplicationRequest request,
        CancellationToken ct)
    {
        var command = new SubmitPublicApplicationCommand(request);
        var result = await _mediator.Send(command, ct);
        
        return CreatedAtAction(
            nameof(HousingSearchController.GetById),
            "HousingSearch",
            new { id = result.HousingSearchId },
            result);
    }
}
```

### Example Request

```json
POST /api/applications/public
{
  "husband": {
    "firstName": "Moshe",
    "lastName": "Cohen",
    "fatherName": "Yaakov",
    "email": "moshe.cohen@example.com",
    "phoneNumbers": [
      { "number": "9085551234", "type": "Cell", "isPrimary": true }
    ],
    "occupation": "Software Engineer",
    "employerName": "Tech Corp"
  },
  "wife": {
    "firstName": "Sarah",
    "maidenName": "Goldstein",
    "fatherName": "David",
    "email": "sarah.cohen@example.com",
    "highSchool": "Beth Rivkah",
    "occupation": "Teacher"
  },
  "address": {
    "street": "123 Brooklyn Ave",
    "city": "Brooklyn",
    "state": "NY",
    "zipCode": "11213"
  },
  "children": [
    { "age": 5, "gender": "Male" },
    { "age": 3, "gender": "Female" }
  ],
  "currentKehila": "Crown Heights",
  "shabbosShul": "770",
  "housingPreferences": {
    "budgetAmount": 450000,
    "minBedrooms": 4,
    "minBathrooms": 2,
    "preferredCities": ["Union", "Roselle Park"],
    "moveTimeline": "ShortTerm"
  }
}
```

### Example Response

```json
{
  "applicantId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "housingSearchId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "confirmationMessage": "Application received! You will receive a confirmation email shortly."
}
```

### Definition of Done

- [ ] POST /api/applications/public endpoint created
- [ ] Creates Applicant with all provided info
- [ ] Creates HousingSearch in Submitted stage
- [ ] Validates all required fields
- [ ] Checks for duplicate emails
- [ ] Returns 201 with IDs on success
- [ ] Returns 400 with validation errors
- [ ] Returns 409 on duplicate email
- [ ] Unit tests for command handler (8+ tests)
- [ ] Validation tests (15+ tests)
- [ ] Integration tests (5+ tests)
- [ ] All tests passing

---

## US-011: Application Confirmation Email

### Story

**As a** family who submitted an application  
**I want to** receive a confirmation email  
**So that** I know my application was received successfully

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2

### Acceptance Criteria

1. Email sent automatically after successful application submission
2. Email includes applicant name and application ID
3. Email sent to husband's email (primary)
4. CC to wife's email if provided
5. Uses AWS SES for sending
6. Handles SES failures gracefully (logs error, doesn't fail application)

### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Email sent | Application submitted | Domain event raised | Confirmation email sent to husband |
| Wife CC'd | Wife email provided | Domain event raised | Email CC'd to wife |
| SES failure | SES unavailable | Attempt to send | Error logged, application not affected |
| Email content | Valid submission | Email sent | Contains name, ID, next steps |

### Technical Implementation

**Domain Event:**
```csharp
public record ApplicationSubmittedEvent(
    Guid ApplicantId,
    Guid HousingSearchId,
    string HusbandEmail,
    string? WifeEmail = null,
    string HusbandFirstName = ""
) : IDomainEvent;
```

**Event Handler:**
```csharp
public class ApplicationSubmittedEventHandler 
    : INotificationHandler<ApplicationSubmittedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<ApplicationSubmittedEventHandler> _logger;

    public async Task Handle(
        ApplicationSubmittedEvent notification, 
        CancellationToken ct)
    {
        try
        {
            var email = new ApplicationReceivedEmail
            {
                To = notification.HusbandEmail,
                Cc = notification.WifeEmail,
                ApplicantName = notification.HusbandFirstName,
                ApplicationId = notification.HousingSearchId.ToString()[..8].ToUpper(),
                SubmittedDate = DateTime.UtcNow
            };
            
            await _emailService.SendApplicationReceivedAsync(email, ct);
            
            _logger.LogInformation(
                "Confirmation email sent for application {ApplicationId}",
                notification.HousingSearchId);
        }
        catch (Exception ex)
        {
            // Log but don't throw - email failure shouldn't fail the application
            _logger.LogError(ex, 
                "Failed to send confirmation email for application {ApplicationId}",
                notification.HousingSearchId);
        }
    }
}
```

**Email Service Interface:**
```csharp
public interface IEmailService
{
    Task SendApplicationReceivedAsync(ApplicationReceivedEmail email, CancellationToken ct);
    Task SendBoardDecisionAsync(BoardDecisionEmail email, CancellationToken ct);
    Task SendStatusChangeAsync(StatusChangeEmail email, CancellationToken ct);
}

public class ApplicationReceivedEmail
{
    public required string To { get; init; }
    public string? Cc { get; init; }
    public required string ApplicantName { get; init; }
    public required string ApplicationId { get; init; }
    public required DateTime SubmittedDate { get; init; }
}
```

**AWS SES Implementation:**
```csharp
public class SesEmailService : IEmailService
{
    private readonly IAmazonSimpleEmailService _ses;
    private readonly EmailSettings _settings;
    private readonly ILogger<SesEmailService> _logger;

    public async Task SendApplicationReceivedAsync(
        ApplicationReceivedEmail email, 
        CancellationToken ct)
    {
        var subject = "Application Received - Welcome!";
        var htmlBody = GenerateApplicationReceivedHtml(email);
        var textBody = GenerateApplicationReceivedText(email);

        var request = new SendEmailRequest
        {
            Source = _settings.FromAddress,
            Destination = new Destination
            {
                ToAddresses = new List<string> { email.To },
                CcAddresses = string.IsNullOrWhiteSpace(email.Cc) 
                    ? new List<string>() 
                    : new List<string> { email.Cc }
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content(htmlBody),
                    Text = new Content(textBody)
                }
            }
        };

        await _ses.SendEmailAsync(request, ct);
    }

    private string GenerateApplicationReceivedHtml(ApplicationReceivedEmail email)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2c5282; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f7fafc; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #718096; }}
        .highlight {{ background-color: #ebf8ff; padding: 15px; border-left: 4px solid #2c5282; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Application Received!</h1>
        </div>
        <div class='content'>
            <p>Dear {email.ApplicantName},</p>
            
            <p>Thank you for applying to join our community! We've received your application and will review it shortly.</p>
            
            <div class='highlight'>
                <strong>Application ID:</strong> {email.ApplicationId}<br/>
                <strong>Submitted:</strong> {email.SubmittedDate:MMMM d, yyyy}
            </div>
            
            <h3>What Happens Next?</h3>
            <ol>
                <li>Your application will be reviewed by our board</li>
                <li>You'll receive an update within 2-3 weeks</li>
                <li>If approved, we'll start matching you with available properties</li>
            </ol>
            
            <p>If you have any questions, please reply to this email or call us at (908) 555-1234.</p>
            
            <p>We look forward to welcoming you to our community!</p>
            
            <p>Best regards,<br/>The Community Relocation Team</p>
        </div>
        <div class='footer'>
            <p>Union County Jewish Community<br/>
            123 Main Street, Union, NJ 07083</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateApplicationReceivedText(ApplicationReceivedEmail email)
    {
        return $@"
Application Received!

Dear {email.ApplicantName},

Thank you for applying to join our community! We've received your application and will review it shortly.

Application ID: {email.ApplicationId}
Submitted: {email.SubmittedDate:MMMM d, yyyy}

What Happens Next?
1. Your application will be reviewed by our board
2. You'll receive an update within 2-3 weeks
3. If approved, we'll start matching you with available properties

If you have any questions, please reply to this email or call us at (908) 555-1234.

We look forward to welcoming you to our community!

Best regards,
The Community Relocation Team

---
Union County Jewish Community
123 Main Street, Union, NJ 07083
";
    }
}
```

**Configuration:**
```csharp
// appsettings.json
{
  "Email": {
    "FromAddress": "noreply@familyrelocation.org",
    "FromName": "Family Relocation System",
    "ReplyToAddress": "coordinator@familyrelocation.org"
  }
}

// EmailSettings.cs
public class EmailSettings
{
    public required string FromAddress { get; init; }
    public string? FromName { get; init; }
    public string? ReplyToAddress { get; init; }
}
```

### Definition of Done

- [ ] IEmailService interface created
- [ ] SesEmailService implementation created
- [ ] ApplicationSubmittedEventHandler sends email
- [ ] HTML and text email templates
- [ ] Error handling (doesn't fail application)
- [ ] Logging for sent emails and failures
- [ ] Unit tests for event handler
- [ ] Integration test with SES (can be mocked)
- [ ] All tests passing

---

## US-012: Create HousingSearch for Applicant

### Story

**As a** coordinator  
**I want to** create a HousingSearch for an existing applicant  
**So that** I can track their house hunting journey

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2

### Acceptance Criteria

1. Can create HousingSearch for applicant without existing HousingSearch
2. Cannot create if applicant already has a HousingSearch (1:1 relationship)
3. Initial stage is "Submitted"
4. Housing preferences optional initially
5. Returns created HousingSearch with ID

### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Create success | Applicant exists, no HousingSearch | POST /api/housing-searches | 201 Created with HousingSearchId |
| Already exists | Applicant has HousingSearch | POST /api/housing-searches | 409 Conflict |
| Applicant not found | Invalid ApplicantId | POST /api/housing-searches | 404 Not Found |
| With preferences | Valid preferences provided | Create HousingSearch | Preferences saved |

### Technical Implementation

**Command:**
```csharp
public record CreateHousingSearchCommand(
    Guid ApplicantId,
    HousingPreferencesRequest? Preferences
) : IRequest<HousingSearchDto>;
```

**Handler:**
```csharp
public class CreateHousingSearchCommandHandler 
    : IRequestHandler<CreateHousingSearchCommand, HousingSearchDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public async Task<HousingSearchDto> Handle(
        CreateHousingSearchCommand request, 
        CancellationToken ct)
    {
        // Verify applicant exists
        var applicantExists = await _context.Set<Applicant>()
            .AnyAsync(a => a.Id == request.ApplicantId, ct);
        
        if (!applicantExists)
            throw new NotFoundException(nameof(Applicant), request.ApplicantId);
        
        // Check for existing HousingSearch
        var existingSearch = await _context.Set<HousingSearch>()
            .AnyAsync(h => h.ApplicantId == request.ApplicantId, ct);
        
        if (existingSearch)
            throw new ConflictException(
                $"Applicant {request.ApplicantId} already has a HousingSearch");
        
        // Create HousingSearch
        var preferences = request.Preferences?.ToDomain() 
            ?? HousingPreferences.Empty();
        
        var housingSearch = HousingSearch.Create(
            applicantId: request.ApplicantId,
            preferences: preferences,
            createdBy: _currentUser.UserId ?? WellKnownIds.SelfSubmittedUserId
        );
        
        _context.Add(housingSearch);
        await _context.SaveChangesAsync(ct);
        
        return housingSearch.ToDto();
    }
}
```

**Controller:**
```csharp
[ApiController]
[Route("api/housing-searches")]
[Authorize]
public class HousingSearchController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    [ProducesResponseType(typeof(HousingSearchDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HousingSearchDto>> Create(
        [FromBody] CreateHousingSearchRequest request,
        CancellationToken ct)
    {
        var command = new CreateHousingSearchCommand(
            request.ApplicantId,
            request.Preferences);
        
        var result = await _mediator.Send(command, ct);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}

public class CreateHousingSearchRequest
{
    public required Guid ApplicantId { get; init; }
    public HousingPreferencesRequest? Preferences { get; init; }
}
```

### Definition of Done

- [ ] POST /api/housing-searches endpoint created
- [ ] Creates HousingSearch in Submitted stage
- [ ] Validates applicant exists
- [ ] Prevents duplicate HousingSearch
- [ ] Unit tests (5+ tests)
- [ ] Integration tests (3+ tests)
- [ ] All tests passing

---

## US-013: View HousingSearch Details

### Story

**As a** coordinator  
**I want to** view HousingSearch details  
**So that** I can see the current status and history of a family's house hunting journey

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2

### Acceptance Criteria

1. Returns HousingSearch with all details
2. Includes current stage and preferences
3. Includes failed contracts history (if any)
4. Includes audit information (created, modified dates)
5. Returns 404 if not found

### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Get by ID | HousingSearch exists | GET /api/housing-searches/{id} | 200 OK with full details |
| Not found | Invalid ID | GET /api/housing-searches/{id} | 404 Not Found |
| With failed contracts | Has contract history | GET /api/housing-searches/{id} | Includes failed contracts array |

### Technical Implementation

**Query:**
```csharp
public record GetHousingSearchByIdQuery(Guid Id) : IRequest<HousingSearchDto>;
```

**Handler:**
```csharp
public class GetHousingSearchByIdQueryHandler 
    : IRequestHandler<GetHousingSearchByIdQuery, HousingSearchDto>
{
    private readonly IApplicationDbContext _context;

    public async Task<HousingSearchDto> Handle(
        GetHousingSearchByIdQuery request, 
        CancellationToken ct)
    {
        var housingSearch = await _context.Set<HousingSearch>()
            .FirstOrDefaultAsync(h => h.Id == request.Id, ct);
        
        if (housingSearch is null)
            throw new NotFoundException(nameof(HousingSearch), request.Id);
        
        return housingSearch.ToDto();
    }
}
```

**DTO:**
```csharp
public class HousingSearchDto
{
    public required Guid Id { get; init; }
    public required Guid ApplicantId { get; init; }
    public required string Stage { get; init; }
    public required HousingPreferencesDto Preferences { get; init; }
    public ContractDto? CurrentContract { get; init; }
    public List<FailedContractDto> FailedContracts { get; init; } = new();
    public string? MovedInStatus { get; init; }
    public string? PauseReason { get; init; }
    public DateTime? PausedDate { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime ModifiedDate { get; init; }
}

public class HousingPreferencesDto
{
    public decimal? BudgetAmount { get; init; }
    public int? MinBedrooms { get; init; }
    public decimal? MinBathrooms { get; init; }
    public List<string> PreferredCities { get; init; } = new();
    public List<string> RequiredFeatures { get; init; } = new();
    public string? MoveTimeline { get; init; }
    public ShulProximityDto? ShulProximity { get; init; }
    
    public string? EstimatedMonthlyPayment { get; init; } // Calculated
}

public class ContractDto
{
    public Guid PropertyId { get; init; }
    public decimal Price { get; init; }
    public DateTime ContractDate { get; init; }
    public DateTime? ExpectedClosingDate { get; init; }
    public DateTime? ActualClosingDate { get; init; }
}

public class FailedContractDto
{
    public Guid PropertyId { get; init; }
    public decimal Price { get; init; }
    public DateTime ContractDate { get; init; }
    public DateTime FailedDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
```

**Controller:**
```csharp
[HttpGet("{id:guid}")]
[ProducesResponseType(typeof(HousingSearchDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<HousingSearchDto>> GetById(
    Guid id, 
    CancellationToken ct)
{
    var query = new GetHousingSearchByIdQuery(id);
    var result = await _mediator.Send(query, ct);
    return Ok(result);
}
```

**Mapper Extension:**
```csharp
public static class HousingSearchMapper
{
    public static HousingSearchDto ToDto(this HousingSearch housingSearch)
    {
        return new HousingSearchDto
        {
            Id = housingSearch.Id,
            ApplicantId = housingSearch.ApplicantId,
            Stage = housingSearch.Stage.ToString(),
            Preferences = housingSearch.Preferences.ToDto(),
            CurrentContract = housingSearch.CurrentContract?.ToDto(),
            FailedContracts = housingSearch.FailedContracts
                .Select(f => f.ToDto())
                .ToList(),
            MovedInStatus = housingSearch.MovedInStatus?.ToString(),
            PauseReason = housingSearch.PauseReason,
            PausedDate = housingSearch.PausedDate,
            CreatedDate = housingSearch.CreatedDate,
            ModifiedDate = housingSearch.ModifiedDate
        };
    }
    
    public static HousingPreferencesDto ToDto(this HousingPreferences prefs)
    {
        return new HousingPreferencesDto
        {
            BudgetAmount = prefs.Budget?.Amount,
            MinBedrooms = prefs.MinBedrooms,
            MinBathrooms = prefs.MinBathrooms,
            PreferredCities = prefs.PreferredCities.ToList(),
            RequiredFeatures = prefs.RequiredFeatures.ToList(),
            MoveTimeline = prefs.MoveTimeline?.ToString(),
            ShulProximity = prefs.ShulProximity?.ToDto(),
            EstimatedMonthlyPayment = prefs.Budget != null 
                ? CalculateMonthlyPayment(prefs) 
                : null
        };
    }
    
    private static string CalculateMonthlyPayment(HousingPreferences prefs)
    {
        // Simple P&I calculation (will be enhanced in US-017)
        var principal = prefs.Budget!.Amount * 0.8m; // 20% down
        var monthlyRate = 0.065m / 12; // 6.5% annual
        var payments = 360m; // 30 years
        
        var monthly = principal * 
            (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), (double)payments)) /
            ((decimal)Math.Pow((double)(1 + monthlyRate), (double)payments) - 1);
        
        return monthly.ToString("C0");
    }
}
```

### Definition of Done

- [ ] GET /api/housing-searches/{id} endpoint created
- [ ] Returns full HousingSearch details
- [ ] Includes preferences and contract history
- [ ] Returns 404 for missing HousingSearch
- [ ] Mapper extension methods created
- [ ] Unit tests (5+ tests)
- [ ] Integration tests (3+ tests)
- [ ] All tests passing

---

## US-014: View HousingSearch Pipeline (Kanban)

### Story

**As a** coordinator  
**I want to** view all HousingSearches in a Kanban-style pipeline  
**So that** I can see the status of all families at a glance

**Priority:** P0  
**Effort:** 8 points  
**Sprint:** 2

### Acceptance Criteria

1. Returns HousingSearches grouped by stage
2. Each card shows: Family name, days in stage, city preference
3. Supports filtering by city, search by family name
4. Includes count per stage
5. Ordered by date within each stage

### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Get pipeline | HousingSearches exist | GET /api/housing-searches/pipeline | Grouped by stage |
| Filter by city | City filter applied | GET /api/housing-searches/pipeline?city=Union | Only Union families |
| Search by name | Name search | GET /api/housing-searches/pipeline?search=Cohen | Matching families |
| Empty stage | No families in stage | GET pipeline | Stage shown with 0 count |

### Technical Implementation

**Query:**
```csharp
public record GetHousingSearchPipelineQuery(
    string? City = null,
    string? Search = null
) : IRequest<HousingSearchPipelineDto>;
```

**Handler:**
```csharp
public class GetHousingSearchPipelineQueryHandler 
    : IRequestHandler<GetHousingSearchPipelineQuery, HousingSearchPipelineDto>
{
    private readonly IApplicationDbContext _context;

    public async Task<HousingSearchPipelineDto> Handle(
        GetHousingSearchPipelineQuery request, 
        CancellationToken ct)
    {
        // Get all housing searches with applicant info
        var query = _context.Set<HousingSearch>()
            .Join(
                _context.Set<Applicant>(),
                hs => hs.ApplicantId,
                a => a.Id,
                (hs, a) => new { HousingSearch = hs, Applicant = a }
            );
        
        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(x => 
                x.HousingSearch.Preferences.PreferredCities.Contains(request.City));
        }
        
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(x =>
                x.Applicant.Husband.FirstName.ToLower().Contains(searchLower) ||
                x.Applicant.Husband.LastName.ToLower().Contains(searchLower) ||
                (x.Applicant.Wife != null && 
                 x.Applicant.Wife.FirstName.ToLower().Contains(searchLower)));
        }
        
        var results = await query.ToListAsync(ct);
        
        // Group by stage
        var stages = Enum.GetValues<HousingSearchStage>()
            .Select(stage => new PipelineStageDto
            {
                Stage = stage.ToString(),
                Count = results.Count(r => r.HousingSearch.Stage == stage),
                Cards = results
                    .Where(r => r.HousingSearch.Stage == stage)
                    .OrderBy(r => r.HousingSearch.ModifiedDate)
                    .Select(r => new PipelineCardDto
                    {
                        HousingSearchId = r.HousingSearch.Id,
                        ApplicantId = r.Applicant.Id,
                        FamilyName = $"{r.Applicant.Husband.LastName} Family",
                        HusbandName = r.Applicant.Husband.FullNameWithFather,
                        PreferredCities = r.HousingSearch.Preferences.PreferredCities.ToList(),
                        Budget = r.HousingSearch.Preferences.Budget?.Formatted,
                        DaysInStage = (int)(DateTime.UtcNow - r.HousingSearch.ModifiedDate).TotalDays,
                        IsPaused = r.HousingSearch.Stage == HousingSearchStage.Paused,
                        PauseReason = r.HousingSearch.PauseReason
                    })
                    .ToList()
            })
            .ToList();
        
        return new HousingSearchPipelineDto
        {
            Stages = stages,
            TotalCount = results.Count,
            FilteredCity = request.City,
            SearchTerm = request.Search
        };
    }
}
```

**DTOs:**
```csharp
public class HousingSearchPipelineDto
{
    public List<PipelineStageDto> Stages { get; init; } = new();
    public int TotalCount { get; init; }
    public string? FilteredCity { get; init; }
    public string? SearchTerm { get; init; }
}

public class PipelineStageDto
{
    public required string Stage { get; init; }
    public int Count { get; init; }
    public List<PipelineCardDto> Cards { get; init; } = new();
}

public class PipelineCardDto
{
    public required Guid HousingSearchId { get; init; }
    public required Guid ApplicantId { get; init; }
    public required string FamilyName { get; init; }
    public required string HusbandName { get; init; }
    public List<string> PreferredCities { get; init; } = new();
    public string? Budget { get; init; }
    public int DaysInStage { get; init; }
    public bool IsPaused { get; init; }
    public string? PauseReason { get; init; }
}
```

**Controller:**
```csharp
[HttpGet("pipeline")]
[ProducesResponseType(typeof(HousingSearchPipelineDto), StatusCodes.Status200OK)]
public async Task<ActionResult<HousingSearchPipelineDto>> GetPipeline(
    [FromQuery] string? city,
    [FromQuery] string? search,
    CancellationToken ct)
{
    var query = new GetHousingSearchPipelineQuery(city, search);
    var result = await _mediator.Send(query, ct);
    return Ok(result);
}
```

### Example Response

```json
{
  "stages": [
    {
      "stage": "Submitted",
      "count": 3,
      "cards": [
        {
          "housingSearchId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
          "applicantId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
          "familyName": "Cohen Family",
          "husbandName": "Moshe Cohen (ben Yaakov)",
          "preferredCities": ["Union", "Roselle Park"],
          "budget": "$450,000",
          "daysInStage": 5,
          "isPaused": false,
          "pauseReason": null
        }
      ]
    },
    {
      "stage": "HouseHunting",
      "count": 2,
      "cards": [...]
    },
    {
      "stage": "UnderContract",
      "count": 1,
      "cards": [...]
    },
    {
      "stage": "Closed",
      "count": 5,
      "cards": [...]
    },
    {
      "stage": "Paused",
      "count": 1,
      "cards": [...]
    },
    {
      "stage": "Rejected",
      "count": 0,
      "cards": []
    }
  ],
  "totalCount": 12,
  "filteredCity": null,
  "searchTerm": null
}
```

### Definition of Done

- [ ] GET /api/housing-searches/pipeline endpoint created
- [ ] Returns all stages with counts
- [ ] Cards include family name, budget, days in stage
- [ ] Filtering by city works
- [ ] Search by family name works
- [ ] Unit tests (8+ tests)
- [ ] Integration tests (5+ tests)
- [ ] All tests passing

---

## US-015: Change HousingSearch Stage

### Story

**As a** coordinator  
**I want to** change a HousingSearch stage  
**So that** I can track the family's progress through the process

**Priority:** P0  
**Effort:** 5 points  
**Sprint:** 2

### Acceptance Criteria

1. Can change stage following valid transitions
2. Invalid transitions rejected with error message
3. Certain transitions require additional data (contract for UnderContract)
4. Stage change creates activity log entry
5. Records who changed and when

### Valid Stage Transitions

```
Submitted → HouseHunting (board approved)
Submitted → Rejected (board rejected)

HouseHunting → UnderContract (requires contract info)
HouseHunting → Paused (requires reason)

UnderContract → Closed (closing complete)
UnderContract → HouseHunting (contract failed - creates FailedContract)
UnderContract → Paused (requires reason)

Paused → HouseHunting (resume search)
Paused → Submitted (restart process)

Closed → (terminal state, no transitions out)
Rejected → (terminal state, no transitions out)
```

### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Valid transition | Stage=Submitted | Change to HouseHunting | Success, stage updated |
| Invalid transition | Stage=Submitted | Change to UnderContract | 400 Bad Request |
| To UnderContract | Stage=HouseHunting | Change to UnderContract without contract | 400 Bad Request |
| Contract fails | Stage=UnderContract | Change to HouseHunting | FailedContract created |
| To Paused | Any active stage | Change to Paused without reason | 400 Bad Request |

### Technical Implementation

**Command:**
```csharp
public record ChangeHousingSearchStageCommand(
    Guid HousingSearchId,
    string NewStage,
    ContractRequest? Contract = null,      // Required for UnderContract
    string? PauseReason = null,            // Required for Paused
    FailedContractRequest? FailedContract = null  // Required when leaving UnderContract
) : IRequest<HousingSearchDto>;

public class ContractRequest
{
    public required Guid PropertyId { get; init; }
    public required decimal Price { get; init; }
    public required DateTime ContractDate { get; init; }
    public DateTime? ExpectedClosingDate { get; init; }
}

public class FailedContractRequest
{
    public required string Reason { get; init; }
    public string? Notes { get; init; }
}
```

**Handler:**
```csharp
public class ChangeHousingSearchStageCommandHandler 
    : IRequestHandler<ChangeHousingSearchStageCommand, HousingSearchDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public async Task<HousingSearchDto> Handle(
        ChangeHousingSearchStageCommand request, 
        CancellationToken ct)
    {
        var housingSearch = await _context.Set<HousingSearch>()
            .FirstOrDefaultAsync(h => h.Id == request.HousingSearchId, ct);
        
        if (housingSearch is null)
            throw new NotFoundException(nameof(HousingSearch), request.HousingSearchId);
        
        if (!Enum.TryParse<HousingSearchStage>(request.NewStage, out var newStage))
            throw new ValidationException($"Invalid stage: {request.NewStage}");
        
        var currentStage = housingSearch.Stage;
        var userId = _currentUser.UserId 
            ?? throw new UnauthorizedAccessException("User ID required");
        
        // Validate and execute transition
        switch (newStage)
        {
            case HousingSearchStage.HouseHunting:
                if (currentStage == HousingSearchStage.Submitted)
                {
                    housingSearch.MoveToHouseHunting(userId);
                }
                else if (currentStage == HousingSearchStage.UnderContract)
                {
                    // Contract failed
                    if (request.FailedContract is null)
                        throw new ValidationException(
                            "Failed contract details required when moving from UnderContract");
                    
                    housingSearch.RecordFailedContract(
                        request.FailedContract.Reason,
                        request.FailedContract.Notes,
                        userId);
                }
                else if (currentStage == HousingSearchStage.Paused)
                {
                    housingSearch.Resume(userId);
                }
                else
                {
                    throw new InvalidStageTransitionException(currentStage, newStage);
                }
                break;
            
            case HousingSearchStage.UnderContract:
                if (currentStage != HousingSearchStage.HouseHunting)
                    throw new InvalidStageTransitionException(currentStage, newStage);
                
                if (request.Contract is null)
                    throw new ValidationException(
                        "Contract details required when moving to UnderContract");
                
                var contract = new Contract(
                    request.Contract.PropertyId,
                    new Money(request.Contract.Price),
                    request.Contract.ContractDate,
                    request.Contract.ExpectedClosingDate);
                
                housingSearch.MoveToUnderContract(contract, userId);
                break;
            
            case HousingSearchStage.Closed:
                if (currentStage != HousingSearchStage.UnderContract)
                    throw new InvalidStageTransitionException(currentStage, newStage);
                
                housingSearch.Close(DateTime.UtcNow, userId);
                break;
            
            case HousingSearchStage.Paused:
                if (string.IsNullOrWhiteSpace(request.PauseReason))
                    throw new ValidationException("Pause reason required");
                
                housingSearch.Pause(request.PauseReason, userId);
                break;
            
            case HousingSearchStage.Rejected:
                if (currentStage != HousingSearchStage.Submitted)
                    throw new InvalidStageTransitionException(currentStage, newStage);
                
                housingSearch.Reject(userId);
                break;
            
            default:
                throw new ValidationException($"Cannot transition to stage: {newStage}");
        }
        
        await _context.SaveChangesAsync(ct);
        
        return housingSearch.ToDto();
    }
}

public class InvalidStageTransitionException : ValidationException
{
    public InvalidStageTransitionException(
        HousingSearchStage from, 
        HousingSearchStage to)
        : base($"Cannot transition from {from} to {to}")
    {
    }
}
```

**Domain Methods (HousingSearch entity):**
```csharp
public class HousingSearch
{
    // ... existing code ...
    
    public void MoveToHouseHunting(Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.Submitted && Stage != HousingSearchStage.Paused)
            throw new InvalidOperationException(
                $"Cannot move to HouseHunting from {Stage}");
        
        Stage = HousingSearchStage.HouseHunting;
        PauseReason = null;
        PausedDate = null;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new HousingSearchStageChangedEvent(Id, Stage));
    }
    
    public void MoveToUnderContract(Contract contract, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.HouseHunting)
            throw new InvalidOperationException(
                $"Cannot move to UnderContract from {Stage}");
        
        CurrentContract = contract;
        Stage = HousingSearchStage.UnderContract;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new HousingSearchStageChangedEvent(Id, Stage));
    }
    
    public void RecordFailedContract(string reason, string? notes, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.UnderContract || CurrentContract is null)
            throw new InvalidOperationException("No active contract to fail");
        
        var failedAttempt = new FailedContractAttempt(
            CurrentContract,
            DateTime.UtcNow,
            reason,
            notes);
        
        FailedContracts.Add(failedAttempt);
        CurrentContract = null;
        Stage = HousingSearchStage.HouseHunting;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new ContractFailedEvent(Id, failedAttempt));
    }
    
    public void Pause(string reason, Guid modifiedBy)
    {
        if (Stage == HousingSearchStage.Closed || Stage == HousingSearchStage.Rejected)
            throw new InvalidOperationException($"Cannot pause from terminal stage {Stage}");
        
        PauseReason = reason;
        PausedDate = DateTime.UtcNow;
        Stage = HousingSearchStage.Paused;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new HousingSearchPausedEvent(Id, reason));
    }
    
    public void Resume(Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.Paused)
            throw new InvalidOperationException("Can only resume from Paused stage");
        
        Stage = HousingSearchStage.HouseHunting;
        PauseReason = null;
        PausedDate = null;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new HousingSearchResumedEvent(Id));
    }
    
    public void Close(DateTime closingDate, Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.UnderContract)
            throw new InvalidOperationException(
                $"Cannot close from {Stage}, must be UnderContract");
        
        if (CurrentContract is null)
            throw new InvalidOperationException("No contract to close");
        
        CurrentContract = CurrentContract with 
        { 
            ActualClosingDate = closingDate 
        };
        Stage = HousingSearchStage.Closed;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new HousingSearchClosedEvent(Id, closingDate));
    }
    
    public void Reject(Guid modifiedBy)
    {
        if (Stage != HousingSearchStage.Submitted)
            throw new InvalidOperationException(
                $"Cannot reject from {Stage}, must be Submitted");
        
        Stage = HousingSearchStage.Rejected;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        
        AddDomainEvent(new HousingSearchRejectedEvent(Id));
    }
}
```

**Controller:**
```csharp
[HttpPut("{id:guid}/stage")]
[ProducesResponseType(typeof(HousingSearchDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<HousingSearchDto>> ChangeStage(
    Guid id,
    [FromBody] ChangeStageRequest request,
    CancellationToken ct)
{
    var command = new ChangeHousingSearchStageCommand(
        id,
        request.NewStage,
        request.Contract,
        request.PauseReason,
        request.FailedContract);
    
    var result = await _mediator.Send(command, ct);
    return Ok(result);
}

public class ChangeStageRequest
{
    public required string NewStage { get; init; }
    public ContractRequest? Contract { get; init; }
    public string? PauseReason { get; init; }
    public FailedContractRequest? FailedContract { get; init; }
}
```

### Example Requests

**Approve (Submitted → HouseHunting):**
```json
PUT /api/housing-searches/{id}/stage
{
  "newStage": "HouseHunting"
}
```

**Under Contract:**
```json
PUT /api/housing-searches/{id}/stage
{
  "newStage": "UnderContract",
  "contract": {
    "propertyId": "c3d4e5f6-a7b8-9012-cdef-123456789012",
    "price": 425000,
    "contractDate": "2026-01-20",
    "expectedClosingDate": "2026-03-15"
  }
}
```

**Contract Failed:**
```json
PUT /api/housing-searches/{id}/stage
{
  "newStage": "HouseHunting",
  "failedContract": {
    "reason": "HomeInspectionIssues",
    "notes": "Structural issues found in basement"
  }
}
```

**Pause:**
```json
PUT /api/housing-searches/{id}/stage
{
  "newStage": "Paused",
  "pauseReason": "Family emergency - will resume in 3 months"
}
```

### Definition of Done

- [ ] PUT /api/housing-searches/{id}/stage endpoint created
- [ ] All valid transitions work correctly
- [ ] Invalid transitions rejected with clear error
- [ ] Contract required for UnderContract transition
- [ ] Failed contract recorded when leaving UnderContract
- [ ] Pause reason required for Paused transition
- [ ] Domain events raised for stage changes
- [ ] Unit tests for all transitions (15+ tests)
- [ ] Integration tests (8+ tests)
- [ ] All tests passing

---

## US-016: Update Housing Preferences

### Story

**As a** coordinator  
**I want to** update a family's housing preferences  
**So that** we can match them with appropriate properties

**Priority:** P0  
**Effort:** 5 points  
**Sprint:** 2

### Acceptance Criteria

1. Can update all housing preference fields
2. Validates budget is positive
3. Validates bedrooms/bathrooms are reasonable
4. Cities limited to Union and Roselle Park
5. MoveTimeline must be valid enum value
6. ShulProximity properly validated
7. Returns updated HousingSearch

### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Update success | Valid preferences | PUT /api/housing-searches/{id}/preferences | 200 OK with updated data |
| Invalid budget | Negative budget | PUT preferences | 400 Bad Request |
| Invalid city | City not Union/Roselle Park | PUT preferences | 400 Bad Request |
| Not found | Invalid HousingSearch ID | PUT preferences | 404 Not Found |

### Technical Implementation

**Command:**
```csharp
public record UpdateHousingPreferencesCommand(
    Guid HousingSearchId,
    HousingPreferencesRequest Preferences
) : IRequest<HousingSearchDto>;
```

**Handler:**
```csharp
public class UpdateHousingPreferencesCommandHandler 
    : IRequestHandler<UpdateHousingPreferencesCommand, HousingSearchDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private static readonly HashSet<string> ValidCities = new() { "Union", "Roselle Park" };

    public async Task<HousingSearchDto> Handle(
        UpdateHousingPreferencesCommand request, 
        CancellationToken ct)
    {
        var housingSearch = await _context.Set<HousingSearch>()
            .FirstOrDefaultAsync(h => h.Id == request.HousingSearchId, ct);
        
        if (housingSearch is null)
            throw new NotFoundException(nameof(HousingSearch), request.HousingSearchId);
        
        var prefs = request.Preferences;
        
        // Validate cities
        if (prefs.PreferredCities?.Any() == true)
        {
            var invalidCities = prefs.PreferredCities
                .Where(c => !ValidCities.Contains(c))
                .ToList();
            
            if (invalidCities.Any())
                throw new ValidationException(
                    $"Invalid cities: {string.Join(", ", invalidCities)}. " +
                    $"Valid options: {string.Join(", ", ValidCities)}");
        }
        
        // Build preferences value object
        var newPreferences = new HousingPreferences(
            budget: prefs.BudgetAmount.HasValue 
                ? new Money(prefs.BudgetAmount.Value) 
                : null,
            minBedrooms: prefs.MinBedrooms,
            minBathrooms: prefs.MinBathrooms,
            preferredCities: prefs.PreferredCities ?? new(),
            requiredFeatures: prefs.RequiredFeatures ?? new(),
            moveTimeline: ParseMoveTimeline(prefs.MoveTimeline),
            shulProximity: prefs.ShulProximity?.ToDomain()
        );
        
        var userId = _currentUser.UserId 
            ?? throw new UnauthorizedAccessException("User ID required");
        
        housingSearch.UpdatePreferences(newPreferences, userId);
        
        await _context.SaveChangesAsync(ct);
        
        return housingSearch.ToDto();
    }
    
    private static MoveTimeline? ParseMoveTimeline(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        
        if (!Enum.TryParse<MoveTimeline>(value, out var result))
            throw new ValidationException($"Invalid move timeline: {value}");
        
        return result;
    }
}
```

**Validator:**
```csharp
public class UpdateHousingPreferencesCommandValidator 
    : AbstractValidator<UpdateHousingPreferencesCommand>
{
    private static readonly HashSet<string> ValidCities = new() { "Union", "Roselle Park" };
    private static readonly HashSet<string> ValidMoveTimelines = 
        Enum.GetNames<MoveTimeline>().ToHashSet();

    public UpdateHousingPreferencesCommandValidator()
    {
        RuleFor(x => x.HousingSearchId)
            .NotEmpty().WithMessage("HousingSearch ID is required");
        
        RuleFor(x => x.Preferences.BudgetAmount)
            .GreaterThan(0).WithMessage("Budget must be positive")
            .LessThanOrEqualTo(10_000_000).WithMessage("Budget seems unrealistic")
            .When(x => x.Preferences.BudgetAmount.HasValue);
        
        RuleFor(x => x.Preferences.MinBedrooms)
            .InclusiveBetween(1, 10).WithMessage("Bedrooms must be 1-10")
            .When(x => x.Preferences.MinBedrooms.HasValue);
        
        RuleFor(x => x.Preferences.MinBathrooms)
            .InclusiveBetween(1, 6).WithMessage("Bathrooms must be 1-6")
            .When(x => x.Preferences.MinBathrooms.HasValue);
        
        RuleForEach(x => x.Preferences.PreferredCities)
            .Must(city => ValidCities.Contains(city))
            .WithMessage("City must be Union or Roselle Park");
        
        RuleFor(x => x.Preferences.MoveTimeline)
            .Must(mt => ValidMoveTimelines.Contains(mt!))
            .WithMessage($"Invalid move timeline. Valid options: {string.Join(", ", ValidMoveTimelines)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Preferences.MoveTimeline));
        
        When(x => x.Preferences.ShulProximity != null, () =>
        {
            RuleFor(x => x.Preferences.ShulProximity!.MaxDistanceMiles)
                .GreaterThan(0).WithMessage("Max distance must be positive")
                .LessThanOrEqualTo(5).WithMessage("Max distance cannot exceed 5 miles");
        });
    }
}
```

**Controller:**
```csharp
[HttpPut("{id:guid}/preferences")]
[ProducesResponseType(typeof(HousingSearchDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<HousingSearchDto>> UpdatePreferences(
    Guid id,
    [FromBody] HousingPreferencesRequest preferences,
    CancellationToken ct)
{
    var command = new UpdateHousingPreferencesCommand(id, preferences);
    var result = await _mediator.Send(command, ct);
    return Ok(result);
}
```

### Example Request

```json
PUT /api/housing-searches/{id}/preferences
{
  "budgetAmount": 475000,
  "minBedrooms": 4,
  "minBathrooms": 2.5,
  "preferredCities": ["Union", "Roselle Park"],
  "requiredFeatures": ["Garage", "Basement", "Central AC"],
  "moveTimeline": "ShortTerm",
  "shulProximity": {
    "preferenceType": "AnyShul",
    "maxDistanceMiles": 0.5
  }
}
```

### Definition of Done

- [ ] PUT /api/housing-searches/{id}/preferences endpoint created
- [ ] All fields validated properly
- [ ] Cities restricted to Union/Roselle Park
- [ ] MoveTimeline enum validated
- [ ] ShulProximity validated
- [ ] Domain event raised (HousingPreferencesUpdated)
- [ ] Unit tests (10+ tests)
- [ ] Validation tests (12+ tests)
- [ ] Integration tests (5+ tests)
- [ ] All tests passing

---

## US-017: Calculate Monthly Payment Estimate

### Story

**As a** coordinator  
**I want to** see estimated monthly payment based on housing preferences  
**So that** I can help families understand affordability

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2

### Acceptance Criteria

1. Calculate P&I (Principal & Interest) based on budget
2. Default assumptions: 20% down, 6.5% rate, 30-year term
3. Allow overriding assumptions
4. Display in HousingPreferences DTO
5. Also available as standalone calculation endpoint

### Acceptance Criteria (Gherkin Format)

| Scenario | Given | When | Then |
|----------|-------|------|------|
| Default calc | Budget $450,000 | Calculate payment | ~$2,275/month (P&I) |
| Custom down | Budget $450,000, 10% down | Calculate payment | ~$2,563/month |
| Custom rate | Budget $450,000, 7.0% rate | Calculate payment | ~$2,395/month |
| No budget | Budget not set | Get preferences | Monthly payment is null |

### Technical Implementation

**Calculator Service:**
```csharp
public interface IMortgageCalculator
{
    MonthlyPaymentResult Calculate(MortgageCalculationRequest request);
}

public class MortgageCalculationRequest
{
    public required decimal PurchasePrice { get; init; }
    public decimal DownPaymentPercent { get; init; } = 20m;
    public decimal AnnualInterestRate { get; init; } = 6.5m;
    public int LoanTermYears { get; init; } = 30;
    public decimal? AnnualPropertyTax { get; init; }
    public decimal? AnnualInsurance { get; init; }
}

public class MonthlyPaymentResult
{
    public required decimal Principal { get; init; }
    public required decimal Interest { get; init; }
    public required decimal PrincipalAndInterest { get; init; }
    public decimal PropertyTax { get; init; }
    public decimal Insurance { get; init; }
    public required decimal TotalMonthly { get; init; }
    
    public string PrincipalAndInterestFormatted => PrincipalAndInterest.ToString("C0");
    public string TotalMonthlyFormatted => TotalMonthly.ToString("C0");
}

public class MortgageCalculator : IMortgageCalculator
{
    public MonthlyPaymentResult Calculate(MortgageCalculationRequest request)
    {
        var downPayment = request.PurchasePrice * (request.DownPaymentPercent / 100);
        var loanAmount = request.PurchasePrice - downPayment;
        var monthlyRate = request.AnnualInterestRate / 100 / 12;
        var numberOfPayments = request.LoanTermYears * 12;
        
        // P&I calculation: M = P * [r(1+r)^n] / [(1+r)^n - 1]
        var powerFactor = (decimal)Math.Pow((double)(1 + monthlyRate), numberOfPayments);
        var monthlyPI = loanAmount * (monthlyRate * powerFactor) / (powerFactor - 1);
        
        // Property tax (estimated 2% annually if not provided)
        var monthlyTax = (request.AnnualPropertyTax ?? request.PurchasePrice * 0.02m) / 12;
        
        // Insurance (estimated 0.5% annually if not provided)
        var monthlyInsurance = (request.AnnualInsurance ?? request.PurchasePrice * 0.005m) / 12;
        
        // Separate P&I into principal and interest for first payment
        var firstMonthInterest = loanAmount * monthlyRate;
        var firstMonthPrincipal = monthlyPI - firstMonthInterest;
        
        return new MonthlyPaymentResult
        {
            Principal = Math.Round(firstMonthPrincipal, 2),
            Interest = Math.Round(firstMonthInterest, 2),
            PrincipalAndInterest = Math.Round(monthlyPI, 2),
            PropertyTax = Math.Round(monthlyTax, 2),
            Insurance = Math.Round(monthlyInsurance, 2),
            TotalMonthly = Math.Round(monthlyPI + monthlyTax + monthlyInsurance, 2)
        };
    }
}
```

**Query for Standalone Calculation:**
```csharp
public record CalculateMonthlyPaymentQuery(
    decimal PurchasePrice,
    decimal DownPaymentPercent = 20,
    decimal AnnualInterestRate = 6.5m,
    int LoanTermYears = 30,
    decimal? AnnualPropertyTax = null,
    decimal? AnnualInsurance = null
) : IRequest<MonthlyPaymentResult>;

public class CalculateMonthlyPaymentQueryHandler 
    : IRequestHandler<CalculateMonthlyPaymentQuery, MonthlyPaymentResult>
{
    private readonly IMortgageCalculator _calculator;

    public async Task<MonthlyPaymentResult> Handle(
        CalculateMonthlyPaymentQuery request, 
        CancellationToken ct)
    {
        var calcRequest = new MortgageCalculationRequest
        {
            PurchasePrice = request.PurchasePrice,
            DownPaymentPercent = request.DownPaymentPercent,
            AnnualInterestRate = request.AnnualInterestRate,
            LoanTermYears = request.LoanTermYears,
            AnnualPropertyTax = request.AnnualPropertyTax,
            AnnualInsurance = request.AnnualInsurance
        };
        
        return await Task.FromResult(_calculator.Calculate(calcRequest));
    }
}
```

**Controller:**
```csharp
[HttpGet("calculate-payment")]
[AllowAnonymous]  // Allow public access for families
[ProducesResponseType(typeof(MonthlyPaymentResult), StatusCodes.Status200OK)]
public async Task<ActionResult<MonthlyPaymentResult>> CalculatePayment(
    [FromQuery] decimal purchasePrice,
    [FromQuery] decimal downPaymentPercent = 20,
    [FromQuery] decimal annualInterestRate = 6.5m,
    [FromQuery] int loanTermYears = 30,
    [FromQuery] decimal? annualPropertyTax = null,
    [FromQuery] decimal? annualInsurance = null,
    CancellationToken ct = default)
{
    var query = new CalculateMonthlyPaymentQuery(
        purchasePrice,
        downPaymentPercent,
        annualInterestRate,
        loanTermYears,
        annualPropertyTax,
        annualInsurance);
    
    var result = await _mediator.Send(query, ct);
    return Ok(result);
}
```

**Update HousingPreferencesDto:**
```csharp
public class HousingPreferencesDto
{
    // ... existing properties ...
    
    // Calculated monthly payment (if budget is set)
    public MonthlyPaymentResult? EstimatedMonthlyPayment { get; init; }
}

// In mapper:
public static HousingPreferencesDto ToDto(
    this HousingPreferences prefs,
    IMortgageCalculator calculator)
{
    MonthlyPaymentResult? payment = null;
    
    if (prefs.Budget != null)
    {
        payment = calculator.Calculate(new MortgageCalculationRequest
        {
            PurchasePrice = prefs.Budget.Amount
        });
    }
    
    return new HousingPreferencesDto
    {
        BudgetAmount = prefs.Budget?.Amount,
        MinBedrooms = prefs.MinBedrooms,
        MinBathrooms = prefs.MinBathrooms,
        PreferredCities = prefs.PreferredCities.ToList(),
        RequiredFeatures = prefs.RequiredFeatures.ToList(),
        MoveTimeline = prefs.MoveTimeline?.ToString(),
        ShulProximity = prefs.ShulProximity?.ToDto(),
        EstimatedMonthlyPayment = payment
    };
}
```

### Example Calculation

**Request:**
```
GET /api/housing-searches/calculate-payment?purchasePrice=450000&downPaymentPercent=20&annualInterestRate=6.5&loanTermYears=30
```

**Response:**
```json
{
  "principal": 327.50,
  "interest": 1950.00,
  "principalAndInterest": 2277.50,
  "propertyTax": 750.00,
  "insurance": 187.50,
  "totalMonthly": 3215.00,
  "principalAndInterestFormatted": "$2,278",
  "totalMonthlyFormatted": "$3,215"
}
```

### Definition of Done

- [ ] MortgageCalculator service created
- [ ] P&I calculation is accurate
- [ ] GET /api/housing-searches/calculate-payment endpoint created
- [ ] HousingPreferencesDto includes calculated payment
- [ ] Default values work (20% down, 6.5%, 30 years)
- [ ] Custom values work
- [ ] Unit tests for calculator (10+ tests)
- [ ] Integration tests (3+ tests)
- [ ] All tests passing

---

## 📋 SPRINT 2 SUMMARY

### NuGet Packages Needed

**Application Layer:**
```bash
# Already installed from Sprint 1
MediatR
FluentValidation
```

**Infrastructure Layer:**
```bash
dotnet add package AWSSDK.SimpleEmail
```

### New Files to Create

```
Application/
  HousingSearches/
    Commands/
      CreateHousingSearch/
        CreateHousingSearchCommand.cs
        CreateHousingSearchCommandHandler.cs
      ChangeHousingSearchStage/
        ChangeHousingSearchStageCommand.cs
        ChangeHousingSearchStageCommandHandler.cs
      UpdateHousingPreferences/
        UpdateHousingPreferencesCommand.cs
        UpdateHousingPreferencesCommandHandler.cs
        UpdateHousingPreferencesCommandValidator.cs
    Queries/
      GetHousingSearchById/
        GetHousingSearchByIdQuery.cs
        GetHousingSearchByIdQueryHandler.cs
      GetHousingSearchPipeline/
        GetHousingSearchPipelineQuery.cs
        GetHousingSearchPipelineQueryHandler.cs
      CalculateMonthlyPayment/
        CalculateMonthlyPaymentQuery.cs
        CalculateMonthlyPaymentQueryHandler.cs
    DTOs/
      HousingSearchDto.cs
      HousingPreferencesDto.cs
      ContractDto.cs
      FailedContractDto.cs
      PipelineDto.cs
    Mappers/
      HousingSearchMapper.cs
  
  Applications/
    Commands/
      SubmitPublicApplication/
        SubmitPublicApplicationCommand.cs
        SubmitPublicApplicationCommandHandler.cs
        SubmitPublicApplicationCommandValidator.cs
    DTOs/
      PublicApplicationRequest.cs
      PublicApplicationResult.cs
  
  Common/
    Services/
      IMortgageCalculator.cs
    Exceptions/
      ConflictException.cs
      InvalidStageTransitionException.cs

Infrastructure/
  Services/
    MortgageCalculator.cs
  Email/
    IEmailService.cs
    SesEmailService.cs
    EmailSettings.cs
    Templates/
      ApplicationReceivedEmail.cs

API/
  Controllers/
    ApplicationsController.cs
    HousingSearchController.cs
```

### Test Coverage Goals

| Area | Target |
|------|--------|
| Command Handlers | 90%+ |
| Query Handlers | 90%+ |
| Validators | 100% |
| Domain Methods | 100% |
| Controllers | 80%+ |

### Sprint 2 Definition of Done

- [ ] All 8 stories implemented
- [ ] All endpoints working
- [ ] Email notifications sending
- [ ] Pipeline view returning grouped data
- [ ] Stage transitions validated
- [ ] Monthly payment calculator accurate
- [ ] All tests passing (target: 100+ new tests)
- [ ] Code reviewed
- [ ] Documentation updated

---

## 🎯 DAILY BREAKDOWN (Suggested)

### Days 1-2: Public Application (US-010, US-011)
- Create SubmitPublicApplicationCommand
- Implement validation
- Set up email service
- Create email templates

### Days 3-4: HousingSearch CRUD (US-012, US-013)
- Create HousingSearch command/query
- Build DTOs and mappers
- Implement controller endpoints

### Days 5-6: Pipeline View (US-014)
- Build pipeline query with grouping
- Implement filtering and search
- Create pipeline DTOs

### Days 7-8: Stage Transitions (US-015)
- Implement all valid transitions
- Add domain methods to HousingSearch
- Handle contract creation/failure

### Days 9-10: Preferences & Calculator (US-016, US-017)
- Update preferences command
- Mortgage calculator service
- Integration and final testing

---

**Sprint 2 Detailed Stories Complete! 🚀**

**Reference Documents:**
- CONVERSATION_MEMORY_LOG.md - Full context
- SPRINT_1_DETAILED_STORIES.md - Sprint 1 patterns
- SOLUTION_STRUCTURE_AND_CODE_v3.md - Code reference
