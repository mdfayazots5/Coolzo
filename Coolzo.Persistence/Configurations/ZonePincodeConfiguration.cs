using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ZonePincodeConfiguration : IEntityTypeConfiguration<ZonePincode>
{
    public void Configure(EntityTypeBuilder<ZonePincode> builder)
    {
        builder.ToTable("tblZonePincode");
        builder.HasKey(entity => entity.ZonePincodeId).HasName("PK_tblZonePincode_ZonePincodeId");

        builder.Property(entity => entity.ZonePincodeId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.Pincode).HasMaxLength(16).IsRequired();
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasOne(entity => entity.Zone)
            .WithMany(zone => zone.ZonePincodes)
            .HasForeignKey(entity => entity.ZoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblZonePincode_ZoneId_tblZone_ZoneId");

        builder.HasIndex(entity => new { entity.ZoneId, entity.Pincode })
            .IsUnique()
            .HasDatabaseName("UK_tblZonePincode_ZoneId_Pincode");

        builder.HasIndex(entity => entity.Pincode)
            .HasDatabaseName("IDX_tblZonePincode_Pincode");

        builder.ConfigureAuditColumns();
    }
}
