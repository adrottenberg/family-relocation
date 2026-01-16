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

        // Basic Properties - Husband
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

        // Wife Info (owned value object)
        builder.OwnsOne(a => a.Wife, wife =>
        {
            wife.Property(w => w.FirstName)
                .HasColumnName("WifeFirstName")
                .HasMaxLength(100);

            wife.Property(w => w.MaidenName)
                .HasColumnName("WifeMaidenName")
                .HasMaxLength(100);

            wife.Property(w => w.FatherName)
                .HasColumnName("WifeFatherName")
                .HasMaxLength(100);

            wife.Property(w => w.HighSchool)
                .HasColumnName("WifeHighSchool")
                .HasMaxLength(200);

            // Ignore computed property
            wife.Ignore(w => w.FullName);
        });

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

            // Ignore computed property
            address.Ignore(addr => addr.FullAddress);
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

        // Note: Email uniqueness should be enforced at application layer
        // since Email is an owned type. Alternatively, add a raw SQL index in migrations.
    }
}
