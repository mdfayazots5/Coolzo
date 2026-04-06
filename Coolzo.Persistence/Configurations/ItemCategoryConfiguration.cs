using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ItemCategoryConfiguration : IEntityTypeConfiguration<ItemCategory>
{
    public void Configure(EntityTypeBuilder<ItemCategory> builder)
    {
        builder.ToTable("tblItemCategory");
        builder.HasKey(entity => entity.ItemCategoryId).HasName("PK_tblItemCategory_ItemCategoryId");

        builder.Property(entity => entity.ItemCategoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CategoryCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.CategoryName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.CategoryCode)
            .IsUnique()
            .HasDatabaseName("UK_tblItemCategory_CategoryCode");

        builder.HasIndex(entity => new { entity.IsActive, entity.CategoryName })
            .HasDatabaseName("IDX_tblItemCategory_IsActive_CategoryName");

        builder.ConfigureAuditColumns();
    }
}
