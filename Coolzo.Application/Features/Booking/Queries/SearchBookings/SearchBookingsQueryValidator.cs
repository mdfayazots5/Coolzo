using FluentValidation;

namespace Coolzo.Application.Features.Booking.Queries.SearchBookings;

public sealed class SearchBookingsQueryValidator : AbstractValidator<SearchBookingsQuery>
{
    public SearchBookingsQueryValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
        RuleFor(request => request.BookingReference).MaximumLength(64);
        RuleFor(request => request.CustomerMobile).Matches("^[0-9]{0,16}$")
            .When(request => !string.IsNullOrWhiteSpace(request.CustomerMobile));
    }
}
