using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class JobReportConfiguration : IEntityTypeConfiguration<JobReport>
{
    public void Configure(EntityTypeBuilder<JobReport> builder)
    {
        builder.ToTable("tblJobReport");
        builder.HasKey(entity => entity.JobReportId);
        builder.Property(entity => entity.JobReportId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.IdempotencyKey).HasMaxLength(128);
        builder.Property(entity => entity.EquipmentCondition).HasMaxLength(64);
        builder.Property(entity => entity.IssuesIdentifiedJson).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.ActionTaken).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.Recommendation).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.Observations).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.QualityScore).HasPrecision(5, 2);

        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany(entity => entity.JobReports)
            .HasForeignKey(entity => entity.ServiceRequestId);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(entity => entity.JobReports)
            .HasForeignKey(entity => entity.JobCardId);

        builder.HasOne(entity => entity.Technician)
            .WithMany(entity => entity.JobReports)
            .HasForeignKey(entity => entity.TechnicianId);

        builder.HasIndex(entity => new { entity.ServiceRequestId, entity.SubmittedAtUtc })
            .HasDatabaseName("IDX_tblJobReport_ServiceRequestId_SubmittedAtUtc");
        builder.HasIndex(entity => entity.IdempotencyKey)
            .HasDatabaseName("IDX_tblJobReport_IdempotencyKey");
    }
}

public sealed class JobPhotoConfiguration : IEntityTypeConfiguration<JobPhoto>
{
    public void Configure(EntityTypeBuilder<JobPhoto> builder)
    {
        builder.ToTable("tblJobPhoto");
        builder.HasKey(entity => entity.JobPhotoId);
        builder.Property(entity => entity.JobPhotoId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PhotoType).HasConversion<int>();
        builder.Property(entity => entity.FileName).HasMaxLength(256);
        builder.Property(entity => entity.ContentType).HasMaxLength(128);
        builder.Property(entity => entity.StorageUrl).HasMaxLength(512);
        builder.Property(entity => entity.UploadedBy).HasMaxLength(128);
        builder.Property(entity => entity.PhotoRemarks).HasMaxLength(512);

        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany(entity => entity.JobPhotos)
            .HasForeignKey(entity => entity.ServiceRequestId);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(entity => entity.JobPhotos)
            .HasForeignKey(entity => entity.JobCardId);

        builder.HasOne(entity => entity.Technician)
            .WithMany(entity => entity.JobPhotos)
            .HasForeignKey(entity => entity.TechnicianId);

        builder.HasOne(entity => entity.JobReport)
            .WithMany(entity => entity.Photos)
            .HasForeignKey(entity => entity.JobReportId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class CustomerSignatureConfiguration : IEntityTypeConfiguration<CustomerSignature>
{
    public void Configure(EntityTypeBuilder<CustomerSignature> builder)
    {
        builder.ToTable("tblCustomerSignature");
        builder.HasKey(entity => entity.CustomerSignatureId);
        builder.Property(entity => entity.CustomerSignatureId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CustomerName).HasMaxLength(128);
        builder.Property(entity => entity.SignatureDataUrl).HasColumnType("nvarchar(max)");
        builder.Property(entity => entity.CapturedBy).HasMaxLength(128);
        builder.Property(entity => entity.SignatureRemarks).HasMaxLength(512);

        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany(entity => entity.CustomerSignatures)
            .HasForeignKey(entity => entity.ServiceRequestId);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(entity => entity.CustomerSignatures)
            .HasForeignKey(entity => entity.JobCardId);

        builder.HasOne(entity => entity.Technician)
            .WithMany(entity => entity.CustomerSignatures)
            .HasForeignKey(entity => entity.TechnicianId);

        builder.HasOne(entity => entity.JobReport)
            .WithMany(entity => entity.Signatures)
            .HasForeignKey(entity => entity.JobReportId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public sealed class PartsRequestConfiguration : IEntityTypeConfiguration<PartsRequest>
{
    public void Configure(EntityTypeBuilder<PartsRequest> builder)
    {
        builder.ToTable("tblPartsRequest");
        builder.HasKey(entity => entity.PartsRequestId);
        builder.Property(entity => entity.PartsRequestId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.Urgency).HasConversion<int>();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.Property(entity => entity.Notes).HasMaxLength(1024);

        builder.HasOne(entity => entity.ServiceRequest)
            .WithMany(entity => entity.PartsRequests)
            .HasForeignKey(entity => entity.ServiceRequestId);

        builder.HasOne(entity => entity.JobCard)
            .WithMany(entity => entity.PartsRequests)
            .HasForeignKey(entity => entity.JobCardId);

        builder.HasOne(entity => entity.Technician)
            .WithMany(entity => entity.PartsRequests)
            .HasForeignKey(entity => entity.TechnicianId);
    }
}

public sealed class PartsRequestItemConfiguration : IEntityTypeConfiguration<PartsRequestItem>
{
    public void Configure(EntityTypeBuilder<PartsRequestItem> builder)
    {
        builder.ToTable("tblPartsRequestItem");
        builder.HasKey(entity => entity.PartsRequestItemId);
        builder.Property(entity => entity.PartsRequestItemId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PartCode).HasMaxLength(64);
        builder.Property(entity => entity.PartName).HasMaxLength(256);
        builder.Property(entity => entity.QuantityRequested).HasPrecision(12, 2);
        builder.Property(entity => entity.QuantityApproved).HasPrecision(12, 2);
        builder.Property(entity => entity.ItemRemarks).HasMaxLength(512);
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();
        builder.HasIndex(entity => entity.ItemId)
            .HasDatabaseName("IDX_tblPartsRequestItem_ItemId");

        builder.HasOne(entity => entity.PartsRequest)
            .WithMany(entity => entity.Items)
            .HasForeignKey(entity => entity.PartsRequestId);

        builder.HasOne(entity => entity.Item)
            .WithMany()
            .HasForeignKey(entity => entity.ItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
