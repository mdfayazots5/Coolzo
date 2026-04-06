using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("tblItem");
        builder.HasKey(entity => entity.ItemId).HasName("PK_tblItem_ItemId");

        builder.Property(entity => entity.ItemId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ItemCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.ItemName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ItemDescription).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.TaxPercentage).HasPrecision(18, 2).HasDefaultValue(0.00m);
        builder.Property(entity => entity.WarrantyDays).HasDefaultValue(0);
        builder.Property(entity => entity.ReorderLevel).HasPrecision(18, 2).HasDefaultValue(0.00m);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasOne(entity => entity.ItemCategory)
            .WithMany(itemCategory => itemCategory.Items)
            .HasForeignKey(entity => entity.ItemCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblItem_ItemCategoryId_tblItemCategory_ItemCategoryId");

        builder.HasOne(entity => entity.UnitOfMeasure)
            .WithMany(unitOfMeasure => unitOfMeasure.Items)
            .HasForeignKey(entity => entity.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblItem_UnitOfMeasureId_tblUnitOfMeasure_UnitOfMeasureId");

        builder.HasOne(entity => entity.Supplier)
            .WithMany(supplier => supplier.Items)
            .HasForeignKey(entity => entity.SupplierId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblItem_SupplierId_tblSupplier_SupplierId");

        builder.HasIndex(entity => entity.ItemCode)
            .IsUnique()
            .HasDatabaseName("UK_tblItem_ItemCode");

        builder.HasIndex(entity => new { entity.IsActive, entity.ItemName })
            .HasDatabaseName("IDX_tblItem_IsActive_ItemName");

        builder.ConfigureAuditColumns();
    }
}
