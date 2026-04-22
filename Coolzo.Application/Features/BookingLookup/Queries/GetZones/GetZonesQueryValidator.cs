using FluentValidation;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetZones;

public sealed class GetZonesQueryValidator : AbstractValidator<GetZonesQuery>
{
    public GetZonesQueryValidator()
    {
        RuleFor(request => request.Search).MaximumLength(64);
    }
}
