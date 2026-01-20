using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ApplicantDocument entity.
/// </summary>
public class ApplicantDocumentConfiguration : IEntityTypeConfiguration<ApplicantDocument>
{
    public void Configure(EntityTypeBuilder<ApplicantDocument> builder)
    {
        builder.ToTable("ApplicantDocuments");

        // Primary Key
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        // Foreign Keys
        builder.Property(d => d.ApplicantId)
            .IsRequired();

        builder.Property(d => d.DocumentTypeId)
            .IsRequired();

        // Properties
        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.FileSizeBytes)
            .IsRequired();

        builder.Property(d => d.UploadedAt)
            .IsRequired();

        builder.Property(d => d.UploadedBy);

        // Ignore domain events (not persisted)
        builder.Ignore(d => d.DomainEvents);

        // Relationships
        builder.HasOne(d => d.Applicant)
            .WithMany(a => a.Documents)
            .HasForeignKey(d => d.ApplicantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.DocumentType)
            .WithMany()
            .HasForeignKey(d => d.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(d => d.ApplicantId);
        builder.HasIndex(d => d.DocumentTypeId);
        builder.HasIndex(d => new { d.ApplicantId, d.DocumentTypeId });
        builder.HasIndex(d => d.UploadedAt);
    }
}
