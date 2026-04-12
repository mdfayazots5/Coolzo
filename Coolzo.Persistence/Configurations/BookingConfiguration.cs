using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("tblBooking");
        builder.HasKey(entity => entity.BookingId).HasName("PK_tblBooking_BookingId");

        builder.Property(entity => entity.BookingId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.BookingReference).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.IdempotencyKey).HasMaxLength(128);
        builder.Property(entity => entity.BookingDateUtc);
        builder.Property(entity => entity.BookingStatus).HasConversion<int>();
        builder.Property(entity => entity.SourceChannel).HasConversion<int>();
        builder.Property(entity => entity.IsEmergency).HasDefaultValue(false);
        builder.Property(entity => entity.EmergencySurchargeAmount).HasColumnType("money").HasDefaultValue(0);
        builder.Property(entity => entity.CustomerNameSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.MobileNumberSnapshot).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.EmailAddressSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.AddressLine1Snapshot).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.AddressLine2Snapshot).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.LandmarkSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.CityNameSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.PincodeSnapshot).HasMaxLength(16).IsRequired();
        builder.Property(entity => entity.ZoneNameSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ServiceNameSnapshot).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.EstimatedPrice).HasColumnType("money");

        builder.HasOne(entity => entity.Customer)
            .WithMany(customer => customer.Bookings)
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBooking_CustomerId_tblCustomer_CustomerId");

        builder.HasOne(entity => entity.CustomerAddress)
            .WithMany(address => address.Bookings)
            .HasForeignKey(entity => entity.CustomerAddressId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBooking_CustomerAddressId_tblCustomerAddress_CustomerAddressId");

        builder.HasOne(entity => entity.Zone)
            .WithMany(zone => zone.Bookings)
            .HasForeignKey(entity => entity.ZoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBooking_ZoneId_tblZone_ZoneId");

        builder.HasOne(entity => entity.SlotAvailability)
            .WithMany(slotAvailability => slotAvailability.Bookings)
            .HasForeignKey(entity => entity.SlotAvailabilityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBooking_SlotAvailabilityId_tblSlotAvailability_SlotAvailabilityId");

        builder.HasIndex(entity => entity.BookingReference)
            .IsUnique()
            .HasDatabaseName("UK_tblBooking_BookingReference");

        builder.HasIndex(entity => entity.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL")
            .HasDatabaseName("UK_tblBooking_IdempotencyKey");

        builder.HasIndex(entity => new { entity.CustomerId, entity.BookingDateUtc })
            .HasDatabaseName("IDX_tblBooking_CustomerId_BookingDateUtc");

        builder.ConfigureAuditColumns();
    }
}
