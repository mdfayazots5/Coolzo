namespace Coolzo.Contracts.Requests.Support;

public sealed record ChangeSupportTicketPriorityRequest
(
    long PriorityId,
    string Remarks
);
