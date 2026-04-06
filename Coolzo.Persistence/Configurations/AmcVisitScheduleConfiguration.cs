using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class AmcVisitScheduleConfiguration : IEntityTypeConfiguration<AmcVisitSchedule>
{
    public void Configure(EntityTypeBuilder<AmcVisitSchedule> builder)
    {
        builder.ToTable("tblAMCVisitSchedule");
        builder.HasKey(entity => entity.AmcVisitScheduleId).HasName("PK_tblAMCVisitSchedule_AmcVisitScheduleId");

        builder.Property(entity => entity.AmcVisitScheduleId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.VisitNumber);
        builder.Property(entity => entity.ScheduledDate).HasColumnType("date");
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.CompletedDateUtc);
        builder.Property(entity => entity.VisitRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.CustomerAmc)
            .WithMany(customerAmc => customerAmc.Visits)
            .HasForeignKey(entity => entity.CustomerAmcId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblAMCVisitSchedule_CustomerAmcId_tblCustomerAMC_CustomerAmcId");

        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblAMCVisitSchedule_ServiceRequestId_tblServiceRequest_ServiceRequestId");

        builder.HasIndex(entity => new { entity.CustomerAmcId, entity.VisitNumber })
            .IsUnique()
            .HasDatabaseName("UK_tblAMCVisitSchedule_CustomerAmcId_VisitNumber");

        builder.HasIndex(entity => new { entity.ScheduledDate, entity.CurrentStatus })
            .HasDatabaseName("IDX_tblAMCVisitSchedule_ScheduledDate_CurrentStatus");

        builder.ConfigureAuditColumns();
    }
}
