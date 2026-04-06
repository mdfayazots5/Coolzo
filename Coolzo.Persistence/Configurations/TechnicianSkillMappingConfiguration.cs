using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class TechnicianSkillMappingConfiguration : IEntityTypeConfiguration<TechnicianSkillMapping>
{
    public void Configure(EntityTypeBuilder<TechnicianSkillMapping> builder)
    {
        builder.ToTable("tblTechnicianSkillMapping");
        builder.HasKey(entity => entity.TechnicianSkillMappingId).HasName("PK_tblTechnicianSkillMapping_TechnicianSkillMappingId");

        builder.Property(entity => entity.TechnicianSkillMappingId).ValueGeneratedOnAdd();

        builder.HasOne(entity => entity.Technician)
            .WithMany(technician => technician.SkillMappings)
            .HasForeignKey(entity => entity.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianSkillMapping_TechnicianId_tblTechnician_TechnicianId");

        builder.HasOne(entity => entity.Service)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianSkillMapping_ServiceId_tblService_ServiceId");

        builder.HasOne(entity => entity.AcType)
            .WithMany()
            .HasForeignKey(entity => entity.AcTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblTechnicianSkillMapping_AcTypeId_tblAcType_AcTypeId");

        builder.HasIndex(entity => new { entity.TechnicianId, entity.ServiceId, entity.AcTypeId })
            .HasDatabaseName("IDX_tblTechnicianSkillMapping_TechnicianId_ServiceId_AcTypeId");

        builder.ConfigureAuditColumns();
    }
}
