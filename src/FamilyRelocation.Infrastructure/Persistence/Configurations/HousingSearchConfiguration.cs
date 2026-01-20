using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core 10 configuration for HousingSearch entity.
/// Uses Npgsql 10 JSON mapping best practices:
/// - OwnsOne + ToJson() for optional complex types
/// - OwnsMany + ToJson() for collections
/// </summary>
public class HousingSearchConfiguration : IEntityTypeConfiguration<HousingSearch>
{
    public void Configure(EntityTypeBuilder<HousingSearch> builder)
    {
        builder.ToTable("HousingSearches");

        // Primary Key
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .HasColumnName("HousingSearchId")
            .ValueGeneratedNever();

        // Ignore the HousingSearchId alias property
        builder.Ignore(h => h.HousingSearchId);

        // Foreign Key to Applicant
        builder.Property(h => h.ApplicantId)
            .IsRequired();

        // Status
        builder.Property(h => h.Stage)
            .IsRequired();

        builder.Property(h => h.StageChangedDate)
            .IsRequired();

        // Current Contract - OPTIONAL complex type stored as JSONB
        // Using OwnsOne + ToJson() for nullable types
        builder.OwnsOne(h => h.CurrentContract, contract =>
        {
            contract.ToJson();
            // Price is a nested value object (Money)
            contract.OwnsOne(c => c.Price);
        });

        // Failed Contracts - collection stored as JSONB (uses backing field)
        // Using OwnsMany + ToJson() for collections
        builder.OwnsMany<FailedContractAttempt>("_failedContracts", failed =>
        {
            failed.ToJson("FailedContracts");
            // Each FailedContractAttempt has a nested Contract with Price
            failed.OwnsOne(f => f.Contract, c => c.OwnsOne(x => x.Price));
        });

        // Ignore the computed property (it uses the backing field)
        builder.Ignore(h => h.FailedContracts);

        // Move In
        builder.Property(h => h.MovedInStatus);
        builder.Property(h => h.MovedInDate);

        // Housing Preferences - OPTIONAL complex type stored as JSONB
        // Using OwnsOne + ToJson() for nullable types
        builder.OwnsOne(h => h.Preferences, prefs =>
        {
            prefs.ToJson();
            // Nested value objects
            prefs.OwnsOne(p => p.Budget);
            prefs.OwnsOne(p => p.ShulProximity);
        });

        // Notes
        builder.Property(h => h.Notes)
            .HasMaxLength(4000);

        // Ignore computed properties
        builder.Ignore(h => h.IsUnderContract);
        builder.Ignore(h => h.IsComplete);
        builder.Ignore(h => h.IsRejected);
        builder.Ignore(h => h.FailedContractCount);

        // Audit
        builder.Property(h => h.CreatedBy);
        builder.Property(h => h.CreatedDate);
        builder.Property(h => h.ModifiedBy);
        builder.Property(h => h.ModifiedDate);
        builder.Property(h => h.IsActive)
            .HasDefaultValue(true);

        // Ignore domain events (not persisted)
        builder.Ignore(h => h.DomainEvents);

        // Indexes
        builder.HasIndex(h => h.ApplicantId);
        builder.HasIndex(h => h.Stage);
        builder.HasIndex(h => h.IsActive);
        builder.HasIndex(h => h.CreatedDate);
    }
}
