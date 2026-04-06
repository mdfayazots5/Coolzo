namespace Coolzo.Domain.Entities;

public sealed record DashboardSummaryReadModel(
    long TotalBookings,
    long TotalServiceRequests,
    long TotalJobs,
    decimal TotalRevenue,
    long TotalAmcCustomers,
    long TotalSupportTickets,
    IReadOnlyCollection<AnalyticsBreakdownItemReadModel> StatusDistribution);

