using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Applicant> Applicants { get; }
    IQueryable<HousingSearch> HousingSearches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
