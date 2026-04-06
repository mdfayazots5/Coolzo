using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("tblStockTransaction");
        builder.HasKey(entity => entity.StockTransactionId).HasName("PK_tblStockTransaction_StockTransactionId");

        builder.Property(entity => entity.StockTransactionId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.TransactionType).HasConversion<int>();
        builder.Property(entity => entity.Quantity).HasPrecision(18, 2);
        builder.Property(entity => entity.UnitCost).HasColumnType("money");
        builder.Property(entity => entity.Amount).HasColumnType("money");
        builder.Property(entity => entity.ReferenceNumber).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.TransactionGroupCode).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.BalanceAfterTransaction).HasPrecision(18, 2);

        builder.HasOne(entity => entity.Item)
            .WithMany(item => item.StockTransactions)
            .HasForeignKey(entity => entity.ItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblStockTransaction_ItemId_tblItem_ItemId");

        builder.HasOne(entity => entity.Warehouse)
            .WithMany(warehouse => warehouse.StockTransactions)
            .HasForeignKey(entity => entity.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblStockTransaction_WarehouseId_tblWarehouse_WarehouseId");

        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.StockTransactions)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblStockTransaction_TechnicianId_tblTechnician_TechnicianId");

        builder.HasOne(entity => entity.JobCard)
            .WithMany()
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblStockTransaction_JobCardId_tblJobCard_JobCardId");

        builder.HasOne(entity => entity.Supplier)
            .WithMany(supplier => supplier.StockTransactions)
            .HasForeignKey(entity => entity.SupplierId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblStockTransaction_SupplierId_tblSupplier_SupplierId");

        builder.HasIndex(entity => new { entity.ItemId, entity.TransactionDateUtc })
            .HasDatabaseName("IDX_tblStockTransaction_ItemId_TransactionDateUtc");

        builder.HasIndex(entity => new { entity.WarehouseId, entity.TransactionDateUtc })
            .HasDatabaseName("IDX_tblStockTransaction_WarehouseId_TransactionDateUtc");

        builder.HasIndex(entity => new { entity.TechnicianId, entity.TransactionDateUtc })
            .HasDatabaseName("IDX_tblStockTransaction_TechnicianId_TransactionDateUtc");

        builder.HasIndex(entity => entity.TransactionGroupCode)
            .HasDatabaseName("IDX_tblStockTransaction_TransactionGroupCode");

        builder.ConfigureAuditColumns();
    }
}
