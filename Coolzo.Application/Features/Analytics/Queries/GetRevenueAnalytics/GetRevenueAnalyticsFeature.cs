using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.Analytics.Queries.GetRevenueAnalytics;

public sealed record GetRevenueAnalyticsQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? TrendBy,
    long? ServiceId) : IRequest<RevenueAnalyticsResponse>;

public sealed class GetRevenueAnalyticsQueryValidator : AbstractValidator<GetRevenueAnalyticsQuery>
{
    public GetRevenueAnalyticsQueryValidator()
    {
        RuleFor(request => request)
            .Must(request => AnalyticsValidationRules.HasValidDateRange(request.DateFrom, request.DateTo))
            .WithMessage("Date range must be valid and must not exceed 366 days.");

        RuleFor(request => request.TrendBy)
            .Must(AnalyticsValidationRules.HasValidTrendBy)
            .WithMessage("TrendBy must be day, week, or month.");

        RuleFor(request => request.ServiceId)
            .GreaterThan(0)
            .When(request => request.ServiceId.HasValue);
    }
}

public sealed class GetRevenueAnalyticsQueryHandler : IRequestHandler<GetRevenueAnalyticsQuery, RevenueAnalyticsResponse>
{
    private readonly IAnalyticsReadRepository _analyticsReadRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAppLogger<GetRevenueAnalyticsQueryHandler> _logger;

    public GetRevenueAnalyticsQueryHandler(
        IAnalyticsReadRepository analyticsReadRepository,
        ICurrentDateTime currentDateTime,
        IAppLogger<GetRevenueAnalyticsQueryHandler> logger)
    {
        _analyticsReadRepository = analyticsReadRepository;
        _currentDateTime = currentDateTime;
        _logger = logger;
    }

    public async Task<RevenueAnalyticsResponse> Handle(GetRevenueAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var filter = AnalyticsQueryFilter.Create(
            request.DateFrom,
            request.DateTo,
            request.TrendBy,
            request.ServiceId,
            null,
            _currentDateTime.UtcNow);
        var readModel = await _analyticsReadRepository.GetRevenueAnalyticsAsync(filter, cancellationToken);

        _logger.LogInformation(
            "Revenue analytics fetched for range {DateFrom} - {DateTo}.",
            filter.DateFrom,
            filter.DateTo);

        return AnalyticsResponseMapper.ToRevenueAnalytics(readModel);
    }
}

