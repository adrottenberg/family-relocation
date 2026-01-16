using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Applicant> ApplicantsDbSet => Set<Applicant>();
    public DbSet<HousingSearch> HousingSearchesDbSet => Set<HousingSearch>();

    // IApplicationDbContext implementation
    IQueryable<Applicant> IApplicationDbContext.Applicants => ApplicantsDbSet;
    IQueryable<HousingSearch> IApplicationDbContext.HousingSearches => HousingSearchesDbSet;

    void IApplicationDbContext.Add<TEntity>(TEntity entity) => base.Add(entity);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
