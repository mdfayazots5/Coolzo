namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportTicketLinkResponse
(
    long SupportTicketLinkId,
    string LinkedEntityType,
    long LinkedEntityId,
    string LinkReference,
    string LinkSummary
);
