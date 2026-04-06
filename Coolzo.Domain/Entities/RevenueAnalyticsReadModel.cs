namespace Coolzo.Domain.Entities;

public sealed record RevenueAnalyticsReadModel(
    decimal TotalRevenue,
    decimal PaidRevenue,
    decimal OutstandingRevenue,
    long InvoiceCount,
    decimal AverageInvoiceValue,
    IReadOnlyCollection<AnalyticsTrendPointReadModel> RevenueTrends,
    IReadOnlyCollection<AnalyticsBreakdownItemReadModel> RevenueByService,
    IReadOnlyCollection<AnalyticsBreakdownItemReadModel> RevenueByCustomerSegment);

