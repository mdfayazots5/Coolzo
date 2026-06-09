using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

internal sealed class PublishedSnapshotConfiguration : IEntityTypeConfiguration<PublishedSnapshot>
{
    public void Configure(EntityTypeBuilder<PublishedSnapshot> builder)
    {
        builder.ToTable("tblPublishedSnapshot");
        builder.HasKey(entity => entity.PublishedSnapshotId)
            .HasName("PK_tblPublishedSnapshot_PublishedSnapshotId");
        builder.Property(entity => entity.PublishedSnapshotId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.Version).IsRequired();
        builder.Property(entity => entity.BucketKey).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.BucketUrl).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.ChecksumHash).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.PayloadSizeBytes).IsRequired();
        builder.Property(entity => entity.IsActive).HasDefaultValue(false);
        builder.HasIndex(entity => entity.Version)
            .IsUnique()
            .HasDatabaseName("UK_tblPublishedSnapshot_Version");
        builder.HasIndex(entity => entity.IsActive)
            .HasDatabaseName("IDX_tblPublishedSnapshot_IsActive");
        builder.ConfigureAuditColumns();
    }
}
