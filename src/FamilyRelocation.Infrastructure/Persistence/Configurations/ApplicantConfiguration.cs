using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

public class ApplicantConfiguration : IEntityTypeConfiguration<Applicant>
{
    public void Configure(EntityTypeBuilder<Applicant> builder)
    {
        builder.ToTable("Applicants");

        // Primary Key
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("ApplicantId")
            .ValueGeneratedNever();

        // Ignore the ApplicantId alias property
        builder.Ignore(a => a.ApplicantId);

        // Basic Properties
        builder.Property(a => a.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.FatherName)
            .HasMaxLength(100);

        // Ignore computed properties
        builder.Ignore(a => a.FullName);
        builder.Ignore(a => a.WifeFullName);

        // Wife Info
        builder.Property(a => a.WifeFirstName)
            .HasMaxLength(100);

        builder.Property(a => a.WifeMaidenName)
            .HasMaxLength(100);

        builder.Property(a => a.WifeFatherName)
            .HasMaxLength(100);

        builder.Property(a => a.WifeHighSchool)
            .HasMaxLength(200);

        // Email Value Object (owned)
        builder.OwnsOne(a => a.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();
        });

        // Address Value Object (owned)
        builder.OwnsOne(a => a.Address, address =>
        {
            address.Property(addr => addr.Street)
                .HasColumnName("Address_Street")
                .HasMaxLength(200);

            address.Property(addr => addr.Street2)
                .HasColumnName("Address_Street2")
                .HasMaxLength(200);

            address.Property(addr => addr.City)
                .HasColumnName("Address_City")
                .HasMaxLength(100);

            address.Property(addr => addr.State)
                .HasColumnName("Address_State")
                .HasMaxLength(2);

            address.Property(addr => addr.ZipCode)
                .HasColumnName("Address_ZipCode")
                .HasMaxLength(10);
        });

        // Phone Numbers (JSON column)
        builder.Property(a => a.PhoneNumbers)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<PhoneNumber>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<PhoneNumber>());

        // Children (JSON column)
        builder.Property(a => a.Children)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Child>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Child>());

        // Ignore computed property
        builder.Ignore(a => a.NumberOfChildren);

        // Community
        builder.Property(a => a.CurrentKehila)
            .HasMaxLength(200);

        builder.Property(a => a.ShabbosShul)
            .HasMaxLength(200);

        // Housing Preferences
        builder.OwnsOne(a => a.Budget, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Budget")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("Budget_Currency")
                .HasMaxLength(3)
                .HasDefaultValue("USD");
        });

        builder.Property(a => a.MinBedrooms);
        builder.Property(a => a.MinBathrooms)
            .HasColumnType("decimal(3,1)");

        // Required Features (JSON column)
        builder.Property(a => a.RequiredFeatures)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());

        // Shul Proximity Preference (JSON column)
        builder.Property(a => a.ShulProximity)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<ShulProximityPreference>(v, (System.Text.Json.JsonSerializerOptions?)null));

        builder.Property(a => a.MoveTimeline);

        builder.Property(a => a.HousingNotes)
            .HasMaxLength(4000);

        // Board Review
        builder.Property(a => a.BoardReviewDate);
        builder.Property(a => a.BoardDecision);
        builder.Property(a => a.BoardDecisionNotes)
            .HasMaxLength(2000);
        builder.Property(a => a.BoardReviewedByUserId);

        // Ignore computed properties
        builder.Ignore(a => a.IsApproved);
        builder.Ignore(a => a.IsPendingBoardReview);

        // Audit
        builder.Property(a => a.CreatedBy);
        builder.Property(a => a.CreatedDate);
        builder.Property(a => a.ModifiedBy);
        builder.Property(a => a.ModifiedDate);
        builder.Property(a => a.IsDeleted)
            .HasDefaultValue(false);

        // Ignore domain events (not persisted)
        builder.Ignore(a => a.DomainEvents);

        // Relationship - one housing search per applicant
        builder.HasOne(a => a.HousingSearch)
            .WithOne(hs => hs.Applicant)
            .HasForeignKey<HousingSearch>(hs => hs.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete
        builder.HasQueryFilter(a => !a.IsDeleted);

        // Indexes
        builder.HasIndex(a => a.IsDeleted);
        builder.HasIndex(a => a.CreatedDate);
        builder.HasIndex(a => a.BoardDecision);
        builder.HasIndex(a => a.ProspectId);

        // Note: Email uniqueness should be enforced at application layer
        // since Email is an owned type. Alternatively, add a raw SQL index in migrations.
    }
}
