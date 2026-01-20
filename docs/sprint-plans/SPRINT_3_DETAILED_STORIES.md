# SPRINT 3 DETAILED USER STORIES
## Family Relocation System - Board Review Workflow, Public Application & CRUD Completion

**Sprint Duration:** 2 weeks
**Sprint Goal:** Public application form, pipeline drag-drop UX with document signing, S3 uploads, and edit applicant UI
**Total Points:** ~34 points (10 Backend + 24 Frontend)
**Prerequisites:** Sprint 2 complete (React app, Pipeline UI, Audit logs)

---

## 📋 SPRINT 3 OVERVIEW

### Stories in This Sprint

#### Backend Stories (10 points)

| ID | Story | Points | Status |
|----|-------|--------|--------|
| US-015 | Stage change API | - | ✅ **ALREADY EXISTS** (`PUT /api/applicants/{id}/stage`) |
| US-016 | Update preferences API | - | ✅ **ALREADY EXISTS** (`PUT /api/applicants/{id}/preferences`) |
| US-019 | Set board decision | - | ✅ **ALREADY EXISTS** (`PUT /api/applicants/{id}/board-review`) |
| US-020 | Reject applicant | - | ✅ **HANDLED BY** board-review (auto-transitions) |
| US-021 | Approve applicant | - | ✅ **HANDLED BY** board-review (auto-transitions) |
| - | Sign agreements API | - | ✅ **ALREADY EXISTS** (`POST /api/applicants/{id}/agreements`) |
| US-022 | Delete applicant (soft delete) | 2 | 🆕 **NEW** |
| US-023 | S3 bucket setup + document upload API | 5 | 🆕 **NEW** |
| - | Minor API fixes/validation | 3 | 🔧 Buffer for any issues |

#### Frontend Stories (24 points)

| ID | Story | Points | Epic |
|----|-------|--------|------|
| US-F07 | Edit applicant form/modal | 5 | Applicant CRUD |
| US-F08 | Board review UI on detail page | 3 | Board Review |
| US-F09 | Pipeline drag-drop with transition modals | 5 | Application Management |
| US-F10 | Public application page (no auth) | 8 | Public Application |
| US-F11 | Document signing modal with S3 upload | 3 | Agreements |

**Total: 34 points (10 Backend + 24 Frontend)**

### 🎉 Backend Already Complete!

Great news - Sprint 1 and Sprint 2 already implemented most backend functionality:

```
✅ PUT  /api/applicants/{id}/stage        - Change stage with contract/closing info
✅ PUT  /api/applicants/{id}/board-review - Set decision, auto-transitions stage
✅ PUT  /api/applicants/{id}/preferences  - Update housing preferences
✅ POST /api/applicants/{id}/agreements   - Sign broker/community agreements
✅ PUT  /api/applicants/{id}              - Update applicant info
✅ POST /api/applicants                   - Create (AllowAnonymous for public form)
```

**Sprint 3 is primarily frontend work!**

---

## 🔧 TECHNICAL CONTEXT (From Sprint 2)

### Current State

1. **Frontend Complete:**
   - Login page with Cognito auth
   - Applicant list with search/filter
   - Applicant detail page (read-only)
   - Pipeline Kanban board (UI only, drag-drop not wired)

2. **Backend Complete:**
   - Applicant CRUD (Create, Read, Update, List)
   - HousingSearch auto-creation
   - Pipeline API (GET /api/applicants/pipeline)
   - Audit log feature

3. **Missing (Frontend Only):**
   - Edit applicant form (frontend)
   - Board review UI on detail page (frontend)
   - Pipeline drag-drop wired to API (frontend)
   - Public application page for families (frontend)
   - Delete applicant API (backend - only new backend work)

### API Endpoints After Sprint 3

```
Applicants:
  POST   /api/applicants                     ✅ Sprint 1
  GET    /api/applicants                     ✅ Sprint 1
  GET    /api/applicants/{id}                ✅ Sprint 1
  PUT    /api/applicants/{id}                ✅ Sprint 1 (needs frontend)
  DELETE /api/applicants/{id}                🆕 Sprint 3
  GET    /api/applicants/pipeline            ✅ Sprint 2

Board Review:
  PUT    /api/applicants/{id}/board-review   🆕 Sprint 3
  POST   /api/applicants/{id}/approve        🆕 Sprint 3
  POST   /api/applicants/{id}/reject         🆕 Sprint 3

Housing Search:
  PUT    /api/applicants/{id}/stage          🆕 Sprint 3
  PUT    /api/applicants/{id}/preferences    🆕 Sprint 3
```

---

# PART 1: BACKEND STORIES

---

## US-015: Wire Up Stage Change API

### Story

**As a** coordinator
**I want to** change an applicant's housing search stage via API
**So that** the pipeline drag-drop works

**Priority:** P0
**Effort:** 2 points
**Sprint:** 3 (carry-over from Sprint 2)

### Background

The Pipeline Kanban UI is built but drag-drop doesn't call the API. Need a simple endpoint to change stage.

### Acceptance Criteria

