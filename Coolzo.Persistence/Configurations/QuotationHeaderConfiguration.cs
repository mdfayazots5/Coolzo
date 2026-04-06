using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class QuotationHeaderConfiguration : IEntityTypeConfiguration<QuotationHeader>
{
    public void Configure(EntityTypeBuilder<QuotationHeader> builder)
    {
        builder.ToTable("tblQuotationHeader");
        builder.HasKey(entity => entity.QuotationHeaderId).HasName("PK_tblQuotationHeader_QuotationHeaderId");

        builder.Property(entity => entity.QuotationHeaderId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.QuotationNumber).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.QuotationDateUtc);
        builder.Property(entity => entity.SubTotalAmount).HasColumnType("money");
        builder.Property(entity => entity.DiscountAmount).HasColumnType("money");
        builder.Property(entity => entity.TaxPercentage).HasColumnType("decimal(9,2)");
        builder.Property(entity => entity.TaxAmount).HasColumnType("money");
        builder.Property(entity => entity.GrandTotalAmount).HasColumnType("money");
        builder.Property(entity => entity.CustomerDecisionRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ApprovedDateUtc);
        builder.Property(entity => entity.RejectedDateUtc);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(jobCard => jobCard.Quotations)
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblQuotationHeader_JobCardId_tblJobCard_JobCardId");

        builder.HasOne(entity => entity.Customer)
            .WithMany(customer => customer.Quotations)
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblQuotationHeader_CustomerId_tblCustomer_CustomerId");

        builder.HasIndex(entity => entity.QuotationNumber)
            .IsUnique()
            .HasDatabaseName("UK_tblQuotationHeader_QuotationNumber");

        builder.HasIndex(entity => entity.JobCardId)
            .IsUnique()
            .HasDatabaseName("UK_tblQuotationHeader_JobCardId");

        builder.HasIndex(entity => new { entity.CustomerId, entity.QuotationDateUtc })
            .HasDatabaseName("IDX_tblQuotationHeader_CustomerId_QuotationDateUtc");

        builder.ConfigureAuditColumns();
    }
}
