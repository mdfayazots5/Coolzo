using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class ServiceRequestStatusHistoryConfiguration : IEntityTypeConfiguration<ServiceRequestStatusHistory>
{
    public void Configure(EntityTypeBuilder<ServiceRequestStatusHistory> builder)
    {
        builder.ToTable("tblServiceRequestStatusHistory");
        builder.HasKey(entity => entity.ServiceRequestStatusHistoryId).HasName("PK_tblServiceRequestStatusHistory_ServiceRequestStatusHistoryId");

        builder.Property(entity => entity.ServiceRequestStatusHistoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.Status).HasConversion<int>();
        builder.Property(entity => entity.Remarks).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.StatusDateUtc);

        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany(serviceRequest => serviceRequest.StatusHistories)
            .HasForeignKey(entity => entity.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblServiceRequestStatusHistory_ServiceRequestId_tblServiceRequest_ServiceRequestId");

        builder.HasIndex(entity => new { entity.ServiceRequestId, entity.StatusDateUtc })
            .HasDatabaseName("IDX_tblServiceRequestStatusHistory_ServiceRequestId_StatusDateUtc");

        builder.ConfigureAuditColumns();
    }
}
