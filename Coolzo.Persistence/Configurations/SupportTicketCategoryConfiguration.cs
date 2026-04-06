using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SupportTicketCategoryConfiguration : IEntityTypeConfiguration<SupportTicketCategory>
{
    public void Configure(EntityTypeBuilder<SupportTicketCategory> builder)
    {
        builder.ToTable("tblSupportTicketCategory");
        builder.HasKey(entity => entity.SupportTicketCategoryId).HasName("PK_tblSupportTicketCategory_SupportTicketCategoryId");

        builder.Property(entity => entity.SupportTicketCategoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.CategoryCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.CategoryName).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.IsActive).HasDefaultValue(true);

        builder.HasIndex(entity => entity.CategoryCode)
            .IsUnique()
            .HasDatabaseName("UK_tblSupportTicketCategory_CategoryCode");

        builder.ConfigureAuditColumns();
    }
}
