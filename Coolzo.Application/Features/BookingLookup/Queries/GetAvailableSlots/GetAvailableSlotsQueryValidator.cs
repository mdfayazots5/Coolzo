using FluentValidation;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryValidator : AbstractValidator<GetAvailableSlotsQuery>
{
    public GetAvailableSlotsQueryValidator()
    {
        RuleFor(request => request.ZoneId).GreaterThan(0);
    }
}
