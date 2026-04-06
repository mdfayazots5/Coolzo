using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class WarrantyRuleConfiguration : IEntityTypeConfiguration<WarrantyRule>
{
    public void Configure(EntityTypeBuilder<WarrantyRule> builder)
    {
        builder.ToTable("tblWarrantyRule");
        builder.HasKey(entity => entity.WarrantyRuleId).HasName("PK_tblWarrantyRule_WarrantyRuleId");

        builder.Property(entity => entity.WarrantyRuleId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.RuleName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.WarrantyDurationDays);
        builder.Property(entity => entity.CoverageDescription).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasOne(entity => entity.Service)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblWarrantyRule_ServiceId_tblService_ServiceId");

        builder.HasOne(entity => entity.AcType)
            .WithMany()
            .HasForeignKey(entity => entity.AcTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblWarrantyRule_AcTypeId_tblAcType_AcTypeId");

        builder.HasOne(entity => entity.Brand)
            .WithMany()
            .HasForeignKey(entity => entity.BrandId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblWarrantyRule_BrandId_tblBrand_BrandId");

        builder.HasIndex(entity => entity.RuleName)
            .IsUnique()
            .HasDatabaseName("UK_tblWarrantyRule_RuleName");

        builder.HasIndex(entity => new { entity.ServiceId, entity.AcTypeId, entity.BrandId, entity.IsActive })
            .HasDatabaseName("IDX_tblWarrantyRule_ServiceId_AcTypeId_BrandId");

        builder.ConfigureAuditColumns();
    }
}
