using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using MediatR;

namespace Coolzo.Application.Features.SystemConfiguration.Queries.GetBusinessHourConfiguration;

public sealed record GetBusinessHourConfigurationQuery : IRequest<IReadOnlyCollection<BusinessHourConfigurationResponse>>;

public sealed class GetBusinessHourConfigurationQueryHandler : IRequestHandler<GetBusinessHourConfigurationQuery, IReadOnlyCollection<BusinessHourConfigurationResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetBusinessHourConfigurationQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<BusinessHourConfigurationResponse>> Handle(GetBusinessHourConfigurationQuery request, CancellationToken cancellationToken)
    {
        var entities = await _adminConfigurationRepository.GetBusinessHoursAsync(cancellationToken);

        return entities.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
