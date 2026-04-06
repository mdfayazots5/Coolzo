using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Application.Common.Models;
using Coolzo.Application.Common.Validation;
using Coolzo.Contracts.Responses.Analytics;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Models;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.Analytics.Queries.GetBookingAnalytics;

public sealed record GetBookingAnalyticsQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string? TrendBy,
    long? ServiceId,
    string? Status) : IRequest<BookingAnalyticsResponse>;

public sealed class GetBookingAnalyticsQueryValidator : AbstractValidator<GetBookingAnalyticsQuery>
{
    public GetBookingAnalyticsQueryValidator()
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

        RuleFor(request => request.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || Enum.TryParse<BookingStatus>(status, true, out _))
            .WithMessage("Status must be a valid booking status.");
    }
}

public sealed class GetBookingAnalyticsQueryHandler : IRequestHandler<GetBookingAnalyticsQuery, BookingAnalyticsResponse>
{
    private readonly IAnalyticsReadRepository _analyticsReadRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IAppLogger<GetBookingAnalyticsQueryHandler> _logger;

    public GetBookingAnalyticsQueryHandler(
        IAnalyticsReadRepository analyticsReadRepository,
        ICurrentDateTime currentDateTime,
        IAppLogger<GetBookingAnalyticsQueryHandler> logger)
    {
        _analyticsReadRepository = analyticsReadRepository;
        _currentDateTime = currentDateTime;
        _logger = logger;
    }

    public async Task<BookingAnalyticsResponse> Handle(GetBookingAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var filter = AnalyticsQueryFilter.Create(
            request.DateFrom,
            request.DateTo,
            request.TrendBy,
            request.ServiceId,
            null,
            _currentDateTime.UtcNow);
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? (BookingStatus?)null
            : Enum.Parse<BookingStatus>(request.Status, true);
        var readModel = await _analyticsReadRepository.GetBookingAnalyticsAsync(filter, (int?)status, cancellationToken);

        _logger.LogInformation(
            "Booking analytics fetched for range {DateFrom} - {DateTo}.",
            filter.DateFrom,
            filter.DateTo);

        return AnalyticsResponseMapper.ToBookingAnalytics(readModel);
    }
}

