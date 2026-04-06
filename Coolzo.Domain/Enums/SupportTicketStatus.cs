namespace Coolzo.Domain.Enums;

public enum SupportTicketStatus
{
    Open = 1,
    InProgress = 2,
    WaitingForCustomer = 3,
    CustomerResponded = 4,
    Escalated = 5,
    Resolved = 6,
    Closed = 7,
    Reopened = 8
}
