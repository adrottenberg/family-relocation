using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for DocumentType entity with seed data.
/// </summary>
public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("DocumentTypes");

        // Primary Key
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        // Properties
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.IsSystemType)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.ModifiedAt);

        // Ignore domain events (not persisted)
        builder.Ignore(d => d.DomainEvents);

        // Indexes
        builder.HasIndex(d => d.Name)
            .IsUnique();

        builder.HasIndex(d => d.IsActive);

        // Seed data for system document types
        builder.HasData(
            new
            {
                Id = WellKnownIds.BrokerAgreementDocumentTypeId,
                Name = "BrokerAgreement",
                DisplayName = "Broker Agreement",
                Description = "Agreement to work with the community's broker",
                IsActive = true,
                IsSystemType = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = WellKnownIds.CommunityTakanosDocumentTypeId,
                Name = "CommunityTakanos",
                DisplayName = "Community Takanos",
                Description = "Community guidelines and rules agreement",
                IsActive = true,
                IsSystemType = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
