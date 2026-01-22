using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(a => a.EntityId).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2000).IsRequired();
        builder.Property(a => a.UserId);
        builder.Property(a => a.UserName).HasMaxLength(200);
        builder.Property(a => a.Timestamp).IsRequired();

        // New fields for communication logging
        builder.Property(a => a.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Enums.ActivityType.System);

        builder.Property(a => a.DurationMinutes);

        builder.Property(a => a.Outcome).HasMaxLength(50);

        builder.Property(a => a.FollowUpReminderId);

        // Indexes for common queries
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.Type);
    }
}
