# Applicant/HousingSearch Stage Separation Refactoring Plan

## Overview

Refactor the stage system to cleanly separate applicant-level concerns (application/approval) from housing-search-level concerns (house hunting journey). This fixes the current design where `HousingSearchStage` mixes two different domain concepts across two aggregate roots.

### Current Problem
- `HousingSearchStage` enum contains both applicant stages (Submitted, BoardApproved, Rejected) and search stages (HouseHunting, UnderContract, etc.)
- Transition logic is split across entities
- One-to-one relationship prevents multiple/sequential house searches per approved applicant

### Solution: Explicit Stage Separation
- **ApplicationStatus** on Applicant: `Submitted | Approved | Rejected`
- **HousingSearchStage** on HousingSearch: `Searching | UnderContract | Closed | MovedIn | Paused`
- One-to-many relationship: Applicant has collection of HousingSearches
- Auto-create first HousingSearch when applicant is approved
- UI focused on 99% case (one search), with tucked-away option to add more

---

## Phase 1: Domain Layer Changes

### 1.1 Create ApplicationStatus Enum

**Create `Domain/Enums/ApplicationStatus.cs`:**
```csharp
namespace FamilyRelocation.Domain.Enums;

public enum ApplicationStatus
{
    Submitted = 0,
    Approved = 1,
    Rejected = 2
}
```

### 1.2 Refactor HousingSearchStage Enum

**Modify `Domain/Enums/HousingSearchStage.cs`:**
```csharp
namespace FamilyRelocation.Domain.Enums;

public enum HousingSearchStage
{
    Searching = 0,      // Was HouseHunting
    UnderContract = 1,
    Closed = 2,
    MovedIn = 3,
    Paused = 4
}
```

### 1.3 Modify Applicant Entity

**Changes to `Domain/Entities/Applicant.cs`:**

1. Add `ApplicationStatus` property
2. Change `HousingSearch` navigation from single to collection
3. Add convenience properties: `ActiveHousingSearch`, `LatestHousingSearch`
4. Update `SetBoardDecision` to create first HousingSearch on approval
5. Add `StartNewHousingSearch()` method for additional searches

### 1.4 Modify HousingSearch Entity

**Changes to `Domain/Entities/HousingSearch.cs`:**

1. Update state machine (remove applicant-level stages)
2. Update factory method - start in Searching stage
3. Remove `StartHouseHunting` method (no longer needed)
4. Update transition methods to use new stage names

---

## Phase 2: Infrastructure Layer Changes

### 2.1 Update ApplicantConfiguration

- Add `Status` column with string conversion
- Change one-to-one to one-to-many relationship
- Configure navigation access to private collection

### 2.2 Create Migration: `SeparateApplicationAndSearchStages`

Data migration steps:
1. Add `Status` column to `Applicants` table
2. Rename 'HouseHunting' stage values to 'Searching'
3. Migrate existing data based on HousingSearch stage
4. Delete HousingSearches for non-approved applicants
5. Ensure approved applicants have at least one HousingSearch

---

## Phase 3: Application Layer Changes

### 3.1 Update DTOs

- Add `Status` to `ApplicantDetailDto`
- Add `HousingSearches` collection
- Add `ActiveHousingSearch` for 99% case
- Create `HousingSearchSummaryDto`
- Update `PipelineApplicantDto` with combined stage view

### 3.2 Refactor Commands

- Rename `ChangeStageCommand` to `ChangeSearchStageCommand`
- Create `StartNewHousingSearchCommand`
- Simplify `SetBoardDecisionCommand` (domain creates search)

### 3.3 Update Queries

- Update `GetPipelineQuery` to return combined view

---

## Phase 4: API Layer Changes

### 4.1 New/Updated Endpoints

- `POST /api/applicants/{id}/housing-searches` - Start new search
- `POST /api/housing-searches/{id}/change-stage` - Change search stage
- Keep `POST /api/applicants/{id}/board-decision` unchanged

---

## Phase 5: Frontend Changes

### 5.1 Update API Types

