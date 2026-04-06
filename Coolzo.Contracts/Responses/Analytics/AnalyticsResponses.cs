namespace Coolzo.Contracts.Responses.Analytics;

public sealed record DashboardSummaryResponse(
    long TotalBookings,
    long TotalServiceRequests,
    long TotalJobs,
    decimal TotalRevenue,
    long TotalAmcCustomers,
    long TotalSupportTickets);

public sealed record DashboardMetricsResponse(
    IReadOnlyCollection<AnalyticsTrendPointResponse> BookingTrends,
    IReadOnlyCollection<AnalyticsBreakdownItemResponse> JobStatusDistribution,
    RevenueSummarySnapshotResponse RevenueSummary,
    SupportOverviewSnapshotResponse SupportOverview);

public sealed record RevenueSummarySnapshotResponse(
    decimal TotalRevenue,
    decimal PaidRevenue,
    decimal OutstandingRevenue,
    long InvoiceCount);

public sealed record SupportOverviewSnapshotResponse(
    long TotalTickets,
    long OpenTickets,
    long ResolvedTickets,
    long EscalationCount,
    decimal AverageResolutionHours);

public sealed record AnalyticsTrendPointResponse(
    string PeriodLabel,
    string PeriodStartDate,
    decimal Value);

public sealed record AnalyticsBreakdownItemResponse(
    string Label,
    decimal Value);

public sealed record BookingAnalyticsResponse(
    long TotalBookings,
    long PendingBookings,
    long ConfirmedBookings,
    long CancelledBookings,
    decimal AverageBookingsPerPeriod,
    IReadOnlyCollection<AnalyticsTrendPointResponse> BookingTrends,
    IReadOnlyCollection<AnalyticsBreakdownItemResponse> StatusDistribution,
    IReadOnlyCollection<AnalyticsBreakdownItemResponse> ServiceDistribution);

public sealed record RevenueAnalyticsResponse(
    decimal TotalRevenue,
    decimal PaidRevenue,
    decimal OutstandingRevenue,
    long InvoiceCount,
    decimal AverageInvoiceValue,
    IReadOnlyCollection<AnalyticsTrendPointResponse> RevenueTrends,
    IReadOnlyCollection<AnalyticsBreakdownItemResponse> RevenueByService,
    IReadOnlyCollection<AnalyticsBreakdownItemResponse> RevenueByCustomerSegment);

public sealed record TechnicianPerformanceResponse(
    long TotalTechnicians,
    long ActiveTechnicians,
    long TotalAssignedJobs,
    long TotalCompletedJobs,
    decimal AverageCompletionHours,
    IReadOnlyCollection<TechnicianPerformanceItemResponse> Technicians);

public sealed record TechnicianPerformanceItemResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    long JobsAssigned,
    long JobsCompleted,
    decimal CompletionRatePercentage,
    decimal AverageCompletionHours,
    long CurrentWorkload);

public sealed record CustomerAnalyticsResponse(
    long TotalCustomers,
    long NewCustomers,
    long ReturningCustomers,
    long RepeatCustomers,
    long AmcCustomers,
    long NonAmcCustomers,
    decimal RepeatRatePercentage,
    IReadOnlyCollection<AnalyticsBreakdownItemResponse> SegmentDistribution,
    IReadOnlyCollection<CustomerTrendPointResponse> CustomerTrends);

public sealed record CustomerTrendPointResponse(
    string PeriodLabel,
    string PeriodStartDate,
    long NewCustomers,
    long ReturningCustomers);

public sealed record SupportAnalyticsResponse(
    long TotalTickets,
    long OpenTickets,
    long ResolvedTickets,
    long EscalationCount,
    decimal AverageResolutionHours,
    IReadOnlyCollection<AnalyticsBreakdownItemResponse> StatusDistribution,
    IReadOnlyCollection<SupportResolutionTrendPointResponse> ResolutionTrends);

public sealed record SupportResolutionTrendPointResponse(
    string PeriodLabel,
    string PeriodStartDate,
    long ResolvedTickets,
    decimal AverageResolutionHours);

public sealed record InventoryAnalyticsResponse(
    long TotalItems,
    long LowStockItems,
    decimal TotalOnHandQuantity,
    decimal ConsumedQuantity,
    IReadOnlyCollection<LowStockInventoryItemResponse> LowStockSummaries,
    IReadOnlyCollection<InventoryConsumptionTrendPointResponse> ConsumptionTrends);

public sealed record LowStockInventoryItemResponse(
    long ItemId,
    string ItemCode,
    string ItemName,
    decimal QuantityOnHand,
    decimal ReorderLevel,
    decimal ShortageQuantity);

public sealed record InventoryConsumptionTrendPointResponse(
    string PeriodLabel,
    string PeriodStartDate,
    decimal QuantityConsumed);

public sealed record DateRangeReportResponse(
    string DateFrom,
    string DateTo,
    long TotalBookings,
    decimal TotalRevenue,
    long CompletedJobs,
    long TotalSupportTickets,
    long ActiveTechnicians,
    long NewCustomers,
    IReadOnlyCollection<AnalyticsTrendPointResponse> BookingTrends,
    IReadOnlyCollection<AnalyticsTrendPointResponse> RevenueTrends,
    IReadOnlyCollection<AnalyticsBreakdownItemResponse> SupportStatusDistribution);

public sealed record ReportExportResponse(
    string Format,
    string FileName,
    string ContentType,
    string Content,
    string GeneratedAtUtc);

