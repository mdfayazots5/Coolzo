using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class TonnageConfiguration : IEntityTypeConfiguration<Tonnage>
{
    public void Configure(EntityTypeBuilder<Tonnage> builder)
    {
        builder.ToTable("tblTonnage");
        builder.HasKey(entity => entity.TonnageId).HasName("PK_tblTonnage_TonnageId");

        builder.Property(entity => entity.TonnageId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.TonnageCode).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.TonnageName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.TonnageCode)
            .IsUnique()
            .HasDatabaseName("UK_tblTonnage_TonnageCode");

        builder.ConfigureAuditColumns();
    }
}
