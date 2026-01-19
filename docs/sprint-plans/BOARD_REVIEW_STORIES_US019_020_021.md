# BOARD REVIEW & APPROVAL - DETAILED USER STORIES
## US-019, US-020, US-021

**Epic:** Board Review & Approval  
**Priority:** P0 (MVP)  
**Total Points:** 13 points  
**Dependencies:** US-010 (Applicant creates HousingSearch), US-015 (Stage change API)

---

## OVERVIEW

These three stories complete the board review workflow:

| ID | Story | Points |
|----|-------|--------|
| US-019 | Board reviews application (set decision) | 5 |
| US-020 | Auto-move to Rejected when board disapproves | 3 |
| US-021 | Approve application and trigger paperwork stage | 5 |
| **Total** | | **13** |

### Workflow Context

```
Applicant submits → Stage: Submitted → Board Reviews (US-019)
                                            ↓
                         ┌──────────────────┼──────────────────┐
                         ↓                  ↓                  ↓
                    APPROVED           REJECTED            DEFERRED
                    (US-021)           (US-020)           (stays in Submitted)
                         ↓                  ↓
              Stage: HouseHunting    Stage: Rejected
              (ready for paperwork)  (auto-move)
```

---

## US-019: Board Reviews Application (Set Decision)

### Story

**As a** board member or coordinator  
**I want to** record the board's decision on an application  
**So that** the family's status is tracked and appropriate next steps can be triggered

**Priority:** P0  
**Effort:** 5 points  
**Sprint:** 2 (if pulling in)

### Background

After a family applies (Stage: Submitted), the board reviews their application at a board meeting. The coordinator records the board's decision. This doesn't automatically change the stage - that's handled by US-020 (rejection) and US-021 (approval).

### Acceptance Criteria

1. Can set BoardDecision on an Applicant (Pending, Approved, Rejected, Deferred)
2. Can set BoardReviewDate (when the board meeting occurred)
3. Can add BoardNotes (optional comments from board)
4. Only applicants in "Submitted" or with decision "Pending/Deferred" can have decision changed
5. Requires authentication (coordinators/admins only)
6. Creates audit log entry for the decision
7. Returns updated Applicant with BoardReview info

### Acceptance Criteria (Gherkin Format)

```gherkin
Feature: Board reviews application and records decision

Scenario: Set board decision to Approved
  Given an applicant exists with Stage "Submitted"
  And BoardDecision is "Pending"
  When I PUT /api/applicants/{id}/board-review with decision "Approved"
  Then the response status is 200 OK
  And the Applicant.BoardReview.Decision equals "Approved"
  And the Applicant.BoardReview.ReviewDate is set

Scenario: Set board decision to Rejected
  Given an applicant exists with Stage "Submitted"
  When I PUT /api/applicants/{id}/board-review with decision "Rejected"
  Then the response status is 200 OK
  And the Applicant.BoardReview.Decision equals "Rejected"

Scenario: Set board decision to Deferred
  Given an applicant exists with Stage "Submitted"
  When I PUT /api/applicants/{id}/board-review with decision "Deferred"
  Then the response status is 200 OK
  And the Applicant.BoardReview.Decision equals "Deferred"
  And the HousingSearch stage remains "Submitted"

Scenario: Include board notes
  Given an applicant exists
  When I PUT /api/applicants/{id}/board-review with notes "Excellent references"
  Then the Applicant.BoardReview.Notes equals "Excellent references"

Scenario: Cannot review applicant not in Submitted stage
  Given an applicant exists with Stage "HouseHunting"
  When I PUT /api/applicants/{id}/board-review with decision "Approved"
  Then the response status is 400 Bad Request
  And the error message indicates applicant is not in reviewable state

Scenario: Cannot review without authentication
  Given an applicant exists
  And I am not authenticated
  When I PUT /api/applicants/{id}/board-review
  Then the response status is 401 Unauthorized

Scenario: Audit log created for decision
  Given an applicant exists
  When I PUT /api/applicants/{id}/board-review with decision "Approved"
  Then an audit log entry is created with action "BoardDecisionSet"
```

### API Specification

**Endpoint:** `PUT /api/applicants/{id}/board-review`

**Authorization:** `[Authorize]` - Coordinators and Admins only

**Request Body:**
```json
{
  "decision": "Approved",          // Required: Approved, Rejected, Deferred
  "reviewDate": "2026-01-15",      // Optional: defaults to today
  "notes": "Strong references from Rabbi Cohen. Family is well-suited."  // Optional
}
```

