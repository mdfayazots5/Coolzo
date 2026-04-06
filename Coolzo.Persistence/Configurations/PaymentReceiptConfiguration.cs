using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceipt>
{
    public void Configure(EntityTypeBuilder<PaymentReceipt> builder)
    {
        builder.ToTable("tblPaymentReceipt");
        builder.HasKey(entity => entity.PaymentReceiptId).HasName("PK_tblPaymentReceipt_PaymentReceiptId");

        builder.Property(entity => entity.PaymentReceiptId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ReceiptNumber).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.ReceiptDateUtc);
        builder.Property(entity => entity.ReceivedAmount).HasColumnType("money");
        builder.Property(entity => entity.BalanceAmount).HasColumnType("money");
        builder.Property(entity => entity.ReceiptRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.InvoiceHeader)
            .WithMany()
            .HasForeignKey(entity => entity.InvoiceHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblPaymentReceipt_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId");

        builder.HasOne(entity => entity.PaymentTransaction)
            .WithOne(transaction => transaction.PaymentReceipt)
            .HasForeignKey<PaymentReceipt>(entity => entity.PaymentTransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblPaymentReceipt_PaymentTransactionId_tblPaymentTransaction_PaymentTransactionId");

        builder.HasIndex(entity => entity.ReceiptNumber)
            .IsUnique()
            .HasDatabaseName("UK_tblPaymentReceipt_ReceiptNumber");

        builder.HasIndex(entity => entity.PaymentTransactionId)
            .IsUnique()
            .HasDatabaseName("UK_tblPaymentReceipt_PaymentTransactionId");

        builder.ConfigureAuditColumns();
    }
}
