using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("tblAuditLog");
        builder.HasKey(entity => entity.AuditLogId).HasName("PK_tblAuditLog_AuditLogId");

        builder.Property(entity => entity.AuditLogId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ActionName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.EntityName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.EntityId).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.TraceId).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.StatusName).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.OldValues).HasMaxLength(512);
        builder.Property(entity => entity.NewValues).HasMaxLength(512);

        builder.HasIndex(entity => entity.UserId).HasDatabaseName("IDX_tblAuditLog_UserId");
        builder.HasIndex(entity => entity.TraceId).HasDatabaseName("IDX_tblAuditLog_TraceId");

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
