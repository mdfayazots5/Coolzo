using Coolzo.Application.Common.Interfaces;
using Coolzo.Contracts.Responses.Amc;
using MediatR;

namespace Coolzo.Application.Features.Amc.Queries.GetCustomerAmc;

public sealed class GetCustomerAmcQueryHandler : IRequestHandler<GetCustomerAmcQuery, IReadOnlyCollection<CustomerAmcResponse>>
{
    private readonly IAmcRepository _amcRepository;
    private readonly ServiceLifecycleAccessService _serviceLifecycleAccessService;

    public GetCustomerAmcQueryHandler(
        IAmcRepository amcRepository,
        ServiceLifecycleAccessService serviceLifecycleAccessService)
    {
        _amcRepository = amcRepository;
        _serviceLifecycleAccessService = serviceLifecycleAccessService;
    }

    public async Task<IReadOnlyCollection<CustomerAmcResponse>> Handle(GetCustomerAmcQuery request, CancellationToken cancellationToken)
    {
        await _serviceLifecycleAccessService.EnsureCustomerReadAccessAsync(request.CustomerId, cancellationToken);

        var subscriptions = await _amcRepository.GetCustomerAmcByCustomerIdAsync(request.CustomerId, cancellationToken);

        return subscriptions
            .Select(AmcResponseMapper.ToCustomerAmc)
            .ToArray();
    }
}
