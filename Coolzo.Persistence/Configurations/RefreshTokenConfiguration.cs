using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("tblRefreshToken");
        builder.HasKey(entity => entity.RefreshTokenId).HasName("PK_tblRefreshToken_RefreshTokenId");

        builder.Property(entity => entity.RefreshTokenId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.TokenValue).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.ReplacedByToken).HasMaxLength(256);

        builder.HasIndex(entity => entity.TokenValue).IsUnique().HasDatabaseName("UK_tblRefreshToken_TokenValue");
        builder.HasIndex(entity => entity.UserId).HasDatabaseName("IDX_tblRefreshToken_UserId");
        builder.HasIndex(entity => entity.UserSessionId).HasDatabaseName("IDX_tblRefreshToken_UserSessionId");

        builder.HasOne(entity => entity.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(entity => entity.UserId)
            .HasConstraintName("FK_tblRefreshToken_UserId_tblUser_UserId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.UserSession)
            .WithMany(userSession => userSession.RefreshTokens)
            .HasForeignKey(entity => entity.UserSessionId)
            .HasConstraintName("FK_tblRefreshToken_UserSessionId_tblUserSession_UserSessionId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
