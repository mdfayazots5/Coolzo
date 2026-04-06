namespace Coolzo.Domain.Entities;

public sealed record SupportAnalyticsReadModel(
    long TotalTickets,
    long OpenTickets,
    long ResolvedTickets,
    long EscalationCount,
    decimal AverageResolutionHours,
    IReadOnlyCollection<AnalyticsBreakdownItemReadModel> StatusDistribution,
    IReadOnlyCollection<SupportResolutionTrendPointReadModel> ResolutionTrends);

