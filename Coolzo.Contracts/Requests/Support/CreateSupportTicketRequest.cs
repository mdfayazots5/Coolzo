namespace Coolzo.Contracts.Requests.Support;

public sealed record CreateSupportTicketRequest
(
    long? CustomerId,
    string Subject,
    long CategoryId,
    long PriorityId,
    string Description,
    IReadOnlyCollection<CreateSupportTicketLinkRequest> Links
);
