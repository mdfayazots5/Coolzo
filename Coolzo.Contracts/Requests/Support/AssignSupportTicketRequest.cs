namespace Coolzo.Contracts.Requests.Support;

public sealed record AssignSupportTicketRequest
(
    long AssignedUserId,
    string Remarks
);