**Response (200 OK):**
```json
{
  "applicantId": "guid",
  "boardReview": {
    "decision": "Approved",
    "reviewDate": "2026-01-15",
    "notes": "Strong references from Rabbi Cohen.",
    "reviewedBy": "coordinator-user-id"
  },
  "housingSearchStage": "Submitted",  // Note: Not changed yet - US-021 handles that
  "message": "Board decision recorded. Use approve/reject endpoints to transition stage."
}
```

**Error Responses:**
- `400 Bad Request` - Invalid decision value or applicant not in reviewable state
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Not authorized (wrong role)
- `404 Not Found` - Applicant doesn't exist

### Technical Implementation

**Command:**
```csharp
public record SetBoardDecisionCommand(
    Guid ApplicantId,
    BoardDecision Decision,
    DateOnly? ReviewDate,
    string? Notes
) : IRequest<SetBoardDecisionResponse>;

public record SetBoardDecisionResponse(
    Guid ApplicantId,
    BoardReviewDto BoardReview,
    HousingSearchStage CurrentStage,
    string Message
);
```

**Handler:**
```csharp
public class SetBoardDecisionCommandHandler 
    : IRequestHandler<SetBoardDecisionCommand, SetBoardDecisionResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public async Task<SetBoardDecisionResponse> Handle(
        SetBoardDecisionCommand request, 
        CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicantId, cancellationToken);

        if (applicant is null)
            throw new NotFoundException(nameof(Applicant), request.ApplicantId);

        // Validate: Can only review if in Submitted stage or decision is Pending/Deferred
        var housingSearch = applicant.HousingSearch 
            ?? throw new InvalidOperationException("Applicant has no HousingSearch");

        if (housingSearch.Stage != HousingSearchStage.Submitted)
            throw new ValidationException("Can only set board decision for applicants in Submitted stage");

        // Update board review
        var reviewDate = request.ReviewDate ?? DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        
        applicant.SetBoardDecision(
            decision: request.Decision,
            reviewDate: reviewDate,
            notes: request.Notes,
            reviewedBy: _currentUserService.UserId);

        await _context.SaveChangesAsync(cancellationToken);

        return new SetBoardDecisionResponse(
            ApplicantId: applicant.Id,
            BoardReview: applicant.BoardReview.ToDto(),
            CurrentStage: housingSearch.Stage,
            Message: GetNextStepMessage(request.Decision)
        );
    }

    private static string GetNextStepMessage(BoardDecision decision) => decision switch
    {
        BoardDecision.Approved => "Board approved. Use POST /api/applicants/{id}/approve to transition to House Hunting.",
        BoardDecision.Rejected => "Board rejected. Use POST /api/applicants/{id}/reject to transition to Rejected stage.",
        BoardDecision.Deferred => "Decision deferred. Applicant remains in Submitted stage for future review.",
        _ => "Decision recorded."
    };
}
```

**Domain Method on Applicant:**
```csharp
public class Applicant
{
    public BoardReview BoardReview { get; private set; } = BoardReview.Pending();

    public void SetBoardDecision(
        BoardDecision decision, 
        DateOnly reviewDate, 
        string? notes,
        Guid reviewedBy)
    {
        BoardReview = new BoardReview(
            decision: decision,
            reviewDate: reviewDate,
            notes: notes,
            reviewedBy: reviewedBy);
        
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = reviewedBy;
    }
}
```

**Validation:**
```csharp
public class SetBoardDecisionCommandValidator : AbstractValidator<SetBoardDecisionCommand>
{
    public SetBoardDecisionCommandValidator()
    {
        RuleFor(x => x.ApplicantId)
            .NotEmpty().WithMessage("ApplicantId is required");

        RuleFor(x => x.Decision)
            .IsInEnum().WithMessage("Invalid board decision value");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }
}
```

### Tests Required

**Unit Tests:**
- `SetBoardDecision_ValidApproval_UpdatesApplicant`
- `SetBoardDecision_ValidRejection_UpdatesApplicant`
- `SetBoardDecision_ValidDeferral_UpdatesApplicant`
- `SetBoardDecision_WithNotes_SavesNotes`
- `SetBoardDecision_WithoutDate_DefaultsToToday`
- `SetBoardDecision_ApplicantNotFound_ThrowsNotFoundException`
- `SetBoardDecision_NotInSubmittedStage_ThrowsValidationException`

