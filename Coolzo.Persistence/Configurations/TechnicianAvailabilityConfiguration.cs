using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class TechnicianAvailabilityConfiguration : IEntityTypeConfiguration<TechnicianAvailability>
{
    public void Configure(EntityTypeBuilder<TechnicianAvailability> builder)
    {
        builder.ToTable("tblTechnicianAvailability");
        builder.HasKey(entity => entity.TechnicianAvailabilityId).HasName("PK_tblTechnicianAvailability_TechnicianAvailabilityId");

        builder.Property(entity => entity.TechnicianAvailabilityId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AvailabilityRemarks).HasMaxLength(256).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.TechnicianAvailabilities)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianAvailability_TechnicianId_tblTechnician_TechnicianId");

        builder.HasIndex(entity => new { entity.TechnicianId, entity.AvailableDate })
            .IsUnique()
            .HasDatabaseName("UK_tblTechnicianAvailability_TechnicianId_AvailableDate");

        builder.ConfigureAuditColumns();
    }
}
