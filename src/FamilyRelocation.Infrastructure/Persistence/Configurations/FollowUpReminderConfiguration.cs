using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

public class FollowUpReminderConfiguration : IEntityTypeConfiguration<FollowUpReminder>
{
    public void Configure(EntityTypeBuilder<FollowUpReminder> builder)
    {
        builder.ToTable("FollowUpReminders");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.Property(r => r.DueDate)
            .IsRequired();

        builder.Property(r => r.DueTime);

        builder.Property(r => r.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ReminderPriority.Normal)
            .IsRequired();

        builder.Property(r => r.EntityType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.EntityId)
            .IsRequired();

        builder.Property(r => r.AssignedToUserId);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ReminderStatus.Open)
            .IsRequired();

        builder.Property(r => r.SendEmailNotification)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(r => r.SnoozedUntil);

        builder.Property(r => r.SnoozeCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .IsRequired();

        builder.Property(r => r.CompletedAt);

        builder.Property(r => r.CompletedBy);

        // Indexes for common queries
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.DueDate);
        builder.HasIndex(r => r.Priority);
        builder.HasIndex(r => r.AssignedToUserId);
        builder.HasIndex(r => new { r.EntityType, r.EntityId });
        builder.HasIndex(r => new { r.Status, r.DueDate });

        // Ignore computed properties
        builder.Ignore(r => r.IsOverdue);
        builder.Ignore(r => r.IsDueToday);
        builder.Ignore(r => r.EffectiveDueDate);
    }
}
