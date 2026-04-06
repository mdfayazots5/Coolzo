namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportTicketListItemResponse
(
    long SupportTicketId,
    string TicketNumber,
    string Subject,
    string CustomerName,
    string CustomerMobile,
    string? LinkedEntityType,
    string LinkedEntitySummary,
    string CategoryName,
    string PriorityName,
    string Status,
    long? AssignedUserId,
    string? AssignedOwnerName,
    DateTime DateCreated,
    DateTime? LastUpdated
);
