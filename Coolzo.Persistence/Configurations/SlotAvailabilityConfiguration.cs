using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SlotAvailabilityConfiguration : IEntityTypeConfiguration<SlotAvailability>
{
    public void Configure(EntityTypeBuilder<SlotAvailability> builder)
    {
        builder.ToTable("tblSlotAvailability");
        builder.HasKey(entity => entity.SlotAvailabilityId).HasName("PK_tblSlotAvailability_SlotAvailabilityId");

        builder.Property(entity => entity.SlotAvailabilityId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.SlotDate).HasColumnType("date");
        builder.Property(entity => entity.AvailableCapacity).HasDefaultValue(1);
        builder.Property(entity => entity.ReservedCapacity).HasDefaultValue(0);
        builder.Property(entity => entity.IsBlocked).HasDefaultValue(false);

        builder.HasOne(entity => entity.SlotConfiguration)
            .WithMany(configuration => configuration.SlotAvailabilities)
            .HasForeignKey(entity => entity.SlotConfigurationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSlotAvailability_SlotConfigurationId_tblSlotConfiguration_SlotConfigurationId");

        builder.HasOne(entity => entity.Zone)
            .WithMany(zone => zone.SlotAvailabilities)
            .HasForeignKey(entity => entity.ZoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSlotAvailability_ZoneId_tblZone_ZoneId");

        builder.HasIndex(entity => new { entity.ZoneId, entity.SlotDate, entity.SlotConfigurationId })
            .IsUnique()
            .HasDatabaseName("UK_tblSlotAvailability_ZoneId_SlotDate_SlotConfigurationId");

        builder.ConfigureAuditColumns();
    }
}
