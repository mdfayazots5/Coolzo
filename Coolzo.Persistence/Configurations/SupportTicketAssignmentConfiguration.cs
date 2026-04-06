using Coolzo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coolzo.Persistence.Configurations;

public sealed class SupportTicketAssignmentConfiguration : IEntityTypeConfiguration<SupportTicketAssignment>
{
    public void Configure(EntityTypeBuilder<SupportTicketAssignment> builder)
    {
        builder.ToTable("tblSupportTicketAssignment");
        builder.HasKey(entity => entity.SupportTicketAssignmentId).HasName("PK_tblSupportTicketAssignment_SupportTicketAssignmentId");

        builder.Property(entity => entity.SupportTicketAssignmentId).ValueGeneratedOnAdd();
        builder.Property(entity => entity.AssignmentRemarks).HasMaxLength(256).HasDefaultValue(string.Empty);
        builder.Property(entity => entity.AssignedDateUtc);
        builder.Property(entity => entity.IsActiveAssignment).HasDefaultValue(true);

        builder.HasOne(entity => entity.SupportTicket)
            .WithMany(supportTicket => supportTicket.Assignments)
            .HasForeignKey(entity => entity.SupportTicketId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSupportTicketAssignment_SupportTicketId_tblSupportTicket_SupportTicketId");

        builder.HasOne(entity => entity.AssignedUser)
            .WithMany()
            .HasForeignKey(entity => entity.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_tblSupportTicketAssignment_AssignedUserId_tblUser_UserId");

        builder.HasIndex(entity => new { entity.SupportTicketId, entity.IsActiveAssignment })
            .HasDatabaseName("IDX_tblSupportTicketAssignment_SupportTicketId_IsActiveAssignment");

        builder.HasIndex(entity => new { entity.AssignedUserId, entity.IsActiveAssignment })
            .HasDatabaseName("IDX_tblSupportTicketAssignment_AssignedUserId_IsActiveAssignment");

        builder.ConfigureAuditColumns();
    }
}