**Integration Tests:**
- `PUT_BoardReview_ValidRequest_Returns200`
- `PUT_BoardReview_InvalidDecision_Returns400`
- `PUT_BoardReview_NotAuthenticated_Returns401`
- `PUT_BoardReview_ApplicantNotFound_Returns404`
- `PUT_BoardReview_WrongStage_Returns400`

---

## US-020: Auto-Move to Rejected When Board Disapproves

### Story

**As a** coordinator  
**I want** applicants to be moved to Rejected stage when I confirm the board's rejection  
**So that** rejected families are clearly separated from the active pipeline

**Priority:** P0  
**Effort:** 3 points  
**Sprint:** 2 (if pulling in)

### Background

After the board decision is recorded as "Rejected" (US-019), the coordinator can confirm the rejection which moves the HousingSearch to the Rejected stage. This is a separate action to allow for any final verification before officially rejecting.

### Acceptance Criteria

1. Endpoint to reject an applicant: `POST /api/applicants/{id}/reject`
2. Only works if BoardDecision is "Rejected" 
3. Transitions HousingSearch.Stage from "Submitted" to "Rejected"
4. Records rejection timestamp
5. Requires authentication
6. Creates audit log entry
7. Returns confirmation with updated status

### Acceptance Criteria (Gherkin Format)

```gherkin
Feature: Reject applicant after board disapproval

Scenario: Reject applicant with Rejected board decision
  Given an applicant exists with BoardDecision "Rejected"
  And HousingSearch is in stage "Submitted"
  When I POST /api/applicants/{id}/reject
  Then the response status is 200 OK
  And the HousingSearch.Stage equals "Rejected"

Scenario: Reject with reason
  Given an applicant exists with BoardDecision "Rejected"
  When I POST /api/applicants/{id}/reject with reason "Did not meet community requirements"
  Then the rejection reason is saved

Scenario: Cannot reject without board rejection decision
  Given an applicant exists with BoardDecision "Pending"
  When I POST /api/applicants/{id}/reject
  Then the response status is 400 Bad Request
  And the error message indicates board decision must be Rejected first

Scenario: Cannot reject already rejected applicant
  Given an applicant exists with HousingSearch stage "Rejected"
  When I POST /api/applicants/{id}/reject
  Then the response status is 400 Bad Request
  And the error message indicates applicant is already rejected

Scenario: Cannot reject approved applicant
  Given an applicant exists with BoardDecision "Approved"
  When I POST /api/applicants/{id}/reject
  Then the response status is 400 Bad Request

Scenario: Audit log created
  Given an applicant with BoardDecision "Rejected"
  When I POST /api/applicants/{id}/reject
  Then an audit log entry is created with action "ApplicationRejected"
```

### API Specification

**Endpoint:** `POST /api/applicants/{id}/reject`

**Authorization:** `[Authorize]` - Coordinators and Admins only

**Request Body (optional):**
```json
{
  "reason": "Did not meet community requirements",  // Optional
  "sendNotification": true  // Optional: for future email feature, defaults to false for now
}
```

**Response (200 OK):**
```json
{
  "applicantId": "guid",
  "housingSearchId": "guid",
  "previousStage": "Submitted",
  "newStage": "Rejected",
  "rejectedAt": "2026-01-15T14:30:00Z",
  "rejectedBy": "coordinator-user-id",
  "reason": "Did not meet community requirements"
}
```

**Error Responses:**
- `400 Bad Request` - Board decision is not Rejected, or already in Rejected stage
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - Applicant doesn't exist

### Technical Implementation

**Command:**
```csharp
public record RejectApplicantCommand(
    Guid ApplicantId,
    string? Reason,
    bool SendNotification = false
) : IRequest<RejectApplicantResponse>;

public record RejectApplicantResponse(
    Guid ApplicantId,
    Guid HousingSearchId,
    HousingSearchStage PreviousStage,
    HousingSearchStage NewStage,
    DateTime RejectedAt,
    Guid RejectedBy,
    string? Reason
);
```

