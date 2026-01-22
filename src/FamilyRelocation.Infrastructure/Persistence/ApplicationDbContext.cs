using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    // IApplicationDbContext implementation
    IQueryable<TEntity> IApplicationDbContext.Set<TEntity>() => base.Set<TEntity>();
    void IApplicationDbContext.Add<TEntity>(TEntity entity) => base.Add(entity);
    void IApplicationDbContext.Remove<TEntity>(TEntity entity) => base.Remove(entity);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events from entities before saving
        var domainEvents = ChangeTracker
            .Entries<Entity<Guid>>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Clear domain events from entities
        foreach (var entry in ChangeTracker.Entries<Entity<Guid>>())
        {
            entry.Entity.ClearDomainEvents();
        }

        // Save changes first
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        if (_domainEventDispatcher != null && domainEvents.Any())
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
