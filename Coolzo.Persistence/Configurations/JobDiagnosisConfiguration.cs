using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class JobDiagnosisConfiguration : IEntityTypeConfiguration<JobDiagnosis>
{
    public void Configure(EntityTypeBuilder<JobDiagnosis> builder)
    {
        builder.ToTable("tblJobDiagnosis");
        builder.HasKey(entity => entity.JobDiagnosisId).HasName("PK_tblJobDiagnosis_JobDiagnosisId");

        builder.Property(entity => entity.JobDiagnosisId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.DiagnosisRemarks).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.DiagnosisDateUtc);

        builder.HasOne(entity => entity.JobCard)
            .WithOne(jobCard => jobCard.JobDiagnosis)
            .HasForeignKey<JobDiagnosis>(entity => entity.JobCardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobDiagnosis_JobCardId_tblJobCard_JobCardId");

        builder.HasOne(entity => entity.ComplaintIssueMaster)
            .WithMany(master => master.JobDiagnoses)
            .HasForeignKey(entity => entity.ComplaintIssueMasterId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobDiagnosis_ComplaintIssueMasterId_tblComplaintIssueMaster_ComplaintIssueMasterId");

        builder.HasOne(entity => entity.DiagnosisResultMaster)
            .WithMany(master => master.JobDiagnoses)
            .HasForeignKey(entity => entity.DiagnosisResultMasterId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobDiagnosis_DiagnosisResultMasterId_tblDiagnosisResultMaster_DiagnosisResultMasterId");

        builder.HasIndex(entity => entity.JobCardId)
            .IsUnique()
            .HasDatabaseName("UK_tblJobDiagnosis_JobCardId");

        builder.ConfigureAuditColumns();
    }
}
