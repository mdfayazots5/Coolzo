using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("tblUser");
        builder.HasKey(entity => entity.UserId).HasName("PK_tblUser_UserId");

        builder.Property(entity => entity.UserId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.UserName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Email).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.FullName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.PasswordStorageMode).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.MustChangePassword).HasDefaultValue(false);
        builder.Property(entity => entity.PasswordLastChangedOnUtc);
        builder.Property(entity => entity.PasswordExpiryOnUtc);
        builder.Property(entity => entity.PasswordUpdatedBy).HasMaxLength(128);
        builder.Property(entity => entity.IsTemporaryPassword).HasDefaultValue(false);
        builder.Property(entity => entity.LastPasswordResetOnUtc);
        builder.Property(entity => entity.PasswordResetSource).HasConversion<string>().HasMaxLength(64);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);
        builder.Property(entity => entity.LastLoginDateUtc);

        builder.HasIndex(entity => entity.UserName).IsUnique().HasDatabaseName("UK_tblUser_UserName");
        builder.HasIndex(entity => entity.Email).IsUnique().HasDatabaseName("UK_tblUser_Email");

        builder.HasMany(entity => entity.PasswordHistories)
            .WithOne(entity => entity.User)
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_tblUserPasswordHistory_UserId_tblUser_UserId");

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
