using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("tblServiceCategory");
        builder.HasKey(entity => entity.ServiceCategoryId).HasName("PK_tblServiceCategory_ServiceCategoryId");

        builder.Property(entity => entity.ServiceCategoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CategoryCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CategoryName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.CategoryCode)
            .IsUnique()
            .HasDatabaseName("UK_tblServiceCategory_CategoryCode");

        builder.HasIndex(entity => entity.CategoryName)
            .HasDatabaseName("IDX_tblServiceCategory_CategoryName");

        builder.ConfigureAuditColumns();
    }
}
