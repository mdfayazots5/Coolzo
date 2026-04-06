namespace Coolzo.Contracts.Responses.Support;

public sealed record SupportTicketJobAlertResponse
(
    bool HasLinkedTickets,
    int TotalLinkedTickets,
    int OpenLinkedTickets,
    string? LatestTicketNumber,
    string? LatestStatus,
    string? LatestSubject
);
