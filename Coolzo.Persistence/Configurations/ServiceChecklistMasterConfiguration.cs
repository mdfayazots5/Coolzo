using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ServiceChecklistMasterConfiguration : IEntityTypeConfiguration<ServiceChecklistMaster>
{
    public void Configure(EntityTypeBuilder<ServiceChecklistMaster> builder)
    {
        builder.ToTable("tblServiceChecklistMaster");
        builder.HasKey(entity => entity.ServiceChecklistMasterId).HasName("PK_tblServiceChecklistMaster_ServiceChecklistMasterId");

        builder.Property(entity => entity.ServiceChecklistMasterId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ChecklistTitle).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ChecklistDescription).HasMaxLength(256).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.Service)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblServiceChecklistMaster_ServiceId_tblService_ServiceId");

        builder.HasIndex(entity => new { entity.ServiceId, entity.ChecklistTitle })
            .HasDatabaseName("UK_tblServiceChecklistMaster_ServiceId_ChecklistTitle")
            .IsUnique();

        builder.ConfigureAuditColumns();
    }
}
