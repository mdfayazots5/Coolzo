using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SupportTicketEscalationConfiguration : IEntityTypeConfiguration<SupportTicketEscalation>
{
    public void Configure(EntityTypeBuilder<SupportTicketEscalation> builder)
    {
        builder.ToTable("tblSupportTicketEscalation");
        builder.HasKey(entity => entity.SupportTicketEscalationId).HasName("PK_tblSupportTicketEscalation_SupportTicketEscalationId");

        builder.Property(entity => entity.SupportTicketEscalationId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.EscalationTarget).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.EscalationRemarks).HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.EscalatedDateUtc);

        builder.HasOne(entity => entity.SupportTicket)
            .WithMany(supportTicket => supportTicket.Escalations)
            .HasForeignKey(entity => entity.SupportTicketId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSupportTicketEscalation_SupportTicketId_tblSupportTicket_SupportTicketId");

        builder.HasIndex(entity => new { entity.SupportTicketId, entity.EscalatedDateUtc })
            .HasDatabaseName("IDX_tblSupportTicketEscalation_SupportTicketId_EscalatedDateUtc");

        builder.ConfigureAuditColumns();
    }
}
