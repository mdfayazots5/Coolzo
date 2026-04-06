using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("tblUserRole");
        builder.HasKey(entity => entity.UserRoleId).HasName("PK_tblUserRole_UserRoleId");

        builder.Property(entity => entity.UserRoleId).ValueGeneratedOnAdd();

        builder.HasIndex(entity => new { entity.UserId, entity.RoleId }).IsUnique().HasDatabaseName("UK_tblUserRole_UserId_RoleId");
        builder.HasIndex(entity => entity.UserId).HasDatabaseName("IDX_tblUserRole_UserId");
        builder.HasIndex(entity => entity.RoleId).HasDatabaseName("IDX_tblUserRole_RoleId");

        builder.HasOne(entity => entity.User)
            .WithMany(user => user.UserRoles)
            .HasForeignKey(entity => entity.UserId)
            .HasConstraintName("FK_tblUserRole_UserId_tblUser_UserId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(entity => entity.RoleId)
            .HasConstraintName("FK_tblUserRole_RoleId_tblRole_RoleId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
