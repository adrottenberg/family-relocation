using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Action)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.OldValues)
            .HasColumnType("jsonb");

        builder.Property(e => e.NewValues)
            .HasColumnType("jsonb");

        builder.Property(e => e.UserEmail)
            .HasMaxLength(256);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);

        // Indexes for common queries
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => e.EntityId);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.UserId);
    }
}
