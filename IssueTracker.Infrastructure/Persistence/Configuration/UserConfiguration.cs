using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IssueTracker.Domain.Entities;

namespace IssueTracker.Infrastructure.Persistence.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.Username)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200);

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedOn)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.LastModifiedOn)
            .HasColumnName("updated_at");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(e => e.LastModifiedBy)
            .HasColumnName("updated_by");

        // Indexes
        builder.HasIndex(e => e.Username)
            .IsUnique()
            .HasDatabaseName("ix_users_username");

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");
    }
}
