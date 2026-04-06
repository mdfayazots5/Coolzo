using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("tblCustomerAddress");
        builder.HasKey(entity => entity.CustomerAddressId).HasName("PK_tblCustomerAddress_CustomerAddressId");

        builder.Property(entity => entity.CustomerAddressId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AddressLabel).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.AddressLine1).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.AddressLine2).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Landmark).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.CityName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Pincode).HasMaxLength(16).IsRequired();
        builder.Property(entity => entity.IsDefault).HasDefaultValue(false);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasOne(entity => entity.Customer)
            .WithMany(customer => customer.CustomerAddresses)
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerAddress_CustomerId_tblCustomer_CustomerId");

        builder.HasOne(entity => entity.Zone)
            .WithMany(zone => zone.CustomerAddresses)
            .HasForeignKey(entity => entity.ZoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblCustomerAddress_ZoneId_tblZone_ZoneId");

        builder.HasIndex(entity => new { entity.CustomerId, entity.Pincode })
            .HasDatabaseName("IDX_tblCustomerAddress_CustomerId_Pincode");

        builder.ConfigureAuditColumns();
    }
}
