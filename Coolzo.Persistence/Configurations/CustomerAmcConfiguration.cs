using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class CustomerAmcConfiguration : IEntityTypeConfiguration<CustomerAmc>
{
    public void Configure(EntityTypeBuilder<CustomerAmc> builder)
    {
        builder.ToTable("tblCustomerAMC");
        builder.HasKey(entity => entity.CustomerAmcId).HasName("PK_tblCustomerAMC_CustomerAmcId");

        builder.Property(entity => entity.CustomerAmcId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.StartDateUtc);
        builder.Property(entity => entity.EndDateUtc);
        builder.Property(entity => entity.TotalVisitCount);
        builder.Property(entity => entity.ConsumedVisitCount).HasDefaultValue(0);
        builder.Property(entity => entity.PriceAmount).HasColumnType("money");

        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerAMC_CustomerId_tblCustomer_CustomerId");

        builder.HasOne(entity => entity.AmcPlan)
            .WithMany(plan => plan.CustomerAmcs)
            .HasForeignKey(entity => entity.AmcPlanId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerAMC_AmcPlanId_tblAMCPlan_AmcPlanId");

        builder.HasOne(entity => entity.JobCard)
            .WithMany()
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerAMC_JobCardId_tblJobCard_JobCardId");

        builder.HasOne(entity => entity.InvoiceHeader)
            .WithMany()
            .HasForeignKey(entity => entity.InvoiceHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerAMC_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId");

        builder.HasIndex(entity => entity.InvoiceHeaderId)
            .IsUnique()
            .HasDatabaseName("UK_tblCustomerAMC_InvoiceHeaderId");

        builder.HasIndex(entity => new { entity.CustomerId, entity.CurrentStatus, entity.EndDateUtc })
            .HasDatabaseName("IDX_tblCustomerAMC_CustomerId_CurrentStatus_EndDateUtc");

        builder.ConfigureAuditColumns();
    }
}