**Handler:**
```csharp
public class RejectApplicantCommandHandler 
    : IRequestHandler<RejectApplicantCommand, RejectApplicantResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public async Task<RejectApplicantResponse> Handle(
        RejectApplicantCommand request, 
        CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicantId, cancellationToken);

        if (applicant is null)
            throw new NotFoundException(nameof(Applicant), request.ApplicantId);

        var housingSearch = applicant.HousingSearch 
            ?? throw new InvalidOperationException("Applicant has no HousingSearch");

        // Validate board decision is Rejected
        if (applicant.BoardReview.Decision != BoardDecision.Rejected)
            throw new ValidationException(
                $"Cannot reject applicant. Board decision must be 'Rejected' but is '{applicant.BoardReview.Decision}'");

        // Validate not already rejected
        if (housingSearch.Stage == HousingSearchStage.Rejected)
            throw new ValidationException("Applicant is already in Rejected stage");

        // Capture previous stage
        var previousStage = housingSearch.Stage;
        var now = _timeProvider.GetUtcNow().DateTime;
        var userId = _currentUserService.UserId;

        // Transition to Rejected
        housingSearch.TransitionTo(HousingSearchStage.Rejected, userId);
        
        // Store rejection reason if provided (could add a RejectionReason property)
        // For now, we can store it in notes or a dedicated field
        
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Future - send notification email if SendNotification is true

        return new RejectApplicantResponse(
            ApplicantId: applicant.Id,
            HousingSearchId: housingSearch.Id,
            PreviousStage: previousStage,
            NewStage: housingSearch.Stage,
            RejectedAt: now,
            RejectedBy: userId,
            Reason: request.Reason
        );
    }
}
```

### Tests Required

**Unit Tests:**
- `RejectApplicant_BoardDecisionRejected_TransitionsToRejected`
- `RejectApplicant_WithReason_SavesReason`
- `RejectApplicant_BoardDecisionNotRejected_ThrowsValidation`
- `RejectApplicant_AlreadyRejected_ThrowsValidation`
- `RejectApplicant_NotFound_ThrowsNotFoundException`

**Integration Tests:**
- `POST_Reject_ValidRequest_Returns200`
- `POST_Reject_BoardNotRejected_Returns400`
- `POST_Reject_AlreadyRejected_Returns400`
- `POST_Reject_NotAuthenticated_Returns401`

---

## US-021: Approve Application and Trigger Paperwork Stage

### Story

**As a** coordinator  
**I want to** approve an application and transition it to the House Hunting stage  
**So that** approved families can begin their home search

**Priority:** P0  
**Effort:** 5 points  
**Sprint:** 2 (if pulling in)

### Background

After the board approves a family (US-019), the coordinator confirms the approval which transitions the HousingSearch from "Submitted" to "HouseHunting". This signals that the family is now actively in the program and ready to start house hunting (after completing paperwork).

### Acceptance Criteria

1. Endpoint to approve an applicant: `POST /api/applicants/{id}/approve`
2. Only works if BoardDecision is "Approved"
3. Transitions HousingSearch.Stage from "Submitted" to "HouseHunting"
4. Records approval timestamp
5. Can optionally include welcome message/notes
6. Requires authentication
7. Creates audit log entry
8. Returns confirmation with updated status and next steps

### Acceptance Criteria (Gherkin Format)

```gherkin
Feature: Approve applicant after board approval

Scenario: Approve applicant with Approved board decision
  Given an applicant exists with BoardDecision "Approved"
  And HousingSearch is in stage "Submitted"
  When I POST /api/applicants/{id}/approve
  Then the response status is 200 OK
  And the HousingSearch.Stage equals "HouseHunting"

Scenario: Approve with welcome notes
  Given an applicant exists with BoardDecision "Approved"
  When I POST /api/applicants/{id}/approve with notes "Welcome! Please complete the broker agreement."
  Then the approval notes are saved

Scenario: Cannot approve without board approval decision
  Given an applicant exists with BoardDecision "Pending"
  When I POST /api/applicants/{id}/approve
  Then the response status is 400 Bad Request
  And the error message indicates board decision must be Approved first

Scenario: Cannot approve already approved applicant
  Given an applicant exists with HousingSearch stage "HouseHunting"
  When I POST /api/applicants/{id}/approve
  Then the response status is 400 Bad Request
  And the error message indicates applicant is already approved

Scenario: Cannot approve rejected applicant
  Given an applicant exists with BoardDecision "Rejected"
  When I POST /api/applicants/{id}/approve
  Then the response status is 400 Bad Request

Scenario: Cannot approve deferred applicant (must change decision first)
  Given an applicant exists with BoardDecision "Deferred"
  When I POST /api/applicants/{id}/approve
  Then the response status is 400 Bad Request
  And the error message indicates board decision must be Approved

Scenario: Audit log created
  Given an applicant with BoardDecision "Approved"
  When I POST /api/applicants/{id}/approve
  Then an audit log entry is created with action "ApplicationApproved"

Scenario: Response includes next steps
  Given an applicant with BoardDecision "Approved"
  When I POST /api/applicants/{id}/approve
  Then the response includes next steps guidance
```

