using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class BillingStatusHistoryConfiguration : IEntityTypeConfiguration<BillingStatusHistory>
{
    public void Configure(EntityTypeBuilder<BillingStatusHistory> builder)
    {
        builder.ToTable("tblBillingStatusHistory");
        builder.HasKey(entity => entity.BillingStatusHistoryId).HasName("PK_tblBillingStatusHistory_BillingStatusHistoryId");

        builder.Property(entity => entity.BillingStatusHistoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.EntityType).HasConversion<int>();
        builder.Property(entity => entity.StatusName).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.StatusDateUtc);

        builder.HasOne(entity => entity.QuotationHeader)
            .WithMany(header => header.BillingStatusHistories)
            .HasForeignKey(entity => entity.QuotationHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBillingStatusHistory_QuotationHeaderId_tblQuotationHeader_QuotationHeaderId");

        builder.HasOne(entity => entity.InvoiceHeader)
            .WithMany(header => header.BillingStatusHistories)
            .HasForeignKey(entity => entity.InvoiceHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBillingStatusHistory_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId");

        builder.HasOne(entity => entity.PaymentTransaction)
            .WithMany(transaction => transaction.BillingStatusHistories)
            .HasForeignKey(entity => entity.PaymentTransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBillingStatusHistory_PaymentTransactionId_tblPaymentTransaction_PaymentTransactionId");

        builder.HasIndex(entity => new { entity.InvoiceHeaderId, entity.StatusDateUtc })
            .HasDatabaseName("IDX_tblBillingStatusHistory_InvoiceHeaderId_StatusDateUtc");

        builder.ConfigureAuditColumns();
    }
}
