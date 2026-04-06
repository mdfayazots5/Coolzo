using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Booking;
using MediatR;

namespace Coolzo.Application.Features.BookingLookup.Queries.GetServiceCategories;

public sealed class GetServiceCategoriesQueryHandler : IRequestHandler<GetServiceCategoriesQuery, IReadOnlyCollection<ServiceCategoryLookupResponse>>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;

    public GetServiceCategoriesQueryHandler(IBookingLookupRepository bookingLookupRepository)
    {
        _bookingLookupRepository = bookingLookupRepository;
    }

    public async Task<IReadOnlyCollection<ServiceCategoryLookupResponse>> Handle(GetServiceCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _bookingLookupRepository.ListServiceCategoriesAsync(request.Search, cancellationToken);

        return categories
            .Select(category => new ServiceCategoryLookupResponse(
                category.ServiceCategoryId,
                category.CategoryName,
                category.Description))
            .ToArray();
    }
}
