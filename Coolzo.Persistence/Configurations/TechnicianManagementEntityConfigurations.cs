using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class TechnicianSkillConfiguration : IEntityTypeConfiguration<TechnicianSkill>
{
    public void Configure(EntityTypeBuilder<TechnicianSkill> builder)
    {
        builder.ToTable("tblTechnicianSkill");
        builder.HasKey(entity => entity.TechnicianSkillId).HasName("PK_tblTechnicianSkill_TechnicianSkillId");
        builder.Property(entity => entity.TechnicianSkillId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.SkillCode).HasMaxLength(64).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.SkillName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.SkillCategory).HasMaxLength(32).HasDefaultValue("special");
        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.Skills)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianSkill_TechnicianId_tblTechnician_TechnicianId");
        builder.HasIndex(entity => new { entity.TechnicianId, entity.SkillName })
            .HasDatabaseName("UK_tblTechnicianSkill_TechnicianId_SkillName")
            .IsUnique();
        builder.ConfigureAuditColumns();
    }
}

public sealed class TechnicianZoneConfiguration : IEntityTypeConfiguration<TechnicianZone>
{
    public void Configure(EntityTypeBuilder<TechnicianZone> builder)
    {
        builder.ToTable("tblTechnicianZone");
        builder.HasKey(entity => entity.TechnicianZoneId).HasName("PK_tblTechnicianZone_TechnicianZoneId");
        builder.Property(entity => entity.TechnicianZoneId).ValueGeneratedOnAdd();
        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.Zones)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianZone_TechnicianId_tblTechnician_TechnicianId");
        builder.HasOne(entity => entity.Zone)
            .WithMany()
            .HasForeignKey(entity => entity.ZoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianZone_ZoneId_tblZone_ZoneId");
        builder.HasIndex(entity => new { entity.TechnicianId, entity.ZoneId })
            .HasDatabaseName("UK_tblTechnicianZone_TechnicianId_ZoneId")
            .IsUnique();
        builder.ConfigureAuditColumns();
    }
}

public sealed class TechnicianAttendanceConfiguration : IEntityTypeConfiguration<TechnicianAttendance>
{
    public void Configure(EntityTypeBuilder<TechnicianAttendance> builder)
    {
        builder.ToTable("tblTechnicianAttendance");
        builder.HasKey(entity => entity.TechnicianAttendanceId).HasName("PK_tblTechnicianAttendance_TechnicianAttendanceId");
        builder.Property(entity => entity.TechnicianAttendanceId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AttendanceStatus).HasMaxLength(32).HasDefaultValue("Pending");
        builder.Property(entity => entity.LocationText).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.LeaveReason).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.Attendances)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianAttendance_TechnicianId_tblTechnician_TechnicianId");
        builder.HasOne(entity => entity.ReviewedByUser)
            .WithMany()
            .HasForeignKey(entity => entity.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianAttendance_ReviewedByUserId_tblUser_UserId");
        builder.HasIndex(entity => new { entity.TechnicianId, entity.AttendanceDate })
            .HasDatabaseName("UK_tblTechnicianAttendance_TechnicianId_AttendanceDate")
            .IsUnique();
        builder.ConfigureAuditColumns();
    }
}

public sealed class TechnicianGpsLogConfiguration : IEntityTypeConfiguration<TechnicianGpsLog>
{
    public void Configure(EntityTypeBuilder<TechnicianGpsLog> builder)
    {
        builder.ToTable("tblTechnicianGPSLog");
        builder.HasKey(entity => entity.TechnicianGpsLogId).HasName("PK_tblTechnicianGPSLog_TechnicianGpsLogId");
        builder.Property(entity => entity.TechnicianGpsLogId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.Latitude).HasColumnType("decimal(10,6)");
        builder.Property(entity => entity.Longitude).HasColumnType("decimal(10,6)");
        builder.Property(entity => entity.TrackingSource).HasMaxLength(32).HasDefaultValue("system");
        builder.Property(entity => entity.LocationText).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.GpsLogs)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianGPSLog_TechnicianId_tblTechnician_TechnicianId");
        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianGPSLog_ServiceRequestId_tblServiceRequest_ServiceRequestId");
        builder.HasIndex(entity => new { entity.TechnicianId, entity.TrackedOnUtc })
            .HasDatabaseName("IDX_tblTechnicianGPSLog_TechnicianId_TrackedOnUtc");
        builder.ConfigureAuditColumns();
    }
}

public sealed class TechnicianPerformanceSummaryConfiguration : IEntityTypeConfiguration<TechnicianPerformanceSummary>
{
    public void Configure(EntityTypeBuilder<TechnicianPerformanceSummary> builder)
    {
        builder.ToTable("tblTechnicianPerformanceSummary");
        builder.HasKey(entity => entity.TechnicianPerformanceSummaryId).HasName("PK_tblTechnicianPerformanceSummary_TechnicianPerformanceSummaryId");
        builder.Property(entity => entity.TechnicianPerformanceSummaryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AverageRating).HasColumnType("decimal(5,2)");
        builder.Property(entity => entity.SlaCompliancePercent).HasColumnType("decimal(5,2)");
        builder.Property(entity => entity.RevisitRatePercent).HasColumnType("decimal(5,2)");
        builder.Property(entity => entity.RevenueGenerated).HasColumnType("decimal(18,2)");
        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.PerformanceSummaries)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianPerformanceSummary_TechnicianId_tblTechnician_TechnicianId");
        builder.HasIndex(entity => new { entity.TechnicianId, entity.SummaryDate })
            .HasDatabaseName("UK_tblTechnicianPerformanceSummary_TechnicianId_SummaryDate")
            .IsUnique();
        builder.ConfigureAuditColumns();
    }
}
