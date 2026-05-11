using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("tblPurchaseOrder");
        builder.HasKey(entity => entity.PurchaseOrderId).HasName("PK_tblPurchaseOrder_PurchaseOrderId");

        builder.Property(entity => entity.PurchaseOrderId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PONumber).HasMaxLength(64);
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.SubtotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(entity => entity.TaxAmount).HasColumnType("numeric(18,2)");
        builder.Property(entity => entity.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(entity => entity.Notes).HasMaxLength(1024);

        builder.HasOne(entity => entity.Supplier)
            .WithMany()
            .HasForeignKey(entity => entity.SupplierId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblPurchaseOrder_SupplierId_tblSupplier_SupplierId");

        builder.HasIndex(entity => entity.PONumber)
            .IsUnique()
            .HasDatabaseName("UK_tblPurchaseOrder_PONumber");

        builder.HasIndex(entity => new { entity.CurrentStatus, entity.ExpectedDeliveryDateUtc })
            .HasDatabaseName("IDX_tblPurchaseOrder_CurrentStatus_ExpectedDeliveryDateUtc");

        builder.ConfigureAuditColumns();
    }
}

public sealed class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("tblPurchaseOrderItem");
        builder.HasKey(entity => entity.PurchaseOrderItemId).HasName("PK_tblPurchaseOrderItem_PurchaseOrderItemId");

        builder.Property(entity => entity.PurchaseOrderItemId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PartCode).HasMaxLength(64);
        builder.Property(entity => entity.PartName).HasMaxLength(256);
        builder.Property(entity => entity.QuantityOrdered).HasPrecision(12, 2);
        builder.Property(entity => entity.QuantityReceived).HasPrecision(12, 2);
        builder.Property(entity => entity.UnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(entity => entity.Amount).HasColumnType("numeric(18,2)");

        builder.HasOne(entity => entity.PurchaseOrder)
            .WithMany(entity => entity.Items)
            .HasForeignKey(entity => entity.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_tblPurchaseOrderItem_PurchaseOrderId_tblPurchaseOrder_PurchaseOrderId");

        builder.HasOne(entity => entity.Item)
            .WithMany()
            .HasForeignKey(entity => entity.ItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblPurchaseOrderItem_ItemId_tblItem_ItemId");

        builder.HasIndex(entity => entity.PurchaseOrderId)
            .HasDatabaseName("IDX_tblPurchaseOrderItem_PurchaseOrderId");

        builder.HasIndex(entity => entity.ItemId)
            .HasDatabaseName("IDX_tblPurchaseOrderItem_ItemId");

        builder.ConfigureAuditColumns();
    }
}
