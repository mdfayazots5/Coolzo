using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("tblUnitOfMeasure");
        builder.HasKey(entity => entity.UnitOfMeasureId).HasName("PK_tblUnitOfMeasure_UnitOfMeasureId");

        builder.Property(entity => entity.UnitOfMeasureId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.UnitCode).HasMaxLength(16).IsRequired();
        builder.Property(entity => entity.UnitName).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.UnitCode)
            .IsUnique()
            .HasDatabaseName("UK_tblUnitOfMeasure_UnitCode");

        builder.HasIndex(entity => new { entity.IsActive, entity.UnitName })
            .HasDatabaseName("IDX_tblUnitOfMeasure_IsActive_UnitName");

        builder.ConfigureAuditColumns();
    }
}
