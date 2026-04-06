using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("tblRolePermission");
        builder.HasKey(entity => entity.RolePermissionId).HasName("PK_tblRolePermission_RolePermissionId");

        builder.Property(entity => entity.RolePermissionId).ValueGeneratedOnAdd();

        builder.HasIndex(entity => new { entity.RoleId, entity.PermissionId }).IsUnique().HasDatabaseName("UK_tblRolePermission_RoleId_PermissionId");
        builder.HasIndex(entity => entity.RoleId).HasDatabaseName("IDX_tblRolePermission_RoleId");
        builder.HasIndex(entity => entity.PermissionId).HasDatabaseName("IDX_tblRolePermission_PermissionId");

        builder.HasOne(entity => entity.Role)
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(entity => entity.RoleId)
            .HasConstraintName("FK_tblRolePermission_RoleId_tblRole_RoleId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Permission)
            .WithMany(permission => permission.RolePermissions)
            .HasForeignKey(entity => entity.PermissionId)
            .HasConstraintName("FK_tblRolePermission_PermissionId_tblPermission_PermissionId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
