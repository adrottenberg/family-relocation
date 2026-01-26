using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PropertyMatch entity.
/// </summary>
public class PropertyMatchConfiguration : IEntityTypeConfiguration<PropertyMatch>
{
    public void Configure(EntityTypeBuilder<PropertyMatch> builder)
    {
        builder.ToTable("PropertyMatches");

        // Primary Key
        builder.HasKey(pm => pm.Id);
        builder.Property(pm => pm.Id)
            .ValueGeneratedNever();

        // Foreign Keys
        builder.Property(pm => pm.HousingSearchId)
            .IsRequired();

        builder.Property(pm => pm.PropertyId)
            .IsRequired();

        // Relationships
        builder.HasOne(pm => pm.HousingSearch)
            .WithMany()
            .HasForeignKey(pm => pm.HousingSearchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pm => pm.Property)
            .WithMany()
            .HasForeignKey(pm => pm.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Status
        builder.Property(pm => pm.Status)
            .IsRequired();

        // Match Score
        builder.Property(pm => pm.MatchScore)
            .IsRequired();

        // Match Details stored as JSONB
        builder.Property(pm => pm.MatchDetails)
            .HasColumnType("jsonb");

        // Notes
        builder.Property(pm => pm.Notes)
            .HasMaxLength(2000);

        // IsAutoMatched
        builder.Property(pm => pm.IsAutoMatched)
            .IsRequired()
            .HasDefaultValue(false);

        // OfferAmount (nullable decimal for when offer is made)
        builder.Property(pm => pm.OfferAmount)
            .HasPrecision(18, 2);

        // Audit fields
        builder.Property(pm => pm.CreatedBy)
            .IsRequired();
        builder.Property(pm => pm.CreatedAt)
            .IsRequired();
        builder.Property(pm => pm.ModifiedBy);
        builder.Property(pm => pm.ModifiedAt);

        // Ignore domain events (not persisted)
        builder.Ignore(pm => pm.DomainEvents);

        // Indexes
        builder.HasIndex(pm => pm.HousingSearchId);
        builder.HasIndex(pm => pm.PropertyId);
        builder.HasIndex(pm => pm.Status);
        builder.HasIndex(pm => pm.MatchScore);

        // Unique constraint: one match per HousingSearch-Property pair
        builder.HasIndex(pm => new { pm.HousingSearchId, pm.PropertyId })
            .IsUnique();
    }
}
