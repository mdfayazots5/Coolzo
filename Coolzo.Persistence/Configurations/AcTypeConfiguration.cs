using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class AcTypeConfiguration : IEntityTypeConfiguration<AcType>
{
    public void Configure(EntityTypeBuilder<AcType> builder)
    {
        builder.ToTable("tblAcType");
        builder.HasKey(entity => entity.AcTypeId).HasName("PK_tblAcType_AcTypeId");

        builder.Property(entity => entity.AcTypeId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AcTypeCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.AcTypeName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.AcTypeCode)
            .IsUnique()
            .HasDatabaseName("UK_tblAcType_AcTypeCode");

        builder.ConfigureAuditColumns();
    }
}
