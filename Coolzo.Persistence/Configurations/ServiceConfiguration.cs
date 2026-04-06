using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("tblService");
        builder.HasKey(entity => entity.ServiceId).HasName("PK_tblService_ServiceId");

        builder.Property(entity => entity.ServiceId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ServiceCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.ServiceName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Summary).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.EstimatedDurationInMinutes).HasDefaultValue(60);
        builder.Property(entity => entity.BasePrice).HasColumnType("money");
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasOne(entity => entity.ServiceCategory)
            .WithMany(category => category.Services)
            .HasForeignKey(entity => entity.ServiceCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblService_ServiceCategoryId_tblServiceCategory_ServiceCategoryId");

        builder.HasOne(entity => entity.PricingModel)
            .WithMany(pricingModel => pricingModel.Services)
            .HasForeignKey(entity => entity.PricingModelId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblService_PricingModelId_tblPricingModel_PricingModelId");

        builder.HasIndex(entity => entity.ServiceCode)
            .IsUnique()
            .HasDatabaseName("UK_tblService_ServiceCode");

        builder.HasIndex(entity => new { entity.ServiceCategoryId, entity.ServiceName })
            .HasDatabaseName("IDX_tblService_ServiceCategoryId_ServiceName");

        builder.ConfigureAuditColumns();
    }
}
