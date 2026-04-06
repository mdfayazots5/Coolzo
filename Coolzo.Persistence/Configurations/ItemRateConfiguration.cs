using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ItemRateConfiguration : IEntityTypeConfiguration<ItemRate>
{
    public void Configure(EntityTypeBuilder<ItemRate> builder)
    {
        builder.ToTable("tblItemRate");
        builder.HasKey(entity => entity.ItemRateId).HasName("PK_tblItemRate_ItemRateId");

        builder.Property(entity => entity.ItemRateId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PurchasePrice).HasColumnType("money");
        builder.Property(entity => entity.SellingPrice).HasColumnType("money");
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasOne(entity => entity.Item)
            .WithMany(item => item.Rates)
            .HasForeignKey(entity => entity.ItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblItemRate_ItemId_tblItem_ItemId");

        builder.HasIndex(entity => new { entity.ItemId, entity.EffectiveFromUtc })
            .HasDatabaseName("IDX_tblItemRate_ItemId_EffectiveFromUtc");

        builder.ConfigureAuditColumns();
    }
}
