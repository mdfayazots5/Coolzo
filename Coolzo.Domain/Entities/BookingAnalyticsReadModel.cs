namespace Coolzo.Domain.Entities;

public sealed record BookingAnalyticsReadModel(
    long TotalBookings,
    long PendingBookings,
    long ConfirmedBookings,
    long CancelledBookings,
    decimal AverageBookingsPerPeriod,
    IReadOnlyCollection<AnalyticsTrendPointReadModel> BookingTrends,
    IReadOnlyCollection<AnalyticsBreakdownItemReadModel> StatusDistribution,
    IReadOnlyCollection<AnalyticsBreakdownItemReadModel> ServiceDistribution);

