using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("tblUserSession");
        builder.HasKey(entity => entity.UserSessionId).HasName("PK_tblUserSession_UserSessionId");

        builder.Property(entity => entity.UserSessionId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AccessTokenJti).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.RefreshToken).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.DeviceName).HasMaxLength(128);
        builder.Property(entity => entity.PlatformName).HasMaxLength(64);
        builder.Property(entity => entity.SessionIpAddress).HasMaxLength(64);
        builder.Property(entity => entity.UserAgent).HasMaxLength(256);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.UserId).HasDatabaseName("IDX_tblUserSession_UserId");
        builder.HasIndex(entity => entity.AccessTokenJti).IsUnique().HasDatabaseName("UK_tblUserSession_AccessTokenJti");

        builder.HasOne(entity => entity.User)
            .WithMany(user => user.UserSessions)
            .HasForeignKey(entity => entity.UserId)
            .HasConstraintName("FK_tblUserSession_UserId_tblUser_UserId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
