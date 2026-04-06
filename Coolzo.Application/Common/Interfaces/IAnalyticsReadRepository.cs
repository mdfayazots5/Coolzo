using Coolzo.Application.Common.Models;
using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IAnalyticsReadRepository
{
    Task<DashboardSummaryReadModel> GetDashboardSummaryAsync(CancellationToken cancellationToken);

    Task<BookingAnalyticsReadModel> GetBookingAnalyticsAsync(
        AnalyticsQueryFilter filter,
        int? bookingStatus,
        CancellationToken cancellationToken);

    Task<RevenueAnalyticsReadModel> GetRevenueAnalyticsAsync(
        AnalyticsQueryFilter filter,
        CancellationToken cancellationToken);

    Task<TechnicianPerformanceReadModel> GetTechnicianPerformanceAsync(
        AnalyticsQueryFilter filter,
        int? serviceRequestStatus,
        CancellationToken cancellationToken);

    Task<CustomerAnalyticsReadModel> GetCustomerAnalyticsAsync(
        AnalyticsQueryFilter filter,
        CancellationToken cancellationToken);

    Task<SupportAnalyticsReadModel> GetSupportAnalyticsAsync(
        AnalyticsQueryFilter filter,
        int? supportTicketStatus,
        CancellationToken cancellationToken);

    Task<InventoryAnalyticsReadModel> GetInventoryAnalyticsAsync(
        AnalyticsQueryFilter filter,
        CancellationToken cancellationToken);

    Task<DateRangeReportReadModel> GetReportByDateRangeAsync(
        AnalyticsQueryFilter filter,
        CancellationToken cancellationToken);
}

