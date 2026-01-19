namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Unit of work pattern interface for transaction management.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made to the context.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
