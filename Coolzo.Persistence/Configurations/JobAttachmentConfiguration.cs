using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class JobAttachmentConfiguration : IEntityTypeConfiguration<JobAttachment>
{
    public void Configure(EntityTypeBuilder<JobAttachment> builder)
    {
        builder.ToTable("tblJobAttachment");
        builder.HasKey(entity => entity.JobAttachmentId).HasName("PK_tblJobAttachment_JobAttachmentId");

        builder.Property(entity => entity.JobAttachmentId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AttachmentType).HasConversion<int>();
        builder.Property(entity => entity.FileName).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.StoredFileName).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.RelativePath).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.AttachmentRemarks).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.UploadedDateUtc);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(jobCard => jobCard.Attachments)
            .HasForeignKey(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobAttachment_JobCardId_tblJobCard_JobCardId");

        builder.HasIndex(entity => new { entity.JobCardId, entity.AttachmentType })
            .HasDatabaseName("IDX_tblJobAttachment_JobCardId_AttachmentType");

        builder.ConfigureAuditColumns();
    }
}