1. `PUT /api/applicants/{id}/stage` changes HousingSearch stage
2. Validates stage transition rules (can't skip stages arbitrarily)
3. Board approval required to move from Submitted → HouseHunting
4. Returns updated applicant with new stage
5. Creates audit log entry

### API Specification

**Endpoint:** `PUT /api/applicants/{id}/stage`

**Request Body:**
```json
{
  "newStage": "HouseHunting"
}
```

**Response (200 OK):**
```json
{
  "applicantId": "guid",
  "previousStage": "Submitted",
  "newStage": "HouseHunting",
  "stageChangedAt": "2026-01-20T10:00:00Z"
}
```

**Validation Rules:**
- Submitted → HouseHunting: Requires BoardDecision = Approved
- Submitted → Rejected: Requires BoardDecision = Rejected (or use /reject endpoint)
- HouseHunting → UnderContract: Allowed
- HouseHunting → Paused: Allowed
- UnderContract → Closed: Allowed
- UnderContract → HouseHunting: Allowed (contract fell through)
- Any → Paused: Allowed
- Paused → Previous stage: Allowed

### Technical Implementation

```csharp
public record ChangeStageCommand(
    Guid ApplicantId,
    HousingSearchStage NewStage
) : IRequest<ChangeStageResponse>;

public class ChangeStageCommandHandler : IRequestHandler<ChangeStageCommand, ChangeStageResponse>
{
    public async Task<ChangeStageResponse> Handle(ChangeStageCommand request, CancellationToken ct)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicantId, ct)
            ?? throw new NotFoundException(nameof(Applicant), request.ApplicantId);

        var housingSearch = applicant.HousingSearch
            ?? throw new InvalidOperationException("Applicant has no HousingSearch");

        var previousStage = housingSearch.Stage;

        // Special validation for Submitted → HouseHunting
        if (previousStage == HousingSearchStage.Submitted &&
            request.NewStage == HousingSearchStage.HouseHunting)
        {
            if (applicant.BoardReview?.Decision != BoardDecision.Approved)
                throw new ValidationException("Board approval required to move to House Hunting");
        }

        housingSearch.TransitionTo(request.NewStage, _currentUserService.UserId);
        await _context.SaveChangesAsync(ct);

        return new ChangeStageResponse(
            applicant.Id, previousStage, housingSearch.Stage, DateTime.UtcNow);
    }
}
```

### Tests Required

- `ChangeStage_ValidTransition_UpdatesStage`
- `ChangeStage_SubmittedToHouseHunting_RequiresBoardApproval`
- `ChangeStage_InvalidTransition_ThrowsValidation`
- Integration: `PUT_Stage_Returns200`

---

## US-016: Update Housing Preferences API

### Story

**As a** coordinator
**I want to** update a family's housing preferences
**So that** I can record their needs for property matching

**Priority:** P0
**Effort:** 3 points
**Sprint:** 3

### Acceptance Criteria

1. `PUT /api/applicants/{id}/preferences` updates HousingPreferences
2. Can update: budget, bedrooms, bathrooms, required features, move timeline, shul proximity
3. Validates budget is positive
4. Creates audit log entry

### API Specification

**Endpoint:** `PUT /api/applicants/{id}/preferences`

**Request Body:**
```json
{
  "budgetAmount": 650000,
  "minBedrooms": 4,
  "minBathrooms": 2,
  "requiredFeatures": ["Garage", "Finished Basement", "Central Air"],
  "moveTimeline": "ShortTerm",
  "shulProximity": {
    "maxWalkingMinutes": 15,
    "preferredShuls": ["Shul A", "Shul B"]
  }
}
```

**Response (200 OK):**
```json
{
  "applicantId": "guid",
  "housingSearchId": "guid",
  "preferences": {
    "budgetAmount": 650000,
    "minBedrooms": 4,
    "minBathrooms": 2,
    "requiredFeatures": ["Garage", "Finished Basement", "Central Air"],
    "moveTimeline": "ShortTerm",
    "shulProximity": { ... }
  },
  "updatedAt": "2026-01-20T10:00:00Z"
}
```

### Technical Implementation

```csharp
public record UpdatePreferencesCommand(
    Guid ApplicantId,
    HousingPreferencesDto Preferences
) : IRequest<UpdatePreferencesResponse>;

public class UpdatePreferencesCommandHandler
    : IRequestHandler<UpdatePreferencesCommand, UpdatePreferencesResponse>
{
    public async Task<UpdatePreferencesResponse> Handle(
        UpdatePreferencesCommand request, CancellationToken ct)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearch)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicantId, ct)
            ?? throw new NotFoundException(nameof(Applicant), request.ApplicantId);

        var housingSearch = applicant.HousingSearch
            ?? throw new InvalidOperationException("Applicant has no HousingSearch");

        var preferences = request.Preferences.ToDomain();
        housingSearch.UpdatePreferences(preferences, _currentUserService.UserId);

        await _context.SaveChangesAsync(ct);

        return new UpdatePreferencesResponse(
            applicant.Id,
            housingSearch.Id,
            housingSearch.Preferences!.ToDto(),
            DateTime.UtcNow);
    }
}
```

---

## US-019: Set Board Decision

### Story

**As a** coordinator or board member
**I want to** record the board's decision on an applicant
**So that** the decision is tracked and next steps can be taken

**Priority:** P0
**Effort:** 5 points
**Sprint:** 3

### Acceptance Criteria

1. `PUT /api/applicants/{id}/board-review` sets board decision
2. Decision options: Pending, Approved, Rejected, Deferred
3. Can set review date (defaults to today)
4. Can add notes
5. Only works for applicants in Submitted stage
6. Creates audit log entry

### API Specification

**Endpoint:** `PUT /api/applicants/{id}/board-review`

**Request Body:**
```json
{
  "decision": "Approved",
  "reviewDate": "2026-01-15",
  "notes": "Strong references from Rabbi Cohen. Unanimous approval."
}
```

**Response (200 OK):**
```json
{
  "applicantId": "guid",
  "boardReview": {
    "decision": "Approved",
    "reviewDate": "2026-01-15",
    "notes": "Strong references from Rabbi Cohen."
  },
  "currentStage": "Submitted",
  "nextSteps": "Use POST /api/applicants/{id}/approve to transition to House Hunting"
}
```

### Technical Implementation

See BOARD_REVIEW_STORIES_US019_020_021.md for full implementation details.

**Key Domain Method:**
```csharp
public class Applicant
{
    public void SetBoardDecision(
        BoardDecision decision,
        DateOnly reviewDate,
        string? notes,
        Guid reviewedBy)
    {
        if (HousingSearch?.Stage != HousingSearchStage.Submitted)
            throw new InvalidOperationException("Can only set board decision for Submitted applicants");

        BoardReview = new BoardReview(decision, reviewDate, notes, reviewedBy);
        ModifiedBy = reviewedBy;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new BoardDecisionSetEvent(Id, decision));
    }
}
```

---

## US-020: Reject Applicant

### Story

**As a** coordinator
**I want to** reject an applicant after board rejection
**So that** they move to the Rejected stage

**Priority:** P0
**Effort:** 3 points
**Sprint:** 3

### Acceptance Criteria

1. `POST /api/applicants/{id}/reject` transitions to Rejected stage
2. Only works if BoardDecision = Rejected
3. Can include rejection reason
4. Creates audit log entry

### API Specification

**Endpoint:** `POST /api/applicants/{id}/reject`

**Request Body (optional):**
```json
{
  "reason": "Did not meet community requirements"
}
```

**Response (200 OK):**
```json
{
  "applicantId": "guid",
  "previousStage": "Submitted",
  "newStage": "Rejected",
  "rejectedAt": "2026-01-20T14:30:00Z"
}
```

---

## US-021: Approve Applicant

### Story

**As a** coordinator
**I want to** approve an applicant after board approval
**So that** they move to House Hunting stage

**Priority:** P0
**Effort:** 5 points
**Sprint:** 3

### Acceptance Criteria

1. `POST /api/applicants/{id}/approve` transitions to HouseHunting stage
2. Only works if BoardDecision = Approved
3. Returns next steps guidance
4. Creates audit log entry

### API Specification

**Endpoint:** `POST /api/applicants/{id}/approve`

**Request Body (optional):**
```json
{
  "notes": "Welcome to the program!"
}
```

**Response (200 OK):**
```json
{
  "applicantId": "guid",
  "previousStage": "Submitted",
  "newStage": "HouseHunting",
  "approvedAt": "2026-01-20T14:30:00Z",
  "nextSteps": [
    "Send broker agreement to family",
    "Send community agreement to family",
    "Collect housing preferences"
  ]
}
```

---

## US-022: Delete Applicant (Soft Delete)

### Story

**As a** coordinator
**I want to** delete an applicant
**So that** I can remove test data or duplicates

**Priority:** P1
**Effort:** 2 points
**Sprint:** 3

### Acceptance Criteria

1. `DELETE /api/applicants/{id}` soft-deletes the applicant
2. Sets IsDeleted = true (don't actually remove from DB)
3. Applicant no longer appears in list/pipeline queries
4. Creates audit log entry
5. Requires authentication

### API Specification

**Endpoint:** `DELETE /api/applicants/{id}`

**Response (204 No Content)**

### Technical Implementation

```csharp
public record DeleteApplicantCommand(Guid ApplicantId) : IRequest;

public class DeleteApplicantCommandHandler : IRequestHandler<DeleteApplicantCommand>
{
    public async Task Handle(DeleteApplicantCommand request, CancellationToken ct)
    {
        var applicant = await _context.Set<Applicant>()
            .FirstOrDefaultAsync(a => a.Id == request.ApplicantId, ct)
            ?? throw new NotFoundException(nameof(Applicant), request.ApplicantId);

        applicant.Delete(_currentUserService.UserId);
        await _context.SaveChangesAsync(ct);
    }
}
```

**Domain Method:**
```csharp
public class Applicant
{
    public bool IsDeleted { get; private set; }

    public void Delete(Guid deletedBy)
    {
        IsDeleted = true;
        ModifiedBy = deletedBy;
        ModifiedDate = DateTime.UtcNow;
    }
}
```

**Query Filter (EF Core):**
```csharp
// In ApplicantConfiguration.cs
builder.HasQueryFilter(a => !a.IsDeleted);
```

---

## US-023: S3 Bucket Setup + Document Upload API

### Story

**As a** coordinator
**I want to** upload signed agreements to cloud storage
**So that** documents are securely stored and accessible

**Priority:** P0
**Effort:** 5 points
**Sprint:** 3

### Background

The agreements API (`POST /api/applicants/{id}/agreements`) expects a document URL. We need S3 infrastructure to store uploaded documents and an API endpoint to handle uploads.

### Acceptance Criteria

1. S3 bucket created for document storage
2. Bucket configured with proper security (private, signed URLs for access)
3. `POST /api/documents/upload` endpoint accepts file uploads
4. Returns S3 URL for the uploaded document
5. Supports PDF and image files (jpg, png)
6. File size limit: 10MB
7. Files organized by applicant ID: `documents/{applicantId}/{type}/{filename}`
8. Requires authentication

### AWS S3 Configuration

**Bucket Name:** `family-relocation-documents-{environment}`

**Bucket Structure:**
```
family-relocation-documents-dev/
├── documents/
│   └── {applicantId}/
│       ├── broker-agreement/
│       │   └── broker-agreement-2026-01-18.pdf
│       └── community-takanos/
│           └── takanos-signed-2026-01-18.pdf
```

**Bucket Policy:**
- Private bucket (no public access)
- IAM role for API server with PutObject, GetObject permissions
- Pre-signed URLs for temporary access (1 hour expiry)

### API Specification

**Endpoint:** `POST /api/documents/upload`

**Request:** `multipart/form-data`
```
file: <binary>
applicantId: guid
documentType: "BrokerAgreement" | "CommunityTakanos" | "Other"
```

**Response (200 OK):**
```json
{
  "documentUrl": "https://family-relocation-documents-dev.s3.amazonaws.com/documents/abc123/broker-agreement/file.pdf",
  "fileName": "broker-agreement-2026-01-18.pdf",
  "fileSize": 245678,
  "contentType": "application/pdf",
  "uploadedAt": "2026-01-18T14:30:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid file type or size exceeded
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - Applicant doesn't exist

### Technical Implementation

**Infrastructure Setup (Terraform/CloudFormation or Manual):**
```hcl
resource "aws_s3_bucket" "documents" {
  bucket = "family-relocation-documents-${var.environment}"
}

resource "aws_s3_bucket_public_access_block" "documents" {
  bucket = aws_s3_bucket.documents.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}
```

**appsettings.json:**
```json
{
  "AWS": {
    "S3": {
      "BucketName": "family-relocation-documents-dev",
      "Region": "us-east-1"
    }
  }
}
```

**Service Interface:**
```csharp
// Application/Common/Interfaces/IDocumentStorageService.cs
public interface IDocumentStorageService
{
    Task<DocumentUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid applicantId,
        string documentType,
        CancellationToken cancellationToken = default);

    Task<string> GetPreSignedUrlAsync(
        string documentUrl,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}

public record DocumentUploadResult(
    string DocumentUrl,
    string FileName,
    long FileSize,
    string ContentType,
    DateTime UploadedAt);
```

**Infrastructure Implementation:**
```csharp
// Infrastructure/AWS/S3DocumentStorageService.cs
public class S3DocumentStorageService : IDocumentStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public async Task<DocumentUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid applicantId,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        var key = $"documents/{applicantId}/{documentType.ToLower()}/{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        var url = $"https://{_bucketName}.s3.amazonaws.com/{key}";

        return new DocumentUploadResult(
            url, fileName, fileStream.Length, contentType, DateTime.UtcNow);
    }

    public async Task<string> GetPreSignedUrlAsync(
        string documentUrl,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var key = ExtractKeyFromUrl(documentUrl);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry),
        };

        return _s3Client.GetPreSignedURL(request);
    }
}
```

**Controller:**
```csharp
// API/Controllers/DocumentsController.cs
[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentStorageService _storageService;
    private readonly IApplicationDbContext _context;
    private static readonly string[] AllowedContentTypes =
        ["application/pdf", "image/jpeg", "image/png"];
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] Guid applicantId,
        [FromForm] string documentType)
    {
        // Validate applicant exists
        var applicantExists = await _context.Set<Applicant>()
            .AnyAsync(a => a.Id == applicantId);
        if (!applicantExists)
            return NotFound(new { message = "Applicant not found" });

        // Validate file
        if (file.Length == 0)
            return BadRequest(new { message = "File is empty" });
        if (file.Length > MaxFileSize)
            return BadRequest(new { message = "File exceeds 10MB limit" });
        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Only PDF and image files allowed" });

        // Upload to S3
        await using var stream = file.OpenReadStream();
        var result = await _storageService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            applicantId,
            documentType);

        return Ok(result);
    }
}
```

### Tests Required

- `Upload_ValidPdf_ReturnsUrl`
- `Upload_ValidImage_ReturnsUrl`
- `Upload_TooLarge_Returns400`
- `Upload_InvalidType_Returns400`
- `Upload_ApplicantNotFound_Returns404`
- `Upload_NotAuthenticated_Returns401`

---

# PART 2: FRONTEND STORIES

---

## US-F07: Edit Applicant Form/Modal

### Story

**As a** coordinator
**I want to** edit an applicant's information
**So that** I can correct mistakes or update details

**Priority:** P0
**Effort:** 5 points
**Sprint:** 3

### Acceptance Criteria

1. Edit button on applicant detail page opens edit modal/drawer
2. Can edit: husband info, wife info, address, children, community info
3. Form pre-populated with current data
4. Validation matches create form
5. Save calls `PUT /api/applicants/{id}`
6. Success shows toast and refreshes detail page

### Technical Implementation

**Components:**
- `EditApplicantDrawer.tsx` - Ant Design Drawer with form
- Uses same form fields as create (if exists) or detail page

**Key Code:**
```tsx
// features/applicants/EditApplicantDrawer.tsx
import { Drawer, Form, Input, Button, message } from 'antd';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { applicantsApi } from '../../api';

