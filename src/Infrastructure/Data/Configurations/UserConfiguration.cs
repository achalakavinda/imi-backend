using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MigratingAssistant.Domain.Entities;

namespace MigratingAssistant.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(t => t.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false);

        builder.Property(t => t.Version)
            .HasColumnName("version")
            .HasDefaultValue(1);

        builder.Property(t => t.Created)
            .HasColumnName("created_at");

        builder.Property(t => t.LastModified)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(t => t.Email)
            .IsUnique();
    }
}