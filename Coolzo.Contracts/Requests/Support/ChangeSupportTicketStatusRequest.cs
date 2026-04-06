namespace Coolzo.Contracts.Requests.Support;

public sealed record ChangeSupportTicketStatusRequest
(
    string Status,
    string Remarks
);
