namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Abstraction over DbContext for Application layer operations.
/// Supports LINQ queries on JSON columns via EF Core's ToJson() configuration.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Returns a queryable set for the specified entity type
    /// </summary>
    IQueryable<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Adds an entity to the context for insertion on SaveChanges
    /// </summary>
    void Add<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Marks an entity for removal on SaveChanges
    /// </summary>
    void Remove<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Saves all changes made to the context.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
