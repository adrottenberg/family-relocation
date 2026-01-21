using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for StageTransitionRequirement entity with seed data.
/// </summary>
public class StageTransitionRequirementConfiguration : IEntityTypeConfiguration<StageTransitionRequirement>
{
    public void Configure(EntityTypeBuilder<StageTransitionRequirement> builder)
    {
        builder.ToTable("StageTransitionRequirements");

        // Primary Key
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(r => r.FromStage)
            .IsRequired();

        builder.Property(r => r.ToStage)
            .IsRequired();

        builder.Property(r => r.DocumentTypeId)
            .IsRequired();

        builder.Property(r => r.IsRequired)
            .IsRequired()
            .HasDefaultValue(true);

        // Ignore domain events (not persisted)
        builder.Ignore(r => r.DomainEvents);

        // Relationships
        builder.HasOne(r => r.DocumentType)
            .WithMany()
            .HasForeignKey(r => r.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes - unique constraint on (FromStage, ToStage, DocumentTypeId)
        builder.HasIndex(r => new { r.FromStage, r.ToStage, r.DocumentTypeId })
            .IsUnique();

        builder.HasIndex(r => new { r.FromStage, r.ToStage });

        // Note: Stage requirements for housing search transitions
        // With the refactored model, document requirements for board approval are
        // now checked at the Applicant level (not HousingSearch transitions).
        //
        // Future seed data can be added for search-level requirements like:
        // - Searching -> UnderContract (might require pre-approval letter)
        // - UnderContract -> Closed (might require closing documents)
        //
        // For now, no seed data - requirements can be configured via Settings page.
    }
}
