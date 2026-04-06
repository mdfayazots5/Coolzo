using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using MediatR;

namespace Coolzo.Application.Features.CMS.Queries.GetPublicFAQContent;

public sealed record GetPublicFAQContentQuery : IRequest<IReadOnlyCollection<CMSFaqResponse>>;

public sealed class GetPublicFAQContentQueryHandler : IRequestHandler<GetPublicFAQContentQuery, IReadOnlyCollection<CMSFaqResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetPublicFAQContentQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<CMSFaqResponse>> Handle(GetPublicFAQContentQuery request, CancellationToken cancellationToken)
    {
        var faqs = await _adminConfigurationRepository.SearchCmsFaqsAsync(null, null, true, true, true, cancellationToken);

        return faqs.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
