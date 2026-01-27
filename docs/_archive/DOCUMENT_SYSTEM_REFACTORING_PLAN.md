# Document System Refactoring Plan

## Overview
Refactor the hardcoded agreement fields (BrokerAgreement, CommunityTakanos) into a configurable, generic document system where:
- Users can define document types
- Stage transitions require dynamically configured document types
- Documents are stored as separate entities linked to applicants
- Document naming follows: `{DocumentType}_{FamilyName}_{timestamp}.{ext}`

## Phase 1: Domain Layer Changes

### 1.1 New Entities

**DocumentType Entity** (`Domain/Entities/DocumentType.cs`)
```csharp
public class DocumentType : BaseEntity
{
    public string Name { get; private set; }           // e.g., "BrokerAgreement"
    public string DisplayName { get; private set; }    // e.g., "Broker Agreement"
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSystemType { get; private set; }     // Prevents deletion of core types

    public static DocumentType Create(...);
}
```

**ApplicantDocument Entity** (`Domain/Entities/ApplicantDocument.cs`)
```csharp
public class ApplicantDocument : BaseEntity
{
    public Guid ApplicantId { get; private set; }
    public Guid DocumentTypeId { get; private set; }
    public string FileName { get; private set; }       // Original filename
    public string StorageKey { get; private set; }     // S3 key
    public string ContentType { get; private set; }
    public long FileSizeBytes { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string? UploadedBy { get; private set; }

    // Navigation
    public DocumentType DocumentType { get; private set; }
    public Applicant Applicant { get; private set; }

    public static ApplicantDocument Create(...);
}
```

**StageTransitionRequirement Entity** (`Domain/Entities/StageTransitionRequirement.cs`)
```csharp
public class StageTransitionRequirement : BaseEntity
{
    public HousingSearchStage FromStage { get; private set; }
    public HousingSearchStage ToStage { get; private set; }
    public Guid DocumentTypeId { get; private set; }
    public bool IsRequired { get; private set; }

    // Navigation
    public DocumentType DocumentType { get; private set; }
}
```

### 1.2 Modify Applicant Entity

Add navigation property:
```csharp
// In Applicant.cs
private readonly List<ApplicantDocument> _documents = new();
public IReadOnlyCollection<ApplicantDocument> Documents => _documents.AsReadOnly();
```

### 1.3 Modify HousingSearch Entity

Remove hardcoded fields:
- Remove: `BrokerAgreementDocumentUrl`, `BrokerAgreementSignedDate`, `BrokerAgreementSigned`
- Remove: `CommunityTakanosDocumentUrl`, `CommunityTakanosSignedDate`, `CommunityTakanosSigned`
- Remove: `AreAgreementsSigned` property
- Remove: `SignBrokerAgreement()` and `SignCommunityTakanos()` methods

Add new method:
```csharp
public bool AreRequiredDocumentsSigned(
    IEnumerable<Guid> requiredDocumentTypeIds,
    IEnumerable<ApplicantDocument> applicantDocuments)
{
    return requiredDocumentTypeIds.All(reqId =>
        applicantDocuments.Any(doc => doc.DocumentTypeId == reqId));
}
```

## Phase 2: Infrastructure Layer Changes

### 2.1 EF Core Configurations

**DocumentTypeConfiguration.cs**
- Table: `DocumentTypes`
- Unique index on `Name`
- Seed data for BrokerAgreement, CommunityTakanos

**ApplicantDocumentConfiguration.cs**
- Table: `ApplicantDocuments`
- Foreign keys to Applicant and DocumentType
- Index on (ApplicantId, DocumentTypeId)

**StageTransitionRequirementConfiguration.cs**
- Table: `StageTransitionRequirements`
- Composite unique index on (FromStage, ToStage, DocumentTypeId)
- Seed data for current requirements (InitialInquiry → HouseHunting requires both agreements)

### 2.2 Migration

Create migration: `AddConfigurableDocumentSystem`
- Create DocumentTypes table with seed data
- Create ApplicantDocuments table
- Create StageTransitionRequirements table with seed data
- Migrate existing document data from HousingSearch to ApplicantDocuments
- Remove old columns from HousingSearches table

## Phase 3: Application Layer Changes

### 3.1 New DTOs

```csharp
public record DocumentTypeDto(Guid Id, string Name, string DisplayName, string? Description, bool IsActive, bool IsSystemType);
public record ApplicantDocumentDto(Guid Id, Guid DocumentTypeId, string DocumentTypeName, string FileName, DateTime UploadedAt, string? UploadedBy);
public record StageTransitionRequirementDto(HousingSearchStage FromStage, HousingSearchStage ToStage, Guid DocumentTypeId, string DocumentTypeName);
```

### 3.2 New Commands/Queries

**Document Types:**
- `GetDocumentTypesQuery` - List all active document types
- `CreateDocumentTypeCommand` - Add new document type
- `UpdateDocumentTypeCommand` - Modify document type
- `DeleteDocumentTypeCommand` - Soft delete (only if not system type)

