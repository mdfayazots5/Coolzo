using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class PricingModelConfiguration : IEntityTypeConfiguration<PricingModel>
{
    public void Configure(EntityTypeBuilder<PricingModel> builder)
    {
        builder.ToTable("tblPricingModel");
        builder.HasKey(entity => entity.PricingModelId).HasName("PK_tblPricingModel_PricingModelId");

        builder.Property(entity => entity.PricingModelId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PricingModelName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.BasePrice).HasColumnType("money");
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.PricingModelName)
            .IsUnique()
            .HasDatabaseName("UK_tblPricingModel_PricingModelName");

        builder.ConfigureAuditColumns();
    }
}
