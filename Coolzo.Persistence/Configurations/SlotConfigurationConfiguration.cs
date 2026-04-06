using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SlotConfigurationConfiguration : IEntityTypeConfiguration<SlotConfiguration>
{
    public void Configure(EntityTypeBuilder<SlotConfiguration> builder)
    {
        builder.ToTable("tblSlotConfiguration");
        builder.HasKey(entity => entity.SlotConfigurationId).HasName("PK_tblSlotConfiguration_SlotConfigurationId");

        builder.Property(entity => entity.SlotConfigurationId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.SlotLabel).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.MaxBookingCount).HasDefaultValue(1);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasOne(entity => entity.Zone)
            .WithMany(zone => zone.SlotConfigurations)
            .HasForeignKey(entity => entity.ZoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSlotConfiguration_ZoneId_tblZone_ZoneId");

        builder.HasIndex(entity => new { entity.ZoneId, entity.StartTime, entity.EndTime })
            .IsUnique()
            .HasDatabaseName("UK_tblSlotConfiguration_ZoneId_StartTime_EndTime");

        builder.ConfigureAuditColumns();
    }
}
