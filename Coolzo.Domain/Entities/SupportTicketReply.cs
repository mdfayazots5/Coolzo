namespace Coolzo.Domain.Entities;

public sealed class SupportTicketReply : AuditableEntity
{
    public long SupportTicketReplyId { get; set; }

    public long SupportTicketId { get; set; }

    public string ReplyText { get; set; } = string.Empty;

    public bool IsInternalOnly { get; set; }

    public bool IsFromCustomer { get; set; }

    public DateTime ReplyDateUtc { get; set; }

    public SupportTicket? SupportTicket { get; set; }
}
