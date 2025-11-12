using MigratingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MigratingAssistant.Infrastructure.Data.Configurations;

public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(512); // Reduced to avoid MySQL index size limit

        builder.Property(rt => rt.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(rt => rt.RevokedAt)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.Reason)
            .HasMaxLength(500);

        // Index for fast token lookup (using unique to ensure no duplicates)
        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        // Index for cleanup queries (find expired tokens)
        builder.HasIndex(rt => rt.ExpiresAt);

        // Composite index for user + expiry queries
        builder.HasIndex(rt => new { rt.UserId, rt.ExpiresAt });
    }
}
