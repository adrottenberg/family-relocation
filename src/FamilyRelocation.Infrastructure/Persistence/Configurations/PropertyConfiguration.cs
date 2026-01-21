using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(p => p.Id);

        builder.OwnsOne(p => p.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200).IsRequired();
            address.Property(a => a.Street2).HasColumnName("Street2").HasMaxLength(100);
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("State").HasMaxLength(2).IsRequired();
            address.Property(a => a.ZipCode).HasColumnName("ZipCode").HasMaxLength(10).IsRequired();
        });

        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount).HasColumnName("Price").HasColumnType("decimal(18,2)").IsRequired();
            price.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3).HasDefaultValue("USD");
        });

        builder.Property(p => p.Bedrooms).IsRequired();
        builder.Property(p => p.Bathrooms).HasColumnType("decimal(3,1)").IsRequired();
        builder.Property(p => p.SquareFeet);
        builder.Property(p => p.LotSize).HasColumnType("decimal(10,4)");
        builder.Property(p => p.YearBuilt);
        builder.Property(p => p.AnnualTaxes).HasColumnType("decimal(18,2)");

        builder.Property(p => p.Features)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.MlsNumber).HasMaxLength(50);
        builder.Property(p => p.Notes).HasMaxLength(2000);
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);

        // Audit fields
        builder.Property(p => p.CreatedBy).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.ModifiedBy);
        builder.Property(p => p.ModifiedAt);

        // Photos relationship
        builder.HasMany(p => p.Photos)
            .WithOne()
            .HasForeignKey(ph => ph.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Index for common queries
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.MlsNumber);
        builder.HasIndex(p => new { p.IsDeleted, p.Status });
    }
}

public class PropertyPhotoConfiguration : IEntityTypeConfiguration<PropertyPhoto>
{
    public void Configure(EntityTypeBuilder<PropertyPhoto> builder)
    {
        builder.ToTable("PropertyPhotos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Url).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.DisplayOrder).HasDefaultValue(0);
        builder.Property(p => p.UploadedAt).IsRequired();

        builder.HasIndex(p => new { p.PropertyId, p.DisplayOrder });
    }
}