interface EditApplicantDrawerProps {
  applicant: ApplicantDto;
  open: boolean;
  onClose: () => void;
}

const EditApplicantDrawer = ({ applicant, open, onClose }: EditApplicantDrawerProps) => {
  const [form] = Form.useForm();
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (data: UpdateApplicantRequest) =>
      applicantsApi.update(applicant.id, data),
    onSuccess: () => {
      message.success('Applicant updated successfully');
      queryClient.invalidateQueries({ queryKey: ['applicant', applicant.id] });
      onClose();
    },
    onError: (err) => {
      message.error('Failed to update applicant');
    },
  });

  const handleSubmit = (values: any) => {
    mutation.mutate(values);
  };

  return (
    <Drawer
      title="Edit Applicant"
      open={open}
      onClose={onClose}
      width={600}
      footer={
        <Button type="primary" onClick={() => form.submit()} loading={mutation.isPending}>
          Save Changes
        </Button>
      }
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={applicant}
        onFinish={handleSubmit}
      >
        {/* Form fields for husband, wife, address, etc. */}
      </Form>
    </Drawer>
  );
};
```

---

## US-F08: Board Review UI on Detail Page

### Story

**As a** coordinator
**I want to** set board decisions from the applicant detail page
**So that** I can manage the approval process

**Priority:** P0
**Effort:** 3 points
**Sprint:** 3

### Acceptance Criteria

1. Board Review section on applicant detail page
2. Shows current decision, review date, notes
3. "Record Decision" button opens modal (if Pending/Deferred)
4. Decision dropdown: Approved, Rejected, Deferred
5. Date picker for review date
6. Notes textarea
7. After decision set, show Approve/Reject action buttons
8. Approve button calls `POST /api/applicants/{id}/approve`
9. Reject button calls `POST /api/applicants/{id}/reject`

### UI Mockup

```
┌─────────────────────────────────────────────────────────────┐
│ BOARD REVIEW                                                │
├─────────────────────────────────────────────────────────────┤
│ Decision:     [Approved]  ✅                                │
│ Review Date:  January 15, 2026                              │
│ Notes:        Strong references from Rabbi Cohen            │
│                                                             │
│ [ Move to House Hunting ]  [ View Notes ]                   │
└─────────────────────────────────────────────────────────────┘
```

**For Pending:**
```
┌─────────────────────────────────────────────────────────────┐
│ BOARD REVIEW                                                │
├─────────────────────────────────────────────────────────────┤
│ Decision:     Pending ⏳                                    │
│                                                             │
│ [ Record Board Decision ]                                   │
└─────────────────────────────────────────────────────────────┘
```

---

## US-F09: Pipeline Drag-Drop Wired to API

### Story

**As a** coordinator
**I want** the pipeline drag-drop to actually change stages
**So that** I can manage workflow visually

**Priority:** P0
**Effort:** 5 points (increased due to modal handling)
**Sprint:** 3

### Acceptance Criteria

1. Dragging card to new column calls `PUT /api/applicants/{id}/stage`
2. Optimistic update (move card immediately, revert on error)
3. **Modal popups for invalid transitions** (not just error toasts)
4. Success updates card position permanently
5. Special handling for Submitted → HouseHunting (needs board approval)

### Transition Error Handling with Modals

Instead of just showing error toasts, we show contextual modals that help users fix the issue:

**Scenario 1: Submitted → HouseHunting (No Board Approval)**
```
┌─────────────────────────────────────────────────────────────┐
│  ⚠️ Board Approval Required                                 │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  The Cohen family cannot move to House Hunting yet.         │
│  The board must approve this applicant first.               │
│                                                             │
│  Current Board Decision: Pending                            │
│                                                             │
│  What would you like to do?                                 │
│                                                             │
│  [ Set Board Decision ]     [ Cancel ]                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```
- "Set Board Decision" opens the board review modal inline
- After setting decision to "Approved", automatically moves to HouseHunting

**Scenario 2: Submitted → HouseHunting (Board Rejected)**
```
┌─────────────────────────────────────────────────────────────┐
│  ❌ Cannot Move to House Hunting                            │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  The Cohen family was rejected by the board.                │
│  They cannot proceed to House Hunting.                      │
│                                                             │
│  Board Decision: Rejected                                   │
│  Review Date: January 15, 2026                              │
│  Notes: Did not meet requirements                           │
│                                                             │
│  [ Move to Rejected Column ]     [ Cancel ]                 │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Scenario 3: HouseHunting → UnderContract (No Contract Info)**
```
┌─────────────────────────────────────────────────────────────┐
│  📝 Contract Information Required                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  To move to Under Contract, please enter contract details:  │
│                                                             │
│  Contract Price *     [$________________]                   │
│  Contract Date *      [📅 Select date    ]                  │
│  Expected Closing     [📅 Select date    ]                  │
│                                                             │
│  [ Save & Move ]              [ Cancel ]                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Scenario 4: UnderContract → Closed**
```
┌─────────────────────────────────────────────────────────────┐
│  🏠 Confirm Closing                                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Congratulations! The Cohen family is closing.              │
│                                                             │
│  Contract Price: $625,000                                   │
│  Contract Date: January 10, 2026                            │
│                                                             │
│  Actual Closing Date * [📅 Select date    ]                 │
│                                                             │
│  [ Confirm Closing ]          [ Cancel ]                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Scenario 5: UnderContract → HouseHunting (Contract Fell Through)**
```
┌─────────────────────────────────────────────────────────────┐
│  😔 Contract Failed                                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Record why the contract fell through:                      │
│                                                             │
│  Reason *                                                   │
│  [▼ Select reason_________________________]                 │
│    • Financing fell through                                 │
│    • Inspection issues                                      │
│    • Seller backed out                                      │
│    • Buyer changed mind                                     │
│    • Other                                                  │
│                                                             │
│  Notes (optional)                                           │
│  [________________________________]                         │
│                                                             │
│  [ Save & Return to Hunting ]     [ Cancel ]                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```
- This increments `failedContractCount` and adds to `failedContracts` list

