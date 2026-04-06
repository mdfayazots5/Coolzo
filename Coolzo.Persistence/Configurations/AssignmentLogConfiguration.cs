using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class AssignmentLogConfiguration : IEntityTypeConfiguration<AssignmentLog>
{
    public void Configure(EntityTypeBuilder<AssignmentLog> builder)
    {
        builder.ToTable("tblAssignmentLog");
        builder.HasKey(entity => entity.AssignmentLogId).HasName("PK_tblAssignmentLog_AssignmentLogId");

        builder.Property(entity => entity.AssignmentLogId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ActionName).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Remarks).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ActionDateUtc);

        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany(serviceRequest => serviceRequest.AssignmentLogs)
            .HasForeignKey(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblAssignmentLog_ServiceRequestId_tblServiceRequest_ServiceRequestId");

        builder.HasOne(entity => entity.PreviousTechnician)
            .WithMany(technician => technician.PreviousAssignmentLogs)
            .HasForeignKey(entity => entity.PreviousTechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblAssignmentLog_PreviousTechnicianId_tblTechnician_TechnicianId");

        builder.HasOne(entity => entity.CurrentTechnician)
            .WithMany(technician => technician.CurrentAssignmentLogs)
            .HasForeignKey(entity => entity.CurrentTechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblAssignmentLog_CurrentTechnicianId_tblTechnician_TechnicianId");

        builder.HasIndex(entity => new { entity.ServiceRequestId, entity.ActionDateUtc })
            .HasDatabaseName("IDX_tblAssignmentLog_ServiceRequestId_ActionDateUtc");

        builder.ConfigureAuditColumns();
    }
}
