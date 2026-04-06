namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportTicketStatusHistoryResponse
(
    long SupportTicketStatusHistoryId,
    string Status,
    string Remarks,
    string CreatedBy,
    DateTime StatusDateUtc
);
