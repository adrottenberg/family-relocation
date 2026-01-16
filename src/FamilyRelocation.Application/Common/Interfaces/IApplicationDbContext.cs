namespace FamilyRelocation.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // DbSets will be added when domain entities are created (US-004)
    // Example:
    // DbSet<Applicant> Applicants { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
