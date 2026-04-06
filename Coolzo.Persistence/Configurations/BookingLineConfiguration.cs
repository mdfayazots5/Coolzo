using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class BookingLineConfiguration : IEntityTypeConfiguration<BookingLine>
{
    public void Configure(EntityTypeBuilder<BookingLine> builder)
    {
        builder.ToTable("tblBookingLine");
        builder.HasKey(entity => entity.BookingLineId).HasName("PK_tblBookingLine_BookingLineId");

        builder.Property(entity => entity.BookingLineId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ModelName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IssueNotes).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.Quantity).HasDefaultValue(1);
        builder.Property(entity => entity.UnitPrice).HasColumnType("money");
        builder.Property(entity => entity.LineTotal).HasColumnType("money");

        builder.HasOne(entity => entity.Booking)
            .WithMany(booking => booking.BookingLines)
            .HasForeignKey(entity => entity.BookingId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBookingLine_BookingId_tblBooking_BookingId");

        builder.HasOne(entity => entity.Service)
            .WithMany(service => service.BookingLines)
            .HasForeignKey(entity => entity.ServiceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBookingLine_ServiceId_tblService_ServiceId");

        builder.HasOne(entity => entity.AcType)
            .WithMany(acType => acType.BookingLines)
            .HasForeignKey(entity => entity.AcTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBookingLine_AcTypeId_tblAcType_AcTypeId");

        builder.HasOne(entity => entity.Tonnage)
            .WithMany(tonnage => tonnage.BookingLines)
            .HasForeignKey(entity => entity.TonnageId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBookingLine_TonnageId_tblTonnage_TonnageId");

        builder.HasOne(entity => entity.Brand)
            .WithMany(brand => brand.BookingLines)
            .HasForeignKey(entity => entity.BrandId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblBookingLine_BrandId_tblBrand_BrandId");

        builder.ConfigureAuditColumns();
    }
}
