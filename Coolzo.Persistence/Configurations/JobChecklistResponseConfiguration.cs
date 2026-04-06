using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class JobChecklistResponseConfiguration : IEntityTypeConfiguration<JobChecklistResponse>
{
    public void Configure(EntityTypeBuilder<JobChecklistResponse> builder)
    {
        builder.ToTable("tblJobChecklistResponse");
        builder.HasKey(entity => entity.JobChecklistResponseId).HasName("PK_tblJobChecklistResponse_JobChecklistResponseId");

        builder.Property(entity => entity.JobChecklistResponseId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ResponseRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ResponseDateUtc);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(jobCard => jobCard.ChecklistResponses)
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobChecklistResponse_JobCardId_tblJobCard_JobCardId");

        builder.HasOne(entity => entity.ServiceChecklistMaster)
            .WithMany(master => master.ChecklistResponses)
            .HasForeignKey(entity => entity.ServiceChecklistMasterId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobChecklistResponse_ServiceChecklistMasterId_tblServiceChecklistMaster_ServiceChecklistMasterId");

        builder.HasIndex(entity => new { entity.JobCardId, entity.ServiceChecklistMasterId })
            .IsUnique()
            .HasDatabaseName("UK_tblJobChecklistResponse_JobCardId_ServiceChecklistMasterId");

        builder.ConfigureAuditColumns();
    }
}
