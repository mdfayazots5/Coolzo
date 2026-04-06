using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("tblSupportTicket");
        builder.HasKey(entity => entity.SupportTicketId).HasName("PK_tblSupportTicket_SupportTicketId");

        builder.Property(entity => entity.SupportTicketId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.TicketNumber).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Subject).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(1024).IsRequired();
        builder.Property(entity => entity.CurrentStatus).HasConversion<int>();

        builder.HasOne(entity => entity.Customer)
            .WithMany()
            .HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSupportTicket_CustomerId_tblCustomer_CustomerId");

        builder.HasOne(entity => entity.Category)
            .WithMany(category => category.SupportTickets)
            .HasForeignKey(entity => entity.SupportTicketCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSupportTicket_SupportTicketCategoryId_tblSupportTicketCategory_SupportTicketCategoryId");

        builder.HasOne(entity => entity.Priority)
            .WithMany(priority => priority.SupportTickets)
            .HasForeignKey(entity => entity.SupportTicketPriorityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSupportTicket_SupportTicketPriorityId_tblSupportTicketPriority_SupportTicketPriorityId");

        builder.HasIndex(entity => entity.TicketNumber)
            .IsUnique()
            .HasDatabaseName("UK_tblSupportTicket_TicketNumber");

        builder.HasIndex(entity => new { entity.CustomerId, entity.DateCreated })
            .HasDatabaseName("IDX_tblSupportTicket_CustomerId_DateCreated");

        builder.HasIndex(entity => new { entity.CurrentStatus, entity.SupportTicketPriorityId })
            .HasDatabaseName("IDX_tblSupportTicket_CurrentStatus_SupportTicketPriorityId");

        builder.ConfigureAuditColumns();
    }
}