**Scenario 6: BoardApproved → HouseHunting (Documents Not Signed)**
```
┌─────────────────────────────────────────────────────────────┐
│  📄 Agreements Required                                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Before house hunting can begin, the following agreements   │
│  must be signed:                                            │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Broker Agreement                           ❌ Missing │   │
│  │ Agreement to work with community broker              │   │
│  │                                                      │   │
│  │ [ 📤 Upload Signed Document ]                        │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Community Takanos                          ❌ Missing │   │
│  │ Community guidelines and expectations                │   │
│  │                                                      │   │
│  │ [ 📤 Upload Signed Document ]                        │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  [ Cancel ]                    [ Continue Without ] (admin) │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**After uploading:**
```
┌─────────────────────────────────────────────────────────────┐
│  📄 Agreements Required                                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Broker Agreement                           ✅ Signed  │   │
│  │ Signed: January 18, 2026                             │   │
│  │ [ 👁️ View ] [ 🔄 Replace ]                            │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Community Takanos                          ✅ Signed  │   │
│  │ Signed: January 18, 2026                             │   │
│  │ [ 👁️ View ] [ 🔄 Replace ]                            │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  [ Cancel ]                         [ Move to House Hunting ]│
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

- Upload goes to S3, returns URL
- URL is saved via `POST /api/applicants/{id}/agreements`
- Both agreements required before "Move to House Hunting" is enabled

