using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Admin;
using MediatR;

namespace Coolzo.Application.Features.ServiceCatalogAdmin.Queries.GetServiceCatalogAdmin;

public sealed record GetServiceCatalogAdminQuery : IRequest<ServiceCatalogAdminResponse>;

public sealed class GetServiceCatalogAdminQueryHandler
    : IRequestHandler<GetServiceCatalogAdminQuery, ServiceCatalogAdminResponse>
{
    private readonly IBookingLookupRepository _bookingLookupRepository;

    public GetServiceCatalogAdminQueryHandler(IBookingLookupRepository bookingLookupRepository)
    {
        _bookingLookupRepository = bookingLookupRepository;
    }

    public async Task<ServiceCatalogAdminResponse> Handle(GetServiceCatalogAdminQuery request, CancellationToken cancellationToken)
    {
        var categories = await _bookingLookupRepository.ListServiceCategoriesAdminAsync(cancellationToken);
        var services = await _bookingLookupRepository.ListServicesAdminAsync(cancellationToken);
        var pricingModels = await _bookingLookupRepository.ListPricingModelsAsync(cancellationToken);

        var serviceCountByCategory = services
            .GroupBy(service => service.ServiceCategoryId)
            .ToDictionary(group => group.Key, group => group.Count());

        return new ServiceCatalogAdminResponse(
            categories
                .Select(category => ServiceCatalogMapper.ToCategoryResponse(
                    category,
                    serviceCountByCategory.TryGetValue(category.ServiceCategoryId, out var count) ? count : 0))
                .ToArray(),
            services.Select(ServiceCatalogMapper.ToServiceResponse).ToArray(),
            pricingModels.Select(ServiceCatalogMapper.ToPricingModelResponse).ToArray());
    }
}
