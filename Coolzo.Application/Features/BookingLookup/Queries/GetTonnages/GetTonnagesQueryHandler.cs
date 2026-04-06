using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetTonnages;

public sealed class GetTonnagesQueryHandler : IRequestHandler<GetTonnagesQuery, IReadOnlyCollection<TonnageLookupResponse>>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;

    public GetTonnagesQueryHandler(IBookingLookupRepository bookingLookupRepository)
    {
        _bookingLookupRepository = bookingLookupRepository;
    }

    public async Task<IReadOnlyCollection<TonnageLookupResponse>> Handle(GetTonnagesQuery request, CancellationToken cancellationToken)
    {
        var tonnages = await _bookingLookupRepository.ListTonnagesAsync(request.Search, cancellationToken);

        return tonnages
            .Select(tonnage => new TonnageLookupResponse(tonnage.TonnageId, tonnage.TonnageName, tonnage.Description))
            .ToArray();
    }
}
