using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ServiceRequestAssignmentConfiguration : IEntityTypeConfiguration<ServiceRequestAssignment>
{
    public void Configure(EntityTypeBuilder<ServiceRequestAssignment> builder)
    {
        builder.ToTable("tblServiceRequestAssignment");
        builder.HasKey(entity => entity.ServiceRequestAssignmentId).HasName("PK_tblServiceRequestAssignment_ServiceRequestAssignmentId");

        builder.Property(entity => entity.ServiceRequestAssignmentId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AssignedDateUtc);
        builder.Property(entity => entity.UnassignedDateUtc);
        builder.Property(entity => entity.AssignmentRemarks).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.UnassignmentRemarks).HasMaxLength(256).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany(serviceRequest => serviceRequest.Assignments)
            .HasForeignKey(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblServiceRequestAssignment_ServiceRequestId_tblServiceRequest_ServiceRequestId");

        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.ServiceRequestAssignments)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblServiceRequestAssignment_TechnicianId_tblTechnician_TechnicianId");

        builder.HasIndex(entity => new { entity.ServiceRequestId, entity.IsActiveAssignment })
            .HasDatabaseName("IDX_tblServiceRequestAssignment_ServiceRequestId_IsActiveAssignment");

        builder.HasIndex(entity => new { entity.TechnicianId, entity.AssignedDateUtc })
            .HasDatabaseName("IDX_tblServiceRequestAssignment_TechnicianId_AssignedDateUtc");

        builder.ConfigureAuditColumns();
    }
}
