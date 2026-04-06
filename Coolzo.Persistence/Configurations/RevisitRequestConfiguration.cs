using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class RevisitRequestConfiguration : IEntityTypeConfiguration<RevisitRequest>
{
    public void Configure(EntityTypeBuilder<RevisitRequest> builder)
    {
        builder.ToTable("tblRevisitRequest");
        builder.HasKey(entity => entity.RevisitRequestId).HasName("PK_tblRevisitRequest_RevisitRequestId");

        builder.Property(entity => entity.RevisitRequestId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.RevisitType).HasConversion<int>();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.RequestedDateUtc);
        builder.Property(entity => entity.PreferredVisitDateUtc);
        builder.Property(entity => entity.IssueSummary).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.RequestRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ChargeAmount).HasColumnType("money");

        builder.HasOne(entity => entity.Booking)
            .WithMany()
            .HasForeignKey(entity => entity.BookingId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRevisitRequest_BookingId_tblBooking_BookingId");

        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRevisitRequest_CustomerId_tblCustomer_CustomerId");

        builder.HasOne(entity => entity.OriginalServiceRequest)
            .WithMany()
            .HasForeignKey(entity => entity.OriginalServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRevisitRequest_OriginalServiceRequestId_tblServiceRequest_ServiceRequestId");

        builder.HasOne(entity => entity.OriginalJobCard)
            .WithMany()
            .HasForeignKey(entity => entity.OriginalJobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRevisitRequest_OriginalJobCardId_tblJobCard_JobCardId");

        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRevisitRequest_ServiceRequestId_tblServiceRequest_ServiceRequestId");

        builder.HasOne(entity => entity.CustomerAmc)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerAmcId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblRevisitRequest_CustomerAmcId_tblCustomerAMC_CustomerAmcId");

        builder.HasIndex(entity => new { entity.BookingId, entity.RequestedDateUtc })
            .HasDatabaseName("IDX_tblRevisitRequest_BookingId_RequestedDateUtc");

        builder.ConfigureAuditColumns();
    }
}
