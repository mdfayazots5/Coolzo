using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetAcTypes;

public sealed class GetAcTypesQueryHandler : IRequestHandler<GetAcTypesQuery, IReadOnlyCollection<AcTypeLookupResponse>>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;

    public GetAcTypesQueryHandler(IBookingLookupRepository bookingLookupRepository)
    {
        _bookingLookupRepository = bookingLookupRepository;
    }

    public async Task<IReadOnlyCollection<AcTypeLookupResponse>> Handle(GetAcTypesQuery request, CancellationToken cancellationToken)
    {
        var acTypes = await _bookingLookupRepository.ListAcTypesAsync(request.Search, cancellationToken);

        return acTypes
            .Select(acType => new AcTypeLookupResponse(acType.AcTypeId, acType.AcTypeName, acType.Description))
            .ToArray();
    }
}