### Technical Implementation

**Component Structure:**
```
src/features/pipeline/
├── PipelinePage.tsx
├── PipelinePage.css
└── modals/
    ├── BoardApprovalRequiredModal.tsx
    ├── ContractInfoModal.tsx
    ├── ClosingConfirmModal.tsx
    ├── ContractFailedModal.tsx
    ├── AgreementsRequiredModal.tsx    (NEW - document signing)
    └── TransitionBlockedModal.tsx  (generic fallback)
```

**Transition Rules (Client-Side):**
```tsx
// features/pipeline/transitionRules.ts

type TransitionCheck = {
  allowed: boolean;
  modal?: 'boardRequired' | 'agreementsRequired' | 'contractInfo' | 'closing' | 'contractFailed' | 'blocked';
  message?: string;
};

export const checkTransition = (
  applicant: PipelineApplicant,
  fromStage: string,
  toStage: string
): TransitionCheck => {
  // Submitted → HouseHunting: Need board approval
  if (fromStage === 'Submitted' && toStage === 'HouseHunting') {
    if (applicant.boardDecision === 'Pending' || applicant.boardDecision === 'Deferred') {
      return { allowed: false, modal: 'boardRequired' };
    }
    if (applicant.boardDecision === 'Rejected') {
      return { allowed: false, modal: 'blocked', message: 'Board rejected this applicant' };
    }
    return { allowed: true };
  }

  // HouseHunting → UnderContract: Need contract info
  if (fromStage === 'HouseHunting' && toStage === 'UnderContract') {
    return { allowed: false, modal: 'contractInfo' };
  }

  // UnderContract → Closed: Need closing date
  if (fromStage === 'UnderContract' && toStage === 'Closed') {
    return { allowed: false, modal: 'closing' };
  }

  // UnderContract → HouseHunting: Contract fell through
  if (fromStage === 'UnderContract' && toStage === 'HouseHunting') {
    return { allowed: false, modal: 'contractFailed' };
  }

  // Any → Paused: Always allowed
  if (toStage === 'Paused') {
    return { allowed: true };
  }

  // Paused → Previous: Always allowed
  if (fromStage === 'Paused') {
    return { allowed: true };
  }

  // Default: Check with API (let server validate)
  return { allowed: true };
};
```

**Main Handler:**
```tsx
// In PipelinePage.tsx
const [modalState, setModalState] = useState<{
  type: string | null;
  applicant: PipelineApplicant | null;
  targetStage: string | null;
}>({ type: null, applicant: null, targetStage: null });

const handleDragEnd = (result: DropResult) => {
  if (!result.destination) return;

  const applicantId = result.draggableId;
  const newStage = result.destination.droppableId;
  const applicant = findApplicant(applicantId);
  const currentStage = result.source.droppableId;

  if (currentStage === newStage) return;

  // Check transition rules
  const check = checkTransition(applicant, currentStage, newStage);

  if (!check.allowed && check.modal) {
    // Show modal instead of making API call
    setModalState({
      type: check.modal,
      applicant,
      targetStage: newStage,
    });
    return;
  }

  // Allowed transition - make API call
  performStageChange(applicantId, newStage);
};

const performStageChange = (applicantId: string, newStage: string) => {
  // Optimistic update
  updateLocalState(applicantId, newStage);

  stageMutation.mutate({ applicantId, newStage });
};

const handleModalSuccess = () => {
  // Modal completed successfully (e.g., board decision set)
  // Refresh pipeline data
  queryClient.invalidateQueries({ queryKey: ['pipeline'] });
  setModalState({ type: null, applicant: null, targetStage: null });
};

const handleModalCancel = () => {
  setModalState({ type: null, applicant: null, targetStage: null });
};

// Render modals
return (
  <>
    {/* Pipeline UI */}

    <BoardApprovalRequiredModal
      open={modalState.type === 'boardRequired'}
      applicant={modalState.applicant}
      onSuccess={handleModalSuccess}
      onCancel={handleModalCancel}
    />

    <ContractInfoModal
      open={modalState.type === 'contractInfo'}
      applicant={modalState.applicant}
      onSuccess={handleModalSuccess}
      onCancel={handleModalCancel}
    />

    <ClosingConfirmModal
      open={modalState.type === 'closing'}
      applicant={modalState.applicant}
      onSuccess={handleModalSuccess}
      onCancel={handleModalCancel}
    />

    <ContractFailedModal
      open={modalState.type === 'contractFailed'}
      applicant={modalState.applicant}
      onSuccess={handleModalSuccess}
      onCancel={handleModalCancel}
    />

    <TransitionBlockedModal
      open={modalState.type === 'blocked'}
      message={modalState.message}
      onClose={handleModalCancel}
    />
  </>
);
```

**Board Approval Required Modal:**
```tsx
// features/pipeline/modals/BoardApprovalRequiredModal.tsx
import { Modal, Form, Select, Input, DatePicker, Button, message } from 'antd';
import { useMutation } from '@tanstack/react-query';

const BoardApprovalRequiredModal = ({ open, applicant, onSuccess, onCancel }) => {
  const [form] = Form.useForm();

  const setDecisionMutation = useMutation({
    mutationFn: (data) => applicantsApi.setBoardDecision(applicant.id, data),
  });

  const approveMutation = useMutation({
    mutationFn: () => applicantsApi.approve(applicant.id),
  });

  const handleSetDecision = async (values) => {
    try {
      await setDecisionMutation.mutateAsync(values);

      if (values.decision === 'Approved') {
        // Auto-approve to move to HouseHunting
        await approveMutation.mutateAsync();
        message.success(`${applicant.familyName} moved to House Hunting`);
      } else {
        message.success('Board decision recorded');
      }

      onSuccess();
    } catch (err) {
      message.error('Failed to set board decision');
    }
  };

  return (
    <Modal
      title="⚠️ Board Approval Required"
      open={open}
      onCancel={onCancel}
      footer={null}
      width={500}
    >
      <p>
        The <strong>{applicant?.familyName}</strong> family cannot move to House Hunting yet.
        The board must approve this applicant first.
      </p>

      <p><strong>Current Board Decision:</strong> {applicant?.boardDecision || 'Pending'}</p>

      <Form form={form} layout="vertical" onFinish={handleSetDecision}>
        <Form.Item
          name="decision"
          label="Board Decision"
          rules={[{ required: true }]}
        >
          <Select>
            <Select.Option value="Approved">Approved</Select.Option>
            <Select.Option value="Rejected">Rejected</Select.Option>
            <Select.Option value="Deferred">Deferred</Select.Option>
          </Select>
        </Form.Item>

        <Form.Item name="reviewDate" label="Review Date">
          <DatePicker style={{ width: '100%' }} />
        </Form.Item>

        <Form.Item name="notes" label="Notes">
          <Input.TextArea rows={3} placeholder="Board meeting notes..." />
        </Form.Item>

        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
          <Button onClick={onCancel}>Cancel</Button>
          <Button
            type="primary"
            htmlType="submit"
            loading={setDecisionMutation.isPending || approveMutation.isPending}
          >
            Set Decision
          </Button>
        </div>
      </Form>
    </Modal>
  );
};
```

