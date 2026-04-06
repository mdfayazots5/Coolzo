using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class AmcPlanConfiguration : IEntityTypeConfiguration<AmcPlan>
{
    public void Configure(EntityTypeBuilder<AmcPlan> builder)
    {
        builder.ToTable("tblAMCPlan");
        builder.HasKey(entity => entity.AmcPlanId).HasName("PK_tblAMCPlan_AmcPlanId");

        builder.Property(entity => entity.AmcPlanId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PlanName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.PlanDescription).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.DurationInMonths);
        builder.Property(entity => entity.VisitCount);
        builder.Property(entity => entity.PriceAmount).HasColumnType("money");
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);
        builder.Property(entity => entity.TermsAndConditions).HasMaxLength(512).HasDefaultValue(string.Empty);

        builder.HasIndex(entity => entity.PlanName)
            .IsUnique()
            .HasDatabaseName("UK_tblAMCPlan_PlanName");

        builder.HasIndex(entity => new { entity.IsActive, entity.PriceAmount })
            .HasDatabaseName("IDX_tblAMCPlan_IsActive_PriceAmount");

        builder.ConfigureAuditColumns();
    }
}
