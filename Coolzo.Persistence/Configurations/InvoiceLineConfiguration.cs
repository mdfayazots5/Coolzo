using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("tblInvoiceLine");
        builder.HasKey(entity => entity.InvoiceLineId).HasName("PK_tblInvoiceLine_InvoiceLineId");

        builder.Property(entity => entity.InvoiceLineId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.LineType).HasConversion<int>();
        builder.Property(entity => entity.LineDescription).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Quantity).HasColumnType("decimal(18,2)");
        builder.Property(entity => entity.UnitPrice).HasColumnType("money");
        builder.Property(entity => entity.LineAmount).HasColumnType("money");

        builder.HasOne(entity => entity.InvoiceHeader)
            .WithMany(header => header.Lines)
            .HasForeignKey(entity => entity.InvoiceHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInvoiceLine_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId");

        builder.HasOne(entity => entity.QuotationLine)
            .WithMany()
            .HasForeignKey(entity => entity.QuotationLineId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblInvoiceLine_QuotationLineId_tblQuotationLine_QuotationLineId");

        builder.HasIndex(entity => entity.InvoiceHeaderId)
            .HasDatabaseName("IDX_tblInvoiceLine_InvoiceHeaderId");

        builder.ConfigureAuditColumns();
    }
}
