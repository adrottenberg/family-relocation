using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

public class ShulConfiguration : IEntityTypeConfiguration<Shul>
{
    public void Configure(EntityTypeBuilder<Shul> builder)
    {
        builder.ToTable("Shuls");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(s => s.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200).IsRequired();
            address.Property(a => a.Street2).HasColumnName("Street2").HasMaxLength(100);
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("State").HasMaxLength(2).IsRequired();
            address.Property(a => a.ZipCode).HasColumnName("ZipCode").HasMaxLength(10).IsRequired();
        });

        builder.OwnsOne(s => s.Location, coords =>
        {
            coords.Property(c => c.Latitude).HasColumnName("Latitude");
            coords.Property(c => c.Longitude).HasColumnName("Longitude");
        });

        builder.Property(s => s.Rabbi).HasMaxLength(200);
        builder.Property(s => s.Denomination).HasMaxLength(100);
        builder.Property(s => s.Website).HasMaxLength(500);
        builder.Property(s => s.Notes).HasMaxLength(2000);
        builder.Property(s => s.IsActive).HasDefaultValue(true);

        // Audit fields
        builder.Property(s => s.CreatedBy).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.ModifiedBy);
        builder.Property(s => s.ModifiedAt);

        // PropertyDistances relationship
        builder.HasMany(s => s.PropertyDistances)
            .WithOne()
            .HasForeignKey(pd => pd.ShulId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for active shuls
        builder.HasQueryFilter(s => s.IsActive);

        // Index for common queries
        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.IsActive);
    }
}

public class PropertyShulDistanceConfiguration : IEntityTypeConfiguration<PropertyShulDistance>
{
    public void Configure(EntityTypeBuilder<PropertyShulDistance> builder)
    {
        builder.ToTable("PropertyShulDistances");

        builder.HasKey(pd => pd.Id);

        builder.Property(pd => pd.PropertyId).IsRequired();
        builder.Property(pd => pd.ShulId).IsRequired();
        builder.Property(pd => pd.DistanceMiles).IsRequired();
        builder.Property(pd => pd.WalkingTimeMinutes).IsRequired();
        builder.Property(pd => pd.CalculatedAt).IsRequired();

        // Unique constraint - one distance record per property-shul pair
        builder.HasIndex(pd => new { pd.PropertyId, pd.ShulId }).IsUnique();

        // Index for querying distances for a property
        builder.HasIndex(pd => pd.PropertyId);

        // Foreign key to Property (manually configured since Property doesn't have navigation property)
        builder.HasOne<Property>()
            .WithMany()
            .HasForeignKey(pd => pd.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
