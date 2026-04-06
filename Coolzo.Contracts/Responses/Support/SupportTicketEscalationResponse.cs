namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportTicketEscalationResponse
(
    long SupportTicketEscalationId,
    string EscalationTarget,
    string EscalationRemarks,
    string EscalatedBy,
    DateTime EscalatedDateUtc
);