---

## US-F10: Public Application Page (No Auth Required)

### Story

**As a** prospective family
**I want to** apply online without creating an account
**So that** I can submit my application easily

**Priority:** P0
**Effort:** 8 points
**Sprint:** 3

### Background

Families should be able to apply to the relocation program from a public URL without logging in. The backend already supports this - `POST /api/applicants` is `[AllowAnonymous]`. This is a multi-step form capturing family information and housing preferences.

### Acceptance Criteria

1. Public route `/apply` accessible without authentication
2. Multi-step form with progress indicator
3. Step 1: Husband information (name, email, phone, occupation)
4. Step 2: Wife information (name, maiden name, email, phone)
5. Step 3: Children (add/remove dynamically)
6. Step 4: Current address and community info
7. Step 5: Housing preferences (budget, bedrooms, move timeline)
8. Step 6: Review and submit
9. Form validation on each step
10. Success page with confirmation message
11. Error handling with user-friendly messages
12. Mobile-responsive layout (families apply from phones)

### UI Design

**Progress Steps:**
```
[1. Husband] → [2. Wife] → [3. Children] → [4. Address] → [5. Preferences] → [6. Review]
```

**Page Layout:**
```
┌─────────────────────────────────────────────────────────────────┐
│  🌳 וועד הישוב                                                   │
│  Family Relocation Program                                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ○───●───○───○───○───○                                          │
│  1   2   3   4   5   6                                          │
│                                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Wife Information                                        │   │
│  │                                                          │   │
│  │  First Name *        [________________]                  │   │
│  │  Maiden Name         [________________]                  │   │
│  │  Email               [________________]                  │   │
│  │  Phone               [________________]                  │   │
│  │  Occupation          [________________]                  │   │
│  │                                                          │   │
│  │  [ ← Back ]                    [ Next → ]               │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                  │
│  Questions? Contact us at apply@vaadhayishuv.org                │
└─────────────────────────────────────────────────────────────────┘
```

**Success Page:**
```
┌─────────────────────────────────────────────────────────────────┐
│                                                                  │
│                    ✓ Application Submitted!                      │
│                                                                  │
│  Thank you for your interest in our community.                   │
│                                                                  │
│  What happens next:                                              │
│  1. Our team will review your application                        │
│  2. The board will discuss at their next meeting                 │
│  3. You'll receive an email with the decision                    │
│                                                                  │
│  Questions? Contact apply@vaadhayishuv.org                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Technical Implementation

**Route Configuration (App.tsx):**
```tsx
// Public route - no auth required
<Route path="/apply" element={<PublicApplicationPage />} />
```

**Component Structure:**
```
src/features/application/
├── PublicApplicationPage.tsx      # Main page with steps
├── PublicApplicationPage.css      # Styles
├── steps/
│   ├── HusbandInfoStep.tsx
│   ├── WifeInfoStep.tsx
│   ├── ChildrenStep.tsx
│   ├── AddressStep.tsx
│   ├── PreferencesStep.tsx
│   └── ReviewStep.tsx
└── ApplicationSuccessPage.tsx     # Success confirmation
```

**Main Component:**
```tsx
// features/application/PublicApplicationPage.tsx
import { useState } from 'react';
import { Steps, Card, Button, Form, message } from 'antd';
import { useMutation } from '@tanstack/react-query';
import { applicantsApi } from '../../api';
import { HusbandInfoStep } from './steps/HusbandInfoStep';
import { WifeInfoStep } from './steps/WifeInfoStep';
import { ChildrenStep } from './steps/ChildrenStep';
import { AddressStep } from './steps/AddressStep';
import { PreferencesStep } from './steps/PreferencesStep';
import { ReviewStep } from './steps/ReviewStep';
import './PublicApplicationPage.css';

interface ApplicationData {
  husband: HusbandInfoDto;
  wife?: SpouseInfoDto;
  children: ChildDto[];
  address: AddressDto;
  currentKehila?: string;
  shabbosShul?: string;
  preferences?: HousingPreferencesDto;
}

const PublicApplicationPage = () => {
  const [currentStep, setCurrentStep] = useState(0);
  const [formData, setFormData] = useState<Partial<ApplicationData>>({});
  const [submitted, setSubmitted] = useState(false);

  const submitMutation = useMutation({
    mutationFn: (data: CreateApplicantRequest) => applicantsApi.create(data),
    onSuccess: () => {
      setSubmitted(true);
    },
    onError: (err) => {
      message.error('Failed to submit application. Please try again.');
    },
  });

  const steps = [
    { title: 'Husband', content: <HusbandInfoStep data={formData} onNext={handleNext} /> },
    { title: 'Wife', content: <WifeInfoStep data={formData} onNext={handleNext} onBack={handleBack} /> },
    { title: 'Children', content: <ChildrenStep data={formData} onNext={handleNext} onBack={handleBack} /> },
    { title: 'Address', content: <AddressStep data={formData} onNext={handleNext} onBack={handleBack} /> },
    { title: 'Preferences', content: <PreferencesStep data={formData} onNext={handleNext} onBack={handleBack} /> },
    { title: 'Review', content: <ReviewStep data={formData} onSubmit={handleSubmit} onBack={handleBack} isLoading={submitMutation.isPending} /> },
  ];

  const handleNext = (stepData: Partial<ApplicationData>) => {
    setFormData(prev => ({ ...prev, ...stepData }));
    setCurrentStep(prev => prev + 1);
  };

  const handleBack = () => {
    setCurrentStep(prev => prev - 1);
  };

  const handleSubmit = () => {
    submitMutation.mutate(formData as CreateApplicantRequest);
  };

  if (submitted) {
    return <ApplicationSuccessPage />;
  }

  return (
    <div className="public-application-page">
      <div className="application-header">
        <div className="logo">🌳</div>
        <h1>וועד הישוב</h1>
        <p>Family Relocation Program Application</p>
      </div>

      <Card className="application-card">
        <Steps current={currentStep} items={steps.map(s => ({ title: s.title }))} />
        <div className="step-content">
          {steps[currentStep].content}
        </div>
      </Card>

      <div className="application-footer">
        <p>Questions? Contact us at apply@vaadhayishuv.org</p>
      </div>
    </div>
  );
};

export default PublicApplicationPage;
```

**Children Step (Dynamic Add/Remove):**
```tsx
// features/application/steps/ChildrenStep.tsx
import { Form, Input, Button, Select, Space, Card } from 'antd';
import { PlusOutlined, MinusCircleOutlined } from '@ant-design/icons';

