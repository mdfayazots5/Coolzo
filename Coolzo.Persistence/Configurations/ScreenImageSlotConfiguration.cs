using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

internal sealed class ScreenImageSlotConfiguration : IEntityTypeConfiguration<ScreenImageSlot>
{
    public void Configure(EntityTypeBuilder<ScreenImageSlot> builder)
    {
        builder.ToTable("tblScreenImageSlot");
        builder.HasKey(entity => entity.ScreenImageSlotId)
            .HasName("PK_tblScreenImageSlot_ScreenImageSlotId");
        builder.Property(entity => entity.ScreenImageSlotId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.PageKey).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.SlotKey).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Breakpoint).HasMaxLength(16).IsRequired();
        builder.Property(entity => entity.RecommendedWidth).HasDefaultValue(0);
        builder.Property(entity => entity.RecommendedHeight).HasDefaultValue(0);
        builder.Property(entity => entity.AltText).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.SuggestedAIPrompt).HasMaxLength(1024).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.ImageUrl).HasMaxLength(512).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);
        builder.HasIndex(entity => new { entity.PageKey, entity.SlotKey, entity.Breakpoint })
            .IsUnique()
            .HasDatabaseName("UK_tblScreenImageSlot_PageKey_SlotKey_Breakpoint");
        builder.ConfigureAuditColumns();
    }
}
