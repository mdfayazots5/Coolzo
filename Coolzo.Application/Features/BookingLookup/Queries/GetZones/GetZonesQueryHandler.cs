using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetZones;

public sealed class GetZonesQueryHandler : IRequestHandler<GetZonesQuery, IReadOnlyCollection<ZoneListItemResponse>>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;

    public GetZonesQueryHandler(IBookingLookupRepository bookingLookupRepository)
    {
        _bookingLookupRepository = bookingLookupRepository;
    }

    public async Task<IReadOnlyCollection<ZoneListItemResponse>> Handle(GetZonesQuery request, CancellationToken cancellationToken)
    {
        var zones = await _bookingLookupRepository.ListZonesAsync(request.Search, cancellationToken);

        return zones
            .Select(zone => new ZoneListItemResponse(zone.ZoneId, zone.ZoneCode, zone.ZoneName, zone.CityName))
            .ToArray();
    }
}
