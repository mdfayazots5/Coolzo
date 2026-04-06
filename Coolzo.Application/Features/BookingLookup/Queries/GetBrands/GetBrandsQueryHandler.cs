using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetBrands;

public sealed class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, IReadOnlyCollection<BrandLookupResponse>>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;

    public GetBrandsQueryHandler(IBookingLookupRepository bookingLookupRepository)
    {
        _bookingLookupRepository = bookingLookupRepository;
    }

    public async Task<IReadOnlyCollection<BrandLookupResponse>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await _bookingLookupRepository.ListBrandsAsync(request.Search, cancellationToken);

        return brands
            .Select(brand => new BrandLookupResponse(brand.BrandId, brand.BrandName, brand.Description))
            .ToArray();
    }
}
