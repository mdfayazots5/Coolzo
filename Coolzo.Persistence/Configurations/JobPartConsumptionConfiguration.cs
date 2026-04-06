using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class JobPartConsumptionConfiguration : IEntityTypeConfiguration<JobPartConsumption>
{
    public void Configure(EntityTypeBuilder<JobPartConsumption> builder)
    {
        builder.ToTable("tblJobPartConsumption");
        builder.HasKey(entity => entity.JobPartConsumptionId).HasName("PK_tblJobPartConsumption_JobPartConsumptionId");

        builder.Property(entity => entity.JobPartConsumptionId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.QuantityUsed).HasPrecision(18, 2);
        builder.Property(entity => entity.UnitPrice).HasColumnType("money");
        builder.Property(entity => entity.LineAmount).HasColumnType("money");
        builder.Property(entity => entity.ConsumptionRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(jobCard => jobCard.PartConsumptions)
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobPartConsumption_JobCardId_tblJobCard_JobCardId");

        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.JobPartConsumptions)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobPartConsumption_TechnicianId_tblTechnician_TechnicianId");

        builder.HasOne(entity => entity.Item)
            .WithMany(item => item.JobPartConsumptions)
            .HasForeignKey(entity => entity.ItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobPartConsumption_ItemId_tblItem_ItemId");

        builder.HasOne(entity => entity.StockTransaction)
            .WithMany(stockTransaction => stockTransaction.JobPartConsumptions)
            .HasForeignKey(entity => entity.StockTransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobPartConsumption_StockTransactionId_tblStockTransaction_StockTransactionId");

        builder.HasIndex(entity => new { entity.JobCardId, entity.ConsumedDateUtc })
            .HasDatabaseName("IDX_tblJobPartConsumption_JobCardId_ConsumedDateUtc");

        builder.ConfigureAuditColumns();
    }
}
