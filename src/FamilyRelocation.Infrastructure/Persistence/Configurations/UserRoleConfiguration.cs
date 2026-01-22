using FamilyRelocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyRelocation.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.CognitoUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasMaxLength(256);

        // Unique constraint: one role per user
        builder.HasIndex(u => new { u.CognitoUserId, u.Role })
            .IsUnique();

        // Index for email lookups
        builder.HasIndex(u => u.Email);

        // Index for user ID lookups
        builder.HasIndex(u => u.CognitoUserId);
    }
}
