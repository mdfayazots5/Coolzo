using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SupportTicketStatusHistoryConfiguration : IEntityTypeConfiguration<SupportTicketStatusHistory>
{
    public void Configure(EntityTypeBuilder<SupportTicketStatusHistory> builder)
    {
        builder.ToTable("tblSupportTicketStatusHistory");
        builder.HasKey(entity => entity.SupportTicketStatusHistoryId).HasName("PK_tblSupportTicketStatusHistory_SupportTicketStatusHistoryId");

        builder.Property(entity => entity.SupportTicketStatusHistoryId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.SupportTicketStatus).HasConversion<int>();
        builder.Property(entity => entity.Remarks).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.StatusDateUtc);

        builder.HasOne(entity => entity.SupportTicket)
            .WithMany(supportTicket => supportTicket.StatusHistories)
            .HasForeignKey(entity => entity.SupportTicketId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSupportTicketStatusHistory_SupportTicketId_tblSupportTicket_SupportTicketId");

        builder.HasIndex(entity => new { entity.SupportTicketId, entity.StatusDateUtc })
            .HasDatabaseName("IDX_tblSupportTicketStatusHistory_SupportTicketId_StatusDateUtc");

        builder.ConfigureAuditColumns();
    }
}
