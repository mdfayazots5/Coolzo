using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class SupportTicket : AuditableEntity
{
    public long SupportTicketId { get; set; }

    public string TicketNumber { get; set; } = string.Empty;

    public long CustomerId { get; set; }

    public long SupportTicketCategoryId { get; set; }

    public long SupportTicketPriorityId { get; set; }

    public SupportTicketStatus CurrentStatus { get; set; } = SupportTicketStatus.Open;

    public string Subject { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Customer? Customer { get; set; }

    public SupportTicketCategory? Category { get; set; }

    public SupportTicketPriority? Priority { get; set; }

    public ICollection<SupportTicketReply> Replies { get; set; } = new List<SupportTicketReply>();

    public ICollection<SupportTicketEscalation> Escalations { get; set; } = new List<SupportTicketEscalation>();

    public ICollection<SupportTicketStatusHistory> StatusHistories { get; set; } = new List<SupportTicketStatusHistory>();

    public ICollection<SupportTicketLink> Links { get; set; } = new List<SupportTicketLink>();

    public ICollection<SupportTicketAssignment> Assignments { get; set; } = new List<SupportTicketAssignment>();
}
