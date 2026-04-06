using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Rules;

public static class SupportTicketStatusRule
{
    public static bool CanTransition(SupportTicketStatus currentStatus, SupportTicketStatus targetStatus)
    {
        return currentStatus switch
        {
            SupportTicketStatus.Open => targetStatus == SupportTicketStatus.InProgress,
            SupportTicketStatus.InProgress => targetStatus == SupportTicketStatus.WaitingForCustomer ||
                targetStatus == SupportTicketStatus.Escalated ||
                targetStatus == SupportTicketStatus.Resolved,
            SupportTicketStatus.WaitingForCustomer => targetStatus == SupportTicketStatus.CustomerResponded,
            SupportTicketStatus.CustomerResponded => targetStatus == SupportTicketStatus.InProgress,
            SupportTicketStatus.Escalated => targetStatus == SupportTicketStatus.InProgress,
            SupportTicketStatus.Resolved => targetStatus == SupportTicketStatus.Closed,
            SupportTicketStatus.Closed => targetStatus == SupportTicketStatus.Reopened,
            SupportTicketStatus.Reopened => targetStatus == SupportTicketStatus.InProgress,
            _ => false
        };
    }
}
