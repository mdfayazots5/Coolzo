using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SupportTicketReplyConfiguration : IEntityTypeConfiguration<SupportTicketReply>
{
    public void Configure(EntityTypeBuilder<SupportTicketReply> builder)
    {
        builder.ToTable("tblSupportTicketReply");
        builder.HasKey(entity => entity.SupportTicketReplyId).HasName("PK_tblSupportTicketReply_SupportTicketReplyId");

        builder.Property(entity => entity.SupportTicketReplyId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.ReplyText).HasMaxLength(1024).IsRequired();
        builder.Property(entity => entity.ReplyDateUtc);

        builder.HasOne(entity => entity.SupportTicket)
            .WithMany(supportTicket => supportTicket.Replies)
            .HasForeignKey(entity => entity.SupportTicketId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSupportTicketReply_SupportTicketId_tblSupportTicket_SupportTicketId");

        builder.HasIndex(entity => new { entity.SupportTicketId, entity.ReplyDateUtc })
            .HasDatabaseName("IDX_tblSupportTicketReply_SupportTicketId_ReplyDateUtc");

        builder.ConfigureAuditColumns();
    }
}
