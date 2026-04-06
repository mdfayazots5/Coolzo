using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("tblSystemSetting");
        builder.HasKey(entity => entity.SystemSettingId).HasName("PK_tblSystemSetting_SystemSettingId");

        builder.Property(entity => entity.SystemSettingId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.SettingKey).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.SettingValue).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.DataType).HasMaxLength(64).IsRequired();

        builder.HasIndex(entity => entity.SettingKey).IsUnique().HasDatabaseName("UK_tblSystemSetting_SettingKey");

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
