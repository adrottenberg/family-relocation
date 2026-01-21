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

    // DbSets for direct access (used by ActivityLogger)
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    // IApplicationDbContext implementation
    IQueryable<TEntity> IApplicationDbContext.Set<TEntity>() => base.Set<TEntity>();
    void IApplicationDbContext.Add<TEntity>(TEntity entity) => base.Add(entity);
    void IApplicationDbContext.Remove<TEntity>(TEntity entity) => base.Remove(entity);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
