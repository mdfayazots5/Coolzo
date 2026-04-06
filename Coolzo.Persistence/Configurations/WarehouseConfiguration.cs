using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("tblWarehouse");
        builder.HasKey(entity => entity.WarehouseId).HasName("PK_tblWarehouse_WarehouseId");

        builder.Property(entity => entity.WarehouseId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.WarehouseCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.WarehouseName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ContactPerson).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.MobileNumber).HasMaxLength(32).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.EmailAddress).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AddressLine1).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AddressLine2).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Landmark).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.CityName).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Pincode).HasMaxLength(16).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.WarehouseCode)
            .IsUnique()
            .HasDatabaseName("UK_tblWarehouse_WarehouseCode");

        builder.HasIndex(entity => new { entity.IsActive, entity.WarehouseName })
            .HasDatabaseName("IDX_tblWarehouse_IsActive_WarehouseName");

        builder.ConfigureAuditColumns();
    }
}
