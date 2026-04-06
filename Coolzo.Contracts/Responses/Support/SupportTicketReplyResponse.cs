namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportTicketReplyResponse
(
    long SupportTicketReplyId,
    string ReplyText,
    bool IsInternalOnly,
    bool IsFromCustomer,
    string CreatedBy,
    DateTime ReplyDateUtc
);
