using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> builder)
    {
        builder.ToTable("tblTechnician");
        builder.HasKey(entity => entity.TechnicianId).HasName("PK_tblTechnician_TechnicianId");

        builder.Property(entity => entity.TechnicianId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.TechnicianCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.TechnicianName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.MobileNumber).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.EmailAddress).HasMaxLength(128).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.MaxDailyAssignments).HasDefaultValue(4);

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnician_UserId_tblUser_UserId");

        builder.HasOne(entity => entity.BaseZone)
            .WithMany()
            .HasForeignKey(entity => entity.BaseZoneId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnician_BaseZoneId_tblZone_ZoneId");

        builder.HasIndex(entity => entity.TechnicianCode)
            .IsUnique()
            .HasDatabaseName("UK_tblTechnician_TechnicianCode");

        builder.HasIndex(entity => new { entity.IsActive, entity.BaseZoneId })
            .HasDatabaseName("IDX_tblTechnician_IsActive_BaseZoneId");

        builder.ConfigureAuditColumns();
    }
}
