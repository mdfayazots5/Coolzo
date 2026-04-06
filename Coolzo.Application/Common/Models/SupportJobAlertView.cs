namespace Coolzo.Application.Common.Models;

public sealed record SupportJobAlertView
(
    bool HasLinkedTickets,
    int TotalLinkedTickets,
    int OpenLinkedTickets,
    string? LatestTicketNumber,
    string? LatestStatus,
    string? LatestSubject
);
