using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.ToTable("tblServiceRequest");
        builder.HasKey(entity => entity.ServiceRequestId).HasName("PK_tblServiceRequest_ServiceRequestId");

        builder.Property(entity => entity.ServiceRequestId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ServiceRequestNumber).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.ServiceRequestDateUtc);

        builder.HasOne(entity => entity.Booking)
            .WithOne(booking => booking.ServiceRequest)
            .HasForeignKey<ServiceRequest>(entity => entity.BookingId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblServiceRequest_BookingId_tblBooking_BookingId");

        builder.HasIndex(entity => entity.ServiceRequestNumber)
            .IsUnique()
            .HasDatabaseName("UK_tblServiceRequest_ServiceRequestNumber");

        builder.HasIndex(entity => entity.BookingId)
            .IsUnique()
            .HasDatabaseName("UK_tblServiceRequest_BookingId");

        builder.ConfigureAuditColumns();
    }
}
