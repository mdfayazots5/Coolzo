using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SupportTicketLinkConfiguration : IEntityTypeConfiguration<SupportTicketLink>
{
    public void Configure(EntityTypeBuilder<SupportTicketLink> builder)
    {
        builder.ToTable("tblSupportTicketLink");
        builder.HasKey(entity => entity.SupportTicketLinkId).HasName("PK_tblSupportTicketLink_SupportTicketLinkId");

        builder.Property(entity => entity.SupportTicketLinkId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.LinkedEntityType).HasConversion<int>();
        builder.Property(entity => entity.LinkReference).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.LinkSummary).HasMaxLength(256).IsRequired();

        builder.HasOne(entity => entity.SupportTicket)
            .WithMany(supportTicket => supportTicket.Links)
            .HasForeignKey(entity => entity.SupportTicketId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSupportTicketLink_SupportTicketId_tblSupportTicket_SupportTicketId");

        builder.HasIndex(entity => new { entity.SupportTicketId, entity.LinkedEntityType, entity.LinkedEntityId })
            .HasDatabaseName("IDX_tblSupportTicketLink_SupportTicketId_LinkedEntityType_LinkedEntityId");

        builder.ConfigureAuditColumns();
    }
}