**Documents:**
- `GetApplicantDocumentsQuery` - List documents for an applicant
- `UploadDocumentCommand` - Upload document (update existing logic)
- `DeleteDocumentCommand` - Remove document

**Stage Requirements:**
- `GetStageRequirementsQuery` - List requirements for a stage transition
- `UpdateStageRequirementsCommand` - Configure requirements

### 3.3 Update ApplicantMapper

Add `Documents` collection to `ApplicantDetailDto`:
```csharp
public List<ApplicantDocumentDto> Documents { get; init; }
```

### 3.4 Document Naming Service

Create `IDocumentNamingService`:
```csharp
public interface IDocumentNamingService
{
    string GenerateStorageKey(string documentTypeName, string familyName, string originalFileName);
}

// Implementation generates: "{DocumentType}_{FamilyName}_{yyyyMMdd_HHmmss}.{ext}"
// Example: "BrokerAgreement_Goldstein_20260120_143052.pdf"
```

## Phase 4: API Layer Changes

### 4.1 New Endpoints

**DocumentTypesController:**
- `GET /api/document-types` - List document types
- `POST /api/document-types` - Create document type
- `PUT /api/document-types/{id}` - Update document type
- `DELETE /api/document-types/{id}` - Delete document type

**Update DocumentsController:**
- Modify upload endpoint to use DocumentTypeId instead of string
- Add `GET /api/documents/applicant/{applicantId}` - List applicant's documents
- Add `DELETE /api/documents/{id}` - Delete document

**StageRequirementsController:**
- `GET /api/stage-requirements` - List all requirements
- `GET /api/stage-requirements/{fromStage}/{toStage}` - Get requirements for transition
- `PUT /api/stage-requirements/{fromStage}/{toStage}` - Update requirements

## Phase 5: Frontend Changes

### 5.1 API Types & Endpoints

Update `api/types/index.ts`:
- Add `DocumentType`, `ApplicantDocument`, `StageRequirement` interfaces

Add `api/endpoints/documentTypes.ts`:
- `getDocumentTypes()`, `createDocumentType()`, etc.

### 5.2 ApplicantDetailPage Updates

- Add "Documents" section showing all uploaded documents
- Each document shows: type name, filename, upload date, view/download link
- Upload button opens modal with document type dropdown

### 5.3 DocumentUploadModal Updates

- Replace hardcoded document type buttons with dynamic list from API
- Fetch document types on mount
- Document type selection via dropdown

### 5.4 Pipeline Stage Transition

- Update `AgreementsRequiredModal` to fetch required documents dynamically
- Check which required documents are missing
- Show list of missing documents
- Allow upload directly from modal

### 5.5 Settings Page (Optional Enhancement)

- Add "Document Types" management section
- CRUD interface for document types
- Configure stage requirements

## Migration Strategy

1. Create new tables with seed data
2. Run data migration script to copy existing document data:
   - For each HousingSearch with BrokerAgreement data → create ApplicantDocument
   - For each HousingSearch with CommunityTakanos data → create ApplicantDocument
3. Verify data migration
4. Deploy new code
5. Remove old columns in subsequent migration

## Files to Create/Modify

**Create:**
- `Domain/Entities/DocumentType.cs`
- `Domain/Entities/ApplicantDocument.cs`
- `Domain/Entities/StageTransitionRequirement.cs`
- `Infrastructure/Persistence/Configurations/DocumentTypeConfiguration.cs`
- `Infrastructure/Persistence/Configurations/ApplicantDocumentConfiguration.cs`
- `Infrastructure/Persistence/Configurations/StageTransitionRequirementConfiguration.cs`
- `Application/Documents/Commands/` (multiple)
- `Application/Documents/Queries/` (multiple)
- `Application/DocumentTypes/` (commands and queries)
- `API/Controllers/DocumentTypesController.cs`
- `API/Controllers/StageRequirementsController.cs`
- `Web/src/api/endpoints/documentTypes.ts`
- `Web/src/api/endpoints/stageRequirements.ts`

**Modify:**
- `Domain/Entities/Applicant.cs` - Add Documents navigation
- `Domain/Entities/HousingSearch.cs` - Remove hardcoded agreement fields
- `Infrastructure/Persistence/ApplicationDbContext.cs` - Add DbSets
- `API/Controllers/DocumentsController.cs` - Update upload logic
- `Application/Applicants/DTOs/ApplicantDetailDto.cs` - Add Documents
- `Application/Applicants/DTOs/ApplicantMapper.cs` - Map documents
- `Web/src/api/types/index.ts` - Add new types
- `Web/src/features/applicants/ApplicantDetailPage.tsx` - Add documents section
- `Web/src/features/applicants/DocumentUploadModal.tsx` - Dynamic document types
- `Web/src/features/pipeline/modals/AgreementsRequiredModal.tsx` - Dynamic requirements

## Verification

1. Run `dotnet build` to verify compilation
2. Run `dotnet ef database update` to apply migration
3. Run `dotnet test` to verify existing tests pass
4. Manual testing:
   - Create a new document type via API
   - Upload a document with the new type
   - Verify document appears in applicant details
   - Test stage transition with required documents
