using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class QuotationLineConfiguration : IEntityTypeConfiguration<QuotationLine>
{
    public void Configure(EntityTypeBuilder<QuotationLine> builder)
    {
        builder.ToTable("tblQuotationLine");
        builder.HasKey(entity => entity.QuotationLineId).HasName("PK_tblQuotationLine_QuotationLineId");

        builder.Property(entity => entity.QuotationLineId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.LineType).HasConversion<int>();
        builder.Property(entity => entity.LineDescription).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Quantity).HasColumnType("decimal(18,2)");
        builder.Property(entity => entity.UnitPrice).HasColumnType("money");
        builder.Property(entity => entity.LineAmount).HasColumnType("money");

        builder.HasOne(entity => entity.QuotationHeader)
            .WithMany(header => header.Lines)
            .HasForeignKey(entity => entity.QuotationHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblQuotationLine_QuotationHeaderId_tblQuotationHeader_QuotationHeaderId");

        builder.HasIndex(entity => entity.QuotationHeaderId)
            .HasDatabaseName("IDX_tblQuotationLine_QuotationHeaderId");

        builder.ConfigureAuditColumns();
    }
}
