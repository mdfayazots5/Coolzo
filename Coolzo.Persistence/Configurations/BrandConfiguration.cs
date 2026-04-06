using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("tblBrand");
        builder.HasKey(entity => entity.BrandId).HasName("PK_tblBrand_BrandId");

        builder.Property(entity => entity.BrandId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.BrandCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.BrandName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.BrandCode)
            .IsUnique()
            .HasDatabaseName("UK_tblBrand_BrandCode");

        builder.ConfigureAuditColumns();
    }
}
