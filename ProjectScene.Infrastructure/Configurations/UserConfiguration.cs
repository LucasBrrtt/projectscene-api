using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectScene.Domain.Entities;

namespace ProjectScene.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Define como a entidade User é persistida na tabela users.
        builder.ToTable("users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100);
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(150);
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
        builder.Property(e => e.AccessLevel).HasColumnName("access_level").HasMaxLength(20);
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.LastLogin).HasColumnName("last_login");
        builder.Property(e => e.RefreshToken).HasColumnName("refresh_token").HasMaxLength(255);
        builder.Property(e => e.RefreshTokenExpiry).HasColumnName("refresh_token_expiry");
        builder.Property(e => e.Username).HasColumnName("username").HasMaxLength(100);
    }
}
