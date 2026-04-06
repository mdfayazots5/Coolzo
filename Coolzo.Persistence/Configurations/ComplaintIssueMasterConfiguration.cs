using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ComplaintIssueMasterConfiguration : IEntityTypeConfiguration<ComplaintIssueMaster>
{
    public void Configure(EntityTypeBuilder<ComplaintIssueMaster> builder)
    {
        builder.ToTable("tblComplaintIssueMaster");
        builder.HasKey(entity => entity.ComplaintIssueMasterId).HasName("PK_tblComplaintIssueMaster_ComplaintIssueMasterId");

        builder.Property(entity => entity.ComplaintIssueMasterId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.IssueName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IssueDescription).HasMaxLength(256).HasDefaultValue(string.Empty);

        builder.HasOne(entity => entity.Service)
            .WithMany()
            .HasForeignKey(entity => entity.ServiceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblComplaintIssueMaster_ServiceId_tblService_ServiceId");

        builder.HasIndex(entity => new { entity.ServiceId, entity.IssueName })
            .HasDatabaseName("IDX_tblComplaintIssueMaster_ServiceId_IssueName");

        builder.ConfigureAuditColumns();
    }
}
