using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class JobCardConfiguration : IEntityTypeConfiguration<JobCard>
{
    public void Configure(EntityTypeBuilder<JobCard> builder)
    {
        builder.ToTable("tblJobCard");
        builder.HasKey(entity => entity.JobCardId).HasName("PK_tblJobCard_JobCardId");

        builder.Property(entity => entity.JobCardId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.JobCardNumber).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.WorkStartedDateUtc);
        builder.Property(entity => entity.WorkInProgressDateUtc);
        builder.Property(entity => entity.WorkCompletedDateUtc);
        builder.Property(entity => entity.SubmittedForClosureDateUtc);
        builder.Property(entity => entity.CompletionSummary).HasMaxLength(512).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.ServiceRequest)
            .WithOne(serviceRequest => serviceRequest.JobCard)
            .HasForeignKey<JobCard>(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblJobCard_ServiceRequestId_tblServiceRequest_ServiceRequestId");

        builder.HasIndex(entity => entity.JobCardNumber)
            .IsUnique()
            .HasDatabaseName("UK_tblJobCard_JobCardNumber");

        builder.HasIndex(entity => entity.ServiceRequestId)
            .IsUnique()
            .HasDatabaseName("UK_tblJobCard_ServiceRequestId");

        builder.ConfigureAuditColumns();
    }
}
