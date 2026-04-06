using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Mappings;
using Coolzo.Contracts.Responses.Admin;
using FluentValidation;
using MediatR;

namespace Coolzo.Application.Features.CMS.Queries.GetCMSBannerList;

public sealed record GetCMSBannerListQuery(
    string? Search,
    bool? IsActive,
    bool? IsPublished) : IRequest<IReadOnlyCollection<CMSBannerResponse>>;

public sealed class GetCMSBannerListQueryValidator : AbstractValidator<GetCMSBannerListQuery>
{
    public GetCMSBannerListQueryValidator()
    {
        RuleFor(request => request.Search).MaximumLength(128);
    }
}

public sealed class GetCMSBannerListQueryHandler : IRequestHandler<GetCMSBannerListQuery, IReadOnlyCollection<CMSBannerResponse>>
{
    private readonly IAdminConfigurationRepository _adminConfigurationRepository;

    public GetCMSBannerListQueryHandler(IAdminConfigurationRepository adminConfigurationRepository)
    {
        _adminConfigurationRepository = adminConfigurationRepository;
    }

    public async Task<IReadOnlyCollection<CMSBannerResponse>> Handle(GetCMSBannerListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _adminConfigurationRepository.SearchCmsBannersAsync(
            request.Search,
            request.IsActive,
            request.IsPublished,
            publicOnly: false,
            activeDate: null,
            cancellationToken);

        return entities.Select(AdminResponseMapper.ToResponse).ToArray();
    }
}
