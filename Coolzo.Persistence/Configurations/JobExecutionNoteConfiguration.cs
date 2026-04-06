using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class JobExecutionNoteConfiguration : IEntityTypeConfiguration<JobExecutionNote>
{
    public void Configure(EntityTypeBuilder<JobExecutionNote> builder)
    {
        builder.ToTable("tblJobExecutionNote");
        builder.HasKey(entity => entity.JobExecutionNoteId).HasName("PK_tblJobExecutionNote_JobExecutionNoteId");

        builder.Property(entity => entity.JobExecutionNoteId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.NoteText).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.NoteDateUtc);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(jobCard => jobCard.ExecutionNotes)
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobExecutionNote_JobCardId_tblJobCard_JobCardId");

        builder.HasIndex(entity => new { entity.JobCardId, entity.NoteDateUtc })
            .HasDatabaseName("IDX_tblJobExecutionNote_JobCardId_NoteDateUtc");

        builder.ConfigureAuditColumns();
    }
}
