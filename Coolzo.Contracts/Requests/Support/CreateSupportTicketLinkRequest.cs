namespace Coolzo.Contracts.Requests.Support;

public sealed record CreateSupportTicketLinkRequest
(
    string LinkedEntityType,
    long LinkedEntityId
);
