namespace Coolzo.Contracts.Requests.Support;

public sealed record EscalateSupportTicketRequest
(
    string EscalationTarget,
    string EscalationRemarks
);
