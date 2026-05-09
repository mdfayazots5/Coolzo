using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Domain.Entities;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.Dashboard.Queries.GetDashboardMetrics;

public sealed record GetDashboardMetricsQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? TrendBy) : IRequest<DashboardMetricsResponse>;

public sealed class GetDashboardMetricsQueryValidator : AbstractValidator<GetDashboardMetricsQuery>
{
    public GetDashboardMetricsQueryValidator()
    {
        RuleFor(request => request)
            .Must(request => AnalyticsValidationRules.HasValidDateRange(request.DateFrom, request.DateTo))
            .WithMessage("Date range must be valid and must not exceed 366 days.");

        RuleFor(request => request.TrendBy)
            .Must(AnalyticsValidationRules.HasValidTrendBy)
            .WithMessage("TrendBy must be day, week, or month.");
    }
}

public sealed class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, DashboardMetricsResponse>
{
    private const int DashboardFallbackLookbackDays = 365;

    private readonly IAnalyticsReadRepository _analyticsReadRepository;
    private readonly ICurrentDateTime _currentDateTime;

    public GetDashboardMetricsQueryHandler(
        IAnalyticsReadRepository analyticsReadRepository,
        ICurrentDateTime currentDateTime)
    {
        _analyticsReadRepository = analyticsReadRepository;
        _currentDateTime = currentDateTime;
    }

    public async Task<DashboardMetricsResponse> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_currentDateTime.UtcNow);
        var filter = AnalyticsQueryFilter.Create(
            request.DateFrom,
            request.DateTo,
            request.TrendBy,
            null,
            null,
            _currentDateTime.UtcNow);

        var dashboardSummary = await _analyticsReadRepository.GetDashboardSummaryAsync(cancellationToken);
        var bookingAnalytics = await _analyticsReadRepository.GetBookingAnalyticsAsync(filter, null, cancellationToken);
        var revenueAnalytics = await _analyticsReadRepository.GetRevenueAnalyticsAsync(filter, cancellationToken);
        var supportAnalytics = await _analyticsReadRepository.GetSupportAnalyticsAsync(filter, null, cancellationToken);

        if (ShouldUseHistoricalFallback(request, dashboardSummary, bookingAnalytics, revenueAnalytics, supportAnalytics))
        {
            var fallbackFilter = AnalyticsQueryFilter.Create(
                today.AddDays(-DashboardFallbackLookbackDays),
                today,
                request.TrendBy,
                null,
                null,
                _currentDateTime.UtcNow);

            bookingAnalytics = await _analyticsReadRepository.GetBookingAnalyticsAsync(fallbackFilter, null, cancellationToken);
            revenueAnalytics = await _analyticsReadRepository.GetRevenueAnalyticsAsync(fallbackFilter, cancellationToken);
            supportAnalytics = await _analyticsReadRepository.GetSupportAnalyticsAsync(fallbackFilter, null, cancellationToken);
        }

        return AnalyticsResponseMapper.ToDashboardMetrics(
            dashboardSummary,
            bookingAnalytics,
            revenueAnalytics,
            supportAnalytics);
    }

    private static bool ShouldUseHistoricalFallback(
        GetDashboardMetricsQuery request,
        DashboardSummaryReadModel dashboardSummary,
        BookingAnalyticsReadModel bookingAnalytics,
        RevenueAnalyticsReadModel revenueAnalytics,
        SupportAnalyticsReadModel supportAnalytics)
    {
        if (request.DateFrom is not null || request.DateTo is not null)
        {
            return false;
        }

        var summaryHasData =
            dashboardSummary.TotalBookings > 0 ||
            dashboardSummary.TotalServiceRequests > 0 ||
            dashboardSummary.TotalJobs > 0 ||
            dashboardSummary.TotalRevenue > 0 ||
            dashboardSummary.TotalSupportTickets > 0;

        if (!summaryHasData)
        {
            return false;
        }

        return bookingAnalytics.TotalBookings == 0 &&
               revenueAnalytics.TotalRevenue == 0 &&
               revenueAnalytics.InvoiceCount == 0 &&
               supportAnalytics.TotalTickets == 0;
    }
}
