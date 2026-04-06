using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SupportTicketPriorityConfiguration : IEntityTypeConfiguration<SupportTicketPriority>
{
    public void Configure(EntityTypeBuilder<SupportTicketPriority> builder)
    {
        builder.ToTable("tblSupportTicketPriority");
        builder.HasKey(entity => entity.SupportTicketPriorityId).HasName("PK_tblSupportTicketPriority_SupportTicketPriorityId");

        builder.Property(entity => entity.SupportTicketPriorityId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PriorityCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.PriorityName).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.PriorityRank).HasDefaultValue(0);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.PriorityCode)
            .IsUnique()
            .HasDatabaseName("UK_tblSupportTicketPriority_PriorityCode");

        builder.HasIndex(entity => new { entity.PriorityRank, entity.SortOrder })
            .HasDatabaseName("IDX_tblSupportTicketPriority_PriorityRank_SortOrder");

        builder.ConfigureAuditColumns();
    }
}
