# Technical Debt

This document tracks technical debt items that should be addressed in future development.

## Architecture

### TD-001: EF Core References in Application Layer

**Status:** Open (Undecided - see detailed analysis)
**Priority:** Medium
**Created:** 2026-01-22
**Last Reviewed:** 2026-01-29

**Description:**
The Application layer currently has 67 handlers that reference `Microsoft.EntityFrameworkCore` for async LINQ extensions (`FirstOrDefaultAsync`, `ToListAsync`, `Include`, etc.). This violates strict Clean Architecture principles where the Application layer should not depend on infrastructure concerns.

**Current State:**
- `IApplicationDbContext` returns `IQueryable<T>` via `Set<T>()`
- Handlers in Application layer use EF Core's async extensions directly
- This creates a dependency on EF Core from Application layer

**Detailed Analysis:**
A comprehensive pros/cons analysis was conducted on 2026-01-29. Key findings:
- The dependency is on query extension methods, not DbContext directly
- Abstracting `Include`/`ThenInclude` is particularly difficult due to complex generic type chaining
- True ORM independence is questionable since `IQueryable` is already a leaky abstraction
- 67 files would need refactoring with uncertain benefit

See **CONVERSATION_MEMORY_LOG.md Section 14** for full discussion of tradeoffs.

**Fix Options:**

1. **Extend IApplicationDbContext**: Add async execution methods (`ToListAsync`, `AnyAsync`, etc.) to the interface
   ```csharp
   // Application layer
   public interface IApplicationDbContext
   {
       IQueryable<TEntity> Set<TEntity>() where TEntity : class;
       Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken ct = default);
       Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken ct = default);
       // etc.
   }
   ```

2. **Separate IAsyncQueryExecutor**: Create a dedicated interface for async query execution

3. **Accept the Dependency**: Keep current implementation as a pragmatic compromise

**Arguments For Removing:**
- Architectural purity
- Cleaner unit tests (can mock interface directly)

**Arguments Against Removing:**
- High refactoring cost (67 files)
- Loss of fluent LINQ style
- Include/ThenInclude is very hard to abstract
- Switching ORMs would require rewriting queries anyway

**Affected Files:**
67 files in `src/FamilyRelocation.Application/` - search for `using Microsoft.EntityFrameworkCore`

**Effort Estimate:** Large (affects core architecture pattern)
