using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.Analytics.Queries.GetCustomerAnalytics;

public sealed record GetCustomerAnalyticsQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? TrendBy) : IRequest<CustomerAnalyticsResponse>;

public sealed class GetCustomerAnalyticsQueryValidator : AbstractValidator<GetCustomerAnalyticsQuery>
{
    public GetCustomerAnalyticsQueryValidator()
    {
        RuleFor(request => request)
            .Must(request => AnalyticsValidationRules.HasValidDateRange(request.DateFrom, request.DateTo))
            .WithMessage("Date range must be valid and must not exceed 366 days.");

        RuleFor(request => request.TrendBy)
            .Must(AnalyticsValidationRules.HasValidTrendBy)
            .WithMessage("TrendBy must be day, week, or month.");
    }
}

public sealed class GetCustomerAnalyticsQueryHandler : IRequestHandler<GetCustomerAnalyticsQuery, CustomerAnalyticsResponse>
{
    private readonly IAnalyticsReadRepository _analyticsReadRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAppLogger<GetCustomerAnalyticsQueryHandler> _logger;

    public GetCustomerAnalyticsQueryHandler(
        IAnalyticsReadRepository analyticsReadRepository,
        ICurrentDateTime currentDateTime,
        IAppLogger<GetCustomerAnalyticsQueryHandler> logger)
    {
        _analyticsReadRepository = analyticsReadRepository;
        _currentDateTime = currentDateTime;
        _logger = logger;
    }

    public async Task<CustomerAnalyticsResponse> Handle(GetCustomerAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var filter = AnalyticsQueryFilter.Create(
            request.DateFrom,
            request.DateTo,
            request.TrendBy,
            null,
            null,
            _currentDateTime.UtcNow);
        var readModel = await _analyticsReadRepository.GetCustomerAnalyticsAsync(filter, cancellationToken);

        _logger.LogInformation(
            "Customer analytics fetched for range {DateFrom} - {DateTo}.",
            filter.DateFrom,
            filter.DateTo);

        return AnalyticsResponseMapper.ToCustomerAnalytics(readModel);
    }
}

