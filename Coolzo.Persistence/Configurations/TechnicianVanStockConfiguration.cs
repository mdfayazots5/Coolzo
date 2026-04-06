using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class TechnicianVanStockConfiguration : IEntityTypeConfiguration<TechnicianVanStock>
{
    public void Configure(EntityTypeBuilder<TechnicianVanStock> builder)
    {
        builder.ToTable("tblTechnicianVanStock");
        builder.HasKey(entity => entity.TechnicianVanStockId).HasName("PK_tblTechnicianVanStock_TechnicianVanStockId");

        builder.Property(entity => entity.TechnicianVanStockId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.QuantityOnHand).HasPrecision(18, 2).HasDefaultValue(0.00m);

        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.VanStocks)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianVanStock_TechnicianId_tblTechnician_TechnicianId");

        builder.HasOne(entity => entity.Item)
            .WithMany(item => item.TechnicianVanStocks)
            .HasForeignKey(entity => entity.ItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianVanStock_ItemId_tblItem_ItemId");

        builder.HasIndex(entity => new { entity.TechnicianId, entity.ItemId })
            .IsUnique()
            .HasDatabaseName("UK_tblTechnicianVanStock_TechnicianId_ItemId");

        builder.HasIndex(entity => new { entity.TechnicianId, entity.LastTransactionDateUtc })
            .HasDatabaseName("IDX_tblTechnicianVanStock_TechnicianId_LastTransactionDateUtc");

        builder.ConfigureAuditColumns();
    }
}
