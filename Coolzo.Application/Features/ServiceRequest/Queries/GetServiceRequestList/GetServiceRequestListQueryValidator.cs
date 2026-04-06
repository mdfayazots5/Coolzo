using Coolzo.Domain.Enums;
using FluentValidation;

namespace Coolzo.Application.Features.ServiceRequest.Queries.GetServiceRequestList;

public sealed class GetServiceRequestListQueryValidator : AbstractValidator<GetServiceRequestListQuery>
{
    public GetServiceRequestListQueryValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
        RuleFor(request => request.BookingId!.Value)
            .GreaterThan(0)
            .When(request => request.BookingId.HasValue);
        RuleFor(request => request.ServiceId!.Value)
            .GreaterThan(0)
            .When(request => request.ServiceId.HasValue);
        RuleFor(request => request.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || Enum.TryParse<ServiceRequestStatus>(status, true, out _))
            .WithMessage("Status must be one of New, Assigned, EnRoute, or Reached.");
    }
}
