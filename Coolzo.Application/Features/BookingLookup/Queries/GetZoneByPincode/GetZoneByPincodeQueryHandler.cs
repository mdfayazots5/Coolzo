using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetZoneByPincode;

public sealed class GetZoneByPincodeQueryHandler : IRequestHandler<GetZoneByPincodeQuery, ZoneLookupResponse>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;

    public GetZoneByPincodeQueryHandler(IBookingLookupRepository bookingLookupRepository)
    {
        _bookingLookupRepository = bookingLookupRepository;
    }

    public async Task<ZoneLookupResponse> Handle(GetZoneByPincodeQuery request, CancellationToken cancellationToken)
    {
        var zone = await _bookingLookupRepository.GetZoneByPincodeAsync(request.Pincode, cancellationToken)
            ?? throw new AppException(ErrorCodes.ZoneNotServed, "The provided pincode is not serviceable.", 404);

        return new ZoneLookupResponse(zone.ZoneId, zone.ZoneName, zone.CityName, request.Pincode);
    }
}
