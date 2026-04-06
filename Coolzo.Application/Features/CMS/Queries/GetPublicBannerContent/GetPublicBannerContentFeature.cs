using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using MediatR;

namespace Coolzo.Application.Features.CMS.Queries.GetPublicBannerContent;

public sealed record GetPublicBannerContentQuery : IRequest<IReadOnlyCollection<CMSBannerResponse>>;

public sealed class GetPublicBannerContentQueryHandler : IRequestHandler<GetPublicBannerContentQuery, IReadOnlyCollection<CMSBannerResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetPublicBannerContentQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<CMSBannerResponse>> Handle(GetPublicBannerContentQuery request, CancellationToken cancellationToken)
    {
        var banners = await _adminConfigurationRepository.SearchCmsBannersAsync(null, true, true, true, DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        return banners.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
