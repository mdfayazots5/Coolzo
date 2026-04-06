using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class WarehouseStockConfiguration : IEntityTypeConfiguration<WarehouseStock>
{
    public void Configure(EntityTypeBuilder<WarehouseStock> builder)
    {
        builder.ToTable("tblWarehouseStock");
        builder.HasKey(entity => entity.WarehouseStockId).HasName("PK_tblWarehouseStock_WarehouseStockId");

        builder.Property(entity => entity.WarehouseStockId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.QuantityOnHand).HasPrecision(18, 2).HasDefaultValue(0.00m);

        builder.HasOne(entity => entity.Warehouse)
            .WithMany(warehouse => warehouse.WarehouseStocks)
            .HasForeignKey(entity => entity.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblWarehouseStock_WarehouseId_tblWarehouse_WarehouseId");

        builder.HasOne(entity => entity.Item)
            .WithMany(item => item.WarehouseStocks)
            .HasForeignKey(entity => entity.ItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblWarehouseStock_ItemId_tblItem_ItemId");

        builder.HasIndex(entity => new { entity.WarehouseId, entity.ItemId })
            .IsUnique()
            .HasDatabaseName("UK_tblWarehouseStock_WarehouseId_ItemId");

        builder.HasIndex(entity => new { entity.WarehouseId, entity.LastTransactionDateUtc })
            .HasDatabaseName("IDX_tblWarehouseStock_WarehouseId_LastTransactionDateUtc");

        builder.ConfigureAuditColumns();
    }
}
