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

        // Seed data: BoardApproved -> HouseHunting requires both agreements
        builder.HasData(
            new
            {
                Id = WellKnownIds.BoardToHuntingBrokerRequirementId,
                FromStage = HousingSearchStage.BoardApproved,
                ToStage = HousingSearchStage.HouseHunting,
                DocumentTypeId = WellKnownIds.BrokerAgreementDocumentTypeId,
                IsRequired = true
            },
            new
            {
                Id = WellKnownIds.BoardToHuntingTakanosRequirementId,
                FromStage = HousingSearchStage.BoardApproved,
                ToStage = HousingSearchStage.HouseHunting,
                DocumentTypeId = WellKnownIds.CommunityTakanosDocumentTypeId,
                IsRequired = true
            }
        );
    }
}
