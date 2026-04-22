using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class TechnicianShiftConfiguration : IEntityTypeConfiguration<TechnicianShift>
{
    public void Configure(EntityTypeBuilder<TechnicianShift> builder)
    {
        builder.ToTable("tblTechnicianShift");
        builder.HasKey(entity => entity.TechnicianShiftId).HasName("PK_tblTechnicianShift_TechnicianShiftId");

        builder.Property(entity => entity.TechnicianShiftId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.DayOfWeekNumber).IsRequired();
        builder.Property(entity => entity.ShiftStartTimeLocal).HasColumnType("time");
        builder.Property(entity => entity.ShiftEndTimeLocal).HasColumnType("time");
        builder.Property(entity => entity.BreakStartTimeLocal).HasColumnType("time");
        builder.Property(entity => entity.BreakEndTimeLocal).HasColumnType("time");
        builder.Property(entity => entity.IsOffDuty).HasDefaultValue(false);

        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.Shifts)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianShift_TechnicianId_tblTechnician_TechnicianId");

        builder.HasIndex(entity => new { entity.TechnicianId, entity.DayOfWeekNumber })
            .IsUnique()
            .HasDatabaseName("UK_tblTechnicianShift_TechnicianId_DayOfWeekNumber");

        builder.ConfigureAuditColumns();
    }
}
