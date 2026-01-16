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

        // Husband Info (JSON column - contains nested Email and PhoneNumbers)
        builder.Property(a => a.Husband)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<HusbandInfo>(v, (System.Text.Json.JsonSerializerOptions?)null)!)
            .IsRequired();

        // Wife Info (JSON column - contains nested Email and PhoneNumbers)
        builder.Property(a => a.Wife)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<SpouseInfo>(v, (System.Text.Json.JsonSerializerOptions?)null));

        // Ignore computed properties
        builder.Ignore(a => a.FamilyName);

        // Address Value Object (owned - flattened to columns)
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

            // Ignore computed property
            address.Ignore(addr => addr.FullAddress);
        });

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

        // Board Review (owned value object)
        builder.OwnsOne(a => a.BoardReview, review =>
        {
            review.Property(r => r.Decision)
                .HasColumnName("BoardDecision");

            review.Property(r => r.Notes)
                .HasColumnName("BoardDecisionNotes")
                .HasMaxLength(2000);

            review.Property(r => r.ReviewDate)
                .HasColumnName("BoardReviewDate");

            review.Property(r => r.ReviewedByUserId)
                .HasColumnName("BoardReviewedByUserId");

            // Ignore computed properties
            review.Ignore(r => r.IsApproved);
            review.Ignore(r => r.IsRejected);
            review.Ignore(r => r.IsPending);
            review.Ignore(r => r.IsDeferred);
        });

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
        builder.HasIndex(a => a.ProspectId);
    }
}