const ChildrenStep = ({ data, onNext, onBack }) => {
  const [form] = Form.useForm();

  return (
    <Form
      form={form}
      layout="vertical"
      initialValues={{ children: data.children || [] }}
      onFinish={(values) => onNext({ children: values.children })}
    >
      <Form.List name="children">
        {(fields, { add, remove }) => (
          <>
            {fields.map(({ key, name, ...restField }) => (
              <Card key={key} size="small" className="child-card">
                <Space align="start">
                  <Form.Item {...restField} name={[name, 'name']} label="Name" rules={[{ required: true }]}>
                    <Input placeholder="Child's name" />
                  </Form.Item>
                  <Form.Item {...restField} name={[name, 'age']} label="Age" rules={[{ required: true }]}>
                    <Input type="number" min={0} max={25} />
                  </Form.Item>
                  <Form.Item {...restField} name={[name, 'gender']} label="Gender" rules={[{ required: true }]}>
                    <Select options={[{ value: 'Male', label: 'Male' }, { value: 'Female', label: 'Female' }]} />
                  </Form.Item>
                  <Form.Item {...restField} name={[name, 'school']} label="School">
                    <Input placeholder="Current school" />
                  </Form.Item>
                  <MinusCircleOutlined onClick={() => remove(name)} />
                </Space>
              </Card>
            ))}
            <Button type="dashed" onClick={() => add()} block icon={<PlusOutlined />}>
              Add Child
            </Button>
          </>
        )}
      </Form.List>

      <div className="step-buttons">
        <Button onClick={onBack}>Back</Button>
        <Button type="primary" htmlType="submit">Next</Button>
      </div>
    </Form>
  );
};
```

### Styling (CSS)

```css
/* features/application/PublicApplicationPage.css */
.public-application-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #f4fbf5 0%, #e8f7ea 100%);
  padding: 40px 20px;
}

.application-header {
  text-align: center;
  margin-bottom: 32px;
}

.application-header .logo {
  font-size: 48px;
  margin-bottom: 8px;
}

.application-header h1 {
  font-size: 28px;
  color: #2d7a3a;
  margin: 0;
}

.application-header p {
  color: #5c605c;
  margin-top: 4px;
}

