using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class JobExecutionTimelineConfiguration : IEntityTypeConfiguration<JobExecutionTimeline>
{
    public void Configure(EntityTypeBuilder<JobExecutionTimeline> builder)
    {
        builder.ToTable("tblJobExecutionTimeline");
        builder.HasKey(entity => entity.JobExecutionTimelineId).HasName("PK_tblJobExecutionTimeline_JobExecutionTimelineId");

        builder.Property(entity => entity.JobExecutionTimelineId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.Status).HasConversion<int>();
        builder.Property(entity => entity.EventType).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.EventTitle).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Remarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.EventDateUtc);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(jobCard => jobCard.ExecutionTimelines)
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobExecutionTimeline_JobCardId_tblJobCard_JobCardId");

        builder.HasIndex(entity => new { entity.JobCardId, entity.EventDateUtc })
            .HasDatabaseName("IDX_tblJobExecutionTimeline_JobCardId_EventDateUtc");

        builder.ConfigureAuditColumns();
    }
}
