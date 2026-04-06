using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class InvoiceHeaderConfiguration : IEntityTypeConfiguration<InvoiceHeader>
{
    public void Configure(EntityTypeBuilder<InvoiceHeader> builder)
    {
        builder.ToTable("tblInvoiceHeader");
        builder.HasKey(entity => entity.InvoiceHeaderId).HasName("PK_tblInvoiceHeader_InvoiceHeaderId");

        builder.Property(entity => entity.InvoiceHeaderId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.InvoiceNumber).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.InvoiceDateUtc);
        builder.Property(entity => entity.SubTotalAmount).HasColumnType("money");
        builder.Property(entity => entity.DiscountAmount).HasColumnType("money");
        builder.Property(entity => entity.TaxPercentage).HasColumnType("decimal(9,2)");
        builder.Property(entity => entity.TaxAmount).HasColumnType("money");
        builder.Property(entity => entity.GrandTotalAmount).HasColumnType("money");
        builder.Property(entity => entity.PaidAmount).HasColumnType("money");
        builder.Property(entity => entity.BalanceAmount).HasColumnType("money");
        builder.Property(entity => entity.LastPaymentDateUtc);

        builder.HasOne(entity => entity.QuotationHeader)
            .WithOne(quotation => quotation.InvoiceHeader)
            .HasForeignKey<InvoiceHeader>(entity => entity.QuotationHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInvoiceHeader_QuotationHeaderId_tblQuotationHeader_QuotationHeaderId");

        builder.HasOne(entity => entity.Customer)
            .WithMany(customer => customer.Invoices)
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInvoiceHeader_CustomerId_tblCustomer_CustomerId");

        builder.HasIndex(entity => entity.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("UK_tblInvoiceHeader_InvoiceNumber");

        builder.HasIndex(entity => entity.QuotationHeaderId)
            .IsUnique()
            .HasDatabaseName("UK_tblInvoiceHeader_QuotationHeaderId");

        builder.HasIndex(entity => new { entity.CustomerId, entity.InvoiceDateUtc })
            .HasDatabaseName("IDX_tblInvoiceHeader_CustomerId_InvoiceDateUtc");

        builder.ConfigureAuditColumns();
    }
}
