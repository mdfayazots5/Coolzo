using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using MediatR;

namespace Coolzo.Application.Features.CMS.Queries.GetPublicHomeCMSContent;

public sealed record GetPublicHomeCMSContentQuery : IRequest<PublicHomeCMSContentResponse>;

public sealed class GetPublicHomeCMSContentQueryHandler : IRequestHandler<GetPublicHomeCMSContentQuery, PublicHomeCMSContentResponse>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetPublicHomeCMSContentQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<PublicHomeCMSContentResponse> Handle(GetPublicHomeCMSContentQuery request, CancellationToken cancellationToken)
    {
        var blocks = await _adminConfigurationRepository.SearchCmsBlocksAsync(null, true, true, cancellationToken);
        var banners = await _adminConfigurationRepository.SearchCmsBannersAsync(null, true, true, true, DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        var faqs = await _adminConfigurationRepository.SearchCmsFaqsAsync(null, null, true, true, true, cancellationToken);
        var displaySettings = await _adminConfigurationRepository.SearchDisplayContentSettingsAsync(null, true, true, cancellationToken);

        return new PublicHomeCMSContentResponse(
            blocks.Select(entity => AdminResponseMapper.ToResponse(entity)).ToArray(),
            banners.Select(AdminResponseMapper.ToResponse).ToArray(),
            faqs.Select(AdminResponseMapper.ToResponse).ToArray(),
            displaySettings.Select(AdminResponseMapper.ToResponse).ToArray());
    }
}
