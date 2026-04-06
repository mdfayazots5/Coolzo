using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
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

        return AnalyticsResponseMapper.ToDashboardMetrics(
            dashboardSummary,
            bookingAnalytics,
            revenueAnalytics,
            supportAnalytics);
    }
}

