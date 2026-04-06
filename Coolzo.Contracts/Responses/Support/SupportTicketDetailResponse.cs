namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportTicketDetailResponse
(
    long SupportTicketId,
    string TicketNumber,
    string Subject,
    string Description,
    long CustomerId,
    string CustomerName,
    string CustomerMobile,
    string CustomerEmail,
    long SupportTicketCategoryId,
    string CategoryName,
    long SupportTicketPriorityId,
    string PriorityName,
    string Status,
    long? AssignedUserId,
    string? AssignedOwnerName,
    DateTime DateCreated,
    DateTime? LastUpdated,
    bool CanCustomerClose,
    bool CanManage,
    IReadOnlyCollection<SupportTicketLinkResponse> Links,
    IReadOnlyCollection<SupportTicketAssignmentResponse> Assignments,
    IReadOnlyCollection<SupportTicketReplyResponse> Replies,
    IReadOnlyCollection<SupportTicketEscalationResponse> Escalations,
    IReadOnlyCollection<SupportTicketStatusHistoryResponse> StatusHistory
);
