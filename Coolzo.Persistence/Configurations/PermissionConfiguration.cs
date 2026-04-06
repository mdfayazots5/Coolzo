using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("tblPermission");
        builder.HasKey(entity => entity.PermissionId).HasName("PK_tblPermission_PermissionId");

        builder.Property(entity => entity.PermissionId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PermissionName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ModuleName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ActionName).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.PermissionName).IsUnique().HasDatabaseName("UK_tblPermission_PermissionName");

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
