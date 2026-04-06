using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("tblRole");
        builder.HasKey(entity => entity.RoleId).HasName("PK_tblRole_RoleId");

        builder.Property(entity => entity.RoleId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.RoleName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.RoleName).IsUnique().HasDatabaseName("UK_tblRole_RoleName");

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
