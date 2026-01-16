using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

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

        // Search Number
        builder.Property(h => h.SearchNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(h => h.SearchNumber)
            .IsUnique();

        // Foreign Key to Applicant
        builder.Property(h => h.ApplicantId)
            .IsRequired();

        // Status
        builder.Property(h => h.Stage)
            .IsRequired();

        builder.Property(h => h.StageChangedDate)
            .IsRequired();

        // Contract Property
        builder.Property(h => h.ContractPropertyId);

        builder.OwnsOne(h => h.ContractPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ContractPrice")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("ContractPrice_Currency")
                .HasMaxLength(3)
                .HasDefaultValue("USD");
        });

        builder.Property(h => h.ContractDate);

        // Failed Contracts (stored as JSON)
        builder.OwnsMany(h => h.FailedContracts, fc =>
        {
            fc.ToJson("FailedContracts");

            fc.Property(f => f.PropertyId);
            fc.Property(f => f.ContractDate);
            fc.Property(f => f.FailedDate);
            fc.Property(f => f.Reason).HasMaxLength(500);

            fc.OwnsOne(f => f.ContractPrice, money =>
            {
                money.Property(m => m.Amount);
                money.Property(m => m.Currency).HasMaxLength(3);
            });
        });

        // Closing
        builder.Property(h => h.ClosingDate);
        builder.Property(h => h.ActualClosingDate);

        // Move In
        builder.Property(h => h.MovedInStatus);
        builder.Property(h => h.MovedInDate);

        // Housing Preferences
        builder.OwnsOne(h => h.Budget, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Budget")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("Budget_Currency")
                .HasMaxLength(3)
                .HasDefaultValue("USD");
        });

        builder.Property(h => h.MinBedrooms);
        builder.Property(h => h.MinBathrooms)
            .HasColumnType("decimal(3,1)");

        // Required Features (JSON column)
        builder.Property(h => h.RequiredFeatures)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());

        // Shul Proximity Preference (JSON column)
        builder.Property(h => h.ShulProximity)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<ShulProximityPreference>(v, (System.Text.Json.JsonSerializerOptions?)null));

        builder.Property(h => h.MoveTimeline);

        // Notes
        builder.Property(h => h.Notes)
            .HasMaxLength(4000);

        // Ignore computed properties
        builder.Ignore(h => h.IsUnderContract);
        builder.Ignore(h => h.IsComplete);
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
        builder.HasIndex(h => h.ContractPropertyId);
    }
}