### API Specification

**Endpoint:** `POST /api/applicants/{id}/approve`

**Authorization:** `[Authorize]` - Coordinators and Admins only

**Request Body (optional):**
```json
{
  "notes": "Welcome to the program! Please complete the broker agreement.",  // Optional
  "sendNotification": true  // Optional: for future email feature, defaults to false for now
}
```

**Response (200 OK):**
```json
{
  "applicantId": "guid",
  "housingSearchId": "guid",
  "previousStage": "Submitted",
  "newStage": "HouseHunting",
  "approvedAt": "2026-01-15T14:30:00Z",
  "approvedBy": "coordinator-user-id",
  "notes": "Welcome to the program!",
  "nextSteps": [
    "Send broker agreement to family",
    "Send community agreement to family",
    "Collect housing preferences",
    "Begin property matching"
  ]
}
```

**Error Responses:**
- `400 Bad Request` - Board decision is not Approved, or already past Submitted stage
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - Applicant doesn't exist

### Technical Implementation

**Command:**
```csharp
public record ApproveApplicantCommand(
    Guid ApplicantId,
    string? Notes,
    bool SendNotification = false
) : IRequest<ApproveApplicantResponse>;

public record ApproveApplicantResponse(
    Guid ApplicantId,
    Guid HousingSearchId,
    HousingSearchStage PreviousStage,
    HousingSearchStage NewStage,
    DateTime ApprovedAt,
    Guid ApprovedBy,
    string? Notes,
    IReadOnlyList<string> NextSteps
);
```

**Handler:**
```csharp
public class ApproveApplicantCommandHandler 
    : IRequestHandler<ApproveApplicantCommand, ApproveApplicantResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    private static readonly IReadOnlyList<string> NextSteps = new[]
    {
        "Send broker agreement to family",
        "Send community agreement to family",
        "Collect housing preferences",
        "Begin property matching"
    };

    public async Task<ApproveApplicantResponse> Handle(
        ApproveApplicantCommand request, 
        CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicantId, cancellationToken);

        if (applicant is null)
            throw new NotFoundException(nameof(Applicant), request.ApplicantId);

        var housingSearch = applicant.HousingSearch 
            ?? throw new InvalidOperationException("Applicant has no HousingSearch");

        // Validate board decision is Approved
        if (applicant.BoardReview.Decision != BoardDecision.Approved)
            throw new ValidationException(
                $"Cannot approve applicant. Board decision must be 'Approved' but is '{applicant.BoardReview.Decision}'");

        // Validate still in Submitted stage
        if (housingSearch.Stage != HousingSearchStage.Submitted)
            throw new ValidationException(
                $"Cannot approve applicant. Must be in 'Submitted' stage but is in '{housingSearch.Stage}'");

        // Capture previous stage
        var previousStage = housingSearch.Stage;
        var now = _timeProvider.GetUtcNow().DateTime;
        var userId = _currentUserService.UserId;

        // Transition to HouseHunting
        housingSearch.TransitionTo(HousingSearchStage.HouseHunting, userId);
        
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Future - send notification email if SendNotification is true

        return new ApproveApplicantResponse(
            ApplicantId: applicant.Id,
            HousingSearchId: housingSearch.Id,
            PreviousStage: previousStage,
            NewStage: housingSearch.Stage,
            ApprovedAt: now,
            ApprovedBy: userId,
            Notes: request.Notes,
            NextSteps: NextSteps
        );
    }
}
```

**Controller:**
```csharp
[ApiController]
[Route("api/applicants")]
[Authorize]
public class ApplicantsController : ControllerBase
{
    private readonly ISender _sender;

    // ... existing endpoints ...

    /// <summary>
    /// Set the board's decision on an application
    /// </summary>
    [HttpPut("{id:guid}/board-review")]
    [ProducesResponseType(typeof(SetBoardDecisionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SetBoardDecisionResponse>> SetBoardDecision(
        Guid id,
        [FromBody] SetBoardDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetBoardDecisionCommand(
            ApplicantId: id,
            Decision: request.Decision,
            ReviewDate: request.ReviewDate,
            Notes: request.Notes);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Approve an applicant (transitions to HouseHunting stage)
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApproveApplicantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApproveApplicantResponse>> ApproveApplicant(
        Guid id,
        [FromBody] ApproveApplicantRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new ApproveApplicantCommand(
            ApplicantId: id,
            Notes: request?.Notes,
            SendNotification: request?.SendNotification ?? false);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Reject an applicant (transitions to Rejected stage)
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(RejectApplicantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RejectApplicantResponse>> RejectApplicant(
        Guid id,
        [FromBody] RejectApplicantRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new RejectApplicantCommand(
            ApplicantId: id,
            Reason: request?.Reason,
            SendNotification: request?.SendNotification ?? false);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}
```

