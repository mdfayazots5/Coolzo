using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class UserPasswordHistoryConfiguration : IEntityTypeConfiguration<UserPasswordHistory>
{
    public void Configure(EntityTypeBuilder<UserPasswordHistory> builder)
    {
        builder.ToTable("tblUserPasswordHistory");
        builder.HasKey(entity => entity.UserPasswordHistoryId).HasName("PK_tblUserPasswordHistory_UserPasswordHistoryId");

        builder.Property(entity => entity.UserPasswordHistoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PasswordValue).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.PasswordStorageMode).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.ChangeSource).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.ChangedOnUtc).IsRequired();

        builder.HasIndex(entity => new { entity.UserId, entity.ChangedOnUtc })
            .HasDatabaseName("IDX_tblUserPasswordHistory_UserId_ChangedOnUtc");

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
