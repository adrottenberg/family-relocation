using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Applicant> Applicants { get; }
    IQueryable<HousingSearch> HousingSearches { get; }

    /// <summary>
    /// Adds an entity to the context for insertion on SaveChanges
    /// </summary>
    void Add<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
