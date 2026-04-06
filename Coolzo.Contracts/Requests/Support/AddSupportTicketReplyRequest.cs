namespace Coolzo.Contracts.Requests.Support;

public sealed record AddSupportTicketReplyRequest
(
    string ReplyText,
    bool IsInternalOnly
);
