using FluentValidation;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetZoneByPincode;

public sealed class GetZoneByPincodeQueryValidator : AbstractValidator<GetZoneByPincodeQuery>
{
    public GetZoneByPincodeQueryValidator()
    {
        RuleFor(request => request.Pincode).Matches("^[0-9]{4,8}$");
    }
}
