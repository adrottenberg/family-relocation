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

        // Foreign Key to Applicant
        builder.Property(h => h.ApplicantId)
            .IsRequired();

        // Status
        builder.Property(h => h.Stage)
            .IsRequired();

        builder.Property(h => h.StageChangedDate)
            .IsRequired();

        // Current Contract (JSON column - contains nested Money)
        builder.Property(h => h.CurrentContract)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<Contract>(v, (System.Text.Json.JsonSerializerOptions?)null));

        // Failed Contracts (JSON column - contains nested Contract with Money)
        builder.Property(h => h.FailedContracts)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<FailedContractAttempt>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<FailedContractAttempt>());

        // Move In
        builder.Property(h => h.MovedInStatus);
        builder.Property(h => h.MovedInDate);

        // Housing Preferences (JSON column - contains nested Money and ShulProximityPreference)
        builder.Property(h => h.Preferences)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<HousingPreferences>(v, (System.Text.Json.JsonSerializerOptions?)null));

        // Notes
        builder.Property(h => h.Notes)
            .HasMaxLength(4000);

        // Required Agreements
        builder.Property(h => h.BrokerAgreementSigned)
            .HasDefaultValue(false);
        builder.Property(h => h.BrokerAgreementDocumentUrl)
            .HasMaxLength(500);
        builder.Property(h => h.BrokerAgreementSignedDate);

        builder.Property(h => h.CommunityTakanosSigned)
            .HasDefaultValue(false);
        builder.Property(h => h.CommunityTakanosDocumentUrl)
            .HasMaxLength(500);
        builder.Property(h => h.CommunityTakanosSignedDate);

        // Ignore computed properties
        builder.Ignore(h => h.IsUnderContract);
        builder.Ignore(h => h.IsComplete);
        builder.Ignore(h => h.IsRejected);
        builder.Ignore(h => h.FailedContractCount);
        builder.Ignore(h => h.AreAgreementsSigned);

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
