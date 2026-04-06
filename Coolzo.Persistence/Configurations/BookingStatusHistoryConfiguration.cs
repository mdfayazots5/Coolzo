using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class BookingStatusHistoryConfiguration : IEntityTypeConfiguration<BookingStatusHistory>
{
    public void Configure(EntityTypeBuilder<BookingStatusHistory> builder)
    {
        builder.ToTable("tblBookingStatusHistory");
        builder.HasKey(entity => entity.BookingStatusHistoryId).HasName("PK_tblBookingStatusHistory_BookingStatusHistoryId");

        builder.Property(entity => entity.BookingStatusHistoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.BookingStatus).HasConversion<int>();
        builder.Property(entity => entity.Remarks).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.StatusDateUtc);

        builder.HasOne(entity => entity.Booking)
            .WithMany(booking => booking.BookingStatusHistories)
            .HasForeignKey(entity => entity.BookingId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBookingStatusHistory_BookingId_tblBooking_BookingId");

        builder.HasIndex(entity => new { entity.BookingId, entity.StatusDateUtc })
            .HasDatabaseName("IDX_tblBookingStatusHistory_BookingId_StatusDateUtc");

        builder.ConfigureAuditColumns();
    }
}
