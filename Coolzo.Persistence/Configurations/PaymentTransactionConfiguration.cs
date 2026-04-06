using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("tblPaymentTransaction");
        builder.HasKey(entity => entity.PaymentTransactionId).HasName("PK_tblPaymentTransaction_PaymentTransactionId");

        builder.Property(entity => entity.PaymentTransactionId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PaymentMethod).HasConversion<int>();
        builder.Property(entity => entity.ReferenceNumber).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IdempotencyKey).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.GatewayTransactionId).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.GatewaySignature).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.WebhookReference).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.PaidAmount).HasColumnType("money");
        builder.Property(entity => entity.PaymentDateUtc);
        builder.Property(entity => entity.TransactionRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.InvoiceHeader)
            .WithMany(invoice => invoice.PaymentTransactions)
            .HasForeignKey(entity => entity.InvoiceHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblPaymentTransaction_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId");

        builder.HasIndex(entity => new { entity.InvoiceHeaderId, entity.PaymentDateUtc })
            .HasDatabaseName("IDX_tblPaymentTransaction_InvoiceHeaderId_PaymentDateUtc");

        builder.HasIndex(entity => entity.IdempotencyKey)
            .HasDatabaseName("IDX_tblPaymentTransaction_IdempotencyKey");

        builder.HasIndex(entity => entity.GatewayTransactionId)
            .HasDatabaseName("IDX_tblPaymentTransaction_GatewayTransactionId");

        builder.ConfigureAuditColumns();
    }
}
