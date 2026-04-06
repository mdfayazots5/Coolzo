using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class TechnicianActivationLogConfiguration : IEntityTypeConfiguration<TechnicianActivationLog>
{
    public void Configure(EntityTypeBuilder<TechnicianActivationLog> builder)
    {
        builder.ToTable("tblTechnicianActivationLog");
        builder.HasKey(entity => entity.TechnicianActivationLogId).HasName("PK_tblTechnicianActivationLog_TechnicianActivationLogId");
        builder.Property(entity => entity.TechnicianActivationLogId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ActivationAction).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.ActivationReason).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.EligibilitySnapshot).HasMaxLength(4000).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Technician)
            .WithMany()
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianActivationLog_TechnicianId_tblTechnician_TechnicianId");
        builder.HasIndex(entity => new { entity.TechnicianId, entity.ActivatedOnUtc }).HasDatabaseName("IDX_tblTechnicianActivationLog_TechnicianId_ActivatedOnUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class HelperProfileConfiguration : IEntityTypeConfiguration<HelperProfile>
{
    public void Configure(EntityTypeBuilder<HelperProfile> builder)
    {
        builder.ToTable("tblHelperProfile");
        builder.HasKey(entity => entity.HelperProfileId).HasName("PK_tblHelperProfile_HelperProfileId");
        builder.Property(entity => entity.HelperProfileId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.HelperCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.HelperName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.MobileNo).HasMaxLength(32).IsRequired();
        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblHelperProfile_UserId_tblUser_UserId");
        builder.HasIndex(entity => entity.HelperCode).IsUnique().HasDatabaseName("UK_tblHelperProfile_HelperCode");
        builder.HasIndex(entity => entity.UserId).IsUnique().HasDatabaseName("UK_tblHelperProfile_UserId");
        builder.ConfigureAuditColumns();
    }
}

public sealed class HelperAssignmentConfiguration : IEntityTypeConfiguration<HelperAssignment>
{
    public void Configure(EntityTypeBuilder<HelperAssignment> builder)
    {
        builder.ToTable("tblHelperAssignment");
        builder.HasKey(entity => entity.HelperAssignmentId).HasName("PK_tblHelperAssignment_HelperAssignmentId");
        builder.Property(entity => entity.HelperAssignmentId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AssignmentStatus).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.AssignmentRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.HelperProfile)
            .WithMany(entity => entity.Assignments)
            .HasForeignKey(entity => entity.HelperProfileId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblHelperAssignment_HelperProfileId_tblHelperProfile_HelperProfileId");
        builder.HasOne(entity => entity.Technician)
            .WithMany()
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblHelperAssignment_TechnicianId_tblTechnician_TechnicianId");
        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblHelperAssignment_ServiceRequestId_tblServiceRequest_ServiceRequestId");
        builder.HasOne(entity => entity.JobCard)
            .WithMany()
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblHelperAssignment_JobCardId_tblJobCard_JobCardId");
        builder.HasIndex(entity => new { entity.HelperProfileId, entity.AssignmentStatus }).HasDatabaseName("IDX_tblHelperAssignment_HelperProfileId_AssignmentStatus");
        builder.ConfigureAuditColumns();
    }
}

public sealed class HelperTaskChecklistConfiguration : IEntityTypeConfiguration<HelperTaskChecklist>
{
    public void Configure(EntityTypeBuilder<HelperTaskChecklist> builder)
    {
        builder.ToTable("tblHelperTaskChecklist");
        builder.HasKey(entity => entity.HelperTaskChecklistId).HasName("PK_tblHelperTaskChecklist_HelperTaskChecklistId");
        builder.Property(entity => entity.HelperTaskChecklistId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.TaskName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.TaskDescription).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasIndex(entity => new { entity.ServiceTypeId, entity.SortOrder }).HasDatabaseName("IDX_tblHelperTaskChecklist_ServiceTypeId_SortOrder");
        builder.ConfigureAuditColumns();
    }
}

public sealed class HelperTaskResponseConfiguration : IEntityTypeConfiguration<HelperTaskResponse>
{
    public void Configure(EntityTypeBuilder<HelperTaskResponse> builder)
    {
        builder.ToTable("tblHelperTaskResponse");
        builder.HasKey(entity => entity.HelperTaskResponseId).HasName("PK_tblHelperTaskResponse_HelperTaskResponseId");
        builder.Property(entity => entity.HelperTaskResponseId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ResponseStatus).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.ResponseRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ResponsePhotoUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.HelperAssignment)
            .WithMany(entity => entity.TaskResponses)
            .HasForeignKey(entity => entity.HelperAssignmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblHelperTaskResponse_HelperAssignmentId_tblHelperAssignment_HelperAssignmentId");
        builder.HasOne(entity => entity.HelperTaskChecklist)
            .WithMany(entity => entity.Responses)
            .HasForeignKey(entity => entity.HelperTaskChecklistId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblHelperTaskResponse_HelperTaskChecklistId_tblHelperTaskChecklist_HelperTaskChecklistId");
        builder.HasIndex(entity => new { entity.HelperAssignmentId, entity.HelperTaskChecklistId }).IsUnique().HasDatabaseName("UK_tblHelperTaskResponse_HelperAssignment_Checklist");
        builder.ConfigureAuditColumns();
    }
}

public sealed class HelperAttendanceConfiguration : IEntityTypeConfiguration<HelperAttendance>
{
    public void Configure(EntityTypeBuilder<HelperAttendance> builder)
    {
        builder.ToTable("tblHelperAttendance");
        builder.HasKey(entity => entity.HelperAttendanceId).HasName("PK_tblHelperAttendance_HelperAttendanceId");
        builder.Property(entity => entity.HelperAttendanceId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AttendanceStatus).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.LocationText).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.HelperProfile)
            .WithMany(entity => entity.Attendances)
            .HasForeignKey(entity => entity.HelperProfileId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblHelperAttendance_HelperProfileId_tblHelperProfile_HelperProfileId");
        builder.HasIndex(entity => new { entity.HelperProfileId, entity.AttendanceDate }).HasDatabaseName("IDX_tblHelperAttendance_HelperProfileId_AttendanceDate");
        builder.ConfigureAuditColumns();
    }
}
