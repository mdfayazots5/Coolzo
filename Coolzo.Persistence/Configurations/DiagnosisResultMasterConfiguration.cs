using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class DiagnosisResultMasterConfiguration : IEntityTypeConfiguration<DiagnosisResultMaster>
{
    public void Configure(EntityTypeBuilder<DiagnosisResultMaster> builder)
    {
        builder.ToTable("tblDiagnosisResultMaster");
        builder.HasKey(entity => entity.DiagnosisResultMasterId).HasName("PK_tblDiagnosisResultMaster_DiagnosisResultMasterId");

        builder.Property(entity => entity.DiagnosisResultMasterId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ResultName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ResultDescription).HasMaxLength(256).HasDefaultValue(string.Empty);

        builder.HasIndex(entity => entity.ResultName)
            .HasDatabaseName("UK_tblDiagnosisResultMaster_ResultName")
            .IsUnique();

        builder.ConfigureAuditColumns();
    }
}