.application-card {
  max-width: 700px;
  margin: 0 auto;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.step-content {
  margin-top: 32px;
  min-height: 300px;
}

.step-buttons {
  display: flex;
  justify-content: space-between;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid #e2e4e2;
}

.application-footer {
  text-align: center;
  margin-top: 24px;
  color: #7a7e7a;
}

.child-card {
  margin-bottom: 16px;
}

/* Mobile responsive */
@media (max-width: 600px) {
  .application-card {
    margin: 0 8px;
  }

  .child-card .ant-space {
    flex-direction: column;
    width: 100%;
  }
}
```

### Tests Required

- Visual testing of all form steps
- Form validation on each step
- Submit flow end-to-end
- Error handling on failed submission
- Mobile responsive layout check

---

## US-F11: Document Signing Modal with S3 Upload

### Story

**As a** coordinator
**I want to** upload signed agreements when moving applicants to House Hunting
**So that** document requirements are enforced and stored securely

**Priority:** P0
**Effort:** 3 points
**Sprint:** 3

### Background

When an applicant is board-approved and the coordinator tries to move them to House Hunting, they must upload signed copies of the Broker Agreement and Community Takanos. This modal integrates with the S3 upload API and agreements API.

### Acceptance Criteria

1. Modal appears when dragging from BoardApproved to HouseHunting (if agreements not signed)
2. Shows status of each agreement (Missing/Signed)
3. Upload button for each agreement type
4. Supports PDF and image files up to 10MB
5. Shows upload progress indicator
6. After upload, calls `POST /api/applicants/{id}/agreements` to record
7. View button opens document in new tab (pre-signed URL)
8. Replace button allows re-uploading
9. "Move to House Hunting" enabled only when both signed
10. Optional "Continue Without" for admin override

### Technical Implementation

```tsx
// features/pipeline/modals/AgreementsRequiredModal.tsx
import { useState } from 'react';
import { Modal, Upload, Button, message, Spin } from 'antd';
import { UploadOutlined, CheckCircleFilled, CloseCircleFilled, EyeOutlined } from '@ant-design/icons';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { documentsApi, applicantsApi } from '../../../api';

interface AgreementsRequiredModalProps {
  open: boolean;
  applicant: PipelineApplicant | null;
  onSuccess: () => void;
  onCancel: () => void;
}

const AgreementsRequiredModal = ({ open, applicant, onSuccess, onCancel }: AgreementsRequiredModalProps) => {
  const queryClient = useQueryClient();
  const [brokerUploading, setBrokerUploading] = useState(false);
  const [takanosUploading, setTakanosUploading] = useState(false);

  const brokerSigned = applicant?.brokerAgreementSigned;
  const takanosSigned = applicant?.communityTakanosSigned;
  const bothSigned = brokerSigned && takanosSigned;

  const uploadMutation = useMutation({
    mutationFn: async ({ file, type }: { file: File; type: 'BrokerAgreement' | 'CommunityTakanos' }) => {
      // 1. Upload to S3
      const uploadResult = await documentsApi.upload(file, applicant!.id, type);

      // 2. Record agreement with URL
      await applicantsApi.recordAgreement(applicant!.id, {
        agreementType: type,
        documentUrl: uploadResult.documentUrl,
        signedDate: new Date().toISOString(),
      });

      return uploadResult;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['pipeline'] });
      message.success('Document uploaded successfully');
    },
    onError: () => {
      message.error('Failed to upload document');
    },
  });

  const handleBrokerUpload = async (file: File) => {
    setBrokerUploading(true);
    try {
      await uploadMutation.mutateAsync({ file, type: 'BrokerAgreement' });
    } finally {
      setBrokerUploading(false);
    }
    return false; // Prevent default upload behavior
  };

  const handleTakanosUpload = async (file: File) => {
    setTakanosUploading(true);
    try {
      await uploadMutation.mutateAsync({ file, type: 'CommunityTakanos' });
    } finally {
      setTakanosUploading(false);
    }
    return false;
  };

  const handleMoveToHouseHunting = async () => {
    try {
      await applicantsApi.changeStage(applicant!.id, { newStage: 'HouseHunting' });
      message.success(`${applicant?.familyName} moved to House Hunting`);
      onSuccess();
    } catch (err) {
      message.error('Failed to move applicant');
    }
  };

  const viewDocument = async (documentUrl: string) => {
    // Get pre-signed URL and open in new tab
    const presignedUrl = await documentsApi.getPresignedUrl(documentUrl);
    window.open(presignedUrl, '_blank');
  };

  return (
    <Modal
      title="📄 Agreements Required"
      open={open}
      onCancel={onCancel}
      width={500}
      footer={[
        <Button key="cancel" onClick={onCancel}>
          Cancel
        </Button>,
        <Button
          key="move"
          type="primary"
          disabled={!bothSigned}
          onClick={handleMoveToHouseHunting}
        >
          Move to House Hunting
        </Button>,
      ]}
    >
      <p style={{ marginBottom: 16 }}>
        Before house hunting can begin, the following agreements must be signed:
      </p>

      {/* Broker Agreement */}
      <div className="agreement-card" style={{ marginBottom: 16, padding: 16, border: '1px solid #d9d9d9', borderRadius: 8 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
          <strong>Broker Agreement</strong>
          {brokerSigned ? (
            <span style={{ color: '#52c41a' }}><CheckCircleFilled /> Signed</span>
          ) : (
            <span style={{ color: '#ff4d4f' }}><CloseCircleFilled /> Missing</span>
          )}
        </div>
        <p style={{ color: '#666', fontSize: 13, marginBottom: 12 }}>
          Agreement to work with community broker
        </p>
        {brokerSigned ? (
          <Button.Group>
            <Button icon={<EyeOutlined />} onClick={() => viewDocument(applicant?.brokerAgreementUrl!)}>
              View
            </Button>
            <Upload beforeUpload={handleBrokerUpload} showUploadList={false} accept=".pdf,.jpg,.jpeg,.png">
              <Button loading={brokerUploading}>Replace</Button>
            </Upload>
          </Button.Group>
        ) : (
          <Upload beforeUpload={handleBrokerUpload} showUploadList={false} accept=".pdf,.jpg,.jpeg,.png">
            <Button icon={<UploadOutlined />} loading={brokerUploading}>
              Upload Signed Document
            </Button>
          </Upload>
        )}
      </div>

      {/* Community Takanos */}
      <div className="agreement-card" style={{ padding: 16, border: '1px solid #d9d9d9', borderRadius: 8 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
          <strong>Community Takanos</strong>
          {takanosSigned ? (
            <span style={{ color: '#52c41a' }}><CheckCircleFilled /> Signed</span>
          ) : (
            <span style={{ color: '#ff4d4f' }}><CloseCircleFilled /> Missing</span>
          )}
        </div>
        <p style={{ color: '#666', fontSize: 13, marginBottom: 12 }}>
          Community guidelines and expectations
        </p>
        {takanosSigned ? (
          <Button.Group>
            <Button icon={<EyeOutlined />} onClick={() => viewDocument(applicant?.communityTakanosUrl!)}>
              View
            </Button>
            <Upload beforeUpload={handleTakanosUpload} showUploadList={false} accept=".pdf,.jpg,.jpeg,.png">
              <Button loading={takanosUploading}>Replace</Button>
            </Upload>
          </Button.Group>
        ) : (
          <Upload beforeUpload={handleTakanosUpload} showUploadList={false} accept=".pdf,.jpg,.jpeg,.png">
            <Button icon={<UploadOutlined />} loading={takanosUploading}>
              Upload Signed Document
            </Button>
          </Upload>
        )}
      </div>
    </Modal>
  );
};

export default AgreementsRequiredModal;
```

**API Client Functions:**
```tsx
// api/endpoints/documents.ts
export const documentsApi = {
  upload: async (file: File, applicantId: string, documentType: string) => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('applicantId', applicantId);
    formData.append('documentType', documentType);

    const response = await apiClient.post('/documents/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  getPresignedUrl: async (documentUrl: string) => {
    const response = await apiClient.get('/documents/presigned-url', {
      params: { documentUrl },
    });
    return response.data.url;
  },
};
```

### Tests Required

- Upload PDF file successfully
- Upload image file successfully
- Reject file over 10MB
- Reject invalid file type
- View document opens pre-signed URL
- Move button disabled until both signed
- Replace document works correctly

---

# PART 3: FILE STRUCTURE

```
src/FamilyRelocation.Application/
├── Common/
│   └── Interfaces/
│       └── IDocumentStorageService.cs    (NEW - S3 abstraction)
├── Applicants/
│   └── Commands/
│       └── DeleteApplicant/              (NEW)
│           ├── DeleteApplicantCommand.cs
│           └── DeleteApplicantCommandHandler.cs

src/FamilyRelocation.Infrastructure/
├── AWS/
│   └── S3DocumentStorageService.cs       (NEW - S3 implementation)

src/FamilyRelocation.API/
├── Controllers/
│   └── DocumentsController.cs            (NEW - upload endpoint)

src/FamilyRelocation.Web/src/
├── features/
│   └── applicants/
│       ├── ApplicantDetailPage.tsx      (modify - add board review section)
│       ├── EditApplicantDrawer.tsx      (new)
│       └── BoardReviewSection.tsx       (new)
│   └── pipeline/
│       ├── PipelinePage.tsx             (modify - wire drag-drop)
│       ├── transitionRules.ts           (new - transition validation)
│       └── modals/                       (new - transition modals)
│           ├── AgreementsRequiredModal.tsx  (NEW - document upload)
│           ├── BoardApprovalRequiredModal.tsx
│           ├── ContractInfoModal.tsx
│           ├── ClosingConfirmModal.tsx
│           ├── ContractFailedModal.tsx
│           └── TransitionBlockedModal.tsx
│   └── application/                      (new - public application)
│       ├── PublicApplicationPage.tsx
│       ├── PublicApplicationPage.css
│       ├── ApplicationSuccessPage.tsx
│       └── steps/
│           ├── HusbandInfoStep.tsx
│           ├── WifeInfoStep.tsx
│           ├── ChildrenStep.tsx
│           ├── AddressStep.tsx
│           ├── PreferencesStep.tsx
│           └── ReviewStep.tsx
├── api/
│   └── endpoints/
│       ├── applicants.ts                (add new endpoints)
│       └── documents.ts                 (NEW - S3 upload)
```

---

# PART 4: SPRINT SCHEDULE

### Week 1: Public Application & Pipeline

| Day | Tasks |
|-----|-------|
| 1-2 | US-F10: Public application page - steps 1-3 (husband, wife, children) |
| 3 | US-F10: Public application page - steps 4-6 (address, preferences, review) |
| 4-5 | US-F09: Pipeline drag-drop with transition modals |

### Week 2: CRUD & Polish

| Day | Tasks |
|-----|-------|
| 1-2 | US-F07: Edit applicant drawer |
| 3 | US-F08: Board review UI on detail page |
| 4 | US-022: Soft delete API + UI |
| 5 | Testing, bug fixes, PR |

---

# PART 5: DEFINITION OF DONE

For each story:
- [ ] Code implemented following existing patterns
- [ ] Unit tests written and passing
- [ ] Integration tests written and passing
- [ ] API tested manually (Swagger/curl)
- [ ] Frontend tested in browser
- [ ] Code committed with descriptive message
- [ ] PR created and reviewed

---

# PART 6: RISKS & MITIGATIONS

| Risk | Mitigation |
|------|------------|
| Stage transition logic complex | Domain already has state machine - follow existing pattern |
| Board review validation edge cases | Clear Gherkin scenarios cover all cases |
| Drag-drop optimistic update tricky | Use TanStack Query's built-in optimistic update support |
| Form validation duplication | Extract shared validation rules to common module |

---

# PART 7: SUCCESS METRICS

Sprint 3 is successful when:

1. **Public Application Form Working:**
   - Families can apply at /apply without logging in
   - Multi-step form captures all required information
   - Success confirmation shown after submission
   - Mobile-responsive design works on phones

2. **Board Review Workflow Complete:**
   - Can set board decision (Approved/Rejected/Deferred)
   - Approve action moves to HouseHunting
   - Reject action moves to Rejected

3. **Pipeline Fully Functional:**
   - Drag-drop actually changes stages
   - Validation prevents invalid transitions
   - Optimistic updates provide good UX

4. **CRUD Cycle Complete:**
   - Can edit applicant from detail page
   - Can delete applicant (soft delete)

5. **Tests Passing:**
   - All existing tests still pass
   - New tests for all new endpoints
   - Target: 320+ total tests

---

**Sprint 3 Plan Complete!**
