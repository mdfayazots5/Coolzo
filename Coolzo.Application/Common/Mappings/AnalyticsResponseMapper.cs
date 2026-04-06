using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Mappings;

public static class AnalyticsResponseMapper
{
    public static DashboardSummaryResponse ToDashboardSummary(DashboardSummaryReadModel readModel)
    {
        return new DashboardSummaryResponse(
            readModel.TotalBookings,
            readModel.TotalServiceRequests,
            readModel.TotalJobs,
            readModel.TotalRevenue,
            readModel.TotalAmcCustomers,
            readModel.TotalSupportTickets);
    }

    public static DashboardMetricsResponse ToDashboardMetrics(
        DashboardSummaryReadModel summary,
        BookingAnalyticsReadModel bookingAnalytics,
        RevenueAnalyticsReadModel revenueAnalytics,
        SupportAnalyticsReadModel supportAnalytics)
    {
        return new DashboardMetricsResponse(
            ToTrendPoints(bookingAnalytics.BookingTrends),
            ToBreakdownItems(summary.StatusDistribution),
            new RevenueSummarySnapshotResponse(
                revenueAnalytics.TotalRevenue,
                revenueAnalytics.PaidRevenue,
                revenueAnalytics.OutstandingRevenue,
                revenueAnalytics.InvoiceCount),
            new SupportOverviewSnapshotResponse(
                supportAnalytics.TotalTickets,
                supportAnalytics.OpenTickets,
                supportAnalytics.ResolvedTickets,
                supportAnalytics.EscalationCount,
                supportAnalytics.AverageResolutionHours));
    }

    public static BookingAnalyticsResponse ToBookingAnalytics(BookingAnalyticsReadModel readModel)
    {
        return new BookingAnalyticsResponse(
            readModel.TotalBookings,
            readModel.PendingBookings,
            readModel.ConfirmedBookings,
            readModel.CancelledBookings,
            readModel.AverageBookingsPerPeriod,
            ToTrendPoints(readModel.BookingTrends),
            ToBreakdownItems(readModel.StatusDistribution),
            ToBreakdownItems(readModel.ServiceDistribution));
    }

    public static RevenueAnalyticsResponse ToRevenueAnalytics(RevenueAnalyticsReadModel readModel)
    {
        return new RevenueAnalyticsResponse(
            readModel.TotalRevenue,
            readModel.PaidRevenue,
            readModel.OutstandingRevenue,
            readModel.InvoiceCount,
            readModel.AverageInvoiceValue,
            ToTrendPoints(readModel.RevenueTrends),
            ToBreakdownItems(readModel.RevenueByService),
            ToBreakdownItems(readModel.RevenueByCustomerSegment));
    }

    public static TechnicianPerformanceResponse ToTechnicianPerformance(TechnicianPerformanceReadModel readModel)
    {
        return new TechnicianPerformanceResponse(
            readModel.TotalTechnicians,
            readModel.ActiveTechnicians,
            readModel.TotalAssignedJobs,
            readModel.TotalCompletedJobs,
            readModel.AverageCompletionHours,
            readModel.Technicians
                .Select(item => new TechnicianPerformanceItemResponse(
                    item.TechnicianId,
                    item.TechnicianCode,
                    item.TechnicianName,
                    item.JobsAssigned,
                    item.JobsCompleted,
                    item.CompletionRatePercentage,
                    item.AverageCompletionHours,
                    item.CurrentWorkload))
                .ToArray());
    }

    public static CustomerAnalyticsResponse ToCustomerAnalytics(CustomerAnalyticsReadModel readModel)
    {
        return new CustomerAnalyticsResponse(
            readModel.TotalCustomers,
            readModel.NewCustomers,
            readModel.ReturningCustomers,
            readModel.RepeatCustomers,
            readModel.AmcCustomers,
            readModel.NonAmcCustomers,
            readModel.RepeatRatePercentage,
            ToBreakdownItems(readModel.SegmentDistribution),
            readModel.CustomerTrends
                .Select(item => new CustomerTrendPointResponse(
                    item.PeriodLabel,
                    item.PeriodStartDate.ToString("yyyy-MM-dd"),
                    item.NewCustomers,
                    item.ReturningCustomers))
                .ToArray());
    }

    public static SupportAnalyticsResponse ToSupportAnalytics(SupportAnalyticsReadModel readModel)
    {
        return new SupportAnalyticsResponse(
            readModel.TotalTickets,
            readModel.OpenTickets,
            readModel.ResolvedTickets,
            readModel.EscalationCount,
            readModel.AverageResolutionHours,
            ToBreakdownItems(readModel.StatusDistribution),
            readModel.ResolutionTrends
                .Select(item => new SupportResolutionTrendPointResponse(
                    item.PeriodLabel,
                    item.PeriodStartDate.ToString("yyyy-MM-dd"),
                    item.ResolvedTickets,
                    item.AverageResolutionHours))
                .ToArray());
    }

    public static InventoryAnalyticsResponse ToInventoryAnalytics(InventoryAnalyticsReadModel readModel)
    {
        return new InventoryAnalyticsResponse(
            readModel.TotalItems,
            readModel.LowStockItems,
            readModel.TotalOnHandQuantity,
            readModel.ConsumedQuantity,
            readModel.LowStockSummaries
                .Select(item => new LowStockInventoryItemResponse(
                    item.ItemId,
                    item.ItemCode,
                    item.ItemName,
                    item.QuantityOnHand,
                    item.ReorderLevel,
                    item.ShortageQuantity))
                .ToArray(),
            readModel.ConsumptionTrends
                .Select(item => new InventoryConsumptionTrendPointResponse(
                    item.PeriodLabel,
                    item.PeriodStartDate.ToString("yyyy-MM-dd"),
                    item.QuantityConsumed))
                .ToArray());
    }

    public static DateRangeReportResponse ToDateRangeReport(DateRangeReportReadModel readModel)
    {
        return new DateRangeReportResponse(
            readModel.DateFrom.ToString("yyyy-MM-dd"),
            readModel.DateTo.ToString("yyyy-MM-dd"),
            readModel.TotalBookings,
            readModel.TotalRevenue,
            readModel.CompletedJobs,
            readModel.TotalSupportTickets,
            readModel.ActiveTechnicians,
            readModel.NewCustomers,
            ToTrendPoints(readModel.BookingTrends),
            ToTrendPoints(readModel.RevenueTrends),
            ToBreakdownItems(readModel.SupportStatusDistribution));
    }

    private static IReadOnlyCollection<AnalyticsTrendPointResponse> ToTrendPoints(
        IReadOnlyCollection<AnalyticsTrendPointReadModel> readModels)
    {
        return readModels
            .Select(item => new AnalyticsTrendPointResponse(
                item.PeriodLabel,
                item.PeriodStartDate.ToString("yyyy-MM-dd"),
                item.Value))
            .ToArray();
    }

    private static IReadOnlyCollection<AnalyticsBreakdownItemResponse> ToBreakdownItems(
        IReadOnlyCollection<AnalyticsBreakdownItemReadModel> readModels)
    {
        return readModels
            .Select(item => new AnalyticsBreakdownItemResponse(item.Label, item.Value))
            .ToArray();
    }
}