### Request DTOs

```csharp
public record SetBoardDecisionRequest(
    BoardDecision Decision,
    DateOnly? ReviewDate = null,
    string? Notes = null
);

public record ApproveApplicantRequest(
    string? Notes = null,
    bool SendNotification = false
);

public record RejectApplicantRequest(
    string? Reason = null,
    bool SendNotification = false
);
```

### Tests Required

**Unit Tests:**
- `ApproveApplicant_BoardDecisionApproved_TransitionsToHouseHunting`
- `ApproveApplicant_WithNotes_SavesNotes`
- `ApproveApplicant_BoardDecisionNotApproved_ThrowsValidation`
- `ApproveApplicant_NotInSubmittedStage_ThrowsValidation`
- `ApproveApplicant_NotFound_ThrowsNotFoundException`
- `ApproveApplicant_ReturnsNextSteps`

**Integration Tests:**
- `POST_Approve_ValidRequest_Returns200`
- `POST_Approve_BoardNotApproved_Returns400`
- `POST_Approve_NotInSubmittedStage_Returns400`
- `POST_Approve_NotAuthenticated_Returns401`

---

## FILE STRUCTURE

```
src/
├── FamilyHousing.Application/
│   └── Applicants/
│       └── Commands/
│           ├── SetBoardDecision/
│           │   ├── SetBoardDecisionCommand.cs
│           │   ├── SetBoardDecisionCommandHandler.cs
│           │   └── SetBoardDecisionCommandValidator.cs
│           ├── ApproveApplicant/
│           │   ├── ApproveApplicantCommand.cs
│           │   ├── ApproveApplicantCommandHandler.cs
│           │   └── ApproveApplicantCommandValidator.cs
│           └── RejectApplicant/
│               ├── RejectApplicantCommand.cs
│               ├── RejectApplicantCommandHandler.cs
│               └── RejectApplicantCommandValidator.cs
│
└── tests/
    ├── FamilyHousing.Application.Tests/
    │   └── Applicants/
    │       └── Commands/
    │           ├── SetBoardDecisionCommandHandlerTests.cs
    │           ├── ApproveApplicantCommandHandlerTests.cs
    │           └── RejectApplicantCommandHandlerTests.cs
    │
    └── FamilyHousing.Api.Tests/
        └── Applicants/
            └── BoardReviewEndpointTests.cs
```

---

## IMPACT ON SPRINT 2

If adding these stories to Sprint 2:

### Updated Sprint 2 Points

| Category | Original | Added | New Total |
|----------|----------|-------|-----------|
| Backend | 17 | 13 | 30 |
| Frontend | 17 | 0 | 17 |
| **Total** | **34** | **13** | **47** |

### Considerations

1. **Feasibility:** 47 points is higher than typical 2-week sprint, but these are straightforward CRUD operations building on existing patterns
2. **Dependencies:** These stories depend on US-010 (HousingSearch creation) which is already in Sprint 2
3. **Value:** Completes the core workflow - without board approval, the pipeline is incomplete
4. **Alternative:** Could split Sprint 2 into Sprint 2A (current) and Sprint 2B (board review)

### Recommendation

**Option A: Pull into Sprint 2** if team capacity allows (47 points)
- Completes core workflow in one sprint
- Avoids half-finished pipeline

**Option B: Create Sprint 2.5** (1 week mini-sprint)
- Current Sprint 2: 34 points
- Sprint 2.5: 13 points (board review)
- Less pressure, same outcome

---

## SUMMARY

| ID | Story | Points | API Endpoint |
|----|-------|--------|--------------|
| US-019 | Board reviews application | 5 | `PUT /api/applicants/{id}/board-review` |
| US-020 | Auto-move to Rejected | 3 | `POST /api/applicants/{id}/reject` |
| US-021 | Approve and transition | 5 | `POST /api/applicants/{id}/approve` |

These three endpoints complete the board review workflow and allow the pipeline to function end-to-end for the core approval/rejection flow.
