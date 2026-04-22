using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.ToTable("tblOtpVerification");
        builder.HasKey(entity => entity.OtpVerificationId).HasName("PK_tblOtpVerification_OtpVerificationId");

        builder.Property(entity => entity.OtpVerificationId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.OtpCode).HasMaxLength(16).IsRequired();
        builder.Property(entity => entity.Purpose).HasMaxLength(64).IsRequired();

        builder.HasIndex(entity => entity.UserId).HasDatabaseName("IDX_tblOtpVerification_UserId");

        builder.ConfigureAuditColumns(includeBranchId: false);
    }
}
