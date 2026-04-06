using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("tblZone");
        builder.HasKey(entity => entity.ZoneId).HasName("PK_tblZone_ZoneId");

        builder.Property(entity => entity.ZoneId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ZoneCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.ZoneName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.CityName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.ZoneCode)
            .IsUnique()
            .HasDatabaseName("UK_tblZone_ZoneCode");

        builder.ConfigureAuditColumns();
    }
}
