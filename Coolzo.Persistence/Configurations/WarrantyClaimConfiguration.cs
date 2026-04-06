using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class WarrantyClaimConfiguration : IEntityTypeConfiguration<WarrantyClaim>
{
    public void Configure(EntityTypeBuilder<WarrantyClaim> builder)
    {
        builder.ToTable("tblWarrantyClaim");
        builder.HasKey(entity => entity.WarrantyClaimId).HasName("PK_tblWarrantyClaim_WarrantyClaimId");

        builder.Property(entity => entity.WarrantyClaimId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.ClaimDateUtc);
        builder.Property(entity => entity.CoverageStartDateUtc);
        builder.Property(entity => entity.CoverageEndDateUtc);
        builder.Property(entity => entity.IsEligible).HasDefaultValue(false);
        builder.Property(entity => entity.ClaimRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.InvoiceHeader)
            .WithMany()
            .HasForeignKey(entity => entity.InvoiceHeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblWarrantyClaim_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId");

        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblWarrantyClaim_CustomerId_tblCustomer_CustomerId");

        builder.HasOne(entity => entity.JobCard)
            .WithMany()
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblWarrantyClaim_JobCardId_tblJobCard_JobCardId");

        builder.HasOne(entity => entity.WarrantyRule)
            .WithMany(rule => rule.WarrantyClaims)
            .HasForeignKey(entity => entity.WarrantyRuleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblWarrantyClaim_WarrantyRuleId_tblWarrantyRule_WarrantyRuleId");

        builder.HasOne(entity => entity.RevisitRequest)
            .WithOne(revisitRequest => revisitRequest.WarrantyClaim)
            .HasForeignKey<RevisitRequest>(revisitRequest => revisitRequest.WarrantyClaimId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRevisitRequest_WarrantyClaimId_tblWarrantyClaim_WarrantyClaimId");

        builder.HasIndex(entity => new { entity.InvoiceHeaderId, entity.CurrentStatus, entity.ClaimDateUtc })
            .HasDatabaseName("IDX_tblWarrantyClaim_InvoiceHeaderId_CurrentStatus_ClaimDateUtc");

        builder.ConfigureAuditColumns();
    }
}
