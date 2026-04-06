namespace Coolzo.Domain.Entities;

public sealed class SupportTicketAssignment : AuditableEntity
{
    public long SupportTicketAssignmentId { get; set; }

    public long SupportTicketId { get; set; }

    public long AssignedUserId { get; set; }

    public DateTime AssignedDateUtc { get; set; }

    public string AssignmentRemarks { get; set; } = string.Empty;

    public bool IsActiveAssignment { get; set; } = true;

    public SupportTicket? SupportTicket { get; set; }

    public User? AssignedUser { get; set; }
}
