# Technical Debt

This document tracks technical debt items that should be addressed in future development.

## Architecture

### TD-001: EF Core References in Application Layer

**Status:** Open
**Priority:** Medium
**Created:** 2026-01-22

**Description:**
The Application layer currently has 63+ handlers that reference `Microsoft.EntityFrameworkCore` for async LINQ extensions (`FirstOrDefaultAsync`, `ToListAsync`, `Include`, etc.). This violates strict Clean Architecture principles where the Application layer should not depend on infrastructure concerns.

**Current State:**
- `IApplicationDbContext` returns `IQueryable<T>` via `Set<T>()`
- Handlers in Application layer use EF Core's async extensions directly
- This creates a dependency on EF Core from Application layer

**Recommended Fix Options:**

1. **Repository Abstractions**: Create repository interfaces in Application layer with async methods, implement in Infrastructure layer
   ```csharp
   // Application layer
   public interface IHousingSearchRepository
   {
       Task<HousingSearch?> GetByIdAsync(Guid id, CancellationToken ct);
       Task<HousingSearch?> GetByIdWithApplicantAsync(Guid id, CancellationToken ct);
   }

   // Infrastructure layer implements with EF Core
   ```

2. **Move Handlers to Infrastructure**: Keep query/command records in Application, but move handlers to Infrastructure layer

3. **Async Queryable Abstraction**: Create `IAsyncQueryableExtensions` interface in Application that Infrastructure implements

**Affected Files:**
63 files in `src/FamilyRelocation.Application/` - search for `using Microsoft.EntityFrameworkCore`

**Effort Estimate:** Large (affects core architecture pattern)
