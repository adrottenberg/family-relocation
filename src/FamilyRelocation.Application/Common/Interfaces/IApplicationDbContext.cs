namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Minimal abstraction over DbContext for Application layer operations.
/// For complex queries that need EF Core specifics, use handlers in Infrastructure
/// that inject ApplicationDbContext directly.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Adds an entity to the context for insertion on SaveChanges
    /// </summary>
    void Add<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
