using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetServices;

public sealed class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, IReadOnlyCollection<ServiceLookupResponse>>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;

    public GetServicesQueryHandler(IBookingLookupRepository bookingLookupRepository)
    {
        _bookingLookupRepository = bookingLookupRepository;
    }

    public async Task<IReadOnlyCollection<ServiceLookupResponse>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _bookingLookupRepository.ListServicesAsync(request.ServiceCategoryId, request.Search, cancellationToken);

        return services
            .Select(service => new ServiceLookupResponse(
                service.ServiceId,
                service.ServiceCategoryId,
                service.ServiceName,
                service.Summary,
                service.BasePrice,
                service.PricingModel?.PricingModelName ?? string.Empty))
            .ToArray();
    }
}
