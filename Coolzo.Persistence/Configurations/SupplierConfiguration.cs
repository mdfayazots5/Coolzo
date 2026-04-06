using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("tblSupplier");
        builder.HasKey(entity => entity.SupplierId).HasName("PK_tblSupplier_SupplierId");

        builder.Property(entity => entity.SupplierId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.SupplierCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.SupplierName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ContactPerson).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.MobileNumber).HasMaxLength(32).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.EmailAddress).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AddressLine).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.SupplierCode)
            .IsUnique()
            .HasDatabaseName("UK_tblSupplier_SupplierCode");

        builder.HasIndex(entity => new { entity.IsActive, entity.SupplierName })
            .HasDatabaseName("IDX_tblSupplier_IsActive_SupplierName");

        builder.ConfigureAuditColumns();
    }
}
