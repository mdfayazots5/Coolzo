namespace Coolzo.Domain.Entities;

public sealed record DateRangeReportReadModel(
    DateOnly DateFrom,
    DateOnly DateTo,
    long TotalBookings,
    decimal TotalRevenue,
    long CompletedJobs,
    long TotalSupportTickets,
    long ActiveTechnicians,
    long NewCustomers,
    IReadOnlyCollection<AnalyticsTrendPointReadModel> BookingTrends,
    IReadOnlyCollection<AnalyticsTrendPointReadModel> RevenueTrends,
    IReadOnlyCollection<AnalyticsBreakdownItemReadModel> SupportStatusDistribution);

