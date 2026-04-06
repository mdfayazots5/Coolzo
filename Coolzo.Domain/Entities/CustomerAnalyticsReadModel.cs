namespace Coolzo.Domain.Entities;

public sealed record CustomerAnalyticsReadModel(
    long TotalCustomers,
    long NewCustomers,
    long ReturningCustomers,
    long RepeatCustomers,
    long AmcCustomers,
    long NonAmcCustomers,
    decimal RepeatRatePercentage,
    IReadOnlyCollection<AnalyticsBreakdownItemReadModel> SegmentDistribution,
    IReadOnlyCollection<CustomerTrendPointReadModel> CustomerTrends);