```typescript
export type ApplicationStatus = 'Submitted' | 'Approved' | 'Rejected';
export type HousingSearchStage = 'Searching' | 'UnderContract' | 'Closed' | 'MovedIn' | 'Paused';
export type PipelineStage = 'Submitted' | 'Approved' | 'Searching' | 'UnderContract' | 'Closed';
```

### 5.2 Update PipelinePage

- Combined stage columns: Submitted, Approved, Searching, UnderContract, Closed
- Stage derivation logic combining ApplicationStatus and HousingSearchStage

### 5.3 Update Transition Rules

- Separate application-level transitions (board decision)
- Search-level transitions (document checks, etc.)

### 5.4 Update ApplicantDetailPage

- Show housing searches section (collapsed if just one)
- Add tucked-away button for new search via dropdown menu

---

## Files to Create

| File | Purpose |
|------|---------|
| `Domain/Enums/ApplicationStatus.cs` | New enum for applicant status |
| `Domain/Events/HousingSearchCreatedEvent.cs` | Event when search created |
| `Application/HousingSearches/Commands/ChangeSearchStage/` | Renamed command |
| `Application/HousingSearches/Commands/StartNewHousingSearch/` | Create new search |

## Files to Modify

| File | Changes |
|------|---------|
| `Domain/Enums/HousingSearchStage.cs` | Remove Submitted, BoardApproved, Rejected; rename HouseHunting to Searching |
| `Domain/Entities/Applicant.cs` | Add Status, change to collection of searches, update methods |
| `Domain/Entities/HousingSearch.cs` | Update state machine, remove applicant-level logic |
| `Infrastructure/Persistence/Configurations/ApplicantConfiguration.cs` | Add Status, change to HasMany |
| `Application/Applicants/DTOs/` | Update DTOs for new structure |
| `Application/Applicants/Commands/SetBoardDecision/` | Simplify (domain creates search) |
| `API/Controllers/ApplicantsController.cs` | Add new search endpoint |
| `Web/src/api/types/index.ts` | Update TypeScript types |
| `Web/src/features/pipeline/PipelinePage.tsx` | Update stage handling |
| `Web/src/features/pipeline/transitionRules.ts` | Separate app vs search rules |
| `Web/src/features/applicants/ApplicantDetailPage.tsx` | Show searches, add button |

---

## Migration Data Transformation

```sql
-- 1. Add Status column
ALTER TABLE "Applicants" ADD COLUMN "Status" varchar(20) DEFAULT 'Submitted';

-- 2. Migrate data based on HousingSearch stage
UPDATE "Applicants" a
SET "Status" = CASE
    WHEN hs."Stage" IN ('BoardApproved', 'HouseHunting', 'UnderContract', 'Closed', 'MovedIn', 'Paused')
        THEN 'Approved'
    WHEN hs."Stage" = 'Rejected' THEN 'Rejected'
    ELSE 'Submitted'
END
FROM "HousingSearches" hs WHERE hs."ApplicantId" = a."Id";

-- 3. Update HousingSearch stages
UPDATE "HousingSearches" SET "Stage" = 'Searching'
WHERE "Stage" IN ('HouseHunting', 'BoardApproved');

-- 4. Delete HousingSearches for non-approved applicants
DELETE FROM "HousingSearches" hs
USING "Applicants" a
WHERE hs."ApplicantId" = a."Id" AND a."Status" IN ('Submitted', 'Rejected');

-- 5. Create HousingSearch for approved applicants without one
INSERT INTO "HousingSearches" ("Id", "ApplicantId", "Stage", "CreatedAt")
SELECT gen_random_uuid(), a."Id", 'Searching', NOW()
FROM "Applicants" a
LEFT JOIN "HousingSearches" hs ON hs."ApplicantId" = a."Id"
WHERE a."Status" = 'Approved' AND hs."Id" IS NULL;
```

---

## Verification

1. **Build**: `dotnet build` - ensure no compilation errors
2. **Migration**: Apply migration and verify data transformation
3. **API Tests**: Test stage transitions via API
4. **Pipeline Display**: Verify combined stage view in UI
5. **Edge Cases**:
   - Submitted applicant → only shows in Submitted column
   - Approved applicant → shows in Searching/UnderContract/etc.
   - Multiple searches → UI shows active search, button for new search visible
