namespace Coolzo.Domain.Entities;

public sealed class SupportTicketPriority : AuditableEntity
{
    public long SupportTicketPriorityId { get; set; }

    public string PriorityCode { get; set; } = string.Empty;

    public string PriorityName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int PriorityRank { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
