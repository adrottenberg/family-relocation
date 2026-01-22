using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Showing entity.
/// </summary>
public class ShowingConfiguration : IEntityTypeConfiguration<Showing>
{
    public void Configure(EntityTypeBuilder<Showing> builder)
    {
        builder.ToTable("Showings");

        // Primary Key
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        // Foreign Key to PropertyMatch
        builder.Property(s => s.PropertyMatchId)
            .IsRequired();

        // Relationship
        builder.HasOne(s => s.PropertyMatch)
            .WithMany()
            .HasForeignKey(s => s.PropertyMatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Scheduled Date and Time
        builder.Property(s => s.ScheduledDate)
            .IsRequired();

        builder.Property(s => s.ScheduledTime)
            .IsRequired();

        // Status
        builder.Property(s => s.Status)
            .IsRequired();

        // Notes
        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        // Broker User ID (optional)
        builder.Property(s => s.BrokerUserId);

        // Audit fields
        builder.Property(s => s.CreatedBy)
            .IsRequired();
        builder.Property(s => s.CreatedAt)
            .IsRequired();
        builder.Property(s => s.ModifiedBy);
        builder.Property(s => s.ModifiedAt);
        builder.Property(s => s.CompletedAt);

        // Ignore domain events (not persisted)
        builder.Ignore(s => s.DomainEvents);

        // Ignore computed properties
        builder.Ignore(s => s.ScheduledDateTime);
        builder.Ignore(s => s.IsUpcoming);

        // Indexes
        builder.HasIndex(s => s.PropertyMatchId);
        builder.HasIndex(s => s.ScheduledDate);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.BrokerUserId);
        builder.HasIndex(s => new { s.ScheduledDate, s.Status });
    }
}
